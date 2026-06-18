# Aftertrace — Design Document

> **Final revision, written against the shipped v1.1.2 build (June 2026).**
> This document replaces the original 19 May concept document and the early
> `ROADMAP.md` / `BACKLOG.md` / `REDESIGN.md` planning files (all preserved in git
> history). It records the design **as shipped** — including what was cut and why —
> so the plan→result connection can be assessed against the actual game.
> Companions: [PLAN.md](PLAN.md) (weekly schedule) · [CHANGELOG.md](CHANGELOG.md)
> (version history mapped to PRs) · [DevLog/](DevLog/) (session-by-session record) ·
> [PlayTestNotes/](PlayTestNotes/) (six testing rounds) ·
> [game README](../Aftertrace_01/README.md) · [CREDITS](../Aftertrace_01/CREDITS.md).

## 1. Game title

**Aftertrace.** The project started as *Echo Shift* and was renamed on 2026-05-28:
"echo"-titled games are a crowded space (see §7), and the new name states the theme
directly — what you find in the facility are the *traces someone left after they were
gone*. The in-fiction word for a replayed recording is still an **echo**, so code
identifiers (`namespace EchoShift`, `EchoRecorder`, `EchoClone`) deliberately keep it.

## 2. One-sentence game idea

A 2D puzzle-platformer where you **record up to five seconds of your own movement and
replay it as a ghost clone**, cooperating with your past self to solve puzzles — and
recover the recordings someone else left behind.

## 3. Intended player experience

Quiet, melancholic, and clever — closer to a music box than an action game:

- **Thinky, not twitchy.** Every room is a small contract: *observe → plan → record →
  act → solve*. The reward feeling is "I out-thought the room with only my own body as
  a tool", never execution under pressure.
- **Calm by construction.** Hazards telegraph on slow cycles, drones chase slower than
  the player runs, failure costs a checkpoint respawn and nothing else. Tension comes
  from *planning under observation* (the stealth sector), not from punishment.
- **Lonely, then accompanied.** The emotional arc is engineered into the mechanic:
  you are alone, so you make copies of yourself for company and cooperation — while the
  story reveals you are doing exactly what a child once did with their recordings.
  The melancholic music-box soundtrack and the austere 1-bit world carry that tone.
- **Short and complete.** 5–10 minutes, three levels, four illustrated story acts,
  a real ending. Depth over count.

## 4. Core mechanic

**Echo recording.** Hold **R** to record up to 5 seconds of your own movement; release
to spawn a glowing echo that replays it exactly, once.

Rules (each one is load-bearing for puzzles):

| Rule | Why it exists |
| --- | --- |
| Only **one echo** exists; a new recording dissolves the old | keeps plans legible; forces commitment |
| The echo is a **one-way platform** — you can stand on its head | turns a recording into vertical reach ("ride your echo") |
| Echoes **press pressure plates** with real weight | the basic two-body puzzle currency |
| **Crates pushed while recording rewind and replay** their pushed path with the clone, then stay where the replay ends | recording becomes physical logistics: one recording can move cargo while you do something else |
| Drones see echoes **first** and chase them; touching a clone **stuns the drone** for a window | the same mechanic doubles as a stealth decoy |
| Echoes dissolve on hazards/mines; the player respawns | hazards interact with both bodies consistently |

The supporting cast: pressure plates + doors (including multi-lock gates that latch only
when **crate + echo + player** press three plates in the same instant), pushable crates
with Sokoban-style chain pushing, proximity mines keyed on player/echo mass (cargo rolls
through clean), cyclic hazard traps, a plate-driven lift, patrol drones with a
line-of-sight cone and a gaze-alarm meter, checkpoints, and one **optional** collectible
memory fragment per level. Fragments are pickups, not goals: a level ends when the player
walks through its exit door into the next story act, and finishing Act 4 marks the game
complete.

## 5. What the player does moment to moment

Second to second: run, jump (variable height), push crates, hold **R**, release, ride,
wait out a sweep, count a mine's fuse, dash through a stun window. Minute to minute, the
loop in every room:

