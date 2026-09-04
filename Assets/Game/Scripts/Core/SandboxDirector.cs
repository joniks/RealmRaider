using RealmRaiders.AI;
using RealmRaiders.CameraSystem;
using RealmRaiders.Characters;
using RealmRaiders.Controllers;
using RealmRaiders.Possession;
using UnityEngine;

namespace RealmRaiders.Core
{
    public enum SandboxMode { Keeper, Hero, PossessedEnt }

    public sealed class SandboxDirector : MonoBehaviour
    {
        public SandboxMode Mode { get; private set; }
        CombatEntity hero, ent;
        PossessionManager possession;
        PrototypeCameraRig cameraRig;

        public void Initialize(CombatEntity heroEntity, CombatEntity entEntity, PossessionManager manager, PrototypeCameraRig rig)
        { hero = heroEntity; ent = entEntity; possession = manager; cameraRig = rig; manager.PossessionChanged += OnPossession; EnterKeeper(); }

        public void EnterHero()
        {
            possession.Release(); Mode = SandboxMode.Hero;
            hero.SetController(hero.Controller<PlayerController>());
            if (!ent.Health.IsDead) ent.SetController(ent.Controller<CreatureBrain>());
            cameraRig.TransitionTo(hero, CameraMode.HeroCombat);
        }

        public void EnterKeeper()
        {
            if (possession.IsPossessing) possession.Release();
            Mode = SandboxMode.Keeper;
            if (!hero.Health.IsDead) hero.SetController(hero.Controller<CreatureBrain>());
            if (!ent.Health.IsDead) ent.SetController(ent.Controller<CreatureBrain>());
            cameraRig.TransitionTo(null, CameraMode.KeeperOverview);
        }

        void OnPossession(CombatEntity value)
        {
            if (value) { Mode = SandboxMode.PossessedEnt; if (!hero.Health.IsDead) hero.SetController(hero.Controller<CreatureBrain>()); }
            else if (Mode == SandboxMode.PossessedEnt) Mode = SandboxMode.Keeper;
        }
    }
}
