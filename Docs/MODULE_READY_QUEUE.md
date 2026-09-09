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

## Accepted — Module Pass MVT 03: Mobile Visual Budget Compatibility Gate

Owner: Module Developer / Technical Art
Workspace: `Modules/RealmRaider.Modules` only; no main checkout or Unity.
Accepted Modules commit: `a3df8ce`
Reserved paths only:

- `Packages/com.realmraiders.character-visual-tuning/Runtime/MobileVisualBudgetCompatibilityGate.cs`
- `Packages/com.realmraiders.character-visual-tuning/Tests/Editor/MobileVisualBudgetCompatibilityGateTests.cs`
- `Packages/com.realmraiders.character-visual-tuning/package.json`

Do not touch the existing untracked `.meta` files in this package or any character-motion paths. Handoff: accepted on 2026-09-10 after static review; no Unity or main-checkout work occurred.

### Player/product value

The prototype has a modular character factory and already carries declared mobile visual budgets for its starter creature profiles. Before more 3D variants are accepted, the art lane needs one deterministic way to reject a profile that exceeds the mobile limits chosen by its caller. This makes future character visual work scalable without giving a package authority over Unity models or gameplay.

### Required implementation

1. Delivered only in `com.realmraiders.character-visual-tuning`: immutable explicit policy, deterministic ordered evidence and focused package coverage for the existing visual-budget declarations.
2. It remains adapter-neutral and cannot choose limits, inspect/import models or change character presentation.

### Non-goals

- Further changes to this accepted pass.

## Accepted — Module Pass MVT 04: Starter Visual Budget Batch Evidence

Owner: Module Developer / Technical Art
Workspace: `Modules/RealmRaider.Modules` only; no main checkout or Unity.
Accepted Modules commit: `4dc6f4d`
Reserved paths only:

- `Packages/com.realmraiders.character-visual-tuning/Runtime/CharacterVisualBudgetBatchReport.cs`
- `Packages/com.realmraiders.character-visual-tuning/Tests/Editor/CharacterVisualBudgetBatchReportTests.cs`
- `Packages/com.realmraiders.character-visual-tuning/package.json`

Do not touch the existing untracked `.meta` files in this package, starter profile values, catalogue sources, character-motion paths or any main-project files. Handoff: accepted on 2026-09-10 after static review; no Unity or main-checkout work occurred.

### Player/product value

The prototype's modular character factory needs a truthful, repeatable way to see whether an explicit roster of declared visual profiles fits one chosen mobile budget policy. A batch result keeps hundreds of future variants reviewable before any model reaches Unity, without making this module an importer or runtime authority.

### Required implementation

1. Delivered only in `com.realmraiders.character-visual-tuning`: immutable ProfileId-ordered rows, copied evidence snapshots and aggregate compatibility totals for an explicitly supplied catalogue and policy.
2. The accepted report remains adapter-neutral and has no authority to choose a policy, inspect a model, import an asset or alter presentation.

### Non-goals

- Further changes to this accepted pass.

## Blocked — next Module visual work needs a real integration seam

The reusable visual-budget tooling is complete through per-profile and roster evidence. The next worthwhile Module lease must be one of:

1. a user-approved source/provenance and art-manifest task for a specific 3D character, or
2. a named future Core integration seam that consumes these already accepted package contracts.

Do not create another generic validator merely to keep the lane busy. Architect records the next clean lease as soon as the frozen Core candidate is accepted or the user selects the next art source.

## Later — Paused until a fresh brief

The legacy character-motion-profiles v0.4 worktree candidate has incomplete handoffs and remains intentionally paused. Do not edit or absorb it. A future fresh Module role may receive a new clean lease after Architect creates a focused brief and reserves a clean package path.
