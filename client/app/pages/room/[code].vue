<script setup lang="ts">
import type { ConnectionStatus } from '~/composables/useParty'
import type { RoomInfo } from '~/utils/types'

const route = useRoute()
const router = useRouter()
const auth = useAuth()

const party = useParty()
const { connectionStatus, roomTotalShots, mediaBlocked } = party

const $q = useNuxtApp().$q as { notify?: (options: { type?: string, message: string }) => void }

const code = computed(() => String(route.params.code ?? '').toUpperCase())
const roomInfo = ref<RoomInfo | null>(null)
const loadError = ref<string | null>(null)
const connecting = ref(true)

const statusText: Record<ConnectionStatus, string> = {
  idle: 'Idle',
  connecting: 'Connecting…',
  connected: 'Live',
  reconnecting: 'Reconnecting…',
  disconnected: 'Offline',
}

const statusClass = computed(
  () =>
    ({
      idle: 'bg-white/40',
      connecting: 'animate-pulse bg-amber-400',
      connected: 'bg-green-400',
      reconnecting: 'animate-pulse bg-amber-400',
      disconnected: 'bg-red-500',
    })[connectionStatus.value],
)

const inviteUrl = computed(() =>
  typeof window === 'undefined' ? '' : `${window.location.origin}/?code=${code.value}`,
)

async function copyInvite() {
  try {
    await navigator.clipboard.writeText(inviteUrl.value)
    $q.notify?.({ type: 'positive', message: 'Invite link copied — send it to your friends!' })
  }
  catch {
    $q.notify?.({ type: 'warning', message: inviteUrl.value })
  }
}

function handleBeforeUnload() {
  party.hardStop()
}

onMounted(async () => {
  window.addEventListener('beforeunload', handleBeforeUnload)

  const me = await auth.fetchMe()
  if (!me) {
    // No identity yet: send them to the landing page with the code pre-filled.
    void router.replace({ path: '/', query: { code: code.value } })
    return
  }

  try {
    roomInfo.value = await $fetch<RoomInfo>(`/api/rooms/${code.value}`)
  }
  catch (err) {
    loadError.value = apiErrorMessage(err, 'Lobby not found.')
    connecting.value = false
    return
  }

  await party.ensureLocalMedia()

  try {
    await party.connect(code.value, me)
  }
  catch {
    loadError.value = 'Could not reach the party server — is the backend running on port 5000?'
  }
  finally {
    connecting.value = false
  }
})

onBeforeUnmount(() => {
  window.removeEventListener('beforeunload', handleBeforeUnload)
  void party.leaveRoom()
})

async function leave() {
  await party.leaveRoom()
  await router.push('/')
}

useHead({ title: () => `${code.value} · Virtual Shot` })
</script>

<template>
  <div class="flex h-[100dvh] flex-col overflow-hidden">
    <!-- Lobby not found / backend unreachable -->
    <div v-if="loadError" class="flex flex-1 flex-col items-center justify-center gap-4 p-6 text-center">
      <div class="text-6xl">🍺</div>
      <div class="text-xl font-bold">{{ loadError }}</div>
      <q-btn color="primary" unelevated no-caps icon-right="home" label="Back to the front door" to="/" />
    </div>

    <template v-else>
      <header class="flex flex-wrap items-center gap-2 border-b border-white/10 bg-party-panel/60 px-4 py-2 backdrop-blur">
        <div class="mr-1 text-lg font-black">
          🥂
        </div>
        <div class="min-w-0 truncate font-bold text-party-gold">
          {{ roomInfo?.name || 'Party' }}
        </div>
        <q-chip dense square color="primary" text-color="white" class="font-mono font-bold tracking-widest">
          {{ code }}
        </q-chip>
        <q-btn flat dense no-caps icon="content_copy" label="Copy invite" class="text-white/70" @click="copyInvite" />
        <div class="ml-auto flex items-center gap-3">
          <span class="flex items-center gap-1.5 text-sm text-white/70">
            <span class="h-2.5 w-2.5 rounded-full" :class="statusClass" />
            {{ statusText[connectionStatus] }}
          </span>
          <span class="text-sm font-bold text-party-gold">🥃 {{ roomTotalShots }}</span>
        </div>
      </header>

      <q-banner v-if="mediaBlocked" class="bg-amber-900/60 py-1 text-amber-200">
        <template #avatar><span class="text-xl">📷</span></template>
        Camera and mic are blocked — you're in ghost mode. You can still chat and react!
      </q-banner>

      <div class="flex min-h-0 flex-1">
        <div class="min-w-0 flex-1 p-2 sm:p-3">
          <VideoGrid :code="code" />
        </div>
        <aside class="hidden w-80 p-2 sm:p-3 md:flex lg:w-96">
          <ChatPanel class="flex-1" />
        </aside>
      </div>

      <ShotBar @leave="leave" />

      <RoundOverlay />
      <ReactionRain />

      <!-- Connecting / camera permission overlay -->
      <div
        v-if="connecting"
        class="fixed inset-0 z-40 flex flex-col items-center justify-center gap-4 bg-black/70"
      >
        <q-spinner-orbit size="80px" color="primary" />
        <div class="text-white/80">
          Warming up the party…
        </div>
      </div>
    </template>
  </div>
</template>
