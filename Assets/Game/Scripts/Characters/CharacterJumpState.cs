using System;

namespace RealmRaiders.Characters
{
    /// <summary>Small deterministic vertical-motion state; the CharacterController remains movement authority.</summary>
    public sealed class CharacterJumpState
    {
        public const float TakeoffSpeed = 6.4f;
        public const float Gravity = -18f;
        public const float MaximumFallSpeed = -20f;
        public const float MaximumAirborneSeconds = 2f;

        public bool IsActive { get; private set; }
        public float VerticalVelocity { get; private set; }
        public float Elapsed { get; private set; }

        public bool TryBegin(bool grounded)
        {
            if (IsActive || !grounded) return false;
            IsActive = true;
            VerticalVelocity = TakeoffSpeed;
            Elapsed = 0;
            return true;
        }

        public float Step(float deltaTime)
        {
            if (!IsActive) return 0;
            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime <= 0) return VerticalVelocity;
            Elapsed += deltaTime;
            if (Elapsed >= MaximumAirborneSeconds) { Cancel(); return 0; }
            VerticalVelocity = Math.Max(MaximumFallSpeed, VerticalVelocity + Gravity * deltaTime);
            return VerticalVelocity;
        }

        public bool ObserveGrounded(bool grounded)
        {
            if (!IsActive || !grounded || VerticalVelocity > 0) return false;
            Cancel();
            return true;
        }

        public void Cancel()
        {
            IsActive = false;
            VerticalVelocity = 0;
            Elapsed = 0;
        }
    }
}
