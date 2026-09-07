# Realm Raiders — Module Host API

Status: first host boundary established in Diamond Pass 09.4.

## Purpose

`RealmRaiders.ModuleContracts` is the small, one-way boundary between the main game and future packages in the separate `RealmRaider.Modules` repository. It allows a module to describe character-catalogue data without gaining authority over gameplay, scenes, UI, saves, bootstraps, or assets.

The contract assembly has no Unity-engine or `RealmRaiders.Runtime` reference. It is intentionally passive: there is no assembly scan, reflection discovery, singleton, `Resources` lookup, `MonoBehaviour`, scene object, or automatic package loading.

## Current public contract

Namespace: `RealmRaiders.Modules`

| Type | Responsibility |
| --- | --- |
| `CharacterBodyFamily` | A stable catalogue classification: `Humanoid`, `LargeCreature`, or `Beast`. |
| `CharacterCatalogueEntry` | Plain data: stable ID, display name, body family, and visual-profile key. |
| `ICharacterCatalogProvider` | Explicitly exposes a stable module ID and its catalogue entries. |

These types are data descriptions only. They do not replace or alias current runtime visual recipes, `CombatEntity`, character definitions, or the existing `CharacterVisualFamily` implementation.

## Rules for a package

- A package may depend on `RealmRaiders.ModuleContracts`, but the contract assembly never depends back on a package.
- Package code must stay self-contained. It may not alter `Assets/Game`, scenes, `ProjectSettings`, shared UI, bootstraps, saves, or gameplay authority.
- Do not copy third-party models, textures, archives, or licence files into a package without a separately approved provenance and distribution decision.
- A package must expose its provider deliberately; it must not register itself by reflection or scan the game for objects.
- A package may propose visual intent, but material-slot mapping and prefab binding remain explicit Core-owned adapters beside `CharacterVisualAssembler` until a real integration needs a wider stable contract.

## Integration sequence

1. Module developer builds and freezes a self-contained package in its own Modules worktree.
2. Reviewer / QA reviews it. The Architect commits the accepted package in the Modules repository; the user pushes it.
3. A separate Core task chooses one known provider explicitly, adds the reviewed package to `Packages/manifest.json`, and writes the narrow adapter required by the existing runtime.
4. QA verifies that integration in the main Unity project. Only then may the Architect update the main repository's submodule pointer.

There is no implicit runtime integration before step 3. This keeps an experimental package from changing the playable prototype merely by being present.

## Compatibility promise

Public names and values in this document's contract are treated as a small API. Additive changes require review; renames, semantic rewrites, or removal require an explicit migration task. Keep IDs lowercase, stable, and suitable for saved/catalogue references.
