using UnityEngine;

namespace RealmRaiders.Core
{
    public static class PrototypeSave
    {
        const string RealmKey = "realmraiders.selectedRealm";
        public static string SelectedRealm { get; private set; } = "Sylvan";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Load()
        { SelectedRealm = PlayerPrefs.GetString(RealmKey, "Sylvan"); }

        public static void SelectRealm(string realm)
        { SelectedRealm = string.IsNullOrWhiteSpace(realm) ? "Sylvan" : realm; PlayerPrefs.SetString(RealmKey, SelectedRealm); PlayerPrefs.Save(); }
    }
}
