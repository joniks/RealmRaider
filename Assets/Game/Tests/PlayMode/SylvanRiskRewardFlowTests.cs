using System.Collections;
using System.Collections.Generic;
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
    public sealed class SylvanRiskRewardFlowTests
    {
        [UnityTest]
        public IEnumerator MoonwellAndOptionalEnt_UseTruthfulExactOnceRaidFlow()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube); ground.name = "Risk Reward Ground"; ground.transform.position = new Vector3(0, -.5f, 0); ground.transform.localScale = new Vector3(30, 1, 30);
            var heroDefinition = Definition("Blood Knight", 100);
            var enemyDefinition = Definition("Sylvan Enemy", 40);
            var hero = Entity("Exact Raid Hero", Vector3.zero, heroDefinition);
            var wolf = Entity("Exact Optional Ent", Vector3.right * 6, enemyDefinition);
            var ent = Entity("Exact Optional Ent", Vector3.right * 8, enemyDefinition);
            var lateEnemy = Entity("Late Enemy", Vector3.right * 10, enemyDefinition);
            var managerObject = new GameObject("Risk Reward Raid Manager", typeof(RaidManager));
            var coreObject = new GameObject("Risk Reward Core", typeof(RealmCore)); coreObject.transform.position = Vector3.forward * 20;
            var wellObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder); wellObject.name = "Moonwell Recovery"; wellObject.transform.position = Vector3.zero; Object.Destroy(wellObject.GetComponent<Collider>());
            var hudObject = new GameObject("Risk Reward Raid HUD", typeof(RaidHUD));
            var cameraObject = new GameObject("Risk Reward Camera", typeof(Camera)); cameraObject.tag = "MainCamera";
            var initialCanvasCount = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Length;
            var initialEventSystemCount = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length;
            var initialListenerCount = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length;
            var hadProgress = PlayerPrefs.HasKey(RealmProgress.KeyForTests);
            var previousProgress = PlayerPrefs.GetString(RealmProgress.KeyForTests, string.Empty);
            try
            {
                GameplayInput.ResetForTests(); RealmProgress.ResetForTests();
                var raid = managerObject.GetComponent<RaidManager>();
                raid.Initialize(hero, System.Array.Empty<RealmNodeView>(), new[] { wolf, ent, lateEnemy }, coreObject.transform.position, ent);
                var facts = new List<RaidRewardFact>(); raid.Rewarded += facts.Add;
                var results = new List<RaidResult>(); raid.Finished += results.Add;
                var core = coreObject.GetComponent<RealmCore>(); core.Initialize(hero); core.InteractionStarted += raid.BeginObjective; core.Completed += raid.CompleteObjective;
                var wellRenderer = wellObject.GetComponent<Renderer>(); var readyColor = wellRenderer.material.color;
                var well = wellObject.AddComponent<MoonwellRecovery>(); well.Initialize(hero, raid, wellRenderer);
                var hud = hudObject.GetComponent<RaidHUD>(); hud.Initialize(raid, hero, core, cameraObject.GetComponent<Camera>(), well);
                Physics.SyncTransforms(); hero.Motor.Move(Vector3.down * .05f); wolf.Motor.Move(Vector3.down * .05f); ent.Motor.Move(Vector3.down * .05f); lateEnemy.Motor.Move(Vector3.down * .05f);
                yield return new WaitForFixedUpdate();

                Assert.That(hero.IsGrounded, Is.True, "The risk/reward fixture must start on factual ground.");
                Assert.That(hud.MoonwellActionVisible, Is.True); Assert.That(hud.MoonwellActionInteractable, Is.False);
                Assert.That(hud.MoonwellActionText, Is.EqualTo("MOONWELL — FULL HP"));
                var actionTransform = hudObject.transform.Find("MOONWELL — READY");
                Assert.That(actionTransform, Is.Not.Null, "The fixture must resolve its own Moonwell action.");
                var action = actionTransform.GetComponent<Button>();
                Assert.That(action.GetComponent<UiPointerOwnership>(), Is.Not.Null, "The recovery action must own its complete UI gesture.");

                hero.Health.TakeDamage(new DamageInfo(60, null, hero.transform.position), 0);
                hero.transform.position = wellObject.transform.position + Vector3.right * (MoonwellRecovery.InteractionRadius + .5f);
                hud.SendMessage("RefreshMoonwellAction", SendMessageOptions.RequireReceiver);
                Assert.That(hud.MoonwellActionText, Is.EqualTo("MOONWELL — DISTANT")); Assert.That(hud.MoonwellActionInteractable, Is.False); Assert.That(well.HasCharge, Is.True);
                Assert.That(wellRenderer.material.color, Is.EqualTo(readyColor), "Unavailable requests cannot spend the well presentation.");

                hero.transform.position = wellObject.transform.position;
                hud.SendMessage("RefreshMoonwellAction", SendMessageOptions.RequireReceiver);
                Assert.That(hud.MoonwellActionText, Is.EqualTo("USE MOONWELL")); Assert.That(hud.MoonwellActionInteractable, Is.True);
                var responsive = hud.GetComponent<ResponsiveHudRoot>();
                responsive.SetOrientationForTests(PrototypeOrientation.Portrait); yield return null; AssertContained(responsive, (RectTransform)action.transform);
                responsive.SetOrientationForTests(PrototypeOrientation.Landscape); yield return null; AssertContained(responsive, (RectTransform)action.transform);
                action.onClick.Invoke();
                Assert.That(hero.Health.Current, Is.EqualTo(70)); Assert.That(well.HasCharge, Is.False);
                Assert.That(hud.MoonwellActionText, Is.EqualTo("MOONWELL — SPENT")); Assert.That(hud.MoonwellActionInteractable, Is.False);
                Assert.That(wellRenderer.material.color, Is.Not.EqualTo(readyColor), "The well changes to spent only after positive recovery.");

                wolf.Health.TakeDamage(new DamageInfo(1000, hero.gameObject, wolf.transform.position), 0);
                Assert.That(raid.Gold, Is.EqualTo(15)); Assert.That(raid.RareMaterials, Is.Zero);
                Assert.That(facts, Has.Count.EqualTo(1)); Assert.That(facts[0].Source, Is.EqualTo(RaidRewardSource.EnemyDefeat)); Assert.That(facts[0].RareMaterialsDelta, Is.Zero);
                ent.Health.TakeDamage(new DamageInfo(1000, hero.gameObject, ent.transform.position), 0);
                Assert.That(raid.Gold, Is.EqualTo(30)); Assert.That(raid.RareMaterials, Is.EqualTo(1)); Assert.That(raid.EnemiesDefeated, Is.EqualTo(2));
                Assert.That(facts, Has.Count.EqualTo(2)); Assert.That(facts[1].Source, Is.EqualTo(RaidRewardSource.EnemyDefeat)); Assert.That(facts[1].RareMaterialsDelta, Is.EqualTo(1));
                Assert.That(ent.Health.TakeDamage(new DamageInfo(1000, hero.gameObject, ent.transform.position), 0), Is.False);
                Assert.That(raid.RareMaterials, Is.EqualTo(1), "Repeated death requests cannot duplicate the optional reward.");

                raid.BeginObjective(); raid.CompleteObjective(); raid.CompleteObjective();
                Assert.That(hud.MoonwellActionVisible, Is.False, "Terminal raid state hides recovery immediately.");
                lateEnemy.Health.TakeDamage(new DamageInfo(1000, hero.gameObject, lateEnemy.transform.position), 0);
                Assert.That(raid.EnemiesDefeated, Is.EqualTo(2)); Assert.That(raid.Gold, Is.EqualTo(130)); Assert.That(raid.RareMaterials, Is.EqualTo(2), "Post-terminal deaths cannot farm rewards.");
                Assert.That(facts, Has.Count.EqualTo(3));

                yield return new WaitForSeconds(1.35f);
                Assert.That(results, Has.Count.EqualTo(1));
                Assert.That(results[0].Gold, Is.EqualTo(130)); Assert.That(results[0].RareMaterials, Is.EqualTo(2)); Assert.That(results[0].EnemiesDefeated, Is.EqualTo(2));
                var stored = RealmProgress.Load(); Assert.That(stored.Gold, Is.EqualTo(130)); Assert.That(stored.RareMaterials, Is.EqualTo(2)); Assert.That(stored.CompletedRaids, Is.EqualTo(1));
                hud.SendMessage("ShowResult", results[0], SendMessageOptions.RequireReceiver);
                Assert.That(RealmProgress.Load().CompletedRaids, Is.EqualTo(1), "Result refresh cannot duplicate the secured reward.");

                Assert.That(Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None), Has.Length.EqualTo(initialCanvasCount + 1));
                Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None), Has.Length.EqualTo(initialEventSystemCount));
                Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None), Has.Length.EqualTo(initialListenerCount));
            }
            finally
            {
                GameplayInput.ResetForTests();
                if (hadProgress) PlayerPrefs.SetString(RealmProgress.KeyForTests, previousProgress); else PlayerPrefs.DeleteKey(RealmProgress.KeyForTests); PlayerPrefs.Save();
                Object.Destroy(hudObject); Object.Destroy(wellObject); Object.Destroy(coreObject); Object.Destroy(managerObject); Object.Destroy(cameraObject);
                Object.Destroy(hero.gameObject); Object.Destroy(wolf.gameObject); Object.Destroy(ent.gameObject); Object.Destroy(lateEnemy.gameObject); Object.Destroy(ground);
                Object.Destroy(heroDefinition); Object.Destroy(enemyDefinition);
            }
        }

        static CharacterDefinition Definition(string displayName, float maximumHealth)
        {
            var definition = ScriptableObject.CreateInstance<CharacterDefinition>(); definition.DisplayName = displayName; definition.Stats = new CombatStats { MaxHealth = maximumHealth, MoveSpeed = 3 }; return definition;
        }

        static CombatEntity Entity(string name, Vector3 position, CharacterDefinition definition)
        {
            var item = new GameObject(name, typeof(CharacterController), typeof(Health), typeof(CombatEntity)); item.transform.position = position;
            var entity = item.GetComponent<CombatEntity>(); entity.Initialize(definition); return entity;
        }

        static void AssertContained(ResponsiveHudRoot root, RectTransform action)
        {
            Canvas.ForceUpdateCanvases();
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(root.transform, action);
            var rect = ((RectTransform)root.transform).rect;
            Assert.That(rect.Contains(new Vector2(bounds.min.x, bounds.min.y)), Is.True, "Moonwell action lower edge must stay inside the safe HUD root.");
            Assert.That(rect.Contains(new Vector2(bounds.max.x, bounds.max.y)), Is.True, "Moonwell action upper edge must stay inside the safe HUD root.");
        }
    }
}
