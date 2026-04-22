# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**The Touch Games** — a Unity 2022.3.20 iOS mini-game collection. The project plans 7 event types (ROYGBIV: Tapper, Swiper, Beeper, Rotator, Timer, Balancer, Tracer). Currently, Sprint Swiper is implemented and running on device.

- Bundle ID: `com.thetouchgames.app`
- Target: iOS 12.0+, landscape orientation locked
- C# 9.0 / .NET Standard 2.1

## Build & Run

Open the project in **Unity 2022.3.20**. There is no CLI build step — use the Unity Editor.

- **Play in Editor**: Open a scene (e.g., `Assets/Scenes/Swiper.unity`) and press Play. Mouse input simulates touch in the editor.
- **Deploy to device**: Build via *File → Build Settings → iOS*, then open the generated Xcode project and deploy. Device ID used during development: `00008030-000964D22182402E`.
  ```
  sudo xcode-select --switch /Applications/Xcode.app
  xcodebuild test -destination "platform=iOS,id=00008030-000964D22182402E" -scheme Unity-iPhone
  ```
- **Tests**: Unity Test Framework is included (`com.unity.test-framework 1.1.33`) but no tests are written yet.

## Architecture

### Scene / Script layout

Each mini-game has its own Unity scene and a MonoBehaviour script of the same name, both in `Assets/Scenes/`. Shared utilities live in `Assets/Namespaces/`.

| File | Purpose |
|---|---|
| `Assets/Scenes/Menu.cs` | Title screen; 2-second intro then shows game-select buttons; loads scenes by name |
| `Assets/Scenes/Swiper.cs` | Sprint Swiper game loop — swipe between two target zones, count down from N, stop timer |
| `Assets/Namespaces/Helpers.cs` | `Timing.CalculateFinalDelta()` — sub-frame accurate hit time |

### Sub-frame timing (key pattern)

Unity runs at 30 fps on device, so `Time.deltaTime` alone gives ~33 ms resolution. `CalculateFinalDelta` in `Helpers.cs` corrects this: it ray-casts (`bounds.IntersectRay`) from the previous (outside) position to the current (inside) position, computes how far into the frame the boundary crossing actually occurred, and returns an adjusted delta. This is used at the end of each swipe to get millisecond-level accuracy.

```
CalculateFinalDelta(bounds, previousPoint, currentPoint, frameDelta)
  → fractional time (float, seconds) of the actual boundary crossing
```

### Input handling

`Application.isEditor` distinguishes editor (mouse) from device (touch). On device, `Input.touchCount == 1` is enforced to prevent multi-touch cheating. Screen-to-world conversion uses `Camera.main.ScreenToWorldPoint`.

### Planned event types (from `working.txt`)

- **Swiper**: Sprint (100 swipes, 2 spots), Middle-distance (400 swipes, 4 corners), Marathon (40 000 swipes)
- **Tapper**: 100 taps between two closer spots
- **Beeper**: Drag between points with a timing threshold window (not too early, not too late)
- **Rotator**: Rotate 5 times, release between arcs
- **Timer**: Hit 5 random beats; hold and hit fast marker on 5th
- **Balancer**: Two-finger balance challenge
- **Tracer**: Follow a path (figure-of-8?)

### Leaderboards (planned)

Evaluated options: CloudOnce (`cloudonce.github.io`) and iOS Game Center Plugin from the Asset Store.
