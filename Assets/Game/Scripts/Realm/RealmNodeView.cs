using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RealmRaiders.Characters;
using UnityEngine;

namespace RealmRaiders.Realm
{
    public sealed class RealmNodeVisit
    {
        public string NodeId { get; }
        public IReadOnlyList<CombatEntity> AliveHostiles { get; }

        internal RealmNodeVisit(string nodeId, List<CombatEntity> aliveHostiles)
        {
            NodeId = nodeId;
            AliveHostiles = new ReadOnlyCollection<CombatEntity>(aliveHostiles.ToArray());
        }
    }

    public sealed class RealmNodeView : MonoBehaviour
    {
        public event Action<RealmNodeView> Visited;
        public event Action<RealmNodeVisit> EncounterEntered;
        public RealmNode Node { get; private set; }
        public bool HasBeenEntered { get; private set; }
        CombatEntity hero;
        Renderer floor;
        RealmNodeSurfacePresentation surfacePresentation;
        GameObject[] contents;
        readonly List<CombatEntity> explicitHostiles = new();

        public void Initialize(RealmNode node, CombatEntity player, Renderer floorRenderer, params GameObject[] nodeContents)
        {
            if (Node != null) Node.FogChanged -= OnFogChanged;
            Node = node; hero = player; floor = floorRenderer;
            surfacePresentation = floor ? floor.GetComponent<RealmNodeSurfacePresentation>() : null;
            contents = nodeContents ?? Array.Empty<GameObject>();
            HasBeenEntered = false;
            explicitHostiles.Clear();
            foreach (var item in contents)
            {
                var hostile = item ? item.GetComponent<CombatEntity>() : null;
                if (hostile && !explicitHostiles.Contains(hostile)) explicitHostiles.Add(hostile);
            }
            node.FogChanged += OnFogChanged; Refresh();
        }

        void Update()
        {
            if (HasBeenEntered || !hero || hero.Health.IsDead) return;
            var delta = hero.transform.position - transform.position; delta.y = 0;
            if (delta.sqrMagnitude <= 42.25f)
            {
                HasBeenEntered = true;
                Node.Visit();
                Visited?.Invoke(this);
                var alive = new List<CombatEntity>(explicitHostiles.Count);
                foreach (var hostile in explicitHostiles)
                    if (hostile && hostile.Health != null && !hostile.Health.IsDead) alive.Add(hostile);
                EncounterEntered?.Invoke(new RealmNodeVisit(Node.Id, alive));
            }
        }

        void OnFogChanged(RealmNode _) => Refresh();

        void Refresh()
        {
            bool revealed = Node.Fog != FogState.Hidden;
            foreach (var item in contents) if (item) item.SetActive(revealed);
            if (!floor) return;
            floor.enabled = revealed;
            var tint = Node.Fog == FogState.Visited ? new Color(.16f, .34f, .18f) : new Color(.08f, .17f, .11f);
            if (surfacePresentation) surfacePresentation.ApplyTint(tint);
            else floor.material.color = tint;
        }

        void OnDestroy()
        {
            if (Node != null) Node.FogChanged -= OnFogChanged;
            explicitHostiles.Clear();
            surfacePresentation = null;
        }
    }
}
