using System;
using RealmRaiders.Characters;
using RealmRaiders.Possession;
using RealmRaiders.Realm;
using UnityEngine;

namespace RealmRaiders.Raid
{
    public enum DefenseState { Watching, Possessing, DefenderVictory, RealmLost }

    public sealed class DefenseManager : MonoBehaviour
    {
        public event Action<DefenseState> StateChanged;
        public DefenseState State { get; private set; }
        public bool IsFinished => State is DefenseState.DefenderVictory or DefenseState.RealmLost;
        public float Duration => Time.time - startedAt;
        float startedAt;
        PossessionManager possession;
        CombatEntity invader;

        public void Initialize(CombatEntity invader, RealmCore core, PossessionManager possessionManager)
        {
            this.invader = invader; possession = possessionManager; startedAt = Time.time; State = DefenseState.Watching;
            invader.Health.Died += Win; core.Completed += Lose; possession.PossessionChanged += OnPossession;
        }

        void OnPossession(CombatEntity value)
        {
            if (State is DefenseState.DefenderVictory or DefenseState.RealmLost) return;
            SetState(value ? DefenseState.Possessing : DefenseState.Watching);
        }
        void Win()
        {
            if (IsFinished) return;
            possession.Release();
            SetState(DefenseState.DefenderVictory);
        }

        void Lose()
        {
            if (IsFinished) return;
            possession.Release();
            if (invader && !invader.Health.IsDead) invader.SetController(null);
            SetState(DefenseState.RealmLost);
        }
        void SetState(DefenseState next) { State = next; StateChanged?.Invoke(next); }
    }
}
