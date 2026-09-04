using System;
using System.Collections.Generic;

namespace RealmRaiders.Realm
{
    public enum FogState { Hidden, Discovered, Visited }

    public sealed class RealmNode
    {
        public string Id { get; }
        public FogState Fog { get; private set; } = FogState.Hidden;
        readonly List<RealmNode> neighbors = new();
        public IReadOnlyList<RealmNode> Neighbors => neighbors;
        public event Action<RealmNode> FogChanged;

        public RealmNode(string id) => Id = id;
        internal void Connect(RealmNode other) { if (!neighbors.Contains(other)) neighbors.Add(other); }
        public void Discover()
        { if (Fog != FogState.Hidden) return; Fog = FogState.Discovered; FogChanged?.Invoke(this); }
        public void Visit()
        {
            if (Fog != FogState.Visited) { Fog = FogState.Visited; FogChanged?.Invoke(this); }
            foreach (var neighbor in neighbors) neighbor.Discover();
        }
    }

    public sealed class RealmGraph
    {
        readonly Dictionary<string, RealmNode> nodes = new();
        public IReadOnlyDictionary<string, RealmNode> Nodes => nodes;
        public RealmNode Add(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || nodes.ContainsKey(id)) throw new ArgumentException("Node id must be unique.", nameof(id));
            return nodes[id] = new RealmNode(id);
        }
        public void Connect(string first, string second)
        {
            var a = nodes[first]; var b = nodes[second]; a.Connect(b); b.Connect(a);
        }
    }
}
