# Realm Raiders — Done Job

## Diamond Pass 15.10 — Blood Knight Motion Plane + Natural Takeoff

Completed on 2026-09-10; included with the next project commit.

### Delivered

- Added an immutable closed local-axis contract to the procedural motion package;
  invalid axes fail safely to the exact legacy local-X behavior.
- Kept `CompatibilityDefault` source- and behavior-compatible while changing only
  `BloodKnightDeviceReadable` locomotion to the evidence-driven local-Z plane.
- Blood Knight arms and legs now receive opposite forward/back stride values and
  the arms retain counter-phase with the legs.
- The factual takeoff window now uses one bounded asymmetric visual pose: a small
  knee bend plus distinct planted/pushing and free-leg thigh/calf rotations.
- No pre-jump delay, position/scale/root write, CharacterController, pivot, input,
  camera, combat, Animator, root motion or physics authority was added.

### Verification

- Module static checks and `git diff --check` passed; only three procedural-motion
  package files changed in Modules commit `6769716`.
- Final EditMode: `250/250` passed, zero failed/skipped/inconclusive, in 0.921 s.
- Final PlayMode: `92/92` passed, zero failed/skipped/inconclusive, in 71.982 s.
- Post-gate Editor log tail contained no new error or exception.
- QA loaded Sylvan landscape with the exact Knight and mobile UI, but could not
  safely inject Game-view inputs; physical-device stride/takeoff quality remains
  unclaimed and is the next user smoke.

### Scope intentionally deferred

- Actual anticipation delay, spine/pivot/root movement, Animator/root motion, IK,
  ragdoll physics, broad skeleton support and further tuning without device evidence.

## Diamond Pass 15.9 — Blood Knight Device Motion Tune

Completed on 2026-09-10; included with the next project commit.

### Delivered

- Converted the procedural driver's hard-coded strengths into immutable bounded
  tuning data. `CompatibilityDefault` preserves the 15.8 behavior, while the named
  `BloodKnightDeviceReadable` preset strengthens phone-visible locomotion, attack,
  jump, hit and death silhouettes without exceeding 30 degrees.
- Added a neutral-by-default Base Body local fit to the shared visual recipe and
  assembler. Only the 3DRT Blood Knight receives the Android-evidence-driven
  180-degree yaw correction, so its visual forward matches the authoritative
  character direction without rotating gameplay.
- The Blood-Knight-only adapter selects the new preset but still maps only factual
  displacement, action, jump, damage and death state to the same six exact bones.
- Gameplay root, CharacterController, Presentation Pivot, camera, input, combat
  timing, possession identity and visual-collider rules remain unchanged.
- Modules commits `0cec1d4` and `45bdf67` also add the accepted motion profile and
  a reusable offline, fail-closed character archive inventory helper.

### Verification

- QA Refresh first exposed and Core removed an invalid direct Module dependency
  from the main test assemblies; no asmdef broadening was retained.
- Final EditMode: `248/248` passed, zero failed/skipped/inconclusive, in 0.921 s.
- Final PlayMode: `92/92` passed, zero failed/skipped/inconclusive, in 71.782 s.
- Post-gate Editor log tail had no new errors or exceptions; `git diff --check` is
  clean.
- QA could not safely enter/control Game view through its automation surface, so
  corrected facing and stronger motion readability remain for the next Android
  export and user smoke; no manual result is claimed.

### Scope intentionally deferred

- Animator/root motion, Humanoid retarget, IK, ragdoll/Rigidbody physics, broad
  skeleton support, gameplay movement/combat changes and speculative axis tuning.
- Guardian Ent acquisition/import remains a separate exact-source gate.

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

## Diamond Pass 08.5 — Defender Opening Beat

Completed on 2026-09-07; include this record with the next project commit.

### Delivered

- Added a three-second automatic opening hold to `RaidInvaderBrain`. During it, the invader remains still and cannot target/attack defenders, advance waypoints, activate traps, or reach the core; it then releases once into the existing unchanged invasion AI.
- Added a compact, non-interactive `INVASION INCOMING — SELECT AND POSSESS` Defender HUD cue with a cached per-second countdown. It is shown only during the opening hold and never intercepts touch.
- Applied the same pacing to Sylvan and Infernal defense while preserving their existing defender names, possession energy, traps, controls, camera, and terminal result flows.
- Made the preparation cue one-shot per defense run: beginning possession dismisses it even if the player releases during the remaining opening hold.

### Verification

- Focused opening-beat PlayMode test proves hold immobility/no target, automatic route resumption, possession and terminal cleanup, and Canvas/EventSystem/AudioListener/raycast invariants.
- EditMode: `30/30` passed, `0` failed.
- PlayMode: `20/20` passed, `0` failed on 2026-09-07.
- Manual Sylvan and Infernal smoke confirmed: preparation cue → normal live invasion after about three seconds → Defender Victory, with no Console C# or runtime exceptions.
- `git diff --check` passes.

### Remaining player feel check

- On Android, confirm three seconds feels like a dramatic invitation rather than a wait. The player must comfortably select and possess during that window, while returning players should not feel held up.

### Scope intentionally deferred

- Manual start/pause controls, a tutorial state machine, post-opener AI/combat/trap/core/energy-balance changes, pathfinding, camera framing changes, persistent settings, scenes/assets/packages, or a UI redesign.

## Diamond Pass 08.6 — Mobile Ability Readiness

Completed on 2026-09-07; include this record with the next project commit.

### Delivered

- Added authoritative read-only `CooldownRemaining` to the existing `AbilityRuntime`; UI does not keep a duplicate cooldown clock or alter combat behavior.
- Added shared presentation-only `AbilityButtonReadiness` for existing Raid, Defender, and Character Sandbox ability buttons.
- Buttons now show their normal action name while ready, `NAME  1.2s` while cooling down, or `NAME — ACTING` while another combat action is resolving. Unavailable buttons are visibly disabled and never intercept additional action input.
- Ability UI immediately follows the directly controlled entity and clears safely on possession/release, death, terminal state, missing ability, and scene transition.
- Cooldown text is rebuilt only when its displayed tenths value changes; ordinary ready-state refreshes create no per-frame countdown text.

### Verification

- Focused PlayMode coverage proves authoritative ready → acting → cooldown → ready transitions, direct-control cleanup, possession/release binding, and Canvas/EventSystem/AudioListener invariants.
- EditMode: `30/30` passed, `0` failed.
- PlayMode: `22/22` passed, `0` failed on 2026-09-07.
- Manual Sylvan Raid and Defender smoke confirmed readable existing ability rows with no Console exceptions; deterministic tests cover action/cooldown and possession/release transitions.
- Unity compiled cleanly and `git diff --check` passes.

### Remaining player feel check

- On Android, use a long-cooldown ability in portrait and landscape. Confirm the countdown is readable, the temporary disabled color is clear, and the action row remains comfortable beside joystick/touch controls.

### Scope intentionally deferred

- Combat balance/timing changes, combos or input buffering, aim assist, new abilities, haptics/audio/VFX, camera work, a new HUD/canvas, persistent settings, scenes/assets/packages, or a UI redesign.

## Diamond Pass 08.7 — Build Plan Readability

Completed on 2026-09-07; include this record with the next project commit.

### Delivered

- Kept the five existing fixed Build slots and their cycle/save/defend flow, but made each choice self-explanatory: lane, selected defender or trap, tactical role, and the existing Threat cost are now visible together.
- Added one live, non-interactive `DEFENSE PLAN — INVADER → HEART TREE` summary that follows the actual route: Root Gate, Outer Guard, Mid Guard, Inner Root, Heart Guard, then Heart Tree.
- The plan updates directly from the current `DefenseLayout`, including the `ENT [POSSESSABLE]` designation when an Ent occupies a slot; it never stores a duplicate layout or combat rule.
- Reused the existing responsive safe-area layout. Portrait places the plan in the deliberate space before Save & Defend; landscape places explanatory copy opposite the fixed slot stack.
- No JSON/save format, Threat budget, allowed-slot rule, slot cycle order, Defender handoff, spawn positions, scene navigation, pointer ownership, or input behavior changed.

### Verification

- Focused Build PlayMode smoke: `1/1` passed.
- EditMode: `31/31` passed, `0` failed on 2026-09-07.
- PlayMode: `23/23` passed, `0` failed on 2026-09-07.
- Coverage proves default and edited plan copy, roles/costs, possessed-Ent designation, portrait/landscape layout separation, non-interactive plan UI, invalid-layout Save disablement, and existing save-to-Defender handoff.
- Unity compiled with no new C# errors or runtime exceptions; `git diff --check` passes.

### Remaining player feel check

- On Android, open Build in portrait and landscape, cycle at least one slot, and confirm the plan is readable at a glance without crowding the fixed controls. Automated layout checks cover the geometry; this manual check is about comfort and first-read clarity.

### Scope intentionally deferred

- Free placement, new slot types, 3D Build previews, drag-and-drop, new defenses, balance changes, a separate planning scene, save/progression changes, or a wider UI redesign.

## Diamond Pass 08.8 — Defender Route Readability

Completed on 2026-09-07; include this record with the next project commit.

### Delivered

- Added one compact, non-interactive Defender HUD route status using the real `RaidInvaderBrain` opening, current target, and next waypoint state.
- The status reports the actual tactical situation: the invader holding at the opening, advancing toward the next configured defense landmark, actively engaging a living defender, or nothing after terminal cleanup.
- Sylvan wording follows the physical route honestly: Root Gate, Guard Line, Inner Root and Heart Guard, then Heart Tree. Infernal uses its matching Flame Trap Line, Hound Line/Lava Gate, Brute Guard, and Infernal Heart sequence.
- The status considers `WaypointIndex` as the next target waypoint. It uses no distance guesswork, scene scan, new timer, or AI-state duplicate, and cached visible state avoids rebuilding text every frame.
- Existing route speed/positions, attack radius, opening hold, targeting, trap behavior, possession, camera, controls, result routing, and the remaining Defender HUD presentation remain unchanged.

### Verification

- Focused Defender opening/route test: `1/1` passed on 2026-09-07.
- EditMode: `31/31` passed, `0` failed on 2026-09-07.
- PlayMode: `23/23` passed, `0` failed on 2026-09-07.
- Coverage proves opening text, real waypoint transition, living-defender engagement, terminal cleanup, non-raycast UI, no extra Canvas/EventSystem/AudioListener, and portrait/landscape non-overlap with existing Defender controls.
- Unity Console had no new C# errors, `NullReferenceException`, or runtime exception; `git diff --check` passes.

### Remaining player feel check

- Unity Game View could not be started by the UI automation in this session, so no manual smoke is claimed. On Android, start Sylvan Defense and verify that the route line remains calm and useful through opening, approach, engagement, and result; briefly open Infernal Defense to check its realm-specific names.

### Scope intentionally deferred

- A minimap/radar, world markers, automatic camera focus, target lock, AI/pathfinding/balance changes, additional defenders/traps, tutorial/progression, new scenes/assets/packages, or a general HUD redesign.

## Diamond Pass 08.9 — Combat Target Readability

Completed on 2026-09-07; include this record with the next project commit.

### Delivered

- Completed the existing combat-awareness presentation pair without changing its threat rules: a visible eligible attacker now receives a compact `ATTACKER  NAME  current/max HP` plate, while an off-screen attacker retains the existing directional `ATTACKER` arrow.
- Reused the existing eligible threat, relevance lifetime, camera awareness, and scene-local Canvas. No new target-selection system, Canvas, EventSystem, AudioListener, scene scan, or health authority was added.
- The plate uses the real display name and health, follows camera projection inside the device safe area, and is non-interactive. It hides before the off-screen arrow appears, so the two signals never compete.
- Plate text is cached and rebuilds only when the target or its displayed health changes. Threat replacement, death, range/intent expiry, terminal state, controller release, disable, and scene cleanup clear it through the existing awareness lifecycle.
- Camera bias, AI, damage/timing, movement, possession, objective compass, HUDs, and mobile controls remain unchanged; this is not lock-on, aim assist, or automatic combat.

### Verification

- Focused combat-awareness PlayMode test: `1/1` passed on 2026-09-07.
- EditMode: `32/32` passed, `0` failed on 2026-09-07.
- PlayMode: `23/23` passed, `0` failed on 2026-09-07.
- Coverage proves visible/off-screen mutual exclusion, real name/health update, safe-area anchors in portrait and landscape, terminal/intent/controller cleanup, non-raycast presentation, and no extra Canvas/EventSystem/AudioListener.
- Unity compiled without C# errors or new runtime exceptions; `git diff --check` passes.

### Remaining player feel check

- Unity Game View could not be started by the UI automation, so no manual smoke is claimed. On Android, bring a nearby attacker into view, damage it once, let it leave the frame, and confirm the quiet plate → directional arrow handoff is informative but not distracting in both orientations.

### Scope intentionally deferred

- Hard target lock, reticle/aim assist, auto-attack, new combat stats or health mechanics, minimap/radar, new camera behavior, VFX/audio/haptics, assets/packages/scenes, or a wider UI redesign.

## Diamond Pass 09.0 — Raid Loop Closure

Completed on 2026-09-07; include this record with the next project commit.

### Delivered

- Raid Result now presents the actual victory or defeat outcome in player language while retaining every real result value: Gold, Rare Materials, enemies defeated, rooms discovered, duration, and core result.
- Added the primary `PLAN NEXT DEFENSE` result action. It loads the existing `RealmBuild` scene for both victory and defeat, completing the prototype's Build → Defense → Raid → Build loop without introducing a progression or economy system.
- Retained `RAID AGAIN` and `MY REALM`; Character Sandbox remains accessible through the Hub instead of occupying a primary post-raid slot.
- Gave the result panel its own deliberate action lane: a vertical, compact portrait stack and a landscape side lane beside the result copy. The normal live-combat HUD reflow leaves these terminal actions intact.
- Raid state/timing, reward calculation, capture/combat/fog/camera/control behavior, existing result audio, scene IDs, and saved Build layout behavior are unchanged.

### Verification

- Focused Raid Result PlayMode test: `1/1` passed on 2026-09-07.
- EditMode: `33/33` passed, `0` failed on 2026-09-07.
- PlayMode: `24/24` passed, `0` failed on 2026-09-07.
- Coverage proves factual victory/defeat copy, all three result routes, `PLAN NEXT DEFENSE` loading the real Build HUD, normal pointer ownership, one Canvas/EventSystem/AudioListener, and portrait/landscape action containment with no overlap.
- Manual Play check opened RealmBuild and confirmed the Build HUD after the result-loop implementation. Full manual raid-result playthrough remains covered by the deterministic PlayMode scenario rather than claimed as observed. Console had no new C# errors, `NullReferenceException`, or runtime exception; `git diff --check` passes.

### Scope intentionally deferred

- Persistent rewards/currencies, upgrades, progression, achievements, analytics, tutorial state, reward-number changes, new scenes/assets/packages, or a wider result-screen redesign.

## Diamond Pass 09.1 — Realm Stores Foundation

Completed on 2026-09-07; include this record with the next project commit.

### Delivered

- Added a compact, versioned local `RealmProgress` JSON record using the established PlayerPrefs persistence style. It stores only Gold, Rare Materials, completed raids, and victories; invalid or malformed input safely returns an empty record.
- Raid HUD secures the exact Gold and Rare Materials from a shown `RaidResult` once. The same RaidManager and HUD both guard duplicate result callbacks, while the existing raid reward calculation remains authoritative and unchanged.
- Hub and Build now show the non-interactive `REALM STORES  •  GOLD  •  RARE MATERIALS` line without new Canvas, EventSystem, AudioListener, scene, spending or upgrade behavior.
- Portrait Build places the stores line in the authored gap between the live defense plan and Save & Defend. Landscape keeps it in the left information column. Existing responsive safe-area layout and pointer ownership are reused.

### Verification

- Focused Raid/stores PlayMode class: `13/13` passed on 2026-09-07.
- EditMode: `35/35` passed, `0` failed on 2026-09-07.
- PlayMode: `24/24` passed, `0` failed on 2026-09-07.
- Coverage proves real victory/defeat accounting, malformed-data fallback, test reset path, duplicate-result idempotency, persisted Build-to-Hub store copy, non-raycast stores, infrastructure invariants, and portrait/landscape label/button separation.
- Manual Unity Build check confirmed the readable portrait stores line and found the initial overlap before it was corrected. Full Raid Result → Build → Hub persistence flow is covered by the deterministic PlayMode scenario; no physical-device result is claimed. Console had no C# errors or runtime exceptions, and `git diff --check` passes.

### Scope intentionally deferred

- Spending, upgrades, unlocks, new currencies, balancing, server/cloud sync, accounts, analytics, reset/settings UI, new scenes/assets/packages, or a general UI redesign.

## Diamond Pass 09.2 — Guardian Ent Cultivation

Completed on 2026-09-07; include this record with the next project commit.

### Delivered

- Added the first honest use for locally earned Realm Stores: Guardian Ent Cultivation has three persistent ranks, each costing exactly `100 GOLD` and `1 RARE MATERIAL`.
- The Build screen now presents a compact, responsive cultivation action with current active bonus, next-rank total, precise cost or missing resources, and a clear fully-cultivated state at rank three.
- Each rank gives only the Guardian Ent in the next Sylvan defense `+10%` of its original maximum health. Rank one therefore changes its real maximum from `340` to `374`; rank three reaches `+30%`.
- Purchases are saved immediately, cannot double-spend while unavailable/capped, and keep the existing five-slot defense plan, threat budget, combat stats, abilities, controls, camera, AI, and Infernal roster unchanged.
- The change is deliberately one prototype investment, not a generic upgrade tree or economy expansion.

### Verification

- Focused Guardian Ent PlayMode coverage: `9/9` passed, including purchase/persistence, truthful UI copy, missing-resource state, portrait/landscape containment, one-time Defender application, and Infernal Brute non-regression.
- EditMode: `37/37` passed, `0` failed.
- PlayMode: `25/25` passed, `0` failed on 2026-09-07 at 22:33 EEST.
- Reviewer / QA independently accepted the frozen diff after finding and verifying fixes for the initially misleading rank copy and insufficient-resource presentation.
- `git diff --check` passes. Unity Console had no new errors.

### Remaining player validation

- On Android, complete one raid or use a future developer-safe grant path, buy rank one, enter Sylvan Defense, and confirm the Ent's `374 / 340` health difference feels understandable through the existing HUD. Rotate once while reviewing the Build card.
- The live successful-purchase smoke was not claimed because this prototype has no player-facing resource grant shortcut; the deterministic PlayMode flow covers it.

### Scope intentionally deferred

- Additional upgrades, new currencies, reset/respec, unlock trees, timers, cloud sync, account data, economy balancing, new units or traps, rewards changes, new assets/packages/scenes, or a broader Build UI redesign.

## Diamond Pass 09.3 — Guardian Ent Growth Readability

Completed on 2026-09-07; include this record with the next project commit.

### Delivered

- Made Guardian Ent cultivation tangible in the actual Sylvan defense: rank zero has no growth mark, while ranks one through three receive a restrained one-, two-, or three-tier leaf-crown presentation below the existing `Presentation Pivot`.
- Added one non-interactive Defender HUD status line that truthfully displays `UNTENDED` or the current rank's exact `+10%`, `+20%`, or `+30%` maximum-health bonus.
- The presentation is visual-only: it adds no enabled collider, rigidbody, input target, Canvas, EventSystem, AudioListener, root movement, combat authority, or saved rank duplicate.
- Only the built Sylvan Guardian Ent receives the presentation and status. Wolves, Blood Knight, Infernal Brute/Hounds, Sandbox, raids, abilities, camera, possession, controls, health calculations, and threat budget remain unchanged.
- Reconfiguration, death, terminal result, disable, destruction, release, and missing-visual paths clean up the owned marker safely; no empty marker roots remain in EditMode teardown.

### Verification

- Focused Guardian Ent EditMode: `7/7` passed.
- Focused Sylvan Realm PlayMode: `9/9` passed.
- Final EditMode: `42/42` passed, `0` failed.
- Final PlayMode: `25/25` passed, `0` failed.
- QA reviewed the frozen diff, found and verified lifecycle cleanup fixes before acceptance. `git diff --check` passes.

### Remaining player validation

- On Android, buy rank one, enter Sylvan Defense, and confirm the subtle Ent growth mark and the right-side vitality line are readable but do not steal attention from possession or combat in portrait and landscape.
- No manual smoke is claimed by the team for this pass.

### Scope intentionally deferred

- New upgrades/currencies, stat changes, combat effects, final VFX/shaders/textures/packages, animation/rig work, world UI markers, a new HUD canvas, generic skill trees, new units/traps, or a Build redesign.

## Diamond Pass 09.4 — Module Host Boundary

Completed on 2026-09-08.

### Delivered

- Added the independent `RealmRaiders.ModuleContracts` assembly under `Assets/Game/Scripts/Modules/Contracts/`. It uses no Unity-engine or game-runtime reference.
- Defined the first stable package-facing character-catalogue API: body family, a plain entry with stable ID/display name/body family/visual-profile key, and an explicit catalogue provider with module ID.
- Kept the boundary deliberately passive: there is no package manifest edit, package loading, reflection, automatic registry, scene/object lookup, gameplay type, save, bootstrap, UI, asset, or player-visible change.
- Added a dedicated EditMode test assembly which references only the contracts assembly and Unity test infrastructure. The existing shared EditMode assembly remains independent of the new boundary.
- Added `Docs/MODULE_API.md`, documenting one-way dependency rules, explicit later Core integration, and the rule that visual material mapping remains a Core-owned adapter beside `CharacterVisualAssembler`.
- Added `/.worktrees/` to Git ignore rules so isolated team worktrees cannot accidentally enter the main project commit.

### Verification

- Focused `CharacterCatalogueProvider_ExposesPlainContractEntries`: `1/1` passed.
- Final EditMode: `43/43` passed, `0` failed.
- Final PlayMode: `25/25` passed, `0` failed.
- QA confirmed both contract and dedicated test assemblies have no engine or `RealmRaiders.Runtime` reference. Manual smoke was not required because the change has no player-visible behavior.
- `git diff --check` passes.

### Scope intentionally deferred

- Installing a package, changing `Packages/manifest.json`, automatic discovery, a runtime registry/adapter, moving existing gameplay or visual types, third-party assets, new characters, scene changes, and any player-visible feature.

## Diamond Pass 09.5 — Visual-Tuning Package Wiring

Completed on 2026-09-08.

### Delivered

- Added the reviewed `com.realmraiders.character-visual-tuning` package through an explicit local `file:` dependency pointing into the pinned Modules submodule.
- Registered that package as testable, so its two pure-data Editor tests run with the project. Unity generated only the expected depth-zero local `packages-lock.json` entry; no registry or network dependency was introduced.
- Kept the package passive: no profile is loaded or applied, and no Blood Knight prefab, material, transform, scene, gameplay, UI, model, rig, asset, or third-party provenance record changed.

### Verification

