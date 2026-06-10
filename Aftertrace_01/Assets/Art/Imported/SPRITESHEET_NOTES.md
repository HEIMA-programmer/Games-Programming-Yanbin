# Spritesheet Audit Notes (CraftPix 1-Bit Kit)

Visual audit of every sheet, frame by frame (annotated-grid inspection, 2026-06-10).
Naming: every sliced frame is `{sheetName}_{index}`, row-major from the top-left.
This file is the source of truth for all frame-array assignments (animations, tiles,
props). Wave 1 = sheets needed for Level_00; Wave 2 audited later (marked TODO).

> Slicing is owned by `Assets/Editor/EchoSpriteSlicer.cs`. Single-sheet re-slice:
> `EchoSpriteSlicer.Slice(path, cellW, cellH)` from script — do NOT blanket-run
> "Slice ALL" after custom rects exist.

---

## Main_Characters/Char_Robot.png — 384×288, grid 48×48 (8×6 = 48 cells) ✔ grid correct

The player robot. Rows top to bottom:

| Frames | Content |
|---|---|
| 0, 8 | walk — stride (legs apart). 0 ≈ 8 (sub-pixel arm variance) |
| 1, 9 | walk — pass (legs together, arm bent) |
| 10 | upright passing pose (legs crossing) |
| 11, 15 | upright idle (11 square-on; 15 narrow profile) |
| 2 | arm extended (point/shoot, standing) |
| 3 | arm extended + muzzle flash |
| 4 | walking + shooting w/ flash |
| **5, 6** | **body dissolving into pixels** — ideal for echo materialize/dissolve |
| 7 | profile stance, arm slightly out |
| 12 | stride with small arm reach |
| **13** | crouch/lean forward, arm out — **push / land pose** |
| 14 | upright, arms crossed small |
| 16–23 | visor-head variant: pointing/action poses (16–21), crouch/jump (22–23) |
| 24–29 | jump set: 24 squat (takeoff), 25 arms-out, 26–28 airborne poses, 29 low slide |
| 32–37 | action row: shooting with sparks (33–36), 37 pointing |
| **40–45** | **death sequence**: 40 hit → 41 falling w/ sparks → 42 collapsing → 43 shatter w/ sparks → 44 parts pile → 45 flat pile |
| 30, 31, 38, 39, 46, 47 | EMPTY |

Suggested sets (verify in motion at Phase 2): run `[0,1,8,9]`-order-by-eye; idle `[11]`(+15 sway);
jump `[26]`; fall `[27]`/`[28]`; push `[13]`; death `[40..45]`; echo fx `[5,6]`.

## Objects/Door.png — 192×48, grid 48×48 (4×1) ✔

`Door_0` closed (white double door) → `Door_1` crack → `Door_2` half open → `Door_3` fully open (black doorway).
Use as `openFrames = [0,1,2,3]` on Door / AutoDoorExit.

## Objects/checkpoint.png — 144×48, grid **16×48 (9×1)** — ✘ was 48×48 (3 poles per cell), FIXED 2026-06-10

9-frame activation strip, one pole per frame:
`0–2` small ball pole (rising) → `3–5` ball + sparks (charging) → `6–8` arrow-flag pole (active).
Idle = 0, activating = 0→8 once, active loop = [6,7,8].
(Checkpoint.prefab has no SpriteRenderer of its own — visual gets built in-scene; safe rename.)

## Objects/Items.png — 128×96, grid 16×16 (8×6) ✔

**Columns = item type, rows = spin phase.** Spin animation = index + 8 per step (6 phases).

| Col (frame 0 idx) | Item |
|---|---|
| 0 | orb/ball |
| 1 | star |
| 2 | cross/plus |
| 3 | ring/donut |
| 4 | heart (current `fragment`) |
| 5 | 4-point gem |
| 6 | square socket gem |
| 7 | key |

E.g. spinning heart = `[4,12,20,28,36,44]`; spinning key = `[7,15,23,31,39,47]`.

