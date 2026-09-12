using System;
using System.Collections;
using System.Collections.Generic;
using RealmRaiders.Characters;
using RealmRaiders.Realm;
using UnityEngine;

namespace RealmRaiders.Raid
{
    public enum RaidState { Idle, RaidStarting, Exploring, Combat, ObjectiveReached, Victory, Defeat, Escape, RaidResult }
    public enum RaidEncounterPhase { Hidden, Discovered, Hostiles, Cleared }

    public readonly struct RaidEncounterState
    {
        public string NodeId { get; }
        public int RemainingHostiles { get; }
        public RaidEncounterPhase Phase { get; }
        public bool Visible => Phase != RaidEncounterPhase.Hidden;

        internal RaidEncounterState(string nodeId, int remainingHostiles, RaidEncounterPhase phase)
        {
            NodeId = nodeId ?? string.Empty;
            RemainingHostiles = Mathf.Max(0, remainingHostiles);
            Phase = phase;
        }

        public static RaidEncounterState Hidden => new(string.Empty, 0, RaidEncounterPhase.Hidden);
    }

    public sealed class RaidEncounterLifecycle
    {
        readonly HashSet<string> enteredNodes = new(StringComparer.Ordinal);
        public RaidEncounterState Current { get; private set; } = RaidEncounterState.Hidden;

        public bool TryEnter(string nodeId, int aliveHostiles, out RaidEncounterState state)
        {
            if (string.IsNullOrWhiteSpace(nodeId)) throw new ArgumentException("Encounter node identity is required.", nameof(nodeId));
            if (aliveHostiles < 0) throw new ArgumentOutOfRangeException(nameof(aliveHostiles));
            if (!enteredNodes.Add(nodeId)) { state = Current; return false; }
            Current = aliveHostiles == 0
                ? new RaidEncounterState(nodeId, 0, RaidEncounterPhase.Discovered)
                : new RaidEncounterState(nodeId, aliveHostiles, RaidEncounterPhase.Hostiles);
            state = Current;
            return true;
        }

        public bool TrySetRemaining(string nodeId, int aliveHostiles, out RaidEncounterState state)
        {
            if (aliveHostiles < 0) throw new ArgumentOutOfRangeException(nameof(aliveHostiles));
            if (Current.Phase != RaidEncounterPhase.Hostiles || Current.NodeId != nodeId || aliveHostiles >= Current.RemainingHostiles)
            { state = Current; return false; }
            Current = aliveHostiles == 0
                ? new RaidEncounterState(nodeId, 0, RaidEncounterPhase.Cleared)
                : new RaidEncounterState(nodeId, aliveHostiles, RaidEncounterPhase.Hostiles);
            state = Current;
            return true;
        }

        public bool TryHide(out RaidEncounterState state)
        {
            if (!Current.Visible) { state = Current; return false; }
            Current = RaidEncounterState.Hidden;
            state = Current;
            return true;
        }

        public void Reset()
        {
            enteredNodes.Clear();
            Current = RaidEncounterState.Hidden;
        }
    }

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
        public event Action<RaidEncounterState> EncounterChanged;
        public event Action<RaidRewardFact> Rewarded;
        public RaidState State { get; private set; } = RaidState.Idle;
        public float Duration => State == RaidState.Idle ? 0 : Time.time - startedAt;
        public int Gold { get; private set; }
        public int RareMaterials { get; private set; }
        public int EnemiesDefeated { get; private set; }
        public int RoomsDiscovered { get; private set; }
        public bool AllowsRecovery => State is RaidState.RaidStarting or RaidState.Exploring or RaidState.Combat or RaidState.ObjectiveReached;
        float startedAt;
        bool realmRewardClaimed;
        CombatEntity hero;
        CombatEntity bonusRareEnemy;
        RealmNodeView[] configuredNodes = Array.Empty<RealmNodeView>();
        CombatEntity[] configuredEnemies = Array.Empty<CombatEntity>();
        readonly HashSet<RealmNodeView> creditedRooms = new();
        readonly HashSet<CombatEntity> creditedEnemies = new();
        readonly Dictionary<CombatEntity, Action> enemyDeathHandlers = new();
        readonly Dictionary<CombatEntity, Action> encounterDeathHandlers = new();
        readonly RaidEncounterLifecycle encounter = new();
        bool initialized;
        bool sourcesSubscribed;
        long rewardSequence;
        Vector3 objectiveRewardPosition;

        public RaidEncounterState Encounter => encounter.Current;

        public void Initialize(CombatEntity raidHero, RealmNodeView[] nodes, CombatEntity[] enemies, Vector3 objectivePosition, CombatEntity optionalBonusRareEnemy = null)
        {
            UnsubscribeSources();
            hero = raidHero;
            bonusRareEnemy = optionalBonusRareEnemy;
            configuredNodes = nodes ?? Array.Empty<RealmNodeView>();
            configuredEnemies = enemies ?? Array.Empty<CombatEntity>();
            creditedRooms.Clear();
            creditedEnemies.Clear();
            Gold = 0;
            RareMaterials = 0;
            EnemiesDefeated = 0;
            RoomsDiscovered = 0;
            encounter.Reset();
            rewardSequence = 0;
            realmRewardClaimed = false;
            objectiveRewardPosition = objectivePosition;
            initialized = true;
            SubscribeSources();
            SetState(RaidState.RaidStarting); startedAt = Time.time; SetState(RaidState.Exploring);
        }

        public void BeginObjective()
        { if (State == RaidState.Exploring || State == RaidState.Combat) SetState(RaidState.ObjectiveReached); }

