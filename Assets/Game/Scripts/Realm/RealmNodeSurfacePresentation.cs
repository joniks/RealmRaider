using System;
using RealmRaiders.Core;
using UnityEngine;

namespace RealmRaiders.Realm
{
    [DisallowMultipleComponent]
    public sealed class RealmNodeSurfacePresentation : MonoBehaviour
    {
        public const string AlbedoResource = "Art/WorldSurfaces/MWS10-SylvanClearingFloor/sylvan-clearing-floor-albedo-rgb-candidate";
        public const string NormalResource = "Art/WorldSurfaces/MWS10-SylvanClearingFloor/sylvan-clearing-floor-mobile-normal-rgb-candidate";
        public const float WorldUnitsPerTile = 3.5f;
        public const float NodeWorldDiameter = 6.5f;
        public const float NormalStrength = .35f;
        public static readonly float TextureTiling = NodeWorldDiameter / WorldUnitsPerTile;

        static Material sharedSurfaceMaterial;
        static Texture2D albedo;
        static Texture2D normal;
        static bool surfaceResolved;
        static Func<string, Texture2D> textureLoader = LoadTexture;

        Renderer surfaceRenderer;
        Material fallbackMaterial;
        MaterialPropertyBlock tintProperties;

        public Renderer SurfaceRenderer => surfaceRenderer;
        public bool UsesSurface { get; private set; }
        public Color AppliedTint { get; private set; }

        public static RealmNodeSurfacePresentation Bind(Renderer renderer, Color fallbackColor)
        {
            if (!renderer) return null;
            var presentation = renderer.GetComponent<RealmNodeSurfacePresentation>()
                ?? renderer.gameObject.AddComponent<RealmNodeSurfacePresentation>();
            presentation.surfaceRenderer = renderer;
            if (ResolveSurface())
            {
                renderer.sharedMaterial = SharedSurfaceMaterial();
                presentation.UsesSurface = true;
            }
            else
            {
                if (!presentation.fallbackMaterial)
                    presentation.fallbackMaterial = PrototypeRuntimeFactory.Material(fallbackColor);
                renderer.material = presentation.fallbackMaterial;
                presentation.UsesSurface = false;
            }
            return presentation;
        }

        public void ApplyTint(Color tint)
        {
            AppliedTint = tint;
            if (!surfaceRenderer) return;
            if (!UsesSurface)
            {
                fallbackMaterial.color = tint;
                return;
            }

            tintProperties ??= new MaterialPropertyBlock();
            surfaceRenderer.GetPropertyBlock(tintProperties);
            var material = surfaceRenderer.sharedMaterial;
            if (material && material.HasProperty("_BaseColor")) tintProperties.SetColor("_BaseColor", tint);
            if (material && material.HasProperty("_Color")) tintProperties.SetColor("_Color", tint);
            surfaceRenderer.SetPropertyBlock(tintProperties);
        }

        static Material SharedSurfaceMaterial()
        {
            if (sharedSurfaceMaterial) return sharedSurfaceMaterial;
            sharedSurfaceMaterial = PrototypeRuntimeFactory.Material(Color.white);
            sharedSurfaceMaterial.name = "Sylvan Clearing Floor Surface";
            sharedSurfaceMaterial.mainTexture = albedo;
            sharedSurfaceMaterial.mainTextureScale = Vector2.one * TextureTiling;
            if (sharedSurfaceMaterial.HasProperty("_BumpMap"))
            {
                sharedSurfaceMaterial.SetTexture("_BumpMap", normal);
                if (sharedSurfaceMaterial.HasProperty("_BumpScale")) sharedSurfaceMaterial.SetFloat("_BumpScale", NormalStrength);
                sharedSurfaceMaterial.EnableKeyword("_NORMALMAP");
            }
            return sharedSurfaceMaterial;
        }

        static bool ResolveSurface()
        {
            if (surfaceResolved) return albedo && normal;
            surfaceResolved = true;
            try
            {
                albedo = textureLoader?.Invoke(AlbedoResource);
                if (albedo) normal = textureLoader?.Invoke(NormalResource);
            }
            catch (Exception)
            {
                albedo = null;
                normal = null;
            }
            if (albedo && normal) return true;
            albedo = null;
            normal = null;
            return false;
        }

        static Texture2D LoadTexture(string resourcePath) => Resources.Load<Texture2D>(resourcePath);

        public static void ConfigureTextureLoaderForTests(Func<string, Texture2D> loader)
        {
            ResetSurfaceCache();
            textureLoader = loader ?? (_ => null);
        }

        public static void ResetTextureLoaderForTests()
        {
            ResetSurfaceCache();
            textureLoader = LoadTexture;
        }

        static void ResetSurfaceCache()
        {
            if (sharedSurfaceMaterial)
            {
                if (Application.isPlaying) Destroy(sharedSurfaceMaterial);
                else DestroyImmediate(sharedSurfaceMaterial);
            }
            sharedSurfaceMaterial = null;
            albedo = null;
            normal = null;
            surfaceResolved = false;
        }

        void OnDestroy()
        {
            surfaceRenderer = null;
            fallbackMaterial = null;
            tintProperties = null;
        }
    }
}
