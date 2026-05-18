# Space Blaster — 2D Game Improvement

**▶ Play in browser:** https://HEIMA-programmer.github.io/Games-Programming-Yanbin/In_class_activity/2D_Game/play/

A top-down 2D space shooter built on top of a Unity 2D starter pack.
Defeat 10 enemies to clear the level while dodging incoming fire and adapting
to ever-rising difficulty.

- **Author:** Yanbin Xu
- **Course:** 2D Game Improvement Assignment
- **Engine:** Unity 2022.3.62f3 (built-in Render Pipeline, 2D)
- **Start scene:** `Assets/Scenes/MainMenu.unity`
- **Game scene:** `Assets/Scenes/SampleScene.unity`

---

## How to Play

| Action | Input |
|--------|-------|
| Move spaceship | `W` `A` `S` `D` |
| Aim | Mouse |
| Fire weapons | Left Mouse Button |
| Pause / Resume | `Esc` |

**Goal:** Defeat **10 enemies** to win the level. You start with **3 HP** and
**3 lives**. Regular enemies are worth **5 points**; the more dangerous Chaser
enemies are worth **20 points**.

---

## Player Flow

```
MainMenu  ──▶  New Game  ──▶  Gameplay  ──▶  Victory / Game Over
   │                              │
   │                              ├──▶  Pause (Esc)  ─▶ Resume / Restart / Quit / Main Menu
   │                              │
   │                              └──▶  HUD always visible (Score / HP / Lives / Enemies X/10)
   │
   ├──▶  Instructions  ──▶  Main Menu
   ├──▶  Level Select  ──▶  Level 1 (Gameplay)
   └──▶  Exit Game
```

---

## Improvements Summary

The base project shipped as a kit of prefabs and scripts with no working scene
and several broken behaviours. The work below maps directly to the assignment's
five rubric headings.

### 1. Goals & Interaction

- **Live objective counter** in the HUD: `Enemies: X / 10` so the player always
  knows how close they are to winning.
- **Instructions page** in the main menu explains the goal, controls, scoring,
  and difficulty rules before the first run.
- **"LEVEL X!" pop-ups** during play (driven by `DifficultyManager`) signal
  that the pace has just stepped up — players read the on-screen progression
  rather than guessing why enemies got harder.

### 2. Menus & Instructions

- **Main Menu** scene (`MainMenu.unity`) with New Game / Instructions /
  Level Select / Exit Game.
- **Pause Menu** (Esc in-game) with Resume / Restart / Quit / Main Menu.
- **Game Over screen** triggered when the player runs out of lives.
- **Victory screen** when 10 enemies are defeated.
- **Instructions page** containing OBJECTIVE / CONTROLS / TIPS / CREDITS — also
  reachable as a back-able sub-page so the player can review the rules without
  leaving the menu.

### 3. HUD & Feedback

- **4-element HUD**: Score, HP (current/max), Lives, Enemies (X/10).
- **HitFlash**: the player sprite briefly flashes red on every hit, even during
  invincibility frames, so every incoming shot is visible.
- **CameraShake**: a short screen shake on player damage adds weight to hits.
- **Hit particles & SFX**: every projectile, enemy death, and player death has
  positioned particle bursts (the original prefabs were spawning particles in
  the wrong world position; this is now fixed).
- **Level-up screen-shake**: a slightly larger shake when difficulty
  increases, reinforcing the "LEVEL X!" message.

### 4. Gameplay Improvement

- **Dynamic Difficulty Scaling** (`DifficultyManager.cs`)
  - Every **3 enemies defeated**, all `EnemySpawner` components get their
    `spawnDelay` multiplied by `0.85` (15 % faster spawning), down to a floor
    of `0.5 s`.
  - Difficulty is capped at level 8 to keep the late game beatable.
  - On every level-up the manager fires a UI pop-up, a camera shake, and
    Debug logs so the change is felt, not just timed.

This single system transforms the game from a flat shooting gallery into a
short escalation curve: the first 30 seconds are calm, the last 30 seconds
are chaotic, and the player can read the difficulty rising in the HUD.

