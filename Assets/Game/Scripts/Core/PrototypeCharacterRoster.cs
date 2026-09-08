using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RealmRaiders.Characters;
using RealmRaiders.Modules;
using RealmRaiders.Modules.StarterCharacterCatalog;

namespace RealmRaiders.Core
{
    public sealed class PrototypeCharacterRosterEntry
    {
        public PrototypeCharacterRosterEntry(string stableId, string displayName, CharacterVisualFamily visualFamily, string visualProfileKey)
        {
            StableId = stableId;
            DisplayName = displayName;
            VisualFamily = visualFamily;
            VisualProfileKey = visualProfileKey;
        }

        public string StableId { get; }
        public string DisplayName { get; }
        public CharacterVisualFamily VisualFamily { get; }
        public string VisualProfileKey { get; }
    }

    /// <summary>Explicit, immutable host snapshot of the five starter character archetypes.</summary>
    public sealed class PrototypeCharacterRoster
    {
        public const string BloodKnightId = "realmraiders.blood-knight";
        public const string GuardianEntId = "realmraiders.guardian-ent";
        public const string SylvanWolfId = "realmraiders.sylvan-wolf";
        public const string InfernalBruteId = "realmraiders.infernal-brute";
        public const string HellhoundId = "realmraiders.hellhound";

        static readonly PrototypeCharacterRosterEntry[] ExpectedEntries =
        {
            new(BloodKnightId, "Blood Knight", CharacterVisualFamily.Humanoid, "realmraiders.blood-knight.3drt-baseline"),
            new(GuardianEntId, "Guardian Ent", CharacterVisualFamily.LargeCreature, "realmraiders.guardian-ent.prototype"),
            new(SylvanWolfId, "Sylvan Wolf", CharacterVisualFamily.Beast, "realmraiders.sylvan-wolf.prototype"),
            new(InfernalBruteId, "Infernal Brute", CharacterVisualFamily.LargeCreature, "realmraiders.infernal-brute.prototype"),
            new(HellhoundId, "Hellhound", CharacterVisualFamily.Beast, "realmraiders.hellhound.prototype")
        };

        static readonly Lazy<PrototypeCharacterRoster> LazyCurrent = new(CreateStarterRoster);
        readonly IReadOnlyList<PrototypeCharacterRosterEntry> entries;
        readonly Dictionary<string, PrototypeCharacterRosterEntry> byStableId;

        public PrototypeCharacterRoster(IReadOnlyList<PrototypeCharacterRosterEntry> source) : this(source, "explicit") { }

        PrototypeCharacterRoster(IReadOnlyList<PrototypeCharacterRosterEntry> source, string sourceModuleId)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (source.Count != ExpectedEntries.Length) throw new InvalidOperationException($"Starter character roster must contain exactly {ExpectedEntries.Length} entries; received {source.Count}.");

            var snapshot = new PrototypeCharacterRosterEntry[source.Count];
            var ids = new HashSet<string>(StringComparer.Ordinal);
            var profiles = new HashSet<string>(StringComparer.Ordinal);
            byStableId = new Dictionary<string, PrototypeCharacterRosterEntry>(StringComparer.Ordinal);
            for (var index = 0; index < source.Count; index++)
            {
                var entry = source[index] ?? throw new InvalidOperationException($"Starter character roster entry {index} is null.");
                RequireValue(entry.StableId, "stable ID", index);
                RequireValue(entry.DisplayName, "display name", index);
                RequireValue(entry.VisualProfileKey, "visual-profile key", index);
                if (!ids.Add(entry.StableId)) throw new InvalidOperationException($"Starter character roster contains duplicate stable ID '{entry.StableId}'.");
                if (!profiles.Add(entry.VisualProfileKey)) throw new InvalidOperationException($"Starter character roster contains duplicate visual-profile key '{entry.VisualProfileKey}'.");

                var immutable = new PrototypeCharacterRosterEntry(entry.StableId, entry.DisplayName, entry.VisualFamily, entry.VisualProfileKey);
                ValidateExpected(index, immutable);
                snapshot[index] = immutable;
                byStableId.Add(immutable.StableId, immutable);
            }

