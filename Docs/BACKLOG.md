# Aftertrace — Issue Backlog

Copy-paste-ready issues and sub-issues for the GitHub board. The milestone plan they belong to
is in [`ROADMAP.md`](ROADMAP.md).

> This is a **planning** document. §2 describes work to come — specs to implement later, not code.

## How to use this file

For each issue below: **title**, a **meta** line (labels / milestone / status / PR), a **body**
(paste into the issue description), and its **sub-issues**.

- **Sub-issues** can be created as GitHub *native sub-issues* (each its own issue, linked to the
  parent). To save time on the finished §1 items, you may instead paste the sub-issue list as a
  **task-list checklist** in the parent body (`- [x] …`). For the planned §2 work, native
  sub-issues are more useful because you work and close them one by one.
- **Completed work (§1):** create the issue, set labels + Milestone 1, **close it**, drop it in
  **Done**. Link its merged PR via the issue's right sidebar → **Development** → *link a pull
  request* (merged PRs cannot auto-close retroactively).
- **Planned work (§2):** create the issue, set labels + milestone, leave it open in **Backlog** /
  **To Do**. When you start it, branch, and put `Closes #<n>` in the PR body so merging
  auto-closes it.

---

# §1 — Completed work (backfill into "Done", Milestone 1)

## Issue: Core echo record/replay mechanic + one-click procedural builder (MVP)

**Labels:** `feature`, `area: core`, `tooling`, `must-have` · **Milestone:** M1 · **Status:** Done · **PR:** #1

Prove the core idea and de-risk it first: hold **R** to record up to 5s of movement, release to
spawn a ghost **echo** clone that replays it and can hold a pressure plate — so one player solves
two-body puzzles. The Concept Document flagged the echo as the project's single biggest risk, so
it ships first as a playable MVP.

**Delivered**

- Snappy 2D controller: run, variable-height jump, coyote time + jump buffer, grounded check, respawn.
- Echo system: capture in `FixedUpdate` for frame-rate-independent replay; the clone is a
  **kinematic `Rigidbody2D` driven by `MovePosition`** so its collider still fires plate triggers
  but it is not pushed by physics; exactly **one** clone at a time.
- Pressure plate ↔ door interaction. One-click procedural pipeline (sprites→PNG, audio→WAV,
  materials, prefabs, scene), idempotent. Level 01 (5 areas) + memory-fragment ending. Unity `.gitignore`.

**Acceptance (met):** the clone holds the plate and the door opens; replay does not drift;
re-running the builder produces no duplicates; clean compile.

**Sub-issues**

- `PlayerController`: run, variable-height jump, coyote time + jump buffer, grounded check, respawn
- `EchoRecorder`: hold-R capture in `FixedUpdate` (≤5s), single-clone rule, record indicator/VFX
- `EchoClone`: kinematic `MovePosition` replay; materialise ripple + dissolve particles
- `PressurePlate` + `Door` trigger interaction
- One-click procedural builder: art / materials / audio / prefabs / scene, idempotent
- Level 01 — 5-area vertical-slice puzzle + memory-fragment ending
- Unity `.gitignore` (keep generated assets; drop `Library/`, `Temp/`, `Logs/`)

---

## Issue: Full game loop — menus, HUD, pause, victory, audio + Level 2

**Labels:** `feature`, `area: ui`, `area: audio`, `area: levels`, `must-have` · **Milestone:** M1 · **Status:** Done · **PR:** #2

Wrap the MVP in a complete game: MainMenu → L1 → Victory → L2 → Victory → Menu, with fade transitions.

**Delivered**

- Main menu (title glow, Start / How to Play / Quit, ambient backdrop, hover + click SFX).
- HUD (fragment icons + record indicator); ESC pause (Resume / Restart / Main Menu); victory
  sequence (flash + burst → narrative → results → Next / Menu).
- Persistent app layer: `AudioManager` (cross-scene crossfade), `SceneFader`, `AppBootstrap`.
- Procedural audio: 10+ SFX + 3 looping BGM. Level 2 "Deep Labs": patrol drone, checkpoints +
  instant respawn, 4 areas. Fix: `EventSystem` per code-built canvas.

**Acceptance (met):** full flow with fades; buttons work; respawn at checkpoint; idempotent rebuild.

**Sub-issues**

