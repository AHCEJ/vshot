import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import type { HubConnection } from '@microsoft/signalr'
import type { ChatMessage, IdentityMe, Participant, ReactionBurst, RoundState, ShotTakenEvent, SignalPayload } from '~/utils/types'
import { drinkEmoji } from '~/utils/types'

interface PeerState {
  pc: RTCPeerConnection
  pendingCandidates: RTCIceCandidateInit[]
}

export type ConnectionStatus = 'idle' | 'connecting' | 'connected' | 'reconnecting' | 'disconnected'

// Module-level singleton state: one party room at a time per tab.
const participants = ref<Participant[]>([])
const chatMessages = ref<ChatMessage[]>([])
const reactionBursts = ref<ReactionBurst[]>([])
const roomTotalShots = ref(0)
const roundState = ref<RoundState | null>(null)
const connectionStatus = ref<ConnectionStatus>('idle')
const roomCode = ref<string | null>(null)
const localStream = shallowRef<MediaStream | null>(null)
const remoteStreams = shallowReactive<Record<number, MediaStream>>({})
const micOn = ref(true)
const camOn = ref(true)
const mediaBlocked = ref(false)

let connection: HubConnection | null = null
let meId = 0
let burstSeq = 0
const peers = new Map<number, PeerState>()
const roundTimers: ReturnType<typeof setTimeout>[] = []

const me = computed<Participant | null>(() => participants.value.find(p => p.userId === meId) ?? null)

/**
 * Live party room: SignalR hub connection for presence/chat/rounds/shots,
 * plus a WebRTC peer mesh for video. Existing members send offers to newcomers.
 */
