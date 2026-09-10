using NUnit.Framework;
using RealmRaiders.Characters;
using RealmRaiders.Core;
using UnityEngine;

namespace RealmRaiders.Tests
{
    public sealed class CharacterVisualBaseBodyFitTests
    {
        [Test]
        public void BaseBodyFit_UsesRecipeRotationOnlyAndKeepsNeutralRecipesUnchanged()
        {
            var source = new GameObject("Authored Visual Source");
            source.transform.localRotation = Quaternion.Euler(7, 23, -4);
            source.AddComponent<BoxCollider>();
            var host = new GameObject("Visual Fit Host");
            var recipe = ScriptableObject.CreateInstance<CharacterVisualRecipe>();
            recipe.Family = CharacterVisualFamily.Humanoid;
            recipe.Primary = Color.red;
            recipe.Secondary = Color.black;
            recipe.AccentColor = Color.yellow;
            recipe.BaseBodyPrefab = source;
            recipe.BaseBodyLocalEulerAngles = new Vector3(0, 180, 0);
            try
            {
                var rootPose = Pose.Of(host.transform);
                var sourcePose = Pose.Of(source.transform);
                var assembler = host.AddComponent<CharacterVisualAssembler>();
                Assert.That(assembler.Assemble(recipe), Is.True);
                var baseBody = assembler.PresentationPivot.Find("Base Body");
                Assert.That(baseBody, Is.Not.Null);
                Assert.That(Quaternion.Angle(baseBody.localRotation, Quaternion.Euler(recipe.BaseBodyLocalEulerAngles) * source.transform.localRotation), Is.LessThan(.001f));
                Assert.That(Pose.Of(host.transform), Is.EqualTo(rootPose));
                foreach (var collider in baseBody.GetComponentsInChildren<Collider>(true)) Assert.That(collider.enabled, Is.False);

                assembler.Clear();
                Assert.That(host.transform.Find("Character Visual Modules"), Is.Null);
                Assert.That(Pose.Of(source.transform), Is.EqualTo(sourcePose));

                var neutral = ScriptableObject.CreateInstance<CharacterVisualRecipe>();
                neutral.Family = CharacterVisualFamily.Humanoid;
                neutral.Primary = Color.white;
                neutral.Secondary = Color.gray;
                neutral.AccentColor = Color.yellow;
                Assert.That(assembler.Assemble(neutral), Is.True);
                Assert.That(Quaternion.Angle(assembler.PresentationPivot.Find("Base Body").localRotation, Quaternion.identity), Is.LessThan(.001f));
                Object.DestroyImmediate(neutral);
            }
            finally
            {
                Object.DestroyImmediate(recipe);
                Object.DestroyImmediate(host);
                Object.DestroyImmediate(source);
            }

            Assert.That(PrototypeRuntimeFactory.BloodKnightRecipe.BaseBodyLocalEulerAngles, Is.EqualTo(new Vector3(0, 180, 0)));
            Assert.That(PrototypeRuntimeFactory.GuardianEntRecipe.BaseBodyLocalEulerAngles, Is.EqualTo(Vector3.zero));
            Assert.That(PrototypeRuntimeFactory.InfernalBruteRecipe.BaseBodyLocalEulerAngles, Is.EqualTo(Vector3.zero));
            Assert.That(PrototypeRuntimeFactory.SylvanBeastRecipe.BaseBodyLocalEulerAngles, Is.EqualTo(Vector3.zero));
            Assert.That(PrototypeRuntimeFactory.InfernalBeastRecipe.BaseBodyLocalEulerAngles, Is.EqualTo(Vector3.zero));
        }

        private readonly struct Pose : System.IEquatable<Pose>
        {
            public Pose(Vector3 position, Quaternion rotation, Vector3 scale) { Position = position; Rotation = rotation; Scale = scale; }
            public Vector3 Position { get; }
            public Quaternion Rotation { get; }
            public Vector3 Scale { get; }
            public static Pose Of(Transform target) => new(target.localPosition, target.localRotation, target.localScale);
            public bool Equals(Pose other) => Position == other.Position && Rotation == other.Rotation && Scale == other.Scale;
            public override bool Equals(object value) => value is Pose other && Equals(other);
            public override int GetHashCode() => Position.GetHashCode() ^ Rotation.GetHashCode() ^ Scale.GetHashCode();
        }
    }
}
