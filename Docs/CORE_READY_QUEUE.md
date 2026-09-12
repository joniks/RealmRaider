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

## Accepted — Diamond Pass 15.2: Infernal Boundary Surface Identity

Apply the accepted original-generated, seam-hardened MWS08 forged black-iron and
obsidian albedo to Infernal arena boundaries only. Preserve the complete boundary
mesh/collider topology, Sylvan albedo+normal path, solid fallbacks and gameplay;
record the crimson-network periodicity as an explicit device caveat.

Acceptance: Architect confirmed byte identity, valid provenance, unique GUIDs,
mobile import settings, style-local cache/fallback behavior and unchanged
Sylvan/geometry/collider scope. QA final native gates passed EditMode `185/185`
and PlayMode `87/87`, both with zero failed/skipped/inconclusive tests. No files
changed after the final suites. Manual Game-view and physical-device appearance
remain unclaimed.

## Accepted — Diamond Pass 15.3: Possession Arrival Impact

Strengthen the most important Keeper → same-creature takeover moment with one
bounded, presentation-only arrival accent that complements the existing slow beat,
pulse, camera transition and confirmation audio without moving the gameplay root,
changing possession timing/authority, adding assets, or producing repeated effects.
Core must first inspect the existing possession/presentation paths and freeze a
minimal lifecycle-clean contract before this ready gate is activated.

Acceptance: after one lifecycle-coverage correction and one no-ground fixture
invariant correction, QA final native gates passed EditMode `186/186` and PlayMode
`88/88`, both with zero failed/skipped/inconclusive tests. No files changed after
the final suites. Manual silhouette readability remains unclaimed.

## Accepted — Diamond Pass 15.4: Infernal Courtyard Floor Surface

Integrate the accepted Modules `2f5457f` MWS11 Infernal courtyard-floor albedo and
normal pair onto the factual `Volcanic Floor` only. Preserve MWS07 on the four
causeway plates, MWS08 on boundaries, all Sylvan paths and every gameplay/collider
contract. The implementation keeps one shared material but uses renderer-local
property blocks for four-world-unit square tiling, so differently sized roots do
not leak scale into one another.

Acceptance: Architect confirmed exact assets/import/provenance, atomic fallback,
surface isolation and unchanged authoritative geometry. After correcting one
test-only production-collider trigger assumption, QA final gates passed EditMode
`190/190` and PlayMode `89/89`, both with zero failures. Manual device appearance
remains unclaimed.

## Accepted — Diamond Pass 15.5: Defeat Presentation Keeps Gameplay Root Authoritative

Replace the current death-time `CombatEntity` root shrink with one bounded
presentation-pivot defeat settle. The visual result must remain readable, but the
entity root transform, CharacterController dimensions and gameplay geometry may
never change. Preserve death callbacks, possession release, results, combat timing
and all existing controller cleanup.

Acceptance: Architect confirmed the sole factual death caller, bounded unscaled
settle, stable pose and exact root/CharacterController preservation. After one
EditMode lifecycle-fixture correction, QA final gates passed EditMode `192/192`
and PlayMode `89/89`, both with zero failures. Manual Game-view readability remains
unclaimed.

## Accepted — Diamond Pass 15.6: Restrained Route Normal Pair Integration

Integrate only the accepted Modules `e4d1358` MWS12 Sylvan and Infernal route
normal candidates beside their existing MWS07 albedos. Each realm keeps one atomic,
independently cached pair and exact solid fallback; MWS10/MWS11 floors, MWS08
boundaries and every geometry/collider/gameplay contract remain isolated.

Acceptance: after correcting two malformed new GUIDs and two verification-only
assertions, QA's final native gates passed EditMode `197/197` and PlayMode `90/90`,
both with zero failed/skipped/inconclusive tests. Manual route relief remains
unobserved; physical-device strength and periodicity are not claimed.

## Closed safely — Diamond Pass 15.7: Blood Knight Humanoid Rig Probe

The explicit 3DRT Humanoid mapping failed with Unity's factual Avatar error
`Transform 'Bip01' not found in HumanDescription`. Core restored both FBX imports
and the active hero to the exact accepted Generic/no-Animator state; no broken
runtime or import change was retained. Offline rig repair remains a later art-pipeline
option rather than a prerequisite for prototype motion.

