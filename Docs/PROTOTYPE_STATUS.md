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

- EditMode: 10 tests passing before the Diamond Pass 01 changes.
- PlayMode: 4 smoke tests passing before the Diamond Pass 01 changes.
- The smoke tests currently validate scene bootstrapping and object counts; deeper gameplay-flow coverage remains in the backlog.

## Known limitations

- The Realm layout and content are generated at runtime from code rather than authored prefabs and persistent ScriptableObject assets.
- There is no player-facing BUILD step yet. The central “everything you build can be played” promise is therefore not fully validated.
- Combat has no production animation, audio, VFX, hit pause, hit reaction, telegraphs or tuned dodge.
- Fog of war is a basic graph-driven show/hide implementation.
- AI uses direct steering instead of navigation/pathfinding.
- Sylvan and Infernal bootstraps still contain duplicated prototype construction code.
- Device controls and performance have not yet been validated on a mid-range Android phone.
- Android and iOS Unity platform modules must be installed before generating Android Studio or Xcode projects.

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
