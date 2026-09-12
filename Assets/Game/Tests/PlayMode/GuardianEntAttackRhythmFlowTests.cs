using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RealmRaiders.AI;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using UnityEngine;
using UnityEngine.TestTools;

namespace RealmRaiders.Tests
{
    public sealed class GuardianEntAttackRhythmFlowTests
    {
        [UnityTest]
        public IEnumerator OptedInEnt_UsesBasicBasicAreaAndFailedStartsDoNotAdvance()
        {
            using var fixture = new RhythmFixture();
            fixture.Brain.Tick();
            Assert.That(fixture.StartedAbilities, Is.EqualTo(new[] { "Smash" }),
                "An unconfigured brain must retain the legacy Basic-only choice.");
            yield return WaitForIdle(fixture.Entity);
            fixture.StartedAbilities.Clear();
            fixture.Brain.ConfigureGuardianEntHeavyAttackRhythm(new[] { fixture.Primary });

            Assert.That(fixture.Entity.TryUse(1, Vector3.forward), Is.True);
            fixture.Brain.Tick();
            Assert.That(fixture.Brain.ConsecutiveBasicCount, Is.Zero,
                "A use rejected by the action gate must not advance the rhythm.");
            yield return WaitForIdle(fixture.Entity);

            fixture.Brain.Tick();
            Assert.That(fixture.StartedAbilities, Is.EqualTo(new[] { "Charge", "Smash" }));
            Assert.That(fixture.Brain.ConsecutiveBasicCount, Is.EqualTo(1));
            yield return WaitForIdle(fixture.Entity);

            fixture.Brain.Tick();
            Assert.That(fixture.StartedAbilities[^1], Is.EqualTo("Smash"));
            Assert.That(fixture.Brain.ConsecutiveBasicCount, Is.EqualTo(2));
            yield return WaitForIdle(fixture.Entity);

            fixture.Brain.Tick();
            Assert.That(fixture.StartedAbilities[^1], Is.EqualTo("Ground Slam"));
            Assert.That(fixture.Brain.ConsecutiveBasicCount, Is.Zero);
        }

        [UnityTest]
        public IEnumerator OptedInEnt_ResetsForTargetControllerDeathAndCanUseImmediateArea()
        {
            using var fixture = new RhythmFixture();
            fixture.Brain.ConfigureGuardianEntHeavyAttackRhythm(new[] { fixture.Primary });
            fixture.Brain.Tick();
            Assert.That(fixture.Brain.ConsecutiveBasicCount, Is.EqualTo(1));
            yield return WaitForIdle(fixture.Entity);

            fixture.Primary.transform.position = new Vector3(0, 0, fixture.Brain.DetectionRange + 1);
            fixture.Brain.Tick();
            Assert.That(fixture.Brain.ConsecutiveBasicCount, Is.Zero,
                "A target outside the configured combat detection range must start a fresh rhythm on return.");
            fixture.Primary.transform.position = new Vector3(0, 0, 1.5f);
            fixture.Brain.Tick();
            Assert.That(fixture.StartedAbilities[^1], Is.EqualTo("Smash"));
            Assert.That(fixture.Brain.ConsecutiveBasicCount, Is.EqualTo(1));
            yield return WaitForIdle(fixture.Entity);

            fixture.Brain.ConfigureGuardianEntHeavyAttackRhythm(new[] { fixture.Primary });
            Assert.That(fixture.Brain.ConsecutiveBasicCount, Is.Zero,
                "Reconfiguration must not inherit an earlier encounter's rhythm.");
            fixture.Brain.Tick();
            Assert.That(fixture.Brain.ConsecutiveBasicCount, Is.EqualTo(1));
            yield return WaitForIdle(fixture.Entity);

            fixture.Brain.Target = fixture.Secondary;
            Assert.That(fixture.Brain.ConsecutiveBasicCount, Is.Zero);
            fixture.Brain.Target = fixture.Primary;
            fixture.Brain.Tick();
            Assert.That(fixture.Brain.ConsecutiveBasicCount, Is.EqualTo(1));
            yield return WaitForIdle(fixture.Entity);

            fixture.Brain.SetControl(false);
            Assert.That(fixture.Brain.ConsecutiveBasicCount, Is.Zero);
            fixture.Brain.ConfigureGuardianEntHeavyAttackRhythm(new[] { fixture.Primary, fixture.Secondary });
            fixture.Brain.Tick();
            Assert.That(fixture.StartedAbilities[^1], Is.EqualTo("Ground Slam"));
            Assert.That(fixture.Brain.ConsecutiveBasicCount, Is.Zero);
            yield return WaitForIdle(fixture.Entity);

            fixture.Brain.ConfigureGuardianEntHeavyAttackRhythm(new[] { fixture.Primary });
            fixture.Brain.Tick();
            Assert.That(fixture.Brain.ConsecutiveBasicCount, Is.EqualTo(1));
            fixture.Primary.Health.TakeDamage(new DamageInfo(1000, null, fixture.Primary.transform.position), 0);
            fixture.Brain.Tick();
            Assert.That(fixture.Brain.ConsecutiveBasicCount, Is.Zero,
                "An unusable configured target must clear retained rhythm state.");
            yield return WaitForIdle(fixture.Entity);

            fixture.Brain.Target = fixture.Secondary;
            fixture.Brain.ConfigureGuardianEntHeavyAttackRhythm(new[] { fixture.Secondary });
            fixture.Brain.Tick();
            Assert.That(fixture.Brain.ConsecutiveBasicCount, Is.EqualTo(1));
            fixture.Entity.Health.TakeDamage(new DamageInfo(1000, null, fixture.Entity.transform.position), 0);
            Assert.That(fixture.Entity.Health.IsDead, Is.True);
            Assert.That(fixture.Brain.ConsecutiveBasicCount, Is.Zero);
        }