- Focused package tests: `VisualTuningDescriptorTests` included and passed in EditMode.
- Final EditMode: `45/45` passed, `0` failed.
- Final PlayMode: `25/25` passed, `0` failed.
- QA confirmed local resolution, the expected local lock entry, unchanged runtime presentation, and a clean `git diff --check`. Manual smoke was not required because the package remains unconsumed by gameplay.

### Scope intentionally deferred

- Loading/applying a visual profile, material-role adapter, tint/scale/pose/shader changes, model or texture import, animation/rig work, runtime module discovery, third-party downloads, new gameplay/UI/scene/save behavior, or any visual adjustment without a deliberate Game View/device review.

## Module Pass MCT 01 — Starter Character Catalogue

Completed on 2026-09-08.

### Delivered

- Added the isolated `com.realmraiders.starter-character-catalog` Modules package with one explicit, passive catalogue provider.
- Its five ordered entries describe the current starter roster only: Blood Knight, Guardian Ent, Sylvan Wolf, Infernal Brute, and Hellhound. Each uses a stable lowercase ID, a shared body family, and a visual-profile key.
- The package depends only on the existing plain `RealmRaiders.ModuleContracts` boundary. It has no Unity-engine reference, gameplay authority, automatic registration, prefab, asset, save, or scene access.
- The package remains staged in the Modules submodule but is deliberately not installed or consumed by the main Unity runtime. A future Core task must choose the first player-visible use and explicitly adapt it.

### Verification

- Reviewer / QA accepted the static package review: exact roster data, assembly isolation, data-only tests, factual documentation, and no copied art/licence record or runtime integration.
- The test assertion was checked for Unity NUnit compatibility before review; Unity was intentionally not opened because the package is not installed in the project.
- `git diff --check` passes for the frozen module candidate.

### Scope intentionally deferred

- Package installation, runtime discovery/registry, character instantiation, visual-profile application, changes to combat/AI/save/UI/scenes, and any model, texture, rig, or licence intake.

## Diamond Pass 09.6 — Dodge: Player Escape

Completed on 2026-09-08.

### Delivered

- Added one direct-control-only `DODGE` action for the Blood Knight during a raid and a possessed defender during defense. It remains unavailable to AI creatures, Keeper view, rooted/dead/terminal states, and while another combat action resolves.
- Dodge uses the most recent meaningful direct movement or tap-to-move direction, with facing as its fallback. It uses the same living entity and `CharacterController`: no respawn, physics body, target change, or ownership swap occurs.
- The move is deliberately narrow and truthful: up to `2.6` units over `0.18s`, with damage immunity only for that same `0.18s` window and a `1.5s` cooldown. It cannot overlap an ability, and root/death/controller release/terminal/disable teardown clean it up.
- Raid and Defender HUDs own a compact non-raycast Dodge control with ready, cooling-down, rooted, busy, and dodging states. Existing ability, release, trap, root-escape, orientation, and pointer-ownership behavior remains intact.

### Verification

- Focused `DodgeFlowTests`: `3/3` passed after scene-isolation cleanup.
- Focused possession regression: `CombatCameraAwareness_TracksActiveCreatureIntentAcrossEdgesAndCleansUp` `1/1` passed.
- Final EditMode: `46/46` passed, `0` failed.
- Final PlayMode: `28/28` passed, `0` failed.
- QA caught that the first test candidate left Sylvan/Defender scenes loaded, which created duplicate AudioListeners for later possession tests. Core corrected only the test teardown; QA then reran the focused checks and final suites. `git diff --check` passes.
- A manual Unity smoke was not run; no physical-device result is claimed.

### Scope intentionally deferred

- Dodge upgrades, stamina, input buffers, AI dodges, i-frame stacking, aim assist, lock-on, haptics, new attacks, new scenes/assets/packages, or changes to combat damage/AI balance.

## Module Pass MVT 02 — Starter Creature Visual Profiles

Completed on 2026-09-08.

### Delivered

- Extended the installed pure-data visual-tuning package with four explicit profiles matching the passive starter catalogue: Guardian Ent, Sylvan Wolf, Infernal Brute, and Hellhound.
- Each profile has a stable key, no-op presentation transform, distinct material-direction roles, and intentionally lean Android ceilings: one material and texture per creature; `4,000` triangles for Ent/Brute and `1,800` for Wolf/Hellhound.
- The profiles provide art direction only. They contain no textures, models, prefabs, material mapping, discovery, registration, scene lookup, or runtime mutation.
- Added a separate Modules research brief that shortlists three direct-source CC0 environment candidates for later user choice. It explicitly records `NOT DOWNLOADED / NOT APPROVED`; no asset or licence record was added to the game.

### Verification

- QA accepted static package isolation and source provenance boundaries before integration.
- Focused `StarterCreatureVisualProfilesTests`: `2/2` passed; the existing visual-tuning tests remained present in the package assembly.
- Final EditMode: `48/48` passed, `0` failed. Final PlayMode: `28/28` passed, `0` failed.
- Unity retained the local pinned package resolver and Console reported `0` warnings and `0` errors. `git diff --check` passes.

### Scope intentionally deferred

- Runtime profile lookup/application, material-role adapter work, model/texture intake, new art licences, visual changes, assets, scenes, gameplay, UI, save behavior, or automated module discovery.

## Diamond Pass 09.7 — Infernal Flame Trap Identity

Completed on 2026-09-08.

### Delivered

- Replaced the Infernal Flame Trap's former one-hit root with a manual, non-controlling burn: `8` damage immediately, then `8` after `0.35s` and `8` after `0.70s`, for `24` total before armor or immunity.
- The existing six-second cooldown prevents overlapping burns. Target death, trap disable, reinitialization, and teardown cancel remaining pulses cleanly.
- Dodge immunity applies independently to each pulse, so only a pulse inside the existing `0.18s` dodge window is ignored. The Sylvan Root Trap remains unchanged.
- Defender HUD copy truthfully reports `IGNITED` and the remaining burn pulses before returning to the normal Flame Trap cooldown. Its informational status text does not capture pointers.

### Verification

- Focused `FlameTrapFlowTests`: `5/5` passed.
- Final EditMode: `48/48` passed, `0` failed.
- Final PlayMode: `33/33` passed, `0` failed.
- QA verified the real Infernal scene's manual trap setting, damage timing, no-root behavior, dodge interaction, lifecycle cleanup, cooldown, HUD state, pointer ownership, and portrait/landscape containment. Console reported `0` warnings and `0` errors; `git diff --check` passes.
- No manual smoke was run; direct-control and possession terminal behavior on a physical device remains user-owned validation.

### Scope intentionally deferred

- Generic status-effect framework, stacking burns, new VFX/audio/haptics, balance progression, AI activation, trap upgrades, new assets, or changes to the Sylvan Root Trap.

## Diamond Pass 09.8 — Combat Camera Readability v2

Completed on 2026-09-08.

### Delivered

- Replaced the camera-child threat Canvas with one visual presentation under each scene's existing gameplay HUD and `ResponsiveHudRoot`. Prototype, raid, Sylvan defense and Infernal defense use explicit scene-local binding without adding discovery, EventSystems, AudioListeners, or input authority.
- A visible eligible attacker receives one factual name/health plate; an off-screen or behind-camera attacker receives one charcoal-backed directional tab. The tab distinguishes `ATTACKER` from `ATTACKING` through words, arrows and secondary color.
- Added `6%` leave / `9%` return hysteresis and bounded `0.20s` one-shot arrival/urgency pulses. The cue remains non-interactive and preserves the existing single-threat selection, `14m` eligibility, `2.2s` explicit relevance and camera-bias caps.
- Added portrait/landscape safe-area sizing, objective-cue separation and a Defender-only landscape action gap so the edge tab does not cover possession, trap, ability, Dodge or release controls.

### Verification

- Focused final-delta combat-camera set: `3/3` passed.
- Focused `SylvanRealmSmokeTests`: `9/9` passed.
- Final EditMode: `49/49` passed, `0` failed.
- Final PlayMode: `34/34` passed, `0` failed.
- QA confirmed no code changed after the final suites and `git diff --check` passes. No manual scene or physical-device smoke was run, and no Console state is claimed.

### Scope intentionally deferred

- Lock-on, target cycling, aim assist, auto-facing/combat, minimap/radar, stronger camera bias, new art/audio/haptics, settings, generic objective navigation, or physical-device noticeability acceptance.

## Module Pass MCR 01 — Modular Character Recipe Contracts

Completed on 2026-09-08.

### Delivered

- Added an isolated `com.realmraiders.modular-character-recipes` package with an immutable, closed recipe schema for the three shared body families and the fixed `base_body`, `head`, `back`, `arms`, and `accent` slot taxonomy.
- Added deterministic validation for IDs, family/slot metadata, exactly one base body, optional-slot uniqueness, and globally unique ordinal-sorted provenance source IDs.
- Added fixed-order BOM-less UTF-8 canonical serialization and a lowercase SHA-256 content hash. Invalid recipes cannot be serialized or hashed, and mutable display names, paths, timestamps, random values, and unknown root fields are excluded by the input-field policy.
- The package remains passive and uninstalled. It includes no starter recipes, Unity objects, assets, discovery, runtime adapter, gameplay authority, save identity, or scene access.

### Verification

- Reviewer / QA accepted the frozen package through read-only static review: closed schema, defensive immutable copies, non-adjacent duplicate handling, deterministic canonicalization, factual documentation, and dependency boundaries.
- Package and assembly-definition JSON parsed successfully. The runtime assembly references only `RealmRaiders.ModuleContracts`, declares `noEngineReferences`, and contains no `UnityEngine` or `RealmRaiders.Runtime` reference.
- Focused Editor NUnit coverage is authored inside the uninstalled package but was intentionally not run. `git diff --check` passes.

### Scope intentionally deferred

- Installing the package, runtime discovery/adapter work, an approved module library, concrete starter recipes, models/textures/rigs/animations, Blender tooling, scene integration, or any gameplay/UI/save behavior.

## Diamond Pass 09.9 — Realm Landmark Silhouette Blockout

Completed on 2026-09-08.

### Delivered

- Added one idempotent presentation-only builder for the Sylvan Heart Tree, Sylvan Root Trap, Infernal Heart and Infernal Flame Trap using existing Unity primitive meshes.
- Heart Tree and Root Trap now use wide organic crown/root and low inward-radial silhouettes; Infernal Heart and Flame Trap use compact claw/spire and lane-aligned chevron silhouettes. Their identity no longer depends only on green versus red.
- The same Heart Tree recipe is used in Sylvan raid and defense. Every new primitive child is non-authoritative and loses its generated collider; objective/trap roots, transforms, colliders, state, damage, cooldown, route, fog and results remain unchanged.
- Presentation construction is duplicate-safe and bounded to `8` added renderers per objective and `6` per trap, with shared cached materials and scene-root cleanup.

### Verification

- Focused `RealmLandmarkPresentationTests`: `5/5` passed.
- Focused final-delta `SylvanRealmSmokeTests`: `9/9` passed after correcting an inactive-only test lookup for the fog-hidden Heart Tree; production behavior did not change.
- Final EditMode: `54/54` passed, `0` failed.
- Final PlayMode: `34/34` passed, `0` failed.
- QA observed the Defender landscape Keeper view with readable route, Root Trap center and HUD/action lane. Console reported `0` errors and `0` warnings; no code changed after the suites and `git diff --check` passes. Portrait and physical Android smoke were not run.

### Scope intentionally deferred

- Imported assets/textures, shaders, particles, animation, audio/haptics, paths/boundaries/terrain dressing, full 12-module environment kit, collider changes, gameplay/balance/save changes, and portrait/physical-device visual acceptance.

## Module Pass MMP 01 — Character Motion Profile Contracts

Completed on 2026-09-08.

### Delivered

- Added an isolated `com.realmraiders.character-motion-profiles` package with immutable metadata for one body family, rig/Animator profile IDs, the fixed `idle`, `locomotion`, `attack_primary`, `attack_ability`, `hit`, and `death` clip keys, faction rhythm, fallback profile, and provenance source IDs.
- Added deterministic validation for schema, IDs, family/rhythm, exactly six unique clip bindings, source-declared family/rig/key compatibility, and globally unique ordinal-sorted source IDs.
- Added validated fixed-order BOM-less UTF-8 serialization and a lowercase SHA-256 content hash. The closed input-field policy excludes mutable display data, asset/scene paths, timestamps, random values, and unknown fields.
- The package remains passive and uninstalled. It contains no animation assets, `Animator`/`AnimationClip` references, gameplay timings or curves, discovery, roster profiles, runtime adapter, root motion, event authority, or gameplay behavior.

### Verification

- Reviewer / QA accepted the frozen package through read-only static review: immutable defensive collections, closed six-clip schema, compatibility checks, non-adjacent duplicate handling, deterministic canonicalization, factual documentation, and dependency boundaries.
- Package and assembly-definition JSON parsed successfully. The runtime assembly references only `RealmRaiders.ModuleContracts`, declares `noEngineReferences`, and contains no `UnityEngine` or `RealmRaiders.Runtime` reference.
- Eight focused Editor NUnit tests are authored inside the uninstalled package but were intentionally not run. `git diff --check` passes.

### Scope intentionally deferred

- Installing the package, concrete family/faction profiles, approved rigs/clips/licences, Animator assets, Core motion adapter, root-motion/event gameplay, runtime discovery, or any visual/gameplay/save integration.

## Diamond Pass 10.0 — Realm Route Readability Blockout

Completed on 2026-09-08.

### Delivered

- Added one idempotent presentation-only route builder with distinct Sylvan organic and Infernal fractured styles using existing Unity opaque primitive meshes.
- Sylvan raid paths now receive one low rounded visual band per authoritative path root; Sylvan defense receives four staggered organic lane masses; Infernal defense receives four low angular causeway plates.
- Generated visual children have no active colliders or gameplay components. Authoritative route/floor roots retain their transforms, colliders, layers, tags, route ownership, navigation and gameplay behavior.
- Construction is deterministic and duplicate-safe, uses bounded renderer counts and shared cached materials, and cleans up with the owning route root and scene.

### Verification

- Focused `RealmRoutePresentationTests`: `4/4` passed.
- Focused `SylvanRealmSmokeTests`: `9/9` passed.
- Final EditMode: `58/58` passed, `0` failed. Final PlayMode: `34/34` passed, `0` failed.
- QA observed the Sylvan landscape Keeper route as continuous with Root Trap and HUD visible; the Infernal angular causeway was exercised by the final PlayMode suite. Console reported `0` errors and `0` warnings apart from two Test Runner information logs.
- No source changed after the final suites and `git diff --check` passes.

### Remaining manual validation

- Portrait Game View and physical Android readability/performance remain user-owned and were not claimed by QA.

### Scope intentionally deferred

- Boundary dressing, terrain replacement, imported assets/textures, custom shaders, particles, fog/light changes, animation, VFX/audio/haptics, new paths or waypoints, collider/NavMesh changes, procedural generation, BUILD/Hub decoration, and gameplay/balance/save changes.

## Design Pass DUX 01 — First Playable Minute v1

Completed on 2026-09-08 and staged in the Modules submodule.

### Delivered

- Defined one non-modal contextual onboarding thread for the existing `BUILD → DEFEND → POSSESS → RETURN → RESULT` route, with exact factual copy, targets and authoritative success signals.
- Covered Contextual, Fingertap and Joystick controls in portrait and landscape, including rotation continuity, multi-touch ownership, dismissal, persistent skip, forced return, death and terminal cleanup.
- Specified one minimal versioned local guide record with idempotent `NotStarted`, `Active`, `Completed` and `Skipped` transitions plus malformed-data fallback.
- Preserved all gameplay, possession, camera, result and save authorities and explicitly rejected tutorial scenes, modal gates, auto-actions, rewards, telemetry and new presentation assets.

### Verification

- Architect reviewed the full brief against the current Hub, Build, Defender HUD, control-style, possession and result code. Current scene routes, button names and authoritative signals match the normative brief.
- Static `git diff --check` passes. No Unity project, runtime source or asset was changed, so Unity suites were intentionally not rerun.

### Scope intentionally deferred

- Guide persistence and presentation code, HUD binding, gameplay observation hooks, focused automated coverage, Game View/device acceptance, localization and any wider tutorial or progression system.

## Diamond Pass 10.1 — First Playable Minute: Hub + BUILD Foundation

Completed on 2026-09-08.

### Delivered

- Added one versioned local first-minute guide record with idempotent `NotStarted`, `Active`, `Completed` and `Skipped` transitions plus safe missing/malformed/unsupported-data fallback.
- Only `START SYLVAN JOURNEY` activates a fresh guide. Legacy Hub routes retain their existing navigation and never activate or advance it.
- BUILD captures its real entry layout and shows truthful `Choose`, `Fix` or `Save` guidance from the actual five-slot layout and existing validation reason. Cycling back to the entry layout, invalid plans and unchanged valid saves do not claim success.
- Dismiss is transient per factual BUILD step and survives rotation; Skip persists once and removes guide presentation and callbacks. Both actions use the existing pointer-ownership contract.
- A changed valid plan accepted through the existing `SAVE & DEFEND` path sets only a consumable process-local handoff fact. `DefenseLayoutSave` remains the sole layout authority, and no defense guide or completion behavior was added.
- The BUILD guide reuses the existing Canvas, responsive HUD and reason lane. Its emphasis is steady and non-raycast, preserves the target button presentation, and cleans up on skip, disable and teardown.

### Verification

- Focused `FirstPlayableMinuteTests`: `5/5` passed.
- Focused `FirstPlayableMinuteFlowTests`: `4/4` passed.
- Final EditMode: `63/63` passed, `0` failed. Final PlayMode: `38/38` passed, `0` failed.
- Focused flows exercised journey-only activation, exact BUILD states, rotation, Dismiss/Skip, valid-save authority and cleanup. Console reported `0` errors and `0` warnings apart from two Test Runner information logs.
- No source changed after the final suites and `git diff --check` passes.

### Remaining manual validation

- A separate interactive Game View smoke and physical Android usability/readability pass were not run and remain user-owned.

### Scope intentionally deferred

- Defender selection/possession/movement/attack/dodge/release/result guidance, guide completion, Infernal or raid onboarding, modal tutorial content, auto-actions, rewards/progression/telemetry, new assets/audio/scenes/packages, and gameplay/camera/balance changes.

## Design Pass DART 01 — Character Production Pipeline v1

Completed on 2026-09-08 and staged in the Modules submodule.

### Delivered

- Defined an offline deterministic character factory around three shared family kits (`Humanoid`, `LargeCreature`, `Beast`), canonical recipes, identity modules and palettes rather than bespoke rigs/controllers per character.
- Specified versioned Generic rig, scale/origin, attachment-anchor, atlas/material, LOD, bone/weight, six-clip motion and Blender-to-Unity import contracts while preserving the gameplay-root `CharacterController` and presentation-pivot boundary.
- Added mobile production ceilings and a single-writer animation rule: no root motion, animation-event gameplay, runtime retargeting, private per-character Animator graph, physics bones or collider-fitting art changes.
- Defined deterministic automation gates, human licence/art/device gates, role ownership and realistic planning ranges: shared family foundations are the expensive setup, while accepted-family identity variants become the repeatable unit.
- Added a precise Guardian Ent first-production card with LargeCreature identifiers, slot/LOD/material/rig budgets, cultivation continuity and the next source/ownership plus neutral-gray rig gate.

### Verification

- Architect reviewed the full brief against the current 3DRT `Take 001` provenance, `CharacterVisualAssembler`, `CharacterVisualMotion`, root `CharacterController`, Guardian Ent cultivation hierarchy, modular-recipe and motion-profile contracts.
- Existing facts and authority boundaries match the project. Static `git diff --check` passes; no Unity project, asset, runtime source or package was changed, so Unity suites were intentionally not rerun.

### Scope intentionally deferred

- Selecting/downloading art, asserting a licence, Blender source creation, rigging, LOD or clip production, concrete recipe/profile instances, Unity import/integration, runtime animation, device performance acceptance and Blood Knight source modification.

## Diamond Pass 10.2 — First Playable Minute: Possession Proof

Completed on 2026-09-08.

### Delivered

- Continued the journey-only guide from the accepted BUILD handoff into Sylvan defense without changing legacy Hub or defense routes.
- Added one monotonic, factual `SELECT → POSSESS → MOVE → ATTACK → DODGE → RELEASE → KEEPER RETURN → RESULT` proof driven only by accepted gameplay actions and the existing same-entity possession flow.
- The guide observes authoritative displacement, accepted ability and dodge calls, explicit release, Keeper camera return, and the real defense terminal result; it never performs movement, combat, targeting, possession, release, camera control, or result progression for the player.
- Added session-local defense eligibility, scene revision tokens and explicit retry authorization so stale teardown, reloads and unrelated defense entry cannot duplicate or incorrectly resume the guide.
- Reused the existing Defender HUD and responsive layout in portrait and landscape, with non-raycast presentation, existing pointer ownership, and cleanup on skip, controller/lifecycle interruption, terminal state and scene teardown.
- Successful ordered completion persists the existing first-minute record exactly once. Early terminal outcomes offer a truthful retry path without claiming completion.

### Verification

- QA-focused first-minute and regression suites passed after correcting scene-token teardown, deterministic movement proof, legacy handoff expectations and PlayerPrefs isolation in Sylvan smoke coverage.
- Final EditMode: `66/66` passed, `0` failed. Final PlayMode: `42/42` passed, `0` failed.
- No source changed after the final suites and `git diff --check` passes. A separate physical Android manual pass was not run; PlayMode exercised the Defender flow and UI proof.

### Scope intentionally deferred

- Infernal or raid onboarding, modal tutorial content, auto-actions, rewards/progression/telemetry, new assets/audio/scenes/packages, wider tutorial replay/settings, and physical-device readability/performance acceptance.

## Research Pass ART 06A-R — Guardian Ent Source Shortlist

Completed on 2026-09-08 and staged in the Modules submodule.

- Compared three creator-published, source-redistributable candidates against the accepted `LargeCreature` family, mobile budgets and repository licence needs.
- Recommended CDmir / TinyWorlds `Forest Monster` under CC0 only after archive-level dependency and texture-provenance inspection; retained Benji Smith `Evil Tree Creature` under CC BY 3.0 as fallback.
- No asset was downloaded, approved, imported or entered into the licence registry. Unity was intentionally not run; static `git diff --check` passes.

## Research Pass ART 06B-R — Shared Animation Library Shortlist

Completed on 2026-09-08 and staged in the Modules submodule.

- Established that Realm Raiders needs one offline-baked six-key set per frozen body-family rig rather than one runtime-retargeted universal library.
- Recommended KayKit Character Animations 1.1 under CC0 for Humanoid intake and a CC0 wolf source as a Beast accelerator; the LargeCreature comparison remains conditional and needs human licence approval.
- No archive was downloaded or approved. Unity was intentionally not run; static `git diff --check` passes.

## Design Pass DLEGAL 01 — Third-Party Notices v1

Completed on 2026-09-08 and staged in the Modules submodule.

