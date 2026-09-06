using UnityEngine;

namespace RealmRaiders.Possession
{
    /// <summary>Visual-only Keeper selection marker. It never participates in physics or input.</summary>
    public sealed class PossessionSelectionPresentation : MonoBehaviour
    {
        Transform ring;
        Transform label;
        Material ringMaterial;
        Camera view;
        float phase;

        public Transform LabelTransform => label;

        public void Initialize(string displayName)
        {
            var ringObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ringObject.name = "Possess Selection Ring";
            ringObject.transform.SetParent(transform, false);
            ringObject.transform.localPosition = new Vector3(0, -.9f, 0);
            ringObject.transform.localScale = new Vector3(2.1f, .035f, 2.1f);
            var collider = ringObject.GetComponent<Collider>();
            if (collider) collider.enabled = false;
            ring = ringObject.transform;
            var ringRenderer = ringObject.GetComponent<Renderer>();
            ringMaterial = ringRenderer.material;
            ringMaterial.color = new Color(.72f, 1f, .22f, .92f);

            var labelObject = new GameObject("Possess Selection Label", typeof(TextMesh));
            labelObject.transform.SetParent(transform, false);
            labelObject.transform.localPosition = Vector3.up * 3.2f;
            label = labelObject.transform;
            var text = labelObject.GetComponent<TextMesh>();
            text.text = $"{displayName.ToUpperInvariant()} SELECTED\nPRESS POSSESS";
            text.characterSize = .11f;
            text.fontSize = 52;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = new Color(.9f, 1f, .35f);
        }

        void LateUpdate()
        {
            if (!view) view = Camera.main;
            if (!view) return;
            phase += Time.unscaledDeltaTime * 2.2f;
            var scale = 1f + Mathf.Sin(phase) * .08f;
            if (ring) ring.localScale = new Vector3(2.1f * scale, .035f, 2.1f * scale);
            if (ringMaterial) ringMaterial.color = new Color(.72f, 1f, .22f, .8f + Mathf.Sin(phase) * .12f);
            if (label)
            {
                // TextMesh renders toward its local -Z side, so its transform faces away from the view.
                var direction = label.position - view.transform.position;
                if (direction.sqrMagnitude > .001f) label.rotation = Quaternion.LookRotation(direction.normalized, view.transform.up);
            }
        }

        void OnDestroy()
        {
            if (ringMaterial) Destroy(ringMaterial);
            ringMaterial = null;
        }
    }
}
