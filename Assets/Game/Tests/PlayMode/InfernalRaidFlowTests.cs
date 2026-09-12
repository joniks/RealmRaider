using System.Collections;
using System.Linq;
using NUnit.Framework;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using RealmRaiders.Core;
using RealmRaiders.Raid;
using RealmRaiders.Realm;
using RealmRaiders.Traps;
using RealmRaiders.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RealmRaiders.Tests
{
    public sealed class InfernalRaidFlowTests
    {
        [UnityTest]
        public IEnumerator BruteFinale_MaterializesDirectEntOptionalHoundsBypassAndSameSceneRetry()
        {
            var saved = new SavedProgress();
            try
            {
                GameplayInput.ResetForTests(); PrototypeJourney.Cancel(); RealmProgress.ResetForTests();
                SceneManager.LoadScene(InfernalRaidBootstrap.SceneName); yield return null; yield return null;

                var raid = Object.FindFirstObjectByType<RaidManager>();
                var hud = Object.FindFirstObjectByType<RaidHUD>();
                var ent = Entity("Guardian Ent"); var houndA = Entity("Hellhound A"); var houndB = Entity("Hellhound B"); var brute = Entity("Infernal Brute");
                var heart = GameObject.Find("Infernal Heart").GetComponent<RealmCore>();
                var flame = Object.FindFirstObjectByType<FlameTrap>();
                Assert.That(raid, Is.Not.Null); Assert.That(hud, Is.Not.Null); Assert.That(flame, Is.Not.Null);
                Assert.That(Object.FindObjectsByType<RaidManager>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
                Assert.That(Object.FindObjectsByType<CombatEntity>(FindObjectsSortMode.None), Has.Length.EqualTo(4));
                Assert.That(Object.FindObjectsByType<FlameTrap>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
                Assert.That(Object.FindObjectsByType<LavaGate>(FindObjectsSortMode.None), Is.Empty);
                Assert.That(Object.FindObjectsByType<Camera>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
                Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
                Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
                Assert.That(Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None), Has.Length.EqualTo(1));

                AssertArchetype(ent, PrototypeCharacterRoster.GuardianEntId); AssertArchetype(houndA, PrototypeCharacterRoster.HellhoundId); AssertArchetype(houndB, PrototypeCharacterRoster.HellhoundId); AssertArchetype(brute, PrototypeCharacterRoster.InfernalBruteId);
                AssertPosition(ent, 0, -30); AssertPosition(houndA, -3.6f, -13); AssertPosition(houndB, 3.6f, -7); AssertPosition(brute, 0, 16); AssertPosition(heart.transform, 0, 30);
                Assert.That(ent.transform.localScale, Is.EqualTo(Vector3.one * 1.45f));
                var player = ent.Controller<PlayerController>(); Assert.That(player, Is.Not.Null); Assert.That(player.IsActive, Is.True); Assert.That(ent.ActiveController, Is.SameAs(player));
                Assert.That(Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None).Count(item => item.IsActive), Is.EqualTo(1));
                Assert.That(ent.Abilities.Select(item => item.Definition.DisplayName), Is.EqualTo(new[] { "Smash", "Charge", "Ground Slam" }));
                Assert.That(ent.GetComponent<CharacterVisualMotion>(), Is.Not.Null, "The accepted Guardian Ent presentation remains animated.");

                Assert.That(flame.Automatic, Is.True); Assert.That(flame.TriggerRadius, Is.EqualTo(2f)); AssertPosition(flame.transform, 0, 2);
                Assert.That(flame.transform.localScale.x, Is.EqualTo(flame.TriggerRadius * 2)); Assert.That(flame.transform.localScale.z, Is.EqualTo(flame.TriggerRadius * 2));
                Assert.That(flame.GetComponentsInChildren<Collider>(true).Any(collider => collider.enabled), Is.False, "The optional hazard presentation cannot block the lane.");
                ent.transform.position = new Vector3(3.5f, ent.transform.position.y, flame.transform.position.z); Physics.SyncTransforms();
                Assert.That(flame.TargetInRange, Is.False, "The accepted right-side route must bypass the 2m flame trigger.");
                ent.transform.position = new Vector3(-3.5f, ent.transform.position.y, flame.transform.position.z); Physics.SyncTransforms();
                Assert.That(flame.TargetInRange, Is.False, "The accepted left-side route must bypass the 2m flame trigger.");

                Assert.That(heart.enabled, Is.False); Assert.That(hud.ObjectiveText, Does.Contain("DEFEAT INFERNAL BRUTE"));
                houndA.Health.TakeDamage(new DamageInfo(10000, ent.gameObject, houndA.transform.position), 0);
                Assert.That(heart.enabled, Is.False, "Hound defeat cannot unlock the Heart.");
                brute.Health.TakeDamage(new DamageInfo(10000, ent.gameObject, brute.transform.position), 0);
                Assert.That(heart.enabled, Is.True, "Only the exact Brute death unlocks the Heart.");
                Assert.That(houndB.Health.IsDead, Is.False, "The second Hound remains a bypassable optional fight.");
                hud.SendMessage("Refresh", SendMessageOptions.RequireReceiver); Assert.That(hud.ObjectiveText, Is.EqualTo("Reach the Infernal Heart"));

                Assert.That(hud.StateText, Does.StartWith("INFERNAL RAID")); Assert.That(hud.HealthText, Does.StartWith("Guardian Ent"));
                Assert.That(hud.AbilityButtonText(0), Is.EqualTo("SMASH")); Assert.That(hud.AbilityButtonText(1), Is.EqualTo("CHARGE")); Assert.That(hud.AbilityButtonText(2), Is.EqualTo("GROUND SLAM"));
                Assert.That(hud.GetComponentsInChildren<Image>(true).Count(image => image.name.StartsWith(HudPresentation.GuardianEntAbilityIconNamePrefix)), Is.EqualTo(3));
                var responsive = hud.GetComponent<ResponsiveHudRoot>(); responsive.SetOrientationForTests(PrototypeOrientation.Portrait); yield return null; AssertCombatActionsClear(hud);
                responsive.SetOrientationForTests(PrototypeOrientation.Landscape); yield return null; AssertCombatActionsClear(hud);

                raid.BeginObjective(); raid.CompleteObjective(); yield return new WaitForSeconds(1.35f);
                Assert.That(hud.ResultPanelVisible, Is.True); Assert.That(hud.ResultPrimaryActionVisible, Is.False); Assert.That(hud.ResultText, Does.Contain("The Infernal Heart fell"));
                Assert.That(RealmProgress.Load().CompletedRaids, Is.EqualTo(1));
                hud.SendMessage("ShowResult", new RaidResult(true, raid.Gold, raid.RareMaterials, raid.EnemiesDefeated, raid.RoomsDiscovered, raid.Duration, true), SendMessageOptions.RequireReceiver);
                Assert.That(RealmProgress.Load().CompletedRaids, Is.EqualTo(1), "Result refresh cannot duplicate Infernal raid rewards.");
                Button(hud, "RAID AGAIN").onClick.Invoke(); yield return null; yield return null;
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(InfernalRaidBootstrap.SceneName));
                Assert.That(Object.FindObjectsByType<RaidManager>(FindObjectsSortMode.None), Has.Length.EqualTo(1)); Assert.That(GameObject.Find("Infernal Heart").GetComponent<RealmCore>().enabled, Is.False);
                Assert.That(Object.FindObjectsByType<Camera>(FindObjectsSortMode.None), Has.Length.EqualTo(1)); Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None), Has.Length.EqualTo(1)); Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
                Assert.That(PrototypeJourney.IsActive, Is.False);
            }
            finally { GameplayInput.ResetForTests(); PrototypeJourney.Cancel(); saved.Restore(); }
        }

        [UnityTest]
        public IEnumerator HeroTerminalState_PreventsLateBruteUnlockAndHubReturnCleansScene()
        {
            var saved = new SavedProgress();
            try
            {
                GameplayInput.ResetForTests(); PrototypeJourney.Cancel(); RealmProgress.ResetForTests();
                SceneManager.LoadScene(InfernalRaidBootstrap.SceneName); yield return null; yield return null;
                var raid = Object.FindFirstObjectByType<RaidManager>(); var hud = Object.FindFirstObjectByType<RaidHUD>(); var ent = Entity("Guardian Ent"); var brute = Entity("Infernal Brute"); var heart = GameObject.Find("Infernal Heart").GetComponent<RealmCore>();
                var finished = 0; raid.Finished += _ => finished++;
                ent.Health.TakeDamage(new DamageInfo(10000, brute.gameObject, ent.transform.position), 0);
                Assert.That(raid.State, Is.EqualTo(RaidState.Defeat));
                brute.Health.TakeDamage(new DamageInfo(10000, ent.gameObject, brute.transform.position), 0);
                Assert.That(heart.enabled, Is.False, "A late Brute death cannot unlock the objective after terminal hero death.");
                Assert.That(raid.Gold, Is.Zero); Assert.That(raid.EnemiesDefeated, Is.Zero, "Post-terminal enemy death cannot be credited.");
                yield return new WaitForSeconds(1.35f);
                Assert.That(finished, Is.EqualTo(1)); Assert.That(hud.ResultPanelVisible, Is.True);
                Button(hud, "MY REALM").onClick.Invoke(); yield return null; yield return null;
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("PrototypeHub"));
                Assert.That(Object.FindObjectsByType<RaidManager>(FindObjectsSortMode.None), Is.Empty); Assert.That(PrototypeJourney.IsActive, Is.False);
            }
            finally { GameplayInput.ResetForTests(); PrototypeJourney.Cancel(); saved.Restore(); }
        }

        static CombatEntity Entity(string name)
        {
            var item = GameObject.Find(name); Assert.That(item, Is.Not.Null, name); return item.GetComponent<CombatEntity>();
        }

        static Button Button(Component root, string name)
        {
            var button = root.GetComponentsInChildren<Button>(true).Single(item => item.name == name); Assert.That(button.GetComponent<UiPointerOwnership>(), Is.Not.Null, name); return button;
        }

        static void AssertArchetype(CombatEntity entity, string expected) => Assert.That(entity.Definition.ArchetypeId, Is.EqualTo(expected));
        static void AssertPosition(CombatEntity entity, float x, float z) => AssertPosition(entity.transform, x, z);
        static void AssertPosition(Transform item, float x, float z) { Assert.That(item.position.x, Is.EqualTo(x).Within(.001f)); Assert.That(item.position.z, Is.EqualTo(z).Within(.001f)); }

        static void AssertCombatActionsClear(RaidHUD hud)
        {
            Canvas.ForceUpdateCanvases();
            var buttons = hud.GetComponentsInChildren<Button>(false).Where(button => button.transform.parent == hud.transform).ToArray();
            for (var i = 0; i < buttons.Length; i++)
            {
                var first = WorldRect((RectTransform)buttons[i].transform);
                for (var j = i + 1; j < buttons.Length; j++) Assert.That(first.Overlaps(WorldRect((RectTransform)buttons[j].transform)), Is.False, $"Infernal raid actions overlap: {buttons[i].name}/{buttons[j].name}");
            }
        }

        static Rect WorldRect(RectTransform rect)
        {
            var corners = new Vector3[4]; rect.GetWorldCorners(corners); return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
        }

        sealed class SavedProgress
        {
            readonly bool hadProgress = PlayerPrefs.HasKey(RealmProgress.KeyForTests);
            readonly string progress = PlayerPrefs.GetString(RealmProgress.KeyForTests, string.Empty);
            public void Restore() { if (hadProgress) PlayerPrefs.SetString(RealmProgress.KeyForTests, progress); else PlayerPrefs.DeleteKey(RealmProgress.KeyForTests); PlayerPrefs.Save(); }
        }
    }
}