- Main menu scene (title, options, ambient backdrop, hover + click SFX)
- HUD (fragment count + recording indicator)
- Pause menu (Resume / Restart / Main Menu) on ESC
- Victory sequence (flash + burst → narrative → results → Next / Menu)
- Persistent app/audio layer (`AudioManager` crossfade, `SceneFader`, `AppBootstrap`)
- Procedural audio set (10+ SFX, 3 looping BGM)
- Level 2 "Deep Labs": drone, checkpoints + respawn, 4 areas
- Fix: `EventSystem` per code-built canvas

---

## Issue: Level 0 & Level 3, level select, and cross-level progression

**Labels:** `feature`, `area: levels`, `area: ui`, `should-have` · **Milestone:** M1 · **Status:** Done · **PR:** #3

Bookend the slice with an intro and a climax, connect all four levels, and make exploration persist.

**Delivered**

- Level 0 "Awakening" (mood + movement only → auto-door into L1). Level 3 "The Core" (four areas:
  rising-platform gauntlet, mirror room, decoy corridor, memory core + four-line ending).
- `MovingPlatform` gains ping-pong mode + travel offset. Fragments: 3/level (9 total), HUD `X/3`,
  victory `Total X/9` + time, persisted via `GameProgress`; menu particles warm once finished.
- Breathing corridors + world-space wall narrative. Level select; full flow via fades, registered
  in Build Settings. Playtest fixes (platform crush, decoy tightening, fragment reachability, plate feedback).

**Acceptance (met):** every level completes from the menu / level select; no crush softlock;
fragments reachable; completion persists.

**Sub-issues**

- Level 0 "Awakening" intro (no echo / HUD / enemy)
- Level 3 "The Core" — four areas + ending
- `MovingPlatform`: ping-pong mode + travel offset
- Fragment system: 3/level, HUD `X/3`, victory `Total X/9`, `GameProgress` persistence
- Level select (`LevelButton`) + full flow registered in Build Settings
- Environmental narrative (breathing corridors + wall text)
- Playtest fixes: platform-crush, decoy tightening, fragment reachability, plate feedback

---

## Issue: Project documentation — dev-log system, READMEs, changelog, Session 01

**Labels:** `documentation` · **Milestone:** M1 · **Status:** Done · **PR:** #4

Record the *process*, not just the build.

**Delivered**

- Game `README.md` (idea, controls, run, accessibility, credits) + repo-root README index.
- DevLog system (template + guide + Session 01). Playtest notes (template + Playtest 01).
- `CHANGELOG.md` (Keep a Changelog, v0.1–0.3). Session-01 screenshots.

**Acceptance (met):** links resolve; screenshots render; no placeholder markers remain.

**Sub-issues**

- Game README (idea, controls, run, accessibility, credits)
- Repo-root README index
- DevLog system (template + guide + Session 01)
- Playtest notes (template + Playtest 01)
- CHANGELOG (Keep a Changelog, v0.1–0.3)

---

## Issue: Line-of-sight chase AI + stealth detection for patrol drones

**Labels:** `feature`, `area: ai`, `should-have` · **Milestone:** M1 · **Status:** Done · **PR:** #5

Session 2 · Part A. Make the detection cone a real sense so the echo decoy and cover matter, and
rework Level 3's enemy areas around it.

**Delivered**

- Drone state machine: Patrol → Alert → Chase → Search → Return (+ Stunned). Real line of sight
  (range + cone half-angle + an unobstructed Ground-layer raycast). Targets player **and** clone,
  **clone first**; chase `6` < player `7.5`; per-drone **leash** (no off-level wander, no corner
  softlock); cone colour follows state; editor gizmo.
- Stealth detection (opt-in per drone): ~1.6s in sight fills a meter (screen reddens) → respawn.
  L3 corridor rebuilt as cover-based stealth; L3 mirror room simplified to an optional echo-lift.
  Detection-cone sprite regenerated; drone prefab rewired; all procedural.

**Acceptance (met):** cone alerts/chases/searches/returns; LoS broken by cover; decoy works;
chase or detection can never trap the player; Level 2 behaviour unchanged.

**Sub-issues**

- Drone state machine (Patrol → Alert → Chase → Search → Return + Stunned)
- Real line of sight (range + cone angle + Ground-layer raycast)
- Target player + clone, **clone-first**; chase slower than the player; per-drone leash
- Stealth detection meter (opt-in) + screen-redden warning + respawn
- Cone colour from state + editor gizmo
- Level 2 cover block; Level 3 corridor rebuilt as cover-based stealth
- Level 3 mirror room → optional echo-lift; remove the lockable gate
- Re-verify: chase / detection can never softlock or push the player off-level

