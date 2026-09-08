using System;
using UnityEngine;

namespace RealmRaiders.Core
{
    public enum FirstPlayableMinuteStatus { NotStarted, Active, Completed, Skipped }
    public enum BuildGuideStep { Hidden, Choose, Fix, Save }
    public enum DefenseGuideStep { Inactive, Select, Possess, Move, Attack, Dodge, Release, KeeperReturn, Result, Retry }
    public enum DefenseGuideTerminalOutcome { None, Completed, Retry }

    [Serializable]
    sealed class FirstPlayableMinuteRecord
    {
        public int schemaVersion;
        public string status;
    }

    public sealed class BuildLayoutSnapshot
    {
        internal readonly DefensePieceType[] Pieces;
        internal BuildLayoutSnapshot(DefensePieceType[] pieces) => Pieces = pieces;
    }

    /// <summary>Pure monotonic proof of the ordered direct-control actions.</summary>
    public sealed class FirstPlayableMinuteDefenseProof
    {
        public DefenseGuideStep Step { get; private set; }
        public DefenseGuideTerminalOutcome TerminalOutcome { get; private set; }

        public FirstPlayableMinuteDefenseProof(bool active)
        {
            Step = active ? DefenseGuideStep.Select : DefenseGuideStep.Inactive;
        }

        public bool TrySelect() => Advance(DefenseGuideStep.Select, DefenseGuideStep.Possess);
        public bool TryPossess() => Advance(DefenseGuideStep.Possess, DefenseGuideStep.Move);
        public bool TryMove() => Advance(DefenseGuideStep.Move, DefenseGuideStep.Attack);
        public bool TryAttack() => Advance(DefenseGuideStep.Attack, DefenseGuideStep.Dodge);
        public bool TryDodge() => Advance(DefenseGuideStep.Dodge, DefenseGuideStep.Release);
        public bool TryRelease() => Advance(DefenseGuideStep.Release, DefenseGuideStep.KeeperReturn);
        public bool TryKeeperReturn() => Advance(DefenseGuideStep.KeeperReturn, DefenseGuideStep.Result);

        public bool Interrupt(bool canRetry)
        {
            if (TerminalOutcome != DefenseGuideTerminalOutcome.None || Step is DefenseGuideStep.Retry or DefenseGuideStep.Result) return false;
            var next = canRetry ? DefenseGuideStep.Select : DefenseGuideStep.Inactive;
            if (Step == next) return false;
            Step = next;
            return true;
        }

        public DefenseGuideTerminalOutcome ReachTerminal()
        {
            if (TerminalOutcome != DefenseGuideTerminalOutcome.None) return DefenseGuideTerminalOutcome.None;
            TerminalOutcome = Step == DefenseGuideStep.Result ? DefenseGuideTerminalOutcome.Completed : DefenseGuideTerminalOutcome.Retry;
            Step = TerminalOutcome == DefenseGuideTerminalOutcome.Completed ? DefenseGuideStep.Result : DefenseGuideStep.Retry;
            return TerminalOutcome;
        }

        public bool ResetForRetry()
        {
            if (TerminalOutcome != DefenseGuideTerminalOutcome.Retry) return false;
            TerminalOutcome = DefenseGuideTerminalOutcome.None;
            Step = DefenseGuideStep.Select;
            return true;
        }

        bool Advance(DefenseGuideStep expected, DefenseGuideStep next)
        {
            if (TerminalOutcome != DefenseGuideTerminalOutcome.None || Step != expected) return false;
            Step = next;
            return true;
        }
    }

    /// <summary>Pure local guide preference plus one consumable BUILD-to-defense session handoff.</summary>
    public static class FirstPlayableMinute
    {
        const string Key = "realmraiders.firstPlayableMinute.v1";
        const int SchemaVersion = 1;
        static bool changedBuildAccepted;
        static bool defenseSessionEligible;
        static bool defenseSceneActive;
        static bool retryAuthorized;
        static int nextDefenseSceneToken;
        static int activeDefenseSceneToken;
        static int successfulWrites;

        public static string KeyForTests => Key;
        public static int SuccessfulWritesForTests => successfulWrites;
        public static bool ChangedBuildAcceptedForSession => changedBuildAccepted;
        public static bool DefenseSessionEligibleForTests => defenseSessionEligible;
        public static bool DefenseSceneActiveForTests => defenseSceneActive;
        public static bool RetryAuthorizedForTests => retryAuthorized;
        public static int ActiveDefenseSceneTokenForTests => activeDefenseSceneToken;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetSession()
        {
            ClearSession();
            successfulWrites = 0;
        }

        public static FirstPlayableMinuteStatus Load()
        {
            var json = PlayerPrefs.GetString(Key, string.Empty);
            if (string.IsNullOrWhiteSpace(json)) return FirstPlayableMinuteStatus.NotStarted;
            try
            {
                var record = JsonUtility.FromJson<FirstPlayableMinuteRecord>(json);
                if (record == null || record.schemaVersion != SchemaVersion || string.IsNullOrWhiteSpace(record.status)) return FirstPlayableMinuteStatus.NotStarted;
                return record.status switch
                {
                    nameof(FirstPlayableMinuteStatus.NotStarted) => FirstPlayableMinuteStatus.NotStarted,
                    nameof(FirstPlayableMinuteStatus.Active) => FirstPlayableMinuteStatus.Active,
                    nameof(FirstPlayableMinuteStatus.Completed) => FirstPlayableMinuteStatus.Completed,
                    nameof(FirstPlayableMinuteStatus.Skipped) => FirstPlayableMinuteStatus.Skipped,
                    _ => FirstPlayableMinuteStatus.NotStarted
                };
            }
            catch
            {
                return FirstPlayableMinuteStatus.NotStarted;
            }
        }

