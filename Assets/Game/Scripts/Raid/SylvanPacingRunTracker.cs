using System;
using System.Collections.Generic;
using RealmRaiders.Characters;
using RealmRaiders.Modules.StarterRealmLayouts;
using RealmRaiders.Modules.SylvanEncounterPacing;
using RealmRaiders.Realm;
using UnityEngine;

namespace RealmRaiders.Raid
{
    public enum SylvanPacingBeatKind { Unknown, Encounter, HazardRoute, RecoveryOpportunity, Objective }
    public enum SylvanPacingRequirement { Unknown, Required, Optional }

    public sealed class SylvanPacingBeatSnapshot
    {
        public SylvanPacingBeatSnapshot(string beatId, string roleId, SylvanPacingBeatKind kind,
            SylvanPacingRequirement requirement)
        { BeatId = beatId; RoleId = roleId; Kind = kind; Requirement = requirement; }

        public string BeatId { get; }
        public string RoleId { get; }
        public SylvanPacingBeatKind Kind { get; }
        public SylvanPacingRequirement Requirement { get; }
    }

    public sealed class SylvanPacingRecipeSnapshot
    {
        readonly SylvanPacingBeatSnapshot[] beats;

        public SylvanPacingRecipeSnapshot(string layoutId, string tacticalSummary,
            IReadOnlyList<SylvanPacingBeatSnapshot> source)
        {
            LayoutId = layoutId;
            TacticalSummary = tacticalSummary;
            beats = source == null ? Array.Empty<SylvanPacingBeatSnapshot>() : Copy(source);
        }

        public string LayoutId { get; }
        public string TacticalSummary { get; }
        public IReadOnlyList<SylvanPacingBeatSnapshot> Beats => Array.AsReadOnly(beats);

        static SylvanPacingBeatSnapshot[] Copy(IReadOnlyList<SylvanPacingBeatSnapshot> source)
        {
            var result = new SylvanPacingBeatSnapshot[source.Count];
            for (var index = 0; index < source.Count; index++) result[index] = source[index];
            return result;
        }
    }

    [DisallowMultipleComponent]
    public sealed class SylvanPacingRunTracker : MonoBehaviour
    {
        sealed class BeatProgress
        {
            public SylvanPacingBeatSnapshot Beat;
            public bool EntryObserved;
            public bool Satisfied;
            public readonly HashSet<CombatEntity> Pending = new();
        }

        readonly List<BeatProgress> beats = new();
        readonly List<BeatProgress> requiredPreObjective = new();
        readonly Dictionary<RealmNodeView, Action<RealmNodeVisit>> nodeHandlers = new();
        readonly HashSet<string> boundRoles = new(StringComparer.Ordinal);
        readonly Dictionary<CombatEntity, Action> deathHandlers = new();
        int nextRequired;
        bool operational;
        RaidManager raid;

        public event Action Changed;
        public string LayoutId { get; private set; }
        public string TacticalSummary { get; private set; }
        public bool IsOperational => operational;
        public bool IsWarded => operational && nextRequired < requiredPreObjective.Count;
        public int RequiredBeatCount => requiredPreObjective.Count;
        public int CompletedRequiredBeatCount => nextRequired;
        public string NextRequirementCopy => IsWarded ? RequirementCopy(requiredPreObjective[nextRequired].Beat) : string.Empty;
        public IReadOnlyList<SylvanPacingBeatSnapshot> BeatSnapshots
        {
            get
            {
                var result = new SylvanPacingBeatSnapshot[beats.Count];
                for (var index = 0; index < beats.Count; index++) result[index] = beats[index].Beat;
                return Array.AsReadOnly(result);
            }
        }

        public static SylvanPacingRecipeSnapshot ResolveSnapshot(string selectedLayoutId)
        {
            try
            {
                var lookup = SylvanEncounterPacingEvidence.FindByLayoutId(selectedLayoutId);
                if (lookup == null || !lookup.Found || lookup.Recipe == null ||
                    !string.Equals(selectedLayoutId, lookup.Recipe.LayoutId, StringComparison.Ordinal) ||
                    !SylvanEncounterPacingEvidence.Validate(lookup.Recipe).IsValid) return null;
                var copied = new SylvanPacingBeatSnapshot[lookup.Recipe.Beats.Count];
                for (var index = 0; index < copied.Length; index++)
                {
                    var beat = lookup.Recipe.Beats[index];
                    if (beat == null) return null;
                    copied[index] = new SylvanPacingBeatSnapshot(beat.BeatId, RoleId(beat.Role),
                        BeatKind(beat.Kind), Requirement(beat.Requirement));
                }
                var snapshot = new SylvanPacingRecipeSnapshot(lookup.Recipe.LayoutId,
                    lookup.Recipe.TacticalSummary, copied);
                return IsValidSnapshot(selectedLayoutId, snapshot) ? snapshot : null;
            }
            catch { return null; }
        }

