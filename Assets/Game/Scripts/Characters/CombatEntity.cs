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
        static long nextSelectionIdentity;
        public const float DodgeDistance = 2.6f;
        public const float DodgeDuration = .18f;
        public const float DodgeImmunityDuration = .18f;
        public const float DodgeCooldown = 1.5f;
        public const float JumpCoyoteSeconds = .10f;
        public const float JumpBufferSeconds = .08f;
        long selectionIdentity;
        public CharacterDefinition Definition { get; private set; }
        public Health Health { get; private set; }
        public CombatStats Stats => Definition.Stats;
        public bool IsPossessable => Definition && Definition.Possessable && !Health.IsDead;
        public IEntityController ActiveController { get; private set; }
        public CharacterController Motor { get; private set; }
        public IReadOnlyList<AbilityRuntime> Abilities => abilities;
        readonly List<AbilityRuntime> abilities = new();
        readonly CombatActionState action = new();
        readonly CharacterJumpState jump = new();
        public event System.Action<CombatPresentationFact> PresentationChanged;
        long presentationActionId;
        Vector3 presentationDirection;
        Quaternion presentationFacing;
        AbilityDefinition presentationAbility;
        float presentationWindup;
        bool presentationTerminal;
        IEntityController[] controllers;
        CombatFeedback feedback;
        Coroutine actionRoutine;
        Coroutine dodgeRoutine;
        bool isDodging;
        float dodgeReadyAt;
        float rootedUntil;
        float lastGroundedAt = float.NegativeInfinity;
        float pendingJumpUntil = float.NegativeInfinity;
        bool fallingJumpRequestUsed;
        public bool IsRooted => Time.time < rootedUntil;
        public CombatActionPhase ActionPhase => action.Phase;
        public bool IsActionResolving => action.IsResolving;
        public bool IsDodging => isDodging;
        public bool IsJumping => jump.IsActive;
        public bool IsGrounded => Motor && Motor.enabled && Motor.isGrounded;
        public float DodgeCooldownRemaining => Mathf.Max(0, dodgeReadyAt - Time.time);
        public bool CanJump => HasDirectJumpAuthority && HasJumpGrounding && !jump.IsActive;
        public bool CanDodge => Health != null && Motor && Motor.enabled && !Health.IsDead && !IsRooted && !isDodging && !jump.IsActive && !action.IsResolving && DodgeCooldownRemaining <= 0 && !GameplayInput.TerminalState && ActiveController is PlayerController player && player.IsActive;
        public long SelectionIdentity
        {
            get
            {
                if (selectionIdentity == 0) selectionIdentity = ++nextSelectionIdentity;
                return selectionIdentity;
            }
        }

        bool HasJumpGrounding
        {
            get
            {
                if (!Motor || !Motor.enabled) return false;
                if (Motor.isGrounded) return true;
                var elapsed = Time.time - lastGroundedAt;
                return lastGroundedAt >= 0 && elapsed >= 0 && elapsed < JumpCoyoteSeconds;
            }
        }

        bool HasDirectJumpAuthority => Health != null && !Health.IsDead && Motor && Motor.enabled && !IsRooted && !isDodging && !action.IsResolving && !GameplayInput.TerminalState && ActiveController is PlayerController player && player.IsActive && player.isActiveAndEnabled;
        bool HasPendingJump => pendingJumpUntil > Time.time;
        bool CanBufferFallingJump => !fallingJumpRequestUsed && !HasPendingJump && HasDirectJumpAuthority && !Motor.isGrounded && jump.IsActive && jump.VerticalVelocity <= 0;

        void Awake() => _ = SelectionIdentity;

        public void Initialize(CharacterDefinition definition)
        {
            Definition = definition;
            Health = GetComponent<Health>();
            Motor = GetComponent<CharacterController>();
            Health.Initialize(definition.Stats.MaxHealth);
            Health.Died += OnDeath;
            var assembler = GetComponent<CharacterVisualAssembler>() ?? gameObject.AddComponent<CharacterVisualAssembler>();
            assembler.Assemble(definition.VisualRecipe);
            feedback = GetComponent<CombatFeedback>() ?? gameObject.AddComponent<CombatFeedback>();
            abilities.Clear();
            if (definition.Abilities != null)
                foreach (var item in definition.Abilities) if (item) abilities.Add(new AbilityRuntime(item));
            controllers = GetComponents<IEntityController>();
        }

        public void SetController(IEntityController next)
        {
            if (controllers == null) controllers = GetComponents<IEntityController>();
            if (ActiveController != null && ActiveController != next) { CancelActionPresentation(CombatPresentationEnd.ControllerChanged); CancelDodge(); CancelJump(); }
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

        void Update()
        {
            if (GameplayInput.TerminalState && !presentationTerminal) { PublishPresentation(CombatPresentationEnd.Terminal); feedback?.ClearDodgeConfirmation(); feedback?.ClearNoHitConfirmation(); feedback?.ClearDefeatConfirmation(); }
            presentationTerminal = GameplayInput.TerminalState;
            if (GameplayInput.TerminalState && (isDodging || Health.IsDamageImmune)) CancelDodge();
            if (GameplayInput.TerminalState || !Motor || !Motor.enabled) CancelJump();
            if (!Health.IsDead) ActiveController?.Tick();
        }

        public bool TryUse(int index, Vector3 direction)
        {
            if (Health.IsDead || jump.IsActive || isDodging || index < 0 || index >= abilities.Count || !action.TryBegin()) return false;
            if (!abilities[index].TryConsume()) { action.Complete(); return false; }
            direction.y = 0;
            if (direction.sqrMagnitude <= .01f) { direction = transform.forward; direction.y = 0; }
            if (direction.sqrMagnitude <= .01f) direction = Vector3.forward;
            actionRoutine = StartCoroutine(Execute(abilities[index].Definition, direction.normalized));
            return true;
        }

        IEnumerator Execute(AbilityDefinition ability, Vector3 direction)
        {
            presentationActionId++;
            presentationDirection = direction;
            presentationFacing = transform.rotation;
            presentationAbility = ability;
            presentationWindup = ability.Windup;
            transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            PublishPresentation();
            feedback.ShowTelegraph(ability, direction);
            yield return new WaitForSeconds(ability.Windup);
            feedback.ClearTelegraph();
            if (Health.IsDead) { action.Complete(); PublishPresentation(CombatPresentationEnd.Death); yield break; }
            action.Impact();
            PublishPresentation();
            if (ability.Kind == AbilityKind.Dash)
            {
                float moved = 0;
                while (moved < ability.DashDistance)
                {
                    var step = Mathf.Min(ability.DashDistance - moved, 16 * Time.deltaTime);
                    Motor.Move(direction * step); moved += step; yield return null;
                }
            }
            var center = transform.position + direction * Mathf.Max(1, ability.Range * .55f);
            bool connected = false, eligibleContact = false;
            var damaged = new HashSet<CombatEntity>();
            foreach (var hit in Physics.OverlapSphere(center, ability.Radius, ~0, QueryTriggerInteraction.Ignore))
            {
                var target = hit.GetComponentInParent<CombatEntity>();
                if (target && target != this && target.Health != null && !target.Health.IsDead && damaged.Add(target))
                {
                    eligibleContact = true;
                    var damage = ability.Damage + Stats.AbilityPower * .25f;
                    var point = hit.ClosestPoint(center);
                    var targetWasAlive = !target.Health.IsDead;
                    var applied = target.Health.TakeDamage(new DamageInfo(damage, gameObject, point), target.Stats.Armor);
                    if (applied)
                    {
                        target.feedback?.ShowHit(damage, point, transform.position);
                        connected = true;
                        var directControl = ActiveController is PlayerController player && player.IsActive && player.isActiveAndEnabled;
                        if (CombatFeedback.ShouldShowDefeat(ability.Kind, damage, true, targetWasAlive, target.Health.IsDead,
                            true, directControl, Health != null && !Health.IsDead, GameplayInput.TerminalState,
                            target.Definition ? target.Definition.DisplayName : null))
                            feedback?.ShowDefeatConfirmation(target, point);
                    }
                    else if (target.Health.IsDamageImmune && target.ActiveController is PlayerController player && player.IsActive && player.isActiveAndEnabled)
                        target.feedback?.ShowDodgeConfirmation(point);
                }
            }
            if (connected) feedback.ShowImpact();
            else
            {
                var directControl = ActiveController is PlayerController player && player.IsActive && player.isActiveAndEnabled;
                if (CombatFeedback.ShouldShowNoHit(ability.Kind, ability.Damage, eligibleContact, connected,
                    directControl, Health != null && !Health.IsDead, GameplayInput.TerminalState))
                    feedback.ShowNoHitConfirmation();
            }
            action.Recover();
            PublishPresentation();
            yield return new WaitForSecondsRealtime(ability.Recovery);
            action.Complete(); actionRoutine = null;
            PublishPresentation(CombatPresentationEnd.Completed);
        }

        public bool TryDodge(Vector3 direction)
        {
            if (!CanDodge) return false;
            direction.y = 0;
            if (direction.sqrMagnitude <= .01f) { direction = transform.forward; direction.y = 0; }
            if (direction.sqrMagnitude <= .01f) direction = Vector3.forward;
            direction.Normalize();
            transform.rotation = Quaternion.LookRotation(direction);
            isDodging = true;
            dodgeReadyAt = Time.time + DodgeCooldown;
            Health.BeginDamageImmunity(DodgeImmunityDuration);
            dodgeRoutine = StartCoroutine(ExecuteDodge(direction));
            return true;
        }

        public bool TryJump()
        {
            if (CanJump)
            {
                ClearPendingJump();
                if (!jump.TryBegin(true)) return false;
                fallingJumpRequestUsed = false;
                return true;
            }

            if (!CanBufferFallingJump) return false;
            pendingJumpUntil = Time.time + JumpBufferSeconds;
            fallingJumpRequestUsed = true;
            return true;
        }

        IEnumerator ExecuteDodge(Vector3 direction)
        {
            var elapsed = 0f;
            var moved = 0f;
            while (elapsed < DodgeDuration && isDodging)
            {
                var stepTime = Mathf.Min(Time.deltaTime, DodgeDuration - elapsed);
                var step = Mathf.Min(DodgeDistance - moved, DodgeDistance / DodgeDuration * stepTime);
                if (step > 0 && Motor && Motor.enabled) Motor.Move(direction * step);
                moved += step;
                elapsed += stepTime;
                yield return null;
            }
            FinishDodge(true);
        }

        public void Move(Vector3 velocity)
        {
            if (isDodging || !Motor || !Motor.enabled) return;
            if (Time.time < rootedUntil) velocity = Vector3.zero;
            velocity.y = 0;
            if (velocity.sqrMagnitude > .01f && !action.IsResolving) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(velocity), 15 * Time.deltaTime);
            var gravity = jump.IsActive ? Vector3.up * jump.Step(Time.deltaTime) : Physics.gravity;
            Motor.Move((velocity + gravity) * Time.deltaTime);
            var grounded = Motor.isGrounded;
            if (grounded) lastGroundedAt = Time.time;
            jump.ObserveGrounded(grounded);
            if (grounded) ConsumePendingJump();
            else if (!HasPendingJump) ClearPendingJump();
        }

        public void ApplyRoot(float seconds)
        {
            rootedUntil = Mathf.Max(rootedUntil, Time.time + Mathf.Max(0, seconds));
            if (IsRooted) { CancelDodge(false); CancelJump(); }
        }
        public void BreakRoot() => rootedUntil = 0;

        void OnDeath()
        {
            rootedUntil = 0; CancelActionPresentation(CombatPresentationEnd.Death); CancelDodge(); CancelJump();
            Controller<PlayerController>()?.ResetEscapeState();
            GetComponent<CharacterVisualMotion>()?.StartDefeat();
            Motor.enabled = false;
        }

        void CancelActionPresentation(CombatPresentationEnd reason)
        {
            action.Complete(); if (actionRoutine != null) StopCoroutine(actionRoutine); actionRoutine = null; feedback?.Cleanup();
            PublishPresentation(reason);
        }

        void PublishPresentation(CombatPresentationEnd end = CombatPresentationEnd.None)
        {
            PresentationChanged?.Invoke(new CombatPresentationFact(presentationActionId, action.Phase, end,
                presentationDirection, presentationFacing, presentationAbility, presentationWindup, Time.time, Time.unscaledTime));
        }

        void CancelDodge(bool clearImmunity = true)
        {
            if (dodgeRoutine != null) StopCoroutine(dodgeRoutine);
            FinishDodge(clearImmunity);
        }

        void FinishDodge(bool clearImmunity)
        {
            dodgeRoutine = null;
            isDodging = false;
            if (clearImmunity && Health) Health.ClearDamageImmunity();
        }

        internal void CancelJump()
        {
            jump.Cancel();
            lastGroundedAt = float.NegativeInfinity;
            ClearPendingJump();
            fallingJumpRequestUsed = false;
        }

        void ConsumePendingJump()
        {
            if (!HasPendingJump)
            {
                ClearPendingJump();
                return;
            }

            if (!CanJump)
            {
                ClearPendingJump();
                return;
            }

            ClearPendingJump();
            if (jump.TryBegin(true)) fallingJumpRequestUsed = false;
        }

        void ClearPendingJump() => pendingJumpUntil = float.NegativeInfinity;

        void OnDisable() { PublishPresentation(CombatPresentationEnd.Disabled); feedback?.ClearDodgeConfirmation(); feedback?.ClearNoHitConfirmation(); feedback?.ClearDefeatConfirmation(); CancelDodge(); CancelJump(); }
        void OnDestroy()
        {
            PublishPresentation(CombatPresentationEnd.Destroyed);
            if (Health != null) Health.Died -= OnDeath;
            feedback?.ClearNoHitConfirmation();
            feedback?.ClearDefeatConfirmation();
            CancelDodge();
            CancelJump();
        }
    }
}
