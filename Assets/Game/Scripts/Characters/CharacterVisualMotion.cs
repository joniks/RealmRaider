using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using UnityEngine;

namespace RealmRaiders.Characters
{
    /// <summary>Presentation-only motion for assembled character visuals. It never moves the CombatEntity root.</summary>
    [DisallowMultipleComponent]
    public sealed class CharacterVisualMotion : MonoBehaviour
    {
        const float TakeoffDuration = .10f;
        const float LandingDuration = .14f;
        public const float PossessionArrivalDuration = .22f;
        public const float DefeatSettleDuration = .24f;
        const float MaximumOffset = .08f;
        const float MinimumScaleFactor = .90f;
        const float MaximumScaleFactor = 1.10f;
        Transform presentationPivot;
        CombatEntity entity;
        Vector3 basePosition;
        Quaternion baseRotation;
        Vector3 baseScale;
        Vector3 lastRootPosition;
        float movement;
        float hitReactionUntil;
        float seed;
        float takeoffUntil;
        float landingUntil;
        float possessionArrivalStartedAt;
        float possessionArrivalUntil;
        bool jumpStateObserved;
        bool observedJumping;
        bool possessionArrivalActive;
        bool defeatActive;
        bool defeatSettled;
        float defeatStartedAt;
        float defeatUntil;
        bool hasOrdinaryPose;
        Vector3 ordinaryPosition;
        Quaternion ordinaryRotation;
        Vector3 ordinaryScale;

        public Transform PresentationPivot => presentationPivot;
        public Vector3 BasePosition => basePosition;
        public Quaternion BaseRotation => baseRotation;
        public Vector3 BaseScale => baseScale;
        public bool IsPossessionArrivalActive => possessionArrivalActive;
        public float PossessionArrivalEndsAt => possessionArrivalUntil;
        public bool IsDefeatActive => defeatActive;
        public bool IsDefeatSettled => defeatSettled;
        public float DefeatEndsAt => defeatUntil;

        void Awake()
        {
            entity = GetComponent<CombatEntity>();
            seed = Mathf.Abs(GetEntityId().GetHashCode() % 997) * .013f;
            lastRootPosition = transform.position;
        }

        public void Bind(Transform pivot)
        {
            ClearDefeat();
            ClearPossessionArrival();
            Restore();
            presentationPivot = pivot;
            if (!presentationPivot) return;
            basePosition = presentationPivot.localPosition;
            baseRotation = presentationPivot.localRotation;
            baseScale = presentationPivot.localScale;
            movement = 0;
            hasOrdinaryPose = false;
            lastRootPosition = transform.position;
            ResetJumpTransitions();
        }

        public void ShowHitReaction() => hitReactionUntil = Mathf.Max(hitReactionUntil, Time.time + .12f);

        public void ClearTransientReaction()
        {
            hitReactionUntil = 0;
            if (!defeatActive) Restore();
        }

        public void Restore()
        {
            if (presentationPivot)
            {
                presentationPivot.localPosition = basePosition;
                presentationPivot.localRotation = baseRotation;
                presentationPivot.localScale = baseScale;
            }
            movement = 0;
            ResetJumpTransitions();
        }

        /// <summary>Starts one unscaled, bounded presentation-only possession accent when a pivot is available.</summary>
        public bool StartPossessionArrival()
        {
            if (!presentationPivot) return false;
            if (possessionArrivalActive && Time.unscaledTime < possessionArrivalUntil) return false;
            possessionArrivalStartedAt = Time.unscaledTime;
            possessionArrivalUntil = possessionArrivalStartedAt + PossessionArrivalDuration;
            possessionArrivalActive = true;
            return true;
        }

        /// <summary>Starts the one factual death response. CombatEntity invokes this only from Health.Died.</summary>
        public bool StartDefeat()
        {
            if (!presentationPivot || defeatActive) return false;
            ClearPossessionArrival();
            hitReactionUntil = 0;
            movement = 0;
            ResetJumpTransitions();
            hasOrdinaryPose = false;
            defeatStartedAt = Time.unscaledTime;
            defeatUntil = defeatStartedAt + DefeatSettleDuration;
            defeatSettled = false;
            defeatActive = true;
            return true;
        }

        /// <summary>Restores a bound pivot before a visual rebind, disable, or destruction can leave a stale defeat pose.</summary>
        public void ClearDefeat()
        {
            if (!defeatActive) return;
            defeatActive = false;
            defeatSettled = false;
            defeatStartedAt = 0;
            defeatUntil = 0;
            Restore();
        }

        /// <summary>Removes only the pending possession accent, retaining ordinary motion and other reactions.</summary>
        public void ClearPossessionArrival()
        {
            if (!possessionArrivalActive) return;
            possessionArrivalActive = false;
            possessionArrivalStartedAt = 0;
            possessionArrivalUntil = 0;
            if (defeatActive || !presentationPivot || !hasOrdinaryPose) return;
            presentationPivot.localPosition = ordinaryPosition;
            presentationPivot.localRotation = ordinaryRotation;
            presentationPivot.localScale = ordinaryScale;
        }

