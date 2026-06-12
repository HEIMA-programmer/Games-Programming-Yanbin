# Aftertrace — Weekly Plan

> The project's plan-of-record, consolidated at v1.1.1. It replaces the original
> `ROADMAP.md` (five-week / four-milestone outline) and `BACKLOG.md` (issue specs),
> both preserved in git history. Each week below lists the goal set for that week and
> the outcome recorded at its end — including what was carried over or cut, with links
> to the DevLog sessions and PRs that document the work. Outcome legend:
> **✓** done · **→** carried to a later week · **✗** cut (with rationale).

## Week 1 — 19–25 May · M1: concept and proof

**Goal:** lock the concept on paper, then prove the core mechanic in a playable slice
before committing the remaining weeks to it.

| Task | Priority | Outcome |
| --- | --- | --- |
| Game Concept Document (idea, MoSCoW, risks, 5-week plan) | Must | ✓ 19 May |
| Repo structure, Unity 2022.3 LTS + URP project, gitignore | Must | ✓ |
| Echo record/replay prototype (hold R ≤ 5 s, one clone) | Must | ✓ PR #1 (v0.1) |
| First provable two-body puzzle (plate + door) | Must | ✓ PR #1 |
| Menus, HUD, pause, victory, persistent app layer, procedural SFX/music | Must | ✓ PR #2 (v0.2) |
| Level set + level select + cross-level progression | Should | ✓ PR #3 (v0.3) |
| Procedural one-click build pipeline (all assets generated) | Should | ✓ — later retired by design (W4) |

**Exit criteria met:** a stranger can play menu→level→win without help. The whole
skeleton landed in one focused build day (25 May) — see DevLog Session 01.

## Week 2 — 26 May – 1 Jun · M2: mechanical depth and an art identity

**Goal:** give the echo more verbs, make enemies real, and commit to a visual identity
the project can actually finish.

| Task | Priority | Outcome |
| --- | --- | --- |
| Drone line-of-sight cone + Patrol/Alert/Chase/Search/Return AI | Must | ✓ PR #5 (v0.4) |
| Stealth detection meter (gaze fills an alarm, not contact-only) | Should | ✓ PR #5 |
| Game-feel pass: juice, hit-stop, richer audio, Exo 2 font | Should | ✓ PR #6–#7 (v0.5) |
| Process backfill: ROADMAP + BACKLOG docs, rename to **Aftertrace** | Must | ✓ PR #144 (v0.5.1) |
| "Ride your echo" — standable clone | Must | ✓ (v0.6.0) |
| **1-bit art direction**: CraftPix kit baseline, tilemap terrain, VT323, scene frame | Must | ✓ PR #146–#148 (v0.6.0) |
| Pixel-perfect rendering + sprite animation; hand-authored tile palettes over blockouts | Should | ✓ PR #149 (v0.7.0, 1 Jun) |
| "Shift" second signature mechanic — design spike | Should | → W3 → **✗ dropped**: crates + decoy already gave the echo three distinct verbs; a fourth would add breadth the schedule could not polish |

**Exit criteria met:** the game looks like *one* game (1-bit + cyan), and the echo has
puzzle, traversal and stealth value. See DevLog Sessions 02–06.

## Week 3 — 2–8 Jun · low-bandwidth planning week

**Goal (reduced on purpose):** other module deadlines ran in parallel; budget the week
for design only, so Week 4 could be pure execution.

| Task | Priority | Outcome |
| --- | --- | --- |
| M3 redesign spec: narrative spine (whose recordings?), diegetic terminal, per-level themes, crates-as-recordables, hazard set | Must | ✓ spec finalised 8 Jun |
| Decide the pipeline question: keep generating scenes or hand-author over frozen blockouts | Must | ✓ decision: hand-author; builders to be retired |
| "Shift" spike (carried from W2) | Could | ✗ dropped (recorded in W2 row) |

**Exit criteria met:** Week 4 starts with a buildable spec, not open questions.

## Week 4 — 9–15 Jun · M3: content, narrative, ship

**Goal:** rebuild all levels to the spec, deliver the story, and ship the presentation
build.

| Task | Priority | Outcome |
| --- | --- | --- |
| Level 0 hand-authored vertical slice; retire scene builders behind a Legacy menu; dialogue box + screen-frame UI; 384×216 pixel-perfect standard | Must | ✓ PR #150 (v0.8.0, 10 Jun) |
| Level 1 "Playroom": recordable crates + chain pushing, proximity mines, lift, dual-plate gate | Must | ✓ PR #151 (v0.9.0) |
| Level 2 "Hide and Seek": five-segment stealth — searchlight drones, gaze alarm, decoy-stun pass, echo step-stool, crate-as-cover | Must | ✓ PR #152 (v0.9.1) |
| Playtest 04 (fresh player) + same-night fix list | Must | ✓ 10 Jun |
| **Level 3** (blockout existed; full build planned) | Could | **✗ cut 11 Jun** — post-playtest call: end on L2's stealth climax and finish the *story* instead; depth over count (DevLog Session 10) |
| Four illustrated cutscene acts + typewriter captions; licensed music-box/chiptune soundtrack with drift-through-silence transitions | Should | ✓ PR #153 (**v1.0.0**, 11 Jun) |
| Dev-log backfill Sessions 03–10 + Playtests 03–05 | Must | ✓ PR #154 (v1.0.1) |
| Presentation polish: L1 freight-minefield + three-lock finale, story-freeze safety, act/level shared music, Jersey 10 title, CC0 backdrops, diamond fragment art | Should | ✓ PR #155 (v1.1.0, 12 Jun) |
| Story-beat correctness: stun-coaching beat fires on the first decoy-stun | Should | ✓ PR #156 (v1.1.1) |
| **Licence compliance**: CraftPix source PNGs out of the public repo (metas kept, restore steps documented); unused Kenney pack removed | Must | ✓ 12 Jun |
| Playtest 05 (ship-build validation) | Must | ✓ 11 Jun |

**Exit criteria met:** v1.0.0+ plays menu → Act 1 → L0 → … → Act 4 → menu with no
debug input and a real ending.

## Week 5 — 16–21 Jun · submission and presentation

**Goal:** make the work assessable.

| Task | Priority | Outcome |
| --- | --- | --- |
| Documentation rebuild: DESIGN.md (as-shipped design), this PLAN.md, CHANGELOG through v1.1.1, project report, external-resource + AI declarations, README known-issues pass | Must | in progress |
| v1.0 screenshot set for the dev-log evidence folder | Should | planned |
| Final full-run validation on a clean checkout | Must | planned |
| 3-minute presentation + oral-exam preparation | Must | planned |
| Submission build (desktop; WebGL only if required) | Must | planned |

## Milestone map (for the original M-numbering)

| Milestone | Where it landed |
| --- | --- |
| M1 — vertical slice | W1 (one-day burst, PR #1–#3) |
| M2 — mechanical depth + art identity | W2 (PR #5–#149) |
| M3 — content, narrative, audio | W4 (PR #150–#154) |
| M4 — climax level + ship | **does not exist as shipped**: the climax level (L3) was cut 11 Jun; M4's polish duties were absorbed into W4's v1.1.x passes; the game ends with Act 4 instead. Decision record: DevLog Session 10. |