## Accepted — Diamond Pass 15.8: Modular Procedural Blood Knight Motion Pilot

Install accepted Modules motion contracts and the bounded procedural six-bone
driver. A thin Core adapter maps factual root displacement, action phase, jump,
damage and death into visual-only local bone rotations under the existing Base
Body; gameplay root, CharacterController, Presentation Pivot, camera, input and
combat timing remain authoritative and unchanged.

Acceptance: after two Module test-compilation corrections, one EditMode lifecycle
fixture correction and one PlayMode float-tolerance correction, QA's final native
gates passed EditMode `244/244` and PlayMode `91/91`, both with zero failed/skipped/
inconclusive tests. The post-gate log tail had no new error/exception. Manual
portrait/landscape motion quality remains unobserved because QA could not safely
operate the Game view.

## Accepted — Diamond Pass 15.9: Blood Knight Device Motion Tune

Android evidence showed that the imported Knight appeared to move backwards and
that the first procedural pose was not noticeable. The accepted correction adds
one neutral-by-default visual Base Body fit, gives only Blood Knight a 180-degree
local yaw, and selects the bounded Module `BloodKnightDeviceReadable` preset. The
compatibility profile retains every 15.8 value; the stronger preset remains within
30 degrees and changes only the exact six bound bone rotations.

Acceptance: after removing a direct package dependency from the Core test
assemblies, QA's final native gates passed EditMode `248/248` and PlayMode `92/92`,
both with zero failed/skipped/inconclusive tests. The post-gate log tail was clean.
QA could not safely enter or control the Game view, so physical-device facing and
motion readability remain the next user-owned observation.

## Now — MART05 Guardian Ent archive intake

The user selected the creator-published Tennessippi Free Treant Pack. Acquire only
the exact official `Treant Package.7z`, preserve the creator page and embedded
licence evidence, then run the accepted offline fail-closed inventory helper from
Modules commit `45bdf67`. No mirror, substitute, extraction, import or licence
approval is authorized before the exact archive is locally supplied and reviewed.

## Ready after MART05

Choose Tree01 or Tree02 against the measured mobile budgets, freeze a LargeCreature
fit/rig/animation manifest, and activate one visual-only Guardian Ent Core binding.
Keep gameplay on the existing entity root and CharacterController, disable source
colliders and root motion, and preserve possession and cultivation continuity.

## Accepted parallel correction — Diamond Pass 15.10: Blood Knight Motion Plane

Android follow-up confirmed that 15.9 made motion visible but the Bip01 local-X
rotations read sideways/inward. Modules commit `6769716` adds a closed safe-axis
contract: CompatibilityDefault retains exact local-X behavior, while only the
Blood Knight preset uses local-Z forward/back counter-swing and one bounded
asymmetric six-bone takeoff pose. No Core call site, gameplay jump timing, root,
pivot, controller, input, camera, Animator or physics behavior changed.

Acceptance: QA final gates passed EditMode `250/250` and PlayMode `92/92`, both
with zero failed/skipped/inconclusive tests; the post-gate log tail was clean.
Landscape Sylvan loaded the exact Knight and mobile UI, but automation could not
safely inject Game-view input, so physical-device stride and takeoff quality remain
the user's next observation.

## Accepted — Diamond Pass 15.11: Blood Knight Readable Stride and Staged Jump

The accepted Modules `501c812` preset doubles the Blood Knight's local-forward
opposing stride without clipping and supplies deterministic continuous jump-pose
sampling. Core adds one factual shared presentation timeline for both pivot and
six-bone consumers: 0.30-second crouch-to-push, 0.40-second takeoff, 0.40-second
fall blend and 0.56-second landing recovery. Sparse frames, exact endpoints and
early grounding remain continuous while gameplay physics, input, root, controller,
collider, combat and camera stay authoritative and unchanged.

Acceptance: after one QA-exposed sparse-frame landing correction, final native
gates passed EditMode `251/251` and PlayMode `92/92`, both with zero failed,
skipped or inconclusive tests; the post-gate log tail contained no errors or
exceptions. Android export was explicitly omitted. Final motion feel remains the
user's direct Unity Game-view acceptance.

## Accepted — Diamond Pass 15.12: Factual Character Motion Dynamics

