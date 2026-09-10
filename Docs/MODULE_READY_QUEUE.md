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

## Accepted — Module Pass MMP 04: Explicit Character Motion Binding Batch

Owner: Module Developer / Technical Art

Workspace: `Modules/RealmRaider.Modules` only; no main checkout or Unity.

Accepted Modules commit: `50d7a4a`

Base: recovered the existing uncommitted v0.4 candidate under a fresh focused brief.
Reserved paths only:

- `Packages/com.realmraiders.character-motion-profiles/Runtime/CharacterMotionBindingBatchEvaluator.cs`
- `Packages/com.realmraiders.character-motion-profiles/Tests/Editor/CharacterMotionBindingBatchEvaluatorTests.cs`
- `Packages/com.realmraiders.character-motion-profiles/Documentation~/CharacterMotionProfiles.md`
- `Packages/com.realmraiders.character-motion-profiles/package.json`

The unrelated untracked `.meta` files, character-visual-tuning package and main checkout were not included.

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

MMP 04 is accepted and committed. The next Module lease must be tied to either a named Core integration seam or a user-approved art source/provenance task; do not invent another generic validator merely to keep the lane busy.

## Accepted — Art Pass MWS 06: Fantasy Surface Texture Pack

Owner: Art / Module Developer / Technical Art

Workspace: `Modules/RealmRaider.Modules` only; no main checkout or Unity.

Accepted Modules commit: `3b1d7d6`

Reserved folder only: `ArtPreviews/MWS06-FantasySurfaceTexturePack/`.

### Player/product value

The first Sylvan and Infernal ground previews proved that real texture immediately improves the greybox. This pass adds four distinct fantasy material directions for paths and boundary structures so the two realms can gain richer visual hierarchy without changing gameplay or importing unreviewed third-party art.

### Required deliverables

1. Generate four original 1024×1024 square, seamless/tileable albedo preview PNGs:
   - Sylvan moss-grown ancient stone path;
   - Sylvan living bark and intertwined root boundary;
   - Infernal cracked volcanic basalt path with restrained ember seams;
   - Infernal forged black iron and obsidian boundary surface.
2. Use a stylized cohesive mobile-fantasy direction: readable medium/large forms, restrained high-frequency noise, no baked directional lighting, no perspective, no isolated object, no text, logo, watermark or recognizable franchise design.
3. Add one local provenance/manifest record in the same folder containing the exact prompts, generation tool, date, intended surface role, preview-only status and explicit statement that no third-party source image was used.
4. Inspect every result at full frame. Reject and regenerate obvious perspective, lighting hotspots, seams, text-like marks or non-surface compositions; do not silently call an imperfect image production-ready.
5. Keep every output isolated in the reserved folder. Do not create Unity `.meta`, normal/height/roughness maps, materials, shaders, import settings or runtime bindings.
6. Handoff to Architect with: exact files; visual distinction; seam/mobile caveats; provenance; selected recommendation; Unity/commit/push status.

### Non-goals

- Main-checkout edits, Unity control/import, gameplay/collision changes, third-party downloads, final-art approval, derived PBR maps, runtime integration or device-performance claims.

Acceptance: four original 1024×1024 RGB previews and a complete local provenance
record were inspected on 2026-09-10. The safest first integration candidate is
the Sylvan moss-grown stone path; the strongest boundary accent is the Sylvan
living-root surface. Repeat-grid seams and device-scale material behavior remain
explicit production gates rather than assumed properties.

## Accepted — Art Pass MWS 07: Walkable Surface Seam Hardening

Owner: Art / Module Developer / Technical Art

Workspace: `Modules/RealmRaider.Modules` only; no main checkout or Unity.

Accepted Modules commit: `6138f71`

Reserved folder only: `ArtPreviews/MWS07-WalkableSurfaceSeamHardening/`.

### Player/product value

