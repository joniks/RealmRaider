using UnityEditor;
using UnityEngine;

namespace RealmRaiders.Editor
{
    public sealed class PrototypeJumpAbilityIconPreviewImport : AssetPostprocessor
    {
        public const string AssetPath = "Assets/Game/Resources/Art/UI/JumpAbility/jump-ability-icon-rgba-candidate.png";

        void OnPreprocessTexture()
        {
            if (assetPath != AssetPath) return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
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
