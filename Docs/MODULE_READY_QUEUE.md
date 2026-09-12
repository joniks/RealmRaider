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

MMP 04 is accepted and committed. Art Pass MWS12 is accepted in Modules commit
`e4d1358`: two restrained strength-`0.20` route normals and exact 2×2 evidence are
deterministically derived from the accepted project-owned MWS07 Sylvan and Infernal
albedos. Existing tool tests passed `5/5`; Architect visually inspected all four
outputs and independently reproduced their exact hashes. Unity/runtime integration
belongs only to Core 15.6.

## Accepted — Art Pass MWS 11: Infernal Courtyard Floor Pair

Owner: Module Developer / Technical Art
Workspace: `Modules/RealmRaider.Modules` only; no main checkout or Unity.
Base commit: `04b8861`
Reserved folder only: `ArtPreviews/MWS11-InfernalCourtyardFloor/`.

Read-only sources/tools:

- `ArtPreviews/MWS03-InfernalGroundMaterialCandidate/infernal-ground-albedo-rgb-candidate.png`
- `ArtPreviews/MWS03-InfernalGroundMaterialCandidate/provenance.json`
- `Tools/surface_maps/derive_normal_map.py`
- `Tools/surface_maps/test_derive_normal_map.py`

### Player/product value

MWS10 prepared a calmer Sylvan clearing-floor layer distinct from its routes and
living-root boundary. Prepare the matching Infernal courtyard-floor layer so a
later named Core pass can give both realms a coherent three-level material
hierarchy rather than reusing route or boundary texture everywhere.

### Required deliverables

1. Starting only from the project-owned original MWS03 Infernal direction, create
   one original 1024×1024 RGB, exact top-down, tileable Infernal courtyard-floor
   albedo candidate: broad cooled basalt/ash slabs, sparse subdued ember fissures,
   calm enough behind characters and distinct from both MWS07 route basalt and
   MWS08 forged boundary iron.
2. Create exact 2048×2048 2×2 albedo repeat evidence. Inspect center cross, outer
   edges, brightness bands, repeated focal slabs and ember rivers; iterate obvious
   defects and record any remaining periodicity honestly.
3. Use the existing deterministic normal tool without modifying it to derive one
   restrained 1024×1024 RGB tangent-space normal candidate at strength no greater
   than `0.35`, plus exact 2×2 normal evidence. Do not exaggerate painted shadows
   into geometry.
4. Add local provenance with exact source path/hash, image-generation prompt/tool,
   deterministic resize/normal commands and versions, parameters, all output hashes,
   date, preview-only status, no-third-party-source statement and explicit Unity/
   mobile/on-device review needs.
5. Run the existing surface-map tool tests and deterministic output/hash checks.
   Inspect every result at full frame. Freeze only the reserved-folder diff and send
   the strict six-line Module handoff.

### Non-goals

Main checkout, Unity or `.meta`, runtime/import/material integration, route/boundary
replacement, emission/height/roughness/metallic maps, shader, gameplay, licence
research, third-party download, final-art approval, commit or push.

Acceptance: Architect inspected the 1024 albedo/normal candidates and both exact
2×2 evidence images, confirmed the stated visible broad-slab/ember periodicity,
validated all recorded hashes and reproduced the normal/evidence output exactly.
The existing surface-map tests passed `5/5`. Accepted in Modules commit `2f5457f`;
Unity/runtime integration remains solely the named Core 15.4 lease.

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

## Accepted — Art Pass MUI 05: Infernal Brute Ability Icon Candidates

Owner: Module Developer / Technical Art

Workspace: `Modules/RealmRaider.Modules` only; no main checkout or Unity.

Base: Modules commit `0834c2f`.

Accepted Modules commit: `a4c41c1`.

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

Acceptance: all three 512×512 RGBA candidates, evidence sheets, alpha checks,
hashes and exact generation prompts were reviewed on 2026-09-10 after one
provenance-only correction. The close impact, forward wedge and radial shock are
distinct at 48 px; Ground Slam retains a named 32 px detail gate.

## Accepted — Art Pass MUI 06: Control-Style Icon Candidates

Owner: Module Developer / Technical RTS/UI Art

Workspace: `Modules/RealmRaider.Modules` only; no main checkout or Unity.

Base: Modules commit `a4c41c1`.