        public static SylvanPacingRunTracker Attach(Transform owner, string selectedLayoutId,
            SylvanPacingRecipeSnapshot snapshot)
        {
            if (!owner || !IsValidSnapshot(selectedLayoutId, snapshot)) return null;
            var tracker = owner.gameObject.AddComponent<SylvanPacingRunTracker>();
            tracker.Initialize(snapshot);
            return tracker;
        }

        void Initialize(SylvanPacingRecipeSnapshot snapshot)
        {
            LayoutId = snapshot.LayoutId;
            TacticalSummary = snapshot.TacticalSummary;
            foreach (var fact in snapshot.Beats)
            {
                var progress = new BeatProgress { Beat = fact };
                beats.Add(progress);
                if (fact.Requirement == SylvanPacingRequirement.Required && fact.Kind != SylvanPacingBeatKind.Objective)
                    requiredPreObjective.Add(progress);
            }
            operational = true;
        }

        public void BindNode(string roleId, RealmNodeView node)
        {
            if (!operational) return;
            if (string.IsNullOrEmpty(roleId) || !node || nodeHandlers.ContainsKey(node) || !boundRoles.Add(roleId) ||
                !beats.Exists(progress => string.Equals(progress.Beat.RoleId, roleId, StringComparison.Ordinal)))
            { DisposeRun(); return; }
            Action<RealmNodeVisit> handler = visit => ObserveNodeEntry(roleId, visit?.AliveHostiles);
            nodeHandlers.Add(node, handler);
            node.EncounterEntered += handler;
        }

        public bool SealBindings()
        {
            if (!operational) return false;
            foreach (var progress in beats)
                if (progress.Beat.Kind != SylvanPacingBeatKind.Objective && !boundRoles.Contains(progress.Beat.RoleId))
                { DisposeRun(); return false; }
            return true;
        }

        public void BindRaid(RaidManager manager)
        {
            if (raid) raid.StateChanged -= OnRaidState;
            raid = manager;
            if (raid) raid.StateChanged += OnRaidState;
        }

        public void ObserveNodeEntry(string roleId, IReadOnlyList<CombatEntity> livingHostiles)
        {
            if (!operational || string.IsNullOrEmpty(roleId)) return;
            var progress = beats.Find(candidate => string.Equals(candidate.Beat.RoleId, roleId, StringComparison.Ordinal));
            if (progress == null || progress.EntryObserved || progress.Beat.Kind == SylvanPacingBeatKind.Objective) return;
            progress.EntryObserved = true;
            if (progress.Beat.Kind != SylvanPacingBeatKind.Encounter)
            {
                progress.Satisfied = true;
                Advance();
                return;
            }

            if (livingHostiles != null)
                for (var index = 0; index < livingHostiles.Count; index++)
                {
                    var hostile = livingHostiles[index];
                    if (!hostile || hostile.Health == null || hostile.Health.IsDead || !progress.Pending.Add(hostile)) continue;
                    var captured = hostile;
                    Action handler = () => OnHostileDied(progress, captured);
                    deathHandlers.Add(captured, handler);
                    captured.Health.Died += handler;
                }
            progress.Satisfied = progress.Pending.Count == 0;
            Advance();
        }

        public void DisposeRun()
        {
            if (!operational && nodeHandlers.Count == 0 && deathHandlers.Count == 0 && !raid) return;
            foreach (var pair in nodeHandlers) if (pair.Key) pair.Key.EncounterEntered -= pair.Value;
            nodeHandlers.Clear();
            boundRoles.Clear();
            foreach (var pair in deathHandlers)
                if (pair.Key && pair.Key.Health != null) pair.Key.Health.Died -= pair.Value;
            deathHandlers.Clear();
            foreach (var progress in beats) progress.Pending.Clear();
            if (raid) raid.StateChanged -= OnRaidState;
            raid = null;
            operational = false;
            Changed?.Invoke();
            Changed = null;
        }

        void OnHostileDied(BeatProgress progress, CombatEntity hostile)
        {
            if (!operational || progress == null || !hostile || !progress.Pending.Remove(hostile)) return;
            if (deathHandlers.TryGetValue(hostile, out var handler))
            {
                deathHandlers.Remove(hostile);
                if (hostile.Health != null) hostile.Health.Died -= handler;
            }
            if (progress.Pending.Count == 0) progress.Satisfied = true;
            Advance();
        }

        void Advance()
        {
            var before = nextRequired;
            while (nextRequired < requiredPreObjective.Count && requiredPreObjective[nextRequired].Satisfied)
                nextRequired++;
            if (nextRequired != before) Changed?.Invoke();
        }

