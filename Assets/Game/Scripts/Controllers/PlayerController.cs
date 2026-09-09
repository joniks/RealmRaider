using System.Collections.Generic;
using RealmRaiders.Characters;
using RealmRaiders.CameraSystem;
using RealmRaiders.Combat;
using RealmRaiders.Core;
using RealmRaiders.Realm;
using RealmRaiders.Traps;
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
        Vector2 pointerPosition;
        int activePointerId = int.MinValue;
        int pointerInteractionRevision;
        string pointerControlStyle;
        bool hasGroundTap;
        Vector2 lastGroundTapScreen;
        Vector3 lastGroundTapWorld;
        float lastGroundTapTime;
        string effectiveControlStyle;
        PrototypeOrientation? testOrientation;
        Camera view;
        int interactionRevision;
        readonly CombatInputBuffer abilityBuffer = new();
        readonly List<RaycastResult> uiRaycastResults = new();
        Health observedHealth;
        public const float SwipePixels = 70f;
        public const float SwipeSeconds = .55f;
        public const float DoubleTapSeconds = .35f;
        public const float DoubleTapScreenPixels = 90f;
        public const float DoubleTapWorldDistance = 1.25f;
        public int RootEscapeProgress { get; private set; }
        float rootBreakUntil;
        public bool RootEscapeVisible => (entity && entity.IsRooted) || Time.time < rootBreakUntil;
        public bool HasDestination => hasDestination;
        public Vector3 Destination => destination;
        public string EffectiveControlStyle => ResolveEffectiveControlStyle();
        public bool HasBufferedAbility { get { PruneAbilityBuffer(); return abilityBuffer.HasPending; } }
        public bool IsAbilityBuffered(int index) { PruneAbilityBuffer(); return abilityBuffer.HasPending && abilityBuffer.PendingAbilityIndex == index; }
        public void ResetEscapeState() { RootEscapeProgress = 0; rootBreakUntil = 0; ResetControlIntent(); }

        void Awake() => entity = GetComponent<CombatEntity>();
        int ControllerKey => GetEntityId().GetHashCode();
        void OnDisable() { ResetControlIntent(); if (entity) entity.CancelJump(); }
        void OnDestroy() { StopObservingDeath(); ResetControlIntent(); ClearCameraAwareness(); GameplayInput.SetDirectControl(ControllerKey, false); }
        public void SetControl(bool active)
        {
            IsActive = active; ResetControlIntent(); interactionRevision = GameplayInput.InteractionRevision; effectiveControlStyle = ResolveEffectiveControlStyle(); if (!active) ResetEscapeState();
            if (!active) { StopObservingDeath(); if (entity) entity.CancelJump(); ClearCameraAwareness(); GameplayInput.SetDirectControl(ControllerKey, false); return; }
            ObserveDeath();
            var nextView = Camera.main;
            if (view && view != nextView) ClearCameraAwareness();
            view = nextView;
            var rig = view ? view.GetComponent<PrototypeCameraRig>() : null;
            var awareness = rig ? rig.GetComponent<CombatCameraAwareness>() : null;
            if (rig && !awareness) awareness = rig.gameObject.AddComponent<CombatCameraAwareness>();
            if (awareness) awareness.SetControlled(entity);
            GameplayInput.SetDirectControl(ControllerKey, true);
        }

        void ClearCameraAwareness()
        {
            var rig = view ? view.GetComponent<PrototypeCameraRig>() : null;
            var awareness = rig ? rig.GetComponent<CombatCameraAwareness>() : null;
            if (awareness) awareness.SetControlled(null);
            rig?.ClearControlYaw();
            view = null;
        }

        public void Tick()
        {
            var nextStyle = ResolveEffectiveControlStyle();
            if (interactionRevision != GameplayInput.InteractionRevision || effectiveControlStyle != nextStyle)
            {
                interactionRevision = GameplayInput.InteractionRevision;
                effectiveControlStyle = nextStyle;
                ResetControlIntent();
            }
            if (GameplayInput.TerminalState || !IsActive || !isActiveAndEnabled || !entity || entity.Health == null || entity.Health.IsDead)
            {
                ResetControlIntent();
                return;
            }
            ConsumeBufferedAbility();
            if (view == null) return;
            var keyboard = Keyboard.current;
            var keyboardMove = keyboard == null ? Vector2.zero : new Vector2((keyboard.dKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed ? 1 : 0), (keyboard.wKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed ? 1 : 0));
            if (!entity.IsRooted && Time.time >= rootBreakUntil) RootEscapeProgress = 0;
            var directMove = GameplayInput.Movement.sqrMagnitude > .001f ? GameplayInput.Movement : Vector2.ClampMagnitude(keyboardMove, 1);
            if (entity.IsDodging) hasDestination = false;
            else if (directMove.sqrMagnitude > .001f) { hasDestination = false; lastMovementDirection = new Vector3(directMove.x, 0, directMove.y).normalized; var before = transform.position; entity.Move(lastMovementDirection * directMove.magnitude * entity.Stats.MoveSpeed); ReportLocomotionAccepted(transform.position - before); }
            else if (hasDestination) { var delta = destination - transform.position; delta.y = 0; if (delta.magnitude < .25f) hasDestination = false; else { lastMovementDirection = delta.normalized; var before = transform.position; entity.Move(lastMovementDirection * entity.Stats.MoveSpeed); ReportLocomotionAccepted(transform.position - before); } }
            else entity.Move(Vector3.zero);
            ProcessRuntimePointers();
        }

        void ProcessRuntimePointers()
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen != null)
            {
                foreach (var touch in touchscreen.touches)
                {
                    var phase = touch.phase.ReadValue();
                    if (phase == UnityEngine.InputSystem.TouchPhase.None || phase == UnityEngine.InputSystem.TouchPhase.Stationary) continue;
                    var pointerId = UiPointerId(touchscreen, touch.touchId.ReadValue());
                    var position = touch.position.ReadValue();
                    if (phase == UnityEngine.InputSystem.TouchPhase.Began) BeginWorldPointer(pointerId, position, IsPointerOverUi(pointerId, position));
                    else if (phase == UnityEngine.InputSystem.TouchPhase.Moved) DragWorldPointer(pointerId, position);
                    else if (phase == UnityEngine.InputSystem.TouchPhase.Ended) EndWorldPointer(pointerId, position, IsPointerOverUi(pointerId, position));
                    else if (phase == UnityEngine.InputSystem.TouchPhase.Canceled) CancelWorldPointer(pointerId);
                }
                return;
            }

            var pointer = Pointer.current;
            if (pointer == null) return;
            var mousePointerId = pointer.deviceId;
            var pointerPosition = pointer.position.ReadValue();
            if (pointer.press.wasPressedThisFrame) BeginWorldPointer(mousePointerId, pointerPosition, IsPointerOverUi(mousePointerId, pointerPosition));
            if (pointer.press.isPressed && pointer.delta.ReadValue().sqrMagnitude > .001f) DragWorldPointer(mousePointerId, pointerPosition);
            if (pointer.press.wasReleasedThisFrame) EndWorldPointer(mousePointerId, pointerPosition, IsPointerOverUi(mousePointerId, pointerPosition));
        }

        public void BeginWorldPointer(int pointerId, Vector2 screenPosition, bool overUi)
        {
            if (!IsActive || !isActiveAndEnabled || !entity || entity.Health == null || entity.Health.IsDead || GameplayInput.TerminalState || !view || activePointerId != int.MinValue) return;
            if (overUi || GameplayInput.IsUiOwned(pointerId)) return;
            activePointerId = pointerId;
            pressPosition = pointerPosition = screenPosition;
            pressTime = Time.unscaledTime;
            pointerInteractionRevision = GameplayInput.InteractionRevision;
            pointerControlStyle = ResolveEffectiveControlStyle();
        }

        public void DragWorldPointer(int pointerId, Vector2 screenPosition)
        {
            if (activePointerId != pointerId) return;
            if (GameplayInput.IsUiOwned(pointerId) || pointerInteractionRevision != GameplayInput.InteractionRevision || pointerControlStyle != ResolveEffectiveControlStyle())
            {
                CancelWorldPointer(pointerId);
                return;
            }
            var delta = screenPosition - pointerPosition;
            pointerPosition = screenPosition;
            if (pointerControlStyle == "Joystick") view.GetComponent<PrototypeCameraRig>()?.RequestManualYaw(delta.x);
        }

        public void EndWorldPointer(int pointerId, Vector2 screenPosition, bool overUi)
        {
            if (activePointerId != pointerId) return;
            var valid = !overUi && !GameplayInput.IsUiOwned(pointerId) && pointerInteractionRevision == GameplayInput.InteractionRevision && pointerControlStyle == ResolveEffectiveControlStyle();
            var style = pointerControlStyle;
            var startedAt = pressTime;
            var startedAtPosition = pressPosition;
            ClearPointerGesture();
            if (!valid || !IsActive || !entity || entity.Health == null || entity.Health.IsDead || GameplayInput.TerminalState || !view) { ClearGroundTap(); return; }

            var delta = screenPosition - startedAtPosition;
            if (entity.IsRooted)
            {
                ClearGroundTap();
                if (delta.magnitude <= SwipePixels)
                {
                    RootEscapeProgress = Mathf.Min(5, RootEscapeProgress + 1);
                    if (RootEscapeProgress >= 5) { entity.BreakRoot(); rootBreakUntil = Time.time + .8f; }
                }
                return;
            }
            if (style == "Joystick") { ClearGroundTap(); return; }
            if (delta.magnitude > SwipePixels && Time.unscaledTime - startedAt < SwipeSeconds)
            {
                ClearGroundTap();
                RequestAbility(1, new Vector3(delta.x, 0, delta.y));
                return;
            }
            ProcessFingertapRelease(screenPosition);
        }

        public void CancelWorldPointer(int pointerId)
        {
            if (activePointerId == pointerId) ClearPointerGesture();
        }

        void ProcessFingertapRelease(Vector2 screenPosition)
        {
            if (!Physics.Raycast(view.ScreenPointToRay(screenPosition), out var hit, 100, ~0, QueryTriggerInteraction.Ignore)) { ClearGroundTap(); return; }
            var enemy = hit.collider.GetComponentInParent<CombatEntity>();
            if (enemy && enemy != entity)
            {
                ClearGroundTap();
                view.GetComponent<CombatCameraAwareness>()?.ReportThreat(enemy);
                var direction = enemy.transform.position - transform.position;
                if (direction.magnitude <= 3.4f) RequestAbility(0, direction);
                else SetDestination(enemy.transform.position);
                return;
            }
            if (!IsValidGroundIntent(hit)) { ClearGroundTap(); return; }

            var now = Time.unscaledTime;
            var sameIntent = hasGroundTap && now - lastGroundTapTime <= DoubleTapSeconds &&
                Vector2.Distance(screenPosition, lastGroundTapScreen) <= DoubleTapScreenPixels &&
                HorizontalDistance(hit.point, lastGroundTapWorld) <= DoubleTapWorldDistance;
            if (sameIntent)
            {
                ClearGroundTap();
                Jump();
                return;
            }
            SetDestination(hit.point);
            hasGroundTap = true;
            lastGroundTapTime = now;
            lastGroundTapScreen = screenPosition;
            lastGroundTapWorld = hit.point;
        }

        static bool IsValidGroundIntent(RaycastHit hit)
        {
            if (!hit.collider || hit.collider.isTrigger || hit.normal.y < .55f) return false;
            if (hit.collider.GetComponentInParent<CombatEntity>() || hit.collider.GetComponentInParent<RealmCore>() || hit.collider.GetComponentInParent<TrapBase>()) return false;
            return true;
        }

        static float HorizontalDistance(Vector3 first, Vector3 second) =>
            Vector2.Distance(new Vector2(first.x, first.z), new Vector2(second.x, second.z));

        static int UiPointerId(Touchscreen touchscreen, int touchId)
        {
            unchecked { return (touchscreen.deviceId << 24) + touchId; }
        }

        bool IsPointerOverUi(int pointerId, Vector2 position)
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null) return false;
            if (eventSystem.IsPointerOverGameObject(pointerId)) return true;
            uiRaycastResults.Clear();
            eventSystem.RaycastAll(new PointerEventData(eventSystem) { pointerId = pointerId, position = position }, uiRaycastResults);
            for (var index = 0; index < uiRaycastResults.Count; index++)
                if (uiRaycastResults[index].gameObject && uiRaycastResults[index].gameObject.GetComponentInParent<Canvas>()) return true;
            return false;
        }

        public bool UseAbility(int index)
        {
            var direction = view ? view.transform.forward : transform.forward; direction.y = 0;
            return RequestAbility(index, direction);
        }

        public bool CanBufferAbility(int index)
        {
            PruneAbilityBuffer();
            return IsActive && isActiveAndEnabled && entity && entity.Health != null && !entity.Health.IsDead && !entity.IsJumping && !GameplayInput.TerminalState && entity.ActionPhase == CombatActionPhase.Recovery && index >= 0 && index < entity.Abilities.Count && entity.Abilities[index].IsReady;
        }

        public bool RequestAbility(int index, Vector3 direction)
        {
            PruneAbilityBuffer();
            if (!IsActive || !isActiveAndEnabled || !entity || entity.Health == null || entity.Health.IsDead || entity.IsJumping || GameplayInput.TerminalState || index < 0 || index >= entity.Abilities.Count) return false;
            ClearGroundTap();
            if (entity.ActionPhase == CombatActionPhase.Recovery)
            {
                if (!entity.Abilities[index].IsReady) return false;
                var snapshot = SafeDirection(direction);
                return abilityBuffer.TryQueue(index, snapshot.x, snapshot.y, snapshot.z, Time.unscaledTime);
            }
            if (entity.ActionPhase != CombatActionPhase.Idle) return false;
            abilityBuffer.Clear();
            return entity.TryUse(index, direction);
        }

        public bool Dodge()
        {
            ClearGroundTap();
            if (!IsActive || !entity.TryDodge(lastMovementDirection)) return false;
            hasDestination = false;
            return true;
        }

        public bool Jump()
        {
            ClearGroundTap();
            if (!IsActive || !isActiveAndEnabled || !entity || !entity.TryJump()) return false;
            abilityBuffer.Clear();
            return true;
        }

        void SetDestination(Vector3 value)
        {
            destination = value;
            hasDestination = true;
            var direction = destination - transform.position; direction.y = 0;
            if (direction.sqrMagnitude > .001f) lastMovementDirection = direction.normalized;
        }

        void ConsumeBufferedAbility()
        {
            PruneAbilityBuffer();
            if (!abilityBuffer.HasPending || entity.ActionPhase != CombatActionPhase.Idle) return;
            if (!abilityBuffer.TryConsume(Time.unscaledTime, out var request)) return;
            entity.TryUse(request.AbilityIndex, new Vector3(request.DirectionX, request.DirectionY, request.DirectionZ));
        }

        void PruneAbilityBuffer()
        {
            if (!IsActive || !isActiveAndEnabled || !entity || entity.Health == null || entity.Health.IsDead || GameplayInput.TerminalState || interactionRevision != GameplayInput.InteractionRevision)
            { abilityBuffer.Clear(); return; }
            abilityBuffer.Expire(Time.unscaledTime);
        }

        public void SetOrientationForTests(PrototypeOrientation? orientation)
        {
            testOrientation = orientation;
            effectiveControlStyle = ResolveEffectiveControlStyle();
            ResetControlIntent();
        }

        string ResolveEffectiveControlStyle()
        {
            var size = ResponsiveLayout.SafeAreaPixels.size;
            if (size.x <= 0 || size.y <= 0) size = new Vector2(Screen.width, Screen.height);
            var orientation = testOrientation ?? ResponsiveLayout.Classify(size);
            return PrototypeSave.EffectiveControlStyle(orientation == PrototypeOrientation.Landscape);
        }

        void ResetControlIntent()
        {
            hasDestination = false;
            lastMovementDirection = Vector3.zero;
            abilityBuffer.Clear();
            ClearPointerGesture();
            ClearGroundTap();
            if (view) view.GetComponent<PrototypeCameraRig>()?.ClearControlYaw();
        }

        void ClearPointerGesture()
        {
            activePointerId = int.MinValue;
            pointerInteractionRevision = 0;
            pointerControlStyle = null;
            pressPosition = pointerPosition = Vector2.zero;
            pressTime = 0;
        }

        void ClearGroundTap()
        {
            hasGroundTap = false;
            lastGroundTapScreen = Vector2.zero;
            lastGroundTapWorld = Vector3.zero;
            lastGroundTapTime = 0;
        }

        void ObserveDeath()
        {
            var next = entity ? entity.Health : null;
            if (observedHealth == next) return;
            StopObservingDeath();
            observedHealth = next;
            if (observedHealth) observedHealth.Died += OnControlledDeath;
        }

        void StopObservingDeath()
        {
            if (observedHealth) observedHealth.Died -= OnControlledDeath;
            observedHealth = null;
        }

        void OnControlledDeath() => ResetControlIntent();

        Vector3 SafeDirection(Vector3 direction)
        {
            direction.y = 0;
            if (!IsFinite(direction) || direction.sqrMagnitude <= .01f) { direction = transform.forward; direction.y = 0; }
            if (!IsFinite(direction) || direction.sqrMagnitude <= .01f) return Vector3.forward;
            return direction.normalized;
        }

        static bool IsFinite(Vector3 value) =>
            !float.IsNaN(value.x) && !float.IsInfinity(value.x) &&
            !float.IsNaN(value.y) && !float.IsInfinity(value.y) &&
            !float.IsNaN(value.z) && !float.IsInfinity(value.z);

        void ReportLocomotionAccepted(Vector3 displacement)
        {
            if (IsActive && entity && !entity.IsRooted && !entity.IsDodging && !entity.IsActionResolving)
            {
                LocomotionAccepted?.Invoke(this, displacement);
                if (ResolveEffectiveControlStyle() == "Fingertap") view?.GetComponent<PrototypeCameraRig>()?.RequestLocomotionYaw(displacement);
            }
        }
    }
}
