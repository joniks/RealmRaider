using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RealmRaiders.UI
{
    public enum ThirdPartyNoticeClass
    {
        RequiredAttribution,
        VoluntaryProvenance
    }

    public sealed class ThirdPartyNoticeEntry
    {
        public string Id { get; }
        public ThirdPartyNoticeClass NoticeClass { get; }
        public string AssetTitle { get; }
        public string Creator { get; }
        public string LicenceName { get; }
        public string SourceUrl { get; }
        public string LicenceUrl { get; }
        public string Credit { get; }
        public string ModificationStatement { get; }

        public ThirdPartyNoticeEntry(
            string id,
            ThirdPartyNoticeClass noticeClass,
            string assetTitle,
            string creator,
            string licenceName,
            string sourceUrl,
            string licenceUrl,
            string credit,
            string modificationStatement = "")
        {
            Id = id;
            NoticeClass = noticeClass;
            AssetTitle = assetTitle;
            Creator = creator;
            LicenceName = licenceName;
            SourceUrl = sourceUrl;
            LicenceUrl = licenceUrl;
            Credit = credit;
            ModificationStatement = modificationStatement;
        }
    }

    /// <summary>Explicit, validated release snapshot. It performs no discovery or I/O.</summary>
    public sealed class ThirdPartyNoticeCatalogue
    {
        public const string RequiredId = "3drt-fantasy-warrior";
        public const string KenneyUiId = "kenney-ui-pack";
        public const string KenneySoundsId = "kenney-interface-sounds";
        public const string RequiredCredit = "“3DRT - Fantasy Warrior” by 3DRT.com is licensed under CC BY 4.0.";
        public const string RequiredSourceUrl = "https://sketchfab.com/3d-models/3drt-fantasy-warrior-d39a0dee0f054c21b6751f7821aa7a8e";
        public const string CcByUrl = "https://creativecommons.org/licenses/by/4.0/";
        public const string Cc0Url = "https://creativecommons.org/publicdomain/zero/1.0/legalcode";
        public const string RequiredChanges = "The original archive, FBX, and texture are retained unchanged. Realm Raiders applies Unity import settings and uses the asset as visual-only presentation.";
        public const string FallbackCopy = "REQUIRED ATTRIBUTION\n\n“3DRT - Fantasy Warrior” by 3DRT.com is licensed under CC BY 4.0.\n\nSource:\nhttps://sketchfab.com/3d-models/3drt-fantasy-warrior-d39a0dee0f054c21b6751f7821aa7a8e\n\nLicence:\nhttps://creativecommons.org/licenses/by/4.0/\n\nAdditional voluntary provenance entries are unavailable in this build.";

        static readonly ThirdPartyNoticeEntry required = new(
            RequiredId,
            ThirdPartyNoticeClass.RequiredAttribution,
            "3DRT - Fantasy Warrior",
            "3DRT.com",
            "CC BY 4.0",
            RequiredSourceUrl,
            CcByUrl,
            RequiredCredit,
            RequiredChanges);

        static readonly ThirdPartyNoticeEntry kenneyUi = new(
            KenneyUiId,
            ThirdPartyNoticeClass.VoluntaryProvenance,
            "UI Pack",
            "Kenney",
            "CC0 1.0 Universal",
            "https://kenney.nl/assets/ui-pack",
            Cc0Url,
            "UI Pack by Kenney — CC0 1.0 Universal.");

        static readonly ThirdPartyNoticeEntry kenneySounds = new(
            KenneySoundsId,
            ThirdPartyNoticeClass.VoluntaryProvenance,
            "Interface Sounds",
            "Kenney",
            "CC0 1.0 Universal",
            "https://kenney.nl/assets/interface-sounds",
            Cc0Url,
            "Interface Sounds by Kenney — CC0 1.0 Universal.");

        static readonly ThirdPartyNoticeCatalogue release = Create(new[] { required, kenneyUi, kenneySounds });
        readonly IReadOnlyList<ThirdPartyNoticeEntry> entries;

        public static ThirdPartyNoticeCatalogue Release => release;
        public IReadOnlyList<ThirdPartyNoticeEntry> Entries => entries;
        public string BodyCopy { get; }
        public bool UsedFallback { get; }
        public bool HasUnavailableOptionalEntries { get; }
        public bool DiagnosticIssued { get; }

        ThirdPartyNoticeCatalogue(IReadOnlyList<ThirdPartyNoticeEntry> validated, string copy, bool fallback, bool optionalUnavailable, bool diagnostic)
        {
            entries = validated;
            BodyCopy = copy;
            UsedFallback = fallback;
            HasUnavailableOptionalEntries = optionalUnavailable;
            DiagnosticIssued = diagnostic;
        }

        public static ThirdPartyNoticeCatalogue Create(IEnumerable<ThirdPartyNoticeEntry> source, Action<string> warning = null)
        {
            warning ??= message => Debug.LogWarning(message);
            if (source == null) return Fallback(warning);

            var supplied = new List<ThirdPartyNoticeEntry>();
            try
            {
                foreach (var entry in source) supplied.Add(entry);
            }
            catch
            {
                return Fallback(warning);
            }
            if (supplied.Count == 0) return Fallback(warning);

            var retained = new List<ThirdPartyNoticeEntry>();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            var optionalUnavailable = false;
            var issue = false;
            var lastOrder = -1;
            foreach (var entry in supplied)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.Id) || !ids.Add(entry.Id)) return Fallback(warning);
                if (entry.NoticeClass != ThirdPartyNoticeClass.RequiredAttribution && entry.NoticeClass != ThirdPartyNoticeClass.VoluntaryProvenance) return Fallback(warning);
                if (MentionsBlockedQuaternius(entry)) { issue = true; continue; }

                var order = EntryOrder(entry.Id);
                if (entry.NoticeClass == ThirdPartyNoticeClass.RequiredAttribution)
                {
                    if (order < 0 || order <= lastOrder) return Fallback(warning);
                    lastOrder = order;
                    if (entry.Id != RequiredId || !MatchesRequired(entry)) return Fallback(warning);
                }
                else
                {
                    if (entry.Id == RequiredId) return Fallback(warning);
                    if (order < 0) { optionalUnavailable = true; issue = true; continue; }
                    if (order <= lastOrder) return Fallback(warning);
                    lastOrder = order;
                    if (!MatchesOptional(entry)) { optionalUnavailable = true; issue = true; continue; }
                }
                retained.Add(entry);
            }

            if (retained.Count == 0 || retained[0].Id != RequiredId) return Fallback(warning);
            if (!Contains(retained, KenneyUiId) || !Contains(retained, KenneySoundsId)) { optionalUnavailable = true; issue = true; }
            if (issue) warning("Third-party notices omitted unavailable or uncleared entries.");

            var immutable = Array.AsReadOnly(retained.ToArray());
            return new ThirdPartyNoticeCatalogue(immutable, BuildCopy(retained, optionalUnavailable), false, optionalUnavailable, issue);
        }

        static ThirdPartyNoticeCatalogue Fallback(Action<string> warning)
        {
            warning("Third-party notices used the required attribution fallback.");
            return new ThirdPartyNoticeCatalogue(Array.AsReadOnly(new[] { required }), FallbackCopy, true, true, true);
        }

        static bool MatchesRequired(ThirdPartyNoticeEntry entry) =>
            entry.NoticeClass == ThirdPartyNoticeClass.RequiredAttribution &&
            entry.AssetTitle == required.AssetTitle && entry.Creator == required.Creator && entry.LicenceName == required.LicenceName &&
            entry.SourceUrl == RequiredSourceUrl && entry.LicenceUrl == CcByUrl && entry.Credit == RequiredCredit &&
            entry.ModificationStatement == RequiredChanges && ValidUrl(entry.SourceUrl) && ValidUrl(entry.LicenceUrl);

        static bool MatchesOptional(ThirdPartyNoticeEntry entry)
        {
            var expected = entry.Id == KenneyUiId ? kenneyUi : entry.Id == KenneySoundsId ? kenneySounds : null;
            return expected != null && entry.NoticeClass == ThirdPartyNoticeClass.VoluntaryProvenance &&
                   entry.AssetTitle == expected.AssetTitle && entry.Creator == expected.Creator && entry.LicenceName == expected.LicenceName &&
                   entry.SourceUrl == expected.SourceUrl && entry.LicenceUrl == expected.LicenceUrl && entry.Credit == expected.Credit &&
                   string.IsNullOrEmpty(entry.ModificationStatement) && ValidUrl(entry.SourceUrl) && ValidUrl(entry.LicenceUrl);
        }

        static bool ValidUrl(string value) => Uri.TryCreate(value, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
        static int EntryOrder(string id) => id == RequiredId ? 0 : id == KenneyUiId ? 1 : id == KenneySoundsId ? 2 : -1;
        static bool Contains(List<ThirdPartyNoticeEntry> entries, string id) { foreach (var entry in entries) if (entry.Id == id) return true; return false; }

        static bool MentionsBlockedQuaternius(ThirdPartyNoticeEntry entry) =>
            ContainsIgnoreCase(entry.Id, "quaternius") || ContainsIgnoreCase(entry.AssetTitle, "quaternius") || ContainsIgnoreCase(entry.Creator, "quaternius");

        static bool ContainsIgnoreCase(string value, string fragment) => !string.IsNullOrEmpty(value) && value.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0;

        static string BuildCopy(List<ThirdPartyNoticeEntry> entries, bool optionalUnavailable)
        {
            var mandatory = entries[0];
            var copy = new StringBuilder(1024);
            copy.Append("REQUIRED ATTRIBUTION\n\n").Append(mandatory.Credit)
                .Append("\n\nSource:\n").Append(mandatory.SourceUrl)
                .Append("\n\nLicence:\n").Append(mandatory.LicenceUrl)
                .Append("\n\nChanges: ").Append(mandatory.ModificationStatement)
                .Append("\n\nVOLUNTARY PROVENANCE CREDITS\n\n")
                .Append("The following assets are CC0 1.0 Universal. Attribution is not required; Realm Raiders lists them voluntarily for provenance.");
            foreach (var entry in entries)
            {
                if (entry.NoticeClass != ThirdPartyNoticeClass.VoluntaryProvenance) continue;
                copy.Append("\n\n").Append(entry.Credit).Append("\nSource: ").Append(entry.SourceUrl);
            }
            if (optionalUnavailable) copy.Append("\n\nSome voluntary provenance entries are unavailable in this build.");
            copy.Append("\n\nCC0 legal code:\n").Append(Cc0Url);
            return copy.ToString();
        }
    }
}
