using System.Collections;
using NUnit.Framework;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using UnityEngine;
using UnityEngine.TestTools;

namespace RealmRaiders.Tests
{
    public sealed class CharacterProceduralMotionAdapterPlayModeTests
    {
        [UnityTest]
        public IEnumerator BloodKnightAdapter_BindsActualHeroMapsFactsAndCleansUp()
        {
            var host = new GameObject("Procedural Motion Play Host");
            var definition = ScriptableObject.CreateInstance<CharacterDefinition>();
            var recipe = ScriptableObject.CreateInstance<CharacterVisualRecipe>();
            var ability = ScriptableObject.CreateInstance<AbilityDefinition>();
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            host.AddComponent<CharacterController>();
            host.AddComponent<Health>();
            var entity = host.AddComponent<CombatEntity>();
            try
            {
                ground.name = "Procedural Motion Test Ground";
                ground.transform.localScale = Vector3.one * 8f;
                host.transform.position = Vector3.up;
                recipe.Family = CharacterVisualFamily.Humanoid;
                recipe.Primary = Color.red;
                recipe.Secondary = Color.black;
                recipe.AccentColor = Color.yellow;
                recipe.BaseBodyPrefab = Resources.Load<GameObject>("Characters/BloodKnightHero");
                Assert.That(recipe.BaseBodyPrefab, Is.Not.Null);
                ability.Kind = AbilityKind.Melee;
                ability.Windup = 0f;
                definition.Stats = CombatStats.BloodKnight;
                definition.VisualRecipe = recipe;
                definition.Abilities = new[] { ability };
                entity.Initialize(definition);

                var adapter = host.GetComponent<CharacterProceduralMotionAdapter>();
                var pivot = host.transform.Find("Character Visual Modules/Presentation Pivot");
                var baseBody = host.transform.Find("Character Visual Modules/Presentation Pivot/Base Body");
                Assert.That(adapter, Is.Not.Null);
                Assert.That(adapter.IsBound, Is.True, "The active Blood Knight must bind its exact six factual Bip01 descendants.");
                Assert.That(pivot, Is.Not.Null);
                Assert.That(baseBody, Is.Not.Null);
                var pivotPose = Pose.Of(pivot);
                var bodyPose = Pose.Of(baseBody);
                var rootPosition = host.transform.position;
                var controller = host.GetComponent<CharacterController>();
                var controllerHeight = controller.height;
                var controllerRadius = controller.radius;
                var controllerCenter = controller.center;
                var bones = RequiredBones(baseBody);
                host.GetComponent<CharacterVisualMotion>().enabled = false;

                var baseline = Snapshot(bones);
                var locomotionObserved = false;
                for (var sample = 0; sample < 3; sample++)
                {
                    host.transform.position += Vector3.forward * .3f;
                    yield return null;
                    locomotionObserved |= RotationDelta(bones[2], baseline[2]) > .001f;
                }
                Assert.That(locomotionObserved, Is.True, "Factual horizontal root displacement must map to locomotion.");
                Assert.That(Pose.Of(pivot), Is.EqualTo(pivotPose));
                Assert.That(Pose.Of(baseBody), Is.EqualTo(bodyPose));
                Assert.That(host.transform.localRotation, Is.EqualTo(Quaternion.identity));
                Assert.That(host.transform.localScale, Is.EqualTo(Vector3.one));
                Assert.That(controller.height, Is.EqualTo(controllerHeight));
                Assert.That(controller.radius, Is.EqualTo(controllerRadius));
                Assert.That(controller.center, Is.EqualTo(controllerCenter));
                Assert.That(Vector3.Distance(host.transform.position, rootPosition + Vector3.forward * .9f), Is.LessThan(.001f));

                Reset(adapter, baseBody);
                baseline = Snapshot(bones);
                host.GetComponent<Health>().TakeDamage(new DamageInfo(1, null, host.transform.position), 0);
                yield return null;
                Assert.That(RotationDelta(bones[0], baseline[0]), Is.EqualTo(14f).Within(.01f), "Only Health.Damaged may drive the hit pose.");

                Reset(adapter, baseBody);
                baseline = Snapshot(bones);
                yield return new WaitForSecondsRealtime(.13f);
                Assert.That(entity.TryUse(0, Vector3.forward), Is.True);
                yield return null;
                Assert.That(entity.ActionPhase, Is.Not.EqualTo(CombatActionPhase.Idle));
                Assert.That(RotationDelta(bones[1], baseline[1]), Is.EqualTo(30f).Within(.01f), "A non-idle action phase must map to generic primary attack after the hit window expires.");

                yield return new WaitForSecondsRealtime(.15f);
                Assert.That(entity.ActionPhase, Is.EqualTo(CombatActionPhase.Idle));
                Reset(adapter, baseBody);
                baseline = Snapshot(bones);

                adapter.enabled = false;
                yield return null;
                Assert.That(adapter.IsBound, Is.False);
                Assert.That(Snapshot(bones), Is.EqualTo(baseline), "The actual disable lifecycle must restore cached bone transforms.");
                adapter.enabled = true;
                yield return null;
                Assert.That(adapter.IsBound, Is.True, "The actual enable lifecycle must rebind the retained Base Body.");

                Reset(adapter, baseBody);
                baseline = Snapshot(bones);
                var player = host.AddComponent<PlayerController>();
                entity.RefreshControllers();
                entity.SetController(player);
                for (var sample = 0; sample < 12 && !entity.IsGrounded; sample++)
                {
                    entity.Move(Vector3.zero);
                    yield return null;
                }
                Assert.That(entity.IsGrounded, Is.True, "The factual jump test needs the existing CharacterController grounding path.");
                Reset(adapter, baseBody);
                baseline = Snapshot(bones);
                Assert.That(entity.TryJump(), Is.True);
                entity.Move(Vector3.zero);
                yield return null;
                Assert.That(entity.IsJumping, Is.True);
                Assert.That(RotationDelta(bones[0], baseline[0]), Is.EqualTo(18f).Within(.01f), "The factual jump start must map to takeoff.");

                yield return new WaitForSecondsRealtime(.12f);
                entity.Move(Vector3.zero);
                yield return null;
                Assert.That(entity.IsJumping, Is.True);
                Assert.That(RotationDelta(bones[0], baseline[0]), Is.EqualTo(22f).Within(.01f), "The ongoing factual jump must map to falling.");

                var landingDeadline = Time.realtimeSinceStartup + 2f;
                while (entity.IsJumping && Time.realtimeSinceStartup < landingDeadline)
                {
                    entity.Move(Vector3.zero);
                    yield return null;
                }
                Assert.That(entity.IsJumping, Is.False, "The existing jump state must settle through CharacterController grounding.");
                Assert.That(entity.IsGrounded, Is.True);
                yield return null;
                Assert.That(RotationDelta(bones[0], baseline[0]), Is.EqualTo(10f).Within(.01f), "The factual grounded transition must map to landing.");

                Reset(adapter, baseBody);
                baseline = Snapshot(bones);
                host.GetComponent<Health>().TakeDamage(new DamageInfo(10000, null, host.transform.position), 0);
                yield return null;
                Assert.That(host.GetComponent<Health>().IsDead, Is.True);
                Assert.That(RotationDelta(bones[0], baseline[0]), Is.EqualTo(16f).Within(.01f), "Health.IsDead must retain death priority.");
                Assert.That(Pose.Of(pivot), Is.EqualTo(pivotPose));
                Assert.That(Pose.Of(baseBody), Is.EqualTo(bodyPose));

                adapter.Clear();
                Assert.That(adapter.IsBound, Is.False);
                Assert.That(Snapshot(bones), Is.EqualTo(baseline));
            }
            finally
            {
                Object.Destroy(host);
                Object.Destroy(ground);
                Object.Destroy(definition);
                Object.Destroy(recipe);
                Object.Destroy(ability);
            }
        }

