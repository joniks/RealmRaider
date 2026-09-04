using System.Collections;
using NUnit.Framework;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Possession;
using RealmRaiders.Raid;
using RealmRaiders.Realm;
using UnityEngine;
using UnityEngine.TestTools;

namespace RealmRaiders.Tests
{
    public sealed class DefenseFlowTests
    {
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

                yield return null;
                yield return null;
                Assert.That(defense.State, Is.EqualTo(DefenseState.RealmLost));

                invader.Health.TakeDamage(new DamageInfo(1000, null, invader.transform.position), 0);
                yield return null;

                Assert.That(defense.State, Is.EqualTo(DefenseState.RealmLost));
                Assert.That(defense.IsFinished, Is.True);
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
    }
}
