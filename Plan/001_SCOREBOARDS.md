# Scoreboards Plan

## Approach
Use **Apple Game Center** via Unity's built-in `Social` API (wraps GameKit on iOS).
- No third-party plugin or backend required
- Native iOS leaderboard UI provided by Apple
- Friends comparison, global rankings, regional rankings all built in
- Requires Apple Developer account (already in progress)

---

## Setup Steps

### 1. App Store Connect
- Log in to appstoreconnect.apple.com
- Create an App entry for `com.thetouchgames.app`
- Under **Services → Game Center**, create one leaderboard per game variant (see IDs below)
- Set score format to **Integer** (scores are whole numbers), sort order **High to Low**

### 2. Xcode
- In the Unity-generated Xcode project, go to **Signing & Capabilities**
- Click **+ Capability** → add **Game Center**

### 3. Unity
- No package installs needed — `UnityEngine.SocialPlatforms` is built in
- Add a `GameCenter.cs` helper (singleton) that handles auth and score submission
- Call auth once at app start (Splash or Menu scene)
- Submit score at `EndGame()` in each game
- Show native leaderboard UI from the end screen

---

## Leaderboard IDs

| Game | Leaderboard ID |
|---|---|
| Swiper Sprint | `com.thetouchgames.swiper.sprint` |
| Swiper Middle | `com.thetouchgames.swiper.middle` |
| Swiper Marathon | `com.thetouchgames.swiper.marathon` |
| Tapper | `com.thetouchgames.tapper` |
| Beeper | `com.thetouchgames.beeper` |
| Rotator | `com.thetouchgames.rotator` |
| Timer | `com.thetouchgames.timer` |
| Balancer | `com.thetouchgames.balancer` |
| Tracer | `com.thetouchgames.tracer` |

These IDs must be entered exactly in App Store Connect and referenced in code.

---

## Code Plan

### GameCenter.cs (new shared script in Assets/Namespaces/)
```
- Authenticate() — call once at startup, silent fail if Game Center unavailable
- ReportScore(long score, string leaderboardId) — submit score after each game
- ShowLeaderboard(string leaderboardId) — open native Game Center UI
```

### Changes to each game script
- After `EndGame()`, call `GameCenter.ReportScore(score, leaderboardId)`
- Add a **Leaderboard** button to the end screen that calls `GameCenter.ShowLeaderboard(leaderboardId)`

### Score types per game
| Game | Score value |
|---|---|
| Swiper | Time in milliseconds (lower = better — need a High to Low workaround, e.g. `999999 - ms`) |
| Tapper | Time in milliseconds (same) |
| Beeper | Swipe count (higher = better) |
| Rotator | Score 0–1000 (higher = better) |
| Timer | Score 0–1000 (higher = better) |
| Balancer | Time survived in milliseconds (higher = better) |
| Tracer | Score 0–2000 (higher = better) |

> **Note on time-based games (Swiper, Tapper):** Game Center only supports High to Low scoring.
> Submit `999999 - timeInMs` so a faster time = higher score on the board.

---

## Outstanding Questions
- Do you want a global leaderboard only, or also a friends-only view? (Game Center provides both automatically)
- Should leaderboards be visible before the player has a score, or only after they've played?

---

## Android (Future)

Google's equivalent is **Play Games Services**. Works the same concept — native leaderboard UI, free, no backend.

Key difference: Unity's built-in `Social` API does **not** wrap Play Games on Android. Would need the **Google Play Games Unity plugin** (free, open source, maintained by Google).

The `GameCenter.cs` helper should be written with an abstraction layer so Android can slot in later without changing the game scripts — e.g. a single `Scoreboards.ReportScore()` call that routes to Game Center on iOS and Play Games on Android.

This is low priority until an Android release is planned.
