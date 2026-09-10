using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using RealmRaiders.Characters;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace RealmRaiders.Editor
{
    /// <summary>QA-only explicit build. No automatic imports, changes to the static pilot, or scene writes.</summary>
    public static class GuardianEntTree01MotionIntake
    {
        const string Parent = "Assets/Game/Art/ThirdParty/Tennessippi/FreeTreantPack/Tree01";
        public const string Folder = Parent + "/MotionPilot";
        public const string ModelPath = Folder + "/Tree01_GenericRuntime.fbx";
        public const string PrefabPath = "Assets/Game/Resources/Characters/GuardianEntTree01Animated.prefab";
        const string BindingName = "GuardianEntTree01Motion";
        public const string BindingPath = "Assets/Game/Resources/Characters/" + BindingName + ".asset";
        const string LocalSource = "Temp/Treant Package/Treant 1/Tree01_FBX.fbx";
        const string SourceHash = "bd90b4f8dd823334cbb226229ef731c1280ae601ab942d16193b96f5f674a02e";
        const string MaterialPath = Parent + "/materials/GuardianEntTree01Mobile.mat";
        const string StaticPrefab = "Assets/Game/Resources/Characters/GuardianEntTree01.prefab";
        const string Owner = "RealmRaiders.Tree01.MotionPilot.15.16B";
        static readonly string[] Takes = { "Idle", "Run", "Attack_1", "Death1" };

        [MenuItem("Realm Raiders/Art Intake/Build Guardian Ent Tree01 Motion Pilot")]
        public static void Build()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling && !EditorApplication.isUpdating,
                "Wait for clean compilation and Edit mode.");
            Require(Hash(LocalSource) == SourceHash && Hash(Parent + "/source/Tree01_FBX.fbx") == SourceHash, "Exact authorized source hash mismatch.");
            var licence = File.ReadAllText(Parent + "/../LICENSE_PROVENANCE.txt");
            Require(licence.Contains(SourceHash) && licence.Contains("https://creativecommons.org/publicdomain/zero/1.0/"), "Accepted provenance is missing.");
            var protectedPaths = new[] { Parent + "/source/Tree01_FBX.fbx", Parent + "/source/Tree01_FBX.fbx.meta",
                StaticPrefab, StaticPrefab + ".meta", MaterialPath, MaterialPath + ".meta" };
            var protectedHashes = protectedPaths.Select(Hash).ToArray();
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            Require(material && AssetDatabase.LoadAssetAtPath<GameObject>(StaticPrefab), "Accepted static visual/material must exist first.");
            foreach (var path in Takes.Select(t => Folder + "/" + t + ".anim").Concat(new[] { ModelPath, PrefabPath, BindingPath, Folder + "/PROVENANCE.txt" }))
                Require(!File.Exists(path) || AssetImporter.GetAtPath(path)?.userData == Owner, "Refusing unowned output: " + path);
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent, "MotionPilot");
            var binding = AssetDatabase.LoadAssetAtPath<LargeCreatureMotionBinding>(BindingPath);
            if (binding) { binding.name = BindingName; binding.ValidationVersion = 0; EditorUtility.SetDirty(binding); AssetDatabase.SaveAssetIfDirty(binding); }
            GameObject root = null;
            var preview = EditorSceneManager.NewPreviewScene();
            try
            {
                if (!File.Exists(ModelPath))
                { File.Copy(LocalSource, ModelPath, false); AssetDatabase.ImportAsset(ModelPath, ImportAssetOptions.ForceSynchronousImport); Mark(ModelPath); }
                Require(Hash(ModelPath) == SourceHash, "Runtime copy differs from exact Tree01 source.");
                ConfigureImport();
                var source = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
                Require(source, "No Generic runtime model.");
                var originals = AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview__", StringComparison.Ordinal)).ToArray();
                var clips = new AnimationClip[Takes.Length];
                for (var i = 0; i < Takes.Length; i++)
                {
                    var found = originals.Where(c => c.name.Substring(c.name.LastIndexOf('|') + 1) == Takes[i]).ToArray();
                    Require(found.Length == 1, "Missing/ambiguous take: " + Takes[i]);
                    clips[i] = SaveSkeletonClip(found[0], Takes[i], i < 2);
                }
                root = new GameObject("GuardianEntTree01Animated"); SceneManager.MoveGameObjectToScene(root, preview);
                var fit = new GameObject("Tree01 Fit").transform; fit.SetParent(root.transform, false); fit.localPosition = new Vector3(0, .004083f, 0);
                var model = (GameObject)PrefabUtility.InstantiatePrefab(source, preview);
                PrefabUtility.UnpackPrefabInstance(model, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                model.name = "Tree01 Source"; model.transform.SetParent(fit, false);
                var animators = root.GetComponentsInChildren<Animator>(true);
                Require(animators.Length == 1 && animators[0].gameObject == model, "Expected one model-root Animator.");
                var animator = animators[0]; animator.applyRootMotion = false; animator.fireEvents = false;
                animator.runtimeAnimatorController = null; animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                Require(animator.avatar && animator.avatar.isValid && !animator.avatar.isHuman, "Invalid Generic avatar.");
                var skins = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                Require(skins.Length == 1 && skins[0].sharedMesh && AssetDatabase.GetAssetPath(skins[0].sharedMesh) == ModelPath &&
                    skins[0].sharedMesh.vertexCount == 3010 && skins[0].sharedMesh.GetIndexCount(0) == 5438 * 3 && skins[0].bones.Length == 31,
                    "Skin differs from accepted 15.16A evidence.");
                skins[0].sharedMaterials = new[] { material }; skins[0].quality = SkinQuality.Bone4;
                skins[0].updateWhenOffscreen = true; // One reversible pilot; no culling-dependent pose freeze.
                var draft = ScriptableObject.CreateInstance<LargeCreatureMotionBinding>();
                draft.name = BindingName;
                try
                {
                    draft.VisualPrefab = root; draft.AnimatorPath = "Tree01 Fit/Tree01 Source"; draft.SkeletonRootPath = "Tree01";
                    draft.AnimatedBonePaths = clips.SelectMany(AnimationUtility.GetCurveBindings).Select(c => c.path)
                        .Distinct().OrderBy(p => p, StringComparer.Ordinal).ToArray();
                    draft.Idle = clips[0]; draft.Run = clips[1]; draft.Attack = clips[2]; draft.Death = clips[3];
                    draft.ValidationVersion = LargeCreatureMotionBinding.CurrentValidationVersion;
                    Require(draft.TryValidate(out var draftIssue), "Transient runtime binding invalid: " + draftIssue);
                    draft.VisualPrefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath, out var saved);
                    Require(saved && draft.VisualPrefab, "Could not save runtime visual prefab."); Mark(PrefabPath);
                    if (!binding)
                    {
                        binding = ScriptableObject.CreateInstance<LargeCreatureMotionBinding>();
                        binding.name = BindingName;
                        AssetDatabase.CreateAsset(binding, BindingPath);
                    }
                    EditorUtility.CopySerialized(draft, binding);
                    binding.name = BindingName; // CopySerialized also copies m_Name; both create and repair must retain file identity.
                    EditorUtility.SetDirty(binding); AssetDatabase.SaveAssetIfDirty(binding); Mark(BindingPath);
                    // Validate serialized asset references, not just the transient builder hierarchy.
                    AssetDatabase.ImportAsset(BindingPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
                    var savedBinding = AssetDatabase.LoadAssetAtPath<LargeCreatureMotionBinding>(BindingPath);
                    Require(savedBinding && savedBinding.name == BindingName, "Saved/reloaded binding missing or main-object name mismatch.");
                    Require(savedBinding.TryValidate(out var savedIssue), "Saved/reloaded runtime binding invalid: " + savedIssue);
                }
                finally { Object.DestroyImmediate(draft); }
                var provenance = "15.16B reversible Generic motion pilot, NOT production LOD/foot-contact approval.\n" +
                    "Source: https://tennessippistudios.itch.io/treant-pack\nLicence: CC0 https://creativecommons.org/publicdomain/zero/1.0/\n" +
                    "Exact local source: " + LocalSource + "\nFBX SHA256: " + SourceHash + "\n" +
                    "See ../../LICENSE_PROVENANCE.txt for creator/archive evidence. No additional download.\n" +
                    "Derived clips: Idle, Run, Attack_1, Death1; only Transform curves below Tree01/ retained. Root/mesh curves and Animation Events excluded.\n" +
                    "Separate Generic copy: 3010 vertices, 5438 triangles, 31 bones, max4 weights; accepted static source/prefab/material unchanged.\n" +
                    "Read/Write off, Medium compression, no colliders/cameras/lights/material import/constraints/custom animation properties.\n";
                File.WriteAllText(Folder + "/PROVENANCE.txt", provenance); AssetDatabase.ImportAsset(Folder + "/PROVENANCE.txt"); Mark(Folder + "/PROVENANCE.txt");
                Debug.Log("[Tree01 Motion Pilot] Built separate runtime prefab, four skeleton-only clips and validated binding. QA focused/final gates and manual feel review remain required.");
            }
            finally
            {
                if (root) Object.DestroyImmediate(root); EditorSceneManager.ClosePreviewScene(preview);
                for (var i = 0; i < protectedPaths.Length; i++) Require(Hash(protectedPaths[i]) == protectedHashes[i], "Protected static file changed: " + protectedPaths[i]);
            }
        }

        static void ConfigureImport()
        {
            var importer = AssetImporter.GetAtPath(ModelPath) as ModelImporter; Require(importer, "ModelImporter missing.");
            importer.globalScale = .7452205f; importer.useFileScale = importer.useFileUnits = importer.bakeAxisConversion = true;
            importer.meshCompression = ModelImporterMeshCompression.Medium; importer.isReadable = false;
            importer.weldVertices = importer.optimizeMeshPolygons = importer.optimizeMeshVertices = true;
            importer.importBlendShapes = importer.importVisibility = importer.addCollider = importer.importCameras = importer.importLights = false;
            importer.importNormals = ModelImporterNormals.Import; importer.importTangents = ModelImporterTangents.None;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.skinWeights = ModelImporterSkinWeights.Custom; importer.maxBonesPerVertex = 4;
            importer.optimizeGameObjects = false; importer.animationType = ModelImporterAnimationType.Generic;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel; importer.importAnimation = true;
            importer.importConstraints = importer.importAnimatedCustomProperties = false;
            importer.animationCompression = ModelImporterAnimationCompression.Off;
            var clips = importer.defaultClipAnimations; Require(clips.Length == 10, "Expected ten authorized source stacks.");
            foreach (var clip in clips)
            {
                clip.lockRootRotation = clip.lockRootHeightY = clip.lockRootPositionXZ = true;
                clip.keepOriginalOrientation = clip.keepOriginalPositionY = clip.keepOriginalPositionXZ = true;
                clip.loopTime = clip.loopPose = false; clip.events = Array.Empty<AnimationEvent>();
            }
            importer.clipAnimations = clips; importer.userData = Owner; importer.SaveAndReimport();
        }

        static AnimationClip SaveSkeletonClip(AnimationClip source, string name, bool loop)
        {
            var draft = new AnimationClip { name = name, frameRate = source.frameRate, legacy = false };
            try
            {
                Require(AnimationUtility.GetObjectReferenceCurveBindings(source).Length == 0, "Unexpected source object-reference curves.");
                foreach (var curve in AnimationUtility.GetCurveBindings(source))
                {
                    Require(curve.type == typeof(Transform), "Unexpected non-transform source animation.");
                    if (!curve.path.StartsWith("Tree01/", StringComparison.Ordinal)) continue;
                    Require(curve.propertyName.StartsWith("m_LocalPosition.", StringComparison.Ordinal) ||
                        curve.propertyName.StartsWith("m_LocalRotation.", StringComparison.Ordinal) || curve.propertyName.StartsWith("m_LocalScale.", StringComparison.Ordinal), "Unexpected transform channel.");
                    var values = AnimationUtility.GetEditorCurve(source, curve);
                    Require(values.keys.All(k => CharacterCombatPresentationTimeline.Finite(k.time) && CharacterCombatPresentationTimeline.Finite(k.value)), "Non-finite source pose.");
                    AnimationUtility.SetEditorCurve(draft, curve, values);
                }
                draft.EnsureQuaternionContinuity();
                var settings = AnimationUtility.GetAnimationClipSettings(draft);
                settings.startTime = 0; settings.stopTime = source.length;
                settings.loopTime = loop; settings.loopBlend = false; AnimationUtility.SetAnimationClipSettings(draft, settings);
                AnimationUtility.SetAnimationEvents(draft, Array.Empty<AnimationEvent>());
                Require(LargeCreatureMotionBinding.SafeClip(draft) && AnimationUtility.GetCurveBindings(draft).Length > 0 && Mathf.Abs(draft.length - source.length) < .001f,
                    "Sanitized clip failed duration/root/event validation: " + name);
                var path = Folder + "/" + name + ".anim";
                var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (existing) { EditorUtility.CopySerialized(draft, existing); EditorUtility.SetDirty(existing); AssetDatabase.SaveAssetIfDirty(existing); }
                else { existing = Object.Instantiate(draft); existing.name = name; AssetDatabase.CreateAsset(existing, path); }
                Mark(path); return existing;
            }
            finally { Object.DestroyImmediate(draft); }
        }
        static string Hash(string path)
        { using var stream = File.OpenRead(path); using var sha = SHA256.Create(); return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant(); }
        static void Mark(string path) { var importer = AssetImporter.GetAtPath(path); Require(importer, "Output importer missing."); importer.userData = Owner; importer.SaveAndReimport(); }
        static void Require(bool value, string message) { if (!value) throw new InvalidOperationException("[Tree01 Motion Pilot] " + message); }
    }
}
