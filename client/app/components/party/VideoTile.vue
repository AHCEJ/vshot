<script setup lang="ts">
import type { Participant } from '~/utils/types'

const props = defineProps<{
  participant: Participant
  stream?: MediaStream | null
  isLocal?: boolean
}>()

const videoEl = ref<HTMLVideoElement | null>(null)

function attachStream() {
  const el = videoEl.value
  if (!el)
    return
  if (props.stream && el.srcObject !== props.stream)
    el.srcObject = props.stream
  el.muted = props.isLocal === true
}

watch(() => props.stream, attachStream)
onMounted(attachStream)
</script>

<template>
  <div class="relative h-full w-full overflow-hidden rounded-2xl border border-white/10 bg-party-panel">
    <video
      v-show="stream && participant.videoOn"
      ref="videoEl"
      class="h-full w-full object-cover"
      :class="{ 'scale-x-[-1]': isLocal }"
      autoplay
      playsinline
    />

    <div v-if="!stream || !participant.videoOn" class="absolute inset-0 flex items-center justify-center">
      <span class="text-6xl drop-shadow-lg">{{ participant.emoji }}</span>
    </div>

    <div class="absolute right-2 bottom-2 left-2 flex items-end gap-2">
      <span class="flex items-center gap-1.5 rounded-full bg-black/55 px-2.5 py-1 text-sm backdrop-blur">
        <span v-if="participant.isHost" title="Host">👑</span>
        <span class="max-w-40 truncate font-semibold">{{ participant.nickname }}</span>
        <span v-if="isLocal" class="text-white/50">(you)</span>
        <span v-if="!participant.audioOn" title="Mic off">🔇</span>
      </span>
      <span
        v-if="participant.sessionShots > 0"
        class="ml-auto flex items-center gap-1 rounded-full bg-black/55 px-2.5 py-1 text-sm font-bold text-party-gold backdrop-blur"
      >
        🥃 {{ participant.sessionShots }}
      </span>
    </div>
  </div>
</template>
