using System;
using UnityEngine;

namespace RealmRaiders.Combat
{
    public sealed class Health : MonoBehaviour
    {
        public event Action<float, float> Changed;
        public event Action Died;
        public float Current { get; private set; }
        public float Maximum { get; private set; }
        public bool IsDead => Current <= 0;

        public void Initialize(float maximum)
        { Maximum = Mathf.Max(1, maximum); Current = Maximum; Changed?.Invoke(Current, Maximum); }

        public void TakeDamage(DamageInfo hit, float armor)
        {
            if (IsDead) return;
            var reduction = 100f / (100f + Mathf.Max(0, armor));
            Current = Mathf.Max(0, Current - hit.Amount * reduction);
            Changed?.Invoke(Current, Maximum);
            if (IsDead) Died?.Invoke();
        }

        public void RestoreFull() { Current = Maximum; Changed?.Invoke(Current, Maximum); }
    }
}
