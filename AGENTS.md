# Realm Raiders — Shared Agent Context

This file is the compact operating context for every coding agent working in this repository. Read it before changing gameplay code. For a fresh, compact team operating summary, read `Docs/TEAM_BRIEF.md`. For product detail, current verified state, and the next assigned task, read these in order:

1. `Docs/PROJECT_CONTEXT.md`
2. `Docs/PROTOTYPE_STATUS.md`
3. `Docs/NEXT_JOB.md` — local handoff; intentionally Git-ignored
4. `Docs/CORE_READY_QUEUE.md` — current and immediately following Core gates
5. `Docs/TEAM_WORKFLOW.md` — role ownership, worktree and handoff rules

## Product truth

Realm Raiders is a mobile-first fantasy action/strategy game.

> Build your Realm. Raid theirs. Become your monsters.

The prototype must make this compact loop satisfying and truthful:

`BUILD → INVADE → FIGHT → POSSESS → DEFEND → RESULT → BUILD`

Design laws:

- Everything built must be playable; do not add decorative systems that imply unavailable gameplay.
- Creatures are characters, not towers.
- Sylvan and Infernal must differ in play rhythm, not only color.
- Mobile portrait and landscape are equal first-class modes.
- Camera assistance is bounded presentation only: never take movement, targeting, or combat authority from the player.
- The most important proof remains the 30–60 second Keeper → possession → direct combat → release/return moment.

## Technical boundaries

- Unity `6000.6.0f1`, URP, New Input System; Android first, iOS supported. Desktop is future scope.
- `CombatEntity` owns authoritative stats, health, movement, abilities, and one active controller.
- Possession swaps `CreatureBrain` and `PlayerController` on the **same entity**. Never replace, respawn, or reset that entity to implement possession.
- Character gameplay uses the root `CharacterController`; visuals are children built by `CharacterVisualAssembler` under `Presentation Pivot`. Visual motion, hit reaction, and future animation must never move gameplay roots/colliders or change combat timing.
- Use `CharacterVisualRecipe` and shared body families for characters. Do not introduce one bespoke gameplay implementation per model.
- Reuse existing scene-local UI roots and `ResponsiveHudRoot`. New informational labels are non-raycast by default. Never casually add a Canvas, EventSystem, AudioListener, singleton, scene, package, or per-frame scene scan.
- Keep both portrait and landscape intentional. A technically rotating but overlapping layout is a defect.
- UI owns its full gestures; world taps, joystick and buttons must not leak into each other.
- Preserve the current camera awareness contract: soft bounded focus, no lock-on/aim assist/auto-combat, and prompt cleanup on controller change, death, terminal state, transition, and teardown.

## Persistence and results

- Existing Build layout, orientation, control-style, selected-realm, and any later progress saves are local `PlayerPrefs` JSON records with versioning and malformed-data fallback.
- A result must be credited or persisted exactly once. Duplicate callbacks, scene transitions, refreshes, and reloads must be idempotent.
- Do not claim cloud sync, accounts, stored rewards, progression, unlocks, or upgrades until the code genuinely provides them.

## Art and performance direction

- Character production is modular: three shared body families (`Humanoid`, `LargeCreature`, `Beast`), shared rigs/animation profiles, visual modules, palettes, and data recipes.
- Keep a character's gameplay collider and visual meshes separate. Visual modules must not add blocking colliders.
- Prefer a few substantial meshes/materials to many tiny GameObjects. Share materials and use mobile-appropriate mesh/texture budgets.
- Third-party art must have a documented commercial-use licence, local licence/provenance record, and explicit import settings. Never download a substitute asset when a selected source is unavailable.

## Working protocol

1. Inspect `git status`, the task scope, and nearby code/tests before editing.
2. Implement the smallest vertical slice that solves the assigned player problem. Preserve explicit non-goals.
3. Add focused regression coverage for new behavior and lifecycle cleanup.
4. Follow `Docs/TEAM_WORKFLOW.md` for verification ownership. On a Core developer lease, do not launch or control Unity; the temporary QA gate runs the focused and final suites. Do **not** repeat green full suites unless code, imports, or tests change afterwards.
5. Run `git diff --check`. Do not commit or push unless explicitly asked.
6. Report only: changed files, focused test, final EditMode/PlayMode totals, one manual-smoke result, and blockers. Never claim a manual smoke that did not occur.

## Feedback-derived working behavior

- Keep moving when the next safe action is already authorized. Do not turn an intermediate progress note into an idle stop; either take the next leased step or report the exact external blocker.
- An intermediate result is never a terminal state. After every handoff, the producing role immediately continues its current lease if unfinished or switches to read-only preparation of the next queued lease while Architect reviews the frozen candidate.
- A user push is never a prerequisite for local continuation. Architect commits accepted work; Core and Module work from the named local committed base whether or not the user has pushed it yet.
- Architect maintains one active and one ready Core gate. In the same coordination turn as a valid handoff, Architect accepts it, returns one concrete defect, or activates the next safe gate; it does not leave an agent idle behind a generic status message.
- A small, explicitly user-requested follow-up may join the current verification batch only when Architect records its separate lease, it does not overlap another writer, and one final EditMode plus PlayMode run occurs after the last code change.
- The user values player-visible progress over process theatre. Prefer one bounded vertical slice that strengthens the initial loop over speculative systems or documentation-only output.
- Treat test time and agent context as scarce: focused checks expose a concrete risk; one final EditMode and one final PlayMode run prove the frozen candidate. Do not use UnityCLI or invent a test workaround while standard Unity GUI verification is available.
- Preserve a clear, deliberate difference between Fingertap and Joystick in both orientations. Input ownership, control copy and camera behavior must match the mode actually shown to the player.
- A role handoff is a compact evidence record, not a conversational summary. Keep it short enough that a replacement role can continue from repository documents and the active lease.
- Architect commits accepted work; the user pushes. Never stage unrelated editor, generated, platform, or dirty Modules changes merely to make a worktree look clean.

## Collaboration rules

- `Docs/NEXT_JOB.md` is the active handoff and is Git-ignored. Update it before a new implementation task starts.
- `Docs/CORE_READY_QUEUE.md` is the tracked Core pipeline. Keep one active gate and one explicit next action or concrete dependency; the ten-minute recovery heartbeat uses it to resume work.
- Follow `Docs/TEAM_WORKFLOW.md`: only the named owner may write a reserved path, and only Core developer edits the main Unity checkout during an active feature.
- When a task is accepted, record it in `Docs/DONE_JOB.md` and refresh `Docs/PROTOTYPE_STATUS.md`; these are committed with the implementation.
- One agent owns a shared gameplay/UI file at a time. Parallel work should use separate file areas: implementation, research/assets, or review/tests.
- `Module Developer / Technical Art` is the fourth persistent role. Its primary output is isolated package code, validators, import/conversion tooling, recipe catalogues and production-ready visual data in a named Modules worktree. Supporting design notes are allowed only when they unblock that implementation; product and game-design priority stays with Architect. It never edits the main checkout or controls Unity.
- Preserve user changes and unrelated files. Generated `Library`, `Logs`, `Temp`, `UserSettings`, IDE files, and platform Build exports stay out of Git.
- Ask before materially broadening scope. Favor a clean next task over silently bundling unrelated polish.
- When a role produces two empty or incomplete handoffs, pause that lane at the next clean boundary and request a fresh role conversation with `TEAM_BRIEF.md` plus one current lease.
