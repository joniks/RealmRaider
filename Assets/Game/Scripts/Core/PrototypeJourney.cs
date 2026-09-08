using UnityEngine;

namespace RealmRaiders.Core
{
    public enum PrototypeJourneyStage { Inactive, Build, Raid, RaidResult, Defense }

    /// <summary>Pure process-session routing state for the canonical Sylvan prototype journey.</summary>
    public static class PrototypeJourney
    {
        public static PrototypeJourneyStage Stage { get; private set; }
        public static bool IsActive => Stage != PrototypeJourneyStage.Inactive;
        public static int ActiveToken { get; private set; }
        static int nextToken;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetSession()
        {
            Stage = PrototypeJourneyStage.Inactive;
            ActiveToken = 0;
            nextToken = 0;
        }

        public static bool TryStart(out int token)
        {
            token = 0;
            if (IsActive) return false;
            ActiveToken = token = NextToken();
            Stage = PrototypeJourneyStage.Build;
            return true;
        }

        public static bool TryBeginRaid(int token) => TryAdvance(token, PrototypeJourneyStage.Build, PrototypeJourneyStage.Raid);
        public static bool TryReachRaidResult(int token) => TryAdvance(token, PrototypeJourneyStage.Raid, PrototypeJourneyStage.RaidResult);
        public static bool TryRetryRaid(int token) => TryAdvance(token, PrototypeJourneyStage.RaidResult, PrototypeJourneyStage.Raid);
        public static bool TryBeginDefense(int token) => TryAdvance(token, PrototypeJourneyStage.RaidResult, PrototypeJourneyStage.Defense);
        public static bool TryCompleteDefense(int token) => TryAdvance(token, PrototypeJourneyStage.Defense, PrototypeJourneyStage.Inactive);

        public static bool Cancel()
        {
            if (!IsActive) return false;
            Stage = PrototypeJourneyStage.Inactive;
            ActiveToken = 0;
            return true;
        }

        public static bool Cancel(int token)
        {
            if (token == 0 || token != ActiveToken) return false;
            return Cancel();
        }

        public static void ResetForTests() => ResetSession();

        static bool TryAdvance(int token, PrototypeJourneyStage expected, PrototypeJourneyStage next)
        {
            if (token == 0 || token != ActiveToken || Stage != expected) return false;
            Stage = next;
            ActiveToken = next == PrototypeJourneyStage.Inactive ? 0 : NextToken();
            return true;
        }

        static int NextToken()
        {
            nextToken = nextToken == int.MaxValue ? 1 : nextToken + 1;
            return nextToken;
        }
    }
}
