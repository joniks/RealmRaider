using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.AI;
using RealmRaiders.CameraSystem;
using RealmRaiders.Controllers;
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
