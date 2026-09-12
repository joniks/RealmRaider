# Realm Raiders — Prototype Status

Last reviewed: 2026-09-12

## Current work and latest acceptance

- User smoke on 2026-09-12 reported Ent moving backwards, Sylvan raid Ent floating
  above the walkable surface, and insufficient crouch/recovery after jumping or
  falling. 18.0 implements the correction; its visual feel still awaits the next
  user-observed run.
- 18.0 is accepted after static review and fresh QA: EditMode 359/359,
  PlayMode 108/108, zero failed/skipped/inconclusive. Imported Ent static/animated
  fits use 180-degree visual yaw and four heel/toe contact anchors against the
  unchanged scaled controller support plane; ordinary falls now produce factual
  landing recovery and jump compression is stronger. Gameplay physics is unchanged.
- QA opened the existing Sylvan scene and focused Game without resizing Test
  Runner, but the accessible surface exposed neither Play controls nor viewport
  pixels/input. Visual direction/foot contact/landing feel remain unobserved;
  green automated results are not a claim of visual or device acceptance.
- Priority is 70% gameplay/content and 30% foundation/polish by planned effort.
  Sylvan/Infernal raid variants, the Keeper Reserve defense tradeoff and the
  Guardian Ent Ground Slam impact ring are accepted. The next gameplay slice
  teaches the existing Root Trap → possess Ent → Ground Slam opportunity without
  taking control from the player.
- MGC01 is statically accepted in Modules `83bcda9`: three authored Sylvan raid
  compositions are prepared for Core 18.3. The package is not yet installed or
  consumed by the game, and its seven NUnit tests have not yet run.
- MGC02 in Modules `8ee1d95` and MGC03 in `db7759c` now drive the accepted
  Infernal Ent raid through an installed local package: two Hellhounds, one
  bypassable Flame Trap, a Brute-only Heart gate and exact reviewed lane/spawn
  facts. Core retains spawning, combat, AI, reward and scene authority.
- Core 18.1 is accepted: the Moonwell provides one bounded 30% recovery choice and
  the exact optional Ent grants +1 Rare through existing exact-once results. Final
  QA passed EditMode `364/364` and PlayMode `109/109`; manual feel is unobserved.
- Core 18.1B is accepted: a separate direct-control Infernal raid uses the animated
  Ent against two optional Hellhounds, one bypassable Flame Trap and a Brute-gated
  Heart, with same-scene retry and Hub return. The user's full suites were green
  and Android export completed; the retained XML independently confirms EditMode
  `380/380`, zero failed/skipped/inconclusive.
- Core 18.2 is accepted: only exact Sylvan raid/defense Guardian Ent AI now uses
  the modular successful `Smash → Smash → Ground Slam` rhythm, with clean target,
  range, controller, death and reconfiguration resets. Guardian Ent Ground Slam
  uses a shared `0.80s` recovery while legacy abilities retain `0.12s`; possession
  remains direct-player authority. QA passed EditMode `394/394` and PlayMode
  `114/114`, zero failed/skipped/inconclusive and no new compiler/runtime errors.
- Core 18.4 is accepted: the Infernal Hub selector cycles Brute Finale, Entry Trial
  and Risk Route as session-only choices; exact validated module facts drive each
  roster, lane, optional Flame Trap and Heart prerequisite. Final QA passed
  EditMode `434/434` and PlayMode `118/118`, zero failed/skipped/inconclusive and
  no new compiler/runtime errors.
- Core 18.5 is accepted: a full valid defense keeps two Wolves and 30 seconds of
  possession, while one open creature slot sacrifices a Wolf for 45 seconds.
  BUILD shows the unsaved draft truthfully; DEFEND derives the saved choice once,
  and release/re-possession does not refill it. QA passed EditMode `443/443` and
  PlayMode `118/118`, zero failed/skipped/inconclusive and no new errors.

## Latest verification

