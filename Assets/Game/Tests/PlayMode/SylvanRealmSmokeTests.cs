using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Raid;
using RealmRaiders.Realm;
using RealmRaiders.Traps;
using RealmRaiders.Possession;
using RealmRaiders.AI;
using RealmRaiders.UI;
using RealmRaiders.Core;
using RealmRaiders.Controllers;
using RealmRaiders.CameraSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace RealmRaiders.Tests
{
    public sealed class SylvanRealmSmokeTests
    {
        [UnityTest]
        public IEnumerator RealmBuild_CreatesFiveSlotsAndBuildHud()
        {
            SceneManager.LoadScene("RealmBuild"); yield return null; yield return null;
            var hud = Object.FindFirstObjectByType<BuildHUD>();
            Assert.That(hud, Is.Not.Null); Assert.That(hud.SlotCount, Is.EqualTo(5));
            var responsive = hud.GetComponent<ResponsiveHudRoot>();
            responsive.SetOrientationForTests(PrototypeOrientation.Portrait); yield return null;
            AssertRealmIdentityMark(hud, HudPresentation.SylvanRealmIdentity, HudPresentation.SylvanRealmIdentityIconResource, "SYLVAN BUILD");
            responsive.SetOrientationForTests(PrototypeOrientation.Landscape); yield return null;
            AssertRealmIdentityMark(hud, HudPresentation.SylvanRealmIdentity, HudPresentation.SylvanRealmIdentityIconResource, "SYLVAN BUILD");
            AssertSingleViewAndListener();
        }

        [UnityTest]
        public IEnumerator RealmBuild_ExplainsLiveFixedPlanWithoutInputOrLayoutArtifacts()
        {
            var previous = PlayerPrefs.GetString(DefenseLayoutSave.KeyForTests, null);
            var hadGuide = PlayerPrefs.HasKey(FirstPlayableMinute.KeyForTests);
            var previousGuide = PlayerPrefs.GetString(FirstPlayableMinute.KeyForTests, string.Empty);
            try
            {
                FirstPlayableMinute.ResetForTests();
                DefenseLayoutSave.Save(DefenseLayout.Default());
                SceneManager.LoadScene("RealmBuild"); yield return null; yield return null;
                var hud = Object.FindFirstObjectByType<BuildHUD>(); var root = Object.FindFirstObjectByType<ResponsiveHudRoot>();
                Assert.That(hud, Is.Not.Null); Assert.That(root, Is.Not.Null);
                Assert.That(hud.SlotCopy(0), Does.Contain("OUTER GUARD\nWOLF • FAST INTERCEPT • 2 THREAT"));
                Assert.That(hud.SlotCopy(2), Does.Contain("HEART GUARD\nENT • POSSESSABLE GUARDIAN • 4 THREAT"));
                Assert.That(hud.DefensePlanText, Does.Contain("ROOT GATE: ROOT TRAP"));
                Assert.That(hud.DefensePlanText, Does.Contain("HEART GUARD: ENT [POSSESSABLE]"));
                Assert.That(hud.DefenseTradeoffText, Does.Contain("Threat: 10/10").And.Contain("PACK PRESSURE").And.Contain("30 SEC CONTROL"));
                Assert.That(GameObject.Find("Defense Plan Summary").GetComponent<Text>().raycastTarget, Is.False);
                Assert.That(hud.RealmStoresText, Does.StartWith("REALM STORES  •"));
                Assert.That(GameObject.Find("Realm Stores").GetComponent<Text>().raycastTarget, Is.False);
                Assert.That(Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
                Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
                Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None), Has.Length.EqualTo(1));

                root.SetOrientationForTests(PrototypeOrientation.Portrait); yield return null; AssertBuildPlanClear();
                root.SetOrientationForTests(PrototypeOrientation.Landscape); yield return null; AssertBuildPlanClear();

                hud.CycleSlotForTests(2);
                Assert.That(hud.SlotCopy(2), Does.Contain("HEART GUARD\nOPEN • UNASSIGNED • 0 THREAT"));
                Assert.That(hud.DefensePlanText, Does.Contain("HEART GUARD: OPEN"));
                Assert.That(hud.DefenseTradeoffText, Does.Contain("Threat: 6/10").And.Contain("TRADEOFF — COMPLETE A VALID PLAN"));
                Assert.That(hud.SaveInteractable, Is.False);
                hud.CycleSlotForTests(1);
                Assert.That(hud.SlotCopy(1), Does.Contain("MID GUARD\nENT • POSSESSABLE GUARDIAN • 4 THREAT"));
                Assert.That(hud.DefensePlanText, Does.Contain("MID GUARD: ENT [POSSESSABLE]"));
                Assert.That(hud.DefenseTradeoffText, Does.Contain("Threat: 8/10").And.Contain("KEEPER RESERVE").And.Contain("1 WOLF SACRIFICED").And.Contain("45 SEC CONTROL"));
                Assert.That(hud.SaveInteractable, Is.True);
                var stillSaved = DefenseLayoutSave.Load();
                Assert.That(stillSaved.Slots[1].Piece, Is.EqualTo(DefensePieceType.Wolf), "Live draft tradeoff copy must not persist before SAVE.");
                Assert.That(stillSaved.Slots[2].Piece, Is.EqualTo(DefensePieceType.Ent), "Live draft tradeoff copy must not mutate the saved layout.");
                root.SetOrientationForTests(PrototypeOrientation.Portrait); yield return null; AssertBuildPlanClear();
                root.SetOrientationForTests(PrototypeOrientation.Landscape); yield return null; AssertBuildPlanClear();

                GameObject.Find("SAVE & DEFEND").GetComponent<Button>().onClick.Invoke();
                yield return null; yield return null;
                Assert.That(Object.FindFirstObjectByType<DefenseManager>(), Is.Not.Null);
                AssertSingleViewAndListener();
            }
            finally
            {
                if (previous == null) PlayerPrefs.DeleteKey(DefenseLayoutSave.KeyForTests); else PlayerPrefs.SetString(DefenseLayoutSave.KeyForTests, previous);
                if (hadGuide) PlayerPrefs.SetString(FirstPlayableMinute.KeyForTests, previousGuide); else PlayerPrefs.DeleteKey(FirstPlayableMinute.KeyForTests);
                PlayerPrefs.Save(); FirstPlayableMinute.ResetBuildHandoff();
            }
        }

        [UnityTest]
        public IEnumerator DefenderTest_UsesSavedCustomLayoutAndFixedPositions()
        {
            var previous = PlayerPrefs.GetString(DefenseLayoutSave.KeyForTests, null);
            var wolfAPosition = default(Vector3); var entPosition = default(Vector3); var wolfBPosition = default(Vector3); var trapPosition = default(Vector3); var captured = false;
            UnityEngine.Events.UnityAction<Scene, LoadSceneMode> captureSpawnPositions = (scene, _) =>
            {
                if (scene.name != "DefenderTest") return;
                var wolfA = GameObject.Find("Realm Wolf A"); var ent = GameObject.Find("Guardian Ent"); var wolfB = GameObject.Find("Realm Wolf B"); var trap = GameObject.Find("Manual Root Trap");
                if (!wolfA || !ent || !wolfB || !trap || wolfA.scene != scene || ent.scene != scene || wolfB.scene != scene || trap.scene != scene) return;
                wolfAPosition = wolfA.transform.position; entPosition = ent.transform.position; wolfBPosition = wolfB.transform.position; trapPosition = trap.transform.position; captured = true;
            };
            SceneManager.sceneLoaded += captureSpawnPositions;
            try
            {
                var layout = new DefenseLayout(new[] {
                    new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Wolf),
                    new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Ent),
                    new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Wolf),
                    new DefenseSlotLayout(DefenseSlotType.Trap, DefensePieceType.Empty),
                    new DefenseSlotLayout(DefenseSlotType.Trap, DefensePieceType.RootTrap) });
                DefenseLayoutSave.Save(layout); SceneManager.LoadScene("DefenderTest");
                yield return null; yield return null;
                Assert.That(captured, Is.True, "DefenderTest sceneLoaded snapshot did not find the four authored slot roots after bootstrap.");
                AssertSlotPosition(wolfAPosition, new Vector3(-3.2f, 0, -4));
                AssertSlotPosition(entPosition, new Vector3(3.2f, 0, 2));
                AssertSlotPosition(wolfBPosition, new Vector3(0, 0, 11));
                AssertSlotPosition(trapPosition, new Vector3(6, 0, 8));
                Assert.That(Object.FindObjectsByType<CombatEntity>(FindObjectsInactive.Include, FindObjectsSortMode.None), Has.Length.EqualTo(4));
                AssertArchetype("Invading Blood Knight", PrototypeCharacterRoster.BloodKnightId);
                AssertArchetype("Realm Wolf A", PrototypeCharacterRoster.SylvanWolfId);
                AssertArchetype("Guardian Ent", PrototypeCharacterRoster.GuardianEntId);
                AssertArchetype("Realm Wolf B", PrototypeCharacterRoster.SylvanWolfId);
                AssertSingleViewAndListener();
            }
            finally
            {
                SceneManager.sceneLoaded -= captureSpawnPositions;
                if (previous == null) PlayerPrefs.DeleteKey(DefenseLayoutSave.KeyForTests); else PlayerPrefs.SetString(DefenseLayoutSave.KeyForTests, previous); PlayerPrefs.Save();
            }
        }

        [UnityTest]
        public IEnumerator GuardianEntVitality_BuildPurchasePersistsAndOnlyChangesTheNextSylvanEnt()
        {
            var previousLayout = PlayerPrefs.GetString(DefenseLayoutSave.KeyForTests, null);
            var hadProgress = PlayerPrefs.HasKey(RealmProgress.KeyForTests);
            var previousProgress = PlayerPrefs.GetString(RealmProgress.KeyForTests, string.Empty);
            var hadGuide = PlayerPrefs.HasKey(FirstPlayableMinute.KeyForTests);
            var previousGuide = PlayerPrefs.GetString(FirstPlayableMinute.KeyForTests, string.Empty);
            try
            {
                FirstPlayableMinute.ResetForTests();
                RealmProgress.ResetForTests();
                RealmProgress.Credit(new RaidResult(true, 100, 1, 0, 0, 1, true));
                DefenseLayoutSave.Save(DefenseLayout.Default());
                SceneManager.LoadScene("RealmBuild"); yield return null; yield return null;
                var hud = Object.FindFirstObjectByType<BuildHUD>(); var root = Object.FindFirstObjectByType<ResponsiveHudRoot>();
                Assert.That(hud, Is.Not.Null); Assert.That(root, Is.Not.Null);
                Assert.That(hud.GuardianEntUpgradeText, Does.Contain("— READY").And.Contain("RANK 0/3").And.Contain("CURRENT: +0% MAX HEALTH IN NEXT DEFENSE").And.Contain("NEXT CULTIVATION: +10% TOTAL").And.Contain("COST: 100 GOLD • 1 RARE MATERIAL"));
                Assert.That(hud.GuardianEntUpgradeStatus, Is.EqualTo(GuardianEntCultivationStatus.Ready));
                Assert.That(hud.GuardianEntUpgradeInteractable, Is.True);
                Assert.That(hud.GuardianEntUpgradeTint, Is.EqualTo(BuildHUD.CultivationReadyTint));
                Assert.That(GameObject.Find("CULTIVATE GUARDIAN ENT").GetComponent<UiPointerOwnership>(), Is.Not.Null);
                root.SetOrientationForTests(PrototypeOrientation.Portrait); yield return null; AssertBuildPlanClear(); AssertCultivationButtonContained();
                root.SetOrientationForTests(PrototypeOrientation.Landscape); yield return null; AssertBuildPlanClear(); AssertCultivationButtonContained();

                Assert.That(hud.PurchaseGuardianEntVitalityForTests(), Is.True);
                Assert.That(hud.RealmStoresText, Is.EqualTo("REALM STORES  •  0 GOLD  •  0 RARE MATERIALS"));
                Assert.That(hud.GuardianEntUpgradeText, Does.Contain("— MISSING").And.Contain("RANK 1/3").And.Contain("CURRENT: +10% MAX HEALTH IN NEXT DEFENSE").And.Contain("NEXT CULTIVATION: +20% TOTAL").And.Contain("NEEDS: 100 GOLD • 1 RARE MATERIAL"));
                Assert.That(hud.GuardianEntUpgradeStatus, Is.EqualTo(GuardianEntCultivationStatus.Missing));
                Assert.That(hud.GuardianEntUpgradeInteractable, Is.False);
                Assert.That(hud.GuardianEntUpgradeTint, Is.EqualTo(BuildHUD.CultivationIdleTint));
                Assert.That(hud.PurchaseGuardianEntVitalityForTests(), Is.False);
                Assert.That(hud.GuardianEntUpgradeStatus, Is.EqualTo(GuardianEntCultivationStatus.Missing));
                Assert.That(RealmProgress.Load().GuardianEntVitalityRank, Is.EqualTo(1), "A failed repeat cannot mutate cultivation progress.");
                root.SetOrientationForTests(PrototypeOrientation.Portrait); yield return null; AssertBuildPlanClear(); AssertCultivationButtonContained();
                root.SetOrientationForTests(PrototypeOrientation.Landscape); yield return null; AssertBuildPlanClear(); AssertCultivationButtonContained();

                GameObject.Find("SAVE & DEFEND").GetComponent<Button>().onClick.Invoke();
                yield return null; yield return null;
                var ent = GameObject.Find("Guardian Ent").GetComponent<CombatEntity>();
                var growth = ent.GetComponent<GuardianEntGrowthPresentation>();
                var defenderHud = Object.FindFirstObjectByType<DefenderHUD>();
                var treeBody = ent.GetComponent<CharacterVisualAssembler>().PresentationPivot.Find("Base Body");
                Assert.That(ent.Definition.VisualRecipe.BaseBodyPrefab, Is.SameAs(Resources.Load<GameObject>("Characters/GuardianEntTree01")));
                Assert.That(treeBody.Find("Tree01 Fit/Tree01 Source"), Is.Not.Null, "Real cultivated defense must use the accepted Tree01 visual.");
                Assert.That(treeBody.GetComponentsInChildren<Collider>(true), Is.Empty);
                Assert.That(ent.Health.Maximum, Is.EqualTo(374).Within(.01f));
                Assert.That(GameObject.Find("Realm Wolf A").GetComponent<CombatEntity>().Health.Maximum, Is.EqualTo(52).Within(.01f));
                Assert.That(GameObject.Find("Invading Blood Knight").GetComponent<CombatEntity>().Health.Maximum, Is.EqualTo(220).Within(.01f));
                Assert.That(growth, Is.Not.Null); Assert.That(growth.Rank, Is.EqualTo(1)); Assert.That(growth.TierCount, Is.EqualTo(1));
                Assert.That(growth.MarkerRoot.parent, Is.EqualTo(ent.GetComponent<CharacterVisualAssembler>().PresentationPivot));
                foreach (var collider in growth.MarkerRoot.GetComponentsInChildren<Collider>(true)) Assert.That(collider.enabled, Is.False);
                Assert.That(growth.MarkerRoot.GetComponentInChildren<UiPointerOwnership>(true), Is.Null);
                Assert.That(GameObject.Find("Realm Wolf A").GetComponent<GuardianEntGrowthPresentation>(), Is.Null);
                Assert.That(GameObject.Find("Invading Blood Knight").GetComponent<GuardianEntGrowthPresentation>(), Is.Null);
                Assert.That(defenderHud.GuardianEntVitalityText, Is.EqualTo("GUARDIAN ENT — RANK 1/3 • +10% MAX HEALTH"));
                Assert.That(defenderHud.GuardianEntVitalityRaycastTarget, Is.False);

                var defenseRoot = Object.FindFirstObjectByType<ResponsiveHudRoot>();
                defenseRoot.SetOrientationForTests(PrototypeOrientation.Portrait); yield return null; AssertGuardianEntStatusContained(defenderHud);
                defenseRoot.SetOrientationForTests(PrototypeOrientation.Landscape); yield return null; AssertGuardianEntStatusContained(defenderHud);
                var marker = growth.MarkerRoot; var possession = Object.FindFirstObjectByType<PossessionManager>();
                possession.Select(ent); Assert.That(possession.PossessSelected(), Is.True); Assert.That(growth.MarkerRoot, Is.SameAs(marker));
                possession.Release(); Assert.That(growth.MarkerRoot, Is.SameAs(marker)); Assert.That(growth.TierCount, Is.EqualTo(1));

                GameObject.Find("Invading Blood Knight").GetComponent<CombatEntity>().Health.TakeDamage(new DamageInfo(1000, null, Vector3.zero), 0);
                Assert.That(marker.gameObject.activeSelf, Is.False); Assert.That(defenderHud.GuardianEntVitalityText, Is.Empty);

                SceneManager.LoadScene("InfernalRealm"); yield return null; yield return null;
                Assert.That(GameObject.Find("Infernal Brute").GetComponent<CombatEntity>().Health.Maximum, Is.EqualTo(380).Within(.01f));
                Assert.That(GameObject.Find("Infernal Brute").GetComponent<GuardianEntGrowthPresentation>(), Is.Null);
            }
            finally
            {
                if (previousLayout == null) PlayerPrefs.DeleteKey(DefenseLayoutSave.KeyForTests); else PlayerPrefs.SetString(DefenseLayoutSave.KeyForTests, previousLayout);
                if (hadProgress) PlayerPrefs.SetString(RealmProgress.KeyForTests, previousProgress); else PlayerPrefs.DeleteKey(RealmProgress.KeyForTests);
                if (hadGuide) PlayerPrefs.SetString(FirstPlayableMinute.KeyForTests, previousGuide); else PlayerPrefs.DeleteKey(FirstPlayableMinute.KeyForTests);
                PlayerPrefs.Save(); FirstPlayableMinute.ResetBuildHandoff();
            }
        }

        static void AssertSlotPosition(Vector3 actual, Vector3 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(.01f));
            Assert.That(actual.z, Is.EqualTo(expected.z).Within(.01f));
        }

        [UnityTest]
        public IEnumerator SylvanRealm_BootstrapsCompletePlayableRaid()
        {
            SylvanRaidCompositionSelection.ResetForTests();
            SceneManager.LoadScene("SylvanRealm");
            yield return null;
            yield return null;

            Assert.That(Object.FindFirstObjectByType<RaidManager>(), Is.Not.Null);
            Assert.That(Object.FindObjectsByType<RealmNodeView>(FindObjectsSortMode.None), Has.Length.EqualTo(7));
            Assert.That(Object.FindObjectsByType<RootTrap>(FindObjectsInactive.Include, FindObjectsSortMode.None), Has.Length.EqualTo(1));
            Assert.That(Object.FindObjectsByType<RealmCore>(FindObjectsInactive.Include, FindObjectsSortMode.None), Has.Length.EqualTo(1));
            Assert.That(Object.FindObjectsByType<CombatEntity>(FindObjectsInactive.Include, FindObjectsSortMode.None), Has.Length.EqualTo(4));
            AssertArchetype("Blood Knight", PrototypeCharacterRoster.BloodKnightId);
            AssertArchetype("Wolf Alpha", PrototypeCharacterRoster.SylvanWolfId);
            AssertArchetype("Wolf Scout", PrototypeCharacterRoster.SylvanWolfId);
            AssertArchetype("Sylvan Ent", PrototypeCharacterRoster.GuardianEntId);
            AssertSylvanRaidRoutes();
            AssertLandmarkPresentation("Heart Tree", 8, "Wide Crown", "Radial Root Left");
            AssertLandmarkPresentation("Root Trap", 6, "Inward Root 1", "Inward Root 4");
            var raidHud = Object.FindFirstObjectByType<RaidHUD>();
            var raidResponsive = raidHud.GetComponent<ResponsiveHudRoot>();
            raidResponsive.SetOrientationForTests(PrototypeOrientation.Portrait); yield return null;
            AssertRealmIdentityMark(raidHud, HudPresentation.SylvanRealmIdentity, HudPresentation.SylvanRealmIdentityIconResource, "SYLVAN RAID");
            raidResponsive.SetOrientationForTests(PrototypeOrientation.Landscape); yield return null;
            AssertRealmIdentityMark(raidHud, HudPresentation.SylvanRealmIdentity, HudPresentation.SylvanRealmIdentityIconResource, "SYLVAN RAID");
            AssertSingleViewAndListener();
            AssertCombatHudBinding();
        }

        [UnityTest]
        public IEnumerator SylvanRaidVariants_MaterializeExactAuthoredFactsAndTruthfulNodeContents()
        {
            var variants = new[]
            {
                new VariantExpectation("realmraiders.sylvan-raid.baseline", "Baseline",
                    new SpawnExpectation("Wolf Alpha", PrototypeCharacterRoster.SylvanWolfId, "Wolf Grove", -13, -11, .75f),
                    new SpawnExpectation("Wolf Scout", PrototypeCharacterRoster.SylvanWolfId, "Wolf Grove", -15.5f, -8.3f, .68f),
                    new SpawnExpectation("Sylvan Ent", PrototypeCharacterRoster.GuardianEntId, "Ent Grove", 14, 4, 1.45f)),
                new VariantExpectation("realmraiders.sylvan-raid.wolf-pressure", "Wolf Pressure",
                    new SpawnExpectation("Wolf Alpha", PrototypeCharacterRoster.SylvanWolfId, "Wolf Grove", -13, -11, .75f),
                    new SpawnExpectation("Wolf Hunter", PrototypeCharacterRoster.SylvanWolfId, "Wolf Grove", -16, -8.5f, .68f),
                    new SpawnExpectation("Sylvan Ent", PrototypeCharacterRoster.GuardianEntId, "Ent Grove", 14, 4, 1.45f),
                    new SpawnExpectation("Moonwell Wolf", PrototypeCharacterRoster.SylvanWolfId, "Moonwell", 8.2f, 28.5f, .68f)),
                new VariantExpectation("realmraiders.sylvan-raid.sentinel-escort", "Sentinel Escort",
                    new SpawnExpectation("Wolf Scout", PrototypeCharacterRoster.SylvanWolfId, "Wolf Grove", -13, -11, .75f),
                    new SpawnExpectation("Ent Sentinel", PrototypeCharacterRoster.GuardianEntId, "Ent Grove", 12.9f, 4, 1.45f),
                    new SpawnExpectation("Ent Grove Wolf", PrototypeCharacterRoster.SylvanWolfId, "Ent Grove", 15.8f, 5.4f, .68f))
            };
            try
            {
                foreach (var variant in variants)
                {
                    SylvanRaidCompositionSelection.Select(variant.Id);
                    SceneManager.LoadScene("SylvanRealm"); yield return null; yield return null;
                    var raid = Object.FindFirstObjectByType<RaidManager>();
                    var hero = SceneEntity("Blood Knight");
                    var sceneEntities = Object.FindObjectsByType<CombatEntity>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    Assert.That(sceneEntities, Has.Length.EqualTo(variant.Spawns.Length + 1));
                    Assert.That(Object.FindFirstObjectByType<RaidHUD>().StateText, Does.StartWith($"SYLVAN RAID • {variant.Name.ToUpperInvariant()}"));
                    foreach (var spawn in variant.Spawns)
                    {
                        var entity = SceneEntity(spawn.Name);
                        Assert.That(entity.Definition.ArchetypeId, Is.EqualTo(spawn.ArchetypeId), spawn.Name);
                        Assert.That(entity.transform.position.x, Is.EqualTo(spawn.X).Within(.001f), spawn.Name);
                        Assert.That(entity.transform.position.z, Is.EqualTo(spawn.Z).Within(.001f), spawn.Name);
                        Assert.That(entity.transform.localScale, Is.EqualTo(Vector3.one * spawn.Scale), spawn.Name);
                    }
                    Assert.That(variant.Spawns.Count(spawn => spawn.ArchetypeId == PrototypeCharacterRoster.GuardianEntId), Is.EqualTo(1));

                    var moonwellWolf = variant.Spawns.Any(spawn => spawn.Name == "Moonwell Wolf") ? SceneEntity("Moonwell Wolf") : null;
                    if (moonwellWolf) Assert.That(moonwellWolf.gameObject.activeSelf, Is.False, "Moonwell Wolf must remain hidden with its authored node.");
                    foreach (var node in new[]
                    {
                        (Id: "Wolf Grove", Position: new Vector3(-14, 0, -10)),
                        (Id: "Ent Grove", Position: new Vector3(14, 0, 4)),
                        (Id: "Moonwell", Position: new Vector3(10, 0, 27)),
                        (Id: "Root Path", Position: new Vector3(0, 0, 5))
                    })
                    {
                        hero.transform.position = new Vector3(node.Position.x, hero.transform.position.y, node.Position.z);
                        var nodeView = Object.FindObjectsByType<RealmNodeView>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                            .Single(view => view.Node.Id == node.Id);
                        nodeView.SendMessage("Update", SendMessageOptions.RequireReceiver);
                        Assert.That(raid.Encounter.NodeId, Is.EqualTo(node.Id));
                        Assert.That(raid.Encounter.RemainingHostiles, Is.EqualTo(variant.Spawns.Count(spawn => spawn.NodeId == node.Id)), node.Id);
                    }
                    if (moonwellWolf) Assert.That(moonwellWolf.gameObject.activeSelf, Is.True, "Entering Moonwell reveals its exact authored hostile.");

                    var wolf = variant.Spawns.Select(spawn => SceneEntity(spawn.Name)).First(entity => entity.Definition.ArchetypeId == PrototypeCharacterRoster.SylvanWolfId);
                    var ent = variant.Spawns.Select(spawn => SceneEntity(spawn.Name)).Single(entity => entity.Definition.ArchetypeId == PrototypeCharacterRoster.GuardianEntId);
                    wolf.Health.TakeDamage(new DamageInfo(10000, hero.gameObject, wolf.transform.position), 0);
                    Assert.That(raid.RareMaterials, Is.Zero);
                    ent.Health.TakeDamage(new DamageInfo(10000, hero.gameObject, ent.transform.position), 0);
                    Assert.That(raid.RareMaterials, Is.EqualTo(1), "Only the selected composition's stable-ID Ent is the bonus target.");
                    ent.Health.TakeDamage(new DamageInfo(10000, hero.gameObject, ent.transform.position), 0);
                    Assert.That(raid.RareMaterials, Is.EqualTo(1), "Bonus reward remains exact-once.");
                }
            }
            finally { SylvanRaidCompositionSelection.ResetForTests(); }
        }

        [UnityTest]
        public IEnumerator HubSylvanSelector_DirectRaidAndRetryPreserveVisibleChoice()
        {
            SylvanRaidCompositionSelection.ResetForTests();
            try
            {
                SceneManager.LoadScene("PrototypeHub"); yield return null; yield return null;
                var hub = Object.FindFirstObjectByType<HubHUD>();
                Assert.That(hub.SylvanRaidVariantText, Is.EqualTo("NEXT SYLVAN RAID: BASELINE — TAP TO CHANGE"));
                var selector = GameObject.Find(HubHUD.SylvanRaidVariantAction).GetComponent<Button>();
                selector.onClick.Invoke();
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("PrototypeHub"), "Changing composition cannot load a scene.");
                Assert.That(hub.SylvanRaidVariantText, Is.EqualTo("NEXT SYLVAN RAID: WOLF PRESSURE — TAP TO CHANGE"));
                selector.onClick.Invoke();
                Assert.That(hub.SylvanRaidVariantText, Is.EqualTo("NEXT SYLVAN RAID: SENTINEL ESCORT — TAP TO CHANGE"));

                GameObject.Find("RAID SYLVAN").GetComponent<Button>().onClick.Invoke(); yield return null; yield return null;
                Assert.That(Object.FindFirstObjectByType<RaidHUD>().StateText, Does.StartWith("SYLVAN RAID • SENTINEL ESCORT"));
                var hud = Object.FindFirstObjectByType<RaidHUD>();
                hud.SendMessage("ShowResult", new RaidResult(true, 0, 0, 0, 0, 1, true), SendMessageOptions.RequireReceiver);
                GameObject.Find("RAID AGAIN").GetComponent<Button>().onClick.Invoke(); yield return null; yield return null;
                Assert.That(SylvanRaidCompositionSelection.DisplayName, Is.EqualTo("Sentinel Escort"));
                Assert.That(Object.FindFirstObjectByType<RaidHUD>().StateText, Does.StartWith("SYLVAN RAID • SENTINEL ESCORT"));
            }
            finally { SylvanRaidCompositionSelection.ResetForTests(); }
        }

        [UnityTest]
        public IEnumerator SylvanRaid_CriticalHealthReusesTheResponsiveLabelAndRestoresOnExit()
        {
            GameplayInput.ResetForTests();
            SceneManager.LoadScene("SylvanRealm");
            yield return null;
            yield return null;
            try
            {
                var hero = GameObject.Find("Blood Knight").GetComponent<CombatEntity>();
                var player = hero.Controller<PlayerController>();
                var hud = Object.FindFirstObjectByType<RaidHUD>();
                var responsive = hud.GetComponent<ResponsiveHudRoot>();
                var healthRect = hud.HealthRect;
                var originalAnchorMin = healthRect.anchorMin;
                var originalAnchorMax = healthRect.anchorMax;
                var originalPivot = healthRect.pivot;
                var originalPosition = healthRect.anchoredPosition;
                var originalSize = healthRect.sizeDelta;
                var initialTextCount = hud.GetComponentsInChildren<Text>(true).Length;

                Assert.That(hud.LowHealthVisible, Is.False);
                Assert.That(hud.HealthText, Is.EqualTo($"Blood Knight  {hero.Health.Current:0}/{hero.Health.Maximum:0} HP"));
                Assert.That(hud.HealthTint, Is.EqualTo(DirectControlHealthReadability.NeutralTint));

                var reduction = 100f / (100f + Mathf.Max(0, hero.Stats.Armor));
                var desiredHealth = hero.Health.Maximum * .24f;
                var damage = (hero.Health.Current - desiredHealth) / reduction;
                Assert.That(hero.Health.TakeDamage(new DamageInfo(damage, null, hero.transform.position), hero.Stats.Armor), Is.True);
                Assert.That(hero.Health.Current, Is.LessThanOrEqualTo(hero.Health.Maximum * DirectControlHealthReadability.LowHealthFraction));
                var expected = DirectControlHealthReadability.Map("Blood Knight", hero.Health.Current, hero.Health.Maximum, true, false);
                Assert.That(hud.LowHealthVisible, Is.True);
                Assert.That(hud.HealthText, Is.EqualTo(expected.Copy));
                Assert.That(hud.HealthTint, Is.EqualTo(DirectControlHealthReadability.LowHealthTint));
                Assert.That(hud.GetComponentsInChildren<Text>(true), Has.Length.EqualTo(initialTextCount), "Low-health readability must reuse the existing raid health label.");

                responsive.SetOrientationForTests(PrototypeOrientation.Portrait);
                yield return null;
                Assert.That(hud.HealthText, Is.EqualTo(expected.Copy));
                AssertHealthRectUnchanged(healthRect, originalAnchorMin, originalAnchorMax, originalPivot, originalPosition, originalSize);
                responsive.SetOrientationForTests(PrototypeOrientation.Landscape);
                yield return null;
                Assert.That(hud.HealthText, Is.EqualTo(expected.Copy));
                AssertHealthRectUnchanged(healthRect, originalAnchorMin, originalAnchorMax, originalPivot, originalPosition, originalSize);

                hero.SetController(null);
                hud.SendMessage("Update", SendMessageOptions.RequireReceiver);
                Assert.That(hud.LowHealthVisible, Is.False, "Controller loss restores neutral raid health presentation.");
                Assert.That(hud.HealthText, Is.EqualTo($"Blood Knight  {hero.Health.Current:0}/{hero.Health.Maximum:0} HP"));
                Assert.That(hud.HealthTint, Is.EqualTo(DirectControlHealthReadability.NeutralTint));

                hero.SetController(player);
                hud.SendMessage("Update", SendMessageOptions.RequireReceiver);
                Assert.That(hud.LowHealthVisible, Is.True);
                hero.Health.TakeDamage(new DamageInfo(10000, null, hero.transform.position), 0);
                Assert.That(hero.Health.IsDead, Is.True);
                Assert.That(Object.FindFirstObjectByType<RaidManager>().State, Is.EqualTo(RaidState.Defeat));
                Assert.That(hud.LowHealthVisible, Is.False, "Death/terminal entry restores the exact neutral presentation.");
                Assert.That(hud.HealthText, Is.EqualTo($"Blood Knight  0/{hero.Health.Maximum:0} HP"));
                Assert.That(hud.HealthTint, Is.EqualTo(DirectControlHealthReadability.NeutralTint));
            }
            finally { GameplayInput.ResetForTests(); }
        }

        [UnityTest]
        public IEnumerator DefenderTest_BootstrapsLiveInvasion()
        {
            SceneManager.LoadScene("DefenderTest");
            yield return null;
            yield return null;

            Assert.That(Object.FindFirstObjectByType<DefenseManager>(), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<PossessionManager>(), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<RaidInvaderBrain>(), Is.Not.Null);
            Assert.That(Object.FindObjectsByType<RootTrap>(FindObjectsInactive.Include, FindObjectsSortMode.None), Has.Length.EqualTo(1));
            Assert.That(Object.FindObjectsByType<CombatEntity>(FindObjectsInactive.Include, FindObjectsSortMode.None), Has.Length.EqualTo(4));
            AssertDefenseRoute("Sylvan Path", RealmRoutePresentation.DefenseRendererCeiling, "Organic Lane Mass 1", "Organic Lane Mass 4");
            AssertLandmarkPresentation("Heart Tree", 8, "Wide Crown", "Radial Root Left");
            AssertLandmarkPresentation("Manual Root Trap", 6, "Inward Root 1", "Inward Root 4");
            var defenderHud = Object.FindFirstObjectByType<DefenderHUD>();
            var defenderResponsive = defenderHud.GetComponent<ResponsiveHudRoot>();
            defenderResponsive.SetOrientationForTests(PrototypeOrientation.Portrait); yield return null;
            AssertRealmIdentityMark(defenderHud, HudPresentation.SylvanRealmIdentity, HudPresentation.SylvanRealmIdentityIconResource, "KEEPER OVERVIEW");
            defenderResponsive.SetOrientationForTests(PrototypeOrientation.Landscape); yield return null;
            AssertRealmIdentityMark(defenderHud, HudPresentation.SylvanRealmIdentity, HudPresentation.SylvanRealmIdentityIconResource, "KEEPER OVERVIEW");
            AssertSingleViewAndListener();
            AssertCombatHudBinding();
        }

        [UnityTest]
        public IEnumerator InfernalRealm_BootstrapsBruteAndSharedTraps()
        {
            SceneManager.LoadScene("InfernalRealm");
            yield return null;
            yield return null;

            Assert.That(Object.FindFirstObjectByType<DefenseManager>(), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<FlameTrap>(), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<LavaGate>(), Is.Not.Null);
            Assert.That(Object.FindObjectsByType<CombatEntity>(FindObjectsInactive.Include, FindObjectsSortMode.None), Has.Length.EqualTo(4));
            AssertArchetype("Invading Blood Knight", PrototypeCharacterRoster.BloodKnightId);
            AssertArchetype("Infernal Brute", PrototypeCharacterRoster.InfernalBruteId);
            AssertArchetype("Hellhound A", PrototypeCharacterRoster.HellhoundId);
            AssertArchetype("Hellhound B", PrototypeCharacterRoster.HellhoundId);
            AssertDefenseRoute("Volcanic Floor", RealmRoutePresentation.DefenseRendererCeiling, "Basalt Causeway Plate 1", "Basalt Causeway Plate 4");
            AssertLandmarkPresentation("Infernal Heart", 6, "Heavy Core", "Claw Left");
            AssertLandmarkPresentation("Flame Trap", 6, "Chevron 1 Left", "Chevron 3 Right");
            var defenderHud = Object.FindFirstObjectByType<DefenderHUD>();
            var defenderResponsive = defenderHud.GetComponent<ResponsiveHudRoot>();
            defenderResponsive.SetOrientationForTests(PrototypeOrientation.Portrait); yield return null;
            AssertRealmIdentityMark(defenderHud, HudPresentation.InfernalRealmIdentity, HudPresentation.InfernalRealmIdentityIconResource, "KEEPER OVERVIEW");
            defenderResponsive.SetOrientationForTests(PrototypeOrientation.Landscape); yield return null;
            AssertRealmIdentityMark(defenderHud, HudPresentation.InfernalRealmIdentity, HudPresentation.InfernalRealmIdentityIconResource, "KEEPER OVERVIEW");
            AssertSingleViewAndListener();
            AssertCombatHudBinding();
        }

        [UnityTest]
        public IEnumerator PrototypeHub_BootstrapsNavigationHud()
        {
            var width = Screen.width; var height = Screen.height; var previousRealm = PrototypeSave.SelectedRealm; var previousControl = PrototypeSave.ControlStylePreference;
            try
            {
                PrototypeSave.SelectRealm(HudPresentation.SylvanRealmIdentity); PrototypeSave.SetControlStyle(InRunControlStyleSelector.Contextual);
                Screen.SetResolution(1920, 1080, false); SceneManager.LoadScene("PrototypeHub");
                yield return null;
                yield return null;
                var hub = Object.FindFirstObjectByType<HubHUD>(); Assert.That(hub, Is.Not.Null);
                SylvanRaidCompositionSelection.ResetForTests(); InfernalRaidCompositionSelection.ResetForTests(); hub.SendMessage("Refresh", SendMessageOptions.RequireReceiver);
                Assert.That(hub.SylvanRaidVariantText, Is.EqualTo("NEXT SYLVAN RAID: BASELINE — TAP TO CHANGE"));
                var variantAction = GameObject.Find(HubHUD.SylvanRaidVariantAction).GetComponent<Button>();
                foreach (var expected in new[] { "WOLF PRESSURE", "SENTINEL ESCORT", "BASELINE" })
                {
                    variantAction.onClick.Invoke();
                    Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("PrototypeHub"));
                    Assert.That(hub.SylvanRaidVariantText, Is.EqualTo($"NEXT SYLVAN RAID: {expected} — TAP TO CHANGE"));
                }
                Assert.That(hub.InfernalRaidVariantText, Does.Contain("BRUTE FINALE").And.Contain("2 HELLHOUNDS + BRUTE").And.Contain("~80 SEC"));
                var infernalVariantAction = GameObject.Find(HubHUD.InfernalRaidVariantAction).GetComponent<Button>();
                foreach (var expected in new[] { "ENTRY TRIAL", "RISK ROUTE", "BRUTE FINALE" })
                {
                    infernalVariantAction.onClick.Invoke();
                    Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("PrototypeHub"));
                    Assert.That(hub.InfernalRaidVariantText, Does.Contain(expected));
                }
                Assert.That(HubHUD.DestinationForButton("START SYLVAN JOURNEY"), Is.EqualTo("RealmBuild"));
                Assert.That(HubHUD.DestinationForButton("BUILD SYLVAN"), Is.EqualTo("RealmBuild"));
                Assert.That(HubHUD.DestinationForButton("DEFEND SYLVAN"), Is.EqualTo("DefenderTest"));
                Assert.That(HubHUD.DestinationForButton("RAID SYLVAN"), Is.EqualTo("SylvanRealm"));
                Assert.That(HubHUD.DestinationForButton("RAID INFERNAL — ENT"), Is.EqualTo("InfernalRaid"));
                Assert.That(HubHUD.DestinationForButton("DEFEND INFERNAL"), Is.EqualTo("InfernalRealm"));
                Assert.That(HubHUD.DestinationForButton("CHARACTER SANDBOX"), Is.EqualTo("CharacterSandbox"));
                foreach (var route in new[] { "START SYLVAN JOURNEY", "BUILD SYLVAN", "DEFEND SYLVAN", "RAID SYLVAN", "RAID INFERNAL — ENT", "DEFEND INFERNAL", "CHARACTER SANDBOX" }) Assert.That(GameObject.Find(route), Is.Not.Null);
                Assert.That(GameObject.Find("Label " + HubHUD.JourneyExplanation).GetComponent<Text>().text, Is.EqualTo(HubHUD.JourneyExplanation));
                Assert.That(hub.RealmStoresText, Does.StartWith("REALM STORES  •"));
                Assert.That(GameObject.Find("Realm Stores").GetComponent<Text>().raycastTarget, Is.False);
                Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
                Assert.That(Object.FindObjectsByType<GraphicRaycaster>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
                var responsive = Object.FindFirstObjectByType<ResponsiveHudRoot>();
                responsive.SetOrientationForTests(PrototypeOrientation.Landscape); yield return null;
                var sylvanMark = AssertRealmIdentityMark(hub, HudPresentation.SylvanRealmIdentity, HudPresentation.SylvanRealmIdentityIconResource, "Selected realm: Sylvan");
                var controlMarks = AssertHubControlStyleMarks();
                Assert.That(Object.FindObjectsByType<InRunControlStyleSelector>(FindObjectsInactive.Include, FindObjectsSortMode.None), Is.Empty);
                var buttons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None); AssertNoButtonOverlap(buttons);
                AssertHubInteractiveLayout(buttons, new Vector2(1920, 1080));
                AssertHubLabelsClear(buttons);
                responsive.SetOrientationForTests(PrototypeOrientation.Portrait); yield return null;
                Assert.That(AssertRealmIdentityMark(hub, HudPresentation.SylvanRealmIdentity, HudPresentation.SylvanRealmIdentityIconResource, "Selected realm: Sylvan"), Is.SameAs(sylvanMark));
                AssertHubControlStyleMarks(controlMarks);
                buttons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
                AssertNoButtonOverlap(buttons);
                AssertHubInteractiveLayout(buttons, new Vector2(1080, 1920));
                AssertHubPortraitSelectorStack();
                AssertHubLabelsClear(buttons);

                foreach (var style in new[] { InRunControlStyleSelector.Contextual, InRunControlStyleSelector.Fingertap, InRunControlStyleSelector.Joystick })
                {
                    GameObject.Find(style.ToUpperInvariant()).GetComponent<Button>().onClick.Invoke();
                    Assert.That(PrototypeSave.ControlStylePreference, Is.EqualTo(style));
                    Assert.That(SelectedSummary().text, Does.Contain("• " + style));
                    AssertHubControlStyleMarks(controlMarks);
                }

                PrototypeSave.SelectRealm(HudPresentation.InfernalRealmIdentity);
                hub.SendMessage("Refresh", SendMessageOptions.RequireReceiver);
                Assert.That(AssertRealmIdentityMark(hub, HudPresentation.InfernalRealmIdentity, HudPresentation.InfernalRealmIdentityIconResource, "Selected realm: Infernal"), Is.SameAs(sylvanMark), "Hub refresh must reuse one mark while replacing its realm sprite.");
                PrototypeSave.SelectRealm(HudPresentation.SylvanRealmIdentity);
                hub.SendMessage("Refresh", SendMessageOptions.RequireReceiver);
                Assert.That(AssertRealmIdentityMark(hub, HudPresentation.SylvanRealmIdentity, HudPresentation.SylvanRealmIdentityIconResource, "Selected realm: Sylvan"), Is.SameAs(sylvanMark));
                AssertSingleViewAndListener();
            }
            finally
            {
                PrototypeSave.SelectRealm(previousRealm);
                PrototypeSave.SetControlStyle(previousControl);
                SylvanRaidCompositionSelection.ResetForTests();
                InfernalRaidCompositionSelection.ResetForTests();
                Screen.SetResolution(width, height, false);
            }
        }

        static Dictionary<string, Image> AssertHubControlStyleMarks(Dictionary<string, Image> expected = null)
        {
            var result = new Dictionary<string, Image>();
            foreach (var pair in new[]
            {
                (InRunControlStyleSelector.Contextual, "CONTEXTUAL"),
                (InRunControlStyleSelector.Fingertap, "FINGERTAP"),
                (InRunControlStyleSelector.Joystick, "JOYSTICK")
            })
            {
                var button = GameObject.Find(pair.Item2).GetComponent<Button>();
                var marks = button.GetComponentsInChildren<Image>(true).Where(image => image.name == HudPresentation.ControlStyleIconName).ToArray();
                Assert.That(marks, Has.Length.EqualTo(1), pair.Item2);
                var mark = marks[0];
                Assert.That(mark.sprite, Is.SameAs(Resources.Load<Sprite>(HudPresentation.ControlStyleIconResourceFor(pair.Item1))), pair.Item2);
                Assert.That(mark.rectTransform.sizeDelta, Is.EqualTo(new Vector2(32, 32)), pair.Item2);
                Assert.That(mark.raycastTarget, Is.False, pair.Item2);
                Assert.That(mark.preserveAspect, Is.True, pair.Item2);
                Assert.That(mark.GetComponent<Button>(), Is.Null, pair.Item2);
                Assert.That(mark.GetComponent<UiPointerOwnership>(), Is.Null, pair.Item2);
                Assert.That(button.GetComponent<UiPointerOwnership>(), Is.Not.Null, pair.Item2);
                Assert.That(button.GetComponentInChildren<Text>(true).text, Is.EqualTo(pair.Item2), pair.Item2);
                if (expected != null) Assert.That(mark, Is.SameAs(expected[pair.Item2]), pair.Item2);
                result.Add(pair.Item2, mark);
            }
            return result;
        }

        static Text SelectedSummary() => Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Single(text => text.text.StartsWith("Selected realm:"));

        static CombatEntity SceneEntity(string name) => Object.FindObjectsByType<CombatEntity>(FindObjectsInactive.Include, FindObjectsSortMode.None).Single(entity => entity.name == name);

        readonly struct SpawnExpectation
        {
            public readonly string Name, ArchetypeId, NodeId;
            public readonly float X, Z, Scale;
            public SpawnExpectation(string name, string archetypeId, string nodeId, float x, float z, float scale)
            { Name = name; ArchetypeId = archetypeId; NodeId = nodeId; X = x; Z = z; Scale = scale; }
        }

        readonly struct VariantExpectation
        {
            public readonly string Id, Name;
            public readonly SpawnExpectation[] Spawns;
            public VariantExpectation(string id, string name, params SpawnExpectation[] spawns)
            { Id = id; Name = name; Spawns = spawns; }
        }

        static Image AssertRealmIdentityMark(Component hud, string realmIdentity, string resourcePath, string expectedCopyPrefix)
        {
            var marks = hud.GetComponentsInChildren<Image>(true).Where(image => image.name == HudPresentation.RealmIdentityIconName).ToArray();
            Assert.That(marks, Has.Length.EqualTo(1), $"{hud.name} must own exactly one realm identity mark.");
            var mark = marks[0];
            var expected = Resources.Load<Sprite>(resourcePath);
            var otherResource = realmIdentity == HudPresentation.SylvanRealmIdentity
                ? HudPresentation.InfernalRealmIdentityIconResource
                : HudPresentation.SylvanRealmIdentityIconResource;
            var other = Resources.Load<Sprite>(otherResource);
            Assert.That(expected, Is.Not.Null);
            Assert.That(other, Is.Not.Null);
            Assert.That(mark.sprite, Is.SameAs(expected));
            Assert.That(mark.sprite, Is.Not.SameAs(other), "A HUD must not show the other realm's mark.");
            Assert.That(mark.raycastTarget, Is.False);
            Assert.That(mark.preserveAspect, Is.True);
            Assert.That(mark.rectTransform.sizeDelta, Is.EqualTo(new Vector2(48, 48)));
            Assert.That(mark.GetComponent<Button>(), Is.Null);
            Assert.That(mark.GetComponent<EventTrigger>(), Is.Null);
            Assert.That(mark.GetComponent<UiPointerOwnership>(), Is.Null);
            var label = mark.GetComponentInParent<Text>();
            Assert.That(label, Is.Not.Null);
            Assert.That(label.text, Does.StartWith(expectedCopyPrefix));
            Assert.That(mark.rectTransform.anchoredPosition.x, Is.GreaterThanOrEqualTo(label.preferredWidth * .5f + 7.9f), "Realm mark must remain adjacent to, not cover, canonical text.");

            var markRect = WorldRect(mark.rectTransform);
            var hudRect = WorldRect(hud.GetComponent<RectTransform>());
            Assert.That(markRect.xMin, Is.GreaterThanOrEqualTo(hudRect.xMin - 1));
            Assert.That(markRect.yMin, Is.GreaterThanOrEqualTo(hudRect.yMin - 1));
            Assert.That(markRect.xMax, Is.LessThanOrEqualTo(hudRect.xMax + 1));
            Assert.That(markRect.yMax, Is.LessThanOrEqualTo(hudRect.yMax + 1));
            foreach (var button in hud.GetComponentsInChildren<Button>(false))
                Assert.That(markRect.Overlaps(WorldRect((RectTransform)button.transform)), Is.False, $"Realm mark overlaps control {button.name}.");
            return mark;
        }

        static Rect WorldRect(RectTransform rect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
        }

        static void AssertNoButtonOverlap(Button[] buttons)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                var a = buttons[i].GetComponent<RectTransform>(); var ac = new Vector3[4]; a.GetWorldCorners(ac);
                for (int j = i + 1; j < buttons.Length; j++)
                {
                    var b = buttons[j].GetComponent<RectTransform>(); var bc = new Vector3[4]; b.GetWorldCorners(bc);
                    var overlapX = Mathf.Min(ac[2].x, bc[2].x) - Mathf.Max(ac[0].x, bc[0].x); var overlapY = Mathf.Min(ac[2].y, bc[2].y) - Mathf.Max(ac[0].y, bc[0].y);
                    Assert.That(overlapX > 0 && overlapY > 0, Is.False, $"HUD buttons overlap: {a.name}/{b.name}");
                }
            }
        }

        static void AssertHubInteractiveLayout(Button[] buttons, Vector2 expectedReference)
        {
            var reference = Object.FindFirstObjectByType<CanvasScaler>().referenceResolution;
            Assert.That(reference, Is.EqualTo(expectedReference), "Hub must author layout against the effective orientation reference.");
            for (var index = 0; index < buttons.Length; index++)
            {
                var a = buttons[index].GetComponent<RectTransform>();
                var bounds = DesignRect(a, reference);
                Assert.That(bounds.xMin, Is.GreaterThanOrEqualTo(0), $"{a.name} leaves the Hub safe reference on the left.");
                Assert.That(bounds.yMin, Is.GreaterThanOrEqualTo(0), $"{a.name} leaves the Hub safe reference at the bottom.");
                Assert.That(bounds.xMax, Is.LessThanOrEqualTo(reference.x), $"{a.name} leaves the Hub safe reference on the right.");
                Assert.That(bounds.yMax, Is.LessThanOrEqualTo(reference.y), $"{a.name} leaves the Hub safe reference at the top.");
                for (var other = index + 1; other < buttons.Length; other++)
                    AssertNoDesignOverlap(a, buttons[other].GetComponent<RectTransform>(), reference, $"Hub authored buttons overlap: {a.name}/{buttons[other].name}");
            }
        }

        static void AssertHubPortraitSelectorStack()
        {
            var reference = new Vector2(1080, 1920);
            var sylvan = GameObject.Find(HubHUD.SylvanRaidVariantAction).GetComponent<RectTransform>();
            var infernal = GameObject.Find(HubHUD.InfernalRaidVariantAction).GetComponent<RectTransform>();
            var journey = GameObject.Find("START SYLVAN JOURNEY").GetComponent<RectTransform>();
            AssertVerticalDesignGap(sylvan, infernal, reference, 12);
            AssertVerticalDesignGap(infernal, journey, reference, 12);
        }

        static void AssertVerticalDesignGap(RectTransform upper, RectTransform lower, Vector2 reference, float expectedGap)
        {
            var gap = DesignRect(upper, reference).yMin - DesignRect(lower, reference).yMax;
            Assert.That(gap, Is.GreaterThanOrEqualTo(expectedGap), $"Hub portrait selector gap is too small: {upper.name}/{lower.name}");
        }

        static void AssertBuildPlanClear()
        {
            var buttons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
            AssertNoButtonOverlap(buttons);
            var plan = GameObject.Find("Defense Plan Summary").GetComponent<Text>().rectTransform;
            var labels = new List<RectTransform>();
            foreach (var text in Object.FindObjectsByType<Text>(FindObjectsSortMode.None)) if (!text.GetComponentInParent<Button>()) labels.Add(text.rectTransform);
            // The orientation override deliberately keeps the Test Runner's physical window
            // unchanged. Validate the authored reference-layout rectangles instead of its
            // distorted world corners, while the regular button check above covers live UI.
            var reference = Object.FindFirstObjectByType<CanvasScaler>().referenceResolution;
            for (var index = 0; index < labels.Count; index++)
            {
                var label = labels[index];
                foreach (var button in buttons) AssertNoDesignOverlap(label, button.GetComponent<RectTransform>(), reference, $"Build label/button overlap: {label.name}/{button.name}");
                for (var other = index + 1; other < labels.Count; other++) AssertNoDesignOverlap(label, labels[other], reference, $"Build labels overlap: {label.name}/{labels[other].name}");
            }
        }

        static void AssertCultivationButtonContained()
        {
            var reference = Object.FindFirstObjectByType<CanvasScaler>().referenceResolution;
            var button = GameObject.Find("CULTIVATE GUARDIAN ENT").GetComponent<RectTransform>();
            var bounds = DesignRect(button, reference);
            Assert.That(bounds.xMin, Is.GreaterThanOrEqualTo(0)); Assert.That(bounds.yMin, Is.GreaterThanOrEqualTo(0));
            Assert.That(bounds.xMax, Is.LessThanOrEqualTo(reference.x)); Assert.That(bounds.yMax, Is.LessThanOrEqualTo(reference.y));
        }

        static void AssertGuardianEntStatusContained(DefenderHUD hud)
        {
            var reference = Object.FindFirstObjectByType<CanvasScaler>().referenceResolution;
            var status = DesignRect(hud.GuardianEntVitalityRect, reference);
            Assert.That(status.xMin, Is.GreaterThanOrEqualTo(0)); Assert.That(status.yMin, Is.GreaterThanOrEqualTo(0));
            Assert.That(status.xMax, Is.LessThanOrEqualTo(reference.x)); Assert.That(status.yMax, Is.LessThanOrEqualTo(reference.y));
            Assert.That(status.Overlaps(DesignRect(hud.DefenderHealthRect, reference)), Is.False, "Guardian vitality status overlaps defender health");
            foreach (var button in Object.FindObjectsByType<Button>(FindObjectsSortMode.None))
                Assert.That(status.Overlaps(DesignRect(button.GetComponent<RectTransform>(), reference)), Is.False, $"Guardian vitality status overlaps {button.name}");
        }

        static void AssertHubLabelsClear(Button[] buttons)
        {
            var labels = new List<RectTransform>();
            foreach (var text in Object.FindObjectsByType<Text>(FindObjectsSortMode.None)) if (!text.GetComponentInParent<Button>()) labels.Add(text.rectTransform);
            for (var i = 0; i < labels.Count; i++)
            {
                var label = labels[i];
                foreach (var button in buttons) AssertNoOverlap(label, button.GetComponent<RectTransform>(), $"Hub label/button overlap: {label.name}/{button.name}");
                for (var j = i + 1; j < labels.Count; j++) AssertNoOverlap(label, labels[j], $"Hub labels overlap: {label.name}/{labels[j].name}");
            }
        }

        static void AssertNoOverlap(RectTransform a, RectTransform b, string message)
        {
            var ac = new Vector3[4]; var bc = new Vector3[4]; a.GetWorldCorners(ac); b.GetWorldCorners(bc);
            var overlapX = Mathf.Min(ac[2].x, bc[2].x) - Mathf.Max(ac[0].x, bc[0].x); var overlapY = Mathf.Min(ac[2].y, bc[2].y) - Mathf.Max(ac[0].y, bc[0].y);
            Assert.That(overlapX > 0 && overlapY > 0, Is.False, message);
        }
        static void AssertNoDesignOverlap(RectTransform a, RectTransform b, Vector2 reference, string message)
        {
            var aRect = DesignRect(a, reference); var bRect = DesignRect(b, reference);
            Assert.That(aRect.Overlaps(bRect), Is.False, message);
        }
        static Rect DesignRect(RectTransform rect, Vector2 parentSize)
        {
            Assert.That(rect.anchorMin, Is.EqualTo(rect.anchorMax), $"Expected a fixed anchor for {rect.name}");
            var pivotPoint = Vector2.Scale(rect.anchorMin, parentSize) + rect.anchoredPosition;
            var min = pivotPoint - Vector2.Scale(rect.pivot, rect.sizeDelta);
            return new Rect(min, rect.sizeDelta);
        }

        static void AssertHealthRectUnchanged(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size)
        {
            Assert.That(rect.anchorMin, Is.EqualTo(anchorMin));
            Assert.That(rect.anchorMax, Is.EqualTo(anchorMax));
            Assert.That(rect.pivot, Is.EqualTo(pivot));
            Assert.That(rect.anchoredPosition, Is.EqualTo(position));
            Assert.That(rect.sizeDelta, Is.EqualTo(size));
        }

        [UnityTest]
        public IEnumerator GameplayHudCreatesLandscapeJoystickWithoutExtraView()
        {
            var width = Screen.width; var height = Screen.height; var previousStyle = PrototypeSave.ControlStylePreference; GameplayInput.ResetForTests(); PrototypeSave.SetControlStyle("Joystick");
            SceneManager.LoadScene("CharacterSandbox"); yield return null; yield return null;
            AssertArchetype("Blood Knight", PrototypeCharacterRoster.BloodKnightId);
            AssertArchetype("Ent", PrototypeCharacterRoster.GuardianEntId);
            var root = Object.FindFirstObjectByType<ResponsiveHudRoot>(); Assert.That(root, Is.Not.Null); root.SetOrientationForTests(PrototypeOrientation.Landscape); yield return null;
            var joystick = Object.FindFirstObjectByType<VirtualJoystick>(FindObjectsInactive.Include); Assert.That(joystick, Is.Not.Null); Assert.That(joystick.gameObject.activeSelf, Is.False);
            GameplayInput.SetDirectControl(4242, true); yield return null;
            Assert.That(joystick.gameObject.activeSelf, Is.True);
            GameplayInput.SetTerminalState(true); yield return null; Assert.That(joystick.gameObject.activeSelf, Is.False);
            GameplayInput.SetTerminalState(false); GameplayInput.SetDirectControl(4242, false); yield return null; Assert.That(joystick.gameObject.activeSelf, Is.False);
            foreach (var button in Object.FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) { var corners = new Vector3[4]; button.GetComponent<RectTransform>().GetWorldCorners(corners); Assert.That(corners[0].x, Is.GreaterThanOrEqualTo(-1)); Assert.That(corners[2].x, Is.LessThanOrEqualTo(Screen.width + 1)); }
            AssertSingleViewAndListener();
            AssertCombatHudBinding();
            PrototypeSave.SetControlStyle(previousStyle); GameplayInput.ResetForTests(); Screen.SetResolution(width, height, false);
        }

        static void AssertCombatHudBinding()
        {
            var awareness = Object.FindFirstObjectByType<CombatCameraAwareness>();
            var responsive = Object.FindFirstObjectByType<ResponsiveHudRoot>();
            Assert.That(Object.FindObjectsByType<CombatCameraAwareness>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
            Assert.That(awareness, Is.Not.Null);
            Assert.That(awareness.HasHudBinding, Is.True);
            Assert.That(awareness.PresentationRoot.parent, Is.EqualTo(responsive.transform));
            Assert.That(GameObject.Find("Combat Threat Indicator"), Is.Null);
            Assert.That(awareness.GetComponentsInChildren<Canvas>(true), Is.Empty);
            Assert.That(Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
            Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
            Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
        }

        static void AssertArchetype(string instanceName, string stableId)
        {
            var namedEntities = new List<CombatEntity>();
            var displayedEntities = new List<CombatEntity>();
            foreach (var entity in Object.FindObjectsByType<CombatEntity>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (entity.gameObject.name == instanceName) namedEntities.Add(entity);
                if (entity.Definition != null && entity.Definition.DisplayName == instanceName) displayedEntities.Add(entity);
            }

            Assert.That(namedEntities, Has.Count.EqualTo(1), $"Expected exactly one CombatEntity object named {instanceName}, including inactive entities.");
            Assert.That(displayedEntities, Has.Count.EqualTo(1), $"Expected exactly one CombatEntity definition displayed as {instanceName}.");
            Assert.That(displayedEntities[0], Is.SameAs(namedEntities[0]), $"{instanceName} object and display aliases resolve to different entities.");
            Assert.That(namedEntities[0].Definition.ArchetypeId, Is.EqualTo(stableId), instanceName);
        }

        static void AssertLandmarkPresentation(string authoritativeName, int rendererCount, string firstSilhouettePart, string secondSilhouettePart)
        {
            GameObject authoritative = null;
            var authoritativeCount = 0;
            foreach (var candidate in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (candidate.name != authoritativeName) continue;
                authoritative = candidate.gameObject;
                authoritativeCount++;
            }
            Assert.That(authoritativeCount, Is.EqualTo(1), $"Expected one authoritative {authoritativeName} root.");
            var presentation = authoritative.transform.Find(RealmLandmarkPresentation.RootName);
            Assert.That(presentation, Is.Not.Null, $"{authoritativeName} has no landmark presentation root.");
            Assert.That(presentation.localPosition, Is.EqualTo(Vector3.zero));
            Assert.That(presentation.localRotation, Is.EqualTo(Quaternion.identity));
            Assert.That(presentation.GetComponentsInChildren<Renderer>(true), Has.Length.EqualTo(rendererCount));
            Assert.That(presentation.Find(firstSilhouettePart), Is.Not.Null);
            Assert.That(presentation.Find(secondSilhouettePart), Is.Not.Null);
            Assert.That(presentation.GetComponentsInChildren<Collider>(true), Is.Empty);
            Assert.That(presentation.GetComponentsInChildren<Rigidbody>(true), Is.Empty);
            Assert.That(presentation.GetComponentsInChildren<CharacterController>(true), Is.Empty);
            Assert.That(presentation.GetComponentsInChildren<Camera>(true), Is.Empty);
            Assert.That(presentation.GetComponentsInChildren<Light>(true), Is.Empty);
            Assert.That(presentation.GetComponentsInChildren<AudioListener>(true), Is.Empty);
            Assert.That(presentation.GetComponentsInChildren<Canvas>(true), Is.Empty);
            Assert.That(presentation.GetComponentsInChildren<EventSystem>(true), Is.Empty);
            Assert.That(presentation.GetComponentsInChildren<MonoBehaviour>(true), Is.Empty);

            var namedRootCount = 0;
            foreach (Transform child in authoritative.transform) if (child.name == RealmLandmarkPresentation.RootName) namedRootCount++;
            Assert.That(namedRootCount, Is.EqualTo(1));
        }

        static void AssertSylvanRaidRoutes()
        {
            var paths = new List<Transform>();
            foreach (var candidate in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (candidate.name == "Living Path") paths.Add(candidate);
            Assert.That(paths, Has.Count.EqualTo(6));

            var positions = new[] { new Vector3(0, -.06f, -40), new Vector3(-7, -.06f, -20), new Vector3(7, -.06f, -13), new Vector3(0, -.06f, -12), new Vector3(5, -.06f, 16), new Vector3(5, -.06f, 39) };
            var scales = new[] { new Vector3(7, .12f, 20), new Vector3(6, .12f, 28), new Vector3(6, .12f, 38), new Vector3(7, .12f, 36), new Vector3(7, .12f, 26), new Vector3(7, .12f, 25) };
            var yaws = new[] { 0f, -35f, 25f, 0f, 22f, -24f };
            for (var index = 0; index < positions.Length; index++)
            {
                Transform path = null;
                foreach (var candidate in paths) if (Vector3.Distance(candidate.position, positions[index]) < .01f) { path = candidate; break; }
                Assert.That(path, Is.Not.Null, $"Missing authoritative Living Path at {positions[index]}.");
                Assert.That(path.localScale, Is.EqualTo(scales[index]));
                Assert.That(Quaternion.Angle(path.rotation, Quaternion.Euler(0, yaws[index], 0)), Is.LessThan(.01f));
                AssertRoutePresentation(path, RealmRoutePresentation.SegmentRendererCeiling, false, "Organic Route Band", "Organic Route Band");
            }
        }

        static void AssertDefenseRoute(string authoritativeName, int rendererCount, string firstPart, string lastPart)
        {
            var route = GameObject.Find(authoritativeName);
            Assert.That(route, Is.Not.Null);
            Assert.That(route.transform.position, Is.EqualTo(new Vector3(0, -.25f, 0)));
            Assert.That(route.transform.localScale, Is.EqualTo(new Vector3(14, .5f, 68)));
            AssertRoutePresentation(route.transform, rendererCount, true, firstPart, lastPart);
        }

        static void AssertRoutePresentation(Transform authoritative, int rendererCount, bool authoritativeRendererEnabled, string firstPart, string lastPart)
        {
            var presentation = authoritative.Find(RealmRoutePresentation.RootName);
            Assert.That(presentation, Is.Not.Null);
            Assert.That(presentation.localPosition, Is.EqualTo(Vector3.zero));
            Assert.That(presentation.localRotation, Is.EqualTo(Quaternion.identity));
            Assert.That(presentation.GetComponentsInChildren<Renderer>(true), Has.Length.EqualTo(rendererCount));
            Assert.That(presentation.Find(firstPart), Is.Not.Null);
            Assert.That(presentation.Find(lastPart), Is.Not.Null);
            Assert.That(presentation.GetComponentsInChildren<Collider>(true), Is.Empty);
            Assert.That(presentation.GetComponentsInChildren<MonoBehaviour>(true), Is.Empty);
            Assert.That(authoritative.GetComponent<Collider>().enabled, Is.True);
            Assert.That(authoritative.GetComponent<Renderer>().enabled, Is.EqualTo(authoritativeRendererEnabled));

            var presentationCount = 0;
            foreach (Transform child in authoritative) if (child.name == RealmRoutePresentation.RootName) presentationCount++;
            Assert.That(presentationCount, Is.EqualTo(1));
        }

        static void AssertSingleViewAndListener()
        {
            Assert.That(Object.FindObjectsByType<Camera>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
            Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
        }
    }
}
