using System.Collections.Generic;
using NUnit.Framework;
using RealmRaiders.Modules;

namespace RealmRaiders.Tests.Modules
{
    public sealed class ModuleContractsTests
    {
        [Test]
        public void CharacterCatalogueProvider_ExposesPlainContractEntries()
        {
            ICharacterCatalogProvider provider = new TestCharacterCatalogProvider();

            Assert.That(provider.ModuleId, Is.EqualTo("tests.character-catalogue"));
            var entries = provider.GetCatalogueEntries();
            Assert.That(entries.Count, Is.EqualTo(3));
            Assert.That(entries[0].StableId, Is.EqualTo("test.knight"));
            Assert.That(entries[0].DisplayName, Is.EqualTo("Test Knight"));
            Assert.That(entries[0].BodyFamily, Is.EqualTo(CharacterBodyFamily.Humanoid));
            Assert.That(entries[0].VisualProfileKey, Is.EqualTo("test.knight.visual"));
            Assert.That(entries[1].BodyFamily, Is.EqualTo(CharacterBodyFamily.LargeCreature));
            Assert.That(entries[2].BodyFamily, Is.EqualTo(CharacterBodyFamily.Beast));

            var stableIds = new HashSet<string>();
            foreach (var entry in entries)
            {
                Assert.That(entry.StableId, Is.Not.Null.And.Not.Empty);
                Assert.That(stableIds.Add(entry.StableId), Is.True, "Catalogue IDs must be stable and unique.");
                Assert.That(entry.DisplayName, Is.Not.Null.And.Not.Empty);
                Assert.That(entry.VisualProfileKey, Is.Not.Null.And.Not.Empty);
            }

            Assert.That(typeof(CharacterCatalogueEntry).IsSerializable, Is.True);
            foreach (var reference in typeof(ICharacterCatalogProvider).Assembly.GetReferencedAssemblies())
                Assert.That(reference.Name, Is.Not.EqualTo("RealmRaiders.Runtime"));
        }

        sealed class TestCharacterCatalogProvider : ICharacterCatalogProvider
        {
            static readonly IReadOnlyList<CharacterCatalogueEntry> entries =
                new CharacterCatalogueEntry[]
                {
                    new("test.knight", "Test Knight", CharacterBodyFamily.Humanoid, "test.knight.visual"),
                    new("test.ent", "Test Ent", CharacterBodyFamily.LargeCreature, "test.ent.visual"),
                    new("test.wolf", "Test Wolf", CharacterBodyFamily.Beast, "test.wolf.visual")
                };

            public string ModuleId => "tests.character-catalogue";
            public IReadOnlyList<CharacterCatalogueEntry> GetCatalogueEntries() => entries;
        }
    }
}
