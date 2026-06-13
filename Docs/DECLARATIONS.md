# Aftertrace — External Resource & AI Declarations

> Field-by-field declarations for **every** external resource and AI tool used in the
> project, in the module's requested format. Anything not listed here (all gameplay
> code, level design, sound effects, and the procedural UI/effect sprites) is original
> work by the developer. Licence texts and download dates are also recorded in
> [CREDITS.md](../Aftertrace_01/CREDITS.md).

---

## A. External resources

### A1. Free Sci-Fi Platformer 1-Bit Pixel Art Game Kit

1. **Name of resource:** Free Sci-Fi Platformer 1-Bit Pixel Art Game Kit
2. **Type:** asset (sprite sheets / tilesets)
3. **Source:** CraftPix.net — <https://craftpix.net/freebies/free-sci-fi-platformer-1-bit-pixel-art-game-kit/>
4. **Licence or permission:** CraftPix Freebies License (<https://craftpix.net/file-licenses/>) — royalty-free use and modification in unlimited projects; **redistribution of the source art files is prohibited**, so the PNGs are excluded from the public repository (local-only, gitignored; only Unity `.meta` import data is tracked) — **including the seven standalone UI sprites cropped from the kit's sheets** in `Assets/Art/Sprites/UI/` — and the README documents how a fresh clone restores them. Downloaded 2026-05-28.
5. **What it provided:** player/echo/drone character sheets, 16×16 terrain tilesets and borders, door/checkpoint/item/prop/trap sheets, GUI tile atlas.
6. **What I used unchanged:** the pixel art itself (no recoloured or repainted copies exist on disk; the only kit-derived files are the gitignored source PNGs and the seven gitignored UI crops, whose source sheets and cells are documented in `Assets/Art/Imported/SPRITESHEET_NOTES.md`).
7. **What I modified:** frame selection and grid slicing (Unity import settings), runtime tinting to the 1-bit palette (white/cyan via `SpriteRenderer.color`), composition into tilemaps, and the seven standalone crops above.
8. **What I created myself:** every level layout, tilemap composition, collision setup and animation timing that uses the kit.
9. **Where it appears in my game:** all three levels, plus the screen/dialogue framing used in levels and cutscenes (characters, terrain, doors, checkpoints, items, props, the screen-frame and portrait crops). The main menu itself uses no kit art.
10. **How it is credited:** `CREDITS.md` (with licence link) and `Assets/Art/Imported/CraftPix1Bit/LICENSE.txt`.

### A2. Industrial Parallax Background

1. **Name:** Industrial Parallax Background
2. **Type:** asset (background art)
3. **Source:** Luis Zuno (@ansimuz) via OpenGameArt — <https://opengameart.org/content/industrial-parallax-background>
4. **Licence:** CC0 1.0 (stated on the page; copy bundled at `Assets/Art/Backgrounds/industrial_LICENSE.txt`). Downloaded 2026-06-12.
5. **What it provided:** layered industrial skyline silhouettes.
6. **Used unchanged:** the silhouette art.
7. **Modified:** dark grey tinting, tiling widths, per-scene layer selection.
8. **Created myself:** the camera-following backdrop rig and per-level layer mixes.
9. **Where it appears:** dim backdrops in the main menu and Levels 0–2.
10. **Credited:** `CREDITS.md` + bundled licence file.

### A3. Stars — Parallax Backgrounds

1. **Name:** Stars (parallax backgrounds)
2. **Type:** asset (background art)
3. **Source:** Bonsaiheldin via OpenGameArt — <https://opengameart.org/content/stars-parallax-backgrounds>
4. **Licence:** CC0 1.0 (stated on the page). Downloaded 2026-06-12.
5. **Provided:** two starfield layers.
6. **Used unchanged:** the star textures.
7. **Modified:** tint/brightness, tiling.
8. **Created myself:** placement and combination with the skyline.
9. **Appears:** main-menu backdrop.
10. **Credited:** `CREDITS.md`.

### A4–A7. Fonts (all SIL Open Font License 1.1)

Common fields — **Type:** asset (font) · **Licence:** SIL OFL 1.1, full licence files
committed beside each font in `Assets/Fonts/` · **Used unchanged:** the `.ttf` files ·
**Modified:** baked into TextMesh Pro SDF atlases (an import step, not a font
modification) · **Created myself:** all text content and layouts · **Credited:**
`CREDITS.md` + per-font OFL text files.

4. **VT323** — Peter Hull, via Google Fonts. Appears: terminal/dialogue text, cutscene captions, HUD counter, menu subtitle.
5. **Exo 2** — Natanael Gama, via Google Fonts. Appears: body/UI text.
6. **Orbitron** — Matt McInerney, via Google Fonts. Appears: nowhere in the shipped build — imported early as a fallback display font and **ships unused** (no scene, prefab or fallback table references it; kept for completeness, declared for honesty).
7. **Jersey 10** — The Soft Type Project (Sarah Cadigan-Fried), via the google/fonts repository (downloaded 2026-06-12). Appears: the main-menu title. The cyan "3D" extrusion is a TextMesh Pro underlay effect I configured, not part of the font.

### A8–A12. Music (OpenGameArt, downloaded 2026-06-11)

Common fields — **Type:** audio · **Used unchanged:** the recordings (two converted
WAV→OGG, otherwise untouched) · **Modified:** none musically; per-track volume scaling
at runtime · **Created myself:** the transition system that plays them
(drift-through-silence) and the act/level pairing · **Credited:** per-track entries
with elected licences in `CREDITS.md`. Where a page offers multiple licences, the
elected one is stated below.

8. **"Slow Melancholic Theme (C64 Style)"** — skrjablin — CC0 1.0 — main menu (the opening act shares Level 0's track instead). <https://opengameart.org/content/slow-melancholic-theme-c64-style>
9. **"First Light Particles"** — Yoiyami — CC0 1.0 — Level 0 (+ Act 1 prelude). <https://opengameart.org/content/first-light-particles-%E2%80%93-cc0-atmospheric-pianoambient-track>
10. **"Forgotten Lullaby" (music-box loop)** — Mega Pixel Music Lab — CC-BY 4.0 (attribution in CREDITS) — Level 1 (+ Act 2 prelude). <https://opengameart.org/content/forgotten-lullaby>
11. **"Spooky Dungeon"** — Memoraphile / You're Perfect Studio — CC0 1.0 — Level 2 (+ Act 3 prelude). <https://opengameart.org/content/spooky-dungeon>
12. **Schumann, *Scenes from Childhood* arranged for music box** — Gregor Quendel — CC-BY 4.0 — movement *Reverie* scores the ending act; five alternate movements ship in the repo unused (declared in CREDITS). <https://opengameart.org/content/schumann-scenes-from-childhood-arranged-for-music-box>

### A13. AI-generated cutscene illustrations

1. **Name:** the ten story-interlude images (`Assets/Art/images/`, acts 1–4)
2. **Type:** AI (image generation)
3. **Source:** generated for this project with ChatGPT (OpenAI), using its GPT Image 2
   image-generation model.
4. **Licence/permission:** generated content used under OpenAI's content terms (outputs
   usable by the generating user); disclosed as AI-generated.
5. **What it provided:** ten illustrations matching the project's 1-bit + cyan style guide.
6. **Used unchanged:** the selected final images.
7. **Modified:** selection/curation from a larger generated set, import settings, framing and pacing inside the cutscene player.
8. **Created myself:** the style guide and prompt brief (drafted with the AI assistant, see §B), the act/caption structure they illustrate, and the cutscene system that presents them.
9. **Where they appear:** scenes `Cut_00`–`Cut_03`.
10. **Credited:** disclosed in `CREDITS.md` ("Cutscene illustrations (AI-generated)") and in the game README.

### A14. Unity Engine, Universal Render Pipeline, TextMesh Pro

1. **Name:** Unity 2022.3.62f3 LTS + URP (2D renderer) + TextMesh Pro
2. **Type:** other (engine/middleware)
3. **Source:** Unity Technologies (Unity Hub / Package Manager)
4. **Licence:** Unity student/personal terms; bundled packages under the Unity Companion License.
5. **Provided:** engine, 2D renderer + 2D lights, text rendering.
6. **Used unchanged:** the engine and packages.
7. **Modified:** project configuration only (URP assets, pixel-perfect camera settings).
8. **Created myself:** everything built on top.
9. **Appears:** the entire project.
10. **Credited:** `CREDITS.md` (Engine & tooling).

### A15. Unity `.gitignore` template

1. **Name:** GitHub `gitignore` repository — Unity template
2. **Type:** template (configuration)
3. **Source:** <https://github.com/github/gitignore> (`Unity.gitignore`)
4. **Licence:** CC0 1.0
5. **Provided:** the standard Unity ignore ruleset.
6. **Used unchanged:** most standard rules.
7. **Modified:** project-specific additions (WebGL hand-in note, the CraftPix local-only block).
8. **Created myself:** the project-specific rules.
9. **Appears:** `Aftertrace_01/.gitignore` (header credits the source).
10. **Credited:** comment at the top of the file.

### A16. Unity-MCP editor bridge (and its bundled runtime libraries)

1. **Name:** Unity-MCP — a Unity Editor plugin (`com.IvanMurzak.McpPlugin` 6.7.1, `McpPlugin.Common` 6.7.1, `ReflectorNet` 5.3.1) plus its NuGet dependency closure.
2. **Type:** other (editor-automation tooling / middleware — **editor-time only, not part of the game build**).
3. **Source:** Ivan Murzak — <https://github.com/IvanMurzak/Unity-MCP>. The bundled runtime libraries are standard NuGet packages: `R3` 1.3.0 (Cysharp) and ~38 `Microsoft.*` / `System.*` .NET libraries (e.g. SignalR client, `Microsoft.CodeAnalysis`, `System.Text.Json`). Full version list: `Assets/Plugins/NuGet/.nuget-installed.json`.
4. **Licence or permission:** the Unity-MCP packages (McpPlugin, McpPlugin.Common, ReflectorNet) are **Apache-2.0**; `R3` and the `Microsoft.*` / `System.*` runtime libraries are **MIT**. All permit redistribution, so the DLLs are tracked in the repo.
5. **What it provided:** a bridge that lets an MCP-speaking AI client (here, Claude Code — see §B) drive the Unity Editor over a local SignalR/HTTP connection — inspecting and modifying scenes, GameObjects, components and prefabs, opening/saving scenes, taking screenshots, toggling play mode, and reading the console. I used it as a productivity layer over editor work I would otherwise do by hand.
6. **What I used unchanged:** the plugin and its DLLs exactly as installed via the package manager.
7. **What I modified:** nothing in the plugin — local configuration only (the editor-server connection).
8. **What I created myself:** every scene, GameObject, component value and asset the tool was used to author or change. The bridge executes editor operations; the level design, the wiring decisions and the verification are mine (§B).
9. **Where it appears:** editor-time only. The DLLs live in `Assets/Plugins/NuGet/`; the plugin is referenced by **no** shipped scene, prefab or gameplay script, and is not included in a player build.
10. **How it is credited:** this declaration, `CREDITS.md` (Engine & tooling), and the binaries' own Apache-2.0/MIT licences as shipped.

**Beyond the resources declared above (A1–A16), no other external assets, templates,
tutorial code or code snippets were used.** All sound effects are synthesised by the
project's own editor scripts; all sprites outside A1 and A13 are generated by the
project's own tooling.

---

## B. AI assistance declaration

**Working model.** I used one AI coding tool throughout development, in a fixed
division of labour: **I owned the design — mechanics, systems, level layouts, art
direction, tuning intent — and the verification; the assistant implemented to my
specification.** It worked two ways under my supervision: editing project files
(scripts, docs) directly, and driving the **Unity Editor through the Unity-MCP bridge**
(§A16) — modifying scenes and prefabs, wiring components, taking screenshots and running
play-mode checks. Both are editor operations I can and do perform by hand; the bridge is
an efficiency layer, not a substitute for knowing the engine. I reviewed every resulting
change, tested it in the editor, and made every commit, merge and release myself.

1. **Tool used:** Claude Code (Anthropic), used in supervised working sessions from
   25 May to June 2026, connected to the Unity Editor through the open-source Unity-MCP
   bridge (§A16) so it could act inside the editor as well as edit files. (A separate AI
   image tool was used only for the cutscene illustrations — declared in §A13.)
2. **What I asked:** to implement the C# systems I had designed (I specified the
   behaviour, the function-level architecture and the acceptance criteria); to help
   debug issues my playtests found; to build and adjust scene content to my layouts;
   to research asset/music licences before anything entered the public repository; and
   to draft documentation (dev logs, READMEs, this document set) from the development
   record for my review.
