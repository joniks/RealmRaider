using System;
using System.Globalization;

namespace RealmRaiders.UI
{
    public enum PossessionEnergyReadabilityLevel
    {
        Normal,
        Warning,
        Critical
    }

    public readonly struct PossessionEnergyReadabilityState
    {
        public PossessionEnergyReadabilityLevel Level { get; }
        public int DisplayedTenths { get; }
        public float Maximum { get; }
        public float NormalizedRemaining { get; }
        public bool UsesLowNormalMeter { get; }

        internal PossessionEnergyReadabilityState(PossessionEnergyReadabilityLevel level, int displayedTenths, float maximum, float normalizedRemaining)
        {
            Level = level;
            DisplayedTenths = displayedTenths;
            Maximum = maximum;
            NormalizedRemaining = normalizedRemaining;
            UsesLowNormalMeter = normalizedRemaining <= .25f;
        }

        public string Copy
        {
            get
            {
                var seconds = DisplayedTenths / 10f;
                return Level switch
                {
                    PossessionEnergyReadabilityLevel.Warning => $"POSSESSION ENDING  {seconds.ToString("0.0", CultureInfo.InvariantCulture)}s",
                    PossessionEnergyReadabilityLevel.Critical => $"RETURN TO KEEPER  {seconds.ToString("0.0", CultureInfo.InvariantCulture)}s",
                    _ => $"Possession energy  {seconds.ToString("0.0", CultureInfo.InvariantCulture)}/{Maximum.ToString("0", CultureInfo.InvariantCulture)}s"
                };
            }
        }

        public bool HasSameSemanticValue(PossessionEnergyReadabilityState other) =>
            Level == other.Level &&
            DisplayedTenths == other.DisplayedTenths &&
            Maximum.Equals(other.Maximum) &&
            UsesLowNormalMeter == other.UsesLowNormalMeter;
    }

    public static class PossessionEnergyReadability
    {
        public static float UrgencyPulseDuration => .24f;
        public static float UrgencyPulseMaximumScale => 1.06f;

        public static PossessionEnergyReadabilityState Map(bool isPossessing, float remaining, float maximum)
        {
            maximum = IsFinite(maximum) && maximum > 0 ? maximum : 0;
            if (!IsFinite(remaining)) remaining = remaining > 0 ? maximum : 0;
            remaining = Math.Max(0, Math.Min(remaining, maximum));

            var level = PossessionEnergyReadabilityLevel.Normal;
            if (isPossessing && remaining > 0 && remaining <= 2f) level = PossessionEnergyReadabilityLevel.Critical;
            else if (isPossessing && remaining > 2f && remaining <= 5f) level = PossessionEnergyReadabilityLevel.Warning;

            var displayedTenths = (int)Math.Round(remaining * 10d, MidpointRounding.AwayFromZero);
            var normalized = maximum > 0 ? remaining / maximum : 0;
            return new PossessionEnergyReadabilityState(level, displayedTenths, maximum, normalized);
        }

        /// <summary>Pure unscaled presentation sample for one bounded urgency pulse.</summary>
        public static float UrgencyPulseScaleAt(float startedAt, float currentTime)
        {
            if (!IsFinite(startedAt) || !IsFinite(currentTime)) return 1;
            if (currentTime < startedAt) return 1;
            var endAt = startedAt + UrgencyPulseDuration;
            if (currentTime >= endAt) return 1;
            var elapsed = currentTime - startedAt;
            var phase = elapsed / UrgencyPulseDuration;
            var amount = (float)Math.Sin(Math.PI * phase);
            var clamped = Math.Max(0d, Math.Min(1d, amount));
            return (float)(1 + (UrgencyPulseMaximumScale - 1) * clamped);
        }

        static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
