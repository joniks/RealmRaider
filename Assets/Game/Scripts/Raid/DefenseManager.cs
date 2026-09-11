using System;
using RealmRaiders.Characters;
using RealmRaiders.Possession;
using RealmRaiders.Realm;
using UnityEngine;

namespace RealmRaiders.Raid
{
    public enum DefenseState { Watching, Possessing, DefenderVictory, RealmLost }

    public readonly struct DefenseResultFact
    {
        public DefenseState Outcome { get; }
        public float Duration { get; }
        public float InvaderHealth { get; }
        public float InvaderMaximumHealth { get; }
        public float CoreProgress { get; }

        public DefenseResultFact(DefenseState outcome, float duration, float invaderHealth, float invaderMaximumHealth, float coreProgress)
        {
            if (outcome is not (DefenseState.DefenderVictory or DefenseState.RealmLost)) throw new ArgumentOutOfRangeException(nameof(outcome));
            if (!Finite(duration) || duration < 0) throw new ArgumentOutOfRangeException(nameof(duration));
            if (!Finite(invaderMaximumHealth) || invaderMaximumHealth <= 0) throw new ArgumentOutOfRangeException(nameof(invaderMaximumHealth));
            if (!Finite(invaderHealth) || invaderHealth < 0 || invaderHealth > invaderMaximumHealth) throw new ArgumentOutOfRangeException(nameof(invaderHealth));
            if (!Finite(coreProgress) || coreProgress < 0 || coreProgress > 1) throw new ArgumentOutOfRangeException(nameof(coreProgress));
            Outcome = outcome;
            Duration = duration;
            InvaderHealth = invaderHealth;
            InvaderMaximumHealth = invaderMaximumHealth;
            CoreProgress = coreProgress;
        }

        static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }

    public sealed class DefenseManager : MonoBehaviour
    {
        public event Action<DefenseState> StateChanged;
        public event Action<DefenseResultFact> Finished;
        public DefenseState State { get; private set; }
        public bool IsFinished => State is DefenseState.DefenderVictory or DefenseState.RealmLost;
        public float Duration => Time.time - startedAt;
        public bool HasResult { get; private set; }
        public DefenseResultFact Result { get; private set; }
        float startedAt;
        PossessionManager possession;
        CombatEntity invader;
        RealmCore core;

        public void Initialize(CombatEntity invader, RealmCore core, PossessionManager possessionManager)
        {
            this.invader = invader; this.core = core; possession = possessionManager; startedAt = Time.time; State = DefenseState.Watching;
            HasResult = false; Result = default;
            invader.Health.Died += Win; core.Completed += Lose; possession.PossessionChanged += OnPossession;
        }

        void OnPossession(CombatEntity value)
        {
            if (HasResult || State is DefenseState.DefenderVictory or DefenseState.RealmLost) return;
            SetState(value ? DefenseState.Possessing : DefenseState.Watching);
        }
        void Win()
        {
            if (!TryFreezeResult(DefenseState.DefenderVictory)) return;
            possession.Release();
            PublishResult();
        }

        void Lose()
        {
            if (!TryFreezeResult(DefenseState.RealmLost)) return;
            possession.Release();
            if (invader && !invader.Health.IsDead) invader.SetController(null);
            PublishResult();
        }
        bool TryFreezeResult(DefenseState outcome)
        {
            if (HasResult || IsFinished) return false;
            Result = new DefenseResultFact(outcome, Mathf.Max(0, Duration), invader.Health.Current, invader.Health.Maximum, core.Progress);
            HasResult = true;
            return true;
        }
        void PublishResult()
        {
            SetState(Result.Outcome);
            Finished?.Invoke(Result);
        }
        void SetState(DefenseState next) { State = next; StateChanged?.Invoke(next); }
    }
}