Accepted Modules commit: `29bc278`.

Reserved folder only:

- `ArtPreviews/MUI06-ControlStyleIcons/`

### Outcome

Prepare one original icon each for the already truthful Contextual/AUTO,
Fingertap/TAP and Joystick/STICK modes so the in-run switcher can later communicate
the real input distinction without relying only on short English labels.

### Required deliverables

1. Create original 512×512 transparent RGBA `contextual`, `fingertap` and
   `joystick` icons in one restrained neutral/Sylvan mobile UI language.
2. Contextual must suggest automatic context choice without a gear or AI/robot
   claim; Fingertap must show a simple touch point/gesture; Joystick must show a
   clear virtual stick. They must remain distinct at 32–48 px.
3. No text, letters, numerals, device brand, hand anatomy detail, backdrop, frame,
   logo, watermark, third-party source or recognizable platform/franchise design.
4. Produce dark-neutral 32 px and 48 px contact sheets plus a 256 px overview.
5. Record exact prompts/tools/date/mappings/output hashes and original-generation
   truth in provenance; verify RGBA/alpha, dimensions, hashes, JSON and whitespace.
6. Freeze only the reserved folder and send compact handoff; no commit or push.

### Non-goals

Main checkout, Unity or `.meta`, switcher/input/camera behavior, runtime binding,
new control modes, animation, accessibility claims, validators or device testing.

Acceptance: three 512×512 RGBA candidates, 32/48 px contact sheets, 256 px
overview, exact prompts/hashes and alpha/dimension checks were reviewed on
2026-09-10. All remain distinct at 32 px; Contextual and Fingertap retain explicit
in-context comprehension gates, while Joystick reads most directly.

## Accepted — Art Pass MUI 07: Realm Identity Icon Candidates

Owner: Module Developer / Technical Art

Workspace: `Modules/RealmRaider.Modules` only; no main checkout or Unity.

Base: Modules commit `29bc278`.

Accepted Modules commit: `04b8861`.

Reserved folder only:

- `ArtPreviews/MUI07-RealmIdentityIcons/`

### Outcome

Create one compact original Sylvan mark and one compact original Infernal mark for
future Hub/HUD realm identity, without implying unavailable factions or systems.

### Required deliverables

1. Create original 512×512 transparent RGBA `sylvan-realm` and `infernal-realm`
   icons. Sylvan should read as living canopy/root/seed continuity; Infernal as
   forged obsidian/ember gate continuity. Avoid generic recolours.
2. Keep silhouettes bold at 32–48 px and compatible with both dark and moderately
   coloured UI panels. No text, letters, numerals, crest frame, flag, map, full
   character, logo, watermark, third-party source or franchise design.
3. Produce dark-neutral 32 px and 48 px contact sheets, one midtone-panel 48 px
   contrast sheet and one 256 px overview.
4. Record exact prompts/tools/date/mappings/output hashes and original-generation
   truth in provenance; verify dimensions, RGBA/alpha, hashes, JSON and whitespace.
5. Freeze only the reserved folder and send compact handoff; no commit or push.

### Non-goals

Main checkout, Unity or `.meta`, Hub/HUD/runtime binding, new realms/factions,
gameplay, animation/VFX, branding finalization, validators or device claims.

Acceptance: two original 512×512 RGBA realm marks, dark 32/48 px evidence,
distinct midtone 48 px evidence, 256 px overview and exact prompt/hash provenance
were reviewed on 2026-09-10. The candidates are structurally distinct and the
Infernal gate remains clear at small size; Sylvan fine root/leaf detail retains a
named 32 px in-context readability gate.

## Lane reset before the next Module gate

MUI07 required two incomplete handoff corrections (missing provenance, then an
identical dark/midtone evidence sheet). Per team protocol, start the next Module
lease from a fresh compact role conversation that reads `Docs/TEAM_BRIEF.md` and
receives one named reserved folder. Do not invent a generic validator or continue
writing in the old lane merely to avoid idle time.

## Accepted — Research Pass MMP05: Blood Knight Motion Pilot Intake

Accepted Modules commit: `122f5f9`.

