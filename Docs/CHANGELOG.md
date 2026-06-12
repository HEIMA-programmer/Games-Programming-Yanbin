# Changelog

All notable changes to **Aftertrace** are recorded here. Format follows
[Keep a Changelog](https://keepachangelog.com/); the project aims to follow
[Semantic Versioning](https://semver.org/). Each entry corresponds to a day's work logged
in [`DevLog/`](DevLog/).

## [Unreleased]

_Nothing yet._

## [1.1.1] — 2026-06-12

PR #156 + licence-compliance commit `0d56e63`.

### Changed

- L2's "rub eyes" coaching beat now fires the first time a drone is **decoy-stunned**
  (new `StoryTrigger.fireOnDroneStun` mode subscribed to a `PatrolDrone.OnStunned`
  event) instead of at a mid-corridor position — the lesson lands when the player first
  proves the mechanic.
- Drone stun timers pause during story freeze, so the escape window the beat advertises
  is still intact when control returns.

### Removed

- **CraftPix kit source PNGs untracked from the public repo** — the kit's licence
  forbids redistributing source files. `.png.meta` files (GUIDs + slicing) stay tracked;
  the game README documents the one-step kit restore for fresh clones.
- Unused **Kenney UI pack** (528 tracked files, zero scene/prefab references).

### Docs

- README / CREDITS licence notes for the above; root README clarifies the MIT licence
  covers original content only.

## [1.1.0] — 2026-06-12

Session 10 follow-up · PR #155 — presentation polish across the whole flow.

### Added

- **Level 1 expansion (34u → 50u):** S3 freight minefield — two chained crates pushed
  through proximity mines that key on *your* mass (cargo rolls through clean); S4
  three-lock finale — a latching gate that needs **crate + echo + player** on three
  plates in the same instant, paced by a sprint mine.
- **Jersey 10** menu title with a cyan TMP-underlay "3D" extrusion (OFL, licence
  committed).
- CC0 backdrops: ansimuz industrial skyline silhouettes + Bonsaiheldin starfield,
  tinted dim as camera-following layers in the menu and all levels.
- Diamond fragment art unified: world pickup, spin animation frames, HUD icon.

### Changed

- **Story freeze now freezes the world:** while a blocking beat is on screen, drones
  (movement + contact), mine fuses, trap damage and echo replay all pause with the
  player — a beat next to a hazard is no longer a death sentence.
- Cutscene acts 1–3 share their following level's music track, making each act the
  level's seamless prelude (same-clip dedupe ramps instead of restarting).

### Fixed

- L2 exit door had lost its AudioSource/clips in the saved scene — re-wired and
  verified from disk.
- Narrative terminal flashed the previous beat's last line while fading in.
- Cutscene caption plate now actually renders (sprite-backed translucent band) and the
  `[ SPACE ]` hint sits inside the riveted screen frame.
- L1 floor seam (old end-cap tile column) and the final door/plate overlap.

## [1.0.1] — 2026-06-11

PR #154 — process record brought up to date: Session 03–10 dev logs, Playtests 03–05,
Session 01/02 + Playtest 02 errata, READMEs updated to the v1.0.0 build.

## [1.0.0] — 2026-06-11

Session 10 · PR #153 — **the presentation build**: the story gets an ending.

### Added

- **Four illustrated cutscene acts** (`Cut_00`–`Cut_03`) between levels: AI-generated
  stills (disclosed in CREDITS), 0.6 s cross-fades, typewriter captions with per-glyph
  blips, SPACE to reveal/advance, ESC to skip; Act 4 marks the game completed and
  returns to the menu.
- **Licensed soundtrack** (OpenGameArt, per-track licences in CREDITS): C64 lullaby
  (menu), *First Light Particles* (L0), *Forgotten Lullaby* music box (L1), *Spooky
  Dungeon* (L2), Quendel's music-box *Reverie* (ending) — with **drift-through-silence**
  transitions (4.5 s squared drift-out begun at scene-fade start, a beat of silence,
  2.5 s bloom-in) replacing hard crossfades.

### Changed

- **Level 3 cut.** The story now ends after Level 2; scene list, progression and level
  select updated. Decision and reasoning recorded in DevLog Session 10 (depth over
  count; finish the *story*, not a fourth blockout).

### Fixed

- Wall-climb exploit (narrowed ground-check so walls stopped counting as floor),
  jump-over-drone cheese, Sokoban crate-chain push hand-over, scene-load flash,
  L0 HUD counter + pause issues, exit-door audio.

## [0.9.1] — 2026-06-10

Session 09 · PR #152 — **Level 2 rebuilt as "Hide and Seek"**, a five-segment stealth
composite: volumetric searchlight drones with a unified gaze-alarm meter, a decoy-stun
pass, an echo step-stool wall, and a crate-as-mobile-cover sentinel zone. Fixed drone
contact triggering, endpoint chase-break, and stunned-contact lethality.

## [0.9.0] — 2026-06-10

Session 08 · PR #151 — **echo-recorded crate system + Level 1 rebuilt as "Playroom"**:
crates pushed during a recording rewind and replay their path; proximity mines; kit-art
lift; a dual-plate gate that provably needs crate *and* echo. Fixed `MovingPlatform`
endpoint freeze and player grounding.

## [0.8.0] — 2026-06-10

Session 07 · PR #150 — **Level 0 hand-authored** over the frozen blockout via editor
tooling; the procedural scene builders retired behind `Aftertrace ▸ Legacy (DANGER)`.
Diegetic dialogue box + portraits, riveted screen-frame UI, and the project-wide
**384×216 / PPU 32 pixel-perfect camera** standard. Sprite grounding fixes.

## [0.7.0] — 2026-06-01

Session 06 · PR #149 — pixel-perfect rendering + a real sprite-animation set for the
player/echo, and environment art switched to hand-authored tile palettes over clean
gameplay blockouts (the bridge between generated and hand-made scenes).

## [0.6.0] — 2026-05-29

Session 3 · M2 opener — Ride Your Echo mechanic + 1-Bit art baseline + menu/HUD polish.

### Added

- **Ride Your Echo (core mechanic).** `EchoClone` gains a child `Standpoint` (BoxCollider2D +
  one-way PlatformEffector2D, surfaceArc 170°) on the Ground layer. Per-frame `OverlapBoxAll`
  above the surface carries riders by the echo's delta — avoids OnCollisionEnter/Exit edge
  cases the effector creates. Trigger circle on the root is unchanged so pressure plates
  still fire; `BeginDissolve` clears the standpoint so the rising echo can't drag the player up.
- **External 1-Bit art baseline.** Sprites switch from `EchoArt` procedural to a curated
  CC0 / royalty-free pack (CraftPix 1-Bit Sci-Fi Platformer Kit + Kenney UI Pack: Sci-Fi).
  `EchoSpriteSlicer` slices every sheet on a per-asset grid; `EchoImportedAssetSettings`
  (an `AssetPostprocessor`) configures Point filter, per-folder PPU, and FullRect mesh.
  `EchoBuildUtils.LoadSprite` maps logical names to named frames; replaced generators
  stay in source as fallback.
- **9-sliced rounded scene frame** (`EchoBuildUtils.AddSceneFrame`) on every canvas —
  menu, HUD, pause, victory — for the framed-scene look of the source kit's promo art.
- **VT323** retro pixel display font (OFL) baked alongside Exo 2 / Orbitron. `EchoFont`
  bakes both as Static TMP assets; `CreateTitleText` routes titles through VT323.
- **Multi-tile variety.** `SolidBlock` picks `platform_wall` for tall walls,
  `platform_ceiling` for high horizontal blocks, default for ground — so the level reads
  as a constructed lab rather than one repeated tile.
- **Dense scene composition.** Menu hero diorama (planet hero + NPC silhouettes flanking
  + ground vignette + corner accents). Level: 13 large 1-Bit backdrops, 60 mid-layer
  Tileset_details circuits, 8 NPC silhouettes, 14 ground props — placed at parallax
  factors 0.08 / 0.32 / 0.5 / 1.0.
- **Modern HUD.** Single gem icon + display-font "X / Y" counter (was a row of three
  outline / filled diamonds). `HUDController` refactored.
- **Checkpoint visual.** `BuildCheckpoint` now ships a CraftPix flag-pole sprite +
  cyan-mint PointLight (was collider-only).
- **Editor tooling.** `EchoSpriteSlicer`, `EchoImportedAssetSettings`, `EchoSpritePicker`
  (`Aftertrace ▸ Sprite Picker` — visual frame browser to find sprite names).
- **`CREDITS.md`** at the repo root listing every external asset / font.
- **Orbitron** + **OFL** files (display-font fallback).

### Changed

- **1-Bit white tints** (`TintPlayer / TintEcho / TintDrone / TintDoor / TintFragment /
  TintEnd`) so the monochrome pop survives — cyan is reserved for HUD / lighting accents.
- **Background tones** are solid muted greys (`TintBgFar / TintBgMid / TintBgNear /
  TintNpc`), not transparent white wash — Obra Dinn / 1-Bit principle: recede via brightness,
  not alpha.
- **Camera background** is now pure `#000000` in every scene (was dark blue `#0a0e1a` for
  M1 levels and per-level hard-coded for Level 2 / 3).
- **Level 1 camera** zoomed in (orthographic size 6.5 → 5.4) for denser composition.
- **Wall narrative tactical panel** — 9-sliced button panel + VT323 outlined text
  replaces the stretched lab-tile + glow halo.
- **HowToPlay / LevelSelect dim panel** alpha 0.9 → 1.0 (no more menu bleed-through).
- **Menu buttons** sized up (360×74 → 460×92), labels pure white Bold at 36pt for
  stronger 1-Bit contrast; procedural button background regenerated with crisp white
  outline (was cyan).
- **`EchoArt.GenerateAll`** skips generators for sprites now sourced externally
  (`BuildPlayer / Echo / Platform / Door / Drone / EndArch / Fragment`) — functions stay
  in source as fallback.
- **ROADMAP.md M3 introduction** revised — the "100%-procedural sprites" principle is now
  the "M2+ art baseline" principle (procedural pipeline kept for everything else;
  sprites / fonts come from licensed packs tracked in `CREDITS.md`).

### Fixed

- Title outline now uses `Object.Instantiate(fontSharedMaterial)` so applying outlines no
  longer triggers Unity's "Instantiating material during edit mode" warning.
- `EchoSpriteSlicer` forces `SpriteMeshType.FullRect` on every sliced sheet so Tiled
  `drawMode` doesn't silently fall back to stretching (fixed flat ground texture).
- `CS0618` (`TextureImporter.spritesheet` obsolete) suppressed locally on the one call
  site — no stable replacement on Unity 2022.3.

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
