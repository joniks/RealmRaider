using RealmRaiders.Characters;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace RealmRaiders.Controllers
{
    [RequireComponent(typeof(CombatEntity))]
    public sealed class PlayerController : MonoBehaviour, IEntityController
    {
        public bool IsActive { get; private set; }
        CombatEntity entity;
        Vector3 destination;
        bool hasDestination;
        Vector2 pressPosition;
        float pressTime;
        Camera view;

        void Awake() => entity = GetComponent<CombatEntity>();
        public void SetControl(bool active) { IsActive = active; hasDestination = false; view = Camera.main; }

        public void Tick()
        {
            if (Pointer.current == null || view == null) return;
            var pointer = Pointer.current;
            if (pointer.press.wasPressedThisFrame)
            { pressPosition = pointer.position.ReadValue(); pressTime = Time.time; }
            if (pointer.press.wasReleasedThisFrame && (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()))
            {
                var release = pointer.position.ReadValue();
                var delta = release - pressPosition;
                if (delta.magnitude > 70 && Time.time - pressTime < .55f)
                    entity.TryUse(1, new Vector3(delta.x, 0, delta.y));
                else if (Physics.Raycast(view.ScreenPointToRay(release), out var hit, 100))
                {
                    var enemy = hit.collider.GetComponentInParent<CombatEntity>();
                    if (enemy && enemy != entity)
                    {
                        var direction = enemy.transform.position - transform.position;
                        if (direction.magnitude <= 3.4f) entity.TryUse(0, direction);
                        else { destination = enemy.transform.position; hasDestination = true; }
                    }
                    else { destination = hit.point; hasDestination = true; }
                }
            }
            if (hasDestination)
            {
                var delta = destination - transform.position; delta.y = 0;
                if (delta.magnitude < .25f) hasDestination = false;
                else entity.Move(delta.normalized * entity.Stats.MoveSpeed);
            }
            else entity.Move(Vector3.zero);
        }

        public void UseAbility(int index)
        {
            if (!IsActive) return;
            var direction = view ? view.transform.forward : transform.forward; direction.y = 0;
            entity.TryUse(index, direction);
        }
    }
}