The official Quaternius Universal Animation Library 2 pack page is recorded as a
specific CC0 commercial-use source candidate, with a conservative requirement to
retain dated official evidence and inspect the actual archive for contradictory
terms before raw files enter Git. The manifest freezes seven semantic pilot slots:
idle, one locomotion loop, jump takeoff/fall/land, one sword attack and death.

No third-party binary, rig, clip, Animator, Unity import or runtime binding was
created. Exact free-archive contents and the current 3DRT Generic rig compatibility
remain the named Core/QA acquisition pilot rather than an assumed result.

## Accepted — Module Pass MMP06: Character Motion Presentation Resolver

Accepted Modules commit: `fc37bc1`.

The motion-profile package now exposes immutable zero-allocation factual input and
deterministically resolves Idle, Locomotion, two attacks, Hit, Death and three jump
phases. It retains no timing, scene, asset, root-motion or gameplay authority.

## Accepted — Module Pass MMP07: Procedural Humanoid Pose Driver

Accepted Modules commit: `daddb37`; Unity test-compilation correction `e57d795`.

The new procedural-motion package binds six exact supplied humanoid bone names
once, caches their local baselines, applies bounded additive rotations for all nine
semantic keys and restores exactly on clear/rebind. It has no MonoBehaviour,
Animator, root motion, physics, scene scan or gameplay dependency. QA installation
in the main project compiled and discovered both Module Editor assemblies; the
complete final EditMode gate passed `244/244` after the two test-only corrections.

## Superseded — Guardian Ent Source Intake

This old waiting gate is complete through the accepted Tree01 static/Generic
pilot and 15.16B integration. Do not repeat acquisition, import or motion readiness
work. Modules baseline is `42fbbc8`; later accepted MMP08–MMP11 and MART05 work are
recorded in PROTOTYPE_STATUS and DONE_JOB.

## Accepted for integration — MGC01 Three Authored Sylvan Raid Compositions

Owner: Module Developer / Technical Art (`module_mart05_3_finish`).
Workspace: `Modules/RealmRaider.Modules` only. Base `42fbbc8`.
Reserved folder: `Packages/com.realmraiders.sylvan-encounters/` (new, clean).
Handoff: frozen package/data/test diff, compact report; no Unity/main/commit/push.

Architect static acceptance: `83bcda9` on 2026-09-12. Six isolated files reviewed;
IDs, node/capsule clearance, immutable content and exact integration seam checked.
Seven NUnit tests authored but not run; the package is not installed in the game.
Do not claim playable variants or Unity verification before 18.3 integration.

Implement immutable explicit content with three full-raid compositions. Spawn
records contain stable archetype ID, unique per-run spawn ID/display name, node
ID, node-local X/Z offset and scale. No stat/ability duplication, Unity dependency,
random selection, spawner, discovery, generic validator/provider framework or Y
coordinates. Core owns placement, selection, authoritative entities and rewards.

- Baseline: Wolf Grove two wolves `(1,-1)` scale .75 and `(-1.5,1.7)` scale .68;
  Ent Grove one Ent `(0,0)` scale 1.45.
- Wolf pressure: Wolf Grove two wolves `(1,-1)` and `(-2,1.5)`; Ent Grove one
  Ent `(0,0)`; Moonwell one wolf `(-1.8,1.5)`. Wolves scale .68–.75. Four total.
- Sentinel escort: Wolf Grove one wolf `(1,-1)`; Ent Grove Ent `(-1.1,0)` and
  one wolf `(1.8,1.4)`. Three total. Keep separation between scaled capsules.

The old Scout offset `(-2,3)` is 3.61m from a node centre and outside the 3.25m
node circle; the prepared baseline deliberately corrects that spawn. Require
offset radius + scaled capsule radius + 0.2m clearance <= 3.25m for every spawn.
All use existing approved roster. No mandatory Root Path trap ambush: current
11m detection can engage before node entry, and unavoidable root damage would
undermine the intended choice. These names describe authored composition, not
new pincer/navigation AI. Package tests prove identities, immutable isolation,
baseline roster/scales, corrected Scout placement, counts/allowed nodes and safe
finite separated offsets.

## Next — MGC01 Core integration handoff

Dependency: package static acceptance and 18.0/18.1/18.2 Core gates. Core 18.3
materializes the chosen composition through existing Entity/Node/RaidManager,
with explicit spawn lists, existing rewards/fog and a truthful pre-run choice.
No automatic integration or extra Modules work before this candidate is reviewed.

