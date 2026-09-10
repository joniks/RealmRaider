using System.Collections;
using NUnit.Framework;
using RealmRaiders.Characters;
using RealmRaiders.Core;
using UnityEngine;
using UnityEngine.TestTools;

namespace RealmRaiders.Tests
{
    public sealed class CharacterVisualBaseBodyFitPlayModeTests
    {
        [UnityTest]
        public IEnumerator BloodKnightBaseBodyFit_FlipsOnlyTheVisualAndClearsSafely()
        {
            var host = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            host.GetComponent<Collider>().enabled = false;
            host.transform.position = new Vector3(3, 1, -2);
            host.transform.rotation = Quaternion.Euler(0, 31, 0);
            host.transform.localScale = new Vector3(1.2f, 1.4f, 1.1f);
            var controller = host.AddComponent<CharacterController>();
            controller.height = 2.4f;
            controller.radius = .55f;
            var recipe = PrototypeRuntimeFactory.BloodKnightRecipe;
            var source = Object.Instantiate(recipe.BaseBodyPrefab);
            try
            {
                var rootPose = Pose.Of(host.transform);
                var controllerHeight = controller.height;
                var controllerRadius = controller.radius;
                var sourceRotation = source.transform.localRotation;
                var assembler = host.AddComponent<CharacterVisualAssembler>();
                Assert.That(assembler.Assemble(recipe), Is.True);

                var pivot = assembler.PresentationPivot;
                var baseBody = pivot.Find("Base Body");
                Assert.That(baseBody, Is.Not.Null);
                var sourceUpperArm = FindDescendant(source.transform, "Bip01 L UpperArm");
                var fittedUpperArm = FindDescendant(baseBody, "Bip01 L UpperArm");
                Assert.That(Quaternion.Angle(baseBody.localRotation, Quaternion.Euler(recipe.BaseBodyLocalEulerAngles) * sourceRotation), Is.LessThan(.001f));
                Assert.That(Vector3.Dot(PlanarForward(baseBody.localRotation), -PlanarForward(sourceRotation)), Is.GreaterThan(.999f));
                Assert.That(sourceUpperArm, Is.Not.Null);
                Assert.That(fittedUpperArm, Is.Not.Null);
                Assert.That(Quaternion.Angle(fittedUpperArm.localRotation, sourceUpperArm.localRotation), Is.LessThan(.001f), "The fit must not alter procedural bone local rotations.");

                yield return null;

                Assert.That(Pose.Of(host.transform), Is.EqualTo(rootPose));
                Assert.That(controller.height, Is.EqualTo(controllerHeight));
                Assert.That(controller.radius, Is.EqualTo(controllerRadius));
                Assert.That(Quaternion.Angle(pivot.localRotation, Quaternion.identity), Is.LessThan(.001f));

                assembler.Clear();
                yield return null;
                Assert.That(host.transform.Find("Character Visual Modules"), Is.Null);
                Assert.That(Pose.Of(host.transform), Is.EqualTo(rootPose));

                var fallback = ScriptableObject.CreateInstance<CharacterVisualRecipe>();
                fallback.Family = CharacterVisualFamily.Humanoid;
                fallback.Primary = Color.white;
                fallback.Secondary = Color.gray;
                fallback.AccentColor = Color.yellow;
                Assert.That(assembler.Assemble(fallback), Is.True);
                Assert.That(Quaternion.Angle(assembler.PresentationPivot.Find("Base Body").localRotation, Quaternion.identity), Is.LessThan(.001f));
                Object.Destroy(fallback);
            }
            finally
            {
                Object.Destroy(host);
                Object.Destroy(source);
            }
        }

        private static Transform FindDescendant(Transform root, string name)
        {
            foreach (var candidate in root.GetComponentsInChildren<Transform>(true))
                if (candidate != root && candidate.name == name) return candidate;
            return null;
        }

        private static Vector3 PlanarForward(Quaternion rotation)
        {
            var forward = rotation * Vector3.forward;
            forward.y = 0;
            return forward.normalized;
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
