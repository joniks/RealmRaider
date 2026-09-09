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

## Active — Module Pass MMP 04: Explicit Character Motion Binding Batch

Owner: Module Developer / Technical Art

Workspace: `Modules/RealmRaider.Modules` only; no main checkout or Unity.

Base: preserve the existing uncommitted v0.4 candidate; this is its fresh recovery brief.
Reserved paths only:

- `Packages/com.realmraiders.character-motion-profiles/Runtime/CharacterMotionBindingBatchEvaluator.cs`
- `Packages/com.realmraiders.character-motion-profiles/Tests/Editor/CharacterMotionBindingBatchEvaluatorTests.cs`
- `Packages/com.realmraiders.character-motion-profiles/Documentation~/CharacterMotionProfiles.md`
- `Packages/com.realmraiders.character-motion-profiles/package.json`

Do not touch the unrelated untracked `.meta` files, character-visual-tuning package or main checkout. Do not stage, commit or push.

### Player/product value

Hundreds of modular characters need a deterministic pre-Unity check that every explicit roster character points to one exact, compatible motion profile. This pass makes a future Core adapter possible without letting the package choose animations, fallbacks or gameplay behavior.

### Required recovery work

1. Audit the current dirty candidate against the existing v0.3 catalogue and compatibility contracts; keep only behavior that is genuinely required for an explicit character-to-profile batch.
2. Resolve concrete compile/test defects inside the reserved paths. In particular, verify null/unreadable sequences, stable IDs, duplicate characters, exact ordinal catalogue lookup, compatibility issue preservation, deterministic ordering, fail-closed empty records and immutable caller snapshots.
3. Run package-local static/compile checks that do not require Unity. If an existing package-wide defect outside the lease prevents a full check, prove the new test file separately and report the external defect precisely instead of widening scope.
4. Freeze the four-path diff and send Architect a six-line handoff: changed paths; contract; checks; integration need; risks/blocker; Unity/commit/push status. An intermediate note is not completion: continue through the next safe check or report the exact external blocker.

### Non-goals

- Unity/editor control; model or clip import; starter profile instances; automatic profile/fallback selection; runtime animation authority; Core integration; main-project edits; unrelated validators or documentation-only expansion.

## Next Module gate

Architect reviews MMP 04 immediately on handoff. Accepted work is committed in the Modules repository and recorded in the main project; a concrete defect returns to the same lease. After that, the next Module lease must be tied to either a named Core integration seam or a user-approved art source/provenance task.
