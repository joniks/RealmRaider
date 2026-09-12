using NUnit.Framework;
using RealmRaiders.Core;
using RealmRaiders.UI;
using UnityEngine;

namespace RealmRaiders.Tests
{
    public sealed class DefenseLayoutTests
    {
        [Test] public void DefaultLayoutIsValidAndCostsTen() { var layout = DefenseLayout.Default(); Assert.That(DefenseLayoutRules.IsValid(layout, out _), Is.True); Assert.That(DefenseLayoutRules.Used(layout), Is.EqualTo(10)); }
        [Test] public void OverBudgetLayoutIsRejected() { var layout = DefenseLayout.Default(); layout.Slots[4] = new DefenseSlotLayout(DefenseSlotType.Trap, DefensePieceType.RootTrap); Assert.That(DefenseLayoutRules.IsValid(layout, out _), Is.False); }
        [Test] public void InvalidCompositionIsRejected() { var layout = DefenseLayout.Default(); layout.Slots[2] = new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Wolf); Assert.That(DefenseLayoutRules.IsValid(layout, out var reason), Is.False); Assert.That(reason, Does.Contain("Ent")); layout = DefenseLayout.Default(); layout.Slots[0] = new DefenseSlotLayout(DefenseSlotType.Trap, DefensePieceType.Empty); Assert.That(DefenseLayoutRules.IsValid(layout, out reason), Is.False); Assert.That(reason, Does.Contain("fit")); }
        [Test] public void SaveLoadPreservesOrderAndContents() { var previous = PlayerPrefs.GetString(DefenseLayoutSave.KeyForTests, null); try { var custom = DefenseLayout.Default(); custom.Slots[0] = new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Ent); custom.Slots[2] = new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Wolf); DefenseLayoutSave.Save(custom); var loaded = DefenseLayoutSave.Load(); Assert.That(loaded.Slots[0].Piece, Is.EqualTo(DefensePieceType.Ent)); Assert.That(loaded.Slots[2].Piece, Is.EqualTo(DefensePieceType.Wolf)); } finally { if (previous == null) PlayerPrefs.DeleteKey(DefenseLayoutSave.KeyForTests); else PlayerPrefs.SetString(DefenseLayoutSave.KeyForTests, previous); PlayerPrefs.Save(); } }
        [Test] public void MalformedJsonFallsBackToDefault() { var previous = PlayerPrefs.GetString(DefenseLayoutSave.KeyForTests, null); try { PlayerPrefs.SetString(DefenseLayoutSave.KeyForTests, "not-json"); var loaded = DefenseLayoutSave.Load(); Assert.That(DefenseLayoutRules.IsValid(loaded, out _), Is.True); Assert.That(loaded.Slots[0].Piece, Is.EqualTo(DefensePieceType.Wolf)); } finally { if (previous == null) PlayerPrefs.DeleteKey(DefenseLayoutSave.KeyForTests); else PlayerPrefs.SetString(DefenseLayoutSave.KeyForTests, previous); PlayerPrefs.Save(); } }
        [Test]
        public void DefensePlanCopyUsesFixedApproachOrderRolesAndExistingCosts()
        {
            var layout = DefenseLayout.Default();
            Assert.That(BuildHUD.FormatSlotCopy(0, layout.Slots[0]), Is.EqualTo("OUTER GUARD\nWOLF • FAST INTERCEPT • 2 THREAT"));
            Assert.That(BuildHUD.FormatSlotCopy(2, layout.Slots[2]), Does.Contain("ENT • POSSESSABLE GUARDIAN • 4 THREAT"));
            var defaultPlan = BuildHUD.FormatDefensePlan(layout);
            Assert.That(defaultPlan, Does.Contain("ROOT GATE: ROOT TRAP  →  OUTER GUARD: WOLF  →  MID GUARD: WOLF"));
            Assert.That(defaultPlan, Does.Contain("HEART GUARD: ENT [POSSESSABLE]  →  HEART TREE"));

            layout.Slots[0] = new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Ent);
            layout.Slots[2] = new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Wolf);
            var changedPlan = BuildHUD.FormatDefensePlan(layout);
            Assert.That(changedPlan, Does.Contain("OUTER GUARD: ENT [POSSESSABLE]"));
            Assert.That(changedPlan, Does.Contain("HEART GUARD: WOLF"));
        }

        [Test]
        public void DefenseTradeoff_ResolvesPackAndEveryReserveCreaturePermutationAndRejectsInvalidDraft()
        {
            var pack = DefenseLayout.Default();
            AssertTradeoff(pack, "PACK_PRESSURE", 30, "PACK PRESSURE", "2 WOLVES", "30 SEC CONTROL");

            for (var openSlot = 0; openSlot < 3; openSlot++)
            {
                for (var entSlot = 0; entSlot < 3; entSlot++)
                {
                    if (entSlot == openSlot) continue;
                    var wolfSlot = 3 - openSlot - entSlot;
                    var reserve = new DefenseLayout(new[]
                    {
                        new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Empty),
                        new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Empty),
                        new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Empty),
                        new DefenseSlotLayout(DefenseSlotType.Trap, DefensePieceType.RootTrap),
                        new DefenseSlotLayout(DefenseSlotType.Trap, DefensePieceType.Empty)
                    });
                    reserve.Slots[entSlot] = new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Ent);
                    reserve.Slots[wolfSlot] = new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Wolf);
                    AssertTradeoff(reserve, "KEEPER_RESERVE", 45, "KEEPER RESERVE", "1 WOLF SACRIFICED", "45 SEC CONTROL");
                }
            }

            var invalid = DefenseLayout.Default();
            invalid.Slots[2] = new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Empty);
            Assert.That(DefenseLayoutRules.IsValid(invalid, out _), Is.False);
            Assert.That(DefenseTradeoffSelection.TryResolve(invalid, out _), Is.False);
            Assert.That(DefenseTradeoffSelection.ResolveOrDefault(invalid).TradeoffId, Is.EqualTo("PACK_PRESSURE"));
            Assert.That(BuildHUD.FormatTradeoffCopy(invalid), Does.Contain("TRADEOFF — COMPLETE A VALID PLAN"));
        }

        [Test]
        public void DefenseTradeoff_EvaluationDoesNotPersistDraftAndMalformedSaveFallsBackToPackPressure()
        {
            var hadLayout = PlayerPrefs.HasKey(DefenseLayoutSave.KeyForTests);
            var previous = PlayerPrefs.GetString(DefenseLayoutSave.KeyForTests, string.Empty);
            try
            {
                const string sentinel = "unsaved-draft-sentinel";
                PlayerPrefs.SetString(DefenseLayoutSave.KeyForTests, sentinel);
                var reserve = new DefenseLayout(new[]
                {
                    new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Wolf),
                    new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Ent),
                    new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Empty),
                    new DefenseSlotLayout(DefenseSlotType.Trap, DefensePieceType.RootTrap),
                    new DefenseSlotLayout(DefenseSlotType.Trap, DefensePieceType.Empty)
                });
                Assert.That(BuildHUD.FormatTradeoffCopy(reserve), Does.Contain("KEEPER RESERVE").And.Contain("45 SEC CONTROL"));
                Assert.That(PlayerPrefs.GetString(DefenseLayoutSave.KeyForTests), Is.EqualTo(sentinel));

                var loaded = DefenseLayoutSave.Load();
                var fallback = DefenseTradeoffSelection.ResolveOrDefault(loaded);
                Assert.That(fallback.TradeoffId, Is.EqualTo("PACK_PRESSURE"));
                Assert.That(fallback.PossessionEnergyMaximumSeconds, Is.EqualTo(30));
            }
            finally
            {
                if (hadLayout) PlayerPrefs.SetString(DefenseLayoutSave.KeyForTests, previous);
                else PlayerPrefs.DeleteKey(DefenseLayoutSave.KeyForTests);
                PlayerPrefs.Save();
            }
        }

        static void AssertTradeoff(DefenseLayout layout, string id, int seconds, params string[] copyParts)
        {
            Assert.That(DefenseLayoutRules.IsValid(layout, out _), Is.True);
            Assert.That(DefenseTradeoffSelection.TryResolve(layout, out var selection), Is.True);
            Assert.That(selection.TradeoffId, Is.EqualTo(id));
            Assert.That(selection.PossessionEnergyMaximumSeconds, Is.EqualTo(seconds));
            foreach (var part in copyParts) Assert.That(BuildHUD.FormatTradeoffCopy(layout), Does.Contain(part));
        }
    }
}
