using System;
using UnityEngine;

namespace RealmRaiders.Combat
{
    public sealed class Health : MonoBehaviour
    {
        public event Action<float, float> Changed;
        public event Action<DamageInfo> Damaged;
        public event Action Died;
        public float Current { get; private set; }
        public float Maximum { get; private set; }
        public bool IsDead => Current <= 0;
        float damageImmuneUntil;
        public bool IsDamageImmune => !IsDead && Time.time < damageImmuneUntil;
        public float DamageImmunityRemaining => Mathf.Max(0, damageImmuneUntil - Time.time);

        public void Initialize(float maximum)
        { damageImmuneUntil = 0; Maximum = Mathf.Max(1, maximum); Current = Maximum; Changed?.Invoke(Current, Maximum); }

        public bool TakeDamage(DamageInfo hit, float armor)
        {
            if (IsDead || IsDamageImmune) return false;
            var reduction = 100f / (100f + Mathf.Max(0, armor));
            Current = Mathf.Max(0, Current - hit.Amount * reduction);
            Damaged?.Invoke(hit);
            Changed?.Invoke(Current, Maximum);
            if (IsDead) Died?.Invoke();
            return true;
        }

        internal void BeginDamageImmunity(float seconds) => damageImmuneUntil = Time.time + Mathf.Max(0, seconds);
        internal void ClearDamageImmunity() => damageImmuneUntil = 0;
        public void RestoreFull() { Current = Maximum; Changed?.Invoke(Current, Maximum); }
    }
}
