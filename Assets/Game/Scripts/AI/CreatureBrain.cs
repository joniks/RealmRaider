using System;
using System.Collections.Generic;
using RealmRaiders.Characters;
using RealmRaiders.Controllers;
using UnityEngine;

namespace RealmRaiders.AI
{
    public enum BrainState { Idle, Detect, Chase, Attack, Return }

    [RequireComponent(typeof(CombatEntity))]
    public sealed class CreatureBrain : MonoBehaviour, IEntityController
    {
        static readonly List<CreatureBrain> activeBrains = new();

        /// <summary>Presentation observers can react to a real AI target/state change without polling the whole scene.</summary>
        public static event Action<CreatureBrain> HostileIntentChanged;
        public static IReadOnlyList<CreatureBrain> ActiveBrains => activeBrains;

        public bool IsActive { get; private set; }
        public BrainState State { get; private set; }
        CombatEntity target;
        public CombatEntity Target
        {
            get => target;
            set
            {
                if (target == value) return;
                target = value;
                PublishIntent();
            }
        }
        public float DetectionRange = 11;
        Vector3 home;
        CombatEntity entity;

        void Awake() { entity = GetComponent<CombatEntity>(); home = transform.position; activeBrains.Add(this); }
        void OnDestroy() { activeBrains.Remove(this); }
        public void SetControl(bool active) { IsActive = active; SetState(BrainState.Idle); PublishIntent(); }
        public void Tick()
        {
            if (!Target || Target.Health.IsDead || Vector3.Distance(home, transform.position) > 18)
            { SetState(BrainState.Return); ReturnHome(); return; }
            var delta = Target.transform.position - transform.position; delta.y = 0;
            if (delta.magnitude > DetectionRange) { SetState(BrainState.Idle); entity.Move(Vector3.zero); }
            else if (delta.magnitude > 3.1f) { SetState(BrainState.Chase); entity.Move(delta.normalized * entity.Stats.MoveSpeed); }
            else { SetState(BrainState.Attack); entity.Move(Vector3.zero); entity.TryUse(0, delta); }
        }
        void ReturnHome()
        {
            var delta = home - transform.position; delta.y = 0;
            if (delta.magnitude < .3f) { SetState(BrainState.Idle); entity.Move(Vector3.zero); }
            else entity.Move(delta.normalized * entity.Stats.MoveSpeed);
        }

        void SetState(BrainState next)
        {
            if (State == next) return;
            State = next;
            PublishIntent();
        }

        void PublishIntent() => HostileIntentChanged?.Invoke(this);
    }
}