- Diamond Pass 08.7 Build Plan Readability is present: the fixed five-slot Build screen now shows each lane, role, cost, and a live invader-to-Heart-Tree defense sequence without changing defense rules.
- Diamond Pass 08.8 Defender Route Readability is present: the defense HUD states the invader's real opening, approach, engagement, and terminal route situation without changing AI or controls.
- Diamond Pass 08.9 Combat Target Readability is present: the existing awareness system identifies a visible eligible attacker with real health and hands off cleanly to its off-screen direction cue.
- Diamond Pass 09.0 Raid Loop Closure is present: factual raid results now have a primary route back to the saved Build plan, while retry and Hub remain available.
- Diamond Pass 09.1 Realm Stores Foundation is present: real raid Gold and Rare Materials are locally recorded once per shown result and are visible as read-only stores in Hub and Build.
- Diamond Pass 09.2 Guardian Ent Cultivation is present: earned stores can buy one capped, persistent, truthful Ent vitality upgrade for the next Sylvan defense.
- Diamond Pass 09.3 Guardian Ent Growth Readability is present: the cultivated defender visibly grows and states its exact earned maximum-health bonus during Sylvan defense.
- Diamond Pass 09.4 Module Host Boundary is present: an independent, passive character-catalogue contract assembly now lets future Modules packages describe data without gaining gameplay or scene authority.
- Diamond Pass 09.5 Visual-Tuning Package Wiring is present: the reviewed local visual-tuning package is resolved through the pinned Modules submodule, while remaining unused by runtime presentation.
- Module Pass MCT 01 Starter Character Catalogue is staged in the pinned Modules submodule: it describes the five starter characters through the passive contracts boundary, but is intentionally not yet installed or consumed by runtime presentation.
- Diamond Pass 09.6 Dodge: Player Escape is present: direct-controlled characters have a narrow, collision-aware escape with explicit 0.18-second damage immunity and lifecycle-safe cleanup; AI behavior remains unchanged.
- Module Pass MVT 02 Starter Creature Visual Profiles is present in the installed tuning package: four mobile-budgeted design profiles are validated but deliberately unconsumed by runtime presentation.
- Diamond Pass 09.7 Infernal Flame Trap Identity is present: the Keeper manually ignites the invader for three separate eight-damage pulses without root or slow, while the HUD truthfully exposes the active burn and cooldown.
- Diamond Pass 09.8 Combat Camera Readability v2 is present: one eligible nearby threat now receives an unmistakable HUD-owned target plate or responsive left/right `ATTACKER`/`ATTACKING` edge tab without changing targeting, movement, combat, or bounded camera authority.
- Module Pass MCR 01 Modular Character Recipe Contracts is staged in the pinned Modules submodule: it defines a deterministic, immutable five-slot recipe boundary, but is intentionally not installed or consumed by runtime presentation.
- Diamond Pass 09.9 Realm Landmark Silhouette Blockout is present: the two realm objectives and race-specific traps now differ through lean visual-only organic versus angular primitive silhouettes while their gameplay roots, colliders and rules remain unchanged.
- Module Pass MMP 01 Character Motion Profile Contracts is staged in the pinned Modules submodule: it defines deterministic six-clip family motion metadata while remaining uninstalled and free of animation assets or runtime authority.
- Diamond Pass 10.0 Realm Route Readability Blockout is present: Sylvan raid and defense routes now use low organic bands/masses while Infernal defense uses low angular causeway plates, without changing authoritative transforms, colliders, navigation or gameplay.
- Design Pass DUX 01 First Playable Minute v1 is staged in the pinned Modules submodule: it specifies a truthful, non-modal BUILD-to-possession onboarding thread for the existing loop, but no tutorial runtime or persistence has been implemented yet.
- Diamond Pass 10.1 First Playable Minute Hub + BUILD Foundation is present: only the primary Sylvan journey activates a versioned local guide, and BUILD truthfully guides one changed valid plan with dismiss/skip and a transient defense handoff.
- Diamond Pass 10.2 First Playable Minute Possession Proof is present: the accepted BUILD handoff continues through factual Sylvan selection, same-entity possession, movement, attack, dodge, explicit release, Keeper return and the real defense result, with lifecycle-safe retry and completion.
- Design Pass DART 01 Character Production Pipeline v1 is staged in the pinned Modules submodule: it defines a deterministic three-family Blender-to-Unity production system and a Guardian Ent first-production gate without claiming unavailable art, rigs or clips.
- Research Passes ART 06A-R and ART 06B-R are staged in the pinned Modules submodule: they shortlist a conditional CC0 Guardian Ent source, KayKit Humanoid motions and a Beast motion accelerator without downloading or approving any archive.
- Design Pass DLEGAL 01 Third-Party Notices v1 is staged in the pinned Modules submodule: it specifies exact release-cleared 3DRT and Kenney notice presentation while keeping Quaternius behind a human licence decision.
- Diamond Pass 10.3 Third-Party Notices is present: the Hub now exposes an offline responsive notice panel with exact mandatory 3DRT attribution, voluntary Kenney provenance, safe fallback and no unresolved Quaternius claim.
- Diamond Pass 10.4 Deterministic Invader Stuck Recovery is present: a measured, bounded sidestep lets the defense invader recover from a real route obstruction without teleporting, skipping waypoints or changing defender/combat authority.
- Design Pass DART 02 Guardian Ent Visual Identity v1 is staged in the pinned Modules submodule: `Ancient Canopy Sentinel` defines the first LargeCreature silhouette, cultivation continuity and mobile production gate.
- Design Pass DART 03 Infernal Brute Visual Identity v1 is staged in the pinned Modules submodule: `Obsidian Gatebreaker` defines a low-wide Infernal counterpart that shares the exact LargeCreature family contract without collapsing into the Ent silhouette.
- Design Pass DENV 01 Sylvan Environment Production v1 is staged in the pinned Modules submodule: `Open Grove Arches` defines the eight-type Sylvan kit, continuity rules and Android budgets without approving an asset source.
- Design Pass DART 04 Beast Family Visual Identity v1 is staged in the pinned Modules submodule: `Mossback Courser` and `Cinderjaw Stalker` define distinct Wolf/Hellhound silhouettes on one shared Beast rig/atlas/motion contract without approving an asset source.
- Design Pass DBND 01 Prototype Arena Boundary Visual Language v1 is staged in the pinned Modules submodule: all four 3D prototype zones now have factual boundary dimensions, shape language and acceptance rules ready for Diamond Pass 10.5 implementation.
- Design Pass DENV 02 Infernal Environment Production v1 is staged in the pinned Modules submodule: `Ironbound Rift Causeway` defines an eight-type basalt/iron kit and Android budgets while preserving the current lane, trap, gate, heart and gameplay truth.
- Diamond Pass 10.5 Prototype Arena Boundaries is present: the Sandbox, both defense lanes and the factual branched Sylvan floor union now have continuous, style-specific static borders that stop ordinary movement, dash and existing AI controllers without changing combat, route or possession authority.
- Module Pass MCR 02 Deterministic Recipe Catalogue is staged in the pinned Modules submodule: explicitly supplied providers now build one immutable, ordinal, fail-closed recipe/character lookup catalogue without discovery, Unity or gameplay authority.
- Diamond Pass 10.6 Possession Energy Return Readability is present: the shared Defender HUD now gives calm five-second and urgent two-second text-first warnings before the existing forced Keeper return, without changing possession energy or controller authority.
- Diamond Pass 10.7 Mobile Combat Input Buffer is present: one ready alternate ability can be visibly queued during the final 0.20 seconds of Recovery and executes once through the existing authoritative combat gate.
- Module Pass MMP 02 Deterministic Motion Profile Catalogue is staged in the pinned Modules submodule: explicitly supplied motion-profile providers build one immutable, ordinal, fail-closed lookup catalogue without discovery, Unity or gameplay authority.
- Module Pass MART 01 Character Art Intake Manifests is staged in the pinned Modules submodule: one immutable record now freezes reviewed character-art provenance, licence, checksum, motion, LOD and safe-import evidence without approving or importing an asset.
- Module Pass MART 02 Deterministic Art Manifest Catalogue is staged in the pinned Modules submodule: explicitly supplied providers now build one immutable, ordinal, fail-closed source/character lookup catalogue without discovery, Unity or import authority.
- Module Pass MART 03 Character Art Measurement Gate is staged in the pinned Modules submodule: explicit post-import measurements can now be checked deterministically against an intake manifest without Unity inspection, filesystem access or import authority.
- Module Pass MART 04 Deterministic Character Art Batch Report is staged in the pinned Modules submodule: explicit manifest–measurement sets now produce immutable source-sorted compliance totals and item issues without discovery or import authority.
- Diamond Pass 10.8 Canonical Sylvan Journey Continuity is present: the primary Hub journey now follows the truthful `BUILD → RAID → DEFEND → BUILD` route while every legacy direct prototype route remains available and session-neutral.
- Diamond Pass 10.9 Stable Starter Roster Host is present: the five starter catalogue archetypes now give every runtime character a stable identity independent of its scene alias while preserving visuals and gameplay values.
- Reliability Follow-up is present: camera-awareness ownership now cleans up on controller loss/destroy and `PossessionFlowTests` clean only fixture/build scenes, not Test Runner's own scene.
- Diamond Pass 11.0 In-Run Control Style Switcher is present: gameplay HUDs now offer a factual persistent `AUTO`/`TAP`/`STICK` cycle with immediate transient-input reset and responsive joystick visibility, while Hub and BUILD controls remain unchanged.
- Module Pass MMP 03 Motion Profile Compatibility Gate is staged in the pinned Modules submodule: explicitly supplied motion targets can be fail-closed checked for family, rig, six clips and fallback policy without Unity, asset or gameplay authority.
- Diamond Pass 11.1 Keeper Touch Selection Reliability is present: registered creatures now use a deterministic mobile press/release selection path with safe near-miss tolerance and strict UI/swipe/state rejection before the existing possession flow.
- Module Preview Passes MWS 02/MWS 03 and MUI 01 are staged in the pinned Modules submodule: original-generated Sylvan/Infernal ground-material and Jump-icon candidates have explicit provenance but remain preview-only until seam, import, readability and mobile-budget gates pass.
- Module Pass MWS 04 World Surface Seam Validator is accepted in the pinned Modules submodule: pure C# callers can deterministically check raw RGBA edge compatibility without granting the package Unity, filesystem, import, shader or gameplay authority.
- Module Pass MWS 05 Surface Preview Budget Gate is accepted in the pinned Modules submodule: caller-declared world-surface facts now receive stable Android-preview budget evidence without asset, Unity, filesystem or import authority.
- Module Pass MVT 03 Mobile Visual Budget Compatibility Gate is accepted in the pinned Modules submodule: character visual profiles can now receive deterministic caller-policy evidence before future model variants are accepted, without model measurement or Unity authority.
- Module Pass MVT 04 Starter Visual Budget Batch Evidence is accepted in the pinned Modules submodule: an explicit roster can now receive an immutable ProfileId-ordered mobile-budget report without discovery, model measurement or Unity authority.
- P0 Sylvan Raid Boundary Path Recovery is present: the factual Portal → Crossroads route has a non-blocking visible route tree and seam-aligned node support collider, while its intended outer boundary closure remains intact.
- Diamond Pass 12.0 Grounded Mobile Jump is present: direct-controlled heroes and possessed defenders have one collision-constrained, non-double `JUMP` action with airborne horizontal movement, lifecycle-safe cancellation and intentional portrait/landscape HUD placement.
- Diamond Pass 12.1 Sylvan Route Is Never a Hidden Puzzle is present: all generated Sylvan node trees remain visible/revealable presentation, but cannot become movement walls; normal Portal → Crossroads walking now proves passage past the first node centre while the factual outer boundary remains closed.
- Diamond Pass 12.2 Intentional Touch Controls and Camera is present: direct-control Joystick now uses stick movement, JUMP and non-UI world-drag camera yaw only; Fingertap retains ground/target taps and gains a guarded empty-ground double-tap jump with factual locomotion camera recentering.
- Diamond Pass 12.3 First Realm Surface Preview is present: the existing Sylvan and Infernal route presentation safely uses two cached, provenance-recorded preview albedos with exact solid-colour fallback and no gameplay or collision changes.
- Diamond Pass 12.4–12.5 Jump Preview and Defender Terminal Cleanup are present: Joystick Jump has a non-interactive, provenance-recorded preview icon while Defender terminal results hide live actions and resist late callback reactivation.
- Diamond Pass 12.6 Spatial Defense Plan Preview is present: RealmBuild now shows the actual five-slot defense order from invader entry through its factual roles to Heart Tree, as a non-interactive visual explanation rather than a new build system.
- Diamond Pass 12.7 Build-to-Defense Deployment Receipt is present: the Sylvan opening states the five pieces actually deployed from the saved Build plan and clears for guide, possession, movement, terminal and transition ownership.
- Diamond Pass 12.8 Camera-Relative Direct Attack Direction is present: untargeted direct attacks follow the visible camera plane at input time, while explicit enemy taps and buffered direction snapshots remain authoritative.
- Diamond Pass 12.9 Possessed-Defense Invader Awareness is present: a factual nearby raid invader targeting the directly controlled possessed defender now feeds the existing bounded plate/edge/focus presentation before the first hit, with lifecycle-safe cleanup and no targeting or combat authority.
- Diamond Pass 13.0 Sylvan Seam-Hardened Path Integration is present: the accepted original-generated MWS07 mossy stone candidate now textures Sylvan walkable route presentation with Repeat/mobile import settings while exact geometry, collision, Infernal art and solid-colour fallback remain unchanged.
- Module Pass MMP 04 Explicit Character Motion Binding Batch is accepted in the pinned Modules submodule: explicit characters can be deterministically checked against exact compatible motion-profile IDs without fallback selection or animation authority.
- QA Tool 01 is present: Reviewer / QA + Build can launch explicit unfiltered EditMode or PlayMode suites from **Realm Raiders → QA** without depending on inaccessible Test Runner buttons, CLI or a second Unity process.
- Module Pass MWS 09 is accepted in the pinned Modules submodule: a deterministic wrapped-edge tool and restrained Sylvan living-root normal-map candidate are ready for the named boundary presentation gate, but remain unimported until Unity orientation and device-lighting review.
- Diamond Pass 13.1 Sylvan Living-Root Boundary Presentation is present: Sylvan arena closure now uses the accepted original-generated root albedo and restrained normal response through deterministic visual-only UV/tangents, while all boundary geometry, colliders, gameplay and non-Sylvan fallbacks remain unchanged.
- Module Pass MWS 10 is accepted in the pinned Modules submodule: a project-owned Sylvan clearing-floor albedo, restrained wrapped-edge normal and exact repeat evidence are ready for a named future Unity integration gate; runtime binding and device periodicity remain unclaimed.
- Art Pass MUI 02 is accepted in the pinned Modules submodule: three original encounter-state icon candidates and actual-size evidence are ready for a later named integration, while the dark hostiles mark retains an explicit in-context contrast gate.
- Art Pass MUI 03 is accepted in the pinned Modules submodule: original Basic Slash, Blood Rush and Heavy Cleave icon candidates are distinct at 48 px and ready for a later named HUD integration, with Basic Slash retaining a 32 px readability gate.
- Diamond Pass 13.2 Factual Encounter Entry and Clear Cue is present: first node visits now present truthful discovery, live hostile count and area-clear copy through existing Health and HUD ownership without gating movement or changing rewards/gameplay.
- Art Pass MUI 04 is accepted in the pinned Modules submodule: original Guardian Ent Smash, Charge and Ground Slam icon candidates are distinct at 48 px, while the detailed radial mark retains an explicit 32 px readability gate.
- Diamond Pass 13.3 Sylvan Clearing-Floor Presentation is present: all existing Sylvan circular node floors share the accepted project-owned clearing albedo/normal with per-node fog tint and exact solid fallback while geometry, collision and encounter authority remain unchanged.
- Art Pass MUI 05 is accepted in the pinned Modules submodule: original Infernal Brute Smash, Charge and Ground Slam candidates preserve shared mechanics with a distinct obsidian/ember skin; the detailed radial mark retains a 32 px gate.
- Diamond Pass 13.4 Factual Encounter Cue Icons is present: the text-first discovery/hostiles/clear cue now has one cached, non-raycast original phase sprite with responsive layout and exact text-only fallback/lifecycle cleanup.
- Art Pass MUI 06 is accepted in the pinned Modules submodule: original Contextual, Fingertap and Joystick candidates remain distinct at 32 px, with the two more abstract modes retaining explicit in-context comprehension gates.
- Art Pass MUI 07 is accepted in the pinned Modules submodule: original Sylvan living-seed and Infernal obsidian-gate marks are ready for a later named realm-identity integration, with Sylvan 32 px fine detail retaining an in-context readability gate.
- Diamond Pass 13.5 Blood Knight Ability Icons is present: the Raid HUD's three existing attacks now add explicit original sprites while preserving labels, readiness/cooldowns, callbacks, responsive layouts and exact text-only fallback.
- Diamond Pass 13.6 Guardian Ent Ability Icons is present: Sylvan possession now adds original Smash and Ground Slam button sprites plus a truthful Fingertap-only `SWIPE: CHARGE` mark, while Joystick and non-Guardian states remain honest.
- Diamond Pass 13.7 Infernal Brute Ability Icons is present: Infernal possession now uses its own obsidian/ember Smash and Ground Slam sprites plus the truthful Fingertap-only Charge mark without changing controls or the Guardian family.
- Diamond Pass 13.8 Realm Identity Marks is present: existing Hub, Build, Raid and Defense realm titles now carry one cached original Sylvan or Infernal mark without becoming controls or weakening exact text-only fallback.
- Diamond Pass 13.9 Guided Possessable Ent Locator is present: the active first-minute Sylvan Select step now points from the existing guide to the exact possessable Ent with one safe-area-clamped noninteractive marker and complete retry/terminal cleanup.
- Diamond Pass 14.0 Explain Premature Explicit Release is present: an early player-requested RELEASE now returns to the existing Select step with one factual retry explanation after Keeper view settles, while forced/death/terminal paths never blame the player.
- Diamond Pass 14.1 Guided Loss Closes Back to Build is present: a factually completed first-minute Sylvan loss now emphasizes the existing RETURN TO BUILD action with truthful adjustment copy while DEFEND AGAIN remains available as a secondary choice.
- Diamond Pass 14.2 Completed Loop Primes the Next Raid is present: returning to RealmBuild from a completed canonical Defense now primes the existing journey so the primary action truthfully becomes SAVE & RAID and advances to SylvanRealm, while ordinary Build entry remains SAVE & DEFEND.
- Diamond Pass 14.3 Raid Result Copy Matches the Next Action is present: journey victory and defeat results now describe the factual immediate DEFEND YOUR REALM step while direct raids retain their planning copy and PLAN NEXT DEFENSE route; metrics and exact-once credit are unchanged.
- Diamond Pass 14.4 In-Run Control Style Mark is present: the existing gameplay selector now reinforces exact CONTROL AUTO/TAP/STICK text with one cached original 32 px mark, complete independent text-only fallback and unchanged control authority.
- Diamond Pass 14.5 Hub Control Choice Marks is present: the existing CONTEXTUAL, FINGERTAP and JOYSTICK choice buttons now reuse their explicit original non-raycast marks while exact text, saved-preference summary, button actions and responsive footprints remain authoritative.
- Diamond Pass 14.6 Grounded Jump Coyote Time is present: direct players receive one strict 0.10-second grace after factual grounded contact while startup airborne, repeated jump and all existing lifecycle/authority gates remain rejected; the QA menu also recovers safely from an unacknowledged test-run start without automatic retries.
- Diamond Pass 14.7 Jump Takeoff and Landing Readability is present: a factual direct jump adds one restrained pivot-only takeoff stretch and one grounded landing settle while the gameplay root, CharacterController, camera, timing and control authority remain unchanged.
- Diamond Pass 14.8 One Falling Jump Buffer is present: exactly one 0.08-second request may be accepted during factual descent of an existing direct-player jump, cannot be refreshed or repeated in that jump, and consumes once only after factual grounding while every lifecycle boundary clears it.
- Diamond Pass 15.0 Possession Energy Urgency Pulse is present: the existing meter gives one bounded 0.24-second noninteractive pulse on factual Warning and Critical entry, never restarts for timer tenths, and restores exact identity on every possession/controller/terminal/component return path without changing energy authority.
- Diamond Pass 15.1 Infernal Seam-Hardened Path Surface is present: Infernal route and defense-lane presentation now use the accepted original-generated MWS07 repeating basalt albedo with exact provenance/mobile import and cached solid fallback while MWS03 remains available but unbound and Sylvan/gameplay stay unchanged.
- Diamond Pass 15.2 Infernal Boundary Surface Identity is present: Infernal arena boundaries now use the accepted original-generated MWS08 repeating forged-iron and obsidian albedo with exact provenance/mobile import and cached solid fallback while Neutral, Sylvan, geometry, collision and gameplay remain unchanged.
- Diamond Pass 15.3 Possession Arrival Impact is present: every factual successful takeover adds one bounded 0.22-second unscaled squash/rebound/settle on the same creature's presentation pivot, with complete release/death/terminal/controller/visual cleanup and no gameplay-root or possession-authority change.
- Diamond Pass 15.4 Infernal Courtyard Floor Surface is present: the factual Volcanic Floor uses the accepted original-generated MWS11 albedo/normal pair with renderer-local square world tiling, while MWS07 causeways, MWS08 boundaries, all Sylvan presentation and gameplay remain unchanged.
- Diamond Pass 15.5 Defeat Presentation Keeps Gameplay Root Authoritative is present: factual death now settles only the visual Presentation Pivot while the entity root and CharacterController geometry remain exact and all existing death/possession/result authority is preserved.
- Module Art Pass MWS12 is accepted in the pinned Modules submodule: restrained deterministic Sylvan and Infernal route normal candidates and exact repeat evidence are ready for a named Unity lighting integration gate.
- Diamond Pass 15.6 Restrained Route Normal Pair Integration is present: Sylvan and Infernal MWS07 routes now use their own restrained MWS12 normal at strength `0.20`, with atomic realm-local fallback and no floor, boundary, geometry, collision or gameplay leakage.
- Module Research Pass MMP05 is accepted in the pinned Modules submodule: the official CC0 Quaternius UAL2 route, seven-slot motion pilot intake and conservative archive-evidence gate are frozen without downloading or importing third-party binaries.
- Module Passes MMP06–MMP07 are accepted in the pinned Modules submodule: immutable factual motion input resolves nine semantic states, and a bounded six-bone procedural humanoid driver applies them without gameplay, root-motion or scene authority. Unity package-test compilation was corrected in Modules commit `e57d795`.
- Diamond Pass 15.8 Modular Procedural Blood Knight Motion Pilot is present: a thin Core adapter maps factual root movement, action, jump, damage and death state into the installed Modules driver, affecting only six exact descendant bones while preserving the gameplay root, CharacterController, Presentation Pivot and Base Body.
- Module Pass MMP08 is accepted in the pinned Modules submodule: immutable bounded tuning retains the exact compatibility profile and adds a stronger Blood Knight device-readable cadence/pose preset without root, position, scale, Animator or physics authority.
- Module Tool MART05.1 is accepted in the pinned Modules submodule: one explicit local character archive can be hashed and inventoried deterministically without extraction, execution, network access or inferred licence approval.
- Diamond Pass 15.9 Blood Knight Device Motion Tune is present: only the imported visual receives a 180-degree recipe fit and its exact six-bone adapter selects the stronger bounded preset, while the gameplay root, CharacterController, Presentation Pivot, camera, input and combat timing remain unchanged.
- Diamond Pass 15.10 Blood Knight Motion Plane is present through Modules commit `6769716`: the device-readable preset now uses a forward/back local-Z locomotion plane with opposing limbs and an asymmetric one-leg takeoff pose, while the compatibility profile and all gameplay authority remain unchanged.
- Module Research Pass MART05 records the exact Tennessippi Guardian Ent Tree01 CC0 archive and local FBX structural evidence in Modules commit `91e0323`; Unity rig, clip, material and performance validation remain separate import facts.
- Diamond Pass 15.11 Blood Knight Readable Stride and Staged Jump is present through Modules commit `501c812`: exact doubled forward/back counter-swing and one shared continuous 0.30/0.40/0.40/0.56-second visual jump timeline affect only the pivot and six bound bones, including sparse-frame and early-ground continuity.
- Diamond Pass 15.12 Factual Character Motion Dynamics is present: one shared per-entity state drives the six bound bones and bounded pivot weight from factual horizontal displacement and yaw, stops exactly at neutral, composes with the staged jump/action/hit priorities and clears on every authority/lifecycle boundary without changing gameplay physics.
- Diamond Pass 15.13 Sagittal Stride and Directional Combat Motion is present through Modules commit `7aae5d6`: the Blood Knight's six cached limb hinges now follow the owned Presentation Pivot's forward/back plane with opposed gait, while factual accepted attacks and damage drive continuous bounded windup/impact/recovery and directional flinch without gameplay authority.
- Diamond Pass 15.14 Blood Knight Upper-Torso Counterweight is present through Modules commit `e02e9a4`: the actual skin-weighted `Bip01 Spine1` receives small bounded walk/attack/hit counterweight while invalid optional evidence falls back to the unchanged six-limb motion and every gameplay transform remains authoritative.
- Diamond Pass 15.15 Guardian Ent Tree01 Visual Pilot is present through Modules evidence commit `a075494`: the exact creator-published CC0 light Tree01 now replaces only the Guardian Ent visual child through a sanitized static prefab, while the same entity, CharacterController, possession, cultivation and primitive fallback remain authoritative.
- Module Pass MART05.3 is accepted through Modules commits `bda0b86` and `8978fe0`: exact Tree01 source identity and temporary-pilot exceptions are separated from production LOD, skin-weight, bone, material and sampled-texture budgets by a fail-closed readiness gate.
- Module Pass MMP06 LargeCreature Motion Readiness is accepted through Modules commits `541a7a4` and `8978fe0`: immutable caller-supplied skin, clip, root-safety, loop, death-hold and fallback facts can be evaluated without Unity, filesystem or gameplay authority.
- Diamond Pass 15.16A Guardian Ent Generic-Rig Feasibility Probe is present: an Editor-only exact-source copy proves one 31-bone skinned mesh and in-place deformation for Idle, Run, Attack_1 and Death1 without changing the accepted static runtime prefab or recipe. It is evidence, not yet a runtime animation binding.
- Diamond Pass 15.16B Modular Guardian Ent Motion Integration is present: the exact Generic Tree01 skin now uses skeleton-only Idle, Run, Attack_1 and Death1 presentation with runtime-disabled events/root motion, deterministic static/primitive fallback and complete graph/bone cleanup while the same gameplay entity remains authoritative.
- Diamond Pass 16.0 Factual Raid Loot Feedback is present: existing exact-once room, enemy and Realm Core rewards now emit immutable receipts and one responsive FIFO HUD cue without changing the economy, persistence, input or result authority.
- Diamond Pass 16.1 Earned Cultivation Affordance is present: the existing Guardian Ent cultivation button now truthfully shows `MISSING`, `READY` or `CAPPED`, exact shortages and immediate post-purchase stores/rank/status refresh without changing the economy or persistence.
- Diamond Pass 16.2 Factual Defense Result Debrief is present: the first authoritative terminal transition freezes outcome, elapsed time, invader health and Core progress exactly once and appends those facts to the existing result copy without changing result actions or gameplay.
- Diamond Pass 16.3 Earned Cultivation Result Handoff is present: only a frozen Sylvan `READY` result points back to the existing explicit Guardian Ent purchase, while missing/capped/malformed/Infernal results and all stores, actions and purchase authority remain unchanged.
- Diamond Pass 16.4 Factual Incoming-Attack Cue is present: an accepted hostile Windup against the current direct player temporarily upgrades the existing singular threat plate/edge tab to factual `INCOMING` copy, keyed by attacker and ActionId with complete authority/lifecycle cleanup and no camera or combat change.
- Diamond Pass 16.5 Truthful Successful-Dodge Confirmation is present: immunity-rejected ability hits no longer create damage/hit/knockback/impact feedback and instead show one bounded direct-player `DODGED` marker, while applied damage, traps, timing and combat authority remain unchanged.
- Diamond Pass 16.6 Direct-Control Critical Health Readability is present: the existing raid and possessed-defender health labels now show factual `LOW HP` copy and a fixed warning tint at living active direct-control health at or below 25%, then restore their exact neutral presentation on every authority and terminal boundary without decorating invader health.
- Module Pass MMP09 is accepted in the pinned Modules submodule: explicit immutable procedural-humanoid tuning providers now build a deterministic exact-lookup catalogue for Compatibility and Blood Knight profiles without discovery or automatic Core application.
- Diamond Pass 16.7 Factual Creature-Return Receipt is present: an explicit release only confirms that the same named living creature resumed defense with unchanged HP after its active AI is restored, while forced terminal/death/energy returns and invalid/repeated paths cannot invent that claim.
- Module Pass MMP10 is accepted in the pinned Modules submodule: stable character/recipe assignments now resolve preferred or explicit fallback procedural-humanoid tuning profiles deterministically without discovery or automatic application.
- Diamond Pass 16.8 Factual Possessed-Creature Defeat Receipt is present: only the exact currently possessed creature's authoritative death reports its factual named loss and zero/maximum HP through the existing notice, while all other release causes retain distinct truthful copy and unchanged authority.
- Module Pass MMP11 is accepted in Modules commit `42fbbc8`: the Blood Knight now has one stable explicit preferred/fallback procedural-motion tuning assignment without discovery, automatic application or gameplay authority.
- Diamond Pass 16.9 Factual Direct-Combat No-Hit Confirmation is present: only an accepted direct-player positive-damage Melee/Area impact with no eligible living contact shows one bounded `NO HIT`; damage, immunity `DODGED`, AI, Dash and canceled paths retain their distinct existing outcomes.
- Diamond Pass 17.0 Explicit Blood Knight Motion-Tuning Resolution is present: the Core adapter now resolves the accepted explicit Modules assignment once and retains the exact device-readable tuning identity, with deterministic Compatibility fail-closed behavior and no retune or gameplay change.
- Diamond Pass 17.1 Factual Direct-Combat Defeat Confirmation is present: an applied direct-player lethal Melee/Area hit now confirms each exact named defeated target once while AI, immunity, misses, invalid and lifecycle-exited paths remain silent.
- Diamond Pass 17.2 Singular Latest-Damage Marker is present: each target now owns at most one active damage number, refreshed to the latest factual hit and point for a bounded lifetime while distinct targets remain independent.
- Current Unity Test Runner baseline: EditMode `359/359` and PlayMode `108/108` passed with `0` failures through the QA menu commands on 2026-09-12.
- Physical-device validation of torso strength, neck/armor clipping, sword/shield follow, corrected knee/stride direction and overall combat feel remains pending; no device performance result is claimed here.

