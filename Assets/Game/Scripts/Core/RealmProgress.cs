using System;
using RealmRaiders.Raid;
using UnityEngine;

namespace RealmRaiders.Core
{
    [Serializable]
    public sealed class RealmProgressData
    {
        public int Version = 1;
        public int Gold;
        public int RareMaterials;
        public int CompletedRaids;
        public int Victories;
        public int GuardianEntVitalityRank;
    }

    /// <summary>Small local record of rewards already brought back from completed raids.</summary>
    public static class RealmProgress
    {
        const string Key = "realmraiders.realmProgress.v1";
        const int CurrentVersion = 1;
        public const int GuardianEntVitalityRankCap = 3;
        public const int GuardianEntVitalityGoldCost = 100;
        public const int GuardianEntVitalityRareMaterialCost = 1;

        public static RealmProgressData Load()
        {
            var json = PlayerPrefs.GetString(Key, string.Empty);
            if (string.IsNullOrWhiteSpace(json)) return Default();
            try
            {
                var value = JsonUtility.FromJson<RealmProgressData>(json);
                return IsValid(value) ? value : Default();
            }
            catch { return Default(); }
        }

        public static RealmProgressData Credit(RaidResult result)
        {
            var value = Load();
            value.Gold += Mathf.Max(0, result.Gold);
            value.RareMaterials += Mathf.Max(0, result.RareMaterials);
            value.CompletedRaids++;
            if (result.Victory) value.Victories++;
            Save(value);
            return value;
        }

        public static string StoreCopy()
        {
            var value = Load();
            return $"REALM STORES  •  {value.Gold} GOLD  •  {value.RareMaterials} RARE MATERIALS";
        }

        public static bool CanPurchaseGuardianEntVitality()
        {
            var value = Load();
            return value.GuardianEntVitalityRank < GuardianEntVitalityRankCap && value.Gold >= GuardianEntVitalityGoldCost && value.RareMaterials >= GuardianEntVitalityRareMaterialCost;
        }

        public static bool TryPurchaseGuardianEntVitality(out RealmProgressData updated)
        {
            updated = Load();
            if (updated.GuardianEntVitalityRank >= GuardianEntVitalityRankCap || updated.Gold < GuardianEntVitalityGoldCost || updated.RareMaterials < GuardianEntVitalityRareMaterialCost) return false;
            updated.Gold -= GuardianEntVitalityGoldCost;
            updated.RareMaterials -= GuardianEntVitalityRareMaterialCost;
            updated.GuardianEntVitalityRank++;
            Save(updated);
            return true;
        }

        public static float GuardianEntMaximumHealth(float baseMaximum)
        {
            var rank = Load().GuardianEntVitalityRank;
            return Mathf.Max(1, baseMaximum) * (1 + rank * .1f);
        }

        static bool IsValid(RealmProgressData value) => value != null && value.Version == CurrentVersion && value.Gold >= 0 && value.RareMaterials >= 0 && value.CompletedRaids >= 0 && value.Victories >= 0 && value.Victories <= value.CompletedRaids && value.GuardianEntVitalityRank >= 0 && value.GuardianEntVitalityRank <= GuardianEntVitalityRankCap;
        static RealmProgressData Default() => new();
        static void Save(RealmProgressData value) { value.Version = CurrentVersion; PlayerPrefs.SetString(Key, JsonUtility.ToJson(value)); PlayerPrefs.Save(); }

        public static string KeyForTests => Key;
        public static void ResetForTests() { PlayerPrefs.DeleteKey(Key); PlayerPrefs.Save(); }
    }
}
