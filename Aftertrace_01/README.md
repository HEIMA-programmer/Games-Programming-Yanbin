# Aftertrace

A 2D puzzle-platformer where you **record your own movements and replay them as a ghost
clone** — cooperating with yourself to solve puzzles that need two bodies.

## The idea

You play a lone machine waking up in an abandoned facility, 94 % of its memory corrupted.
Your one ability: hold **R** to record up to five seconds of movement, then release to
spawn a glowing *echo* of yourself that replays exactly what you just did. Use the echo to
hold a pressure plate open, stand on its head to clear a wall, push a crate that replays
its recorded path, or send it ahead as a decoy that a searchlight drone will chase — then
do your half of the job while your past self does the other.

Every room is a small loop: **observe → plan → record → act → solve.** Between levels,
four illustrated cutscene acts piece together whose recordings you keep finding — the
story ends when the last trace is recovered.

## Controls

| Action | Key |
| ------ | --- |
| Move | `A` / `D` (or `←` / `→`) |
| Jump | `Space` / `W` / `↑` (variable height — hold for higher) |
| Record echo | Hold `R` (up to 5 s); release to spawn the clone |
| Push a crate | Walk into its side (no extra key) |
| Dialogue beats | Any key / click — skip the typewriter, then continue |
| Cutscenes | `Space` / `Enter` / click — reveal caption, then next slide; `Esc` skips the act |
| Pause | `Esc` (in levels) |
| Menus | Mouse; `Esc` closes overlay panels |

Echo rules: only one echo exists at a time (a new recording dissolves the old one); you can
**stand on your echo** — it's a one-way platform from above; any crate you moved while
recording rewinds and **replays its exact path** alongside the clone, and stays where the
replay ends.

## How to run

The game targets **Unity 2022.3.62f3 (LTS)** with the Universal Render Pipeline (2D
renderer). All assets are committed, so it runs straight from a clone:

1. Open the `Aftertrace_01` folder in Unity Hub (Unity 2022.3.62f3).
2. Open `Assets/_Scenes/MainMenu.unity`.
3. Press **Play**, then **Start Game** — the opening cutscene leads into Level 0.
   (**Level Select** jumps straight into levels.)

To make a standalone build, use **File ▸ Build Settings ▸ Build** (the scene list is
already configured).

**A note on the pipeline.** The project began fully procedural (editor scripts generated
every sprite, sound, prefab and scene). It evolved deliberately — documented in the
[dev log](../Docs/DevLog/) — and the shipped scenes are now **hand-authored** and are the
source of truth. Current editor tooling lives under the **`Aftertrace`** menu (cutscene
builder, sprite/art tools, audio regeneration); the retired procedural level builders are
fenced behind `Aftertrace ▸ Legacy (DANGER — overwrites hand-made scenes)` and produce
gameplay blockouts only — don't run them on the shipped scenes.

## The game

| Scene | Name | What it is |
| ----- | ---- | ---------- |
| `Cut_00`–`Cut_03` | Acts 1–4 | Illustrated story interludes between levels (typewriter captions, skippable) |
| `Level_00` | Awakening | The echo tutorial: movement, hazards, and the record-and-replay contract |
| `Level_01` | Sector 01 — Playroom | Dual-mechanic puzzles: pushable (and recordable) crates, proximity mines, a lift, and a gate that provably needs crate *and* echo |
| `Level_02` | Sector 02 — Hide and Seek | Five-segment stealth: volumetric searchlight drones with a gaze-alarm meter, decoy-stun passes, an echo step-stool wall, and crate-as-mobile-cover |