## Implemented milestones

| Milestone | Status | Current implementation |
| --- | --- | --- |
| 1. Character Sandbox | Functional greybox | Blood Knight and Ent share combat, movement, abilities, AI, health/death, ability-readiness UI and eligible-attacker identification. |
| 2. Possession | Functional greybox | Keeper selection, same-entity controller swap, camera transition, readable marker/feedback, release and death handling. |
| 3. Sylvan Raid | Functional greybox | Seven-node Realm graph, fog states, Wolves, Ent, Root Trap, Heart Tree, objective compass, ability readiness and result-to-Build loop closure. |
| 4. Keeper Defense | Functional greybox | Brief opening hold, readable live invader route state, manual Root Trap, possessable Guardian Ent with persistent three-rank vitality and a visual growth signal, plus 30-second energy pool. |
| 5. Infernal Realm | Functional greybox | Brute, Hellhounds, a manual three-pulse Flame Trap without control effects, Lava Gate and Infernal Heart defense. |
| Prototype Hub | Functional | Stores realm, orientation and control choices; starts the canonical Sylvan build → raid → defend journey and shows current read-only Realm Stores while retaining direct prototype-scene access. |
| Build Plan | Functional greybox | Fixed five-slot defense configuration with live lane/role/cost copy, one capped Ent-vitality investment using earned local stores, and a non-interactive invader-to-Heart-Tree plan. |