        [UnityTest]
        public IEnumerator RecoveryUsesPerAbilityDurationAndDirectControlNeverAutoAttacks()
        {
            using var fixture = new RhythmFixture(.12f, .8f);
            var player = fixture.Entity.Controller<PlayerController>();
            fixture.Brain.ConfigureGuardianEntHeavyAttackRhythm(new[] { fixture.Primary });
            fixture.Entity.SetController(player);
            var actionCount = fixture.StartedAbilities.Count;
            yield return null;
            yield return null;
            Assert.That(fixture.StartedAbilities.Count, Is.EqualTo(actionCount),
                "Direct possession must not receive an AI-authored action.");

            Assert.That(fixture.Entity.TryUse(2, Vector3.forward), Is.True,
                "Direct control retains authority to choose Ground Slam.");
            yield return WaitForPhase(fixture.Entity, CombatActionPhase.Recovery);
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(fixture.Entity.ActionPhase, Is.EqualTo(CombatActionPhase.Recovery));
            yield return WaitForIdle(fixture.Entity);

            Assert.That(fixture.Entity.TryUse(0, Vector3.forward), Is.True);
            yield return WaitForPhase(fixture.Entity, CombatActionPhase.Recovery);
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(fixture.Entity.ActionPhase, Is.EqualTo(CombatActionPhase.Idle));

            fixture.Entity.SetController(fixture.Brain);
            Assert.That(fixture.Brain.ConsecutiveBasicCount, Is.Zero,
                "Returning from possession must start a clean AI rhythm.");
        }

        static IEnumerator WaitForIdle(CombatEntity entity) => WaitForPhase(entity, CombatActionPhase.Idle);

        static IEnumerator WaitForPhase(CombatEntity entity, CombatActionPhase phase)
        {
            var deadline = Time.realtimeSinceStartup + 2f;
            while (entity && entity.ActionPhase != phase && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(entity.ActionPhase, Is.EqualTo(phase));
        }

        sealed class RhythmFixture : System.IDisposable
        {
            readonly GameObject root;
            readonly GameObject primaryObject;
            readonly GameObject secondaryObject;
            readonly CharacterDefinition definition;
            readonly CharacterDefinition targetDefinition;
            readonly AbilityDefinition smash;
            readonly AbilityDefinition charge;
            readonly AbilityDefinition slam;

            public readonly CombatEntity Entity;
            public readonly CombatEntity Primary;
            public readonly CombatEntity Secondary;
            public readonly CreatureBrain Brain;
            public readonly List<string> StartedAbilities = new();

            public RhythmFixture(float basicRecovery = .01f, float areaRecovery = .01f)
            {
                root = new GameObject("Guardian Ent", typeof(CharacterController), typeof(Health),
                    typeof(CombatEntity), typeof(PlayerController), typeof(CreatureBrain));
                primaryObject = new GameObject("Primary Target", typeof(CharacterController), typeof(Health), typeof(CombatEntity));
                secondaryObject = new GameObject("Secondary Target", typeof(CharacterController), typeof(Health), typeof(CombatEntity));
                definition = ScriptableObject.CreateInstance<CharacterDefinition>();
                targetDefinition = ScriptableObject.CreateInstance<CharacterDefinition>();
                smash = Ability("Smash", AbilityKind.Melee, basicRecovery);
                charge = Ability("Charge", AbilityKind.Dash, basicRecovery);
                slam = Ability("Ground Slam", AbilityKind.Area, areaRecovery);
                definition.DisplayName = "Guardian Ent";
                definition.ArchetypeId = "realmraiders.guardian-ent";
                definition.Stats = new CombatStats { MaxHealth = 200, MoveSpeed = 3 };
                definition.Abilities = new[] { smash, charge, slam };
                targetDefinition.DisplayName = "Target";
                targetDefinition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 3 };
                targetDefinition.Abilities = System.Array.Empty<AbilityDefinition>();
                Entity = root.GetComponent<CombatEntity>(); Entity.Initialize(definition);
                Primary = primaryObject.GetComponent<CombatEntity>(); Primary.Initialize(targetDefinition);
                Secondary = secondaryObject.GetComponent<CombatEntity>(); Secondary.Initialize(targetDefinition);
                root.transform.position = Vector3.zero;
                primaryObject.transform.position = new Vector3(0, 0, 1.5f);
                secondaryObject.transform.position = new Vector3(1.5f, 0, 1.5f);
                Brain = root.GetComponent<CreatureBrain>();
                Brain.Target = Primary;
                Entity.PresentationChanged += fact =>
                {
                    if (fact.Phase == CombatActionPhase.Windup && fact.End == CombatPresentationEnd.None && fact.Ability)
                        StartedAbilities.Add(fact.Ability.DisplayName);
                };
            }

            static AbilityDefinition Ability(string name, AbilityKind kind, float recovery)
            {
                var ability = ScriptableObject.CreateInstance<AbilityDefinition>();
                ability.DisplayName = name;
                ability.Kind = kind;
                ability.Damage = 0;
                ability.Range = 2.7f;
                ability.Radius = 4;
                ability.Windup = .01f;
                ability.Cooldown = 0;
                ability.DashDistance = 0;
                ability.Recovery = recovery;
                return ability;
            }

            public void Dispose()
            {
                Object.Destroy(root);
                Object.Destroy(primaryObject);
                Object.Destroy(secondaryObject);
                Object.Destroy(definition);
                Object.Destroy(targetDefinition);
                Object.Destroy(smash);
                Object.Destroy(charge);
                Object.Destroy(slam);
            }
        }
    }
}
