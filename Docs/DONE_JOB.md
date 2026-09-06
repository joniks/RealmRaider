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

## Diamond Pass 06.1 — Visual Motion Layer

Completed on 2026-09-05; include this record with the next project commit.

### Delivered

- Added `CharacterVisualMotion` and a dedicated `Presentation Pivot` below each assembled visual hierarchy.
- Recipe-built fallback creatures and the imported Blood Knight receive bounded presentation-only idle, movement, action-phase, and hit-reaction motion.
- Motion never changes the `CombatEntity` root transform, `CharacterController`, combat timing, damage, target selection, camera target, possession, or controls.
- `CombatFeedback` starts and clears only the visual hit reaction; cleanup restores the pivot's exact base position, rotation, and scale.
- Missing visual roots continue to degrade safely without affecting gameplay.

### Verification

- EditMode: `28/28` passed, `0` failed.
- PlayMode: `14/14` passed, `0` failed on 2026-09-05 at 13:49 EEST.
- Regression tests prove bounded/restored pivot transforms, unchanged host roots, action/hit integration, cleanup, and no visual colliders.
- Manual smoke: Sylvan Realm Blood Knight loaded, remained camera/root-stable through idle and a CLEAVE action; Infernal Keeper overview kept the fallback Brute, invader/trap flow, and stable camera.
- Console was clean in both manual starts and during tests. `git diff --check` passes.

### Remaining manual feel check

- The pivot sway/bob is intentionally subtle and cannot be judged from a static screenshot. Confirm comfort and readability on a physical mobile device before treating its amplitude as final.

## Diamond Pass 06.2 — Interface & Audio First Read

Completed on 2026-09-05; include this record with the next project commit.

### Delivered

- Added a small scene-local `HudPresentation` helper for the runtime-created UGUI HUDs. It skins buttons with the selected Kenney depth sprite while preserving existing realm/semantic tints and falls back safely when an imported resource is unavailable.
- Hub, Build, Raid, Defender, and Character Sandbox HUD button factories now share that presentation layer without changing layouts, labels, pointer ownership, input routing, scene navigation, or gameplay actions.
- Added one normal 2D `AudioSource` per HUD root, with no added `AudioListener`, persistent singleton, coroutine, or per-frame audio work.
- Ordinary buttons receive a restrained click; successful possession and successful trap activation receive confirmation; victory/defeat receives one guarded result cue. Failed/unavailable Trap and possession attempts never receive a confirmation cue.
- Added the selected Kenney CC0 UI/audio assets, Unity import metadata, local licence file, and project provenance record.
- Hardened the presentation helper for Unity's editor/runtime differences: it lazily initializes in EditMode, handles Unity object null semantics correctly, and creates a 9-slice runtime Sprite from the source texture when Unity does not expose a Sprite asset directly.

### Verification

- EditMode: `29/29` passed, `0` failed on 2026-09-05 at 17:43 EEST.
- PlayMode: `15/15` passed, `0` failed on 2026-09-05 at 17:45 EEST.
- Focused regression coverage verifies button-skin resource fallback, preserved button tint, one scene-local audio source, no HUD audio listener, and guarded result-cue state.
- Unity recompilation completed without C# errors. The final PlayMode run did not add an audio-listener warning or exception to the Editor log.
- `git diff --check` passes.
- Project owner manually tested the presentation pass and accepted the player-facing result.

### Scope intentionally deferred

- Full UI redesign, icon placement, Audio Mixer/settings UI, music, ambience, haptics, and additional third-party UI/audio packs.

## Diamond Pass 06.3 — Combat Camera Awareness

Completed on 2026-09-05; include this record with the next project commit.

### Delivered