## Verification baseline

- Current verified baseline: EditMode `359/359` and PlayMode `108/108` passed with `0` failures on 2026-09-12.
- The current PlayMode suite includes smoke coverage for PrototypeHub, RealmBuild, SylvanRealm, DefenderTest and InfernalRealm, possession, UI presentation, visual motion and camera awareness.

## Known limitations

- The Realm layout and content are generated at runtime from code rather than authored prefabs and persistent ScriptableObject assets.
- The former repeated two-AudioListener test warning was fixture-only and is
  resolved without changing runtime audio ownership; the final PlayMode gate
  produced zero new duplicate-listener warnings.
- The BUILD step is a compact five-slot runtime greybox with a live defense-plan summary; full device usability and performance remain unvalidated.
- Combat presentation uses bounded pivot motion plus a modular six-bone procedural Blood Knight pilot, action telegraphs, concise HUD/audio feedback, and one deliberately narrow direct-control dodge. Unity feedback drove corrected visual forward fit, doubled forward/back counter-swing and a continuous deep-crouch/one-leg-push/fall/landing timeline; its latest feel still needs user confirmation, and there is no final clip-driven animation rig or production VFX.
- Fog of war is a basic graph-driven show/hide implementation.
- AI uses direct steering instead of navigation/pathfinding; the defense invader now has only a narrow deterministic route-obstruction recovery, not general navigation.
- Realm-specific layout, colors, statistics, names and HUD wiring remain in their bootstraps; shared material, ability, entity, camera, light and EventSystem construction is centralized in a small core helper.
- Device controls, audio balance and performance have not yet been validated on a representative Android phone.
- Adaptive portrait/landscape layout plus selectable Contextual, Fingertap and Joystick control styles are implemented; physical-device rotation, focus-loss and layout checks remain outstanding.
- Direct-controlled characters now have one grounded jump, a strict 0.10-second post-edge coyote window, one non-refreshing 0.08-second falling pre-landing buffer and bounded pivot-only takeoff/landing accents, including a guarded Fingertap empty-ground double-tap path that keeps the initial destination for ordinary airborne movement. It intentionally has no double/wall/charged jump, hold-to-bunny-hop, air dodge/attack, Animator/root motion, gameplay VFX/audio or physical-device validation. A Unity Game-view smoke confirmed Sylvan portrait and landscape control copy; the new timing/visual accents plus possessed-Defender direct-control Game View remain manually unobserved because QA automation lacked safe Game View interaction, although their control paths are covered by PlayMode tests.
- Camera framing, threat-cue readability, safe-area layout and state-continuity behavior are covered in code/tests. Joystick manual yaw and factual Fingertap locomotion recentering are intentionally presentation-only; untargeted direct attacks now use the visible camera plane without target lock or camera-relative movement. Physical-device noticeability, rotation continuity, attack feel and comfort remain unverified.
- Android Studio and Xcode export checks are documented in `Docs/PLATFORM_BUILDS.md`; a physical-device performance/usability pass remains outstanding.
- The visual-tuning Modules package is installed and its pure-data tests run, but no module provider/profile is discovered or integrated into runtime presentation yet.
- The starter-character-catalogue package is installed and explicitly hosted as the five-entry prototype identity roster; authoritative stats, abilities, roles and scene construction intentionally remain in Core bootstraps.
- The modular-character-recipes package now includes deterministic explicit-provider catalogue construction in the Modules submodule but remains uninstalled; no approved module library or starter recipe instances exist yet.
- The character-motion-profiles package now includes deterministic explicit-provider catalogue construction, compatibility evaluation and explicit binding-batch evidence but remains uninstalled; its isolated package tests are authored but not yet run. MMP05 freezes a CC0 UAL2 pilot intake, but no archive, approved shared rig, clip binding, concrete motion provider or Core presentation adapter exists yet.
- The character-art-manifests package now includes deterministic evidence validation, hashing, explicit-provider catalogue construction, an adapter-neutral measurement gate and immutable batch reporting but remains uninstalled; its isolated package tests are authored but not yet run, and no concrete source manifest, measurement adapter or Unity importer is approved.
- Guardian Ent now has an exact creator-published CC0 Tree01 animated Generic
  pilot plus deterministic accepted-static and primitive fallbacks. Idle, Run,
  Attack_1 and Death1 drive only the owned visual skeleton; runtime events and root
  motion are disabled. It still exceeds the production LOD0 cap at 5,438 triangles,
  has no LOD1/LOD2, relies on importer pruning for 373 source points above four
  influences, and needs user-owned motion-feel/device validation.
