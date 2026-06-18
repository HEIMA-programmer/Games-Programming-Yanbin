# Aftertrace — Project Report

> Post-project report for the shipped v1.1.2 build (June 2026). Written to be read
> alongside [DESIGN.md](DESIGN.md) (what the game is), [PLAN.md](PLAN.md) (when things
> happened), [CHANGELOG.md](CHANGELOG.md) (what shipped in each version),
> [DevLog/](DevLog/) (how each session actually went) and
> [PlayTestNotes/](PlayTestNotes/) (what testing found).

## 1. Design choices

**One mechanic, many verbs.** The whole game is one verb — *record yourself* — pushed
through three relationships: cooperation (plates, riding), logistics (crates replay
their pushed paths), and misdirection (the clone as a stealth decoy that stuns drones).
The 5-second budget and the one-echo rule are deliberate restrictions: plans stay short
enough to hold in your head, and committing to a recording *means* something.

**Depth over count.** Three levels, each teaching a different relationship to the
mechanic, beat four levels that each teach nothing new. This principle was written down
in Week 1 and was eventually exercised for real when Level 3 was cut (§5).

**Diegetic storytelling.** Story arrives through an in-world terminal (queued beats
with portraits), through four illustrated acts between levels, and through the mechanic
itself: the player spends the game making recordings while recovering the recordings a
child left behind. The most important coaching beat fires only when the player first
*proves* the decoy-stun mechanic — the game teaches at the moment of competence, not at
a floor trigger.

**A strict 1-bit identity.** Pure black/white with a single cyan accent reserved for
the echo, the recording vignette and the HUD. The constraint made a one-person art
pipeline feasible and made the mechanic the most colourful thing in the game.

**Calm difficulty.** Chase speeds sit below run speed, hazards telegraph on slow
cycles, failure is a checkpoint respawn. The intended experience (DESIGN.md §3) is
"out-think the room", so the difficulty lives in planning, never in punishment.

## 2. Technical decisions

- **Deterministic replay:** the recorder samples per physics frame (`FixedUpdate`) and
  the clone replays kinematically, so a recording does the same thing every time —
  the property every puzzle depends on.
- **Hand-authored scenes as the source of truth.** The project began 100% procedural
  (editor scripts generated every sprite, sound, prefab and scene — ideal for a one-day
  MVP). As art direction became a requirement, generated *composition* became the
  bottleneck, so the shipped gameplay scenes moved to hand-authoring; the level
  generators were first retired behind a guarded `Legacy (DANGER)` menu (v0.8.0) and
  then **deleted outright in v1.1.0** — they survive in git history. What remains in
  `Assets/Editor/` is the tooling still in use — the cutscene-scene builder (the four
  cutscene scenes stay tool-built from its data table) and the deterministic SFX
  synthesiser — plus the retained sprite-generator source, which together document the
  provenance of every non-imported asset.
- **A persistent App layer** (audio, fades, progress) bootstrapped from `Resources`, so
  any scene can be played directly in the editor — a small decision that paid for
  itself every testing hour.
- **Selective world-freeze instead of timescale pause.** Blocking story beats freeze
  the player, so `NarrativeTerminal.StoryFreeze` gates drones, mine fuses, trap damage,
  echo replay — and the drone *stun countdown* — while UI typewriters keep running on
  unscaled time. Pausing `Time.timeScale` would have frozen the prose along with the
  world.
- **Music transitions as design:** tracks never crossfade; the old track drifts out
  over 4.5 s (squared falloff, started when the scene fade begins), a beat of silence,
  then the new track blooms in. Acts 1–3 share their following level's track, so each
  cutscene is its level's prelude and the seam disappears.
- **Pixel-perfect discipline:** one project-wide camera standard (384×216 reference,
  PPU 32) instead of per-scene zoom choices.
- **Deterministic procedural audio** (fixed RNG seed) so regenerating SFX never dirties
  git.
- **Licence-gated public repo:** nothing enters the repository unless its licence
  permits redistribution; the one exception (the CraftPix kit) is handled by keeping
  its source PNGs — and the seven UI sprites cropped from its sheets — local-only with
  tracked import metadata (§7; remediation story in §8).

