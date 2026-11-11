<script setup lang="ts">
import type { Participant } from '~/utils/types'

defineProps<{ code?: string }>()

const { participants, remoteStreams, localStream, me } = useParty()
const auth = useAuth()

interface Tile {
  key: string
  participant: Participant
  stream: MediaStream | null
  isLocal: boolean
}

const tiles = computed<Tile[]>(() => {
  const list: Tile[] = []
  for (const participant of participants.value) {
    if (me.value && participant.userId === me.value.userId) {
      list.push({ key: `me-${participant.userId}`, participant, stream: localStream.value, isLocal: true })
    }
    else {
      list.push({
        key: `peer-${participant.userId}`,
        participant,
        stream: remoteStreams[participant.userId] ?? null,
        isLocal: false,
      })
    }
  }

  // Before the first room state arrives, still show my own tile.
  const fallback = auth.user.value
  if (!me.value && fallback) {
    list.push({
      key: 'me-pending',
      participant: {
        userId: fallback.userId,
        nickname: fallback.nickname,
        emoji: fallback.emoji,
        audioOn: true,
        videoOn: true,
        sessionShots: 0,
        isHost: false,
      },
      stream: localStream.value,
      isLocal: true,
    })
  }
  return list
})

const gridClass = computed(() => {
  const count = Math.max(tiles.value.length, 1)
  if (count <= 1)
    return 'grid-cols-1'
  if (count <= 2)
    return 'grid-cols-1 sm:grid-cols-2'
  if (count <= 4)
    return 'grid-cols-2'
  if (count <= 9)
    return 'grid-cols-2 lg:grid-cols-3'
  return 'grid-cols-2 lg:grid-cols-3 xl:grid-cols-4'
})
</script>

<template>
  <div class="h-full w-full">
    <div v-if="!tiles.length" class="flex h-full flex-col items-center justify-center gap-3 text-center">
      <div class="text-6xl">
        🥂
      </div>
      <div class="text-lg font-bold">
        Waiting for the crew…
      </div>
      <div class="text-white/60">
        Share lobby number
        <span class="ml-1 font-mono text-xl font-bold tracking-[0.2em] text-party-gold">{{ code }}</span>
      </div>
    </div>

    <div v-else class="grid h-full w-full auto-rows-fr gap-2 sm:gap-3" :class="gridClass">
      <VideoTile
        v-for="tile in tiles"
        :key="tile.key"
        :participant="tile.participant"
        :stream="tile.stream"
        :is-local="tile.isLocal"
      />
    </div>
  </div>
</template>
