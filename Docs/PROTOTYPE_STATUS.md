# Realm Raiders — Prototype Status

Last reviewed: 2026-09-04

## Implemented milestones

| Milestone | Status | Current implementation |
| --- | --- | --- |
| 1. Character Sandbox | Functional greybox | Blood Knight and Ent share combat, movement, abilities, AI, health and death. |
| 2. Possession | Functional greybox | Keeper selection, same-entity controller swap, camera transition, release and death handling. |
| 3. Sylvan Raid | Functional greybox | Seven-node Realm graph, fog states, Wolves, Ent, Root Trap, Heart Tree and raid result. |
| 4. Keeper Defense | Functional greybox | AI invader route, manual Root Trap, possessable Guardian Ent and 30-second energy pool. |
| 5. Infernal Realm | Functional greybox | Brute, Hellhounds, Flame Trap, Lava Gate and Infernal Heart defense. |
| Prototype Hub | Functional | Routes between the available scenes and stores the selected Realm locally. |

## Verification baseline

- Previous verified baseline: EditMode 17/17 and PlayMode 9/9 on 2026-09-04.
- Final Unity Test Runner rerun completed on 2026-09-04 15:05 EEST: EditMode 24/24 and PlayMode 10/10 passed.
- The current PlayMode suite includes smoke coverage for PrototypeHub, RealmBuild, SylvanRealm, DefenderTest and InfernalRealm.

## Known limitations

- The Realm layout and content are generated at runtime from code rather than authored prefabs and persistent ScriptableObject assets.
- The BUILD step is a compact five-slot runtime greybox; full device usability and performance remain unvalidated.
- Combat has no production animation, audio, VFX, hit pause, hit reaction, telegraphs or tuned dodge.
- Fog of war is a basic graph-driven show/hide implementation.
- AI uses direct steering instead of navigation/pathfinding.
- Realm-specific layout, colors, statistics, names and HUD wiring remain in their bootstraps; shared material, ability, entity, camera, light and EventSystem construction is centralized in a small core helper.
- Device controls and performance have not yet been validated on a mid-range Android phone.
- Adaptive portrait/landscape layout plus selectable Contextual, Fingertap and Joystick control styles are implemented, but device/simulator rotation and focus-loss checks remain outstanding.
- Camera framing/state continuity review remains code-level only; device rotation, camera framing and continuity have not been manually verified.
- Android Studio and Xcode export checks are documented in `Docs/PLATFORM_BUILDS.md`; a physical-device performance/usability pass remains outstanding.

## Directory guide

```text
Assets/Game/
  Editor/             Project setup and platform export tools
  Scenes/             Hub, sandbox, raid and defense entry scenes
  Scripts/
    AI/               Creature and invader controllers
    Camera/           Keeper, Hero and possessed camera modes
    Characters/       Shared combat entity and definitions
    Combat/           Stats, health, damage and abilities
    Controllers/      Swappable player/controller contract
    Core/             Scene bootstraps and prototype persistence
    Possession/       Controller swap and possession energy
    Raid/             Raid and defense state/results
    Realm/            Realm graph, fog views and Core objective
    Traps/            Shared trap state plus race-specific traps
    UI/               Runtime prototype HUDs
  Tests/
    EditMode/         Pure logic tests
    PlayMode/         Scene and gameplay-flow tests
Docs/                 Product context, status, builds and backlog
Builds/               Generated Android Studio/Xcode projects; Git-ignored
```
