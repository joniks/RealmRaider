using System.Linq;
using NUnit.Framework;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Core;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RealmRaiders.Tests
{
    public sealed class LargeCreatureMotionTests
    {
        const string Folder = "Assets/Game/Art/ThirdParty/Tennessippi/FreeTreantPack/Tree01/MotionPilot";
        static LargeCreatureMotionBinding Binding()
        {
            const string path = "Assets/Game/Resources/Characters/GuardianEntTree01Motion.asset";
            var mainAsset = AssetDatabase.LoadMainAssetAtPath(path);
            Assert.That(mainAsset, Is.Not.Null, "QA must first run Build Guardian Ent Tree01 Motion Pilot.");
            Assert.That(mainAsset.name, Is.EqualTo(System.IO.Path.GetFileNameWithoutExtension(path)),
                "The binding main-object name must survive CreateAsset and CopySerialized and match its filename.");
            var binding = Resources.Load<LargeCreatureMotionBinding>("Characters/GuardianEntTree01Motion");
            Assert.That(binding, Is.Not.Null, "QA must first run Build Guardian Ent Tree01 Motion Pilot.");
            Assert.That(binding, Is.SameAs(mainAsset));
            Assert.That(binding.TryValidate(out var issue), Is.True, "Cold Resources binding invalid: " + issue);
            return binding;
        }

        [Test]
        public void LargeCreatureMotion_ExactRuntimeCopyHasOnlySkeletonCurvesAndMobileImport()
        {
            var binding = Binding();
            Assert.That(AssetDatabase.GetAssetPath(binding.VisualPrefab), Is.EqualTo("Assets/Game/Resources/Characters/GuardianEntTree01Animated.prefab"));
            var importer = AssetImporter.GetAtPath(Folder + "/Tree01_GenericRuntime.fbx") as ModelImporter;
            Assert.That(importer, Is.Not.Null);
            Assert.That(importer.animationType, Is.EqualTo(ModelImporterAnimationType.Generic));
            Assert.That(importer.isReadable || importer.addCollider || importer.importCameras || importer.importLights ||
                importer.importConstraints || importer.importAnimatedCustomProperties, Is.False);
            Assert.That(importer.maxBonesPerVertex, Is.EqualTo(4));
            Assert.That(importer.materialImportMode, Is.EqualTo(ModelImporterMaterialImportMode.None));
            Assert.That(importer.meshCompression, Is.EqualTo(ModelImporterMeshCompression.Medium));
            using var sha = System.Security.Cryptography.SHA256.Create();
            using var source = System.IO.File.OpenRead(Folder + "/Tree01_GenericRuntime.fbx");
            Assert.That(System.BitConverter.ToString(sha.ComputeHash(source)).Replace("-", "").ToLowerInvariant(),
                Is.EqualTo("bd90b4f8dd823334cbb226229ef731c1280ae601ab942d16193b96f5f674a02e"));
            foreach (var clip in new[] { binding.Idle, binding.Run, binding.Attack, binding.Death })
            {
                Assert.That(LargeCreatureMotionBinding.SafeClip(clip), Is.True);
                Assert.That(AnimationUtility.GetObjectReferenceCurveBindings(clip), Is.Empty);
                Assert.That(AnimationUtility.GetCurveBindings(clip), Is.Not.Empty);
                foreach (var curve in AnimationUtility.GetCurveBindings(clip))
                {
                    Assert.That(curve.type, Is.EqualTo(typeof(Transform)));
                    Assert.That(curve.path, Does.StartWith("Tree01/"));
                    Assert.That(binding.AnimatedBonePaths, Does.Contain(curve.path));
                }
            }
            Assert.That(binding.Idle.isLooping && binding.Run.isLooping, Is.True);
            Assert.That(binding.Attack.isLooping || binding.Death.isLooping, Is.False);
            var forbiddenDependencies = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(binding), true)
                .Where(p => p.Replace('\\', '/').StartsWith("Assets/Game/Editor/", System.StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p, System.StringComparer.Ordinal).ToArray();
            Assert.That(forbiddenDependencies, Is.Empty,
                "Runtime binding depends on project-owned Editor assets: " + string.Join(", ", forbiddenDependencies));
        }

        [Test]
        public void LargeCreatureMotion_InvalidEvidenceEventsAndForeignComponentsFailClosed()
        {
            var original = Binding(); var copy = Object.Instantiate(original);
            var clip = Object.Instantiate(original.Idle); var prefab = Object.Instantiate(original.VisualPrefab);
            try
            {
                copy.ValidationVersion = 0; Assert.That(copy.IsValid, Is.False);
                copy.ValidationVersion = LargeCreatureMotionBinding.CurrentValidationVersion;
                copy.Idle = null; Assert.That(copy.IsValid, Is.False);
                copy.Idle = clip;
                AnimationUtility.SetAnimationEvents(clip, new[] { new AnimationEvent { functionName = "Forbidden" } });
                Assert.That(AnimationUtility.GetAnimationEvents(clip), Has.Length.EqualTo(1));
                Assert.That(LargeCreatureMotionBinding.SafeClip(clip), Is.False);
                Assert.That(copy.IsValid, Is.False);
                AnimationUtility.SetAnimationEvents(clip, System.Array.Empty<AnimationEvent>());
                Assert.That(LargeCreatureMotionBinding.SafeClip(clip), Is.True);
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.x"), AnimationCurve.Linear(0, 0, 1, 1));
                Assert.That(copy.IsValid, Is.False, "Generic root-transform curves must also be rejected, not only Animator root-motion curves.");
                copy.Idle = original.Idle; copy.VisualPrefab = prefab;
                Assert.That(copy.IsValid, Is.True);
                var animator = prefab.GetComponentInChildren<Animator>(); animator.applyRootMotion = true;
                Assert.That(copy.IsValid, Is.False); animator.applyRootMotion = false;
                animator.fireEvents = true;
                Assert.That(copy.IsValid, Is.True, "Non-persistent fireEvents must be enforced by the runtime adapter, not required in prefab data.");
                prefab.AddComponent<BoxCollider>(); Assert.That(copy.IsValid, Is.False);
                copy.VisualPrefab = original.VisualPrefab; copy.AnimatedBonePaths = new[] { "Tree01" };
                Assert.That(copy.IsValid, Is.False);
            }
            finally { Object.DestroyImmediate(copy); Object.DestroyImmediate(clip); Object.DestroyImmediate(prefab); }
        }

        [TestCase(true, true, true, true, LargeCreatureVisualState.Death)]
        [TestCase(false, true, true, true, LargeCreatureVisualState.Hit)]
        [TestCase(false, false, true, true, LargeCreatureVisualState.Attack)]
        [TestCase(false, false, false, true, LargeCreatureVisualState.Move)]
        [TestCase(false, false, false, false, LargeCreatureVisualState.Idle)]
        public void LargeCreatureMotion_UsesSharedFactualPriority(bool dead, bool hit, bool attack, bool moving, LargeCreatureVisualState expected)
            => Assert.That(LargeCreatureMotionAdapter.ResolveState(dead, hit, attack, moving), Is.EqualTo(expected));

        [Test]
        public void LargeCreatureMotion_FallbackOrderAndClearLeaveExactlyOneVisual()
        {
            var binding = Binding();
            var recipe = Object.Instantiate(PrototypeRuntimeFactory.GuardianEntRecipe);
            var invalid = Object.Instantiate(binding); invalid.ValidationVersion = 0;
            var host = GameObject.CreatePrimitive(PrimitiveType.Cube); host.GetComponent<Collider>().enabled = false;
            var entity = host.AddComponent<CombatEntity>();
            var definition = ScriptableObject.CreateInstance<CharacterDefinition>(); definition.Stats = CombatStats.Ent;
            try
            {
                recipe.LargeCreatureMotion = binding; definition.VisualRecipe = recipe; entity.Initialize(definition);
                var assembler = host.GetComponent<CharacterVisualAssembler>();
                var adapter = host.GetComponent<LargeCreatureMotionAdapter>();
                Assert.That(adapter.IsBound, Is.True);
                Assert.That(assembler.VisualRoot.GetComponentInChildren<Animator>().fireEvents, Is.False,
                    "Binding the saved prefab must disable runtime Animation Events before playback.");
                Assert.That(assembler.VisualRoot.GetComponentsInChildren<SkinnedMeshRenderer>(), Has.Length.EqualTo(1));
                var rootPose = host.transform.position; var pivotPose = assembler.PresentationPivot.localPosition;
                adapter.Sample(10, 10, .1f);
                Assert.That(host.transform.position, Is.EqualTo(rootPose));
                Assert.That(assembler.PresentationPivot.localPosition, Is.EqualTo(pivotPose));
                var oldBones = assembler.VisualRoot.GetComponentsInChildren<SkinnedMeshRenderer>()[0].bones;
                adapter.Clear(); // EditMode tests the owned API; actual OnDisable lifecycle is covered in PlayMode.
                Assert.That(adapter.HasGraph, Is.False);
                Assert.That(adapter.IsBound, Is.False);
                foreach (var path in binding.AnimatedBonePaths)
                {
                    var expectedBone = binding.VisualPrefab.transform.Find(binding.AnimatorPath + "/" + path);
                    var bone = assembler.PresentationPivot.Find("Base Body/" + binding.AnimatorPath + "/" + path);
                    Assert.That(bone.localPosition, Is.EqualTo(expectedBone.localPosition));
                    Assert.That(Quaternion.Angle(bone.localRotation, expectedBone.localRotation), Is.LessThan(.001f));
                    Assert.That(bone.localScale, Is.EqualTo(expectedBone.localScale));
                }
                assembler.Assemble(recipe);
                Assert.That(adapter.IsBound, Is.True);
                Assert.That(assembler.VisualRoot.GetComponentInChildren<Animator>().fireEvents, Is.False,
                    "A rebuilt visual must also receive the non-persistent runtime safety flag.");
                recipe.LargeCreatureMotion = invalid; assembler.Assemble(recipe);
                Assert.That(adapter.IsBound || adapter.HasGraph, Is.False);
                Assert.That(assembler.VisualRoot.GetComponentsInChildren<SkinnedMeshRenderer>(), Is.Empty);
                Assert.That(assembler.VisualRoot.GetComponentsInChildren<MeshRenderer>(), Has.Length.EqualTo(1));
                Assert.That(oldBones.All(b => !b), Is.True);
                recipe.BaseBodyPrefab = null; assembler.Assemble(recipe);
                Assert.That(assembler.PresentationPivot.childCount, Is.EqualTo(6));
                Assert.That(assembler.PresentationPivot.Find("Base Body").localScale, Is.EqualTo(new Vector3(1.45f, 1.25f, .85f)));
                Assert.That(host.GetComponent<Renderer>().enabled, Is.False);
                assembler.Clear();
                Assert.That(adapter.IsBound || adapter.HasGraph, Is.False);
                Assert.That(host.transform.Find("Character Visual Modules"), Is.Null);
                Assert.That(host.GetComponent<Renderer>().enabled, Is.True);
            }
            finally { Object.DestroyImmediate(host); Object.DestroyImmediate(definition); Object.DestroyImmediate(recipe); Object.DestroyImmediate(invalid); }
        }
    }
}
