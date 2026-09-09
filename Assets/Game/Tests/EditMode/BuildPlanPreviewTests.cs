using NUnit.Framework;
using RealmRaiders.Core;
using RealmRaiders.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RealmRaiders.Tests
{
    public sealed class BuildPlanPreviewTests
    {
        [Test]
        public void PreviewMapsApproachOrderAndRefreshesExistingNonInteractiveNodes()
        {
            var go = new GameObject("Build Plan Preview Test", typeof(RectTransform), typeof(BuildPlanPreview));
            try
            {
                var preview = go.GetComponent<BuildPlanPreview>();
                var layout = DefenseLayout.Default();
                preview.Initialize();
                preview.Refresh(layout);

                Assert.That(preview.NodeCount, Is.EqualTo(5));
                Assert.That(new[] { preview.LayoutIndexAt(0), preview.LayoutIndexAt(1), preview.LayoutIndexAt(2), preview.LayoutIndexAt(3), preview.LayoutIndexAt(4) },
                    Is.EqualTo(new[] { 3, 0, 1, 4, 2 }));
                Assert.That(preview.PieceAt(0), Is.EqualTo(DefensePieceType.RootTrap));
                Assert.That(preview.NodeTextAt(0), Is.EqualTo("INVADER → ROOT GATE\nROOT TRAP\nMANUAL HOLD TRAP"));
                Assert.That(preview.NodeTextAt(1), Is.EqualTo("OUTER GUARD\nWOLF\nFAST INTERCEPT"));
                Assert.That(preview.NodeTextAt(3), Is.EqualTo("INNER ROOT\nOPEN\nUNASSIGNED"));
                Assert.That(preview.NodeTextAt(4), Is.EqualTo("HEART GUARD\nENT\nPOSSESSABLE GUARDIAN"));
                Assert.That(preview.EndpointText, Is.EqualTo("HEART TREE\nREALM CORE"));

                var originalNodes = new RectTransform[preview.NodeCount];
                for (var index = 0; index < originalNodes.Length; index++) originalNodes[index] = preview.NodeRectAt(index);
                var originalLane = preview.LaneRect;
                var originalEndpoint = preview.EndpointRect;

                layout.Slots[3] = new DefenseSlotLayout(DefenseSlotType.Trap, DefensePieceType.Empty);
                layout.Slots[4] = new DefenseSlotLayout(DefenseSlotType.Trap, DefensePieceType.RootTrap);
                preview.Initialize();
                preview.Refresh(layout);

                Assert.That(preview.PieceAt(0), Is.EqualTo(DefensePieceType.Empty));
                Assert.That(preview.NodeTextAt(0), Does.Contain("ROOT GATE\nOPEN\nUNASSIGNED"));
                Assert.That(preview.PieceAt(3), Is.EqualTo(DefensePieceType.RootTrap));
                Assert.That(preview.NodeTextAt(3), Does.Contain("INNER ROOT\nROOT TRAP\nMANUAL HOLD TRAP"));
                Assert.That(preview.LaneRect, Is.SameAs(originalLane));
                Assert.That(preview.EndpointRect, Is.SameAs(originalEndpoint));
                for (var index = 0; index < originalNodes.Length; index++) Assert.That(preview.NodeRectAt(index), Is.SameAs(originalNodes[index]));

                Assert.That(preview.GetComponentsInChildren<Image>(true), Has.Length.EqualTo(7), "One lane, five nodes and one endpoint are built once.");
                Assert.That(preview.GetComponentsInChildren<Text>(true), Has.Length.EqualTo(6));
                Assert.That(preview.GetComponentsInChildren<Selectable>(true), Is.Empty);
                Assert.That(preview.GetComponentsInChildren<UiPointerOwnership>(true), Is.Empty);
                Assert.That(preview.GetComponentsInChildren<Canvas>(true), Is.Empty);
                Assert.That(preview.GetComponentsInChildren<EventSystem>(true), Is.Empty);
                Assert.That(preview.GetComponentsInChildren<AudioListener>(true), Is.Empty);
                foreach (var graphic in preview.GetComponentsInChildren<Graphic>(true)) Assert.That(graphic.raycastTarget, Is.False, graphic.name);
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}
