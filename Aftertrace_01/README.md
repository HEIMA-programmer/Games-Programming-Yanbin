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
renderer).

**One asset must be downloaded separately — the CraftPix art kit.** Its free licence
allows shipping the sprites inside a game but forbids redistributing the source PNGs, so
this public repository carries only the project's `.png.meta` files (import settings +
sprite slicing), not the images themselves. To restore them in a fresh clone:

1. Download the free
   [Sci-Fi Platformer 1-Bit Game Kit](https://craftpix.net/freebies/free-sci-fi-platformer-1-bit-pixel-art-game-kit/)
   from CraftPix.
2. Copy its PNGs into `Assets/Art/Imported/CraftPix1Bit/`, next to the committed
   `.png.meta` files — same subfolders and file names as in the kit
   (`Enemies/`, `GUI/`, `Main_Characters/`, `Objects/`, `Tileset/`, `Traps/`).
3. Do this **before opening the project in Unity for the first time** — Unity deletes
   orphaned `.meta` files on open, and with them the GUIDs and sprite slicing that every
   scene and prefab references. (A copy that already includes the PNGs — e.g. a
   submitted archive — just runs.)

The same licence rule covers **seven standalone UI sprites cropped from the kit's sheets**
in `Assets/Art/Sprites/UI/` (the three dialogue portraits, the riveted screen frame, the
terminal frame, the plate pill and the lift platform): their PNGs are local-only too, with
tracked `.meta` files. A fresh clone restores them by re-cropping from the downloaded kit —
each file's source sheet and cells are listed in
`Assets/Art/Imported/SPRITESHEET_NOTES.md` (§ "standalone crops") — or by copying the
folder from a working copy or the submitted archive. (`white.png` in the same folder is
project-generated and ships in the repo.)

Then:

1. Open the `Aftertrace_01` folder in Unity Hub (Unity 2022.3.62f3).
2. Open `Assets/_Scenes/MainMenu.unity`.
3. Press **Play**, then **Start Game** — the opening cutscene leads into Level 0.
   (**Level Select** jumps straight into levels.)

To make a standalone build, use **File ▸ Build Settings ▸ Build** (the scene list is
already configured).

**A note on the pipeline.** The project began fully procedural (editor scripts generated
every sprite, sound, prefab and scene). It evolved deliberately — documented in the
[dev log](../Docs/DevLog/) — and the shipped gameplay scenes and menu are now
**hand-authored** and are the source of truth. The procedural level builders were first
retired behind a guarded `Legacy` menu (v0.8.0) and then **deleted outright in v1.1.0
(PR #155)** — they survive only in git history. The `Aftertrace` editor menu now has
exactly two entries: **Build Cutscene Scenes** (regenerates `Cut_00`–`Cut_03` from the
data table in `EchoCutscenes.cs` — the four cutscene scenes are tool-built, unlike the
hand-authored levels) and **Art ▸ Regenerate Audio Only** (re-synthesises the SFX WAVs in
place). The retained `EchoArt.cs` source documents how the procedural sprites were
generated but has no menu entry.

## The game

| Scene | Name | What it is |
| ----- | ---- | ---------- |
| `Cut_00`–`Cut_03` | Acts 1–4 | Illustrated story interludes between levels (typewriter captions, skippable) |
| `Level_00` | Awakening | The echo tutorial: movement, hazards, and the record-and-replay contract |
| `Level_01` | Sector 01 — Playroom | Dual-mechanic puzzles: pushable (and recordable) crates, proximity mines, a lift, and a gate that provably needs crate *and* echo |
| `Level_02` | Sector 02 — Hide and Seek | Five-segment stealth: volumetric searchlight drones with a gaze-alarm meter, decoy-stun passes, an echo step-stool wall, and crate-as-mobile-cover |

Flow: Menu → Act 1 → Level 0 → Act 2 → Level 1 → Act 3 → Level 2 → Act 4 → Menu.
One **optional** memory fragment ("recovered trace") per level — 3 in total; fragments
are collectibles, not completion requirements (the exit door is what finishes a level),
and collected fragments persist across runs. About 5–10 minutes of play. (A fourth level existed as a blockout and was
deliberately cut to keep depth over count; the blockout and its retired generator
were removed — the decision is documented in the Docs/ dev logs.)

- **Objective:** solve each room with your echo, recover the memory fragments, and
  reach each level's exit — the four story acts reveal whose recordings you keep
  finding.
- **Win / completion:** reaching the exit door finishes a level; finishing Act 4 marks
  the game complete and returns to the menu (progress persists between runs).
- **Lose:** there is no permanent fail state — hazards, mines and drones respawn you at
  the last checkpoint.

## Known issues & limitations

No open gameplay bugs are known in the shipped build (v1.1.1 — see
[`Docs/CHANGELOG.md`](../Docs/CHANGELOG.md)). Current limitations, with how they are
managed, are listed in [`Docs/PROJECT_REPORT.md`](../Docs/PROJECT_REPORT.md) §9 — the
short version: keyboard/mouse only (no gamepad), desktop builds only, in-game text is
English only, one save profile, and fresh clones must restore the CraftPix kit before
first open (see *How to run* above).

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
  the ten cutscene illustrations are AI-generated and disclosed. The CraftPix kit's source
  PNGs — and the seven standalone UI sprites cropped from its sheets — are excluded from
  the public repository per its licence (see *How to run*). Full per-asset details in
  [`CREDITS.md`](CREDITS.md).

## Credits

- **Design, programming, level design, tooling, audio synthesis:** Yanbin Xu
- **Sprites (1-Bit baseline):**
  [CraftPix Sci-Fi Platformer 1-Bit Game Kit](https://craftpix.net/freebies/free-sci-fi-platformer-1-bit-pixel-art-game-kit/)
  (royalty-free; source PNGs not redistributed here — see *How to run*).
- **Fonts (OFL):** [VT323](https://fonts.google.com/specimen/VT323) (terminal/captions) ·
  [Exo 2](https://fonts.google.com/specimen/Exo+2) (body) ·
  [Jersey 10](https://fonts.google.com/specimen/Jersey+10) (menu title) ·
  [Orbitron](https://fonts.google.com/specimen/Orbitron) (ships unused).
- **Sound effects:** generated procedurally at edit time (16-bit PCM WAV, deterministic).
- **Music (OpenGameArt, CC0 / CC-BY 4.0):** skrjablin, Yoiyami, Mega Pixel Music Lab,
  Memoraphile / You're Perfect Studio, and Gregor Quendel's music-box arrangements of
  Schumann's *Scenes from Childhood* — per-track licences in [`CREDITS.md`](CREDITS.md).
- **Cutscene illustrations:** AI-generated to the project's 1-Bit + cyan style guide,
  curated and integrated by the developer (disclosed in [`CREDITS.md`](CREDITS.md)).
- **Engine & tools:** Unity 2022.3 LTS, Universal Render Pipeline, TextMesh Pro.
- **Full declarations:** field-by-field external-resource and AI-assistance
  declarations (including AI coding support) live in
  [`Docs/DECLARATIONS.md`](../Docs/DECLARATIONS.md).

## How this project was built

Developed in the open with a session-by-session record so the process — not just the final
build — can be followed:

- **Design document:** [`Docs/DESIGN.md`](../Docs/DESIGN.md) — the design as shipped
  (supersedes the original concept document, which lives in git history)
- **Plan:** [`Docs/PLAN.md`](../Docs/PLAN.md) — week-by-week plan and outcomes
- **Dev log:** [`../Docs/DevLog/`](../Docs/DevLog/) — eleven sessions, from one-day
  prototype through the v1.1.x endgame and the documentation rebuild
- **Playtest notes:** [`../Docs/PlayTestNotes/`](../Docs/PlayTestNotes/) — five rounds,
  including the peer playtests that re-shaped the art direction and the final fix list

## Project structure

```
Aftertrace_01/
├── Assets/
│   ├── _Scenes/     MainMenu, Cut_00…Cut_03, Level_00…Level_02
│   ├── Scripts/     Player, Echo, Environment, Enemy, UI, App, Management, Camera, Effects
│   ├── Editor/      Aftertrace tooling: cutscene builder, audio generator, retained art-generator source
│   ├── Art/         Imported/ (CraftPix — PNGs local-only) · Sprites/ · Palettes/ · images/ (cutscenes)
│   ├── Audio/       procedural SFX WAVs · Music/ (licensed OGG/MP3 tracks)
│   ├── Fonts/  Prefabs/  Resources/  Settings/
│   └── ...
├── CREDITS.md       full asset/licence list
└── README.md
```