- The Guardian Ent visual direction has reached a reversible pilot; Sylvan
  environment production remains incomplete beyond the accepted preview surfaces.
- Infernal Brute production is also design-only: its Obsidian Gatebreaker direction still needs a human-approved owned or third-party source and a shared Ent/Brute LargeCreature rig compatibility proof.
- Beast production is design-only: the Wolf/Hellhound family still needs a human-approved source/provenance route and one frozen Beast rig/bind/anchor proof before assets or shared clips are produced.
- Prototype arena boundaries are implemented and automated collision/AI coverage is green; the P0 factual Portal → Crossroads movement/dash regression is also green. Full physical-device edge/action, encounter-completion and portrait-to-landscape feel remain user-owned.
- The 12.1 automated proof now confirms ordinary Portal → Crossroads traversal through the first large node and preserved outer-boundary containment. QA confirmed the recovered Sylvan landscape Game View renders, but its automation surface could not safely inject the direct walk or outer-edge input; those manual/device observations remain user-owned.
- Infernal environment production is design-only: the Ironbound Rift Causeway kit still needs a human-approved source/provenance route and neutral-gray production proof before Unity integration.
- The Hub notice panel presents the release-cleared 3DRT and Kenney records. The specifically CC0-labelled Quaternius UAL2 pack is accepted only as a future motion-source candidate; it remains absent from runtime and notices until an official archive is acquired, its embedded terms/hashes are recorded and actual files are integrated.
- The first-playable-minute proof and canonical Sylvan `BUILD → RAID → DEFEND → BUILD` journey are implemented through real results; physical-device noticeability, wider onboarding and optional replay/settings remain unfinished.
- Realm landmarks, central routes and continuous arena boundaries now have distinct organic versus angular primitive blockouts, but terrain, final materials/textures, environment assets and physical-device portrait review remain unfinished.
- The first imported realm surfaces are intentionally reversible preview art with local original-generated provenance. Route and selected floor/boundary normals now exist, but their physical-device strength/periodicity, second-tile or macro variation, final-art approval and performance measurements remain unfinished.
- The Jump icon is an original-generated preview with local provenance, not final approved UI art. Its focused automated behavior is green, but its planned Defender/Sylvan Game View and physical-device readability checks remain pending.
- The 12.6 Build Plan preview was manually readable in portrait. Its manual Game-view slot-cycle and `1280×720` landscape smoke were not observed because pointer input was unavailable during the check; the focused PlayMode flow test covers those behaviors.

