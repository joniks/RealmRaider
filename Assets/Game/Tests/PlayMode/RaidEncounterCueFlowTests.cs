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
            var listenerCountBeforeFixture = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length;
            Assert.That(listenerCountBeforeFixture, Is.LessThanOrEqualTo(1), "The encounter fixture must not inherit an invalid listener state.");
            var cameraObject = new GameObject("Encounter Camera", typeof(Camera)); cameraObject.tag = "MainCamera";
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
            var slashDefinition = ScriptableObject.CreateInstance<AbilityDefinition>();
            var rushDefinition = ScriptableObject.CreateInstance<AbilityDefinition>();
            var cleaveDefinition = ScriptableObject.CreateInstance<AbilityDefinition>();
            GameObject cueLabelObject = null;
            GameObject cueIconObject = null;
            GameObject rewardLabelObject = null;
            GameObject[] abilityIconObjects = System.Array.Empty<GameObject>();
            var initialCanvasCount = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Length;
            var initialEventSystemCount = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length;
            try
            {
                GameplayInput.ResetForTests();
                PrototypeSave.SetControlStyle(InRunControlStyleSelector.Contextual);
                Assert.That(cameraObject.GetComponent<AudioListener>(), Is.Null, "The non-spatial encounter fixture must not add a second scene listener.");
                heroDefinition.DisplayName = "Encounter Hero";
                heroDefinition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 3 };
                ConfigureAbility(slashDefinition, "Basic Slash");
                ConfigureAbility(rushDefinition, "Blood Rush");
                ConfigureAbility(cleaveDefinition, "Heavy Cleave");
                heroDefinition.Abilities = new[] { slashDefinition, rushDefinition, cleaveDefinition };
                hostileDefinition.DisplayName = "Encounter Hostile";
                hostileDefinition.Stats = new CombatStats { MaxHealth = 40, MoveSpeed = 0 };

                var hero = heroObject.GetComponent<CombatEntity>(); hero.Initialize(heroDefinition); hero.SetController(hero.Controller<PlayerController>()); hero.Motor.enabled = false;
                var firstHostile = firstHostileObject.GetComponent<CombatEntity>(); firstHostile.Initialize(hostileDefinition);
                var secondHostile = secondHostileObject.GetComponent<CombatEntity>(); secondHostile.Initialize(hostileDefinition);
                emptyNodeObject.transform.position = Vector3.zero;
                hostileNodeObject.transform.position = new Vector3(20, 0, 0);
                coreObject.transform.position = new Vector3(42, 0, 7);
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
                var raid = managerObject.GetComponent<RaidManager>(); raid.Initialize(hero, new[] { emptyNode, hostileNode }, new[] { firstHostile, secondHostile }, coreObject.transform.position);
                var rewardFacts = new List<RaidRewardFact>(); raid.Rewarded += rewardFacts.Add;
                var hud = hudObject.GetComponent<RaidHUD>(); hud.Initialize(raid, hero, core, cameraObject.GetComponent<Camera>());
                yield return null;

                var cue = hud.EncounterCue;
                var reward = hud.RewardCue;
                rewardLabelObject = reward.Rect.gameObject;
                cueLabelObject = cue.Rect.gameObject;
                cueIconObject = cue.IconRect.gameObject;
                var responsive = hud.GetComponent<ResponsiveHudRoot>();
                var discoveredIcon = Resources.Load<Sprite>(RaidEncounterCue.DiscoveredIconResource);
                var hostilesIcon = Resources.Load<Sprite>(RaidEncounterCue.HostilesIconResource);
                var clearedIcon = Resources.Load<Sprite>(RaidEncounterCue.ClearedIconResource);
                abilityIconObjects = AbilityButtons(hudObject).Select((button, index) =>
                    button.transform.Find(HudPresentation.AbilityIconNamePrefix + index).gameObject).ToArray();
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
                Assert.That(reward.Visible, Is.True);
                Assert.That(reward.RaycastTarget, Is.False);
                Assert.That(reward.Text, Is.EqualTo("+5 GOLD  •  ROOM DISCOVERED\nTOTAL 5 GOLD • 0 RARE"));
                Assert.That(reward.Current.Source, Is.EqualTo(RaidRewardSource.RoomDiscovery));
                Assert.That(reward.Current.WorldPosition, Is.EqualTo(emptyNodeObject.transform.position));
                Assert.That(reward.PresentedCount, Is.EqualTo(1)); Assert.That(reward.PendingCount, Is.Zero);
                Assert.That(rewardFacts, Has.Count.EqualTo(1));
                Assert.That(rewardFacts[0].GoldDelta, Is.EqualTo(5)); Assert.That(rewardFacts[0].TotalGold, Is.EqualTo(5));
                Assert.That(Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None), Has.Length.EqualTo(initialCanvasCount + 1));
                Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None), Has.Length.EqualTo(initialEventSystemCount));
                Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None), Has.Length.EqualTo(listenerCountBeforeFixture));

                responsive.SetOrientationForTests(PrototypeOrientation.Portrait);
                yield return null;
                AssertLayout(cue, responsive, PrototypeOrientation.Portrait, hudObject);
                AssertRewardLayout(reward, responsive, PrototypeOrientation.Portrait, hudObject);
                AssertAbilityIcons(hudObject, PrototypeOrientation.Portrait);
                yield return new WaitForSecondsRealtime(RaidEncounterCue.DiscoveryDuration + .05f);
                Assert.That(cue.Visible, Is.False, "Discovery presentation must clear on its own timeout.");
                Assert.That(cue.IconVisible, Is.False);
                Assert.That(cue.IconSprite, Is.Null);
                Assert.That(reward.Visible, Is.False);

                var abilityButtons = AbilityButtons(hudObject);
                var abilityCopy = new[] { "SLASH", "BLOOD RUSH", "CLEAVE" };
                for (var index = 0; index < abilityButtons.Length; index++)
                {
                    var readyAt = hero.Abilities[index].ReadyAt;
                    abilityButtons[index].onClick.Invoke();
                    Assert.That(hero.Abilities[index].ReadyAt, Is.GreaterThan(readyAt),
                        $"{abilityCopy[index]} must retain the existing callback for ability index {index}.");
                    yield return WaitForIdle(hero);
                    yield return null;
                    Assert.That(hud.AbilityButtonText(index), Does.StartWith(abilityCopy[index] + "  "),
                        "The icon must not replace authoritative cooldown copy.");
                    Assert.That(abilityIconObjects[index].GetComponent<Image>().sprite,
                        Is.SameAs(Resources.Load<Sprite>(HudPresentation.AbilityIconResourceFor(index, abilityCopy[index]))));
                }

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
                Assert.That(reward.Visible, Is.True);
                Assert.That(reward.Text, Is.EqualTo("+5 GOLD  •  ROOM DISCOVERED\nTOTAL 10 GOLD • 0 RARE"));
                Assert.That(reward.Current.WorldPosition, Is.EqualTo(hostileNodeObject.transform.position));

                firstHostile.Health.TakeDamage(new DamageInfo(1000, heroObject, firstHostileObject.transform.position), 0);
                Assert.That(cue.Text, Is.EqualTo("WOLF GROVE • 1 HOSTILE"));
                Assert.That(cue.IconSprite, Is.SameAs(hostilesIcon));
                Assert.That(raid.EnemiesDefeated, Is.EqualTo(1));
                Assert.That(raid.Gold, Is.EqualTo(25));
                Assert.That(rewardFacts, Has.Count.EqualTo(3));
                Assert.That(reward.PendingCount, Is.EqualTo(1), "A simultaneous real credit is queued, not overwritten.");
                Assert.That(hudObject.GetComponentsInChildren<Text>(true).Count(item => item.name == RaidRewardCue.LabelName), Is.EqualTo(1));
                yield return new WaitForSecondsRealtime(RaidRewardCue.Duration + .05f);
                Assert.That(reward.Text, Is.EqualTo("+15 GOLD  •  ENEMY DEFEATED\nTOTAL 25 GOLD • 0 RARE"));
                Assert.That(reward.Current.WorldPosition, Is.EqualTo(firstHostileObject.transform.position));
                Assert.That(reward.PresentedCount, Is.EqualTo(3));
                var presentedAfterFirstDeath = reward.PresentedCount;
                firstHostile.Health.TakeDamage(new DamageInfo(1000, heroObject, firstHostileObject.transform.position), 0);
                Assert.That(raid.EnemiesDefeated, Is.EqualTo(1), "A duplicate death attempt cannot award or transition twice.");
                Assert.That(raid.Gold, Is.EqualTo(25));
                Assert.That(reward.PresentedCount, Is.EqualTo(presentedAfterFirstDeath));
                Assert.That(reward.PendingCount, Is.Zero, "A duplicate death cannot queue duplicate feedback.");
                Assert.That(rewardFacts, Has.Count.EqualTo(3), "A duplicate callback cannot publish a second immutable receipt.");

                secondHostile.Health.TakeDamage(new DamageInfo(1000, heroObject, secondHostileObject.transform.position), 0);
                Assert.That(cue.Text, Is.EqualTo("WOLF GROVE • AREA CLEAR"));
                Assert.That(cue.IconSprite, Is.SameAs(clearedIcon));
                Assert.That(raid.Encounter.Phase, Is.EqualTo(RaidEncounterPhase.Cleared));
                Assert.That(raid.EnemiesDefeated, Is.EqualTo(2));
                Assert.That(raid.RoomsDiscovered, Is.EqualTo(2));
                Assert.That(raid.Gold, Is.EqualTo(40));
                Assert.That(rewardFacts, Has.Count.EqualTo(4));
                Assert.That(reward.PendingCount, Is.EqualTo(1));
                yield return new WaitForSecondsRealtime(RaidRewardCue.Duration + .05f);
                Assert.That(reward.Text, Is.EqualTo("+15 GOLD  •  ENEMY DEFEATED\nTOTAL 40 GOLD • 0 RARE"));
                Assert.That(reward.Current.WorldPosition, Is.EqualTo(secondHostileObject.transform.position));
                Assert.That(reward.PresentedCount, Is.EqualTo(4));

                responsive.SetOrientationForTests(PrototypeOrientation.Landscape);
                yield return null;
                AssertLayout(cue, responsive, PrototypeOrientation.Landscape, hudObject);
                AssertRewardLayout(reward, responsive, PrototypeOrientation.Landscape, hudObject);
                AssertAbilityIcons(hudObject, PrototypeOrientation.Landscape);
                heroObject.transform.position += Vector3.right * 10;
                yield return null;
                heroObject.transform.position = hostileNodeObject.transform.position;
                yield return null;
                Assert.That(raid.RoomsDiscovered, Is.EqualTo(2), "Re-entry must not credit the room or replay encounter state.");
                Assert.That(raid.Gold, Is.EqualTo(40));
                Assert.That(rewardFacts, Has.Count.EqualTo(4), "Room re-entry cannot publish another reward fact.");

                GameplayInput.SetTerminalState(true);
                yield return null;
                Assert.That(cue.Visible, Is.False, "Terminal state must yield to result presentation.");
                Assert.That(cue.IconVisible, Is.False);
                Assert.That(reward.Visible, Is.False);
                GameplayInput.SetTerminalState(false);
                cue.Show(raid.Encounter);
                Assert.That(cue.Visible, Is.True);
                reward.Enqueue(rewardFacts[2]); reward.Enqueue(rewardFacts[3]);
                Assert.That(reward.Visible, Is.True); Assert.That(reward.PendingCount, Is.EqualTo(1));
                hudObject.SetActive(false);
                Assert.That(cue.Visible, Is.False, "HUD disable must immediately clear presentation.");
                Assert.That(cue.IconVisible, Is.False);
                Assert.That(reward.Visible, Is.False); Assert.That(reward.PendingCount, Is.Zero,
                    "HUD disable must clear current and queued reward presentation.");
                hudObject.SetActive(true);
                Assert.That(cue.Visible, Is.False);
                Assert.That(reward.Visible, Is.False);
                cue.Show(raid.Encounter);
                Assert.That(reward.Visible, Is.False);
                raid.BeginObjective();
                raid.CompleteObjective();
                Assert.That(raid.State, Is.EqualTo(RaidState.Victory));
                Assert.That(raid.Encounter.Visible, Is.False, "A terminal raid state must clear encounter lifecycle state.");
                Assert.That(cue.Visible, Is.False);
                Assert.That(raid.Gold, Is.EqualTo(140), "Encounter presentation must not alter the existing objective or room rewards.");
                Assert.That(reward.Visible, Is.True, "The successful Core mutation must publish before result cleanup.");
                Assert.That(reward.Text, Is.EqualTo("+100 GOLD • +1 RARE  •  REALM CORE DEFEATED\nTOTAL 140 GOLD • 1 RARE"));
                Assert.That(reward.Current.Source, Is.EqualTo(RaidRewardSource.RealmCoreVictory));
                Assert.That(reward.Current.WorldPosition, Is.EqualTo(coreObject.transform.position));
                Assert.That(rewardFacts, Has.Count.EqualTo(5));
                Assert.That(rewardFacts.Select(fact => fact.Sequence), Is.EqualTo(new long[] { 1, 2, 3, 4, 5 }));
                Assert.That(rewardFacts[4].GoldDelta, Is.EqualTo(100)); Assert.That(rewardFacts[4].RareMaterialsDelta, Is.EqualTo(1));
                Assert.That(rewardFacts[4].TotalGold, Is.EqualTo(140)); Assert.That(rewardFacts[4].TotalRareMaterials, Is.EqualTo(1));
                yield return new WaitForSeconds(1.3f);
                Assert.That(reward.Visible, Is.False, "Raid result must clear current and pending loot presentation.");
                Assert.That(reward.PendingCount, Is.Zero);
            }
            finally
            {
                GameplayInput.ResetForTests();
                PrototypeSave.SetControlStyle(previousControlStyle);
                foreach (var item in new[] { hudObject, managerObject, coreObject, hostileNodeObject, emptyNodeObject, secondHostileObject, firstHostileObject, heroObject, cameraObject })
                    if (item) Object.Destroy(item);
                Object.Destroy(heroDefinition); Object.Destroy(hostileDefinition);
                Object.Destroy(slashDefinition); Object.Destroy(rushDefinition); Object.Destroy(cleaveDefinition);
            }
            yield return null;
            Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None), Has.Length.EqualTo(listenerCountBeforeFixture), "Encounter fixture cleanup must preserve the entering scene's listener ownership.");
            Assert.That(cueLabelObject == null, Is.True, "Scene teardown must destroy its encounter label.");
            Assert.That(cueIconObject == null, Is.True, "Scene teardown must destroy its encounter icon.");
            Assert.That(rewardLabelObject == null, Is.True, "Scene teardown must destroy its reward label and pending presentation.");
            Assert.That(abilityIconObjects.All(icon => icon == null), Is.True, "Scene teardown must destroy all ability icons.");
        }

        static void ConfigureAbility(AbilityDefinition definition, string name)
        {
            definition.DisplayName = name;
            definition.Kind = AbilityKind.Melee;
            definition.Damage = 0;
            definition.Range = 1;
            definition.Radius = .1f;
            definition.Windup = .01f;
            definition.Cooldown = .5f;
        }

        static IEnumerator WaitForIdle(CombatEntity entity)
        {
            var timeout = Time.realtimeSinceStartup + 2f;
            while (entity.ActionPhase != CombatActionPhase.Idle && Time.realtimeSinceStartup < timeout) yield return null;
            Assert.That(entity.ActionPhase, Is.EqualTo(CombatActionPhase.Idle));
        }

        static Button[] AbilityButtons(GameObject hud) => new[] { "SLASH", "BLOOD RUSH", "CLEAVE" }
            .Select(name => hud.transform.Find(name).GetComponent<Button>()).ToArray();

        static void AssertAbilityIcons(GameObject hud, PrototypeOrientation orientation)
        {
            var buttons = AbilityButtons(hud);
            var copy = new[] { "SLASH", "BLOOD RUSH", "CLEAVE" };
            Assert.That(hud.GetComponentsInChildren<Image>(true)
                .Count(image => image.name.StartsWith(HudPresentation.AbilityIconNamePrefix)), Is.EqualTo(3));
            for (var index = 0; index < buttons.Length; index++)
            {
                var buttonRect = (RectTransform)buttons[index].transform;
                var icon = buttons[index].transform.Find(HudPresentation.AbilityIconNamePrefix + index).GetComponent<Image>();
                var label = buttons[index].GetComponentInChildren<Text>();
                Assert.That(icon.transform.parent, Is.SameAs(buttons[index].transform));
                Assert.That(icon.sprite, Is.SameAs(Resources.Load<Sprite>(HudPresentation.AbilityIconResourceFor(index, copy[index]))));
                Assert.That(icon.raycastTarget, Is.False);
                Assert.That(icon.GetComponent<Button>(), Is.Null);
                Assert.That(icon.rectTransform.sizeDelta, Is.EqualTo(new Vector2(44, 44)));
                Assert.That(icon.rectTransform.anchoredPosition, Is.EqualTo(new Vector2(10, 0)));
                var iconBounds = WorldRect(icon.rectTransform);
                Assert.That(iconBounds.Overlaps(WorldRect(label.rectTransform)), Is.False,
                    $"{orientation} {copy[index]} icon overlaps its authoritative copy.");
                var buttonBounds = WorldRect(buttonRect);
                var iconCorners = new Vector3[4]; icon.rectTransform.GetWorldCorners(iconCorners);
                Assert.That(buttonBounds.Contains(iconCorners[0]) && buttonBounds.Contains(iconCorners[2]), Is.True,
                    $"{orientation} {copy[index]} icon must remain inside its existing button.");
                for (var other = 0; other < buttons.Length; other++)
                    if (other != index) Assert.That(iconBounds.Overlaps(WorldRect((RectTransform)buttons[other].transform)), Is.False,
                        $"{orientation} {copy[index]} icon overlaps {copy[other]}.");
                Assert.That(label.text, Does.StartWith(copy[index]));
                Assert.That(label.rectTransform.offsetMin.x, Is.GreaterThanOrEqualTo(58));
                var expectedAnchor = orientation == PrototypeOrientation.Portrait ? new Vector2(.5f, 0) : new Vector2(1, 0);
                Assert.That(buttonRect.anchorMin, Is.EqualTo(expectedAnchor));
                Assert.That(buttonRect.anchorMax, Is.EqualTo(expectedAnchor));
            }
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

        static void AssertRewardLayout(RaidRewardCue cue, ResponsiveHudRoot responsive, PrototypeOrientation orientation, GameObject hud)
        {
            var expectedSize = orientation == PrototypeOrientation.Portrait ? new Vector2(760, 92) : new Vector2(620, 82);
            var expectedPosition = orientation == PrototypeOrientation.Portrait ? new Vector2(0, -370) : new Vector2(-285, -370);
            Assert.That(cue.Rect.sizeDelta, Is.EqualTo(expectedSize));
            Assert.That(cue.Rect.anchoredPosition, Is.EqualTo(expectedPosition));
            var bounds = WorldRect(cue.Rect);
            var encounter = hud.GetComponent<RaidEncounterCue>();
            Assert.That(bounds.Overlaps(WorldRect(encounter.Rect)), Is.False, $"{orientation} reward cue overlaps encounter truth.");
            foreach (var button in hud.GetComponentsInChildren<Button>(false))
                Assert.That(bounds.Overlaps(WorldRect((RectTransform)button.transform)), Is.False, $"{orientation} reward cue overlaps {button.name}.");
            foreach (var text in hud.GetComponentsInChildren<Text>(false))
            {
                if (text.rectTransform == cue.Rect || text.GetComponentInParent<Button>()) continue;
                Assert.That(bounds.Overlaps(WorldRect(text.rectTransform)), Is.False,
                    $"{orientation} reward cue overlaps {text.name}.");
            }
            if (responsive.JoystickRect)
                Assert.That(bounds.Overlaps(WorldRect(responsive.JoystickRect)), Is.False,
                    $"{orientation} reward cue overlaps joystick controls.");
            var root = (RectTransform)hud.transform;
            var rootBounds = WorldRect(root);
            var corners = new Vector3[4]; cue.Rect.GetWorldCorners(corners);
            Assert.That(rootBounds.Contains(corners[0]) && rootBounds.Contains(corners[2]), Is.True, $"{orientation} reward cue outside responsive safe-area root.");
        }

        static Rect WorldRect(RectTransform rect)
        {
            var corners = new Vector3[4]; rect.GetWorldCorners(corners);
            return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
        }
    }
}
