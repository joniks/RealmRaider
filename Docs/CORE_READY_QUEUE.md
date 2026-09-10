# Realm Raiders — Core Ready Queue

This is the tracked continuation queue for the main Unity checkout. `NEXT_JOB.md` remains the full active lease; this file states what happens immediately after every intermediate result so a valid handoff cannot become silent idle time.

## Accepted — Diamond Passes 12.7–12.9

- Diamond Pass 12.7 Build-to-Defense Deployment Receipt is accepted after the user's positive manual smoke and the final automated gates.
- Diamond Pass 12.8 Camera-Relative Direct Attack Direction is accepted with deterministic legacy PlayMode gate repairs.
- Diamond Pass 12.9 Possessed-Defense Invader Awareness is accepted with factual event-driven invader intent, bounded existing awareness presentation and complete lifecycle cleanup.
- Final Unity GUI Test Runner: EditMode `137/137`, PlayMode `79/79`, `0` failures. PlayMode XML completed 2026-09-10 05:31:27Z; scoped `git diff --check` is clean.
- Architect records and commits only the accepted paths; user push is asynchronous publication and does not block 13.0.

## Now — Diamond Pass 13.0: Sylvan Seam-Hardened Path Integration

Owner: Core developer

Workspace: main checkout; source only, no Unity.

Base: Architect's accepted local 12.9 commit.

Reserved paths:

- `Assets/Game/Resources/Art/WorldSurfaces/MWS07-SylvanPath/`
- `Assets/Game/Editor/PrototypeWorldSurfacePreviewImport.cs`
- `Assets/Game/Scripts/Core/RealmRoutePresentation.cs`
- `Assets/Game/Tests/EditMode/RealmRouteSurfaceMaterialPreviewTests.cs`
- `Assets/Game/Tests/PlayMode/RealmRouteSurfaceScenePreviewTests.cs`

Handoff: frozen scoped diff, `git diff --check`, six-line Core report; no stage, commit, push or Unity.

### Outcome

Replace only the current provisional Sylvan walkable-path albedo with the accepted seam-hardened MWS07 Sylvan stone candidate so the first realm reads as intentional fantasy terrain at gameplay scale. Preserve exact geometry, collision, route ownership and fallback behavior.

### Guardrails

- Copy only `Modules/RealmRaider.Modules/ArtPreviews/MWS07-WalkableSurfaceSeamHardening/sylvan-stone-path-edge-hardened-candidate.png` plus a concise local provenance record into the reserved main-project resource folder; the Modules source stays read-only.
- Import as mobile albedo: sRGB, mipmaps, bilinear, Repeat, non-readable, Android ASTC 6×6 and maximum 512. Do not import the 2×2 evidence image.
- Keep one lazily shared material per existing Sylvan role; no per-frame allocations, new shader/package, runtime file access or scene scan.
- Preserve the exact solid-colour fallback when the resource is absent or loading throws.
- Prove exact resource/provenance binding, import settings, material caching, Repeat wrap, no colliders on presentation children and unchanged authoritative transform/mesh/collider.
- Do not change Infernal art, gameplay/collision, bootstraps/scenes, camera, UI, input, AI, persistence, lighting or boundary material.

## Next gate — Architect verification

After Core freezes 13.0, Architect performs static/provenance preflight, focused surface import/binding coverage, then exactly one final EditMode and one final PlayMode run. A concrete failure returns to the same lease; green evidence advances immediately to acceptance records, commit and the Android GUI export without restarting a healthy Editor.

## Ready after 13.0

Prepare the matching Infernal walkable-path integration from the accepted MWS07 candidate, but do not modify its files until the Sylvan material has passed visual scale/readability and collision gates.