## Accepted for integration — MGC02 Infernal Raid Pacing Presets

Owner: Module Developer / Technical Art (`module_mart05_3_finish`).
Accepted Modules commit: `8ee1d95` on 2026-09-12.
Package: `Packages/com.realmraiders.infernal-encounters/`.

Three immutable, no-engine pacing presets now describe an Infernal Ent raid without
spawning content or taking gameplay authority. Architect rejected the first finale
draft because it required every hostile and included a second lava choke. The
accepted `BruteFinale` is exactly two Hellhounds, one optional/bypassable Flame
Trap, the Infernal Brute and the Heart; only the explicit Brute-defeated gate
permits the Heart. Eight focused NUnit tests are authored but have not run because
the isolated host lacks its old .NET 2 runtime. Unity verification begins only when
Core deliberately installs/integrates the package.

## Accepted for integration — MGC03 Infernal Ent Trial Spatial Recipe

Owner: Module Developer / Technical Art (`module_mart05_3_finish`).
Accepted Modules commit: `db7759c` on 2026-09-12. Same package only; no main
checkout or Unity. Immutable adapter-neutral coordinates, scale, safe-lane,
Flame Trap radius/automatic/bypass and exact stable-ID bindings now cover the
approved 18.1B `BruteFinale`. Core keeps spawning, combat, AI, reward and scene
authority.

Seven focused tests are authored for complete unique beat mappings, one hazard,
safe Ent clearance, two valid bypass lanes, immutable snapshots and no Unity/game
runtime dependency. They have not run in isolation because the available host
lacks .NET 2; Unity verification belongs to the later deliberate integration.

## Accepted for integration — MGC04 Large-Creature Attack Rhythm Recipe

Owner: Module Developer / Technical Art (`module_mart05_3_finish`).
Accepted Modules commits: `4223e58`, corrected by `ca4eaf2` on 2026-09-12.
Package: `Packages/com.realmraiders.large-creature-combat-rhythm/`.

Prepare one explicit Guardian Ent attack-rhythm recipe for Core 18.2 without
duplicating damage, cooldowns, movement, target acquisition or gameplay timing.
It must express semantic ability choices and bounded eligibility facts so Core can
retain final AI authority and use the same existing CombatEntity abilities.

The accepted recipe keeps Charge direct-player-only. AI may use Basic and Area;
two explicitly configured eligible targets permit immediate Area, while one target
permits Area after two consecutive Basics. Target facts and Area range/recovery
remain sourced from existing Core authority. Package NUnit tests are authored but
Unity QA has not yet run them.

## Accepted for integration — MGC04.2 Heavy Attack Rhythm Evaluator

Owner: Module Developer / Technical Art (`module_mart05_3_finish`).
Accepted Modules commit: `37d9d36` on 2026-09-12. Same isolated no-engine package.

Move one genuinely reusable decision kernel out of future Core 18.2: immutable
input/result plus a deterministic evaluator for the accepted Guardian Ent recipe.
Zero eligible targets yields no action; the multi-target threshold yields Area;
one eligible target yields Basic until the configured consecutive-Basic threshold,
then Area. Null/malformed facts fail closed. The package must not retain state,
advance time, find targets, duplicate range/damage/cooldown values or execute an
ability. Core remains responsible for supplying explicit target/count facts,
resetting the count and invoking the existing CombatEntity action.

Unity integration QA passed as part of Core 18.2: the complete final EditMode gate
was `394/394`, including the actual 13 MGC04/MGC04.2 package tests; PlayMode was
`114/114`. The package retains no Unity, timing, target discovery or execution
authority.

## Accepted — AVFX01 Ent Ground Slam Preview and Mobile Intake

Accepted Modules commits: `544fab4` and `b00f6b1` on 2026-09-12. One original,
reference-free 1024 RGBA Sylvan root-energy ring has exact provenance and clean
zero-alpha outer edges. Deterministic 512/256 derivatives preserve the empty centre
and 64 px silhouette; the 256 candidate is the recommended first Core pilot.
Unity blend/import, short lifetime and overdraw remain a later named integration
gate; no `.meta`, material, shader or runtime binding is claimed.

## Accepted — MGC05–MGC07 Infernal Variant Production Data