Flow: Menu → Act 1 → Level 0 → Act 2 → Level 1 → Act 3 → Level 2 → Act 4 → Menu.
One memory fragment ("recovered trace") per level — 3 in total; progress persists across
runs. About 10–15 minutes of play. (A fourth level existed as a blockout and was
deliberately cut to keep depth over count — `Level_03.unity` remains in the repo,
unreferenced, as part of the project's history.)

## Accessibility, legal & ethical notes

- **Input:** keyboard for play, mouse for menus. Movement and jump each accept multiple
  keys; no simultaneous-press requirements.
- **Feedback is paired:** important events have both a visual and an audio cue (record
  vignette, plate light + click, drone beam brightness + alarm meter, door animation +
  sound), so no single channel is required.
- **No harsh strobing** — hazards telegraph with a slow alpha pulse; lighting is soft.
  Pacing is calm: hazards are cyclical and readable, the stealth meter fills gradually,
  and failure means a checkpoint respawn, not lost progress.
- **Privacy/security:** the game is fully offline and collects no data.
- **Assets:** code, level design, tooling and all sound effects are original; sprites and
  fonts use licensed CC0 / royalty-free / OFL packs; music is licensed from OpenGameArt;
  the ten cutscene illustrations are AI-generated and disclosed. Full per-asset details in
  [`CREDITS.md`](CREDITS.md).

## Credits

- **Design, programming, level design, tooling, audio synthesis:** Yanbin Xu
- **Sprites (1-Bit baseline):**
  [CraftPix Sci-Fi Platformer 1-Bit Game Kit](https://craftpix.net/freebies/free-sci-fi-platformer-1-bit-pixel-art-game-kit/)
  (royalty-free) + [Kenney UI Pack: Sci-Fi](https://kenney.nl/assets/ui-pack-sci-fi) (CC0).
- **Fonts (OFL):** [VT323](https://fonts.google.com/specimen/VT323) (titles/terminal) ·
  [Exo 2](https://fonts.google.com/specimen/Exo+2) (body) ·
  [Orbitron](https://fonts.google.com/specimen/Orbitron) (fallback display).
- **Sound effects:** generated procedurally at edit time (16-bit PCM WAV, deterministic).
- **Music (OpenGameArt, CC0 / CC-BY 4.0):** skrjablin, Yoiyami, Mega Pixel Music Lab,
  Memoraphile / You're Perfect Studio, and Gregor Quendel's music-box arrangements of
  Schumann's *Scenes from Childhood* — per-track licences in [`CREDITS.md`](CREDITS.md).
- **Cutscene illustrations:** AI-generated to the project's 1-Bit + cyan style guide,
  curated and integrated by the developer (disclosed in [`CREDITS.md`](CREDITS.md)).
- **Engine & tools:** Unity 2022.3 LTS, Universal Render Pipeline, TextMesh Pro.

## How this project was built

Developed in the open with a session-by-session record so the process — not just the final
build — can be followed:

- **Concept:** [Game Concept Document](../5.19-Aftertrace%20%E2%80%94%20Game%20Concept%20Document.md)
- **Dev log:** [`../Docs/DevLog/`](../Docs/DevLog/) — ten sessions, from one-day prototype
  to the v1.0.0 presentation build
- **Playtest notes:** [`../Docs/PlayTestNotes/`](../Docs/PlayTestNotes/) — five rounds,
  including the peer playtests that re-shaped the art direction and the final fix list

## Project structure

```
Aftertrace_01/
├── Assets/
│   ├── _Scenes/     MainMenu, Cut_00…Cut_03, Level_00…Level_02 (+ the cut Level_03)
│   ├── Scripts/     Player, Echo, Environment, Enemy, UI, App, Management, Camera, Effects, Level
│   ├── Editor/      Aftertrace tooling (cutscene/art/audio) + retired legacy builders
│   ├── Art/         Imported/ (CraftPix, Kenney) · Sprites/ · Palettes/ · images/ (cutscenes)
│   ├── Audio/       procedural SFX WAVs · Music/ (licensed OGG/MP3 tracks)
│   ├── Fonts/  Prefabs/  Resources/  Settings/
│   └── ...
├── CREDITS.md       full asset/licence list
└── README.md
```
