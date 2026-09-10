using UnityEngine;

namespace RealmRaiders.Core
{
    public enum RealmRouteStyle
    {
        SylvanOrganic,
        InfernalFractured
    }

    /// <summary>Builds the two approved low route silhouettes beneath existing authoritative floor roots.</summary>
    public static class RealmRoutePresentation
    {
        public const string RootName = "Realm Route Presentation";
        public const int SegmentRendererCeiling = 1;
        public const int DefenseRendererCeiling = 4;
        public const string SylvanAlbedoResource = "Art/WorldSurfaces/MWS07-SylvanPath/sylvan-stone-path-edge-hardened-candidate";
        public const string InfernalAlbedoResource = "Art/WorldSurfaces/MWS07-InfernalPath/infernal-basalt-path-edge-hardened-candidate";
        public const string InfernalCourtyardAlbedoResource = "Art/WorldSurfaces/MWS11-InfernalCourtyardFloor/infernal-courtyard-floor-albedo-rgb-candidate";
        public const string InfernalCourtyardNormalResource = "Art/WorldSurfaces/MWS11-InfernalCourtyardFloor/infernal-courtyard-floor-mobile-normal-rgb-candidate";
        public const float InfernalCourtyardNormalStrength = .30f;
        public const float InfernalCourtyardWorldUnitsPerTile = 4f;

        static Material sylvanFloor;
        static Material sylvanRoute;
        static Material infernalFloor;
        static Material infernalRoute;
        static Texture2D sylvanAlbedo;
        static Texture2D infernalAlbedo;
        static Texture2D infernalCourtyardAlbedo;
        static Texture2D infernalCourtyardNormal;
        static bool sylvanAlbedoResolved;
        static bool infernalAlbedoResolved;
        static bool infernalCourtyardSurfaceResolved;
        static System.Func<string, Texture2D> textureLoader = LoadTexture;

        public static Transform BuildSegment(Transform authoritativeRoot, RealmRouteStyle style)
        {
            return Build(authoritativeRoot, style, true);
        }

        public static Transform BuildDefenseLane(Transform authoritativeRoot, RealmRouteStyle style)
        {
            return Build(authoritativeRoot, style, false);
        }

        static Transform Build(Transform authoritativeRoot, RealmRouteStyle style, bool singleSegment)
        {
            if (!authoritativeRoot) return null;
            var existing = authoritativeRoot.Find(RootName);
            if (existing) return existing;

            var dimensions = Abs(authoritativeRoot.lossyScale);
            var presentation = new GameObject(RootName).transform;
            presentation.SetParent(authoritativeRoot, false);
            presentation.localPosition = Vector3.zero;
            presentation.localRotation = Quaternion.identity;
            presentation.localScale = Reciprocal(authoritativeRoot.lossyScale);

            var floorMaterial = style == RealmRouteStyle.SylvanOrganic ? SylvanFloor : InfernalFloor;
            SetRootMaterial(authoritativeRoot, floorMaterial,
                style == RealmRouteStyle.InfernalFractured && InfernalCourtyardSurfaceAvailable
                    ? new Vector2(dimensions.x / InfernalCourtyardWorldUnitsPerTile, dimensions.z / InfernalCourtyardWorldUnitsPerTile)
                    : (Vector2?)null);
            if (singleSegment)
            {
                BuildCoveredSegment(presentation, style, dimensions);
                var authoritativeRenderer = authoritativeRoot.GetComponent<Renderer>();
                if (authoritativeRenderer) authoritativeRenderer.enabled = false;
            }
            else if (style == RealmRouteStyle.SylvanOrganic) BuildSylvanDefenseLane(presentation, dimensions);
            else BuildInfernalDefenseLane(presentation, dimensions);
            return presentation;
        }

