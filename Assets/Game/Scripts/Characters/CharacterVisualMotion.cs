using RealmRaiders.Combat;
using UnityEngine;

namespace RealmRaiders.Characters
{
    /// <summary>Presentation-only motion for assembled character visuals. It never moves the CombatEntity root.</summary>
    [DisallowMultipleComponent]
    public sealed class CharacterVisualMotion : MonoBehaviour
    {
        Transform presentationPivot;
        CombatEntity entity;
        Vector3 basePosition;
        Quaternion baseRotation;
        Vector3 baseScale;
        Vector3 lastRootPosition;
        float movement;
        float hitReactionUntil;
        float seed;

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
        }

        public void ShowHitReaction() => hitReactionUntil = Mathf.Max(hitReactionUntil, Time.time + .12f);

        public void ClearTransientReaction()
        {
            hitReactionUntil = 0;
            Restore();
        }

        public void Restore()
        {
            if (!presentationPivot) return;
            presentationPivot.localPosition = basePosition;
            presentationPivot.localRotation = baseRotation;
            presentationPivot.localScale = baseScale;
            movement = 0;
        }

        /// <summary>Samples bounded local presentation values. Public for deterministic regression coverage.</summary>
        public void Sample(float clock, float deltaTime, Vector3 horizontalVelocity, CombatActionPhase phase)
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
            presentationPivot.localPosition = Vector3.Lerp(presentationPivot.localPosition, position, Mathf.Clamp01(deltaTime * 12f));
            presentationPivot.localRotation = Quaternion.Slerp(presentationPivot.localRotation, rotation, Mathf.Clamp01(deltaTime * 14f));
            presentationPivot.localScale = scale;
        }

        void LateUpdate()
        {
            var deltaTime = Time.deltaTime;
            var displacement = transform.position - lastRootPosition;
            lastRootPosition = transform.position;
            var velocity = deltaTime > .0001f ? displacement / deltaTime : Vector3.zero;
            Sample(Time.time, deltaTime, new Vector3(velocity.x, 0, velocity.z), entity ? entity.ActionPhase : CombatActionPhase.Idle);
        }

        void OnDisable() => ClearTransientReaction();
        void OnDestroy() => Restore();
    }
}
