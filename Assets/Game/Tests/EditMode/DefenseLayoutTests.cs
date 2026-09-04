using NUnit.Framework;
using RealmRaiders.Core;
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
    }
}
