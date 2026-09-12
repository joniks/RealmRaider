using NUnit.Framework;
using RealmRaiders.AI;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using RealmRaiders.Raid;
using RealmRaiders.Realm;
using UnityEngine;

namespace RealmRaiders.Tests
{
    public sealed class RealmNodeEncounterActivationTests
    {
        GameObject root;
        CharacterDefinition definition;

        [SetUp]
        public void SetUp()
        {
            GameplayInput.SetTerminalState(false);
            root = new GameObject("Node Entry Test Root");
            definition = ScriptableObject.CreateInstance<CharacterDefinition>();
            definition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 4, AttackSpeed = 1 };
            definition.Abilities = System.Array.Empty<AbilityDefinition>();
            definition.Possessable = true;
        }

        [TearDown]
        public void TearDown()
        {
            GameplayInput.SetTerminalState(false);
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void ExploredPresentationIsVisibleButExactHostileRemainsDormantUntilFirstEntry()
        {
            var hero = Entity("Hero", Vector3.left * 30, false, out _, out _);
            var hostile = Entity("Exact Hostile", Vector3.zero, true, out var brain, out _);
            var graph = new RealmGraph();
            var portal = graph.Add("Portal");
            var grove = graph.Add("Grove");
            graph.Connect("Portal", "Grove");
            var area = new GameObject("Grove Area");
            area.transform.SetParent(root.transform);
            var view = area.AddComponent<RealmNodeView>();
            RealmNodeVisit visit = null;
            var entryCount = 0;
            view.EncounterEntered += fact => { visit = fact; entryCount++; };
            view.Initialize(grove, hero, null, hostile.gameObject, null, hostile.gameObject);

            Assert.That(hostile.gameObject.activeSelf, Is.False);
            portal.Visit();
            Assert.That(hostile.gameObject.activeSelf, Is.True, "Exploration reveals exact presentation.");
            Assert.That(hostile.enabled, Is.False);
            Assert.That(brain.IsActive, Is.False);
            Assert.That(hostile.ActiveController, Is.SameAs(brain));
            Assert.That(view.GetComponent<NodeEntryEncounterActivation>().TrackedHostileCount, Is.EqualTo(1));

            var position = hostile.transform.position;
            hero.transform.position = area.transform.position;
            view.EvaluateEntry();
            Assert.That(hostile.enabled, Is.True);
            Assert.That(brain.IsActive, Is.True);
            Assert.That(hostile.ActiveController, Is.SameAs(brain));
            Assert.That(hostile.transform.position, Is.EqualTo(position));
            Assert.That(hostile.Health.Current, Is.EqualTo(100));
            Assert.That(entryCount, Is.EqualTo(1));
            Assert.That(visit.AliveHostiles, Has.Count.EqualTo(1));
            Assert.That(visit.AliveHostiles[0], Is.SameAs(hostile));

            view.EvaluateEntry();
            Assert.That(entryCount, Is.EqualTo(1), "Entry activation and fact publication are idempotent.");
        }

        [Test]
        public void LateEntryNeverRestoresDeadPossessedOrTerminalHostileAi()
        {
            var dead = Entity("Dead Before Entry", Vector3.left, true, out var deadBrain, out _);
            var deadGate = Gate("Dead Gate", dead, null, dead);
            dead.Health.TakeDamage(new DamageInfo(1000, null, dead.transform.position), 0);
            deadGate.Activate();
            Assert.That(dead.Health.IsDead, Is.True);
            Assert.That(dead.enabled, Is.False);
            Assert.That(deadBrain.IsActive, Is.False);

            var possessed = Entity("Possessed Before Entry", Vector3.right, true, out var possessedBrain, out var player);
            var possessedGate = Gate("Possessed Gate", possessed);
            possessed.SetController(player);
            possessedGate.Activate();
            Assert.That(possessed.enabled, Is.True, "A controller swap keeps entity ticking without restoring AI.");
            Assert.That(possessed.ActiveController, Is.SameAs(player));
            Assert.That(player.IsActive, Is.True);
            Assert.That(possessedBrain.IsActive, Is.False);

            var terminal = Entity("Terminal Before Entry", Vector3.forward, true, out var terminalBrain, out _);
            var terminalGate = Gate("Terminal Gate", terminal);
            GameplayInput.SetTerminalState(true);
            terminalGate.Activate();
            terminalGate.Activate();
            Assert.That(terminalGate.IsActivated, Is.True);
            Assert.That(terminal.enabled, Is.False);
            Assert.That(terminalBrain.IsActive, Is.False);
        }

        [Test]
        public void NullEmptyDuplicateAndTeardownDoNotTouchUnrelatedController()
        {
            var entity = Entity("Player Controlled", Vector3.zero, true, out var brain, out var player);
            entity.SetController(player);
            var gate = Gate("Safe Gate", entity, null, entity);
            Assert.That(gate.TrackedHostileCount, Is.EqualTo(1));
            Assert.That(gate.IsDormant(entity), Is.False);
            Assert.That(entity.enabled, Is.True);
            Assert.That(entity.ActiveController, Is.SameAs(player));
            Assert.That(brain.IsActive, Is.False);

            gate.Initialize(null);
            Assert.That(gate.TrackedHostileCount, Is.Zero);
            Object.DestroyImmediate(gate);
            Assert.That(entity.ActiveController, Is.SameAs(player));
            Assert.That(player.IsActive, Is.True);
            Assert.That(brain.IsActive, Is.False);
        }

        [Test]
        public void ReinitializeAndExplicitReleaseAreIdempotentAndRestoreOnlyOwnedLivingDormancy()
        {
            var entity = Entity("Dormant Reinitialize", Vector3.zero, true, out var brain, out _);
            var gate = Gate("Reinitialize Gate", entity);
            Assert.That(gate.IsDormant(entity), Is.True);
            Assert.That(entity.enabled, Is.False);
            Assert.That(brain.IsActive, Is.False);

            gate.Initialize(System.Array.Empty<CombatEntity>());
            Assert.That(gate.TrackedHostileCount, Is.Zero);
            Assert.That(entity.enabled, Is.True);
            Assert.That(entity.ActiveController, Is.SameAs(brain));
            Assert.That(brain.IsActive, Is.True);

            var swapped = Entity("Dormant Then Swapped", Vector3.right, true, out var swappedBrain, out var player);
            var swappedGate = Gate("Swapped Reinitialize Gate", swapped);
            Assert.That(swappedGate.IsDormant(swapped), Is.True);
            swapped.SetController(player);
            Assert.That(swapped.enabled, Is.False, "Controller selection alone cannot release gate-owned ticking dormancy.");
            swappedGate.Initialize(System.Array.Empty<CombatEntity>());
            Assert.That(swapped.enabled, Is.True);
            Assert.That(swapped.ActiveController, Is.SameAs(player));
            Assert.That(player.IsActive, Is.True);
            Assert.That(swappedBrain.IsActive, Is.False);

            gate.Initialize(new[] { entity });
            Assert.That(gate.IsDormant(entity), Is.True);
            gate.Release();
            gate.Release();
            Assert.That(entity.enabled, Is.True);
            Assert.That(entity.ActiveController, Is.SameAs(brain));
            Assert.That(brain.IsActive, Is.True);
            Object.DestroyImmediate(gate);
            Assert.That(entity.enabled, Is.True);
            Assert.That(entity.ActiveController, Is.SameAs(brain));
            Assert.That(brain.IsActive, Is.True);
        }

        NodeEntryEncounterActivation Gate(string name, params CombatEntity[] entities)
        {
            var owner = new GameObject(name, typeof(NodeEntryEncounterActivation));
            owner.transform.SetParent(root.transform);
            var gate = owner.GetComponent<NodeEntryEncounterActivation>();
            gate.Initialize(entities);
            return gate;
        }

        CombatEntity Entity(string name, Vector3 position, bool withControllers,
            out CreatureBrain brain, out PlayerController player)
        {
            var entityObject = new GameObject(name, typeof(CharacterController), typeof(Health), typeof(CombatEntity));
            entityObject.transform.SetParent(root.transform);
            entityObject.transform.position = position;
            brain = withControllers ? entityObject.AddComponent<CreatureBrain>() : null;
            player = withControllers ? entityObject.AddComponent<PlayerController>() : null;
            var entity = entityObject.GetComponent<CombatEntity>();
            entity.Initialize(definition);
            if (brain) entity.SetController(brain);
            return entity;
        }
    }
}
