using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace RealmRaiders.Editor.Tree01Probe
{
    /// <summary>Explicit, Editor-only evidence builder. Never referenced by a runtime recipe.</summary>
    public static class GuardianEntTree01MotionProbe
    {
        public const string Root = "Assets/Game/Editor/Tree01MotionProbe";
        public const string Generated = Root + "/Generated";
        public const string ModelPath = Generated + "/Tree01_GenericProbe.fbx";
        public const string PrefabPath = Generated + "/Tree01_GenericProbe.prefab";
        public const string EvidencePath = Generated + "/ProbeEvidence.txt";
        public const string SourceHash = "bd90b4f8dd823334cbb226229ef731c1280ae601ab942d16193b96f5f674a02e";
        const string LocalSource = "Temp/Treant Package/Treant 1/Tree01_FBX.fbx";
        const string StaticModel = "Assets/Game/Art/ThirdParty/Tennessippi/FreeTreantPack/Tree01/source/Tree01_FBX.fbx";
        const string StaticPrefab = "Assets/Game/Resources/Characters/GuardianEntTree01.prefab";
        const string MaterialPath = "Assets/Game/Art/ThirdParty/Tennessippi/FreeTreantPack/Tree01/materials/GuardianEntTree01Mobile.mat";
        const string Owner = "RealmRaiders.Tree01.GenericProbe.15.16A";
        static readonly string[] SelectedTakes = { "Idle", "Run", "Attack_1", "Death1" };
        static readonly string[] ProtectedFiles = { StaticModel, StaticModel + ".meta", StaticPrefab, StaticPrefab + ".meta",
            MaterialPath, MaterialPath + ".meta", "Assets/Game/Scripts/Core/PrototypeRuntimeFactory.cs" };

        [MenuItem("Realm Raiders/Art Intake/Build Tree01 Generic Motion Probe")]
        public static void BuildProbe()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling && !EditorApplication.isUpdating,
                "Wait for Edit mode and successful compilation/import completion.");
            AssertRuntimeIsolation();
            Require(Hash(LocalSource) == SourceHash && Hash(StaticModel) == SourceHash, "Exact local/static source SHA-256 mismatch.");
            var protectedHashes = ProtectedFiles.Select(Hash).ToArray();
            foreach (var path in new[] { ModelPath, PrefabPath, EvidencePath }) RequireOwnedOrAbsent(path);
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            Require(material, "Accepted static albedo material is unavailable; no replacement is permitted.");
            if (!AssetDatabase.IsValidFolder(Generated)) AssetDatabase.CreateFolder(Root, "Generated");
            var report = new StringBuilder("Tree01 Generic feasibility probe — NOT runtime/production approval\n");
            report.AppendLine("source.sha256=" + SourceHash);
            report.AppendLine("unity=" + Application.unityVersion);
            report.AppendLine("sampling=21 explicit points per selected clip; no gameplay or Animation Events; visual quality still manual");
            try
            {
                if (!File.Exists(ModelPath))
                {
                    File.Copy(LocalSource, ModelPath, false);
                    AssetDatabase.ImportAsset(ModelPath, ImportAssetOptions.ForceSynchronousImport);
                    MarkOwned(ModelPath);
                }
                Require(Hash(ModelPath) == SourceHash, "Probe source is not the exact authorized FBX.");
                ConfigureImporter();
                var source = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
                Require(source, "Generic import did not produce a GameObject.");
                foreach (var group in source.GetComponentsInChildren<Component>(true)
                    .GroupBy(c => c ? c.GetType().FullName : "MISSING SCRIPT").OrderBy(g => g.Key, StringComparer.Ordinal))
                    report.AppendLine("component." + group.Key + "=" + group.Count());
                var clips = AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<AnimationClip>()
                    .Where(c => !c.name.StartsWith("__preview__", StringComparison.Ordinal)).OrderBy(c => c.name, StringComparer.Ordinal).ToArray();
                foreach (var clip in clips) DescribeClip(clip, report);
                MeasureAndSave(source, clips, material, report);
                AssertRuntimeIsolation();
                report.AppendLine("result=MEASURED; inspect per-clip risks, then perform manual preview; not an animation integration approval");
            }
            catch (Exception error)
            {
                report.AppendLine("result=BLOCKED: " + error.GetType().Name + ": " + error.Message);
                throw;
            }
            finally
            {
                for (var i = 0; i < ProtectedFiles.Length; i++)
                    Require(Hash(ProtectedFiles[i]) == protectedHashes[i], "Protected static asset changed: " + ProtectedFiles[i]);
                File.WriteAllText(EvidencePath, report.ToString().Replace("\r\n", "\n"), new UTF8Encoding(false));
                AssetDatabase.ImportAsset(EvidencePath, ImportAssetOptions.ForceSynchronousImport);
                MarkOwned(EvidencePath);
            }
            Debug.Log("[Tree01 Generic Probe] Evidence saved: " + EvidencePath + ". Static pilot preserved. Inspect all four selected clips manually; no runtime binding created.");
        }

        [MenuItem("Realm Raiders/Art Intake/Inspect Tree01 Generic Motion Probe")]
        public static void InspectProbe()
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            Require(model, "Build the isolated probe first.");
            Selection.activeObject = model;
            EditorGUIUtility.PingObject(model);
        }

        static void ConfigureImporter()
        {
            var importer = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
            Require(importer, "Probe ModelImporter missing.");
            importer.globalScale = .7452205f; importer.useFileScale = true; importer.useFileUnits = true;
            importer.bakeAxisConversion = true;
            importer.meshCompression = ModelImporterMeshCompression.Medium;
            importer.isReadable = true; // Editor-only measurement copy; never a production memory setting.
            importer.weldVertices = true; importer.optimizeMeshPolygons = true; importer.optimizeMeshVertices = true;
            importer.importBlendShapes = false; importer.importVisibility = false;
            importer.addCollider = false; importer.importCameras = false; importer.importLights = false;
            importer.importNormals = ModelImporterNormals.Import; importer.importTangents = ModelImporterTangents.None;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.skinWeights = ModelImporterSkinWeights.Custom; importer.maxBonesPerVertex = 4;
            importer.optimizeGameObjects = false;
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation = true;
            importer.importConstraints = false; importer.importAnimatedCustomProperties = false;
            importer.animationCompression = ModelImporterAnimationCompression.Off;
            var clips = importer.defaultClipAnimations;
            Require(clips.Length == 10, "Expected the ten source takes; got " + clips.Length + ".");
            foreach (var clip in clips)
            {
                clip.lockRootRotation = true; clip.lockRootHeightY = true; clip.lockRootPositionXZ = true;
                clip.keepOriginalOrientation = true; clip.keepOriginalPositionY = true; clip.keepOriginalPositionXZ = true;
                clip.loopTime = false; // Measure raw endpoints; wrapping could hide a bad loop seam.
                clip.loopPose = false;
                clip.events = Array.Empty<AnimationEvent>();
            }
            importer.clipAnimations = clips;
            importer.userData = Owner;
            importer.SaveAndReimport();
            ValidateImporter();
        }

        public static void ValidateImporter()
        {
            var importer = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
            Require(importer && importer.animationType == ModelImporterAnimationType.Generic && importer.importAnimation &&
                importer.avatarSetup == ModelImporterAvatarSetup.CreateFromThisModel && !importer.optimizeGameObjects &&
                !importer.addCollider && !importer.importCameras && !importer.importLights && !importer.importConstraints &&
                !importer.importAnimatedCustomProperties && importer.materialImportMode == ModelImporterMaterialImportMode.None &&
                importer.skinWeights == ModelImporterSkinWeights.Custom && importer.maxBonesPerVertex == 4,
                "Generic probe safety settings mismatch.");
            Require(importer.clipAnimations.Length == 10 && importer.clipAnimations.All(c =>
                c.lockRootRotation && c.lockRootHeightY && c.lockRootPositionXZ && (c.events == null || c.events.Length == 0)),
                "Probe clip root baking / event exclusion mismatch.");
        }

        static void MeasureAndSave(GameObject source, AnimationClip[] clips, Material material, StringBuilder report)
        {
            ValidateComponents(source);
            var preview = EditorSceneManager.NewPreviewScene();
            GameObject root = null;
            try
            {
                root = new GameObject("EDITOR ONLY Tree01 Generic Probe");
                SceneManager.MoveGameObjectToScene(root, preview);
                var pivot = Child(root.transform, "Presentation Pivot");
                var body = Child(pivot, "Base Body");
                var fit = Child(body, "Tree01 Fit"); fit.localPosition = new Vector3(0, .004083f, 0);
                var model = (GameObject)PrefabUtility.InstantiatePrefab(source, preview);
                PrefabUtility.UnpackPrefabInstance(model, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                model.transform.SetParent(fit, false);
                var animator = model.GetComponentInChildren<Animator>(true);
                Require(animator && animator.gameObject == model && animator.avatar && animator.avatar.isValid && !animator.avatar.isHuman,
                    "Generic root Animator/Avatar is absent, invalid or Humanoid.");
                animator.runtimeAnimatorController = null; animator.applyRootMotion = false;
                ValidateComponents(root);
                var renderer = model.GetComponentInChildren<SkinnedMeshRenderer>(true);
                renderer.sharedMaterials = new[] { material };
                renderer.quality = SkinQuality.Bone4;
                var mesh = renderer.sharedMesh;
                Require(mesh && AssetDatabase.GetAssetPath(mesh) == ModelPath && mesh.subMeshCount == 1 &&
                    mesh.GetTopology(0) == MeshTopology.Triangles && mesh.GetIndexCount(0) == 5438 * 3,
                    "Generic mesh identity/topology differs from the accepted source.");
                Require(renderer.rootBone && renderer.rootBone.IsChildOf(model.transform) &&
                    renderer.bones.All(b => b && b.IsChildOf(model.transform)), "Unowned or missing skin bones.");
                Require(mesh.bindposes.Length == renderer.bones.Length, "Bindpose/bone count mismatch.");
                report.AppendLine($"skin.vertices={mesh.vertexCount}; triangles={mesh.GetIndexCount(0) / 3}; bones={renderer.bones.Length}; bindposes={mesh.bindposes.Length}");
                for (var i = 0; i < renderer.bones.Length; i++)
                {
                    report.AppendLine("bone[" + i + "]=" + AnimationUtility.CalculateTransformPath(renderer.bones[i], model.transform));
                    var bindpose = mesh.bindposes[i];
                    Require(Enumerable.Range(0, 16).All(element => Finite(bindpose[element])), "Non-finite bindpose.");
                    report.AppendLine("bindpose[" + i + "]=" + string.Join(",", Enumerable.Range(0, 16).Select(element => F(bindpose[element]))));
                }
                DescribeWeights(mesh, report);
                foreach (var take in SelectedTakes)
                {
                    var matches = clips.Where(c => TakeName(c.name) == take).ToArray();
                    Require(matches.Length == 1, "Missing/ambiguous selected take: " + take);
                    SampleClip(model, root.transform, pivot, fit, renderer, matches[0], report);
                }
                ValidateComponents(root);
                Require(!animator.applyRootMotion && !animator.runtimeAnimatorController, "Probe acquired animation authority.");
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath, out var saved);
                Require(saved, "Probe prefab could not be saved.");
                MarkOwned(PrefabPath);
            }
            finally
            {
                if (root) Object.DestroyImmediate(root);
                EditorSceneManager.ClosePreviewScene(preview);
            }
        }

        public static void ValidateComponents(GameObject root)
        {
            Require(root, "Missing probe hierarchy.");
            foreach (var component in root.GetComponentsInChildren<Component>(true))
                Require(component && (component is Transform || component is SkinnedMeshRenderer || component is Animator),
                    "Forbidden probe component: " + (component ? component.GetType().FullName : "missing script"));
            Require(root.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length == 1 &&
                root.GetComponentsInChildren<Animator>(true).Length == 1, "Expected one skinned renderer and one visual-only Animator.");
            var animator = root.GetComponentInChildren<Animator>(true);
            Require(!animator.runtimeAnimatorController, "Probe must have no AnimatorController.");
            Require(!animator.applyRootMotion, "Probe root motion must be disabled.");
        }

        static void DescribeWeights(Mesh mesh, StringBuilder report)
        {
            using var perVertex = mesh.GetBonesPerVertex();
            using var weights = mesh.GetAllBoneWeights();
            var maximum = perVertex.Length == 0 ? 0 : perVertex.Max(v => (int)v);
            var empty = perVertex.Count(v => v == 0);
            var maxSumError = 0f; var cursor = 0;
            foreach (var count in perVertex)
            {
                var sum = 0f;
                for (var j = 0; j < count; j++)
                {
                    var weight = weights[cursor++];
                    Require(weight.boneIndex >= 0 && weight.boneIndex < mesh.bindposes.Length && Finite(weight.weight) && weight.weight >= 0,
                        "Invalid imported bone weight.");
                    sum += weight.weight;
                }
                maxSumError = Mathf.Max(maxSumError, Mathf.Abs(sum - 1));
            }
            report.AppendLine($"weights.maximum={maximum}; unweighted={empty}; sumError={F(maxSumError)}; editorReadWrite=true");
            Require(perVertex.Length == mesh.vertexCount && maximum <= 4 && empty == 0 && cursor == weights.Length && maxSumError < .001f,
                "Imported four-weight skin is incomplete/unnormalized.");
        }

        static void DescribeClip(AnimationClip clip, StringBuilder report)
        {
            report.AppendLine($"clip={clip.name}; seconds={F(clip.length)}; fps={F(clip.frameRate)}; loopCandidate={TakeName(clip.name) == "Idle" || TakeName(clip.name) == "Run"}; importedLoop={clip.isLooping}");
            Require(!clip.legacy && clip.length > 0 && Finite(clip.length), "Invalid Generic clip: " + clip.name);
            Require(AnimationUtility.GetAnimationEvents(clip).Length == 0 && AnimationUtility.GetObjectReferenceCurveBindings(clip).Length == 0,
                "Events or object-reference bindings are forbidden in the probe.");
            foreach (var binding in AnimationUtility.GetCurveBindings(clip).OrderBy(b => b.path, StringComparer.Ordinal)
                .ThenBy(b => b.type.FullName, StringComparer.Ordinal).ThenBy(b => b.propertyName, StringComparer.Ordinal))
            {
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                var keys = curve.keys;
                var span = keys.Length == 0 ? 0 : keys.Max(k => k.value) - keys.Min(k => k.value);
                report.AppendLine($"  binding={binding.path}|{binding.type.FullName}|{binding.propertyName}; keySpan={F(span)}; risk={BindingRisk(binding.path, binding.propertyName)}");
            }
        }

        public static string BindingRisk(string path, string property)
        {
            if (string.IsNullOrEmpty(path)) return "ANIMATION_ROOT (even constant curves can overwrite fit)";
            if (path.Split('/').Any(p => p == "Presentation Pivot" || p == "Base Body" || p == "Tree01 Fit")) return "PROTECTED_PRESENTATION_TRANSFORM";
            if (path == "Tree01") return "SOURCE_ROOT (inspect local transform and in-place evidence)";
            if (property.StartsWith("Root", StringComparison.Ordinal) || property.StartsWith("Motion", StringComparison.Ordinal)) return "ROOT_MOTION_CHANNEL";
            return "DESCENDANT (not a safety approval)";
        }

        static void SampleClip(GameObject model, Transform root, Transform pivot, Transform fit, SkinnedMeshRenderer renderer, AnimationClip clip, StringBuilder report)
        {
            var transforms = root.GetComponentsInChildren<Transform>(true);
            var baseline = transforms.Select(t => new LocalPose(t)).ToArray();
            var sourceRoot = model.transform.Find("Tree01");
            Require(sourceRoot, "Expected explicit Tree01 source root for drift measurement.");
            var protectedTransforms = new[] { root, pivot, fit.parent, fit, model.transform, sourceRoot };
            var protectedBaseline = protectedTransforms.Select(t => new LocalPose(t)).ToArray();
            var maxPosition = new float[protectedTransforms.Length];
            var maxRotation = new float[protectedTransforms.Length];
            var maxScale = new float[protectedTransforms.Length];
            var baked = new Mesh();
            Vector3[] first = null, last = null;
            float maxPoseChange = 0, maxCentredChange = 0;
            try
            {
                for (var step = 0; step <= 20; step++)
                {
                    for (var i = 0; i < transforms.Length; i++) baseline[i].Restore(transforms[i]);
                    clip.SampleAnimation(model, clip.length * step / 20f);
                    renderer.BakeMesh(baked);
                    var vertices = baked.vertices;
                    Require(vertices.Length == renderer.sharedMesh.vertexCount && vertices.All(v => Finite(v.x) && Finite(v.y) && Finite(v.z)),
                        "Invalid baked mesh sample: " + clip.name);
                    if (first == null) first = vertices;
                    var firstCentre = Centre(first); var centre = Centre(vertices);
                    for (var i = 0; i < vertices.Length; i++)
                    {
                        maxPoseChange = Mathf.Max(maxPoseChange, Vector3.Distance(vertices[i], first[i]));
                        maxCentredChange = Mathf.Max(maxCentredChange, Vector3.Distance(vertices[i] - centre, first[i] - firstCentre));
                    }
                    last = vertices;
                    for (var i = 0; i < protectedTransforms.Length; i++)
                    {
                        var t = protectedTransforms[i]; var p = protectedBaseline[i];
                        maxPosition[i] = Mathf.Max(maxPosition[i], Vector3.Distance(t.localPosition, p.Position));
                        maxRotation[i] = Mathf.Max(maxRotation[i], Quaternion.Angle(t.localRotation, p.Rotation));
                        maxScale[i] = Mathf.Max(maxScale[i], Vector3.Distance(t.localScale, p.Scale));
                    }
                }
                var endpoint = first.Select((v, i) => Vector3.Distance(v, last[i])).DefaultIfEmpty(0).Max();
                report.AppendLine($"sample={clip.name}; maxVertexDelta={F(maxPoseChange)}; maxCentredVertexDelta={F(maxCentredChange)}; endpointDelta={F(endpoint)}; poseChanged={maxCentredChange > .0001f}");
                for (var i = 0; i < protectedTransforms.Length; i++)
                    report.AppendLine($"  drift={protectedTransforms[i].name}; position={F(maxPosition[i])}; rotationDegrees={F(maxRotation[i])}; scale={F(maxScale[i])}");
                var inPlace = maxPosition.All(v => v <= .0001f) && maxRotation.All(v => v <= .01f) && maxScale.All(v => v <= .0001f);
                report.AppendLine($"  sampledInPlace={inPlace}; sampledPoseChangeInPlace={inPlace && maxCentredChange > .0001f}");
                report.AppendLine("  judgement=FINITE SAMPLES ONLY; root curves, visual deformation, foot contact and death hold require manual review; no runtime approval");
                Require(maxPosition.Take(4).All(v => v <= .000001f) && maxRotation.Take(4).All(v => v <= .0001f) && maxScale.Take(4).All(v => v <= .000001f),
                    "Clip sampling changed a protected probe wrapper.");
            }
            finally
            {
                for (var i = 0; i < transforms.Length; i++) if (transforms[i]) baseline[i].Restore(transforms[i]);
                Object.DestroyImmediate(baked);
            }
        }

        static Transform Child(Transform parent, string name)
        { var child = new GameObject(name).transform; child.SetParent(parent, false); return child; }
        static Vector3 Centre(Vector3[] vertices)
        { var sum = Vector3.zero; foreach (var v in vertices) sum += v; return vertices.Length == 0 ? sum : sum / vertices.Length; }
        static string TakeName(string name) => name.Substring(name.LastIndexOf('|') + 1);
        static string F(float value) => value.ToString("R", CultureInfo.InvariantCulture);
        static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        public static string Hash(string path)
        { using var input = File.OpenRead(path); using var sha = SHA256.Create(); return BitConverter.ToString(sha.ComputeHash(input)).Replace("-", "").ToLowerInvariant(); }
        static void RequireOwnedOrAbsent(string path)
        { if (File.Exists(path)) Require(AssetImporter.GetAtPath(path)?.userData == Owner, "Refusing unowned output: " + path); }
        static void MarkOwned(string path)
        { var importer = AssetImporter.GetAtPath(path); Require(importer, "Output importer absent: " + path); importer.userData = Owner; importer.SaveAndReimport(); }
        static void Require(bool condition, string message)
        { if (!condition) throw new InvalidOperationException("[Tree01 Generic Probe] " + message); }

        public static bool IsProbePath(string path) => path != null &&
            (path.Replace('\\', '/').Equals(Root, StringComparison.OrdinalIgnoreCase) || path.Replace('\\', '/').StartsWith(Root + "/", StringComparison.OrdinalIgnoreCase));

        public static void AssertNoProbeDependencies(string[] dependencies)
        { foreach (var path in dependencies) Require(!IsProbePath(path), "Runtime dependency enters Editor-only probe: " + path); }

        public static void AssertRuntimeIsolation()
        {
            var roots = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path)
                .Concat(AssetDatabase.GetAllAssetPaths().Where(p => p.StartsWith("Assets/", StringComparison.Ordinal) &&
                    p.Contains("/Resources/") && !p.Contains("/Editor/") && !AssetDatabase.IsValidFolder(p)))
                .Distinct().OrderBy(p => p, StringComparer.Ordinal).ToArray();
            foreach (var root in roots) AssertNoProbeDependencies(AssetDatabase.GetDependencies(root, true));
        }

        readonly struct LocalPose
        {
            public readonly Vector3 Position, Scale;
            public readonly Quaternion Rotation;
            public LocalPose(Transform target) { Position = target.localPosition; Rotation = target.localRotation; Scale = target.localScale; }
            public void Restore(Transform target) { target.localPosition = Position; target.localRotation = Rotation; target.localScale = Scale; }
        }
    }

    /// <summary>Fail closed if a later edit accidentally references probe content from a player build.</summary>
    public sealed class Tree01ProbeBuildGuard : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;
        public void OnPreprocessBuild(BuildReport report) => GuardianEntTree01MotionProbe.AssertRuntimeIsolation();
    }
}
