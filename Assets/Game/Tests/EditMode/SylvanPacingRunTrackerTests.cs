using NUnit.Framework;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Raid;
using RealmRaiders.Realm;
using UnityEngine;

namespace RealmRaiders.Tests
{
    public sealed class SylvanPacingRunTrackerTests
    {
        const string Ancient = "realmraiders.sylvan-layout.ancient-crossroads";
        const string Forked = "realmraiders.sylvan-layout.forked-canopy";
        const string Serpent = "realmraiders.sylvan-layout.serpent-roots";
        GameObject root;
        CharacterDefinition definition;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("Pacing Tracker Tests");
            definition = ScriptableObject.CreateInstance<CharacterDefinition>();
            definition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 3 };
            definition.Abilities = System.Array.Empty<AbilityDefinition>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(definition);
        }

        [TestCase(Ancient, 1, "DEFEAT GUARDIAN ENT")]
        [TestCase(Forked, 1, "DEFEAT GUARDIAN ENT")]
        [TestCase(Serpent, 4, "DEFEAT SYLVAN WOLVES")]
        public void ExactValidatedRecipeResolvesToImmutableCoreSnapshot(string layoutId, int required, string next)
        {
            var snapshot = SylvanPacingRunTracker.ResolveSnapshot(layoutId);
            Assert.That(snapshot, Is.Not.Null);
            Assert.That(snapshot.LayoutId, Is.EqualTo(layoutId));
            Assert.That(snapshot.Beats, Is.Not.Empty);
            var copied = new SylvanPacingBeatSnapshot[snapshot.Beats.Count];
            for (var index = 0; index < copied.Length; index++) copied[index] = snapshot.Beats[index];
            var immutable = new SylvanPacingRecipeSnapshot(snapshot.LayoutId, snapshot.TacticalSummary, copied);
            copied[0] = null;
            Assert.That(immutable.Beats[0], Is.Not.Null, "Core snapshot must not retain caller-owned collection storage.");
            var tracker = SylvanPacingRunTracker.Attach(root.transform, layoutId, immutable);
            Assert.That(tracker, Is.Not.Null);
            Assert.That(tracker.RequiredBeatCount, Is.EqualTo(required));
            Assert.That(tracker.NextRequirementCopy, Is.EqualTo(next));
        }

        [Test]
        public void RequiredFactsAreRecordedOutOfOrderButAdvanceOnlyAsAuthored()
        {
            var tracker = Attach(Serpent);
            var wolf = Entity("Exact Wolf");
            var unrelated = Entity("Unrelated");
            var deadEnt = Entity("Dead Ent");
            deadEnt.Health.TakeDamage(new DamageInfo(1000, null, Vector3.zero), 0);

            tracker.ObserveNodeEntry("RootPathHazard", System.Array.Empty<CombatEntity>());
            tracker.ObserveNodeEntry("WolfGroveEncounter", new[] { wolf, wolf });
            Assert.That(tracker.CompletedRequiredBeatCount, Is.Zero);
            Assert.That(tracker.NextRequirementCopy, Is.EqualTo("DEFEAT SYLVAN WOLVES"));
            unrelated.Health.TakeDamage(new DamageInfo(1000, null, Vector3.zero), 0);
            Assert.That(tracker.CompletedRequiredBeatCount, Is.Zero, "Unrelated death cannot satisfy the exact encounter snapshot.");

            wolf.Health.TakeDamage(new DamageInfo(1000, null, Vector3.zero), 0);
            Assert.That(tracker.CompletedRequiredBeatCount, Is.EqualTo(2), "Previously observed Root entry advances only after Wolf completes.");
            Assert.That(tracker.NextRequirementCopy, Is.EqualTo("DEFEAT GUARDIAN ENT"));
            tracker.ObserveNodeEntry("EntGroveEncounter", new[] { deadEnt });
            Assert.That(tracker.CompletedRequiredBeatCount, Is.EqualTo(3), "Dead-at-entry required encounter is already clear.");
            tracker.ObserveNodeEntry("MoonwellRecovery", null);
            Assert.That(tracker.IsWarded, Is.False);
            Assert.That(tracker.CompletedRequiredBeatCount, Is.EqualTo(4));
            tracker.ObserveNodeEntry("MoonwellRecovery", null);
            Assert.That(tracker.CompletedRequiredBeatCount, Is.EqualTo(4), "Duplicate facts remain idempotent.");
        }

        [Test]
        public void OptionalBeatNeverBlocksAndDisposeRemovesExactDeathOwnership()
        {
            var ancient = Attach(Ancient);
            var optionalWolf = Entity("Optional Wolf");
            ancient.ObserveNodeEntry("WolfGroveEncounter", new[] { optionalWolf });
            Assert.That(ancient.NextRequirementCopy, Is.EqualTo("DEFEAT GUARDIAN ENT"));
            ancient.ObserveNodeEntry("EntGroveEncounter", System.Array.Empty<CombatEntity>());
            Assert.That(ancient.IsWarded, Is.False);

            var serpentOwner = new GameObject("Disposable Tracker");
            serpentOwner.transform.SetParent(root.transform);
            var serpent = SylvanPacingRunTracker.Attach(serpentOwner.transform, Serpent,
                SylvanPacingRunTracker.ResolveSnapshot(Serpent));
            var exactWolf = Entity("Disposable Exact Wolf");
            serpent.ObserveNodeEntry("WolfGroveEncounter", new[] { exactWolf });
            serpent.DisposeRun();
            exactWolf.Health.TakeDamage(new DamageInfo(1000, null, Vector3.zero), 0);
            Assert.That(serpent.IsOperational, Is.False);
            Assert.That(serpent.CompletedRequiredBeatCount, Is.Zero, "Disposed run cannot receive stale death callbacks.");
        }

        [Test]
        public void MissingMalformedMismatchedAndIncompleteBindingsFailClosedWithoutWard()
        {
            Assert.That(SylvanPacingRunTracker.ResolveSnapshot(null), Is.Null);
            Assert.That(SylvanPacingRunTracker.ResolveSnapshot("realmraiders.sylvan-layout.missing"), Is.Null);
            var malformed = new SylvanPacingRecipeSnapshot(Ancient, "MALFORMED", new[]
            {
                new SylvanPacingBeatSnapshot("duplicate", "EntGroveEncounter", SylvanPacingBeatKind.Encounter, SylvanPacingRequirement.Required),
                new SylvanPacingBeatSnapshot("duplicate", "HeartTreeObjective", SylvanPacingBeatKind.Objective, SylvanPacingRequirement.Required)
            });
            Assert.That(SylvanPacingRunTracker.Attach(root.transform, Ancient, malformed), Is.Null);
            var valid = SylvanPacingRunTracker.ResolveSnapshot(Ancient);
            var lowercaseSummary = new SylvanPacingRecipeSnapshot(valid.LayoutId,
                valid.TacticalSummary.ToLowerInvariant(), valid.Beats);
            Assert.That(SylvanPacingRunTracker.Attach(root.transform, Ancient, lowercaseSummary), Is.Null,
                "Core must preserve the module's uppercase tactical-summary validation after copying facts.");
            Assert.That(SylvanPacingRunTracker.Attach(root.transform, Forked,
                SylvanPacingRunTracker.ResolveSnapshot(Ancient)), Is.Null);

            var owner = new GameObject("Incomplete Bindings"); owner.transform.SetParent(root.transform);
            var tracker = SylvanPacingRunTracker.Attach(owner.transform, Ancient,
                SylvanPacingRunTracker.ResolveSnapshot(Ancient));
            var view = new GameObject("One Bound Node", typeof(RealmNodeView)).GetComponent<RealmNodeView>();
            view.transform.SetParent(root.transform);
            tracker.BindNode("WolfGroveEncounter", view);
            tracker.BindNode("RootPathHazard", view);
            Assert.That(tracker.IsOperational, Is.False, "Duplicate view binding invalidates the partial tracker.");
            Assert.That(tracker.IsWarded, Is.False, "Invalid binding falls back to legacy unwarded behavior.");
        }

        SylvanPacingRunTracker Attach(string layoutId) => SylvanPacingRunTracker.Attach(root.transform,
            layoutId, SylvanPacingRunTracker.ResolveSnapshot(layoutId));

        CombatEntity Entity(string name)
        {
            var owner = new GameObject(name, typeof(CharacterController), typeof(Health), typeof(CombatEntity));
            owner.transform.SetParent(root.transform);
            var entity = owner.GetComponent<CombatEntity>();
            entity.Initialize(definition);
            return entity;
        }
    }
}
