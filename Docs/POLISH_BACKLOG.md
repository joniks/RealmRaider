# Realm Raiders — Diamond Polish Backlog

Work in order. Each pass should remain playable and keep all previous tests green.

## Pass 01 — Foundation and truthful flow

- [x] Make defense outcomes terminal and irreversible.
- [x] Start builds from Prototype Hub.
- [x] Use correct Sylvan and Infernal HUD terminology.
- [x] Make Hub routes match what each scene actually implements.
- [ ] Reduce duplicated runtime bootstrap construction safely.
- [ ] Add gameplay-flow tests for defense results, possession and camera/listener invariants.
- [ ] Create the first Git checkpoint after platform exports are verified.

## Pass 02 — Minimal BUILD proof

- [ ] Add 3–5 fixed placement slots to a small defense layout.
- [ ] Let the player place a creature and trap within a simple Threat Budget.
- [ ] Save the chosen layout locally.
- [ ] Start defense using exactly the saved entities and positions.
- [ ] Return to Build after the result.

This pass should prove the product promise without becoming a full dungeon editor.

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