            SourceModuleId = sourceModuleId;
            entries = new ReadOnlyCollection<PrototypeCharacterRosterEntry>(snapshot);
        }

        public static PrototypeCharacterRoster Current => LazyCurrent.Value;
        public static string ExplicitProviderTypeName => typeof(StarterCharacterCatalogProvider).FullName;
        public string SourceModuleId { get; }
        public IReadOnlyList<PrototypeCharacterRosterEntry> Entries => entries;
        public PrototypeCharacterRosterEntry this[int index] => entries[index];

        public bool TryGet(string stableId, out PrototypeCharacterRosterEntry entry)
        {
            entry = null;
            return stableId != null && byStableId.TryGetValue(stableId, out entry);
        }

        public PrototypeCharacterRosterEntry GetRequired(string stableId)
        {
            if (stableId != null && byStableId.TryGetValue(stableId, out var entry)) return entry;
            throw new KeyNotFoundException($"Unknown starter character stable ID '{stableId ?? "<null>"}'. Lookup is case-sensitive.");
        }

        static PrototypeCharacterRoster CreateStarterRoster()
        {
            ICharacterCatalogProvider provider = new StarterCharacterCatalogProvider();
            if (provider.ModuleId != StarterCharacterCatalogProvider.StableModuleId)
                throw new InvalidOperationException($"Starter character provider module ID '{provider.ModuleId}' does not match '{StarterCharacterCatalogProvider.StableModuleId}'.");
            var catalogue = provider.GetCatalogueEntries() ?? throw new InvalidOperationException("Starter character provider returned a null catalogue.");
            var mapped = new PrototypeCharacterRosterEntry[catalogue.Count];
            for (var index = 0; index < catalogue.Count; index++)
            {
                var entry = catalogue[index] ?? throw new InvalidOperationException($"Starter character provider entry {index} is null.");
                mapped[index] = new PrototypeCharacterRosterEntry(entry.StableId, entry.DisplayName, ConvertFamily(entry.BodyFamily), entry.VisualProfileKey);
            }
            return new PrototypeCharacterRoster(mapped, provider.ModuleId);
        }

        static CharacterVisualFamily ConvertFamily(CharacterBodyFamily family) => family switch
        {
            CharacterBodyFamily.Humanoid => CharacterVisualFamily.Humanoid,
            CharacterBodyFamily.LargeCreature => CharacterVisualFamily.LargeCreature,
            CharacterBodyFamily.Beast => CharacterVisualFamily.Beast,
            _ => throw new InvalidOperationException($"Starter character catalogue uses unsupported body family '{family}'.")
        };

        static void ValidateExpected(int index, PrototypeCharacterRosterEntry entry)
        {
            var expected = ExpectedEntries[index];
            if (!IsKnownId(entry.StableId)) throw new InvalidOperationException($"Starter character roster contains unknown stable ID '{entry.StableId}'.");
            if (entry.StableId != expected.StableId) throw new InvalidOperationException($"Starter character roster entry {index} must be '{expected.StableId}', not '{entry.StableId}'.");
            if (entry.DisplayName != expected.DisplayName || entry.VisualFamily != expected.VisualFamily || entry.VisualProfileKey != expected.VisualProfileKey)
                throw new InvalidOperationException($"Starter character roster data for '{entry.StableId}' does not match its approved display name, family, and visual-profile key.");
        }

        static bool IsKnownId(string stableId)
        {
            foreach (var expected in ExpectedEntries) if (expected.StableId == stableId) return true;
            return false;
        }

        static void RequireValue(string value, string field, int index)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException($"Starter character roster entry {index} has no {field}.");
        }
    }
}
