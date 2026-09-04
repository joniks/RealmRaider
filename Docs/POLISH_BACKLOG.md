# Realm Raiders — Diamond Polish Backlog

Work in order. Each pass should remain playable and keep all previous tests green.

## Pass 01 — Foundation and truthful flow

- [x] Make defense outcomes terminal and irreversible.
- [x] Start builds from Prototype Hub.
- [x] Use correct Sylvan and Infernal HUD terminology.
- [x] Make Hub routes match what each scene actually implements.
- [x] Reduce duplicated runtime bootstrap construction safely (shared, parameter-only runtime factory under `Assets/Game/Scripts/Core`).
- [x] Add gameplay-flow tests for terminal defense results, same-entity possession/release and camera/listener invariants.
- [x] First Git checkpoint is `37aad5e`; Android Studio and Xcode export verification is recorded in `Docs/PLATFORM_BUILDS.md` (2026-09-04).

## Pass 02 — Minimal BUILD proof

- [x] Add five fixed placement slots to a small defense layout.
- [x] Let the player place a creature and trap within a simple Threat Budget.
- [x] Save the chosen layout locally.
- [x] Start defense using exactly the saved entities and positions.
- [x] Return to Build after the result.

Verified on 2026-09-04 with EditMode 17/17 and PlayMode 9/9 passing. A final manual portrait flow remains as a device-facing smoke check.

This pass should prove the product promise without becoming a full dungeon editor.

## Pass 02.5 — Adaptive orientation and classic controls

- [x] Support portrait and both landscape orientations without reloading the active scene.
- [x] Preserve Fingertap controls and add a classic virtual joystick, with either method selectable in both orientations.
- [x] Keep attack, ability, trap and possession actions reachable in responsive gameplay HUDs.
- [x] Make Hub, BUILD and gameplay HUDs responsive and safe-area aware at the code/test level.
- [x] Prevent joystick and other UI touches from triggering world movement or combat gestures.
- [x] Preserve runtime gameplay state when the responsive layout changes.
- [x] Add automated orientation/input coverage.
- [ ] Verify rotation, safe areas, focus loss and both control styles on mobile targets.

Implementation is in place. Final Unity Test Runner rerun on 2026-09-04 15:05 EEST passed EditMode 24/24 and PlayMode 10/10. Mobile/manual portrait-landscape verification remains pending.

## Pass 02.6 — Root Trap tap-to-escape

- [x] Add five-tap Root Trap escape prompt and progress for the directly controlled creature.
- [x] Consume escape taps and preserve the existing root timeout fallback.
- [x] Reset escape state on release, death, timeout and control changes.
- [x] Make Root Trap collision non-blocking after release.

Baseline suites remain green at EditMode 24/24 and PlayMode 10/10 (rerun 2026-09-04 15:27 EEST). Manual device feel validation remains pending.

## Pass 03 — Possession WOW moment

- [ ] Selection outline and clear possess affordance.
- [ ] Short time slowdown before takeover.
- [ ] Strongly eased camera dive and pull-out.
- [ ] Possession VFX, audio sting and mobile haptic.
- [ ] Visual energy meter and forced release feedback.
- [ ] Preserve the same entity, health and ability state throughout the swap.

## Pass 04 — Combat feel

- [ ] Action state with windup, impact and recovery.
- [ ] Dodge with an intentional invulnerability window.
- [ ] Input buffering and mobile aim assistance.
- [ ] Enemy telegraphs.
- [ ] Hit flash, reaction, knockback, hit pause and camera shake.
- [ ] Hold/release heavy attack.
- [ ] Visible root and stun feedback.

## Pass 05 — Realm identity

- [ ] Sylvan control/terrain rhythm and organic presentation.
- [ ] Infernal aggression/destruction rhythm and heavier impacts.
- [ ] Race-specific UI copy, colors, audio and possession treatment.
- [ ] Replace repeated runtime definitions with authored data and prefabs.

## Pass 06 — Mobile validation

- [ ] Respect safe areas and multiple portrait aspect ratios.
- [ ] Prevent UI touches from triggering world movement.
- [ ] Validate one-hand reach and button sizing.
- [ ] Android Studio export and physical Android device run.
- [ ] Xcode export, signing and physical/simulator iOS run.
- [ ] Maintain 60 FPS on the selected mid-range Android reference device.
- [ ] Run short usability tests without verbal instructions.

## Explicitly out of scope

Backend, real multiplayer, accounts, matchmaking, guilds, chat, IAP, battle pass, open world, large inventory, breeding, crafting and production-scale content.
