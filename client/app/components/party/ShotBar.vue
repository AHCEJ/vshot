<script setup lang="ts">
const { micOn, camOn, mediaBlocked, toggleMic, toggleCam, startRound, takeShot } = useParty()

const emit = defineEmits<{ leave: [] }>()

const drink = ref('tequila')
const shooting = ref(false)
const rounding = ref(false)

const drinkOptions = DRINKS.map(d => ({ label: `${d.emoji} ${d.label}`, value: d.id }))

async function onTakeShot() {
  shooting.value = true
  try {
    await takeShot(drink.value)
  }
  finally {
    shooting.value = false
  }
}

async function onStartRound() {
  rounding.value = true
  try {
    await startRound(drink.value)
  }
  finally {
    rounding.value = false
  }
}
</script>

<template>
  <footer
    class="flex flex-wrap items-center justify-center gap-2 border-t border-white/10 bg-party-panel/80 px-3 py-2 backdrop-blur sm:justify-between"
  >
    <q-btn-toggle
      v-model="drink"
      :options="drinkOptions"
      dense
      no-caps
      unelevated
      rounded
      color="white/10"
      text-color="white"
      toggle-color="primary"
      toggle-text-color="white"
      class="flex-wrap"
    />

    <div class="flex items-center gap-2">
      <q-btn
        color="negative"
        unelevated
        no-caps
        icon-right="local_bar"
        label="TAKE A SHOT"
        class="pulse-glow-anim font-black"
        :loading="shooting"
        @click="onTakeShot"
      />
      <q-btn
        color="accent"
        text-color="black"
        outline
        no-caps
        icon-right="timer"
        label="START ROUND"
        :loading="rounding"
        @click="onStartRound"
      />
    </div>

    <div class="flex items-center gap-1">
      <q-btn
        flat
        round
        :icon="micOn ? 'mic' : 'mic_off'"
        :color="micOn ? 'white' : 'negative'"
        @click="toggleMic"
      />
      <q-btn
        flat
        round
        :disable="mediaBlocked"
        :icon="camOn ? 'videocam' : 'videocam_off'"
        :color="camOn ? 'white' : 'negative'"
        @click="toggleCam"
      />
      <q-btn flat round color="negative" icon="logout" @click="emit('leave')" />
    </div>
  </footer>
</template>
