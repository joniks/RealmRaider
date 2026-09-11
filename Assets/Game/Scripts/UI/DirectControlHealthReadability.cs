using System.Globalization;
using UnityEngine;

namespace RealmRaiders.UI
{
    public readonly struct DirectControlHealthReadabilityState
    {
        public string Copy { get; }
        public Color Tint { get; }
        public bool IsLow { get; }

        internal DirectControlHealthReadabilityState(string copy, Color tint, bool isLow)
        {
            Copy = copy;
            Tint = tint;
            IsLow = isLow;
        }
    }

    public static class DirectControlHealthReadability
    {
        public const float LowHealthFraction = .25f;
        public static Color NeutralTint => Color.white;
        public static Color LowHealthTint => new(1f, .48f, .2f);

        public static DirectControlHealthReadabilityState Map(string displayName, float current, float maximum,
            bool directControl, bool terminal)
        {
            displayName ??= string.Empty;
            var validHealth = IsFinite(current) && IsFinite(maximum) && maximum > 0;
            var low = directControl && !terminal && validHealth && current > 0 && current <= maximum * LowHealthFraction;
            var copy = low
                ? $"LOW HP — {displayName} {Format(current)}/{Format(maximum)}"
                : $"{displayName}  {Format(current)}/{Format(maximum)} HP";
            return new DirectControlHealthReadabilityState(copy, low ? LowHealthTint : NeutralTint, low);
        }

        static string Format(float value) => value.ToString("0", CultureInfo.InvariantCulture);
        static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
