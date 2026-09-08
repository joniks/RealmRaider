using System.Collections;
using System.Collections.Generic;
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
            Assert.That(hud, Is.Not.Null); Assert.That(hud.SlotCount, Is.EqualTo(5)); AssertSingleViewAndListener();
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
                Assert.That(hud.SaveInteractable, Is.False);
                hud.CycleSlotForTests(1);
                Assert.That(hud.SlotCopy(1), Does.Contain("MID GUARD\nENT • POSSESSABLE GUARDIAN • 4 THREAT"));
                Assert.That(hud.DefensePlanText, Does.Contain("MID GUARD: ENT [POSSESSABLE]"));
                Assert.That(hud.SaveInteractable, Is.True);

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
                Assert.That(hud.GuardianEntUpgradeText, Does.Contain("RANK 0/3").And.Contain("CURRENT: +0% MAX HEALTH IN NEXT DEFENSE").And.Contain("NEXT CULTIVATION: +10% TOTAL").And.Contain("COST: 100 GOLD • 1 RARE MATERIAL"));
                Assert.That(hud.GuardianEntUpgradeInteractable, Is.True);
                Assert.That(GameObject.Find("CULTIVATE GUARDIAN ENT").GetComponent<UiPointerOwnership>(), Is.Not.Null);
                root.SetOrientationForTests(PrototypeOrientation.Portrait); yield return null; AssertBuildPlanClear();
                root.SetOrientationForTests(PrototypeOrientation.Landscape); yield return null; AssertBuildPlanClear();

                Assert.That(hud.PurchaseGuardianEntVitalityForTests(), Is.True);
                Assert.That(hud.RealmStoresText, Is.EqualTo("REALM STORES  •  0 GOLD  •  0 RARE MATERIALS"));
                Assert.That(hud.GuardianEntUpgradeText, Does.Contain("RANK 1/3").And.Contain("CURRENT: +10% MAX HEALTH IN NEXT DEFENSE").And.Contain("NEXT CULTIVATION: +20% TOTAL").And.Contain("NEEDS: 100 GOLD • 1 RARE MATERIAL"));
                Assert.That(hud.GuardianEntUpgradeInteractable, Is.False);
                Assert.That(hud.PurchaseGuardianEntVitalityForTests(), Is.False);

                GameObject.Find("SAVE & DEFEND").GetComponent<Button>().onClick.Invoke();
                yield return null; yield return null;
                var ent = GameObject.Find("Guardian Ent").GetComponent<CombatEntity>();
                var growth = ent.GetComponent<GuardianEntGrowthPresentation>();
                var defenderHud = Object.FindFirstObjectByType<DefenderHUD>();
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
            AssertSingleViewAndListener();
            AssertCombatHudBinding();
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
            AssertSingleViewAndListener();
            AssertCombatHudBinding();
        }

        [UnityTest]
        public IEnumerator PrototypeHub_BootstrapsNavigationHud()
        {
            var width = Screen.width; var height = Screen.height; Screen.SetResolution(1920, 1080, false); SceneManager.LoadScene("PrototypeHub");
            yield return null;
            yield return null;
            var hub = Object.FindFirstObjectByType<HubHUD>(); Assert.That(hub, Is.Not.Null);
            Assert.That(HubHUD.DestinationForButton("START SYLVAN JOURNEY"), Is.EqualTo("RealmBuild"));
            Assert.That(HubHUD.DestinationForButton("BUILD SYLVAN"), Is.EqualTo("RealmBuild"));
            Assert.That(HubHUD.DestinationForButton("DEFEND SYLVAN"), Is.EqualTo("DefenderTest"));
            Assert.That(HubHUD.DestinationForButton("RAID SYLVAN"), Is.EqualTo("SylvanRealm"));
            Assert.That(HubHUD.DestinationForButton("DEFEND INFERNAL"), Is.EqualTo("InfernalRealm"));
            Assert.That(HubHUD.DestinationForButton("CHARACTER SANDBOX"), Is.EqualTo("CharacterSandbox"));
            foreach (var route in new[] { "START SYLVAN JOURNEY", "BUILD SYLVAN", "DEFEND SYLVAN", "RAID SYLVAN", "DEFEND INFERNAL", "CHARACTER SANDBOX" }) Assert.That(GameObject.Find(route), Is.Not.Null);
            Assert.That(GameObject.Find("Label " + HubHUD.JourneyExplanation).GetComponent<Text>().text, Is.EqualTo(HubHUD.JourneyExplanation));
            Assert.That(hub.RealmStoresText, Does.StartWith("REALM STORES  •"));
            Assert.That(GameObject.Find("Realm Stores").GetComponent<Text>().raycastTarget, Is.False);
            Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
            Assert.That(Object.FindObjectsByType<GraphicRaycaster>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
            Object.FindFirstObjectByType<ResponsiveHudRoot>().SetOrientationForTests(PrototypeOrientation.Landscape); yield return null;
            var buttons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None); AssertNoButtonOverlap(buttons);
            AssertHubLabelsClear(buttons);
            Object.FindFirstObjectByType<ResponsiveHudRoot>().SetOrientationForTests(PrototypeOrientation.Portrait); yield return null;
            AssertNoButtonOverlap(Object.FindObjectsByType<Button>(FindObjectsSortMode.None));
            AssertHubLabelsClear(Object.FindObjectsByType<Button>(FindObjectsSortMode.None));
            AssertSingleViewAndListener();
            Screen.SetResolution(width, height, false);
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
