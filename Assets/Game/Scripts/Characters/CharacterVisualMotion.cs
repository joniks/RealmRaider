using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using UnityEngine;

namespace RealmRaiders.Characters
{
    /// <summary>Presentation-only motion for assembled character visuals. It never moves the CombatEntity root.</summary>
    [DisallowMultipleComponent]
    public sealed class CharacterVisualMotion : MonoBehaviour
    {
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
        float lastRootYaw;
        float dynamicsClock = float.NaN;
        object dynamicsController;
        readonly CharacterMotionDynamics dynamics = new();
        CharacterJumpPresentationTimeline jumpPresentation;
        float hitReactionUntil;
        float seed;
        float possessionArrivalStartedAt;
        float possessionArrivalUntil;
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
            jumpPresentation = GetComponent<CharacterJumpPresentationTimeline>() ?? gameObject.AddComponent<CharacterJumpPresentationTimeline>();
            basePosition = presentationPivot.localPosition;
            baseRotation = presentationPivot.localRotation;
            baseScale = presentationPivot.localScale;
            ResetDynamics();
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
            ResetDynamics();
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
            ResetDynamics();
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
            SamplePose(clock, Time.unscaledTime, deltaTime, horizontalVelocity, phase, CharacterJumpPresentationSample.None);
        }

        /// <summary>Samples a pose with an explicit unscaled clock for deterministic presentation regression coverage.</summary>
        public void Sample(float clock, float unscaledClock, float deltaTime, Vector3 horizontalVelocity, CombatActionPhase phase)
        {
            SamplePose(clock, unscaledClock, deltaTime, horizontalVelocity, phase, CharacterJumpPresentationSample.None);
        }

        /// <summary>Samples a factual jump transition and its bounded local presentation response.</summary>
        public void Sample(float clock, float deltaTime, Vector3 horizontalVelocity, CombatActionPhase phase, bool isJumping, bool isGrounded, bool hasDirectControl)
        {
            SamplePose(clock, Time.unscaledTime, deltaTime, horizontalVelocity, phase,
                ObserveJumpPresentation(clock, isJumping, isGrounded, hasDirectControl));
        }

