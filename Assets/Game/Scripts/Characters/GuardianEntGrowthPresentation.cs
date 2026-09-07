using RealmRaiders.Combat;
using UnityEngine;

namespace RealmRaiders.Characters
{
    /// <summary>Visual-only cultivation tiers attached beneath the Guardian Ent presentation pivot.</summary>
    [ExecuteAlways, DisallowMultipleComponent]
    public sealed class GuardianEntGrowthPresentation : MonoBehaviour
    {
        static readonly Vector3[] tierPositions =
        {
            new(0, 1.08f, .2f),
            new(-.52f, .92f, .12f),
            new(.52f, .92f, .12f)
        };

        static Material cultivationMaterial;
        Transform markerRoot;
        Health health;

        public int Rank { get; private set; }
        public int TierCount => markerRoot ? markerRoot.childCount : 0;
        public Transform MarkerRoot => markerRoot;

        public void Configure(int rank)
        {
            UnsubscribeHealth();
            ClearMarker(true);
            Rank = Mathf.Clamp(rank, 0, 3);
            health = GetComponent<Health>();
            if (health) health.Died += OnDied;
            if (Rank == 0 || health && health.IsDead) return;

            var assembler = GetComponent<CharacterVisualAssembler>();
            var pivot = assembler ? assembler.PresentationPivot : null;
            if (!pivot) return;

            markerRoot = new GameObject("Guardian Ent Cultivation").transform;
            markerRoot.SetParent(pivot, false);
            for (var tier = 0; tier < Rank; tier++) BuildTier(tier);
        }

        public void SetVisible(bool visible)
        {
            if (markerRoot) markerRoot.gameObject.SetActive(visible);
        }

        public void Clear()
        {
            UnsubscribeHealth();
            ClearMarker(true);
            Rank = 0;
        }

        void BuildTier(int index)
        {
            var tier = new GameObject($"Cultivation Tier {index + 1}").transform;
            tier.SetParent(markerRoot, false);
            tier.localPosition = tierPositions[index];
            AddLeaf(tier, "Leaf Left", new Vector3(-.11f, 0, 0), Quaternion.Euler(0, 0, 30));
            AddLeaf(tier, "Leaf Right", new Vector3(.11f, 0, 0), Quaternion.Euler(0, 0, -30));
        }

        static void AddLeaf(Transform parent, string name, Vector3 position, Quaternion rotation)
        {
            var leaf = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leaf.name = name;
            leaf.transform.SetParent(parent, false);
            leaf.transform.localPosition = position;
            leaf.transform.localRotation = rotation;
            leaf.transform.localScale = new Vector3(.13f, .34f, .07f);
            var collider = leaf.GetComponent<Collider>();
            if (collider) collider.enabled = false;
            leaf.GetComponent<Renderer>().sharedMaterial = CultivationMaterial();
        }

        static Material CultivationMaterial()
        {
            if (cultivationMaterial) return cultivationMaterial;
            cultivationMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            cultivationMaterial.name = "Guardian Ent Cultivation Shared";
            cultivationMaterial.color = new Color(.25f, .86f, .28f);
            return cultivationMaterial;
        }

        void OnDied() => ClearMarker();
        void ClearMarker(bool immediate = false)
        {
            if (!markerRoot) return;
            markerRoot.gameObject.SetActive(false);
            if (immediate || !Application.isPlaying) DestroyImmediate(markerRoot.gameObject); else Destroy(markerRoot.gameObject);
            markerRoot = null;
        }
        void UnsubscribeHealth()
        {
            if (health) health.Died -= OnDied;
            health = null;
        }
        void OnDisable()
        {
            UnsubscribeHealth();
            ClearMarker(true);
        }
        void OnDestroy()
        {
            UnsubscribeHealth();
            ClearMarker(true);
        }
    }
}
