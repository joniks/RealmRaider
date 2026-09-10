using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using RealmRaiders.Core;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RealmRaiders.Editor.Tree01Probe.Tests
{
    public sealed class GuardianEntTree01MotionProbeTests
    {
        [Test]
        public void ProbeAssemblyAndDependenciesCannotEnterPlayerBuild()
        {
            Assert.That(CompilationPipeline.GetAssemblies(AssembliesType.Player).Any(a => a.name.Contains("Tree01MotionProbe")), Is.False);
            Assert.That(GuardianEntTree01MotionProbe.Generated, Does.Contain("/Editor/").And.Not.Contain("/Resources/"));
            Assert.DoesNotThrow(GuardianEntTree01MotionProbe.AssertRuntimeIsolation);
            var recipes = new[] { PrototypeRuntimeFactory.GuardianEntRecipe, PrototypeRuntimeFactory.BloodKnightRecipe,
                PrototypeRuntimeFactory.InfernalBruteRecipe, PrototypeRuntimeFactory.SylvanBeastRecipe, PrototypeRuntimeFactory.InfernalBeastRecipe };
            foreach (var recipe in recipes)
                if (recipe.BaseBodyPrefab)
                    Assert.DoesNotThrow(() => GuardianEntTree01MotionProbe.AssertNoProbeDependencies(
                        AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(recipe.BaseBodyPrefab), true)));
            Assert.That(AssetDatabase.GetAssetPath(PrototypeRuntimeFactory.GuardianEntRecipe.BaseBodyPrefab),
                Is.EqualTo("Assets/Game/Resources/Characters/GuardianEntTree01.prefab"));
        }

        [Test]
        public void DependencyGuardRejectsOnlyTheExactProbeSubtree()
        {
            Assert.Throws<InvalidOperationException>(() => GuardianEntTree01MotionProbe.AssertNoProbeDependencies(new[] { GuardianEntTree01MotionProbe.PrefabPath }));
            Assert.Throws<InvalidOperationException>(() => GuardianEntTree01MotionProbe.AssertNoProbeDependencies(new[] { GuardianEntTree01MotionProbe.ModelPath.Replace('/', '\\') }));
            Assert.DoesNotThrow(() => GuardianEntTree01MotionProbe.AssertNoProbeDependencies(new[] {
                "Assets/Game/Resources/Characters/GuardianEntTree01.prefab", GuardianEntTree01MotionProbe.Root + "Unrelated/model.fbx" }));
        }

        [TestCase(typeof(BoxCollider))]
        [TestCase(typeof(Rigidbody))]
        [TestCase(typeof(Camera))]
        [TestCase(typeof(Light))]
        [TestCase(typeof(Animation))]
        [TestCase(typeof(MeshFilter))]
        public void ProbeRejectsForbiddenComponents(Type forbidden)
        {
            var root = new GameObject("Probe validation fixture", typeof(SkinnedMeshRenderer), typeof(Animator));
            root.SetActive(false);
            try
            {
                Assert.DoesNotThrow(() => GuardianEntTree01MotionProbe.ValidateComponents(root));
                root.AddComponent(forbidden);
                Assert.Throws<InvalidOperationException>(() => GuardianEntTree01MotionProbe.ValidateComponents(root));
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void ProbeRejectsRootMotionAndDuplicateRenderers()
        {
            var root = new GameObject("Probe validation fixture", typeof(SkinnedMeshRenderer), typeof(Animator));
            try
            {
                var animator = root.GetComponent<Animator>();
                animator.applyRootMotion = true;
                Assert.Throws<InvalidOperationException>(() => GuardianEntTree01MotionProbe.ValidateComponents(root));
                animator.applyRootMotion = false;
                var duplicate = new GameObject("Duplicate", typeof(SkinnedMeshRenderer)); duplicate.transform.SetParent(root.transform);
                Assert.Throws<InvalidOperationException>(() => GuardianEntTree01MotionProbe.ValidateComponents(root));
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void BindingRisksExposeRootAndProtectedPresentationCurves()
        {
            Assert.That(GuardianEntTree01MotionProbe.BindingRisk("", "m_LocalPosition.x"), Does.StartWith("ANIMATION_ROOT"));
            Assert.That(GuardianEntTree01MotionProbe.BindingRisk("Presentation Pivot/Base Body", "m_LocalScale.x"), Is.EqualTo("PROTECTED_PRESENTATION_TRANSFORM"));
            Assert.That(GuardianEntTree01MotionProbe.BindingRisk("Tree01 Fit", "m_LocalRotation.w"), Is.EqualTo("PROTECTED_PRESENTATION_TRANSFORM"));
            Assert.That(GuardianEntTree01MotionProbe.BindingRisk("Tree01", "m_LocalScale.x"), Does.StartWith("SOURCE_ROOT"));
            Assert.That(GuardianEntTree01MotionProbe.BindingRisk("Tree01/spine", "m_LocalRotation.x"), Does.StartWith("DESCENDANT"));
        }

        [Test]
        public void BuiltProbeIsExactGenericVisualOnlyAndHasEvidenceForAllFourTakes()
        {
            Assert.That(File.Exists(GuardianEntTree01MotionProbe.EvidencePath), Is.True, "QA must run the explicit probe builder first; an import blocker is not a pass.");
            Assert.That(GuardianEntTree01MotionProbe.Hash(GuardianEntTree01MotionProbe.ModelPath), Is.EqualTo(GuardianEntTree01MotionProbe.SourceHash));
            Assert.DoesNotThrow(GuardianEntTree01MotionProbe.ValidateImporter);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GuardianEntTree01MotionProbe.PrefabPath);
            Assert.That(prefab, Is.Not.Null);
            Assert.DoesNotThrow(() => GuardianEntTree01MotionProbe.ValidateComponents(prefab));
            var renderer = prefab.GetComponentInChildren<SkinnedMeshRenderer>(true);
            Assert.That(AssetDatabase.GetAssetPath(renderer.sharedMesh), Is.EqualTo(GuardianEntTree01MotionProbe.ModelPath));
            Assert.That(renderer.bones.All(b => b && b.IsChildOf(prefab.transform)), Is.True);
            Assert.That(renderer.sharedMesh.bindposes.Length, Is.EqualTo(renderer.bones.Length));
            Assert.That(renderer.sharedMesh.subMeshCount, Is.EqualTo(1));
            Assert.That(renderer.sharedMesh.GetIndexCount(0), Is.EqualTo(5438 * 3));
            Assert.That(renderer.sharedMaterials, Has.Length.EqualTo(1));
            Assert.That(AssetDatabase.GetAssetPath(renderer.sharedMaterial), Does.EndWith("Tree01/materials/GuardianEntTree01Mobile.mat"));
            var animator = prefab.GetComponentInChildren<Animator>(true);
            Assert.That(animator.avatar && animator.avatar.isValid && !animator.avatar.isHuman, Is.True);
            var report = File.ReadAllText(GuardianEntTree01MotionProbe.EvidencePath);
            Assert.That(report, Does.Contain("result=MEASURED").And.Not.Contain("result=BLOCKED"));
            foreach (var take in new[] { "Idle", "Run", "Attack_1", "Death1" })
                Assert.That(report.Split('\n').Any(line => line.StartsWith("sample=", StringComparison.Ordinal) &&
                    line.Substring(0, line.IndexOf(';')).EndsWith(take, StringComparison.Ordinal)), Is.True, "Missing selected sample: " + take);
            Assert.That(report, Does.Contain("weights.maximum=").And.Contain("sampledInPlace=").And.Contain("endpointDelta="));
        }
    }
}
