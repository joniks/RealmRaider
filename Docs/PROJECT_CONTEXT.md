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
- Android first, portrait, one-hand interaction target.
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
