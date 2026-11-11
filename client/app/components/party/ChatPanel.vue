<script setup lang="ts">
import type { ChatMessage } from '~/utils/types'

const QUICK_REACTIONS = ['🍻', '😂', '🔥', '❤️', '🤯', '👏']

const { chatMessages, sendChat, sendReaction } = useParty()

const draft = ref('')
const listEl = ref<HTMLElement | null>(null)

const isSystem = (message: ChatMessage) => message.userId < 0

watch(
  () => chatMessages.value.length,
  async () => {
    await nextTick()
    listEl.value?.scrollTo({ top: listEl.value.scrollHeight })
  },
)

async function send() {
  const text = draft.value.trim()
  if (!text)
    return
  draft.value = ''
  await sendChat(text)
}
</script>

<template>
  <div class="glass-panel flex h-full flex-col overflow-hidden">
    <div class="border-b border-white/10 px-4 py-3 font-bold">
      💬 Party chat
    </div>

    <div ref="listEl" class="scrollbar-thin flex flex-1 flex-col gap-2 overflow-y-auto px-4 py-3">
      <div v-if="!chatMessages.length" class="py-8 text-center text-sm text-white/40">
        No messages yet — say hi! 👋
      </div>
      <template v-for="(message, index) in chatMessages" :key="index">
        <div v-if="isSystem(message)" class="py-0.5 text-center text-xs text-white/50 italic">
          {{ message.emoji }} {{ message.nickname }} {{ message.text }}
        </div>
        <div v-else class="max-w-[85%]">
          <div class="mb-0.5 text-xs text-white/50">
            {{ message.emoji }} {{ message.nickname }}
          </div>
          <div class="rounded-xl bg-white/10 px-3 py-1.5 text-sm">
            {{ message.text }}
          </div>
        </div>
      </template>
    </div>

    <div class="flex justify-center gap-1 border-t border-white/10 px-3 py-2 text-2xl">
      <button
        v-for="emoji in QUICK_REACTIONS"
        :key="emoji"
        type="button"
        class="rounded-lg p-1 transition hover:scale-125"
        :aria-label="`React with ${emoji}`"
        @click="sendReaction(emoji)"
      >
        {{ emoji }}
      </button>
    </div>

    <form class="flex items-center gap-2 p-3" @submit.prevent="send">
      <q-input v-model="draft" outlined dense placeholder="Say something…" class="flex-1" />
      <q-btn round dense unelevated color="primary" icon="send" type="submit" :disable="!draft.trim()" />
    </form>
  </div>
</template>
