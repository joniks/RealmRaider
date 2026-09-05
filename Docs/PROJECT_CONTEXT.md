# Realm Raiders — Project Context

## Product promise

Realm Raiders is a mobile-first fantasy action/strategy game where the player's constructed Realm becomes a playable level. The player builds and defends a living Realm, raids other Realms with a Hero, and can possess an existing creature during defense without replacing or respawning it.

**Build your Realm. Raid theirs. Become your monsters.**

## Core loop

```text
BUILD → RAISE CREATURES → CUSTOMIZE DEFENSE → INVADE
      → EXPLORE + FIGHT → LOOT → UPGRADE → DEFEND → BUILD
```

The prototype must prove a compact version:

```text
BUILD → INVADE → FIGHT → POSSESS → DEFEND → RESULT
```

## Design laws

1. Everything you build can be played.
2. Creatures are characters, not towers.
3. Every race changes gameplay, not only visuals.
4. Heroes and creatures use the same combat foundation.
5. Controls are designed for mobile, not reduced from a PC MMORPG.
6. A raid should take roughly 2–4 minutes and invite another attempt.
7. Realm progress should be visually visible.
8. Player-made defense needs constraints such as a Threat Budget.
9. Monetization must not destroy gameplay fairness.
10. Open world starts only after the core loop is independently fun.

## Orientation and control decision

Realm Raiders must support both portrait and landscape as first-class play styles, not as one stretched interface.

- Portrait is the convenient one-hand mode. Its contextual default keeps tap-to-move, swipe combat, and reachable action controls.
- Landscape is the classic two-hand action mode. Its contextual default uses a virtual joystick on the lower-left and combat actions on the lower-right, leaving the center clear.
- The player chooses `Auto`, `Portrait`, or `Landscape`; the preference is saved. `Auto` follows device orientation, while either locked choice must be respected.
- The player also chooses a saved control style: `Contextual`, `Fingertap`, or `Joystick`. These are three preferences built from two actual movement methods.
- `Contextual` is the default: Fingertap in portrait and Joystick in landscape.
- `Fingertap` enables tap-to-move and swipe combat in both orientations.
- `Joystick` enables analog joystick movement in both orientations; world taps may still select/target enemies but must not also set a movement destination.
- Changing control style at runtime clears the previous destination, joystick vector, and in-progress gesture without changing gameplay state.
- Hub and BUILD adapt to both orientations without a joystick.
- Keeper view has no joystick until the player possesses and directly controls a creature, even when `Joystick` is selected.
- Rotation never reloads a scene or resets combat, health, cooldowns, possession energy, defense state, or unsaved BUILD choices.
- UI touches remain owned by UI for the full gesture, and landscape must support holding the joystick while pressing an action with another finger.
- Camera framing must preserve gameplay fairness: landscape may show a wider composition, but must not reveal threats materially earlier or provide a competitive advantage.
- Both modes must be independently laid out, safe-area aware, readable, and tested. A technically rotatable but broken secondary layout is not acceptable.

## Combat camera framing decision

An enemy that matters to the current fight must not remain silently outside the left or right edge of the screen.

- When the player explicitly targets an enemy, enters its meaningful combat range, or starts an ability against it, the camera should use a **soft target focus**: ease its yaw/framing toward a composition that keeps both the controlled character and primary target readable.
- Never hard-snap, steal the player's movement direction, or permanently lock the camera to an AI target. Focus must be bounded, ease in/out, and release on target death, distance, player retarget, possession release, terminal result, or manual camera transition.
- If a relevant target cannot fit fairly in frame, show a compact edge indicator with direction and urgency instead of hiding the threat. This is especially important in portrait.
- Landscape may allow a slightly wider soft-focus composition, but both orientations must preserve equivalent tactical information and avoid giving one orientation an unfair early-warning advantage.
- Target focus is a future presentation/camera pass. It must not change targeting, damage, AI, movement, controls, or gameplay balance.

This direction is supported by mobile-game precedent and interaction research: Mario Kart Tour exposes Portrait, Landscape, and automatic switching with landscape control-side choices; NIKKE added horizontal play after strong player demand but required later landscape UI improvements; usability research found strong two-thumb landscape accuracy while portrait retains one-hand convenience. The intended Realm Raiders split is therefore accessibility in portrait and traditional action control in landscape.

## Prototype races

### Sylvan — The Wilds

- Identity: control, terrain and mobility.
- Core: Heart Tree.
- Units: Warden direction, Wolves, Ent.
- Defense: Root Trap, Thorn Wall direction.

### Infernal — The Abyss

- Identity: aggression and destruction.
- Core: Infernal Heart.
- Units: Blood Knight, Hellhounds, Brute.
- Defense: Flame Trap and Lava Gate.

The two races must eventually feel different in rhythm, navigation and defensive decisions. Color swaps alone do not satisfy the design.

## Technical foundation

- Unity 6000.6.0f1, URP and New Input System.
- Android first, with equal first-class portrait and landscape support: one-hand direct controls in portrait and classic two-hand controls in landscape.
- `CombatEntity` owns stats, health, abilities, movement and one active `IEntityController`.
- Possession switches `CreatureBrain` to `PlayerController` on the same entity, then restores AI on release.
- No backend, multiplayer, accounts, guilds, IAP, open world or production content pipeline in this prototype.

## Prototype success test

The most important experience is a 30–60 second defense moment:

1. See the Realm from Keeper view.
2. Watch an invader approach.
3. Select a creature.
4. Trigger possession.
5. Follow a fast camera dive into the same creature.
6. Fight using that creature.
7. Release or die and return to Keeper view.

If this moment is not immediately understandable and satisfying, more content will not solve the prototype.
