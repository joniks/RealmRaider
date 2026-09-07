using System.Linq;
using NUnit.Framework;
using RealmRaiders.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RealmRaiders.Tests
{
    public sealed class RealmLandmarkPresentationTests
    {
        [TestCase(RealmLandmarkRecipe.SylvanHeartTree, 8)]
        [TestCase(RealmLandmarkRecipe.SylvanRootTrap, 6)]
        [TestCase(RealmLandmarkRecipe.InfernalHeart, 6)]
        [TestCase(RealmLandmarkRecipe.InfernalFlameTrap, 6)]
        public void RecipeIsIdempotentBoundedAndDoesNotMutateAuthoritativeRoot(RealmLandmarkRecipe recipe, int expectedRenderers)
        {
            var root = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            root.transform.SetPositionAndRotation(new Vector3(3, 4, 5), Quaternion.Euler(0, 27, 0));
            root.transform.localScale = new Vector3(2.7f, .1f, 2.7f);
            var position = root.transform.position;
            var rotation = root.transform.rotation;
            var scale = root.transform.localScale;
            var active = root.activeSelf;
            var authoritativeCollider = root.GetComponent<Collider>();
            authoritativeCollider.isTrigger = true;
            try
            {
                var presentation = RealmLandmarkPresentation.Build(root.transform, recipe);
                var repeated = RealmLandmarkPresentation.Build(root.transform, recipe);

                Assert.That(repeated, Is.SameAs(presentation));
                Assert.That(root.transform.Cast<Transform>().Count(child => child.name == RealmLandmarkPresentation.RootName), Is.EqualTo(1));
                Assert.That(presentation.GetComponentsInChildren<Renderer>(true), Has.Length.EqualTo(expectedRenderers));
                Assert.That(expectedRenderers, Is.LessThanOrEqualTo(RealmLandmarkPresentation.RendererCeiling(recipe)));
                AssertPresentationOnly(presentation);
                Assert.That(root.transform.position, Is.EqualTo(position));
                Assert.That(root.transform.rotation, Is.EqualTo(rotation));
                Assert.That(root.transform.localScale, Is.EqualTo(scale));
                Assert.That(root.activeSelf, Is.EqualTo(active));
                Assert.That(authoritativeCollider.enabled, Is.True);
                Assert.That(authoritativeCollider.isTrigger, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void SylvanRecipeRepeatsDeterministicallyAndFactionStructuresStayDistinct()
        {
            var first = new GameObject("First Sylvan");
            var second = new GameObject("Second Sylvan");
            var infernal = new GameObject("Infernal");
            var rootTrap = new GameObject("Root Trap");
            var flameTrap = new GameObject("Flame Trap");
            try
            {
                var firstTree = RealmLandmarkPresentation.Build(first.transform, RealmLandmarkRecipe.SylvanHeartTree);
                var secondTree = RealmLandmarkPresentation.Build(second.transform, RealmLandmarkRecipe.SylvanHeartTree);
                var infernalHeart = RealmLandmarkPresentation.Build(infernal.transform, RealmLandmarkRecipe.InfernalHeart);
                var roots = RealmLandmarkPresentation.Build(rootTrap.transform, RealmLandmarkRecipe.SylvanRootTrap);
                var chevrons = RealmLandmarkPresentation.Build(flameTrap.transform, RealmLandmarkRecipe.InfernalFlameTrap);

                Assert.That(Signature(firstTree), Is.EqualTo(Signature(secondTree)));
                var firstRenderers = firstTree.GetComponentsInChildren<Renderer>(true);
                var secondRenderers = secondTree.GetComponentsInChildren<Renderer>(true);
                for (var index = 0; index < firstRenderers.Length; index++) Assert.That(firstRenderers[index].sharedMaterial, Is.SameAs(secondRenderers[index].sharedMaterial));
                Assert.That(Signature(firstTree), Is.Not.EqualTo(Signature(infernalHeart)));
                Assert.That(Signature(roots), Is.Not.EqualTo(Signature(chevrons)));
                Assert.That(firstTree.Find("Wide Crown"), Is.Not.Null);
                Assert.That(firstTree.Find("Radial Root Left"), Is.Not.Null);
                Assert.That(infernalHeart.Find("Heavy Core"), Is.Not.Null);
                Assert.That(infernalHeart.Find("Claw Left"), Is.Not.Null);
                Assert.That(roots.Find("Inward Root 1"), Is.Not.Null);
                Assert.That(chevrons.Find("Chevron 1 Left"), Is.Not.Null);
            }
            finally
            {
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
                Object.DestroyImmediate(infernal);
                Object.DestroyImmediate(rootTrap);
                Object.DestroyImmediate(flameTrap);
            }
        }

        static string Signature(Transform root)
        {
            return string.Join("|", root.Cast<Transform>().Select(child => $"{child.name}:{child.localPosition}:{child.localRotation.eulerAngles}:{child.localScale}"));
        }

        static void AssertPresentationOnly(Transform root)
        {
            Assert.That(root.GetComponentsInChildren<Collider>(true), Is.Empty);
            Assert.That(root.GetComponentsInChildren<Rigidbody>(true), Is.Empty);
            Assert.That(root.GetComponentsInChildren<CharacterController>(true), Is.Empty);
            Assert.That(root.GetComponentsInChildren<Camera>(true), Is.Empty);
            Assert.That(root.GetComponentsInChildren<Light>(true), Is.Empty);
            Assert.That(root.GetComponentsInChildren<AudioListener>(true), Is.Empty);
            Assert.That(root.GetComponentsInChildren<Canvas>(true), Is.Empty);
            Assert.That(root.GetComponentsInChildren<EventSystem>(true), Is.Empty);
            Assert.That(root.GetComponentsInChildren<MonoBehaviour>(true), Is.Empty);
        }
    }
}
