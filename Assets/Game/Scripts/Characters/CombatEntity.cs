using System;
using System.Collections;
using System.Collections.Generic;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using UnityEngine;

namespace RealmRaiders.Characters
{
    [RequireComponent(typeof(CharacterController), typeof(Health))]
    public sealed class CombatEntity : MonoBehaviour
    {
        public event Action<CombatEntity> Selected;
        public CharacterDefinition Definition { get; private set; }
        public Health Health { get; private set; }
        public CombatStats Stats => Definition.Stats;
        public bool IsPossessable => Definition && Definition.Possessable && !Health.IsDead;
        public IEntityController ActiveController { get; private set; }
        public CharacterController Motor { get; private set; }
        public IReadOnlyList<AbilityRuntime> Abilities => abilities;
        readonly List<AbilityRuntime> abilities = new();
        IEntityController[] controllers;
        float rootedUntil;
        public bool IsRooted => Time.time < rootedUntil;

        public void Initialize(CharacterDefinition definition)
        {
            Definition = definition;
            Health = GetComponent<Health>();
            Motor = GetComponent<CharacterController>();
            Health.Initialize(definition.Stats.MaxHealth);
            Health.Died += OnDeath;
            abilities.Clear();
            if (definition.Abilities != null)
                foreach (var item in definition.Abilities) if (item) abilities.Add(new AbilityRuntime(item));
            controllers = GetComponents<IEntityController>();
        }

        public void SetController(IEntityController next)
        {
            if (controllers == null) controllers = GetComponents<IEntityController>();
            foreach (var controller in controllers) controller.SetControl(controller == next);
            ActiveController = next;
        }

        public T Controller<T>() where T : class, IEntityController
        {
            if (controllers == null) controllers = GetComponents<IEntityController>();
            foreach (var controller in controllers) if (controller is T match) return match;
            return null;
        }

        public void RefreshControllers() => controllers = GetComponents<IEntityController>();

        void Update() { if (!Health.IsDead) ActiveController?.Tick(); }

        public bool TryUse(int index, Vector3 direction)
        {
            if (index < 0 || index >= abilities.Count || !abilities[index].TryConsume()) return false;
            StartCoroutine(Execute(abilities[index].Definition, direction.sqrMagnitude > .01f ? direction.normalized : transform.forward));
            return true;
        }

        IEnumerator Execute(AbilityDefinition ability, Vector3 direction)
        {
            transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            yield return new WaitForSeconds(ability.Windup);
            if (Health.IsDead) yield break;
            if (ability.Kind == AbilityKind.Dash)
            {
                float moved = 0;
                while (moved < ability.DashDistance)
                {
                    var step = Mathf.Min(ability.DashDistance - moved, 16 * Time.deltaTime);
                    Motor.Move(transform.forward * step); moved += step; yield return null;
                }
            }
            var center = transform.position + transform.forward * Mathf.Max(1, ability.Range * .55f);
            foreach (var hit in Physics.OverlapSphere(center, ability.Radius, ~0, QueryTriggerInteraction.Ignore))
            {
                var target = hit.GetComponentInParent<CombatEntity>();
                if (target && target != this && !target.Health.IsDead)
                    target.Health.TakeDamage(new DamageInfo(ability.Damage + Stats.AbilityPower * .25f, gameObject, hit.ClosestPoint(center)), target.Stats.Armor);
            }
        }

        public void Move(Vector3 velocity)
        {
            if (Time.time < rootedUntil) velocity = Vector3.zero;
            if (velocity.sqrMagnitude > .01f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(velocity), 15 * Time.deltaTime);
            Motor.Move((velocity + Physics.gravity) * Time.deltaTime);
        }

        public void ApplyRoot(float seconds) => rootedUntil = Mathf.Max(rootedUntil, Time.time + Mathf.Max(0, seconds));
        public void BreakRoot() => rootedUntil = 0;

        void OnMouseDown() => Selected?.Invoke(this);
        void OnDeath() { rootedUntil = 0; Controller<PlayerController>()?.ResetEscapeState(); Motor.enabled = false; transform.localScale *= .75f; }
    }
}
