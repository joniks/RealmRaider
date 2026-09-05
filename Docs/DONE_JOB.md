# Realm Raiders — Done Job

## Diamond Pass 02.5 — Adaptive orientation and classic controls

Completed on 2026-09-04 from commit `1b4a551`.

### Delivered

- Portrait, Landscape Left and Landscape Right are enabled; Portrait Upside Down is disabled.
- The Prototype Hub exposes saved `Auto`, `Portrait`, and `Landscape` orientation preferences.
- The Prototype Hub exposes saved `Contextual`, `Fingertap`, and `Joystick` control preferences.
- `Contextual` uses Fingertap in portrait and Joystick in landscape; either real movement method can also be forced in both orientations.
- A shared responsive safe-area root adapts Hub, BUILD, sandbox, raid, and defense HUDs without reloading the scene.
- Directly controlled creatures consume normalized joystick movement through a shared input abstraction.
- The virtual joystick is shown only during direct control when Joystick is the effective method; it is hidden in Hub, BUILD, unpossessed Keeper view, and terminal results.
- Joystick mode never creates a tap-to-move destination, including after tapping a distant enemy.
- Changing control style or effective orientation clears joystick movement, tap destinations, and partial gestures without resetting gameplay state.
- UI pointer ownership prevents buttons and joystick gestures from leaking into world movement or swipe combat.
- Hub buttons have non-overlapping portrait and landscape layouts.

### Verification

- EditMode: `24/24` passed in Unity Test Runner on 2026-09-04 at 15:05 EEST, reported by the project owner.
- PlayMode: `10/10` passed; Unity `TestResults.xml` records `total="10"`, `passed="10"`, `failed="0"` on 2026-09-04 at 15:06 EEST.
- The PlayMode suite includes scene smoke coverage for PrototypeHub, RealmBuild, SylvanRealm, DefenderTest, and InfernalRealm.
- `git diff --check` passes.
- Generated `Library`, `Logs`, `Temp`, `UserSettings`, and `Builds` content remains outside the Git change set.

### Remaining manual validation

- Rotate a physical Android device and the iPhone 16 Pro simulator during live play.
- Confirm notch/home-indicator safe areas, camera framing, focus-loss joystick reset, and comfort of both control styles.
- Manual device results are intentionally not claimed by this completion record.

### Scope intentionally deferred

- Root Trap multi-tap escape interaction.
- Controller/gamepad support and remapping.
- Floating/customizable joystick placement.
- Production art, VFX, audio, haptics, and combat-feel polish.
- Backend, multiplayer, economy, and inventory work.

## Diamond Pass 02.6–02.7 — Root Trap interaction and clarity

Completed on 2026-09-04; included with the next project commit.

### Delivered

- A directly controlled rooted creature can break free through five deliberate taps, with visible progress and the normal root timeout retained as fallback.
- Escape state resets on death and control changes; Root Trap collision is non-blocking.
- Defender HUD now explains out-of-range, ready-to-activate, rooted, and cooldown trap states.
- A successful Root Trap activation gives an immediate `ROOTED!` world marker and a short visual pulse, including while wolves engage the invader.
- The world marker faces the active camera and the Defender HUD guards its initialization sequence.

### Verification

- PlayMode: `10/10` passed in Unity Test Runner on 2026-09-04 at 20:58 EEST after the final feedback fixes.
- The project compiled successfully after the final fixes and `git diff --check` passes.
- Manual DefenderTest visual validation remains for the project owner.

## Diamond Pass 02.8 — Trap Camera Moment

Completed on 2026-09-04; included with the next project commit.

### Delivered

- A successful defensive trap activation in Keeper Overview creates a short, smooth focus on the trap and captured invader.
- The beat eases in, holds while `ROOTED!` is readable, then returns to the exact prior overview pose.
- It never starts for an out-of-range action, terminal result, camera transition, or possessed defender.
- The helper lives in the existing `PrototypeCameraRig`; it is reusable by a future defense trap.

### Verification

- PlayMode: `10/10` passed in Unity Test Runner on 2026-09-04 at 21:31 EEST after the camera-focus implementation.
- `git diff --check` passes.
- Manual portrait and landscape feel validation remains for the project owner.

## Diamond Pass 03 — Possession WOW Moment

Completed on 2026-09-05; included with the next project commit.

### Delivered

- Possessable creatures now have a single cleanup-safe selection presentation: a ground ring and `SELECTED — PRESS POSSESS` marker.
- The same living Ent or Brute is kept through possession: its position, health, ability state, and identity are preserved while controllers are swapped.
- Possession adds a brief colour-based takeover pulse, guarded mobile haptic, a short slow-motion beat, and a longer eased camera dive.
- Slow-motion safely restores time scale and fixed timestep on normal release, energy-depletion release, death, and object cleanup.
- Defender HUD now includes a possession-energy meter with low-energy colour warning and distinct release feedback for voluntary versus energy-depleted exit.
- Camera transition cleanup no longer leaves `PrototypeCameraRig.IsTransitioning` stuck when it cancels a prior camera routine.

### Verification

