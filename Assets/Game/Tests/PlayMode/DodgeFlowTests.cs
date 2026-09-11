using System.Collections;
using NUnit.Framework;
using RealmRaiders.AI;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using RealmRaiders.Possession;
using RealmRaiders.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RealmRaiders.Tests
{
    public sealed class DodgeFlowTests
    {
        [UnityTearDown]
        public IEnumerator ResetTransientState()
        {
            GameplayInput.ResetForTests();
            Time.timeScale = 1;
            var active = SceneManager.GetActiveScene();
            if (active.IsValid() && (active.name == "SylvanRealm" || active.name == "DefenderTest"))
            {
                var cleanup = SceneManager.CreateScene("DodgeFlowTests Cleanup");
                SceneManager.SetActiveScene(cleanup);
                yield return SceneManager.UnloadSceneAsync(active);
            }
            else yield return null;
        }

        [UnityTest]
        public IEnumerator DirectPlayerDodge_UsesMovementIntentAndHasANarrowDamageWindow()
        {
            GameplayInput.ResetForTests();
            Time.timeScale = 1;
            var cameraObject = MainCamera();
            var fixture = new EntityFixture(false);
            try
            {
                fixture.Entity.SetController(fixture.Player);
                GameplayInput.SetMovement(Vector2.right);
                fixture.Player.Tick();
                GameplayInput.ClearMovement();
                var start = fixture.Entity.transform.position;

                Assert.That(fixture.Player.Dodge(), Is.True);
                Assert.That(fixture.Entity.IsDodging, Is.True);
                Assert.That(fixture.Entity.Health.IsDamageImmune, Is.True);
                Assert.That(fixture.Entity.Health.DamageImmunityRemaining, Is.LessThanOrEqualTo(CombatEntity.DodgeImmunityDuration + .0001f));
                fixture.Entity.Health.TakeDamage(new DamageInfo(25, null, fixture.Entity.transform.position), 0);
                Assert.That(fixture.Entity.Health.Current, Is.EqualTo(100));

                yield return new WaitForSeconds(CombatEntity.DodgeImmunityDuration + .08f);
                var displacement = fixture.Entity.transform.position - start;
                Assert.That(fixture.Entity.IsDodging, Is.False);
                Assert.That(fixture.Entity.Health.IsDamageImmune, Is.False);
                Assert.That(displacement.x, Is.GreaterThan(2.4f).And.LessThanOrEqualTo(CombatEntity.DodgeDistance + .05f));
                Assert.That(Mathf.Abs(displacement.z), Is.LessThan(.05f));
                fixture.Entity.Health.TakeDamage(new DamageInfo(25, null, fixture.Entity.transform.position), 0);
                Assert.That(fixture.Entity.Health.Current, Is.EqualTo(75));
                Assert.That(fixture.Player.Dodge(), Is.False, "Cooldown must reject an immediate second dodge.");

                yield return new WaitForSeconds(CombatEntity.DodgeCooldown);
                Assert.That(fixture.Player.Dodge(), Is.True);
                var feedback = fixture.Root.GetComponent<CombatFeedback>(); feedback.ShowDodgeConfirmation(fixture.Entity.transform.position);
                Assert.That(feedback.DodgeConfirmationVisible, Is.True);
                GameplayInput.SetTerminalState(true);
                yield return null;
                Assert.That(fixture.Entity.IsDodging, Is.False);
                Assert.That(fixture.Entity.Health.IsDamageImmune, Is.False);
                Assert.That(feedback.DodgeConfirmationVisible, Is.False, "Terminal state clears dodge confirmation.");
            }
            finally
            {
                GameplayInput.ResetForTests();
                fixture.Dispose();
                Object.Destroy(cameraObject);
            }
        }

        [UnityTest]
        public IEnumerator SuccessfulDodge_ShowsOneTruthfulConfirmationWithoutHitFeedback()
        {
            GameplayInput.ResetForTests(); Time.timeScale = 1;
            var cameraObject = MainCamera();
            var target = new EntityFixture(false);
            var attackerObject = new GameObject("Dodge Confirmation Attacker", typeof(CharacterController), typeof(Health), typeof(CombatEntity), typeof(PlayerController));
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube); ground.name = "Dodge Confirmation Ground"; ground.transform.position = new Vector3(0, -.25f, 0); ground.transform.localScale = new Vector3(20, .5f, 20);
            var attackerDefinition = ScriptableObject.CreateInstance<CharacterDefinition>();
            var attack = ScriptableObject.CreateInstance<AbilityDefinition>();
            try
            {
                attack.DisplayName = "Confirmation Strike"; attack.Kind = AbilityKind.Area; attack.Damage = 25; attack.Range = 1; attack.Radius = 4; attack.Windup = 0; attack.Cooldown = 0;
                attackerDefinition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 1 }; attackerDefinition.Abilities = new[] { attack };
                var attacker = attackerObject.GetComponent<CombatEntity>(); attacker.Initialize(attackerDefinition); attacker.SetController(attackerObject.GetComponent<PlayerController>());
                target.Root.transform.position = new Vector3(0, 1, 0); attackerObject.transform.position = new Vector3(0, 1, -1); Physics.SyncTransforms();
                target.Entity.SetController(target.Player);
                var feedback = target.Root.GetComponent<CombatFeedback>();
                GameplayInput.SetMovement(Vector2.right); target.Player.Tick(); GameplayInput.ClearMovement();
                var dodgeStart = target.Entity.transform.position;

                Assert.That(target.Player.Dodge(), Is.True);
                Assert.That(attacker.TryUse(0, Vector3.forward), Is.True);
                var impactDeadline = Time.realtimeSinceStartup + 1;
                while (!feedback.DodgeConfirmationVisible && attacker.IsActionResolving && Time.realtimeSinceStartup < impactDeadline) yield return null;

                Assert.That(target.Entity.Health.Current, Is.EqualTo(100), "The existing dodge immunity remains authoritative.");
                Assert.That(feedback.DodgeConfirmationVisible, Is.True);
                var confirmation = GameObject.Find("Combat Dodge Confirmation");
                Assert.That(confirmation, Is.Not.Null);
                Assert.That(confirmation.GetComponent<TextMesh>().text, Is.EqualTo("DODGED"));
                Assert.That(confirmation.GetComponent<CameraFacingMarker>(), Is.Not.Null);
                Assert.That(confirmation.GetComponent<Collider>(), Is.Null);
                Assert.That(GameObject.Find("Combat Damage"), Is.Null, "An immunity-rejected hit cannot display damage feedback.");
                Assert.That(GameObject.Find("Ability Impact"), Is.Null, "An immunity-rejected hit cannot count as an attacker impact.");
                Assert.That(attacker.GetComponent<CombatFeedback>().NoHitConfirmationVisible, Is.False, "An eligible immunity contact owns DODGED without a competing attacker NO HIT.");

                feedback.ShowDodgeConfirmation(target.Entity.transform.position);
                feedback.ShowDodgeConfirmation(target.Entity.transform.position);
                Assert.That(Object.FindObjectsByType<CameraFacingMarker>(FindObjectsSortMode.None), Has.Length.EqualTo(1), "The visible confirmation window deduplicates repeated rejected hits.");

                yield return new WaitForSecondsRealtime(CombatFeedback.DodgeConfirmationDuration + .05f);
                Assert.That(feedback.DodgeConfirmationVisible, Is.False);
                Assert.That(GameObject.Find("Combat Dodge Confirmation"), Is.Null);
                var dodgeDisplacement = target.Entity.transform.position - dodgeStart;
                Assert.That(dodgeDisplacement.x, Is.GreaterThan(2.4f).And.LessThanOrEqualTo(CombatEntity.DodgeDistance + .05f));
                Assert.That(Mathf.Abs(dodgeDisplacement.z), Is.LessThan(.05f), "Rejected hit feedback cannot add its ordinary knockback.");
                Assert.That(target.Player.Dodge(), Is.False, "A failed cooldown dodge cannot create confirmation.");

                Assert.That(attacker.TryUse(0, Vector3.forward), Is.True);
                impactDeadline = Time.realtimeSinceStartup + 1;
                while (target.Entity.Health.Current == 100 && attacker.IsActionResolving && Time.realtimeSinceStartup < impactDeadline) yield return null;
                Assert.That(target.Entity.Health.Current, Is.EqualTo(75));
                Assert.That(feedback.DodgeConfirmationVisible, Is.False);
                Assert.That(GameObject.Find("Combat Damage"), Is.Not.Null, "An applied hit retains ordinary target feedback.");
                Assert.That(GameObject.Find("Ability Impact"), Is.Not.Null, "An applied hit retains ordinary attacker impact feedback.");
                Assert.That(attacker.GetComponent<CombatFeedback>().NoHitConfirmationVisible, Is.False, "Applied damage cannot also report NO HIT.");
            }
            finally
            {
                GameplayInput.ResetForTests(); target.Dispose(); Object.Destroy(cameraObject); Object.Destroy(attackerObject); Object.Destroy(ground); Object.Destroy(attackerDefinition); Object.Destroy(attack);
            }
        }

        [UnityTest]
        public IEnumerator EmptyDirectMelee_ShowsOneBoundedNoHitWithoutGameplayOrSceneArtifacts()
        {
            GameplayInput.ResetForTests(); Time.timeScale = 1;
            var cameraObject = MainCamera();
            var fixture = new EntityFixture(true);
            try
            {
                fixture.Entity.SetController(fixture.Player);
                var feedback = fixture.Root.GetComponent<CombatFeedback>();
                var rootPosition = fixture.Root.transform.position;
                var cameraPosition = cameraObject.transform.position;
                var cameraRotation = cameraObject.transform.rotation;
                var cameraCount = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Length;
                var listenerCount = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length;
                var eventSystemCount = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length;

                Assert.That(fixture.Entity.TryUse(0, Vector3.forward), Is.True);
                var deadline = Time.realtimeSinceStartup + 1;
                while (!feedback.NoHitConfirmationVisible && fixture.Entity.IsActionResolving && Time.realtimeSinceStartup < deadline) yield return null;

                Assert.That(feedback.NoHitConfirmationVisible, Is.True);
                var first = GameObject.Find("Combat No Hit Confirmation");
                Assert.That(first, Is.Not.Null);
                Assert.That(first.GetComponent<TextMesh>().text, Is.EqualTo("NO HIT"));
                Assert.That(first.GetComponent<CameraFacingMarker>(), Is.Not.Null);
                Assert.That(first.GetComponent<Collider>(), Is.Null);
                Assert.That(GameObject.Find("Combat Damage"), Is.Null);
                Assert.That(GameObject.Find("Ability Impact"), Is.Null);
                Assert.That(fixture.Root.transform.position.x, Is.EqualTo(rootPosition.x).Within(.001f));
                Assert.That(fixture.Root.transform.position.z, Is.EqualTo(rootPosition.z).Within(.001f));
                Assert.That(cameraObject.transform.position, Is.EqualTo(cameraPosition));
                Assert.That(cameraObject.transform.rotation, Is.EqualTo(cameraRotation));
                Assert.That(GameplayInput.Movement, Is.EqualTo(Vector2.zero));
                Assert.That(Object.FindObjectsByType<Camera>(FindObjectsSortMode.None), Has.Length.EqualTo(cameraCount));
                Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None), Has.Length.EqualTo(listenerCount));
                Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None), Has.Length.EqualTo(eventSystemCount));

                deadline = Time.realtimeSinceStartup + 1;
                while (fixture.Entity.IsActionResolving && Time.realtimeSinceStartup < deadline) yield return null;
                Assert.That(fixture.Entity.TryUse(0, Vector3.forward), Is.True, "A separate ready action supplies the repeat whiff.");
                deadline = Time.realtimeSinceStartup + 1;
                while (!feedback.NoHitConfirmationVisible && fixture.Entity.IsActionResolving && Time.realtimeSinceStartup < deadline) yield return null;
                Assert.That(feedback.NoHitConfirmationVisible, Is.True);
                Assert.That(Object.FindObjectsByType<CameraFacingMarker>(FindObjectsSortMode.None), Has.Length.EqualTo(1), "Repeated whiffs remain a singular presentation.");

                yield return new WaitForSecondsRealtime(CombatFeedback.NoHitConfirmationDuration + .05f);
                Assert.That(feedback.NoHitConfirmationVisible, Is.False);
                Assert.That(GameObject.Find("Combat No Hit Confirmation"), Is.Null);
            }
            finally
            {
                GameplayInput.ResetForTests(); fixture.Dispose(); Object.Destroy(cameraObject);
            }
        }

        [UnityTest]
        public IEnumerator NoHitConfirmation_RejectsAiDashInvalidRejectedAndCanceledActions()
        {
            GameplayInput.ResetForTests(); Time.timeScale = 1;
            var cameraObject = MainCamera();
            var aiFixture = new EntityFixture(true);
            var dashFixture = new EntityFixture(true);
            var invalidFixture = new EntityFixture(true);
            var canceledFixture = new EntityFixture(true);
            try
            {
                aiFixture.Root.transform.position = Vector3.zero;
                dashFixture.Root.transform.position = Vector3.right * 20;
                invalidFixture.Root.transform.position = Vector3.right * 40;
                canceledFixture.Root.transform.position = Vector3.right * 60;
                Physics.SyncTransforms();

                aiFixture.Entity.SetController(aiFixture.Ai);
                Assert.That(aiFixture.Entity.TryUse(0, Vector3.forward), Is.True);
                yield return new WaitForSecondsRealtime(.3f);
                Assert.That(aiFixture.Root.GetComponent<CombatFeedback>().NoHitConfirmationVisible, Is.False, "AI whiffs stay silent.");

                dashFixture.Ability.Kind = AbilityKind.Dash; dashFixture.Ability.DashDistance = .1f;
                dashFixture.Entity.SetController(dashFixture.Player);
                Assert.That(dashFixture.Entity.TryUse(0, Vector3.forward), Is.True);
                yield return new WaitForSecondsRealtime(.35f);
                Assert.That(dashFixture.Root.GetComponent<CombatFeedback>().NoHitConfirmationVisible, Is.False, "Traversal-capable Dash stays outside this feedback.");

                invalidFixture.Ability.Damage = 0;
                invalidFixture.Entity.SetController(invalidFixture.Player);
                Assert.That(invalidFixture.Entity.TryUse(0, Vector3.forward), Is.True);
                yield return new WaitForSecondsRealtime(.3f);
                Assert.That(invalidFixture.Root.GetComponent<CombatFeedback>().NoHitConfirmationVisible, Is.False);
                invalidFixture.Ability.Damage = float.NaN;
                Assert.That(invalidFixture.Entity.TryUse(0, Vector3.forward), Is.True);
                yield return new WaitForSecondsRealtime(.3f);
                Assert.That(invalidFixture.Root.GetComponent<CombatFeedback>().NoHitConfirmationVisible, Is.False, "Invalid damage cannot produce outcome copy.");

                canceledFixture.Ability.Windup = 1;
                canceledFixture.Entity.SetController(canceledFixture.Player);
                Assert.That(canceledFixture.Entity.TryUse(0, Vector3.forward), Is.True);
                Assert.That(canceledFixture.Entity.TryUse(0, Vector3.forward), Is.False, "Overlapping input is rejected without feedback.");
                canceledFixture.Entity.SetController(canceledFixture.Ai);
                yield return null;
                Assert.That(canceledFixture.Root.GetComponent<CombatFeedback>().NoHitConfirmationVisible, Is.False, "Controller loss cancels windup before impact.");
                Assert.That(GameObject.Find("Combat No Hit Confirmation"), Is.Null);
            }
            finally
            {
                GameplayInput.ResetForTests();
                aiFixture.Dispose(); dashFixture.Dispose(); invalidFixture.Dispose(); canceledFixture.Dispose();
                Object.Destroy(cameraObject);
            }
        }

        [UnityTest]
        public IEnumerator Dodge_RejectsAiRootAndAbilityOverlapAndCleansLifecycleState()
        {
            GameplayInput.ResetForTests();
            Time.timeScale = 1;
            var cameraObject = MainCamera();
            var fixture = new EntityFixture(true);
            var deathFixture = new EntityFixture(false);
            var disableFixture = new EntityFixture(false);
            var trapFixture = new EntityFixture(false);
            EntityFixture destroyFixture = new EntityFixture(false);
            try
            {
                fixture.Entity.SetController(fixture.Ai);
                Assert.That(fixture.Entity.TryDodge(Vector3.right), Is.False, "AI control must never gain dodge.");
                var fixtureFeedback = fixture.Root.GetComponent<CombatFeedback>(); fixtureFeedback.ShowDodgeConfirmation(fixture.Entity.transform.position);
                Assert.That(fixtureFeedback.DodgeConfirmationVisible, Is.False, "AI/nonimmune targets cannot create confirmation.");
                fixtureFeedback.ShowNoHitConfirmation();
                Assert.That(fixtureFeedback.NoHitConfirmationVisible, Is.False, "AI cannot directly fabricate NO HIT.");
                fixture.Entity.SetController(fixture.Player);
                fixture.Entity.ApplyRoot(1);
                Assert.That(fixture.Player.Dodge(), Is.False);
                Assert.That(fixture.Player.RootEscapeVisible, Is.True); Assert.That(fixture.Player.RootEscapeProgress, Is.Zero);
                fixture.Entity.BreakRoot();
                fixtureFeedback.ShowNoHitConfirmation();
                Assert.That(fixtureFeedback.NoHitConfirmationVisible, Is.True);
                GameplayInput.SetTerminalState(true);
                fixture.Entity.SendMessage("Update", SendMessageOptions.RequireReceiver);
                Assert.That(fixture.Player.Dodge(), Is.False);
                Assert.That(fixtureFeedback.NoHitConfirmationVisible, Is.False, "Terminal state clears NO HIT synchronously on the existing terminal edge.");
                GameplayInput.SetTerminalState(false);
                fixture.Entity.SendMessage("Update", SendMessageOptions.RequireReceiver);

                Assert.That(fixture.Entity.TryUse(0, Vector3.forward), Is.True);
                Assert.That(fixture.Player.Dodge(), Is.False, "An active ability must own the action window.");
                yield return new WaitForSeconds(.35f);

                fixture.Entity.transform.rotation = Quaternion.LookRotation(Vector3.left);
                Assert.That(fixture.Player.Dodge(), Is.True);
                Assert.That(Vector3.Dot(fixture.Entity.transform.forward, Vector3.left), Is.GreaterThan(.99f), "No movement history must fall back to facing.");
                Assert.That(fixture.Entity.TryUse(0, Vector3.forward), Is.False, "Abilities must not begin during dodge.");
                fixtureFeedback.ShowDodgeConfirmation(fixture.Entity.transform.position); Assert.That(fixtureFeedback.DodgeConfirmationVisible, Is.True);
                fixtureFeedback.ShowNoHitConfirmation(); Assert.That(fixtureFeedback.NoHitConfirmationVisible, Is.True);
                fixture.Entity.SetController(fixture.Ai);
                Assert.That(fixture.Entity.IsDodging, Is.False);
                Assert.That(fixture.Entity.Health.IsDamageImmune, Is.False);
                Assert.That(fixtureFeedback.DodgeConfirmationVisible, Is.False, "Controller swap clears confirmation.");
                Assert.That(fixtureFeedback.NoHitConfirmationVisible, Is.False, "Controller swap clears NO HIT.");

                deathFixture.Entity.SetController(deathFixture.Player);
                Assert.That(deathFixture.Player.Dodge(), Is.True);
                var deathFeedback = deathFixture.Root.GetComponent<CombatFeedback>(); deathFeedback.ShowDodgeConfirmation(deathFixture.Entity.transform.position);
                Assert.That(deathFeedback.DodgeConfirmationVisible, Is.True);
                deathFeedback.ShowNoHitConfirmation(); Assert.That(deathFeedback.NoHitConfirmationVisible, Is.True);
                deathFixture.Entity.SendMessage("OnDeath", SendMessageOptions.RequireReceiver);
                Assert.That(deathFixture.Entity.IsDodging, Is.False);
                Assert.That(deathFixture.Entity.Health.IsDamageImmune, Is.False);
                Assert.That(deathFeedback.DodgeConfirmationVisible, Is.False, "Death cleanup clears confirmation.");
                Assert.That(deathFeedback.NoHitConfirmationVisible, Is.False, "Death cleanup clears NO HIT.");
                deathFixture.Entity.Health.TakeDamage(new DamageInfo(1000, null, Vector3.zero), 0);
                Assert.That(deathFixture.Entity.Health.IsDead, Is.True);
                Assert.That(deathFixture.Player.Dodge(), Is.False);

                disableFixture.Entity.SetController(disableFixture.Player);
                var disableFeedback = disableFixture.Root.GetComponent<CombatFeedback>();
                disableFeedback.ShowNoHitConfirmation(); Assert.That(disableFeedback.NoHitConfirmationVisible, Is.True);
                disableFeedback.enabled = false;
                Assert.That(disableFeedback.NoHitConfirmationVisible, Is.False, "Feedback disable clears NO HIT.");
                disableFeedback.enabled = true;
                Assert.That(disableFixture.Player.Dodge(), Is.True);
                disableFeedback.ShowDodgeConfirmation(disableFixture.Entity.transform.position);
                disableFeedback.ShowNoHitConfirmation();
                Assert.That(disableFeedback.DodgeConfirmationVisible, Is.True);
                Assert.That(disableFeedback.NoHitConfirmationVisible, Is.True);
                disableFixture.Entity.enabled = false;
                Assert.That(disableFixture.Entity.IsDodging, Is.False);
                Assert.That(disableFixture.Entity.Health.IsDamageImmune, Is.False);
                Assert.That(disableFeedback.DodgeConfirmationVisible, Is.False, "CombatEntity disable clears confirmation.");
                Assert.That(disableFeedback.NoHitConfirmationVisible, Is.False, "CombatEntity disable clears NO HIT.");
                disableFeedback.ShowNoHitConfirmation();
                Assert.That(disableFeedback.NoHitConfirmationVisible, Is.False, "A disabled source cannot recreate NO HIT.");

                trapFixture.Entity.SetController(trapFixture.Player);
                Assert.That(trapFixture.Player.Dodge(), Is.True);
                trapFixture.Entity.ApplyRoot(1);
                Assert.That(trapFixture.Entity.IsDodging, Is.False, "A root must stop dodge travel.");
                Assert.That(trapFixture.Entity.Health.IsDamageImmune, Is.True, "Root-before-damage traps must preserve the exact immunity window.");
                Assert.That(trapFixture.Entity.Health.TakeDamage(new DamageInfo(20, null, Vector3.zero), 0), Is.False);
                Assert.That(trapFixture.Entity.Health.Current, Is.EqualTo(100), "Trap damage inside the dodge window must be ignored.");
                var trapFeedback = trapFixture.Root.GetComponent<CombatFeedback>();
                Assert.That(trapFeedback.DodgeConfirmationVisible, Is.False, "Direct trap damage does not invent ability-impact confirmation.");
                trapFeedback.ShowDodgeConfirmation(trapFixture.Entity.transform.position); Assert.That(trapFeedback.DodgeConfirmationVisible, Is.True);
                trapFeedback.Cleanup(); Assert.That(trapFeedback.DodgeConfirmationVisible, Is.False, "Visual cleanup clears confirmation.");

                destroyFixture.Entity.SetController(destroyFixture.Player);
                Assert.That(destroyFixture.Player.Dodge(), Is.True);
                var destroyFeedback = destroyFixture.Root.GetComponent<CombatFeedback>(); destroyFeedback.ShowDodgeConfirmation(destroyFixture.Entity.transform.position);
                destroyFeedback.ShowNoHitConfirmation();
                var teardownMarker = GameObject.Find("Combat Dodge Confirmation"); Assert.That(teardownMarker, Is.Not.Null);
                var noHitTeardownMarker = GameObject.Find("Combat No Hit Confirmation"); Assert.That(noHitTeardownMarker, Is.Not.Null);
                destroyFixture.Dispose(); destroyFixture = null;
                yield return null; yield return null;
                Assert.That(teardownMarker == null, Is.True, "Entity/scene teardown destroys the detached world-space confirmation.");
                Assert.That(noHitTeardownMarker == null, Is.True, "Entity/scene teardown destroys NO HIT.");
            }
            finally
            {
                GameplayInput.ResetForTests();
                fixture.Dispose(); deathFixture.Dispose(); disableFixture.Dispose(); trapFixture.Dispose();
                destroyFixture?.Dispose();
                Object.Destroy(cameraObject);
            }
        }

        [UnityTest]
        public IEnumerator RaidAndPossessedDefenderHud_OwnAndLayOutDodgeInBothOrientations()
        {
            GameplayInput.ResetForTests();
            SceneManager.LoadScene("SylvanRealm");
            yield return null;
            yield return null;

            var raidHud = Object.FindFirstObjectByType<RaidHUD>();
            var raidRoot = raidHud.GetComponent<ResponsiveHudRoot>();
            Assert.That(raidHud.DodgeButtonVisible, Is.True);
            Assert.That(raidHud.DodgeButtonInteractable, Is.True);
            AssertUiOwnership(raidHud.DodgeButtonRect);
            raidRoot.SetOrientationForTests(PrototypeOrientation.Portrait); yield return null; AssertActionLayout(raidRoot, raidHud.DodgeButtonRect);
            raidRoot.SetOrientationForTests(PrototypeOrientation.Landscape); yield return null; AssertActionLayout(raidRoot, raidHud.DodgeButtonRect);
            var hero = GameObject.Find("Blood Knight").GetComponent<CombatEntity>();
            GameObject.Find("DODGE").GetComponent<Button>().onClick.Invoke(); yield return null;
            Assert.That(hero.IsDodging, Is.True); Assert.That(raidHud.DodgeButtonInteractable, Is.False); Assert.That(raidHud.DodgeButtonText, Does.Contain("DODG"));
            hero.ApplyRoot(1); yield return null;
            Assert.That(raidHud.DodgeButtonVisible, Is.True); Assert.That(raidHud.DodgeButtonInteractable, Is.False); Assert.That(raidHud.DodgeButtonText, Does.Contain("ROOTED"));
            hero.BreakRoot(); GameplayInput.SetTerminalState(true); yield return null; Assert.That(raidHud.DodgeButtonVisible, Is.False);

            GameplayInput.SetTerminalState(false);
            SceneManager.LoadScene("DefenderTest");
            yield return null;
            yield return null;
            var defenderHud = Object.FindFirstObjectByType<DefenderHUD>();
            var defenderRoot = defenderHud.GetComponent<ResponsiveHudRoot>();
            Assert.That(defenderHud.DodgeButtonVisible, Is.False);
            var possession = Object.FindFirstObjectByType<PossessionManager>();
            var ent = GameObject.Find("Guardian Ent").GetComponent<CombatEntity>();
            possession.Select(ent); Assert.That(possession.PossessSelected(), Is.True); yield return null;
            Assert.That(defenderHud.DodgeButtonVisible, Is.True); Assert.That(defenderHud.DodgeButtonInteractable, Is.True);
            AssertUiOwnership(defenderHud.DodgeButtonRect);
            defenderRoot.SetOrientationForTests(PrototypeOrientation.Portrait); yield return null; AssertActionLayout(defenderRoot, defenderHud.DodgeButtonRect);
            defenderRoot.SetOrientationForTests(PrototypeOrientation.Landscape); yield return null; AssertActionLayout(defenderRoot, defenderHud.DodgeButtonRect);
            GameObject.Find("DODGE").GetComponent<Button>().onClick.Invoke(); yield return null; Assert.That(ent.IsDodging, Is.True);
            possession.Release(); yield return null; Assert.That(defenderHud.DodgeButtonVisible, Is.False); Assert.That(ent.IsDodging, Is.False); Assert.That(ent.Health.IsDamageImmune, Is.False);
            possession.Select(ent); Assert.That(possession.PossessSelected(), Is.True); GameplayInput.SetTerminalState(true); yield return null; Assert.That(defenderHud.DodgeButtonVisible, Is.False);
            GameplayInput.SetTerminalState(false); possession.Release(); Time.timeScale = 1; GameplayInput.ResetForTests();
        }

        static GameObject MainCamera()
        {
            var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camera.tag = "MainCamera";
            return camera;
        }

        static void AssertUiOwnership(RectTransform button)
        {
            var ownership = button.GetComponent<UiPointerOwnership>();
            Assert.That(ownership, Is.Not.Null);
            var pointer = new PointerEventData(EventSystem.current) { pointerId = 906 };
            ownership.OnPointerDown(pointer); Assert.That(GameplayInput.IsUiOwned(pointer.pointerId), Is.True);
            ownership.OnPointerUp(pointer); Assert.That(GameplayInput.IsUiOwned(pointer.pointerId), Is.False);
        }

        static void AssertActionLayout(ResponsiveHudRoot root, RectTransform dodge)
        {
            var reference = root.GetComponent<CanvasScaler>().referenceResolution;
            var dodgeRect = DesignRect(dodge, reference);
            Assert.That(dodgeRect.xMin, Is.GreaterThanOrEqualTo(0)); Assert.That(dodgeRect.yMin, Is.GreaterThanOrEqualTo(0));
            Assert.That(dodgeRect.xMax, Is.LessThanOrEqualTo(reference.x)); Assert.That(dodgeRect.yMax, Is.LessThanOrEqualTo(reference.y));
            foreach (var button in root.GetComponentsInChildren<Button>(false))
            {
                var other = (RectTransform)button.transform;
                if (other == dodge || other.parent != root.transform) continue;
                Assert.That(dodgeRect.Overlaps(DesignRect(other, reference)), Is.False, $"Dodge overlaps {button.name}");
            }
        }

        static Rect DesignRect(RectTransform rect, Vector2 parentSize)
        {
            Assert.That(rect.anchorMin, Is.EqualTo(rect.anchorMax));
            var pivotPoint = Vector2.Scale(rect.anchorMin, parentSize) + rect.anchoredPosition;
            return new Rect(pivotPoint - Vector2.Scale(rect.pivot, rect.sizeDelta), rect.sizeDelta);
        }

        sealed class EntityFixture
        {
            public readonly GameObject Root;
            public readonly CombatEntity Entity;
            public readonly PlayerController Player;
            public readonly CreatureBrain Ai;
            readonly CharacterDefinition definition;
            public readonly AbilityDefinition Ability;

            public EntityFixture(bool withAbility)
            {
                Root = new GameObject("Dodge Test Entity", typeof(CharacterController), typeof(Health), typeof(CombatEntity), typeof(PlayerController), typeof(CreatureBrain));
                definition = ScriptableObject.CreateInstance<CharacterDefinition>();
                definition.DisplayName = "Dodge Test Entity";
                definition.Possessable = true;
                definition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 4, AttackSpeed = 1 };
                if (withAbility)
                {
                    Ability = ScriptableObject.CreateInstance<AbilityDefinition>();
                    Ability.DisplayName = "Test Action"; Ability.Kind = AbilityKind.Melee; Ability.Windup = .1f; Ability.Cooldown = 0; Ability.Range = 1; Ability.Radius = .1f;
                    definition.Abilities = new[] { Ability };
                }
                Entity = Root.GetComponent<CombatEntity>(); Player = Root.GetComponent<PlayerController>(); Ai = Root.GetComponent<CreatureBrain>();
                Entity.Initialize(definition);
            }

            public void Dispose()
            {
                Object.Destroy(Root);
                Object.Destroy(definition);
                if (Ability) Object.Destroy(Ability);
            }
        }
    }
}
