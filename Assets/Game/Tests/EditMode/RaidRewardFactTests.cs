using NUnit.Framework;
using RealmRaiders.Raid;
using RealmRaiders.UI;
using UnityEngine;

namespace RealmRaiders.Tests
{
    public sealed class RaidRewardFactTests
    {
        [Test]
        public void FactsRetainExactDeltaTotalSourceAndPosition()
        {
            var position = new Vector3(3.5f, 2, -8);
            var fact = new RaidRewardFact(7, RaidRewardSource.RealmCoreVictory, 100, 1, 145, 2, position);
            Assert.That(fact.Sequence, Is.EqualTo(7));
            Assert.That(fact.Source, Is.EqualTo(RaidRewardSource.RealmCoreVictory));
            Assert.That(fact.GoldDelta, Is.EqualTo(100)); Assert.That(fact.RareMaterialsDelta, Is.EqualTo(1));
            Assert.That(fact.TotalGold, Is.EqualTo(145)); Assert.That(fact.TotalRareMaterials, Is.EqualTo(2));
            Assert.That(fact.WorldPosition, Is.EqualTo(position));
            Assert.That(RaidRewardCue.CopyFor(fact), Is.EqualTo("+100 GOLD • +1 RARE  •  REALM CORE DEFEATED\nTOTAL 145 GOLD • 2 RARE"));
        }

        [Test]
        public void FactsRejectImpossibleDeltasTotalsSourcesAndPositions()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new RaidRewardFact(0, RaidRewardSource.RoomDiscovery, 5, 0, 5, 0, Vector3.zero));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new RaidRewardFact(1, (RaidRewardSource)99, 5, 0, 5, 0, Vector3.zero));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new RaidRewardFact(1, RaidRewardSource.RoomDiscovery, 0, 0, 0, 0, Vector3.zero));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new RaidRewardFact(1, RaidRewardSource.RoomDiscovery, 5, 0, 4, 0, Vector3.zero));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new RaidRewardFact(1, RaidRewardSource.EnemyDefeat, 15, 0, 15, 0, new Vector3(float.NaN, 0, 0)));
        }

        [Test]
        public void QueuePreservesOrderAndRejectsDuplicateReceiptsUntilLifecycleReset()
        {
            var queue = new RaidRewardFactQueue();
            var room = new RaidRewardFact(1, RaidRewardSource.RoomDiscovery, 5, 0, 5, 0, Vector3.left);
            var enemy = new RaidRewardFact(2, RaidRewardSource.EnemyDefeat, 15, 0, 20, 0, Vector3.right);
            Assert.That(queue.TryEnqueue(room), Is.True);
            Assert.That(queue.TryEnqueue(enemy), Is.True);
            Assert.That(queue.TryEnqueue(room), Is.False);
            Assert.That(queue.Count, Is.EqualTo(2));
            Assert.That(queue.TryDequeue(out var first), Is.True); Assert.That(first.Sequence, Is.EqualTo(1));
            Assert.That(queue.TryDequeue(out var second), Is.True); Assert.That(second.Sequence, Is.EqualTo(2));
            Assert.That(queue.TryDequeue(out _), Is.False);
            queue.Clear();
            Assert.That(queue.TryEnqueue(room), Is.True, "A new raid/HUD lifecycle may reuse its own sequence numbers.");
        }

        [TestCase(RaidRewardSource.RoomDiscovery, 5, 0, 5, 0, "+5 GOLD  •  ROOM DISCOVERED\nTOTAL 5 GOLD • 0 RARE")]
        [TestCase(RaidRewardSource.EnemyDefeat, 15, 0, 20, 0, "+15 GOLD  •  ENEMY DEFEATED\nTOTAL 20 GOLD • 0 RARE")]
        public void CopyUsesOnlyFactValues(RaidRewardSource source, int gold, int rare, int totalGold, int totalRare, string expected)
            => Assert.That(RaidRewardCue.CopyFor(new RaidRewardFact(1, source, gold, rare, totalGold, totalRare, Vector3.zero)), Is.EqualTo(expected));
    }
}
