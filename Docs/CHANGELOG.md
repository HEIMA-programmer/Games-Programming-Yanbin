# Changelog

All notable changes to **Echo Shift** are recorded here. Format follows
[Keep a Changelog](https://keepachangelog.com/); the project aims to follow
[Semantic Versioning](https://semver.org/). Each entry corresponds to a day's work logged
in [`DevLog/`](DevLog/).

## [Unreleased]

_Work in progress for the next session._

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