---

## Issue: Polish pass — Exo 2 UI font, game feel, richer procedural audio

**Labels:** `polish`, `area: ui`, `area: audio`, `area: art`, `should-have` · **Milestone:** M1 · **Status:** Done · **PR:** #6

Session 2 · Part B — the polish pass. (Echo trail, landing dust, hard-landing shake already
shipped in Session 1; this is the genuinely-new work.)

**Delivered**

- **Exo 2 (OFL)** TMP font via `EchoFont`, with graceful fallback to the TMP default. Death
  feedback: hit-stop + camera shake (unscaled). Pressure-plate press particle. Richer procedural
  audio (layered transients/harmonics + a pad). Fixed RNG seed → byte-identical WAVs.
- Docs: corrected the Session-01 polish TODO, README credits/OFL wording, changelog 0.4.0/0.5.0.

**Acceptance (met):** UI renders in Exo 2 with no overflow (and builds with no font); death gives
hit-stop + shake; the plate puffs; audio fuller without clipping; WAVs stable; Session-1 juice + AI still work.

**Sub-issues**

- Exo 2 (OFL) TMP font build step + graceful fallback
- Death feedback: hit-stop + camera shake (unscaled time)
- Pressure-plate press particle
- Richer procedural audio (layered transients/harmonics + BGM pad)
- Fixed RNG seed → byte-identical WAVs
- Docs: README credits/OFL, changelog 0.4.0/0.5.0, Session-01 correction

---

## Issue: Bake the Exo 2 TMP font asset as Static (stop runtime git churn)

**Labels:** `bug`, `area: ui`, `tooling` · **Milestone:** M1 · **Status:** Done · **PR:** #7

The generated `Exo2-Regular SDF.asset` showed as "modified" after merely opening/playing the
project, because `EchoFont` generated a **Dynamic** TMP font that rewrites its atlas at runtime.

**Delivered**

- Pre-bake the glyph set the UI uses (printable ASCII + Latin-1 + `← → ↑ ↓ — – … ·` and quotes),
  then set `atlasPopulationMode = Static`. Auto-heal on the next Build All; safe fallback.

**Acceptance (met):** after opening/playing, `git status` shows the font asset unmodified; UI text
renders with no missing-glyph boxes.

**Sub-issues**

- Pre-bake the glyph set + set `atlasPopulationMode = Static`
- Auto-heal an existing dynamic asset on the next Build All; safe fallback

---

## Issue: Ride Your Echo + 1-Bit art baseline + menu / HUD polish

**Labels:** `feature`, `area: core`, `area: art`, `area: ui`, `must-have` · **Milestone:** M2 · **Status:** Done · **PR:** #TBD

Opens Milestone 2 with one core mechanic + a full 1-Bit visual baseline that the rest of M2 / M3
can build on. Closes the "Ride Your Echo" issue from M2 §2 and supersedes M3's
"100%-procedural sprites" principle.

**Delivered**

- **Ride Your Echo (mechanic).** `EchoClone` gains a child `Standpoint` (BoxCollider2D +
  one-way PlatformEffector2D, surfaceArc 170°) on the Ground layer; per-frame `OverlapBoxAll`
  above the surface carries riders by the echo's delta (avoids OnCollisionEnter/Exit edge
  cases the effector creates). Trigger circle on the root stays for pressure plates;
  `BeginDissolve` clears the standpoint so the rising echo can't drag the player up.
- **Art baseline switch.** Sprites swap from `EchoArt`'s pixel-by-pixel generators to a
  curated 1-Bit pack: CraftPix Sci-Fi Platformer 1-Bit Game Kit (royalty-free) + Kenney UI
  Pack: Sci-Fi (CC0). `EchoSpriteSlicer` slices every spritesheet on a per-asset grid;
  `EchoImportedAssetSettings` (an `AssetPostprocessor`) auto-configures Point filter,
  per-folder PPU, and FullRect mesh; `EchoBuildUtils.LoadSprite` maps logical names
  (`player`, `echo`, `drone`, `platform`, `platform_wall`, `platform_ceiling`, `door`,
  `endarch`, `fragment`, `arrow`, `checkpoint`, `background`) to named frames; `EchoArt`
  generators for replaced sprites stay in source as fallback / rollback.
- **1-Bit visual identity.** Camera background pure black; `Tint*` palette keeps sprites
  near-white so the monochrome pop survives (Downwell / Obra Dinn principle); cyan kept
  only as a HUD/lighting accent. Multi-tile variety: walls / ground / ceiling pick
  different Tileset frames so the level reads as a constructed lab.
