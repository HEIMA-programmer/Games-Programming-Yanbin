# Credits

## Code, design & gameplay
- **Yanbin Xu** — design, programming, level design, all editor / procedural tooling,
  audio synthesis pipeline, and integration of the external art / font packs below.

## Art assets (external)

Since M2, Aftertrace ships with a curated 1-Bit pixel-art baseline drawn from
royalty-free / CC0 packs, tinted and composed by this project's editor scripts.
Earlier milestones used 100%-procedural sprites; those generators are kept in source
as fallback (see `Assets/Editor/EchoArt.cs`).

- **[Free Sci-Fi Platformer 1-Bit Pixel Art Game Kit](https://craftpix.net/freebies/free-sci-fi-platformer-1-bit-pixel-art-game-kit/)** — CraftPix.net.
  Royalty-free for commercial and non-commercial use, modification allowed, attribution
  not required. Downloaded 2026-05-28. Used for: player, echo, drone, tilesets
  (floor / wall / borders / details), backgrounds, door, checkpoint, items, props
  (boxes, machines), GUI frame tiles and the dialogue portraits — across all three levels
  and the screen/dialogue framing (the main menu itself uses no kit art).
  Full licence: <https://craftpix.net/file-licenses/> (see also
  `Assets/Art/Imported/CraftPix1Bit/LICENSE.txt`).
  **Note:** that licence forbids redistributing the source art files, so the kit's PNGs
  are **not committed to this public repository** — only the project's `.png.meta` files
  (import settings / sprite slicing) are. The same rule covers the seven standalone UI
  sprites cropped from the kit's sheets in `Assets/Art/Sprites/UI/` (portraits, screen /
  terminal frames, plate pill, lift platform): local-only PNGs with tracked metas, crop
  sources listed per file in `Assets/Art/Imported/SPRITESHEET_NOTES.md`. The README's
  *How to run* section explains how to restore everything in a fresh clone.