## Traps/Trap1.png — 432×48, grid 48×48 (9×1) ✔ — PROXIMITY MINE / burst

`0–1` armed device idle (small riveted mine) → `2–3` triggered (spikes pop) →
`4–7` explosion expanding → `8` smoke rings fading.
Use: idle loop [0,1], explode-once [2..8].

## Traps/Trap3.png — 432×80, grid 48×80 (9×1) ✔ — ELECTRIC ARC VENT (floor plate)

`0` plate idle (no arc) → `1–2` first sparks (telegraph) → `3–8` full lightning column (loop).
Maps directly onto HazardTrap: inactive=`[0]`, warning=`[1,2]`, active loop=`[3..8]`.

## Enemies/Alien1.png — 384×240, grid 48×48 (8×5) ✔ — tentacled crawler (current "drone")

| Frames | Content |
|---|---|
| 0–2, 5–7 | sit/crawl cycle A |
| 8–18 | crawl variants (8–15 cycle B, 16–18) |
| 19–23 | **leap/dash** — tentacles streaming |
| 24–26 | attack/flail |
| **27–29** | **upright floating/ghost hover** — best fit for patrol hover loop |
| 32–37 | crawl/squash; 35–37 collapsing flat (death) |
| 3, 4, 30, 31, 38, 39 | EMPTY |

## Tileset/Tileset.png — 272×224, grid 16×16 (17×14) ✔ — terrain & panels (3×3 blocks)

Organized as 3×3 nine-slice terrain groups (top row / middle / bottom row of indices):

| Group (rows of 3) | Content |
|---|---|
| `0,1,2 / 17,18,19 / 34,35,36` | tech-trim platform: riveted dark top band, white interior, dither fade bottom |
| `6,7,8 / 23,24,25 / 40,41,42` | **speckled white rock terrain** — full 9-slice blob (current `platform` = 24 = its center) → RuleTile source |
| `9,10 / 26,27` + `57,58,59` | speckle variants / extra bottom edges |
| `12,13,14 / 29,30,31 / 46,47,48` | dark slit tech-wall block |
| `51,52,53` & `63,64,65` | standalone tech border strips |
| `85,86,87 / 102,103,104 / 119,120,121` | diagonal hazard-stripe block (+`88,89/105,106` white-bg variant) |
| `91,92,93 / 108,109,110 / 125,126,127` | concentric square ornament panel |
| `94,95 / 111,112` | diamond ornament (black bg) · `173,174 / 190,191` (white bg) |
| `97,98,99 / 114,115,116 / 131,132,133` | horizontal louver/vent block (+`100,101/117,118` dark variant) |
| `136–138`, `142–144/159–161`, `148–150/165–167`, `153–155` | thin connector/hatch strips |
| `170–172 / 187–189 / 204–206 / 221–223` | square-socket wall panel (3 wide × 4 tall) |
| `176–178 / 193–195 / 210–212 / 227–229` | dense circuit-board block |
| `179–181 / 196–198 / 213,214` | circuit variant |
| `182–184`, `199–201`, `216–218` | thin trim strips |
| rest | EMPTY |

## Tileset/Tileset_Borders.png — 176×208, grid 16×16 (11×13) ✔ — assembled 48×48 FRAMES (not terrain caps!)

Each is a 3×3 rounded-square frame (use whole, or as 9-slice for panels):
dotted frame on white `0,1,2/11,12,13/22,23,24`; double-line `3,4/14,15/...`; riveted chunky `6,7,8/17,18,19/28,29,30`;
inner-white `9,10/20,21/31,32`; same set on black bg rows 3–5 (`33–65`); octagonal frame `77–101` area;
hazard-striped frames `80–82/91–93/102–104` (black) & `83–85/94–96/...` (white) … rows 7–12 = same frames on white bg.
**Terrain edge caps live in Tileset.png's own 3×3 groups, not here.**

## Tileset/Tileset_details.png — 304×144, grid 16×16 (19×9) — ⚠ MULTI-CELL PROPS

