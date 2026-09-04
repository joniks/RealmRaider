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
        public CombatEntity Selected { get; private set; }
        public CombatEntity Possessed { get; private set; }
        public bool IsPossessing => Possessed;
        PrototypeCameraRig cameraRig;
        PossessionEnergy energy;

        public void Initialize(PrototypeCameraRig rig) => cameraRig = rig;
        public void ConfigureEnergy(PossessionEnergy value) => energy = value;
        public void Register(CombatEntity entity) => entity.Selected += Select;

        void Update()
        {
            if (Possessed && energy != null && !energy.Consume(Time.deltaTime)) Release();
        }
        public void Select(CombatEntity entity)
        {
            if (IsPossessing || !entity || !entity.IsPossessable) return;
            Selected = entity; SelectionChanged?.Invoke(Selected);
        }

        public bool PossessSelected()
        {
            if (!Selected || !Selected.IsPossessable || IsPossessing || (energy != null && energy.IsDepleted)) return false;
            var player = Selected.Controller<PlayerController>();
            if (player == null) return false;
            Possessed = Selected;
            Possessed.SetController(player);
            Possessed.Health.Died += Release;
            cameraRig.TransitionTo(Possessed, CameraMode.PossessedCreature);
            PossessionChanged?.Invoke(Possessed);
            return true;
        }

        public void Release()
        {
            if (!Possessed) return;
            Possessed.Health.Died -= Release;
            var ai = Possessed.Controller<CreatureBrain>();
            if (ai != null && !Possessed.Health.IsDead) Possessed.SetController(ai);
            Possessed = null; Selected = null;
            cameraRig.TransitionTo(null, CameraMode.KeeperOverview);
            SelectionChanged?.Invoke(null); PossessionChanged?.Invoke(null);
        }
    }
}
