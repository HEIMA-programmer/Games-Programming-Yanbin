# Changelog

All notable changes to **Aftertrace** are recorded here. Format follows
[Keep a Changelog](https://keepachangelog.com/); the project aims to follow
[Semantic Versioning](https://semver.org/). Each entry corresponds to a day's work logged
in [`DevLog/`](DevLog/).

## [Unreleased]

_Nothing yet._

## [0.5.1] — 2026-05-28

Session 2 · Section C — Milestone-1 process backfill + rename.

### Added

- `Docs/ROADMAP.md` — the five-week, four-milestone plan (M1 vertical slice → M2
  mechanical depth → M3 art/world/audio identity → M4 climax + ship) with guiding
  principles, the label scheme, and GitHub-process recommendations.
- `Docs/BACKLOG.md` — copy-paste-ready issue / sub-issue text for the GitHub board: §1
  backfills PRs #1–#7 as closed "Done" Milestone-1 issues; §2 specs out every Milestone
  2–4 issue (Ride Your Echo, crate-carry, device pack, scanner enemy, "Shift" spike,
  art-direction & palette, normal-map generation, audio identity, finale, builds,
  v1.0.0 release, etc.).

### Changed

- **Project renamed: *Echo Shift* → *Aftertrace*.** `Echo_Shift_01/` is now
  `Aftertrace_01/`; the concept document, READMEs, DevLog header, screenshots index,
  `.gitattributes`, and Unity's `productName` all follow. The C# namespace `EchoShift`
  and the `EchoShift ▸ Build All` editor menu deliberately stay — they're code identifiers,
  not the player-facing title.

## [0.5.0] — 2026-05-27

Session 2 · Section B — polish pass (game feel, audio, font) + a same-day follow-up fix.

### Added

- UI font: the game now uses **Exo 2** (OFL) across menus, HUD, and narrative — generated from
  a committed `.ttf` during Build All, with a graceful fallback to the TMP default if absent.
- A particle burst when a pressure plate is pressed.
- A brief hit-stop + camera shake on death/respawn.
- Richer procedural audio: layered harmonics/transients on impacts and a pad layer under the music.

### Changed

- Audio synthesis now uses a fixed RNG seed, so the generated WAVs are identical on every
  Build All (no more spurious git diffs).

### Fixed

- The generated Exo 2 TMP font asset is now baked as **Static** with the UI's glyph set
  pre-included, so opening or playing the project no longer dirties `Exo2-Regular SDF.asset`
  in git (the old Dynamic asset rewrote its atlas on demand at runtime).

## [0.4.0] — 2026-05-26

Session 2 · Section A — real enemy detection + chase AI.

### Added

- Patrol drones gained a real line-of-sight sense + chase AI: a Patrol → Alert → Chase →
  Search → Return state machine. A drone sees the player or an echo clone through its
  detection cone (range + angle + an unobstructed ray on the Ground layer) and prefers the
  clone, so a recorded decoy reliably draws it off the path; the cone colour now follows the
  drone's state.
- Stealth detection rule (opt-in per drone): a drone that holds the player in its cone too long
  fills a detection meter and catches them, not just on contact. The screen reddens as a warning.
- Level 2: a low cover block past the first drone that teaches breaking line of sight.
- Level 3 corridor rebuilt as a stealth section: cover pillars block line of sight, so you hide
  in cover, time the cone sweeps, or record a clone — the drones lock their cones onto the decoy
  and leave your lane unwatched. (Level 2 drones keep the old behaviour for now.)
- Level 3 mirror room simplified to an optional "echo lift": hold the plate with a clone, ride
  up, and jump for the fragment. Removed a redundant mid-level door whose one-way gate could
  lock it permanently.

### Changed

- The detection cone is now functional (was decorative) and resized so the visible cone
  equals the real detection range.
- Chase speed (6) stays below the player's run speed (7.5) and a per-drone leash bounds how
  far a drone will chase — the player can always escape and is never cornered into a softlock.

## [0.3.0] — 2026-05-25

### Added

- Level_00 (intro) and Level_03.
- Level-select screen (`LevelButton`).
- `GameProgress` save layer tracking memory fragments, completion times, and overall
  progress across all levels.

## [0.2.0] — 2026-05-25

### Added

- Main menu, HUD, pause menu, and victory screen.
- Persistent app layer: `AudioManager`, `SceneFader`, `AppBootstrap`.
- Procedurally synthesised music and sound effects.
- `PatrolDrone` enemy, `MovingPlatform`, `Checkpoint` + respawn.
- Level_02.

## [0.1.0] — 2026-05-25

### Added

- Core echo mechanic: hold **R** to record up to 5s of motion, release to spawn a clone
  (`EchoRecorder`, `EchoClone`, `RecordedFrame`) that replays it and can hold a pressure
  plate.
- `PlayerController` (run, jump, facing, grounded check, respawn).
- `PressurePlate` + `Door` puzzle interaction.
- One-click Unity editor pipeline generating all sprites, audio, prefabs, and scenes from
  source (every asset original).
- Level_01 — first working puzzle (the MVP).
