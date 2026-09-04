# Realm Raiders — Done Job

Archived specification for the completed implementation pass. Verification status is recorded in the project status and backlog documents.

## Completion report

- Implementation: complete. The Hub now opens a five-slot Sylvan BUILD screen, the layout is validated and saved separately from the HUD, and `DefenderTest` creates the saved defenders and trap at their fixed slots.
- Navigation: `Prototype Hub → BUILD SYLVAN → SAVE & DEFEND → DefenderTest → result → RETURN TO BUILD` is wired, and `RealmBuild` is included in editor and platform build scene lists.
- EditMode: 17/17 passed in Unity Test Runner on 2026-09-04, as reported by the project owner.
- PlayMode: 9/9 passed in Unity Test Runner on 2026-09-04 after correcting the position regression. Earlier review runs exposed a brittle exact Y assertion and that swapped creature types inherited the previous occupant's height. Spawn X/Z now comes from the fixed slot while Y is derived from the creature collider size (Wolf `0.7`, Ent `2.1`), with only Unity's small controller settling tolerance allowed.
- Additional review fixes: all five BUILD slot buttons were moved on-screen, saved layouts must preserve the fixed three-creature/two-trap slot schema, and swapped Wolves/Ents now spawn at the correct height.
- Static verification: `git diff --check` passes, `Docs/NEXT_JOB.md` is local-only, and generated Unity/build directories are absent from the Git change list.
- Automated verification baseline: EditMode 17/17 and PlayMode 9/9 passing; Unity Console shows no C# compilation errors.
- Manual verification not recorded: complete the Hub → Build → Defense → Result → Build flow once in portrait mode as the final device-facing smoke check.

The original assignment follows for traceability.

## Diamond Pass 02 — Minimal BUILD proof

### Starting point

- Project: `/Users/janiskepulis/Documents/RealmRaider`
- Expected branch: `main`
- Expected base commit: `bed2e56` (`Complete Diamond Pass 01 foundation`)
- Unity: `6000.6.0f1`, URP, New Input System
- Current verified baseline: EditMode 12/12 and PlayMode 7/7 passing

Before changing files, inspect `git status` and the current implementation. Preserve user changes. Do not commit or push unless the user explicitly asks.

## Objective

Implement the first player-facing BUILD step and prove the product promise:

> The defense the player builds is the defense they play.

The complete flow must be:

```text
Prototype Hub → BUILD SYLVAN → SAVE & DEFEND → DefenderTest → result → RETURN TO BUILD
```

Keep this as a compact greybox proof. Do not turn it into a general dungeon editor.

## 1. Realm Build scene

Create:

`Assets/Game/Scenes/RealmBuild.unity`

Add a runtime bootstrap and a simple mobile-first portrait UI with five fixed Sylvan defense slots:

- three creature slots;
- two trap slots.

Creature slots allow:

- Empty;
- Wolf — 2 Threat;
- Ent — 4 Threat.

Trap slots allow:

- Empty;
- Root Trap — 2 Threat.

The total Threat Budget is 10.

Default layout:

1. Wolf
2. Wolf
3. Ent
4. Root Trap
5. Empty

This default must reproduce the current Sylvan defense composition.

## 2. Player interaction

The player must be able to:

- tap a slot;
- choose a piece allowed by that slot;
- empty a slot;
- see used and available Threat Budget;
- press `SAVE & DEFEND`.

Do not implement drag-and-drop. Slot selection and clear buttons are sufficient.

Enable `SAVE & DEFEND` only when:

- total cost is at most 10;
- exactly one Ent is placed, preserving the possession flow;
- at least one Wolf is placed;
- exactly one Root Trap is placed.

Show a clear player-facing reason when the layout is invalid.

## 3. Data and persistence

Create a small UI-independent, testable data model. Suitable concepts include:

- `DefensePieceType`;
- `DefenseSlotType`;
- `DefenseSlotLayout`;
- `DefenseLayout`;
- `ThreatBudget` or an equivalent validator.

Persist the layout as JSON in PlayerPrefs with a versioned key such as:

`realmraiders.sylvanDefenseLayout.v1`

Persistence must be separate from the HUD. Missing, malformed, incompatible, or invalid saved data must fall back to the default layout.

## 4. Build exactly the saved defense

Refactor `DefenderBootstrap` so it creates:

- exactly the saved creatures;
- exactly the saved trap;
- each piece at its saved fixed slot position.

Do not silently add fallback defenders after a valid layout has loaded.

Build AI targets and the `RaidInvaderBrain` defender collection from the dynamically created saved creatures.

Preserve the current:

- invader and route;
- Heart Tree;
- possession flow;
- `DefenseManager` behavior;
- terminal results;
- HUD and camera behavior.

## 5. Navigation and builds

- Add `BUILD SYLVAN` to Prototype Hub.
- It opens `RealmBuild`.
- `SAVE & DEFEND` saves and opens `DefenderTest`.
- A completed defense offers `RETURN TO BUILD`.
- Preserve the existing raid, Infernal defense, and Character Sandbox routes.

Add `RealmBuild` to:

- `ProjectSettings/EditorBuildSettings.asset`;
- `PlatformBuild.Scenes`.

`PrototypeHub` must remain the first build scene.

## 6. Automated tests

Add EditMode coverage proving:

- the default layout is valid and costs 10 Threat;
- an over-budget layout is rejected;
- invalid creature/trap composition is rejected;
- save/load round-trip preserves slot order and contents;
- malformed saved JSON falls back to the default layout.

Add PlayMode coverage proving:

- `RealmBuild` creates five slots and its BUILD HUD;
- a saved custom layout creates exactly the expected creatures and trap in `DefenderTest`;
- created objects use the saved slot positions;
- BUILD → DEFEND retains exactly one camera and one `AudioListener`.

Tests that modify PlayerPrefs must isolate and restore their data. Keep all existing tests green.

## 7. Out of scope

Do not add:

- free placement or drag-and-drop;
- object rotation or a grid editor;
- Infernal BUILD mode;
- inventory or economy systems;
- backend or multiplayer;
- external assets;
- production art, VFX, or audio.

Keep the current runtime-generated greybox approach.

## 8. Verification

Before declaring the job complete:

1. Run all EditMode tests.
2. Run all PlayMode tests.
3. Manually verify the full Hub → Build → Defense → Result → Build flow.
4. Confirm there are no C# errors in the Unity Console.
5. Run `git diff --check`.
6. Confirm `Library`, `Builds`, `Logs`, `UserSettings`, and Player Test Runner settings are absent from Git changes.

Only after successful verification update:

- `Docs/POLISH_BACKLOG.md`;
- `Docs/PROTOTYPE_STATUS.md`;
- `README.md` with BUILD usage instructions.

## Handoff report

Finish with:

- changed and added files;
- a short architecture summary;
- exact EditMode and PlayMode results;
- manual flow result;
- known limitations or risks;
- final `git status`.