        void OnRaidState(RaidState state)
        {
            if (state is RaidState.Victory or RaidState.Defeat or RaidState.Escape or RaidState.RaidResult) DisposeRun();
        }

        void OnDisable() => DisposeRun();
        void OnDestroy() => DisposeRun();

        static bool IsValidSnapshot(string selectedLayoutId, SylvanPacingRecipeSnapshot snapshot)
        {
            if (snapshot == null || string.IsNullOrEmpty(selectedLayoutId) ||
                !string.Equals(selectedLayoutId, snapshot.LayoutId, StringComparison.Ordinal) ||
                string.IsNullOrWhiteSpace(snapshot.TacticalSummary) ||
                !string.Equals(snapshot.TacticalSummary, snapshot.TacticalSummary.ToUpperInvariant(), StringComparison.Ordinal) ||
                snapshot.Beats.Count == 0) return false;
            var beatIds = new HashSet<string>(StringComparer.Ordinal);
            var roles = new HashSet<string>(StringComparer.Ordinal);
            var objectiveCount = 0;
            for (var index = 0; index < snapshot.Beats.Count; index++)
            {
                var beat = snapshot.Beats[index];
                if (beat == null || !HasStableId(beat.BeatId) || !beatIds.Add(beat.BeatId) ||
                    string.IsNullOrEmpty(beat.RoleId) || !roles.Add(beat.RoleId) ||
                    !KindMatchesRole(beat.RoleId, beat.Kind) ||
                    beat.Requirement is not (SylvanPacingRequirement.Required or SylvanPacingRequirement.Optional)) return false;
                if (beat.Kind != SylvanPacingBeatKind.Objective) continue;
                objectiveCount++;
                if (beat.Requirement != SylvanPacingRequirement.Required || index != snapshot.Beats.Count - 1) return false;
            }
            return objectiveCount == 1;
        }

        static bool HasStableId(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            for (var index = 0; index < value.Length; index++)
            {
                var symbol = value[index];
                if (!(symbol >= 'a' && symbol <= 'z') && !(symbol >= '0' && symbol <= '9') &&
                    symbol != '.' && symbol != '_' && symbol != '-') return false;
            }
            return true;
        }

        static bool KindMatchesRole(string roleId, SylvanPacingBeatKind kind) => roleId switch
        {
            "WolfGroveEncounter" or "EntGroveEncounter" => kind == SylvanPacingBeatKind.Encounter,
            "RootPathHazard" => kind == SylvanPacingBeatKind.HazardRoute,
            "MoonwellRecovery" => kind == SylvanPacingBeatKind.RecoveryOpportunity,
            "HeartTreeObjective" => kind == SylvanPacingBeatKind.Objective,
            _ => false
        };

        static string RequirementCopy(SylvanPacingBeatSnapshot beat) => beat.RoleId switch
        {
            "WolfGroveEncounter" => "DEFEAT SYLVAN WOLVES",
            "RootPathHazard" => "ENTER ROOT PATH",
            "EntGroveEncounter" => "DEFEAT GUARDIAN ENT",
            "MoonwellRecovery" => "ENTER MOONWELL",
            _ => "COMPLETE THE NEXT REALM BEAT"
        };

        static string RoleId(SylvanRealmNodeMaterializationRole role) => role switch
        {
            SylvanRealmNodeMaterializationRole.WolfGroveEncounter => "WolfGroveEncounter",
            SylvanRealmNodeMaterializationRole.RootPathHazard => "RootPathHazard",
            SylvanRealmNodeMaterializationRole.EntGroveEncounter => "EntGroveEncounter",
            SylvanRealmNodeMaterializationRole.MoonwellRecovery => "MoonwellRecovery",
            SylvanRealmNodeMaterializationRole.HeartTreeObjective => "HeartTreeObjective",
            _ => string.Empty
        };

        static SylvanPacingBeatKind BeatKind(SylvanEncounterPacingBeatKind kind) => kind switch
        {
            SylvanEncounterPacingBeatKind.Encounter => SylvanPacingBeatKind.Encounter,
            SylvanEncounterPacingBeatKind.HazardRoute => SylvanPacingBeatKind.HazardRoute,
            SylvanEncounterPacingBeatKind.RecoveryOpportunity => SylvanPacingBeatKind.RecoveryOpportunity,
            SylvanEncounterPacingBeatKind.Objective => SylvanPacingBeatKind.Objective,
            _ => SylvanPacingBeatKind.Unknown
        };

        static SylvanPacingRequirement Requirement(SylvanEncounterPacingRequirement requirement) => requirement switch
        {
            SylvanEncounterPacingRequirement.Required => SylvanPacingRequirement.Required,
            SylvanEncounterPacingRequirement.Optional => SylvanPacingRequirement.Optional,
            _ => SylvanPacingRequirement.Unknown
        };
    }
}