        static Material SylvanFloor => Shared(ref sylvanFloor, new Color(.08f, .24f, .1f), SylvanTexture);
        static Material SylvanRoute => Shared(ref sylvanRoute, new Color(.25f, .36f, .22f), SylvanTexture);
        static Material InfernalFloor => SharedInfernalCourtyardFloor();
        static Material InfernalRoute => Shared(ref infernalRoute, new Color(.27f, .23f, .2f), InfernalTexture);
        static Texture2D SylvanTexture => ResolveTexture(ref sylvanAlbedo, ref sylvanAlbedoResolved, SylvanAlbedoResource);
        static Texture2D InfernalTexture => ResolveTexture(ref infernalAlbedo, ref infernalAlbedoResolved, InfernalAlbedoResource);
        static bool InfernalCourtyardSurfaceAvailable => ResolveInfernalCourtyardSurface();

        static Material Shared(ref Material material, Color fallbackColor, Texture2D albedo)
        {
            if (material) return material;
            material = PrototypeRuntimeFactory.Material(albedo ? Color.white : fallbackColor);
            if (albedo) material.mainTexture = albedo;
            return material;
        }

        static Material SharedInfernalCourtyardFloor()
        {
            if (infernalFloor) return infernalFloor;
            infernalFloor = PrototypeRuntimeFactory.Material(new Color(.12f, .045f, .035f));
            if (!ResolveInfernalCourtyardSurface()) return infernalFloor;
            infernalFloor.color = Color.white;
            infernalFloor.mainTexture = infernalCourtyardAlbedo;
            if (infernalFloor.HasProperty("_BumpMap"))
            {
                infernalFloor.SetTexture("_BumpMap", infernalCourtyardNormal);
                if (infernalFloor.HasProperty("_BumpScale")) infernalFloor.SetFloat("_BumpScale", InfernalCourtyardNormalStrength);
                infernalFloor.EnableKeyword("_NORMALMAP");
            }
            return infernalFloor;
        }

        static Texture2D ResolveTexture(ref Texture2D texture, ref bool resolved, string resourcePath)
        {
            if (resolved) return texture;
            resolved = true;
            try { texture = textureLoader?.Invoke(resourcePath); }
            catch (System.Exception) { texture = null; }
            return texture;
        }

        static bool ResolveInfernalCourtyardSurface()
        {
            if (infernalCourtyardSurfaceResolved) return infernalCourtyardAlbedo && infernalCourtyardNormal;
            infernalCourtyardSurfaceResolved = true;
            try
            {
                infernalCourtyardAlbedo = textureLoader?.Invoke(InfernalCourtyardAlbedoResource);
                if (infernalCourtyardAlbedo) infernalCourtyardNormal = textureLoader?.Invoke(InfernalCourtyardNormalResource);
            }
            catch (System.Exception)
            {
                infernalCourtyardAlbedo = null;
                infernalCourtyardNormal = null;
            }
            if (infernalCourtyardAlbedo && infernalCourtyardNormal) return true;
            infernalCourtyardAlbedo = null;
            infernalCourtyardNormal = null;
            return false;
        }

        static Texture2D LoadTexture(string resourcePath) => Resources.Load<Texture2D>(resourcePath);

        public static void ConfigureTextureLoaderForTests(System.Func<string, Texture2D> loader)
        {
            ResetMaterialCache();
            textureLoader = loader ?? (_ => null);
        }

        public static void ResetTextureLoaderForTests()
        {
            ResetMaterialCache();
            textureLoader = LoadTexture;
        }

        static void ResetMaterialCache()
        {
            DestroyMaterial(ref sylvanFloor);
            DestroyMaterial(ref sylvanRoute);
            DestroyMaterial(ref infernalFloor);
            DestroyMaterial(ref infernalRoute);
            sylvanAlbedo = infernalAlbedo = infernalCourtyardAlbedo = infernalCourtyardNormal = null;
            sylvanAlbedoResolved = infernalAlbedoResolved = infernalCourtyardSurfaceResolved = false;
        }

        static void DestroyMaterial(ref Material material)
        {
            if (material)
            {
                if (Application.isPlaying) UnityEngine.Object.Destroy(material);
                else UnityEngine.Object.DestroyImmediate(material);
            }
            material = null;
        }

