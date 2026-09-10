using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using RealmRaiders.Modules.CharacterMotionProfiles;
using RealmRaiders.Modules.CharacterProceduralMotion;
using UnityEngine;

namespace RealmRaiders.Characters
{
    /// <summary>
    /// Core-owned bridge from factual combat state to the module's visual-only six-bone driver.
    /// It never changes the gameplay root, controller, presentation pivot, or base-body transform.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CharacterProceduralMotionAdapter : MonoBehaviour
    {
        const float HitResponseSeconds = .12f;

        static readonly HumanoidBoneNameMap BloodKnightBones = new(
            "Bip01 L UpperArm", "Bip01 R UpperArm", "Bip01 L Thigh",
            "Bip01 R Thigh", "Bip01 L Calf", "Bip01 R Calf");

        readonly ProceduralHumanoidPoseDriver driver = new(ProceduralHumanoidMotionTuning.BloodKnightDeviceReadable);
        CombatEntity entity;
        Health health;
        Transform baseBody;
        CharacterVisualMotion visualMotion;
        CharacterJumpPresentationTimeline jumpPresentation;
        float hitResponseUntil;
        bool subscribed;

        public bool IsBound => driver.IsBound;

        void Awake()
        {
            entity = GetComponent<CombatEntity>();
            health = GetComponent<Health>();
        }

        /// <summary>Binds the exact Blood Knight bones. Missing or duplicate names leave visuals untouched.</summary>
        public bool Bind(Transform visualBaseBody)
        {
            Clear();
            baseBody = visualBaseBody;
            if (!entity) entity = GetComponent<CombatEntity>();
            if (!health) health = GetComponent<Health>();
            if (!baseBody || !entity || !health || !driver.Bind(baseBody, BloodKnightBones))
            {
                baseBody = null;
                return false;
            }

            jumpPresentation = GetComponent<CharacterJumpPresentationTimeline>() ?? gameObject.AddComponent<CharacterJumpPresentationTimeline>();
            jumpPresentation.Configure(driver.Tuning.JumpPresentationDurationMultiplier, driver.Tuning.TakeoffStraightenDurationMultiplier);
            visualMotion = GetComponent<CharacterVisualMotion>();
            if (!visualMotion) { driver.Clear(); baseBody = null; return false; }
            visualMotion.ResetDynamics();
            health.Damaged += OnDamaged;
            subscribed = true;
            return true;
        }

        /// <summary>Restores cached bone transforms and removes all event ownership before hierarchy teardown.</summary>
        public void Clear()
        {
            Unsubscribe();
            driver.Clear();
            baseBody = null;
            hitResponseUntil = 0;
            if (visualMotion) visualMotion.ResetDynamics();
            jumpPresentation?.ResetTimeline();
        }

        void LateUpdate() => SamplePresentation(Time.unscaledTime, Time.time, Time.deltaTime, entity && entity.IsJumping, entity && entity.IsGrounded, CharacterJumpPresentationTimeline.HasFactualDirectControl(entity));

        /// <summary>Samples factual state at explicit clocks for deterministic presentation coverage; it has no gameplay authority.</summary>
        public void SamplePresentation(float unscaledClock, float presentationClock, float deltaTime, bool isJumping, bool isGrounded, bool hasDirectControl)
        {
            if (!driver.IsBound || !entity || !health || !visualMotion) return;

            jumpPresentation ??= GetComponent<CharacterJumpPresentationTimeline>() ?? gameObject.AddComponent<CharacterJumpPresentationTimeline>();
            var jump = jumpPresentation.Observe(unscaledClock, isJumping, isGrounded, hasDirectControl);
            var dynamics = visualMotion.SampleFactualDynamics(unscaledClock, deltaTime,
                (hasDirectControl || CharacterVisualMotion.HasMotionAuthority(entity)) && !health.IsDead && !GameplayInput.TerminalState, jump);

            var input = new CharacterMotionPresentationInput(
                health.IsDead ? MotionPresentationReaction.Death : unscaledClock < hitResponseUntil ? MotionPresentationReaction.Hit : MotionPresentationReaction.None,
                entity.ActionPhase == CombatActionPhase.Idle ? MotionPresentationAttack.None : MotionPresentationAttack.Primary,
                ResolveJumpPhase(jump.Phase),
                dynamics.Speed > 0);
            // The module evaluates sin((clock + dt) * cadence). Supply distance phase and zero time advance.
            var gaitClock = dynamics.Speed > 0 ? dynamics.Phase / driver.Tuning.SwingCadenceRadiansPerSecond : 0f;
            driver.Sample(input, dynamics.Speed, gaitClock, 0f, jump.Progress);
        }

        static MotionPresentationJumpPhase ResolveJumpPhase(CharacterJumpPresentationPhase phase)
        {
            return phase == CharacterJumpPresentationPhase.Takeoff ? MotionPresentationJumpPhase.Takeoff :
                phase == CharacterJumpPresentationPhase.Falling ? MotionPresentationJumpPhase.Falling :
                phase == CharacterJumpPresentationPhase.Landing ? MotionPresentationJumpPhase.Landing : MotionPresentationJumpPhase.None;
        }

        void OnDamaged(DamageInfo _)
        {
            if (health && !health.IsDead) hitResponseUntil = Mathf.Max(hitResponseUntil, Time.unscaledTime + HitResponseSeconds);
        }

        void Unsubscribe()
        {
            if (subscribed && health) health.Damaged -= OnDamaged;
            subscribed = false;
        }

        void OnDisable()
        {
            Unsubscribe();
            driver.Clear();
            hitResponseUntil = 0;
            if (visualMotion) visualMotion.ResetDynamics();
            jumpPresentation?.ResetTimeline();
        }

        void OnEnable()
        {
            if (baseBody && !driver.IsBound) Bind(baseBody);
        }

        void OnDestroy() => Clear();
    }
}
