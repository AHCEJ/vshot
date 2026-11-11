let audioContext: AudioContext | null = null

function context(): AudioContext | null {
  if (typeof window === 'undefined')
    return null
  audioContext ??= new (window.AudioContext || (window as unknown as { webkitAudioContext: typeof AudioContext }).webkitAudioContext)()
  if (audioContext.state === 'suspended')
    void audioContext.resume()
  return audioContext
}

function tone(freq: number, delaySec: number, durSec: number, type: OscillatorType = 'sine', vol = 0.12) {
  const ac = context()
  if (!ac)
    return
  const t0 = ac.currentTime + delaySec
  const osc = ac.createOscillator()
  const gain = ac.createGain()
  osc.type = type
  osc.frequency.setValueAtTime(freq, t0)
  gain.gain.setValueAtTime(0.0001, t0)
  gain.gain.exponentialRampToValueAtTime(vol, t0 + 0.01)
  gain.gain.exponentialRampToValueAtTime(0.0001, t0 + durSec)
  osc.connect(gain)
  gain.connect(ac.destination)
  osc.start(t0)
  osc.stop(t0 + durSec + 0.05)
}

/** Tiny WebAudio sound effects - no audio assets needed. */
export function useSounds() {
  return {
    tick: () => tone(660, 0, 0.15, 'square', 0.14),
    cheers: () => {
      tone(523, 0, 0.25)
      tone(659, 0.12, 0.25)
      tone(784, 0.24, 0.25)
      tone(1047, 0.36, 0.55)
    },
    clink: () => {
      tone(1568, 0, 0.2, 'triangle', 0.16)
      tone(2093, 0.05, 0.25, 'triangle', 0.1)
    },
    join: () => {
      tone(440, 0, 0.15, 'sine', 0.09)
      tone(587, 0.12, 0.2, 'sine', 0.09)
    },
  }
}