Replace global-clock marching with one shared per-entity presentation state driven
only by factual horizontal displacement and yaw. The state feeds the bounded pivot
and existing six-bone driver once per timestamp, eases starts/stops, returns to
exact neutral and clears on every controller, root, terminal, death and visual
lifecycle boundary. Gameplay movement, root, collider, input, camera and combat
authority remain unchanged.

Acceptance: after two missing import corrections and one factual zero-progress
jump-test stabilization, focused motion and real-jump checks passed. QA final gates
passed EditMode `255/255` and PlayMode `92/92`, with no post-gate errors or
exceptions. No Android export or manual smoke was performed.

## Accepted — Diamond Pass 15.13: Sagittal Stride and Directional Combat Motion

Modules commit `7aae5d6` supplies a backward-compatible continuous combat-pose
sample and six character-oriented semantic hinges. Core explicitly binds the
actual Base Body to its owned Presentation Pivot, snapshots accepted action and
damage facts, and drives continuous windup, impact, follow-through and hit recoil.
No gameplay root, CharacterController, movement, camera, input, targeting, damage,
cooldown or combat-phase timing authority moved into presentation.

Acceptance: focused QA passed combat presentation `10/10`, adapter `9/9` and the
actual imported Blood Knight Play path `1/1`. Final native gates passed EditMode
`276/276` and PlayMode `92/92`, with no Console/Editor-log errors or exceptions.
The user's Game-view check remains authoritative for knee, weapon-side and stride
feel.

## Accepted — Diamond Pass 15.14: Blood Knight Upper-Torso Counterweight

Modules commit `e02e9a4` adds one optional baseline-relative upper-torso transform
without changing legacy six-bone output. Core opts in only when the actual
skin-weighted `Bip01 Spine1` passes exact ownership, ancestry, scale and axis
checks; every failure retains the accepted limb motion. Existing factual clocks
drive walk ≤2°, attack ≤8° and hit ≤5°, while jump/death remain neutral at the
torso and all gameplay transforms stay unchanged.

Acceptance: focused Module `30/30`, main Adapter `17/17` and actual imported hero
Play `1/1` passed. Final native gates passed EditMode `288/288` and PlayMode
`92/92`; the post-gate log contained no errors/exceptions. Game-view aesthetics
remain user-owned.

## Accepted — Diamond Pass 15.15: Guardian Ent Tree01 Visual Pilot

Modules `a075494` freezes the exact Tennessippi Free Treant Pack/Tree01 identity,
creator-page CC0 evidence, selected light textures and measured import/budget facts.
Core binds one visual-only Guardian Ent prefab through the existing
LargeCreature recipe while preserving the same entity, CharacterController,
possession, cultivation and exact primitive fallback. The over-budget 5,438-triangle
source is a temporary prototype exception only; no source physics, colliders,
Animator, root motion or unverified animation may enter gameplay. Architect reviews
the frozen diff; QA's explicit builder created one factual static renderer, and
the final native gates passed EditMode `300/300` and PlayMode `93/93`. A light
Tree01 was visible without pink material or obvious clipping. Cultivation,
possession/release and true portrait/landscape interaction remain a user-owned
visual smoke. Android export remains excluded.

## Accepted — Diamond Pass 15.16A: Guardian Ent Generic-Rig Feasibility Probe

Modules commits `bda0b86`, `541a7a4` and `8978fe0` add fail-closed Tree01
production and reusable LargeCreature-motion readiness contracts. Core's isolated
Editor-only exact-source probe creates no runtime recipe dependency and leaves the
accepted static Guardian prefab untouched. Its generated evidence proves one
3,010-vertex, 31-bone/31-bindpose SkinnedMeshRenderer with at most four normalized
weights and in-place deformation for Idle, Run, Attack_1 and Death1. Idle and Run
have exact sampled endpoint closure; Death1 retains a distinct final pose.

Acceptance: the probe builder produced `MEASURED`; QA's full EditMode gate passed
`320/320`, including probe `11/11` and LargeCreature readiness `9/9`, with zero
failed/skipped/inconclusive tests and no error in the final run span. Manual clip
preview, foot contact, visible loop quality and Death1 held-pose aesthetics remain
honestly unobserved, so no runtime animation approval is implied.

## Accepted — Diamond Pass 15.16B: Modular Guardian Ent Motion Integration