- EditMode: `24/24` passed in Unity Test Runner on 2026-09-05 at 01:04 EEST.
- PlayMode: `11/11` passed in Unity Test Runner on 2026-09-05 at 01:05 EEST, including forced-release time/control cleanup coverage.
- DefenderTest was started after clearing Console; no new errors or `NullReferenceException` occurred.
- `git diff --check` passes.
- Manual portrait and landscape possession feel validation remains for the project owner.

## Diamond Pass 04.1 — Combat Readability and Impact

Completed on 2026-09-05; included with the next project commit.

### Delivered

- Shared combat action phases now make windup, impact, recovery, and idle state explicit and prevent overlapping abilities.
- Existing melee, dash, and area abilities receive compact non-blocking telegraphs during windup.
- Connected hits now show target flash, safe micro-reaction, camera-facing damage marker, and actor impact feedback.
- Combat feedback cleans up on action completion, controller swap, possession release, death, disable, reload, and terminal flow.
- The same shared feedback works across Blood Knight, Guardian Ent, Infernal Brute, and AI-controlled creatures without changing combat data or balance.

### Verification

- EditMode: `25/25` passed after the final controller-cleanup fix.
- PlayMode: `12/12` passed in Unity Test Runner on 2026-09-05 at 01:24 EEST; `TestResults.xml` records `total="12"`, `passed="12"`, `failed="0"`.
- Manual smoke checks confirmed Sylvan direct attacks plus Defender and Infernal AI feedback, with no new Console errors.
- `git diff --check` passes.
- Full manual possession → ability → release validation remains for the project owner.

## Diamond Pass 05.1 — Modular Character Factory Foundation

Completed on 2026-09-05; include this record with the next project commit.

### Delivered

- `CharacterDefinition` now owns an explicit `CharacterVisualRecipe`; visuals are never selected from a character name.
- Recipes provide `Humanoid`, `LargeCreature`, and `Beast` families, optional head/back/arms/accent modules, faction palette data, and future prefab plus Animator Controller slots.
- `CharacterVisualAssembler` creates and removes only a visual child hierarchy. Its fallback primitives and imported-prefab slots never add blocking gameplay colliders.
- The current roster uses explicit cached recipes: Blood Knight is Humanoid; Guardian Ent and Infernal Brute are distinct LargeCreature variants; Wolves and Hellhounds are distinct Beast variants.
- Recipe instances and palette materials are cached, so scene reloads do not create a new recipe for every roster access and no visuals are created every frame.
- Combat hit feedback refreshes child renderers, so recipe-built visuals receive the existing combat flash safely.
- Missing or invalid recipes preserve the entity's original primitive visual and leave gameplay intact.

### Verification

- EditMode: `26/26` passed, `0` failed in Unity Test Runner on 2026-09-05 at 10:47 EEST.
- PlayMode: `12/12` passed, `0` failed in Unity Test Runner on 2026-09-05 at 10:46 EEST.
- Tests prove deterministic module hierarchy, fallback visual restoration, possession with one entity/controller, non-blocking module colliders, and removal of the visual child hierarchy after cleanup.
- Unity Assets Refresh completed without C# errors; Console was clean after the final PlayMode run.
- `git diff --check` passes.

### Scope intentionally deferred

- Final 3D meshes, FBX import, rigs, animation clips, Animator graphs, textures, LODs, and character-production tooling.
- Third-party asset selection and licence/provenance intake. A future pass must use a clearly licensed source and retain its licence record.

## Diamond Pass 05.2 — Licensed Blood Knight Hero Intake

Completed on 2026-09-05; include this record with the next project commit.

### Delivered

- Added the CC0-licensed Quaternius Animated Knight `KnightCharacter.fbx` and retained its local licence notice plus a project-wide provenance record.
- Created the visual-only `Resources/Characters/BloodKnightHero` prefab and explicitly binds it through `BloodKnightRecipe`; the Blood Knight no longer uses a capsule fallback when the resource is present.
- The imported model has Medium mesh compression, Read/Write disabled, and no imported colliders, cameras, lights, or animation clips.
- The assembled hero remains a child of `CharacterVisualAssembler`; it does not add or replace the root `CharacterController`, health, combat, AI, input, camera, or possession systems.
- Existing hero recipe modules are disabled so the imported knight silhouette remains readable. All other roster families remain on their modular fallback recipes.

### Verification

- EditMode: `27/27` passed, `0` failed.
- PlayMode: `13/13` passed, `0` failed on 2026-09-05 at 11:08 EEST.
- New tests verify the resource hero binding, visual hierarchy, disabled module colliders, and primitive fallback when a hero prefab is unavailable.
- Manual Sylvan Realm smoke confirms Blood Knight uses the imported knight while HUD, camera, and controls start normally.
- Console was clean after the smoke run; no new C# or runtime exceptions were recorded. `git diff --check` passes.

### Known limitation

- Unity reported an invalid Humanoid avatar mapping for this legacy FBX. It is intentionally imported as a safe Generic rig; no Animator Controller or animation playback was added. A future animation pass requires a compatible rig/model or a separately validated retargeting solution.
