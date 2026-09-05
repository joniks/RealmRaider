using RealmRaiders.Combat;
using UnityEngine;

namespace RealmRaiders.Characters
{
    [CreateAssetMenu(menuName = "Realm Raiders/Character")]
    public sealed class CharacterDefinition : ScriptableObject
    {
        public string DisplayName = "Character";
        public CombatStats Stats;
        public AbilityDefinition[] Abilities;
        public bool Possessable;
        public Color PlaceholderColor = Color.white;
        public CharacterVisualRecipe VisualRecipe;
    }
}