- Defined one offline, closable Hub notice panel using the existing responsive UI, with exact mandatory 3DRT CC BY 4.0 credit and voluntary Kenney CC0 provenance entries.
- Explicitly excluded the retained Quaternius asset from runtime notice copy until a human resolves the conflict between the acquisition record and the publisher's later licence page.
- No runtime, asset or licence-registry file was changed. Unity was intentionally not run; static `git diff --check` passes.

## Design Pass DART 02 — Guardian Ent Visual Identity v1

Completed on 2026-09-08 and staged in the Modules submodule.

- Selected the `Ancient Canopy Sentinel` direction: tall trunk mass, irregular wide crown, rooted arms and cumulative cultivation ranks 0–3 on the shared `LargeCreature` family.
- Froze slot, palette, rig/motion, LOD, renderer/material and portrait/landscape readability constraints while preserving the gameplay-root and `Presentation Pivot` boundary.
- Source selection and asset production remain gated. Unity was intentionally not run; static `git diff --check` passes.

## Design Pass DENV 01 — Sylvan Environment Production v1

Completed on 2026-09-08 and staged in the Modules submodule.

- Selected the `Open Grove Arches` direction and defined an exact eight-type modular environment kit for continuous BUILD → DEFEND → RAID visual language.
- Froze clearance, collision separation, camera composition, LOD/culling/batching, material/texture and Android production ceilings with a primitive fallback path.
- No art source or licence was approved and no Unity content was changed. Unity was intentionally not run; static `git diff --check` passes.

## Diamond Pass 10.3 — Third-Party Notices

Completed on 2026-09-08.

### Delivered

- Added one always-available `THIRD-PARTY NOTICES` entry to the existing Prototype Hub and one scene-local, closable, scrollable panel under its existing Canvas and responsive safe-area root.
- Added an explicit immutable offline catalogue containing the mandatory 3DRT CC BY 4.0 credit and the two voluntary Kenney CC0 provenance credits in accepted order. Quaternius is deliberately absent until its acquisition-licence conflict receives a human decision.
- Catalogue validation provides an exact mandatory 3DRT fallback, omits isolated malformed optional entries, rejects catalogue-wide faults, and emits at most one construction diagnostic without filesystem, Markdown, asset discovery or network access.
- The modal blocks underlying Hub interaction, owns pointer/scroll/close/Back gestures, restores prior interactability and focus, reopens at the mandatory credit, and preserves safe portrait/landscape layout and normalized reading position.
- Opening, closing, fallback and rotation leave realm, stores, control/orientation preferences, first-minute status and route destinations unchanged; no Canvas, EventSystem, AudioListener, scene, asset, package or persistence was added.

### Verification

- Focused notices: EditMode `5/5` and PlayMode `3/3` passed. Focused Hub navigation regression: `1/1` passed.
- Final EditMode: `71/71` passed, `0` failed. Final PlayMode: `45/45` passed, `0` failed.
- `git diff --check` passes. A separate manual Editor or physical Android smoke was not run; the focused PlayMode flow covered modal open/close, scroll, rotation, Back, fallback and route neutrality.

### Scope intentionally deferred

- Resolving or displaying the Quaternius licence, editing the authoring registry, full licence text, legal advice, browser/network links, privacy/terms UI, new assets, gameplay changes and physical-device readability acceptance.

## Design Pass DART 03 — Infernal Brute Visual Identity v1

Completed on 2026-09-08 and staged in the Modules submodule.

- Selected `Obsidian Gatebreaker`: a low, wide siege-creature silhouette with recessed head, separated massive fists, short legs and a broken back ridge that remains distinct from the tall Guardian Ent without relying on color.
- Froze the modular `base_body`/`head`/`back` recipe intent, shared `LargeCreature` rig/bind/anchor compatibility, Infernal palette, six-motion rhythm, LOD/material/texture budgets and portrait/landscape readability gates.
- Preserved same-entity possession, root collider, gameplay timing and `Presentation Pivot` authority; no source, licence, asset, rig, animation, recipe instance, Unity import or gameplay change was made.
- Architect reviewed the complete isolated brief and static `git diff --check` passes. Unity was intentionally not run.

## Diamond Pass 10.4 — Deterministic Invader Stuck Recovery

Completed on 2026-09-08.

### Delivered

- Added a deterministic measured-progress watchdog for the defense invader while it is intentionally advancing toward an existing route waypoint.
- Recovery uses a bounded right-first sidestep, a measured opposite-side retry and a narrow route-segment corridor, then exits immediately after truthful forward progress resumes.
- Defender acquisition, waypoint order, route authority, attack behavior and the existing root `CharacterController` remain authoritative; recovery never teleports, disables collision, skips waypoints or changes combat facts.
- Opening hold, waypoint pause, root, active actions, defender engagement, controller changes, death, terminal state, disable and teardown suppress or reset recovery without stale side intent.
- Focused PlayMode isolation now removes unrelated pre-existing scene colliders only for the fixture and restores them, preventing the Prototype Hub `Realm Pillar` from becoming an undeclared waypoint obstacle in full-suite order.

### Verification

- Focused EditMode recovery logic: `4/4` passed.
- Focused PlayMode route recovery: `2/2` passed on 2026-09-08 at 06:26:22Z–06:26:28Z.
- Final EditMode: `75/75` passed, `0` failed. Final PlayMode: `47/47` passed, `0` failed on 2026-09-08 at 06:27:35Z–06:28:09Z.
- DefenderTest manual smoke reached the normal Keeper opening with the invader and Ent healthy and the factual `INVADER HOLDING — ROOT GATE AHEAD` route state visible. Direct trap activation was not performed and is not claimed.
- No source changed after the final suites. Console reported `0` warnings and `0` errors, `git diff --check` passes, and no temporary `InitTestScene` assets remain in Git status.

### Scope intentionally deferred

- General pathfinding/NavMesh, crowd avoidance, new routes, layout changes, teleport/ghost recovery, camera/UI/combat/trap changes and physical-device verification.
- Continuous arena boundaries remain the separate Diamond Pass 10.5.

## Design Pass DART 04 — Beast Family Visual Identity v1

Completed on 2026-09-08 and staged in the Modules submodule.

- Selected `Mossback Courser` for the Sylvan Wolf and `Cinderjaw Stalker` for the Hellhound, with distinct shape-first silhouettes that share one exact Beast family rig, bind, anchors, atlas layout, LOD policy and six-key motion contract.
- Froze five-slot recipe intent, palette separation, mobile mesh/renderer/material/texture budgets, faction rhythm and same-entity/root-collider authority boundaries.
- Source selection, licence approval, asset production, animation and Unity integration remain gated. Architect reviewed the isolated brief; static `git diff --check` passes and Unity was intentionally not run.

## Design Pass DBND 01 — Prototype Arena Boundary Visual Language v1

Completed on 2026-09-08 and staged in the Modules submodule.

- Defined one build-once boundary language for `CharacterSandbox`, `DefenderTest`, `InfernalRealm` and `SylvanRealm`, while explicitly excluding the two UI-only scenes.
- Froze factual footprints, starting visual/collider dimensions, neutral/Sylvan/Infernal shape language, rectangle seam rules, Sylvan node/path-union openings, mobile budgets and gameplay-authority boundaries.
- No code, Unity asset, model, texture or gameplay behavior was added. Architect reviewed the isolated brief; static `git diff --check` passes and Unity was intentionally not run.

## Design Pass DENV 02 — Infernal Environment Production v1

Completed on 2026-09-08 and staged in the Modules submodule.

- Selected `Ironbound Rift Causeway`: straight basalt strata, hard notches, restrained iron and a framed destination that remains distinct from Sylvan without relying on red color or glow.
- Defined an eight-type modular kit, placement/reuse rules, atlas/material/LOD/mobile budgets, Flame Trap/Gate/Heart signal separation and compatibility with the accepted Infernal boundary contract.
- Source selection, licence approval, production assets and Core integration remain gated. Architect reviewed the isolated brief; static `git diff --check` passes and Unity was intentionally not run.

## Diamond Pass 10.5 — Prototype Arena Boundaries

Completed on 2026-09-08.

### Delivered

- Added one explicit, build-once boundary builder for the four 3D prototype zones while leaving `PrototypeHub` and `RealmBuild` without world borders.
- Replaced the Sandbox's disconnected stones with a continuous neutral rectangle and added continuous Sylvan-root and Infernal-basalt rectangles to the two defense lanes.
- Corrected only the two authorized final Sylvan path visual yaws, proved the factual floor union changes from three disconnected components to one, and built the border from the exposed contour of the inset seven-node/six-path union without bridging empty space.
- Kept one combined renderer and one shared material per style, static non-trigger `BoxCollider` runs, explicit mobile budgets and scene-local idempotent ownership without scanning, per-frame work, triggers, rigidbodies, NavMesh or gameplay authority.
- Added focused coverage for extents, corner overlap, Sylvan connectivity/inset/openings, budgets, idempotence, scene integration, ordinary and fast movement, existing dash completion and real `CreatureBrain` collision behavior.

### Verification

- Focused boundary builder EditMode: `3/3` passed.
- Focused boundary flow PlayMode: `5/5` passed on 2026-09-08 at 07:27:11Z–07:27:14Z.
- Final EditMode: `78/78` passed, `0` failed. Final PlayMode: `52/52` passed, `0` failed on 2026-09-08 at 07:28:39Z–07:29:16Z.
- The earlier `DodgeFlow` 0.00000006-second boundary miss did not reproduce on the unchanged final candidate. Console reported `0` info, `0` warnings and `0` errors; `git diff --check` passes and no temporary `InitTestScene` assets remain.
- QA launched all four gameplay scenes and observed continuous boundaries with their factual opening HUD/state. Collision actions, full encounter completion and portrait-to-landscape manual flow were not completed and remain user-owned alongside physical Android validation.

### Scope intentionally deferred

- Kill planes, teleport/fall recovery, new terrain, navigation/pathfinding, route or balance changes, jumping/climbing, hazards/damage, breakable walls, production art/textures, VFX/audio and physical-device performance claims.

## Module Pass MCR 02 — Deterministic Recipe Catalogue

Completed on 2026-09-08 and staged in the Modules submodule.

### Delivered

- Added an explicit `IModularCharacterRecipeProvider` boundary and immutable catalogue construction for the existing modular-character recipe contract.
- Snapshots caller-supplied providers and recipes, sorts successful recipes with ordinal semantics and prepares exact recipe-ID and character-ID dictionaries without discovery, reflection, filesystem access, singleton ownership or Unity dependency.
- Fails closed with deterministic structured issues for missing/unreadable providers, invalid module IDs, null/invalid recipes and duplicate provider, recipe or character identities.
- Added package tests for input-order determinism, exact lookup behavior, duplicate/invalid rejection, collection immutability and the passive dependency boundary; package documentation and version are updated to `0.2.0`.

### Verification

- Architect reviewed all five changed package files and confirmed that the runtime assembly remains plain C# with no Unity/game-runtime, filesystem, discovery or concrete-character dependency.
- Package manifest JSON and `git diff --check` pass. The package remains uninstalled, so no Unity Test Runner result is claimed for this isolated pass.

### Scope intentionally deferred

- Concrete character recipes, approved asset sources, provider discovery, package installation, Core host integration, runtime visual consumption, Unity assets and gameplay changes.

## Diamond Pass 10.6 — Possession Energy Return Readability

Completed on 2026-09-08.

### Delivered

- Added a deterministic, immutable presentation mapper that derives possession-energy copy, urgency and normalized meter state without owning Unity objects, timers, saves or gameplay authority.
- The shared Sylvan and Infernal Defender HUD keeps its normal factual meter above five seconds, shows `POSSESSION ENDING` from five seconds, and changes to `RETURN TO KEEPER` for the final two seconds.
- The meter continues to follow authoritative energy while text and color work is cached by semantic tenth/state, avoiding repeated identical label rebuilding.
- Manual release, forced release, controller loss, death and terminal result immediately return the meter to factual normal presentation through existing possession state.
- Possession energy amount, drain, forced-return timing, controllers, camera, health, combat, dodge and result behavior remain unchanged.
- Stabilized the pre-existing dodge-immunity upper-bound assertion with a test-only floating-point tolerance; production dodge timing remains exactly `0.18s`.

### Verification

- Focused mapper EditMode coverage passed `24/24`; focused shared-HUD PlayMode coverage passed `14/14` before the final test-only tolerance adjustment.
- Final EditMode: `87/87` passed, `0` failed. Final PlayMode: `53/53` passed, `0` failed.
- `git diff --check` passes. QA did not perform a separate manual smoke; physical Android validation remains user-owned.

### Scope intentionally deferred

- Energy balance or replenishment, a second terminal state, new controls/settings, haptics/audio/VFX, new UI roots/assets, race-specific timing and physical-device claims.

## Diamond Pass 10.7 — Mobile Combat Input Buffer

Completed on 2026-09-08.

### Delivered

- Added one deterministic, controller-owned `0.20s` input buffer for a direct player's next eligible ability during Recovery.
- The newest ready alternate request replaces the prior request and executes at most once, only after the authoritative action returns to Idle and only through the existing `CombatEntity.TryUse` gate.
- HUD buttons, short-range enemy taps and swipe abilities now share the same player request path; AI continues using its unchanged authoritative combat path.
- Windup, Impact, invalid/cooling abilities, dodge, inactive/dead/terminal play and expired requests never queue or execute.
- The existing ability buttons truthfully show `NEXT` and `QUEUED` without adding a Canvas, button or raycast surface.
- Controller loss, possession release, death, disable/destroy, terminal state, expiry and input interaction-revision changes clear the pending request immediately.
- Movement, joystick/fingertap ownership, combat timing and values, possession, camera awareness, first-minute flow and AI behavior remain unchanged.

### Verification

- Focused EditMode coverage proves exact-boundary expiry, replacement, deterministic direction capture and consume-once behavior.
- Focused PlayMode coverage proves hero and possessed-defender execution plus rejection and lifecycle cleanup paths.
- Final EditMode: `90/90` passed, `0` failed. Final PlayMode: `53/53` passed, `0` failed on 2026-09-08.
- No gameplay source, import or test changed after the final suites, and `git diff --check` passes.
- A separate Editor or physical Android manual smoke was not run; device feel validation remains user-owned.

### Scope intentionally deferred

- Combo trees, animation canceling, attack-speed or recovery changes, queued movement/dodge/trap/possession actions, multi-command queues, auto-targeting, AI buffering, new effects/assets and balance changes.

## Module Pass MMP 02 — Deterministic Motion Profile Catalogue

Completed on 2026-09-08 and staged in the Modules submodule.

### Delivered

- Added an explicit motion-profile provider boundary and one deterministic immutable catalogue for the existing character-motion contracts.
- Caller-owned provider and profile collections are snapshotted, successful profiles are ordered with ordinal semantics, and exact profile-ID lookup is fail-closed.
- Missing, unreadable, invalid and duplicate inputs produce stable structured issues without reflection, discovery, filesystem access, Unity or gameplay authority.
- Added isolated package tests for deterministic ordering, exact lookup, invalid/duplicate rejection and collection immutability; package documentation and version are updated to `0.2.0`.

### Verification

- Independent static review found no code-level blocker and confirmed that all five changes remain inside `Packages/com.realmraiders.character-motion-profiles/`.
- Runtime keeps `noEngineReferences` and depends only on the passive module-contract assembly; manifests parse and package `git diff --check` passes.
- The package remains uninstalled, so its authored Unity package tests have not yet run and no Test Runner result is claimed.

### Scope intentionally deferred

- Concrete motion providers, animation clips/controllers, package installation, Core presentation integration, provider discovery, runtime retargeting and gameplay changes.

## Module Pass MART 01 — Character Art Intake Manifests

Completed on 2026-09-08 and staged in the Modules submodule.

### Delivered

- Added one immutable, closed version-1 evidence record for a reviewed character-art source: stable character/source/rig identity, exact source and licence links, attribution and change note, archive SHA-256, safe repository-relative source file, six motion keys, ordered LOD budgets and explicit import flags.
- Added deterministic fail-closed validation with structured issue paths, fixed-field BOM-less UTF-8 canonical JSON and a lowercase SHA-256 content hash.
- Enforced the visual/gameplay boundary by rejecting source colliders, root motion and animation events while retaining renderer, material, texture and triangle ceilings for mobile review.
- Added 11 isolated package tests covering valid canonical data, malformed paths/URLs/checksums, motion and LOD completeness/order, budget/import restrictions, immutability and dependency boundaries.

### Verification

- Architect and an independent static reviewer confirmed the eight-file package stays under `Packages/com.realmraiders.character-art-manifests/`, keeps `noEngineReferences` and references only the passive module-contract assembly.
- Runtime and Editor-test compiler source checks, package JSON parsing and `git diff --check` passed. The package remains uninstalled, so no Unity Test Runner result is claimed.

### Scope intentionally deferred

- Concrete third-party or owned-source manifests, source approval/download, Unity importing, models, rigs, clips, materials, prefabs, package installation, discovery and Core runtime integration.

## Module Pass MART 02 — Deterministic Art Manifest Catalogue

Completed on 2026-09-08 and staged in the Modules submodule.

### Delivered

- Added an explicit `ICharacterArtIntakeManifestProvider` boundary and immutable catalogue construction for the accepted art-intake evidence record.
- Snapshots caller-supplied providers and manifests, sorts successful records by source ID with ordinal semantics and provides exact source-ID and unique character-ID lookup.
- Missing, unreadable, invalid, duplicate or ambiguous inputs return no partial catalogue and produce deterministic structured issues without reflection, discovery, filesystem, network, Unity or import authority.
- Added five focused package tests for deterministic order, exact lookup, snapshot immutability, empty input and the complete fail-closed provider/manifest boundary; package version is `0.2.0`.

### Verification

- Architect reviewed the four-file change and confirmed the runtime contract remains plain C# with `noEngineReferences` and no concrete asset, licence claim, game-runtime or Unity dependency.
- Runtime and Editor-test compiler source checks, package JSON assertions and `git diff --check` passed. The package remains uninstalled, so its authored package tests have not run in Unity.

### Scope intentionally deferred

- Concrete source providers, source approval or acquisition, package installation, provider discovery, Unity importer/editor tooling, model processing, asset creation and Core runtime consumption.

## Module Pass MART 03 — Character Art Measurement Gate

Completed on 2026-09-08 and staged in the Modules submodule.

### Delivered

- Added an immutable adapter-neutral measurement snapshot for one imported character-art source: exact character/source identity, one triangle count per LOD, renderer/material/texture ceilings, maximum texture edge, prohibited collider/root-motion/animation-event observations and imported motion-clip IDs.
- Added a deterministic fail-closed compliance evaluator that compares those explicit measurements with an already valid intake manifest and preserves structured manifest-validation evidence.
- Exact manifest limits pass; malformed identity, null or unreadable collections, missing/duplicate/unknown LODs, invalid or over-budget counts, prohibited import state and inconsistent animation/clip coverage fail with ordinal-sorted semantic issue paths.
- Caller-owned collections are snapshotted and exposed read-only. The package still performs no Unity inspection, filesystem or network access, discovery, import, source acquisition or gameplay work.
- Added eight focused source tests for boundary values, budget failures, exact identity, prohibited import state, animation coverage, unreadable input, immutability, deterministic issue order and dependency isolation; package version is `0.3.0`.

### Verification

- Architect reviewed all five changed files and confirmed the implementation remains inside `Packages/com.realmraiders.character-art-manifests/`, keeps the existing plain-C# `noEngineReferences` boundary and adds no game-runtime or import authority.
- Runtime and Editor-test compiler source checks, package JSON parsing and `git diff --check` passed. The package remains uninstalled, so its authored package tests have not run in Unity and no Test Runner result is claimed.
- Accepted in the Modules submodule as commit `ec873f3` (`feat: validate character art measurements`); push remains user-owned.

### Scope intentionally deferred

- A Unity or Blender measurement adapter, concrete source manifests/measurements, source approval or acquisition, package installation, automated importing/model processing, assets and Core runtime consumption.

## Module Pass MART 04 — Deterministic Character Art Batch Report

Completed on 2026-09-08 and staged in the Modules submodule.

### Delivered

- Added an explicit batch-provider and manifest–measurement pair boundary for evaluating large character-art intake sets without automatic discovery.
- The builder snapshots caller-supplied providers/items, rejects null or unreadable structure and duplicate module/source/character IDs, then evaluates every uniquely identified item through the existing compliance gate.
- A successful immutable report is sorted by source ID with ordinal semantics, exposes exact source/character lookup and truthful compliant/noncompliant totals, and retains every item-level compliance issue under a stable semantic path.
- Structural failures return no partial report, while an identifiable but noncompliant item remains visible in the report so failed intake cannot disappear from aggregate counts.
- Added eight focused source tests for order invariance, structural failure, unreadable inputs, duplicate identity, issue preservation, immutability, exact lookup/totals and dependency isolation; package version is `0.4.0`.

### Verification

- Architect reviewed all four changed files and confirmed that the package remains explicit, deterministic and independent of Unity, game runtime, filesystem, network, discovery and import authority.
- Runtime and Editor-test compiler source checks, package JSON parsing and `git diff --check` passed. The package remains uninstalled, so no Unity Test Runner result is claimed for its authored tests.
- Accepted in the Modules submodule as commit `d716abb` (`feat: report character art intake batches`); push remains user-owned.

### Scope intentionally deferred

- Concrete providers, source approval/acquisition, a Unity or Blender measurement adapter, package installation, automatic imports, asset processing and Core runtime consumption.

## Diamond Pass 10.8 — Canonical Sylvan Journey Continuity

Completed on 2026-09-08.

### Delivered

- Added one session-only, tokenized and fail-closed `PrototypeJourney` state helper for the explicit `Build → Raid → RaidResult → Defense` sequence without a scene object, save migration, gameplay authority or automatic progression.
- The Hub's primary Sylvan journey now truthfully opens BUILD; a valid active BUILD shows `SAVE & RAID`, both factual Sylvan raid outcomes show `DEFEND YOUR REALM`, and the factual Sylvan defense result completes once through `RETURN TO BUILD`.
- Preserved every direct Hub/BUILD/raid/defense/Infernal route, retry and Hub action. Direct routes never inherit or advance stale journey state, and teardown/controller-independent cancellation fails closed.
- Preserved the changed-BUILD first-minute handoff across raid so it begins only in the real Sylvan defense, while existing exact-once raid reward crediting and guide completion remain unchanged.
- Reworked the existing raid and defense result action lanes into explicit normalized portrait/landscape regions under their current HUD roots; the final portrait defense lane remains clear of the first-minute guide controls.
- Stabilized the existing saved-layout regression by snapshotting authored DefenderTest slot positions from `sceneLoaded` after bootstrap and before live AI movement, preserving all expected coordinates and the original `.01` tolerance.

### Verification

- Focused journey logic EditMode coverage passed `4/4`; focused journey PlayMode coverage passed `4/4`.
- Focused first-minute result-layout regression passed `1/1`; focused fixed-layout spawn snapshot regression passed `1/1`.
- Final EditMode: `94/94` passed, `0` failed at 13:33:33 local time. Final PlayMode: `57/57` passed, `0` failed at 13:34:53 local time.
- No source changed after the final suites and `git diff --check` passes. Manual Editor and physical Android journey smoke remain user-owned and were not run by QA.

