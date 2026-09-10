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

## Accepted — Diamond Pass 13.5: Blood Knight Ability Icons

Owner: Core developer. Base: Architect's accepted 13.4 commit. Full lease:
`Docs/NEXT_JOB.md`.

Import only the accepted original MUI03 Basic Slash, Blood Rush and Heavy Cleave
sprites with exact provenance/mobile settings. Decorate the existing three RaidHUD
ability buttons without changing labels, readiness/cooldowns, input or combat.
Missing/throwing resources retain the exact text-only controls.

Acceptance: QA's native gates passed EditMode `162/162` and PlayMode `82/82`,
both with zero failed/skipped/inconclusive tests; Console was `0/0/0`. Manual
portrait/landscape icon readability remains unobserved because the first Game-view
click failed externally with `windowNotFoundAtPosition`; no runtime failure was
recorded.

## Accepted — Diamond Pass 13.6: Guardian Ent Ability Icons

Owner: Core developer. Base: Architect's accepted 13.5 commit. Full lease:
`Docs/NEXT_JOB.md`.

Import only the accepted original MUI04 Smash, Charge and Ground Slam sprites with
exact provenance/mobile settings. Decorate only the Sylvan possessed-defender
ability controls while preserving every label, callback, readiness/cooldown rule,
possession lifecycle and exact text-only fallback. Infernal icons remain a
separate later gate. Core never controls Unity; QA alone verifies the frozen
candidate.

Acceptance: after two narrow PlayMode fixture corrections (energy baseline and
Unity destroyed-object null semantics), QA's final native gates passed EditMode
`166/166` and PlayMode `82/82`, both with zero failed/skipped/inconclusive tests;
Console was `0/0/0`. Manual Guardian/control-style readability remains unobserved
because the first Game-view click failed externally with
`windowNotFoundAtPosition`; no runtime failure was recorded.

## Accepted — Diamond Pass 13.7: Infernal Brute Ability Icons

Owner: Core developer. Base: Architect's accepted 13.6 commit. Full lease:
`Docs/NEXT_JOB.md`.

Import only the accepted original MUI05 Smash, Charge and Ground Slam sprites with
exact provenance/mobile settings. Decorate the Infernal Brute's two existing
buttons plus its truthful Fingertap-only `SWIPE: CHARGE` affordance, preserving
text, readiness/cooldowns, callbacks, control-style and possession lifecycle.
Guardian behavior remains unchanged; unknown defenders retain exact text-only
fallback. Core never controls Unity; QA alone verifies the frozen candidate.

Acceptance: QA's native gates passed EditMode `170/170` and PlayMode `82/82`,
both with zero failed/skipped/inconclusive tests; Console was `0/0/0`. Manual
Infernal/Guardian icon readability remains unobserved because the first Game-view
click failed externally with `windowNotFoundAtPosition`; no runtime failure was
recorded.

## Accepted — Diamond Pass 13.8: Realm Identity Marks

Owner: Core developer. Base: Architect's accepted 13.7 commit. Full lease:
`Docs/NEXT_JOB.md`.

Import only the accepted original MUI07 Sylvan and Infernal marks with exact
provenance/mobile settings. Add one cached, non-raycast 48 px mark to the existing
canonical realm title in Hub, Build, Raid and Defense HUDs, without turning it
into a control or weakening its text. Unknown/missing identities retain exact
text-only layout; realm refresh and teardown cannot leave stale marks.

Acceptance: after one shared 2 px adjacency correction and its exact test
expectation update, QA's native gates passed EditMode `174/174` and PlayMode
`82/82`, both with zero failed/skipped/inconclusive tests; Console was `0/0/0`.
Manual realm-mark readability remains unobserved because the first Game-view click
failed externally with `windowNotFoundAtPosition`; no runtime failure was recorded.

## Accepted — Diamond Pass 13.9: Guided Possessable Ent Locator

Owner: Core developer. Base: Architect's accepted 13.8 commit. Full lease:
`Docs/NEXT_JOB.md`.

During the active first-minute Sylvan Defense Select step, show one compact,
noninteractive, safe-area-clamped marker projected from the explicitly supplied
Guardian Ent. It hides on selection/possession/death/terminal/skip/teardown and
may reuse the same object after a real retry/forced return. It never selects,
moves the camera, changes controller authority or discovers an entity by scan.

Acceptance: QA's native gates passed EditMode `174/174` and PlayMode `82/82`,
both with zero failed/skipped/inconclusive tests; Console was `0/0/0`. Manual
marker readability remains unobserved because the first Game-view click failed
externally with `windowNotFoundAtPosition`; no runtime failure was recorded.

## Accepted — Diamond Pass 14.0: Explain Premature Explicit Release