Activate one reversible Guardian-only visual binding for the proven Generic
Tree01 hierarchy: Idle/Run, one factual Attack action and Death1, with the existing
bounded hit response retained because the source has no hit clip. Root motion,
Animation Events and gameplay callbacks stay disabled; `CharacterVisualMotion`
remains sole Presentation Pivot writer, and the accepted static Tree01 plus exact
primitive fallback remain deterministic. QA must verify one frozen integration
with final EditMode/PlayMode and the user owns final motion-feel approval.

Production optimisation remains separate: true LOD0/1/2 caps
(`4000/2000/800`) and cleaned four-weight source skinning are still required before
Tree01 can be called production-ready.

Acceptance: the exact-source runtime builder persisted a valid named binding;
after correcting only stale-assembly and EditMode test mechanics, QA's final native
gates passed EditMode `328/328` and PlayMode `94/94`, both with zero failed,
skipped or inconclusive tests. The actual Guardian motion/possession/cultivation
path and retained static fallback passed. Manual Game-view motion feel remains
honestly unobserved.

## Accepted — Diamond Pass 16.0: Factual Raid Loot Feedback

Make the already-authoritative raid rewards visible at the moment they are earned:
publish one immutable reward fact from the existing exact-once room/enemy/core
credit paths and present a short non-raycast world/HUD cue such as `+15 GOLD` or
`+100 GOLD • +1 RARE`. The existing reward amounts, result totals, persistence,
enemy death, objective and scene flow remain unchanged. Duplicate death/result/
teardown callbacks must never duplicate a cue or credit. Missing UI retains the
current silent-but-correct reward behavior. This is feedback, not a pickup,
inventory, drop table, magnet, economy or balance system.

Acceptance: QA first verified fresh Runtime/EditMode/PlayMode assemblies. Final
native gates passed EditMode `333/333` and PlayMode `94/94`, both with zero
failed/skipped/inconclusive tests and no post-gate compiler error or exception.
Manual Game-view cue readability remains honestly unobserved.

## Accepted — Diamond Pass 16.1: Earned Cultivation Affordance

Make the prototype's only real loot-to-upgrade payoff unmistakable on the existing
Build screen. Map loaded Realm Progress to exact `MISSING`, `READY` or `CAPPED`
status; only the existing Guardian Ent cultivation button receives truthful copy,
interactability and a fixed bounded ready tint. A successful existing purchase
must immediately refresh stores, rank and status. Preserve the exact costs, rank
cap, persistence schema, defense stats, saved layout and navigation. This adds no
new UI object, upgrade, currency, tutorial, animation, asset or reward dependency.

Acceptance: QA first verified fresh Runtime/EditMode/PlayMode assemblies. Final
native gates passed EditMode `334/334` and PlayMode `94/94`, both with zero
failed/skipped/inconclusive tests and no compiler error or exception. Manual Build
readability remains unobserved because the Game view was inaccessible. Repeated
two-AudioListener test warnings are recorded as a separate quality defect.

## Accepted — Diamond Pass 16.2: Factual Defense Result Debrief

Freeze one immutable result fact on the first authoritative defense terminal
transition and append only factual outcome, elapsed duration, invader remaining
health and Realm Core danger to the existing result text. Later callbacks,
orientation and refresh cannot overwrite or duplicate the debrief. Preserve the
first-minute guide suffix and existing result actions. This adds no score, reward,
history, causal diagnosis, telemetry, UI object, economy or combat change.

Acceptance: QA verified fresh assemblies; final native gates passed EditMode
`337/337` and PlayMode `94/94`, both with zero failed/skipped/inconclusive tests
and no compiler error or exception. Manual result readability remains unobserved.
The repeated two-AudioListener warning persisted with 669 entries and is isolated
as the next root-cause investigation rather than silently bundled into 16.2.

## Accepted — QA Quality Gate 02: AudioListener Warning Root Cause

Prove which exact runtime or test-fixture lifecycle creates overlapping active
AudioListeners during the full PlayMode gate. Begin read-only. Only after one
reproducible owner is identified may Architect lease the smallest correction.
Preserve camera, audio, scene and test authority; do not suppress the warning,
disable audio globally, add a singleton or weaken assertions merely to quiet logs.

Acceptance: read-only evidence isolated the warning to one non-spatial encounter
fixture that added its own listener after PrototypeHub already supplied the valid
scene listener. The one-file test correction left runtime untouched. QA's final
PlayMode gate passed `94/94` with zero failures and zero new duplicate-listener
warnings; EditMode was intentionally not repeated.