- Added `CombatCameraAwareness`: a scene-local, presentation-only helper that considers a nearby threat only after an explicit player enemy tap or damage received from that threat.
- Added a smooth, bounded camera framing request through `PrototypeCameraRig`; it never selects targets, changes player movement, rotates the player, spends abilities, or changes combat data.
- Added one non-interactive, non-raycast UGUI screen-edge `THREAT` indicator for a currently eligible off-screen opponent.
- Eligibility is limited to direct control, living nearby entities, a 2.2-second relevance window, and normal non-terminal combat state. Death, despawn, distance, terminal state, controller swap, possession release, scene reload, Keeper overview, and camera transitions clear the threat and camera request.
- Trap focus, normal Snap/Transition flows, possession changes, and inactive player controllers safely release or avoid creating awareness state.

### Verification

- EditMode: `29/29` passed, `0` failed after the inactive-helper cleanup.
- PlayMode: `16/16` passed, `0` failed on 2026-09-05 at 18:54 EEST; focused coverage verifies nearby reported threat eligibility, bounded focus, out-of-range release, terminal cleanup, and indicator state.
- Manual smoke: Infernal Realm completed a Defender Victory route in portrait and then 800×480 landscape; Sylvan Realm started in landscape and then 800×480 portrait. HUD/camera stayed stable and no leftover indicator or Console errors appeared.
- The short manual Sylvan run did not produce an explicitly reported/attacking off-screen threat, so the indicator/focus moment itself is verified by focused PlayMode coverage rather than a manually witnessed Game view instance.
- Unity compilation was clean apart from pre-existing `CS0618` deprecation warnings. `git diff --check` passes.

### Scope intentionally deferred

- Hard target lock, aim assist, automatic combat, minimap/radar, camera settings, and any change to camera gameplay or balance.

## Diamond Pass 07.1 — Licensed Fantasy Warrior Blood Knight

Completed on 2026-09-06 in commit `c57d14e`.

### Delivered

- Replaced the active Blood Knight visual with the authorised 3DRT Fantasy Warrior, while preserving its visual-only `CharacterVisualAssembler` integration.
- Retained the original Quaternius presentation as `BloodKnightHero_QuaterniusFallback.prefab`; no source art or gameplay system was removed.
- Added the creator-published 3DRT archive, FBX and texture with a local provenance record and a full CC BY 4.0 register entry. The required attribution is recorded for a future player-facing third-party-notices screen.
- Configured the imported visual for mobile: published ~2.5k-triangle source, Medium mesh compression, disabled Read/Write, no colliders/cameras/lights, and a 1024px Android texture override.
- Attempted Humanoid validation; the asset remains safely Generic because Unity did not produce a human-bone mapping. Its one imported clip is retained, but no Animator Controller or gameplay animation system was introduced.
- Added EditMode coverage proving the active 3DRT binding, preserved Quaternius fallback separation, and visual-only/mobile import settings.

### Verification

- EditMode: `30/30` passed, `0` failed.
- PlayMode: `16/16` passed, `0` failed.
- Manual Sylvan smoke confirmed the 3DRT Blood Knight is visible and gameplay/HUD remains functional; Infernal fallback flow remained stable. Both runs left Console at `0` errors.
- `git diff --check` passed before commit; generated Library, Logs and Builds content remains outside Git.

### Scope intentionally deferred

- Validation and optional use of the source animation clip.
- A player-facing third-party-notices screen, further model/texture edits, LODs, retargeting, and new animation content.

## Diamond Pass 07.2 — Blood Knight Animation Proof

Completed on 2026-09-06; include this record with the next project commit.

### Delivered

- Inspected the authorised 3DRT `Take 001` source clip rather than assuming it was a usable idle.
- Confirmed the Generic clip is 33.367 seconds at 30 FPS (frames 0–1001), with Loop Time and Loop Pose disabled and no Root Motion Node.
- Previewed the sequence across its timeline; it is a long authored series of materially different poses, not a compact idle or showcase loop readable from the Sylvan camera.
- Correctly retained the existing bounded `CharacterVisualMotion` presentation layer instead of forcing an Animator, controller, root motion or unsuitable clip into gameplay.
- Added regression checks that the active 3DRT visual remains Animator-free and that the Quaternius fallback remains independent, with no controller or root-motion authority.

### Verification