Owner: Core developer. Base: Architect's accepted 13.9 commit. Full lease:
`Docs/NEXT_JOB.md`.

When a fresh player explicitly taps RELEASE before the ordered guide reaches its
Release step, preserve the existing factual return to Select but explain it once:
`RELEASED EARLY — SELECT THE ENT TO TRY AGAIN`. Never blame the player for forced
release, energy depletion, death, terminal state or controller loss; clear the
reason on reselect/progress/accepted release/skip/teardown.

Acceptance: QA's native gates passed EditMode `174/174` and PlayMode `82/82`,
both with zero failed/skipped/inconclusive tests; Console was `0/0/0`. Manual
possess/release/Keeper copy remains unobserved because the first Game-view click
failed externally with `windowNotFoundAtPosition`; no runtime failure was recorded.

## Accepted — Diamond Pass 14.1: Guided Loss Closes Back to Build

Owner: Core developer. Base: Architect's accepted 14.0 commit `28bf0a4`. Full
lease: `Docs/NEXT_JOB.md`.

After a correctly completed first-minute control proof ending in `RealmLost`,
guide the player through the existing RETURN TO BUILD action so the canonical
`RESULT → BUILD` loop closes and the failed defence can be adjusted. Keep DEFEND
AGAIN available as an unchanged secondary action; preserve early-terminal Retry,
victory, outcome and scene authority.

Acceptance: QA's fresh native gates passed EditMode `174/174` and PlayMode
`82/82`, both with zero failed/skipped/inconclusive tests; Console was `0/0/0`.
The final batch includes the separate fixture-order stabilization that creates an
airborne Jump test entity at its requested position before adding its controller;
runtime jump code and strict assertions were unchanged. Manual loss/result copy
remains unobserved because the first Game-view click failed externally with
`windowNotFoundAtPosition`; no runtime failure was recorded.

## Accepted — Diamond Pass 14.2: Completed Loop Primes the Next Raid

Owner: Core developer. Base: Architect's accepted 14.1 commit. Full lease:
`Docs/NEXT_JOB.md`.

Returning to RealmBuild from a factually completed canonical defense must prime
the existing journey Build stage so the existing primary action truthfully reads
`SAVE & RAID` and advances to SylvanRealm. Direct/legacy Build, early Retry and
Infernal paths remain inactive and keep `SAVE & DEFEND`.

Acceptance: QA's fresh native gates passed EditMode `174/174` and PlayMode
`82/82`, both with zero failed/skipped/inconclusive tests; Console was `0/0/0`.
Manual journey return remains unobserved because the first Game-view click failed
externally with `windowNotFoundAtPosition`; no runtime failure was recorded.

## Accepted — Diamond Pass 14.3: Raid Result Copy Matches the Next Action

Owner: Core developer. Base: Architect's accepted 14.2 commit. Full lease:
`Docs/NEXT_JOB.md`.

Journey Raid results already continue directly through DEFEND YOUR REALM, but
their copy falsely promises another planning/Build step. Align only the journey
victory/defeat sentence with the factual immediate Defense action while direct
Raid keeps its existing planning copy and PLAN NEXT DEFENSE route.

Acceptance: QA's fresh native gates passed EditMode `174/174` and PlayMode
`82/82`, both with zero failed/skipped/inconclusive tests; Console was `0/0/0`.
Manual Raid result remains unobserved because the first Game-view click failed
externally with `windowNotFoundAtPosition`; no runtime failure was recorded.

## Accepted — Diamond Pass 14.4: In-Run Control Style Mark

Owner: Core developer. Base: Architect's accepted 14.3 commit `b9a140b`. Full
lease: `Docs/NEXT_JOB.md`.

Import only the three accepted original MUI06 control-style marks with exact
provenance/mobile settings. Add one cached, non-raycast 32 px mark to the existing
in-run selector, synchronized with saved AUTO/TAP/STICK while retaining exact
text, control semantics, footprint and a complete text-only fallback.

Acceptance: QA's fresh native gates passed EditMode `178/178` and PlayMode
`82/82`, both with zero failed/skipped/inconclusive tests; Console was `0/0/0`.
Landscape manual smoke observed synchronized AUTO/TAP/STICK marks in Raid and
Defender and terminal hiding; portrait and possessed-Defender entry remain
unobserved because the UI state did not settle for those steps.

## Accepted — Diamond Pass 14.5: Hub Control Choice Marks

Owner: Core developer. Base: Architect's accepted 14.4 commit `0312034`. Full lease:
`Docs/NEXT_JOB.md`.

Reuse the accepted 14.4 sprites so the existing Hub CONTEXTUAL, FINGERTAP and
JOYSTICK buttons each carry their explicit non-raycast mark beside unchanged
text. Preserve button footprints, selection truth, actions and independent
text-only fallback; do not add a selected-state highlight.

