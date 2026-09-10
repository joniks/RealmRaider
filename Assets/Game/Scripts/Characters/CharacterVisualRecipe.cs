using UnityEngine;

namespace RealmRaiders.Characters
{
    public enum CharacterVisualFamily { Humanoid, LargeCreature, Beast }
    public enum VisualModuleStyle { None, Crown, Horns, ShoulderPads, Bark, Blade, Claws, Spikes, Mane }

    /// <summary>
    /// Data contract for a future mesh/prefab, palette, rig-family and animator pipeline.
    /// The current assembler uses primitive fallbacks when no prefab is assigned.
    /// </summary>
    [CreateAssetMenu(menuName = "Realm Raiders/Character Visual Recipe")]
    public sealed class CharacterVisualRecipe : ScriptableObject
    {
        public CharacterVisualFamily Family;
        public VisualModuleStyle Head;
        public VisualModuleStyle Back;
        public VisualModuleStyle Arms;
        public VisualModuleStyle Accent;
        public Color Primary = Color.white;
        public Color Secondary = Color.gray;
        public Color AccentColor = Color.yellow;
        [Header("Future production slots")]
        public GameObject BaseBodyPrefab;
        [Header("Optional LargeCreature pilot; static BaseBody remains the first fallback")]
        public LargeCreatureMotionBinding LargeCreatureMotion;
        public CharacterVisualRecipe MissingBaseBodyFallback;
        /// <summary>Optional authored visual fit applied only to the assembled Base Body; zero preserves its source rotation.</summary>
        public Vector3 BaseBodyLocalEulerAngles = Vector3.zero;
        public GameObject HeadPrefab;
        public GameObject BackPrefab;
        public GameObject ArmsPrefab;
        public GameObject AccentPrefab;
        public RuntimeAnimatorController AnimatorController;

        public bool IsValid => Primary.a > 0 && Secondary.a > 0 && AccentColor.a > 0;
    }
}
