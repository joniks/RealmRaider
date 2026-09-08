using NUnit.Framework;
using RealmRaiders.AI;
using UnityEngine;

namespace RealmRaiders.Tests
{
    public sealed class InvaderStuckRecoveryTests
    {
        static readonly Vector3 Waypoint = new(0, 0, 10);

        [Test]
        public void StuckWindowRequiresContinuousInsufficientRootProgress()
        {
            var recovery = new InvaderStuckRecovery();
            recovery.Reset(Vector3.zero);
            recovery.Evaluate(Vector3.zero, Vector3.zero, Waypoint, 4, InvaderStuckRecovery.StuckWindow - .01f, true);
            Assert.That(recovery.IsRecovering, Is.False);
            recovery.Evaluate(Vector3.zero, Vector3.zero, Waypoint, 4, .02f, true);
            Assert.That(recovery.IsRecovering, Is.True);
            Assert.That(recovery.Side, Is.EqualTo(1));

            recovery.Reset(Vector3.zero);
            var position = Vector3.zero;
            for (var step = 0; step < 30; step++)
            {
                position.z += InvaderStuckRecovery.MinimumProgressSpeed * .2f;
                recovery.Evaluate(position, Vector3.zero, Waypoint, 4, .1f, true);
                Assert.That(recovery.IsRecovering, Is.False, "Normal slow forward progress must reset the continuous stuck window.");
            }
        }

        [Test]
        public void FailedFirstAttemptUsesOppositeSideAndProgressExitsImmediately()
        {
            var recovery = EnterRecovery();
            Assert.That(recovery.Side, Is.EqualTo(1));

            recovery.Evaluate(Vector3.zero, Vector3.zero, Waypoint, 4, InvaderStuckRecovery.FailedAttemptWindow + .01f, true);
            Assert.That(recovery.IsRecovering, Is.True);
            Assert.That(recovery.Side, Is.EqualTo(-1));

            recovery.Evaluate(new Vector3(0, 0, InvaderStuckRecovery.RecoveryExitProgress + .01f), Vector3.zero, Waypoint, 4, .1f, true);
            Assert.That(recovery.IsRecovering, Is.False);
            Assert.That(recovery.Side, Is.Zero);
            Assert.That(recovery.StuckSeconds, Is.Zero);
        }

        [Test]
        public void RecoveryDirectionCannotStepFartherOutsideSegmentCorridor()
        {
            var recovery = EnterRecovery();
            var atRightCap = new Vector3(InvaderStuckRecovery.CorridorHalfWidth, 0, 0);
            recovery.Pause(atRightCap);
            var direction = recovery.Evaluate(atRightCap, Vector3.zero, Waypoint, 4, .1f, true);
            Assert.That(Vector3.Dot(direction, Vector3.right), Is.LessThanOrEqualTo(.0001f));

            var nearRightCap = new Vector3(InvaderStuckRecovery.CorridorHalfWidth - .01f, 0, 0);
            recovery.Pause(nearRightCap);
            direction = recovery.Evaluate(nearRightCap, Vector3.zero, Waypoint, 4, .1f, true);
            var predicted = nearRightCap + direction * 4 * .1f;
            Assert.That(InvaderStuckRecovery.CorridorOffset(predicted, Vector3.zero, Waypoint), Is.LessThanOrEqualTo(InvaderStuckRecovery.CorridorHalfWidth + .001f));
        }

        [Test]
        public void PausePreservesRecoveryWhileResetClearsEveryTimerAndSide()
        {
            var recovery = EnterRecovery();
            var side = recovery.Side;
            for (var step = 0; step < 20; step++) recovery.Pause(Vector3.zero);
            Assert.That(recovery.IsRecovering, Is.True);
            Assert.That(recovery.Side, Is.EqualTo(side));
            Assert.That(recovery.AttemptSeconds, Is.Zero);

            recovery.Reset(Vector3.zero);
            Assert.That(recovery.IsRecovering, Is.False);
            Assert.That(recovery.Side, Is.Zero);
            Assert.That(recovery.StuckSeconds, Is.Zero);
            Assert.That(recovery.AttemptSeconds, Is.Zero);
        }

        static InvaderStuckRecovery EnterRecovery()
        {
            var recovery = new InvaderStuckRecovery();
            recovery.Reset(Vector3.zero);
            recovery.Evaluate(Vector3.zero, Vector3.zero, Waypoint, 4, InvaderStuckRecovery.StuckWindow + .01f, true);
            Assert.That(recovery.IsRecovering, Is.True);
            return recovery;
        }
    }
}
