using RealmRaiders.Characters;
using RealmRaiders.Raid;
using UnityEngine;

namespace RealmRaiders.Realm
{
    public enum MoonwellRecoveryState { Unavailable, Ready, FullHealth, OutOfRange, Spent }

    public sealed class MoonwellRecovery : MonoBehaviour
    {
        public const float HealFraction = .3f;
        public const float InteractionRadius = 4f;

        CombatEntity hero;
        RaidManager raid;
        Renderer presentationRenderer;
        Color readyColor;
        bool initialized;
        bool hasCharge;
        bool useInProgress;

        public bool HasCharge => initialized && hasCharge;
        public bool IsSpent => initialized && !hasCharge;
        public CombatEntity Hero => hero;
        public RaidManager Raid => raid;
        public MoonwellRecoveryState State
        {
            get
            {
                if (!initialized || !isActiveAndEnabled || !hero || !hero.isActiveAndEnabled || !raid || !raid.isActiveAndEnabled || hero.Health == null || hero.Health.IsDead || !raid.AllowsRecovery)
                    return MoonwellRecoveryState.Unavailable;
                if (!hasCharge) return MoonwellRecoveryState.Spent;
                if (hero.Health.Current >= hero.Health.Maximum) return MoonwellRecoveryState.FullHealth;
                return IsHeroInRange() ? MoonwellRecoveryState.Ready : MoonwellRecoveryState.OutOfRange;
            }
        }
        public bool CanUse => !useInProgress && State == MoonwellRecoveryState.Ready;

        public void Initialize(CombatEntity exactHero, RaidManager raidManager, Renderer wellRenderer)
        {
            if (initialized && hero == exactHero && raid == raidManager) return;
            hero = exactHero;
            raid = raidManager;
            presentationRenderer = wellRenderer;
            initialized = true;
            hasCharge = true;
            useInProgress = false;
            if (presentationRenderer && presentationRenderer.sharedMaterial) readyColor = presentationRenderer.sharedMaterial.color;
        }

        public float TryUse()
        {
            if (!CanUse) return 0;
            useInProgress = true;
            hasCharge = false;
            try
            {
                var restored = hero.Health.Restore(hero.Health.Maximum * HealFraction);
                if (restored <= 0) { hasCharge = true; return 0; }
                ApplySpentPresentation();
                return restored;
            }
            finally { useInProgress = false; }
        }

        bool IsHeroInRange()
        {
            var offset = hero.transform.position - transform.position;
            offset.y = 0;
            return offset.sqrMagnitude <= InteractionRadius * InteractionRadius;
        }

        void ApplySpentPresentation()
        {
            if (!presentationRenderer) return;
            presentationRenderer.material.color = Color.Lerp(readyColor, new Color(.08f, .11f, .12f, 1), .7f);
        }
    }
}
