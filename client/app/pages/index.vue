<script setup lang="ts">
import { z } from 'zod'
import type { RoomInfo } from '~/utils/types'

const EMOJIS = ['😎', '🥳', '🔥', '🐺', '🦊', '🐸', '👻', '🤖', '🍕', '🦄', '🐱', '🚀']

const route = useRoute()
const router = useRouter()
const auth = useAuth()
const { request: createRoom, loading: creating, error: createError } = useRequest<RoomInfo>()
const { request: joinRoom, loading: joining, error: joinError } = useRequest<RoomInfo>()

const hasInviteCode = computed(() => typeof route.query.code === 'string' && route.query.code.length > 0)

const schema = z.object({
  nickname: z
    .string()
    .trim()
    .min(2, 'Nickname needs at least 2 characters.')
    .max(32, 'Max 32 characters.'),
  emoji: z.string().min(1).max(8),
  roomName: z.string().trim().max(64, 'Max 64 characters.'),
  joinCode: z
    .string()
    .trim()
    .toUpperCase()
    .refine(
      value => value === '' || /^[A-Z0-9]{4,8}$/.test(value),
      'Lobby numbers are 4-8 letters and digits.',
    ),
})

type LandingValues = z.infer<typeof schema>

// One form, two intents: host a new lobby or join one by number.
const intent = ref<'create' | 'join'>('create')

const form = useForm({
  schema,
  initialValues: {
    nickname: '',
    emoji: EMOJIS[0],
    roomName: '',
    joinCode: typeof route.query.code === 'string' ? route.query.code.toUpperCase() : '',
  },
  onSubmit: values => (intent.value === 'create' ? doCreate(values) : doJoin(values)),
})

const joinCodeInput = computed({
  get: () => form.values.joinCode,
  set: (value: string) => {
    form.values.joinCode = value.toUpperCase()
  },
})

onMounted(async () => {
  const me = await auth.fetchMe()
  if (me) {
    form.values.nickname = me.nickname
    form.values.emoji = me.emoji
  }
})

async function ensureIdentity(values: { nickname: string, emoji: string }): Promise<void> {
  const me = auth.user.value
  if (!me)
    await auth.joinAsGuest(values.nickname, values.emoji)
  else if (me.nickname !== values.nickname || me.emoji !== values.emoji)
    await auth.updateProfile(values.nickname, values.emoji)
}

async function doCreate(values: LandingValues): Promise<void> {
  const room = await createRoom(async () => {
    await ensureIdentity(values)
    return $fetch<RoomInfo>('/api/rooms', {
      method: 'POST',
      body: { name: values.roomName || undefined },
    })
  })
  if (room)
    await router.push(`/room/${room.code}`)
}

async function doJoin(values: LandingValues): Promise<void> {
  if (!values.joinCode) {
    form.errors.joinCode = 'Enter the lobby number your friend shared.'
    return
  }
  const room = await joinRoom(async () => {
    await ensureIdentity(values)
    return $fetch<RoomInfo>('/api/rooms/join', {
      method: 'POST',
      body: { code: values.joinCode },
    })
  })
  if (room)
    await router.push(`/room/${room.code}`)
}

function submitCreate() {
  intent.value = 'create'
  void form.submit()
}

function submitJoin() {
  intent.value = 'join'
  void form.submit()
}

useHead({ title: 'Virtual Shot 🥂 — party together, wherever you are' })
</script>

