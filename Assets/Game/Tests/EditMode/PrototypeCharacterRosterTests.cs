using System;
using System.Collections.Generic;
using NUnit.Framework;
using RealmRaiders.Characters;
using RealmRaiders.Core;

namespace RealmRaiders.Tests
{
    public sealed class PrototypeCharacterRosterTests
    {
        static readonly ExpectedEntry[] Expected =
        {
            new(PrototypeCharacterRoster.BloodKnightId, "Blood Knight", CharacterVisualFamily.Humanoid, "realmraiders.blood-knight.3drt-baseline"),
            new(PrototypeCharacterRoster.GuardianEntId, "Guardian Ent", CharacterVisualFamily.LargeCreature, "realmraiders.guardian-ent.prototype"),
            new(PrototypeCharacterRoster.SylvanWolfId, "Sylvan Wolf", CharacterVisualFamily.Beast, "realmraiders.sylvan-wolf.prototype"),
            new(PrototypeCharacterRoster.InfernalBruteId, "Infernal Brute", CharacterVisualFamily.LargeCreature, "realmraiders.infernal-brute.prototype"),
            new(PrototypeCharacterRoster.HellhoundId, "Hellhound", CharacterVisualFamily.Beast, "realmraiders.hellhound.prototype")
        };

        [Test]
        public void CurrentSnapshotsExactOrderedStarterCatalogue()
        {
            var roster = PrototypeCharacterRoster.Current;

            Assert.That(roster.SourceModuleId, Is.EqualTo("realmraiders.starter-character-catalog"));
            Assert.That(roster.Entries, Has.Count.EqualTo(Expected.Length));
            for (var index = 0; index < Expected.Length; index++)
            {
                var entry = roster[index];
                Assert.That(entry, Is.SameAs(roster.Entries[index]));
                Assert.That(entry.StableId, Is.EqualTo(Expected[index].StableId), $"stable ID at index {index}");
                Assert.That(entry.DisplayName, Is.EqualTo(Expected[index].DisplayName), $"display name at index {index}");
                Assert.That(entry.VisualFamily, Is.EqualTo(Expected[index].VisualFamily), $"family at index {index}");
                Assert.That(entry.VisualProfileKey, Is.EqualTo(Expected[index].VisualProfileKey), $"visual profile at index {index}");
            }
        }

        [Test]
        public void LookupIsOrdinalCaseSensitiveAndRejectsUnknownIds()
        {
            var roster = PrototypeCharacterRoster.Current;

            Assert.That(roster.TryGet(PrototypeCharacterRoster.SylvanWolfId, out var wolf), Is.True);
            Assert.That(wolf, Is.SameAs(roster[2]));
            Assert.That(roster.TryGet(PrototypeCharacterRoster.SylvanWolfId.ToUpperInvariant(), out _), Is.False);
            Assert.That(roster.TryGet(null, out _), Is.False);
            var error = Assert.Throws<KeyNotFoundException>(() => roster.GetRequired("realmraiders.unknown"));
            Assert.That(error.Message, Does.Contain("Unknown starter character stable ID").And.Contain("case-sensitive"));
        }

        [Test]
        public void VisualRecipeMappingPreservesApprovedIdentityAndFamily()
        {
            var roster = PrototypeCharacterRoster.Current;
            var expectedRecipes = new[]
            {
                PrototypeRuntimeFactory.BloodKnightRecipe,
                PrototypeRuntimeFactory.GuardianEntRecipe,
                PrototypeRuntimeFactory.SylvanBeastRecipe,
                PrototypeRuntimeFactory.InfernalBruteRecipe,
                PrototypeRuntimeFactory.InfernalBeastRecipe
            };

            for (var index = 0; index < roster.Entries.Count; index++)
            {
                var recipe = PrototypeRuntimeFactory.VisualRecipeFor(roster[index]);
                Assert.That(recipe, Is.SameAs(expectedRecipes[index]), roster[index].StableId);
                Assert.That(recipe.Family, Is.EqualTo(roster[index].VisualFamily), roster[index].StableId);
            }
        }

        [Test]
        public void ConstructionRejectsDuplicateUnknownAndMismatchedCatalogueData()
        {
            var duplicateId = CopyCurrent();
            duplicateId[1] = new PrototypeCharacterRosterEntry(duplicateId[0].StableId, duplicateId[1].DisplayName, duplicateId[1].VisualFamily, duplicateId[1].VisualProfileKey);
            Assert.That(Assert.Throws<InvalidOperationException>(() => new PrototypeCharacterRoster(duplicateId)).Message, Does.Contain("duplicate stable ID"));

            var duplicateProfile = CopyCurrent();
            duplicateProfile[1] = new PrototypeCharacterRosterEntry(duplicateProfile[1].StableId, duplicateProfile[1].DisplayName, duplicateProfile[1].VisualFamily, duplicateProfile[0].VisualProfileKey);
            Assert.That(Assert.Throws<InvalidOperationException>(() => new PrototypeCharacterRoster(duplicateProfile)).Message, Does.Contain("duplicate visual-profile key"));

            var unknown = CopyCurrent();
            unknown[2] = new PrototypeCharacterRosterEntry("realmraiders.unknown", unknown[2].DisplayName, unknown[2].VisualFamily, unknown[2].VisualProfileKey);
            Assert.That(Assert.Throws<InvalidOperationException>(() => new PrototypeCharacterRoster(unknown)).Message, Does.Contain("unknown stable ID"));

            var wrongFamily = CopyCurrent();
            wrongFamily[4] = new PrototypeCharacterRosterEntry(wrongFamily[4].StableId, wrongFamily[4].DisplayName, CharacterVisualFamily.Humanoid, wrongFamily[4].VisualProfileKey);
            Assert.That(Assert.Throws<InvalidOperationException>(() => new PrototypeCharacterRoster(wrongFamily)).Message, Does.Contain("does not match"));
        }

        [Test]
        public void HostNamesOneExplicitProviderWithoutAutomaticDiscovery()
        {
            Assert.That(PrototypeCharacterRoster.ExplicitProviderTypeName, Is.EqualTo("RealmRaiders.Modules.StarterCharacterCatalog.StarterCharacterCatalogProvider"));
            Assert.That(PrototypeCharacterRoster.Current.SourceModuleId, Is.EqualTo("realmraiders.starter-character-catalog"));
        }

        static PrototypeCharacterRosterEntry[] CopyCurrent()
        {
            var current = PrototypeCharacterRoster.Current.Entries;
            var copy = new PrototypeCharacterRosterEntry[current.Count];
            for (var index = 0; index < current.Count; index++)
            {
                var entry = current[index];
                copy[index] = new PrototypeCharacterRosterEntry(entry.StableId, entry.DisplayName, entry.VisualFamily, entry.VisualProfileKey);
            }
            return copy;
        }

        readonly struct ExpectedEntry
        {
            public ExpectedEntry(string stableId, string displayName, CharacterVisualFamily visualFamily, string visualProfileKey)
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
    }
}