        public void CompleteObjective()
        {
            if (State != RaidState.ObjectiveReached) return;
            Gold += 100; RareMaterials += 1;
            PublishReward(RaidRewardSource.RealmCoreVictory, 100, 1, objectiveRewardPosition);
            SetState(RaidState.Victory); StartCoroutine(ShowResult(true));
        }

        // The result screen can be refreshed or recreated, but this raid may secure its rewards once.
        public bool TryClaimRealmRewards()
        {
            if (realmRewardClaimed) return false;
            realmRewardClaimed = true;
            return true;
        }

        void OnRoomVisited(RealmNodeView node)
        {
            if (!node || IsTerminal(State) || !creditedRooms.Add(node)) return;
            RoomsDiscovered++; Gold += 5;
            PublishReward(RaidRewardSource.RoomDiscovery, 5, 0, node.transform.position);
        }

        void OnEnemyDied(CombatEntity enemy)
        {
            if (!enemy || IsTerminal(State) || !creditedEnemies.Add(enemy)) return;
            var rareDelta = enemy == bonusRareEnemy ? 1 : 0;
            EnemiesDefeated++; Gold += 15; RareMaterials += rareDelta;
            PublishReward(RaidRewardSource.EnemyDefeat, 15, rareDelta, enemy.transform.position);
        }

        void PublishReward(RaidRewardSource source, int goldDelta, int rareDelta, Vector3 worldPosition)
            => Rewarded?.Invoke(new RaidRewardFact(++rewardSequence, source, goldDelta, rareDelta, Gold, RareMaterials, worldPosition));

        void OnEncounterEntered(RealmNodeVisit visit)
        {
            if (visit == null || IsTerminal(State)) return;
            var alive = new HashSet<CombatEntity>();
            foreach (var hostile in visit.AliveHostiles)
                if (hostile && hostile.Health != null && !hostile.Health.IsDead) alive.Add(hostile);
            if (!encounter.TryEnter(visit.NodeId, alive.Count, out var state)) return;

            ClearEncounterDeathHandlers();
            foreach (var hostile in alive)
            {
                var captured = hostile;
                Action onDeath = () => OnEncounterHostileDied(captured);
                encounterDeathHandlers.Add(captured, onDeath);
                captured.Health.Died += onDeath;
            }
            EncounterChanged?.Invoke(state);
        }

        void OnEncounterHostileDied(CombatEntity hostile)
        {
            if (!hostile || !encounterDeathHandlers.TryGetValue(hostile, out var onDeath)) return;
            encounterDeathHandlers.Remove(hostile);
            if (hostile.Health != null) hostile.Health.Died -= onDeath;
            if (encounter.TrySetRemaining(encounter.Current.NodeId, encounterDeathHandlers.Count, out var state))
                EncounterChanged?.Invoke(state);
        }
        void OnHeroDied()
        {
            if (IsTerminal(State)) return;
            Gold = Mathf.RoundToInt(Gold * .5f); SetState(RaidState.Defeat); StartCoroutine(ShowResult(false));
        }
        IEnumerator ShowResult(bool victory) { yield return new WaitForSeconds(1.25f); SetState(RaidState.RaidResult); Finished?.Invoke(new RaidResult(victory, Gold, RareMaterials, EnemiesDefeated, RoomsDiscovered, Duration, victory)); }
        void SetState(RaidState next)
        {
            State = next;
            if (IsTerminal(next)) HideEncounter();
            StateChanged?.Invoke(next);
        }

        static bool IsTerminal(RaidState state) => state is RaidState.Victory or RaidState.Defeat or RaidState.Escape or RaidState.RaidResult;

        void SubscribeSources()
        {
            if (!initialized || sourcesSubscribed) return;
            sourcesSubscribed = true;
            if (hero && hero.Health != null) hero.Health.Died += OnHeroDied;
            foreach (var node in configuredNodes)
            {
                if (!node) continue;
                node.Visited -= OnRoomVisited;
                node.EncounterEntered -= OnEncounterEntered;
                node.Visited += OnRoomVisited;
                node.EncounterEntered += OnEncounterEntered;
            }
            foreach (var enemy in configuredEnemies)
            {
                if (!enemy || enemy.Health == null || enemyDeathHandlers.ContainsKey(enemy)) continue;
                var captured = enemy;
                Action onDeath = () => OnEnemyDied(captured);
                enemyDeathHandlers.Add(captured, onDeath);
                captured.Health.Died += onDeath;
            }
        }

        void UnsubscribeSources()
        {
            if (hero && hero.Health != null) hero.Health.Died -= OnHeroDied;
            foreach (var node in configuredNodes)
            {
                if (!node) continue;
                node.Visited -= OnRoomVisited;
                node.EncounterEntered -= OnEncounterEntered;
            }
            foreach (var pair in enemyDeathHandlers)
                if (pair.Key && pair.Key.Health != null) pair.Key.Health.Died -= pair.Value;
            enemyDeathHandlers.Clear();
            ClearEncounterDeathHandlers();
            sourcesSubscribed = false;
        }

        void ClearEncounterDeathHandlers()
        {
            foreach (var pair in encounterDeathHandlers)
                if (pair.Key && pair.Key.Health != null) pair.Key.Health.Died -= pair.Value;
            encounterDeathHandlers.Clear();
        }

        void HideEncounter()
        {
            ClearEncounterDeathHandlers();
            if (encounter.TryHide(out var state)) EncounterChanged?.Invoke(state);
        }

        void OnEnable()
        {
            if (initialized) SubscribeSources();
        }

        void OnDisable()
        {
            HideEncounter();
            UnsubscribeSources();
        }

        void OnDestroy()
        {
            initialized = false;
            HideEncounter();
            UnsubscribeSources();
        }
    }
}
