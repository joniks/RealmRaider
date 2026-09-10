using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using RealmRaiders.Modules.CharacterMotionProfiles;
using RealmRaiders.Modules.CharacterProceduralMotion;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace RealmRaiders.Characters
{
    public enum LargeCreatureVisualState { Idle, Move, Attack, Hit, Death }

    /// <summary>Consumes passive combat facts. Owns only a manual visual graph and bones below Base Body.</summary>
    [DisallowMultipleComponent]
    public sealed class LargeCreatureMotionAdapter : MonoBehaviour
    {
        LargeCreatureMotionBinding binding;
        Transform body;
        Animator animator;
        CombatEntity entity;
        Health health;
        CharacterVisualMotion motion;
        readonly CharacterCombatPresentationTimeline combat = new();
        PlayableGraph graph;
        AnimationClipPlayable clipPlayer;
        AnimationPlayableOutput output;
        AnimationClip currentClip;
        Transform[] bones;
        Vector3[] positions, scales;
        Quaternion[] rotations;
        object controller;
        bool subscribed, dead, deathHeld;
        float deathStarted, stateStarted;
        public bool IsBound => isActiveAndEnabled && binding && body && animator && bones != null;
        public bool HasGraph => graph.IsValid();
        public bool IsDeathHeld => deathHeld;
        public LargeCreatureVisualState State { get; private set; }
        public float ClipTime { get; private set; }

        public bool Bind(Transform visualBaseBody, LargeCreatureMotionBinding data)
        {
            Clear();
            entity = GetComponent<CombatEntity>(); health = GetComponent<Health>(); motion = GetComponent<CharacterVisualMotion>();
            var assembler = GetComponent<CharacterVisualAssembler>();
            if (!entity || !health || !motion || !assembler || !visualBaseBody || visualBaseBody.parent != assembler.PresentationPivot ||
                motion.PresentationPivot != assembler.PresentationPivot || !data || !data.IsValid || !data.ValidateHierarchy(visualBaseBody)) return false;
            binding = data; body = visualBaseBody; animator = body.Find(data.AnimatorPath).GetComponent<Animator>();
            animator.applyRootMotion = false;
            animator.fireEvents = false; // Runtime-only Animator flag: enforce for every bind/rebuild before sampling.
            bones = new Transform[data.AnimatedBonePaths.Length]; positions = new Vector3[bones.Length];
            rotations = new Quaternion[bones.Length]; scales = new Vector3[bones.Length];
            for (var i = 0; i < bones.Length; i++)
            {
                bones[i] = animator.transform.Find(data.AnimatedBonePaths[i]);
                positions[i] = bones[i].localPosition; rotations[i] = bones[i].localRotation; scales[i] = bones[i].localScale;
            }
            controller = entity.ActiveController; stateStarted = Time.unscaledTime;
            Subscribe();
            Sample(Time.time, Time.unscaledTime, 0);
            return true;
        }

        void Subscribe()
        {
            if (subscribed) return;
            entity.PresentationChanged += OnAction; health.Damaged += OnDamage; subscribed = true;
        }
        void Unsubscribe()
        {
            if (!subscribed) return;
            if (entity) entity.PresentationChanged -= OnAction;
            if (health) health.Damaged -= OnDamage;
            subscribed = false;
        }
        void OnAction(CombatPresentationFact fact)
        {
            if (!IsBound) return;
            if (fact.End == CombatPresentationEnd.Death) { BeginDeath(fact.UnscaledTime); return; }
            if (fact.End != CombatPresentationEnd.None && fact.End != CombatPresentationEnd.Completed)
            {
                // A terminal callback after death must not erase the held defeat pose.
                if (dead && (fact.End == CombatPresentationEnd.Terminal || fact.End == CombatPresentationEnd.ControllerChanged)) return;
                ResetTransient();
                if (fact.End == CombatPresentationEnd.Disabled || fact.End == CombatPresentationEnd.Destroyed || fact.End == CombatPresentationEnd.Terminal) Unsubscribe();
                return;
            }
            if (!health.IsDead && !GameplayInput.TerminalState && entity.isActiveAndEnabled)
            { SynchronizeController(); combat.OnAction(fact); }
        }
        void OnDamage(DamageInfo hit)
        {
            if (!IsBound || health.IsDead || GameplayInput.TerminalState || !entity.isActiveAndEnabled || hit.Amount <= 0) return;
            SynchronizeController();
            combat.OnHit(Time.unscaledTime, 0);
            // The existing bounded pivot reaction remains the only hit response; no source Hit clip exists.
            motion.ShowHitReaction();
        }
        void BeginDeath(float clock)
        {
            if (dead) return;
            combat.Clear(); health.Damaged -= OnDamage; DestroyGraph(); RestoreBones();
            dead = true; deathHeld = false; deathStarted = clock; State = LargeCreatureVisualState.Death;
        }

        public static LargeCreatureVisualState ResolveState(bool isDead, bool hit, bool attack, bool moving)
        {
            var key = CharacterMotionPresentationResolver.Resolve(new CharacterMotionPresentationInput(
                isDead ? MotionPresentationReaction.Death : hit ? MotionPresentationReaction.Hit : MotionPresentationReaction.None,
                attack ? MotionPresentationAttack.Primary : MotionPresentationAttack.None, MotionPresentationJumpPhase.None, moving));
            return key == MotionClipKey.Death ? LargeCreatureVisualState.Death : key == MotionClipKey.Hit ? LargeCreatureVisualState.Hit :
                key == MotionClipKey.AttackPrimary ? LargeCreatureVisualState.Attack : key == MotionClipKey.Locomotion ? LargeCreatureVisualState.Move : LargeCreatureVisualState.Idle;
        }

        /// <summary>Explicit clocks sample actual root displacement and captured action facts, never movement intent.</summary>
        public void Sample(float scaledClock, float unscaledClock, float deltaTime)
        {
            if (!IsBound || !isActiveAndEnabled) return;
            if (!CharacterCombatPresentationTimeline.Finite(scaledClock) || !CharacterCombatPresentationTimeline.Finite(unscaledClock)) { ResetTransient(); return; }
            if (!entity.isActiveAndEnabled) { Unsubscribe(); ResetTransient(); return; }
            if (health.IsDead) BeginDeath(unscaledClock);
            if (dead)
            {
                if (deathHeld) return;
                Play(binding.Death, Mathf.Clamp(unscaledClock - deathStarted, 0, binding.Death.length));
                if (ClipTime >= binding.Death.length) { deathHeld = true; DestroyGraph(); Unsubscribe(); }
                return;
            }
            SynchronizeController();
            if (GameplayInput.TerminalState) { Unsubscribe(); ResetTransient(); return; }
            if (!subscribed) Subscribe();
            var pose = combat.Observe(scaledClock, unscaledClock);
            var dynamics = motion.SampleFactualDynamics(unscaledClock, deltaTime, CharacterVisualMotion.HasMotionAuthority(entity), CharacterJumpPresentationSample.None);
            var next = ResolveState(false, combat.HasHit, combat.HasAttack, dynamics.Speed > .02f);
            if (next != State) { State = next; stateStarted = unscaledClock; }
            if (State == LargeCreatureVisualState.Hit) return; // Freeze owned skeleton; bounded pivot hit wins over attack/locomotion.
            if (State == LargeCreatureVisualState.Attack)
            {
                var progress = pose.AttackStage == ProceduralHumanoidAttackStage.Windup ? pose.AttackProgress * .35f :
                    pose.AttackStage == ProceduralHumanoidAttackStage.Impact ? .35f + pose.AttackProgress * .30f : .65f + pose.AttackProgress * .35f;
                Play(binding.Attack, Mathf.Clamp01(progress) * binding.Attack.length);
            }
            else
            {
                var clip = State == LargeCreatureVisualState.Move ? binding.Run : binding.Idle;
                Play(clip, Mathf.Repeat(Mathf.Max(0, unscaledClock - stateStarted), clip.length));
            }
        }

        void Play(AnimationClip clip, float time)
        {
            if (!graph.IsValid())
            {
                graph = PlayableGraph.Create("LargeCreature visual motion"); graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
                output = AnimationPlayableOutput.Create(graph, "Skeleton only", animator);
                animator.applyRootMotion = false; animator.fireEvents = false;
            }
            if (currentClip != clip)
            {
                if (clipPlayer.IsValid()) graph.DestroyPlayable(clipPlayer);
                clipPlayer = AnimationClipPlayable.Create(graph, clip); clipPlayer.SetApplyFootIK(false); clipPlayer.SetApplyPlayableIK(false);
                clipPlayer.SetSpeed(0); output.SetSourcePlayable(clipPlayer); currentClip = clip;
            }
            ClipTime = time; clipPlayer.SetTime(time);
            if (!graph.IsPlaying()) graph.Play();
            graph.Evaluate(0);
        }
        void DestroyGraph()
        {
            if (graph.IsValid()) graph.Destroy();
            graph = default; clipPlayer = default; output = default; currentClip = null;
        }
        void RestoreBones()
        {
            if (bones == null) return;
            for (var i = 0; i < bones.Length; i++) if (bones[i])
            { bones[i].localPosition = positions[i]; bones[i].localRotation = rotations[i]; bones[i].localScale = scales[i]; }
        }
        void ResetTransient()
        {
            DestroyGraph(); RestoreBones(); combat.Clear(); dead = deathHeld = false;
            State = LargeCreatureVisualState.Idle; ClipTime = 0; stateStarted = Time.unscaledTime;
            if (motion) motion.ResetDynamics();
        }
        void SynchronizeController()
        { if (!ReferenceEquals(controller, entity.ActiveController)) { ResetTransient(); controller = entity.ActiveController; } }
        public void Clear()
        {
            Unsubscribe(); ResetTransient(); binding = null; body = null; animator = null;
            bones = null; positions = scales = null; rotations = null; controller = null;
        }
        void LateUpdate() => Sample(Time.time, Time.unscaledTime, Time.deltaTime);
        void OnDisable() { Unsubscribe(); ResetTransient(); }
        void OnDestroy() => Clear();
    }
}
