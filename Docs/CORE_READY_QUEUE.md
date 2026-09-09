# Realm Raiders — Core Ready Queue

This is the tracked continuation queue for the main Unity checkout. `NEXT_JOB.md` remains the full active lease; this file states what happens immediately after every intermediate result so a valid handoff cannot become silent idle time.

## Current verification batch — Diamond Passes 12.7 and 12.8

- Diamond Pass 12.7 Build-to-Defense Deployment Receipt is source-frozen and has a positive user manual smoke. Its focused and final automated evidence remains pending.
- The user explicitly authorized one small follow-up before that final evidence: Diamond Pass 12.8 Camera-Relative Direct Attack Direction.
- Architect has approved one combined final verification batch after 12.8 freezes. The frozen 12.7 files remain outside the 12.8 write lease.

## Now — Diamond Pass 12.8: Camera-Relative Direct Attack Direction

Owner: Core developer

Workspace: main checkout; source only, no Unity.

Base: local `46bd32a` plus the frozen, uncommitted 12.7 candidate.

Reserved paths: `Assets/Game/Scripts/Controllers/PlayerController.cs`, `Assets/Game/Tests/PlayMode/CombatCameraReadabilityTests.cs`; `Assets/Game/Scripts/Characters/CombatEntity.cs` only if the focused held-movement test proves that active movement overwrites the accepted attack direction.

Handoff: frozen scoped diff, `git diff --check`, six-line Core report; no stage, commit, push or Unity.

### Outcome

Untargeted direct-player abilities resolve their world direction from the visible camera plane at input time. Swipe-up means camera-forward and swipe-right means camera-right after camera yaw. HUD abilities keep camera-forward. An explicit Fingertap enemy tap remains a stronger factual target direction. A buffered ability keeps its accepted input-time direction even if the camera later moves.

### Guardrails

- Keep AI target direction, enemy-tap direction, movement basis, dodge, jump, damage, range, cooldown, action timing, camera framing/yaw speed, HUD, persistence and scenes unchanged.
- Do not introduce aim assist, target lock, automatic target acquisition or camera authority over movement.
- If held direct movement can rotate an already accepted attack away from its camera direction during Windup/Impact, freeze only that action's accepted planar direction while preserving existing translation and timing. Do not broadly refactor `CombatEntity`.

## Next gate — Architect verification

After Core freezes 12.8, Architect performs static preflight, the two focused 12.7 tests, focused camera-direction coverage, then exactly one final EditMode and one final PlayMode run for the combined candidate. Concrete failures return to the same lease; green evidence advances immediately to acceptance records and commit.

## Next implementation

Before the combined candidate is committed, Architect must name the next player-visible Core slice or record the exact missing product decision. Core may prepare that next slice read-only while verification runs, but may not change main-checkout source until Architect activates its lease.
