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
  not required. Used for: player, echo, drone, tilesets (floor / wall / borders / details),
  backgrounds, door, checkpoint, items, props (boxes, machines), GUI tiles, icons.
  Full licence: <https://craftpix.net/file-licenses/> (see also
  `Assets/Art/Imported/CraftPix1Bit/LICENSE.txt`).

- **[UI Pack: Sci-Fi](https://kenney.nl/assets/ui-pack-sci-fi)** — Kenney
  ([kenney.nl](https://kenney.nl)). **CC0 1.0 Universal** — public domain, free for any use,
  attribution not required. See `Assets/Art/Imported/KenneyUI/LICENSE.txt`. Available as a
  full atlas for menu / panel work.

## Fonts (SIL Open Font License 1.1)
- **[Exo 2](https://fonts.google.com/specimen/Exo+2)** — Natanael Gama. Body / HUD font.
  See `Assets/Fonts/OFL.txt`.
- **[Orbitron](https://fonts.google.com/specimen/Orbitron)** — Matt McInerney. Available
  display font (fallback for VT323). See `Assets/Fonts/Orbitron-OFL.txt`.
- **[VT323](https://fonts.google.com/specimen/VT323)** — Peter Hull. Retro pixel terminal
  display font used for the menu title and HUD counter. See `Assets/Fonts/VT323-OFL.txt`.

## Audio
- **All sound effects and music** — synthesised at edit time by
  `Assets/Editor/EchoAudio.cs` (16-bit PCM WAV, deterministic from a fixed RNG seed).
  No third-party audio assets.

## Engine & tooling
- **Unity 2022.3.62f3 LTS** with the **Universal Render Pipeline** — Unity Technologies.
- **TextMesh Pro** — Unity Technologies.