- EditMode: `30/30` passed, `0` failed.
- PlayMode: `16/16` passed, `0` failed on 2026-09-06; `TestResults.xml` records 2026-09-05 21:36:18Z–21:36:21Z.
- Manual Sylvan and Infernal starts remained stable with no new Animator/camera layer or Console exception.
- `git diff --check` passes.

### Decision

No source animation was integrated. A future animation pass needs a short, deliberately selected licensed idle/combat set or a separately authorised retargeting pipeline; this pass does not pretend the existing long clip solves that need.

## Diamond Pass 08.1 — Threat Readability on Mobile

Completed on 2026-09-06; include this record with the next project commit.

### Delivered

- Extended the presentation-only combat camera awareness to recognise nearby creatures that are actively targeting the directly controlled character and are in `Chase` or `Attack`, while preserving explicit enemy-tap and damage reports.
- Added a bounded active-creature registry and intent-change signal, avoiding per-frame scene scans. Existing eligible pursuers are reconciled safely after an old threat is cleared.
- Made the off-screen indicator more legible on phones: it now says `ATTACKER`, is larger/bold, non-interactive, and anchored inside the device safe area on the correct horizontal edge.
- Strengthened the still-bounded soft camera bias so a nearby flanking attacker is perceptible without hard lock-on, movement changes, player rotation, aim assist, target selection, or combat/balance changes.
- Preserved cleanup on terminal state, controller release, possession changes, death, distance, camera transitions, and Keeper overview.

### Verification

- EditMode: `30/30` passed, `0` failed.
- PlayMode: `17/17` passed, `0` failed (final run duration: 3.177 seconds).
- The focused PlayMode coverage now verifies real `CombatCameraAwareness` indicator visibility and actual left/right edge direction from active creature intent, plus nearby focus, release, terminal cleanup, and controller cleanup.
- `git diff --check` passes.

### Remaining player feel check

- Automated coverage proves the behaviour and cleanup. Confirm on a physical phone that the stronger soft focus is noticeable but comfortable in both portrait and landscape; tune only the presentation constants if it feels too subtle or too assertive.

### Scope intentionally deferred

- Hard target lock, aim assist, automatic combat, minimap/radar, camera settings, and any gameplay or balance change.

## Diamond Pass 08.2 — First-Play Route & Hub Clarity

Completed on 2026-09-06; include this record with the next project commit.

### Delivered

- Added a visually primary `START SYLVAN JOURNEY` action in the Prototype Hub. It begins at the existing Sylvan Build scene and makes the intended loop explicit: build defences → defend the realm → raid the enemy.
- Kept Build Sylvan, Sylvan Defense, Sylvan Raid, Infernal Defense, and Character Sandbox intact as secondary prototype routes with their original scene destinations.
- Added compact explanations for orientation and control preferences: `AUTO` follows rotation, while `CONTEXTUAL` uses fingertap in portrait and joystick in landscape. The saved preferences and their behavior were not changed.
- Re-spaced the Hub hierarchy for portrait and landscape, including compact helper-label heights and a landscape-specific text placement, so labels, controls, journey callout, and route buttons remain distinct.
- Refreshed `PROTOTYPE_STATUS.md` to reflect the current mobile controls, visual/audio presentation, camera-awareness delivery, verification baseline, and still-open physical-device checks.

### Verification

- Focused Hub smoke: `1/1` passed.
- EditMode: `30/30` passed, `0` failed.
- PlayMode: `17/17` passed, `0` failed on 2026-09-06.
- The Hub test now proves the primary and legacy route destinations, journey copy, one EventSystem/GraphicRaycaster, and no label/button or label/label overlap in both portrait and landscape.
- Unity compiled without new C# errors; Console only contained pre-existing obsolete-API warnings. `git diff --check` passes.

### Remaining player feel check

- The Unity main window was unavailable after Test Runner, so no manual Hub smoke is claimed. On the next Android build, open the Hub in portrait and landscape, confirm the journey action is obvious and everything is comfortably readable, then tap it once to confirm it enters the existing Build scene.

### Scope intentionally deferred

- Persistent tutorial completion, campaign/progression logic, new scenes, settings UI, additional art/audio, and any Build/Defense/Raid gameplay change.

