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
