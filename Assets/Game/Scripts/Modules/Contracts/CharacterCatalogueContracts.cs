using System;
using System.Collections.Generic;

namespace RealmRaiders.Modules
{
    /// <summary>Namespaced visual body families understood by character-catalogue modules.</summary>
    public enum CharacterBodyFamily
    {
        Humanoid,
        LargeCreature,
        Beast
    }

    /// <summary>Plain catalogue data supplied by a module; it has no runtime or visual-recipe authority.</summary>
    [Serializable]
    public sealed class CharacterCatalogueEntry
    {
        public CharacterCatalogueEntry(
            string stableId,
            string displayName,
            CharacterBodyFamily bodyFamily,
            string visualProfileKey)
        {
            StableId = stableId;
            DisplayName = displayName;
            BodyFamily = bodyFamily;
            VisualProfileKey = visualProfileKey;
        }

        public string StableId { get; }
        public string DisplayName { get; }
        public CharacterBodyFamily BodyFamily { get; }
        public string VisualProfileKey { get; }
    }

    /// <summary>A passive source of character catalogue data owned by one stable module ID.</summary>
    public interface ICharacterCatalogProvider
    {
        string ModuleId { get; }
        IReadOnlyList<CharacterCatalogueEntry> GetCatalogueEntries();
    }
}
