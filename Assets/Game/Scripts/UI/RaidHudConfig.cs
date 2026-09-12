using System;

namespace RealmRaiders.UI
{
    public sealed class RaidHudConfig
    {
        readonly string[] abilityLabels;

        RaidHudConfig(string realmIdentity, string stateTitle, string heroName, string objectiveName,
            string lockedObjectiveCopy, string retryScene, string victoryCopy, string defeatCopy,
            bool supportsJourney, bool showPlanNextDefense, bool guardianEntAbilities, params string[] abilities)
        {
            RealmIdentity = realmIdentity;
            StateTitle = stateTitle;
            HeroName = heroName;
            ObjectiveName = objectiveName;
            LockedObjectiveCopy = lockedObjectiveCopy;
            RetryScene = retryScene;
            VictoryCopy = victoryCopy;
            DefeatCopy = defeatCopy;
            SupportsJourney = supportsJourney;
            ShowPlanNextDefense = showPlanNextDefense;
            UsesGuardianEntAbilityIcons = guardianEntAbilities;
            abilityLabels = abilities == null ? Array.Empty<string>() : (string[])abilities.Clone();
        }

        public string RealmIdentity { get; }
        public string StateTitle { get; }
        public string HeroName { get; }
        public string ObjectiveName { get; }
        public string LockedObjectiveCopy { get; }
        public string RetryScene { get; }
        public string VictoryCopy { get; }
        public string DefeatCopy { get; }
        public bool SupportsJourney { get; }
        public bool ShowPlanNextDefense { get; }
        public bool UsesGuardianEntAbilityIcons { get; }
        public string AbilityLabel(int index) => index >= 0 && index < abilityLabels.Length ? abilityLabels[index] : string.Empty;

        public static RaidHudConfig Sylvan { get; } = new(
            HudPresentation.SylvanRealmIdentity, "SYLVAN RAID", "Blood Knight", "Heart Tree", string.Empty,
            "SylvanRealm", "The Heart Tree fell. Return to your Realm and plan the next defense.",
            "Revise the next defense, or try this raid again.", true, true, false,
            "SLASH", "BLOOD RUSH", "CLEAVE");

        public static RaidHudConfig InfernalEnt { get; } = new(
            HudPresentation.InfernalRealmIdentity, "INFERNAL RAID", "Guardian Ent", "Infernal Heart",
            "DEFEAT INFERNAL BRUTE TO UNLOCK THE HEART", "InfernalRaid",
            "The Infernal Heart fell. Return to the Hub or raid again.",
            "The Guardian Ent fell. Retry the Infernal raid or return to the Hub.", false, false, true,
            "SMASH", "CHARGE", "GROUND SLAM");
    }
}
