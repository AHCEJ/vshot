<script setup lang="ts">
const { reactionBursts } = useParty()

function burstStyle(id: number): Record<string, string> {
  return {
    left: `${3 + ((id * 37) % 90)}%`,
    fontSize: `${28 + ((id * 13) % 5) * 9}px`,
  }
}
</script>

<template>
  <teleport to="body">
    <div class="pointer-events-none fixed inset-0 z-40 overflow-hidden">
      <div
        v-for="burst in reactionBursts"
        :key="burst.id"
        class="reaction-float absolute bottom-24"
        :style="burstStyle(burst.id)"
      >
        {{ burst.emoji }}
      </div>
    </div>
  </teleport>
</template>
