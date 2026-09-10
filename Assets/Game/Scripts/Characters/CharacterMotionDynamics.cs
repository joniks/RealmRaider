using UnityEngine;

namespace RealmRaiders.Characters
{
    /// <summary>Pure presentation state. Distance drives phase; time only eases weight toward factual velocity.</summary>
    public sealed class CharacterMotionDynamics
    {
        public const float Cadence = 7.5f;
        public const float SpeedResponse = 10f;
        public const float TurnResponse = 12f;
        public const float MaximumTurnLean = 6f;
        public const float MaximumWeightPitch = 3f;
        const float NeutralThreshold = .0001f;
        float forwardLag;

        public float Phase { get; private set; }
        public float Speed { get; private set; }
        public float ForwardSpeed { get; private set; }
        public float WeightPitch { get; private set; }
        public float TurnLean { get; private set; }

        public void Step(Vector3 localDisplacement, float yawDelta, float deltaTime, float referenceSpeed)
        {
            if (!Finite(deltaTime) || deltaTime <= 0) return;
            if (!Finite(localDisplacement.x) || !Finite(localDisplacement.z) || !Finite(yawDelta) ||
                !Finite(referenceSpeed) || referenceSpeed <= 0) { Reset(); return; }

            var distance = new Vector2(localDisplacement.x, localDisplacement.z).magnitude;
            var speed = Mathf.Clamp01(distance / deltaTime / referenceSpeed);
            var forward = Mathf.Clamp(localDisplacement.z / deltaTime / referenceSpeed, -1f, 1f);
            // Cap implausible displacement to full-speed cadence; never infer travel from input or time alone.
            Phase = Mathf.Repeat(Phase + speed * deltaTime * Cadence, Mathf.PI * 2f);
            var decay = Mathf.Exp(-SpeedResponse * deltaTime);
            var response = 1f - decay;
            Speed = Mathf.Lerp(Speed, speed, response);
            // Exact two-pole response for constant factual velocity across this interval.
            // Its difference gives a smooth signed acceleration/braking accent without differentiating noisy input.
            forwardLag = forward + (forwardLag - forward + SpeedResponse * (ForwardSpeed - forward) * deltaTime) * decay;
            ForwardSpeed = Mathf.Lerp(ForwardSpeed, forward, response);
            WeightPitch = Mathf.Clamp((ForwardSpeed - forwardLag) * 8f, -MaximumWeightPitch, MaximumWeightPitch);
            var turn = -Mathf.Clamp(yawDelta / deltaTime / 180f, -1f, 1f) * MaximumTurnLean;
            TurnLean = Mathf.Lerp(TurnLean, turn, 1f - Mathf.Exp(-TurnResponse * deltaTime));

            if (speed == 0 && Speed < NeutralThreshold)
            {
                Speed = 0;
                ForwardSpeed = 0;
                forwardLag = 0;
                WeightPitch = 0;
                Phase = 0;
            }
            if (turn == 0 && Mathf.Abs(TurnLean) < NeutralThreshold) TurnLean = 0;
        }

        public void Reset() { Phase = 0; Speed = 0; ForwardSpeed = 0; forwardLag = 0; WeightPitch = 0; TurnLean = 0; }
        static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
