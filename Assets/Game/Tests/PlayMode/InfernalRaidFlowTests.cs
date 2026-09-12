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
        public IEnumerator AuthoredVariants_MaterializeExactFactsUnlockTruthfullyAndRejectLateTerminalDeaths()
        {
            var saved = new SavedProgress();
            var previousTimeScale = Time.timeScale;
            var variants = new[]
            {
                new VariantExpectation("realmraiders.infernal-raid.brute-finale", "Brute Finale", 30, true,
                    new HostileExpectation("Hellhound A", PrototypeCharacterRoster.HellhoundId, -3.6f, -13, .7f),
                    new HostileExpectation("Hellhound B", PrototypeCharacterRoster.HellhoundId, 3.6f, -7, .7f),
                    new HostileExpectation("Infernal Brute", PrototypeCharacterRoster.InfernalBruteId, 0, 16, 1.45f)),
                new VariantExpectation("realmraiders.infernal-raid.entry-trial", "Entry Trial", 16, false,
                    new HostileExpectation("Hellhound A", PrototypeCharacterRoster.HellhoundId, -3.6f, -13, .7f)),
                new VariantExpectation("realmraiders.infernal-raid.risk-route", "Risk Route", 22, true,
                    new HostileExpectation("Hellhound A", PrototypeCharacterRoster.HellhoundId, -3.6f, -13, .7f),
                    new HostileExpectation("Hellhound B", PrototypeCharacterRoster.HellhoundId, 3.6f, -7, .7f))
            };
            try
            {
                foreach (var variant in variants)
                {
                    Time.timeScale = 0; GameplayInput.ResetForTests(); PrototypeJourney.Cancel(); RealmProgress.ResetForTests();
                    InfernalRaidCompositionSelection.Select(variant.Id);
                    SceneManager.LoadScene(InfernalRaidBootstrap.SceneName); yield return null; yield return null;

                    var raid = Object.FindFirstObjectByType<RaidManager>(); var hud = Object.FindFirstObjectByType<RaidHUD>();
                    var ent = Entity("Guardian Ent"); var heart = GameObject.Find("Infernal Heart").GetComponent<RealmCore>();
                    Assert.That(Object.FindObjectsByType<CombatEntity>(FindObjectsSortMode.None), Has.Length.EqualTo(variant.Hostiles.Length + 1));
                    AssertArchetype(ent, PrototypeCharacterRoster.GuardianEntId); AssertPosition(ent, 0, -30);
                    Assert.That(ent.transform.localScale, Is.EqualTo(Vector3.one * 1.45f));
                    Assert.That(ent.ActiveController, Is.SameAs(ent.Controller<PlayerController>()));
                    Assert.That(ent.Abilities.Select(item => item.Definition.DisplayName), Is.EqualTo(new[] { "Smash", "Charge", "Ground Slam" }));
                    AssertPosition(heart.transform, 0, variant.HeartZ); Assert.That(heart.enabled, Is.False);
                    var floor = GameObject.Find("Infernal Raid Floor").transform;
                    Assert.That(floor.localScale.x, Is.EqualTo(14f).Within(.001f));
                    Assert.That(floor.localScale.z, Is.EqualTo(variant.HeartZ + 38).Within(.001f));
                    Assert.That(hud.StateText, Does.StartWith($"INFERNAL RAID • {variant.DisplayName.ToUpperInvariant()}"));
                    Assert.That(hud.ObjectiveText, Is.EqualTo(variant.Id.EndsWith("brute-finale")
                        ? "DEFEAT INFERNAL BRUTE TO UNLOCK THE HEART"
                        : "DEFEAT ALL HOSTILES TO UNLOCK THE HEART"));

                    foreach (var expected in variant.Hostiles)
                    {
                        var hostile = Entity(expected.Name); AssertArchetype(hostile, expected.ArchetypeId);
                        AssertPosition(hostile, expected.X, expected.Z);
                        Assert.That(hostile.transform.localScale, Is.EqualTo(Vector3.one * expected.Scale));
                    }
                    var flames = Object.FindObjectsByType<FlameTrap>(FindObjectsSortMode.None);
                    Assert.That(flames, Has.Length.EqualTo(variant.HasFlame ? 1 : 0));
                    if (variant.HasFlame)
                    {
                        var flame = flames[0]; AssertPosition(flame.transform, 0, 2); Assert.That(flame.TriggerRadius, Is.EqualTo(2f)); Assert.That(flame.Automatic, Is.True);
                        Assert.That(flame.GetComponentsInChildren<Collider>(true).Any(collider => collider.enabled), Is.False);
                        ent.transform.position = new Vector3(3.5f, ent.transform.position.y, 2); Physics.SyncTransforms(); Assert.That(flame.TargetInRange, Is.False);
                        ent.transform.position = new Vector3(-3.5f, ent.transform.position.y, 2); Physics.SyncTransforms(); Assert.That(flame.TargetInRange, Is.False);
                    }

                    var orderedHostiles = variant.Hostiles.Select(item => Entity(item.Name)).ToArray();
                    if (variant.Id.EndsWith("brute-finale"))
                    {
                        orderedHostiles[0].Health.TakeDamage(new DamageInfo(10000, ent.gameObject, orderedHostiles[0].transform.position), 0);
                        Assert.That(heart.enabled, Is.False, "Optional Hound death cannot unlock Brute Finale.");
                        orderedHostiles.Single(item => item.Definition.ArchetypeId == PrototypeCharacterRoster.InfernalBruteId).Health.TakeDamage(
                            new DamageInfo(10000, ent.gameObject, Vector3.zero), 0);
                    }
                    else
                    {
                        for (var index = 0; index < orderedHostiles.Length; index++)
                        {
                            orderedHostiles[index].Health.TakeDamage(new DamageInfo(10000, ent.gameObject, orderedHostiles[index].transform.position), 0);
                            if (index < orderedHostiles.Length - 1) Assert.That(heart.enabled, Is.False, "All-hostiles gate unlocked early.");
                        }
                    }
                    Assert.That(heart.enabled, Is.True, variant.DisplayName);
                    Time.timeScale = previousTimeScale;
                    raid.BeginObjective(); raid.CompleteObjective(); yield return new WaitForSeconds(1.35f);
                    Assert.That(RealmProgress.Load().CompletedRaids, Is.EqualTo(1));
                    hud.SendMessage("ShowResult", new RaidResult(true, raid.Gold, raid.RareMaterials, raid.EnemiesDefeated, raid.RoomsDiscovered, raid.Duration, true), SendMessageOptions.RequireReceiver);
                    Assert.That(RealmProgress.Load().CompletedRaids, Is.EqualTo(1), $"{variant.DisplayName} result must remain exact-once.");

                    Time.timeScale = 0; RealmProgress.ResetForTests();
                    SceneManager.LoadScene(InfernalRaidBootstrap.SceneName); yield return null; yield return null;
                    raid = Object.FindFirstObjectByType<RaidManager>(); ent = Entity("Guardian Ent"); heart = GameObject.Find("Infernal Heart").GetComponent<RealmCore>();
                    ent.Health.TakeDamage(new DamageInfo(10000, null, ent.transform.position), 0);
                    foreach (var expected in variant.Hostiles)
                    {
                        var hostile = Entity(expected.Name);
                        hostile.Health.TakeDamage(new DamageInfo(10000, ent.gameObject, hostile.transform.position), 0);
                    }
                    Assert.That(raid.State, Is.EqualTo(RaidState.Defeat));
                    Assert.That(heart.enabled, Is.False, $"{variant.DisplayName} cannot unlock after terminal defeat.");
                    Assert.That(raid.EnemiesDefeated, Is.Zero, "Late terminal deaths cannot earn raid credit.");
                }
            }
            finally
            {
                Time.timeScale = previousTimeScale; InfernalRaidCompositionSelection.ResetForTests();
                GameplayInput.ResetForTests(); PrototypeJourney.Cancel(); saved.Restore();
            }
        }

        [UnityTest]
        public IEnumerator HubInfernalSelector_DirectRaidAndRetryRetainRiskRouteWithoutChangingRealmOnSelection()
        {
            var previousRealm = PrototypeSave.SelectedRealm;
            var saved = new SavedProgress();
            InfernalRaidCompositionSelection.ResetForTests();
            try
            {
                RealmProgress.ResetForTests();
                SceneManager.LoadScene("PrototypeHub"); yield return null; yield return null;
                var hub = Object.FindFirstObjectByType<HubHUD>(); var realmBeforeSelection = PrototypeSave.SelectedRealm;
                Assert.That(hub.InfernalRaidVariantText, Does.Contain("BRUTE FINALE").And.Contain("2 HELLHOUNDS + BRUTE"));
                var selector = GameObject.Find(HubHUD.InfernalRaidVariantAction).GetComponent<Button>();
                selector.onClick.Invoke();
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("PrototypeHub"));
                Assert.That(PrototypeSave.SelectedRealm, Is.EqualTo(realmBeforeSelection));
                Assert.That(hub.InfernalRaidVariantText, Does.Contain("ENTRY TRIAL").And.Contain("1 HELLHOUND"));
                selector.onClick.Invoke();
                Assert.That(hub.InfernalRaidVariantText, Does.Contain("RISK ROUTE").And.Contain("OPTIONAL FLAME BYPASS"));

                GameObject.Find("RAID INFERNAL — ENT").GetComponent<Button>().onClick.Invoke(); yield return null; yield return null;
                var hud = Object.FindFirstObjectByType<RaidHUD>();
                Assert.That(hud.StateText, Does.StartWith("INFERNAL RAID • RISK ROUTE"));
                hud.SendMessage("ShowResult", new RaidResult(true, 0, 0, 0, 0, 1, true), SendMessageOptions.RequireReceiver);
                Button(hud, "RAID AGAIN").onClick.Invoke(); yield return null; yield return null;
                Assert.That(InfernalRaidCompositionSelection.DisplayName, Is.EqualTo("Risk Route"));
                Assert.That(Object.FindFirstObjectByType<RaidHUD>().StateText, Does.StartWith("INFERNAL RAID • RISK ROUTE"));
            }
            finally
            {
                PrototypeSave.SelectRealm(previousRealm); InfernalRaidCompositionSelection.ResetForTests(); saved.Restore();
            }
        }

        [UnityTest]
        public IEnumerator BruteFinale_MaterializesDirectEntOptionalHoundsBypassAndSameSceneRetry()
        {
            var saved = new SavedProgress();
            var previousTimeScale = Time.timeScale;
            try
            {
                InfernalRaidCompositionSelection.ResetForTests();
                Time.timeScale = 0;
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
                Assert.That(ent.Abilities[2].Definition.Recovery, Is.EqualTo(.8f).Within(.0001f));
                Assert.That(ent.GetComponent<CharacterVisualMotion>(), Is.Not.Null, "The accepted Guardian Ent presentation remains animated.");
                Time.timeScale = previousTimeScale;

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
            finally { Time.timeScale = previousTimeScale; InfernalRaidCompositionSelection.ResetForTests(); GameplayInput.ResetForTests(); PrototypeJourney.Cancel(); saved.Restore(); }
        }

        [UnityTest]
        public IEnumerator HeroTerminalState_PreventsLateBruteUnlockAndHubReturnCleansScene()
        {
            var saved = new SavedProgress();
            try
            {
                InfernalRaidCompositionSelection.ResetForTests();
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
            finally { InfernalRaidCompositionSelection.ResetForTests(); GameplayInput.ResetForTests(); PrototypeJourney.Cancel(); saved.Restore(); }
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

        readonly struct HostileExpectation
        {
            public HostileExpectation(string name, string archetypeId, float x, float z, float scale)
            { Name = name; ArchetypeId = archetypeId; X = x; Z = z; Scale = scale; }
            public string Name { get; }
            public string ArchetypeId { get; }
            public float X { get; }
            public float Z { get; }
            public float Scale { get; }
        }

        readonly struct VariantExpectation
        {
            public VariantExpectation(string id, string displayName, float heartZ, bool hasFlame, params HostileExpectation[] hostiles)
            { Id = id; DisplayName = displayName; HeartZ = heartZ; HasFlame = hasFlame; Hostiles = hostiles; }
            public string Id { get; }
            public string DisplayName { get; }
            public float HeartZ { get; }
            public bool HasFlame { get; }
            public HostileExpectation[] Hostiles { get; }
        }

        sealed class SavedProgress
        {
            readonly bool hadProgress = PlayerPrefs.HasKey(RealmProgress.KeyForTests);
            readonly string progress = PlayerPrefs.GetString(RealmProgress.KeyForTests, string.Empty);
            public void Restore() { if (hadProgress) PlayerPrefs.SetString(RealmProgress.KeyForTests, progress); else PlayerPrefs.DeleteKey(RealmProgress.KeyForTests); PlayerPrefs.Save(); }
        }
    }
}
