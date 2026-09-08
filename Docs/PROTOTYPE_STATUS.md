# Realm Raiders — Prototype Status

Last reviewed: 2026-09-08

## Latest verification

- Diamond Pass 08.7 Build Plan Readability is present: the fixed five-slot Build screen now shows each lane, role, cost, and a live invader-to-Heart-Tree defense sequence without changing defense rules.
- Diamond Pass 08.8 Defender Route Readability is present: the defense HUD states the invader's real opening, approach, engagement, and terminal route situation without changing AI or controls.
- Diamond Pass 08.9 Combat Target Readability is present: the existing awareness system identifies a visible eligible attacker with real health and hands off cleanly to its off-screen direction cue.
- Diamond Pass 09.0 Raid Loop Closure is present: factual raid results now have a primary route back to the saved Build plan, while retry and Hub remain available.
- Diamond Pass 09.1 Realm Stores Foundation is present: real raid Gold and Rare Materials are locally recorded once per shown result and are visible as read-only stores in Hub and Build.
- Diamond Pass 09.2 Guardian Ent Cultivation is present: earned stores can buy one capped, persistent, truthful Ent vitality upgrade for the next Sylvan defense.
- Diamond Pass 09.3 Guardian Ent Growth Readability is present: the cultivated defender visibly grows and states its exact earned maximum-health bonus during Sylvan defense.
- Diamond Pass 09.4 Module Host Boundary is present: an independent, passive character-catalogue contract assembly now lets future Modules packages describe data without gaining gameplay or scene authority.
- Diamond Pass 09.5 Visual-Tuning Package Wiring is present: the reviewed local visual-tuning package is resolved through the pinned Modules submodule, while remaining unused by runtime presentation.
- Module Pass MCT 01 Starter Character Catalogue is staged in the pinned Modules submodule: it describes the five starter characters through the passive contracts boundary, but is intentionally not yet installed or consumed by runtime presentation.
- Diamond Pass 09.6 Dodge: Player Escape is present: direct-controlled characters have a narrow, collision-aware escape with explicit 0.18-second damage immunity and lifecycle-safe cleanup; AI behavior remains unchanged.
- Module Pass MVT 02 Starter Creature Visual Profiles is present in the installed tuning package: four mobile-budgeted design profiles are validated but deliberately unconsumed by runtime presentation.
- Diamond Pass 09.7 Infernal Flame Trap Identity is present: the Keeper manually ignites the invader for three separate eight-damage pulses without root or slow, while the HUD truthfully exposes the active burn and cooldown.
- Diamond Pass 09.8 Combat Camera Readability v2 is present: one eligible nearby threat now receives an unmistakable HUD-owned target plate or responsive left/right `ATTACKER`/`ATTACKING` edge tab without changing targeting, movement, combat, or bounded camera authority.
- Module Pass MCR 01 Modular Character Recipe Contracts is staged in the pinned Modules submodule: it defines a deterministic, immutable five-slot recipe boundary, but is intentionally not installed or consumed by runtime presentation.
- Diamond Pass 09.9 Realm Landmark Silhouette Blockout is present: the two realm objectives and race-specific traps now differ through lean visual-only organic versus angular primitive silhouettes while their gameplay roots, colliders and rules remain unchanged.
- Module Pass MMP 01 Character Motion Profile Contracts is staged in the pinned Modules submodule: it defines deterministic six-clip family motion metadata while remaining uninstalled and free of animation assets or runtime authority.
- Diamond Pass 10.0 Realm Route Readability Blockout is present: Sylvan raid and defense routes now use low organic bands/masses while Infernal defense uses low angular causeway plates, without changing authoritative transforms, colliders, navigation or gameplay.
- Design Pass DUX 01 First Playable Minute v1 is staged in the pinned Modules submodule: it specifies a truthful, non-modal BUILD-to-possession onboarding thread for the existing loop, but no tutorial runtime or persistence has been implemented yet.
- Diamond Pass 10.1 First Playable Minute Hub + BUILD Foundation is present: only the primary Sylvan journey activates a versioned local guide, and BUILD truthfully guides one changed valid plan with dismiss/skip and a transient defense handoff.
- Diamond Pass 10.2 First Playable Minute Possession Proof is present: the accepted BUILD handoff continues through factual Sylvan selection, same-entity possession, movement, attack, dodge, explicit release, Keeper return and the real defense result, with lifecycle-safe retry and completion.
- Design Pass DART 01 Character Production Pipeline v1 is staged in the pinned Modules submodule: it defines a deterministic three-family Blender-to-Unity production system and a Guardian Ent first-production gate without claiming unavailable art, rigs or clips.
- Research Passes ART 06A-R and ART 06B-R are staged in the pinned Modules submodule: they shortlist a conditional CC0 Guardian Ent source, KayKit Humanoid motions and a Beast motion accelerator without downloading or approving any archive.
- Design Pass DLEGAL 01 Third-Party Notices v1 is staged in the pinned Modules submodule: it specifies exact release-cleared 3DRT and Kenney notice presentation while keeping Quaternius behind a human licence decision.
- Diamond Pass 10.3 Third-Party Notices is present: the Hub now exposes an offline responsive notice panel with exact mandatory 3DRT attribution, voluntary Kenney provenance, safe fallback and no unresolved Quaternius claim.
- Design Pass DART 02 Guardian Ent Visual Identity v1 is staged in the pinned Modules submodule: `Ancient Canopy Sentinel` defines the first LargeCreature silhouette, cultivation continuity and mobile production gate.
- Design Pass DART 03 Infernal Brute Visual Identity v1 is staged in the pinned Modules submodule: `Obsidian Gatebreaker` defines a low-wide Infernal counterpart that shares the exact LargeCreature family contract without collapsing into the Ent silhouette.
- Design Pass DENV 01 Sylvan Environment Production v1 is staged in the pinned Modules submodule: `Open Grove Arches` defines the eight-type Sylvan kit, continuity rules and Android budgets without approving an asset source.
- Current Unity Test Runner baseline: EditMode 71/71 and PlayMode 45/45 passed on 2026-09-08.
- Physical-device validation remains pending; no device performance result is claimed here.

