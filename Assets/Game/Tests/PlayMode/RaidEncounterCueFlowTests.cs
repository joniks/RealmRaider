using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using RealmRaiders.Core;
using RealmRaiders.Raid;
using RealmRaiders.Realm;
using RealmRaiders.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RealmRaiders.Tests
{
    public sealed class RaidEncounterCueFlowTests
    {
        [UnityTest]
        public IEnumerator EmptyAndHostileNodesPublishTruthfulResponsiveCueWithExactOnceAccountingAndCleanup()
        {
            var previousControlStyle = PrototypeSave.ControlStylePreference;
            var cameraObject = new GameObject("Encounter Camera", typeof(Camera), typeof(AudioListener)); cameraObject.tag = "MainCamera";
            var heroObject = new GameObject("Encounter Hero", typeof(CharacterController), typeof(Health), typeof(CombatEntity), typeof(PlayerController));
            var firstHostileObject = new GameObject("First Explicit Hostile", typeof(CharacterController), typeof(Health), typeof(CombatEntity));
            var secondHostileObject = new GameObject("Second Explicit Hostile", typeof(CharacterController), typeof(Health), typeof(CombatEntity));
            var emptyNodeObject = new GameObject("Crossroads Node");
            var hostileNodeObject = new GameObject("Wolf Grove Node");
            var emptyFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var hostileFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var coreObject = new GameObject("Encounter Core", typeof(RealmCore));
            var managerObject = new GameObject("Encounter Raid Manager", typeof(RaidManager));
            var hudObject = new GameObject("Encounter Raid HUD", typeof(RaidHUD));
            var heroDefinition = ScriptableObject.CreateInstance<CharacterDefinition>();
            var hostileDefinition = ScriptableObject.CreateInstance<CharacterDefinition>();
            GameObject cueLabelObject = null;
            GameObject cueIconObject = null;
            var initialCanvasCount = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Length;
            var initialEventSystemCount = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length;
            var initialListenerCount = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length;
            try
            {
                GameplayInput.ResetForTests();
                PrototypeSave.SetControlStyle(InRunControlStyleSelector.Contextual);
                heroDefinition.DisplayName = "Encounter Hero";
                heroDefinition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 3 };
                hostileDefinition.DisplayName = "Encounter Hostile";
                hostileDefinition.Stats = new CombatStats { MaxHealth = 40, MoveSpeed = 0 };

                var hero = heroObject.GetComponent<CombatEntity>(); hero.Initialize(heroDefinition); hero.SetController(hero.Controller<PlayerController>()); hero.Motor.enabled = false;
                var firstHostile = firstHostileObject.GetComponent<CombatEntity>(); firstHostile.Initialize(hostileDefinition);
                var secondHostile = secondHostileObject.GetComponent<CombatEntity>(); secondHostile.Initialize(hostileDefinition);
                emptyNodeObject.transform.position = Vector3.zero;
                hostileNodeObject.transform.position = new Vector3(20, 0, 0);
                heroObject.transform.position = Vector3.zero;
                emptyFloor.transform.SetParent(emptyNodeObject.transform, false);
                hostileFloor.transform.SetParent(hostileNodeObject.transform, false);

                var emptyNode = emptyNodeObject.AddComponent<RealmNodeView>();
                emptyNode.Initialize(new RealmNode("Crossroads"), hero, emptyFloor.GetComponent<Renderer>());
                var hostileNode = hostileNodeObject.AddComponent<RealmNodeView>();
                hostileNode.Initialize(new RealmNode("Wolf Grove"), hero, hostileFloor.GetComponent<Renderer>(), firstHostileObject, secondHostileObject);
                RealmNodeVisit hostileVisit = null;
                hostileNode.EncounterEntered += visit => hostileVisit = visit;

                var core = coreObject.GetComponent<RealmCore>(); core.Initialize(hero);
                var raid = managerObject.GetComponent<RaidManager>(); raid.Initialize(hero, new[] { emptyNode, hostileNode }, new[] { firstHostile, secondHostile });
                var hud = hudObject.GetComponent<RaidHUD>(); hud.Initialize(raid, hero, core, cameraObject.GetComponent<Camera>());
                yield return null;

                var cue = hud.EncounterCue;
                cueLabelObject = cue.Rect.gameObject;
                cueIconObject = cue.IconRect.gameObject;
                var responsive = hud.GetComponent<ResponsiveHudRoot>();
                var discoveredIcon = Resources.Load<Sprite>(RaidEncounterCue.DiscoveredIconResource);
                var hostilesIcon = Resources.Load<Sprite>(RaidEncounterCue.HostilesIconResource);
                var clearedIcon = Resources.Load<Sprite>(RaidEncounterCue.ClearedIconResource);
                Assert.That(cue, Is.Not.Null);
                Assert.That(cue.transform, Is.SameAs(hudObject.transform));
                Assert.That(cue.Rect.parent, Is.SameAs(hudObject.transform));
                Assert.That(cue.IconRect.parent, Is.SameAs(hudObject.transform));
                Assert.That(hudObject.GetComponentsInChildren<Image>(true).Count(item => item.name == RaidEncounterCue.IconName), Is.EqualTo(1));
                Assert.That(cue.RaycastTarget, Is.False);
                Assert.That(cue.IconRaycastTarget, Is.False);
                Assert.That(cue.Visible, Is.True);
                Assert.That(cue.Text, Is.EqualTo("CROSSROADS DISCOVERED"));
                Assert.That(cue.IconVisible, Is.True);
                Assert.That(cue.IconSprite, Is.SameAs(discoveredIcon));
                Assert.That(raid.RoomsDiscovered, Is.EqualTo(1));
                Assert.That(raid.Gold, Is.EqualTo(5));
                Assert.That(Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None), Has.Length.EqualTo(initialCanvasCount + 1));
                Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None), Has.Length.EqualTo(initialEventSystemCount));
                Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None), Has.Length.EqualTo(initialListenerCount));

                responsive.SetOrientationForTests(PrototypeOrientation.Portrait);
                yield return null;
                AssertLayout(cue, responsive, PrototypeOrientation.Portrait, hudObject);
                yield return new WaitForSecondsRealtime(RaidEncounterCue.DiscoveryDuration + .05f);
                Assert.That(cue.Visible, Is.False, "Discovery presentation must clear on its own timeout.");
                Assert.That(cue.IconVisible, Is.False);
                Assert.That(cue.IconSprite, Is.Null);

                heroObject.transform.position = hostileNodeObject.transform.position;
                Physics.SyncTransforms();
                yield return null;
                Assert.That(hostileVisit, Is.Not.Null);
                Assert.That(hostileVisit.NodeId, Is.EqualTo("Wolf Grove"));
                Assert.That(hostileVisit.AliveHostiles, Has.Count.EqualTo(2));
                Assert.That(hostileVisit.AliveHostiles[0], Is.SameAs(firstHostile));
                Assert.That(hostileVisit.AliveHostiles[1], Is.SameAs(secondHostile));
                var readOnlyHostiles = hostileVisit.AliveHostiles as IList<CombatEntity>;
                Assert.That(readOnlyHostiles, Is.Not.Null);
                Assert.Throws<System.NotSupportedException>(() => readOnlyHostiles.Clear());
                Assert.That(cue.Text, Is.EqualTo("WOLF GROVE • 2 HOSTILES"));
                Assert.That(cue.IconSprite, Is.SameAs(hostilesIcon));
                Assert.That(raid.RoomsDiscovered, Is.EqualTo(2));
                Assert.That(raid.Gold, Is.EqualTo(10));

                firstHostile.Health.TakeDamage(new DamageInfo(1000, heroObject, firstHostileObject.transform.position), 0);
                Assert.That(cue.Text, Is.EqualTo("WOLF GROVE • 1 HOSTILE"));
                Assert.That(cue.IconSprite, Is.SameAs(hostilesIcon));
                Assert.That(raid.EnemiesDefeated, Is.EqualTo(1));
                Assert.That(raid.Gold, Is.EqualTo(25));
                firstHostile.Health.TakeDamage(new DamageInfo(1000, heroObject, firstHostileObject.transform.position), 0);
                Assert.That(raid.EnemiesDefeated, Is.EqualTo(1), "A duplicate death attempt cannot award or transition twice.");
                Assert.That(raid.Gold, Is.EqualTo(25));

                secondHostile.Health.TakeDamage(new DamageInfo(1000, heroObject, secondHostileObject.transform.position), 0);
                Assert.That(cue.Text, Is.EqualTo("WOLF GROVE • AREA CLEAR"));
                Assert.That(cue.IconSprite, Is.SameAs(clearedIcon));
                Assert.That(raid.Encounter.Phase, Is.EqualTo(RaidEncounterPhase.Cleared));
                Assert.That(raid.EnemiesDefeated, Is.EqualTo(2));
                Assert.That(raid.RoomsDiscovered, Is.EqualTo(2));
                Assert.That(raid.Gold, Is.EqualTo(40));

                responsive.SetOrientationForTests(PrototypeOrientation.Landscape);
                yield return null;
                AssertLayout(cue, responsive, PrototypeOrientation.Landscape, hudObject);
                heroObject.transform.position += Vector3.right * 10;
                yield return null;
                heroObject.transform.position = hostileNodeObject.transform.position;
                yield return null;
                Assert.That(raid.RoomsDiscovered, Is.EqualTo(2), "Re-entry must not credit the room or replay encounter state.");
                Assert.That(raid.Gold, Is.EqualTo(40));

                GameplayInput.SetTerminalState(true);
                yield return null;
                Assert.That(cue.Visible, Is.False, "Terminal state must yield to result presentation.");
                Assert.That(cue.IconVisible, Is.False);
                GameplayInput.SetTerminalState(false);
                cue.Show(raid.Encounter);
                Assert.That(cue.Visible, Is.True);
                hudObject.SetActive(false);
                Assert.That(cue.Visible, Is.False, "HUD disable must immediately clear presentation.");
                Assert.That(cue.IconVisible, Is.False);
                hudObject.SetActive(true);
                Assert.That(cue.Visible, Is.False);
                cue.Show(raid.Encounter);
                raid.BeginObjective();
                raid.CompleteObjective();
                Assert.That(raid.State, Is.EqualTo(RaidState.Victory));
                Assert.That(raid.Encounter.Visible, Is.False, "A terminal raid state must clear encounter lifecycle state.");
                Assert.That(cue.Visible, Is.False);
                Assert.That(raid.Gold, Is.EqualTo(140), "Encounter presentation must not alter the existing objective or room rewards.");
            }
            finally
            {
                GameplayInput.ResetForTests();
                PrototypeSave.SetControlStyle(previousControlStyle);
                foreach (var item in new[] { hudObject, managerObject, coreObject, hostileNodeObject, emptyNodeObject, secondHostileObject, firstHostileObject, heroObject, cameraObject })
                    if (item) Object.Destroy(item);
                Object.Destroy(heroDefinition); Object.Destroy(hostileDefinition);
            }
            yield return null;
            Assert.That(cueLabelObject == null, Is.True, "Scene teardown must destroy its encounter label.");
            Assert.That(cueIconObject == null, Is.True, "Scene teardown must destroy its encounter icon.");
        }

        static void AssertLayout(RaidEncounterCue cue, ResponsiveHudRoot responsive, PrototypeOrientation orientation, GameObject hud)
        {
            var expectedSize = orientation == PrototypeOrientation.Portrait ? new Vector2(680, 72) : new Vector2(540, 64);
            var expectedPosition = orientation == PrototypeOrientation.Portrait ? new Vector2(24, -285) : new Vector2(25, -290);
            var expectedIconSize = orientation == PrototypeOrientation.Portrait ? new Vector2(44, 44) : new Vector2(40, 40);
            var expectedIconPosition = orientation == PrototypeOrientation.Portrait ? new Vector2(-350, -321) : new Vector2(-270, -322);
            Assert.That(cue.Rect.sizeDelta, Is.EqualTo(expectedSize));
            Assert.That(cue.Rect.anchoredPosition, Is.EqualTo(expectedPosition));
            Assert.That(cue.IconRect.sizeDelta, Is.EqualTo(expectedIconSize));
            Assert.That(cue.IconRect.anchoredPosition, Is.EqualTo(expectedIconPosition));
            var cueBounds = WorldRect(cue.Rect);
            var iconBounds = WorldRect(cue.IconRect);
            Assert.That(cueBounds.Overlaps(iconBounds), Is.False, $"{orientation} icon overlaps authoritative copy.");
            foreach (var button in hud.GetComponentsInChildren<Button>(false))
            {
                Assert.That(cueBounds.Overlaps(WorldRect((RectTransform)button.transform)), Is.False, $"{orientation} cue overlaps {button.name}.");
                Assert.That(iconBounds.Overlaps(WorldRect((RectTransform)button.transform)), Is.False, $"{orientation} icon overlaps {button.name}.");
            }
            foreach (var text in hud.GetComponentsInChildren<Text>(false))
            {
                if (text.rectTransform == cue.Rect || text.GetComponentInParent<Button>()) continue;
                Assert.That(cueBounds.Overlaps(WorldRect(text.rectTransform)), Is.False, $"{orientation} cue overlaps {text.name}.");
                Assert.That(iconBounds.Overlaps(WorldRect(text.rectTransform)), Is.False, $"{orientation} icon overlaps {text.name}.");
            }
            if (responsive.JoystickRect)
            {
                Assert.That(cueBounds.Overlaps(WorldRect(responsive.JoystickRect)), Is.False, $"{orientation} cue overlaps joystick lane.");
                Assert.That(iconBounds.Overlaps(WorldRect(responsive.JoystickRect)), Is.False, $"{orientation} icon overlaps joystick lane.");
            }
        }

        static Rect WorldRect(RectTransform rect)
        {
            var corners = new Vector3[4]; rect.GetWorldCorners(corners);
            return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
        }
    }
}