        public static bool TryStart()
        {
            if (Load() != FirstPlayableMinuteStatus.NotStarted) return false;
            Save(FirstPlayableMinuteStatus.Active);
            return true;
        }

        public static bool TryComplete()
        {
            if (Load() != FirstPlayableMinuteStatus.Active) return false;
            Save(FirstPlayableMinuteStatus.Completed);
            ClearSession();
            return true;
        }

        public static bool Skip()
        {
            var status = Load();
            if (status != FirstPlayableMinuteStatus.NotStarted && status != FirstPlayableMinuteStatus.Active) return false;
            Save(FirstPlayableMinuteStatus.Skipped);
            ClearSession();
            return true;
        }

        public static BuildLayoutSnapshot CaptureBuildEntry(DefenseLayout layout)
        {
            if (layout?.Slots == null || layout.Slots.Length != 5) return null;
            var pieces = new DefensePieceType[layout.Slots.Length];
            for (var index = 0; index < pieces.Length; index++) pieces[index] = layout.Slots[index].Piece;
            return new BuildLayoutSnapshot(pieces);
        }

        public static BuildGuideStep EvaluateBuild(BuildLayoutSnapshot entry, DefenseLayout current, out string validationReason)
        {
            var valid = DefenseLayoutRules.IsValid(current, out validationReason);
            if (Matches(entry, current)) return BuildGuideStep.Choose;
            return valid ? BuildGuideStep.Save : BuildGuideStep.Fix;
        }

        public static string BuildCopy(BuildGuideStep step, string validationReason)
        {
            return step switch
            {
                BuildGuideStep.Choose => "CHANGE ONE DEFENSE — TAP A SLOT",
                BuildGuideStep.Fix => $"KEEP CHOOSING — {validationReason}",
                BuildGuideStep.Save => "PLAN READY — SAVE & DEFEND",
                _ => string.Empty
            };
        }

        public static void ResetBuildHandoff() => ClearSession();

        public static bool TryAcceptChangedBuild(BuildLayoutSnapshot entry, DefenseLayout current)
        {
            if (changedBuildAccepted || Load() != FirstPlayableMinuteStatus.Active) return false;
            if (EvaluateBuild(entry, current, out _) != BuildGuideStep.Save) return false;
            changedBuildAccepted = true;
            return true;
        }

        public static bool ConsumeChangedBuildHandoff()
        {
            if (!changedBuildAccepted) return false;
            changedBuildAccepted = false;
            return true;
        }

        public static bool TryBeginSylvanDefense(out int sceneToken)
        {
            sceneToken = 0;
            if (Load() != FirstPlayableMinuteStatus.Active) return false;
            if (changedBuildAccepted)
            {
                changedBuildAccepted = false;
                defenseSessionEligible = true;
                defenseSceneActive = true;
                retryAuthorized = false;
                sceneToken = activeDefenseSceneToken = NextDefenseSceneToken();
                return true;
            }
            if (!defenseSessionEligible || !retryAuthorized) return false;
            retryAuthorized = false;
            defenseSceneActive = true;
            sceneToken = activeDefenseSceneToken = NextDefenseSceneToken();
            return true;
        }

        public static bool PrepareDefenseRetry(int sceneToken)
        {
            if (sceneToken == 0 || sceneToken != activeDefenseSceneToken || Load() != FirstPlayableMinuteStatus.Active || !defenseSessionEligible || !defenseSceneActive) return false;
            retryAuthorized = true;
            return true;
        }

        public static void EndDefenseScene(int sceneToken)
        {
            if (sceneToken == 0 || sceneToken != activeDefenseSceneToken) return;
            activeDefenseSceneToken = 0;
            defenseSceneActive = false;
            if (!retryAuthorized) ClearSession();
        }

        public static void ResetForTests()
        {
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.Save();
            ClearSession();
            successfulWrites = 0;
        }

        public static void ResetProcessSessionForTests()
        {
            ClearSession();
            successfulWrites = 0;
        }

        static void ClearSession()
        {
            changedBuildAccepted = false;
            defenseSessionEligible = false;
            defenseSceneActive = false;
            retryAuthorized = false;
            activeDefenseSceneToken = 0;
        }

        static int NextDefenseSceneToken()
        {
            nextDefenseSceneToken = nextDefenseSceneToken == int.MaxValue ? 1 : nextDefenseSceneToken + 1;
            return nextDefenseSceneToken;
        }

        static bool Matches(BuildLayoutSnapshot entry, DefenseLayout current)
        {
            if (entry?.Pieces == null || current?.Slots == null || entry.Pieces.Length != current.Slots.Length) return false;
            for (var index = 0; index < entry.Pieces.Length; index++) if (entry.Pieces[index] != current.Slots[index].Piece) return false;
            return true;
        }

        static void Save(FirstPlayableMinuteStatus status)
        {
            var record = new FirstPlayableMinuteRecord { schemaVersion = SchemaVersion, status = status.ToString() };
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(record));
            PlayerPrefs.Save();
            successfulWrites++;
        }
    }
}
