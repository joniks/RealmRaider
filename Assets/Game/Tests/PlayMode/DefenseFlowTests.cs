using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.AI;
using RealmRaiders.CameraSystem;
using RealmRaiders.Controllers;
using RealmRaiders.Core;
using RealmRaiders.Possession;
using RealmRaiders.Raid;
using RealmRaiders.Realm;
using RealmRaiders.Traps;
using RealmRaiders.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RealmRaiders.Tests
{
    public sealed class DefenseFlowTests
    {
        [UnityTest]
        public IEnumerator DefenderVictoryCannotBeOverwrittenByLaterCoreLoss()
        {
            var invaderObject = new GameObject("Test Invader", typeof(CharacterController), typeof(Health), typeof(CombatEntity));
            var coreObject = new GameObject("Test Core", typeof(RealmCore));
            var possessionObject = new GameObject("Test Possession", typeof(PossessionManager));
            var defenseObject = new GameObject("Test Defense", typeof(DefenseManager));
            var definition = ScriptableObject.CreateInstance<CharacterDefinition>();
            try
            {
                definition.DisplayName = "Test Invader";
                definition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 1, AttackSpeed = 1 };
                definition.Abilities = System.Array.Empty<AbilityDefinition>();
                var invader = invaderObject.GetComponent<CombatEntity>();
                invader.Initialize(definition);
                var core = coreObject.GetComponent<RealmCore>();
                core.InteractionDuration = .001f;
                core.Initialize(invader);
                var defense = defenseObject.GetComponent<DefenseManager>();
                defense.Initialize(invader, core, possessionObject.GetComponent<PossessionManager>());
                var results = new List<DefenseResultFact>();
                defense.Finished += results.Add;

                invader.Health.TakeDamage(new DamageInfo(1000, null, invader.transform.position), 0);
                Assert.That(defense.State, Is.EqualTo(DefenseState.DefenderVictory));
                Assert.That(defense.HasResult, Is.True);
                Assert.That(results, Has.Count.EqualTo(1));
                var frozen = defense.Result;
                Assert.That(frozen.Outcome, Is.EqualTo(DefenseState.DefenderVictory));
                Assert.That(frozen.InvaderHealth, Is.Zero);
                Assert.That(frozen.InvaderMaximumHealth, Is.EqualTo(100));
                Assert.That(frozen.CoreProgress, Is.Zero);
                invader.Health.RestoreFull();
                yield return null;
                yield return null;
                Assert.That(defense.State, Is.EqualTo(DefenseState.DefenderVictory));
                Assert.That(defense.IsFinished, Is.True);
                Assert.That(results, Has.Count.EqualTo(1), "A later Core callback cannot publish another terminal fact.");
                Assert.That(defense.Result.Outcome, Is.EqualTo(frozen.Outcome));
                Assert.That(defense.Result.Duration, Is.EqualTo(frozen.Duration), "Live time cannot overwrite the frozen victory duration.");
                Assert.That(defense.Result.InvaderHealth, Is.EqualTo(frozen.InvaderHealth));
                Assert.That(defense.Result.CoreProgress, Is.EqualTo(frozen.CoreProgress), "Live Core progress cannot overwrite the frozen victory fact.");
            }
            finally
            {
                Object.Destroy(defenseObject); Object.Destroy(possessionObject); Object.Destroy(coreObject);
                Object.Destroy(invaderObject); Object.Destroy(definition);
            }
        }

        [UnityTest]
        public IEnumerator RealmLostCannotBeOverwrittenByLaterInvaderDeath()
        {
            var invaderObject = new GameObject("Test Invader", typeof(CharacterController), typeof(Health), typeof(CombatEntity));
            var coreObject = new GameObject("Test Core", typeof(RealmCore));
            var possessionObject = new GameObject("Test Possession", typeof(PossessionManager));
            var defenseObject = new GameObject("Test Defense", typeof(DefenseManager));
            var definition = ScriptableObject.CreateInstance<CharacterDefinition>();

            try
            {
                definition.DisplayName = "Test Invader";
                definition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 1, AttackSpeed = 1 };
                definition.Abilities = System.Array.Empty<AbilityDefinition>();

                var invader = invaderObject.GetComponent<CombatEntity>();
                invader.Initialize(definition);
                var core = coreObject.GetComponent<RealmCore>();
                core.InteractionDuration = .001f;
                core.Initialize(invader);
                var defense = defenseObject.GetComponent<DefenseManager>();
                defense.Initialize(invader, core, possessionObject.GetComponent<PossessionManager>());
                var results = new List<DefenseResultFact>();
                defense.Finished += results.Add;

                yield return null;
                yield return null;
                Assert.That(defense.State, Is.EqualTo(DefenseState.RealmLost));
                Assert.That(defense.HasResult, Is.True);
                Assert.That(results, Has.Count.EqualTo(1));
                var frozen = defense.Result;
                Assert.That(frozen.Outcome, Is.EqualTo(DefenseState.RealmLost));
                Assert.That(frozen.InvaderHealth, Is.EqualTo(100));
                Assert.That(frozen.InvaderMaximumHealth, Is.EqualTo(100));
                Assert.That(frozen.CoreProgress, Is.EqualTo(1));

                invader.Health.TakeDamage(new DamageInfo(1000, null, invader.transform.position), 0);
                yield return null;

                Assert.That(defense.State, Is.EqualTo(DefenseState.RealmLost));
                Assert.That(defense.IsFinished, Is.True);
                Assert.That(results, Has.Count.EqualTo(1), "A later death callback cannot publish another terminal fact.");
                Assert.That(defense.Result.Outcome, Is.EqualTo(frozen.Outcome));
                Assert.That(defense.Result.Duration, Is.EqualTo(frozen.Duration), "Live time cannot overwrite the frozen loss duration.");
                Assert.That(defense.Result.InvaderHealth, Is.EqualTo(frozen.InvaderHealth), "Live health cannot overwrite the frozen loss fact.");
                Assert.That(defense.Result.CoreProgress, Is.EqualTo(frozen.CoreProgress));
            }
            finally
            {
                Object.Destroy(defenseObject);
                Object.Destroy(possessionObject);
                Object.Destroy(coreObject);
                Object.Destroy(invaderObject);
                Object.Destroy(definition);
            }
        }

        [UnityTest]
        public IEnumerator DefenderOpeningBeat_HoldsInvaderThenResumesAndCleansPreparationCue()
        {
            var cameraObject = new GameObject("Test Camera", typeof(Camera), typeof(AudioListener), typeof(PrototypeCameraRig)); cameraObject.tag = "MainCamera";
            var invaderObject = new GameObject("Test Invader", typeof(CharacterController), typeof(Health), typeof(CombatEntity), typeof(RaidInvaderBrain));
            var defenderObject = new GameObject("Test Defender", typeof(CharacterController), typeof(Health), typeof(CombatEntity), typeof(PlayerController), typeof(CreatureBrain));
            var coreObject = new GameObject("Test Core", typeof(RealmCore));
            var possessionObject = new GameObject("Test Possession", typeof(PossessionManager));
            var defenseObject = new GameObject("Test Defense", typeof(DefenseManager));
            var trapObject = new GameObject("Test Trap", typeof(RootTrap));
            var hudObject = new GameObject("Test Defender HUD", typeof(DefenderHUD));
            var invaderDefinition = ScriptableObject.CreateInstance<CharacterDefinition>();
            var defenderDefinition = ScriptableObject.CreateInstance<CharacterDefinition>();
            GameplayInput.SetTerminalState(false);
            try
            {
                invaderDefinition.DisplayName = "Test Invader"; invaderDefinition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 4, AttackSpeed = 1 }; invaderDefinition.Abilities = System.Array.Empty<AbilityDefinition>();
                defenderDefinition.DisplayName = "Test Defender"; defenderDefinition.Possessable = true; defenderDefinition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 3, AttackSpeed = 1 }; defenderDefinition.Abilities = System.Array.Empty<AbilityDefinition>();
                var invader = invaderObject.GetComponent<CombatEntity>(); invader.Initialize(invaderDefinition);
                var defender = defenderObject.GetComponent<CombatEntity>(); defenderObject.transform.position = new Vector3(100, 1, 0); defender.Initialize(defenderDefinition); defender.SetController(defender.Controller<CreatureBrain>());
                var brain = invaderObject.GetComponent<RaidInvaderBrain>(); brain.Configure(new[] { invaderObject.transform.position, new Vector3(0, 1, 5) }, new[] { defender }, .25f); invader.SetController(brain);
                var core = coreObject.GetComponent<RealmCore>(); coreObject.transform.position = new Vector3(0, 0, 40); core.Initialize(invader);
                var possession = possessionObject.GetComponent<PossessionManager>(); var rig = cameraObject.GetComponent<PrototypeCameraRig>(); rig.ConfigureOverview(new Vector3(0, 12, -12), Quaternion.Euler(45, 0, 0)); rig.SnapToOverview(); possession.Initialize(rig); var energy = new PossessionEnergy(30); possession.ConfigureEnergy(energy); possession.Register(defender);
                var defense = defenseObject.GetComponent<DefenseManager>(); defense.Initialize(invader, core, possession);
                var trap = trapObject.GetComponent<RootTrap>(); trap.Automatic = false; trap.Initialize(invader);
                var canvasBeforeHud = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Length;
                var eventSystemsBeforeHud = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length;
                var listenersBeforeHud = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length;
                var hud = hudObject.GetComponent<DefenderHUD>(); hud.Initialize(defense, possession, energy, invader, defender, trap, core, DefenseHudConfig.Sylvan);

                var start = invaderObject.transform.position;
                brain.Tick(); hud.SendMessage("RefreshOpeningCue", SendMessageOptions.RequireReceiver);
                Assert.That(brain.IsOpeningHold, Is.True);
                Assert.That(brain.CurrentTarget, Is.Null);
                Assert.That(invaderObject.transform.position.x, Is.EqualTo(start.x).Within(.001f));
                Assert.That(invaderObject.transform.position.z, Is.EqualTo(start.z).Within(.001f));
                Assert.That(defender.Health.Current, Is.EqualTo(defender.Health.Maximum));
                Assert.That(hud.OpeningCueVisible, Is.True);
                Assert.That(hud.OpeningCueRaycastTarget, Is.False);
                Assert.That(hud.RouteStatusText, Does.Contain("INVADER HOLDING — ROOT GATE AHEAD"));
                Assert.That(hud.RouteStatusRaycastTarget, Is.False);
                Assert.That(Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None), Has.Length.EqualTo(canvasBeforeHud + 1));
                Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None), Has.Length.EqualTo(eventSystemsBeforeHud));
                Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None), Has.Length.EqualTo(listenersBeforeHud));
                var responsive = Object.FindFirstObjectByType<ResponsiveHudRoot>();
                responsive.SetOrientationForTests(PrototypeOrientation.Portrait); AssertRouteStatusClear();
                responsive.SetOrientationForTests(PrototypeOrientation.Landscape); AssertRouteStatusClear();

                possession.Select(defender);
                Assert.That(possession.PossessSelected(), Is.True);
                Assert.That(hud.OpeningCueVisible, Is.False);
                possession.Release(); hud.SendMessage("RefreshOpeningCue", SendMessageOptions.RequireReceiver);
                Assert.That(hud.OpeningCueVisible, Is.False);
                var openingDeadline = Time.realtimeSinceStartup + .75f;
                while ((brain.IsOpeningHold || brain.WaypointIndex == 0) && Time.realtimeSinceStartup < openingDeadline)
                {
                    brain.Tick();
                    yield return null;
                }
                Assert.That(brain.IsOpeningHold, Is.False);
                Assert.That(brain.WaypointIndex, Is.EqualTo(1));
                hud.SendMessage("RefreshRouteStatus", SendMessageOptions.RequireReceiver);
                Assert.That(hud.RouteStatusText, Does.Contain("ROOT GATE"));

                brain.Configure(System.Array.Empty<Vector3>(), new[] { defender }, 0);
                invader.SetController(brain);
                defenderObject.transform.position = invaderObject.transform.position + Vector3.forward;
                brain.Tick(); hud.SendMessage("RefreshRouteStatus", SendMessageOptions.RequireReceiver);
                Assert.That(brain.CurrentTarget, Is.EqualTo(defender));
                Assert.That(hud.RouteStatusText, Is.EqualTo("INVADER ENGAGING TEST DEFENDER"));

                invader.Health.TakeDamage(new DamageInfo(1000, null, invader.transform.position), 0);
                Assert.That(defense.State, Is.EqualTo(DefenseState.DefenderVictory));
                Assert.That(hud.OpeningCueVisible, Is.False);
                Assert.That(hud.RouteStatusText, Is.Empty);
            }
            finally
            {
                GameplayInput.SetTerminalState(false);
                Object.Destroy(hudObject); Object.Destroy(trapObject); Object.Destroy(defenseObject); Object.Destroy(possessionObject); Object.Destroy(coreObject);
                Object.Destroy(defenderObject); Object.Destroy(invaderObject); Object.Destroy(cameraObject); Object.Destroy(invaderDefinition); Object.Destroy(defenderDefinition);
            }
        }

        [UnityTest]
        public IEnumerator RootTrapOpportunity_UsesOnlyLiveSylvanFactsAndCleansEveryLifecycle()
        {
            GameplayInput.SetTerminalState(false);
            var fixture = new RootOpportunityFixture(DefenseHudConfig.Sylvan, false);
            try
            {
                Assert.That(fixture.Hud.RootTrapOpportunityVisible, Is.False, "An unused trap must not advertise the combo.");
                Assert.That(fixture.Trap.TryActivate(), Is.False, "An out-of-range activation must remain failed.");
                fixture.Refresh();
                Assert.That(fixture.Hud.RootTrapOpportunityVisible, Is.False);

                fixture.Invader.transform.position = fixture.Trap.transform.position;
                Physics.SyncTransforms();
                fixture.ActivateThroughHud();
                Assert.That(fixture.Trap.RecentlyActivated, Is.True);
                Assert.That(fixture.Invader.IsRooted, Is.True);
                Assert.That(fixture.Invader.Health.Current, Is.EqualTo(88).Within(.001f), "Root Trap damage must remain unchanged.");
                Assert.That(fixture.Hud.RootTrapOpportunityVisible, Is.False, "An unregistered Ent is not a truthful possession opportunity.");

                fixture.Possession.Register(fixture.Defender);
                fixture.Refresh();
                Assert.That(fixture.Rig.IsTransitioning, Is.True, "Trap focus temporarily owns the camera.");
                Assert.That(fixture.Possession.CanSelect(fixture.Defender), Is.False, "Selection is rejected during the trap-focus transition.");
                Assert.That(fixture.Hud.RootTrapOpportunityVisible, Is.False, "The cue cannot invite a rejected selection.");
                fixture.SettleKeeperCamera();
                fixture.Refresh();
                AssertOpportunityVisible(fixture);
                Assert.That(fixture.Possession.CanSelect(fixture.Defender), Is.True);
                Assert.That(fixture.Possession.Selected, Is.Null, "The cue must not select for the player.");
                Assert.That(fixture.Possession.Possessed, Is.Null, "The cue must not possess for the player.");
                Assert.That(fixture.Defender.ActiveController, Is.SameAs(fixture.Defender.Controller<CreatureBrain>()));
                Assert.That(fixture.Defender.Abilities[2].CooldownRemaining, Is.EqualTo(0).Within(.001f), "The cue must not activate Ground Slam.");
                Assert.That(Object.FindObjectsByType<CombatEntity>(FindObjectsSortMode.None), Has.Length.EqualTo(fixture.GameplayEntitiesAfterBuild));
                Assert.That(Object.FindObjectsByType<RootTrap>(FindObjectsSortMode.None), Has.Length.EqualTo(fixture.RootTrapsAfterBuild));
                AssertOpportunityLayout(fixture, PrototypeOrientation.Portrait);
                AssertOpportunityLayout(fixture, PrototypeOrientation.Landscape);

                fixture.Rig.SnapTo(fixture.Defender, CameraMode.HeroCombat);
                fixture.Refresh();
                Assert.That(fixture.Possession.CanSelect(fixture.Defender), Is.False, "Selection requires Keeper overview.");
                Assert.That(fixture.Hud.RootTrapOpportunityVisible, Is.False);
                fixture.SettleKeeperCamera();
                fixture.Refresh();
                AssertOpportunityVisible(fixture);

                fixture.Possession.enabled = false;
                fixture.Refresh();
                Assert.That(fixture.Possession.CanSelect(fixture.Defender), Is.False, "A disabled manager cannot accept Keeper selection.");
                Assert.That(fixture.Hud.RootTrapOpportunityVisible, Is.False);
                fixture.Possession.enabled = true;
                fixture.Refresh();
                AssertOpportunityVisible(fixture);

                var exactDefender = fixture.Defender;
                var defenderHealth = exactDefender.Health.Current;
                fixture.Possession.Select(exactDefender);
                Assert.That(fixture.Possession.PossessSelected(), Is.True);
                fixture.Hud.SendMessage("RefreshAbilityButtons", SendMessageOptions.RequireReceiver);
                Assert.That(fixture.Hud.RootTrapOpportunityVisible, Is.False, "Possession immediately owns the moment.");
                Assert.That(fixture.Possession.Possessed, Is.SameAs(exactDefender));
                Assert.That(exactDefender.Health.Current, Is.EqualTo(defenderHealth));
                Assert.That(fixture.Invader.IsRooted, Is.True);
                Assert.That(exactDefender.ActiveController, Is.SameAs(exactDefender.Controller<PlayerController>()));
                Assert.That(fixture.Hud.AbilityButtonText(1), Does.StartWith("GROUND SLAM"));
                Assert.That(fixture.Hud.AbilityButtonInteractable(1), Is.True);
                Assert.That(exactDefender.Abilities[2].CooldownRemaining, Is.EqualTo(0).Within(.001f));

                fixture.Possession.Release();
                fixture.Refresh();
                Assert.That(fixture.Possession.CanSelect(exactDefender), Is.False, "Release transition must not advertise an unavailable tap.");
                Assert.That(fixture.Hud.RootTrapOpportunityVisible, Is.False);
                fixture.SettleKeeperCamera();
                fixture.Refresh();
                AssertOpportunityVisible(fixture);
                Assert.That(exactDefender.ActiveController, Is.SameAs(exactDefender.Controller<CreatureBrain>()));

                fixture.Energy.Consume(fixture.Energy.Remaining);
                Assert.That(fixture.Hud.RootTrapOpportunityVisible, Is.False, "Depleted energy removes the opportunity.");
                fixture.Energy.Refill();
                AssertOpportunityVisible(fixture);

                fixture.Hud.enabled = false;
                Assert.That(fixture.Hud.RootTrapOpportunityVisible, Is.False, "HUD disable must clear presentation immediately.");
                fixture.Hud.enabled = true;
                fixture.Refresh();
                AssertOpportunityVisible(fixture);

                exactDefender.Health.TakeDamage(new DamageInfo(1000, null, exactDefender.transform.position), 0);
                Assert.That(fixture.Hud.RootTrapOpportunityVisible, Is.False, "A dead Guardian Ent cannot be offered.");
                fixture.Invader.Health.TakeDamage(new DamageInfo(1000, null, fixture.Invader.transform.position), 0);
                Assert.That(fixture.Defense.IsFinished, Is.True);
                Assert.That(fixture.Hud.RootTrapOpportunityVisible, Is.False, "Terminal result presentation must own the HUD.");

                Object.Destroy(fixture.Hud.gameObject);
                yield return null;
                Assert.That(Object.FindObjectsByType<Text>(FindObjectsSortMode.None).Count(text => text.name == "Root Trap Possession Opportunity"), Is.Zero);
            }
            finally
            {
                fixture.Destroy();
                GameplayInput.SetTerminalState(false);
            }
            yield return null;

            var expiredRoot = new RootOpportunityFixture(DefenseHudConfig.Sylvan, true);
            try
            {
                expiredRoot.Invader.transform.position = expiredRoot.Trap.transform.position;
                Physics.SyncTransforms();
                expiredRoot.ActivateThroughHud();
                expiredRoot.SettleKeeperCamera();
                expiredRoot.Refresh();
                AssertOpportunityVisible(expiredRoot);
                expiredRoot.Invader.BreakRoot();
                expiredRoot.Refresh();
                Assert.That(expiredRoot.Hud.RootTrapOpportunityVisible, Is.False, "Root expiry removes the factual opportunity.");
            }
            finally { expiredRoot.Destroy(); GameplayInput.SetTerminalState(false); }
            yield return null;

            var infernal = new RootOpportunityFixture(DefenseHudConfig.Infernal, true);
            try
            {
                infernal.Invader.transform.position = infernal.Trap.transform.position;
                Physics.SyncTransforms();
                infernal.ActivateThroughHud();
                Assert.That(infernal.Invader.IsRooted, Is.True);
                Assert.That(infernal.Hud.RootTrapOpportunityVisible, Is.False, "A non-Sylvan HUD never advertises the Sylvan combo.");
            }
            finally { infernal.Destroy(); GameplayInput.SetTerminalState(false); }
            yield return null;

            foreach (var abilityState in new[] { GroundSlamFixtureState.Missing, GroundSlamFixtureState.Wrong, GroundSlamFixtureState.Cooldown })
            {
                var unavailable = new RootOpportunityFixture(DefenseHudConfig.Sylvan, true, abilityState);
                try
                {
                    if (abilityState == GroundSlamFixtureState.Cooldown)
                        Assert.That(unavailable.Defender.Abilities[2].TryConsume(), Is.True);
                    unavailable.Invader.transform.position = unavailable.Trap.transform.position;
                    Physics.SyncTransforms();
                    unavailable.ActivateThroughHud();
                    unavailable.SettleKeeperCamera();
                    unavailable.Refresh();
                    Assert.That(unavailable.Hud.RootTrapOpportunityVisible, Is.False, $"A {abilityState} Ground Slam cannot be advertised.");
                }
                finally { unavailable.Destroy(); GameplayInput.SetTerminalState(false); }
                yield return null;
            }

            var retry = new RootOpportunityFixture(DefenseHudConfig.Sylvan, true);
            try
            {
                Assert.That(retry.Hud.RootTrapOpportunityVisible, Is.False, "A clean retry starts without stale presentation.");
                Assert.That(retry.Hud.GetComponentsInChildren<Text>(true).Count(text => text.name == "Root Trap Possession Opportunity"), Is.EqualTo(1));
                retry.Invader.transform.position = retry.Trap.transform.position;
                Physics.SyncTransforms();
                retry.ActivateThroughHud();
                retry.SettleKeeperCamera();
                retry.Refresh();
                AssertOpportunityVisible(retry);
            }
            finally { retry.Destroy(); GameplayInput.SetTerminalState(false); }
        }

        static void AssertOpportunityVisible(RootOpportunityFixture fixture)
        {
            Assert.That(fixture.Hud.RootTrapOpportunityVisible, Is.True);
            Assert.That(fixture.Hud.RootTrapOpportunityText, Is.EqualTo("ROOTED — POSSESS ENT, THEN GROUND SLAM"));
            Assert.That(fixture.Hud.RootTrapOpportunityRaycastTarget, Is.False);
            Assert.That(fixture.Hud.GetComponentsInChildren<Text>(true).Count(text => text.name == "Root Trap Possession Opportunity"), Is.EqualTo(1));
            Assert.That(Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None), Has.Length.EqualTo(fixture.CanvasBeforeHud + 1));
            Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None), Has.Length.EqualTo(fixture.EventSystemsBeforeHud));
            Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None), Has.Length.EqualTo(fixture.ListenersBeforeHud));
        }

        static void AssertOpportunityLayout(RootOpportunityFixture fixture, PrototypeOrientation orientation)
        {
            fixture.Hud.GetComponent<ResponsiveHudRoot>().SetOrientationForTests(orientation);
            var reference = fixture.Hud.GetComponent<CanvasScaler>().referenceResolution;
            var opportunity = DesignRect(fixture.Hud.RootTrapOpportunityRect, reference);
            Assert.That(opportunity.xMin, Is.GreaterThanOrEqualTo(0));
            Assert.That(opportunity.yMin, Is.GreaterThanOrEqualTo(0));
            Assert.That(opportunity.xMax, Is.LessThanOrEqualTo(reference.x), $"Opportunity must remain inside {orientation} safe layout.");
            Assert.That(opportunity.yMax, Is.LessThanOrEqualTo(reference.y), $"Opportunity must remain inside {orientation} safe layout.");
            var trapStatus = fixture.Hud.GetComponentsInChildren<Text>(true).Single(text => text.name == "Trap Status");
            Assert.That(opportunity.Overlaps(DesignRect(trapStatus.rectTransform, reference)), Is.False, $"Opportunity overlaps trap copy in {orientation}.");
            foreach (var button in fixture.Hud.GetComponentsInChildren<Button>(true).Where(button => button.transform.parent == fixture.Hud.transform))
                Assert.That(opportunity.Overlaps(DesignRect((RectTransform)button.transform, reference)), Is.False, $"Opportunity overlaps {button.name} in {orientation}.");
        }

        enum GroundSlamFixtureState { Ready, Missing, Wrong, Cooldown }

        sealed class RootOpportunityFixture
        {
            readonly GameObject cameraObject, invaderObject, defenderObject, coreObject, possessionObject, defenseObject, trapObject;
            readonly CharacterDefinition invaderDefinition, defenderDefinition;
            readonly AbilityDefinition smash, charge, groundSlam;
            public readonly DefenderHUD Hud;
            public readonly CombatEntity Invader, Defender;
            public readonly PossessionManager Possession;
            public readonly PossessionEnergy Energy;
            public readonly DefenseManager Defense;
            public readonly RootTrap Trap;
            public readonly PrototypeCameraRig Rig;
            public readonly int CanvasBeforeHud, EventSystemsBeforeHud, ListenersBeforeHud, GameplayEntitiesAfterBuild, RootTrapsAfterBuild;

            public RootOpportunityFixture(DefenseHudConfig config, bool registerDefender, GroundSlamFixtureState groundSlamState = GroundSlamFixtureState.Ready)
            {
                cameraObject = new GameObject("Root Opportunity Camera", typeof(Camera), typeof(AudioListener), typeof(PrototypeCameraRig)); cameraObject.tag = "MainCamera";
                invaderObject = new GameObject("Root Opportunity Invader", typeof(CharacterController), typeof(Health), typeof(CombatEntity));
                defenderObject = new GameObject("Root Opportunity Guardian", typeof(CharacterController), typeof(Health), typeof(CombatEntity), typeof(PlayerController), typeof(CreatureBrain));
                coreObject = new GameObject("Root Opportunity Core", typeof(RealmCore));
                possessionObject = new GameObject("Root Opportunity Possession", typeof(PossessionManager));
                defenseObject = new GameObject("Root Opportunity Defense", typeof(DefenseManager));
                trapObject = new GameObject("Root Opportunity Trap", typeof(RootTrap));
                var hudObject = new GameObject("Root Opportunity HUD", typeof(DefenderHUD));
                invaderDefinition = ScriptableObject.CreateInstance<CharacterDefinition>();
                defenderDefinition = ScriptableObject.CreateInstance<CharacterDefinition>();
                smash = Ability("Smash", AbilityKind.Melee); charge = Ability("Charge", AbilityKind.Dash);
                groundSlam = Ability(groundSlamState == GroundSlamFixtureState.Wrong ? "Thorn Burst" : "Ground Slam", AbilityKind.Area);
                invaderDefinition.DisplayName = "Raid Invader"; invaderDefinition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 3, AttackSpeed = 1 }; invaderDefinition.Abilities = System.Array.Empty<AbilityDefinition>();
                defenderDefinition.DisplayName = "Guardian Ent"; defenderDefinition.ArchetypeId = PrototypeCharacterRoster.GuardianEntId; defenderDefinition.Possessable = true;
                defenderDefinition.Stats = new CombatStats { MaxHealth = 120, MoveSpeed = 3, AttackSpeed = 1 };
                defenderDefinition.Abilities = groundSlamState == GroundSlamFixtureState.Missing ? new[] { smash, charge } : new[] { smash, charge, groundSlam };
                Invader = invaderObject.GetComponent<CombatEntity>(); Invader.Initialize(invaderDefinition);
                Defender = defenderObject.GetComponent<CombatEntity>(); Defender.Initialize(defenderDefinition); Defender.SetController(Defender.Controller<CreatureBrain>()); defenderObject.transform.position = new Vector3(20, 0, 0);
                Rig = cameraObject.GetComponent<PrototypeCameraRig>(); Rig.ConfigureOverview(new Vector3(0, 12, -12), Quaternion.Euler(45, 0, 0)); Rig.SnapToOverview();
                Possession = possessionObject.GetComponent<PossessionManager>(); Possession.Initialize(Rig); Energy = new PossessionEnergy(30); Possession.ConfigureEnergy(Energy); if (registerDefender) Possession.Register(Defender);
                coreObject.transform.position = new Vector3(100, 0, 100); var core = coreObject.GetComponent<RealmCore>(); core.Initialize(Invader);
                Defense = defenseObject.GetComponent<DefenseManager>(); Defense.Initialize(Invader, core, Possession);
                trapObject.transform.position = new Vector3(12, 0, 0); Trap = trapObject.GetComponent<RootTrap>(); Trap.Automatic = false; Trap.Initialize(Invader);
                CanvasBeforeHud = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Length;
                EventSystemsBeforeHud = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length;
                ListenersBeforeHud = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length;
                Hud = hudObject.GetComponent<DefenderHUD>(); Hud.Initialize(Defense, Possession, Energy, Invader, Defender, Trap, core, config);
                GameplayEntitiesAfterBuild = Object.FindObjectsByType<CombatEntity>(FindObjectsSortMode.None).Length;
                RootTrapsAfterBuild = Object.FindObjectsByType<RootTrap>(FindObjectsSortMode.None).Length;
            }

            public void Refresh() => Hud.SendMessage("Refresh", SendMessageOptions.RequireReceiver);

            public void SettleKeeperCamera()
            {
                Rig.StopAllCoroutines();
                Rig.SnapTo(null, CameraMode.KeeperOverview);
            }

            public void ActivateThroughHud()
            {
                Refresh();
                Hud.GetComponentsInChildren<Button>(true).Single(button => button.name == "ACTIVATE TRAP").onClick.Invoke();
                Refresh();
            }

            public void Destroy()
            {
                if (Hud) Object.Destroy(Hud.gameObject);
                Object.Destroy(trapObject); Object.Destroy(defenseObject); Object.Destroy(possessionObject); Object.Destroy(coreObject);
                Object.Destroy(defenderObject); Object.Destroy(invaderObject); Object.Destroy(cameraObject);
                Object.Destroy(invaderDefinition); Object.Destroy(defenderDefinition); Object.Destroy(smash); Object.Destroy(charge); Object.Destroy(groundSlam);
            }

            static AbilityDefinition Ability(string name, AbilityKind kind)
            {
                var ability = ScriptableObject.CreateInstance<AbilityDefinition>(); ability.DisplayName = name; ability.Kind = kind;
                ability.Damage = 10; ability.Range = 2; ability.Radius = 3; ability.Windup = .01f; ability.Recovery = .01f; ability.Cooldown = .5f;
                return ability;
            }
        }

        static void AssertRouteStatusClear()
        {
            var route = GameObject.Find("Invader Route Status").GetComponent<RectTransform>();
            var reference = Object.FindFirstObjectByType<CanvasScaler>().referenceResolution;
            foreach (var button in Object.FindObjectsByType<Button>(FindObjectsSortMode.None))
                Assert.That(DesignRect(route, reference).Overlaps(DesignRect(button.GetComponent<RectTransform>(), reference)), Is.False, $"Route status overlaps {button.name}");
            foreach (var label in Object.FindObjectsByType<Text>(FindObjectsSortMode.None))
                if (label.rectTransform != route && !label.GetComponentInParent<Button>())
                    Assert.That(DesignRect(route, reference).Overlaps(DesignRect(label.rectTransform, reference)), Is.False, $"Route status overlaps {label.name}");
        }

        static Rect DesignRect(RectTransform rect, Vector2 parentSize)
        {
            var anchorMin = Vector2.Scale(rect.anchorMin, parentSize);
            var anchorSize = Vector2.Scale(rect.anchorMax - rect.anchorMin, parentSize);
            var size = anchorSize + rect.sizeDelta;
            var pivotPoint = anchorMin + Vector2.Scale(anchorSize, rect.pivot) + rect.anchoredPosition;
            return new Rect(pivotPoint - Vector2.Scale(size, rect.pivot), size);
        }
    }
}