        void SamplePose(float clock, float unscaledClock, float deltaTime, Vector3 horizontalVelocity, CombatActionPhase phase, CharacterJumpPresentationSample jump, bool sampleDynamics = true)
        {
            if (!presentationPivot || float.IsNaN(horizontalVelocity.x) || float.IsNaN(horizontalVelocity.z)) return;
            if (defeatActive)
            {
                ApplyDefeatPose(unscaledClock);
                return;
            }
            if (deltaTime <= 0) return;
            if (sampleDynamics)
            {
                if (jump.Phase != CharacterJumpPresentationPhase.None || phase != CombatActionPhase.Idle) dynamics.Reset();
                else dynamics.Step(transform.InverseTransformDirection(horizontalVelocity * deltaTime), 0, deltaTime, entity && entity.Definition ? entity.Stats.MoveSpeed : 4f);
            }
            var locomotionVisible = phase == CombatActionPhase.Idle && jump.Phase == CharacterJumpPresentationPhase.None && Time.time >= hitReactionUntil;
            var movement = locomotionVisible ? dynamics.Speed : 0f;
            var idleWeight = 1f - movement * .72f;
            var breath = Mathf.Sin(clock * 2.1f + seed) * .014f * idleWeight;
            var sway = Mathf.Sin(clock * 1.35f + seed) * .009f * idleWeight;
            var bob = Mathf.Sin(dynamics.Phase * 2f) * .032f * movement;
            var lean = locomotionVisible ? dynamics.TurnLean : 0f;
            var pitch = locomotionVisible ? dynamics.ForwardSpeed * 2f + dynamics.WeightPitch : 0f;
            if (phase == CombatActionPhase.Windup) pitch += 7f;
            else if (phase == CombatActionPhase.Impact) pitch -= 5f;
            else if (phase == CombatActionPhase.Recovery) pitch -= 2f;
            if (Time.time < hitReactionUntil) lean += 5f;

            var position = basePosition + new Vector3(sway, breath + bob, 0);
            var rotation = baseRotation * Quaternion.Euler(pitch, 0, lean);
            // Breathing owns translation only; signed idle breath must not invert a small jump scale response.
            var scale = baseScale;
            var takeoffResponse = jump.Phase == CharacterJumpPresentationPhase.Takeoff ? jump.Progress :
                jump.Phase == CharacterJumpPresentationPhase.Falling ? 1f - jump.Progress : 0f;
            if (takeoffResponse > 0)
            {
                position += Vector3.up * (.034f * takeoffResponse);
                scale = Vector3.Scale(scale, Vector3.Lerp(Vector3.one, new Vector3(.965f, 1.075f, .965f), takeoffResponse));
            }
            if (jump.Phase == CharacterJumpPresentationPhase.Landing)
            {
                var landing = 1f - Mathf.Abs(jump.Progress * 2f - 1f);
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
            presentationPivot.localPosition = arrivalSettledThisSample ? position : Vector3.Lerp(presentationPivot.localPosition, position, 1f - Mathf.Exp(-deltaTime * 12f));
            presentationPivot.localRotation = arrivalSettledThisSample ? rotation : Quaternion.Slerp(presentationPivot.localRotation, rotation, 1f - Mathf.Exp(-deltaTime * 14f));
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

        CharacterJumpPresentationSample ObserveJumpPresentation(float unscaledClock, bool isJumping, bool isGrounded, bool hasDirectControl)
        {
            jumpPresentation ??= GetComponent<CharacterJumpPresentationTimeline>() ?? gameObject.AddComponent<CharacterJumpPresentationTimeline>();
            return jumpPresentation.Observe(unscaledClock, isJumping, isGrounded, hasDirectControl);
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
            jumpPresentation?.ResetTimeline();
        }

        /// <summary>Rebases the shared gait on lifecycle changes; never writes an authoritative transform.</summary>
        public void ResetDynamics()
        {
            dynamics.Reset();
            lastRootPosition = transform.position;
            lastRootYaw = transform.eulerAngles.y;
            dynamicsController = entity?.ActiveController;
            dynamicsClock = float.NaN;
        }

        public static bool HasMotionAuthority(CombatEntity target)
        {
            return target && target.Health != null && !target.Health.IsDead && target.Motor && target.Motor.enabled &&
                !target.IsRooted && !GameplayInput.TerminalState && target.ActiveController != null && target.ActiveController.IsActive &&
                (!(target.ActiveController is Behaviour controller) || (controller && controller.isActiveAndEnabled));
        }

        /// <summary>Both visual consumers share one sample per timestamp, independent of LateUpdate order.</summary>
        public CharacterMotionDynamics SampleFactualDynamics(float clock, float deltaTime, bool allowed, CharacterJumpPresentationSample jump)
        {
            if (!ReferenceEquals(dynamicsController, entity?.ActiveController)) ResetDynamics();
            if (!allowed || jump.Phase != CharacterJumpPresentationPhase.None || (entity && entity.ActionPhase != CombatActionPhase.Idle))
            {
                ResetDynamics();
                return dynamics;
            }
            if (dynamicsClock == clock) return dynamics;
            dynamicsClock = clock;
            var displacement = transform.position - lastRootPosition;
            var yaw = transform.eulerAngles.y;
            var yawDelta = Mathf.DeltaAngle(lastRootYaw, yaw);
            lastRootPosition = transform.position;
            lastRootYaw = yaw;
            displacement.y = 0;
            dynamics.Step(transform.InverseTransformDirection(displacement), yawDelta, deltaTime, entity && entity.Definition ? entity.Stats.MoveSpeed : 4f);
            return dynamics;
        }

        /// <summary>Explicit clocks keep the real transform-to-pivot path deterministically testable.</summary>
        public void SampleFactualPose(float clock, float unscaledClock, float deltaTime)
        {
            var jump = ObserveJumpPresentation(unscaledClock, entity && entity.IsJumping, entity && entity.IsGrounded, CharacterJumpPresentationTimeline.HasFactualDirectControl(entity));
            SampleFactualDynamics(unscaledClock, deltaTime, HasMotionAuthority(entity), jump);
            SamplePose(clock, unscaledClock, deltaTime, Vector3.zero, entity ? entity.ActionPhase : CombatActionPhase.Idle, jump, false);
        }

        void LateUpdate() => SampleFactualPose(Time.time, Time.unscaledTime, Time.deltaTime);

        void OnDisable() { ClearDefeat(); ClearPossessionArrival(); ClearTransientReaction(); }
        void OnDestroy() { ClearDefeat(); ClearPossessionArrival(); Restore(); }
    }
}
