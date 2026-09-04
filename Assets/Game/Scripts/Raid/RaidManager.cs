using System;
using System.Collections;
using RealmRaiders.Characters;
using RealmRaiders.Realm;
using UnityEngine;

namespace RealmRaiders.Raid
{
    public enum RaidState { Idle, RaidStarting, Exploring, Combat, ObjectiveReached, Victory, Defeat, Escape, RaidResult }

    public readonly struct RaidResult
    {
        public readonly bool Victory;
        public readonly int Gold, RareMaterials, EnemiesDefeated, RoomsDiscovered;
        public readonly float Duration;
        public readonly bool CoreReached;
        public RaidResult(bool victory, int gold, int rare, int enemies, int rooms, float duration, bool core)
        { Victory = victory; Gold = gold; RareMaterials = rare; EnemiesDefeated = enemies; RoomsDiscovered = rooms; Duration = duration; CoreReached = core; }
    }

    public sealed class RaidManager : MonoBehaviour
    {
        public event Action<RaidState> StateChanged;
        public event Action<RaidResult> Finished;
        public RaidState State { get; private set; } = RaidState.Idle;
        public float Duration => State == RaidState.Idle ? 0 : Time.time - startedAt;
        public int Gold { get; private set; }
        public int RareMaterials { get; private set; }
        public int EnemiesDefeated { get; private set; }
        public int RoomsDiscovered { get; private set; }
        float startedAt;
        CombatEntity hero;

        public void Initialize(CombatEntity raidHero, RealmNodeView[] nodes, CombatEntity[] enemies)
        {
            hero = raidHero; hero.Health.Died += OnHeroDied;
            foreach (var node in nodes) node.Visited += OnRoomVisited;
            foreach (var enemy in enemies) enemy.Health.Died += OnEnemyDied;
            SetState(RaidState.RaidStarting); startedAt = Time.time; SetState(RaidState.Exploring);
        }

        public void BeginObjective()
        { if (State == RaidState.Exploring || State == RaidState.Combat) SetState(RaidState.ObjectiveReached); }

        public void CompleteObjective()
        {
            if (State != RaidState.ObjectiveReached) return;
            Gold += 100; RareMaterials += 1; SetState(RaidState.Victory); StartCoroutine(ShowResult(true));
        }

        void OnRoomVisited(RealmNodeView node) { RoomsDiscovered++; Gold += 5; }
        void OnEnemyDied() { EnemiesDefeated++; Gold += 15; }
        void OnHeroDied()
        {
            if (State is RaidState.Victory or RaidState.RaidResult) return;
            Gold = Mathf.RoundToInt(Gold * .5f); SetState(RaidState.Defeat); StartCoroutine(ShowResult(false));
        }
        IEnumerator ShowResult(bool victory) { yield return new WaitForSeconds(1.25f); SetState(RaidState.RaidResult); Finished?.Invoke(new RaidResult(victory, Gold, RareMaterials, EnemiesDefeated, RoomsDiscovered, Duration, victory)); }
        void SetState(RaidState next) { State = next; StateChanged?.Invoke(next); }
    }
}
