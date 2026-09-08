using System;

namespace RealmRaiders.Controllers
{
    public readonly struct BufferedAbilityRequest
    {
        public int AbilityIndex { get; }
        public float DirectionX { get; }
        public float DirectionY { get; }
        public float DirectionZ { get; }

        internal BufferedAbilityRequest(int abilityIndex, float directionX, float directionY, float directionZ)
        {
            AbilityIndex = abilityIndex;
            DirectionX = directionX;
            DirectionY = directionY;
            DirectionZ = directionZ;
        }
    }

    public sealed class CombatInputBuffer
    {
        public const float WindowSeconds = .20f;

        int abilityIndex = -1;
        float directionX;
        float directionY;
        float directionZ;
        float expiresAt;

        public bool HasPending => abilityIndex >= 0;
        public int PendingAbilityIndex => abilityIndex;

        public bool TryQueue(int index, float x, float y, float z, float now)
        {
            if (index < 0 || !IsFinite(now)) return false;
            NormalizeSafe(ref x, ref y, ref z);
            abilityIndex = index;
            directionX = x;
            directionY = y;
            directionZ = z;
            expiresAt = now + WindowSeconds;
            return true;
        }

        public bool TryConsume(float now, out BufferedAbilityRequest request)
        {
            request = default;
            if (!HasPending) return false;

            var index = abilityIndex;
            var x = directionX;
            var y = directionY;
            var z = directionZ;
            var valid = IsFinite(now) && now < expiresAt;
            Clear();
            if (!valid) return false;

            request = new BufferedAbilityRequest(index, x, y, z);
            return true;
        }

        public bool Expire(float now)
        {
            if (!HasPending || IsFinite(now) && now < expiresAt) return false;
            Clear();
            return true;
        }

        public void Clear()
        {
            abilityIndex = -1;
            directionX = 0;
            directionY = 0;
            directionZ = 0;
            expiresAt = 0;
        }

        static void NormalizeSafe(ref float x, ref float y, ref float z)
        {
            if (!IsFinite(x) || !IsFinite(y) || !IsFinite(z))
            {
                x = 0;
                y = 0;
                z = 1;
                return;
            }

            var magnitudeSquared = (double)x * x + (double)y * y + (double)z * z;
            if (magnitudeSquared <= .0001d)
            {
                x = 0;
                y = 0;
                z = 1;
                return;
            }

            var inverseMagnitude = 1d / Math.Sqrt(magnitudeSquared);
            x = (float)(x * inverseMagnitude);
            y = (float)(y * inverseMagnitude);
            z = (float)(z * inverseMagnitude);
        }

        static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
