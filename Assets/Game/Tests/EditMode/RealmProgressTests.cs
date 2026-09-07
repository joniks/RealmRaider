using NUnit.Framework;
using RealmRaiders.Core;
using RealmRaiders.Raid;
using UnityEngine;

namespace RealmRaiders.Tests
{
    public sealed class RealmProgressTests
    {
        [Test]
        public void RealmProgress_CreditsVictoryAndDefeatFromTheirActualRaidResults()
        {
            WithSavedProgress(() =>
            {
                RealmProgress.ResetForTests();
                RealmProgress.Credit(new RaidResult(true, 115, 2, 4, 3, 46.8f, true));
                var value = RealmProgress.Credit(new RaidResult(false, 25, 0, 1, 2, 18.2f, false));

                Assert.That(value.Gold, Is.EqualTo(140));
                Assert.That(value.RareMaterials, Is.EqualTo(2));
                Assert.That(value.CompletedRaids, Is.EqualTo(2));
                Assert.That(value.Victories, Is.EqualTo(1));
                Assert.That(RealmProgress.StoreCopy(), Is.EqualTo("REALM STORES  •  140 GOLD  •  2 RARE MATERIALS"));
            });
        }

        [Test]
        public void RealmProgress_MalformedOrInvalidDataFallsBackSafely()
        {
            WithSavedProgress(() =>
            {
                PlayerPrefs.SetString(RealmProgress.KeyForTests, "{not valid json");
                var malformed = RealmProgress.Load();
                Assert.That(malformed.Gold, Is.Zero);
                Assert.That(malformed.CompletedRaids, Is.Zero);

                PlayerPrefs.SetString(RealmProgress.KeyForTests, "{\"Version\":99,\"Gold\":500}");
                var invalid = RealmProgress.Load();
                Assert.That(invalid.Gold, Is.Zero);
                Assert.That(invalid.RareMaterials, Is.Zero);
            });
        }

        [Test]
        public void GuardianEntVitalityPurchaseUsesExactCostsAndStopsAtTheThreeRankCap()
        {
            WithSavedProgress(() =>
            {
                RealmProgress.ResetForTests();
                Assert.That(RealmProgress.TryPurchaseGuardianEntVitality(out _), Is.False);
                RealmProgress.Credit(new RaidResult(true, 300, 3, 0, 0, 1, true));

                Assert.That(RealmProgress.TryPurchaseGuardianEntVitality(out var first), Is.True);
                Assert.That(first.Gold, Is.EqualTo(200)); Assert.That(first.RareMaterials, Is.EqualTo(2)); Assert.That(first.GuardianEntVitalityRank, Is.EqualTo(1));
                Assert.That(RealmProgress.TryPurchaseGuardianEntVitality(out _), Is.True);
                Assert.That(RealmProgress.TryPurchaseGuardianEntVitality(out var capped), Is.True);
                Assert.That(capped.Gold, Is.Zero); Assert.That(capped.RareMaterials, Is.Zero); Assert.That(capped.GuardianEntVitalityRank, Is.EqualTo(3));
                Assert.That(RealmProgress.CanPurchaseGuardianEntVitality(), Is.False);
                Assert.That(RealmProgress.TryPurchaseGuardianEntVitality(out var unchanged), Is.False);
                Assert.That(unchanged.GuardianEntVitalityRank, Is.EqualTo(3)); Assert.That(unchanged.Gold, Is.Zero);
            });
        }

        [Test]
        public void GuardianEntVitalityCalculatesTenPercentPerRankFromTheOriginalEntMaximumHealth()
        {
            WithSavedProgress(() =>
            {
                RealmProgress.ResetForTests();
                RealmProgress.Credit(new RaidResult(true, 200, 2, 0, 0, 1, true));
                RealmProgress.TryPurchaseGuardianEntVitality(out _); RealmProgress.TryPurchaseGuardianEntVitality(out _);
                Assert.That(RealmProgress.GuardianEntMaximumHealth(340), Is.EqualTo(408).Within(.01f));
            });
        }

        static void WithSavedProgress(System.Action action)
        {
            var hadValue = PlayerPrefs.HasKey(RealmProgress.KeyForTests);
            var previous = PlayerPrefs.GetString(RealmProgress.KeyForTests, string.Empty);
            try { action(); }
            finally
            {
                if (hadValue) PlayerPrefs.SetString(RealmProgress.KeyForTests, previous);
                else PlayerPrefs.DeleteKey(RealmProgress.KeyForTests);
                PlayerPrefs.Save();
            }
        }
    }
}