### 5. Technical Quality & Polish

The base project had several latent bugs that broke the player experience —
all fixed:

| Original bug | Fix |
|--------------|-----|
| Chaser enemies "self-destructed" against the asteroid wall, each one counting as a kill — the game spuriously declared victory after \~10 seconds with no player input | Added a guard in `Damage.DealDamage` so `Enemy.DoBeforeDestroy` only fires when the target is **not** `isAlwaysInvincible` (i.e., walls don't count as kills). |
| Bullet hit particles flew off to random screen positions, looking like the bullet had "ricocheted" | Fixed `LocalPosition`/`LocalRotation` on every effect prefab (12 in total) so particles spawn at the impact point, set particles to one-shot (`looping: 0`, `stopAction: Destroy`, `lengthInSec: 0.5`), changed render mode from Stretched-Billboard to Billboard, and zeroed `startSpeed` so particles stay put. |
| Chaser damage of `10` instantly killed the 3-HP player on first contact | Re-balanced to `1` per touch, so contact is punishing but not instant death. |
| Damage feedback was invisible during invincibility frames — the player could not tell hits had registered | Re-ordered `Health.TakeDamage` so HitFlash and CameraShake always fire on hit, while the actual damage is gated by invincibility. |
| HUD text invisible because RectTransform pivots defaulted to centre while anchors were corner-based | Set pivots to match anchors (top-left for Score/HP, top-right for Lives/Enemies). |
| Instructions panel was an empty stub in the prefab | Replaced with a full INSTRUCTIONS page containing OBJECTIVE, CONTROLS, TIPS, and CREDITS, parented to the original `Instructions` UIPage so the existing `GoToPageByName("Instructions")` flow keeps working. |
| TMP labels swallowed mouse clicks because `RaycastTarget: 1` is the default | Disabled `RaycastTarget` on display-only TMP labels (Title, BodyText) so the Main Menu button under them is clickable. |

---

## Project Structure (where each improvement lives)

```
Assets/
├── Scenes/
│   ├── MainMenu.unity      ← Start-up scene (menu + camera + UIManager)
│   └── SampleScene.unity   ← Main game scene (player, enemies, HUD)
├── Scripts/
│   ├── Polish/             ← NEW scripts written for this submission
│   │   ├── HitFlash.cs            – Sprite colour flash on damage
│   │   ├── CameraShake.cs         – Screen-shake offset for CameraController
│   │   └── DifficultyManager.cs   – Scales spawn rates with kill count
│   ├── UI/                 ← NEW HUD display scripts
│   │   ├── HealthDisplay.cs           – "HP: 2 / 3"
│   │   ├── LivesDisplay.cs            – "Lives: 3"
│   │   └── EnemiesDefeatedDisplay.cs  – "Enemies: 4 / 10"
│   ├── Camera/CameraController.cs     – Modified to read CameraShake offset
│   ├── Health&Damage/
│   │   ├── Damage.cs                  – Guard against invincible kills
│   │   └── Health.cs                  – Always-on hit feedback
│   └── Utility/GameManager.cs         – Added public EnemiesDefeated getter
└── Prefabs/                ← Modified effect prefabs (positions / particles)
```

---

## Credits

- **Base project / sprites / audio:** Unity 2D Starter Pack (provided)
- **Improvements, scripts, scene assembly:** Yanbin Xu, 2026

---

## Build Instructions (Developer)

To produce a standalone executable:

1. Open the project in Unity **2022.3.62f3** (or compatible 2022 LTS).
2. `File → Build Settings…`
3. **Scenes In Build** should be:
   - `0` `Scenes/MainMenu` (start scene — **must be index 0**)
   - `1` `Scenes/SampleScene`
4. **Platform:** `Windows, Mac, Linux` (or your target).
5. **Architecture:** `x86_64` (Windows).
6. Click **Build**, pick an empty output folder, and wait for the build to
   finish.
7. Run `SpaceBlaster.exe` (or whatever name you chose) in the output folder.
