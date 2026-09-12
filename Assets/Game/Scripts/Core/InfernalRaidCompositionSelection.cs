using System;
using RealmRaiders.Modules.InfernalEncounters;
using UnityEngine;

namespace RealmRaiders.Core
{
    /// <summary>Session-only explicit Infernal raid choice; it never reads or writes persistent state.</summary>
    public static class InfernalRaidCompositionSelection
    {
        static readonly string[] OrderedCompositionIds =
        {
            StarterInfernalRaidPacingCatalogue.BruteFinale.CompositionId,
            StarterInfernalRaidPacingCatalogue.EntryTrial.CompositionId,
            StarterInfernalRaidPacingCatalogue.RiskRoute.CompositionId
        };

        static string selectedCompositionId;

        internal static InfernalRaidCompositionFacts Current => Resolve(selectedCompositionId);
        public static string CompositionId => Current.Pacing.CompositionId;
        public static string DisplayName => Current.Presentation.DisplayName;
        public static string TacticalSummary => Current.Presentation.TacticalSummary;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetSession() => selectedCompositionId = OrderedCompositionIds[0];

        public static void Cycle()
        {
            var currentId = CompositionId;
            var currentIndex = 0;
            for (var index = 0; index < OrderedCompositionIds.Length; index++)
                if (string.Equals(OrderedCompositionIds[index], currentId, StringComparison.Ordinal))
                { currentIndex = index; break; }
            selectedCompositionId = OrderedCompositionIds[(currentIndex + 1) % OrderedCompositionIds.Length];
        }

        public static void Select(string compositionId) => selectedCompositionId = Resolve(compositionId).Pacing.CompositionId;

        public static void ResetForTests() => ResetSession();

        static InfernalRaidCompositionFacts Resolve(string compositionId)
        {
            var resolvedId = IsSupported(compositionId) ? compositionId : OrderedCompositionIds[0];
            InfernalRaidPacingComposition pacing = null;
            foreach (var candidate in StarterInfernalRaidPacingCatalogue.All)
                if (string.Equals(candidate?.CompositionId, resolvedId, StringComparison.Ordinal))
                { pacing = candidate; break; }

            var spatialLookup = InfernalEntTrialSpatialRecipeEvidence.FindByCompositionId(resolvedId);
            var presentationLookup = StarterInfernalRaidPresentationCatalogue.FindByCompositionId(resolvedId);
            if (pacing == null || !spatialLookup.Found || !presentationLookup.Found ||
                !InfernalEntTrialSpatialRecipeEvidence.Validate(spatialLookup.Recipe).IsValid ||
                !InfernalRaidPresentationEvidence.Validate(presentationLookup.Presentation).IsValid ||
                !string.Equals(pacing.CompositionId, spatialLookup.Recipe.CompositionId, StringComparison.Ordinal) ||
                !string.Equals(pacing.CompositionId, presentationLookup.Presentation.CompositionId, StringComparison.Ordinal))
                throw new InvalidOperationException($"Infernal raid composition '{resolvedId}' failed exact module lookup or validation.");
            return new InfernalRaidCompositionFacts(pacing, spatialLookup.Recipe, presentationLookup.Presentation);
        }

        static bool IsSupported(string compositionId)
        {
            if (string.IsNullOrWhiteSpace(compositionId)) return false;
            foreach (var id in OrderedCompositionIds)
                if (string.Equals(id, compositionId, StringComparison.Ordinal)) return true;
            return false;
        }
    }

    internal readonly struct InfernalRaidCompositionFacts
    {
        public InfernalRaidCompositionFacts(InfernalRaidPacingComposition pacing, InfernalEntTrialSpatialRecipe spatial,
            InfernalRaidPresentationFact presentation)
        { Pacing = pacing; Spatial = spatial; Presentation = presentation; }

        public InfernalRaidPacingComposition Pacing { get; }
        public InfernalEntTrialSpatialRecipe Spatial { get; }
        public InfernalRaidPresentationFact Presentation { get; }
    }
}