3. **What output I used:** code and scene changes that passed my review and in-editor
   testing; documentation drafts after my editing; licence research after I checked the
   cited sources.
4. **What I changed:** the development record shows a steady pattern of me redirecting
   or overriding the assistant's output. Examples traceable in the history:
   - **Story presentation details:** I rejected an opaque caption plate and had it
     reverted to the translucent band so the scene stays visible behind captions;
     I caught the previous story beat flashing through when the next one opened, and
     the cutscene `[ SPACE ]` hint clipping under the screen frame, and directed both
     fixes.
   - **World rules during dialogue:** after dying to a mine while frozen in a story
     beat, I specified that story freeze must extend to the whole world — drones,
     mine fuses, traps, echo replay, and later the drone's stun countdown. I also had
     the stun-coaching beat changed from a floor-position trigger to firing at the
     player's *first proven* decoy-stun.
   - **Player-behaviour rules:** I banned the wall-climb and the jump-over-the-drone
     routes after finding them in play, and required side-by-side crates to push as a
     chained train instead of jamming.
   - **Audio feel:** I replaced the original shorter fade-outs with long "drift-away"
     transitions, and asked for each act's music to carry into its level as a prelude.
   - **Scope:** I cut Level 3 for the three-minute presentation and chose deepening
     Level 1 (freight minefield, three-lock finale) over adding new surface area.