Convert the two clearest realm path directions into honestly reviewable tile
candidates so Core can later test first-world texture integration without visible
grid seams or importing every exploratory surface.

### Required deliverables

1. Starting only from the accepted MWS06 Sylvan stone and Infernal basalt source
   previews, create one edge-hardened 1024×1024 RGB candidate for each realm.
2. Preserve the established palette and large mobile-readable forms. Do not add
   perspective, directional lighting, text, symbols, isolated props or new IP.
3. Produce a 2048×2048 2×2 repeat-grid review image for each candidate. The grid
   is validation evidence only, not a Unity import asset.
4. Inspect the center cross and all four repeated edges at full frame. Iterate on
   obvious hard seams, repeated focal stones, clipped ember rivers or brightness
   bands; record any remaining caveat honestly.
5. Add a provenance record with exact source paths, every generation/edit tool,
   exact prompts or deterministic operations, date and preview-only status.
6. Do not create `.meta`, normal/height/roughness/emission maps, materials,
   shaders, import settings or runtime bindings. Do not open Unity.

### Non-goals

- Main checkout, runtime integration, boundary surfaces, derived PBR maps,
  gameplay changes, third-party downloads, device-performance claims, commit or
  push by the producing role.

Acceptance: both 1024×1024 RGB path candidates and exact 2×2 repeat evidence
were inspected on 2026-09-10. No hard center-cross seam is visible. Sylvan is
approved as the first future Unity integration candidate; Infernal remains
approved for preview with an explicit on-device ember-periodicity check.

## Accepted — Art Pass MWS 08: Realm Boundary Seam Hardening

Owner: Art / Module Developer / Technical Art

Workspace: `Modules/RealmRaider.Modules` only; no main checkout or Unity.

Accepted Modules commit: `c4295e4`

Reserved folder only: `ArtPreviews/MWS08-RealmBoundarySeamHardening/`.

### Player/product value

Prepare one distinctive boundary surface per realm so arena borders can become
legible fantasy structures instead of generic grey geometry when Core later
integrates the selected visuals.

### Required deliverables

1. Starting only from the accepted MWS06 Sylvan living-root and Infernal forged
   iron/obsidian previews, create one edge-hardened 1024×1024 RGB candidate for
   each realm.
2. Preserve the realm palettes and broad mobile-readable forms. Reduce painted
   contact-shadow/depth hotspots where possible without flattening the material
   identity; add no perspective, symbols, text, props, new IP or directional
   lighting.
3. Produce one exact 2048×2048 2×2 repeat-grid review image per candidate.
4. Inspect center cross, repeated edges, brightness bands and dominant repeating
   root/plate motifs. Iterate obvious hard seams and record remaining periodicity.
5. Add a provenance record containing exact MWS06 source paths and hashes,
   prompts/tools/operations, date, preview-only status and no-third-party-source
   statement.
6. No Unity, `.meta`, PBR maps, materials, shaders, import settings, runtime
   binding, commit or push by the producing role.

### Non-goals

- Main checkout, gameplay/collision changes, walkable surfaces, production
  approval, device-performance claims or runtime integration.

Acceptance: both 1024×1024 RGB boundary candidates and their exact 2×2 repeat
evidence were inspected on 2026-09-10. Neither has a hard center seam. Sylvan is
the preferred first boundary integration candidate; Infernal remains suitable
for preview with an explicit on-device crimson-network periodicity check.

## Accepted — Module Pass MWS 09: Sylvan Living-Root Mobile Normal Map

Owner: Module Developer / Technical Art

Workspace: `Modules/RealmRaider.Modules` only; no main checkout or Unity.

Accepted Modules commit: `f3475ac` (based on `c4295e4`).

Reserved paths only:

- `Tools/surface_maps/derive_normal_map.py`
- `Tools/surface_maps/test_derive_normal_map.py`
- `ArtPreviews/MWS09-SylvanLivingRootMobileMaps/`

### Player/product value