Accepted Modules commits: `76b7097`, `911678a` and `2a53c36` on 2026-09-12.
Package: `Packages/com.realmraiders.infernal-encounters/` only.

- MGC05 supplies exact immutable spatial recipes for Entry Trial and Risk Route
  alongside the unchanged Brute Finale, including truthful no-hazard/one-hazard
  cardinality and safe bypass corridors.
- MGC06 supplies exact ordinal lookup and deterministic fail-closed evidence that
  spatial facts match their pacing composition, including hazard representation,
  placement, nonblocking presentation and left/right bypass semantics.
- MGC07 supplies cached canonical names and concise all-caps tactical summaries
  derived only from existing enemies, optional Flame bypass and approximate time.

The complete package tests ran in the 18.3 Unity gate; final EditMode was
`423/423`. These modules do not select, spawn, time, reward or render a raid; Core
18.4 remains the sole gameplay/UI adapter and authority.

## Accepted — MGC08 Sylvan Variant Presentation Facts

Accepted Modules commit: `d9c9fb3` on 2026-09-12. Package:
`Packages/com.realmraiders.sylvan-encounters/` only.

The cached, immutable catalogue gives Baseline, Wolf Pressure and Sentinel Escort
one exact ordinal composition lookup and concise truthful summaries derived only
from their authored roster/node facts. It introduces no selection, spawning,
rewards, persistence or Unity authority. Its focused package tests were discovered
in the 18.4 Unity batch; final EditMode was `434/434`.

## Accepted — MGC09 Sylvan Defense Reserve Facts

Owner: Module Developer / Technical Art. Workspace:
`Modules/RealmRaider.Modules` only; no main checkout or Unity.

Accepted Modules commit: `1cc4cbd` on 2026-09-12.

Prepare one small no-engine package for the next playable BUILD → DEFEND choice.
An immutable caller-supplied roster summary is evaluated into exactly one of two
cached facts:

- `PACK_PRESSURE`: two Wolves, one Guardian Ent and one Root Trap; possession
  energy remains the existing `30` seconds.
- `KEEPER_RESERVE`: one Wolf, one Guardian Ent, one Root Trap and one open creature
  slot; the sacrificed Wolf raises possession energy to `45` seconds.

The evaluator receives explicit counts only after Core validates its own layout.
It uses ordinal IDs, stable issue ordering and fail-closed invalid evidence. Facts
may include only stable ID, display name, concise tactical summary and energy
maximum. Do not copy slot geometry, Threat costs, stats, AI, trap, possession,
save or Unity behavior into the module. Add focused tests for both exact matches,
null/negative/unknown/cardinality failures, immutable snapshots and deterministic
lookup. No generic rules engine, provider framework, persistence or automatic Core
application. Commit/push remain Architect/user owned.

Outcome: the isolated no-engine package supplies cached `PACK_PRESSURE`
(2 Wolves, 30 seconds) and `KEEPER_RESERVE` (1 Wolf plus one open creature slot,
45 seconds) facts through a deterministic fail-closed roster evaluator. Static
checks pass; its focused NUnit tests are authored and enter Unity discovery with
Core 18.5. The module has no layout, save, timer, trap, possession or Unity
authority.

## Accepted for integration — MGC10 Starter Realm Layout Selection

Accepted Modules commit: `af9a97c` on 2026-09-12. Package:
`Packages/com.realmraiders.starter-realm-layouts/`.

Three cached Sylvan starter layouts expose immutable node, undirected-edge,
landmark and expansion-socket facts. Selection from a caller-owned integer seed is
deterministic and avoids an exact previous layout when asked; malformed IDs,
coordinates, topology and unsafe active paths fail closed. The package owns no
save, RNG lifecycle, scene, spawn, combat, reward or Unity behavior.

## Accepted for integration — MGC10.1 Materialization Facts

Accepted Modules commit: `d58c2cb` on 2026-09-12. The same package adds closed
node and landmark presentation roles plus finite 6–8 unit path-width facts so a
later Core adapter does not infer content from names. Static checks and independent
review are clean; package NUnit tests wait for deliberate Unity installation.

## Accepted for integration — MGC10.2 Full Sylvan Loop Compatibility

Owner: Module Developer / Technical Art. Workspace: Modules repository only.