        /// <summary>Samples bounded local presentation values. Public for deterministic regression coverage.</summary>
        public void Sample(float clock, float deltaTime, Vector3 horizontalVelocity, CombatActionPhase phase)
        {
            SamplePose(clock, Time.unscaledTime, deltaTime, horizontalVelocity, phase);
        }

        /// <summary>Samples a pose with an explicit unscaled clock for deterministic presentation regression coverage.</summary>
        public void Sample(float clock, float unscaledClock, float deltaTime, Vector3 horizontalVelocity, CombatActionPhase phase)
        {
            SamplePose(clock, unscaledClock, deltaTime, horizontalVelocity, phase);
        }

        /// <summary>Samples a factual jump transition and its bounded local presentation response.</summary>
        public void Sample(float clock, float deltaTime, Vector3 horizontalVelocity, CombatActionPhase phase, bool isJumping, bool isGrounded, bool hasDirectControl)
        {
            ObserveJumpTransition(clock, isJumping, isGrounded, hasDirectControl);
            SamplePose(clock, Time.unscaledTime, deltaTime, horizontalVelocity, phase);
        }

        void SamplePose(float clock, float unscaledClock, float deltaTime, Vector3 horizontalVelocity, CombatActionPhase phase)
        {
            if (!presentationPivot || float.IsNaN(horizontalVelocity.x) || float.IsNaN(horizontalVelocity.z)) return;
            if (defeatActive)
            {
                ApplyDefeatPose(unscaledClock);
                return;
            }
            if (deltaTime <= 0) return;
            var targetMovement = Mathf.Clamp01(new Vector2(horizontalVelocity.x, horizontalVelocity.z).magnitude / 4f);
            movement = Mathf.MoveTowards(movement, targetMovement, deltaTime * 7f);
            var idleWeight = 1f - movement * .72f;
            var breath = Mathf.Sin(clock * 2.1f + seed) * .014f * idleWeight;
            var sway = Mathf.Sin(clock * 1.35f + seed) * .009f * idleWeight;
            var bob = Mathf.Sin(clock * (5.5f + movement * 2.5f) + seed) * .032f * movement;
            var localVelocity = transform.InverseTransformDirection(horizontalVelocity);
            var lean = Mathf.Clamp(-localVelocity.x * 2.1f, -7f, 7f) * movement;
            var pitch = Mathf.Clamp(localVelocity.z * 1.4f, -5f, 5f) * movement;
            if (phase == CombatActionPhase.Windup) pitch += 7f;
            else if (phase == CombatActionPhase.Impact) pitch -= 5f;
            else if (phase == CombatActionPhase.Recovery) pitch -= 2f;
            if (Time.time < hitReactionUntil) lean += 5f;

            var position = basePosition + new Vector3(sway, breath + bob, 0);
            var rotation = baseRotation * Quaternion.Euler(pitch, 0, lean);
            var scale = baseScale * (1f + breath * .12f);
            var takeoff = ResponseWeight(takeoffUntil, TakeoffDuration, clock);
            if (takeoff > 0)
            {
                position += Vector3.up * (.034f * takeoff);
                scale = Vector3.Scale(scale, Vector3.Lerp(Vector3.one, new Vector3(.965f, 1.075f, .965f), takeoff));
            }
            var landing = ResponseWeight(landingUntil, LandingDuration, clock);
            if (landing > 0)
            {
                position += Vector3.down * (.040f * landing);
                scale = Vector3.Scale(scale, Vector3.Lerp(Vector3.one, new Vector3(1.045f, .925f, 1.045f), landing));
            }
            ordinaryPosition = ClampOffset(position);
            ordinaryRotation = rotation;
            ordinaryScale = ClampScale(scale);
            hasOrdinaryPose = true;
            position = ordinaryPosition;
            scale = ordinaryScale;
            var arrivalWasActive = possessionArrivalActive;
            var arrival = PossessionArrivalPose(unscaledClock);
            var arrivalSettledThisSample = arrivalWasActive && !possessionArrivalActive;
            position += Vector3.up * arrival.y;
            scale = Vector3.Scale(scale, arrival.scale);
            position = ClampOffset(position);
            scale = ClampScale(scale);
            presentationPivot.localPosition = arrivalSettledThisSample ? position : Vector3.Lerp(presentationPivot.localPosition, position, Mathf.Clamp01(deltaTime * 12f));
            presentationPivot.localRotation = arrivalSettledThisSample ? rotation : Quaternion.Slerp(presentationPivot.localRotation, rotation, Mathf.Clamp01(deltaTime * 14f));
            presentationPivot.localScale = scale;
        }

