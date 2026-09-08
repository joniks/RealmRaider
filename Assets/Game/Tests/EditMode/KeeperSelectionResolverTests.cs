using NUnit.Framework;
using RealmRaiders.Possession;
using UnityEngine;

namespace RealmRaiders.Tests
{
    public sealed class KeeperSelectionResolverTests
    {
        [Test]
        public void ExactHitPrecedesCloserNearMiss()
        {
            var candidates = new[]
            {
                Candidate(20, Vector2.zero),
                Candidate(10, new Vector2(400, 400), exact: true)
            };

            Assert.That(KeeperSelectionResolver.TryResolve(candidates, Vector2.zero, new Vector2(1000, 1600), out var selected), Is.True);
            Assert.That(selected, Is.EqualTo(10));
        }

        [Test]
        public void NearMissRadiusUsesShortScreenEdgeInBothOrientations()
        {
            var inside = new[] { Candidate(7, new Vector2(53.9f, 0)) };
            var outside = new[] { Candidate(7, new Vector2(54.1f, 0)) };
            var portrait = new Vector2(1080, 1920);
            var landscape = new Vector2(1920, 1080);

            Assert.That(KeeperSelectionResolver.TryResolve(inside, Vector2.zero, portrait, .05f, out _), Is.True);
            Assert.That(KeeperSelectionResolver.TryResolve(inside, Vector2.zero, landscape, .05f, out _), Is.True);
            Assert.That(KeeperSelectionResolver.TryResolve(outside, Vector2.zero, portrait, .05f, out _), Is.False);
            Assert.That(KeeperSelectionResolver.TryResolve(outside, Vector2.zero, landscape, .05f, out _), Is.False);
        }

        [Test]
        public void NearMissThresholdIsInclusive()
        {
            var atThreshold = new[] { Candidate(4, new Vector2(50, 0)) };
            var outside = new[] { Candidate(4, new Vector2(50.01f, 0)) };

            Assert.That(KeeperSelectionResolver.TryResolve(atThreshold, Vector2.zero, new Vector2(1000, 2000), .05f, out _), Is.True);
            Assert.That(KeeperSelectionResolver.TryResolve(outside, Vector2.zero, new Vector2(1000, 2000), .05f, out _), Is.False);
        }

        [Test]
        public void EqualNearMissesUseStableIdentityRegardlessOfCandidateOrder()
        {
            var first = new[] { Candidate(42, new Vector2(-20, 0)), Candidate(9, new Vector2(20, 0)) };
            var reversed = new[] { first[1], first[0] };

            Assert.That(KeeperSelectionResolver.TryResolve(first, Vector2.zero, new Vector2(1000, 1000), out var firstResult), Is.True);
            Assert.That(KeeperSelectionResolver.TryResolve(reversed, Vector2.zero, new Vector2(1000, 1000), out var reversedResult), Is.True);
            Assert.That(firstResult, Is.EqualTo(9));
            Assert.That(reversedResult, Is.EqualTo(9));
        }

        [Test]
        public void RejectedCandidateStatesNeverResolve()
        {
            var candidates = new[]
            {
                Candidate(1, Vector2.zero, exact: true, registered: false),
                Candidate(2, Vector2.zero, exact: true, possessable: false),
                Candidate(3, Vector2.zero, exact: true, alive: false),
                Candidate(4, Vector2.zero, exact: true, visible: false),
                Candidate(5, Vector2.zero, exact: true, occluded: true),
                Candidate(0, Vector2.zero, exact: true)
            };

            Assert.That(KeeperSelectionResolver.TryResolve(candidates, Vector2.zero, new Vector2(1000, 1000), out _), Is.False);
            Assert.That(KeeperSelectionResolver.TryResolve(candidates, Vector2.zero, Vector2.zero, out _), Is.False);
            Assert.That(typeof(Object).IsAssignableFrom(typeof(KeeperSelectionResolver)), Is.False);
        }

        static KeeperSelectionCandidate Candidate(long id, Vector2 position, bool exact = false,
            bool registered = true, bool possessable = true, bool alive = true,
            bool visible = true, bool occluded = false)
        {
            return new KeeperSelectionCandidate(id, position, exact, registered, possessable, alive, visible, occluded);
        }
    }
}