The accepted living-root boundary albedo now has a named Core integration gate.
Give that boundary mobile-readable light response without changing collision,
gameplay, source art or importing speculative third-party content.

### Required implementation

1. Implement a deterministic command-line normal-map derivation tool. It accepts
   an explicit input/output path and bounded strength, preserves source dimensions,
   wraps sampling at every edge and writes an RGB tangent-space normal map.
2. Add tool-level tests for deterministic output, dimensions, wrapped edges,
   bounded/normalized channels and invalid input/strength handling. Tests must not
   require Unity or network access.
3. Starting only from the accepted MWS08 Sylvan living-root edge-hardened albedo,
   generate one restrained 1024×1024 mobile normal candidate plus exact 2048×2048
   2×2 repeat evidence. Broad roots must read; painted contact-shadow noise must
   not become exaggerated geometry.
4. Record exact source path/hash, tool command/version, parameters, output hashes,
   date and the fact that no third-party source was used.
5. Freeze the reserved diff and report code, generated assets, checks, visual
   caveats, future Core integration seam and Unity/commit/push status.

### Non-goals

Main checkout, Unity or `.meta` files, runtime/material integration, albedo edits,
height/displacement, colliders, shaders, gameplay, third-party downloads, commit
or push by the producing role.

Acceptance: deterministic wrapped-edge normal generation, 5/5 tool tests,
matching source/output hashes, 1024×1024 RGB candidate and exact 2048×2048 RGB
repeat evidence were reviewed on 2026-09-10. The normal remains deliberately
restrained and requires Unity import-orientation and device-lighting validation.

## Next Module gate

QA accepted the 13.1 boundary import without a strength/orientation correction:
no obvious seam, streak or gap was observed in Sylvan portrait or Defender
landscape. Advance to MWS10 below rather than producing another boundary variant.

## Accepted — Module Pass MWS 10: Sylvan Clearing Floor Production Candidate

Owner: Module Developer / Technical Art

Workspace: `Modules/RealmRaider.Modules` only; no main checkout or Unity.

Base: Modules commit `f3475ac`.

Accepted Modules commit: `646ecde`.

Reserved folder only:

- `ArtPreviews/MWS10-SylvanClearingFloor/`

### Outcome

Turn the existing project-owned MWS02 Sylvan ground direction into one honestly
reviewable circular-node/clearing surface for the next visual integration gate.

### Required deliverables

1. Start only from the accepted MWS02 Sylvan ground RGB source in the main project;
   record its exact path/hash. No third-party source or new franchise reference.
2. Produce one edge-hardened 1024×1024 RGB albedo with broad, mobile-readable
   moss/soil/leaf forms that remain distinct from the MWS07 stone path and MWS08
   living-root border. Avoid a central emblem, directional light or perspective.
3. Produce exact 2048×2048 2×2 albedo repeat evidence and inspect its center cross.
4. Use the accepted `Tools/surface_maps/derive_normal_map.py` with a restrained,
   recorded strength to produce one 1024×1024 RGB tangent normal plus exact 2×2
   repeat evidence. Do not edit the accepted tool in this lease.
5. Add one provenance JSON with source/tool parameters, operations, dimensions,
   all hashes, preview-only status and no-third-party-source truth.
6. Verify RGB/no-alpha dimensions, deterministic normal regeneration, JSON and
   whitespace; hand off exact visual caveats and recommended Unity tile scale.

### Non-goals

Unity or `.meta`, main checkout, runtime/material integration, colliders/gameplay,
height/displacement/emission/masks, tool edits, Infernal art, commit or push.

Acceptance: the 1024×1024 RGB albedo and restrained tangent normal, exact
2048×2048 repeat evidence, source/output hashes and provenance were reviewed on
2026-09-10. No hard center-cross seam is visible. Small leaf/pebble repetition
and the recommended 3.5-world-unit tile scale remain explicit Unity/device gates.

## Next Module gate

