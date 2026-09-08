using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RealmRaiders.UI;
using UnityEngine;

namespace RealmRaiders.Tests
{
    public sealed class ThirdPartyNoticesTests
    {
        [Test]
        public void ReleaseCatalogueHasExactClearedOrderAndCopy()
        {
            var catalogue = ThirdPartyNoticeCatalogue.Release;
            Assert.That(catalogue.UsedFallback, Is.False);
            Assert.That(catalogue.DiagnosticIssued, Is.False);
            Assert.That(catalogue.Entries, Has.Count.EqualTo(3));
            Assert.That(catalogue.Entries[0].Id, Is.EqualTo(ThirdPartyNoticeCatalogue.RequiredId));
            Assert.That(catalogue.Entries[1].Id, Is.EqualTo(ThirdPartyNoticeCatalogue.KenneyUiId));
            Assert.That(catalogue.Entries[2].Id, Is.EqualTo(ThirdPartyNoticeCatalogue.KenneySoundsId));
            Assert.That(catalogue.Entries[0].NoticeClass, Is.EqualTo(ThirdPartyNoticeClass.RequiredAttribution));
            Assert.That(catalogue.Entries[1].NoticeClass, Is.EqualTo(ThirdPartyNoticeClass.VoluntaryProvenance));
            Assert.That(catalogue.Entries[2].NoticeClass, Is.EqualTo(ThirdPartyNoticeClass.VoluntaryProvenance));
            Assert.That(catalogue.Entries[0].Credit, Is.EqualTo(ThirdPartyNoticeCatalogue.RequiredCredit));
            Assert.That(catalogue.BodyCopy, Does.Contain(ThirdPartyNoticeCatalogue.RequiredSourceUrl).And.Contain(ThirdPartyNoticeCatalogue.CcByUrl));
            Assert.That(catalogue.BodyCopy, Does.Contain("UI Pack by Kenney — CC0 1.0 Universal.").And.Contain("Interface Sounds by Kenney — CC0 1.0 Universal.").And.Contain(ThirdPartyNoticeCatalogue.Cc0Url));
            Assert.That(catalogue.BodyCopy, Does.Contain("Attribution is not required"));
            Assert.That(catalogue.BodyCopy.ToLowerInvariant(), Does.Not.Contain("quaternius"));
        }

        [Test]
        public void CatalogueWideFailuresUseOneExactMandatoryFallback()
        {
            var release = ThirdPartyNoticeCatalogue.Release.Entries;
            var invalidClass = Copy(release[0], noticeClass: (ThirdPartyNoticeClass)99);
            var cases = new IEnumerable<ThirdPartyNoticeEntry>[]
            {
                null,
                new ThirdPartyNoticeEntry[0],
                new[] { release[0], Copy(release[0]) },
                new[] { Copy(release[0], id: " ") },
                new[] { invalidClass },
                new[] { release[1], release[2] },
                new[] { Copy(release[0], title: "") },
                new[] { Copy(release[0], sourceUrl: "not-a-url") },
                new[] { Copy(release[0], licenceUrl: "") }
            };

            foreach (var source in cases)
            {
                var warnings = 0;
                var catalogue = ThirdPartyNoticeCatalogue.Create(source, _ => warnings++);
                Assert.That(catalogue.UsedFallback, Is.True);
                Assert.That(catalogue.BodyCopy, Is.EqualTo(ThirdPartyNoticeCatalogue.FallbackCopy));
                Assert.That(catalogue.Entries, Has.Count.EqualTo(1));
                Assert.That(catalogue.Entries[0].Credit, Is.EqualTo(ThirdPartyNoticeCatalogue.RequiredCredit));
                Assert.That(catalogue.BodyCopy, Does.Contain("Additional voluntary provenance entries are unavailable in this build."));
                Assert.That(warnings, Is.EqualTo(1));
            }
        }

