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
        bool started, complete;

        public void Initialize(CombatEntity target) => hero = target;

        void Update()
        {
            if (complete || !hero || hero.Health.IsDead) return;
            var delta = hero.transform.position - transform.position; delta.y = 0;
            if (delta.sqrMagnitude <= 12.25f)
            {
                if (!started) { started = true; InteractionStarted?.Invoke(); }
                Progress = Mathf.Clamp01(Progress + Time.deltaTime / InteractionDuration); ProgressChanged?.Invoke(Progress);
                if (Progress >= 1) { complete = true; Completed?.Invoke(); }
            }
            else if (Progress > 0)
            { Progress = Mathf.Max(0, Progress - Time.deltaTime * .6f); ProgressChanged?.Invoke(Progress); }
        }
    }
}
