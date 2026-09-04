using System;
using UnityEngine;

namespace RealmRaiders.Core
{
    public enum DefensePieceType { Empty, Wolf, Ent, RootTrap }
    public enum DefenseSlotType { Creature, Trap }

    [Serializable]
    public struct DefenseSlotLayout
    {
        public DefenseSlotType SlotType;
        public DefensePieceType Piece;
        public DefenseSlotLayout(DefenseSlotType slotType, DefensePieceType piece) { SlotType = slotType; Piece = piece; }
    }

    [Serializable]
    public sealed class DefenseLayout
    {
        public int Version = 1;
        public DefenseSlotLayout[] Slots;
        public DefenseLayout(DefenseSlotLayout[] slots) { Slots = slots; }
        public static DefenseLayout Default() => new(new[] {
            new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Wolf),
            new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Wolf),
            new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Ent),
            new DefenseSlotLayout(DefenseSlotType.Trap, DefensePieceType.RootTrap),
            new DefenseSlotLayout(DefenseSlotType.Trap, DefensePieceType.Empty) });
    }

    public static class DefenseLayoutRules
    {
        public const int Budget = 10;
        public static int Cost(DefensePieceType piece) => piece == DefensePieceType.Wolf || piece == DefensePieceType.RootTrap ? 2 : piece == DefensePieceType.Ent ? 4 : 0;
        public static bool IsAllowed(DefenseSlotLayout slot) => slot.Piece == DefensePieceType.Empty || slot.SlotType == DefenseSlotType.Creature && (slot.Piece == DefensePieceType.Wolf || slot.Piece == DefensePieceType.Ent) || slot.SlotType == DefenseSlotType.Trap && slot.Piece == DefensePieceType.RootTrap;
        public static int Used(DefenseLayout layout) { if (layout?.Slots == null) return int.MaxValue; var total = 0; foreach (var slot in layout.Slots) total += Cost(slot.Piece); return total; }
        public static bool IsValid(DefenseLayout layout, out string reason)
        {
            reason = string.Empty;
            if (layout == null || layout.Version != 1 || layout.Slots == null || layout.Slots.Length != 5) { reason = "Choose all five defense slots."; return false; }
            int ents = 0, wolves = 0, traps = 0;
            for (var index = 0; index < layout.Slots.Length; index++)
            {
                var slot = layout.Slots[index];
                var expectedType = index < 3 ? DefenseSlotType.Creature : DefenseSlotType.Trap;
                if (slot.SlotType != expectedType || !IsAllowed(slot)) { reason = "That piece does not fit this slot."; return false; }
                if (slot.Piece == DefensePieceType.Ent) ents++;
                if (slot.Piece == DefensePieceType.Wolf) wolves++;
                if (slot.Piece == DefensePieceType.RootTrap) traps++;
            }
            if (Used(layout) > Budget) { reason = "Threat Budget exceeded."; return false; }
            if (ents != 1) { reason = "Place exactly one Ent."; return false; }
            if (wolves < 1) { reason = "Place at least one Wolf."; return false; }
            if (traps != 1) { reason = "Place exactly one Root Trap."; return false; }
            return true;
        }
    }

    public static class DefenseLayoutSave
    {
        const string Key = "realmraiders.sylvanDefenseLayout.v1";
        public static void Save(DefenseLayout layout) { PlayerPrefs.SetString(Key, JsonUtility.ToJson(layout)); PlayerPrefs.Save(); }
        public static DefenseLayout Load()
        {
            var json = PlayerPrefs.GetString(Key, string.Empty);
            if (string.IsNullOrWhiteSpace(json)) return DefenseLayout.Default();
            try { var layout = JsonUtility.FromJson<DefenseLayout>(json); return DefenseLayoutRules.IsValid(layout, out _) ? layout : DefenseLayout.Default(); }
            catch { return DefenseLayout.Default(); }
        }
        public static void Clear() => PlayerPrefs.DeleteKey(Key);
        public static string KeyForTests => Key;
    }
}
