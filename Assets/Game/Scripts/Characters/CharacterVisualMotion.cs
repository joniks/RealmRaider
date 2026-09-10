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
        bool jumpStateObserved;
        bool observedJumping;

        public Transform PresentationPivot => presentationPivot;
        public Vector3 BasePosition => basePosition;
        public Quaternion BaseRotation => baseRotation;
        public Vector3 BaseScale => baseScale;

        void Awake()
        {
            entity = GetComponent<CombatEntity>();
            seed = Mathf.Abs(GetEntityId().GetHashCode() % 997) * .013f;
            lastRootPosition = transform.position;
        }

        public void Bind(Transform pivot)
        {
            Restore();
            presentationPivot = pivot;
            if (!presentationPivot) return;
            basePosition = presentationPivot.localPosition;
            baseRotation = presentationPivot.localRotation;
            baseScale = presentationPivot.localScale;
            movement = 0;
            lastRootPosition = transform.position;
            ResetJumpTransitions();
        }

        public void ShowHitReaction() => hitReactionUntil = Mathf.Max(hitReactionUntil, Time.time + .12f);

        public void ClearTransientReaction()
        {
            hitReactionUntil = 0;
            Restore();
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

        /// <summary>Samples bounded local presentation values. Public for deterministic regression coverage.</summary>
        public void Sample(float clock, float deltaTime, Vector3 horizontalVelocity, CombatActionPhase phase)
        {
            SamplePose(clock, deltaTime, horizontalVelocity, phase);
        }

        /// <summary>Samples a factual jump transition and its bounded local presentation response.</summary>
        public void Sample(float clock, float deltaTime, Vector3 horizontalVelocity, CombatActionPhase phase, bool isJumping, bool isGrounded, bool hasDirectControl)
        {
            ObserveJumpTransition(clock, isJumping, isGrounded, hasDirectControl);
            SamplePose(clock, deltaTime, horizontalVelocity, phase);
        }

        void SamplePose(float clock, float deltaTime, Vector3 horizontalVelocity, CombatActionPhase phase)
        {
            if (!presentationPivot || deltaTime <= 0 || float.IsNaN(horizontalVelocity.x) || float.IsNaN(horizontalVelocity.z)) return;
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
            position = ClampOffset(position);
            scale = ClampScale(scale);
            presentationPivot.localPosition = Vector3.Lerp(presentationPivot.localPosition, position, Mathf.Clamp01(deltaTime * 12f));
            presentationPivot.localRotation = Quaternion.Slerp(presentationPivot.localRotation, rotation, Mathf.Clamp01(deltaTime * 14f));
            presentationPivot.localScale = scale;
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
            Sample(Time.time, deltaTime, new Vector3(velocity.x, 0, velocity.z), entity ? entity.ActionPhase : CombatActionPhase.Idle,
                entity && entity.IsJumping, entity && entity.IsGrounded, HasFactualDirectJumpControl());
        }

        void OnDisable() => ClearTransientReaction();
        void OnDestroy() => Restore();
    }
}
