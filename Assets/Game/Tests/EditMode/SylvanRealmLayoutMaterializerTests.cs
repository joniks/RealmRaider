using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RealmRaiders.Core;
using UnityEngine;

namespace RealmRaiders.Tests
{
    public sealed class SylvanRealmLayoutMaterializerTests
    {
        const string RealmId = "102132435465768798a9bacbdcedfe0f";
        bool hadIdentity;
        string previousIdentity;

        [SetUp]
        public void SetUp()
        {
            hadIdentity = PlayerPrefs.HasKey(SylvanStarterRealmIdentity.KeyForTests);
            previousIdentity = PlayerPrefs.GetString(SylvanStarterRealmIdentity.KeyForTests, string.Empty);
            SylvanStarterRealmIdentity.ResetForTests();
        }

        [TearDown]
        public void TearDown()
        {
            SylvanStarterRealmIdentity.ResetForTests();
            if (hadIdentity) PlayerPrefs.SetString(SylvanStarterRealmIdentity.KeyForTests, previousIdentity);
            PlayerPrefs.Save();
        }

        [TestCase(0, 7)]
        [TestCase(1, 7)]
        [TestCase(2, 6)]
        public void CachedRecipe_MaterializesEveryExactFactDeterministicallyWithoutMutation(int seed, int edgeCount)
        {
            var identity = Identity(seed);
            var sourceSignature = SourceSignature(identity);
            var recipe = RecipeObject(identity);
            var rawNodes = Items(recipe, "Nodes");
            var rawEdges = Items(recipe, "Edges");
            var first = SylvanRealmLayoutMaterializer.Create(identity);
            var second = SylvanRealmLayoutMaterializer.Create(identity);

            Assert.That(first.LayoutId, Is.EqualTo(identity.LayoutId));
            Assert.That(first.Nodes, Has.Count.EqualTo(7));
            Assert.That(first.Edges, Has.Count.EqualTo(edgeCount));
            Assert.That(first.Nodes.Select(node => node.GraphId), Is.EquivalentTo(new[]
                { "Portal", "Crossroads", "Wolf Grove", "Root Path", "Ent Grove", "Moonwell", "Heart Tree" }));
            Assert.That(first.Nodes.Select(node => node.SourceNodeId), Is.EqualTo(second.Nodes.Select(node => node.SourceNodeId)));
            Assert.That(first.Edges.Select(edge => edge.SourceEdgeId), Is.EqualTo(second.Edges.Select(edge => edge.SourceEdgeId)));

            var circles = first.CreateNodeFootprints();
            Assert.That(circles, Has.Length.EqualTo(7));
            for (var i = 0; i < circles.Length; i++)
            {
                Assert.That(circles[i].Center, Is.EqualTo(first.Nodes[i].Center));
                Assert.That(circles[i].Radius, Is.EqualTo(SylvanRealmLayoutMaterializer.NodeRadius));
                var raw = rawNodes[i];
                var roleId = Value(raw, "MaterializationRole").ToString();
                var semantic = ExpectedSemantic(roleId);
                Assert.That(first.Nodes[i].SourceNodeId, Is.EqualTo(Value<string>(raw, "NodeId")));
                Assert.That(first.Nodes[i].RoleId, Is.EqualTo(roleId));
                Assert.That(first.Nodes[i].GraphId, Is.EqualTo(semantic.graphId));
                Assert.That(first.Nodes[i].Label, Is.EqualTo(semantic.label));
                Assert.That(first.Nodes[i].Center, Is.EqualTo(new Vector2(Value<float>(raw, "X"), Value<float>(raw, "Z"))));
            }
            for (var i = 0; i < first.Edges.Count; i++)
            {
                var edge = first.Edges[i];
                var raw = rawEdges[i];
                var fromId = Value<string>(raw, "FromNodeId");
                var toId = Value<string>(raw, "ToNodeId");
                var rawFrom = rawNodes.Single(node => Value<string>(node, "NodeId") == fromId);
                var rawTo = rawNodes.Single(node => Value<string>(node, "NodeId") == toId);
                var rawFromCenter = new Vector2(Value<float>(rawFrom, "X"), Value<float>(rawFrom, "Z"));
                var rawToCenter = new Vector2(Value<float>(rawTo, "X"), Value<float>(rawTo, "Z"));
                var delta = rawToCenter - rawFromCenter;
                Assert.That(edge.SourceEdgeId, Is.EqualTo(Value<string>(raw, "EdgeId")));
                Assert.That(edge.From.SourceNodeId, Is.EqualTo(fromId));
                Assert.That(edge.To.SourceNodeId, Is.EqualTo(toId));
                Assert.That(edge.IsActivePathSafe, Is.EqualTo(Value<bool>(raw, "IsActivePathSafe")));
                Assert.That(edge.FloorPathWidth, Is.EqualTo(Value<float>(raw, "FloorPathWidth")));
                Assert.That(edge.Footprint.Center, Is.EqualTo((rawFromCenter + rawToCenter) * .5f));
                Assert.That(edge.Footprint.Size.x, Is.EqualTo(Value<float>(raw, "FloorPathWidth")));
                Assert.That(edge.Footprint.Size.y, Is.EqualTo(delta.magnitude).Within(.0001f));
                Assert.That(Mathf.DeltaAngle(edge.Footprint.Yaw, Mathf.Atan2(delta.x, delta.y) * Mathf.Rad2Deg), Is.Zero.Within(.0001f));
                Assert.That(second.Edges[i].Footprint.Center, Is.EqualTo(edge.Footprint.Center));
                Assert.That(second.Edges[i].Footprint.Size, Is.EqualTo(edge.Footprint.Size));
                Assert.That(second.Edges[i].Footprint.Yaw, Is.EqualTo(edge.Footprint.Yaw));
            }

            var graph = first.BuildGraph();
            Assert.That(graph.Nodes.Values.Sum(node => node.Neighbors.Count) / 2, Is.EqualTo(edgeCount));
            Assert.That(SourceSignature(identity), Is.EqualTo(sourceSignature), "Materialization must not mutate module facts.");
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void CachedRecipe_BuildsOneBoundedConnectedSylvanContour(int seed)
        {
            var plan = SylvanRealmLayoutMaterializer.Create(Identity(seed));
            var circles = plan.CreateNodeFootprints();
            var paths = plan.CreatePathFootprints();
            var owner = new GameObject("Layout Boundary Test");
            try
            {
                Assert.That(PrototypeArenaBoundaryBuilder.CountFootprintComponents(circles, paths), Is.EqualTo(1));
                var boundary = PrototypeArenaBoundaryBuilder.BuildSylvan(owner.transform, circles, paths);
                Assert.That(boundary, Is.Not.Null);
                Assert.That(boundary.GetComponentsInChildren<MeshRenderer>(true), Has.Length.EqualTo(1));
                var colliders = boundary.GetComponentsInChildren<Collider>(true);
                Assert.That(colliders.Length, Is.InRange(1, PrototypeArenaBoundaryBuilder.SylvanColliderBudget));
                Assert.That(colliders.All(collider => !collider.isTrigger && collider.gameObject.isStatic), Is.True);
            }
            finally { Object.DestroyImmediate(owner); }
        }

        SylvanStarterRealmIdentityResult Identity(int seed)
        {
            SylvanStarterRealmIdentity.ResetForTests();
            SylvanStarterRealmIdentity.SetFactoriesForTests(() => RealmId, () => seed);
            return SylvanStarterRealmIdentity.LoadOrCreate();
        }

        static string SourceSignature(SylvanStarterRealmIdentityResult identity)
        {
            var recipe = RecipeObject(identity);
            return string.Join("|", Items(recipe, "Nodes").Select(node =>
                    $"N:{Value<string>(node, "NodeId")}:{Value(node, "MaterializationRole")}:{Value<float>(node, "X"):R}:{Value<float>(node, "Z"):R}")
                .Concat(Items(recipe, "Edges").Select(edge =>
                    $"E:{Value<string>(edge, "EdgeId")}:{Value<string>(edge, "FromNodeId")}:{Value<string>(edge, "ToNodeId")}:{Value<bool>(edge, "IsActivePathSafe")}:{Value<float>(edge, "FloorPathWidth"):R}")));
        }

        static (string graphId, string label) ExpectedSemantic(string roleId) => roleId switch
        {
            "PortalStart" => ("Portal", "PORTAL"),
            "LandmarkJunction" => ("Crossroads", "CROSSROADS"),
            "WolfGroveEncounter" => ("Wolf Grove", "WOLF GROVE"),
            "RootPathHazard" => ("Root Path", "ROOT PATH"),
            "EntGroveEncounter" => ("Ent Grove", "ENT GROVE"),
            "MoonwellRecovery" => ("Moonwell", "MOONWELL"),
            "HeartTreeObjective" => ("Heart Tree", "HEART TREE"),
            _ => throw new System.ArgumentOutOfRangeException(nameof(roleId))
        };

        static object RecipeObject(SylvanStarterRealmIdentityResult identity) =>
            typeof(SylvanStarterRealmIdentityResult).GetProperty("Recipe", BindingFlags.Instance | BindingFlags.Public).GetValue(identity);

        static object[] Items(object owner, string property) =>
            ((IEnumerable)owner.GetType().GetProperty(property, BindingFlags.Instance | BindingFlags.Public).GetValue(owner)).Cast<object>().ToArray();

        static object Value(object owner, string property) =>
            owner.GetType().GetProperty(property, BindingFlags.Instance | BindingFlags.Public).GetValue(owner);

        static T Value<T>(object owner, string property) => (T)Value(owner, property);
    }
}
