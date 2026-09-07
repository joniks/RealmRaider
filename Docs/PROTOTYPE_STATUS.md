# Realm Raiders — Prototype Status

Last reviewed: 2026-09-06

## Latest verification

- Diamond Pass 08.5 defender opening beat is present: each defense gives a brief, non-interactive opening hold so possession is practical before the normal automatic invasion begins.
- Current Unity Test Runner baseline: EditMode 30/30 and PlayMode 20/20 passed on 2026-09-07.
- Physical-device validation remains pending; no device performance result is claimed here.

## Implemented milestones

| Milestone | Status | Current implementation |
| --- | --- | --- |
| 1. Character Sandbox | Functional greybox | Blood Knight and Ent share combat, movement, abilities, AI, health and death. |
| 2. Possession | Functional greybox | Keeper selection, same-entity controller swap, camera transition, readable marker/feedback, release and death handling. |
| 3. Sylvan Raid | Functional greybox | Seven-node Realm graph, fog states, Wolves, Ent, Root Trap, Heart Tree, objective compass and raid result. |
| 4. Keeper Defense | Functional greybox | Brief opening hold, AI invader route, manual Root Trap, possessable Guardian Ent and 30-second energy pool. |
| 5. Infernal Realm | Functional greybox | Brute, Hellhounds, Flame Trap, Lava Gate and Infernal Heart defense. |
| Prototype Hub | Functional | Stores realm, orientation and control choices; presents the Sylvan build → defend → raid route while retaining prototype-scene access. |

## Verification baseline

- Current verified baseline: EditMode 30/30 and PlayMode 20/20 on 2026-09-07.
- The current PlayMode suite includes smoke coverage for PrototypeHub, RealmBuild, SylvanRealm, DefenderTest and InfernalRealm, possession, UI presentation, visual motion and camera awareness.

## Known limitations

- The Realm layout and content are generated at runtime from code rather than authored prefabs and persistent ScriptableObject assets.
- The BUILD step is a compact five-slot runtime greybox; full device usability and performance remain unvalidated.
- Combat presentation uses bounded visual motion, action telegraphs and concise HUD/audio feedback; it still has no final animation rig, production VFX or tuned dodge.
- Fog of war is a basic graph-driven show/hide implementation.
- AI uses direct steering instead of navigation/pathfinding.
- Realm-specific layout, colors, statistics, names and HUD wiring remain in their bootstraps; shared material, ability, entity, camera, light and EventSystem construction is centralized in a small core helper.
- Device controls, audio balance and performance have not yet been validated on a representative Android phone.
- Adaptive portrait/landscape layout plus selectable Contextual, Fingertap and Joystick control styles are implemented; physical-device rotation, focus-loss and layout checks remain outstanding.
- Camera framing and state-continuity behavior is covered in code/tests, but physical-device framing and rotation continuity remain unverified.
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
