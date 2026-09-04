using RealmRaiders.Characters;
using RealmRaiders.Controllers;
using UnityEngine;

namespace RealmRaiders.AI
{
    public enum BrainState { Idle, Detect, Chase, Attack, Return }

    [RequireComponent(typeof(CombatEntity))]
    public sealed class CreatureBrain : MonoBehaviour, IEntityController
    {
        public bool IsActive { get; private set; }
        public BrainState State { get; private set; }
        public CombatEntity Target { get; set; }
        public float DetectionRange = 11;
        Vector3 home;
        CombatEntity entity;

        void Awake() { entity = GetComponent<CombatEntity>(); home = transform.position; }
        public void SetControl(bool active) { IsActive = active; State = BrainState.Idle; }
        public void Tick()
        {
            if (!Target || Target.Health.IsDead || Vector3.Distance(home, transform.position) > 18)
            { State = BrainState.Return; ReturnHome(); return; }
            var delta = Target.transform.position - transform.position; delta.y = 0;
            if (delta.magnitude > DetectionRange) { State = BrainState.Idle; entity.Move(Vector3.zero); }
            else if (delta.magnitude > 3.1f) { State = BrainState.Chase; entity.Move(delta.normalized * entity.Stats.MoveSpeed); }
            else { State = BrainState.Attack; entity.Move(Vector3.zero); entity.TryUse(0, delta); }
        }
        void ReturnHome()
        {
            var delta = home - transform.position; delta.y = 0;
            if (delta.magnitude < .3f) { State = BrainState.Idle; entity.Move(Vector3.zero); }
            else entity.Move(delta.normalized * entity.Stats.MoveSpeed);
        }
    }
}
