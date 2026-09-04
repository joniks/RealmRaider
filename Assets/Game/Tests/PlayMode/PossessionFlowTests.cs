using System.Collections;
using NUnit.Framework;
using RealmRaiders.CameraSystem;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using RealmRaiders.AI;
using RealmRaiders.Possession;
using UnityEngine;
using UnityEngine.TestTools;

namespace RealmRaiders.Tests
{
    public sealed class PossessionFlowTests
    {
        [UnityTest]
        public IEnumerator PossessionPreservesEntityStateAndRestoresAiWithSingleView()
        {
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(PrototypeCameraRig));
            cameraObject.tag = "MainCamera";
            var entityObject = new GameObject("Possessable", typeof(CharacterController), typeof(Health), typeof(CombatEntity), typeof(PlayerController), typeof(CreatureBrain));
            var managerObject = new GameObject("Possession Manager", typeof(PossessionManager));
            var definition = ScriptableObject.CreateInstance<CharacterDefinition>();
            var ability = ScriptableObject.CreateInstance<AbilityDefinition>();
            try
            {
                ability.DisplayName = "Test Strike"; ability.Cooldown = 10; ability.Windup = .1f;
                definition.DisplayName = "Possessable";
                definition.Possessable = true;
                definition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 1, AttackSpeed = 1 };
                definition.Abilities = new[] { ability };
                var entity = entityObject.GetComponent<CombatEntity>();
                entity.Initialize(definition);
                var ai = entityObject.GetComponent<CreatureBrain>();
                var player = entityObject.GetComponent<PlayerController>();
                entity.SetController(ai);
                var rig = cameraObject.GetComponent<PrototypeCameraRig>();
                rig.ConfigureOverview(new Vector3(0, 5, -8), Quaternion.identity);
                rig.SnapToOverview();
                var manager = managerObject.GetComponent<PossessionManager>();
                manager.Initialize(rig); manager.Register(entity); manager.Select(entity);
                entity.Health.TakeDamage(new DamageInfo(17, null, entity.transform.position), 0);
                var healthBefore = entity.Health.Current;
                Assert.That(entity.TryUse(0, Vector3.forward), Is.True);
                var readyBefore = entity.Abilities[0].IsReady;

                Assert.That(manager.PossessSelected(), Is.True);
                yield return null;
                Assert.That(manager.Possessed, Is.SameAs(entity));
                Assert.That(manager.Possessed.gameObject, Is.SameAs(entity.gameObject));
                Assert.That(entity.Health.Current, Is.EqualTo(healthBefore));
                Assert.That(entity.Abilities[0].IsReady, Is.EqualTo(readyBefore));
                Assert.That(player.IsActive, Is.True); Assert.That(ai.IsActive, Is.False);
                Assert.That(Object.FindObjectsByType<Camera>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
                Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None), Has.Length.EqualTo(1));

                manager.Release();
                yield return null;
                Assert.That(manager.Possessed, Is.Null);
                Assert.That(entity.Health.Current, Is.EqualTo(healthBefore));
                Assert.That(entity.Abilities[0].IsReady, Is.EqualTo(readyBefore));
                Assert.That(player.IsActive, Is.False); Assert.That(ai.IsActive, Is.True);
                Assert.That(Object.FindObjectsByType<Camera>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
                Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
            }
            finally
            {
                Object.Destroy(managerObject); Object.Destroy(entityObject); Object.Destroy(cameraObject);
                Object.Destroy(definition); Object.Destroy(ability);
            }
        }
    }
}