- **Scene frame.** New `BuildFrameBorder` procedural sprite (9-sliced rounded white
  outline) is stretched as `EchoBuildUtils.AddSceneFrame` on every canvas — menu, HUD,
  pause, victory — for the CraftPix promo-art framed-scene look.
- **Dense composition.** Menu hero diorama: huge planet hero + corner backdrops + NPC
  silhouettes flanking + ground prop vignette + top icons + bottom detail strip, all
  placed outside the button column (no overlap). Level: 13 large 1-Bit backdrops (parallax
  0.08), 60 mid-layer Tileset_details circuits (parallax 0.32), 8 NPC silhouettes
  (parallax 0.5), 14 ground props (boxes + machines); camera zoomed `6.5 → 5.4` for
  density.
- **Typography.** Added Orbitron (display fallback) and VT323 (retro pixel terminal,
  title default) as OFL fonts. `EchoFont` bakes both as Static TMP assets alongside
  Exo 2; new `CreateTitleText` routes menu / pause / victory titles through VT323 with a
  per-instance outline material.
- **Modern HUD.** Replaced the 3-diamond fragment row with a single gem icon
  (`Items_4`) + Orbitron / VT323 "X / Y" counter; `HUDController` refactored.
- **Wall narrative tactical panel.** `CreateWallNarrative` now uses a 9-sliced button
  panel + outlined display-font text instead of a stretched lab tile + glow halo.
- **UI bug.** HowToPlay / LevelSelect dim panel alpha 0.9 → 1.0 (no more bleed-through);
  title-only outline polish on AFTERTRACE / HOW TO PLAY / SELECT LEVEL / PAUSED / Victory
  level name. Button background regenerated with crisp white outline (was cyan); button
  size and label weight bumped for stronger 1-Bit contrast.
- **Checkpoint visual.** Was a collider-only prefab; now ships with a CraftPix flag-pole
  sprite + cyan-mint PointLight.
- **Editor tooling.** `EchoSpriteSlicer` (menu + Build All step), `EchoSpritePicker`
  (`Aftertrace ▸ Sprite Picker` — visual frame browser), `EchoImportedAssetSettings`.

**Acceptance (met):** echo holds plates AND carries riders horizontally/vertically without
jitter; side-touch doesn't drag the player; dissolve drops the rider cleanly; HowToPlay /
LevelSelect overlays are opaque; menu titles use VT323, body uses Exo 2; every Aftertrace
scene shows a closed scene-frame; all four levels load and complete with the new art.

**Sub-issues**

- `EchoClone` Standpoint + PlatformEffector2D + overlap-based carry; `BeginDissolve` cleanup
- `EchoSpriteSlicer` + `EchoImportedAssetSettings` + `LoadSprite` → imported pipeline
- `BuildFrameBorder` 9-sliced frame + `AddSceneFrame` on menu / HUD / pause / victory
- VT323 / Orbitron baking; `CreateTitleText` + `ApplyOutline` helpers
- HUD refactor (single gem + counter); `CreateWallNarrative` tactical panel
- Dense menu diorama + zoomed level + multi-tile walls/ground/ceiling
- `EchoSpritePicker` editor window; `EchoFont` two-font bake
- `CREDITS.md` + README / ROADMAP licensing-story revision

---

# §2 — Planned backlog (game-core focus)

## Milestone 2 — Mechanical Depth & Variety (Week 3 · Jun 1–7)

### Issue: Ride Your Echo (clone as a standable platform)

**Labels:** `feature`, `area: core`, `should-have` · **Milestone:** M2 · **Status:** Done — see §1 entry "Ride Your Echo + 1-Bit art baseline + menu / HUD polish" above.

Today the echo is a single `CircleCollider2D (isTrigger, r=0.4)` + `PlateActivator` — it triggers
plates, but you fall through it. Make the clone a **surface you can stand on and be carried by**,
so *"record yourself walking / rising, then ride your past self"* becomes a puzzle verb — the most
thematically honest extension of cooperating with yourself.

**Technical notes (spec)**

- Keep the existing **trigger** collider (plates use `OnTriggerEnter/Exit` via `PlateActivator`).
  Add a **second, solid** collider for the top: a thin `BoxCollider2D` (clone or child) with a
  one-way **`PlatformEffector2D`** (surface arc up), on a layer the player collides with.
