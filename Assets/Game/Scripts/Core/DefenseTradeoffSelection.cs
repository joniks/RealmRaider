using RealmRaiders.Modules.SylvanDefenseTradeoffs;

namespace RealmRaiders.Core
{
    public sealed class DefenseTradeoffSelection
    {
        DefenseTradeoffSelection(SylvanDefenseTradeoffFact fact)
        {
            TradeoffId = fact.TradeoffId;
            DisplayName = fact.DisplayName;
            TacticalSummary = fact.TacticalSummary;
            PossessionEnergyMaximumSeconds = fact.PossessionEnergyMaximumSeconds;
        }

        public string TradeoffId { get; }
        public string DisplayName { get; }
        public string TacticalSummary { get; }
        public int PossessionEnergyMaximumSeconds { get; }
        public string BuildCopy => $"{DisplayName.ToUpperInvariant()} — {TacticalSummary}";

        public static bool TryResolve(DefenseLayout layout, out DefenseTradeoffSelection selection)
        {
            selection = null;
            if (!DefenseLayoutRules.IsValid(layout, out _)) return false;

            var wolves = 0;
            var guardianEnts = 0;
            var rootTraps = 0;
            var openCreatureSlots = 0;
            for (var index = 0; index < layout.Slots.Length; index++)
            {
                var slot = layout.Slots[index];
                if (slot.Piece == DefensePieceType.Wolf) wolves++;
                else if (slot.Piece == DefensePieceType.Ent) guardianEnts++;
                else if (slot.Piece == DefensePieceType.RootTrap) rootTraps++;
                else if (slot.SlotType == DefenseSlotType.Creature) openCreatureSlots++;
            }

            var result = SylvanDefenseTradeoffEvaluator.Evaluate(
                new SylvanDefenseRosterSummary(wolves, guardianEnts, rootTraps, openCreatureSlots));
            if (!result.HasTradeoff) return false;
            selection = new DefenseTradeoffSelection(result.Tradeoff);
            return true;
        }

        public static DefenseTradeoffSelection ResolveOrDefault(DefenseLayout layout)
        {
            if (TryResolve(layout, out var selection)) return selection;
            if (TryResolve(DefenseLayout.Default(), out selection)) return selection;
            throw new System.InvalidOperationException("The default Sylvan defense layout has no tradeoff fact.");
        }
    }
}