        void ApplyDefeatPose(float unscaledClock)
        {
            var progress = Mathf.Clamp01((unscaledClock - defeatStartedAt) / DefeatSettleDuration);
            var weight = Mathf.SmoothStep(0, 1, progress);
            var defeatedPosition = ClampOffset(basePosition + Vector3.down * .055f);
            var defeatedRotation = baseRotation * Quaternion.Euler(52f, 0, -8f);
            var defeatedScale = ClampScale(Vector3.Scale(baseScale, new Vector3(1.065f, .90f, 1.065f)));
            presentationPivot.localPosition = Vector3.Lerp(basePosition, defeatedPosition, weight);
            presentationPivot.localRotation = Quaternion.Slerp(baseRotation, defeatedRotation, weight);
            presentationPivot.localScale = Vector3.Lerp(baseScale, defeatedScale, weight);
            defeatSettled = unscaledClock >= defeatUntil;
        }

        (Vector3 scale, float y) PossessionArrivalPose(float unscaledClock)
        {
            if (!possessionArrivalActive) return (Vector3.one, 0);
            if (unscaledClock >= possessionArrivalUntil)
            {
                possessionArrivalActive = false;
                possessionArrivalStartedAt = 0;
                possessionArrivalUntil = 0;
                return (Vector3.one, 0);
            }

            var progress = Mathf.Clamp01((unscaledClock - possessionArrivalStartedAt) / PossessionArrivalDuration);
            if (progress < .32f)
            {
                var weight = Mathf.SmoothStep(0, 1, progress / .32f);
                return (Vector3.Lerp(Vector3.one, new Vector3(1.055f, .925f, 1.055f), weight), -.018f * weight);
            }
            if (progress < .72f)
            {
                var weight = Mathf.SmoothStep(0, 1, (progress - .32f) / .40f);
                return (Vector3.Lerp(new Vector3(1.055f, .925f, 1.055f), new Vector3(.985f, 1.025f, .985f), weight), Mathf.Lerp(-.018f, .009f, weight));
            }
            var settle = Mathf.SmoothStep(0, 1, (progress - .72f) / .28f);
            return (Vector3.Lerp(new Vector3(.985f, 1.025f, .985f), Vector3.one, settle), Mathf.Lerp(.009f, 0, settle));
        }

        void ObserveJumpTransition(float clock, bool isJumping, bool isGrounded, bool hasDirectControl)
        {
            if (!hasDirectControl)
            {
                ResetJumpTransitions();
                return;
            }

            if (!jumpStateObserved)
            {
                jumpStateObserved = true;
                observedJumping = isJumping;
                return;
            }

            if (!observedJumping && isJumping)
            {
                takeoffUntil = clock + TakeoffDuration;
                landingUntil = 0;
            }
            else if (observedJumping && !isJumping)
            {
                takeoffUntil = 0;
                landingUntil = isGrounded ? clock + LandingDuration : 0;
            }

            observedJumping = isJumping;
        }

        static float ResponseWeight(float until, float duration, float clock)
        {
            if (until <= clock || duration <= 0) return 0;
            return Mathf.Clamp01((until - clock) / duration);
        }

        Vector3 ClampOffset(Vector3 position)
        {
            var offset = position - basePosition;
            return offset.sqrMagnitude <= MaximumOffset * MaximumOffset ? position : basePosition + offset.normalized * MaximumOffset;
        }

        Vector3 ClampScale(Vector3 scale)
        {
            return new Vector3(
                ClampScaleAxis(scale.x, baseScale.x),
                ClampScaleAxis(scale.y, baseScale.y),
                ClampScaleAxis(scale.z, baseScale.z));
        }

        static float ClampScaleAxis(float value, float baseValue)
        {
            var minimum = baseValue * MinimumScaleFactor;
            var maximum = baseValue * MaximumScaleFactor;
            return Mathf.Clamp(value, Mathf.Min(minimum, maximum), Mathf.Max(minimum, maximum));
        }

        void ResetJumpTransitions()
        {
            takeoffUntil = 0;
            landingUntil = 0;
            jumpStateObserved = false;
            observedJumping = false;
        }

        bool HasFactualDirectJumpControl()
        {
            return entity && entity.Health != null && !entity.Health.IsDead && entity.Motor && entity.Motor.enabled &&
                   !entity.IsRooted && !GameplayInput.TerminalState && entity.ActiveController is PlayerController player &&
                   player.IsActive && player.isActiveAndEnabled;
        }

        void LateUpdate()
        {
            var deltaTime = Time.deltaTime;
            var displacement = transform.position - lastRootPosition;
            lastRootPosition = transform.position;
            var velocity = deltaTime > .0001f ? displacement / deltaTime : Vector3.zero;
            ObserveJumpTransition(Time.time, entity && entity.IsJumping, entity && entity.IsGrounded, HasFactualDirectJumpControl());
            SamplePose(Time.time, Time.unscaledTime, deltaTime, new Vector3(velocity.x, 0, velocity.z), entity ? entity.ActionPhase : CombatActionPhase.Idle);
        }

        void OnDisable() { ClearDefeat(); ClearPossessionArrival(); ClearTransientReaction(); }
        void OnDestroy() { ClearDefeat(); ClearPossessionArrival(); Restore(); }
    }
}