- Diamond Pass 18.3 Authored Sylvan Encounter Variants is present: the Hub now
  offers a session-only Baseline/Wolf Pressure/Sentinel Escort choice, and direct
  raid, journey and retry materialize its exact package-authored enemies through
  existing node/reward authority. Wolf Pressure keeps its Moonwell Wolf hidden
  until reveal, every run has one exact Ent bonus target, and the Raid HUD names
  the active variant. Final QA passed EditMode `423/423` and PlayMode `116/116`.
- Diamond Pass 18.4 Authored Infernal Ent Raid Variants is present: Brute Finale
  remains the default, Entry Trial removes the Flame Trap and uses an all-hostiles
  Heart gate, and Risk Route retains two Hellhounds plus one bypassable Flame Trap
  with the same truthful gate. Direct launch and retry retain the shown selection;
  final QA passed EditMode `434/434` and PlayMode `118/118`.
- Modules MGC05–MGC08 are pinned: Infernal pacing/spatial/presentation facts and
  Sylvan presentation summaries remain immutable no-engine data. Core alone owns
  selection, spawning, combat, objectives, rewards and UI.
- Diamond Pass 18.5 Keeper Reserve Defense Tradeoff is present: `PACK PRESSURE`
  keeps two autonomous Wolves and 30 seconds of control; `KEEPER RESERVE` leaves
  one creature slot open for 45 seconds. The existing save remains the only source
  of truth, scene reload creates a fresh local pool, and explicit release retains
  remaining energy. Final QA passed EditMode `443/443` and PlayMode `118/118`.