## Implemented milestones

| Milestone | Status | Current implementation |
| --- | --- | --- |
| 1. Character Sandbox | Functional greybox | Blood Knight and Ent share combat, movement, abilities, AI, health/death, ability-readiness UI and eligible-attacker identification. |
| 2. Possession | Functional greybox | Keeper selection, same-entity controller swap, camera transition, readable marker/feedback, release and death handling. |
| 3. Sylvan Raid | Functional greybox | Seven-node Realm graph, fog states, Wolves, Ent, Root Trap, Heart Tree, objective compass, ability readiness and result-to-Build loop closure. |
| 4. Keeper Defense | Functional greybox | Brief opening hold, readable live invader route state, manual Root Trap, possessable Guardian Ent with persistent three-rank vitality and a visual growth signal, plus 30-second energy pool. |
| 5. Infernal Realm | Functional greybox | Brute, Hellhounds, a manual three-pulse Flame Trap without control effects, Lava Gate and Infernal Heart defense. |
| Prototype Hub | Functional | Stores realm, orientation and control choices; presents the Sylvan build → defend → raid route and current read-only Realm Stores while retaining prototype-scene access. |
| Build Plan | Functional greybox | Fixed five-slot defense configuration with live lane/role/cost copy, one capped Ent-vitality investment using earned local stores, and a non-interactive invader-to-Heart-Tree plan. |

## Verification baseline

- Current verified baseline: EditMode 71/71 and PlayMode 45/45 on 2026-09-08.
- The current PlayMode suite includes smoke coverage for PrototypeHub, RealmBuild, SylvanRealm, DefenderTest and InfernalRealm, possession, UI presentation, visual motion and camera awareness.

## Known limitations

- The Realm layout and content are generated at runtime from code rather than authored prefabs and persistent ScriptableObject assets.
- The BUILD step is a compact five-slot runtime greybox with a live defense-plan summary; full device usability and performance remain unvalidated.
- Combat presentation uses bounded visual motion, action telegraphs, concise HUD/audio feedback, and one deliberately narrow direct-control dodge; it still has no final animation rig or production VFX.
- Fog of war is a basic graph-driven show/hide implementation.
- AI uses direct steering instead of navigation/pathfinding.
- Realm-specific layout, colors, statistics, names and HUD wiring remain in their bootstraps; shared material, ability, entity, camera, light and EventSystem construction is centralized in a small core helper.
- Device controls, audio balance and performance have not yet been validated on a representative Android phone.
- Adaptive portrait/landscape layout plus selectable Contextual, Fingertap and Joystick control styles are implemented; physical-device rotation, focus-loss and layout checks remain outstanding.
- Camera framing, threat-cue readability, safe-area layout and state-continuity behavior are covered in code/tests, but physical-device noticeability and rotation continuity remain unverified.
- Android Studio and Xcode export checks are documented in `Docs/PLATFORM_BUILDS.md`; a physical-device performance/usability pass remains outstanding.
- The visual-tuning Modules package is installed and its pure-data tests run, but no module provider/profile is discovered or integrated into runtime presentation yet.
- The modular-character-recipes package is staged in the Modules submodule but remains uninstalled; no approved module library or starter recipe instances exist yet.
- The character-motion-profiles package is staged but uninstalled; no approved shared rigs, clips, concrete motion profiles or Core presentation adapter exist yet.
- The character production pipeline is accepted, but Guardian Ent still needs an approved owned/licensed source, gameplay-envelope capture, neutral-gray LOD0 and a frozen LargeCreature rig/bind/anchor manifest before animation or Unity integration.
- Guardian Ent and Sylvan environment production directions are accepted, but remain design-only until their source, licence and human art gates pass.
- Infernal Brute production is also design-only: its Obsidian Gatebreaker direction still needs a human-approved owned or third-party source and a shared Ent/Brute LargeCreature rig compatibility proof.
- The Hub notice panel presents the release-cleared 3DRT and Kenney records; Quaternius remains excluded from its runtime catalogue until the acquisition-licence conflict receives a human decision and evidence record.
- The first-playable-minute Hub, BUILD and Sylvan defense proof are implemented through the real result; physical-device noticeability, wider onboarding and optional replay/settings remain unfinished.
- Realm landmarks and central routes now have distinct organic versus angular primitive blockouts, but boundaries, terrain, final materials/textures, environment assets and physical-device portrait review remain unfinished.

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
    Modules/          Passive host contracts for separately reviewed packages
  Tests/
    EditMode/         Pure logic tests
    PlayMode/         Scene and gameplay-flow tests
Docs/                 Product context, status, builds and backlog
Modules/              Git submodule: isolated package catalogue
Builds/               Generated Android Studio/Xcode projects; Git-ignored
```
