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