- The clone is kinematic and moves with `MovePosition`; make the ride **smooth** against those
  discrete steps (PlatformEffector + continuous collision; or parent the player on contact; or add
  the clone's per-step delta to the player). Pick whichever does not jitter.
- Update `EchoPrefabs.BuildEcho` procedurally; confirm plates still detect the clone.

**Acceptance criteria**

- Riding works while the clone walks across a gap and while it rises on a platform.
- No jitter while riding; no softlock from riding into a ceiling/wall.
- Pressure plates still activate from the clone. Fully procedural (Build All wires it).

**Sub-issues**

- Add a one-way solid top surface (`BoxCollider2D` + `PlatformEffector2D`) without breaking the plate trigger
- Smooth riding on the kinematic clone (no jitter vs. `MovePosition`); choose the carry method
- `EchoPrefabs`: wire the new collider/effector procedurally; verify plates still fire
- One puzzle each for "ride across" and "ride up"; integrate into an existing area
- Playtest: ride-into-ceiling / ride-off-screen / ride + plate — no softlocks

---

### Issue: Echo carries & pushes objects (crates)

**Labels:** `feature`, `area: core`, `should-have` · **Milestone:** M2

Add a pushable **crate** the player *and* the echo can move. Recorded, the echo can carry/push a
crate where it's needed — opening box-on-plate, box-as-step, and stack-to-height puzzles from the
existing record/replay verb.

**Technical notes (spec)**

- A `Rigidbody2D` crate with friction; player + echo push on contact; the crate carries a
  `PlateActivator` so it weighs down plates.
- **Determinism is the risk:** a physics-driven crate may diverge between record and replay. Either
  make pushes deterministic in `FixedUpdate`, or **record the crate's state alongside the echo** and
  replay it. Decide and note it.

**Acceptance criteria**

- A recorded echo reproduces a push reliably — no record/replay divergence.
- A crate on a plate holds it; a crate can be a step to reach height; crates can stack.
- No crate can be lost off-level or wedged into a softlock. Procedural prefab + placement.

**Sub-issues**

- Pushable crate prefab (`Rigidbody2D` + `PlateActivator`), generated by `EchoPrefabs`
- Player & echo push the crate on contact
- Deterministic push on replay (record crate state if needed) — no divergence
- One puzzle each: box-on-plate, box-as-step, stack-to-height
- Playtest: crate loss / wedge / softlock checks

---

### Issue: Puzzle device pack — lever, timed gate, echo-blockable beam

**Labels:** `feature`, `area: levels`, `should-have` · **Milestone:** M2

A small set of devices that compose with the echo for variety:

- **Toggle lever** — flips a latched target (door / platform / beam) and stays; the echo can throw
  it while you do something else.
- **Timed gate** — opens for *N* seconds when triggered, then recloses; race it or send the echo.
- **Echo-blockable energy beam** — a line hazard that respawns you on contact, but the echo's body
  (or a crate) blocks it so you can cross.

**Acceptance criteria**

- Lever toggles a latched target and persists; throwable by player or echo.
- Timed gate opens for a tunable, **telegraphed** duration and recloses.
- Beam respawns on contact but is clearly blocked by the echo/crate.
- All three are reusable, procedurally built, and used in ≥1 puzzle each.

**Sub-issues**

- Toggle lever device (latched state + visual/audio) — prefab + builder
- Timed gate (open *N* s, telegraphed countdown) — prefab + builder
- Echo-blockable energy beam hazard — prefab + builder
- One puzzle each demonstrating lever / timed gate / beam
- *(could-have)* a redirector/mirror that bends the beam

---

### Issue: New threat — scanner/turret enemy + environmental hazards

**Labels:** `feature`, `area: ai`, `should-have` · **Milestone:** M2

The patrol drone is the only threat. Add one new archetype + hazards for danger variety. The new
enemy is a **fixed scanner/turret** with a sweeping cone or a periodic beam-pulse — it can't be
lured by movement like a patrol, so the echo must **block/time** rather than bait it. Plus
**spikes** (instant respawn) and a **periodic crusher** (timing).

**Acceptance criteria**

- One new enemy archetype, clearly readable and distinct from the patrol drone.
- ≥2 hazard types (spikes + crusher) that respawn the player fairly (checkpointed).
- Integrates with the stealth/respawn systems; no unfair/foreknowledge-only deaths.

**Sub-issues**

- Scanner/turret enemy (sweep or pulse) — script + prefab + builder, with editor gizmo
- Spikes hazard (instant respawn) — prefab + builder
- Periodic crusher hazard (telegraphed timing) — prefab + builder
- Integrate threats with checkpoints/detection; fairness pass

---

### Issue: "Shift" signature mechanic — design spike + prototype

**Labels:** `feature`, `area: core`, `should-have` · **Milestone:** M2

The title is *Echo **Shift*** but only "Echo" is expressed. Design and spike a **Shift**: a
world-state toggle that swaps two layers of the level (phase A vs B — platforms/hazards/doors that
exist in only one phase) that the player can shift and that the **echo can be recorded interacting
with across phases** (record in A, replay while you hold B). High payoff (a second signature verb +
the title earned), higher risk — so spike feasibility and fun **before** committing the week.

**Acceptance criteria (spike)**

- A written design (rules; what shifts; how the echo interacts across phases) + a throwaway
  prototype room.
- A **go / no-go** decision: if fun and feasible in the timebox, open a follow-up build issue; if
  not, defer with notes — do not sink the week into it.

**Sub-issues**

- Design note: Shift rules + how echo recording interacts across phases
- Prototype one throwaway room to test the feel
- Go/no-go decision; open a follow-up build issue *or* document the deferral

---

### Issue: Puzzle redesign pass — one new rule per sector

**Labels:** `enhancement`, `area: levels`, `should-have` · **Milestone:** M2

With the new toolkit, redesign the existing sectors so each showcases a distinct rule/verb
(Braid-style), raising variety and fun without adding many levels — e.g. Sector 01 teaches *ride*,
Deep Labs adds *carry* + *beam*, The Core combines *devices* + the new threat.

**Acceptance criteria**

- Each existing sector has a signature new verb/device.
- Difficulty ramps across sectors; the record mechanic stays clearly taught.
- No regressions / softlocks (re-verify with the mechanic-validation playtest).

**Sub-issues**

- Map each sector → its signature new rule/verb
- Redesign the puzzles per sector (procedural builders)
- Re-verify solvability + fragment reachability after changes

---

### Issue: Mechanic-validation playtest

**Labels:** `playtest` · **Milestone:** M2

A quick playtest focused only on whether the new verbs/devices are **teachable, fun, and
softlock-free**. A minority process step that serves the game work.

**Acceptance criteria:** each new verb is introduced safely and understood; no softlocks; top
issues fixed and re-tested.

**Sub-issues**

- Play the new/redesigned sections; note teach / fun / softlock issues
- Fix top issues; quick re-test

---

### Issue: Session-03 dev log + changelog

**Labels:** `documentation` · **Milestone:** M2

Record Session 03 (the new toolkit, the redesign, the Shift spike) with decisions / problems /
testing; bump the CHANGELOG (target `v0.7`).

**Sub-issues**

- Write `Docs/DevLog/Session-03`
- CHANGELOG (`v0.7`) + screenshots of the new mechanics

---

## Milestone 3 — Art Direction, World & Audio Identity (Week 4 · Jun 8–14)

> All upgrades stay **procedural** — better generators (`EchoArt`, `EchoMaterials`, `EchoAudio`,
> the level builders), not hand-placed assets. 100% original, one-click.

### Issue: Art-direction & palette-ramp system

**Labels:** `area: art`, `should-have` · **Milestone:** M3

The foundation for the visual upgrade: a cohesive direction (mood, contrast, shape language) and a
**per-sector palette with shaded ramps**, exposed as a utility in `EchoArt` so every sprite/tile
draws from one source. Sector arc: cold blue lab → teal deep labs → warm amber core.

**Acceptance criteria**

- A palette/ramp utility used by the generators; per-sector palettes defined.
- A one-page art-direction note (mood, palette, do/don't) for consistency.

**Sub-issues**

- Define per-sector palettes + shaded ramps (utility in `EchoArt`)
- Write a one-page art-direction note (mood, contrast, shape language)
- Refactor the existing generators to draw from the palette system

---

### Issue: Normal-map generation for sprites

**Labels:** `area: art`, `should-have` · **Milestone:** M3

The project uses URP 2D lights but the sprites are flat. **Generate a normal map alongside each
sprite** (height/bevel → normal) so the lights give real relief — a large visual jump with no
hand-art. Assign the secondary normal texture on the lit material.

**Acceptance criteria**

- Generators emit a matching normal map per lit sprite; the lit material uses it.
- Tasteful relief under the 2D lights; no artefacts; still one-click; performance holds.

**Sub-issues**

- Generate normal maps in `EchoArt` (bevel/height → normal)
- Wire normal maps into the lit material (`EchoMaterials`)
- Tune the light response per sector; performance regression check

---

### Issue: Character & entity art + animation

**Labels:** `area: art`, `should-have` · **Milestone:** M3

Upgrade the player, echo, and enemy sprites (more shape/detail via the palette + normals) and add
real animation — **squash-&-stretch** on jump/land (Celeste-style, with dust follow), run frames,
enemy idle/alert, echo shimmer.

**Acceptance criteria**

- Player/echo/enemies look distinctly more crafted; animations read clearly and match motion.
- Squash-&-stretch on jump/land; all procedural/generated.

**Sub-issues**

- Upgraded player sprite + squash-&-stretch jump/land
- Upgraded echo sprite + shimmer/ghost treatment
- Upgraded enemy sprites + idle/alert animation
- Hook the animation into `PlayerAnimator` (procedural) where applicable

---

### Issue: Environment tilesets & props

**Labels:** `area: art`, `area: levels`, `should-have` · **Milestone:** M3

Replace plain blocks with proper **auto-tiled** ground/wall tilesets (edges, corners, surface
detail) and **decorative props** (pipes, cables, screens, debris) placed by the builders — per-sector
set dressing.

**Acceptance criteria**

- Auto-tiled edges/corners on ground/walls; sectors visually themed.
- Props placed procedurally; no collision/readability regressions.

**Sub-issues**

- Generate ground/wall tileset (edges, corners, surface detail) + auto-tiling in the builders
- Decorative prop set per sector (pipes / cables / screens / debris)
- Place props in the level builders; readability/collision check

---

### Issue: Backgrounds & lighting mood

**Labels:** `area: art`, `should-have` · **Milestone:** M3

Layered parallax backgrounds with depth + **per-sector lighting palette** and atmosphere
(fog/particles), building on the existing parallax + bloom.

**Acceptance criteria**

- Multi-layer backgrounds per sector; per-sector lighting mood (cold→warm arc).
- Atmosphere tasteful; no strobing; performance holds.

**Sub-issues**

- Layered parallax backgrounds per sector (generated)
- Per-sector lighting palette + atmosphere (fog/particles)
- Performance/readability pass

---

### Issue: VFX pass

**Labels:** `polish`, `area: art` · **Milestone:** M3

Bring all VFX (echo materialise/dissolve, dust, plate press, death, collectible, the new beam/
devices) up to the new art bar and palette.

**Acceptance criteria:** VFX consistent with the palette; satisfying; no outliers; performance fine.

**Sub-issues**

- Rework echo / dust / dissolve / plate / death / collectible VFX to the palette
- New-device VFX (beam, lever, gate) consistency

---

### Issue: Audio identity — per-sector music + ambience + richer SFX

**Labels:** `area: audio`, `should-have` · **Milestone:** M3

Give each sector its own sound — distinct procedural **music themes**, **ambient beds**, and more
characterful SFX (still 100% procedural, `EchoAudio` upgrades; keep the fixed seed so WAVs stay
stable).

**Acceptance criteria**

- Distinct music per sector + ambience; richer SFX; no clipping; reproducible (seeded).

**Sub-issues**

- Per-sector procedural music themes
- Ambient beds per sector
- Richer/characterful SFX (keep the fixed seed → stable WAVs)

---

### Issue: Narrative & world depth

**Labels:** `area: narrative`, `should-have` · **Milestone:** M3

Deepen the story of the machine waking in the abandoned lab, replaying its own echoes. Give the
**memory fragments lore** (each carries a line/log on pickup), strengthen **environmental
storytelling** (wall text + set dressing telling the lab's fall), and write a more resonant,
multi-beat **ending**.

**Acceptance criteria**

- Fragments carry story (a line/log on pickup); set dressing reinforces it.
- A clear arc (waking → remembering → the truth) and a stronger ending.

**Sub-issues**

- Lore-bearing fragments (a line/log per fragment)
- Environmental storytelling pass (wall text + set dressing)
- Rewrite/extend the ending into a multi-beat resolution

---

### Issue: Session-04 dev log + changelog

**Labels:** `documentation` · **Milestone:** M3

Record Session 04 (the art/world/audio upgrade); bump the CHANGELOG (target `v0.8`).

**Sub-issues**

- Write `Docs/DevLog/Session-04`
- CHANGELOG (`v0.8`) + before/after art screenshots

---

## Milestone 4 — Climax, Polish & Ship (Week 5 · Jun 15–21)

### Issue: Climactic finale

**Labels:** `feature`, `area: core`, `should-have` · **Milestone:** M4

A memorable capstone that uses the whole toolkit (ride, carry, devices, stealth, Shift if shipped).
Design candidate: **"confront your echoes"** — choreograph several sequential echoes to solve a
multi-stage gate — or a **timed escape** as the lab collapses, tied to the ending.

**Acceptance criteria**

- A distinct, climactic sequence combining ≥3 mechanics; solvable and fair; ties into the ending.
- Procedural; playtested for softlocks.

**Sub-issues**

- Design the finale (which mechanics, the beat-by-beat)
- Build it procedurally (new builder or extend The Core)
- Playtest for fairness / softlocks; tune

---

### Issue: Whole-game variety & difficulty / pacing pass

**Labels:** `enhancement`, `area: levels` · **Milestone:** M4

Tune the full arc now that every mechanic exists.

**Acceptance criteria:** sensible difficulty curve end to end; each sector feels distinct; teaching
is clean; no dead spots.

**Sub-issues**

- Full-arc difficulty / pacing review
- Tune per-sector distinctiveness; fix dead spots

---

### Issue: Final game-feel / juice pass

**Labels:** `polish` · **Milestone:** M4

Apply the juice lessons across the game: screenshake with an easing curve + decay (50–300 ms),
squash-&-stretch where it helps, snappy transitions, satisfying pickups/impacts.

**Sub-issues**

- Screenshake easing + decay tuning across events
- Transition / feedback polish (pickups, doors, victory)

---

### Issue: Essentials — settings + minimal accessibility

**Labels:** `feature`, `accessibility`, `should-have` · **Milestone:** M4

The minimum systems players expect, kept deliberately small: **volume** (master/music/SFX,
persisted), **reduce-motion** (tones down shake/hit-stop), and **hold-vs-toggle record**. Reachable
from the main menu and the pause menu.

**Acceptance criteria**

- Volume persists & applies across scenes; reduce-motion + hold/toggle work; reachable from menu
  and pause; degrades gracefully on WebGL.

**Sub-issues**

- Volume sliders (master/music/SFX) persisted to PlayerPrefs (optional `AudioMixer`)
- Reduce-motion toggle (shake/hit-stop) + hold-vs-toggle record option
- Settings panel reachable from the main menu + pause menu

---

### Issue: Windows + WebGL builds (smoke-tested)

**Labels:** `area: build` · **Milestone:** M4

**Acceptance criteria:** Windows build runs the full flow from a clean folder; WebGL build runs all
scenes in a browser; audio/input/settings work; no missing assets.

**Sub-issues**

- Windows build + full-flow smoke test from the `.exe`
- WebGL build + browser smoke test (all scenes); fix WebGL specifics

---

### Issue: v1.0.0 release — tag + GitHub Release + builds + CHANGELOG 1.0.0

**Labels:** `area: build`, `documentation` · **Milestone:** M4

**Acceptance criteria:** tag pushed; Release published; both builds downloadable; notes summarise the slice.

**Sub-issues**

- CHANGELOG `1.0.0` section
- Tag `v1.0.0` + GitHub Release with both builds attached

---

### Issue: Final playtest + bug fix

**Labels:** `playtest` · **Milestone:** M4

**Acceptance criteria:** `Playtest-03` logged; all high/medium issues fixed or explicitly deferred;
no known softlocks.

**Sub-issues**

- Final playtest (fresh player if possible) on desktop + WebGL
- Write `Docs/PlayTestNotes/Playtest-03`; fix high/medium; record deferrals

---

### Issue: Final docs — README, credits, Session-05 dev log, retrospective, report/demo

**Labels:** `documentation` · **Milestone:** M4

The report and demo video are delivered **outside the repo** (tracked here for visibility).

**Sub-issues**

- README final pass (controls / run / builds-release links)
- Credits + licence check (Exo 2 OFL, Unity / TMP)
- Session-05 dev log + retrospective
- Report draft + demo video (delivered outside the repo)

---

### Issue: Final triage — close remaining issues + all milestones

**Labels:** `documentation` · **Milestone:** M4

**Acceptance criteria:** every open issue resolved or explicitly deferred with a note; all four
milestones closed; the board has nothing stranded in progress.

**Sub-issues**

- Resolve / defer all open issues with a note
- Close M1–M4; tidy the board
