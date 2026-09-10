using NUnit.Framework;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Modules.CharacterProceduralMotion;
using UnityEngine;

namespace RealmRaiders.Tests
{
    public sealed class CharacterCombatPresentationTests
    {
        [Test]
        public void EntityFacts_AreReadonlySynchronousSnapshotsAndCancellationDoesNotInventAnAction()
        {
            var host = new GameObject("Passive Combat Facts");
            var entity = host.AddComponent<CombatEntity>();
            var ability = ScriptableObject.CreateInstance<AbilityDefinition>();
            var facts = new System.Collections.Generic.List<CombatPresentationFact>();
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            try
            {
                // Drive the existing state synchronously; the real accepted TryUse path is covered in PlayMode.
                var state = (CombatActionState)typeof(CombatEntity).GetField("action", flags).GetValue(entity);
                typeof(CombatEntity).GetField("presentationActionId", flags).SetValue(entity, 7L);
                typeof(CombatEntity).GetField("presentationDirection", flags).SetValue(entity, Vector3.right);
                typeof(CombatEntity).GetField("presentationFacing", flags).SetValue(entity, Quaternion.Euler(0, 30, 0));
                typeof(CombatEntity).GetField("presentationAbility", flags).SetValue(entity, ability);
                typeof(CombatEntity).GetField("presentationWindup", flags).SetValue(entity, .25f);
                entity.PresentationChanged += facts.Add;
                var publish = typeof(CombatEntity).GetMethod("PublishPresentation", flags);
                state.TryBegin(); publish.Invoke(entity, new object[] { CombatPresentationEnd.None });
                state.Impact(); publish.Invoke(entity, new object[] { CombatPresentationEnd.None });
                state.Recover(); publish.Invoke(entity, new object[] { CombatPresentationEnd.None });
                Assert.That(facts.Count, Is.EqualTo(3));
                Assert.That(facts[0].Phase, Is.EqualTo(CombatActionPhase.Windup));
                Assert.That(facts[1].Phase, Is.EqualTo(CombatActionPhase.Impact));
                Assert.That(facts[2].Phase, Is.EqualTo(CombatActionPhase.Recovery));
                Assert.That(entity.ActionPhase, Is.EqualTo(CombatActionPhase.Recovery), "Publishing cannot advance gameplay.");
                foreach (var fact in facts)
                {
                    Assert.That(fact.ActionId, Is.EqualTo(7));
                    Assert.That(fact.WorldDirection, Is.EqualTo(Vector3.right));
                    Assert.That(fact.FacingBeforeAction, Is.EqualTo(Quaternion.Euler(0, 30, 0)));
                    Assert.That(fact.Ability, Is.SameAs(ability));
                    Assert.That(fact.ScaledTime, Is.EqualTo(Time.time));
                    Assert.That(fact.UnscaledTime, Is.EqualTo(Time.unscaledTime));
                }
                ability.Windup = 9;
                Assert.That(facts[0].WindupSeconds, Is.EqualTo(.25f), "The duration is a value snapshot, not a live ability read.");
                typeof(CombatEntity).GetMethod("CancelActionPresentation", flags).Invoke(entity, new object[] { CombatPresentationEnd.ControllerChanged });
                Assert.That(entity.ActionPhase, Is.EqualTo(CombatActionPhase.Idle));
                Assert.That(facts[3].ActionId, Is.EqualTo(7));
                Assert.That(facts[3].End, Is.EqualTo(CombatPresentationEnd.ControllerChanged));
                foreach (var field in typeof(CombatPresentationFact).GetFields()) Assert.That(field.IsInitOnly, Is.True);
            }
            finally
            {
                entity.PresentationChanged -= facts.Add;
                Object.DestroyImmediate(host); Object.DestroyImmediate(ability);
            }
        }

        static CombatPresentationFact Fact(long id, CombatActionPhase phase, float scaled, float unscaled,
            CombatPresentationEnd end = CombatPresentationEnd.None) => new(id, phase, end, Vector3.right,
                Quaternion.identity, null, .2f, scaled, unscaled);

