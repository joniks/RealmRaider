using UnityEngine;

namespace RealmRaiders.Combat
{
    public enum AbilityKind { Melee, Dash, Area }

    [CreateAssetMenu(menuName = "Realm Raiders/Ability")]
    public sealed class AbilityDefinition : ScriptableObject
    {
        public string DisplayName = "Attack";
        public AbilityKind Kind;
        [Min(0)] public float Damage = 20;
        [Min(0)] public float Range = 2;
        [Min(0)] public float Radius = 2;
        [Min(0)] public float Cooldown = 1;
        [Min(0)] public float Windup = .2f;
        [Min(0)] public float DashDistance = 5;
    }
}