Props span 2–4 cells; the 16px grid chops them. Keep grid names (legacy refs) and
**append custom whole-prop rects on demand** (named `Tileset_details_prop_*`) when placing decor.
Map: pipes/conduits w/ joints (cols 0–3, rows 0–3); rock/bubble clusters (cols 5–9, rows 0–3);
valve pipes `10–13`, winged console `29–31/48–50`, small machine `32/51`; hanging cables+plugs (cols 15–18);
turbine cross `96,97/114–116`; console banks `100–103/119–123`; wire spiders `105–109/124–128`;
ceiling arc `110–113/129–132`; torpedo machine `133–135/152–155`; console cluster `138–141/157–160`;
antenna cables `143–146/162–165`; bolts/arrows `148–151/167–170`.

## Tileset/Background_n_details.png — 320×160, grid 80×80 (4×2) ✔ — background murals (dark grey on black)

`0` dither gradient fade · `1` diagonal beam/girder · `2` circuit junction w/ rivets · `3` structural corner girders
`4` planet + moons cluster · `5` octagonal portholes panel · `6` starfield · `7` circuit traces.
Place as SpriteRenderers on parallax layers (80px ≠ 16px grid), positions snapped to 1/32.

## GUI/GUI_Elements.png — 96×160, grid 16×16 (6×10) ✔

- Battery meter strips (2 cells wide): `(0,1) (6,7) (12,13) (18,19) (24,25) (30,31) (36,37) (42,43) (48,49) (54,55)` = full→empty 10 states.
- Rounded screen frame 2×2: `2,3 / 8,9`; counter zeros `4,5`.
- Bolt icon `10`; diagonal corner `11`.
- **Portrait frames 2×2 with human face (headphones)**: `20,21/26,27`, `32,33/38,39`, `44,45/50,51` — 3 variants.
  → terminal speaker avatar for the child-engineer story.
- Bolt+bars energy levels: `22,23 / 28,29 / 34,35 / 40,41 / 46,47 / 52,53` (VI→I).
- Solid dots: `56` black round, `57` white round, `58` black square, `59` white square.

## GUI/Tileset_GUI.png — 176×368, grid 16×16 (11×23) ✔ — UI chrome kit

- Rows 0–5: same 3×3 frames as Tileset_Borders (dotted/riveted/double/inner) black & white versions.
- Pill buttons 3×1ish: white-on-frame `66–68`, `72–74`; close chip `70` / `76`.
- Octagon frames: black bg `88–112` area, white bg `121–151` area; hazard-striped square `91–93/102–103/113,114` etc.
- Button bars: dark `154–156`, `160–162`; white pill `176–178`, `182–184`; black pill `198–200`, `204–206`;
  dark trapezoid plate `220–222`, `226–228`; white trapezoid `242–244`, `248–250`; close chips `158/164`.
- 9-slice candidate for the terminal window: dotted 3×3 frame (`0,1,2 / 11,12,13 / 22,23,24`).

## GUI/Icons.png — 96×192, grid 16×16 (6×12) ✔

Rows 0–5 WHITE, rows 6–11 same icons BLACK (+36 offset):
`0` skull · `1` gun · `2` chest · `3` lock · `4` key · `5` heart · `6` coins · `7` star · `8` magnet · `9` speech bubble ·
`10` gem · `11` ammo · `12` target · `13` refresh · `14/15` audio-/+ · `16` play · `17` pause · `18` $ · `19` trophy ·
`20` person · `21` check · `22` X · `23` square · `24` info · `25` ? · `26` O · `27` … · `28` grid · `29` gear ·
`30/31/32/33` arrows up/down/left/right. (Black right arrow = `69` = current `arrow` mapping.)

---

## Custom whole-prop rects (appended to importers — keep this list in sync!)

