using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealmRaiders.Raid
{
    public enum RaidRewardSource { RoomDiscovery, EnemyDefeat, RealmCoreVictory }

    /// <summary>Immutable receipt emitted only after one successful authoritative raid reward mutation.</summary>
    public readonly struct RaidRewardFact
    {
        public long Sequence { get; }
        public RaidRewardSource Source { get; }
        public int GoldDelta { get; }
        public int RareMaterialsDelta { get; }
        public int TotalGold { get; }
        public int TotalRareMaterials { get; }
        public Vector3 WorldPosition { get; }

        public RaidRewardFact(long sequence, RaidRewardSource source, int goldDelta, int rareDelta,
            int totalGold, int totalRare, Vector3 worldPosition)
        {
            if (sequence <= 0) throw new ArgumentOutOfRangeException(nameof(sequence));
            if (!Enum.IsDefined(typeof(RaidRewardSource), source)) throw new ArgumentOutOfRangeException(nameof(source));
            if (goldDelta < 0 || rareDelta < 0 || goldDelta + rareDelta <= 0) throw new ArgumentOutOfRangeException(nameof(goldDelta));
            if (totalGold < goldDelta || totalRare < rareDelta) throw new ArgumentOutOfRangeException(nameof(totalGold));
            if (!Finite(worldPosition)) throw new ArgumentOutOfRangeException(nameof(worldPosition));
            Sequence = sequence; Source = source; GoldDelta = goldDelta; RareMaterialsDelta = rareDelta;
            TotalGold = totalGold; TotalRareMaterials = totalRare; WorldPosition = worldPosition;
        }

        static bool Finite(Vector3 value) => !float.IsNaN(value.x) && !float.IsInfinity(value.x) &&
            !float.IsNaN(value.y) && !float.IsInfinity(value.y) && !float.IsNaN(value.z) && !float.IsInfinity(value.z);
    }

    /// <summary>FIFO presentation ownership. Duplicate sequence receipts cannot replay feedback.</summary>
    public sealed class RaidRewardFactQueue
    {
        readonly Queue<RaidRewardFact> pending = new();
        readonly HashSet<long> accepted = new();
        public int Count => pending.Count;

        public bool TryEnqueue(RaidRewardFact fact)
        {
            if (fact.Sequence <= 0 || !accepted.Add(fact.Sequence)) return false;
            pending.Enqueue(fact);
            return true;
        }

        public bool TryDequeue(out RaidRewardFact fact)
        {
            if (pending.Count == 0) { fact = default; return false; }
            fact = pending.Dequeue();
            return true;
        }

        public void Clear()
        {
            pending.Clear();
            accepted.Clear();
        }
    }
}