## Accepted — Diamond Pass 16.3: Earned Cultivation Result Handoff

When the first authoritative Sylvan terminal result freezes, reuse the existing
loaded cultivation affordance truth. Only `READY` appends one factual line to the
existing result text that points to the unchanged explicit Build purchase. Missing,
capped, malformed fallback and all Infernal results remain unchanged. Preserve the
first-minute suffix, orientation layout, result actions, stores and player purchase
authority. No new UI object, economy rule or automatic spend.

Acceptance: QA verified fresh assemblies. Final native gates passed EditMode
`339/339` and PlayMode `94/94`, both with zero failed/skipped/inconclusive tests.
The measured result-text fit remained inside its authored portrait/landscape region;
no compiler error, exception or new duplicate-listener warning occurred. Manual
result-to-Build readability remains unobserved.

## Accepted — Diamond Pass 16.4: Factual Incoming-Attack Cue

During exact accepted hostile Windup against the current direct player, reuse the
existing singular non-raycast threat plate/edge tab to show factual `INCOMING`
copy. Key it by attacker and ActionId and clear at phase/lifecycle boundaries.
Preserve generic threat fallback, recent-damage urgency, camera/player/targeting
authority and all combat timing. No new UI object or gameplay change.

Acceptance: after two focused QA defect cycles and one Architect-caught recursive
reconciliation risk, the final fresh native gates passed EditMode `339/339` and
PlayMode `96/96`, both with zero failed/skipped/inconclusive tests. Retarget,
same-threat pulse history, ActionId/fallback and authority cleanup all passed; no
recursion, NRE, compiler/runtime error or new duplicate-listener warning occurred.

## Accepted — Diamond Pass 16.5: Truthful Successful-Dodge Confirmation

Make Health report whether its existing damage path applied. Only applied ability
hits may produce hit/damage/knockback/impact feedback; a direct player protected by
the existing dodge-immunity window instead receives one bounded, collider-free
`DODGED` confirmation through existing CombatFeedback. Preserve all gameplay
timing, damage values, authority and non-ability trap behavior.

Acceptance: QA verified fresh assemblies. Final native gates passed EditMode
`340/340` and PlayMode `97/97`, both with zero failed/skipped/inconclusive tests.
Dodge coverage passed `4/4`; no stale marker, compiler/runtime error or new
duplicate-listener warning occurred. Manual incoming-to-dodge feel remains
unobserved.

## Accepted — Diamond Pass 16.6: Direct-Control Critical Health Readability

Reuse only the existing raid hero-health and defense possessed-creature health
labels to make the factual one-hit-from-death state unmistakable. At valid living
direct-control health at or below exactly 25%, show compact `LOW HP` copy and one
fixed warning tint. Restore the exact neutral presentation on recovery above the
boundary, Keeper view, release, death, terminal state and teardown. Never decorate
the defense invader-health label. No new UI object or gameplay authority.

Acceptance: fresh assemblies; final native gates passed EditMode `341/341` and
PlayMode `99/99`, both with zero failed/skipped/inconclusive tests. Controller,
possession, death, terminal, orientation and invader-isolation coverage passed;
there was no compiler/runtime error or new duplicate-listener warning. Manual
portrait/landscape readability remains unobserved.

## Accepted — Diamond Pass 16.7: Factual Creature-Return Receipt

On successful explicit release only, after the same living entity factually
restores its active `CreatureBrain`, reuse the existing bounded MomentFeedback
notice to confirm that named creature resumed defense and show its unchanged
current/maximum HP. Forced release, death, energy depletion, absent/inactive AI,
invalid data, repeated release and terminal UI must preserve existing fallback or
silence. No new UI object, event, timer or gameplay authority.

Acceptance: fresh main and Modules assemblies; final native gates passed EditMode
`347/347` and PlayMode `101/101`, both with zero failed/skipped/inconclusive tests.
Same-entity, unchanged-HP, active-AI, no-AI, repeated, forced death/energy and
terminal win/loss paths passed without compiler/runtime errors or new
duplicate-listener warnings. Manual readability remains unobserved.

## Accepted — Diamond Pass 16.8: Factual Possessed-Creature Defeat Receipt