        [Test]
        public void MalformedOptionalIsOmittedWithoutLosingValidClearedEntries()
        {
            var release = ThirdPartyNoticeCatalogue.Release.Entries;
            var unknown = new ThirdPartyNoticeEntry("unknown-voluntary", ThirdPartyNoticeClass.VoluntaryProvenance, "Unknown", "Unknown", "CC0", "https://example.invalid/asset", ThirdPartyNoticeCatalogue.Cc0Url, "Unknown credit");
            var cases = new[]
            {
                new[] { release[0], Copy(release[1], sourceUrl: ""), release[2] },
                new[] { release[0], unknown, release[2] }
            };

            foreach (var source in cases)
            {
                var warnings = 0;
                var catalogue = ThirdPartyNoticeCatalogue.Create(source, _ => warnings++);
                Assert.That(catalogue.UsedFallback, Is.False);
                Assert.That(catalogue.HasUnavailableOptionalEntries, Is.True);
                Assert.That(catalogue.Entries, Has.Count.EqualTo(2));
                Assert.That(catalogue.Entries[0].Id, Is.EqualTo(ThirdPartyNoticeCatalogue.RequiredId));
                Assert.That(catalogue.Entries[1].Id, Is.EqualTo(ThirdPartyNoticeCatalogue.KenneySoundsId));
                Assert.That(catalogue.BodyCopy, Does.Contain("Some voluntary provenance entries are unavailable in this build."));
                Assert.That(catalogue.BodyCopy, Does.Not.Contain("UI Pack by Kenney").And.Not.Contain("Unknown credit"));
                Assert.That(warnings, Is.EqualTo(1));
            }
        }

        [Test]
        public void UnclearedQuaterniusEntryIsNeverExposedAndWarnsOnce()
        {
            var release = ThirdPartyNoticeCatalogue.Release.Entries;
            var quaternius = new ThirdPartyNoticeEntry("quaternius-animated-knight", ThirdPartyNoticeClass.VoluntaryProvenance, "Animated Knight Pack", "Quaternius", "CC0 1.0 Universal", "https://quaternius.com/packs/knightcharacter.html", ThirdPartyNoticeCatalogue.Cc0Url, "Uncleared credit");
            var warnings = 0;
            var catalogue = ThirdPartyNoticeCatalogue.Create(new[] { release[0], quaternius, release[1], release[2] }, _ => warnings++);

            Assert.That(catalogue.UsedFallback, Is.False);
            Assert.That(catalogue.Entries, Has.Count.EqualTo(3));
            Assert.That(catalogue.BodyCopy.ToLowerInvariant(), Does.Not.Contain("quaternius"));
            Assert.That(warnings, Is.EqualTo(1));
        }

        [Test]
        public void AuthoringRegistryGateKeepsFourthEntryOutsideRuntimeUntilHumanResolution()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var registry = File.ReadAllText(Path.Combine(projectRoot, "Docs", "THIRD_PARTY_ASSETS.md"));
            var decision = File.ReadAllText(Path.Combine(projectRoot, "Modules", "RealmRaider.Modules", "Design", "ThirdPartyNoticesV1.md"));
            Assert.That(Regex.Matches(registry, "^## ", RegexOptions.Multiline), Has.Count.EqualTo(4));
            Assert.That(registry, Does.Contain("## Quaternius — Animated Knight Pack").And.Contain("**Acquired:** 2026-09-05"));
            Assert.That(decision, Does.Contain("Blocked — do not display until human resolution is recorded"));
            Assert.That(ThirdPartyNoticeCatalogue.Release.Entries, Has.Count.EqualTo(3));
            Assert.That(ThirdPartyNoticeCatalogue.Release.BodyCopy.ToLowerInvariant(), Does.Not.Contain("quaternius"));
        }

        static ThirdPartyNoticeEntry Copy(
            ThirdPartyNoticeEntry source,
            string id = null,
            ThirdPartyNoticeClass? noticeClass = null,
            string title = null,
            string sourceUrl = null,
            string licenceUrl = null)
        {
            return new ThirdPartyNoticeEntry(
                id ?? source.Id,
                noticeClass ?? source.NoticeClass,
                title ?? source.AssetTitle,
                source.Creator,
                source.LicenceName,
                sourceUrl ?? source.SourceUrl,
                licenceUrl ?? source.LicenceUrl,
                source.Credit,
                source.ModificationStatement);
        }
    }
}