### Scope intentionally deferred

- New scenes, automatic progression, skipped combat, combat/AI/balance/possession changes, new persistence or rewards, wider tutorial content, new assets/packages/UI roots, desktop support and physical-device claims.

## Diamond Pass 10.9 — Stable Starter Roster Host

Completed on 2026-09-08.

### Delivered

- Installed only the pinned local `com.realmraiders.starter-character-catalog` package, exposed its three Editor tests through `testables`, and added the minimum explicit runtime assembly references.
- Added one lazy immutable host snapshot of the exact five starter archetypes using an explicitly constructed provider; ordinal stable-ID lookup, family conversion and visual-profile mapping fail closed without reflection or discovery.
- Runtime-created character definitions now carry a stable archetype ID separately from their scene-instance and display aliases. Blood Knight, Guardian Ent, Sylvan Wolf, Infernal Brute and Hellhound aliases retain their existing names.
- Replaced the two Wolf display-name branches with explicit stable-archetype decisions while preserving every existing stat, ability, cooldown, role, possessability, controller, prefab/fallback, palette and scene count.
- Preserved current visual recipes through an exhaustive profile-to-recipe mapping and retained the same-entity possession contract.
- Stabilized the alias regression helper so fog-hidden entities are included while requiring exactly one matching object name, exactly one matching definition display name, the same entity and the exact expected archetype ID.
- Tracked the eight Unity-generated starter-package `.meta` files in the Modules submodule so package GUIDs remain stable across checkouts.

### Verification

- Focused package Editor tests: `3/3` passed. Focused roster host EditMode tests: `5/5` passed.
- Focused Sylvan, Defender, Infernal and Character Sandbox alias checks each passed `1/1`, including the inactive fog-hidden `Wolf Alpha` and all requested scene aliases.
- Final EditMode: `102/102` passed, `0` failed at 14:04:42 local time. Final PlayMode: `57/57` passed, `0` failed at 14:05:56 local time.
- Console ended with `0` warnings and `0` errors; `git diff --check` passes. QA did not run a separate manual smoke; physical Android validation remains user-owned.

### Scope intentionally deferred

- Recipe/motion/art-manifest package installation, dynamic module discovery, stats or ability migration, new characters/models/animations, visual changes, save migration, gameplay/balance/UI changes and physical-device claims.

## Reliability Follow-up — Camera Awareness Lifecycle and PlayMode Isolation

Completed on 2026-09-09.

### Delivered

- Direct-controller deactivation and destruction now explicitly clear the existing camera-awareness ownership before releasing direct input, including a change of `Main Camera`.
- `PossessionFlowTests` now creates and removes only build-scene or fixture-owned cleanup scenes. The Unity Test Runner's own `Untitled` scene is never unloaded, preventing the prior teardown hang and cross-test camera leakage.
- No camera targeting, movement, combat, possession, scene, save or visual behavior was broadened.

### Verification

- Focused `PossessionFlowTests`: `14/14` passed in `10.063s`.
- Shared final verification: EditMode `113/113` passed and PlayMode `60/60` passed, both with `0` failures; PlayMode exited normally.
- Console after EditMode contained `3` logs, `0` warnings and `0` errors. No manual smoke was run.

### Scope intentionally deferred

- Camera tuning, new focus rules, test-runner tooling/CLI and any gameplay change.

## Diamond Pass 11.0 — In-Run Control Style Switcher

Completed on 2026-09-09.

### Delivered

- Added one scene-local, pointer-owning selector under each existing gameplay `ResponsiveHudRoot` for Character Sandbox, Sylvan Raid and both Defender flows; Hub and BUILD remain unchanged.
- The live control cycle is exactly `Contextual → Fingertap → Joystick → Contextual`, persists through the existing preference and states factual concise copy: `CONTROL: AUTO`, `CONTROL: TAP` or `CONTROL: STICK`.
- A live switch clears transient movement, destination and UI-gesture ownership without reloading a scene or changing entity identity, health, cooldown, possession energy, controller ownership, combat or journey state.
- Joystick visibility now refreshes immediately: it remains hidden in Keeper view, appears after possession when appropriate and disappears immediately for Fingertap; terminal results hide the selector and release its transient input.
- Portrait and landscape use a dedicated safe-area selector position outside the shared combat action column, and focused coverage asserts one selector/Canvas/EventSystem plus continuity and containment.

### Verification

- Focused selector coverage: EditMode `3/3` passed and PlayMode `5/5` passed.
- Final shared verification after the frozen candidate: EditMode `113/113` passed and PlayMode `60/60` passed, `0` failures; PlayMode exited normally.
- Console after EditMode: `3` logs, `0` warnings and `0` errors. No manual smoke or physical-device validation is claimed.
- `git diff --check` passed before acceptance.

### Scope intentionally deferred

- In-run orientation switching, new settings/modal/Canvas, rebinding, desktop/gamepad controls, pause behavior, aim assist, camera changes, haptics/audio/VFX, save migration and physical-device claims.

## Module Pass MMP 03 — Motion Profile Compatibility Gate

Completed on 2026-09-09 and accepted in the Modules submodule.

### Delivered

- Added an adapter-neutral, immutable compatibility gate for one explicitly supplied character-motion target and profile.
- The gate validates exact body family, rig profile, the six required keyed clips and declared fallback policy, preserving existing profile-validation evidence with deterministic semantic issue paths.
- Added eight isolated Editor tests, package documentation and version `0.3.0`; the package remains plain C#, `noEngineReferences`, uninstalled and without discovery, asset loading, Unity, animation or gameplay authority.

### Verification

- Independent static review confirmed scope inside `Packages/com.realmraiders.character-motion-profiles/`, explicit immutable contract behavior and no Core integration need.
- New runtime and test sources, package JSON and `git diff --check` passed. A broad static test-folder check retains one pre-existing NUnit overload diagnostic outside the new files.
- Accepted in Modules commit `180285f` (`feat: validate motion profile compatibility`). Unity was intentionally not run; no Test Runner result is claimed.

### Scope intentionally deferred

- Concrete motion providers, rigs, clips/controllers, profile selection, package installation, Core presentation integration, Unity import/retargeting and gameplay authority.

## Diamond Pass 11.1 — Keeper Touch Selection Reliability

Completed on 2026-09-09.

### Delivered

- Replaced legacy `CombatEntity.OnMouseDown()` selection with one `PossessionManager`-owned Keeper press→release path that considers only explicitly registered creatures.
- Added deterministic exact-hit precedence, short-edge-normalized near-miss tolerance and stable entity-identity tie-breaking through a small pure resolver.
- UI-owned, cancelled, swiped, terminal, transition, active-possession, dead, unregistered, occluded and out-of-radius inputs now select nothing.
- Existing selection presentation, possession, entity identity, health, cooldowns, possession energy, controllers, camera, colliders, combat and journey state remain authoritative and unchanged by selection.

### Verification

- The all-assembly Unity GUI runs included the new Keeper resolver and flow coverage; their result rows were green.
- Final EditMode: `118/118` passed, `0` failed. Final PlayMode: `62/62` passed, `0` failed and exited Play Mode normally.
- No manual smoke was run; QA changed no code after the final suites, and `git diff --check` passed before acceptance.

### Scope intentionally deferred

- Aim assist/lock-on, camera/framing, movement/targeting, HUD/layout/copy, AI/balance, collider geometry, persistence, scenes, packages, art, VFX/audio/haptics, desktop controls and physical-device claims.

## P0 — Sylvan Raid Boundary Path Recovery

Completed on 2026-09-09.

### Delivered

- Repaired only the factual Portal → Crossroads route obstruction introduced by arena-boundary work: the visible Portal route tree remains as presentation but no longer owns a blocking collider.
- Replaced the flattened node-floor primitive's raised `CapsuleCollider` with a static, non-trigger support-surface `MeshCollider`, aligned with the connecting path top surface so the Crossroads rim no longer forms a movement lip.
- Preserved the intended closed south Portal shoulder, the full outer boundary, factual route footprint, possession, combat, dash, navigation authority and all scene content.
- Added a factual all-node-to-path join geometry regression plus Portal-to-Crossroads normal movement and existing Blood Rush flow coverage.

### Verification

- Focused boundary coverage: EditMode `4/4` passed and PlayMode `6/6` passed.
- Final shared verification: EditMode `119/119` passed and PlayMode `63/63` passed, with `0` failures.
- Unity Console ended with `0` errors and `0` exceptions; `git diff --check` passed before acceptance.
- Manual Android/portrait/landscape Portal-route smoke remains user-owned and pending; it is not claimed by this acceptance.

### Scope intentionally deferred

- Boundary visual redesign, terrain, navigation/pathfinding, movement/dash tuning, combat/possession changes, new map content, art, UI changes, device-performance claims and any unrelated route work.

## Module Preview Passes MWS 02, MWS 03 and MUI 01 — World-Surface and Jump-Icon Candidates

Accepted on 2026-09-09 in the Modules submodule.

### Delivered

- Added separately documented original-generated `1024×1024` Sylvan moss/root-stone and Infernal basalt/iron/ember albedo plus normal-map candidates.
- Added one original-generated `512×512` transparent Jump action icon candidate designed for `64 px` mobile readability.
- Every candidate records its provenance, exact role, intended Unity import settings and a clear `preview-import-candidate-not-approved-for-runtime` status. No third-party source is involved.

### Verification

- Reviewed from the committed Modules sources as preview-only assets; they do not alter Unity package resolution, scenes, runtime visuals, gameplay, licensing notices or device claims.
- `git diff --check` was clean when each isolated Modules commit was accepted.

### Scope intentionally deferred

- Texture tileability/seam proof, Unity import, normal-map orientation, compression/memory budget, sprite readability in a real HUD, material/shader setup, runtime installation and all gameplay integration.

## Diamond Pass 12.0 — Grounded Mobile Jump

Completed on 2026-09-09.

### Delivered

- Added one deterministic `CharacterJumpState` owned by the existing `CombatEntity` and moved only through the existing root `CharacterController`. A live direct player can rise, retain normal horizontal movement and land while static arena boundaries still constrain the root.
- Added strict real gates: no jump while airborne, rooted, dodging, resolving an ability, terminal, dead, controller-disabled or during controller replacement/release. A jump also rejects abilities and dodge until it clears; root, death, disable, terminal and controller swap cancel it immediately.
- Added pointer-owning `JUMP` controls under the existing Raid and Defender HUD roots. They are hidden outside direct control and state truthfully as `JUMP`, `JUMP — ROOTED`, `JUMP — AIRBORNE` or `JUMP — BUSY`; the Defender responsive action layout explicitly reserves a non-overlapping landscape position.
- Added focused pure jump-state coverage and PlayMode coverage for rise/land, no double jump, lifecycle gates, airborne boundary containment, UI ownership and both responsive HUD orientations.
- The existing first-minute defense guide was shifted only after a QA-proven opening-lane overlap, and its assertion now reports exact label identity/text/rect data if that regression returns.

### Verification

- Focused stale-assembly recovery check: `OrderedSylvanProofCompletesOnFactualVictory` passed `1/1` in `4.443s` after Unity reopened cleanly.
- Final Unity GUI Test Runner: EditMode `120/120` passed and PlayMode `67/67` passed, `0` failures, completed 2026-09-09 19:47:08 EEST. No source changed after the final suites and `git diff --check` passed.
- Follow-up Unity Game-view smoke: before Play Mode SylvanRealm showed `No cameras rendering`, but Play Mode rendered through `Main Camera` in both portrait Free Aspect and `1280×720` landscape. `JUMP` visibly entered `AIRBORNE`; an immediate second press left it airborne, and it returned to normal `JUMP` after landing. Console visibly showed `0` errors and `0` warnings after ordinary compilation.
- Possessed Defender and arena-edge containment were not observed in that limited UI pass because its controls/target were not exposed; no broader manual claim is made. `Assets/UniversalRenderPipelineGlobalSettings.asset` remains a separate unrelated editor change, is unproven as a render blocker, and is excluded from this pass.

### Scope intentionally deferred

- Coyote time, input buffering, double/wall/charged jumps, air attacks/dashes, AI jump, animation, VFX/audio/haptics, the preview Jump icon, terrain/puzzle changes, desktop controls and physical-device claims.

## Diamond Pass 12.1 — Sylvan Route Is Never a Hidden Puzzle

Completed on 2026-09-09.

### Delivered

- Changed generated Sylvan node trees from accidental physics obstacles into presentation-only revealables. Their renderer and graph-driven reveal behavior remain intact; their primitive colliders are removed. Node support floors, combat entities, route dimensions and the factual outer arena boundary remain authoritative and unchanged.
- Extended the Portal → Crossroads regression from merely entering the node rim to ordinary forward walking past the Crossroads centre. The proof checks the hero remains inside the factual Portal/path/Crossroads union, receives no damage, leaves no action resolving, and cannot encounter a route-facing node-tree collider.
- Repaired an unrelated test-fixture assumption found by the final suite: the shared first-minute control-proof helper now explicitly selects its already-established portrait test layout before asserting portrait contextual copy. Production UI, input, saved preferences and later explicit portrait/landscape coverage are unchanged.

### Verification

- Focused 12.1 coverage passed: `SylvanPortalToCrossroadsSupportsNormalMovementAndDashWithoutStateDamage` `1/1` in `4.368s`, and `SylvanCorrectedJunctionsStayOpenWhileOuterPathAndNodeShouldersBlockMotion` `1/1` in `0.058s`.
- The fixture-specific rerun `OrderedSylvanProofCompletesOnFactualLoss` passed `1/1` in `4.427s` after the deterministic test-layout correction.
- Final Unity GUI Test Runner: EditMode `120/120` passed; PlayMode `67/67` passed with `0` failures (2026-09-09 17:41:02–17:41:53 local run). `git diff --check` passed before acceptance.
- QA recovered Unity through the normal Hub path and confirmed SylvanRealm renders at `1280×720` landscape with Main Camera, Portal and the first circle visible. Its Game-view automation did not expose a safe direct-walk or outer-boundary input path, so this acceptance does **not** claim those manual observations; no source changed after final suites.

### Scope intentionally deferred

- Terrain, waypoint/map redesign, navigation/pathfinding, jump tuning, camera/HUD changes, combat/AI/balance, art/materials/textures, physical-device validation and any additional traversal mechanic.

## Diamond Pass 12.2 — Intentional Touch Controls and Camera

Completed on 2026-09-09.

### Delivered

- Direct-controlled Joystick mode now intentionally uses analog stick movement, the existing `JUMP` button and a non-UI world drag for bounded manual orbital camera yaw. World tap-to-move, enemy target taps and swipe ability gestures no longer leak through in this style.
- Fingertap retains its single-tap ground and enemy behavior, while two quick nearby releases on the same valid empty-ground intent trigger the existing grounded jump exactly once. The first tap's destination is preserved, so existing airborne horizontal movement creates the expected forward leap.
- Fingertap rejects a double-tap jump from UI-owned gestures, swipes, enemy/possessable/interactive hits, invalid states and control-style or orientation changes. Existing controller, terminal, death, transition and teardown paths clear partial pointer, jump and camera intent safely.
- The existing camera rig now accepts only bounded presentation yaw: Joystick drag updates manual yaw, while Fingertap recenters softly only from factual accepted locomotion displacement. Neither path changes movement, targeting, combat direction, ability authority, threat focus or camera pitch/vertical framing.
- Existing Raid and Defender HUD roots now state the effective control contract truthfully and show `JUMP` only for effective Joystick control, including both possession states and responsive orientations.

### Verification

- Focused PlayMode coverage passed: `CombatCameraReadabilityTests` `3/3`, `JumpFlowTests` `6/6`, and `InRunControlStyleFlowTests` `3/3`, including valid/rejected Fingertap double taps, Joystick camera-only drag, factual movement yaw and lifecycle cleanup.
- Final Unity GUI Test Runner: EditMode `120/120` passed and PlayMode `71/71` passed, both with `0` failures on 2026-09-09. QA used no CLI and no source changed after the final suites; `git diff --check` passed.
- Game View smoke observed Sylvan Realm at `1280×720` landscape with the stick/look/JUMP contract and portrait with the tap/double-tap contract. DefenderTest Keeper start, selected Ent availability and a clean Console (`0` logs, warnings and errors) were observed.
- QA could not safely complete the possessed DefenderTest Game View interaction after its coordinate frame became unstable. This manual limitation is explicit; possession/control behavior remains covered by the final PlayMode suite and physical-device validation remains user-owned.

### Scope intentionally deferred

- New jump physics, double/wall/charged jumps, aim assist, target lock, automatic combat, targeting redesign, camera collision/zoom/pitch, new control roots, desktop controls, animation/VFX/audio/haptics, save changes and physical-device claims.

## Diamond Pass 12.3 — First Realm Surface Preview

Completed on 2026-09-09.

### Delivered

- Installed only the approved preview albedos for Sylvan and Infernal world surfaces, together with their local original-generated provenance records. Normal-map candidates remain excluded.
- Added a deterministic import rule limited to those two assets: sRGB, mipmaps, bilinear filtering, clamp wrap, Read/Write disabled and Android `512` / ASTC `6×6` preview settings.
- The existing route presentation now caches four shared materials and safely binds the appropriate realm albedo to its floor and route presentation. A missing or failing resource returns to the exact prior solid-colour materials without throwing or allocating per renderer.
- Routes, gameplay roots, colliders, boundaries, paths, entities, camera, input, combat, AI, traps, HUD and scene flow are unchanged; this is a material-only presentation layer.
- Added focused material-cache/fallback and scene-collision regression coverage.

### Verification

- Focused EditMode preview provider test passed `1/1`; focused PlayMode scene binding/collision test passed `1/1`.
- Final Unity GUI Test Runner: EditMode `121/121` passed and PlayMode `72/72` passed, both with `0` failures. No source changed after the final suites; `git diff --check` passed.
- QA Game View smoke confirmed distinct imported Sylvan and Infernal surfaces in both `1280×720` portrait and landscape, with Console `0` logs, warnings and errors.

### Scope intentionally deferred

- These source records explicitly label both images as preview candidates, not approved final runtime art. Tileability/seam proof, normal-map orientation, shader work, terrain, lighting/post-processing, memory/performance measurement and physical-device validation remain unclaimed.

## Diamond Pass 12.4–12.5 — Jump Preview and Defender Terminal Cleanup

Completed on 2026-09-09.

### Delivered

- Added the project-owned, original-generated transparent Jump icon as a clearly labelled preview resource with its provenance record and a one-asset mobile Sprite import rule.
- The icon is one cached, non-raycast visual child of the existing Raid or Defender `JUMP` button. It preserves the original button action and leaves a functional text-only fallback if the resource is absent.
- The existing Jump labels retain bounded best-fit space for `JUMP`, `JUMP — ROOTED`, `JUMP — AIRBORNE` and `JUMP — BUSY`. Fingertap retains its existing hidden-Jump contract.
- Repaired the reproducible Defender terminal HUD defect: factual victory/loss now immediately hides and disables every live gameplay action — possess, release, trap, abilities, dodge and Jump — while result actions remain available.
- Terminal guards prevent late selection, possession, release and refresh callbacks from restoring live actions over the result; a new retry scene restores the normal trap and selection-driven controls.

### Verification

- Focused `JumpActionIconPreviewTests` and `JumpActionIconScenePreviewTests` each passed `1/1`; the reproduced `EarlyTerminalRetriesWhileForcedAndControllerLossNeverProveRelease` regression passed `1/1` after a normal Unity restart rebuilt the stale PlayMode assembly.
- Final EditMode passed `122/122` with `0` failures. The project owner manually cleared the Test Runner filter, ran PlayMode Run All and confirmed it was green; QA could not reliably retain the transient Test Runner summary panel to capture its numeric total.
- `git diff --check` passed for the accepted candidate. Unity reserialized only non-semantic whitespace in the already committed 12.3 texture metas; it was normalized outside this feature.

### Remaining manual validation

- QA deliberately did not make blind scene selections while the project owner was active in Unity, so the planned Defender terminal/retry and Sylvan Jump-icon Game View smoke remains unclaimed. Confirm those two views and physical-device readability before treating the preview icon as final UI art.

### Scope intentionally deferred

- Final UI-art approval, other ability icons, layout redesign, new HUD roots, control/jump/physics/camera changes, VFX/audio/haptics, device-performance measurements and any general onboarding redesign.

## Diamond Pass 12.6 — Spatial Defense Plan Preview

Completed on 2026-09-10.

### Delivered

- The existing RealmBuild HUD now has one non-interactive, scene-local visual plan from invader entry through the five factual defense slots to the Heart Tree.
- The preview uses only the current `DefenseLayout`: Wolf is fast intercept, Ent is possessable guardian, Root Trap is manual hold trap, and Open is unassigned. It neither adds a defense rule nor gains gameplay, pointer, persistence, camera or scene authority.
- The fixed five plan nodes, lane and endpoint are built once and refresh in place after initial load, slot cycling, valid saving and cultivation refresh.
- Portrait and landscape use intentional HUD lanes; the preview remains non-raycast and adds no Button, Canvas, EventSystem, AudioListener or scene system.
- The PlayMode fixture now unloads its RealmBuild scene after completion, so it does not leak its camera, HUD or cultivation button into later PlayMode tests.

### Verification

- Focused `BuildPlanPreviewTests` and `BuildPlanPreviewFlowTests` each passed `1/1`.
- Final Unity Test Runner: EditMode `123/123` passed and PlayMode `74/74` passed, both with `0` failures.
- RealmBuild portrait smoke confirmed the preview is readable and the Editor log showed no new runtime error. Game-view pointer input was unavailable during that smoke, so manual slot-cycle and `1280×720` landscape observations are not claimed; their lifecycle and layout behavior are covered by the PlayMode flow test.
- The 12.6 runtime/test candidate remained unchanged after the final suites. `git diff --check` passed for the accepted paths.

### Scope intentionally deferred

- Drag/drop placement, editable route geometry, new defense pieces/rules/costs, build animation, terrain/world art, audio/VFX/haptics, new persistence and physical-device validation.

## Module Pass MWS 04 — World Surface Seam Validator

Completed on 2026-09-10 and accepted in the Modules submodule.

### Delivered

- Added an isolated, editor-independent RGBA seam validator for caller-supplied row-major pixels.
- The immutable input snapshot and deterministic analyser report left-to-right and top-to-bottom mean/worst edge deltas; a surface passes only when each worst delta is at or below its supplied threshold.
- The package has no Unity, file-system, image-decoding, shader, importer, discovery or gameplay authority.

### Verification

- Static package review confirmed the explicit RGBA `/1020` contract, one-channel `0.25`, full-RGBA `1.0` and threshold-boundary coverage.
- Scoped whitespace and banned-API scans passed. The pure package is not installed, so no Unity Test Runner result is claimed.
- Accepted in Modules commit `69f7779` (`feat: add world surface seam validator`).

### Scope intentionally deferred

- A host pixel provider, texture decoding, Unity import validation, package installation, shader/material action and any runtime use.

## Module Pass MWS 05 — Surface Preview Budget Gate