Accepted Modules commit: `8b56968` on 2026-09-12 (`e130a95` implementation plus
the Unity-discovered complete invalid-layout issue expectation).

Before Core materializes a seeded starter map, every recipe must carry the exact
seven existing loop roles: Portal, junction, Wolf encounter, Root Path hazard, Ent
encounter, Moonwell and Heart Tree. The three recipes must retain tactically
distinct topology signatures, safe Start→Heart reachability and physical
node/corridor clearance. Add exact persisted-ID resolution for Core 18.8 without
adding persistence or selection authority. Architect reviews and commits the
frozen candidate; Unity package tests run only after main integration.

Outcome: Ancient Crossroads, Forked Canopy and Serpent Roots retain pairwise
distinct tactical role/safe-edge signatures and the complete seven-role Sylvan
loop. A fail-closed geometry validator protects the existing 3.25-unit Core node
footprint and full corridor widths from unintended route unions. The exact-ID
resolver returns only a strictly matched cached validated recipe and owns no RNG,
persistence or gameplay authority. Static checks and independent review found no
issues. Unity discovered one stale expected issue list; the corrected package gate
is included in `8b56968` and reruns with Core 18.8.

## Accepted and integrated — MGC11 Sylvan Encounter Pacing

Accepted Modules implementation `7f550b7`, with the direct Unity test-assembly
dependency correction at `3b8e0e5`. Three immutable exact-layout recipes describe
the Ancient/Forked optional branches and the Serpent Wolf → Root → Ent → Moonwell
gauntlet without owning scene, entry, combat, rewards, persistence or UI. Core
18.11 installed the package and final Unity QA passed EditMode `513/513` and
PlayMode `124/124`.

## Accepted for later integration — MGC12 Realm Growth Contracts

Accepted Modules commit `3b23c13`. A pure caller-supplied catalogue validates
monotonic level, footprint, node and expansion-capacity facts and resolves exact
tier IDs or the greatest satisfied level. It deliberately defines no production
thresholds, currency, save, map, multiplayer or Unity authority.

## Accepted for later integration — MGC13 Starter Realm Expansion Planner

Accepted Modules commit `3c11a20`. The no-engine package composes the accepted
starter-layout and growth-contract APIs into one stateless evaluator: given an
exact cached layout and caller-resolved valid tier, it returns the first N authored
expansion sockets in their original order as an immutable plan. Capacity zero is
valid; overflow, invalid identities/facts, malformed sockets and nonfinite
coordinates fail closed with stable evidence. It never chooses levels, generates
geometry, persists state or owns scene/gameplay/network behavior. Focused NUnit
coverage is authored; Unity discovery waits for a deliberate Core integration.

## Accepted and integrated — MGC14A Faction-Neutral Realm Layout Contract

Accepted Modules commit `6b5da88`. One additive no-engine contract now owns
immutable faction-neutral graph/node/edge/landmark/socket facts, ordinal role IDs
and structural validation for safe reachability and physical node/corridor
clearance. The existing Sylvan public API and exact-seven validator remain intact
through an additive field-for-field adapter. Main installed the package explicitly;
its tests passed in the Core 18.13 EditMode `526/526` gate.

## Accepted for later integration — MGC14B Infernal Starter Defense Layouts

Accepted Modules commit `2f4b728`. Ashen Spur, Cinder Fork and Ember Circuit are
three cached immutable Infernal defense graphs using only existing Entry,
Hellhound, Flame Trap, Infernal Brute and Infernal Heart concepts. Generic and
Infernal validators enforce exact roles, independent topology/safe-path
signatures, safe geometry and deterministic exact/seeded selection. The new
package remains uninstalled; focused NUnit is authored and enters Unity only with
a deliberate Core materialization gate.

## Accepted for later integration — MGC15 Infernal Defense Pacing

Accepted Modules commit `b3d1eed`. A new no-engine package maps each exact MGC14B
layout to one immutable five-beat Entry/Hellhound/Flame/Brute/Heart rhythm.
Required/optional dependency DAGs are truthful: Entry and final Heart are required,
required beats cannot depend on optional beats, and every required beat is a Heart
prerequisite. Exact lookup, safe-role reachability and pairwise semantic pacing
signatures fail closed. The package remains uninstalled; its focused NUnit tests
enter Unity only with a named Core integration.
