using System;

namespace RealmRaiders.Possession
{
    public sealed class PossessionEnergy
    {
        public event Action<float, float> Changed;
        public float Maximum { get; }
        public float Remaining { get; private set; }
        public bool IsDepleted => Remaining <= 0;

        public PossessionEnergy(float maximum)
        { Maximum = Math.Max(0, maximum); Remaining = Maximum; }

        public bool Consume(float amount)
        {
            if (amount <= 0 || IsDepleted) return !IsDepleted;
            Remaining = Math.Max(0, Remaining - amount); Changed?.Invoke(Remaining, Maximum);
            return !IsDepleted;
        }

        public void Refill()
        { Remaining = Maximum; Changed?.Invoke(Remaining, Maximum); }
    }
}
