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
    }
}
