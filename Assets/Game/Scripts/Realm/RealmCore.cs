using System;
using RealmRaiders.Characters;
using UnityEngine;

namespace RealmRaiders.Realm
{
    public sealed class RealmCore : MonoBehaviour
    {
        public event Action InteractionStarted;
        public event Action Completed;
        public event Action<float> ProgressChanged;
        public float Progress { get; private set; }
        public float InteractionDuration = 2.5f;
        CombatEntity hero;
        Func<bool> warded;
        bool started, complete;
        public bool HeroInRange
        {
            get
            {
                if (!isActiveAndEnabled || complete || !hero || hero.Health == null || hero.Health.IsDead) return false;
                var delta = hero.transform.position - transform.position;
                delta.y = 0;
                return delta.sqrMagnitude <= 12.25f;
            }
        }
        public bool IsWarded => warded != null && warded();

        public void Initialize(CombatEntity target) => hero = target;
        public void SetInteractionWard(Func<bool> predicate) => warded = predicate;

        void Update()
        {
            if (complete || !hero || hero.Health == null || hero.Health.IsDead) return;
            var heroInRange = HeroInRange;
            if (heroInRange && IsWarded)
            {
                started = false;
                if (Progress > 0) { Progress = 0; ProgressChanged?.Invoke(Progress); }
                return;
            }
            if (heroInRange)
            {
                if (!started) { started = true; InteractionStarted?.Invoke(); }
                Progress = Mathf.Clamp01(Progress + Time.deltaTime / InteractionDuration); ProgressChanged?.Invoke(Progress);
                if (Progress >= 1) { complete = true; Completed?.Invoke(); }
            }
            else if (Progress > 0)
            { Progress = Mathf.Max(0, Progress - Time.deltaTime * .6f); ProgressChanged?.Invoke(Progress); }
        }

        void OnDestroy() => warded = null;
    }
}
