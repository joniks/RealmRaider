using System;
using RealmRaiders.Characters;
using UnityEngine;

namespace RealmRaiders.Realm
{
    public sealed class RealmNodeView : MonoBehaviour
    {
        public event Action<RealmNodeView> Visited;
        public RealmNode Node { get; private set; }
        public bool HasBeenEntered { get; private set; }
        CombatEntity hero;
        Renderer floor;
        GameObject[] contents;

        public void Initialize(RealmNode node, CombatEntity player, Renderer floorRenderer, params GameObject[] nodeContents)
        {
            Node = node; hero = player; floor = floorRenderer; contents = nodeContents ?? Array.Empty<GameObject>();
            node.FogChanged += _ => Refresh(); Refresh();
        }

        void Update()
        {
            if (HasBeenEntered || !hero || hero.Health.IsDead) return;
            var delta = hero.transform.position - transform.position; delta.y = 0;
            if (delta.sqrMagnitude <= 42.25f)
            { HasBeenEntered = true; Node.Visit(); Visited?.Invoke(this); }
        }

        void Refresh()
        {
            bool revealed = Node.Fog != FogState.Hidden;
            foreach (var item in contents) if (item) item.SetActive(revealed);
            if (!floor) return;
            floor.enabled = revealed;
            floor.material.color = Node.Fog == FogState.Visited ? new Color(.16f, .34f, .18f) : new Color(.08f, .17f, .11f);
        }
    }
}