## Diamond Pass 08.3 — Possession Moment Readability

Completed on 2026-09-06; include this record with the next project commit.

### Delivered

- Replaced the greybox possession selection ring/label with a visual-only `PossessionSelectionPresentation`: a restrained pulsing ground marker and camera-facing selected-creature label. It adds no input ownership or active collider and does not move the entity or camera.
- Added one short, non-interactive HUD confirmation for takeover and voluntary/forced return, using the actual creature display name. Sandbox, Sylvan Defense, and Infernal Defense retain their existing possession energy, direct-control HUD, audio/haptic, slow beat, and camera transition.
- Made selection, pulse, release, death, and destruction cleanup idempotent; the per-selection material instance is explicitly released on presentation destruction.
- Corrected a discovered Hub responsive-layout regression: the Hub now owns explicit portrait and landscape action layouts rather than allowing the generic action reflow to overwrite them. Portrait uses distinct information and action columns; landscape retains a deliberate two-column layout.
- Replaced the Hub overlap test's incorrect pivot/anchor approximation with real `RectTransform` world-corner checks.

### Verification

- Focused possession-presentation test passed, including billboard orientation, non-blocking colliders, same-entity takeover/release, feedback, and no canvas/EventSystem/AudioListener artifacts.
- EditMode: `30/30` passed, `0` failed.
- PlayMode: `18/18` passed, `0` failed on 2026-09-06.
- Manual Sandbox smoke passed: selection marker, takeover confirmation, ability use, release, and cleanup were visible with no artifacts. DefenderTest reached terminal cleanup with no exceptions; its rapid AI route prevented a manual possession before terminal, so automated possession coverage remains the proof for that boundary.
- Unity compiled without C# errors or new runtime exceptions; `git diff --check` passes.

### Remaining player feel check

- On a physical device, confirm the pulse/label is readable but restrained in portrait and landscape, then repeat one full Defender possession before terminal. Tune only presentation constants if it feels too subtle or too prominent.

### Scope intentionally deferred

- New possession rules, camera effects, persistent tutorial/progression state, assets/packages, animation work, combat/trap/energy balance changes, and a general UI redesign.

## Diamond Pass 08.4 — Raid Objective Compass

Completed on 2026-09-06; include this record with the next project commit.

### Delivered

- Connected the existing Sylvan Heart Tree `RealmCore` and raid camera directly to the existing `RaidHUD`; no scene scan, second canvas, EventSystem, or AudioListener was added.
- Added one safe-area, non-interactive `HEART TREE` edge cue that appears only when the active objective is outside the camera view. It maps real camera projection to the correct left/right edge in portrait and landscape.
- Kept the objective cue distinct from the orange attacker warning: it is a calm goal signal only and never requests camera focus, selects a target, moves/rotates the hero, alters fog, or changes objective/combat timing.
- The cue hides when the Heart Tree is visible, objective capture has begun, a terminal result is active, the hero is dead, references are missing, or the scene is being torn down.

### Verification

- Focused objective-compass PlayMode test: `1/1` passed, covering real camera projection left/right, on-screen hide, capture hide, terminal cleanup, and UI-input infrastructure invariants.
- EditMode: `30/30` passed, `0` failed.
- PlayMode: `19/19` passed, `0` failed on 2026-09-06.
- Manual Sylvan smoke observed `◀ HEART TREE` safely on the left in landscape and `HEART TREE ▶` safely on the right after the Hub portrait preference was selected; action buttons remained reachable.
- Unity Console/Editor log had no new C# or runtime exceptions. `git diff --check` passes.

### Remaining player feel check

- Unity Game View remained Free Aspect during the manual check, so a true physical portrait viewport still needs confirmation on Android. Verify that the cue is calm, readable, and visually distinct from `ATTACKER` during one physical-device raid.

### Scope intentionally deferred

- Minimap/radar, waypoint path, automatic movement, hard target lock, camera focus, fog changes, gameplay balance changes, persistent settings, new art/assets/packages, or a UI redesign.
