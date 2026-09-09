# Realm Raiders — Module Ready Queue

This is the single work queue for the isolated Module Developer / Technical Art lane. It prevents idle time without inventing unrelated work. The queue is advanced only by Architect after an accepted or rejected module handoff.

## Now — Module Pass MWS 05: Surface Preview Budget Gate

Owner: Module Developer / Technical Art
Workspace: `Modules/RealmRaider.Modules` only; no main checkout or Unity.
Base commit: `69f7779`
Reserved path: `Packages/com.realmraiders.world-surface-validation/` only.
Handoff: frozen isolated diff, static checks, required six-line Module report. No commit or push.

### Player/product value

The project has two intentionally provisional world-surface previews. Before a future Core task can make more visual changes, artists need a repeatable way to reject a candidate that cannot meet the existing Android preview budget. This is preparation for visible realm art, not a runtime system.

### Required implementation

1. Extend only the existing pure-C# `com.realmraiders.world-surface-validation` package with an immutable caller-supplied surface-preview metadata contract and deterministic validation result.
2. Validate only declared facts: semantic identifier, positive source dimensions, square/power-of-two source requirement, declared Android maximum dimension, Read/Write disabled, mipmaps enabled, and a supported mobile compression label. The exact accepted constraints must be explicit data/code, not a filesystem or Unity import inspection.
3. Return stable, ordinal issue codes/messages so a later Core/Editor adapter can present evidence without the package loading an asset, reading a file, using Unity APIs or deciding import actions.
4. Add focused package tests for one accepted `1024→512` preview profile and boundary/invalid cases. Update the package README and version only as needed.

### Non-goals

- Unity import inspection or modification, image decoding, filesystem access, shader/material changes, package installation, Core integration, asset downloads, provenance decisions, runtime code, gameplay, scene control or new art.

## Next — Paused until a fresh brief

The legacy character-motion-profiles v0.4 worktree candidate has incomplete handoffs and remains intentionally paused. Do not edit or absorb it. A future fresh Module role may receive a new clean lease after Architect creates a focused brief and reserves a clean package path.
