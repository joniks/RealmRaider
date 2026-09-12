using RealmRaiders.Modules.SylvanEncounters;
using UnityEngine;

namespace RealmRaiders.Core
{
    /// <summary>Session-only explicit Sylvan raid choice; it never reads or writes persistent state.</summary>
    public static class SylvanRaidCompositionSelection
    {
        static SylvanRaidComposition selected;

        internal static SylvanRaidComposition Current => Resolve(selected?.CompositionId);
        public static string CompositionId => Current.CompositionId;
        public static string DisplayName => Current.DisplayName;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetSession() => selected = StarterSylvanRaidCompositions.Baseline;

        public static void Cycle()
        {
            var all = StarterSylvanRaidCompositions.All;
            var current = Current;
            var currentIndex = 0;
            for (var index = 0; index < all.Count; index++)
                if (all[index].CompositionId == current.CompositionId) { currentIndex = index; break; }
            selected = all[(currentIndex + 1) % all.Count];
        }

        public static void Select(string compositionId) => selected = Resolve(compositionId);

        public static void ResetForTests() => ResetSession();

        static SylvanRaidComposition Resolve(string compositionId)
        {
            var all = StarterSylvanRaidCompositions.All;
            if (!string.IsNullOrEmpty(compositionId))
                for (var index = 0; index < all.Count; index++)
                    if (all[index]?.CompositionId == compositionId) return all[index];
            return StarterSylvanRaidCompositions.Baseline;
        }
    }
}