        private static Transform[] RequiredBones(Transform baseBody)
        {
            var names = new[]
            {
                "Bip01 L UpperArm", "Bip01 R UpperArm", "Bip01 L Thigh",
                "Bip01 R Thigh", "Bip01 L Calf", "Bip01 R Calf"
            };
            var bones = new Transform[names.Length];
            for (var index = 0; index < names.Length; index++)
            {
                bones[index] = FindDescendant(baseBody, names[index]);
                Assert.That(bones[index], Is.Not.Null, names[index]);
            }
            return bones;
        }

        private static Transform FindDescendant(Transform root, string name)
        {
            foreach (var candidate in root.GetComponentsInChildren<Transform>(true))
                if (candidate != root && candidate.name == name) return candidate;
            return null;
        }

        private static Pose[] Snapshot(Transform[] bones)
        {
            var result = new Pose[bones.Length];
            for (var index = 0; index < bones.Length; index++) result[index] = Pose.Of(bones[index]);
            return result;
        }

        private static bool AnyRotationChanged(Transform[] bones, Pose[] baseline)
        {
            for (var index = 0; index < bones.Length; index++)
                if (Quaternion.Angle(bones[index].localRotation, baseline[index].Rotation) > .001f) return true;
            return false;
        }

        private static float RotationDelta(Transform bone, Pose baseline)
        {
            return Quaternion.Angle(bone.localRotation, baseline.Rotation);
        }

        private static void Reset(CharacterProceduralMotionAdapter adapter, Transform baseBody)
        {
            adapter.Clear();
            Assert.That(adapter.Bind(baseBody), Is.True);
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