## 3. Problems and limitations

Problems that cost real time, and what fixed them (full detail in the DevLog):

- **A sprite-less UI `Image` silently refused to render** on the cutscene canvas
  despite verifiably correct state — the caption plate "existed" but never drew. Fix:
  back every tintable plate with a real sprite (a generated 8×8 white texture). The
  cause was never fully identified; the rule ("plates are sprite-backed") is recorded
  so it cannot recur.
- **Saved-scene state loss:** a fix (the Level 2 exit-door audio) vanished from the
  saved scene once. The lesson became a discipline: every scene edit is verified by
  *reopening the scene from disk* before it counts as done.
- **Physics edge cases:** Sokoban-style chain pushing needed an explicit force
  hand-over between crates plus a depenetration pass after replay; an over-wide ground
  check let players climb walls (fixed by narrowing it, which also killed a
  jump-over-the-drone cheese).
- **Presentation bugs found by replaying the whole game:** the previous story beat
  flashed through on the next one; the cutscene `[ SPACE ]` hint clipped under the
  screen frame; a mid-corridor trigger fired a lesson about a mechanic the player had
  not used yet (now event-driven).
- **Residue found by the post-docs audit (v1.1.2):** the early builds completed a level
  through an in-level victory screen, but shipped levels end at the **exit door** into
  the next story act and every memory fragment is optional — which had quietly made the
  whole `VictoryScreen`/`CompleteLevel` path (plus its victory music and the orphaned
  gameplay-UI builder) unreachable. The dead path was deleted, and the actual completion
  flow is now stated explicitly in the README and DESIGN.md §4/§12.

Current limitations are listed with their management in §9.

## 4. Testing and what changed because of it

Six recorded rounds in [PlayTestNotes/](PlayTestNotes/), each tied to the changes it
caused:

| Round | Build | What it changed |
| --- | --- | --- |
| 01 (self, 25 May) | v0.1–0.3 | first feel fixes on the day-one slice |
| 02 (self, 27 May) | v0.4 | the game-feel polish pass (juice, audio, font) of v0.5 |
| 03 (coursemate, 28 May) | v0.5 | the **art-direction pivot**: feedback made it plain the procedural look read as a blockout, which kicked off the 1-bit baseline |
| 04 (fresh player, 10 Jun) | v0.9.1 | a six-issue severity list fixed the same night; its pacing findings fed the **Level 3 cut** decision |
| 05 (self, 11 Jun) | v1.0.0 | ship validation against round 04's fixes; caught the silent L2 exit door (re-wired in v1.1.0) |
| 06 (self, 18 Jun) | v1.1.2 | final ship-build validation on the standalone player: L1's S3/S4 finale, the event-driven stun beat and the world-freeze rule all confirmed; L2 exit door now audible; doubled as the demo-video run |

