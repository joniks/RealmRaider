using System.Collections.Generic;
using RealmRaiders.AI;
using RealmRaiders.Characters;
using RealmRaiders.Controllers;
using UnityEngine;

namespace RealmRaiders.Raid
{
    /// <summary>
    /// Node-local authority gate for explicitly supplied hostile entities. It
    /// keeps presentation reveal independent from the existing AI controller.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NodeEntryEncounterActivation : MonoBehaviour
    {
        sealed class Entry
        {
            public CombatEntity Entity;
            public CreatureBrain Brain;
            public bool DormancyApplied;
        }

        readonly List<Entry> entries = new();
        bool activated;

        public bool IsActivated => activated;
        public int TrackedHostileCount => entries.Count;

        public void Initialize(IReadOnlyList<CombatEntity> explicitHostiles)
        {
            ReleaseOwnedDormancy();
            entries.Clear();
            activated = false;
            if (explicitHostiles == null) return;

            for (var index = 0; index < explicitHostiles.Count; index++)
            {
                var entity = explicitHostiles[index];
                if (!entity || entries.Exists(entry => entry.Entity == entity)) continue;
                var brain = entity.Controller<CreatureBrain>();
                var entry = new Entry { Entity = entity, Brain = brain };
                if (brain && entity.Health != null && !entity.Health.IsDead && entity.ActiveController == brain)
                {
                    brain.SetControl(false);
                    entity.enabled = false;
                    entry.DormancyApplied = true;
                }
                entries.Add(entry);
            }
        }

        public void Activate()
        {
            if (activated) return;
            activated = true;
            if (GameplayInput.TerminalState) return;

            foreach (var entry in entries)
            {
                var entity = entry.Entity;
                var brain = entry.Brain;
                if (!entity || entity.Health == null || entity.Health.IsDead) continue;
                if (brain && entity.ActiveController == brain)
                {
                    entity.enabled = true;
                    brain.SetControl(true);
                    entry.DormancyApplied = false;
                }
                else if (entry.DormancyApplied)
                {
                    // A controller swap (for example possession) remains
                    // authoritative; restore ticking without granting AI.
                    entity.enabled = true;
                    entry.DormancyApplied = false;
                }
            }
        }

        public bool IsDormant(CombatEntity entity)
        {
            var entry = entries.Find(candidate => candidate.Entity == entity);
            return entry != null && entry.DormancyApplied && !activated && entity && !entity.enabled;
        }

        public void Release() => ReleaseOwnedDormancy();

        void ReleaseOwnedDormancy()
        {
            if (GameplayInput.TerminalState) return;
            foreach (var entry in entries)
            {
                var entity = entry.Entity;
                var brain = entry.Brain;
                if (!entry.DormancyApplied || !entity || entity.Health == null || entity.Health.IsDead ||
                    !entity.gameObject.scene.IsValid() || !entity.gameObject.scene.isLoaded) continue;
                entity.enabled = true;
                if (brain && entity.ActiveController == brain) brain.SetControl(true);
                entry.DormancyApplied = false;
            }
        }

        void OnDisable() => Release();

        void OnDestroy()
        {
            Release();
            entries.Clear();
        }
    }
}
