using UnityEngine;
using RealmRaiders.Controllers;

namespace RealmRaiders.Core
{
    public static class PrototypeSave
    {
        const string RealmKey = "realmraiders.selectedRealm";
        const string OrientationKey = "realmraiders.orientation.v1";
        const string ControlKey = "realmraiders.controlStyle.v1";
        public static string SelectedRealm { get; private set; } = "Sylvan";
        public static string OrientationPreference { get; private set; } = "Auto";
        public static string ControlStylePreference { get; private set; } = "Contextual";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Load()
        { SelectedRealm = PlayerPrefs.GetString(RealmKey, "Sylvan"); SetOrientation(PlayerPrefs.GetString(OrientationKey, "Auto")); SetControlStyle(PlayerPrefs.GetString(ControlKey, "Contextual")); }

        public static void SelectRealm(string realm)
        { SelectedRealm = string.IsNullOrWhiteSpace(realm) ? "Sylvan" : realm; PlayerPrefs.SetString(RealmKey, SelectedRealm); PlayerPrefs.Save(); }
        public static void SetOrientation(string value)
        {
            if (value != "Auto" && value != "Portrait" && value != "Landscape") value = "Auto";
            OrientationPreference = value; PlayerPrefs.SetString(OrientationKey, value); PlayerPrefs.Save();
            Screen.orientation = value == "Portrait" ? ScreenOrientation.Portrait : value == "Landscape" ? ScreenOrientation.LandscapeLeft : ScreenOrientation.AutoRotation;
        }
        public static void SetControlStyle(string value)
        {
            if (value != "Contextual" && value != "Fingertap" && value != "Joystick") value = "Contextual";
            ControlStylePreference = value; PlayerPrefs.SetString(ControlKey, value); PlayerPrefs.Save();
            GameplayInput.ResetTransientInput();
        }
        public static string EffectiveControlStyle(bool landscape) => ControlStylePreference == "Contextual" ? (landscape ? "Joystick" : "Fingertap") : ControlStylePreference;
    }
}
