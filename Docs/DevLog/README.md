# Development Log

This folder is a session-by-session record of how **Aftertrace** was designed, built,
tested, and refined. It exists so that the *process* — not only the final build — can be
followed and assessed.

![Aftertrace main menu](screenshots/menu.png)

## How it works

- **One session = one focused block of work** (early on roughly one per day; in the final
  sprint several blocks in a day). Each session is its own file named
  `Session-NN_YYYY-MM-DD.md`.
- Every session records six things: the **plan**, **what changed** since last time, the
  **design/technical decisions** and how the thinking evolved, **problems and fixes**,
  **testing notes**, and the **plan for next session**.
- Each session links to the **commits / pull requests** made that day, so this written
  record and the Git history corroborate each other.
- Early high-level changes (v0.1 → v0.6) are also summarised in
  [`../CHANGELOG.md`](../CHANGELOG.md); from v0.6 onwards the per-session logs are the
  canonical record. Formal playtest rounds (someone sits down and plays a build) are
  written up in [`../PlayTestNotes/`](../PlayTestNotes/) and linked from the relevant
  session.

## Reading order

Start at Session 01 and read forward. The log is meant to show how the idea moved from
the [Game Concept Document](../../5.19-Aftertrace%20%E2%80%94%20Game%20Concept%20Document.md)
to a polished vertical slice.

## A note on the development model

The development model itself evolved, and the log records each shift honestly:

1. **Day 1 (2026-05-25):** the core game and four levels were built rapidly with a custom
   one-click Unity editor pipeline that procedurally generated **all** art, audio, prefabs
   and scenes — every asset original, the whole project reproducible from source.
2. **Milestone 2 (from Session 03, 2026-05-28):** after a fresh-player playtest, the sprite
   baseline switched to licensed 1-Bit pixel-art kits (credited in
   [`CREDITS.md`](../../Aftertrace_01/CREDITS.md)) while the pipeline stayed one-click.
3. **From Session 06–07 (2026-06-01 → 06-10):** environment art and then level layout
   pivoted to **hand-authored scenes** over procedural gameplay blockouts; the builders
   were retired behind a guarded Legacy menu, and the scenes became the source of truth.
   The final build adds licensed music and disclosed AI-illustrated cutscene art (also in
   `CREDITS.md`); all sound effects remain procedurally synthesized.

From Day 1 onward this log documents the iterative phase: playtesting, balancing,
bug-fixing, redesign, and polish.

## Index

- [Session 01 — 2026-05-25](Session-01_2026-05-25.md) — Core echo mechanic, one-click
  builder, four levels + menu, complete game loop.
- [Session 02 — 2026-05-26 / 2026-05-27](Session-02_2026-05-26.md) — Real drone
  line-of-sight + chase AI and a stealth-detection rule; polish pass (Exo 2 font, death
  hit-stop + shake, plate particle, richer procedural audio); static font-asset fix; and
  the Milestone-1 process backfill — ROADMAP + BACKLOG written, project renamed
  *Echo Shift → Aftertrace*.
- [Session 03 — 2026-05-28](Session-03_2026-05-28.md) — Fresh-player playtest
  ([Playtest 03](../PlayTestNotes/Playtest-03_2026-05-28.md)) re-orders the M2 plan:
  **Ride Your Echo** (standable clone) + the 1-Bit art-baseline switch to licensed kits,
  with `CREDITS.md` and new sprite tooling.
- [Session 04 — 2026-05-29 (pre-dawn)](Session-04_2026-05-29.md) — True 1-Bit pass: flat
  unlit monochrome render, state via brightness/alpha, dithered-rock terrain, clean
  planet/star backgrounds; rising-platform jump fix.
- [Session 05 — 2026-05-29 (evening)](Session-05_2026-05-29.md) — Pixel-scale unification
  (PPU 32 everywhere) and code-painted Tilemap terrain + facility-wall backgrounds from the
  kit's real tiles; collision untouched.
- [Session 06 — 2026-06-01](Session-06_2026-06-01.md) — Pixel-perfect camera + sprite
  animation (player frames, animated doors, exit cinematic) — and the pivot: environment
  art goes **hand-authored** over clean gameplay blockouts, with builder safety guards.
- [Session 07 — 2026-06-10 (afternoon)](Session-07_2026-06-10.md) — Level 0 rebuilt by
  hand as the echo-tutorial vertical slice; diegetic dialogue terminal + screen frame;
  384×216 pixel-perfect camera; sprite grounding; **builders retired** — scenes become the
  source of truth.
- [Session 08 — 2026-06-10 (afternoon)](Session-08_2026-06-10.md) — Echo-recorded crate
  system ("record the world you touched") + Level 1 rebuilt as **SECTOR 01 — PLAYROOM**
  with proximity mines, a lift, and a dual-plate gate where crate *and* echo are provably
  mandatory; MovingPlatform endpoint-freeze fix.
- [Session 09 — 2026-06-10 (evening)](Session-09_2026-06-10.md) — Level 2 rebuilt as
  **SECTOR 02 — HIDE AND SEEK**, a five-segment stealth composite; drone overhaul
  (volumetric searchlight, unified gaze-alarm, decoy-stun, harmless-when-stunned); the
  echo step-stool becomes mandatory.
- [Session 10 — 2026-06-10 → 06-11 (overnight)](Session-10_2026-06-10.md) — Final shape:
  **Level 3 cut**, story ends after Level 2; four AI-illustrated cutscene acts; licensed
  music-box/chiptune soundtrack with drift-through-silence transitions; the
  [Playtest 04](../PlayTestNotes/Playtest-04_2026-06-10.md) fix round (wall-climb, crate
  trains, scene-load flash, L0 HUD); ship-build validation in
  [Playtest 05](../PlayTestNotes/Playtest-05_2026-06-11.md). **v1.0.0.**

---

**To start a new session:** copy [`_TEMPLATE.md`](_TEMPLATE.md), rename it
`Session-NN_YYYY-MM-DD.md`, fill it in the same day, and add a line to the index above.
