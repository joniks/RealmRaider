using RealmRaiders.Characters;
using UnityEngine;
using UnityEngine.UI;

namespace RealmRaiders.UI
{
    /// <summary>Small presentation-only view over the authoritative ability runtime state.</summary>
    public sealed class AbilityButtonReadiness
    {
        readonly Button button;
        readonly Text label;
        readonly Image image;
        readonly string readyText;
        readonly string actingText;
        readonly int abilityIndex;
        readonly Color readyColor;
        readonly Color unavailableColor;
        string displayedText;
        bool displayedInteractable;
        Color displayedColor;
        int displayedCooldownTenths = -1;

        public string Text => label ? label.text : string.Empty;
        public bool IsInteractable => button && button.interactable;

        public AbilityButtonReadiness(Button source, string semanticLabel, int index)
        {
            button = source;
            label = source ? source.GetComponentInChildren<Text>() : null;
            image = source ? source.GetComponent<Image>() : null;
            readyText = semanticLabel;
            actingText = semanticLabel + " — ACTING";
            abilityIndex = index;
            readyColor = image ? image.color : Color.white;
            unavailableColor = Color.Lerp(readyColor, Color.black, .48f);
            displayedInteractable = true;
            Apply(readyText, false, unavailableColor);
        }

        public void Refresh(CombatEntity entity, bool directControl)
        {
            if (!entity || !directControl || entity.Health == null || entity.Health.IsDead || abilityIndex < 0 || abilityIndex >= entity.Abilities.Count)
            {
                displayedCooldownTenths = -1;
                Apply(readyText, false, unavailableColor);
                return;
            }

            if (entity.IsActionResolving)
            {
                displayedCooldownTenths = -1;
                Apply(actingText, false, unavailableColor);
                return;
            }

            var runtime = entity.Abilities[abilityIndex];
            var remaining = runtime.CooldownRemaining;
            if (remaining > .001f)
            {
                var tenths = Mathf.CeilToInt(remaining * 10f);
                if (displayedCooldownTenths != tenths)
                {
                    displayedCooldownTenths = tenths;
                    Apply(readyText + "  " + (tenths / 10f).ToString("0.0") + "s", false, unavailableColor);
                }
                else Apply(displayedText, false, unavailableColor);
                return;
            }

            displayedCooldownTenths = -1;
            Apply(readyText, true, readyColor);
        }

        void Apply(string text, bool interactable, Color color)
        {
            if (label && displayedText != text) { label.text = text; displayedText = text; }
            if (button && displayedInteractable != interactable) { button.interactable = interactable; displayedInteractable = interactable; }
            if (image && displayedColor != color) { image.color = color; displayedColor = color; }
        }
    }
}
