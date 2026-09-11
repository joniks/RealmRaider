using RealmRaiders.Controllers;
using RealmRaiders.Raid;
using UnityEngine;
using UnityEngine.UI;

namespace RealmRaiders.UI
{
    /// <summary>Short HUD receipt for authoritative RaidManager reward facts. It owns no economy or input.</summary>
    [DisallowMultipleComponent]
    public sealed class RaidRewardCue : MonoBehaviour
    {
        public const string LabelName = "Raid Loot Feedback";
        public const float Duration = 1.05f;
        readonly RaidRewardFactQueue queue = new();
        ResponsiveHudRoot responsive;
        RectTransform labelRect;
        Text label;
        float hideAt;
        RaidRewardFact current;

        public bool Visible => label && label.gameObject.activeSelf;
        public string Text => Visible ? label.text : string.Empty;
        public bool RaycastTarget => label && label.raycastTarget;
        public RectTransform Rect => labelRect;
        public RaidRewardFact Current => current;
        public int PendingCount => queue.Count;
        public int PresentedCount { get; private set; }

        public void Initialize(ResponsiveHudRoot responsiveRoot)
        {
            if (responsive) responsive.LayoutChanged -= ApplyLayout;
            responsive = responsiveRoot;
            if (!label) CreateLabel();
            if (responsive)
            {
                responsive.LayoutChanged -= ApplyLayout;
                responsive.LayoutChanged += ApplyLayout;
                ApplyLayout(responsive.Orientation);
            }
            Clear();
        }

        public void Enqueue(RaidRewardFact fact)
        {
            if (!isActiveAndEnabled || GameplayInput.TerminalState || !queue.TryEnqueue(fact)) return;
            if (!Visible) ShowNext();
        }

        public static string CopyFor(RaidRewardFact fact)
        {
            var source = fact.Source switch
            {
                RaidRewardSource.RoomDiscovery => "ROOM DISCOVERED",
                RaidRewardSource.EnemyDefeat => "ENEMY DEFEATED",
                RaidRewardSource.RealmCoreVictory => "REALM CORE DEFEATED",
                _ => string.Empty
            };
            if (string.IsNullOrEmpty(source) || fact.Sequence <= 0) return string.Empty;
            var gain = fact.GoldDelta > 0 ? $"+{fact.GoldDelta} GOLD" : string.Empty;
            if (fact.RareMaterialsDelta > 0)
                gain += (gain.Length > 0 ? " • " : string.Empty) + $"+{fact.RareMaterialsDelta} RARE";
            return $"{gain}  •  {source}\nTOTAL {fact.TotalGold} GOLD • {fact.TotalRareMaterials} RARE";
        }

        void Update()
        {
            if (GameplayInput.TerminalState) { Clear(); return; }
            if (Visible && Time.unscaledTime >= hideAt) ShowNext();
        }

        void ShowNext()
        {
            if (!queue.TryDequeue(out current))
            {
                current = default; hideAt = 0;
                if (label) { label.text = string.Empty; label.gameObject.SetActive(false); }
                return;
            }
            var copy = CopyFor(current);
            if (string.IsNullOrEmpty(copy)) { ShowNext(); return; }
            label.text = copy;
            label.gameObject.SetActive(true);
            hideAt = Time.unscaledTime + Duration;
            PresentedCount++;
        }

        public void Clear()
        {
            queue.Clear(); current = default; hideAt = 0; PresentedCount = 0;
            if (label) { label.text = string.Empty; label.gameObject.SetActive(false); }
        }

        void CreateLabel()
        {
            var item = new GameObject(LabelName, typeof(RectTransform), typeof(Text));
            item.transform.SetParent(transform, false);
            labelRect = (RectTransform)item.transform;
            label = item.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(1f, .88f, .28f);
            label.raycastTarget = false;
            item.SetActive(false);
        }

        void ApplyLayout(PrototypeOrientation orientation)
        {
            if (!labelRect) return;
            labelRect.anchorMin = labelRect.anchorMax = new Vector2(.5f, 1);
            labelRect.pivot = new Vector2(.5f, 1);
            labelRect.anchoredPosition = orientation == PrototypeOrientation.Portrait ? new Vector2(0, -370) : new Vector2(-285, -370);
            labelRect.sizeDelta = orientation == PrototypeOrientation.Portrait ? new Vector2(760, 92) : new Vector2(620, 82);
            label.fontSize = orientation == PrototypeOrientation.Portrait ? 27 : 24;
        }

        void OnEnable()
        {
            if (!responsive) return;
            responsive.LayoutChanged -= ApplyLayout;
            responsive.LayoutChanged += ApplyLayout;
        }
        void OnDisable() { if (responsive) responsive.LayoutChanged -= ApplyLayout; Clear(); }
        void OnDestroy() { if (responsive) responsive.LayoutChanged -= ApplyLayout; }
    }
}
