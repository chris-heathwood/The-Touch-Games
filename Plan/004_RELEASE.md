# App Store Release Plan

## Status: Pre-release

---

## 1. In-Unity Tasks

- [ ] Add Leaderboard Button GameObject to each of the 7 game scenes and assign in Inspector
- [ ] Add Game Center capability verified via XcodePostBuild (automated)
- [ ] Set correct Leaderboard ID in Inspector for Swiper Sprint, Middle, Marathon scenes
- [ ] Confirm Build Settings scene order: MadeWithClaude (0), ProducedByMrH (1), Splash (2), Menu (3), games...
- [ ] Set final Version (e.g. `1.0`) and Build number in Player Settings

---

## 2. TestFlight Testing Checklist

Test each game end-to-end on device:

- [ ] Swiper Sprint, Middle, Marathon
- [ ] Tapper
- [ ] Beeper
- [ ] Rotator
- [ ] Timer
- [ ] Balancer
- [ ] Tracer
- [ ] Menu scroll and scene navigation
- [ ] Back to menu returns to correct row
- [ ] Splash → credit screens → menu flow
- [ ] Game Center authentication (sign in with sandbox account)
- [ ] Score submitted after each game
- [ ] Leaderboard button opens native Game Center UI

---

## 3. App Store Connect — Metadata

All required before submission:

- [ ] **App name**: The Touch Games
- [ ] **Subtitle** (optional, 30 chars): e.g. "7 mini-games, one leaderboard"
- [ ] **Description**: what the app is, what the games are
- [ ] **Keywords**: games, touch, reaction, speed, arcade (30 keywords max)
- [ ] **Category**: Games → Arcade (primary), Games → Sports (secondary)
- [ ] **Age Rating**: complete the questionnaire (likely 4+)
- [ ] **Support URL**: needs a live URL — even a simple GitHub page or link-in-bio works
- [ ] **Privacy Policy URL**: required by Apple — see section 4 below

---

## 4. Privacy Policy

Apple requires a privacy policy URL for all apps, even if the app collects no data.

- [ ] Write a one-page privacy policy stating the app collects no personal data
- [ ] Host it somewhere public (GitHub Pages, Notion public page, or similar)
- [ ] Paste the URL into App Store Connect

---

## 5. Screenshots

Required sizes (can be done in simulator or on device):

- [ ] **6.9" display** (iPhone 16 Pro Max) — 1320×2868px — at least 3 screenshots
- [ ] **6.5" display** (iPhone 14 Plus) — optional but recommended
- [ ] Screenshots should show: menu, at least 3 different games, end screen with score

Tip: use the iOS Simulator in Xcode (File → New Simulator) to capture clean screenshots.

---

## 6. App Privacy Questionnaire (App Store Connect)

Under **App Privacy**, declare what data the app collects:

- [ ] Game Center handles its own data — declare "Data Not Collected" for everything else

---

## 7. Privacy Manifest (Technical — Unity/Xcode)

Apple requires a `PrivacyInfo.xcprivacy` file for iOS 17+ apps. Unity 2022.3.20 may not generate one automatically.

- [ ] Check if Unity generates a PrivacyInfo.xcprivacy in the Xcode build
- [ ] If not, add one declaring no API usage beyond Game Center

---

## 8. Final Build & Submit

- [ ] Increment Build number in Unity Player Settings
- [ ] Build → Archive → Upload in Xcode
- [ ] Wait for processing in App Store Connect
- [ ] Select build under the App Store tab (not TestFlight)
- [ ] Complete all metadata and screenshots
- [ ] Click **Submit for Review**

---

## 9. After Approval

- [ ] Submit Game Center leaderboards for review (required to go live on App Store)
- [ ] Set leaderboards to Live in App Store Connect
- [ ] Share App Store link

---

## Outstanding

- Claude logo usage rights — confirm with Anthropic before submission