<template>
  <div class="relative min-h-screen overflow-hidden px-4 py-10">
    <div class="pointer-events-none absolute inset-0 -z-10">
      <div class="absolute left-1/2 top-[-20%] h-[40rem] w-[40rem] -translate-x-1/2 rounded-full bg-party-accent/20 blur-3xl" />
      <div class="absolute bottom-[-30%] right-[-10%] h-[30rem] w-[30rem] rounded-full bg-party-gold/10 blur-3xl" />
    </div>

    <div class="mx-auto flex max-w-5xl flex-col gap-8">
      <header class="text-center">
        <h1 class="text-5xl font-black sm:text-6xl">
          <span class="bg-gradient-to-r from-party-accent via-orange-400 to-party-gold bg-clip-text text-transparent">
            Virtual Shot
          </span>
          <span class="ml-2">🥂</span>
        </h1>
        <p class="mt-3 text-lg text-white/70">
          Video-party with your friends and take shots together — wherever you all are.
        </p>
      </header>

      <div class="grid gap-5 lg:grid-cols-3">
        <q-card flat class="glass-panel p-5">
          <h2 class="mb-3 text-lg font-bold">1 · Pick your vibe</h2>
          <q-input
            v-model="form.values.nickname"
            label="Nickname"
            outlined
            maxlength="32"
            class="mb-4"
            :error="!!form.errors.nickname"
            :error-message="form.errors.nickname"
          />
          <div class="mb-2 text-xs tracking-wider text-white/50 uppercase">Choose an emoji face</div>
          <div class="flex flex-wrap gap-2">
            <button
              v-for="emoji in EMOJIS"
              :key="emoji"
              type="button"
              class="flex h-11 w-11 items-center justify-center rounded-xl text-2xl transition"
              :class="form.values.emoji === emoji ? 'bg-party-accent/30 ring-2 ring-party-accent' : 'bg-white/5 hover:bg-white/10'"
              :aria-label="`Choose ${emoji}`"
              @click="form.values.emoji = emoji"
            >
              {{ emoji }}
            </button>
          </div>
          <p v-if="auth.user.value" class="mt-4 text-xs text-white/40">
            Welcome back, {{ auth.user.value.emoji }} {{ auth.user.value.nickname }} — your vibe is remembered.
          </p>
        </q-card>

        <q-card flat class="glass-panel flex flex-col p-5">
          <h2 class="mb-3 text-lg font-bold">2 · Host a party 🎉</h2>
          <p class="mb-4 text-sm text-white/60">
            Create a lobby and get a short lobby number to share with your friends.
          </p>
          <q-input
            v-model="form.values.roomName"
            label="Lobby name (optional)"
            outlined
            maxlength="64"
            class="mb-4"
            :error="!!form.errors.roomName"
            :error-message="form.errors.roomName"
          />
          <q-btn
            color="primary"
            size="lg"
            unelevated
            no-caps
            icon-right="celebration"
            label="Create lobby"
            class="mt-auto w-full font-bold"
            :loading="creating"
            @click="submitCreate"
          />
          <div v-if="createError" class="mt-3 text-sm text-red-400">{{ createError }}</div>
        </q-card>

        <q-card flat class="glass-panel flex flex-col p-5" :class="{ 'ring-2 ring-party-accent': hasInviteCode }">
          <h2 class="mb-3 text-lg font-bold">…or join one 🚪</h2>
          <p class="mb-4 text-sm text-white/60">
            Got a lobby number from a friend? Drop it in and join the party.
          </p>
          <q-input
            v-model="joinCodeInput"
            label="Lobby number"
            outlined
            maxlength="8"
            class="mb-4 font-mono text-xl font-bold tracking-widest uppercase"
            :error="!!form.errors.joinCode"
            :error-message="form.errors.joinCode"
          >
            <template #prepend><span class="text-2xl">🔢</span></template>
          </q-input>
          <q-btn
            color="accent"
            text-color="black"
            size="lg"
            unelevated
            no-caps
            icon-right="login"
            label="Join the party"
            class="mt-auto w-full font-bold"
            :loading="joining"
            @click="submitJoin"
          />
          <div v-if="joinError" class="mt-3 text-sm text-red-400">{{ joinError }}</div>
        </q-card>
      </div>

      <footer class="text-center text-xs text-white/35">
        No sign-up, no accounts — just a nickname. Drink responsibly. 💛
      </footer>
    </div>
  </div>
</template>
