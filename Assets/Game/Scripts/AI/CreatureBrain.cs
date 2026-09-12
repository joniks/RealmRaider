using System;
using System.Collections.Generic;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using RealmRaiders.Modules.LargeCreatureCombatRhythm;
using UnityEngine;

namespace RealmRaiders.AI
{
    public enum BrainState { Idle, Detect, Chase, Attack, Return }

    [RequireComponent(typeof(CombatEntity))]
    public sealed class CreatureBrain : MonoBehaviour, IEntityController
    {
        static readonly List<CreatureBrain> activeBrains = new();

        /// <summary>Presentation observers can react to a real AI target/state change without polling the whole scene.</summary>
        public static event Action<CreatureBrain> HostileIntentChanged;
        public static IReadOnlyList<CreatureBrain> ActiveBrains => activeBrains;

        public bool IsActive { get; private set; }
        public BrainState State { get; private set; }
        CombatEntity target;
        public CombatEntity Target
        {
            get => target;
            set
            {
                if (target == value) return;
                target = value;
                ResetHeavyAttackRhythm();
                PublishIntent();
            }
        }
        public float DetectionRange = 11;
        Vector3 home;
        CombatEntity entity;
        Health health;
        LargeCreatureCombatRhythmRecipe heavyAttackRhythm;
        readonly List<CombatEntity> explicitRhythmTargets = new();
        int consecutiveBasicCount;

        public int ConsecutiveBasicCount => consecutiveBasicCount;
        public bool UsesHeavyAttackRhythm => heavyAttackRhythm != null;

        void Awake()
        {
            entity = GetComponent<CombatEntity>();
            health = GetComponent<Health>();
            if (health) health.Died += OnEntityDied;
            home = transform.position;
            activeBrains.Add(this);
        }

        void OnDisable() => ResetHeavyAttackRhythm();

        void OnDestroy()
        {
            if (health) health.Died -= OnEntityDied;
            ResetHeavyAttackRhythm();
            activeBrains.Remove(this);
        }

        public void SetControl(bool active)
        {
            IsActive = active;
            if (!active) ResetHeavyAttackRhythm();
            SetState(BrainState.Idle);
            PublishIntent();
        }

        void ConfigureHeavyAttackRhythm(
            LargeCreatureCombatRhythmRecipe recipe,
            IReadOnlyList<CombatEntity> explicitEligibleTargets)
        {
            heavyAttackRhythm = recipe;
            explicitRhythmTargets.Clear();
            if (explicitEligibleTargets != null)
                for (var index = 0; index < explicitEligibleTargets.Count; index++)
                    if (explicitEligibleTargets[index]) explicitRhythmTargets.Add(explicitEligibleTargets[index]);
            ResetHeavyAttackRhythm();
        }

        public void ConfigureGuardianEntHeavyAttackRhythm(IReadOnlyList<CombatEntity> explicitEligibleTargets) =>
            ConfigureHeavyAttackRhythm(GuardianEntCombatRhythmRecipes.GuardianEnt, explicitEligibleTargets);

        public void Tick()
        {
            if (!Target || Target.Health.IsDead || Vector3.Distance(home, transform.position) > 18)
            { ResetHeavyAttackRhythm(); SetState(BrainState.Return); ReturnHome(); return; }
            var delta = Target.transform.position - transform.position; delta.y = 0;
            if (delta.magnitude > DetectionRange) { ResetHeavyAttackRhythm(); SetState(BrainState.Idle); entity.Move(Vector3.zero); }
            else if (delta.magnitude > 3.1f) { SetState(BrainState.Chase); entity.Move(delta.normalized * entity.Stats.MoveSpeed); }
            else { SetState(BrainState.Attack); entity.Move(Vector3.zero); TryAttack(delta); }
        }

        void TryAttack(Vector3 direction)
        {
            if (heavyAttackRhythm == null)
            {
                entity.TryUse(0, direction);
                return;
            }

            var eligibleTargetCount = CountEligibleRhythmTargets();
            var decision = HeavyAttackRhythmEvaluator.Evaluate(
                new HeavyAttackRhythmInput(heavyAttackRhythm, eligibleTargetCount, consecutiveBasicCount));
            var abilityIndex = decision.Action switch
            {
                HeavyAttackRhythmAction.Basic => 0,
                HeavyAttackRhythmAction.Area => 2,
                _ => -1
            };
            if (abilityIndex < 0 || !entity.TryUse(abilityIndex, direction)) return;
            if (decision.Action == HeavyAttackRhythmAction.Basic) consecutiveBasicCount++;
            else if (decision.Action == HeavyAttackRhythmAction.Area) consecutiveBasicCount = 0;
        }

        int CountEligibleRhythmTargets()
        {
            if (entity.Abilities.Count <= (int)LargeCreatureAbilitySlot.Area) return 0;
            var radius = entity.Abilities[(int)LargeCreatureAbilitySlot.Area].Definition.Radius;
            var count = 0;
            for (var index = 0; index < explicitRhythmTargets.Count; index++)
            {
                var candidate = explicitRhythmTargets[index];
                if (!candidate || candidate.Health == null || candidate.Health.IsDead ||
                    Vector3.Distance(transform.position, candidate.transform.position) > radius) continue;
                var duplicate = false;
                for (var earlier = 0; earlier < index; earlier++)
                    if (explicitRhythmTargets[earlier] == candidate) { duplicate = true; break; }
                if (!duplicate) count++;
            }
            return count;
        }

        void OnEntityDied() => ResetHeavyAttackRhythm();
        void ResetHeavyAttackRhythm() => consecutiveBasicCount = 0;
        void ReturnHome()
        {
            var delta = home - transform.position; delta.y = 0;
            if (delta.magnitude < .3f) { SetState(BrainState.Idle); entity.Move(Vector3.zero); }
            else entity.Move(delta.normalized * entity.Stats.MoveSpeed);
        }

        void SetState(BrainState next)
        {
            if (State == next) return;
            State = next;
            PublishIntent();
        }

        void PublishIntent() => HostileIntentChanged?.Invoke(this);
    }
}
