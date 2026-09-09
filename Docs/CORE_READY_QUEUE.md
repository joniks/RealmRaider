# Realm Raiders — Core Ready Queue

This is the tracked continuation queue for the main Unity checkout. `NEXT_JOB.md` remains the full active lease; this file states what happens immediately after every intermediate result so a valid handoff cannot become silent idle time.

## Accepted — Diamond Passes 12.7 and 12.8

- Diamond Pass 12.7 Build-to-Defense Deployment Receipt is accepted after the user's positive manual smoke and the final automated gates.
- Diamond Pass 12.8 Camera-Relative Direct Attack Direction is accepted with deterministic legacy PlayMode gate repairs.
- Final Unity GUI Test Runner: EditMode `137/137`, PlayMode `78/78`, `0` failures. PlayMode XML completed 2026-09-09 22:18:41Z; `git diff --check` is clean.
- Architect records and commits only the accepted paths; user push is asynchronous publication and does not block 12.9.

## Now — Diamond Pass 12.9: Possessed-Defense Invader Awareness

Owner: Core developer

Workspace: main checkout; source only, no Unity.

Base: Architect's accepted local 12.7/12.8 commit.

Reserved paths:

- `Assets/Game/Scripts/AI/RaidInvaderBrain.cs`
- `Assets/Game/Scripts/Camera/CombatCameraAwareness.cs`
- `Assets/Game/Tests/PlayMode/PossessionFlowTests.cs`

Handoff: frozen scoped diff, `git diff --check`, six-line Core report; no stage, commit, push or Unity.

### Outcome

During the signature possessed-defense fight, factual `RaidInvaderBrain` target intent feeds the existing bounded camera-awareness presentation before the first hit. A nearby Blood Knight targeting the possessed defender can show the existing plate or left/right edge cue without gaining targeting, combat or camera authority.

### Guardrails

- Event-driven intent only; no scene scan or new per-frame global search.
- Reuse the existing focus/plate/edge-cue system, 14 m eligibility and current text/layout roots.
- `ATTACKING` urgency is factual only during accepted Windup/Impact; existing damage recency remains unchanged.
- Clear immediately on retarget/range exit, invader control loss/death/destroy, possession/controller loss, terminal state, camera transition and teardown.
- Do not change AI target selection, route/opening/recovery timing, damage, abilities, player input, `CombatEntity`, HUD, bootstraps, scenes, persistence or art.

## Next gate — Architect verification

After Core freezes 12.9, Architect performs static preflight, focused possessed-defense awareness coverage, then exactly one final EditMode and one final PlayMode run. A concrete failure returns to the same lease; green evidence advances immediately to acceptance records and commit.

## Ready after 12.9

Architect prepares the next player-visible slice read-only while 12.9 is verified. Core may inspect that queued scope but may not modify its files until Architect activates the lease.
