using UnityEditor;
using UnityEngine;

namespace RealmRaiders.Editor
{
    public sealed class PrototypeWorldSurfacePreviewImport : AssetPostprocessor
    {
        public const string SylvanAssetPath = "Assets/Game/Resources/Art/WorldSurfaces/MWS07-SylvanPath/sylvan-stone-path-edge-hardened-candidate.png";
        public const string InfernalAssetPath = "Assets/Game/Resources/Art/WorldSurfaces/MWS07-InfernalPath/infernal-basalt-path-edge-hardened-candidate.png";

        void OnPreprocessTexture()
        {
            if (assetPath != SylvanAssetPath && assetPath != InfernalAssetPath) return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.mipmapEnabled = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.isReadable = false;

            var android = importer.GetPlatformTextureSettings("Android");
            android.name = "Android";
            android.overridden = true;
            android.maxTextureSize = 512;
            android.format = TextureImporterFormat.ASTC_6x6;
            android.textureCompression = TextureImporterCompression.Compressed;
            importer.SetPlatformTextureSettings(android);
        }
    }
}
