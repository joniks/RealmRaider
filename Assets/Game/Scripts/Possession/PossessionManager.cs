using System;
using RealmRaiders.AI;
using RealmRaiders.CameraSystem;
using RealmRaiders.Characters;
using RealmRaiders.Controllers;
using UnityEngine;

namespace RealmRaiders.Possession
{
    public sealed class PossessionManager : MonoBehaviour
    {
        public event Action<CombatEntity> SelectionChanged;
        public event Action<CombatEntity> PossessionChanged;
        public event Action<bool> Released;
        public event Action<string> MomentFeedback;
        public CombatEntity Selected { get; private set; }
        public CombatEntity Possessed { get; private set; }
        public bool IsPossessing => Possessed;
        PrototypeCameraRig cameraRig;
        PossessionEnergy energy;
        GameObject selectionVisual;
        GameObject possessionPulse;
        Coroutine pulseRoutine;
        Coroutine slowBeat;
        float normalTimeScale = 1, normalFixedDeltaTime = .02f;

        public void Initialize(PrototypeCameraRig rig) => cameraRig = rig;
        public void ConfigureEnergy(PossessionEnergy value) => energy = value;
        public void Register(CombatEntity entity) { entity.Selected += Select; entity.Health.Died += () => { if (Selected == entity) { ClearSelection(); Selected = null; SelectionChanged?.Invoke(null); } }; }

        void Update()
        {
            if (Possessed && energy != null && !energy.Consume(Time.deltaTime)) Release(true, "POSSESSION ENERGY DEPLETED — RETURNING TO KEEPER");
        }
        public void Select(CombatEntity entity)
        {
            if (IsPossessing || !entity || !entity.IsPossessable) return;
            Selected = entity; ShowSelection(entity); SelectionChanged?.Invoke(Selected);
        }

        public bool PossessSelected()
        {
            if (!Selected || !Selected.IsPossessable || IsPossessing || (energy != null && energy.IsDepleted)) return false;
            var player = Selected.Controller<PlayerController>();
            if (player == null) return false;
            Possessed = Selected;
            ClearSelection();
            Possessed.SetController(player);
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

        void OnPossessedDied() => Release(true, "POSSESSION ENDED — RETURNING TO KEEPER");
        public void Release() => Release(false);
        public void Release(bool forced) => Release(forced, forced ? "POSSESSION ENDED — RETURNING TO KEEPER" : "RELEASED — KEEPER OVERVIEW");
        void Release(bool forced, string feedback)
        {
            if (!Possessed) return;
            var released = Possessed;
            released.Health.Died -= OnPossessedDied;
            var ai = released.Controller<CreatureBrain>();
            if (ai != null && !released.Health.IsDead) released.SetController(ai);
            else released.SetController(null);
            Possessed = null; Selected = null;
            ClearSelection(); ClearPulse();
            RestoreTime();
            cameraRig.TransitionTo(null, CameraMode.KeeperOverview);
            SelectionChanged?.Invoke(null); PossessionChanged?.Invoke(null);
            Released?.Invoke(forced);
            MomentFeedback?.Invoke(feedback);
        }

        void ShowSelection(CombatEntity entity)
        {
            ClearSelection(); selectionVisual = new GameObject("Possession Selection Presentation", typeof(PossessionSelectionPresentation)); selectionVisual.transform.SetParent(entity.transform, false);
            selectionVisual.GetComponent<PossessionSelectionPresentation>().Initialize(entity.Definition.DisplayName);
        }
        void ClearSelection() { if (selectionVisual) { selectionVisual.SetActive(false); Destroy(selectionVisual); } selectionVisual = null; }
        void Pulse(CombatEntity entity) { ClearPulse(); pulseRoutine = StartCoroutine(PulseRoutine(entity)); }
        System.Collections.IEnumerator PulseRoutine(CombatEntity entity) { possessionPulse = GameObject.CreatePrimitive(PrimitiveType.Sphere); possessionPulse.name = "Possession Pulse"; possessionPulse.transform.position = entity.transform.position + Vector3.up * 1.2f; var collider = possessionPulse.GetComponent<Collider>(); if (collider) collider.enabled = false; possessionPulse.GetComponent<Renderer>().material.color = new Color(.7f, 1f, .2f, .55f); for (float t = 0; t < .55f; t += Time.unscaledDeltaTime) { if (!possessionPulse) yield break; possessionPulse.transform.localScale = Vector3.one * Mathf.Lerp(.3f, 3.5f, t / .55f); yield return null; } ClearPulse(); }
        void ClearPulse() { if (pulseRoutine != null) StopCoroutine(pulseRoutine); pulseRoutine = null; if (possessionPulse) Destroy(possessionPulse); possessionPulse = null; }
        void StartSlowBeat() { RestoreTime(); slowBeat = StartCoroutine(SlowBeat()); }
        System.Collections.IEnumerator SlowBeat() { normalTimeScale = Time.timeScale; normalFixedDeltaTime = Time.fixedDeltaTime; Time.timeScale = .4f; Time.fixedDeltaTime = normalFixedDeltaTime * .4f; yield return new WaitForSecondsRealtime(.35f); slowBeat = null; RestoreTime(); }
        void RestoreTime() { if (slowBeat != null) StopCoroutine(slowBeat); slowBeat = null; Time.timeScale = normalTimeScale; Time.fixedDeltaTime = normalFixedDeltaTime; }
        void OnDestroy() { ClearSelection(); ClearPulse(); RestoreTime(); }
    }
}