- Module MGC09 is pinned: its no-engine evaluator owns only the two immutable
  roster/copy/energy facts. Core still validates layouts and owns persistence,
  spawning, possession, timers and UI.
- Diamond Pass 18.6 Guardian Ent Ground Slam Impact VFX is present: an eligible
  exact Ground Slam hit or whiff emits one short collider-free Sylvan root-energy
  ring at the authoritative Area centre and radius; connected-hit impact remains
  distinct and every controller/death/terminal/disable/destroy path clears it.
  Final QA passed EditMode `444/444` and PlayMode `120/120`.

## Directory guide

```text
Assets/Game/
  Editor/             Project setup and platform export tools
  Scenes/             Hub, sandbox, raid and defense entry scenes
  Scripts/
    AI/               Creature and invader controllers
    Camera/           Keeper, Hero and possessed camera modes
    Characters/       Shared combat entity and definitions
    Combat/           Stats, health, damage and abilities
    Controllers/      Swappable player/controller contract
    Core/             Scene bootstraps and prototype persistence
    Possession/       Controller swap and possession energy
    Raid/             Raid and defense state/results
    Realm/            Realm graph, fog views and Core objective
    Traps/            Shared trap state plus race-specific traps
    UI/               Runtime prototype HUDs
    Modules/          Passive host contracts for separately reviewed packages
  Tests/
    EditMode/         Pure logic tests
    PlayMode/         Scene and gameplay-flow tests
Docs/                 Product context, status, builds and backlog
Modules/              Git submodule: isolated package catalogue
Builds/               Generated Android Studio/Xcode projects; Git-ignored
```
