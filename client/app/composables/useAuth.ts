import type { IdentityMe } from '~/utils/types'

const user = useState<IdentityMe | null>('vshot:user', () => null)

/** Nickname-only identity backed by an httpOnly cookie. */
export function useAuth() {
  async function fetchMe(): Promise<IdentityMe | null> {
    try {
      user.value = await $fetch<IdentityMe>('/api/identity/me')
    }
    catch {
      user.value = null
    }
    return user.value
  }

  async function joinAsGuest(nickname: string, emoji: string): Promise<IdentityMe> {
    user.value = await $fetch<IdentityMe>('/api/identity/guest', {
      method: 'POST',
      body: { nickname, emoji },
    })
    return user.value
  }

  async function updateProfile(nickname: string, emoji: string): Promise<IdentityMe> {
    user.value = await $fetch<IdentityMe>('/api/identity', {
      method: 'PATCH',
      body: { nickname, emoji },
    })
    return user.value
  }

  async function logout(): Promise<void> {
    try {
      await $fetch('/api/identity/logout', { method: 'POST' })
    }
    catch {}
    user.value = null
  }

  return { user, fetchMe, joinAsGuest, updateProfile, logout }
}