Completed on 2026-09-10 and accepted in the Modules submodule.

### Delivered

- Extended the isolated world-surface-validation package with immutable caller-declared preview metadata and a deterministic mobile-budget result.
- The gate accepts only a non-empty semantic identifier, positive square power-of-two source dimensions, an Android maximum dimension no greater than `512`, Read/Write disabled, mipmaps enabled, and the explicit `ASTC_6x6` or `ETC2_RGBA8` labels.
- Failures have fixed ordinal codes and messages, so a later adapter can show clear evidence without this package loading assets, reading files, inspecting Unity import settings, or making import decisions.

### Verification

- Static review confirmed the `1024 → 512` accepted profile, `512/513` boundary, invalid-dimension, stable multi-issue and null-input coverage.
- Scoped whitespace and forbidden-API checks passed. The isolated package remains uninstalled, so no Unity Test Runner result is claimed.
- Accepted in Modules commit `f35927e` (`feat: add world surface preview budget gate`).

### Scope intentionally deferred

- Texture decoding, importer inspection/action, Core integration, package installation, shader/material work, new art and gameplay/runtime use.

## Module Pass MVT 03 — Mobile Visual Budget Compatibility Gate

Completed on 2026-09-10 and accepted in the Modules submodule.

### Delivered

- Extended the isolated character-visual-tuning package with an immutable, caller-supplied mobile visual-budget policy and deterministic compatibility result for the existing immutable visual-budget declaration.
- It reports stable ordinal evidence for absent or invalid budget/policy inputs and for excess materials, textures, texture-edge size or triangle count. It has no defaults, model measurement, asset/import inspection, Unity or gameplay authority.

### Verification

- Static review confirmed accepted Blood Knight starter-budget coverage, every individual exceed condition, deterministic multi-issue order, invalid/null input and immutable input/result coverage.
- Scoped whitespace and forbidden-API checks passed. The pure package remains uninstalled, so no Unity Test Runner result is claimed.
- Accepted in Modules commit `a3df8ce` (`feat: add mobile visual budget gate`).

### Scope intentionally deferred

- Selecting production limits, measuring/importing models, modifying starter profiles, Unity/Core integration, asset downloads, animation, materials/shaders and runtime use.

## Module Pass MVT 04 — Starter Visual Budget Batch Evidence

Completed on 2026-09-10 and accepted in the Modules submodule.

### Delivered

- Extended the isolated character-visual-tuning package with one immutable ProfileId-ordered batch report over an explicitly supplied existing visual-tuning catalogue and caller policy.
- Every row holds a copied declared budget and the exact MVT 03 compatibility evidence; aggregate compatible/incompatible totals are frozen with the report. Missing catalogue evidence is explicit, while absent or invalid policies are truthfully represented on each affected row.
- The package still cannot choose a budget, discover providers, measure a 3D model, inspect assets/import settings or affect Unity gameplay.

### Verification

- Static review confirmed deterministic order regardless of caller insertion, mixed-result totals, missing catalogue, absent/invalid policy evidence and report/row snapshot immutability.
- Scoped whitespace and forbidden-API checks passed. The isolated package remains uninstalled, so no Unity Test Runner result is claimed.
- Accepted in Modules commit `4dc6f4d` (`feat: add visual budget batch report`).

### Scope intentionally deferred

- Model measurement, policy selection, starter-profile edits, asset/import work, Unity/Core integration, animation/material work and runtime use.

## Diamond Pass 12.7 — Build-to-Defense Deployment Receipt

Completed on 2026-09-10.

### Delivered

- Sylvan defense now opens with one factual, non-interactive receipt of the five defense slots that were actually instantiated from the saved Build plan.
- The receipt names the real Root Gate, guards, open slots and Heart Tree route without adding a new defense rule or duplicating unavailable pieces.
- It yields the instructional lane to the first-minute guide and clears on possession, movement, terminal result, retry, transition, disable and teardown in both orientations.

### Verification

- Focused EditMode receipt data/layout coverage and PlayMode `SylvanDeploymentReceipt_UsesActualDeploymentAndCleansEveryOpeningExit` passed in the final suites.
- The project owner manually confirmed the resulting prototype presentation was good before final automated acceptance.
- Final Unity GUI Test Runner baseline shared with 12.8: EditMode `137/137` and PlayMode `78/78`, `0` failures, on 2026-09-10. `git diff --check` passed.

### Scope intentionally deferred

- Editable deployment during defense, new defense pieces/rules, route editing, animation, sound/haptics, final art and physical-device readability claims.

## Diamond Pass 12.8 — Camera-Relative Direct Attack Direction

Completed on 2026-09-10.

### Delivered

- Untargeted direct-player attacks now snapshot their world direction from the visible camera plane: screen-up/HUD means camera-forward and Fingertap screen-right means camera-right after camera yaw.
- A factual Fingertap enemy tap remains stronger explicit target intent; Joystick world drag remains camera-only and does not cast an attack.
- Buffered abilities retain their input-time direction. Ongoing movement may translate the actor during an action but cannot rotate its accepted telegraph, dash/impact geometry or recovery facing away from that direction.
- Stabilized three legacy PlayMode proofs by pumping their exact state transitions with bounded deadlines and adding only float-representation headroom at the intended landing tolerance boundary; gameplay timings and assertions remain intact.

### Verification

- New PlayMode coverage passed for HeroCombat/PossessedCreature, portrait/landscape, Fingertap/Joystick, enemy-tap priority, buffered snapshots and held-movement action direction.
- Final Unity GUI Test Runner: EditMode `137/137` and PlayMode `78/78`, both with `0` failures. PlayMode XML recorded 2026-09-09 22:17:47Z–22:18:41Z; no source changed after the final suites and `git diff --check` passed.
- No physical-device or subjective attack-feel claim is made for this pass; the user owns the next device feel check.

### Scope intentionally deferred

- Aim assist, target lock/acquisition, camera-relative movement, camera framing/yaw tuning, AI direction, combat values/timing, animation/VFX/audio/haptics and physical-device validation.

## Module Pass MMP 04 — Explicit Character Motion Binding Batch

Completed on 2026-09-10 and accepted in the Modules submodule.

### Delivered

- Added an immutable explicit character-to-motion-profile batch evaluator over the existing deterministic catalogue and compatibility gate.
- Requests are snapshotted, exact ordinal lookup is required, compatibility issue detail is preserved, duplicate/malformed/missing input fails closed, and successful records are character-ID ordered.
- A partially unreadable input stream discards partial data and returns only the unreadable collection boundary; the package does not select fallbacks, clips or animation authority.

### Verification

- Scoped whitespace and prohibited-runtime scans passed, with focused NUnit coverage authored for the full contract including partial enumeration failure.
- The isolated package remains uninstalled and its external `RealmRaiders.ModuleContracts` source is absent from the Modules repository, so no standalone compile or Unity Test Runner result is claimed.
- Accepted in Modules commit `50d7a4a` (`feat: add character motion binding batch gate`).

### Scope intentionally deferred

- Package installation, concrete motion profiles/clips, model/rig import, fallback selection, Core adapter/runtime animation authority and Unity integration.

## Diamond Pass 12.9 — Possessed-Defense Invader Awareness

Completed on 2026-09-10.

### Delivered

- `RaidInvaderBrain` now publishes factual current hostile intent through an event-driven active-invader lifecycle without changing target selection, route timing, recovery or attacks.
- A directly controlled possessed defender can receive the existing bounded camera focus, target plate and left/right edge cue before the first hit when its factual invader is alive and within the existing 14-metre eligibility range.
- `ATTACKING` urgency is limited to accepted Windup/Impact phases. Retargeting, range exit, invader controller loss/death/destruction, possession release, terminal state, camera transition and teardown clear the presentation.
- The implementation adds no scene scan, target lock, aim assist, new Canvas or gameplay authority.

### Verification

- Static review and scoped `git diff --check` passed for the three reserved implementation/test paths.
- The focused test `CombatCameraAwareness_TracksRaidInvaderIntentDuringPossessedDefenseAndCleansUp` passed inside the final suite in `0.230860` seconds.
- Final Unity GUI Test Runner: EditMode `137/137` and PlayMode `79/79`, both with `0` failures. PlayMode ran 2026-09-10 05:30:33Z–05:31:27Z; EditMode completed at 05:32:42Z.
- A filtered PlayMode `Run Selected` first stalled in Test Runner staging before entering the test. It was cancelled; after clearing the filter, ordinary `Run All` completed normally. No Mac or Unity restart was required.
- No manual Game-view or physical-device smoke is claimed for this pass; device noticeability remains user-owned.

### Scope intentionally deferred

- Target lock, aim assist, AI changes, combat tuning, new HUD/camera systems, animation/VFX/audio/haptics and physical-device readability tuning.

## Diamond Pass 13.0 — Sylvan Seam-Hardened Path Integration

Completed on 2026-09-10.

### Delivered

- Copied only the accepted original-generated MWS07 Sylvan RGB path candidate into the main project with its exact Modules source, commit `6138f71`, SHA-256 and preview-only provenance.
- Sylvan floor and route presentation now use the new seam-hardened resource with Repeat wrapping, while Infernal and boundary materials remain unchanged.
- The importer keeps sRGB, mipmaps, bilinear filtering and non-readable source data with Android maximum 512 and ASTC 6×6 compression.
- Existing lazy resource resolution, separate role-shared materials and exact null/throw solid-colour fallbacks are preserved. No geometry, collider, shader, package or gameplay authority changed.

### Verification

- Static review confirmed the project PNG is byte-identical to the Modules source (`c5043bfb…`), provenance JSON is valid and the resource folder contains no repeat-grid evidence or PBR maps.
- Focused EditMode coverage proves resource/provenance/import settings, cache behavior and exact null/throw fallbacks. Focused PlayMode coverage proves Sylvan segment/defense and Infernal transforms, mesh, collider identity/state/shape/layer plus collider-free presentation children.
- Final Unity GUI Test Runner: EditMode `139/139` and PlayMode `79/79`, both with `0` failures. PlayMode XML records 2026-09-10 06:24:30Z–06:25:24Z.
- Architect observed the SylvanRealm Game view with the new mossy-stone route, hero and HUD visible. Movement, device performance and final-art approval are not claimed.
- Full new-file whitespace, scoped diff and provenance checks are clean after a `.meta` whitespace-only cleanup that changed no import value.

### Scope intentionally deferred

- Infernal/boundary integration, second-tile macro variation, terrain/shader work, normal/height/emission maps, final-art approval and physical-device performance/readability.

## QA Tool 01 — Accessible Unity Test Gates

Completed on 2026-09-10.

### Delivered

- Added two accessible **Realm Raiders → QA** commands that launch one full,
  explicitly unfiltered EditMode or PlayMode suite through Unity's public
  `TestRunnerApi` without manipulating the Test Runner window.
- The gate refuses compilation, asset update, Play Mode and overlapping owned-run
  states; it logs factual start/final totals and preserves only minimal ownership
  across PlayMode domain reload.
- Core and Module remain Unity-free. Reviewer / QA + Build is restored as the
  sole Unity/test/build operator; Architect retains static review and acceptance.

### Verification

- QA invoked each new command exactly once in the existing healthy Unity GUI.
- EditMode passed `139/139`; PlayMode passed `79/79`, both with `0` failures,
  skipped or inconclusive tests.
- PlayMode ownership recovered after script reload and cleared on completion;
  both menu commands were enabled afterwards. No CLI, restart, overlapping run,
  Test Runner filter/layout change, source edit, commit or export occurred in QA.

### Scope intentionally deferred

- Focused-test selection through this menu, automatic suite chaining, CLI tests,
  automatic retries and platform export orchestration.

## Module Pass MWS 09 — Sylvan Living-Root Mobile Normal Map

Completed on 2026-09-10 and accepted in the Modules submodule.

### Delivered

- Added a deterministic, dependency-free PNG tool that derives normalized RGB
  tangent-space normals from explicit RGB/RGBA input with wrapped-edge sampling
  and bounded strength.
- Produced one restrained 1024×1024 Sylvan living-root normal candidate and an
  exact 2048×2048 2×2 repeat-evidence image from the accepted MWS08 source.
- Provenance records the exact source/tool commands, parameters and source/output
  hashes; no third-party source or Unity authority was introduced.

### Verification

- All `5/5` tool tests passed. Repeated generation was byte-identical, declared
  SHA-256 values matched, dimensions/colour modes were correct and the full-frame
  repeat grid showed no hard center-cross seam.
- The generated relief remains intentionally subtle so painted bark detail does
  not become exaggerated mobile geometry.
- Accepted in Modules commit `f3475ac` (`art: derive Sylvan living-root normal map`).

### Scope intentionally deferred

- Unity import orientation, material binding, Android compression/device lighting,
  colliders/gameplay, height/displacement maps and Infernal derivatives.

## Diamond Pass 13.1 — Sylvan Living-Root Boundary Presentation

Completed on 2026-09-10.

### Delivered

- Imported the accepted original-generated MWS08 living-root albedo and MWS09
  restrained tangent normal byte-identically with local provenance and explicit
  Repeat/mipmap/mobile ASTC 6×6 settings.
- Only `SylvanRoots` receives one atomically lazy-loaded shared textured material;
  missing or throwing resources retain the exact solid fallback, while Neutral
  and Infernal boundaries remain texture-free.
- The combined visual mesh now has deterministic oblique UVs and tangents without
  adding or moving vertices, triangles, children, colliders or gameplay authority.

### Verification

- Architect static checks confirmed exact source hashes, valid provenance,
  intended import settings, reserved scope and clean whitespace.
- QA native gates passed EditMode `145/145` and PlayMode `80/80`, both with zero
  failed, skipped or inconclusive tests; the new six EditMode and one PlayMode
  boundary-surface cases were included.
- SylvanRealm portrait showed restrained living-root relief with no obvious
  streaking, seams or gaps; three safe taps advanced Portal → Crossroads and
  Rooms 1 → 2. DefenderTest landscape showed the same stable treatment while
  autonomous engagement reduced invader health `220 → 174`. Console remained
  at zero logs, warnings and errors.

### Scope intentionally deferred

- Final-art/device-performance approval, Infernal/Neutral textures, triplanar or
  terrain shaders, height/displacement/emission/masks and geometry/gameplay changes.

## Module Pass MWS 10 — Sylvan Clearing Floor Production Candidate

Completed on 2026-09-10 and accepted in the Modules submodule.

### Delivered

- Derived one distinct 1024×1024 RGB Sylvan clearing-floor albedo from the
  project-owned MWS02 direction, plus exact 2048×2048 repeat evidence.
- Used the accepted deterministic wrapped-edge tool at strength `0.35` to create
  one restrained RGB tangent normal and exact 2×2 evidence.
- Recorded the exact source path/hash, operation, tool command, output hashes,
  dimensions and no-third-party-source truth in local provenance.

### Verification

- Declared and computed SHA-256 values match; all images have the declared RGB,
  no-alpha dimensions and the provenance JSON plus whitespace checks are clean.
- Full-frame review found no hard center-cross seam. Small foliage/pebble
  periodicity remains an explicit Unity/device check rather than final approval.
- Accepted in Modules commit `646ecde` (`art: prepare Sylvan clearing surface`).

### Scope intentionally deferred

- Unity import/material binding, Android compression, on-device lighting and
  periodicity, runtime geometry/collision/gameplay, final-art approval and
  Infernal clearing art.

## Art Pass MUI 02 — Encounter Cue Icon Candidates

Completed on 2026-09-10 and accepted in the Modules submodule.

### Delivered

- Produced three original 512×512 RGBA preview icons for factual `discovered`,
  `hostiles present` and `area clear` states without text or third-party sources.
- Added a dark-neutral 48 px actual-size contact sheet, a 256 px overview and
  provenance containing exact prompts, mappings and SHA-256 values.

### Verification

- Dimensions, RGB/RGBA modes, transparent/partial/opaque alpha evidence, JSON,
  declared hashes and whitespace are clean.
- All three silhouettes remain distinct at 48 px. The dark hostiles silhouette
  retains an explicit 32 px/in-context contrast gate and is not runtime-approved.
- Accepted in Modules commit `3bc738c` (`art: prepare encounter cue icon set`).

### Scope intentionally deferred

- Unity sprite import, runtime binding, 13.2 text/layout changes, animation,
  font/audio/VFX, gameplay/reward changes and final UI approval.

## Art Pass MUI 03 — Blood Knight Ability Icon Candidates

Completed on 2026-09-10 and accepted in the Modules submodule.

### Delivered

- Produced three original 512×512 RGBA preview icons for Basic Slash, Blood Rush
  and Heavy Cleave with one shared dark-iron, crimson and pale-gold language.
- Added 48 px and 256 px dark-neutral evidence sheets plus exact prompt, mapping,
  alpha and SHA-256 provenance without third-party sources or model likeness.

### Verification

- Dimensions, alpha evidence, JSON, declared hashes and whitespace are clean.
- The quick diagonal slash, forward charge wedge and broad area crescent remain
  visibly distinct at 48 px; Basic Slash keeps a named 32 px/in-context gate.
- Accepted in Modules commit `3fa21af` (`art: prepare Blood Knight ability icons`).

### Scope intentionally deferred

- Unity sprite import, RaidHUD binding, mobile compression, 32 px comprehension,
  cooldown/readiness behavior, combat balance, animation/VFX/audio and final art.

## Diamond Pass 13.2 — Factual Encounter Entry and Clear Cue

Completed on 2026-09-10.

### Delivered

- Realm nodes now publish their immutable first-visit identity and explicitly
  supplied, currently alive `CombatEntity` contents without scene scans, tags,
  names or polling.
- `RaidManager` owns an idempotent scene-local entry/count/clear lifecycle using
  existing Health death events while preserving exact-once room/enemy rewards.
- `RaidHUD` has one brief non-raycast responsive text cue for discovery, factual
  hostile count and `AREA CLEAR`, with timeout, next-state, terminal, disable and
  teardown cleanup.

### Verification

- Architect static review confirmed reserved scope, event unsubscription,
  gameplay/input boundaries and clean whitespace.
- QA native gates passed EditMode `148/148` and PlayMode `81/81`, with zero
  failed, skipped or inconclusive tests after correcting one Unity fake-null test
  assertion. Console after the final run was 0 logs, 0 warnings and 0 errors.
- Manual Crossroads → Wolf Grove portrait/landscape smoke is not claimed: CUA
  returned `noWindowsAvailable` for Game-view actions. This is an automation
  observation blocker, not a recorded runtime failure.

### Scope intentionally deferred

- Encounter icons, audio/VFX/haptics, gates/doors, loot, minimap, AI, combat,
  camera, route/reward changes and physical-device/manual cue readability.

## Diamond Pass 13.3 — Sylvan Clearing-Floor Presentation

Completed on 2026-09-10.

### Delivered

- Imported the accepted MWS10 original-generated 1024² RGB clearing albedo and
  restrained tangent normal byte-identically with local provenance and explicit
  Repeat/mipmap/Android 512 ASTC 6×6 settings.
- All existing Sylvan circular node floors share one cached textured material at
  the accepted initial 3.5-world-unit scale while fog tint remains per renderer.
- Atomic resource failure retains the existing solid Moss path; geometry,
  colliders, node radius/events, routes, encounter logic and non-Sylvan art stay
  unchanged.

### Verification

- Architect confirmed exact Modules `646ecde` hashes, valid provenance, import
  intent, atomic fallback, shared-material behavior and reserved whitespace.
- QA native gates passed EditMode `154/154` and PlayMode `82/82`, both with zero
  failed, skipped or inconclusive tests; Console was 0 logs/warnings/errors.
- Manual node-scale, fog, normal orientation, repetition, movement and portrait
  smoke are not claimed: the first Game-view click failed with CUA
  `windowNotFoundAtPosition`, after which QA correctly stopped further clicks.

### Scope intentionally deferred

- Final-art/device approval, manual periodicity and normal review, terrain or
  triplanar shaders, second-tile variation, Infernal surfaces and gameplay changes.

## Art Pass MUI 05 — Infernal Brute Ability Icon Candidates

Completed on 2026-09-10 and accepted in the Modules submodule.

### Delivered

- Produced original 512×512 RGBA Infernal Smash, Charge and Ground Slam previews
  with forged obsidian, restrained ember-red and pale-hot edges.
- Kept the shared mechanical meanings while making silhouettes materially
  different from Guardian Ent living-root icons; added 48/256 px evidence.
- Provenance contains exact prompts, mappings and SHA-256 values without
  third-party sources or character likeness.

### Verification

- Dimensions, alpha evidence, JSON, hashes and whitespace are clean after one
  missing-prompt provenance correction.
- Close impact, forward momentum and radial shock remain distinct at 48 px;
  Ground Slam retains a named 32 px detail gate.
- Accepted in Modules commit `a4c41c1` (`art: prepare Infernal Brute ability icons`).

### Scope intentionally deferred

- Unity import, HUD/runtime binding, mobile compression, 32 px comprehension,
  combat logic/balance, animation/VFX/audio and final art.

## Diamond Pass 13.4 — Factual Encounter Cue Icons

Completed on 2026-09-10.

### Delivered

- Imported the three accepted original MUI02 encounter sprites byte-identically
  with exact provenance and explicit single-sprite Android settings.
- The existing text-first encounter cue now owns exactly one non-raycast phase
  icon for discovery, hostiles and clear states, with intentional 44 px portrait
  and 40 px landscape placement.
- Per-phase cached loading preserves exact text-only behavior when sprites are
  missing or throw; every existing timeout, next-node, terminal and teardown
  cleanup remains authoritative.

### Verification

- Architect confirmed exact Modules `3bc738c` hashes, provenance/prompts, sprite
  import settings, phase mapping, one-image ownership and clean fallback scope.
- QA native gates passed EditMode `158/158` and PlayMode `82/82`, both with zero
  failed/skipped/inconclusive tests; Console was 0 logs/warnings/errors.
- Manual icon meaning, contrast, count/clear transition and both-orientation layout
  are not claimed because the first Game-view click failed externally and QA
  correctly stopped repeated attempts.

### Scope intentionally deferred

- Device/manual readability, animation/glow, ability/control icons, audio/VFX,
  encounter/gameplay changes and final UI approval.

## Art Pass MUI 06 — Control-Style Icon Candidates

Completed on 2026-09-10 and accepted in the Modules submodule.

### Delivered

- Produced original 512×512 RGBA preview icons for truthful Contextual,
  Fingertap and Joystick modes without text, device brands or AI claims.
- Added actual-size 32 px and 48 px dark-neutral sheets, a 256 px overview and
  exact prompt/mapping/hash provenance.

### Verification

- Dimensions, RGBA/alpha, all six hashes, JSON and whitespace are clean.
- All three remain distinct at 32 px. Joystick is clearest; Contextual and
  Fingertap retain named in-context comprehension checks before integration.
- Accepted in Modules commit `29bc278` (`art: prepare control-style icons`).

### Scope intentionally deferred

- Unity import, in-run switcher binding, mobile compression, input/camera changes,
  accessibility/device claims, animation and final UI approval.

## Art Pass MUI 04 — Guardian Ent Ability Icon Candidates