## Accepted — Art Pass MUI 02: Encounter Cue Icon Candidates

Owner: Module Developer / Technical Art

Workspace: `Modules/RealmRaider.Modules` only; no main checkout or Unity.

Base: Modules commit `646ecde`.

Accepted Modules commit: `3bc738c`.

Reserved folder only:

- `ArtPreviews/MUI02-EncounterCueIconSet/`

### Outcome

Prepare a compact original visual vocabulary for the factual 13.2 encounter cue:
node discovered, hostiles present and area clear. These remain optional preview
candidates until the text-first cue passes QA and Architect names an integration.

### Required deliverables

1. Create three cohesive original 512×512 transparent-background RGBA PNGs:
   `discovered`, `hostiles` and `area-clear`.
2. Use one bold mobile-fantasy silhouette per icon, readable at 32–48 px; restrained
   pale-gold/Sylvan accent, no text, numerals, frame, glow cloud, logo, watermark,
   gradient backdrop or recognizable franchise design.
3. Produce one neutral dark 48 px contact sheet showing all three at actual size,
   plus one 256 px overview sheet. Evidence sheets are not runtime imports.
4. Record exact prompts/tools/date/output hashes, original-generation truth,
   intended factual mapping and preview-only status in one provenance JSON.
5. Inspect silhouette distinction, alpha edges and actual-size legibility. Report
   any ambiguity honestly; do not call the set runtime-approved.
6. Verify dimensions, RGBA/alpha, JSON, hashes and whitespace. Freeze only the
   reserved folder and send a compact handoff; no commit or push.

### Non-goals

Main checkout, Unity or `.meta`, 13.2 code/text/layout changes, runtime binding,
animation, font, audio/VFX, gameplay/reward logic, third-party sources or package
validators.

Acceptance: all three 512×512 RGBA candidates, their actual-size 48 px contact
sheet, 256 px overview, alpha evidence, declared hashes and original-generation
provenance were reviewed on 2026-09-10. The silhouettes are distinct at 48 px;
the darker hostiles mark still requires explicit 32 px/in-context contrast review.

## Accepted — Art Pass MUI 03: Blood Knight Ability Icon Candidates

Owner: Module Developer / Technical Art

Workspace: `Modules/RealmRaider.Modules` only; no main checkout or Unity.

Base: Modules commit `3bc738c`.

Accepted Modules commit: `3fa21af`.

Reserved folder only:

- `ArtPreviews/MUI03-BloodKnightAbilityIcons/`

### Outcome

Prepare an original, cohesive icon family for the three truthful direct-control
actions already present in the first Sylvan raid: Basic Slash, Blood Rush and
Heavy Cleave. The set remains preview-only until a later named HUD integration.

### Required deliverables

1. Create three original 512×512 transparent-background RGBA PNGs: `basic-slash`,
   `blood-rush` and `heavy-cleave`.
2. Use bold mobile-readable silhouettes with shared Blood Knight palette and
   shape language. Slash must read as a quick single arc, Rush as forward motion,
   and Cleave as a heavier broad area arc without relying on text or numerals.
3. Avoid model likeness, logos, gore, realistic blood, a full character, backdrop,
   frame, watermark or recognizable franchise design. Use no third-party source.
4. Produce one neutral dark 48 px actual-size contact sheet and one 256 px overview.
   Evidence sheets are not runtime assets.
5. Record exact prompts/tools/date/output hashes, intended ability mapping,
   original-generation truth and preview-only status in one provenance JSON.
6. Inspect silhouette distinction, transparent edges and 32–48 px readability;
   verify dimensions, RGBA/alpha, JSON, hashes and whitespace. Freeze only the
   reserved folder and send a compact handoff; no commit or push.

### Non-goals

Main checkout, Unity or `.meta`, RaidHUD or ability changes, runtime binding,
cooldown/readiness logic, combat balance, animation/VFX/audio, third-party source,
character/model art or package validators.

