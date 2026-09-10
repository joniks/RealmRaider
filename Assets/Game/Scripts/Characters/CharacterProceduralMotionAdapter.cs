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
        public const float CombatCrossfadeSeconds = .04f;

        static readonly HumanoidBoneNameMap BloodKnightBones = new(
            "Bip01 L UpperArm", "Bip01 R UpperArm", "Bip01 L Thigh",
            "Bip01 R Thigh", "Bip01 L Calf", "Bip01 R Calf");

        readonly ProceduralHumanoidPoseDriver driver = new(ProceduralHumanoidMotionTuning.BloodKnightDeviceReadable);
        CombatEntity entity;
        Health health;
        Transform baseBody;
        Transform orientationReference;
        CharacterVisualMotion visualMotion;
        CharacterJumpPresentationTimeline jumpPresentation;
        readonly CharacterCombatPresentationTimeline combat = new();
        readonly Transform[] bones = new Transform[6];
        readonly Quaternion[] lastPose = new Quaternion[6];
        readonly Quaternion[] blendFrom = new Quaternion[6];
        object sampledController;
        float blendStarted;
        bool blending, hasPose;
        int lastPriority;
        long lastActionId;
        bool subscribed;

        public bool IsBound => driver.IsBound;
        public bool HasCombatPresentation => combat.HasHit || combat.HasAttack;

        void Awake()
        {
            entity = GetComponent<CombatEntity>();
            health = GetComponent<Health>();
        }

        /// <summary>Binds the exact Blood Knight bones. Missing or duplicate names leave visuals untouched.</summary>
        public bool Bind(Transform visualBaseBody, Transform presentationPivot)
        {
            Clear();
            if (!entity) entity = GetComponent<CombatEntity>();
            if (!health) health = GetComponent<Health>();
            visualMotion = GetComponent<CharacterVisualMotion>();
            var assembler = GetComponent<CharacterVisualAssembler>();
            if (!visualBaseBody || !presentationPivot || !entity || !health || !visualMotion || !assembler ||
                assembler.PresentationPivot != presentationPivot || visualMotion.PresentationPivot != presentationPivot ||
                !presentationPivot.IsChildOf(transform) || visualBaseBody.parent != presentationPivot ||
                !driver.Bind(visualBaseBody, presentationPivot, BloodKnightBones, ProceduralHumanoidAxisPolicy.CharacterSagittalPlane))
            {
                Clear();
                return false;
            }
            baseBody = visualBaseBody;
            orientationReference = presentationPivot;
            CacheBones();
            jumpPresentation = GetComponent<CharacterJumpPresentationTimeline>() ?? gameObject.AddComponent<CharacterJumpPresentationTimeline>();
            jumpPresentation.Configure(driver.Tuning.JumpPresentationDurationMultiplier, driver.Tuning.TakeoffStraightenDurationMultiplier);
            visualMotion.ResetDynamics();
            health.Damaged += OnDamaged;
            entity.PresentationChanged += OnAction;
            sampledController = entity.ActiveController;
            subscribed = true;
            return true;
        }

        /// <summary>Restores cached bone transforms and removes all event ownership before hierarchy teardown.</summary>
        public void Clear()
        {
            Unsubscribe();
            driver.Clear();
            baseBody = null;
            orientationReference = null;
            sampledController = null;
            ClearCombat();
            for (var i = 0; i < bones.Length; i++) bones[i] = null;
            if (visualMotion) visualMotion.ResetDynamics();
            jumpPresentation?.ResetTimeline();
        }

        void LateUpdate() => SamplePresentation(Time.unscaledTime, Time.time, Time.deltaTime, entity && entity.IsJumping, entity && entity.IsGrounded, CharacterJumpPresentationTimeline.HasFactualDirectControl(entity));

        /// <summary>Samples factual state at explicit clocks for deterministic presentation coverage; it has no gameplay authority.</summary>
        public void SamplePresentation(float unscaledClock, float presentationClock, float deltaTime, bool isJumping, bool isGrounded, bool hasDirectControl)
        {
            if (!driver.IsBound || !entity || !health || !visualMotion) return;
            if (!baseBody || !orientationReference) { Clear(); return; }
            if (!CharacterCombatPresentationTimeline.Finite(unscaledClock) || !CharacterCombatPresentationTimeline.Finite(presentationClock)) { ClearCombat(); return; }

            var sample = ObserveCombat(presentationClock, unscaledClock);

            jumpPresentation ??= GetComponent<CharacterJumpPresentationTimeline>() ?? gameObject.AddComponent<CharacterJumpPresentationTimeline>();
            var jump = jumpPresentation.Observe(unscaledClock, isJumping, isGrounded, hasDirectControl);
            var dynamics = visualMotion.SampleFactualDynamics(unscaledClock, deltaTime,
                (hasDirectControl || CharacterVisualMotion.HasMotionAuthority(entity)) && !health.IsDead && !GameplayInput.TerminalState, jump);

            var input = new CharacterMotionPresentationInput(
                health.IsDead ? MotionPresentationReaction.Death : combat.HasHit ? MotionPresentationReaction.Hit : MotionPresentationReaction.None,
                combat.HasAttack ? MotionPresentationAttack.Primary : MotionPresentationAttack.None,
                ResolveJumpPhase(jump.Phase),
                dynamics.Speed > 0);
            // The module evaluates sin((clock + dt) * cadence). Supply distance phase and zero time advance.
            var gaitClock = dynamics.Speed > 0 ? dynamics.Phase / driver.Tuning.SwingCadenceRadiansPerSecond : 0f;
            var priority = health.IsDead ? 3 : combat.HasHit ? 2 : combat.HasAttack ? 1 : 0;
            if (priority != 3 && hasPose && (priority != lastPriority && (priority > 0 || lastPriority > 0) || priority == 1 && lastActionId != combat.ActionId))
            {
                for (var i = 0; i < bones.Length; i++) blendFrom[i] = lastPose[i];
                blendStarted = priority == 2 ? combat.HitStartedAt : lastPriority == 2 ? combat.HitEndsAt :
                    priority == 1 ? combat.AttackStartedAt : combat.AttackEndsAt;
                blending = true;
            }
            lastPriority = priority; lastActionId = combat.ActionId;
            driver.Sample(input, dynamics.Speed, gaitClock, 0f, jump.Progress, sample);
            var blend = Mathf.SmoothStep(0, 1, (unscaledClock - blendStarted) / CombatCrossfadeSeconds);
            for (var i = 0; i < bones.Length; i++)
            {
                if (blending && priority != 3) bones[i].localRotation = Quaternion.Slerp(blendFrom[i], bones[i].localRotation, blend);
                lastPose[i] = bones[i].localRotation;
            }
            if (blend >= 1 || priority == 3) blending = false;
            hasPose = true;
        }

        /// <summary>Shared pure-clock combat sample; pivot and bones consume the same event snapshot.</summary>
        public ProceduralHumanoidCombatPoseSample ObserveCombat(float scaledClock, float unscaledClock)
        {
            if (!driver.IsBound || !entity || !health) return default;
            if (!ReferenceEquals(sampledController, entity.ActiveController) || !entity.isActiveAndEnabled ||
                (entity.ActiveController != null && !entity.ActiveController.IsActive) || GameplayInput.TerminalState || health.IsDead)
            {
                ClearCombat(); sampledController = entity.ActiveController;
            }
            return combat.Observe(scaledClock, unscaledClock);
        }

        static MotionPresentationJumpPhase ResolveJumpPhase(CharacterJumpPresentationPhase phase)
        {
            return phase == CharacterJumpPresentationPhase.Takeoff ? MotionPresentationJumpPhase.Takeoff :
                phase == CharacterJumpPresentationPhase.Falling ? MotionPresentationJumpPhase.Falling :
                phase == CharacterJumpPresentationPhase.Landing ? MotionPresentationJumpPhase.Landing : MotionPresentationJumpPhase.None;
        }

        void OnDamaged(DamageInfo hit)
        {
            if (!health || health.IsDead || !entity.isActiveAndEnabled || GameplayInput.TerminalState) { ClearCombat(); return; }
            if (hit.Amount <= 0 || !CharacterCombatPresentationTimeline.Finite(hit.Amount)) return;
            SynchronizeController();
            combat.OnHit(Time.unscaledTime, CharacterCombatPresentationTimeline.RecoilDirection(hit, transform.position, transform.rotation));
        }

        void OnAction(CombatPresentationFact fact)
        {
            if (GameplayInput.TerminalState || !health || health.IsDead || !entity.isActiveAndEnabled || fact.End != CombatPresentationEnd.None && fact.End != CombatPresentationEnd.Completed)
            {
                ClearCombat();
                visualMotion?.ClearTransientReaction();
                if (driver.IsBound) driver.Sample(new CharacterMotionPresentationInput(health && health.IsDead ? MotionPresentationReaction.Death : MotionPresentationReaction.None,
                    MotionPresentationAttack.None, MotionPresentationJumpPhase.None, false), 0, 0, 0, 0, default);
            }
            else { SynchronizeController(); combat.OnAction(fact); }
        }

        void SynchronizeController()
        {
            if (!ReferenceEquals(sampledController, entity.ActiveController)) { ClearCombat(); sampledController = entity.ActiveController; }
        }

        void ClearCombat()
        {
            combat.Clear(); blending = hasPose = false; lastPriority = 0; lastActionId = 0;
        }

        void CacheBones()
        {
            foreach (var candidate in baseBody.GetComponentsInChildren<Transform>(true))
            {
                var index = candidate.name switch
                {
                    "Bip01 L UpperArm" => 0, "Bip01 R UpperArm" => 1, "Bip01 L Thigh" => 2,
                    "Bip01 R Thigh" => 3, "Bip01 L Calf" => 4, "Bip01 R Calf" => 5, _ => -1
                };
                if (candidate != baseBody && index >= 0) bones[index] = candidate;
            }
        }

        void Unsubscribe()
        {
            if (subscribed && health) health.Damaged -= OnDamaged;
            if (subscribed && entity) entity.PresentationChanged -= OnAction;
            subscribed = false;
        }

        void OnDisable()
        {
            Unsubscribe();
            driver.Clear();
            ClearCombat();
            sampledController = null;
            if (visualMotion) { visualMotion.ClearTransientReaction(); visualMotion.ResetDynamics(); }
            jumpPresentation?.ResetTimeline();
        }

        void OnEnable()
        {
            if (baseBody && orientationReference && !driver.IsBound) Bind(baseBody, orientationReference);
        }

        void OnDestroy() => Clear();
    }
}