Completed on 2026-09-10 and accepted in the Modules submodule.

### Delivered

- Produced original 512×512 RGBA Smash, Charge and Ground Slam previews using a
  cohesive ancient living-wood, moss and pale-gold visual language.
- Added 48 px/256 px evidence and exact prompts, mappings, alpha and SHA-256
  provenance without third-party sources or character likeness.

### Verification

- Dimensions, alpha evidence, JSON, hashes and whitespace are clean.
- Close impact, forward root momentum and radial root shock are distinct at
  48 px; the detailed Ground Slam retains a named 32 px/in-context gate.
- Accepted in Modules commit `0834c2f` (`art: prepare Guardian Ent ability icons`).

### Scope intentionally deferred

- Unity import, DefenderHUD binding, mobile compression, ability/combat changes,
  animation/VFX/audio, 32 px comprehension and final art.

## Diamond Pass 13.5 — Blood Knight Ability Icons

Completed on 2026-09-10.

### Delivered

- Imported only the three accepted original MUI03 Blood Knight ability sprites
  byte-identically with local provenance and explicit mobile UI import settings.
- Decorated the existing `SLASH`, `BLOOD RUSH` and `CLEAVE` controls through an
  explicit stable mapping while preserving labels, cooldown/readiness truth,
  callbacks, navigation and responsive layouts.
- Each non-raycast image is cached once and exact text-only controls remain when
  a resource is missing or throws; teardown removes the presentation cleanly.

### Verification

- Architect confirmed exact MUI03 hashes/provenance/import settings, explicit
  mapping, cache/fallback behavior, input ownership and scoped whitespace.
- QA native gates passed EditMode `162/162` and PlayMode `82/82`, both with zero
  failed/skipped/inconclusive tests; Console was 0 logs/warnings/errors.
- Manual 32–48 px meaning, cooldown readability and both-orientation layout are
  not claimed because the first Game-view click failed externally with
  `windowNotFoundAtPosition`; this is not a recorded runtime failure.

### Scope intentionally deferred

- Device/manual readability, animation/glow, audio/VFX/haptics, Guardian and
  Infernal ability icons, combat changes and final UI approval.

## Art Pass MUI 07 — Realm Identity Icon Candidates

Completed on 2026-09-10 and accepted in the Modules submodule.

### Delivered

- Produced original 512×512 transparent Sylvan living-seed/root and Infernal
  forged-obsidian gate marks with materially different fantasy silhouettes.
- Added real-size dark 32/48 px, distinct midtone 48 px and 256 px evidence plus
  exact prompts, mappings, original-generation truth and SHA-256 provenance.

### Verification

- Dimensions, alpha, all six image hashes, JSON and whitespace are clean after
  correcting an initially duplicated dark/midtone evidence sheet.
- Both realm identities remain distinct at small size; Sylvan fine root/leaf
  detail retains a named 32 px in-context readability gate.
- Accepted in Modules commit `04b8861` (`art: prepare realm identity icons`).

### Scope intentionally deferred

- Unity import/runtime binding, Hub/HUD layout, final branding, animation/VFX,
  accessibility/device claims and gameplay changes.

## Diamond Pass 13.6 — Guardian Ent Ability Icons

Completed on 2026-09-10.

### Delivered

- Imported the accepted original MUI04 Guardian Ent ability trio byte-identically
  with exact provenance and explicit mobile UI sprite settings.
- Added non-raycast Smash and Ground Slam icons to the two existing Sylvan
  possessed-defender buttons; Charge is a non-interactive `SWIPE: CHARGE` mark
  only when Fingertap really owns that gesture.
- Joystick, Keeper, release, death, terminal, Infernal and unknown-defender states
  hide or avoid the Charge presentation and preserve their truthful controls.

### Verification

- Architect confirmed exact MUI04 hashes/imports, explicit archetype and semantic
  mapping, cache/fallback behavior, no new Charge button and lifecycle cleanup.
- QA final native gates passed EditMode `166/166` and PlayMode `82/82`, both with
  zero failed/skipped/inconclusive tests; Console was 0 logs/warnings/errors.
- Two PlayMode fixture defects were corrected without runtime changes: elapsed
  energy was reset to a deterministic baseline and destroyed objects use Unity
  null semantics. Manual layout/meaning is not claimed because the first Game-view
  click failed externally with `windowNotFoundAtPosition`.

### Scope intentionally deferred

- Device/manual readability, Infernal ability icons, animation/VFX/audio, combat,
  input/camera changes and final UI approval.

## Diamond Pass 13.7 — Infernal Brute Ability Icons

Completed on 2026-09-10.

### Delivered

- Imported the accepted original MUI05 Infernal Brute trio byte-identically with
  exact provenance and explicit mobile UI sprite settings.
- Added family-isolated obsidian/ember Smash and Ground Slam sprites to the two
  existing buttons plus a non-interactive Fingertap-only `SWIPE: CHARGE` mark;
  Joystick still owns world-drag camera look and shows no false Charge gesture.
- Guardian, Blood Knight, unknown identities, labels, callbacks, readiness/queue,
  possession/release/death/terminal behavior and text-only fallback remain intact.

### Verification

- Architect confirmed exact MUI05 hashes/imports, separate family caches/prefixes,
  explicit identity/semantic mapping, no new control and lifecycle cleanup.
- QA native gates passed EditMode `170/170` and PlayMode `82/82`, both with zero
  failed/skipped/inconclusive tests; Console was 0 logs/warnings/errors.
- Manual Infernal/Guardian presentation is not claimed because the first Game-view
  click failed externally with `windowNotFoundAtPosition`; no runtime failure was
  observed.

### Scope intentionally deferred

- Device/manual readability, realm identity marks, animation/VFX/audio, combat,
  input/camera changes and final UI approval.

## Diamond Pass 13.8 — Realm Identity Marks

Completed on 2026-09-10.

### Delivered

- Imported only the accepted original MUI07 Sylvan living-seed and Infernal
  obsidian-gate marks with exact provenance and mobile UI sprite settings.
- Added one cached, reusable 48 px non-raycast child to existing canonical realm
  text in Hub, Build, Raid and both Defense configurations; it is never a button.
- Explicit identity mapping, Hub refresh/swap and missing/throwing/unknown fallback
  preserve exact text and prevent stale or duplicate marks.

### Verification

- Architect confirmed exact MUI07 hashes/imports, two-family cache, one-image
  ownership, state refresh/teardown, fallback and no gameplay/input authority.
- QA native gates passed EditMode `174/174` and PlayMode `82/82`, both with zero
  failed/skipped/inconclusive tests; Console was 0 logs/warnings/errors.
- The initial strict PlayMode layout gate found a shared ~1 px text overlap; the
  gap was increased from 8 to 10 px and the exact EditMode expectation followed.
  Manual readability remains unclaimed after the first external Game-view click
  failed with `windowNotFoundAtPosition`.

### Scope intentionally deferred

- 32 px runtime use, device/final branding approval, animation/VFX/audio, new
  realms, gameplay changes and physical-device validation.

## Diamond Pass 13.9 — Guided Possessable Ent Locator

Completed on 2026-09-10.

### Delivered

- Added one cached `▼  POSSESSABLE ENT` marker under the existing first-minute
  Defense guide, projected only from its explicitly supplied Guardian Ent.
- The non-raycast Text/Outline marker clamps inside the responsive safe area in
  portrait and landscape without any selection, camera or gameplay authority.
- Selection/possession/dismiss/death/terminal/skip/shutdown hide it; factual
  forced Keeper return reuses it and teardown leaves no orphan.

### Verification

- Architect confirmed explicit-target ownership, one-object reuse, safe-area
  projection, noninteraction, lifecycle cleanup and the two-file scope.
- QA native gates passed EditMode `174/174` and PlayMode `82/82`, both with zero
  failed/skipped/inconclusive tests; Console was 0 logs/warnings/errors.
- Manual marker placement/readability is not claimed because the first Game-view
  click failed externally with `windowNotFoundAtPosition`; no runtime failure was
  observed.

### Scope intentionally deferred

- General objective/quest/target markers, camera focus, input changes, tutorial
  expansion, device final polish and gameplay content.

## Diamond Pass 14.0 — Explain Premature Explicit Release

Completed on 2026-09-10.

### Delivered

- Records only a real, unforced player RELEASE request made too early in the
  active first-minute possession proof.
- After factual Keeper return, Select shows `RELEASED EARLY — SELECT THE ENT TO
  TRY AGAIN`; reselect clears it and the existing Ent marker remains reusable.
- Forced release, energy depletion, death, terminal state, controller loss,
  correct release, skip, shutdown and teardown never retain player-blaming copy.

### Verification

- Architect confirmed explicit-intent provenance, delayed factual copy timing,
  forced-path exclusion, clearing lifecycle and unchanged release authority.
- QA native gates passed EditMode `174/174` and PlayMode `82/82`, both with zero
  failed/skipped/inconclusive tests; Console was 0 logs/warnings/errors.
- Manual copy flow is not claimed because the first Game-view click failed
  externally with `windowNotFoundAtPosition`; no runtime failure was observed.

### Scope intentionally deferred

- Disabling RELEASE, general onboarding/notifications, persistence, camera/input,
  gameplay changes, content expansion and device final polish.

## Diamond Pass 14.1 — Guided Loss Closes Back to Build

Completed on 2026-09-10.

### Delivered

- A factually completed guided `RealmLost` result now says `REALM LOST — RETURN
  TO BUILD AND ADJUST DEFENCES` and emphasizes the existing RETURN TO BUILD.
- DEFEND AGAIN stays visible and functional as the unchanged secondary action;
  victory and early-terminal Retry presentation remain unchanged.
- The factual-loss regression follows one existing button invocation into one
  RealmBuild/BuildHUD, proves guide/result cleanup and verifies no duplicate
  guide-completion write or RealmProgress mutation.
- A separate test-fixture stabilization now creates its airborne Jump entity at
  the requested position before adding CharacterController, removing a stale
  `isGrounded` initialization flake without changing runtime or weakening checks.

### Verification

- Architect confirmed the completed-loss-only branch, exact copy, existing-button
  emphasis, secondary action, transition cleanup and fixture-only repair scope.
- QA native gates passed EditMode `174/174` and PlayMode `82/82`, both with zero
  failed/skipped/inconclusive tests; the full PlayMode gate includes JumpGate.
- Console after stopped smoke was 0 logs/warnings/errors. Manual loss/result flow
  is not claimed because the first Game-view click failed externally with
  `windowNotFoundAtPosition`; no runtime failure was observed.

### Scope intentionally deferred

- General result/tutorial redesign, new navigation, reward/progression or journey
  persistence changes, gameplay/balance work and device final polish.

## Diamond Pass 14.2 — Completed Loop Primes the Next Raid

Completed on 2026-09-10.

### Delivered

- RETURN TO BUILD from a factually completed canonical Sylvan Defense now starts
  the existing in-session journey Build stage immediately before RealmBuild.
- Existing BuildHUD behavior therefore exposes exact `SAVE & RAID`; one existing
  click advances to SylvanRealm with journey Stage=Raid.
- Direct/legacy Build, early Retry and Infernal paths remain inactive and retain
  their existing `SAVE & DEFEND` behavior and scene authority.
- The factual-loss regression proves the Build and Raid stages, exact action
  label, cleanup and unchanged guide-completion writes and RealmProgress credit.

### Verification

- Architect confirmed completed-result provenance, existing TryStart API use,
  unchanged BuildHUD/callback authority and inactive legacy paths.
- QA native gates passed EditMode `174/174` and PlayMode `82/82`, both with zero
  failed/skipped/inconclusive tests; Console was 0 logs/warnings/errors.
- Manual journey return is not claimed because the first Game-view click failed
  externally with `windowNotFoundAtPosition`; no runtime failure was observed.

### Scope intentionally deferred

- New navigation, persistent journey state, Build layout/validation, reward or
  progression changes, gameplay/balance work and device final polish.

## Diamond Pass 14.3 — Raid Result Copy Matches the Next Action

Completed on 2026-09-10.

### Delivered

- Journey victory and defeat results now use route-specific sentences that point
  truthfully to the existing immediate `DEFEND YOUR REALM` action.
- Direct Raid retains its existing public planning/revision copy,
  `PLAN NEXT DEFENSE` action and RealmBuild route.
- One shared formatter preserves every existing result metric and secured-reward
  line; action selection, destinations, retry/Hub and exact-once credit remain
  unchanged.
- Journey victory, factual defeat, repeated result and direct-result regressions
  now prove copy/action alignment and unchanged route/credit authority.

### Verification

- Architect confirmed factual journey-result branching, exact sentences, direct
  compatibility, shared metrics and unchanged result authority.
- QA native gates passed EditMode `174/174` and PlayMode `82/82`, both with zero
  failed/skipped/inconclusive tests; Console was 0 logs/warnings/errors.
- Manual result copy is not claimed because the first Game-view click failed
  externally with `windowNotFoundAtPosition`; no runtime failure was observed.

### Scope intentionally deferred

- General result redesign/localization, navigation, reward/progression changes,
  gameplay/balance work and device final polish.

## Diamond Pass 14.4 — In-Run Control Style Mark

Completed on 2026-09-10.

### Delivered

- Imported only the three accepted original MUI06 Contextual, Fingertap and
  Joystick sprites with byte-exact hashes, local provenance and explicit mobile
  Single-Sprite settings.
- The existing in-run selector now reuses one cached, 32 px non-raycast mark that
  switches with saved AUTO/TAP/STICK while exact text remains visible.
- Missing or throwing resources hide stale art and restore the captured centered
  text layout without changing selector footprint, click, persistence, effective
  orientation or input authority.
- EditMode covers bytes/provenance/import, mapping/cache/one-child and mixed
  fallback; PlayMode covers live cycling, ownership, terminal lifecycle and
  Hub/Build absence.

### Verification

- Architect confirmed source hashes against Modules `04b8861`, provenance,
  cached mapping, noninteraction, fallback geometry and scoped import settings.
- QA native gates passed EditMode `178/178` and PlayMode `82/82`, both with zero
  failed/skipped/inconclusive tests; Console was 0 logs/warnings/errors.
- Landscape smoke observed synchronized AUTO/TAP/STICK marks in Raid and
  Defender plus terminal hiding. Portrait and possessed-Defender entry were not
  observed because the UI state did not settle for those steps.

### Scope intentionally deferred

- Hub button decoration, selected-state art, control behavior, animation/audio,
  final-art approval and physical-device validation.

## Diamond Pass 14.5 — Hub Control Choice Marks

Completed on 2026-09-10.

### Delivered

- Existing Hub CONTEXTUAL, FINGERTAP and JOYSTICK buttons now bind explicitly to
  the three already imported MUI06 saved-style sprites.
- Each retains exact text, tint, action, pointer ownership and responsive button
  footprint; the existing selected summary remains the only preference truth.
- HudPresentation now stores text-only fallback geometry independently per label,
  so one missing/throwing sprite cannot disturb another decorated button.
- Tests cover three-button mapping/cache/fallback independence, icon reuse,
  portrait/landscape rectangles, selection-summary updates and singleton safety.

### Verification

- Architect confirmed explicit mapping, independent fallback, unchanged control
  authority and reuse of committed assets without provenance/import changes.
- QA native gates passed EditMode `178/178` and PlayMode `82/82`, both with zero
  failed/skipped/inconclusive tests.
- Game-view imagery and numeric Console counters were unavailable to QA's
  automation surface; no Hub readability claim is made. Editor log tail showed no
  warning/error/exception entries after the run.

### Scope intentionally deferred

- Selected-state art, orientation/navigation icons, control behavior, final-art
  approval, manual portrait readability and physical-device validation.

## Diamond Pass 14.6 — Grounded Jump Coyote Time

Completed on 2026-09-10.

### Delivered

- Direct-controlled characters may jump during one strict 0.10-second grace
  window after the existing CharacterController records factual grounded contact.
- Airborne spawn, repeated jump and attempts at or beyond the boundary still
  reject; root, terminal, controller swap, disabled Motor, death, disable and
  destroy cleanup cannot carry grace into a later state.
- Joystick and Fingertap continue to call the same CombatEntity jump authority;
  no input buffering, delayed execution or control-mode behavior was introduced.
- The QA menu now clears unacknowledged test-run ownership after a bounded idle
  start timeout and exposes one safe manual stale-ownership command without
  automatically retrying tests or changing Test Runner state.

### Verification

- Architect confirmed the strict timing boundary, factual Motor contact, unchanged
  jump impulse and conjunctive authority gates; `git diff --check` is clean.
- After one user-authorized controlled Editor restart repaired the stalled Unity
  compile/domain-reload pipeline, QA gates passed EditMode `178/178` and PlayMode
  `83/83`, both with zero failed/skipped/inconclusive tests.
- Editor log tail showed no error/exception entries. Manual ledge behavior is not
  claimed because QA could not safely operate the Game view.

### Scope intentionally deferred

- Jump input buffering, double/wall/charged/air jump, AI jump, jump tuning,
  animation/VFX/audio/haptics, device validation and broader movement changes.

## Diamond Pass 14.7 — Jump Takeoff and Landing Readability

Completed on 2026-09-10.

### Delivered

- A factual direct jump now gives the existing Presentation Pivot one short
  takeoff stretch and one grounded landing settle, composed with idle, movement,
  action and hit presentation.
- Every local offset remains within 0.08 m and every scale axis within 0.90–1.10
  of the captured base pose; the gameplay root, CharacterController and camera
  retain unchanged authority.
- Initialization, ordinary grounded motion, release/controller swap, root,
  terminal, death, disabled Motor, component disable/destroy and rebind cleanup
  cannot synthesize a delayed landing response.
- Deterministic EditMode coverage proves transition, composition, bounds, restore
  and rebind behavior; PlayMode proves real jump pivot isolation and cleanup.

### Verification

- Architect confirmed pivot-only ownership, strict bounds, factual transition
  gating, exact restore behavior and unchanged jump/input/gameplay authority;
  `git diff --check` is clean.
- QA final gates passed EditMode `179/179` and PlayMode `84/84`, both with zero
  failed/skipped/inconclusive tests; Editor log tail showed no error/exception.
- Manual Sylvan/possessed-Defender readability is not claimed because QA could
  not safely operate the Game view through its automation surface.

### Scope intentionally deferred

- Animator/rig/clip/root-motion integration, input buffering, jump physics,
  double/wall/charged/air jump, gameplay VFX/audio/haptics, camera/UI changes and
  physical-device performance validation.

## Diamond Pass 14.8 — One Falling Jump Buffer

Completed on 2026-09-10.

### Delivered

- A valid direct player may submit exactly one 0.08-second pending jump request
  while the existing active jump is factually descending.
- Immediate grounded/coyote jump remains first. Startup airborne, ascent and
  ledge falls without an active jump never queue; repeated or same-descent
  post-expiry presses return false without refreshing the request.
- Factual CharacterController grounding consumes a live request once and begins
  the unchanged jump state; expiry or invalid authority clears it.
- Root, terminal, controller release/swap, disabled Motor or PlayerController,
  death, entity disable and destroy cleanup cannot carry intent into a later
  state. Joystick and Fingertap ownership remain unchanged.

### Verification

- Architect found and closed the initial same-descent post-expiry requeue leak,
  then confirmed strict timing, one-use guard, factual grounding and complete
  lifecycle cleanup; `git diff --check` is clean.
- After Unity's import channel recovered without a restart, QA final gates passed
  EditMode `179/179` and PlayMode `85/85`, both with zero failed/skipped/
  inconclusive tests; Editor log tail showed no error/exception entries.
- Manual Sylvan/possessed-Defender pre-landing behavior is not claimed because QA
  could not safely operate the Game view through its automation surface.

### Scope intentionally deferred

- Double/wall/charged/air jump, hold-to-bunny-hop, buffer refresh, physics/coyote
  tuning, AI jump, action/dodge buffering, animation/VFX/audio/haptics, camera/UI
  changes and physical-device performance validation.

## Diamond Pass 15.0 — Possession Energy Urgency Pulse

Completed on 2026-09-10.

### Delivered

- The existing possession-energy meter now gives one smooth 0.24-second
  horizontal pulse between exact 1.00 and 1.06 scale on factual Warning entry and
  one new pulse on Critical entry.
- Timer tenths and fill updates within one urgency level never restart the pulse;
  explicit low-energy possession may pulse once, while ordinary controller restore
  or component re-enable cannot synthesize a new alert.
- Release, controller loss, forced depletion, death, terminal/result, disable,
  destroy and initialization restore exact identity scale and clear history.
- Existing text, thresholds, colour, fill, anchors, offsets, size, raycast/input,
  possession timing and gameplay authority remain unchanged for both realms.

### Verification

- Architect closed an exact float-duration boundary and separated true possession
  entry from ordinary controller restoration; strict same-level nonrestart and
  lifecycle assertions remain intact; `git diff --check` is clean.
- QA final gates passed EditMode `180/180` and PlayMode `87/87`, both with zero
  failed/skipped/inconclusive tests; Editor log tail showed no error/exception.
- Manual Sylvan/Infernal Warning-to-Critical readability is not claimed because
  QA could not safely operate the Game view.

### Scope intentionally deferred

- Energy balance/thresholds/duration/release behavior, repeating alerts, camera,
  controls, audio/haptics/gameplay VFX/new art, general HUD redesign, navigation,
  progression and physical-device performance validation.

## Diamond Pass 15.1 — Infernal Seam-Hardened Path Surface

Completed on 2026-09-10.

### Delivered

- Copied only the accepted original-generated MWS07 Infernal basalt RGB candidate
  from Modules commit `6138f71`; source and destination share exact SHA-256
  `c8df59807a4fe21c9a5cc27ce3f776f43f1be1a689aab0366c27fd245606da88`.
- Added unique Unity metadata and local preview provenance with the explicit
  bright-ember 1024-pixel device-periodicity caveat and no third-party source.
- Infernal routes now bind the MWS07 resource with Repeat, mipmaps, Bilinear,
  non-readable sRGB and Android 512 ASTC 6×6 import settings.
- The legacy MWS03 asset remains present but unbound; Sylvan binding, material
  cache/fallback, route geometry, renderer/collider and gameplay authority are
  unchanged.

### Verification

- Architect confirmed byte equality, RGB dimensions, unique GUIDs, provenance,
  exact resource/import mapping, independent Infernal null/throw fallback and
  unchanged Sylvan/gameplay scope; `git diff --check` is clean.
- QA Assets Refresh reported no C# or import errors; final gates passed EditMode
  `182/182` and PlayMode `87/87`, both with zero failed/skipped/inconclusive tests;
  Editor log tail showed no error/exception.
- Manual Infernal/Sylvan seam and periodicity observation is not claimed because
  QA could not safely operate the Game view.

### Scope intentionally deferred

- MWS03 deletion, boundary surfaces, normal/height/roughness/emission, shaders,
  UV redesign, second-tile/macro variation, geometry/collision/gameplay, lighting,
  camera/UI, production-art approval and device-performance claims.

## Diamond Pass 15.2 — Infernal Boundary Surface Identity

