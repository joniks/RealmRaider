using System;
using System.Collections.Generic;
using RealmRaiders.Modules.StarterRealmLayouts;
using RealmRaiders.Realm;
using UnityEngine;

namespace RealmRaiders.Core
{
    public sealed class SylvanRealmMaterializationNode
    {
        internal SylvanRealmMaterializationNode(string sourceNodeId, SylvanRealmNodeMaterializationRole role,
            string graphId, string label, Vector2 center)
        {
            SourceNodeId = sourceNodeId;
            Role = role;
            GraphId = graphId;
            Label = label;
            Center = center;
        }

        public string SourceNodeId { get; }
        public string RoleId => Role.ToString();
        internal SylvanRealmNodeMaterializationRole Role { get; }
        public string GraphId { get; }
        public string Label { get; }
        public Vector2 Center { get; }
    }

    public sealed class SylvanRealmMaterializationEdge
    {
        internal SylvanRealmMaterializationEdge(string sourceEdgeId, SylvanRealmMaterializationNode from,
            SylvanRealmMaterializationNode to, bool isActivePathSafe, float floorPathWidth,
            ArenaPathFootprint footprint)
        {
            SourceEdgeId = sourceEdgeId;
            From = from;
            To = to;
            IsActivePathSafe = isActivePathSafe;
            FloorPathWidth = floorPathWidth;
            Footprint = footprint;
        }

        public string SourceEdgeId { get; }
        public SylvanRealmMaterializationNode From { get; }
        public SylvanRealmMaterializationNode To { get; }
        public bool IsActivePathSafe { get; }
        public float FloorPathWidth { get; }
        public ArenaPathFootprint Footprint { get; }
    }

    public sealed class SylvanRealmLayoutPlan
    {
        readonly SylvanRealmMaterializationNode[] nodes;
        readonly SylvanRealmMaterializationEdge[] edges;

        internal SylvanRealmLayoutPlan(string layoutId, SylvanRealmMaterializationNode[] nodes,
            SylvanRealmMaterializationEdge[] edges,
            Dictionary<SylvanRealmNodeMaterializationRole, SylvanRealmMaterializationNode> byRole)
        {
            LayoutId = layoutId;
            this.nodes = (SylvanRealmMaterializationNode[])nodes.Clone();
            this.edges = (SylvanRealmMaterializationEdge[])edges.Clone();
            PortalStart = byRole[SylvanRealmNodeMaterializationRole.PortalStart];
            LandmarkJunction = byRole[SylvanRealmNodeMaterializationRole.LandmarkJunction];
            WolfGroveEncounter = byRole[SylvanRealmNodeMaterializationRole.WolfGroveEncounter];
            RootPathHazard = byRole[SylvanRealmNodeMaterializationRole.RootPathHazard];
            EntGroveEncounter = byRole[SylvanRealmNodeMaterializationRole.EntGroveEncounter];
            MoonwellRecovery = byRole[SylvanRealmNodeMaterializationRole.MoonwellRecovery];
            HeartTreeObjective = byRole[SylvanRealmNodeMaterializationRole.HeartTreeObjective];
        }

        public string LayoutId { get; }
        public IReadOnlyList<SylvanRealmMaterializationNode> Nodes => Array.AsReadOnly(nodes);
        public IReadOnlyList<SylvanRealmMaterializationEdge> Edges => Array.AsReadOnly(edges);
        public SylvanRealmMaterializationNode PortalStart { get; }
        public SylvanRealmMaterializationNode LandmarkJunction { get; }
        public SylvanRealmMaterializationNode WolfGroveEncounter { get; }
        public SylvanRealmMaterializationNode RootPathHazard { get; }
        public SylvanRealmMaterializationNode EntGroveEncounter { get; }
        public SylvanRealmMaterializationNode MoonwellRecovery { get; }
        public SylvanRealmMaterializationNode HeartTreeObjective { get; }

        public ArenaCircleFootprint[] CreateNodeFootprints()
        {
            var result = new ArenaCircleFootprint[nodes.Length];
            for (var i = 0; i < nodes.Length; i++) result[i] = new ArenaCircleFootprint(nodes[i].Center, SylvanRealmLayoutMaterializer.NodeRadius);
            return result;
        }

        public ArenaPathFootprint[] CreatePathFootprints()
        {
            var result = new ArenaPathFootprint[edges.Length];
            for (var i = 0; i < edges.Length; i++) result[i] = edges[i].Footprint;
            return result;
        }

