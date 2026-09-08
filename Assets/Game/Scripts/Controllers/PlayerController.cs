using RealmRaiders.Characters;
using RealmRaiders.CameraSystem;
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
        public event System.Action<PlayerController, Vector3> LocomotionAccepted;
        public bool IsActive { get; private set; }
        CombatEntity entity;
        Vector3 destination;
        Vector3 lastMovementDirection;
        bool hasDestination;
        Vector2 pressPosition;
        float pressTime;
        Camera view;
        bool pointerStartedOnUi;
        int interactionRevision;
        public int RootEscapeProgress { get; private set; }
        float rootBreakUntil;
        public bool RootEscapeVisible => (entity && entity.IsRooted) || Time.time < rootBreakUntil;
        public void ResetEscapeState() { RootEscapeProgress = 0; rootBreakUntil = 0; hasDestination = false; lastMovementDirection = Vector3.zero; }

        void Awake() => entity = GetComponent<CombatEntity>();
        int ControllerKey => GetEntityId().GetHashCode();
        void OnDestroy() => GameplayInput.SetDirectControl(ControllerKey, false);
        public void SetControl(bool active)
        {
            IsActive = active; hasDestination = false; lastMovementDirection = Vector3.zero; if (!active) ResetEscapeState(); view = Camera.main;
            var rig = view ? view.GetComponent<PrototypeCameraRig>() : null;
            var awareness = rig ? rig.GetComponent<CombatCameraAwareness>() : null;
            if (active && rig && !awareness) awareness = rig.gameObject.AddComponent<CombatCameraAwareness>();
            if (awareness) awareness.SetControlled(active ? entity : null);
            GameplayInput.SetDirectControl(ControllerKey, active);
        }

        public void Tick()
        {
            if (view == null) return;
            if (interactionRevision != GameplayInput.InteractionRevision) { interactionRevision = GameplayInput.InteractionRevision; hasDestination = false; lastMovementDirection = Vector3.zero; pointerStartedOnUi = false; pressPosition = default; pressTime = 0; }
            var keyboard = Keyboard.current;
            var keyboardMove = keyboard == null ? Vector2.zero : new Vector2((keyboard.dKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed ? 1 : 0), (keyboard.wKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed ? 1 : 0));
            if (!entity.IsRooted && Time.time >= rootBreakUntil) RootEscapeProgress = 0;
            var directMove = GameplayInput.Movement.sqrMagnitude > .001f ? GameplayInput.Movement : Vector2.ClampMagnitude(keyboardMove, 1);
            if (entity.IsDodging) hasDestination = false;
            else if (directMove.sqrMagnitude > .001f) { hasDestination = false; lastMovementDirection = new Vector3(directMove.x, 0, directMove.y).normalized; var before = transform.position; entity.Move(lastMovementDirection * directMove.magnitude * entity.Stats.MoveSpeed); ReportLocomotionAccepted(transform.position - before); }
            else if (hasDestination) { var delta = destination - transform.position; delta.y = 0; if (delta.magnitude < .25f) hasDestination = false; else { lastMovementDirection = delta.normalized; var before = transform.position; entity.Move(lastMovementDirection * entity.Stats.MoveSpeed); ReportLocomotionAccepted(transform.position - before); } }
            else entity.Move(Vector3.zero);
            if (Pointer.current == null) return;
            var pointer = Pointer.current;
            if (pointer.press.wasPressedThisFrame)
            { pointerStartedOnUi = GameplayInput.HasUiOwnership || EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(); pressPosition = pointer.position.ReadValue(); pressTime = Time.time; }
            if (pointer.press.wasReleasedThisFrame && !pointerStartedOnUi && !GameplayInput.HasUiOwnership && (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()))
            {
                var release = pointer.position.ReadValue();
                if (entity.IsRooted) { RootEscapeProgress = Mathf.Min(5, RootEscapeProgress + 1); if (RootEscapeProgress >= 5) { entity.BreakRoot(); rootBreakUntil = Time.time + .8f; } pointerStartedOnUi = false; return; }
                var delta = release - pressPosition;
                if (delta.magnitude > 70 && Time.time - pressTime < .55f)
                    entity.TryUse(1, new Vector3(delta.x, 0, delta.y));
                else if (Physics.Raycast(view.ScreenPointToRay(release), out var hit, 100))
                {
                    var usingJoystick = PrototypeSave.EffectiveControlStyle(ResponsiveLayout.Classify(new Vector2(Screen.width, Screen.height)) == PrototypeOrientation.Landscape) == "Joystick";
                    var enemy = hit.collider.GetComponentInParent<CombatEntity>();
                    if (enemy && enemy != entity)
                    {
                        view.GetComponent<CombatCameraAwareness>()?.ReportThreat(enemy);
                        var direction = enemy.transform.position - transform.position;
                        if (direction.magnitude <= 3.4f) entity.TryUse(0, direction);
                        else if (!usingJoystick) SetDestination(enemy.transform.position);
                    }
                    else if (!usingJoystick) SetDestination(hit.point);
                }
            }
            if (pointer.press.wasReleasedThisFrame) pointerStartedOnUi = false;
        }

        public bool UseAbility(int index)
        {
            if (!IsActive) return false;
            var direction = view ? view.transform.forward : transform.forward; direction.y = 0;
            return entity.TryUse(index, direction);
        }

        public bool Dodge()
        {
            if (!IsActive || !entity.TryDodge(lastMovementDirection)) return false;
            hasDestination = false;
            return true;
        }

        void SetDestination(Vector3 value)
        {
            destination = value;
            hasDestination = true;
            var direction = destination - transform.position; direction.y = 0;
            if (direction.sqrMagnitude > .001f) lastMovementDirection = direction.normalized;
        }

        void ReportLocomotionAccepted(Vector3 displacement)
        {
            if (IsActive && entity && !entity.IsRooted && !entity.IsDodging && !entity.IsActionResolving)
                LocomotionAccepted?.Invoke(this, displacement);
        }
    }
}
