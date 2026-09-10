using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace RealmRaiders.Editor
{
    /// <summary>Explicit QA-owned intake, never an import callback or runtime factory.</summary>
    public static class GuardianEntTree01Intake
    {
        const string Root = "Assets/Game/Art/ThirdParty/Tennessippi/FreeTreantPack";
        const string ModelPath = Root + "/Tree01/source/Tree01_FBX.fbx";
        const string AlbedoPath = Root + "/Tree01/textures/Tree01 Albedo Light.png";
        const string NormalPath = Root + "/Tree01/textures/Treant_1_LP_DefaultMaterial_Normal.png";
        const string MaskPath = Root + "/Tree01/textures/Treant_1_LP_DefaultMaterial_MaskMap.png";
        const string MaterialPath = Root + "/Tree01/materials/GuardianEntTree01Mobile.mat";
        const string PrefabPath = "Assets/Game/Resources/Characters/GuardianEntTree01.prefab";
        const string Owner = "RealmRaiders.Tree01.LightPilot.15.15";
        const string ShaderName = "Universal Render Pipeline/Simple Lit";

        [MenuItem("Realm Raiders/Art Intake/Build Guardian Ent Tree01 Pilot")]
        public static void BuildPilot()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling && !EditorApplication.isUpdating,
                "Wait for Edit mode and the current import/compilation to finish.");
            // Validate everything before creating/updating either output asset. No importer repair here.
            ValidateInputs();
            ValidateOutputOwnership(MaterialPath, typeof(Material));
            ValidateOutputOwnership(PrefabPath, typeof(GameObject));
            var shader = Shader.Find(ShaderName);
            Require(shader, "URP Simple Lit shader is unavailable; no replacement shader is permitted.");
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            Require(source, "The exact Tree01 source has not imported as a GameObject.");
            ValidateHierarchy(source);
            Require(AssetDatabase.IsValidFolder(Root + "/Tree01") &&
                AssetDatabase.IsValidFolder("Assets/Game/Resources/Characters"), "Expected output parent folders must already be imported.");

            // Isolated unsaved preview scene: never modify or save the user's open gameplay scene.
            var preview = EditorSceneManager.NewPreviewScene();
            GameObject root = null;
            Material draft = null;
            try
            {
                root = new GameObject("GuardianEntTree01");
                SceneManager.MoveGameObjectToScene(root, preview);
                var fit = new GameObject("Tree01 Fit").transform;
                fit.SetParent(root.transform, false);
                fit.localPosition = new Vector3(0, .004083f, 0);
                var body = (GameObject)PrefabUtility.InstantiatePrefab(source, preview);
                PrefabUtility.UnpackPrefabInstance(body, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                body.name = "Tree01 Source";
                body.transform.SetParent(fit, false); // Preserve the imported bind transforms, not a guessed FBX fileID.
                ValidateHierarchy(root);

                draft = new Material(shader) { name = "GuardianEntTree01Mobile" };
                foreach (var property in draft.GetTexturePropertyNames()) draft.SetTexture(property, null);
                draft.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(AlbedoPath));
                draft.SetColor("_BaseColor", Color.white);
                draft.SetColor("_SpecColor", Color.black);
                draft.SetColor("_EmissionColor", Color.black);
                draft.SetFloat("_Smoothness", 0);
                draft.SetFloat("_Surface", 0);
                draft.SetFloat("_AlphaClip", 0);
                draft.SetFloat("_SpecularHighlights", 0);
                draft.DisableKeyword("_NORMALMAP");
                draft.DisableKeyword("_EMISSION");
                draft.DisableKeyword("_ALPHATEST_ON");
                draft.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                draft.renderQueue = -1;

                if (!AssetDatabase.IsValidFolder(Root + "/Tree01/materials"))
                    Require(!string.IsNullOrEmpty(AssetDatabase.CreateFolder(Root + "/Tree01", "materials")), "Cannot create the owned material folder.");
                var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
                if (material)
                {
                    EditorUtility.CopySerialized(draft, material);
                    EditorUtility.SetDirty(material);
                    AssetDatabase.SaveAssetIfDirty(material);
                }
                else
                {
                    material = draft;
                    AssetDatabase.CreateAsset(material, MaterialPath);
                    draft = null; // Asset now owns it.
                }
                MarkOwned(MaterialPath);
                var renderer = root.GetComponentInChildren<MeshRenderer>(true);
                renderer.sharedMaterials = new[] { material };
                var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath, out var saved);
                Require(saved && prefab, "Unity could not save the Tree01 prefab. Inspect the exact output paths before retrying.");
                MarkOwned(PrefabPath);
                Debug.Log($"[Tree01 Intake] Saved {MaterialPath} and {PrefabPath}. " +
                    $"One static renderer, {renderer.GetComponent<MeshFilter>().sharedMesh.GetIndexCount(0) / 3} triangles. " +
                    "Pilot fit and mobile readability still require QA; no animation or production-budget claim.");
            }
            finally
            {
                if (draft) Object.DestroyImmediate(draft);
                if (root) Object.DestroyImmediate(root);
                EditorSceneManager.ClosePreviewScene(preview);
            }
        }

        static void ValidateInputs()
        {
            VerifyHash(ModelPath, "bd90b4f8dd823334cbb226229ef731c1280ae601ab942d16193b96f5f674a02e");
            VerifyHash(AlbedoPath, "cd50e1a9179f1242a862f6fb7f8448cff5cbbd56d599e687d916dc8583625573");
            VerifyHash(NormalPath, "1b0372d5b2797d520c33cfa2839b46f4536f2ce15104e23f5c50cb4a1b503d2f");
            VerifyHash(MaskPath, "72b6fe38c9ca4f1b6a7f6f457a73b4900a7897eb3129a4079472eec5c48e093c");
            var provenance = File.ReadAllText(Root + "/LICENSE_PROVENANCE.txt");
            Require(provenance.Contains("https://tennessippistudios.itch.io/treant-pack") &&
                provenance.Contains("https://creativecommons.org/publicdomain/zero/1.0/") &&
                provenance.Contains("1fcddcbd1fbbc8ec84f6001b2192d22794a927c9be2a929457a537e908862cd8"), "Accepted provenance is absent or changed.");
            var model = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
            Require(model, "Tree01 ModelImporter is unavailable.");
            Require(Mathf.Abs(model.globalScale - .7452205f) < .0000001f && model.useFileScale && model.useFileUnits && model.bakeAxisConversion,
                "Tree01 scale/file-unit/axis settings differ from the accepted manifest.");
            Require(model.meshCompression == ModelImporterMeshCompression.Medium && !model.isReadable && model.weldVertices &&
                model.optimizeMeshPolygons && model.optimizeMeshVertices && !model.importBlendShapes && !model.importVisibility &&
                !model.addCollider && !model.importCameras && !model.importLights, "Tree01 mesh/safety import settings differ from the manifest.");
            Require(!model.importAnimation && model.animationType == ModelImporterAnimationType.None &&
                model.importNormals == ModelImporterNormals.Import && model.importTangents == ModelImporterTangents.None &&
                model.materialImportMode == ModelImporterMaterialImportMode.None && model.skinWeights == ModelImporterSkinWeights.Custom &&
                model.maxBonesPerVertex == 4, "Tree01 rig, material, normals or weight settings differ from the static pilot.");
            Require(!AssetDatabase.LoadAllAssetsAtPath(ModelPath).Any(asset => asset is AnimationClip || asset is Material),
                "Unexpected imported animation or embedded material.");
            ValidateTexture(AlbedoPath, TextureImporterType.Default, true, TextureImporterAlphaSource.None);
            ValidateTexture(NormalPath, TextureImporterType.NormalMap, false, TextureImporterAlphaSource.None);
            ValidateTexture(MaskPath, TextureImporterType.Default, false, TextureImporterAlphaSource.FromInput);
        }

        static void ValidateTexture(string path, TextureImporterType type, bool srgb, TextureImporterAlphaSource alpha)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            Require(importer, "TextureImporter unavailable: " + path);
            var android = importer.GetPlatformTextureSettings("Android");
            Require(importer.textureType == type && importer.sRGBTexture == srgb && importer.alphaSource == alpha &&
                !importer.isReadable && importer.mipmapEnabled && importer.maxTextureSize == 1024 &&
                importer.filterMode == FilterMode.Bilinear && importer.anisoLevel == 1 && !importer.flipGreenChannel &&
                android.overridden && android.maxTextureSize == 1024 && android.format == TextureImporterFormat.ASTC_6x6,
                "Texture settings differ from the light manifest: " + path);
            Require(AssetDatabase.LoadAssetAtPath<Texture2D>(path), "Texture has not imported: " + path);
        }

        static void ValidateHierarchy(GameObject root)
        {
            // Fail closed rather than executing/removing unknown scripts or silently accepting new source components.
            foreach (var component in root.GetComponentsInChildren<Component>(true))
                Require(component && (component is Transform || component is MeshFilter || component is MeshRenderer),
                    "Unexpected Tree01 component: " + (component ? component.GetType().Name : "missing script"));
            var renderers = root.GetComponentsInChildren<MeshRenderer>(true);
            var filters = root.GetComponentsInChildren<MeshFilter>(true);
            Require(renderers.Length == 1 && filters.Length == 1, "Expected exactly one Tree01 static MeshFilter/MeshRenderer pair.");
            Require(renderers[0].gameObject == filters[0].gameObject,
                "Tree01 MeshFilter and MeshRenderer must belong to the same mesh GameObject.");
            var mesh = filters[0].sharedMesh;
            Require(mesh && AssetDatabase.GetAssetPath(mesh) == ModelPath && mesh.subMeshCount == 1 &&
                mesh.GetTopology(0) == MeshTopology.Triangles && mesh.GetIndexCount(0) == 5438 * 3,
                "Imported Tree01 mesh/submesh/triangle identity differs from the accepted pilot.");
        }

        static void VerifyHash(string path, string expected)
        {
            using var input = File.OpenRead(path);
            using var sha = SHA256.Create();
            var actual = BitConverter.ToString(sha.ComputeHash(input)).Replace("-", "").ToLowerInvariant();
            Require(actual == expected, "Source SHA-256 mismatch: " + path + " (" + actual + ")");
        }

        static void ValidateOutputOwnership(string path, Type expectedType)
        {
            if (!File.Exists(path)) return;
            var existing = AssetDatabase.LoadMainAssetAtPath(path);
            var importer = AssetImporter.GetAtPath(path);
            Require(existing && existing.GetType() == expectedType && importer && importer.userData == Owner,
                "Refusing to overwrite an unowned or unexpected output: " + path);
        }

        static void MarkOwned(string path)
        {
            var importer = AssetImporter.GetAtPath(path);
            Require(importer, "Output importer unavailable: " + path);
            importer.userData = Owner;
            importer.SaveAndReimport();
        }

        static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("[Tree01 Intake] " + message);
        }
    }
}