1. **Read the room** — what needs two bodies at once? (a plate and a door; a crate and a
   gap; a drone's gaze and the lane behind it)
2. **Plan the recording** — where must the *other me* be, doing what, for how long?
3. **Record** — perform the other half: stand on the plate, push the crate, walk into the
   light as bait.
4. **Release and act** — while the echo replays, do your half: run through the door,
   jump off its head, sneak past the rubbing-its-eyes drone.
5. **Iterate** — a wrong recording costs five seconds, not a life.

Each level flavours the loop: **Level 0** teaches the contract; **Level 1 (Playroom)**
turns recordings into freight logistics (crates, mines, a three-lock finale); **Level 2
(Hide and Seek)** turns them into misdirection (searchlights, decoy-stuns, mobile cover).
Story beats fire diegetically — the most important one only when the player first *proves*
the decoy-stun mechanic, not at a floor position.

## 6. Target player

Players who like compact, readable puzzle-platformers (Braid, Limbo, Portal's pacing) and
finish a game in one sitting. Comfortable from roughly age 10–12 upward: the game asks
for reading and planning rather than reflexes, and contains nothing stronger than a robot
respawning at a checkpoint. Low dexterity floor — multiple key bindings for movement and
jump (record is one held key, pause is Esc), no simultaneous-press requirements, chase
speeds tuned below run speed, generous
checkpoints — with the challenge concentrated in planning. Session length 5–10 minutes.
Secondary audience: assessors and portfolio readers, which is why the game is scoped to be
*complete* (menu → story → three levels → ending) rather than broad.

## 7. Reference games or inspirations

The record-and-replay clone is an established indie mechanic family. Aftertrace cites
its three closest relatives openly and positions itself against them:

| Reference | What it proves / what Aftertrace takes |
| --- | --- |
| **Chronotron** (Flash, 2008) | The mechanical ancestor: record yourself, then cooperate with the replay. Aftertrace tightens the idea to *one* echo on a 5-second budget so every plan stays legible — and adds what Chronotron never had: a visual identity and a story. |
| **The Company of Myself** (Flash, 2009) | Proof a record-clone platformer can be *about* something — its clones are loneliness made playable. Aftertrace walks the same emotional road with a robot, and lets that tone drive the art and the music. |
| **Echoplex** (PC, 2018) | The nearest narrative neighbour: echoes plus recovered memory fragments — but first-person, with the echo as a *threat*. Aftertrace inverts the relationship: the echo is your only ally. |

**Film inspiration (worldview):** the story owes most to **WALL-E** (Pixar, 2008) — a
small machine alone among what people left behind, keeping an old recording alive. The
premise here is my rewrite of that feeling from my own film-watching, bent to my own
interests: not a civilisation's leftovers but **one child's**, and the machine does not
just replay what it finds — it *makes recordings of itself* to survive the puzzles. The
game's gentleness about memory and loss is deliberately closer to Pixar than to
dystopia.

**Visual / audio anchors:** the 1-bit discipline of *Return of the Obra Dinn* ("recede
by brightness, not alpha") and Gregor Quendel's music-box arrangements of Schumann's
*Scenes from Childhood*, one of which scores the ending.

## 8. What is original or creative about the idea

The base mechanic is an established family (§7) and this document does not claim
otherwise — no researched title, however, combines what follows. The original work
sits in three layers:

**Game design.** Levels introduce exactly one new relationship to the mechanic each —
cooperation (L0), logistics (L1), misdirection (L2) — so difficulty climbs by *ideas*
rather than stat tuning, and every rule is taught by use before it is required under
pressure. Inside that curve sit the mechanical inventions: ride the echo's head as a
one-way platform; bait a drone into touching your clone to *stun* it and sprint the
window; push two crates as a chained train through mines that key on **your** mass, so
cargo rolls through clean; latch a three-lock gate that needs crate, echo and player
pressing in the same instant. Recordings move *cargo*, not just the body — one
recording is a freight schedule — and the same clone that solves puzzles is the stealth
decoy, so the puzzle and stealth systems share one verb instead of living in separate
levels. Hazards telegraph on slow, fixed cycles: the challenge is always planning the
recording, never reflexes.

**Art combination.** The originality is in the *system*, not in original drawings: one
royalty-free 1-bit kit runtime-tinted into a strict black/white palette; a single cyan
accent reserved for the mechanic (the echo, the recording vignette, the recovered
traces), so the most colourful thing in the game *is* the mechanic; procedurally
generated UI and effect sprites where the kit ends; CC0 silhouette backdrops recessed
by brightness for depth; and AI illustrations generated to the same style guide so
cutscenes and gameplay read as one object. The constraint became the identity — and it
is what made a one-person art pipeline shippable.

**Worldview and story.** The narrative is built so the mechanic *is* the story: you
spend the game recording yourself while recovering the recordings a child left behind,
and the four illustrated acts reveal whose they are. The premise reworks the feeling of
*WALL-E* (§7) from my own viewing experience into something smaller and more personal,
and it is delivered diegetically — a terminal with two voices (the SYSTEM's clipped
white lines, the child's cyan PLAYBACK lines) and act stills between levels — rather
than through exposition.

## 9. Vertical slice plan

**Definition used:** one level that proves the whole experience — full mechanic loop
(record / replay / ride / plate / hazard / fragment / exit), final art direction, final
UI, audio, story delivery, and stable build — reachable from the main menu.

- **Day-1 throwaway slice (v0.1–v0.3, 25 May):** the entire game skeleton was stood up
  procedurally in one build day to de-risk the mechanic. This proved fun fast but looked
  like a blockout.
- **The real slice (v0.8.0, 10 June):** Level 0 ("Awakening") hand-authored over the
  frozen blockout with the 1-bit art baseline, diegetic narrative terminal, dialogue
  portraits, 384×216 pixel-perfect camera, and the full observe→record→solve tutorial
  contract. Acceptance: a new player reaches the exit unaided, every rule of the core
  contract (record / replay / ride / plate / hazard / fragment / exit) is demonstrated
  at least once — systems that deliberately debut later (drones, mines, the lift) are
  not part of the slice — and no debug input exists in the build.
- Levels 1–2 then reused the slice's systems and pipeline (see [PLAN.md](PLAN.md) W4).

## 10. Must-have, should-have, could-have, and cut-first features (as shipped)

Status: ✓ shipped · ✗ cut (with the cut recorded and justified).

**Must-have — all shipped ✓**

- Echo record/replay with the 5-second budget; one-echo rule ✓
- At least one puzzle that *provably* needs two bodies (dual-plate gate, L1) ✓
- Pressure plates, doors, checkpoints, respawn ✓
- Menu / HUD / pause / progress persistence across runs ✓
- One tutorial + two full levels; complete game loop with an ending ✓
- All assets original or redistribution-safe-licensed, fully credited ✓

**Should-have — all shipped ✓**

- Patrol drones with a real line-of-sight cone, chase/search/return AI, stealth
  detection meter ✓
- Recordable crates with path replay + chain pushing ✓
- Proximity mines, cyclic hazards, plate-driven lift ✓
- Diegetic story terminal with blocking beats (world freezes with the player) ✓
- Illustrated cutscene acts + licensed soundtrack (was stretch, shipped in v1.0.0) ✓

**Could-have**

- Third full level (Level 3) — **✗ cut** (see Cut-first)
- "Shift" signature mechanic (a second recording verb) — **✗ dropped in Week 3**:
  crates + the stealth decoy already gave the echo three distinct verbs; a fourth would
  have added breadth the schedule could not polish.
- WebGL build for browser play — not shipped; desktop submission build suffices.
- Gamepad support — cut early; keyboard+mouse is the assessed target.

**Cut-first (declared in advance, and exercised)**

- **Procedural level generation** — retired by design on 10 June (fenced behind a guarded
  `Legacy (DANGER)` editor menu), then **deleted outright in v1.1.0 (PR #155, 12 June)**:
  shipped gameplay scenes and the menu are hand-authored; the four cutscene scenes remain
  tool-built by `EchoCutscenes` (current tooling); the deleted builders live in git history.
- **Level 3** — existed as a blockout; cut on 11 June after Playtest 04 showed the
  presentation was stronger ending after Level 2's stealth climax than after a fourth
  level the schedule could only blockout-polish. The story now ends properly in Act 4.
  (Decision record: DevLog Session 10.)

## 11. Unity development plan

- **Engine:** Unity 2022.3.62f3 LTS, Universal Render Pipeline (2D renderer), TextMesh
  Pro. Resolution identity: PixelPerfectCamera at **384×216 reference, PPU 32**.
- **Scene architecture:** one scene per level/cutscene/menu; a persistent `App` prefab
  (AudioManager, SceneFader, progress) bootstrapped from `Resources` so any scene can be
  played directly in-editor.
- **Physics:** dynamic Rigidbody2D player vs. kinematic replayed echo; trigger-based
  hazards/plates; layers keep echo standpoints one-way and crates chain-pushable. Replay
  is recorded per-`FixedUpdate` frame for determinism.
- **Pipeline strategy (it evolved, deliberately):** Phase 1 — everything (sprites, SFX,
  prefabs, scenes) generated by editor scripts for one-day iteration. Phase 2 — licensed
  1-bit art baseline + tilemaps. Phase 3 — hand-authored gameplay scenes as the single
  source of truth; the level generators were first fenced, then deleted (PR #155), while
  the four cutscene scenes stay tool-built from `EchoCutscenes`' data table. Each shift
  is logged in the DevLog with reasons.
- **Process:** public GitHub repo; one PR per feature, each merge leaving a playable
  build — 22 PRs in total (#1–#7 and #144–#158; the number gap is GitHub issue numbering,
  and four of them are docs-only — #4, #145, #154, #157), plus one direct licence-compliance commit (`0d56e63`)
  recorded in the changelog; playtest rounds gating milestones; licence check before any
  asset enters the repo.
- **Testing:** manual in-editor play passes per PR, recorded in per-session verification
  tables; ad-hoc scripted in-editor walkthroughs with reflection-injected input during
  the Level 2 rebuild (PR #152 — the scripts were not retained in the repo); six
  recorded playtest rounds with fix lists ([PlayTestNotes/](PlayTestNotes/)).

## 12. Main systems / scripts

All gameplay code is original C# in `Aftertrace_01/Assets/Scripts/` (namespace
`EchoShift`). The systems an assessor should look at first:

| Area | Scripts | What they do |
| --- | --- | --- |
| Player | `PlayerController`, `PlayerAnimator` | run/jump with variable height, coyote-style ground check, control lockout during story; sprite animation states |
| Echo | `EchoRecorder`, `EchoClone`, `RecordedFrame` | per-physics-frame recording (≤5 s), kinematic replay, standable head platform, dissolve rules |
| Crates | `PushableCrate` | push detection, **chain pushing** (Sokoban hand-over), record/replay of pushed paths, live-crate depenetration after replay |
| Plates & doors | `PressurePlate`, `Door`, `PlateActivator`, `AutoDoorExit` | weight detection (player/echo/crate), multi-plate doors with `requireAll` + latch, exit doors with audio — walking through the exit door is what ends a level |
| World & pickups | `Collectible`, `Checkpoint`, `MovingPlatform` | optional memory-fragment pickups (with their memory beats), respawn checkpoints, patrolling and plate-driven platforms (the L1 lift) |
| Enemies | `PatrolDrone` | Patrol→Alert→Chase→Search→Return state machine, LOS cone (range+angle+ray), clone-priority targeting, decoy **stun** (with `OnStunned` event), gaze-alarm detection meter, leash |
| Hazards | `ProximityMine`, `HazardTrap`, `KillZone` | armed-fuse-cooldown mines that ignore crates; cyclic telegraphed traps; bounds safety |
| Narrative | `NarrativeTerminal`, `StoryTrigger`, `CutscenePlayer` | queued diegetic beats with typewriter + portraits; **StoryFreeze** (world pauses with the player); position *or* event-triggered beats; illustrated acts with caption SFX and skip |
| App | `GameManager`, `GameProgress`, `AudioManager`, `SceneFader`, `SceneMusic`, `AppBootstrap` | respawn/fragments/detection meter; cross-run persistence; music with **drift-through-silence** transitions (4.5 s drift-out, silence gap, bloom-in; same-track dedupe lets a cutscene act act as its level's prelude); fade-safe scene loads |
| UI | `HUDController`, `MainMenuController`, `PauseMenu`, `LevelButton`, `ButtonSfx` | fragment counter, record key hint, menus with the dimensional title |
| Effects | `CameraFollow`, `CameraShake`, `RecordingVignette`, `Flicker`, `PulseGlow`, `FloatingBob`, `SpriteFrameLooper` | clamped pixel-snapped camera, cyan record vignette, ambient motion |
| Editor tooling | `EchoArt`, `EchoAudio`, `EchoCutscenes`, `EchoBuildUtils` | the cutscene-scene builder and the deterministic SFX synthesiser still in use, plus the retained sprite-generator source — the provenance of every non-imported asset |

> **Removed after the post-docs audit (v1.1.2):** early builds completed a level through
> an in-level victory screen; once shipped levels switched to ending at the exit door
> (with every fragment optional), that path — `VictoryScreen`, `GameManager.CompleteLevel`,
> the victory music, per-level best times, and the orphaned `EchoUI` gameplay-UI builder —
> became unreachable and was deleted. It survives in git history.

## 13. Asset / resource plan

Policy first: **the repository is public, so nothing enters it unless its licence allows
redistribution** (CC0 / CC-BY / OFL), with one managed exception:

- **CraftPix 1-bit kit** (player, tiles, props, drone — royalty-free, but source-file
  redistribution forbidden): the PNGs are **local-only** (gitignored); the repo tracks
  only `.meta` files so a fresh clone restores by re-downloading the kit (steps in the
  game README). The same rule covers the seven standalone UI sprites cropped from the
  kit's sheets (`Assets/Art/Sprites/UI/` — crop sources in `SPRITESHEET_NOTES.md`).
  Tinting is done at runtime (`SpriteRenderer.color`) so no recoloured derived
  textures exist on disk.
- **CC0:** ansimuz industrial parallax silhouettes, Bonsaiheldin starfield (backdrops).
- **OFL fonts:** VT323 (terminal/captions), Exo 2 (body), Orbitron (fallback), Jersey 10
  (menu title; the cyan "3D" extrusion is a TMP underlay, not the font).
- **Music (OpenGameArt, CC0 / CC-BY 4.0):** per-track elected licences in
  [CREDITS](../Aftertrace_01/CREDITS.md); attribution travels with the repo.
- **Original:** all SFX (procedurally synthesised, deterministic), all UI/effect sprites
  not from the kit, all code, all level design.
- **AI-generated:** the ten cutscene illustrations, generated to the project's style
  guide, curated and integrated by the developer — disclosed in CREDITS and in the
  declarations document.

## 14. Legal, ethical, social, accessibility and security considerations

- **Legal:** per-asset licences verified at import time and listed in CREDITS with
  sources and download dates; the CraftPix redistribution clause is honoured by keeping
  its sources out of the public repo (discovered mid-project, fixed by untracking — the
  repo's MIT licence is scoped to original content only). The unused Kenney pack was
  removed rather than shipped unaudited.
- **Ethical:** AI-generated images are disclosed, not passed off as drawn; AI coding
  assistance is declared (see declarations); music attribution follows each track's
  elected licence.
- **Social/content:** offline, single-player, no chat, no violence beyond a robot
  respawning at a checkpoint; the theme (memory and loss) is handled gently.
- **Accessibility:** multiple bindings for movement and jump (record and pause are
  single keys) and no simultaneous-press chords; key events pair channels where it
  matters (plate light + click, door animation + sound, record vignette + start blip),
  and drone threat is encoded twice over visually (beam-brightness ramp + the screen-tint
  detection meter); no harsh strobing — hazards telegraph on slow alpha pulses; story
  beats and cutscenes are skippable; failure is a checkpoint respawn, never lost
  progress; 1-bit palette keeps contrast maximal.
- **Security/privacy:** fully offline; no analytics, accounts, or personal data;
  progress is a small local save on the player's machine.

## 15. Development schedule / milestone plan

Five weeks from concept to submission; the detailed week-by-week plan (goals, task
tables, outcomes, carry-overs) is [PLAN.md](PLAN.md).

| Week | Dates | Milestone focus | Headline outcome |
| --- | --- | --- | --- |
| W1 | 19–25 May | M1 — concept & vertical slice | Concept doc; one-day MVP burst: echo mechanic, menus/HUD/audio, 4 procedural levels (v0.1–0.3, PR #1–#3) |
| W2 | 26 May – 1 Jun | M2 — mechanical depth & art identity | Drone LOS AI + stealth rule; game-feel pass; rename to *Aftertrace*; **1-bit art pivot** (CraftPix baseline, tilemaps, VT323); pixel-perfect + animation (v0.4–0.7, PR #5–#149) |
| W3 | 2–8 Jun | (low-bandwidth — parallel coursework) | M3 redesign spec: narrative spine, crates-as-recordables, per-level themes |
| W4 | 9–15 Jun | M3 — content, narrative & ship | Three levels hand-rebuilt (#150–#152); Playtest 04; **Level 3 cut**; four cutscene acts + soundtrack = **v1.0.0** (#153); presentation polish + licence compliance (v1.1.x, #155–#156); submission-docs rebuild (#157, 13 Jun) |
| W5 | 16–21 Jun | Submission & presentation | Post-audit documentation corrections + dead-code cleanup (v1.1.2), current-build screenshot set, final ship-build validation (Playtest 06), presentation and demo video |

There is no separate "M4" milestone: its planned content (a climax level) was cut in
favour of finishing the story with cutscenes, and its polish duties were absorbed into
W4. The decision and its reasoning are recorded in DevLog Session 10.
