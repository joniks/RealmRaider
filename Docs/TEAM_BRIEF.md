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
| Architect | Priority, leases, docs, static preflight, accepted commits | Implement beside Core, push, repeat suites |
| Core | One named main-checkout vertical slice | Unity, tests, docs/process edits, commit/push |
| QA | Frozen-candidate review, Unity GUI, evidence | Source edits, CLI test workarounds, commit/push |
| Module / Technical Art | One isolated Modules package/tool/data slice | Main checkout, Unity, mixed bundles, integration/commit/push |

## Fast, reliable rhythm

1. Read `NEXT_JOB.md`, inspect status and edit only the named lease.
2. Core freezes a narrow candidate and reports six facts. Architect checks scope and `git diff --check`.
3. QA runs focused checks, then exactly one final EditMode and PlayMode run after the final change. A green suite is never rerun without a concrete cause.
4. QA reports only observed manual smoke. A UI/lock/tool limitation is recorded precisely, not disguised as a gameplay failure or success.
5. Architect records accepted facts, commits only accepted paths, and leaves push to the user. URP, generated files and unrelated Modules state are never swept into a feature commit.

## Communication rules

- Continue autonomously when the next safe action is already authorized; do not stop after a partial status update.
- Report a real pass, failure or blocker immediately. A generic “done” is not a handoff.
- Keep handoffs to six lines and role checkpoints to eight bullets. The repository, not a long transcript, is the source of truth.
- Prefer focused regression coverage. New features need lifecycle cleanup tests; broad suites are release evidence, not exploratory debugging.
- If something external blocks work (locked Mac, unavailable Unity panel, missing user approval), state the exact boundary and preserve the frozen candidate.
