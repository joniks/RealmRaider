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
        public CombatEntity Selected { get; private set; }
        public CombatEntity Possessed { get; private set; }
        public bool IsPossessing => Possessed;
        PrototypeCameraRig cameraRig;
        PossessionEnergy energy;
        GameObject selectionVisual;
        Coroutine slowBeat;
        float normalTimeScale = 1, normalFixedDeltaTime = .02f;

        public void Initialize(PrototypeCameraRig rig) => cameraRig = rig;
        public void ConfigureEnergy(PossessionEnergy value) => energy = value;
        public void Register(CombatEntity entity) { entity.Selected += Select; entity.Health.Died += () => { if (Selected == entity) { ClearSelection(); Selected = null; SelectionChanged?.Invoke(null); } }; }

        void Update()
        {
            if (Possessed && energy != null && !energy.Consume(Time.deltaTime)) Release(true);
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
            return true;
        }

        void OnPossessedDied() => Release(true);
        public void Release() => Release(false);
        public void Release(bool forced)
        {
            if (!Possessed) return;
            Possessed.Health.Died -= OnPossessedDied;
            var ai = Possessed.Controller<CreatureBrain>();
            if (ai != null && !Possessed.Health.IsDead) Possessed.SetController(ai);
            Possessed = null; Selected = null;
            RestoreTime();
            cameraRig.TransitionTo(null, CameraMode.KeeperOverview);
            SelectionChanged?.Invoke(null); PossessionChanged?.Invoke(null);
            Released?.Invoke(forced);
        }

        void ShowSelection(CombatEntity entity)
        {
            ClearSelection(); selectionVisual = new GameObject("Possession Selection Presentation"); selectionVisual.transform.SetParent(entity.transform, false);
            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder); ring.name = "Possess Selection Ring"; ring.transform.SetParent(selectionVisual.transform, false); ring.transform.localPosition = new Vector3(0, -.9f, 0); ring.transform.localScale = new Vector3(2.1f, .035f, 2.1f); var collider = ring.GetComponent<Collider>(); if (collider) collider.isTrigger = true; ring.GetComponent<Renderer>().material.color = new Color(.85f, 1f, .2f, .9f);
            var label = new GameObject("Possess Selection Label", typeof(TextMesh)); label.transform.SetParent(selectionVisual.transform, false); label.transform.localPosition = Vector3.up * 3.2f; var text = label.GetComponent<TextMesh>(); text.text = "SELECTED — PRESS POSSESS"; text.characterSize = .11f; text.fontSize = 52; text.anchor = TextAnchor.MiddleCenter; text.color = new Color(.9f, 1f, .3f);
        }
        void ClearSelection() { if (selectionVisual) Destroy(selectionVisual); selectionVisual = null; }
        void Pulse(CombatEntity entity) { StartCoroutine(PulseRoutine(entity)); }
        System.Collections.IEnumerator PulseRoutine(CombatEntity entity) { var pulse = GameObject.CreatePrimitive(PrimitiveType.Sphere); pulse.name = "Possession Pulse"; pulse.transform.position = entity.transform.position + Vector3.up * 1.2f; pulse.GetComponent<Collider>().isTrigger = true; pulse.GetComponent<Renderer>().material.color = new Color(.7f, 1f, .2f, .55f); for (float t = 0; t < .55f; t += Time.unscaledDeltaTime) { pulse.transform.localScale = Vector3.one * Mathf.Lerp(.3f, 3.5f, t / .55f); yield return null; } Destroy(pulse); }
        void StartSlowBeat() { RestoreTime(); slowBeat = StartCoroutine(SlowBeat()); }
        System.Collections.IEnumerator SlowBeat() { normalTimeScale = Time.timeScale; normalFixedDeltaTime = Time.fixedDeltaTime; Time.timeScale = .4f; Time.fixedDeltaTime = normalFixedDeltaTime * .4f; yield return new WaitForSecondsRealtime(.35f); slowBeat = null; RestoreTime(); }
        void RestoreTime() { if (slowBeat != null) StopCoroutine(slowBeat); slowBeat = null; Time.timeScale = normalTimeScale; Time.fixedDeltaTime = normalFixedDeltaTime; }
        void OnDestroy() { ClearSelection(); RestoreTime(); }
    }
}
