<script setup lang="ts">
const { roundState, takeShot } = useParty()

const drinking = ref(false)

async function drink() {
  if (!roundState.value || drinking.value)
    return
  drinking.value = true
  try {
    await takeShot(roundState.value.drink)
  }
  finally {
    drinking.value = false
  }
}
</script>

<template>
  <teleport to="body">
    <transition name="fade">
      <div
        v-if="roundState"
        class="fixed inset-0 z-50 flex items-center justify-center bg-black/75 backdrop-blur-sm"
      >
        <!-- Countdown phase -->
        <div v-if="roundState.phase === 'countdown'" class="select-none text-center">
          <p class="mb-4 text-sm tracking-[0.3em] text-party-gold uppercase">
            {{ drinkEmoji(roundState.drink) }} {{ drinkLabel(roundState.drink) }} round — {{ roundState.startedByName }}
          </p>
          <div :key="roundState.count" class="count-pop-anim text-[10rem] leading-none font-black text-white">
            {{ roundState.count }}
          </div>
        </div>

        <!-- Cheers phase -->
        <div v-else class="select-none text-center">
          <h2 class="mb-4 text-6xl font-black text-party-gold sm:text-8xl">
            CHEERS! 🥂
          </h2>
          <p class="mb-8 text-lg text-white/80">
            {{ drinkEmoji(roundState.drink) }} {{ drinkLabel(roundState.drink) }} — down it!
          </p>
          <q-btn
            v-if="!roundState.meDrank"
            color="primary"
            size="xl"
            unelevated
            no-caps
            icon-right="local_bar"
            label="I DRANK!"
            class="pulse-glow-anim font-black"
            :loading="drinking"
            @click="drink"
          />
          <div v-else class="animate-pulse text-white/60">
            Nice. Waiting for the crew…
          </div>
        </div>
      </div>
    </transition>
  </teleport>
</template>