        static void BuildCoveredSegment(Transform root, RealmRouteStyle style, Vector3 dimensions)
        {
            var surface = dimensions.y * .5f + .04f;
            if (style == RealmRouteStyle.SylvanOrganic)
            {
                Part(root, "Organic Route Band", PrimitiveType.Capsule, new Vector3(0, surface, 0), new Vector3(dimensions.x, dimensions.z * .5f, .08f), Quaternion.Euler(90, 0, 0), SylvanRoute);
            }
            else
            {
                Part(root, "Fractured Route Plate", PrimitiveType.Cube, new Vector3(0, surface, 0), new Vector3(dimensions.x, .08f, dimensions.z), Quaternion.identity, InfernalRoute);
            }
        }

        static void BuildSylvanDefenseLane(Transform root, Vector3 dimensions)
        {
            var spacing = dimensions.z / 4f;
            var width = Mathf.Min(dimensions.x * .62f, 8.6f);
            var offsets = new[] { -.25f, .3f, -.35f, .2f };
            for (var index = 0; index < 4; index++)
            {
                var z = -dimensions.z * .5f + spacing * (index + .5f);
                Part(root, $"Organic Lane Mass {index + 1}", PrimitiveType.Capsule, new Vector3(offsets[index], dimensions.y * .5f + .045f, z), new Vector3(width, (spacing + 1f) * .5f, .09f), Quaternion.Euler(90, 0, 0), SylvanRoute);
            }
        }

        static void BuildInfernalDefenseLane(Transform root, Vector3 dimensions)
        {
            var spacing = dimensions.z / 4f;
            var width = Mathf.Min(dimensions.x * .6f, 8.2f);
            var offsets = new[] { -.2f, .25f, -.15f, .2f };
            var yaws = new[] { -1.5f, 1f, -1f, 1.5f };
            for (var index = 0; index < 4; index++)
            {
                var z = -dimensions.z * .5f + spacing * (index + .5f);
                Part(root, $"Basalt Causeway Plate {index + 1}", PrimitiveType.Cube, new Vector3(offsets[index], dimensions.y * .5f + .04f, z), new Vector3(width, .08f, spacing - .5f), Quaternion.Euler(0, yaws[index], 0), InfernalRoute);
            }
        }

        static void SetRootMaterial(Transform root, Material material, Vector2? textureTiling = null)
        {
            var renderer = root.GetComponent<Renderer>();
            if (!renderer) return;
            renderer.sharedMaterial = material;
            if (!textureTiling.HasValue) return;
            var properties = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(properties);
            var scaleOffset = new Vector4(textureTiling.Value.x, textureTiling.Value.y, 0, 0);
            if (material.HasProperty("_BaseMap")) properties.SetVector("_BaseMap_ST", scaleOffset);
            if (material.HasProperty("_MainTex")) properties.SetVector("_MainTex_ST", scaleOffset);
            if (material.HasProperty("_BumpMap")) properties.SetVector("_BumpMap_ST", scaleOffset);
            renderer.SetPropertyBlock(properties);
        }

        static Transform Part(Transform parent, string name, PrimitiveType type, Vector3 position, Vector3 scale, Quaternion rotation, Material material)
        {
            var part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localRotation = rotation;
            part.transform.localScale = scale;
            var collider = part.GetComponent<Collider>();
            if (collider)
            {
                collider.enabled = false;
                if (Application.isPlaying) UnityEngine.Object.Destroy(collider); else UnityEngine.Object.DestroyImmediate(collider);
            }
            part.GetComponent<Renderer>().sharedMaterial = material;
            return part.transform;
        }

        static Vector3 Abs(Vector3 value) => new(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
        static Vector3 Reciprocal(Vector3 value) => new(SafeReciprocal(value.x), SafeReciprocal(value.y), SafeReciprocal(value.z));
        static float SafeReciprocal(float value) => Mathf.Abs(value) < .0001f ? 1 : 1 / value;
    }
}
