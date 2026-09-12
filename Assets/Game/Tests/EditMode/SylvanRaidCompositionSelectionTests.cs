using NUnit.Framework;
using RealmRaiders.Core;

namespace RealmRaiders.Tests
{
    public sealed class SylvanRaidCompositionSelectionTests
    {
        [SetUp]
        public void SetUp() => SylvanRaidCompositionSelection.ResetForTests();

        [Test]
        public void SelectionStartsBaselineCyclesExactCatalogueOrderAndWraps()
        {
            SylvanRealmBootstrap.ValidateSelectedCompositionForTests();
            AssertSelection("realmraiders.sylvan-raid.baseline", "Baseline");
            SylvanRaidCompositionSelection.Cycle();
            SylvanRealmBootstrap.ValidateSelectedCompositionForTests();
            AssertSelection("realmraiders.sylvan-raid.wolf-pressure", "Wolf Pressure");
            SylvanRaidCompositionSelection.Cycle();
            SylvanRealmBootstrap.ValidateSelectedCompositionForTests();
            AssertSelection("realmraiders.sylvan-raid.sentinel-escort", "Sentinel Escort");
            SylvanRaidCompositionSelection.Cycle();
            AssertSelection("realmraiders.sylvan-raid.baseline", "Baseline");
        }

        [Test]
        public void UnknownSelectionFailsClosedToBaselineWithoutPersistentState()
        {
            var realm = UnityEngine.PlayerPrefs.GetString("realmraiders.selectedRealm", string.Empty);
            var orientation = UnityEngine.PlayerPrefs.GetString("realmraiders.orientation.v1", string.Empty);
            var control = UnityEngine.PlayerPrefs.GetString("realmraiders.controlStyle.v1", string.Empty);

            SylvanRaidCompositionSelection.Select("unknown.composition");
            AssertSelection("realmraiders.sylvan-raid.baseline", "Baseline");

            Assert.That(UnityEngine.PlayerPrefs.GetString("realmraiders.selectedRealm", string.Empty), Is.EqualTo(realm));
            Assert.That(UnityEngine.PlayerPrefs.GetString("realmraiders.orientation.v1", string.Empty), Is.EqualTo(orientation));
            Assert.That(UnityEngine.PlayerPrefs.GetString("realmraiders.controlStyle.v1", string.Empty), Is.EqualTo(control));
        }

        static void AssertSelection(string compositionId, string displayName)
        {
            Assert.That(SylvanRaidCompositionSelection.CompositionId, Is.EqualTo(compositionId));
            Assert.That(SylvanRaidCompositionSelection.DisplayName, Is.EqualTo(displayName));
        }
    }
}
