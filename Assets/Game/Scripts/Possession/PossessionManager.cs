using System;
using System.Collections.Generic;
using System.Globalization;
using RealmRaiders.AI;
using RealmRaiders.CameraSystem;
using RealmRaiders.Characters;
using RealmRaiders.Controllers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace RealmRaiders.Possession
{
    public sealed class PossessionManager : MonoBehaviour
    {
        public const float KeeperTapMaximumDuration = .45f;
        public const float KeeperTapSlopShortEdge = .035f;
        public const string ExplicitReleaseFallback = "RELEASED — KEEPER OVERVIEW";
        public const string ForcedReleaseFallback = "POSSESSION ENDED — RETURNING TO KEEPER";
        public event Action<CombatEntity> SelectionChanged;
        public event Action<CombatEntity> PossessionChanged;
        public event Action<bool> Released;
        public event Action<string> MomentFeedback;
        public CombatEntity Selected { get; private set; }
        public CombatEntity Possessed { get; private set; }
        public bool IsPossessing => Possessed;
        public PrototypeCameraRig CameraRig => cameraRig;
        PrototypeCameraRig cameraRig;
        PossessionEnergy energy;
        GameObject selectionVisual;
        GameObject possessionPulse;
        Coroutine pulseRoutine;
        Coroutine slowBeat;
        float normalTimeScale = 1, normalFixedDeltaTime = .02f;
        readonly List<CombatEntity> registered = new();
        readonly Dictionary<CombatEntity, Action> registeredDeaths = new();
        readonly Dictionary<int, KeeperPress> keeperPresses = new();
        readonly List<KeeperSelectionCandidate> selectionCandidates = new();
        readonly List<RaycastResult> uiRaycastResults = new();

        readonly struct KeeperPress
        {
            public KeeperPress(Vector2 position, float startedAt, bool blocked)
            { Position = position; StartedAt = startedAt; Blocked = blocked; }

            public Vector2 Position { get; }
            public float StartedAt { get; }
            public bool Blocked { get; }
        }

        public void Initialize(PrototypeCameraRig rig) => cameraRig = rig;
        public void ConfigureEnergy(PossessionEnergy value) => energy = value;
        public void Register(CombatEntity entity)
        {
            if (!entity || entity.Health == null || registered.Contains(entity)) return;
            registered.Add(entity);
            Action onDeath = () => OnRegisteredDied(entity);
            registeredDeaths.Add(entity, onDeath);
            entity.Health.Died += onDeath;
        }

        void Update()
        {
            if (GameplayInput.TerminalState)
            {
                keeperPresses.Clear();
                ClearSelected(true);
                ClearPossessionArrival(Possessed);
            }
            else if (CanAcceptKeeperSelection()) PollKeeperSelectionInput();
            else keeperPresses.Clear();
            if (Possessed && (!Possessed.Health || Possessed.Health.IsDead || !(Possessed.ActiveController is PlayerController)))
                ClearPossessionArrival(Possessed);
            if (Possessed && energy != null && !energy.Consume(Time.deltaTime)) Release(true, "POSSESSION ENERGY DEPLETED — RETURNING TO KEEPER");
        }
        public void Select(CombatEntity entity)
        {
            if (IsPossessing || GameplayInput.TerminalState || !IsRegisteredAndSelectable(entity)) return;
            Selected = entity; ShowSelection(entity); SelectionChanged?.Invoke(Selected);
        }

        public void BeginKeeperPress(int pointerId, Vector2 position, float startedAt, bool uiOwned = false)
        {
            if (keeperPresses.ContainsKey(pointerId)) return;
            var blocked = uiOwned || GameplayInput.HasUiOwnership || GameplayInput.IsUiOwned(pointerId) || !CanAcceptKeeperSelection();
            keeperPresses.Add(pointerId, new KeeperPress(position, startedAt, blocked));
        }

        public bool EndKeeperPress(int pointerId, Vector2 position, float releasedAt, Vector2 screenSize, bool uiOwned = false)
        {
            if (!keeperPresses.TryGetValue(pointerId, out var press)) return false;
            keeperPresses.Remove(pointerId);
            if (press.Blocked || uiOwned || GameplayInput.HasUiOwnership || GameplayInput.IsUiOwned(pointerId) || !CanAcceptKeeperSelection()) return false;
            var duration = releasedAt - press.StartedAt;
            if (duration < 0 || duration > KeeperTapMaximumDuration) return false;
            var slop = KeeperSelectionResolver.ShortEdge(screenSize) * KeeperTapSlopShortEdge;
            if (slop <= 0 || (position - press.Position).sqrMagnitude > slop * slop) return false;
            return TrySelectAtScreenPoint(position, screenSize);
        }

        public void CancelKeeperPress(int pointerId) => keeperPresses.Remove(pointerId);

        public bool PossessSelected()
        {
            if (!Selected || !Selected.IsPossessable || IsPossessing || (energy != null && energy.IsDepleted)) return false;
            var player = Selected.Controller<PlayerController>();
            if (player == null) return false;
            keeperPresses.Clear();
            Possessed = Selected;
            ClearSelection();
            Possessed.SetController(player);
            Possessed.GetComponent<CharacterVisualMotion>()?.StartPossessionArrival();
            Possessed.Health.Died += OnPossessedDied;
            cameraRig.TransitionTo(Possessed, CameraMode.PossessedCreature, .85f);
            Pulse(Possessed); StartSlowBeat();
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
            PossessionChanged?.Invoke(Possessed);
            MomentFeedback?.Invoke($"YOU CONTROL: {Possessed.Definition.DisplayName.ToUpperInvariant()}");
            return true;
        }

        void OnPossessedDied()
        {
            var defeated = Possessed;
            var health = defeated ? defeated.Health : null;
            var displayName = defeated && defeated.Definition ? defeated.Definition.DisplayName : string.Empty;
            var feedback = DefeatReturnFeedbackCopy(displayName, health ? health.Current : float.NaN,
                health ? health.Maximum : float.NaN, defeated && defeated == Possessed, health && health.IsDead,
                GameplayInput.TerminalState);
            Release(true, feedback);
        }
        public void Release() => Release(false);
        public void Release(bool forced) => Release(forced, forced ? ForcedReleaseFallback : ExplicitReleaseFallback);
        void Release(bool forced, string feedback)
        {
            if (!Possessed) return;
            var released = Possessed;
            ClearPossessionArrival(released);
            released.Health.Died -= OnPossessedDied;
            var ai = released.Controller<CreatureBrain>();
            if (ai != null && !released.Health.IsDead) released.SetController(ai);
            else released.SetController(null);
            if (!forced)
            {
                var restoredActiveAi = ai != null && released.ActiveController == ai && ai.IsActive;
                var displayName = released.Definition ? released.Definition.DisplayName : string.Empty;
                feedback = ExplicitReleaseFeedbackCopy(displayName, released.Health.Current, released.Health.Maximum,
                    restoredActiveAi, GameplayInput.TerminalState);
            }
            Possessed = null; Selected = null;
            keeperPresses.Clear();
            ClearSelection(); ClearPulse();
            RestoreTime();
            cameraRig.TransitionTo(null, CameraMode.KeeperOverview);
            SelectionChanged?.Invoke(null); PossessionChanged?.Invoke(null);
            Released?.Invoke(forced);
            MomentFeedback?.Invoke(feedback);
        }

        public static string ExplicitReleaseFeedbackCopy(string displayName, float current, float maximum,
            bool restoredActiveAi, bool terminal)
        {
            if (!restoredActiveAi || terminal || string.IsNullOrWhiteSpace(displayName) || !Finite(current) ||
                !Finite(maximum) || current <= 0 || maximum <= 0 || current > maximum)
                return ExplicitReleaseFallback;
            return $"RELEASED — {displayName.Trim().ToUpperInvariant()} RESUMED DEFENSE\n{current.ToString("0.#", CultureInfo.InvariantCulture)}/{maximum.ToString("0.#", CultureInfo.InvariantCulture)} HP";
        }

        public static string DefeatReturnFeedbackCopy(string displayName, float current, float maximum,
            bool exactPossessedEntity, bool dead, bool terminal)
        {
            if (!exactPossessedEntity || !dead || terminal || string.IsNullOrWhiteSpace(displayName) ||
                !Finite(current) || current != 0 || !Finite(maximum) || maximum <= 0)
                return ForcedReleaseFallback;
            return $"{displayName.Trim().ToUpperInvariant()} DEFEATED — RETURNING TO KEEPER\n0/{maximum.ToString("0.#", CultureInfo.InvariantCulture)} HP";
        }

        static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        void ShowSelection(CombatEntity entity)
        {
            ClearSelection(); selectionVisual = new GameObject("Possession Selection Presentation", typeof(PossessionSelectionPresentation)); selectionVisual.transform.SetParent(entity.transform, false);
            selectionVisual.GetComponent<PossessionSelectionPresentation>().Initialize(entity.Definition.DisplayName);
        }
        void ClearSelection() { if (selectionVisual) { selectionVisual.SetActive(false); Destroy(selectionVisual); } selectionVisual = null; }
        void ClearSelected(bool notify)
        {
            var hadSelection = Selected;
            ClearSelection();
            Selected = null;
            if (notify && hadSelection) SelectionChanged?.Invoke(null);
        }

        static void ClearPossessionArrival(CombatEntity entity)
        {
            if (entity) entity.GetComponent<CharacterVisualMotion>()?.ClearPossessionArrival();
        }

        void OnRegisteredDied(CombatEntity entity)
        {
            if (Selected == entity) ClearSelected(true);
            keeperPresses.Clear();
        }

        bool CanAcceptKeeperSelection()
        {
            return isActiveAndEnabled && !IsPossessing && !GameplayInput.TerminalState && cameraRig &&
                   !cameraRig.IsTransitioning && cameraRig.Mode == CameraMode.KeeperOverview;
        }

        bool IsRegisteredAndSelectable(CombatEntity entity)
        {
            return entity && registered.Contains(entity) && entity.Health != null && !entity.Health.IsDead && entity.IsPossessable;
        }

        public bool CanSelect(CombatEntity entity) => CanAcceptKeeperSelection() && IsRegisteredAndSelectable(entity);

        void PollKeeperSelectionInput()
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen != null)
            {
                foreach (var touch in touchscreen.touches)
                {
                    var pointerId = UiPointerId(touchscreen, touch.touchId.ReadValue());
                    var position = touch.position.ReadValue();
                    if (touch.press.wasPressedThisFrame)
                        BeginKeeperPress(pointerId, position, Time.unscaledTime, IsUiOwned(pointerId, position));
                    if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled)
                        CancelKeeperPress(pointerId);
                    else if (touch.press.wasReleasedThisFrame)
                        EndKeeperPress(pointerId, position, Time.unscaledTime, CurrentScreenSize(), IsUiOwned(pointerId, position));
                }
                return;
            }

            var mouse = Mouse.current;
            if (mouse == null) return;
            var mousePointerId = mouse.deviceId;
            var mousePosition = mouse.position.ReadValue();
            if (mouse.leftButton.wasPressedThisFrame)
                BeginKeeperPress(mousePointerId, mousePosition, Time.unscaledTime, IsUiOwned(mousePointerId, mousePosition));
            if (mouse.leftButton.wasReleasedThisFrame)
                EndKeeperPress(mousePointerId, mousePosition, Time.unscaledTime, CurrentScreenSize(), IsUiOwned(mousePointerId, mousePosition));
        }

        bool TrySelectAtScreenPoint(Vector2 pointerPosition, Vector2 screenSize)
        {
            var view = cameraRig ? cameraRig.GetComponent<Camera>() : null;
            if (!view) return false;
            Physics.SyncTransforms();

            CombatEntity exactEntity = null;
            if (Physics.Raycast(view.ScreenPointToRay(pointerPosition), out var exactHit, 500f, ~0, QueryTriggerInteraction.Ignore))
            {
                exactEntity = exactHit.collider.GetComponentInParent<CombatEntity>();
                if (exactEntity && !IsRegisteredAndSelectable(exactEntity)) return false;
            }

            selectionCandidates.Clear();
            for (var index = registered.Count - 1; index >= 0; index--)
            {
                var entity = registered[index];
                if (!entity)
                {
                    registered.RemoveAt(index);
                    continue;
                }

                var point = SelectionPoint(entity);
                var projected = view.WorldToScreenPoint(point);
                var exact = entity == exactEntity;
                var visible = exact || projected.z > 0 && view.pixelRect.Contains(new Vector2(projected.x, projected.y));
                var occluded = !exact && (!visible || !HasLineOfSight(view, entity, point));
                selectionCandidates.Add(new KeeperSelectionCandidate(entity.SelectionIdentity,
                    new Vector2(projected.x, projected.y), exact, true, entity.IsPossessable,
                    entity.Health != null && !entity.Health.IsDead, visible, occluded));
            }

            if (!KeeperSelectionResolver.TryResolve(selectionCandidates, pointerPosition, screenSize, out var stableId)) return false;
            for (var index = 0; index < registered.Count; index++)
            {
                var entity = registered[index];
                if (!entity || entity.SelectionIdentity != stableId) continue;
                Select(entity);
                return Selected == entity;
            }
            return false;
        }

        static Vector3 SelectionPoint(CombatEntity entity)
        {
            return entity.Motor && entity.Motor.enabled
                ? entity.Motor.bounds.center
                : entity.transform.position + Vector3.up;
        }

        static bool HasLineOfSight(Camera view, CombatEntity entity, Vector3 point)
        {
            var delta = point - view.transform.position;
            var distance = delta.magnitude;
            if (distance <= .001f) return true;
            if (!Physics.Raycast(view.transform.position, delta / distance, out var hit, distance + .05f, ~0, QueryTriggerInteraction.Ignore)) return false;
            return hit.collider.GetComponentInParent<CombatEntity>() == entity;
        }

        static Vector2 CurrentScreenSize() => new(Screen.width, Screen.height);

        static int UiPointerId(Touchscreen touchscreen, int touchId)
        {
            unchecked { return (touchscreen.deviceId << 24) + touchId; }
        }

        bool IsUiOwned(int pointerId, Vector2 position)
        {
            if (GameplayInput.HasUiOwnership || GameplayInput.IsUiOwned(pointerId)) return true;
            var eventSystem = EventSystem.current;
            if (eventSystem == null) return false;
            if (eventSystem.IsPointerOverGameObject(pointerId)) return true;
            uiRaycastResults.Clear();
            eventSystem.RaycastAll(new PointerEventData(eventSystem) { pointerId = pointerId, position = position }, uiRaycastResults);
            for (var index = 0; index < uiRaycastResults.Count; index++)
                if (uiRaycastResults[index].gameObject && uiRaycastResults[index].gameObject.GetComponentInParent<Canvas>()) return true;
            return false;
        }
        void Pulse(CombatEntity entity) { ClearPulse(); pulseRoutine = StartCoroutine(PulseRoutine(entity)); }
        System.Collections.IEnumerator PulseRoutine(CombatEntity entity) { possessionPulse = GameObject.CreatePrimitive(PrimitiveType.Sphere); possessionPulse.name = "Possession Pulse"; possessionPulse.transform.position = entity.transform.position + Vector3.up * 1.2f; var collider = possessionPulse.GetComponent<Collider>(); if (collider) collider.enabled = false; possessionPulse.GetComponent<Renderer>().material.color = new Color(.7f, 1f, .2f, .55f); for (float t = 0; t < .55f; t += Time.unscaledDeltaTime) { if (!possessionPulse) yield break; possessionPulse.transform.localScale = Vector3.one * Mathf.Lerp(.3f, 3.5f, t / .55f); yield return null; } ClearPulse(); }
        void ClearPulse() { if (pulseRoutine != null) StopCoroutine(pulseRoutine); pulseRoutine = null; if (possessionPulse) Destroy(possessionPulse); possessionPulse = null; }
        void StartSlowBeat() { RestoreTime(); slowBeat = StartCoroutine(SlowBeat()); }
        System.Collections.IEnumerator SlowBeat() { normalTimeScale = Time.timeScale; normalFixedDeltaTime = Time.fixedDeltaTime; Time.timeScale = .4f; Time.fixedDeltaTime = normalFixedDeltaTime * .4f; yield return new WaitForSecondsRealtime(.35f); slowBeat = null; RestoreTime(); }
        void RestoreTime() { if (slowBeat != null) StopCoroutine(slowBeat); slowBeat = null; Time.timeScale = normalTimeScale; Time.fixedDeltaTime = normalFixedDeltaTime; }
        void OnDisable() { keeperPresses.Clear(); ClearSelected(false); ClearPossessionArrival(Possessed); }
        void OnApplicationFocus(bool focus) { if (!focus) keeperPresses.Clear(); }
        void OnDestroy()
        {
            foreach (var pair in registeredDeaths)
                if (pair.Key && pair.Key.Health != null) pair.Key.Health.Died -= pair.Value;
            registeredDeaths.Clear(); registered.Clear(); keeperPresses.Clear(); selectionCandidates.Clear(); uiRaycastResults.Clear();
            ClearSelection(); ClearPossessionArrival(Possessed); ClearPulse(); RestoreTime();
        }
    }
}
