# Virtual Shot 🥂

Meet up with your friends on live video and take shots together — no matter where everyone is.

- **Nickname-only identity** — no sign-up, no accounts, no email.
- Create a party lobby and share the short **6-character lobby number** (or a copy-paste invite link).
- Friends just type the lobby number and drop straight into the room.
- **Live video for everyone** (peer-to-peer WebRTC mesh), party chat, and floating reactions.
- **START ROUND** triggers a synchronized 3… 2… 1… **CHEERS!** countdown — everyone drinks at the same moment.
- Shots are counted per person and for the whole room, and logged behind the scenes for lifetime stats.

## Stack

| Area | Tech |
| --- | --- |
| Backend | .NET 10, FastEndpoints, EF Core (SQLite), SignalR hub (`/hubs/party`), cookie auth, Mapster |
| Frontend | Nuxt 4 (SPA), Vue 3 + TypeScript, Quasar (`nuxt-quasar-ui`), UnoCSS, Zod |
| Realtime video | WebRTC mesh with STUN only (no TURN — peers behind strict NATs may not connect) |

## Run it

Prerequisites: .NET 10 SDK, Node.js 20+, pnpm.

```bash
# terminal 1 — backend on http://localhost:5000
pnpm watch

# terminal 2 — frontend on http://localhost:5101
pnpm dev
```

The frontend dev server proxies `/api` and `/hubs` to the backend. The SQLite database
(`backend/vshot.db`) is created and migrated automatically on first backend start.

## How a party works

1. Open <http://localhost:5101>, pick a nickname and an emoji face.
2. **Create lobby** → you get a lobby number like `K7X2M4`.
3. Friends open the site, punch in the number, and join the video room.
4. Chat, spam reactions, mute your mic or camera.
5. Anyone hits **START ROUND** → everyone sees the same 3-2-1 countdown → **CHEERS!** → mark your shot with **I DRANK!** or the **TAKE A SHOT** button.

Drink responsibly. 💛

## Project layout

```
/
├── backend/   # .NET 10 API + SignalR party hub (FastEndpoints, EF Core)
└── client/    # Nuxt 4 SPA (Quasar, UnoCSS, WebRTC mesh)
```

See `AGENTS.md` for contributor conventions.
