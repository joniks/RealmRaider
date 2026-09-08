using System;
using UnityEngine;

namespace RealmRaiders.Core
{
    public enum FirstPlayableMinuteStatus { NotStarted, Active, Completed, Skipped }
    public enum BuildGuideStep { Hidden, Choose, Fix, Save }

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

    /// <summary>Pure local guide preference plus one consumable BUILD-to-defense session handoff.</summary>
    public static class FirstPlayableMinute
    {
        const string Key = "realmraiders.firstPlayableMinute.v1";
        const int SchemaVersion = 1;
        static bool changedBuildAccepted;
        static int successfulWrites;

        public static string KeyForTests => Key;
        public static int SuccessfulWritesForTests => successfulWrites;
        public static bool ChangedBuildAcceptedForSession => changedBuildAccepted;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetSession()
        {
            changedBuildAccepted = false;
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
            return true;
        }

        public static bool Skip()
        {
            var status = Load();
            if (status != FirstPlayableMinuteStatus.NotStarted && status != FirstPlayableMinuteStatus.Active) return false;
            Save(FirstPlayableMinuteStatus.Skipped);
            changedBuildAccepted = false;
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

        public static void ResetBuildHandoff() => changedBuildAccepted = false;

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

        public static void ResetForTests()
        {
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.Save();
            changedBuildAccepted = false;
            successfulWrites = 0;
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