        [Test]
        public void Timeline_PreservesSynchronousImpactRecoveryAndCompletionWithoutGameplayTimingAuthority()
        {
            var timeline = new CharacterCombatPresentationTimeline();
            timeline.OnAction(Fact(1, CombatActionPhase.Windup, 2, 10));
            var start = timeline.Observe(2, 10);
            Assert.That(start.AttackProgress, Is.Zero);
            Assert.That(start.SignedAttackDirection, Is.EqualTo(1));
            Assert.That(timeline.Observe(2.1f, 10.1f).AttackProgress, Is.EqualTo(.5f).Within(.0001f));
            Assert.That(timeline.Observe(2.1f, 11).AttackProgress, Is.EqualTo(.5f).Within(.0001f), "Paused scaled windup does not advance with unscaled time.");
            timeline.OnAction(Fact(1, CombatActionPhase.Impact, 2.2f, 11.1f));
            timeline.OnAction(Fact(1, CombatActionPhase.Recovery, 2.2f, 11.1f));
            var impact = timeline.Observe(2.2f, 11.1f);
            Assert.That(impact.AttackStage, Is.EqualTo(ProceduralHumanoidAttackStage.Impact));
            Assert.That(impact.AttackProgress, Is.Zero);
            Assert.That(timeline.Observe(2.24f, 11.14f).AttackProgress, Is.EqualTo(.5f).Within(.0001f));
            var recovery = timeline.Observe(2.28f, 11.18f);
            Assert.That(recovery.AttackStage, Is.EqualTo(ProceduralHumanoidAttackStage.Recovery));
            Assert.That(recovery.AttackProgress, Is.EqualTo(0).Within(.0001f));
            timeline.OnAction(Fact(1, CombatActionPhase.Idle, 2.32f, 11.22f, CombatPresentationEnd.Completed));
            Assert.That(timeline.Observe(2.36f, 11.26f).AttackProgress, Is.EqualTo(.5f).Within(.0001f));
            Assert.That(timeline.Observe(3, 12).AttackBlend, Is.Zero);
            Assert.That(timeline.HasAttack, Is.False);
        }

        [Test]
        public void Timeline_EqualTimestampsAreIdempotentAndSparseFramesMatchDenseSampling()
        {
            var sparse = new CharacterCombatPresentationTimeline();
            var dense = new CharacterCombatPresentationTimeline();
            foreach (var timeline in new[] { sparse, dense })
            {
                timeline.OnAction(Fact(4, CombatActionPhase.Windup, 0, 0));
                timeline.OnAction(Fact(4, CombatActionPhase.Impact, .2f, .2f));
                timeline.OnAction(Fact(4, CombatActionPhase.Recovery, .2f, .2f));
                timeline.OnHit(.21f, -.7f);
            }
            for (var i = 0; i < 36; i++) dense.Observe(i / 100f, i / 100f);
            var expected = sparse.Observe(.35f, .35f);
            Assert.That(dense.Observe(.35f, .35f), Is.EqualTo(expected));
            Assert.That(dense.Observe(.35f, .35f), Is.EqualTo(expected));
            Assert.That(sparse.HasAttack, Is.True);
            Assert.That(sparse.HasHit, Is.True, "Caller preserves Hit > Attack while the action clock continues underneath.");
            Assert.That(sparse.Observe(1, 1).HitWeight, Is.Zero);
            Assert.That(sparse.HasHit, Is.False);
            Assert.That(sparse.HasAttack, Is.False);
        }

