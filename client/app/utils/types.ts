export interface IdentityMe {
  userId: number
  nickname: string
  emoji: string
  lifetimeShots: number
}

export interface Participant {
  userId: number
  nickname: string
  emoji: string
  audioOn: boolean
  videoOn: boolean
  sessionShots: number
  isHost: boolean
}

export interface ChatMessage {
  userId: number
  nickname: string
  emoji: string
  text: string
  at: string
}

export interface ShotTakenEvent {
  userId: number
  nickname: string
  emoji: string
  drink: string
  userSessionShots: number
  roomTotalShots: number
}

export interface RoundState {
  drink: string
  startedByName: string
  phase: 'countdown' | 'cheers'
  count: number
  meDrank: boolean
}

export interface ReactionBurst {
  id: number
  emoji: string
  nickname: string
}

export interface SignalPayload {
  kind: 'offer' | 'answer' | 'candidate'
  sdp?: string
  candidate?: string
}

export interface RoomInfo {
  code: string
  name: string
  hostNickname: string
  participants?: Participant[]
  roomTotalShots?: number
}

export interface Drink {
  id: string
  label: string
  emoji: string
}

export const DRINKS: Drink[] = [
  { id: 'tequila', label: 'Tequila', emoji: '🥃' },
  { id: 'beer', label: 'Beer', emoji: '🍺' },
  { id: 'wine', label: 'Wine', emoji: '🍷' },
  { id: 'cocktail', label: 'Cocktail', emoji: '🍸' },
  { id: 'water', label: 'Water', emoji: '💧' },
]

export function drinkEmoji(drink: string): string {
  return DRINKS.find(d => d.id === drink)?.emoji ?? '🥤'
}

export function drinkLabel(drink: string): string {
  return DRINKS.find(d => d.id === drink)?.label ?? drink
}
