using RealmRaiders.Core;
using UnityEngine;
using UnityEngine.UI;

namespace RealmRaiders.UI
{
    public sealed class DefenseDeploymentReceiptData
    {
        public const int SlotCount = 5;
        readonly DefensePieceType[] pieces = new DefensePieceType[SlotCount];
        readonly string tradeoffId;
        readonly string tradeoffDisplayName;
        readonly string tacticalSummary;
        readonly int possessionEnergyMaximumSeconds;

        public DefenseDeploymentReceiptData(DefensePieceType[] deployedPieces, DefenseTradeoffSelection tradeoff = null)
        {
            if (deployedPieces != null)
                for (var index = 0; index < Mathf.Min(SlotCount, deployedPieces.Length); index++)
                    pieces[index] = deployedPieces[index];
            if (tradeoff == null) return;
            tradeoffId = tradeoff.TradeoffId;
            tradeoffDisplayName = tradeoff.DisplayName;
            tacticalSummary = tradeoff.TacticalSummary;
            possessionEnergyMaximumSeconds = tradeoff.PossessionEnergyMaximumSeconds;
        }

        public DefensePieceType PieceAt(int slotIndex) => slotIndex >= 0 && slotIndex < pieces.Length ? pieces[slotIndex] : DefensePieceType.Empty;
        public string TradeoffId => tradeoffId ?? string.Empty;
        public string TradeoffDisplayName => tradeoffDisplayName ?? string.Empty;
        public string TacticalSummary => tacticalSummary ?? string.Empty;
        public int PossessionEnergyMaximumSeconds => possessionEnergyMaximumSeconds;
    }

    [DisallowMultipleComponent]
    public sealed class DefenseDeploymentReceipt : MonoBehaviour
    {
        public const string ObjectName = "Defense Deployment Receipt";
        static readonly int[] ApproachOrder = { 3, 0, 1, 4, 2 };
        static readonly string[] SlotNames = { "OUTER GUARD", "MID GUARD", "HEART GUARD", "ROOT GATE", "INNER ROOT" };

        DefenseDeploymentReceiptData data;
        Text label;
        RectTransform rect;

        public string Copy => label ? label.text : string.Empty;
        public bool Visible => label && label.gameObject.activeSelf;
        public bool RaycastTarget => label && label.raycastTarget;
        public RectTransform Rect => rect;
        public DefensePieceType PieceAtSlot(int slotIndex) => data?.PieceAt(slotIndex) ?? DefensePieceType.Empty;
        public string TradeoffId => data?.TradeoffId ?? string.Empty;
        public int PossessionEnergyMaximumSeconds => data?.PossessionEnergyMaximumSeconds ?? 0;

        public void Initialize(DefenseDeploymentReceiptData deployment)
        {
            data = deployment ?? new DefenseDeploymentReceiptData(null);
            rect = (RectTransform)transform;
            label = GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 20;
            label.alignment = TextAnchor.UpperCenter;
            label.color = new Color(.75f, .92f, .7f);
            label.raycastTarget = false;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 14;
            label.resizeTextMaxSize = 20;
            label.text = Format(data);
            SetVisible(false);
        }

        public void ApplyOrientation(PrototypeOrientation orientation)
        {
            if (!rect) rect = (RectTransform)transform;
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1);
            rect.pivot = new Vector2(.5f, 1);
            rect.anchoredPosition = orientation == PrototypeOrientation.Portrait ? new Vector2(0, -530) : new Vector2(-60, -530);
            rect.sizeDelta = orientation == PrototypeOrientation.Portrait ? new Vector2(1000, 88) : new Vector2(1120, 88);
        }

        public void SetVisible(bool visible)
        {
            if (!label) label = GetComponent<Text>();
            if (label && label.gameObject.activeSelf != visible) label.gameObject.SetActive(visible);
        }

        public static string Format(DefenseDeploymentReceiptData deployment)
        {
            deployment ??= new DefenseDeploymentReceiptData(null);
            var heading = string.IsNullOrWhiteSpace(deployment.TradeoffDisplayName)
                ? "DEPLOYED DEFENSE — INVADER → HEART TREE"
                : $"DEPLOYED: {deployment.TradeoffDisplayName.ToUpperInvariant()} — {deployment.TacticalSummary}";
            return $"{heading}\n" +
                   $"{Stage(deployment, ApproachOrder[0])}  →  {Stage(deployment, ApproachOrder[1])}  →  {Stage(deployment, ApproachOrder[2])}\n" +
                   $"{Stage(deployment, ApproachOrder[3])}  →  {Stage(deployment, ApproachOrder[4])}  →  HEART TREE";
        }

        static string Stage(DefenseDeploymentReceiptData deployment, int slotIndex) => $"{SlotNames[slotIndex]}: {PieceName(deployment.PieceAt(slotIndex))}";

        static string PieceName(DefensePieceType piece) => piece switch
        {
            DefensePieceType.Wolf => "WOLF",
            DefensePieceType.Ent => "GUARDIAN ENT",
            DefensePieceType.RootTrap => "ROOT TRAP",
            _ => "OPEN"
        };
    }
}