Completed on 2026-09-10.

### Delivered

- Copied only the accepted original-generated MWS08 Infernal forged-iron and
  obsidian RGB candidate from Modules commit `c4295e4`; source and destination
  share exact SHA-256
  `b8d21cdc6e1b29fae4e9586c64b034a26755b86989213fca4280106ac906745d`.
- Added unique Unity metadata and local preview provenance with no third-party
  source and the explicit crimson-network 1024-pixel device-periodicity caveat.
- Infernal boundaries now lazily load and share the MWS08 albedo with Repeat,
  mipmaps, Bilinear, non-readable sRGB and Android 512 ASTC 6×6 settings.
- Missing or throwing resource loads retain the exact existing solid Infernal
  fallback and are cached; Neutral and Sylvan resource/material behavior remains
  independent and unchanged.

### Verification

- Architect confirmed byte equality, 1024×1024 RGB dimensions, unique GUIDs,
  valid provenance, exact import settings, style-local cache/fallback behavior,
  preserved Sylvan binding and unchanged geometry/collider scope;
  `git diff --check` is clean.
- QA final native gates passed EditMode `185/185` in 0.689 seconds and PlayMode
  `87/87` in 70.294 seconds, both with zero failed/skipped/inconclusive tests.
  No files changed after the final suites.
- Manual Infernal/Sylvan texture readability, repetition and character contrast
  are not claimed because QA could not safely operate the Game view.

### Scope intentionally deferred

- Normal/height/metallic/roughness/emission maps, shaders, second-tile or macro
  variation, UV/geometry/collider/gameplay changes, final-art approval and
  physical-device performance claims.

## Diamond Pass 15.3 — Possession Arrival Impact

Completed on 2026-09-10.

### Delivered

- Every factual successful possession now starts one 0.22-second unscaled
  squash, rebound and exact settle on the same entity's existing presentation
  pivot; selection, rejection and ordinary controller restoration cannot
  synthesize or restart it.
- The response composes with the existing idle, movement, action, hit and jump
  pose, stays inside the existing offset/scale clamps and creates no new object,
  asset, renderer, material, collider or input path.
- Explicit and energy-forced release, death, terminal state, external controller
  loss, manager/motion disable or destroy and visual rebind clear the response
  while preserving the gameplay root, CharacterController, health, abilities,
  action, jump, pulse, slow beat, camera, audio and haptic authority.

### Verification

- Architect returned one missing lifecycle-coverage gap and then rejected one
  test-only false invariant: a no-ground fixture's authoritative gravity may move
  Y after a yielded frame. The final proof checks full root invariance directly
  around possession and permits only factual vertical movement afterwards;
  `git diff --check` is clean.
- QA final native gates after that correction passed EditMode `186/186` in 0.695
  seconds and PlayMode `88/88` in 70.698 seconds, both with zero failed/skipped/
  inconclusive tests. No files changed after the final suites.
- Manual Sylvan silhouette readability is not claimed because QA could not safely
  observe or operate the Game view.

### Scope intentionally deferred

- Pose amplitude tuning without device evidence, new animation clips/Animator or
  root motion, changes to pulse/slow beat/camera/audio/haptic, new VFX/assets,
  gameplay timing/authority and physical-device performance claims.

## Diamond Pass 15.4 — Infernal Courtyard Floor Surface

Completed on 2026-09-10.

### Delivered

- Copied only the accepted original-generated MWS11 Infernal courtyard albedo and
  restrained normal from Modules commit `2f5457f`; destination SHA-256 values are
  `666ce002ccf7dc577264eef1062e0d100fab2cb5195058398186427e2269a52c`
  and `731339751ae4c004379cbba1c424e87efc33f390e9ef5708071b5e1076bc46f7`.
- The factual `Volcanic Floor` now uses the pair atomically at normal strength
  `0.30`, while all four `Basalt Causeway Plate` children retain MWS07 and the
  Infernal boundary retains MWS08.
- One shared floor material is retained, with renderer-local property blocks
  providing square world-space tiling at four world units per tile so differently
  sized Infernal roots cannot overwrite each other's scale.
- Missing or throwing albedo/normal loads preserve and cache the exact existing
  solid floor fallback; Sylvan surfaces, geometry, colliders, hierarchy, AI, fog
  and gameplay authority remain unchanged.

### Verification

- Architect confirmed byte identity, 1024×1024 RGB dimensions, unique GUIDs,
  valid provenance, exactly two runtime PNGs, Android 512 ASTC 6×6 import, atomic
  cache/fallback and route/floor isolation; `git diff --check` is clean.
- Static review caught and closed shared-material tiling leakage before QA. QA then
  exposed one test-only trigger assumption; the corrected test preserves each
  collider's factual original trigger state.
- Final QA gates passed EditMode `190/190` in 0.758 seconds and PlayMode `89/89`
  in 70.132 seconds, with zero failed/skipped/inconclusive tests and no Editor-log
  errors or exceptions after the frozen candidate.
- Manual Infernal portrait/landscape appearance is not claimed because QA could
  not safely observe the Game view; physical-device scale, repetition and contrast
  remain user-owned review.

### Scope intentionally deferred

- Emission, height, roughness, metallic, macro variation, second tiles, shaders,
  lighting redesign, route/boundary replacement, geometry/collider/gameplay
  changes, production-art approval and physical-device performance claims.

## Diamond Pass 15.5 — Defeat Presentation Keeps Gameplay Root Authoritative

Completed on 2026-09-10.

### Delivered

- Factual `Health.Died` no longer multiplies the authoritative `CombatEntity`
  root scale. Root transform and CharacterController geometry remain unchanged,
  while the existing death path still disables the Motor.
- Death starts one non-restarting 0.24-second unscaled fall/squash on the existing
  `Presentation Pivot`, then holds an exact stable defeated pose.
- The response cancels transient hit, action, jump and possession-arrival motion
  before taking presentation ownership; rebind, component disable and destruction
  restore the old pivot and cannot strand stale visual state.
- Possession forced return, death callbacks, result/reward idempotence, camera and
  controller authority, entity lifetime and combat timing remain unchanged.

### Verification

- Architect reviewed the single factual runtime caller, bounded/stable sampling,
  lifecycle composition and root/collider invariants; the new test companion GUID
  is unique and `git diff --check` is clean.
- QA exposed one invalid EditMode assumption about runtime `OnDisable`; coverage
  was corrected to keep deterministic explicit cleanup in EditMode and prove the
  real disable callback in PlayMode without adding `ExecuteAlways` behavior.
- Final QA gates passed EditMode `192/192` in 0.767 seconds and PlayMode `89/89`
  in 70.367 seconds, with zero failed/skipped/inconclusive tests and no Editor-log
  errors or exceptions after the frozen candidate.
- Manual death-pose readability is not claimed because QA could not safely observe
  the Game view; physical-device feel remains user-owned.

### Scope intentionally deferred

- Revive/ragdoll/corpse lifetime, death VFX/audio/haptics/loot/camera/UI, balance,
  collider disabling, destruction, Animator/rig/clips/root motion, new assets and
  result-flow changes.

## Diamond Pass 15.6 — Restrained Route Normal Pair Integration

Completed on 2026-09-10.

### Delivered

- Copied only the accepted original-generated MWS12 Sylvan and Infernal RGB normal
  candidates from Modules commit `e4d1358`; destination SHA-256 values are
  `aec5888a187057ba19b5a41c98c79100719f01a50aec8c0a7eeb25d8ea514dae`
  and `34a99a389b7ffa28b37b81e7b02460a5af7646358726b27d347280bcc23aa816`.
- Each normal is paired only with its existing MWS07 realm route albedo at restrained
  `_BumpScale` `0.20`; Sylvan floors/nodes, the MWS11 Infernal courtyard and both
  MWS08 boundary families keep their own presentation.
- Each realm resolves and caches its albedo+normal route pair atomically and
  independently. A missing or throwing member preserves that route family's exact
  prior solid-colour fallback without adding per-route material instances or
  per-frame work.
- Import settings are supplied linear NormalMap, Repeat, mipmaps, Bilinear,
  non-readable and Android 512 ASTC 6×6; local provenance records the visible
  periodicity device caveat and excludes the two evidence sheets from Resources.

### Verification

- Architect confirmed source/destination byte identity, exactly two runtime PNGs,
  unique valid 32-hex GUIDs, import/provenance truth, cache/fallback independence,
  surface isolation and unchanged geometry/collider/gameplay scope;
  `git diff --check` is clean.
- QA first exposed two malformed 31-character GUIDs, then two test-contract defects
  (exact floating transform equality and a brittle provenance phrase). The final
  frozen candidate corrects only those metadata/test defects.
- Final QA gates passed EditMode `197/197` in 0.781 seconds and PlayMode `90/90`
  in 70.62 seconds, with zero failed/skipped/inconclusive tests and no new log
  errors or exceptions.
- Manual route-relief readability is not claimed because the Game view was not
  safely observable; physical-device strength and repetition remain user-owned.

### Scope intentionally deferred

- Albedo edits, floor/node/boundary replacement, height/roughness/metallic/emission,
  shaders/lighting redesign, second tiles or macro variation, geometry/UV/collider/
  gameplay changes, final-art approval and physical-device performance claims.

## Diamond Pass 15.8 — Modular Procedural Blood Knight Motion Pilot

Completed on 2026-09-10.

### Delivered

- Installed the accepted local `character-motion-profiles` and
  `character-procedural-motion` packages and their Editor tests.
- Added one thin Core adapter that binds only the active 3DRT Blood Knight's six
  exact upper-arm, thigh and calf descendants and maps factual movement, action,
  jump, damage and death state into immutable Module input.
- The Module driver changes only cached child-bone local rotations. The gameplay
  root, CharacterController, Presentation Pivot, Base Body, controller, input,
  camera and combat timing remain unchanged; missing bones fail closed.
- Clear, reassemble, disable, re-enable and destruction restore cached bone
  baselines and event ownership without hierarchy scans or per-frame allocation.

### Verification

- Architect reviewed the package boundary, binding order, fail-closed behavior,
  lifecycle cleanup and root/pivot/body/collider invariants; `git diff --check` is
  clean.
- QA exposed and closed two Module test compilation mistakes, one invalid
  EditMode MonoBehaviour-lifecycle assumption and one exact-float test assertion.
- Final QA gates passed EditMode `244/244` in 0.866 seconds and PlayMode `91/91`
  in 71.795 seconds, with zero failed/skipped/inconclusive tests. The post-gate
  log tail contained no new error or exception.
- Manual portrait/landscape motion quality is not claimed because QA could not
  safely observe or control the Game view; Android device tuning remains user-owned.

### Scope intentionally deferred

- Final bone-axis/amplitude/cadence tuning, clip-driven Animator, Humanoid retarget,
  root motion, IK, ragdoll/Rigidbody physics, broad skeleton support, model or
  material replacement, camera/input/combat changes and physical-device approval.

## Diamond Pass 15.11 — Blood Knight Readable Stride and Staged Jump

Completed on 2026-09-10.

### Delivered

- Modules commits `91e0323` and `501c812` preserve the exact Guardian Ent Tree01
  source/CC0 intake evidence and evolve the reusable procedural humanoid driver.
- Blood Knight locomotion now uses an explicit mobile-readable forward/back sample:
  opposing upper arms `±56°`, thighs `∓50°` and calves `±26°`; the compatibility
  preset retains its exact prior values and axes.
- One shared, idempotent presentation-only jump timeline drives the existing pivot
  and six bound bones: bilateral deep crouch, 0.30-second straighten into one-leg
  push, hold through 0.40 seconds, 0.40-second push-to-fall blend, then a 0.56-second
  fall-to-compression-to-baseline landing.
- Early factual grounding finishes only the remaining visual fall blend before the
  landing response. It never delays or changes authoritative grounding, jump input,
  `CharacterJumpState`, entity root, `CharacterController`, combat or camera.
- Same-timestamp consumers, sparse frames, exact endpoints, controller loss,
  disable, clear and rebind produce deterministic cleanup without pose snapping.

### Verification

- Architect and an independent reviewer closed a landing catch-up error, an exact
  endpoint/idempotence error, pivot takeoff-to-fall popping and early-ground bone
  discontinuity before acceptance; `git diff --check` is clean.
- QA's first EditMode run exposed the sparse-frame landing bug (`250/251`); the
  bounded correction was made with GPT-6 Astra and no gameplay timing change.
- Final QA gates passed EditMode `251/251` in 0.895 seconds and PlayMode `92/92`
  in 73.123 seconds, with zero failed/skipped/inconclusive tests and no post-gate
  errors or exceptions.
- Android was intentionally not exported. Subjective motion quality remains the
  user's direct Unity Game-view acceptance.

### Scope intentionally deferred

- Animator clips, root motion, IK, pelvis translation, ragdoll/Rigidbody physics,
  broad skeleton retargeting and final physical-device motion approval.

## Diamond Pass 15.12 — Factual Character Motion Dynamics

Completed on 2026-09-10.

### Delivered

- One per-entity presentation state now advances the Blood Knight gait only from
  factual horizontal root displacement. Standing freezes the phase, filtered limb
  weight settles to exact neutral, and a later start cannot inherit an arbitrary
  global-clock pose.
- The same idempotent sample feeds both the six bound bones and Presentation Pivot,
  independent of their `LateUpdate` order. Factual acceleration/braking adds at
  most `3°` of pitch and factual yaw adds at most `6°` of turn lean.
- Locomotion accents yield to hit, action and the accepted staged jump. Controller
  changes, rooting, terminal state, death, disable, clear and rebind reset the
  presentation without moving the gameplay root, `CharacterController`, body,
  camera, input or combat authority.
- Idle breathing now translates without changing neutral scale, so takeoff,
  falling, landing and idle meet at continuous scale boundaries. The real jump
  flow test observes factual nonzero phase progress instead of assuming that the
  first coroutine frame already has a nonzero pose.

### Verification

- QA first caught two missing namespace imports; the minimal compile correction
  changed no behavior. A later `91/92` PlayMode run exposed the zero-progress
  jump-test assumption, which was corrected without epsilon motion or gameplay
  timing changes.
- Final focused checks passed: the real jump visual-flow test and the complete
  `18/18` CharacterProceduralMotion EditMode group.
- Final QA gates passed EditMode `255/255` in 0.847 seconds and PlayMode `92/92`
  in 73.99 seconds, with no post-gate errors or exceptions.
- Android was intentionally not exported and no manual smoke is claimed. Final
  motion feel remains the user's direct Unity Game-view acceptance.

### Scope intentionally deferred

- Continuous directional attack and damage-reaction timelines, additional
  spine/pelvis/foot bindings, clip-driven animation, IK, root motion and ragdoll.

## Diamond Pass 15.13 — Sagittal Stride and Directional Combat Motion

Completed on 2026-09-11.

### Delivered

- Modules commit `7aae5d6` replaces Blood Knight's fragile FBX-local motion axis
  with six cached semantic hinges derived from the owned Presentation Pivot.
  Arms and legs now move in the character's forward/back sagittal plane, with
  opposing left/right gait signs; the same plane is used by jump and combat poses.
- Legacy/default/custom module callers retain their exact local-axis behavior.
  Missing, degenerate, mirrored or foreign semantic bindings fail closed and
  restore the exact cached bone baselines.
- Accepted actions publish passive, immutable presentation facts without changing
  cooldowns, damage, targeting or combat phase timing. A per-entity visual clock
  preserves same-frame Impact/Recovery and supplies bounded windup, impact and
  follow-through poses.
- Factual damage produces a short directional hit response from a source snapshot,
  then point fallback, then neutral fallback. Presentation priority remains
  `Death > Hit > Attack > Jump > Locomotion > Idle` and clears across controller,
  possession, terminal, death, disable, rebind and teardown boundaries.
- Only the six exact descendant bones and existing Presentation Pivot receive
  bounded visual motion; the gameplay root, CharacterController, Base Body,
  movement, camera, input and combat authority remain unchanged.

### Verification

- Independent static review found no blocker; `git diff --check` is clean in both
  repositories.
- Focused QA passed `CharacterCombatPresentationTests` `10/10`, corrected
  `CharacterProceduralMotionAdapterTests` `9/9`, and the actual imported Blood
  Knight PlayMode path `1/1`.
- Final QA gates passed EditMode `276/276` in 0.909 seconds and PlayMode `92/92`
  in 73.663 seconds, with no Console or Editor-log errors/exceptions.
- Android export was intentionally omitted. Actual knee bend, weapon-side motion
  and overall stride feel remain the user's direct Unity Game-view acceptance.

### Scope intentionally deferred

- Spine, pelvis and foot bindings; foot planting/IK; Animator clips, root motion,
  ragdoll/Rigidbody physics and broad skeleton retargeting.

## Diamond Pass 15.14 — Blood Knight Upper-Torso Counterweight

Completed on 2026-09-11.

### Delivered

- Modules commit `e02e9a4` adds a backward-compatible optional upper-torso bind to
  the reusable procedural humanoid driver. Legacy six-bone callers and outputs
  remain exact.
- The actual Blood Knight opts into its exact weighted chain
  `Bip01 → Bip01 Pelvis → Bip01 Spine → Bip01 Spine1`; invalid, duplicate,
  reflected, unsupported or wrongly parented optional torso evidence disables only
  this extension and preserves accepted limb motion.
- `Spine1` receives caller-clocked, baseline-relative sagittal counterweight only:
  walk at most `2°`, attack at most `8°`, and hit response at most `5°`.
  Jump and death add no torso accent.
- The optional seventh transform participates in existing crossfade, clear,
  rebind, disable/enable, terminal and death restoration without moving or scaling
  the gameplay root, Base Body, Presentation Pivot or CharacterController.

### Verification

- Independent Module and Core static reviews found no blocker; both repositories
  pass `git diff --check`.
- Focused QA passed the complete Module driver group `30/30`, main adapter group
  `17/17`, and the actual imported/skin-weighted Blood Knight PlayMode path `1/1`.
- Final QA gates passed EditMode `288/288` in 0.967 seconds and PlayMode `92/92`
  in 73.065 seconds. The post-gate Editor log contained no errors/exceptions.
- Android export was intentionally omitted. Torso strength, neck/armor clipping,
  weapon follow and overall gait/combat feel remain the user's Game-view approval.

### Scope intentionally deferred

- Pelvis/foot/toe motion, foot planting, IK, authored clips, root motion,
  ragdoll/Rigidbody physics and generic retargeting.

## Diamond Pass 15.15 — Guardian Ent Tree01 Visual Pilot

Completed on 2026-09-11; included with this project commit.

### Delivered

- Modules commit `a075494` records the exact locally supplied Tennessippi Free
  Treant Pack archive, Tree01 FBX and selected light texture hashes, official
  creator-page CC0 evidence, offline structure and explicit prototype-budget
  exception.
- Imported only the exact Tree01 FBX plus its light albedo, normal and mask into a
  provenance-recorded third-party folder. The pilot material samples only the
  1024 mobile light albedo; normal and mask remain retained but unbound.
- Added an explicit, idempotent QA-owned intake command that validates hashes and
  import settings before creating one project-owned URP material and one sanitized
  static visual prefab in an isolated preview scene.
- Guardian Ent's cached LargeCreature recipe now binds the resource when present,
  suppresses primitive overlays, and preserves the exact primitive recipe when it
  is unavailable. A fallback cached before the builder can promote in the same
  Editor domain without replacing live entities or requiring a restart.
- The prefab contains only transforms and one factual `MeshFilter`/`MeshRenderer`
  pair. It adds no Collider, Rigidbody, Animator, script, camera, light, root
  motion or gameplay authority; `CombatEntity`, `CharacterController`, possession,
  health, cooldowns and cultivation remain on the same root.

### Verification

- Exact source bytes and all four SHA-256 values match the accepted intake;
  `git diff --check` is clean.
- QA's explicit builder succeeded with one static renderer and `5,438` triangles.
  Two pre-gate attempts correctly exposed a stale assembly caused by an unsupported
  test attribute; after the compatibility fix and a verified Assets Refresh, the
  new assembly was used.
- Final QA gates passed EditMode `300/300` and PlayMode `93/93`, with no new C#
  errors, exceptions or assertions in the post-gate log.
- A manual Defender view showed the light Tree01 Ent without a pink material or
  obvious clipping. Cultivation, possession/release and true portrait/landscape
  interaction remain honestly unobserved because the automation could not select
  the Ent and the Game view remained `1280×720`.

### Scope intentionally deferred

- Production LOD0/1/2 (`4000/2000/800`), cleaned four-weight skinning, clip/rig
  validation, animation, normal/mask use, root motion, IK, gameplay physics and
  final physical-device visual/performance approval.

## Diamond Pass 15.16A — Guardian Ent Generic-Rig Feasibility Probe

Completed on 2026-09-11; included with this project commit.

### Delivered

- Modules commits `bda0b86`, `541a7a4` and `8978fe0` add engine-neutral,
  fail-closed Tree01 production-readiness and reusable LargeCreature-motion
  evidence contracts without weakening the existing nine-key motion profile.
- Added one Editor-only exact-source Generic import probe with its own assemblies,
  build guard and tests. It is outside Resources, build scenes and every runtime
  recipe, and it protects the accepted static FBX, prefab, material and Guardian
  recipe hashes before and after generation.
- The explicit QA-owned builder produced one isolated SkinnedMeshRenderer/Animator
  preview, a deterministic text report and no controller, root motion, Animation
  Events, physics, colliders, camera, light, gameplay script or runtime binding.
- Generated evidence records all source clips and 21 samples for selected Idle,
  Run, Attack_1 and Death1 takes. The accepted static Guardian visual remains the
  runtime path and exact primitive fallback remains unchanged.

### Verification

- Generic evidence is `MEASURED`: 3,010 vertices, 5,438 triangles, 31 bones and
  bindposes, maximum four weights, zero unweighted vertices and zero protected
  root/pivot/fit drift.
- Idle, Run, Attack_1 and Death1 all changed the centred mesh while remaining
  sampled in place. Idle and Run returned to identical sampled endpoints; Death1
  retained a distinct final pose.
- After one Unity/NUnit assertion compatibility correction, QA's full EditMode gate
  passed `320/320`; probe tests passed `11/11`, LargeCreature readiness passed
  `9/9`, and the final run span contained no errors. PlayMode was intentionally not
  repeated because no runtime code, scene, recipe or gameplay asset changed.

### Scope intentionally deferred

- Runtime animation binding, visible loop/foot-contact and Death1 held-pose review,
  final motion feel, root motion, IK, ragdoll/physics, production LODs, source-level
  weight cleanup and physical-device performance.

## Diamond Pass 15.16B — Modular Guardian Ent Motion Integration

Completed on 2026-09-11; included with this project commit.

### Delivered

- Added a separate exact-source Generic runtime Tree01 visual, four sanitized
  skeleton-only clips and an explicit Resources binding. The accepted static Ent
  prefab/source remains unchanged and is still the first fallback before the
  existing primitive recipe.
- Added one reusable `LargeCreatureMotionAdapter` that maps factual death, bounded
  hit response, accepted attack phases, measured movement and idle state to the
  visual skeleton only. The same `CombatEntity`, root `CharacterController`,
  Presentation Pivot, health, cultivation, cooldowns and controller authority
  remain authoritative.
