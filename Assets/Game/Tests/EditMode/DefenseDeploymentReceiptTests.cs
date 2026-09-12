using NUnit.Framework;
using RealmRaiders.Core;
using RealmRaiders.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RealmRaiders.Tests
{
    public sealed class DefenseDeploymentReceiptTests
    {
        [Test]
        public void DeploymentReceipt_SnapshotsActualPiecesAndRemainsPresentationOnlyInBothLayouts()
        {
            var deployed = new[]
            {
                DefensePieceType.Wolf,
                DefensePieceType.Ent,
                DefensePieceType.Empty,
                DefensePieceType.RootTrap,
                DefensePieceType.Empty
            };
            var layout = new DefenseLayout(new[]
            {
                new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Wolf),
                new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Ent),
                new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Empty),
                new DefenseSlotLayout(DefenseSlotType.Trap, DefensePieceType.RootTrap),
                new DefenseSlotLayout(DefenseSlotType.Trap, DefensePieceType.Empty)
            });
            Assert.That(DefenseTradeoffSelection.TryResolve(layout, out var tradeoff), Is.True);
            var data = new DefenseDeploymentReceiptData(deployed, tradeoff);
            deployed[0] = DefensePieceType.Empty;
            var go = new GameObject(DefenseDeploymentReceipt.ObjectName, typeof(RectTransform), typeof(Text), typeof(DefenseDeploymentReceipt));
            try
            {
                var receipt = go.GetComponent<DefenseDeploymentReceipt>();
                receipt.Initialize(data);

                Assert.That(receipt.Copy, Is.EqualTo(
                    "DEPLOYED: KEEPER RESERVE — 1 WOLF SACRIFICED • 45 SEC CONTROL\n" +
                    "ROOT GATE: ROOT TRAP  →  OUTER GUARD: WOLF  →  MID GUARD: GUARDIAN ENT\n" +
                    "INNER ROOT: OPEN  →  HEART GUARD: OPEN  →  HEART TREE"));
                Assert.That(receipt.PieceAtSlot(0), Is.EqualTo(DefensePieceType.Wolf), "The receipt must own a deployment snapshot, not a mutable layout reference.");
                Assert.That(receipt.PieceAtSlot(-1), Is.EqualTo(DefensePieceType.Empty));
                Assert.That(receipt.PieceAtSlot(5), Is.EqualTo(DefensePieceType.Empty));
                Assert.That(receipt.TradeoffId, Is.EqualTo("KEEPER_RESERVE"));
                Assert.That(receipt.PossessionEnergyMaximumSeconds, Is.EqualTo(45));
                Assert.That(receipt.Visible, Is.False);
                Assert.That(receipt.RaycastTarget, Is.False);
                Assert.That(receipt.GetComponentsInChildren<Button>(true), Is.Empty);
                Assert.That(receipt.GetComponentsInChildren<Selectable>(true), Is.Empty);
                Assert.That(receipt.GetComponentsInChildren<UiPointerOwnership>(true), Is.Empty);
                Assert.That(receipt.GetComponentsInChildren<Canvas>(true), Is.Empty);
                Assert.That(receipt.GetComponentsInChildren<EventSystem>(true), Is.Empty);
                Assert.That(receipt.GetComponentsInChildren<AudioListener>(true), Is.Empty);
                foreach (var graphic in receipt.GetComponentsInChildren<Graphic>(true)) Assert.That(graphic.raycastTarget, Is.False, graphic.name);

                receipt.ApplyOrientation(PrototypeOrientation.Portrait);
                AssertContained(DesignRect(receipt.Rect, new Vector2(1080, 1920)), new Vector2(1080, 1920));
                receipt.ApplyOrientation(PrototypeOrientation.Landscape);
                AssertContained(DesignRect(receipt.Rect, new Vector2(1920, 1080)), new Vector2(1920, 1080));
            }
            finally { Object.DestroyImmediate(go); }
        }

        static void AssertContained(Rect bounds, Vector2 reference)
        {
            Assert.That(bounds.xMin, Is.GreaterThanOrEqualTo(0));
            Assert.That(bounds.yMin, Is.GreaterThanOrEqualTo(0));
            Assert.That(bounds.xMax, Is.LessThanOrEqualTo(reference.x));
            Assert.That(bounds.yMax, Is.LessThanOrEqualTo(reference.y));
        }

        static Rect DesignRect(RectTransform rect, Vector2 parentSize)
        {
            var anchorMin = Vector2.Scale(rect.anchorMin, parentSize);
            var anchorSize = Vector2.Scale(rect.anchorMax - rect.anchorMin, parentSize);
            var size = anchorSize + rect.sizeDelta;
            var pivotPoint = anchorMin + Vector2.Scale(anchorSize, rect.pivot) + rect.anchoredPosition;
            return new Rect(pivotPoint - Vector2.Scale(size, rect.pivot), size);
        }
    }
}
