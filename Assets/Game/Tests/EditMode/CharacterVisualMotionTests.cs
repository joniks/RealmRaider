using NUnit.Framework;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using UnityEngine;

namespace RealmRaiders.Tests
{
    public sealed class CharacterVisualMotionTests
    {
        [Test]
        public void DefeatPresentation_UsesOnlyPivotWithUnscaledBoundedStableSettle()
        {
            var host = new GameObject("Defeat Visual Motion Host");
            var pivot = new GameObject("Presentation Pivot").transform;
            pivot.SetParent(host.transform, false);
            var motion = host.AddComponent<CharacterVisualMotion>();
            try
            {
                motion.Bind(pivot);
                var rootPosition = host.transform.position;
                var rootRotation = host.transform.rotation;
                var rootScale = host.transform.localScale;

                Assert.That(motion.StartDefeat(), Is.True);
                Assert.That(motion.StartDefeat(), Is.False, "A factual death response cannot restart while it owns the pivot.");
                var startedAt = motion.DefeatEndsAt - CharacterVisualMotion.DefeatSettleDuration;
                motion.Sample(0, startedAt, .016f, Vector3.zero, CombatActionPhase.Idle);
                Assert.That(motion.IsDefeatActive, Is.True);
                Assert.That(motion.IsDefeatSettled, Is.False);
                Assert.That(pivot.localPosition, Is.EqualTo(motion.BasePosition));
                Assert.That(pivot.localRotation, Is.EqualTo(motion.BaseRotation));
                Assert.That(pivot.localScale, Is.EqualTo(motion.BaseScale));

                motion.Sample(0, startedAt + CharacterVisualMotion.DefeatSettleDuration * .5f, .016f, Vector3.zero, CombatActionPhase.Windup);
                Assert.That(pivot.localPosition.y, Is.LessThan(motion.BasePosition.y));
                Assert.That(Quaternion.Angle(pivot.localRotation, motion.BaseRotation), Is.GreaterThan(10f));
                Assert.That(pivot.localScale.y, Is.LessThan(motion.BaseScale.y));
                Assert.That(Vector3.Distance(pivot.localPosition, motion.BasePosition), Is.LessThanOrEqualTo(.08f));
                Assert.That(host.transform.position, Is.EqualTo(rootPosition));
                Assert.That(host.transform.rotation, Is.EqualTo(rootRotation));
                Assert.That(host.transform.localScale, Is.EqualTo(rootScale));

                motion.Sample(0, motion.DefeatEndsAt, 0, Vector3.zero, CombatActionPhase.Recovery);
                var settledPosition = pivot.localPosition;
                var settledRotation = pivot.localRotation;
                var settledScale = pivot.localScale;
                Assert.That(motion.IsDefeatSettled, Is.True);
                motion.Sample(0, motion.DefeatEndsAt + 10f, .016f, Vector3.forward * 9f, CombatActionPhase.Impact);
                Assert.That(pivot.localPosition, Is.EqualTo(settledPosition), "The final defeated pose remains stable instead of restarting ordinary motion.");
                Assert.That(pivot.localRotation, Is.EqualTo(settledRotation));
                Assert.That(pivot.localScale, Is.EqualTo(settledScale));
            }
            finally { Object.DestroyImmediate(host); }
        }

        [Test]
        public void DefeatPresentation_RebindAndExplicitClearRestoreBoundPivots()
        {
            var host = new GameObject("Defeat Visual Motion Cleanup Host");
            var original = new GameObject("Original Presentation Pivot").transform;
            original.SetParent(host.transform, false);
            var replacement = new GameObject("Replacement Presentation Pivot").transform;
            replacement.SetParent(host.transform, false);
            var motion = host.AddComponent<CharacterVisualMotion>();
            try
            {
                motion.Bind(original);
                Assert.That(motion.StartDefeat(), Is.True);
                motion.Sample(0, motion.DefeatEndsAt - CharacterVisualMotion.DefeatSettleDuration * .5f, .016f, Vector3.zero, CombatActionPhase.Idle);
                Assert.That(Quaternion.Angle(original.localRotation, motion.BaseRotation), Is.GreaterThan(.1f));

                motion.Bind(replacement);
                Assert.That(motion.IsDefeatActive, Is.False);
                Assert.That(original.localPosition, Is.EqualTo(Vector3.zero));
                Assert.That(original.localRotation, Is.EqualTo(Quaternion.identity));
                Assert.That(original.localScale, Is.EqualTo(Vector3.one));

                Assert.That(motion.StartDefeat(), Is.True);
                motion.Sample(0, motion.DefeatEndsAt - CharacterVisualMotion.DefeatSettleDuration * .5f, .016f, Vector3.zero, CombatActionPhase.Idle);
                motion.ClearDefeat();
                Assert.That(motion.IsDefeatActive, Is.False);
                Assert.That(replacement.localPosition, Is.EqualTo(Vector3.zero));
                Assert.That(replacement.localRotation, Is.EqualTo(Quaternion.identity));
                Assert.That(replacement.localScale, Is.EqualTo(Vector3.one));
            }
            finally { Object.DestroyImmediate(host); }
        }
    }
}
