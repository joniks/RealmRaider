using RealmRaiders.Core;
using UnityEngine;
using UnityEngine.UI;

namespace RealmRaiders.UI
{
    /// <summary>Scene-local, visual-only projection of the current five-slot defense layout.</summary>
    [DisallowMultipleComponent]
    public sealed class BuildPlanPreview : MonoBehaviour
    {
        const int NodeTotal = 5;
        const int ItemTotal = NodeTotal + 1;
        static readonly int[] ApproachOrder = { 3, 0, 1, 4, 2 };

        readonly Image[] nodes = new Image[NodeTotal];
        readonly Text[] nodeLabels = new Text[NodeTotal];
        readonly DefensePieceType[] displayedPieces = new DefensePieceType[NodeTotal];
        RectTransform root;
        Image lane;
        Image endpoint;
        Text endpointLabel;
        bool built;

        public int NodeCount => NodeTotal;
        public RectTransform RootRect => root;
        public RectTransform LaneRect => lane ? lane.rectTransform : null;
        public RectTransform EndpointRect => endpoint ? endpoint.rectTransform : null;
        public int LayoutIndexAt(int nodeIndex) => IsNodeIndex(nodeIndex) ? ApproachOrder[nodeIndex] : -1;
        public DefensePieceType PieceAt(int nodeIndex) => IsNodeIndex(nodeIndex) ? displayedPieces[nodeIndex] : DefensePieceType.Empty;
        public string NodeTextAt(int nodeIndex) => IsNodeIndex(nodeIndex) && nodeLabels[nodeIndex] ? nodeLabels[nodeIndex].text : string.Empty;
        public RectTransform NodeRectAt(int nodeIndex) => IsNodeIndex(nodeIndex) && nodes[nodeIndex] ? nodes[nodeIndex].rectTransform : null;
        public string EndpointText => endpointLabel ? endpointLabel.text : string.Empty;

        public void Initialize()
        {
            if (!built) BuildOnce();
        }

        public void Refresh(DefenseLayout layout)
        {
            Initialize();
            for (var nodeIndex = 0; nodeIndex < NodeTotal; nodeIndex++)
            {
                var layoutIndex = ApproachOrder[nodeIndex];
                var piece = LayoutPiece(layout, layoutIndex);
                displayedPieces[nodeIndex] = piece;
                nodes[nodeIndex].color = PieceColor(piece);
                nodeLabels[nodeIndex].text = NodeCopy(nodeIndex, layoutIndex, piece);
            }
        }

        public void ApplyOrientation(PrototypeOrientation orientation)
        {
            Initialize();
            var landscape = orientation == PrototypeOrientation.Landscape;
            root.anchorMin = root.anchorMax = new Vector2(.5f, 0);
            root.pivot = new Vector2(.5f, 0);
            root.anchoredPosition = landscape ? new Vector2(-430, 150) : new Vector2(0, 420);
            root.sizeDelta = landscape ? new Vector2(950, 90) : new Vector2(1020, 90);
            LayoutItems();
        }

        public static string SlotName(int index) => index switch
        {
            0 => "OUTER GUARD",
            1 => "MID GUARD",
            2 => "HEART GUARD",
            3 => "ROOT GATE",
            4 => "INNER ROOT",
            _ => "DEFENSE POINT"
        };

        public static string PieceName(DefensePieceType piece) => piece switch
        {
            DefensePieceType.Wolf => "WOLF",
            DefensePieceType.Ent => "ENT",
            DefensePieceType.RootTrap => "ROOT TRAP",
            _ => "OPEN"
        };

        public static string Role(DefensePieceType piece) => piece switch
        {
            DefensePieceType.Wolf => "FAST INTERCEPT",
            DefensePieceType.Ent => "POSSESSABLE GUARDIAN",
            DefensePieceType.RootTrap => "MANUAL HOLD TRAP",
            _ => "UNASSIGNED"
        };

        void BuildOnce()
        {
            built = true;
            root = (RectTransform)transform;
            lane = CreateImage("Invader Approach Lane", new Color(.55f, .72f, .42f, .42f));
            for (var index = 0; index < NodeTotal; index++)
                nodes[index] = CreateImage($"Defense Plan Node {index + 1}", PieceColor(DefensePieceType.Empty));
            endpoint = CreateImage("Heart Tree Endpoint", new Color(.22f, .52f, .2f, .96f));
            for (var index = 0; index < NodeTotal; index++)
                nodeLabels[index] = CreateLabel($"Defense Plan Node {index + 1} Label");
            endpointLabel = CreateLabel("Heart Tree Endpoint Label");
            endpointLabel.text = "HEART TREE\nREALM CORE";
            ApplyOrientation(PrototypeOrientation.Portrait);
        }

        void LayoutItems()
        {
            var width = root.sizeDelta.x;
            const float margin = 8;
            const float gap = 8;
            const float itemHeight = 74;
            var itemWidth = (width - margin * 2 - gap * (ItemTotal - 1)) / ItemTotal;

            Place(lane.rectTransform, new Vector2(margin, 43), new Vector2(width - margin * 2, 5));
            for (var index = 0; index < NodeTotal; index++)
            {
                var position = new Vector2(margin + index * (itemWidth + gap), 8);
                Place(nodes[index].rectTransform, position, new Vector2(itemWidth, itemHeight));
                Place(nodeLabels[index].rectTransform, position + new Vector2(5, 3), new Vector2(itemWidth - 10, itemHeight - 6));
            }

            var endpointPosition = new Vector2(margin + NodeTotal * (itemWidth + gap), 8);
            Place(endpoint.rectTransform, endpointPosition, new Vector2(itemWidth, itemHeight));
            Place(endpointLabel.rectTransform, endpointPosition + new Vector2(5, 3), new Vector2(itemWidth - 10, itemHeight - 6));
        }

        Image CreateImage(string objectName, Color color)
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        Text CreateLabel(string objectName)
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(transform, false);
            var label = go.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 14;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 9;
            label.resizeTextMaxSize = 14;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.raycastTarget = false;
            return label;
        }

        static void Place(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = Vector2.zero;
            rect.pivot = Vector2.zero;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        static string NodeCopy(int nodeIndex, int layoutIndex, DefensePieceType piece)
        {
            var stage = SlotName(layoutIndex);
            if (nodeIndex == 0) stage = "INVADER → " + stage;
            return $"{stage}\n{PieceName(piece)}\n{Role(piece)}";
        }

        static DefensePieceType LayoutPiece(DefenseLayout layout, int index) =>
            layout?.Slots != null && index >= 0 && index < layout.Slots.Length ? layout.Slots[index].Piece : DefensePieceType.Empty;

        static Color PieceColor(DefensePieceType piece) => piece switch
        {
            DefensePieceType.Wolf => new Color(.2f, .42f, .24f, .96f),
            DefensePieceType.Ent => new Color(.38f, .43f, .15f, .96f),
            DefensePieceType.RootTrap => new Color(.45f, .29f, .12f, .96f),
            _ => new Color(.15f, .18f, .16f, .9f)
        };

        static bool IsNodeIndex(int index) => index >= 0 && index < NodeTotal;
    }
}
