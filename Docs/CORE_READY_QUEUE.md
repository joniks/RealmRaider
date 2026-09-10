# Realm Raiders — Core Ready Queue

This is the tracked continuation queue for the main Unity checkout. `NEXT_JOB.md` remains the full active lease; this file states what happens immediately after every intermediate result so a valid handoff cannot become silent idle time.

## Accepted — Diamond Passes 12.7–13.0

- Diamond Pass 12.7 Build-to-Defense Deployment Receipt is accepted after the user's positive manual smoke and the final automated gates.
- Diamond Pass 12.8 Camera-Relative Direct Attack Direction is accepted with deterministic legacy PlayMode gate repairs.
- Diamond Pass 12.9 Possessed-Defense Invader Awareness is accepted with factual event-driven invader intent, bounded existing awareness presentation and complete lifecycle cleanup.
- Diamond Pass 13.0 Sylvan Seam-Hardened Path Integration is accepted with byte-identical original-generated provenance, Repeat/mobile import settings and unchanged authoritative geometry/collision.
- Final Unity GUI Test Runner: EditMode `139/139`, PlayMode `79/79`, `0` failures. PlayMode XML completed 2026-09-10 06:25:24Z; scoped and complete-new-file checks are clean.
- Architect records and commits only the accepted paths; user push is asynchronous publication and does not block QA Tool 01.

## Accepted — QA Tool 01: Accessible Unity Test Gates

Owner: Core developer

Workspace: main checkout; source only, no Unity.

Base: Architect's accepted local 13.0 commit.

Reserved paths:

- `Assets/Game/Editor/RealmRaidersTestGate.cs`

Handoff: frozen scoped diff, `git diff --check`, six-line Core report; no stage, commit, push or Unity.

### Outcome

Expose two accessible Unity menu commands that start full EditMode or PlayMode runs through the installed official `TestRunnerApi`. This bypasses the Test Runner window's inaccessible internal buttons without CLI, Editor restart, window rearrangement or a second Unity process.

### Guardrails

- Add **Realm Raiders → QA → Run All EditMode Tests** and **Run All PlayMode Tests**; use public `UnityEditor.TestTools.TestRunner.Api` only.
- Each command supplies exactly one unfiltered `Filter` for its mode. It must not inspect or inherit the Test Runner search field.
- Refuse while compiling, updating, entering/inside Play Mode or while a gate launched by this tool is already active. Never start a second run or Unity process.
- Register a lightweight callback that logs factual passed/failed/skipped totals and clears the owned active state; persist only the minimum session state needed across a PlayMode domain reload.
- Do not mutate tests, Test Runner layout/filter, scenes, packages, ProjectSettings or gameplay. Do not auto-chain broad suites; Architect deliberately invokes each final gate once.
- Architect updates the QA guide after QA's direct acceptance; Core does not edit process documentation.

## Verification

Reviewer / QA + Build invoked each new menu command exactly once in the existing
healthy Editor. EditMode passed `139/139`; PlayMode passed `79/79`. PlayMode
ownership recovered across domain reload and cleared after completion. No CLI,
restart, overlapping run or Test Runner layout/filter change occurred.

## Accepted — Diamond Pass 13.1: Sylvan Living-Root Boundary Presentation

Owner: Core developer. Base: `0ae2db1`. Full lease: `Docs/NEXT_JOB.md`.

Core integrates the byte-identical accepted MWS08 Sylvan living-root albedo and
MWS09 restrained normal candidate as presentation only. The combined visual mesh
gets deterministic UVs/tangents, while every boundary vertex position, triangle,
collider, gameplay layer and shared-style material budget remains authoritative
and unchanged. Core never controls Unity; Architect statically reviews the frozen
candidate, then Reviewer / QA + Build owns focused/final suites and manual smoke.

Acceptance: QA's native gates passed EditMode `145/145` and PlayMode `80/80`.
SylvanRealm portrait and DefenderTest landscape showed restrained root relief with
no obvious seams, streaking or gaps; Portal → Crossroads movement and autonomous
defense engagement continued, and the Console remained clean.

## Accepted — Diamond Pass 13.2: Factual Encounter Entry and Clear Cue

Owner: Core developer. Base: `d5bdf91`. Full lease: `Docs/NEXT_JOB.md`.

Publish the first-entered node and its explicitly supplied alive hostile contents
through the existing raid/HUD path. Show one short, non-raycast, responsive cue for
discovery, factual hostile count and `AREA CLEAR`, with exact-once room/reward
behavior and complete terminal/teardown cleanup. This is presentation only: it
must not gate movement, discover enemies by scan/name, or change AI, combat,
camera, rewards, fog, input or route availability. Core never controls Unity;
Architect reviews the frozen diff and QA alone produces Unity evidence.

Acceptance: QA's final native gates passed EditMode `148/148` and PlayMode
`81/81`, both with zero failed/skipped/inconclusive tests; Console was `0/0/0`.
The assigned Game-view smoke remains honestly unobserved because CUA returned
`noWindowsAvailable` for every Game-view action, not because of a runtime failure.

## Accepted — Diamond Pass 13.3: Sylvan Clearing-Floor Presentation

Owner: Core developer. Base: Architect's accepted 13.2 commit. Full lease:
`Docs/NEXT_JOB.md`.

Import only the accepted MWS10 Sylvan clearing albedo and restrained normal with
exact provenance/mobile settings, then bind them only to existing circular Sylvan
node-floor presentation. Preserve every transform, mesh, collider, visit radius,
fog/encounter/route/combat behavior and exact missing-resource fallback. Core does
not control Unity; Architect statically reviews and QA alone verifies the frozen
candidate.

Acceptance: after one test-only source/import-dimension correction, QA's native
gates passed EditMode `154/154` and PlayMode `82/82`, both with zero
failed/skipped/inconclusive tests; Console was `0/0/0`. Manual surface scale,
normal and periodicity remain unobserved because the first Game-view click failed
with `windowNotFoundAtPosition`; this is not an automated runtime failure.

## Accepted — Diamond Pass 13.4: Factual Encounter Cue Icons

Owner: Core developer. Base: Architect's accepted 13.3 commit. Full lease:
`Docs/NEXT_JOB.md`.

Import the accepted original MUI02 icon trio with exact provenance/mobile sprite
settings. Add one non-raycast phase icon to the existing `RaidEncounterCue` while
keeping truthful text primary, responsive layout intentional and every timeout,
terminal and teardown rule unchanged. No gameplay/input/camera/reward authority.

Acceptance: QA's native gates passed EditMode `158/158` and PlayMode `82/82`,
both with zero failed/skipped/inconclusive tests; Console was `0/0/0`. Manual
40–44 px meaning/contrast/layout remains unobserved because the first Game-view
click failed externally; no runtime failure was recorded.

## Now — Diamond Pass 13.5: Blood Knight Ability Icons

Owner: Core developer. Base: Architect's accepted 13.4 commit. Full lease:
`Docs/NEXT_JOB.md`.

Import only the accepted original MUI03 Basic Slash, Blood Rush and Heavy Cleave
sprites with exact provenance/mobile settings. Decorate the existing three RaidHUD
ability buttons without changing labels, readiness/cooldowns, input or combat.
Missing/throwing resources retain the exact text-only controls.

## Ready after 13.5

Use QA's actual 32–48 px comprehension/contrast observation for one bounded
correction or accept the set. Then choose Guardian Ent MUI04 as one separate
DefenderHUD gate; do not bundle Infernal MUI05 in the same change.
