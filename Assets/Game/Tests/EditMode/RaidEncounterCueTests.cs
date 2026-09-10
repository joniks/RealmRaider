using NUnit.Framework;
using RealmRaiders.Raid;
using RealmRaiders.UI;

namespace RealmRaiders.Tests
{
    public sealed class RaidEncounterCueTests
    {
        [Test]
        public void LifecyclePublishesDiscoveryCountsAndClearWithoutDuplicateTransitions()
        {
            var lifecycle = new RaidEncounterLifecycle();

            Assert.That(lifecycle.TryEnter("Crossroads", 0, out var discovery), Is.True);
            Assert.That(discovery.Phase, Is.EqualTo(RaidEncounterPhase.Discovered));
            Assert.That(discovery.NodeId, Is.EqualTo("Crossroads"));
            Assert.That(discovery.RemainingHostiles, Is.Zero);
            Assert.That(lifecycle.TryEnter("Crossroads", 0, out _), Is.False, "Re-entry cannot replay discovery.");

            Assert.That(lifecycle.TryEnter("Wolf Grove", 2, out var entered), Is.True);
            Assert.That(entered.Phase, Is.EqualTo(RaidEncounterPhase.Hostiles));
            Assert.That(entered.RemainingHostiles, Is.EqualTo(2));
            Assert.That(lifecycle.TrySetRemaining("Wolf Grove", 2, out _), Is.False, "A duplicate count is not a transition.");
            Assert.That(lifecycle.TrySetRemaining("Crossroads", 1, out _), Is.False, "A stale node cannot alter the current encounter.");
            Assert.That(lifecycle.TrySetRemaining("Wolf Grove", 1, out var oneLeft), Is.True);
            Assert.That(oneLeft.RemainingHostiles, Is.EqualTo(1));
            Assert.That(lifecycle.TrySetRemaining("Wolf Grove", 0, out var cleared), Is.True);
            Assert.That(cleared.Phase, Is.EqualTo(RaidEncounterPhase.Cleared));
            Assert.That(lifecycle.TrySetRemaining("Wolf Grove", 0, out _), Is.False, "Clear is exact-once.");
        }

        [Test]
        public void CopyIsFactualForEmptyPluralSingularClearAndHiddenStates()
        {
            var lifecycle = new RaidEncounterLifecycle();
            lifecycle.TryEnter("Moonwell", 0, out var empty);
            Assert.That(RaidEncounterCue.CopyFor(empty), Is.EqualTo("MOONWELL DISCOVERED"));

            lifecycle.TryEnter("Wolf Grove", 2, out var plural);
            Assert.That(RaidEncounterCue.CopyFor(plural), Is.EqualTo("WOLF GROVE • 2 HOSTILES"));
            lifecycle.TrySetRemaining("Wolf Grove", 1, out var singular);
            Assert.That(RaidEncounterCue.CopyFor(singular), Is.EqualTo("WOLF GROVE • 1 HOSTILE"));
            lifecycle.TrySetRemaining("Wolf Grove", 0, out var clear);
            Assert.That(RaidEncounterCue.CopyFor(clear), Is.EqualTo("WOLF GROVE • AREA CLEAR"));

            lifecycle.TryHide(out var hidden);
            Assert.That(RaidEncounterCue.CopyFor(hidden), Is.Empty);
        }

        [Test]
        public void HiddenLifecycleKeepsVisitIdentityAndRejectsInvalidCounts()
        {
            var lifecycle = new RaidEncounterLifecycle();
            Assert.Throws<System.ArgumentException>(() => lifecycle.TryEnter(" ", 0, out _));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => lifecycle.TryEnter("Root Path", -1, out _));
            lifecycle.TryEnter("Root Path", 1, out _);
            Assert.Throws<System.ArgumentOutOfRangeException>(() => lifecycle.TrySetRemaining("Root Path", -1, out _));
            Assert.That(lifecycle.TryHide(out var hidden), Is.True);
            Assert.That(hidden.Visible, Is.False);
            Assert.That(lifecycle.TryHide(out _), Is.False);
            Assert.That(lifecycle.TryEnter("Root Path", 1, out _), Is.False, "Hiding presentation cannot make a visited node new again.");
        }
    }
}
