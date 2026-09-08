using System.Collections.Generic;
using UnityEngine;

namespace RealmRaiders.Possession
{
    public readonly struct KeeperSelectionCandidate
    {
        public KeeperSelectionCandidate(long stableId, Vector2 screenPosition, bool exactHit,
            bool registered, bool possessable, bool alive, bool visible, bool occluded)
        {
            StableId = stableId;
            ScreenPosition = screenPosition;
            ExactHit = exactHit;
            Registered = registered;
            Possessable = possessable;
            Alive = alive;
            Visible = visible;
            Occluded = occluded;
        }

        public long StableId { get; }
        public Vector2 ScreenPosition { get; }
        public bool ExactHit { get; }
        public bool Registered { get; }
        public bool Possessable { get; }
        public bool Alive { get; }
        public bool Visible { get; }
        public bool Occluded { get; }
        public bool IsEligible => StableId > 0 && Registered && Possessable && Alive && Visible && !Occluded;
    }

    public static class KeeperSelectionResolver
    {
        public const float NearMissRadiusShortEdge = .055f;

        public static float ShortEdge(Vector2 screenSize)
        {
            if (!IsFinite(screenSize.x) || !IsFinite(screenSize.y)) return 0;
            return Mathf.Max(0, Mathf.Min(screenSize.x, screenSize.y));
        }

        public static bool TryResolve(IReadOnlyList<KeeperSelectionCandidate> candidates,
            Vector2 pointerPosition, Vector2 screenSize, out long stableId)
        {
            return TryResolve(candidates, pointerPosition, screenSize, NearMissRadiusShortEdge, out stableId);
        }

        public static bool TryResolve(IReadOnlyList<KeeperSelectionCandidate> candidates,
            Vector2 pointerPosition, Vector2 screenSize, float normalizedRadius, out long stableId)
        {
            stableId = 0;
            if (candidates == null || !IsFinite(pointerPosition.x) || !IsFinite(pointerPosition.y)) return false;

            var shortEdge = ShortEdge(screenSize);
            if (shortEdge <= 0 || !IsFinite(normalizedRadius) || normalizedRadius < 0) return false;

            var exactId = long.MaxValue;
            for (var index = 0; index < candidates.Count; index++)
            {
                var candidate = candidates[index];
                if (candidate.IsEligible && candidate.ExactHit && candidate.StableId < exactId)
                    exactId = candidate.StableId;
            }

            if (exactId != long.MaxValue)
            {
                stableId = exactId;
                return true;
            }

            var radius = shortEdge * normalizedRadius;
            var radiusSquared = radius * radius;
            var bestDistanceSquared = float.PositiveInfinity;
            var bestId = long.MaxValue;
            for (var index = 0; index < candidates.Count; index++)
            {
                var candidate = candidates[index];
                if (!candidate.IsEligible || candidate.ExactHit) continue;
                var distanceSquared = (candidate.ScreenPosition - pointerPosition).sqrMagnitude;
                if (!IsFinite(distanceSquared) || distanceSquared > radiusSquared) continue;
                if (distanceSquared < bestDistanceSquared ||
                    Mathf.Approximately(distanceSquared, bestDistanceSquared) && candidate.StableId < bestId)
                {
                    bestDistanceSquared = distanceSquared;
                    bestId = candidate.StableId;
                }
            }

            if (bestId == long.MaxValue) return false;
            stableId = bestId;
            return true;
        }

        static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
