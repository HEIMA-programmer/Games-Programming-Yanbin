# Aftertrace — Level, Narrative & Onboarding Redesign (M3)

> **Status: DRAFT FOR REVIEW** · 2026-06-08 · Milestone 3 (Art Direction, World & Audio)
>
> This spec turns the four existing levels from disconnected blockouts into a coherent,
> taught, story-carried arc — **deepening the same four levels, not adding more** (refinement
> over scale). It pulls in two unused art categories as real gameplay (**push-crates**,
> **traps/hazards**) and replaces bare black-screen text with a **diegetic terminal** narrative
> layer plus in-level **story/hint triggers**.
>
> Nothing here is built yet. Approve / edit the beats below, then implementation follows
> phase by phase (§5). The **narrative copy is a first draft** — the voice is yours to tune.

## 0. Locked decisions (this pass)

1. **Level 0 becomes the echo tutorial.** It stops being an empty corridor and teaches the
   core verb inside the awakening.
2. **New devices activated from unused art:** `Objects/Boxes` → **push-crate**,
   `Traps/Trap1–6` → **spikes / crusher hazards**. *Not* this pass: second enemy archetype
   (`Alien2–6`), the "Shift" signature mechanic. (Both remain on the backlog.)
3. **Narrative vehicle:** a **diegetic "diagnostic terminal"** as the backbone (CraftPix
   `Tileset_GUI` frame + VT323 pixel font + typewriter + light glitch/scanline + cyan accent),
   plus **3–4 AI-generated 1-Bit "memory images"** at the biggest beats, framed inside the
   terminal's monitor border.

---

## 1. Narrative spine

### 1.1 The arc (what the story now *is*)