        public RealmGraph BuildGraph()
        {
            var graph = new RealmGraph();
            foreach (var node in nodes) graph.Add(node.GraphId);
            foreach (var edge in edges) graph.Connect(edge.From.GraphId, edge.To.GraphId);
            return graph;
        }
    }

    public static class SylvanRealmLayoutMaterializer
    {
        public const float NodeRadius = 3.25f;

        public static SylvanRealmLayoutPlan Create(SylvanStarterRealmIdentityResult identity)
        {
            if (identity == null || !identity.HasIdentity || identity.Recipe == null)
                throw new InvalidOperationException("A complete persisted Sylvan starter realm identity is required.");
            if (!string.Equals(identity.LayoutId, identity.Recipe.LayoutId, StringComparison.Ordinal))
                throw new InvalidOperationException("Persisted Sylvan layout ID does not match its resolved recipe.");
            var validation = RealmLayoutRecipeValidator.ValidateStarterRecipe(identity.Recipe);
            if (!validation.IsValid)
                throw new InvalidOperationException($"Persisted Sylvan layout recipe '{identity.LayoutId}' failed strict validation.");

            var nodes = new SylvanRealmMaterializationNode[identity.Recipe.Nodes.Count];
            var bySourceId = new Dictionary<string, SylvanRealmMaterializationNode>(StringComparer.Ordinal);
            var byRole = new Dictionary<SylvanRealmNodeMaterializationRole, SylvanRealmMaterializationNode>();
            for (var i = 0; i < identity.Recipe.Nodes.Count; i++)
            {
                var source = identity.Recipe.Nodes[i];
                var semantic = Semantic(source.MaterializationRole);
                var node = new SylvanRealmMaterializationNode(source.NodeId, source.MaterializationRole, semantic.graphId, semantic.label,
                    new Vector2(source.X, source.Z));
                if (bySourceId.ContainsKey(source.NodeId) || byRole.ContainsKey(source.MaterializationRole))
                    throw new InvalidOperationException("Validated Sylvan recipe contains duplicate materialization facts.");
                bySourceId.Add(source.NodeId, node);
                byRole.Add(source.MaterializationRole, node);
                nodes[i] = node;
            }

            var edges = new SylvanRealmMaterializationEdge[identity.Recipe.Edges.Count];
            for (var i = 0; i < identity.Recipe.Edges.Count; i++)
            {
                var source = identity.Recipe.Edges[i];
                if (!bySourceId.TryGetValue(source.FromNodeId, out var from) ||
                    !bySourceId.TryGetValue(source.ToNodeId, out var to))
                    throw new InvalidOperationException($"Sylvan edge '{source.EdgeId}' references a missing node.");
                var delta = to.Center - from.Center;
                var footprint = new ArenaPathFootprint((from.Center + to.Center) * .5f,
                    new Vector2(source.FloorPathWidth, delta.magnitude),
                    Mathf.Atan2(delta.x, delta.y) * Mathf.Rad2Deg);
                edges[i] = new SylvanRealmMaterializationEdge(source.EdgeId, from, to,
                    source.IsActivePathSafe, source.FloorPathWidth, footprint);
            }
            return new SylvanRealmLayoutPlan(identity.LayoutId, nodes, edges, byRole);
        }

        static (string graphId, string label) Semantic(SylvanRealmNodeMaterializationRole role) => role switch
        {
            SylvanRealmNodeMaterializationRole.PortalStart => ("Portal", "PORTAL"),
            SylvanRealmNodeMaterializationRole.LandmarkJunction => ("Crossroads", "CROSSROADS"),
            SylvanRealmNodeMaterializationRole.WolfGroveEncounter => ("Wolf Grove", "WOLF GROVE"),
            SylvanRealmNodeMaterializationRole.RootPathHazard => ("Root Path", "ROOT PATH"),
            SylvanRealmNodeMaterializationRole.EntGroveEncounter => ("Ent Grove", "ENT GROVE"),
            SylvanRealmNodeMaterializationRole.MoonwellRecovery => ("Moonwell", "MOONWELL"),
            SylvanRealmNodeMaterializationRole.HeartTreeObjective => ("Heart Tree", "HEART TREE"),
            _ => throw new InvalidOperationException($"Unsupported Sylvan materialization role '{role}'.")
        };
    }
}
