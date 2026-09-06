using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RealmRaiders.Characters;
using RealmRaiders.Raid;
using RealmRaiders.Realm;
using RealmRaiders.Traps;
using RealmRaiders.Possession;
using RealmRaiders.AI;
using RealmRaiders.UI;
using RealmRaiders.Core;
using RealmRaiders.Controllers;
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
        public IEnumerator DefenderTest_UsesSavedCustomLayoutAndFixedPositions()
        {
            var previous = PlayerPrefs.GetString(DefenseLayoutSave.KeyForTests, null);
            try
            {
                var layout = new DefenseLayout(new[] {
                    new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Wolf),
                    new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Ent),
                    new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Wolf),
                    new DefenseSlotLayout(DefenseSlotType.Trap, DefensePieceType.Empty),
                    new DefenseSlotLayout(DefenseSlotType.Trap, DefensePieceType.RootTrap) });
                DefenseLayoutSave.Save(layout); SceneManager.LoadScene("DefenderTest"); yield return null; yield return null;
                Assert.That(Object.FindObjectsByType<CombatEntity>(FindObjectsInactive.Include, FindObjectsSortMode.None), Has.Length.EqualTo(4));
                AssertSlotPosition(GameObject.Find("Realm Wolf A").transform.position, new Vector3(-3.2f, 0, -4));
                AssertSlotPosition(GameObject.Find("Guardian Ent").transform.position, new Vector3(3.2f, 0, 2));
                AssertSlotPosition(GameObject.Find("Realm Wolf B").transform.position, new Vector3(0, 0, 11));
                AssertSlotPosition(GameObject.Find("Manual Root Trap").transform.position, new Vector3(6, 0, 8));
                AssertSingleViewAndListener();
            }
            finally { if (previous == null) PlayerPrefs.DeleteKey(DefenseLayoutSave.KeyForTests); else PlayerPrefs.SetString(DefenseLayoutSave.KeyForTests, previous); PlayerPrefs.Save(); }
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
            AssertSingleViewAndListener();
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
            AssertSingleViewAndListener();
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
            AssertSingleViewAndListener();
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

        [UnityTest]
        public IEnumerator GameplayHudCreatesLandscapeJoystickWithoutExtraView()
        {
            var width = Screen.width; var height = Screen.height; var previousStyle = PrototypeSave.ControlStylePreference; GameplayInput.ResetForTests(); PrototypeSave.SetControlStyle("Joystick");
            SceneManager.LoadScene("CharacterSandbox"); yield return null; yield return null;
            var root = Object.FindFirstObjectByType<ResponsiveHudRoot>(); Assert.That(root, Is.Not.Null); root.SetOrientationForTests(PrototypeOrientation.Landscape); yield return null;
            var joystick = Object.FindFirstObjectByType<VirtualJoystick>(FindObjectsInactive.Include); Assert.That(joystick, Is.Not.Null); Assert.That(joystick.gameObject.activeSelf, Is.False);
            GameplayInput.SetDirectControl(4242, true); yield return null;
            Assert.That(joystick.gameObject.activeSelf, Is.True);
            GameplayInput.SetTerminalState(true); yield return null; Assert.That(joystick.gameObject.activeSelf, Is.False);
            GameplayInput.SetTerminalState(false); GameplayInput.SetDirectControl(4242, false); yield return null; Assert.That(joystick.gameObject.activeSelf, Is.False);
            foreach (var button in Object.FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) { var corners = new Vector3[4]; button.GetComponent<RectTransform>().GetWorldCorners(corners); Assert.That(corners[0].x, Is.GreaterThanOrEqualTo(-1)); Assert.That(corners[2].x, Is.LessThanOrEqualTo(Screen.width + 1)); }
            AssertSingleViewAndListener();
            PrototypeSave.SetControlStyle(previousStyle); GameplayInput.ResetForTests(); Screen.SetResolution(width, height, false);
        }

        static void AssertSingleViewAndListener()
        {
            Assert.That(Object.FindObjectsByType<Camera>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
            Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
        }
    }
}
