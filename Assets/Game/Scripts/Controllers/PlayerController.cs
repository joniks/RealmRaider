using RealmRaiders.Characters;
using RealmRaiders.Core;
using RealmRaiders.UI;
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
        bool pointerStartedOnUi;
        int interactionRevision;

        void Awake() => entity = GetComponent<CombatEntity>();
        int ControllerKey => GetEntityId().GetHashCode();
        void OnDestroy() => GameplayInput.SetDirectControl(ControllerKey, false);
        public void SetControl(bool active) { IsActive = active; hasDestination = false; view = Camera.main; GameplayInput.SetDirectControl(ControllerKey, active); }

        public void Tick()
        {
            if (view == null) return;
            if (interactionRevision != GameplayInput.InteractionRevision) { interactionRevision = GameplayInput.InteractionRevision; hasDestination = false; pointerStartedOnUi = false; pressPosition = default; pressTime = 0; }
            var keyboard = Keyboard.current;
            var keyboardMove = keyboard == null ? Vector2.zero : new Vector2((keyboard.dKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed ? 1 : 0), (keyboard.wKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed ? 1 : 0));
            var directMove = GameplayInput.Movement.sqrMagnitude > .001f ? GameplayInput.Movement : Vector2.ClampMagnitude(keyboardMove, 1);
            if (directMove.sqrMagnitude > .001f) { hasDestination = false; entity.Move(new Vector3(directMove.x, 0, directMove.y) * entity.Stats.MoveSpeed); }
            else if (hasDestination) { var delta = destination - transform.position; delta.y = 0; if (delta.magnitude < .25f) hasDestination = false; else entity.Move(delta.normalized * entity.Stats.MoveSpeed); }
            else entity.Move(Vector3.zero);
            if (Pointer.current == null) return;
            var pointer = Pointer.current;
            if (pointer.press.wasPressedThisFrame)
            { pointerStartedOnUi = GameplayInput.HasUiOwnership || EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(); pressPosition = pointer.position.ReadValue(); pressTime = Time.time; }
            if (pointer.press.wasReleasedThisFrame && !pointerStartedOnUi && !GameplayInput.HasUiOwnership && (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()))
            {
                var release = pointer.position.ReadValue();
                var delta = release - pressPosition;
                if (delta.magnitude > 70 && Time.time - pressTime < .55f)
                    entity.TryUse(1, new Vector3(delta.x, 0, delta.y));
                else if (Physics.Raycast(view.ScreenPointToRay(release), out var hit, 100))
                {
                    var usingJoystick = PrototypeSave.EffectiveControlStyle(ResponsiveLayout.Classify(new Vector2(Screen.width, Screen.height)) == PrototypeOrientation.Landscape) == "Joystick";
                    var enemy = hit.collider.GetComponentInParent<CombatEntity>();
                    if (enemy && enemy != entity)
                    {
                        var direction = enemy.transform.position - transform.position;
                        if (direction.magnitude <= 3.4f) entity.TryUse(0, direction);
                        else if (!usingJoystick) { destination = enemy.transform.position; hasDestination = true; }
                    }
                    else if (!usingJoystick) { destination = hit.point; hasDestination = true; }
                }
            }
            if (pointer.press.wasReleasedThisFrame) pointerStartedOnUi = false;
        }

        public void UseAbility(int index)
        {
            if (!IsActive) return;
            var direction = view ? view.transform.forward : transform.forward; direction.y = 0;
            entity.TryUse(index, direction);
        }
    }
}