        [Test]
        public void Timeline_HitHasContinuousRisePeakRecoveryAndRepeatedHitRestartsFromCurrentDirectionalWeight()
        {
            var timeline = new CharacterCombatPresentationTimeline();
            timeline.OnHit(0, -1);
            Assert.That(timeline.Observe(0, 0).HitWeight, Is.Zero);
            Assert.That(timeline.Observe(.03f, .03f).HitWeight, Is.EqualTo(.5f).Within(.00001f));
            Assert.That(timeline.Observe(.06f, .06f).HitWeight, Is.EqualTo(1));
            var before = timeline.Observe(.12f, .12f);
            Assert.That(before.HitWeight, Is.EqualTo(.5f).Within(.00001f));
            timeline.OnHit(.12f, 1);
            var repeated = timeline.Observe(.12f, .12f);
            Assert.That(repeated.HitWeight, Is.EqualTo(before.HitWeight));
            Assert.That(repeated.SignedRecoilDirection, Is.EqualTo(before.SignedRecoilDirection));
            var intermediate = timeline.Observe(.15f, .15f);
            Assert.That(intermediate.HitWeight, Is.InRange(.5f, 1f));
            Assert.That(intermediate.SignedRecoilDirection, Is.InRange(-1f, 1f));
            Assert.That(timeline.Observe(.18f, .18f).SignedRecoilDirection, Is.EqualTo(1).Within(.0001f));
            Assert.That(timeline.Observe(.31f, .31f).HitWeight, Is.Zero);
            for (var i = 0; i < 20; i++)
            {
                timeline.OnHit(1 + i * .01f, i % 2 == 0 ? -1 : 1);
                var sample = timeline.Observe(1 + i * .01f, 1 + i * .01f);
                Assert.That(sample.HitWeight, Is.InRange(0f, 1f));
                Assert.That(sample.SignedRecoilDirection, Is.InRange(-1f, 1f));
            }
        }

        [TestCase(CombatPresentationEnd.ControllerChanged)]
        [TestCase(CombatPresentationEnd.Death)]
        [TestCase(CombatPresentationEnd.Terminal)]
        [TestCase(CombatPresentationEnd.Disabled)]
        [TestCase(CombatPresentationEnd.Destroyed)]
        public void Timeline_ClearDropsAllTransientFactsAndRejectsStaleActionCallbacks(CombatPresentationEnd end)
        {
            var timeline = new CharacterCombatPresentationTimeline();
            timeline.OnAction(Fact(2, CombatActionPhase.Windup, 0, 0));
            timeline.OnHit(0, 1);
            timeline.OnAction(Fact(2, CombatActionPhase.Idle, .1f, .1f, end));
            timeline.OnAction(Fact(2, CombatActionPhase.Windup, 0, 0));
            timeline.OnAction(Fact(2, CombatActionPhase.Impact, .2f, .2f));
            var sample = timeline.Observe(.3f, .3f);
            Assert.That(sample.AttackBlend, Is.Zero);
            Assert.That(sample.HitWeight, Is.Zero);
            timeline.OnAction(Fact(3, CombatActionPhase.Windup, .4f, .4f));
            Assert.That(timeline.Observe(.5f, .5f).AttackBlend, Is.EqualTo(1));
            timeline.Observe(float.NaN, .6f);
            Assert.That(timeline.HasAttack, Is.False);
        }

        [Test]
        public void DamageDirection_SnapshotsSourceThenPointThenNeutralInCharacterFrame()
        {
            var source = new GameObject("Damage Source");
            try
            {
                source.transform.position = Vector3.left;
                var hit = new DamageInfo(1, source, Vector3.right);
                var value = CharacterCombatPresentationTimeline.RecoilDirection(hit, Vector3.zero, Quaternion.identity);
                Assert.That(value, Is.EqualTo(1));
                source.transform.position = Vector3.right;
                Assert.That(value, Is.EqualTo(1), "Previously captured scalar does not track source transforms.");
                source.transform.position = Vector3.zero;
                Assert.That(CharacterCombatPresentationTimeline.RecoilDirection(hit, Vector3.zero, Quaternion.identity), Is.EqualTo(-1));
                Assert.That(CharacterCombatPresentationTimeline.RecoilDirection(new DamageInfo(1, null, Vector3.zero), Vector3.zero, Quaternion.identity), Is.Zero);
                Assert.That(CharacterCombatPresentationTimeline.RecoilDirection(new DamageInfo(1, null, new Vector3(float.NaN, 0, 1)), Vector3.zero, Quaternion.identity), Is.Zero);
                Assert.That(CharacterCombatPresentationTimeline.SignedDirection(Vector3.forward, Quaternion.Euler(0, 90, 0)), Is.EqualTo(-1).Within(.0001f));
            }
            finally { Object.DestroyImmediate(source); }
        }
    }
}