- Root motion, controllers, Animation Events and root/motion curves fail closed.
  Unity's non-persistent `Animator.fireEvents` flag is enforced on every runtime
  bind/rebuild before playback rather than trusted as prefab data.
- Death plays once and holds its final pose; controller changes, terminal state,
  disable, rebuild and teardown clear the owned graph, subscriptions and bone
  baselines deterministically.
- The intake builder now validates the saved and reloaded binding, while its
  diagnostic contract reports the first invalid clip, hierarchy, avatar, skin or
  bone fact instead of returning an opaque failure.

### Verification

- QA rebuilt the motion pilot from the exact accepted source; the serialized
  binding retained `m_Name: GuardianEntTree01Motion` with no blank-name warning.
- The final refreshed test assembly passed EditMode `328/328`, zero failed,
  skipped or inconclusive, in 1.756 s (job
  `469474bf-a790-4ad4-a99e-e2d2dd327eb3`).
- The final PlayMode gate passed `94/94`, zero failed, skipped or inconclusive,
  in 73.422 s (job `fea6d58f-f387-4d63-b6f7-f84665422f48`). The actual animated
  Guardian same-entity possession/cultivation/root invariants and retained static
  fallback both passed.
- The final run contained no new compile error or actionable exception;
  `git diff --check` is clean.

### Scope intentionally deferred

- Manual portrait/landscape clip feel, foot contact and Death1 aesthetics remain
  unobserved because QA could not safely access the Game view.
- Production LOD0/1/2 (`4000/2000/800`), source-level weight cleanup, IK,
  ragdoll/physics-authoritative limbs, retargeting and physical-device performance.

## Diamond Pass 16.0 — Factual Raid Loot Feedback

Completed on 2026-09-11; included with this project commit.

### Delivered

- `RaidManager` now emits one immutable sequenced receipt only after each existing
  exact-once room, enemy or Realm Core reward mutation. Each receipt carries the
  real delta, resulting totals, source category and explicitly supplied position.
- Added one existing-HUD-owned, non-raycast FIFO cue that keeps simultaneous
  rewards readable without adding a Canvas, input target, scene scan or economy
  authority. Duplicate sequence receipts cannot replay presentation.
- Room re-entry, duplicate death, terminal/result flow, HUD disable, retry and
  teardown clear or suppress the cue without changing reward amounts, defeat
  halving, Rare Material rules, persistence or scene routing.
- Portrait and landscape use fixed responsive bounds that remain separate from
  encounter truth, ability controls and the virtual joystick.

### Verification

- QA verified fresh Runtime/EditMode/PlayMode assemblies after one explicit
  Assets Refresh; every DLL timestamp was newer than the changed source/tests.
- Final EditMode passed `333/333`, zero failed/skipped/inconclusive, in 1.797 s
  (job `9f25172f-2f97-44b1-8792-dba320e774b0`).
- Final PlayMode passed `94/94`, zero failed/skipped/inconclusive, in 77.193 s
  (job `d617fb9d-ad2c-494e-8e69-cbc3b1ec1a69`).
- The post-gate log contained no compiler error or exception; `git diff --check`
  is clean.

### Scope intentionally deferred

- The Sylvan portrait/landscape room-to-enemy-to-Core visual smoke remains
  unobserved because QA could not safely access Game-view content.
- No pickup object, inventory, drop table, rarity roll, new currency, economy
  rebalance, VFX package, audio, haptics or platform export was added.

## Diamond Pass 16.1 — Earned Cultivation Affordance

Completed on 2026-09-11; included with this project commit.

### Delivered

- The existing Guardian Ent cultivation button now maps one loaded authoritative
  Realm Progress snapshot to exact `MISSING`, `READY` or `CAPPED` presentation.
- `READY` requires the existing 100 Gold plus one Rare Material cost below the
  existing rank-three cap. Every missing state reports the exact shortage, while
  capped progress cannot remain interactable.
- A successful existing purchase immediately refreshes Realm Stores, rank, copy,
  interactability and one fixed ready tint. Failed repeat purchases leave progress
  and presentation truthful.
- No new UI object, economy rule, currency, persistence schema, defense stat,
  navigation path or reward dependency was introduced.

### Verification

- QA verified that Runtime, EditMode and PlayMode assemblies were newer than all
  three changed source/test files before running the final gates.
- Final EditMode passed `334/334`, zero failed/skipped/inconclusive, in 1.75 s
  (job `3406bdea-d1c4-4915-a41c-9e098ef0d887`).
- Final PlayMode passed `94/94`, zero failed/skipped/inconclusive, in 76.996 s
  (job `ff90d3cc-f185-41a0-a079-610b21db8a61`,
  2026-09-11 00:18:44Z–00:20:01Z).
- The post-gate scan found no compiler error, runtime exception or test failure;
  `git diff --check` is clean.

### Scope intentionally deferred

- Manual `READY`/`MISSING`/`CAPPED` and portrait/landscape Build readability
  remains unobserved because QA could not safely access the Game view.
- Repeated two-AudioListener warnings are an existing test-scene noise defect and
  are queued separately; 16.1 did not add or alter any AudioListener.

## Diamond Pass 16.2 — Factual Defense Result Debrief

Completed on 2026-09-11; included with this project commit.

### Delivered

- `DefenseManager` freezes one immutable result fact on the first authoritative
  victory or loss transition before possession/controller cleanup can alter the
  observed battle state.
- The fact contains only outcome, elapsed duration, invader current/maximum health
  and factual Realm Core progress. Reentrant or later death/Core callbacks cannot
  overwrite or republish it.
- The existing result label appends deterministic victory/loss facts while keeping
  the first-minute guide suffix and all three existing result actions unchanged.
- No score, history, causal diagnosis, reward, economy, combat rule, persistence
  record, UI object or Canvas was added.

### Verification

- QA verified fresh Runtime, EditMode and PlayMode assemblies before the gates.
- Final EditMode passed `337/337`, zero failed/skipped/inconclusive, in 1.764 s
  (job `6c78cc66-913b-450e-a27b-f7c704705146`).
- Final PlayMode passed `94/94`, zero failed/skipped/inconclusive, in 77.388 s
  (job `41b91408-d77b-4944-934f-dfb1467231ec`,
  2026-09-11 00:28:54Z–00:30:11Z).
- The post-gate scan found no compiler error, runtime exception or test failure;
  `git diff --check` is clean.

### Scope intentionally deferred

- Manual result copy/action readability in portrait and landscape remains
  unobserved because QA could not safely access the Game view.
- The unrelated repeated two-AudioListener warning persisted with 669 entries in
  the final PlayMode span and requires a separate root-cause gate.

## QA Quality Gate 02 — AudioListener Fixture Ownership

Completed on 2026-09-11; included with this project commit.

### Delivered

- Read-only evidence proved the duplicate-listener flood was isolated to one
  PlayMode fixture, not a production bootstrap or scene lifecycle defect.
- `RaidEncounterCueFlowTests` now records the entering scene's listener count
  before constructing fixtures and creates its non-spatial camera without an
  unnecessary second `AudioListener`.
- The fixture explicitly requires at most one entering listener and proves the
  count is unchanged after HUD creation and cleanup. Runtime camera/audio code,
  scenes and ProjectSettings remain untouched.

### Verification

- QA verified the refreshed PlayMode assembly was newer than the changed test.
- The single final PlayMode gate passed `94/94`, zero
  failed/skipped/inconclusive, in 77.049 s (job
  `1926b3cf-ecf8-4fe5-91b9-3683478ac9d6`,
  2026-09-11 00:38:37Z–00:39:54Z).
- Duplicate-listener warning count had delta `0` across the final run; the
  post-gate scan found no compiler error or exception. EditMode was intentionally
  not repeated because no runtime or EditMode source changed.

## Diamond Pass 16.3 — Earned Cultivation Result Handoff

Completed on 2026-09-11; included with this project commit.

### Delivered

- The first authoritative Sylvan terminal result takes one Realm Progress snapshot
  and reuses the existing cultivation affordance truth.
- Only a frozen `READY` state appends the factual instruction to return to Build
  and strengthen the Guardian Ent. Missing, capped, malformed fallback and every
  Infernal result retain their existing copy.
- Later terminal callbacks, orientation refresh and PlayerPrefs mutation cannot
  duplicate or rewrite the frozen result. The first-minute suffix, all three
  result actions, stores and explicit purchase authority remain unchanged.
- The existing result Text uses a bounded ready-only font/layout adjustment, and
  PlayMode coverage measures natural text height rather than only rectangle overlap.

### Verification

- QA verified fresh Runtime, EditMode and PlayMode assemblies before final gates.
- Final EditMode passed `339/339`, zero failed/skipped/inconclusive, in 1.791 s
  (job `a4f5027a-aa2b-4142-a8cd-63b4ef9de2a5`).
- Final PlayMode passed `94/94`, zero failed/skipped/inconclusive, in 77.623 s
  (job `2dcb775d-97e7-48b0-b947-b32e0d2f6160`,
  2026-09-11 00:53:54Z–00:55:12Z).
- No text-fit assertion, compiler error, exception or new duplicate-listener warning
  occurred; `git diff --check` is clean.

### Scope intentionally deferred

- Manual Sylvan result → Return to Build → Ready → explicit cultivate readability
  in portrait/landscape remains unobserved because QA could not safely access the
  Game view.

## Diamond Pass 16.4 — Factual Incoming-Attack Cue

Completed on 2026-09-11; included with this project commit.

### Delivered

- The already tracked eligible hostile now enters one exact incoming state only
  from its accepted `CombatPresentationFact` Windup against the current living
  direct-controlled entity.
- State is keyed by attacker plus ActionId and reuses the existing singular,
  non-raycast threat plate/edge tab for `INCOMING` copy. Impact/end and every
  authority, target, distance, death, terminal, transition, rebind and teardown
  boundary clear or revert it.
- Generic `ATTACKER`/`ATTACKING`, recent-damage urgency, safe-area dimensions,
  camera focus, player movement, targeting and combat timing remain authoritative
  and unchanged.
- CreatureBrain target loss now hides the cue and clears focus synchronously while
  preserving same-threat one-shot pulse history until bounded next-frame stale
  cleanup; no recursive intent reconciliation is possible.

### Verification

- QA verified fresh Runtime and PlayMode assemblies after the final correction.
- Final EditMode passed `339/339`, zero failed/skipped/inconclusive, in 1.779 s
  (job `e4ee97ef-c7cf-46eb-9fcd-2cf203bbbecb`).
- Final PlayMode passed `96/96`, zero failed/skipped/inconclusive, in 77.821 s
  (job `ccac54b1-528c-4923-bef2-d3b31610f51d`,
  2026-09-11 01:31:21Z–01:32:38Z).
- Retarget synchronous hide, pulse preservation, accepted-windup fallback and
  RaidInvader cleanup all passed. No recursion, NRE, compiler/runtime error or new
  duplicate-listener warning occurred; `git diff --check` is clean.

### Scope intentionally deferred

- Manual Sylvan possession and possessed-defense incoming-cue readability in both
  orientations remains unobserved because QA could not safely access the Game view.

## Diamond Pass 16.5 — Truthful Successful-Dodge Confirmation

Completed on 2026-09-11; included with this project commit.

### Delivered

- `Health.TakeDamage` now reports whether its existing damage path was applied;
  dead or immunity-rejected hits return false without changing health or publishing
  damage/change/death events.
- Ability overlap creates damage text, hit reaction, bounded knockback and attacker
  impact only for an applied hit. Existing armor, values and event ordering remain
  unchanged.
- A factual rejected hit against the living direct player during existing dodge
  immunity shows one camera-facing, collider-free world-space `DODGED` marker for
  0.45 seconds. It deduplicates and clears on every authority/lifecycle boundary.
- AI, traps, failed dodge, ordinary misses and nonimmune targets cannot invent the
  confirmation; all dodge and combat timings remain unchanged.

### Verification

- QA verified fresh Runtime, EditMode and PlayMode assemblies before final gates.
- Final EditMode passed `340/340`, zero failed/skipped/inconclusive, in 1.787 s
  (job `2fab33c0-ba22-44bb-93ed-ba1f1fbdac6c`).
- Final PlayMode passed `97/97`, zero failed/skipped/inconclusive, in 78.675 s
  (job `a11756a0-e6a2-434e-b7c0-f2ce35d90c35`,
  2026-09-11 01:44:18Z–01:45:37Z).
- Dodge coverage passed `4/4`, including truthful rejection, expiry, later applied
  hit and cleanup. No stale marker, compiler/runtime error or new duplicate-listener
  warning occurred; `git diff --check` is clean.

### Scope intentionally deferred

- Manual incoming → dodge confirmation feel in Sylvan and possessed Defense
  remains unobserved because QA could not safely access the Game view.

## Diamond Pass 16.6 — Direct-Control Critical Health Readability

Completed on 2026-09-11; included with this project commit.

### Delivered

- A deterministic presentation mapper marks valid living direct-control health at
  or below exactly 25% with compact `LOW HP — <NAME> <CURRENT>/<MAXIMUM>` copy and
  one fixed high-contrast tint.
- Raid and possessed-defense reuse only their existing health labels. Keeper view,
  invader health and every neutral state retain their factual existing copy and
  white tint; no Canvas, label, gameplay event or authority was added.
- Health change, possession, release, controller loss, death, terminal state,
  disable and teardown restore the exact neutral presentation. HUD event
  subscriptions are symmetric and repeated frame refreshes avoid redundant text
  and colour writes.

### Verification

- QA verified fresh Runtime, EditMode and PlayMode assemblies.
- Final EditMode passed `341/341`, zero failed/skipped/inconclusive, in 1.79 s
  (job `b7830092-f21b-4d35-861a-d6fb1e162bd1`).
- After one test-timing correction that left runtime unchanged, final PlayMode
  passed `99/99`, zero failed/skipped/inconclusive, in 79.233 s (job
  `7acfbab8-511a-4e1f-9f9c-1f72aeda4f18`,
  2026-09-11 02:03:02Z–02:04:22Z).
- No compiler/runtime error, exception or new duplicate-`AudioListener` warning
  occurred; `git diff --check` is clean.

### Scope intentionally deferred

- Manual raid and possessed-defense LOW HP readability in portrait/landscape
  remains unobserved because QA could not safely access the Game view.

## Module Pass MMP09 — Explicit Procedural Humanoid Tuning Catalogue

Completed on 2026-09-11 in Modules commit `a9f65dc`.

### Delivered

- An immutable, explicit provider/catalogue boundary maps stable profile IDs to
  existing procedural humanoid tuning objects without discovery or automatic Core
  application.
- Catalogue construction snapshots caller inputs, uses ordinal deterministic order
  and exact lookup, and fails closed with structured issues for invalid, duplicate,
  null or unreadable providers and profiles.
- The starter provider exposes Compatibility and Blood Knight device-readable
  profiles while preserving the exact existing tuning object identity and all
  accepted gait, jump, attack and hit values.

### Verification

- QA verified a fresh Modules/runtime/EditMode assembly in the main Unity host.
- All five new catalogue tests were discovered inside the final EditMode gate;
  that gate passed `347/347` with zero failed/skipped/inconclusive tests.
- No compiler/runtime error, exception or new duplicate-`AudioListener` warning
  occurred. The package remains passive and unintegrated into Core selection.

## Diamond Pass 16.7 — Factual Creature-Return Receipt

Completed on 2026-09-11; included with this project commit.

### Delivered

- A successful explicit release now confirms, through the existing bounded notice,
  that the named living creature resumed defense and shows its unchanged factual
  current/maximum HP.
- The receipt is emitted only after the same entity restores its active
  `CreatureBrain`. Missing/inactive AI, invalid health/name, terminal state and
  repeated/no-possession release retain the exact existing fallback or silence.
- Defense victory and Realm loss now classify their automatic possession return as
  forced/system-owned, so they cannot emit a false resumed-defense claim. Existing
  energy and death forced-release copy remains unchanged.
- No UI object, timer, gameplay event, HP, AI, possession, camera or combat timing
  was added or changed.

### Verification

- QA verified fresh main and Modules Runtime/EditMode/PlayMode assemblies.
- Final EditMode passed `347/347`, zero failed/skipped/inconclusive, in 1.777 s
  (job `66c30102-7c75-4d8a-955c-fbc780ecce57`).
- Final PlayMode passed `101/101`, zero failed/skipped/inconclusive, in 78.955 s
  (job `686920b1-006f-4b4c-9d48-e615b04aa0a7`,
  2026-09-11 02:17:14Z–02:18:33Z).
- Win/loss, death, energy depletion, no-AI, repeat release, same-entity unchanged-HP
  and orientation coverage passed. No compiler/runtime error, exception or new
  duplicate-listener warning occurred; `git diff --check` is clean.

### Scope intentionally deferred

- Manual portrait/landscape receipt readability and autonomous-resume feel remain
  unobserved because QA could not safely access the Game view.

## Module Pass MMP10 — Procedural Humanoid Tuning Assignment

Completed on 2026-09-11 in Modules commit `58189a8`.

### Delivered

- An immutable explicit character/recipe assignment names one preferred and one
  fallback profile from the accepted MMP09 tuning catalogue.
- The passive resolver returns deterministic `Preferred`, `Fallback` or structured
  `Rejected` results with exact case-sensitive stable IDs. It never discovers,
  mutates or applies tuning and gives no scene or gameplay authority to Modules.
- Invalid, null or unavailable assignments fail closed; a present malformed
  preferred profile cannot silently yield to fallback.

### Verification

- Four new resolver tests were discovered in the fresh final EditMode assembly and
  passed inside the `352/352` gate with no compiler/runtime error or exception.
- No automatic Core integration was added; the main project only pins the accepted
  Modules commit.

## Diamond Pass 16.8 — Factual Possessed-Creature Defeat Receipt

Completed on 2026-09-11; included with this project commit.

### Delivered

- Only the authoritative death callback for the exact currently possessed entity
  can reuse existing MomentFeedback for the factual named defeat plus zero/maximum
  HP receipt.
- Eligibility requires the same still-possessed dead entity, finite exact-zero
  current health, positive finite maximum, valid name and non-terminal ownership.
  Every mismatch preserves the exact generic forced-return fallback.
- The same dead GameObject/entity remains at zero HP with inactive player and AI
  controllers while the unchanged camera return, death presentation, event order,
  timeout and teardown remain authoritative.
- Explicit living release, energy expiry and terminal win/loss keep their distinct
  accepted 16.7/forced copy; no new UI, event, timer or gameplay state was added.

### Verification

- QA verified fresh main and Modules assemblies. Final EditMode passed `352/352`,
  zero failed/skipped/inconclusive, in 1.796 s (job
  `e579a93e-010a-41ea-813e-015a32e71754`).
- Two initial PlayMode failures proved only stale Sylvan fixture name expectations;
  after the test-only correction and an objectively newer PlayMode DLL, the single
  replacement gate passed `102/102`, zero failed/skipped/inconclusive, in 82.037 s
  (job `0c9b9435-0f01-4b1d-9ebb-ad3f7db5d9fe`,
  2026-09-11 02:35:30Z–02:36:52Z).
- No compiler/runtime error, exception or new duplicate-`AudioListener` warning
  occurred; `git diff --check` is clean.

### Scope intentionally deferred

- Manual named defeat receipt fit and Keeper-return/corpse feel in both
  orientations remains unobserved because QA could not safely access the Game view.

## Module Pass MMP11 — Starter Procedural-Humanoid Tuning Assignment

Completed on 2026-09-11 in Modules commit `42fbbc8`.

### Delivered

- One immutable, stable `realmraiders.blood-knight` assignment explicitly selects
  the accepted device-readable tuning profile and names Compatibility as its only
  fallback.
- The assignment is passive data: no discovery, collection, scene access,
  automatic application or gameplay authority was added.

### Verification

- Four new MMP11 Editor tests were discovered after one controlled Unity refresh
  and passed inside the final EditMode `357/357` gate.
- Exact ID, singleton/immutability, preferred and explicit-fallback behavior passed;
  there was no compiler/runtime error or exception.

## Diamond Pass 16.9 — Factual Direct-Combat No-Hit Confirmation

Completed on 2026-09-11; included with this project commit.

### Delivered

- An accepted living direct player's finite positive-damage Melee or Area action
  now shows one `NO HIT` world marker only after authoritative overlap finds no
  eligible living non-self creature.
- Applied damage keeps existing hit/impact feedback, while an immunity-rejected
  eligible contact keeps only its singular `DODGED` result. AI, Dash, invalid,
  rejected and canceled actions remain silent.
- The collider-free camera-facing marker is bounded to 0.45 seconds, restarts
  singularly and clears on the next action, applied impact, controller change,
  death, terminal state, disable, destroy and teardown.

### Verification

- QA verified objectively newer main and Modules Runtime/EditMode/PlayMode
  assemblies after one controlled refresh.
- Final EditMode passed `357/357`, zero failed/skipped/inconclusive, in 1.809 s
  (job `2e323666-1ee2-4638-a166-c20e09690c84`).
- Final PlayMode passed `104/104`, zero failed/skipped/inconclusive, in 84.199 s
  (job `c655f29f-9425-4a6b-8c63-f495060683f4`,
  2026-09-11 02:52:11Z–02:53:35Z).
- No compiler/runtime error, new Console error or new duplicate-`AudioListener`
  warning occurred; `git diff --check` is clean.

### Scope intentionally deferred

- Manual `NO HIT` readability in portrait/landscape remains unobserved because QA
  could not safely access the Game view without changing the user's UI.

## Diamond Pass 17.0 — Explicit Blood Knight Motion-Tuning Resolution

Completed on 2026-09-11; included with this project commit.

### Delivered

- The Core procedural adapter resolves the accepted explicit MMP11 Blood Knight
  assignment once against the explicitly supplied starter catalogue/provider.
- Preferred resolution retains the exact existing
  `BloodKnightDeviceReadable` tuning object. Explicit Compatibility fallback and
  rejected/null catalogue states fail closed to the exact existing
  `CompatibilityDefault` object.
- No discovery, scene scan, per-frame resolution, tuning-value, pose, root,
  controller, collider, Presentation Pivot or gameplay behavior changed.

### Verification

- QA verified objectively newer Runtime/EditMode/PlayMode assemblies after one
  controlled refresh.
- Final EditMode passed `358/358`, zero failed/skipped/inconclusive, in 1.829 s
  (job `bb210ce7-fa2c-499e-b487-cd9869263676`).
- Final PlayMode passed `104/104`, zero failed/skipped/inconclusive, in 83.851 s
  (job `b079314b-e2f0-4714-b855-43846a0edfbc`,
  2026-09-11 03:01:31Z–03:02:55Z).
- Exact tuning identity, deterministic fallback, real bind/rebind and unchanged
  root/controller/pivot ownership passed without compiler/runtime errors, new
  Console errors or new duplicate-`AudioListener` warnings.

### Scope intentionally deferred

- No manual motion check was required because this gate deliberately changes no
  tuning value or pose behavior.
