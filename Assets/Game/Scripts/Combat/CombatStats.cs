using System;
using UnityEngine;

namespace RealmRaiders.Combat
{
    [Serializable]
    public struct CombatStats
    {
        [Min(1)] public float MaxHealth;
        [Min(0)] public float AttackDamage;
        [Min(0.1f)] public float AttackSpeed;
        [Min(0.1f)] public float MoveSpeed;
        [Min(0)] public float Armor;
        [Min(0)] public float AbilityPower;

        public static CombatStats BloodKnight => new CombatStats
        { MaxHealth = 150, AttackDamage = 22, AttackSpeed = 1.25f, MoveSpeed = 6.2f, Armor = 8, AbilityPower = 20 };

        public static CombatStats Ent => new CombatStats
        { MaxHealth = 340, AttackDamage = 34, AttackSpeed = .65f, MoveSpeed = 3.2f, Armor = 18, AbilityPower = 32 };
    }
}
