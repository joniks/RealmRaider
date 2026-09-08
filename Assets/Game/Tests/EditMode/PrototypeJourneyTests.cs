using NUnit.Framework;
using RealmRaiders.Core;

namespace RealmRaiders.Tests
{
    public sealed class PrototypeJourneyTests
    {
        [SetUp]
        public void SetUp() => PrototypeJourney.ResetForTests();

        [TearDown]
        public void TearDown() => PrototypeJourney.ResetForTests();

        [Test]
        public void CanonicalJourneyAdvancesOnlyInOrderAndCompletesOnce()
        {
            Assert.That(PrototypeJourney.TryStart(out var token), Is.True);
            Assert.That(token, Is.Not.Zero);
            Assert.That(PrototypeJourney.Stage, Is.EqualTo(PrototypeJourneyStage.Build));
            Assert.That(PrototypeJourney.TryStart(out _), Is.False);

            Assert.That(PrototypeJourney.TryBeginRaid(token), Is.True);
            Assert.That(PrototypeJourney.TryBeginRaid(token), Is.False);
            token = PrototypeJourney.ActiveToken;
            Assert.That(PrototypeJourney.TryReachRaidResult(token), Is.True);
            token = PrototypeJourney.ActiveToken;
            Assert.That(PrototypeJourney.TryBeginDefense(token), Is.True);
            token = PrototypeJourney.ActiveToken;
            Assert.That(PrototypeJourney.TryCompleteDefense(token), Is.True);

            Assert.That(PrototypeJourney.Stage, Is.EqualTo(PrototypeJourneyStage.Inactive));
            Assert.That(PrototypeJourney.TryCompleteDefense(PrototypeJourney.ActiveToken), Is.False);
            Assert.That(PrototypeJourney.IsActive, Is.False);
            Assert.That(PrototypeJourney.ActiveToken, Is.Zero);
        }

        [Test]
        public void OutOfOrderAndDuplicateCallsFailClosedWithoutChangingStage()
        {
            AssertRejectedAt(0, PrototypeJourneyStage.Inactive);

            Assert.That(PrototypeJourney.TryStart(out var token), Is.True);
            Assert.That(PrototypeJourney.TryStart(out _), Is.False);
            Assert.That(PrototypeJourney.TryReachRaidResult(token), Is.False);
            Assert.That(PrototypeJourney.TryRetryRaid(token), Is.False);
            Assert.That(PrototypeJourney.TryBeginDefense(token), Is.False);
            Assert.That(PrototypeJourney.TryCompleteDefense(token), Is.False);
            Assert.That(PrototypeJourney.Stage, Is.EqualTo(PrototypeJourneyStage.Build));

            Assert.That(PrototypeJourney.TryBeginRaid(token), Is.True);
            var staleBuildToken = token;
            token = PrototypeJourney.ActiveToken;
            Assert.That(PrototypeJourney.TryStart(out _), Is.False);
            Assert.That(PrototypeJourney.TryBeginRaid(token), Is.False);
            Assert.That(PrototypeJourney.TryRetryRaid(token), Is.False);
            Assert.That(PrototypeJourney.TryBeginDefense(token), Is.False);
            Assert.That(PrototypeJourney.TryCompleteDefense(token), Is.False);
            Assert.That(PrototypeJourney.Stage, Is.EqualTo(PrototypeJourneyStage.Raid));

            AssertRejectedAt(staleBuildToken, PrototypeJourneyStage.Raid);
            AssertRejectedAt(token + 1, PrototypeJourneyStage.Raid);
        }

        [Test]
        public void RaidRetryIsExplicitAndReturnsToTheSameOrderedGate()
        {
            Assert.That(PrototypeJourney.TryStart(out var token), Is.True);
            Assert.That(PrototypeJourney.TryBeginRaid(token), Is.True);
            token = PrototypeJourney.ActiveToken;
            Assert.That(PrototypeJourney.TryReachRaidResult(token), Is.True);
            token = PrototypeJourney.ActiveToken;
            Assert.That(PrototypeJourney.TryRetryRaid(token), Is.True);
            token = PrototypeJourney.ActiveToken;
            Assert.That(PrototypeJourney.Stage, Is.EqualTo(PrototypeJourneyStage.Raid));
            Assert.That(PrototypeJourney.TryRetryRaid(token), Is.False);
            Assert.That(PrototypeJourney.TryReachRaidResult(token), Is.True);
            token = PrototypeJourney.ActiveToken;
            Assert.That(PrototypeJourney.TryBeginDefense(token), Is.True);
        }

        [Test]
        public void CancellationIsIdempotentAndFreshSessionCanStartAgain()
        {
            Assert.That(PrototypeJourney.Cancel(), Is.False);
            Assert.That(PrototypeJourney.TryStart(out var firstToken), Is.True);
            Assert.That(PrototypeJourney.Cancel(firstToken + 1), Is.False);
            Assert.That(PrototypeJourney.Cancel(firstToken), Is.True);
            Assert.That(PrototypeJourney.Cancel(), Is.False);
            Assert.That(PrototypeJourney.Stage, Is.EqualTo(PrototypeJourneyStage.Inactive));
            Assert.That(PrototypeJourney.TryStart(out var secondToken), Is.True);
            Assert.That(secondToken, Is.GreaterThan(firstToken));
            Assert.That(PrototypeJourney.Cancel(firstToken), Is.False, "An older scene token cannot cancel a newer journey.");
            PrototypeJourney.ResetForTests();
            Assert.That(PrototypeJourney.Stage, Is.EqualTo(PrototypeJourneyStage.Inactive));
        }

        static void AssertRejectedAt(int token, PrototypeJourneyStage expected)
        {
            Assert.That(PrototypeJourney.TryBeginRaid(token), Is.False);
            Assert.That(PrototypeJourney.TryReachRaidResult(token), Is.False);
            Assert.That(PrototypeJourney.TryRetryRaid(token), Is.False);
            Assert.That(PrototypeJourney.TryBeginDefense(token), Is.False);
            Assert.That(PrototypeJourney.TryCompleteDefense(token), Is.False);
            Assert.That(PrototypeJourney.Cancel(token), Is.False);
            Assert.That(PrototypeJourney.Stage, Is.EqualTo(expected));
        }
    }
}