Only the authoritative death of the currently possessed entity may replace the
generic forced-return copy with a factual named defeat and zero/maximum HP receipt.
Reuse the existing MomentFeedback notice; preserve same-entity death, camera and
controller cleanup. Invalid, non-dead, terminal, energy, explicit-release,
win/loss and repeated paths retain their exact current copy or silence. No new UI,
timer, event, state or gameplay authority.

Acceptance: fresh main and Modules assemblies; final native gates passed EditMode
`352/352` and PlayMode `102/102`, both with zero failed/skipped/inconclusive tests.
Same-entity death, exact HP/name, inactive controllers, camera return, orientation,
timeout and release-cause separation passed without compiler/runtime errors or new
duplicate-listener warnings. Manual readability remains unobserved.

## Accepted — Diamond Pass 16.9: Factual Direct-Combat No-Hit Confirmation

After one accepted direct-player positive-damage Melee or Area action reaches its
authoritative impact overlap, reuse the existing CombatFeedback world-marker pattern
for one bounded `NO HIT` only when zero eligible living contacts were found. Applied
damage keeps ordinary feedback; an eligible contact that rejects damage keeps only
`DODGED`. AI, Dash, invalid damage, canceled/rejected actions and lifecycle exits
never show it. No range, cooldown, timing, targeting, camera or input change.

Acceptance: fresh main and Modules assemblies; final native gates passed EditMode
`357/357` and PlayMode `104/104`, both with zero failed/skipped/inconclusive tests.
The exact empty-impact, applied-hit, immunity, AI/Dash/invalid/canceled and lifecycle
paths passed without compiler/runtime errors or new duplicate-listener warnings.
Manual marker readability remains unobserved.

## Accepted — Diamond Pass 17.0: Explicit Blood Knight Motion-Tuning Resolution

Owner: Core developer on GPT-5.6 Sol. Replace the adapter's hard-coded tuning
selection with the accepted explicit MMP11 Blood Knight assignment resolved against
the accepted starter provider/catalogue. Preferred resolution must preserve the
exact current `BloodKnightDeviceReadable` object and therefore every accepted pose,
timing and gameplay invariant. Any rejected/unavailable assignment fails closed to
the exact compatibility tuning without discovery, scene scans or module-owned
gameplay authority. This is a narrow modularity gate, not a motion retune.

Acceptance: objectively fresh assemblies; final native gates passed EditMode
`358/358` and PlayMode `104/104`, both with zero failed/skipped/inconclusive tests.
Preferred identity, explicit Compatibility fallback, rejected/null fail-closed and
actual bind/rebind ownership passed without compiler/runtime errors or new
duplicate-listener warnings. No manual motion check was required because no tuning
value or pose behavior changed.

## Accepted — Diamond Pass 17.1: Factual Direct-Combat Defeat Confirmation

Owner: Core developer on GPT-5.6 Sol. When an accepted direct-player Melee or Area
hit applies damage and that same hit authoritatively transitions an eligible living
enemy to dead, show one bounded factual `DEFEATED — <NAME>` through the existing
world-marker presentation. Preserve damage, reward, death, AI and encounter
authority exactly. AI kills, already-dead/invalid targets, immunity, misses,
repeated colliders/callbacks and lifecycle exits cannot duplicate or invent it.

Acceptance: after replacing only an ungrounded multi-stage test fixture with one
owned ground surface, objectively fresh final gates passed EditMode `359/359` and
PlayMode `106/106`, both with zero failed/skipped/inconclusive tests. Direct and
multi-target lethal transitions, deduplication, exclusions and lifecycle cleanup
passed without compiler/runtime errors or new duplicate-listener warnings.

## Accepted — Diamond Pass 17.2: Singular Latest-Damage Marker

Each target owns at most one active `Combat Damage` marker. A later applied hit
reuses it with the latest exact value and point and restarts the bounded 0.65-second
lifetime; distinct targets remain independent and every existing hit effect stays
unchanged.

Acceptance: after one test-only deferred-`Destroy` timing correction, fresh final
gates passed EditMode `359/359` and PlayMode `107/107`, both with zero
failed/skipped/inconclusive tests. Reuse, latest data, refreshed expiry,
multi-target independence and all cleanup paths passed without compiler/runtime
errors or new duplicate-listener warnings.

## Closed — 17.3 Read-Only Audit