- **[Industrial Parallax Background](https://opengameart.org/content/industrial-parallax-background)**
  — Luis Zuno ([@ansimuz](https://ansimuz.com)). **CC0** (stated on the OpenGameArt page and
  in the bundled `Assets/Art/Backgrounds/industrial_LICENSE.txt`). Silhouette layers used as
  the dimmed backdrops in the main menu and Levels 0–2; the `industrial_fore` layer ships
  unused (downloaded 2026-06-12).

- **[Stars — Parallax Backgrounds](https://opengameart.org/content/stars-parallax-backgrounds)**
  — Bonsaiheldin. **CC0** (stated on the OpenGameArt page). Starfield backdrop behind the
  main menu (`stars_small`; `stars_big` ships unused — downloaded 2026-06-12).

## Cutscene illustrations (AI-generated)
- The ten story-interlude images in `Assets/Art/images/` (acts 1-4, shown by the
  `Cut_00`–`Cut_03` scenes) were generated with an AI image model from prompts written
  for this project's 1-Bit + cyan style guide, then curated and integrated by the
  developer. Disclosed here in the interest of transparency.

## Fonts (SIL Open Font License 1.1)
- **[Exo 2](https://fonts.google.com/specimen/Exo+2)** — Natanael Gama. Body / HUD font.
  See `Assets/Fonts/OFL.txt` (downloaded 2026-05-27).
- **[Orbitron](https://fonts.google.com/specimen/Orbitron)** — Matt McInerney. Imported as
  a fallback display font; **ships unused** — no scene, prefab or fallback table references
  it in the final build. See `Assets/Fonts/Orbitron-OFL.txt` (downloaded 2026-05-28).
- **[VT323](https://fonts.google.com/specimen/VT323)** — Peter Hull. Retro pixel terminal
  display font used for the terminal/dialogue text, HUD counter, cutscene captions and
  menu subtitle. See `Assets/Fonts/VT323-OFL.txt` (downloaded 2026-05-28).
- **[Jersey 10](https://fonts.google.com/specimen/Jersey+10)** — The Soft Type Project
  (Sarah Cadigan-Fried). Chunky pixel display font used for the menu title (the cyan
  "3D" extrusion is a TextMesh Pro underlay, not part of the font). See
  `Assets/Fonts/Jersey10-OFL.txt` (downloaded 2026-06-12 from the google/fonts repo).

## Audio

### Sound effects
- **All sound effects** — synthesised at edit time by `Assets/Editor/EchoAudio.cs`
  and `Assets/Editor/EchoCutscenes.cs` (16-bit PCM WAV, deterministic). No
  third-party SFX.

### Music (`Assets/Audio/Music/`, downloaded 2026-06-11 from OpenGameArt.org)
Where a track is multi-licensed on its OpenGameArt page, the license elected for
this project is the one stated below. A copy of these attributions ships next to the
audio files in `Assets/Audio/Music/ATTRIBUTION.txt`, so a build or copy that carries
the music folder carries the CC-BY credit with it.

- **"Slow Melancholic Theme (C64 Style)" — skrjablin** (`menu_c64_lullaby.ogg`,
  main menu — the opening cutscene `Cut_00` shares Level 0's track instead). Elected
  license: **CC0 1.0**.
  <https://opengameart.org/content/slow-melancholic-theme-c64-style>
- **"First Light Particles" — Yoiyami** (`l0_first_light.ogg`, Level 0; converted
  WAV→OGG, otherwise unchanged). License: **CC0 1.0**.
  <https://opengameart.org/content/first-light-particles-%E2%80%93-cc0-atmospheric-pianoambient-track>
- **"Forgotten Lullaby" (Music Box Loop variant) — Mega Pixel Music Lab**
  (`l1_forgotten_lullaby.ogg`, Level 1; converted WAV→OGG, otherwise unchanged).
  Elected license: **CC-BY 4.0** <https://creativecommons.org/licenses/by/4.0/>.
  <https://opengameart.org/content/forgotten-lullaby>
- **"Spooky Dungeon" — Memoraphile / You're Perfect Studio**
  (`l2_spooky_dungeon.ogg`, Level 2). Elected license: **CC0 1.0**.
  <https://opengameart.org/content/spooky-dungeon>
- **Schumann, "Scenes from Childhood" arranged for music box — Gregor Quendel**
  (arr.; after Robert Schumann; source notes credit Bernd Krueger /
  piano-midi.de). Movement used: *Reverie* (`cut3_reverie.mp3`, ending cutscene
  — cutscene acts 1–3 share their following level's track instead). Shipped
  unused alternates: *A Tale of Distant Lands*, *Pleading Child*, *The Poet
  Speaks*, *Blind Man's Buff*, *Hobgoblin*. License:
  **CC-BY 4.0** <https://creativecommons.org/licenses/by/4.0/>.
  <https://opengameart.org/content/schumann-scenes-from-childhood-arranged-for-music-box>

## Engine & tooling
- **Unity 2022.3.62f3 LTS** with the **Universal Render Pipeline** — Unity Technologies.
- **TextMesh Pro** — Unity Technologies. Its bundled essentials include the Liberation Sans
  font (SIL OFL 1.1 — `Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt`) and the
  EmojiOne sample sprites (CC-BY 4.0 — `Assets/TextMesh Pro/Sprites/EmojiOne Attribution.txt`).
- **[Unity-MCP](https://github.com/IvanMurzak/Unity-MCP)** — Ivan Murzak. An editor-time
  bridge that let the AI assistant act inside the Unity Editor (see
  [`Docs/DECLARATIONS.md`](../Docs/DECLARATIONS.md) §A16 and §B). Not part of the game
  build. Plugin packages (`McpPlugin` / `McpPlugin.Common` / `ReflectorNet`) under
  **Apache-2.0**; bundled runtime libraries `R3` (Cysharp) and `Microsoft.*` / `System.*`
  under **MIT**. Binaries in `Assets/Plugins/NuGet/` (versions in `.nuget-installed.json`).
