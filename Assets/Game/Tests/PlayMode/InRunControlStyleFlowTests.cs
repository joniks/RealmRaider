using System.Collections;
using System.Linq;
using NUnit.Framework;
using RealmRaiders.CameraSystem;
using RealmRaiders.Characters;
using RealmRaiders.Controllers;
using RealmRaiders.Core;
using RealmRaiders.Possession;
using RealmRaiders.Realm;
using RealmRaiders.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RealmRaiders.Tests
{
    public sealed class InRunControlStyleFlowTests
    {
        [UnityTest]
        public IEnumerator GameplayHudFlowsCreateOneOwnedResponsiveSelectorWhileHubAndBuildStayUnchanged()
        {
            var previous = PrototypeSave.ControlStylePreference;
            try
            {
                GameplayInput.ResetForTests();
                PrototypeSave.SetControlStyle(InRunControlStyleSelector.Contextual);
                foreach (var sceneName in new[] { "CharacterSandbox", "SylvanRealm", "DefenderTest", "InfernalRealm" })
                {
                    SceneManager.LoadScene(sceneName);
                    yield return null;
                    yield return null;
                    var selectors = Object.FindObjectsByType<InRunControlStyleSelector>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    Assert.That(selectors, Has.Length.EqualTo(1), sceneName);
                    var selector = selectors[0];
                    var root = selector.GetComponentInParent<ResponsiveHudRoot>();
                    Assert.That(root, Is.Not.Null, sceneName);
                    Assert.That(selector.transform.parent, Is.EqualTo(root.transform), sceneName);
                    Assert.That(root.GetComponentsInChildren<InRunControlStyleSelector>(true), Has.Length.EqualTo(1), sceneName);
                    Assert.That(selector.SelectorButton.GetComponent<UiPointerOwnership>(), Is.Not.Null, sceneName);
                    Assert.That(selector.SelectorButton.targetGraphic.raycastTarget, Is.True, sceneName);
                    Assert.That(selector.SelectorLabel.transform.parent, Is.EqualTo(selector.SelectorButton.transform), sceneName);
                    Assert.That(selector.SelectorLabel.raycastTarget, Is.False, sceneName);
                    AssertControlStyleIcon(selector, InRunControlStyleSelector.Contextual, true);
                    Assert.That(Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None), Has.Length.EqualTo(1), sceneName);
                    Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None), Has.Length.EqualTo(1), sceneName);
                    AssertResponsiveLiveLayout(root, selector, PrototypeOrientation.Portrait);
                    AssertResponsiveLiveLayout(root, selector, PrototypeOrientation.Landscape);
                    AssertPointerOwnership(selector);
                }

                foreach (var sceneName in new[] { "PrototypeHub", "RealmBuild" })
                {
                    SceneManager.LoadScene(sceneName);
                    yield return null;
                    yield return null;
                    Assert.That(Object.FindObjectsByType<InRunControlStyleSelector>(FindObjectsInactive.Include, FindObjectsSortMode.None), Is.Empty, sceneName);
                }
            }
            finally
            {
                PrototypeSave.SetControlStyle(previous);
                GameplayInput.SetTerminalState(false);
                GameplayInput.ResetForTests();
            }
        }

        [UnityTest]
        public IEnumerator LiveRaidSwitchResetsInputAndPreservesControlledEntityState()
        {
            var previous = PrototypeSave.ControlStylePreference;
            try
            {
                GameplayInput.ResetForTests();
                PrototypeSave.SetControlStyle(InRunControlStyleSelector.Contextual);
                SceneManager.LoadScene("SylvanRealm");
                yield return null;
                yield return null;

                var hud = Object.FindFirstObjectByType<RaidHUD>();
                var root = hud.GetComponent<ResponsiveHudRoot>();
                var selector = hud.ControlStyleSelector;
                var hero = FindEntity("Blood Knight");
                var controller = hero.Controller<PlayerController>();
                root.SetOrientationForTests(PrototypeOrientation.Portrait);
                RefreshControlPresentation(hud);
                Assert.That(selector.SavedStyle, Is.EqualTo(InRunControlStyleSelector.Contextual));
                Assert.That(selector.EffectiveStyle, Is.EqualTo(InRunControlStyleSelector.Fingertap));
                Assert.That(selector.SelectorLabel.text, Is.EqualTo(InRunControlStyleSelector.AutoCopy));
                var controlIcon = selector.SelectorButton.transform.Find(HudPresentation.ControlStyleIconName);
                AssertControlStyleIcon(selector, InRunControlStyleSelector.Contextual, true);
                Assert.That(root.JoystickVisible, Is.False);
                Assert.That(hud.JumpButtonVisible, Is.False);
                Assert.That(hud.ControlHintText, Does.Contain("DOUBLE-TAP GROUND: JUMP"));
                root.SetOrientationForTests(PrototypeOrientation.Landscape); RefreshControlPresentation(hud);
                Assert.That(hud.JumpButtonVisible, Is.True, "Contextual landscape must expose the joystick JUMP button.");
                Assert.That(hud.ControlHintText, Does.Contain("DRAG WORLD: LOOK"));
                root.SetOrientationForTests(PrototypeOrientation.Portrait); RefreshControlPresentation(hud);
                Assert.That(hud.JumpButtonVisible, Is.False);

                var sceneHandle = SceneManager.GetActiveScene().handle.GetRawData();
                var entityId = hero.GetEntityId();
                var position = hero.transform.position;
                var health = hero.Health.Current;
                var cooldownReadyAt = hero.Abilities[0].ReadyAt;
                var activeController = hero.ActiveController;
                GameplayInput.SetMovement(new Vector2(.75f, .25f));
                GameplayInput.ClaimUiPointer(1101);
                var revision = GameplayInput.InteractionRevision;

                selector.Cycle();
                RefreshControlPresentation(hud);
                Assert.That(selector.SavedStyle, Is.EqualTo(InRunControlStyleSelector.Fingertap));
                Assert.That(selector.SelectorLabel.text, Is.EqualTo(InRunControlStyleSelector.TapCopy));
                AssertControlStyleIcon(selector, InRunControlStyleSelector.Fingertap, true);
                Assert.That(selector.SelectorButton.transform.Find(HudPresentation.ControlStyleIconName), Is.SameAs(controlIcon));
                Assert.That(GameplayInput.Movement, Is.EqualTo(Vector2.zero));
                Assert.That(GameplayInput.HasUiOwnership, Is.False);
                Assert.That(GameplayInput.InteractionRevision, Is.GreaterThan(revision));
                Assert.That(root.JoystickVisible, Is.False);
                Assert.That(hud.JumpButtonVisible, Is.False);
                AssertEntityContinuity(hero, controller, activeController, entityId, position, health, cooldownReadyAt, sceneHandle);

                selector.Cycle();
                RefreshControlPresentation(hud);
                Assert.That(selector.SavedStyle, Is.EqualTo(InRunControlStyleSelector.Joystick));
                Assert.That(selector.SelectorLabel.text, Is.EqualTo(InRunControlStyleSelector.StickCopy));
                AssertControlStyleIcon(selector, InRunControlStyleSelector.Joystick, true);
                Assert.That(selector.SelectorButton.transform.Find(HudPresentation.ControlStyleIconName), Is.SameAs(controlIcon));
                Assert.That(root.JoystickVisible, Is.True);
                Assert.That(hud.JumpButtonVisible, Is.True);
                Assert.That(hud.ControlHintText, Does.Contain("DRAG WORLD: LOOK").And.Contain("JUMP: LEAP"));
                AssertEntityContinuity(hero, controller, activeController, entityId, position, health, cooldownReadyAt, sceneHandle);

                selector.Cycle();
                RefreshControlPresentation(hud);
                Assert.That(selector.SavedStyle, Is.EqualTo(InRunControlStyleSelector.Contextual));
                Assert.That(selector.EffectiveStyle, Is.EqualTo(InRunControlStyleSelector.Fingertap));
                AssertControlStyleIcon(selector, InRunControlStyleSelector.Contextual, true);
                Assert.That(root.JoystickVisible, Is.False);
                Assert.That(hud.JumpButtonVisible, Is.False);
                selector.Cycle();
                RefreshControlPresentation(hud);
                Assert.That(selector.SavedStyle, Is.EqualTo(InRunControlStyleSelector.Fingertap));
                Assert.That(root.JoystickVisible, Is.False);
                AssertEntityContinuity(hero, controller, activeController, entityId, position, health, cooldownReadyAt, sceneHandle);
            }
            finally
            {
                PrototypeSave.SetControlStyle(previous);
                GameplayInput.SetTerminalState(false);
                GameplayInput.ResetForTests();
            }
        }

        [UnityTest]
        public IEnumerator DefenderKeeperHidesStickAndPossessionSurvivesLiveSwitchAndTerminalVisibility()
        {
            var previous = PrototypeSave.ControlStylePreference;
            var previousLayout = PlayerPrefs.GetString(DefenseLayoutSave.KeyForTests, null);
            PossessionManager possession = null;
            try
            {
                GameplayInput.ResetForTests();
                PrototypeSave.SetControlStyle(InRunControlStyleSelector.Joystick);
                DefenseLayoutSave.Save(DefenseLayout.Default());
                SceneManager.LoadScene("DefenderTest");
                yield return null;
                yield return null;

                var hud = Object.FindFirstObjectByType<DefenderHUD>();
                var root = hud.GetComponent<ResponsiveHudRoot>();
                var selector = hud.ControlStyleSelector;
                possession = Object.FindFirstObjectByType<PossessionManager>();
                var defender = FindEntity("Guardian Ent");
                root.SetOrientationForTests(PrototypeOrientation.Portrait);
                Assert.That(selector.Visible, Is.True);
                Assert.That(selector.EffectiveStyle, Is.EqualTo(InRunControlStyleSelector.Joystick));
                Assert.That(root.JoystickVisible, Is.False, "Keeper view must not show the selected joystick.");
                Assert.That(hud.JumpButtonVisible, Is.False, "Keeper view must not show JUMP.");

                possession.Select(defender);
                Assert.That(possession.PossessSelected(), Is.True);
                yield return null;
                Assert.That(root.JoystickVisible, Is.True);
                Assert.That(hud.JumpButtonVisible, Is.True);
                Assert.That(hud.ControlHintText, Does.Contain("DRAG WORLD: LOOK").And.Contain("JUMP: LEAP"));

                var player = defender.Controller<PlayerController>();
                var sceneHandle = SceneManager.GetActiveScene().handle.GetRawData();
                var entityId = defender.GetEntityId();
                var position = defender.transform.position;
                var health = defender.Health.Current;
                var cooldownReadyAt = defender.Abilities[0].ReadyAt;
                var remainingEnergy = hud.PossessionEnergyRemaining;

                selector.Cycle();
                RefreshControlPresentation(hud);
                Assert.That(selector.SavedStyle, Is.EqualTo(InRunControlStyleSelector.Contextual));
                Assert.That(selector.EffectiveStyle, Is.EqualTo(InRunControlStyleSelector.Fingertap));
                Assert.That(root.JoystickVisible, Is.False);
                Assert.That(hud.JumpButtonVisible, Is.False);
                Assert.That(hud.ControlHintText, Does.Contain("DOUBLE-TAP GROUND: JUMP"));
                root.SetOrientationForTests(PrototypeOrientation.Landscape); RefreshControlPresentation(hud);
                Assert.That(hud.JumpButtonVisible, Is.True, "Contextual possessed landscape must expose JUMP.");
                root.SetOrientationForTests(PrototypeOrientation.Portrait); RefreshControlPresentation(hud);
                Assert.That(hud.JumpButtonVisible, Is.False);
                selector.Cycle();
                RefreshControlPresentation(hud);
                Assert.That(selector.SavedStyle, Is.EqualTo(InRunControlStyleSelector.Fingertap));
                Assert.That(root.JoystickVisible, Is.False);
                AssertPossessionContinuity(hud, possession, defender, player, entityId, position, health, cooldownReadyAt, remainingEnergy, sceneHandle);

                selector.Cycle();
                RefreshControlPresentation(hud);
                Assert.That(selector.SavedStyle, Is.EqualTo(InRunControlStyleSelector.Joystick));
                Assert.That(root.JoystickVisible, Is.True);
                Assert.That(hud.JumpButtonVisible, Is.True);
                AssertPossessionContinuity(hud, possession, defender, player, entityId, position, health, cooldownReadyAt, remainingEnergy, sceneHandle);

                GameplayInput.ClaimUiPointer(1102);
                GameplayInput.SetTerminalState(true);
                selector.RefreshNow();
                RefreshControlPresentation(hud);
                Assert.That(selector.Visible, Is.False);
                AssertControlStyleIcon(selector, InRunControlStyleSelector.Joystick, false);
                Assert.That(root.JoystickVisible, Is.False);
                Assert.That(hud.JumpButtonVisible, Is.False);
                Assert.That(GameplayInput.HasUiOwnership, Is.False);
                selector.Cycle();
                Assert.That(selector.SavedStyle, Is.EqualTo(InRunControlStyleSelector.Joystick), "A terminal selector cannot change the preference.");
                var terminalRevision = GameplayInput.InteractionRevision;
                selector.RefreshNow();
                Assert.That(selector.Visible, Is.False, "A terminal refresh cannot restore the selector.");
                Assert.That(GameplayInput.InteractionRevision, Is.EqualTo(terminalRevision), "A stable terminal state should not repeat its cleanup.");
                GameplayInput.SetTerminalState(false);
                selector.RefreshNow();
                RefreshControlPresentation(hud);
                Assert.That(selector.Visible, Is.True);
                AssertControlStyleIcon(selector, InRunControlStyleSelector.Joystick, true);
                Assert.That(root.JoystickVisible, Is.True);
                Assert.That(hud.JumpButtonVisible, Is.True);
                AssertPossessionContinuity(hud, possession, defender, player, entityId, position, health, cooldownReadyAt, remainingEnergy, sceneHandle);
            }
            finally
            {
                GameplayInput.SetTerminalState(false);
                if (possession && possession.IsPossessing) possession.Release();
                PrototypeSave.SetControlStyle(previous);
                if (previousLayout == null) PlayerPrefs.DeleteKey(DefenseLayoutSave.KeyForTests); else PlayerPrefs.SetString(DefenseLayoutSave.KeyForTests, previousLayout);
                PlayerPrefs.Save();
                GameplayInput.ResetForTests();
            }
        }

        static CombatEntity FindEntity(string displayName)
        {
            CombatEntity match = null;
            var count = 0;
            foreach (var entity in Object.FindObjectsByType<CombatEntity>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!entity.Definition || entity.Definition.DisplayName != displayName) continue;
                match = entity;
                count++;
            }
            Assert.That(count, Is.EqualTo(1), displayName);
            return match;
        }

        static void RefreshControlPresentation(RaidHUD hud)
        {
            hud.SendMessage("RefreshJumpButton", SendMessageOptions.RequireReceiver);
            hud.SendMessage("RefreshControlHint", SendMessageOptions.RequireReceiver);
        }

        static void RefreshControlPresentation(DefenderHUD hud)
        {
            hud.SendMessage("RefreshJumpButton", SendMessageOptions.RequireReceiver);
            hud.SendMessage("RefreshControlHint", SendMessageOptions.RequireReceiver);
        }

        static void AssertEntityContinuity(CombatEntity entity, PlayerController controller, IEntityController activeController, EntityId entityId, Vector3 position, float health, float readyAt, ulong sceneHandle)
        {
            Assert.That(SceneManager.GetActiveScene().handle.GetRawData(), Is.EqualTo(sceneHandle));
            Assert.That(entity.GetEntityId(), Is.EqualTo(entityId));
            Assert.That(entity.transform.position, Is.EqualTo(position));
            Assert.That(entity.Health.Current, Is.EqualTo(health));
            Assert.That(entity.Abilities[0].ReadyAt, Is.EqualTo(readyAt));
            Assert.That(entity.ActiveController, Is.SameAs(activeController));
            Assert.That(controller.IsActive, Is.True);
        }

        static void AssertPossessionContinuity(DefenderHUD hud, PossessionManager possession, CombatEntity entity, PlayerController player, EntityId entityId, Vector3 position, float health, float readyAt, float energy, ulong sceneHandle)
        {
            AssertEntityContinuity(entity, player, player, entityId, position, health, readyAt, sceneHandle);
            Assert.That(possession.Possessed, Is.SameAs(entity));
            Assert.That(hud.PossessionEnergyRemaining, Is.EqualTo(energy));
        }

        static void AssertResponsiveLiveLayout(ResponsiveHudRoot root, InRunControlStyleSelector selector, PrototypeOrientation orientation)
        {
            root.SetOrientationForTests(orientation);
            selector.RefreshNow();
            var reference = root.GetComponent<CanvasScaler>().referenceResolution;
            var selectorBounds = DesignRect(selector.SelectorRect, reference);
            AssertContained(selectorBounds, reference, orientation.ToString());
            var expected = orientation == PrototypeOrientation.Portrait ? new Rect(40, 480, 280, 64) : new Rect(40, 430, 280, 64);
            Assert.That(selectorBounds, Is.EqualTo(expected), orientation.ToString());

            var edgeSize = CombatCameraAwareness.EdgeSizeFor(orientation);
            var edgeCenterY = reference.y * (orientation == PrototypeOrientation.Portrait ? .54f : .5f);
            var leftAwarenessBand = new Rect(0, edgeCenterY - edgeSize.y * .5f, edgeSize.x, edgeSize.y);
            Assert.That(selectorBounds.Overlaps(leftAwarenessBand), Is.False, $"Selector enters the {orientation} edge-awareness band.");
            if (root.JoystickRect) Assert.That(selectorBounds.Overlaps(DesignRect(root.JoystickRect, reference)), Is.False, $"Selector overlaps the {orientation} joystick.");
            foreach (var other in Object.FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (other == selector.SelectorButton) continue;
                Assert.That(selectorBounds.Overlaps(DesignRect((RectTransform)other.transform, reference)), Is.False, $"Selector overlaps {other.name} in {orientation}.");
            }
        }

        static void AssertPointerOwnership(InRunControlStyleSelector selector)
        {
            var ownership = selector.SelectorButton.GetComponent<UiPointerOwnership>();
            var pointer = new PointerEventData(EventSystem.current) { pointerId = 1103 };
            ownership.OnPointerDown(pointer);
            Assert.That(GameplayInput.IsUiOwned(pointer.pointerId), Is.True);
            ownership.OnCancel(pointer);
            Assert.That(GameplayInput.IsUiOwned(pointer.pointerId), Is.False);
        }

        static void AssertControlStyleIcon(InRunControlStyleSelector selector, string savedStyle, bool visibleInHierarchy)
        {
            var children = selector.SelectorButton.transform.Cast<Transform>()
                .Where(child => child.name == HudPresentation.ControlStyleIconName).ToArray();
            Assert.That(children, Has.Length.EqualTo(1), savedStyle);
            var icon = children[0].GetComponent<Image>();
            Assert.That(icon, Is.Not.Null, savedStyle);
            Assert.That(icon.sprite, Is.SameAs(Resources.Load<Sprite>(HudPresentation.ControlStyleIconResourceFor(savedStyle))), savedStyle);
            Assert.That(icon.rectTransform.sizeDelta, Is.EqualTo(new Vector2(32, 32)), savedStyle);
            Assert.That(icon.preserveAspect, Is.True, savedStyle);
            Assert.That(icon.raycastTarget, Is.False, savedStyle);
            Assert.That(icon.gameObject.activeInHierarchy, Is.EqualTo(visibleInHierarchy), savedStyle);
            Assert.That(selector.SelectorLabel.text, Is.EqualTo(InRunControlStyleSelector.CopyFor(savedStyle)), savedStyle);
            Assert.That(selector.SelectorButton.GetComponent<UiPointerOwnership>(), Is.Not.Null, savedStyle);
        }

        static Rect DesignRect(RectTransform rect, Vector2 parentSize)
        {
            Assert.That(rect.anchorMin, Is.EqualTo(rect.anchorMax), rect.name);
            var pivotPoint = Vector2.Scale(rect.anchorMin, parentSize) + rect.anchoredPosition;
            return new Rect(pivotPoint - Vector2.Scale(rect.pivot, rect.sizeDelta), rect.sizeDelta);
        }

        static void AssertContained(Rect bounds, Vector2 reference, string message)
        {
            Assert.That(bounds.xMin, Is.GreaterThan(0), message);
            Assert.That(bounds.yMin, Is.GreaterThan(0), message);
            Assert.That(bounds.xMax, Is.LessThan(reference.x), message);
            Assert.That(bounds.yMax, Is.LessThan(reference.y), message);
        }
    }
}