The audit produced no implemented feature. Its device-smoke dependency does not
block development and is superseded by the user's 2026-09-12 concrete feedback.

## Accepted — 18.0 Ent Grounding and Landing Recovery (foundation/polish)

Owner: Core developer. Base `d861f9c`. Full reserved paths and acceptance are in
`Docs/NEXT_JOB.md`. Correct imported Ent forward/foot fit in animated and static
paths, and add factual jump/non-jump-fall landing compression and recovery.
Architect static review then QA-only Unity; no export in this iteration.
QA final EditMode 359/359 and PlayMode 108/108 passed with no failed/skipped/
inconclusive tests or new compiler/runtime errors. Manual visual feel remains
unobserved because the accessible Game surface lacks Play/pixel/input controls;
this precise evidence boundary does not block 18.1. No Android export requested.

## Accepted — 18.1 Sylvan Risk/Reward Route Choice (gameplay/content)

Activated after accepted 18.0 commit `2bf8f4f`; exact lease is in NEXT_JOB.
Use the existing Moonwell node for one real bounded recovery per raid; keep its
charge when the hero is full and prohibit resurrection/terminal healing. Give the
explicit optional Sylvan Ent encounter one extra Rare Material on its factual
defeat through existing exact-once raid rewards. This creates a reason to take the
dangerous detour and a limited recovery resource for continuing to Heart Tree.
Keep control-mode gestures and save/result credit ownership intact. Reserve exact
paths in NEXT_JOB before implementation. Starting balance and follow-ups are in
`Docs/GAMEPLAY_ROADMAP.md`; no new model/package is required by this gate.

Core's read-only audit identifies this narrow reservation for activation:
`Core/SylvanRealmBootstrap.cs`, `Raid/RaidManager.cs`, `UI/RaidHUD.cs`,
`Combat/Health.cs`, new `Realm/MoonwellRecovery.cs`, and focused Edit/Play tests
under `Assets/Game`. Health needs one finite positive capped heal that reports
actual restored HP without resetting immunity or resurrecting. RaidManager can
receive an explicit optional bonus-enemy reference and reuse creditedEnemies plus
existing EnemyDefeat receipts; no new reward type or identity-by-name lookup.
The HUD consumes the explicitly supplied well and keeps one visible, deliberate
use action in the existing responsive root.

Acceptance: the existing Moonwell now has one deliberate 30%-maximum-health
recovery charge with full/out-of-range/dead/disabled/terminal no-waste behavior,
and the exact optional Ent grants one additional Rare Material through the existing
exact-once reward/result pipeline. Two strict-float EditMode assertions and one
global PlayMode button lookup were corrected without changing runtime behavior.
Final QA passed EditMode `364/364` and PlayMode `109/109`, zero failed/skipped/
inconclusive, with no new compiler/runtime errors. Manual Moonwell/Ent feel remains
unobserved because the accessible Game surface exposes no viewport or controls.

## Active — 18.1B Infernal Raid with Ent Hero (gameplay/content)

User requested on 2026-09-12. Activate immediately after 18.1 acceptance. Full
outcome, proposed paths and QA gate: `Docs/INFERNAL_ENT_RAID_JOB.md`. Separate
InfernalRaid scene and Hub entry, direct Ent, two Hellhounds, bypassable fire
hazard, Brute guarding the Heart and correct same-scene retry. Core alone owns
main integration; shared RaidHUD cannot be edited alongside 18.1.

Core activates immediately from the accepted 18.1 commit. Modules `8ee1d95` and
`db7759c` provide reviewed no-engine pacing and spatial facts; Core may explicitly
install and adapt them but retains all spawning, Heart-gate, AI, combat, scene,
reward and input authority. The first playable trial remains the exact BruteFinale
with one Flame Trap and a Brute-only Heart prerequisite.

## Following — 18.2 Ent Attack Rhythm (gameplay/content)

Dependency: accepted 18.1B commit. Add a narrowly opted-in Ent AI attack pattern
using existing basic Smash and Ground Slam: readable anticipation, evadable impact
and a recovery opening. Preserve the shared action gate; no simultaneous attacks,
input steering, auto-player attacks or changed possession identity. Other brains
retain their current behavior unless explicitly configured. Core prepares exact
paths/pattern parameters read-only; Architect writes the lease before activation.