5. **How I tested it:** every change was played in the editor before it counted — a
   full pass of the affected level, and a full menu-to-ending run before each merge.
   Five recorded playtest rounds (two with an external coursemate tester) produced
   severity-ranked issue lists that were fixed and then re-verified against the next
   build — Playtest 05 validates Playtest 04's fixes item by item
   ([PlayTestNotes/](PlayTestNotes/)). UI and scene work was checked visually against
   screenshots; after one fix was silently lost in a save, scene changes were also
   confirmed by reopening the saved scene from disk. Puzzle invariants — doors latching
   open, mines ignoring crates, the fragment reachable from the crate top but provably
   not from the ground, story beats firing once and only on their intended condition —
   were checked manually in the editor each pass, supplemented during the Level 2
   rebuild by ad-hoc scripted walkthroughs with reflection-injected input (PR #152;
   those scripts were run-and-discarded, not retained in the repo). Nothing was
   committed untested, and every commit and merge is mine.
6. **What I understand:** I double-checked every AI-assisted script before it was
   committed, and I can explain the systems they implement — above all the core
   mechanic and gameplay scripts: the recorder/clone pipeline (per-physics-frame
   sampling, kinematic replay, the one-echo rule and dissolve conditions), the player
   controller (variable jump height, the ground check and why its size matters), the
   pressure-plate/door system including the three-lock latch, the drone's five-state
   machine and vision cone (range + angle + sight ray, clone-priority targeting, the
   leash), the story-freeze rule and everything it gates, and the audio transition
   curves (drift-out, silence gap, bloom-in). The same applies to this documentation
   set: I verified it against the repository history so it records what actually
   happened, completely. I also understand the **editor operations** the Unity-MCP
   bridge (§A16) carried out on my instruction — scene composition, component wiring,
   the YAML serialization of scenes and prefabs, screenshots and play-mode verification
   — well enough to direct each one and to review its result the same way I review code;
   the bridge accelerated editor work I can do by hand, it did not stand in for knowing
   how the engine behaves.
7. **What I still do not fully understand:** the internals I use through configuration
   rather than having opened up — the shader-level detail of TextMesh Pro's SDF text
   rendering and its underlay (I know what the underlay does and why it produces the
   menu title's extrusion, not the shader mathematics); the DSP internals of the
   procedural sound-effect synthesis scripts (I directed and accepted their results by
   ear); how URP's 2D volumetric lights are rendered behind the drone searchlights; and
   the inner workings of the **Unity-MCP bridge itself** — the MCP/SignalR transport
   between the AI client and the editor, and the ReflectorNet layer that maps a tool
   call to a Unity API call (I use it through its tool interface and verify its effects
   in the editor, but did not implement it). For each of these I can explain what it
   does, why it is in the project, and how it is configured — but not their inner
   implementation.
8. **Where it appears in the project:** the runtime gameplay code
   (`Aftertrace_01/Assets/Scripts/`), the editor tooling (`Assets/Editor/`), scene
   content built with that tooling, and the documentation under `Docs/`. The cutscene
   images are the separate declaration in §A13.