Beyond playtests, every PR merged only after a full play pass of the affected level,
recorded in the DevLog's per-session verification tables; the Level 2 rebuild
additionally used ad-hoc scripted in-editor walkthroughs with reflection-injected
input (PR #152 — those scripts were run-and-discarded, not retained in the repo).

## 5. Reflection: how the game developed from concept to final version

The concept survived; the *production model* changed three times, and the scope changed
once — all recorded as they happened:

1. **All-procedural → licensed 1-bit baseline (28–29 May).** Generated sprites were
   perfect for proving the mechanic in a day and wrong for having an identity. Playtest
   03 made that unmistakable.
2. **Generated scenes → hand-authored scenes (1–10 June).** Once look mattered,
   composition became design work no generator should own. The blockouts were frozen as
   gameplay ground truth and everything visible was placed by hand.
3. **Four levels + a climax milestone → three levels + a finished story (11 June).**
   The honest version of "depth over count": Level 3 existed as a blockout, and cutting
   it bought a real ending (four illustrated acts, a licensed soundtrack) plus a polish
   pass the fourth level would have consumed.
4. **Scale → refinement (12 June).** The last gameplay work expanded an *existing*
   level (Level 1's freight minefield and three-lock finale) rather than adding surface
   area — one more system interaction, zero new scope.

What never moved: the core mechanic and its rules (hold-to-record, 5 seconds, one
echo), written on 19 May and shipped unchanged; and the experience pillars (calm,
melancholic, thinky).

**What this project taught me.**

1. **Testing and detail *are* the experience.** The bugs that mattered were never
   compile errors. They were a player frozen mid-dialogue while a mine kept burning
   beside them; a lesson that displayed before the player had earned it; a caption
   plate that hid the very scene it was captioning. Every one of them was invisible in
   the code and obvious in play — which is why six recorded playtest rounds, and a
   full menu-to-ending run before every merge, were worth more than any amount of
   re-reading the scripts. A game's quality lives in details that only testing can
   surface.
2. **A game is interlocking disciplines, not a program.** Earlier coursework was
   carried by code alone. This project only worked where art, mechanics, level
   geometry, music and worldbuilding *served each other*: the cyan accent is the
   mechanic, the music box is the story, the freeze rule is the dialogue, the level
   order is the tutorial. Any of them built in isolation would have produced a demo,
   not a game.
3. **Creativity is the human part.** AI made implementation dramatically faster, but
   every decision that makes the game *itself* — which art kit to commit to, what the
   cyan means, whose recordings these are, which film feeling to borrow, what to cut —
   required taste and intent that cannot be delegated. The tool executes; the
   direction, selection and meaning are authorship, and they took most of the thinking.
4. **Depth beats breadth — when you actually pay for it.** "Depth over count" was a
   slogan in Week 1 and an invoice in Week 4: cutting Level 3 is what bought the
   ending, the polish pass and the licence cleanup. The scope decisions shaped this
   game more than any feature I added; and since playtests kept correcting the plan,
   reacting well turned out to matter more than predicting well.

## 6. Personal contribution

This is a single-developer project: game design, all level design, the C# gameplay and
editor code, the procedural art/SFX pipeline, asset integration and licensing, testing,
documentation and the presentation build were produced by me, with AI assistance used
as a declared tool under my direction (§7 and
[DECLARATIONS.md](DECLARATIONS.md)). Every commit and merge in the repository is mine.

The parts I would point an assessor to as most representative of my own ability:

- **Level design with a taught difficulty curve.** Each level introduces exactly one
  new relationship to the same mechanic (cooperation → logistics → misdirection), and
  escalates internally — Level 1 ends in a freight minefield and then a three-lock
  finale that needs crate, echo and player in the same instant. Every rule is
  introduced by a safe situation before it is ever demanded under pressure, and the
  trap and terrain set is built to be *read* (fixed cycles, slow telegraphs).
- **Worldview and story design.** The child's-recordings premise — my rework of the
  feeling of *WALL-E* into something smaller and more personal — the four-act
  structure, and the two-voice script (the SYSTEM's clipped white lines against the
  child's cyan PLAYBACK lines).
- **Player-experience detail.** The rules that make the game feel finished came from
  my own play: the world freezing *with* the player during story beats (down to mine
  fuses and drone stun timers), the stun lesson firing only at the player's first
  proven stun, the long "drift away" music transitions, and act music flowing into its
  level as a prelude.
- **Audio and cutscene direction.** Pairing each space with its mood — a C64 lullaby
  for the menu, piano ambience for waking, a literal music box for the playroom,
  a tense pulse for hide-and-seek, Schumann's *Reverie* for the ending — and the
  brief, pacing and curation of the ten illustrated stills.
- **Scope judgement.** The one-day MVP to de-risk the mechanic, the art-direction
  pivot, and the Level 3 cut: the three calls that made the game finishable at
  quality.

## 7. Use of templates, assets, tutorials, or AI support

- **Templates / starter kits:** none for the game itself. The Unity `.gitignore` is
  based on GitHub's public template (declared in
  [DECLARATIONS.md](DECLARATIONS.md)).
- **Tutorials:** no tutorial code was copied into the project.
- **External assets:** one royalty-free 1-bit art kit (its source files — and the seven
  UI sprites cropped from them — kept out of the public repo per its licence), CC0
  backgrounds, OFL fonts, and CC0/CC-BY music — every
  item credited in [CREDITS.md](../Aftertrace_01/CREDITS.md) and declared field-by-field
  in [DECLARATIONS.md](DECLARATIONS.md). All sound effects and the remaining sprites
  are original and generated by the project's own editor tooling.
- **AI support, disclosed in two places:** (a) the ten cutscene illustrations are
  AI-generated to the project's style guide, then curated and integrated by me;
  (b) **Claude Code (Anthropic)** worked as an implementation assistant under my
  direction throughout development — connected to the Unity Editor through the
  open-source **Unity-MCP** bridge (Ivan Murzak, Apache-2.0; declared in
  [DECLARATIONS.md](DECLARATIONS.md) §A16) so it could act inside the editor as well as
  edit files. I designed the mechanics, systems and levels and reviewed/tested
  everything it produced. The full eight-field declaration, including what I changed and
  what I tested, is in [DECLARATIONS.md](DECLARATIONS.md).

## 8. Organisation, time management, independent work, professionalism

- **Organisation:** one PR per feature across the #1–#7 and #144+ PR series (the
  number gap is GitHub issue numbering, and several are documentation-only), each gameplay
  merge leaving a playable build, plus one direct licence-compliance commit recorded in
  the changelog; a session-based dev log (Sessions 01–02 written day-of, Sessions 03–11
  disclosed backfills reconstructed from the PR record and the actual diffs); playtests
  with severity lists gating each milestone; a documented decision record for every
  pivot.
- **Time management:** the schedule absorbed a parallel-deadline week by design — Week
  3 was budgeted for planning only (the redesign spec), so Week 4 could be pure
  execution. The one-day MVP at the start bought certainty about the mechanic before
  any large investment.
- **Professionalism under correction:** when reading the art kit's licence revealed
  that committing its source PNGs to a public repository was not permitted, the repo
  was remediated the same day (sources untracked, restore steps documented, an unused
  pack removed entirely) and the fix is recorded in the changelog rather than hidden.
- **Independent work:** all decisions, designs and verifications are my own; assistance
  (human playtesters, AI tools) is named and scoped in the declarations.
- **Time management (self-assessment):** the honest weakness is cadence. Several PRs
  grew large, so the gaps between merges were sometimes days long (visible in the
  merge history), which made progress lumpy and re-entry more expensive than it needed
  to be. Two habits kept that from hurting the result: every session log ends with the
  next session's plan, so picking the project back up was cheap; and the milestone
  rhythm held — Week 3 was deliberately kept light to absorb parallel deadlines so
  Week 4 could be pure execution, and the total hours were where the plan needed them.
  Next time I would cap PR size and keep a fixed, shorter session cadence even in
  light weeks — steadier beats bigger.

## 9. Known limitations and how they are managed

| Limitation | Management |
| --- | --- |
| Keyboard/mouse only — no gamepad | Out of scope for the assessed build; the input surface is small (legacy `Input` reads in five scripts, the record key a serialized field), so adding one later is a bounded change |
| Desktop only; WebGL untested | Submission targets a desktop build; the repo documents how to build |
| In-game text is English only | A deliberate scope cut (the pixel fonts carry no CJK glyphs); all text lives in builder/scene data, so localisation is a data change |
| One save profile | Matches the 5–10 minute scope; `GameProgress` isolates persistence behind one class |
| Fresh clones must re-download the art kit before first open | A licence obligation, not a defect — the README documents the restore steps (the kit, plus the seven gitignored UI crops) and the order that protects Unity's import metadata |
| The decoy-stun coaching beat fires at the *first* stun anywhere in Level 2 | Intentional (teach at first proof of competence) and recorded; scoping it to specific drones is a one-field change if testing ever argues for it |
| Music tracks come from different artists and masterings | Per-track volume scaling in the audio manager levels the mix |
