using NUnit.Framework;
using RealmRaiders.Core;
using UnityEngine;

namespace RealmRaiders.Tests
{
    public sealed class InfernalRaidCompositionSelectionTests
    {
        [SetUp]
        public void SetUp() => InfernalRaidCompositionSelection.ResetForTests();

        [Test]
        public void SelectionStartsBruteFinaleCyclesExactPlayerOrderAndWraps()
        {
            AssertSelection("realmraiders.infernal-raid.brute-finale", "Brute Finale",
                "2 HELLHOUNDS + BRUTE • OPTIONAL FLAME BYPASS • ~80 SEC");
            InfernalRaidBootstrap.ValidateSelectedRecipeForTests();
            InfernalRaidCompositionSelection.Cycle();
            AssertSelection("realmraiders.infernal-raid.entry-trial", "Entry Trial", "1 HELLHOUND • ~35 SEC");
            InfernalRaidBootstrap.ValidateSelectedRecipeForTests();
            InfernalRaidCompositionSelection.Cycle();
            AssertSelection("realmraiders.infernal-raid.risk-route", "Risk Route",
                "2 HELLHOUNDS • OPTIONAL FLAME BYPASS • ~55 SEC");
            InfernalRaidBootstrap.ValidateSelectedRecipeForTests();
            InfernalRaidCompositionSelection.Cycle();
            AssertSelection("realmraiders.infernal-raid.brute-finale", "Brute Finale",
                "2 HELLHOUNDS + BRUTE • OPTIONAL FLAME BYPASS • ~80 SEC");
        }

        [Test]
        public void UnknownSelectionFailsClosedWithoutChangingPersistentPreferences()
        {
            var realm = PlayerPrefs.GetString("realmraiders.selectedRealm", string.Empty);
            var orientation = PlayerPrefs.GetString("realmraiders.orientation.v1", string.Empty);
            var control = PlayerPrefs.GetString("realmraiders.controlStyle.v1", string.Empty);

            InfernalRaidCompositionSelection.Cycle();
            InfernalRaidCompositionSelection.Select("unknown.composition");
            AssertSelection("realmraiders.infernal-raid.brute-finale", "Brute Finale",
                "2 HELLHOUNDS + BRUTE • OPTIONAL FLAME BYPASS • ~80 SEC");

            Assert.That(PlayerPrefs.GetString("realmraiders.selectedRealm", string.Empty), Is.EqualTo(realm));
            Assert.That(PlayerPrefs.GetString("realmraiders.orientation.v1", string.Empty), Is.EqualTo(orientation));
            Assert.That(PlayerPrefs.GetString("realmraiders.controlStyle.v1", string.Empty), Is.EqualTo(control));
        }

        static void AssertSelection(string id, string displayName, string summary)
        {
            Assert.That(InfernalRaidCompositionSelection.CompositionId, Is.EqualTo(id));
            Assert.That(InfernalRaidCompositionSelection.DisplayName, Is.EqualTo(displayName));
            Assert.That(InfernalRaidCompositionSelection.TacticalSummary, Is.EqualTo(summary));
        }
    }
}