| Sheet | Sprite name | Rect (x,y,w,h bottom-origin) | Content |
|---|---|---|---|
| Tileset_GUI | `Tileset_GUI_frame48` | 0,320,48,48 | dotted 9-slice frame, white bg (border 16) |
| Tileset_GUI | `Tileset_GUI_frame48_black` | 0,272,48,48 | dotted 9-slice frame, black bg (border 16) — terminal window |
| Tileset_GUI | `Tileset_GUI_pill48` | 0,96,48,16 | white pill button in frame — pressure-plate visual (content 13px tall, padBottom 3) |
| Tileset_details | `Tileset_details_prop_console` | 80,32,64,32 | console bank machines w/ legs (padBottom 2) — boot-zone set dressing |
| Tileset_details | `Tileset_details_prop_cables` | 272,96,32,48 | hanging cable pair w/ plugs (hangs from rect top) |
| Tileset_details | `Tileset_details_prop_arc` | 240,32,64,32 | ceiling arc lamp + pendant (padTop 1) |

⚠ **NEVER reference importer-appended custom rects from prefabs/scenes** — they all get
FileId 0 ("Identifier uniqueness violation" warning), so saved references randomly resolve
to null after any reload/reimport (bit us twice: terminal frame, then screen frame+portrait).
Grid-named `{sheet}_{i}` sprites are stable; custom rects are fine for tile assets and
one-off lookups but NOT for serialized references.
**Rule: any sprite a prefab/scene must reference gets cropped to a standalone PNG** in
`Assets/Art/Sprites/UI/` (PPU 32, point, single, FullRect; 9-slice border on the importer).
Current standalone set: `portrait_a/b/c` (GUI_Elements faces), `screen_frame_riveted`
(Tileset_Borders 6-8/17-19/28-30, border 16 — full-screen border, Image type **Tiled**),
`terminal_frame_black`, `plate_pill`.

## Grounding rule (measured 2026-06-10)

The kit reserves a consistent **13px bottom padding** inside 48px-tall cells (stride frames
14px = intentional 1px bounce). With center pivots: `visualOffsetY = (cellH/2 − padBottom)/32`.
Applied: Player/Echo Visual −0.15625; Checkpoint Visual −0.65625 (root at y=1); Door Body
y=0.75 world @ ×2 scale (padBottom 12); Trap3 root 0.84375; Items pad 2 (floats by design).

## Background composition language (from official kit previews, studied 2026-06-10)

Official mockups never leave the backdrop black: bays divided by hatched girder beams
(`136-138` h / rotated for verticals) + rivet trim (`216-218`), filled with diagonal-strut
panels (`85/102/119` 9-slice), dark circuit traces (`179-181/196-198`), dark louvers
(`100,101/117,118`), slit wall (`12-14/29-31/46-48` — door shafts), socket/ornament panels;
greys reserved for background depth (BackWall tilemap.color 0.42, murals 0.26–0.5),
foreground stays pure white. Tile assets for all of these live in `Assets/Art/Tiles/`.

## Objects/Boxes.png — 128×128, grid 32×32 (4×4) ✔ — supply chests (audited 2026-06-10)

| Frames | Content |
|---|---|
| 0, 2 | closed chest w/ clasps, flat top — **the pushable crate** (content 16×14 px, padBottom 13) |
| 1, 3 | ornate chest w/ lock emblem (closed) |
| 4, 8, 12 | simpler closed chest variants |
| 5, 9, 13 | chest OPEN (lid up) — future "loot/reveal" beats |
| 6, 7, 10, 11 | broken/crumbling chest |
| 14, 15 | scattered debris |

PushableCrate.prefab uses `Boxes_0` at ×2 scale (1×0.875u collider), Visual child local (0,-0.25).

## Wave 2 — TODO (audit before use)

`Main_Characters/Char_Boy.png` (48×48), `Char_Girl.png` (48×48), `Char_fire.png` (32×32 — verify!),
`Enemies/Alien2–6`, `Objects/Cups.png` (16×16 vials),
`Traps/Trap2.png` (16×16), `Trap4.png` (48×48), `Trap5.png` (32×32), `Trap6.png` (48×48),
`GUI/Numbers.png`, `GUI/Text1/2_*.png`.
