# Realm Raiders — Playable Prototype

Playable mobile-first Unity prototype for the first two milestones:

- **Character Sandbox:** Blood Knight and Ent share movement, health, armor, attacks, abilities, AI, and death systems.
- **Possession:** switch repeatedly from Keeper overview into the existing Ent and back without loading a scene or creating another character.
- **Sylvan Realm Raid:** explore a seven-node Realm with fog-of-war, fight two Wolves and an Ent, survive a Root Trap, and capture the Heart Tree.
- **Defender Experience:** watch a live AI invasion from Keeper view, trigger a Root Trap, possess the Guardian Ent, and manage a 30-second possession-energy pool.
- **Infernal Realm:** defend an Infernal Heart against a Blood Knight using a Brute, two Hellhounds, Flame Trap, and Lava Gate.
- **Complete prototype loop:** enter the hub, choose a Realm, raid/defend, view results, and return to My Realm.
- **Minimal BUILD proof:** choose five fixed Sylvan creature/trap slots, save the layout, defend exactly that layout, then return to BUILD.

The arena, placeholder characters, materials, abilities, cameras, and UI are generated when the sandbox scene starts. This keeps the prototype self-contained and makes balance values easy to move into authored ScriptableObject assets later.

## Requirements and run

1. Install Unity `6000.6.0f1`. Add Android Build Support for Android Studio exports and iOS Build Support for Xcode exports.
2. Open this folder in Unity Hub. Let Package Manager restore URP and the New Input System.
3. Open `Assets/Game/Scenes/PrototypeHub.unity` and press Play.
4. If Unity asks to switch input handling, choose the New Input System or Both and restart the Editor.

The project setup hook creates and assigns a URP renderer/pipeline asset on first import. Android is configured for portrait orientation, IL2CPP, API 26+, and a 60 FPS runtime target.

Platform export instructions are in [`Docs/PLATFORM_BUILDS.md`](Docs/PLATFORM_BUILDS.md). Project intent, current milestone status, and the bounded polish backlog live in [`Docs/PROJECT_CONTEXT.md`](Docs/PROJECT_CONTEXT.md), [`Docs/PROTOTYPE_STATUS.md`](Docs/PROTOTYPE_STATUS.md), and [`Docs/POLISH_BACKLOG.md`](Docs/POLISH_BACKLOG.md).

## Controls

- **KEEPER:** top-down overview. Tap the green Ent, then press **POSSESS**.
- **PLAY HERO:** control the Blood Knight to validate Milestone 1 combat.
- Tap ground to move.
- Tap the opponent to approach/attack (tap again when in range).
- Swipe to use the directional dash/charge.
- While possessing the Ent, use **SMASH** and **GROUND SLAM** buttons.
- **RELEASE:** instantly restores the Ent's AI and blends back to Keeper view.

Mouse input simulates touch in the Editor.

## Sylvan raid

Open `Assets/Game/Scenes/SylvanRealm.unity` and press Play. Move from the Portal through the Crossroads toward the Heart Tree. Visiting a node reveals only its connected neighbors. The raid result tracks Gold, rare materials, defeated enemies, discovered rooms, duration, and whether the Core was reached. On death, 50% of collected Gold is retained; capturing the Heart Tree grants the victory reward.

## Defender test

Open `Assets/Game/Scenes/DefenderTest.unity` and press Play. The Blood Knight automatically advances toward the Heart Tree and attacks defenders in range. Activate the Root Trap while the invader is standing on its green marker. Tap the Guardian Ent and press **POSSESS ENT** to enter combat; **RELEASE** returns to Keeper view. Possession consumes the shared 30-second pool and releases automatically at zero.

## Sylvan BUILD

From the hub choose **BUILD SYLVAN**. Configure the three creature slots and two trap slots, keeping one Ent, at least one Wolf, exactly one Root Trap, and no more than 10 Threat. Press **SAVE & DEFEND** to load exactly that fixed-slot layout in Defender Test; after the result choose **RETURN TO BUILD**.

## Infernal realm

Open `Assets/Game/Scenes/InfernalRealm.unity` and press Play. The Blood Knight advances through the volcanic path toward the Infernal Heart. The Brute is possessable from Keeper view; Hellhounds attack as fast support creatures. Flame Trap and Lava Gate share the common `TrapBase` state/cooldown implementation.

## Prototype hub

Open `Assets/Game/Scenes/PrototypeHub.unity` to start at **MY REALM**. The hub stores the selected Realm locally with PlayerPrefs and routes to Character Sandbox, Defender Test, Sylvan Raid, or Infernal Realm. Each result screen offers a retry plus **MY REALM** to complete the prototype loop.

## Architecture

`CombatEntity` owns stats, health, abilities, movement, and a single active `IEntityController`. `PlayerController` and `CreatureBrain` coexist on the same GameObject; `SetController` activates exactly one. `PossessionManager` stores the selected/possessed entity, swaps its controller, coordinates the camera, and restores AI on release or death.

The ability layer is data-driven through `AbilityDefinition` and runtime cooldown state. Runtime-created definitions are used for the zero-asset sandbox, while the Create Asset menu supports authored configurations without code changes.

Main areas:

- `Assets/Game/Scripts/Combat` — health, damage, stats, reusable abilities
- `Assets/Game/Scripts/Characters` — common hero/creature entity and character config
- `Assets/Game/Scripts/Controllers` and `AI` — swappable player and FSM controllers
- `Assets/Game/Scripts/Possession` — selection, possession, safe release
- `Assets/Game/Scripts/Realm` and `Raid` — graph fog, Core objective, state machine, result data
- `Assets/Game/Scripts/Camera` — Keeper/Hero/possessed camera modes and 0.65 s blends
- `Assets/Game/Tests/EditMode` — reusable combat logic tests

Run automated tests from **Window → General → Test Runner → EditMode**.

## Known limitations

- Placeholder primitives have no animation, audio, VFX, hit reactions, camera shake, or production collision tuning.
- AI is deliberately minimal and uses direct steering rather than NavMesh pathfinding.
- Heavy-attack hold/release and Ent Root Burst are deferred; the milestone includes basic, dash/charge, and area abilities.
- Combat targeting is single-tap and intentionally simple; no aim indicator or touch-device tuning pass has been completed.
- Runtime ScriptableObjects are not persistent authored assets yet.

## Recommended next steps

1. Tune touch thresholds, attack buffering, hit pause, and Ent weight on a mid-range Android device.
2. Add heavy-attack hold/release, telegraphs, hit reactions, camera shake, and short possession VFX/audio.
3. Convert the default runtime definitions to authored assets and prefabs.
4. Begin post-prototype work with stronger art, audio/VFX, and production-ready scene/prefab authoring.