Acceptance: QA's fresh native gates passed EditMode `178/178` and PlayMode
`82/82`, both with zero failed/skipped/inconclusive tests. Game-view imagery and
Console counters were unavailable to the automation surface, so Hub icon/text
readability and numeric Console state are not manually claimed; Editor log tail
showed no warning/error/exception entries.

## Accepted — Diamond Pass 14.6: Grounded Jump Coyote Time

Owner: Core developer. Base: Architect's accepted 14.5 commit `ab9e5b3`. Full lease:
`Docs/NEXT_JOB.md`.

Give direct players a strict 0.10-second grace after walking off a grounded edge,
using CombatEntity's existing Motor movement and jump authority. Preserve startup
airborne rejection, no double jump, every root/action/dodge/terminal/death/
controller gate and distinct Joystick/Fingertap intent.

Acceptance: after the one user-authorized controlled Editor restart repaired the
stalled compile/domain-reload pipeline, QA's menu gates passed EditMode `178/178`
and PlayMode `83/83`, both with zero failed/skipped/inconclusive tests. The new
start-ack watchdog and explicit stale-ownership command loaded and the command
correctly reported that no ownership was active. Editor log tail showed no
error/exception entries; a manual ledge smoke is not claimed because Game view
was not safely operable through the QA automation surface.

## Accepted — Diamond Pass 14.7: Jump Takeoff and Landing Readability

Add one bounded presentation-only takeoff stretch and landing settle under the
existing `Presentation Pivot`, driven by factual jump-state transitions. Gameplay
root, CharacterController, jump timing, input semantics and combat authority must
remain unchanged. Core may prepare this lease only after the 14.6 commit; input
buffering and visual-profile integration remain separate later gates.

Acceptance: QA's final native gates passed EditMode `179/179` and PlayMode
`84/84`, both with zero failed/skipped/inconclusive tests. Editor log tail showed
no error/exception entries. Manual Sylvan/Defender visual readability is not
claimed because the Game view was not safely operable through QA automation.

## Accepted — Diamond Pass 14.8: One Falling Jump Buffer

Allow exactly one short pre-landing jump request only while a direct player's
existing active jump is factually descending. Preserve immediate grounded/coyote
jump, reject repeated presses without refreshing expiry and clear the request on
every existing jump lifecycle boundary. Visual-profile integration still requires
approved non-no-op Module data rather than invented Core values.

Acceptance: after Unity's temporarily unresponsive import channel recovered
without a restart, QA's final native gates passed EditMode `179/179` and PlayMode
`85/85`, both with zero failed/skipped/inconclusive tests. Editor log tail showed
no error/exception entries. Manual pre-landing behavior is not claimed because
the Game view was not safely operable through QA automation.

## Accepted — Diamond Pass 15.0: Possession Energy Urgency Pulse

Make the existing warning and critical possession-energy states harder to miss on
a phone through one bounded, noninteractive pulse on the existing energy meter.
Preserve current text, colour, timer, release authority, responsive layout and
all gameplay timing; do not add a Canvas, object, sound or repeated alarm.

Acceptance: after one exact float-boundary correction and one explicit
possession-entry lifecycle correction, QA's final native gates passed EditMode
`180/180` and PlayMode `87/87`, both with zero failed/skipped/inconclusive tests.
Editor log tail showed no error/exception entries. Manual Sylvan/Infernal pulse
readability is not claimed because Game view was not safely operable.

## Accepted — Diamond Pass 15.1: Infernal Seam-Hardened Path Surface

Replace only the legacy MWS03 Infernal route albedo binding with the accepted
original-generated, seam-hardened MWS07 basalt candidate. Preserve all geometry,
collision, materials/fallback authority, Sylvan binding and gameplay. Import as a
reversible preview with exact provenance and mobile Repeat settings.

Acceptance: QA Assets Refresh imported the new asset without C# or import errors;
final native gates passed EditMode `182/182` and PlayMode `87/87`, both with zero
failed/skipped/inconclusive tests. Editor log tail showed no error/exception.
Manual seam/periodicity and Sylvan regression are not claimed because Game view
was not safely operable through QA automation.

## Now — Diamond Pass 15.2: Infernal Boundary Surface Identity

Apply the accepted original-generated, seam-hardened MWS08 forged black-iron and
obsidian albedo to Infernal arena boundaries only. Preserve the complete boundary
mesh/collider topology, Sylvan albedo+normal path, solid fallbacks and gameplay;
record the crimson-network periodicity as an explicit device caveat.

## Ready after 15.2

Use QA's factual Infernal/Sylvan boundary rendering and periodicity observation to
accept or return one bounded import/material correction. Normal/emission maps,
shader redesign and macro variation remain separate, evidence-led gates.
