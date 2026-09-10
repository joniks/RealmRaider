# Realm Raiders — Team Brief

This is the short operational context for a fresh team conversation. It complements, but does not replace, `AGENTS.md`, `PROJECT_CONTEXT.md`, `PROTOTYPE_STATUS.md`, `NEXT_JOB.md` and `TEAM_WORKFLOW.md`.

## What we are proving

> Build your Realm. Raid theirs. Become your monsters.

The player must understand and enjoy this truthful short loop:

`BUILD → INVADE → FIGHT → POSSESS → DEFEND → RESULT → BUILD`

The highest-value proof is the 30–60 second Keeper → same-creature possession → direct fight → return moment. Do not build decorative systems that imply missing gameplay.

## Non-negotiable game rules

- Android is first; portrait and landscape are separate intentional mobile layouts.
- `CombatEntity` owns gameplay. Possession swaps controllers on the same entity. The root `CharacterController` remains gameplay authority.
- Visuals, material previews, animation and camera are presentation only. They never change colliders, movement, targeting, combat timing or AI authority.
- Contextual control is Fingertap in portrait and Joystick in landscape. Joystick is stick + JUMP + world-drag camera yaw; Fingertap owns ground/target taps, swipe ability and guarded empty-ground double-tap jump.
- Camera help is soft, bounded and cleanup-safe. It never locks on, aims, moves or fights for the player.

## Team operating model

| Role | Owns | Must not do |
| --- | --- | --- |
| Architect / Product Lead | Priority, leases, docs, static preflight, acceptance decisions and accepted commits | Implement beside Core, control Unity, run tests/builds, push |
| Core | One named main-checkout vertical slice | Unity, tests, docs/process edits, commit/push |
| Module / Technical Art | One isolated Modules package/tool/data slice | Main checkout, Unity, mixed bundles, integration/commit/push |
| Reviewer / QA + Build | The single Unity GUI instance, focused/final tests, observed smoke and build/export evidence | Implement source, restart a healthy Editor, run CLI beside GUI, change layout, commit/push |

The separate QA conversation is active again. Only QA controls Unity and produces Unity test/build evidence; Architect performs static preflight and acceptance, while Core and Module remain Unity-free.

## Fast, reliable rhythm

1. Read `NEXT_JOB.md`, inspect status and edit only the named lease.
2. Core freezes a narrow candidate and reports six facts. Architect checks scope and `git diff --check`, then issues one named QA lease.
3. QA runs focused checks, then exactly one final EditMode and PlayMode run after the final change. A green suite is never rerun without a concrete cause.
4. QA reports only observed manual smoke. A UI/lock/tool limitation is recorded precisely, not disguised as a gameplay failure or success.
5. Architect records accepted facts, commits only accepted paths, and leaves push to the user. URP, generated files and unrelated Modules state are never swept into a feature commit.
6. QA alone uses the established Unity Test Runner GUI: open Test Runner, clear the filter → click one gate → wait for the UI/result to settle → verify the result before the next click. After each accepted Core commit, QA uses **Realm Raiders → Build → Export Android Studio Project** in the already-open healthy Editor; it does not close or restart it for export. The CLI wrapper is a fallback only when the Editor is already closed and that boundary was approved. Never run a second Unity process against the same project or use `Reimport All`, Library deletion or blind UI clicks.
7. A frozen handoff is not idle permission: Core immediately prepares the next `CORE_READY_QUEUE.md` gate read-only, while Architect reviews or verifies. A user push never blocks local continuation.
8. A filtered PlayMode `Run Selected` can stall in Test Runner staging before the test method begins. This is not evidence of a gameplay loop and does not justify a Mac/Unity restart: cancel that run, focus Test Runner, clear the filter, verify the full count and use one ordinary `Run All`.
9. A launched suite is still active work, not a handoff. QA waits for that exact run's finished callback and totals; a resumed QA turn waits only and never clicks Run a second time.

## Communication rules

- Continue autonomously when the next safe action is already authorized; do not stop after a partial status update.
- After every handoff, transition in the same turn: continue the lease, prepare the next queued gate read-only, or receive one precise defect/dependency. Small user-authorized follow-ups may share one final verification batch only when Architect records the exception and keeps file ownership disjoint.
- Report a real pass, failure or blocker immediately. A generic “done” is not a handoff.
- Keep handoffs to six lines and role checkpoints to eight bullets. The repository, not a long transcript, is the source of truth.
- Prefer focused regression coverage. New features need lifecycle cleanup tests; broad suites are release evidence, not exploratory debugging.
- If something external blocks work (locked Mac, unavailable Unity panel, missing user approval), state the exact boundary and preserve the frozen candidate.