export function useParty() {
  const sounds = useSounds()

  // ---------- local media ----------

  async function ensureLocalMedia(): Promise<void> {
    if (localStream.value)
      return
    mediaBlocked.value = false
    micOn.value = true
    camOn.value = true
    try {
      const stream = await navigator.mediaDevices.getUserMedia({
        video: { width: { ideal: 640 }, height: { ideal: 480 }, facingMode: 'user' },
        audio: true,
      })
      localStream.value = stream
      stream.getAudioTracks().forEach(t => (t.enabled = micOn.value))
      stream.getVideoTracks().forEach(t => (t.enabled = camOn.value))
    }
    catch {
      localStream.value = null
      mediaBlocked.value = true
      micOn.value = false
      camOn.value = false
    }
  }

  // ---------- WebRTC mesh ----------

  function sendSignal(target: number, payload: SignalPayload) {
    if (!connection || !roomCode.value)
      return
    connection.invoke('SendSignal', roomCode.value, target, payload).catch(() => {})
  }

  function createPeer(userId: number): PeerState {
    closePeer(userId)
    const pc = new RTCPeerConnection({
      iceServers: [{ urls: ['stun:stun.l.google.com:19302'] }],
    })
    const state: PeerState = { pc, pendingCandidates: [] }
    peers.set(userId, state)

    const stream = localStream.value
    if (stream)
      stream.getTracks().forEach(track => pc.addTrack(track, stream))

    pc.ontrack = (event) => {
      if (event.streams[0])
        remoteStreams[userId] = event.streams[0]
    }
    pc.onicecandidate = (event) => {
      if (event.candidate)
        sendSignal(userId, { kind: 'candidate', candidate: JSON.stringify(event.candidate.toJSON()) })
    }
    pc.onconnectionstatechange = () => {
      if (pc.connectionState === 'failed')
        closePeer(userId)
    }
    return state
  }

  function closePeer(userId: number) {
    const state = peers.get(userId)
    if (!state)
      return
    try {
      state.pc.close()
    }
    catch {}
    peers.delete(userId)
    delete remoteStreams[userId]
  }

  async function initiateOffer(userId: number) {
    const state = peers.get(userId) ?? createPeer(userId)
    if (state.pc.signalingState !== 'stable')
      return
    const offer = await state.pc.createOffer()
    await state.pc.setLocalDescription(offer)
    sendSignal(userId, { kind: 'offer', sdp: JSON.stringify(state.pc.localDescription?.toJSON()) })
  }

  async function handleSignal(from: number, payload: SignalPayload) {
    if (payload.kind === 'offer' && payload.sdp) {
      let state = peers.get(from)
      if (state && state.pc.signalingState !== 'stable') {
        // Peer reconnected (or glare) - rebuild and answer their offer.
        closePeer(from)
        state = undefined
      }
      state ??= createPeer(from)
      await state.pc.setRemoteDescription(JSON.parse(payload.sdp) as RTCSessionDescriptionInit)
      const answer = await state.pc.createAnswer()
      await state.pc.setLocalDescription(answer)
      sendSignal(from, { kind: 'answer', sdp: JSON.stringify(state.pc.localDescription?.toJSON()) })
      flushCandidates(from)
    }
    else if (payload.kind === 'answer' && payload.sdp) {
      const state = peers.get(from)
      if (!state || state.pc.signalingState !== 'have-local-offer')
        return
      await state.pc.setRemoteDescription(JSON.parse(payload.sdp) as RTCSessionDescriptionInit)
      flushCandidates(from)
    }
    else if (payload.kind === 'candidate' && payload.candidate) {
      const state = peers.get(from)
      if (!state)
        return
      const init = JSON.parse(payload.candidate) as RTCIceCandidateInit
      if (state.pc.remoteDescription)
        await state.pc.addIceCandidate(init).catch(() => {})
      else
        state.pendingCandidates.push(init)
    }
  }

  function flushCandidates(userId: number) {
    const state = peers.get(userId)
    if (!state)
      return
    const pending = state.pendingCandidates.splice(0)
    for (const candidate of pending)
      state.pc.addIceCandidate(candidate).catch(() => {})
  }

  // ---------- synchronized rounds ----------

  function clearRoundTimers() {
    for (const timer of roundTimers)
      clearTimeout(timer)
    roundTimers.length = 0
  }

  function startRoundLocally(drink: string, startedByName: string) {
    clearRoundTimers()
    roundState.value = { drink, startedByName, phase: 'countdown', count: 3, meDrank: false }
    sounds.tick()
    const setCount = (count: number) => {
      if (roundState.value?.phase === 'countdown') {
        roundState.value.count = count
        sounds.tick()
      }
    }
    roundTimers.push(setTimeout(() => setCount(2), 1000))
    roundTimers.push(setTimeout(() => setCount(1), 2000))
    roundTimers.push(setTimeout(() => {
      if (!roundState.value)
        return
      roundState.value.phase = 'cheers'
      sounds.cheers()
    }, 3000))
    roundTimers.push(setTimeout(() => {
      if (roundState.value?.phase === 'cheers')
        roundState.value = null
    }, 13000))
  }

  // ---------- connect / disconnect ----------

  async function connect(code: string, identity: IdentityMe): Promise<void> {
    meId = identity.userId
    roomCode.value = code
    connectionStatus.value = 'connecting'

    const conn = new HubConnectionBuilder()
      .withUrl('/hubs/party', { withCredentials: true })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(LogLevel.Information)
      .build()
    connection = conn

    conn.on('RoomState', (list: Participant[], totalShots: number) => {
      participants.value = list
      roomTotalShots.value = totalShots
    })
    conn.on('ParticipantJoined', (participant: Participant) => {
      const index = participants.value.findIndex(p => p.userId === participant.userId)
      if (index >= 0)
        participants.value.splice(index, 1, participant)
      else
        participants.value.push(participant)
      sounds.join()
      if (participant.userId !== meId)
        initiateOffer(participant.userId).catch(() => {})
    })
    conn.on('ParticipantLeft', (userId: number) => {
      participants.value = participants.value.filter(p => p.userId !== userId)
      closePeer(userId)
    })
    conn.on('ChatMessage', (message: ChatMessage) => {
      chatMessages.value.push(message)
    })
    conn.on('StreamStateChanged', (userId: number, audioOn: boolean, videoOn: boolean) => {
      const participant = participants.value.find(p => p.userId === userId)
      if (participant) {
        participant.audioOn = audioOn
        participant.videoOn = videoOn
      }
    })
    conn.on('Reaction', (userId: number, emoji: string) => {
      const participant = participants.value.find(p => p.userId === userId)
      reactionBursts.value.push({ id: burstSeq++, emoji, nickname: participant?.nickname ?? 'someone' })
      if (reactionBursts.value.length > 20)
        reactionBursts.value.splice(0, reactionBursts.value.length - 20)
    })
    conn.on('RoundStarted', (drink: string, startedByName: string) => {
      startRoundLocally(drink, startedByName)
    })
    conn.on('ShotTaken', (evt: ShotTakenEvent) => {
      roomTotalShots.value = evt.roomTotalShots
      const participant = participants.value.find(p => p.userId === evt.userId)
      if (participant)
        participant.sessionShots = evt.userSessionShots
      if (evt.userId === meId && roundState.value)
        roundState.value.meDrank = true
      chatMessages.value.push({
        userId: -1,
        nickname: evt.nickname,
        emoji: drinkEmoji(evt.drink),
        text: `downed a ${evt.drink} (${evt.userSessionShots} this session)`,
        at: new Date().toISOString(),
      })
      reactionBursts.value.push({ id: burstSeq++, emoji: drinkEmoji(evt.drink), nickname: evt.nickname })
      sounds.clink()
    })
    conn.on('SignalReceived', (from: number, payload: SignalPayload) => {
      handleSignal(from, payload).catch(() => {})
    })
    conn.on('SessionReplaced', () => {
      connectionStatus.value = 'disconnected'
      conn.stop().catch(() => {})
      resetState()
    })
    conn.on('RoomError', (message: string) => {
      connectionStatus.value = 'disconnected'
      chatMessages.value.push({ userId: -1, nickname: 'Virtual Shot', emoji: '⚠️', text: message, at: new Date().toISOString() })
    })
    conn.onreconnecting(() => (connectionStatus.value = 'reconnecting'))
    conn.onreconnected(async () => {
      connectionStatus.value = 'connected'
      if (roomCode.value)
        await conn.invoke('JoinRoom', roomCode.value).catch(() => {})
    })
    conn.onclose(() => {
      if (connectionStatus.value !== 'disconnected')
        connectionStatus.value = 'disconnected'
    })

    await conn.start()
    connectionStatus.value = 'connected'
    await conn.invoke('JoinRoom', code)
    await conn.invoke('SetStreamState', code, micOn.value, camOn.value).catch(() => {})
  }

  function stopLocalTracks() {
    localStream.value?.getTracks().forEach(track => track.stop())
    localStream.value = null
  }

  function resetState() {
    for (const userId of [...peers.keys()])
      closePeer(userId)
    participants.value = []
    chatMessages.value = []
    reactionBursts.value = []
    roomTotalShots.value = 0
    roundState.value = null
    roomCode.value = null
    connection = null
    meId = 0
    clearRoundTimers()
    connectionStatus.value = 'idle'
  }

  async function leaveRoom(): Promise<void> {
    clearRoundTimers()
    if (connection && roomCode.value) {
      try {
        await connection.invoke('LeaveRoom', roomCode.value)
      }
      catch {}
    }
    try {
      await connection?.stop()
    }
    catch {}
    stopLocalTracks()
    resetState()
  }

  /** Best-effort stop for page unload - the server cleans up on disconnect. */
  function hardStop() {
    connection?.stop().catch(() => {})
  }

  // ---------- actions ----------

  async function sendChat(text: string) {
    if (connection && roomCode.value)
      await connection.invoke('SendChat', roomCode.value, text).catch(() => {})
  }

  async function sendReaction(emoji: string) {
    if (connection && roomCode.value)
      await connection.invoke('SendReaction', roomCode.value, emoji).catch(() => {})
  }

  async function startRound(drink: string) {
    if (connection && roomCode.value)
      await connection.invoke('StartRound', roomCode.value, drink).catch(() => {})
  }

  async function takeShot(drink: string) {
    if (connection && roomCode.value)
      await connection.invoke('TakeShot', roomCode.value, drink).catch(() => {})
  }

  async function toggleMic() {
    micOn.value = !micOn.value
    localStream.value?.getAudioTracks().forEach(t => (t.enabled = micOn.value))
    if (connection && roomCode.value)
      await connection.invoke('SetStreamState', roomCode.value, micOn.value, camOn.value).catch(() => {})
  }

  async function toggleCam() {
    camOn.value = !camOn.value
    localStream.value?.getVideoTracks().forEach(t => (t.enabled = camOn.value))
    if (connection && roomCode.value)
      await connection.invoke('SetStreamState', roomCode.value, micOn.value, camOn.value).catch(() => {})
  }

  return {
    // state
    participants,
    chatMessages,
    reactionBursts,
    roomTotalShots,
    roundState,
    connectionStatus,
    roomCode,
    localStream,
    remoteStreams,
    micOn,
    camOn,
    mediaBlocked,
    me,
    // lifecycle
    ensureLocalMedia,
    connect,
    leaveRoom,
    hardStop,
    // actions
    sendChat,
    sendReaction,
    startRound,
    takeShot,
    toggleMic,
    toggleCam,
  }
}