Acceptance: all three 512×512 RGBA candidates, 48 px contact sheet, 256 px
overview, alpha checks, hashes and original-generation provenance were reviewed
on 2026-09-10. The quick slash, forward rush and broad cleave remain distinct at
48 px; Basic Slash retains an explicit 32 px/in-context readability gate.

## Accepted — Art Pass MUI 04: Guardian Ent Ability Icon Candidates

Owner: Module Developer / Technical Art

Workspace: `Modules/RealmRaider.Modules` only; no main checkout or Unity.

Base: Modules commit `3fa21af`.

Accepted Modules commit: `0834c2f`.

Reserved folder only:

- `ArtPreviews/MUI04-GuardianEntAbilityIcons/`

### Outcome

Prepare an original icon family for the three truthful Guardian Ent possession
actions: Smash, Charge and Ground Slam. This directly supports the prototype's
Keeper → possession → direct combat proof but remains preview-only until named.

### Required deliverables

1. Create three original 512×512 transparent RGBA PNGs: `smash`, `charge` and
   `ground-slam`.
2. Use one cohesive ancient-living-wood/Sylvan palette. Smash must read as one
   close heavy impact, Charge as forward trunk/root momentum, and Ground Slam as
   a radial earth/root shock distinct from the other two at 32–48 px.
3. Use no full character, model likeness, text, numerals, frame, backdrop, logo,
   watermark, gore, third-party source or recognizable franchise design.
4. Produce a neutral dark 48 px actual-size contact sheet and 256 px overview;
   evidence sheets are not runtime imports.
5. Record exact prompts/tools/date/output hashes, ability mapping,
   original-generation truth and preview-only status in provenance JSON.
6. Verify dimensions, RGBA/alpha, JSON, hashes, whitespace and actual-size
   distinction. Freeze only the reserved folder and send a compact handoff; no
   commit or push.

### Non-goals

Main checkout, Unity or `.meta`, DefenderHUD/abilities, runtime binding,
cooldowns/combat tuning, animation/VFX/audio, character/model art or validators.

Acceptance: all three 512×512 RGBA candidates, evidence sheets, alpha checks,
hashes and provenance were reviewed on 2026-09-10. Close Smash, forward Charge
and radial Ground Slam remain distinct at 48 px; Ground Slam retains a named
32 px/in-context detail gate.

## Now — Art Pass MUI 05: Infernal Brute Ability Icon Candidates

Owner: Module Developer / Technical Art

Workspace: `Modules/RealmRaider.Modules` only; no main checkout or Unity.

Base: Modules commit `0834c2f`.

Reserved folder only:

- `ArtPreviews/MUI05-InfernalBruteAbilityIcons/`

### Outcome

Create an Infernal visual skin for the existing Smash, Charge and Ground Slam
mechanics so future data-driven HUD binding can distinguish realm identity without
forking ability logic. The set remains preview-only.

### Required deliverables

1. Create original 512×512 transparent RGBA `smash`, `charge` and `ground-slam`
   icons with forged obsidian, restrained ember-red and pale-hot edge accents.
2. Keep the same truthful mechanical readings as MUI04—close impact, forward
   momentum, radial area shock—but make every silhouette clearly Infernal and not
   a recoloured copy of the living-root set at 32–48 px.
3. No full character/model likeness, text, numerals, frame, backdrop, logo,
   watermark, gore, third-party source or recognizable franchise design.
4. Produce a dark-neutral 48 px contact sheet and 256 px overview, record exact
   prompts/tools/date/mapping/hashes and original-generation truth in provenance.
5. Verify dimensions, RGBA/alpha, JSON, hashes, whitespace and actual-size
   distinction; freeze only the reserved folder and send compact handoff.

### Non-goals

Main checkout, Unity or `.meta`, runtime/HUD binding, ability logic/balance,
animation/VFX/audio, Infernal model/environment art, validators, commit or push.
