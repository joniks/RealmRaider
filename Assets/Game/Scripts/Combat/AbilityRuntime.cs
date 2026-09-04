using UnityEngine;

namespace RealmRaiders.Combat
{
    public sealed class AbilityRuntime
    {
        public AbilityDefinition Definition { get; }
        public float ReadyAt { get; private set; }
        public bool IsReady => Time.time >= ReadyAt;
        public AbilityRuntime(AbilityDefinition definition) => Definition = definition;
        public bool TryConsume()
        {
            if (!IsReady) return false;
            ReadyAt = Time.time + Definition.Cooldown;
            return true;
        }
    }
}
