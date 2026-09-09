# Realm Raiders — Module Ready Queue

This is the single work queue for the isolated Module Developer / Technical Art lane. It prevents idle time without inventing unrelated work. The queue is advanced only by Architect after an accepted or rejected module handoff.

## Accepted — Module Pass MWS 05: Surface Preview Budget Gate

Owner: Module Developer / Technical Art
Workspace: `Modules/RealmRaider.Modules` only; no main checkout or Unity.
Accepted Modules commit: `f35927e`
Reserved path: `Packages/com.realmraiders.world-surface-validation/` only.
Handoff: accepted on 2026-09-10 after static review. No Unity or main-checkout work occurred.

### Player/product value

The project has two intentionally provisional world-surface previews. Before a future Core task can make more visual changes, artists need a repeatable way to reject a candidate that cannot meet the existing Android preview budget. This is preparation for visible realm art, not a runtime system.

### Required implementation

1. Delivered only in `com.realmraiders.world-surface-validation`: immutable caller-supplied metadata, deterministic stable issue evidence, focused package tests and a package version/readme update.
2. The accepted contract remains adapter-neutral; it neither loads assets nor inspects/changes Unity import settings.

### Non-goals

- Further changes to this accepted pass.

## Now — Module Pass MVT 03: Mobile Visual Budget Compatibility Gate

Owner: Module Developer / Technical Art
Workspace: `Modules/RealmRaider.Modules` only; no main checkout or Unity.
Base commit: `f35927e`
Reserved paths only:

- `Packages/com.realmraiders.character-visual-tuning/Runtime/MobileVisualBudgetCompatibilityGate.cs`
- `Packages/com.realmraiders.character-visual-tuning/Tests/Editor/MobileVisualBudgetCompatibilityGateTests.cs`
- `Packages/com.realmraiders.character-visual-tuning/package.json`

Do not touch the existing untracked `.meta` files in this package or any character-motion paths. Handoff: frozen isolated diff, static checks and the six-line Module report. No commit or push.

### Player/product value

The prototype has a modular character factory and already carries declared mobile visual budgets for its starter creature profiles. Before more 3D variants are accepted, the art lane needs one deterministic way to reject a profile that exceeds the mobile limits chosen by its caller. This makes future character visual work scalable without giving a package authority over Unity models or gameplay.

### Required implementation

1. Add a pure-C# immutable policy contract with explicitly supplied maximum material count, texture count, texture-edge pixels and triangle count, plus a deterministic compatibility result for the existing `MobileVisualBudget` data type.
2. Return stable ordinal issue codes and messages for every exceeded or invalid declared limit. Preserve the input and make the result immutable; do not use default policy values, discovery, file access, Unity APIs or import inspection.
3. Add focused NUnit tests proving: an accepted current starter budget against an explicit policy; each over-budget dimension; deterministic multi-issue ordering; invalid/null input; and that the existing budget object remains unchanged.
4. Bump this package version only as needed. Do not modify starter profiles, catalogue code, runtime Core bindings or main-project assets.

### Non-goals

- Importing or measuring a model, choosing production limits, changing current profile values, Unity integration, asset downloads, scene/runtime/gameplay work, animation, shaders/materials, package installation or art decisions.

## Later — Paused until a fresh brief

The legacy character-motion-profiles v0.4 worktree candidate has incomplete handoffs and remains intentionally paused. Do not edit or absorb it. A future fresh Module role may receive a new clean lease after Architect creates a focused brief and reserves a clean package path.
