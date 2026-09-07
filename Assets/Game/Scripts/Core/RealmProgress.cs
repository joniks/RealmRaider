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
    }

    /// <summary>Small local record of rewards already brought back from completed raids.</summary>
    public static class RealmProgress
    {
        const string Key = "realmraiders.realmProgress.v1";
        const int CurrentVersion = 1;

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

        static bool IsValid(RealmProgressData value) => value != null && value.Version == CurrentVersion && value.Gold >= 0 && value.RareMaterials >= 0 && value.CompletedRaids >= 0 && value.Victories >= 0 && value.Victories <= value.CompletedRaids;
        static RealmProgressData Default() => new();
        static void Save(RealmProgressData value) { value.Version = CurrentVersion; PlayerPrefs.SetString(Key, JsonUtility.ToJson(value)); PlayerPrefs.Save(); }

        public static string KeyForTests => Key;
        public static void ResetForTests() { PlayerPrefs.DeleteKey(Key); PlayerPrefs.Save(); }
    }
}