A machine wakes in a lab **it built**, its memory 94% corrupted. Its one ability — spawning an
**echo** of itself — is also *how it has survived alone*: it cooperates with recordings of its
own past. Across four sectors it recovers the truth: it built this place to **keep one memory
safe**; the people it knew **left**; it stayed, and for a very long time it has been
**replaying echoes — rehearsing the day they were still here.** The player + echo ("a memory in
two bodies") mirror the machine + the one it lost.

This keeps every existing shipped line ("I built it. They left. But I remained. Waiting.
Echoing.") and gives them a cause, a middle, and an emotional floor. Crucially it **ties the
mechanic to the theme**: *you record echoes because it rehearses the past.*

### 1.2 Beat sheet & copy (DRAFT — tune the voice)

All text renders in the terminal unless noted. `>` lines = system log; quoted lines = recovered memory.

**L0 — Boot (on wake):**
```
> SYSTEM REBOOT …
> memory core: 94% CORRUPTED
> motor functions: ONLINE
> recovering identity … [FAILED]
> … who switched me back on?
```

**L0 — Echo discovery (hint trigger, at the first two-body gate):**
```
> anomaly: a second signal, moving exactly like me.
  [HOLD R] to record — release to let it repeat what you did.
```

**L0 — First fragment (on pickup → exit):**
> "I have done this before. Many times."

**L1 "Sector 01" — Intro card (on entry):** `SECTOR 01 — the place I called home`
**L1 — Fragments (on pickup):**
- "These corridors — I walked them every day."
- "I built the doors to open only for two. I never asked why."
**L1 — Exit memory:**
> "I remember… this was my home. But a home needs more than one."

**L2 "Deep Labs" — Intro card:** `SECTOR 02 · DEEP LABS — where the others worked`
**L2 — Fragments:**
- "The drones used to follow me. Now they hunt my echo."
- "Someone stood here, watching the lights. Not me."
**L2 — Exit memory:**
> "I remember… I wasn't always alone. I made copies of everything — except them."

**L3 "The Core" — Intro card:** `SECTOR 03 · THE CORE — the last thing I saved`
**L3 — Fragments:**
- "They left when the world outside went quiet."
- "I kept the lights on. I kept… practicing."
- "If I record us often enough, maybe one day it plays back real."
**L3 — Ending (multi-beat; terminal + memory image; then fade):**
```
> memory core: 100% RECOVERED
```
> "I remember now.
> This lab — I built it, to keep one day safe:
> the day before they evacuated — the day they were still here.
> I cannot bring them back.
> But I can still rehearse them."

[ terminal: **HOLD R — one last time** ]
[ the player records; an echo of the people replays, standing where they once stood ]

> "There. Now I am not alone in the frame.
> … play it again."

### 1.3 The 3–4 AI memory images

Used **only** at these beats, shown inside the terminal monitor frame:

1. **Awakening** — a single light/eye opening in the dark; the lab glimpsed for the first time.
2. **L2 exit** — two abstract silhouettes side by side in the lab (the "other").
3. **L3 mid** — one silhouette leaving through a closing door; the lab empty.
4. **Ending** — the machine alone beside a flickering echo of the other.

**Style guide (hard constraints, so AI art doesn't clash):** pure 1-Bit (black + white dither
only), **single cyan accent** (`#00d4ff`), ~128–160 px, point-filtered/no smoothing, figures kept
**abstract/silhouette** (avoids uncanny AI faces + stays on-style), composed to sit inside the
`Tileset_GUI` rounded "screen" frame with a faint scanline. **Disclose in `CREDITS.md`.**

---

## 2. New systems to build (shared geography)

| System | Purpose | Art / tech | Notes |
| --- | --- | --- | --- |
| **`NarrativeTerminal`** (UI) | One reusable terminal window that types out text, optional glitch, optional embedded memory image + speaker glyph. Drives boot log, intro cards, fragment records, exit memories, ending. | `Tileset_GUI` 9-slice frame + VT323 (already baked) + Icons glyphs | Refactor `Level0Intro` and `VictoryScreen` to present *through* this, so all narrative shares one look. |
| **`StoryTrigger` / `HintZone`** (gameplay) | A trigger volume placed in a level; on first player-enter, fires a beat (story = terminal panel; hint = light non-blocking prompt). | reuses `NarrativeTerminal`; hint uses `Icons` arrow/key glyph | This is the "reach a spot → get taught / get story" tech that's currently **absent**. One-shot, optional brief pause, persists via a per-level seen-set. Hooks cleanly onto existing `GameManager`. |
| **`PushableCrate`** | A crate the player **and** the echo can push: box-on-plate, box-as-step, stack-to-height. | `Objects/Boxes` (has open/closed/stacked frames) | Carries a `PlateActivator` so it weighs plates. **Determinism risk:** a physics crate may diverge on replay — either drive pushes deterministically in `FixedUpdate` or record the crate's transform alongside the echo. Decide at build; must not desync. Bound it so it can't be lost off-level (respawn-on-fall). |
| **`Hazard`** (Spike + Crusher) | Danger variety. Spike = instant respawn on contact; Crusher = telegraphed periodic slam. | `Traps/Trap1–6` (multi-frame animated sheets) | Integrate with the existing checkpoint/respawn + detection systems. **No foreknowledge-only deaths** — always telegraph + checkpoint before. |
| **Fragment lore** | Each memory fragment carries a line/record shown on pickup (§1.2). | extend `Collectible`; distinct shapes from `Objects/Items` | Turns fragments from silent pickups into the story's delivery mechanism. |

---

## 3. Per-level redesign

Each level now has **one signature rule** (Braid "one new rule per sector"), a clear teach, and
a story spine. Layout is described as left-to-right areas; exact coordinates come at build time.

### L0 — Awakening · *teaches: move/jump + echo basics*
- **A. Boot chamber** — player wakes (lie→stand frame); terminal boot log (§1.2); control granted. **Hint:** move (← →) with arrow glyph. Set dressing: a dark, dead cradle/console (it built this).
- **B. First gap** — one small jump (keep the existing gap + safety floor). **Hint:** jump.
- **C. Echo gate** — a pressure plate across a gap from a door; you can't hold the plate *and* reach the door. **Hint:** *HOLD R to record… release.* Record yourself onto the plate → echo holds it → door opens. **Anti-softlock:** plate always reachable, recording retryable infinitely, no hazards, safety floor under the gap.
- **D. Threshold** — walk into the light → first fragment ("I have done this before…") → L1.
- **Devices/art:** terminal UI, plate + door (existing). No crates/traps (keep L0 clean). **Threat:** none.

### L1 — Sector 01 · *signature: RIDE your echo · introduces CRATES* · theme "home"
- **1. Re-entry** — intro card; a single plate→door to re-ground echo basics.
- **2. Ride across** — a gap too wide to jump: record an echo walking a low ledge, then **ride its Standpoint** across (first real use of the M2 ride mechanic). **Hint:** ride.
- **3. Ride up** — echo recorded rising on a moving platform acts as a lift to a high ledge (fragment up there).
- **4. Crate intro** — push a `PushableCrate` onto a plate to hold a door (**box-on-plate**). **Hint:** push. The echo can push too (record it pushing while you run ahead).
- **5. Combine** — **box-as-step** to reach the exit fragment; exit memory.
- **Fragments:** 3 (ride-up ledge / behind crate / on path). **Threat:** none.

### L2 — Deep Labs · *signature: DECOY vs drone · introduces HAZARDS* · theme "not alone"
- **1. Re-entry** — intro card; patrol drone re-introduced (decoy refresher), cover pillar.
- **2. Spike corridor** — floor **spikes** with safe gaps; checkpoint before. **Hint:** hazard (red glyph).
- **3. Decoy + spikes** — lure the drone with an echo while crossing a spike-flanked path.
- **4. Crusher + crate** — a telegraphed periodic **crusher**; a crate either holds a plate that gates it or is a **step over a spike pit**.
- **5. Gauntlet** — drone + hazard combined; exit memory.
- **Fragments:** 3. **Threat:** patrol drone (existing) + hazards. (No new enemy this pass.)

### L3 — The Core · *signature: COMBINE everything + finale* · theme "the truth"
- **1. Stealth corridor** — intro card; existing cover-based stealth, refreshed.
- **2. Ride + crate + hazard** combo room.
- **3. Decoy + crusher** gauntlet.
- **4. Memory Core (finale)** — a short multi-stage choreography (a couple of sequential echoes + a crate) opens the core; then the **ending terminal sequence + memory image** (§1.2) plays — closing on the interactive "HOLD R, one last time" beat where the player replays the evacuated humans beside the machine.
- **Fragments:** 3 (the three L3 lore lines). **Threat:** stealth drones + hazards.

---

## 4. Build & workflow

- **Geometry lives in C# Editor scripts:** `EchoLevel0.cs` (L0), `EchoScene.cs` (L1),
  `EchoLevel2.cs` (L2), `EchoLevel3.cs` (L3). New devices get prefabs via `EchoPrefabs` + builder
  helpers; new scripts under `Assets/Scripts/…`.
- **The one caveat:** re-running `Aftertrace ▸ Build Level X` regenerates that level's blockout
  and **overwrites hand-painted tilemap art in that scene**. So per level: **finalize geometry +
  devices first → playtest → then (re)paint art.** L1's current art will be repainted — accepted.
- Keeping geometry in C# preserves the "fully procedural, one-click rebuild" story for the
  oral exam — prefer editing the builders over hand-moving colliders in the scene.

## 5. Suggested sequencing (one slice at a time, each playtested)

1. **Phase 1 — Narrative + onboarding tech + L0 rebuild.** Build `NarrativeTerminal` +
   `StoryTrigger/HintZone`, rebuild L0 as the echo tutorial. → A vertical slice that proves the
   new presentation *and* the teaching. **Playtest before going further.**
2. **Phase 2 — Devices.** `PushableCrate` + `Hazard` (spike, crusher) prefabs + builder support.
3. **Phase 3 — Redesign L1 → L2 → L3**, one at a time, each rebuilt + playtested + repainted.
4. **Phase 4 — Memory images + ending polish + audio mood pass** (rest of M3).

## 6. Resolved (2026-06-08)

- **Story voice:** terse machine-log + short memories — restrained, cold. ✔
- **The lost "other":** the **humans who evacuated** the lab. ✔
- **Ending:** lean into **emotional tension** — the interactive "HOLD R, one last time" capstone
  in §1.2: the player is made complicit in the machine replaying the people it lost. ✔
- **L1 scope:** **full redesign** of all areas (workload not a concern). ✔
