using System.Collections;
using RealmRaiders.Characters;
using RealmRaiders.Controllers;
using RealmRaiders.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace RealmRaiders.Combat
{
    /// <summary>Deliberately compact greybox readability feedback for CombatEntity actions.</summary>
    [DisallowMultipleComponent]
    public sealed class CombatFeedback : MonoBehaviour
    {
        public const float DodgeConfirmationDuration = .45f;
        public const float NoHitConfirmationDuration = .45f;
        public const float DefeatConfirmationDuration = .65f;
        public const float DamageMarkerDuration = .65f;
        public const float GroundSlamImpactDuration = .34f;
        public const float GroundSlamAlphaRiseDuration = .04f;
        public const float GroundSlamScaleDuration = .16f;
        public const float GroundSlamInitialScale = .7f;
        public const string GroundSlamImpactObjectName = "Guardian Ent Ground Slam Impact";
        const string GroundSlamSpriteResource = "Art/VFX/EntGroundSlam/ent-ground-slam-radial-decal-mobile-256";
        static readonly int ColorId = Shader.PropertyToID("_BaseColor");
        static readonly System.Collections.Generic.Dictionary<Color, Material> materials = new();
        static Sprite groundSlamSprite;
        static Material groundSlamMaterial;
        static bool groundSlamSpriteLoadAttempted;
        GameObject telegraph;
        GameObject damageMarker;
        Coroutine damageMarkerRoutine;
        GameObject dodgeConfirmation;
        Coroutine dodgeConfirmationRoutine;
        GameObject noHitConfirmation;
        Coroutine noHitConfirmationRoutine;
        GameObject defeatConfirmation;
        Coroutine defeatConfirmationRoutine;
        CombatFeedback defeatConfirmationSource;
        GameObject groundSlamImpact;
        SpriteRenderer groundSlamRenderer;
        Coroutine groundSlamRoutine;
        float groundSlamMaximumDiameter;
        float groundSlamAlpha;
        MaterialPropertyBlock groundSlamPropertyBlock;
        readonly System.Collections.Generic.List<CombatFeedback> defeatConfirmationTargets = new();
        readonly System.Collections.Generic.List<GameObject> transient = new();
        Renderer[] renderers;
        public bool DamageMarkerVisible => damageMarker && damageMarker.activeSelf;
        public bool DodgeConfirmationVisible => dodgeConfirmation && dodgeConfirmation.activeSelf;
        public bool NoHitConfirmationVisible => noHitConfirmation && noHitConfirmation.activeSelf;
        public bool DefeatConfirmationVisible => defeatConfirmation && defeatConfirmation.activeSelf;
        public bool GroundSlamImpactVisible => groundSlamImpact && groundSlamImpact.activeSelf;
        public GameObject GroundSlamImpactObject => groundSlamImpact;
        public float GroundSlamImpactMaximumDiameter => groundSlamMaximumDiameter;
        public float GroundSlamImpactAlpha => groundSlamAlpha;

        void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>();
            groundSlamPropertyBlock = new MaterialPropertyBlock();
        }
        public void ShowTelegraph(AbilityDefinition ability, Vector3 direction)
        {
            ClearDefeatConfirmation();
            ClearNoHitConfirmation();
            ClearTelegraph();
            telegraph = ability.Kind == AbilityKind.Area
                ? FlatPrimitive("Area Impact Radius", PrimitiveType.Cylinder, transform.position + direction * Mathf.Max(1, ability.Range * .55f), new Vector3(ability.Radius * 2, .025f, ability.Radius * 2), new Color(1f, .62f, .12f, .42f))
                : ability.Kind == AbilityKind.Dash
                    ? FlatPrimitive("Dash Direction", PrimitiveType.Cube, transform.position + direction * (ability.DashDistance * .5f), new Vector3(.34f, .025f, ability.DashDistance), new Color(.28f, .82f, 1f, .42f))
                    : FlatPrimitive("Melee Range", PrimitiveType.Cube, transform.position + direction * Mathf.Max(.7f, ability.Range * .5f), new Vector3(Mathf.Max(1.1f, ability.Radius * 1.2f), .025f, Mathf.Max(1, ability.Range)), new Color(1f, .85f, .16f, .42f));
            telegraph.transform.rotation = Quaternion.LookRotation(direction);
        }

        public void ClearTelegraph() { if (telegraph) Destroy(telegraph); telegraph = null; }

        public void ShowHit(float damage, Vector3 point, Vector3 source)
        {
            renderers = GetComponentsInChildren<Renderer>();
            StartCoroutine(Flash());
            GetComponent<CharacterVisualMotion>()?.ShowHitReaction();
            var entity = GetComponent<CombatEntity>();
            if (entity && entity.Motor && entity.Motor.enabled)
            {
                var away = transform.position - source; away.y = 0;
                if (away.sqrMagnitude > .01f) entity.Motor.Move(away.normalized * .16f);
            }
            if (!damageMarker)
            {
                damageMarker = new GameObject("Combat Damage", typeof(TextMesh), typeof(CameraFacingMarker));
                var createdText = damageMarker.GetComponent<TextMesh>(); createdText.anchor = TextAnchor.MiddleCenter; createdText.characterSize = .09f; createdText.fontSize = 54; createdText.color = new Color(1f, .86f, .25f);
            }
            damageMarker.transform.position = point + Vector3.up * 1.35f;
            damageMarker.GetComponent<TextMesh>().text = $"-{damage:0}";
            if (damageMarkerRoutine != null) StopCoroutine(damageMarkerRoutine);
            damageMarkerRoutine = StartCoroutine(ClearDamageMarkerAfter(damageMarker));
        }

        void ClearDamageMarker()
        {
            if (damageMarkerRoutine != null) StopCoroutine(damageMarkerRoutine);
            damageMarkerRoutine = null;
            if (damageMarker)
            {
                damageMarker.SetActive(false);
                Destroy(damageMarker);
            }
            damageMarker = null;
        }

        public void ShowDodgeConfirmation(Vector3 point)
        {
            if (dodgeConfirmation) return;
            var entity = GetComponent<CombatEntity>();
            if (!entity || entity.Health == null || entity.Health.IsDead || !entity.Health.IsDamageImmune || entity.ActiveController is not PlayerController player || !player.IsActive || !player.isActiveAndEnabled) return;
            dodgeConfirmation = new GameObject("Combat Dodge Confirmation", typeof(TextMesh), typeof(CameraFacingMarker));
            dodgeConfirmation.transform.position = point + Vector3.up * 1.5f;
            var text = dodgeConfirmation.GetComponent<TextMesh>(); text.text = "DODGED"; text.anchor = TextAnchor.MiddleCenter; text.characterSize = .09f; text.fontSize = 54; text.color = new Color(.35f, .9f, 1f);
            transient.Add(dodgeConfirmation);
            dodgeConfirmationRoutine = StartCoroutine(ClearDodgeConfirmationAfter(dodgeConfirmation));
        }

        public void ClearDodgeConfirmation()
        {
            if (dodgeConfirmationRoutine != null) StopCoroutine(dodgeConfirmationRoutine);
            dodgeConfirmationRoutine = null;
            if (dodgeConfirmation)
            {
                dodgeConfirmation.SetActive(false);
                transient.Remove(dodgeConfirmation);
                Destroy(dodgeConfirmation);
            }
            dodgeConfirmation = null;
        }

        public void ShowNoHitConfirmation()
        {
            if (!isActiveAndEnabled) return;
            var entity = GetComponent<CombatEntity>();
            if (!entity || !entity.isActiveAndEnabled || entity.Health == null || entity.Health.IsDead || GameplayInput.TerminalState ||
                entity.ActiveController is not PlayerController player || !player.IsActive || !player.isActiveAndEnabled) return;
            if (!noHitConfirmation)
            {
                noHitConfirmation = new GameObject("Combat No Hit Confirmation", typeof(TextMesh), typeof(CameraFacingMarker));
                var text = noHitConfirmation.GetComponent<TextMesh>(); text.text = "NO HIT"; text.anchor = TextAnchor.MiddleCenter; text.characterSize = .09f; text.fontSize = 54; text.color = new Color(1f, .72f, .22f);
                transient.Add(noHitConfirmation);
            }
            noHitConfirmation.transform.position = transform.position + Vector3.up * 1.6f;
            if (noHitConfirmationRoutine != null) StopCoroutine(noHitConfirmationRoutine);
            noHitConfirmationRoutine = StartCoroutine(ClearNoHitConfirmationAfter(noHitConfirmation));
        }

        public void ClearNoHitConfirmation()
        {
            if (noHitConfirmationRoutine != null) StopCoroutine(noHitConfirmationRoutine);
            noHitConfirmationRoutine = null;
            if (noHitConfirmation)
            {
                noHitConfirmation.SetActive(false);
                transient.Remove(noHitConfirmation);
                Destroy(noHitConfirmation);
            }
            noHitConfirmation = null;
        }

        public static bool ShouldShowNoHit(AbilityKind kind, float damage, bool eligibleContact,
            bool appliedHit, bool directControl, bool sourceAlive, bool terminal)
        {
            var offensiveKind = kind is AbilityKind.Melee or AbilityKind.Area;
            return offensiveKind && !float.IsNaN(damage) && !float.IsInfinity(damage) && damage > 0 &&
                   !eligibleContact && !appliedHit && directControl && sourceAlive && !terminal;
        }

        public static bool ShouldShowDefeat(AbilityKind kind, float damage, bool appliedHit,
            bool targetWasAlive, bool targetIsDead, bool eligibleNonSelfTarget, bool directControl,
            bool sourceAlive, bool terminal, string targetDisplayName)
        {
            var offensiveKind = kind is AbilityKind.Melee or AbilityKind.Area;
            return offensiveKind && !float.IsNaN(damage) && !float.IsInfinity(damage) && damage > 0 &&
                   appliedHit && targetWasAlive && targetIsDead && eligibleNonSelfTarget && directControl &&
                   sourceAlive && !terminal && !string.IsNullOrWhiteSpace(targetDisplayName);
        }

        internal void ShowDefeatConfirmation(CombatEntity target, Vector3 point)
        {
            var source = GetComponent<CombatEntity>();
            if (!isActiveAndEnabled || !source || !source.isActiveAndEnabled || source.Health == null || source.Health.IsDead ||
                GameplayInput.TerminalState || source.ActiveController is not PlayerController player || !player.IsActive || !player.isActiveAndEnabled ||
                !target || target == source || !target.isActiveAndEnabled || target.Health == null || !target.Health.IsDead ||
                !target.Definition || string.IsNullOrWhiteSpace(target.Definition.DisplayName)) return;
            var targetFeedback = target.GetComponent<CombatFeedback>();
            if (!targetFeedback || !targetFeedback.isActiveAndEnabled || targetFeedback.defeatConfirmation) return;

            targetFeedback.defeatConfirmationSource = this;
            targetFeedback.defeatConfirmation = new GameObject("Combat Defeat Confirmation", typeof(TextMesh), typeof(CameraFacingMarker));
            targetFeedback.defeatConfirmation.transform.position = point + Vector3.up * 1.7f;
            var text = targetFeedback.defeatConfirmation.GetComponent<TextMesh>();
            text.text = $"DEFEATED — {target.Definition.DisplayName}";
            text.anchor = TextAnchor.MiddleCenter;
            text.characterSize = .075f;
            text.fontSize = 54;
            text.color = new Color(1f, .78f, .25f);
            targetFeedback.transient.Add(targetFeedback.defeatConfirmation);
            defeatConfirmationTargets.Add(targetFeedback);
            targetFeedback.defeatConfirmationRoutine = targetFeedback.StartCoroutine(
                targetFeedback.ClearDefeatConfirmationAfter(targetFeedback.defeatConfirmation));
        }

        public void ClearDefeatConfirmation()
        {
            ClearOwnedDefeatConfirmation();
            while (defeatConfirmationTargets.Count > 0)
            {
                var target = defeatConfirmationTargets[defeatConfirmationTargets.Count - 1];
                defeatConfirmationTargets.RemoveAt(defeatConfirmationTargets.Count - 1);
                if (target) target.ClearDefeatConfirmationFrom(this);
            }
        }

        public void ShowImpact()
        {
            ClearNoHitConfirmation();
            var pulse = FlatPrimitive("Ability Impact", PrimitiveType.Cylinder, transform.position + Vector3.up * .05f, new Vector3(1.45f, .02f, 1.45f), new Color(.95f, 1f, .5f, .5f));
            Track(pulse, .2f);
        }

        public static bool ShouldShowGuardianEntGroundSlam(string archetypeId, AbilityDefinition ability,
            bool sourceActive, bool sourceAlive, bool terminal)
        {
            return sourceActive && sourceAlive && !terminal && ability &&
                   string.Equals(archetypeId, PrototypeCharacterRoster.GuardianEntId, System.StringComparison.Ordinal) &&
                   ability.Kind == AbilityKind.Area &&
                   string.Equals(ability.DisplayName, "Ground Slam", System.StringComparison.Ordinal) &&
                   !float.IsNaN(ability.Radius) && !float.IsInfinity(ability.Radius) && ability.Radius > 0;
        }

        public void ShowGuardianEntGroundSlamImpact(AbilityDefinition ability, Vector3 factualAreaCenter)
        {
            var entity = GetComponent<CombatEntity>();
            var archetypeId = entity && entity.Definition ? entity.Definition.ArchetypeId : null;
            if (!ShouldShowGuardianEntGroundSlam(archetypeId, ability,
                    isActiveAndEnabled && entity && entity.isActiveAndEnabled,
                    entity && entity.Health != null && !entity.Health.IsDead, GameplayInput.TerminalState)) return;

            ClearGroundSlamImpact();
            var sprite = GroundSlamSprite();
            var material = GroundSlamMaterial(sprite);
            if (!sprite || !material || sprite.bounds.size.x <= 0) return;

            groundSlamImpact = new GameObject(GroundSlamImpactObjectName, typeof(SpriteRenderer));
            groundSlamImpact.transform.position = factualAreaCenter + Vector3.up * .03f;
            groundSlamImpact.transform.rotation = Quaternion.Euler(90, 0, 0);
            groundSlamRenderer = groundSlamImpact.GetComponent<SpriteRenderer>();
            groundSlamRenderer.sprite = sprite;
            groundSlamRenderer.sharedMaterial = material;
            groundSlamRenderer.color = Color.white;
            groundSlamRenderer.shadowCastingMode = ShadowCastingMode.Off;
            groundSlamRenderer.receiveShadows = false;
            groundSlamRenderer.sortingOrder = 2;
            groundSlamMaximumDiameter = ability.Radius * 2;
            ApplyGroundSlamSample(0);
            groundSlamRoutine = StartCoroutine(AnimateGroundSlamImpact());
        }

        public void ClearGroundSlamImpact()
        {
            if (groundSlamRoutine != null) StopCoroutine(groundSlamRoutine);
            groundSlamRoutine = null;
            if (groundSlamImpact)
            {
                groundSlamImpact.SetActive(false);
                Destroy(groundSlamImpact);
            }
            groundSlamImpact = null;
            groundSlamRenderer = null;
            groundSlamMaximumDiameter = 0;
            groundSlamAlpha = 0;
        }

        public static float GroundSlamScaleAt(float elapsed)
        {
            return Mathf.Lerp(GroundSlamInitialScale, 1, Mathf.Clamp01(elapsed / GroundSlamScaleDuration));
        }

        public static float GroundSlamAlphaAt(float elapsed)
        {
            if (elapsed <= 0 || elapsed >= GroundSlamImpactDuration) return 0;
            if (elapsed < GroundSlamAlphaRiseDuration) return Mathf.Clamp01(elapsed / GroundSlamAlphaRiseDuration);
            return 1 - Mathf.Clamp01((elapsed - GroundSlamAlphaRiseDuration) /
                                     (GroundSlamImpactDuration - GroundSlamAlphaRiseDuration));
        }

        public void Cleanup()
        {
            StopAllCoroutines(); damageMarkerRoutine = null; dodgeConfirmationRoutine = null; noHitConfirmationRoutine = null; defeatConfirmationRoutine = null; ClearTelegraph();
            groundSlamRoutine = null;
            ClearGroundSlamImpact();
            ClearDamageMarker();
            ClearDefeatConfirmation();
            if (dodgeConfirmation)
            {
                dodgeConfirmation.SetActive(false);
                transient.Remove(dodgeConfirmation);
                Destroy(dodgeConfirmation);
            }
            dodgeConfirmation = null;
            if (noHitConfirmation)
            {
                noHitConfirmation.SetActive(false);
                transient.Remove(noHitConfirmation);
                Destroy(noHitConfirmation);
            }
            noHitConfirmation = null;
            GetComponent<CharacterVisualMotion>()?.ClearTransientReaction();
            foreach (var item in transient) if (item) Destroy(item);
            transient.Clear();
        }

        IEnumerator Flash()
        {
            var block = new MaterialPropertyBlock();
            foreach (var renderer in renderers) if (renderer) { renderer.GetPropertyBlock(block); block.SetColor(ColorId, Color.white); renderer.SetPropertyBlock(block); }
            yield return new WaitForSecondsRealtime(.1f);
            foreach (var renderer in renderers) if (renderer) renderer.SetPropertyBlock(null);
        }

        IEnumerator ClearDodgeConfirmationAfter(GameObject expected)
        {
            yield return new WaitForSecondsRealtime(DodgeConfirmationDuration);
            if (dodgeConfirmation != expected) yield break;
            dodgeConfirmationRoutine = null;
            if (dodgeConfirmation)
            {
                dodgeConfirmation.SetActive(false);
                transient.Remove(dodgeConfirmation);
                Destroy(dodgeConfirmation);
            }
            dodgeConfirmation = null;
        }

        IEnumerator ClearDamageMarkerAfter(GameObject expected)
        {
            yield return new WaitForSecondsRealtime(DamageMarkerDuration);
            if (damageMarker != expected) yield break;
            damageMarkerRoutine = null;
            ClearDamageMarker();
        }

        IEnumerator ClearNoHitConfirmationAfter(GameObject expected)
        {
            yield return new WaitForSecondsRealtime(NoHitConfirmationDuration);
            if (noHitConfirmation != expected) yield break;
            noHitConfirmationRoutine = null;
            if (noHitConfirmation)
            {
                noHitConfirmation.SetActive(false);
                transient.Remove(noHitConfirmation);
                Destroy(noHitConfirmation);
            }
            noHitConfirmation = null;
        }

        IEnumerator ClearDefeatConfirmationAfter(GameObject expected)
        {
            yield return new WaitForSecondsRealtime(DefeatConfirmationDuration);
            if (defeatConfirmation != expected) yield break;
            ClearOwnedDefeatConfirmation();
        }

        void ClearDefeatConfirmationFrom(CombatFeedback expectedSource)
        {
            if (defeatConfirmationSource == expectedSource) ClearOwnedDefeatConfirmation();
        }

        void ClearOwnedDefeatConfirmation()
        {
            if (defeatConfirmationRoutine != null) StopCoroutine(defeatConfirmationRoutine);
            defeatConfirmationRoutine = null;
            var source = defeatConfirmationSource;
            defeatConfirmationSource = null;
            if (defeatConfirmation)
            {
                defeatConfirmation.SetActive(false);
                transient.Remove(defeatConfirmation);
                Destroy(defeatConfirmation);
            }
            defeatConfirmation = null;
            if (source) source.defeatConfirmationTargets.Remove(this);
        }

        GameObject FlatPrimitive(string name, PrimitiveType type, Vector3 position, Vector3 scale, Color color)
        {
            var item = GameObject.CreatePrimitive(type); item.name = name; item.transform.position = position; item.transform.localScale = scale;
            var collider = item.GetComponent<Collider>(); if (collider) collider.enabled = false;
            var renderer = item.GetComponent<Renderer>(); renderer.sharedMaterial = SharedMaterial(color);
            return item;
        }
        static Material SharedMaterial(Color color)
        {
            if (materials.TryGetValue(color, out var material) && material) return material;
            material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            material.name = "Combat Feedback Shared"; material.color = color; materials[color] = material;
            return material;
        }
        static Sprite GroundSlamSprite()
        {
            if (!groundSlamSpriteLoadAttempted)
            {
                groundSlamSpriteLoadAttempted = true;
                groundSlamSprite = Resources.Load<Sprite>(GroundSlamSpriteResource);
            }
            return groundSlamSprite;
        }
        static Material GroundSlamMaterial(Sprite sprite)
        {
            if (!sprite) return null;
            if (groundSlamMaterial) return groundSlamMaterial;
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (!shader) return null;
            groundSlamMaterial = new Material(shader) { name = "Guardian Ent Ground Slam Shared", renderQueue = (int)RenderQueue.Transparent };
            groundSlamMaterial.SetOverrideTag("RenderType", "Transparent");
            groundSlamMaterial.SetFloat("_Surface", 1);
            groundSlamMaterial.SetFloat("_Blend", 0);
            groundSlamMaterial.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            groundSlamMaterial.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            groundSlamMaterial.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            groundSlamMaterial.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
            groundSlamMaterial.SetFloat("_ZWrite", 0);
            groundSlamMaterial.SetFloat("_Cull", (float)CullMode.Off);
            groundSlamMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            groundSlamMaterial.SetShaderPassEnabled("ShadowCaster", false);
            groundSlamMaterial.SetTexture("_BaseMap", sprite.texture);
            groundSlamMaterial.SetTexture("_MainTex", sprite.texture);
            return groundSlamMaterial;
        }
        IEnumerator AnimateGroundSlamImpact()
        {
            var elapsed = 0f;
            while (groundSlamImpact && elapsed < GroundSlamImpactDuration)
            {
                elapsed = Mathf.Min(GroundSlamImpactDuration, elapsed + Time.unscaledDeltaTime);
                ApplyGroundSlamSample(elapsed);
                yield return null;
            }
            groundSlamRoutine = null;
            if (groundSlamImpact)
            {
                groundSlamImpact.SetActive(false);
                Destroy(groundSlamImpact);
            }
            groundSlamImpact = null;
            groundSlamRenderer = null;
            groundSlamMaximumDiameter = 0;
            groundSlamAlpha = 0;
        }
        void ApplyGroundSlamSample(float elapsed)
        {
            if (!groundSlamImpact || !groundSlamRenderer || !groundSlamRenderer.sprite) return;
            var localDiameter = groundSlamMaximumDiameter / groundSlamRenderer.sprite.bounds.size.x;
            var scale = localDiameter * GroundSlamScaleAt(elapsed);
            groundSlamImpact.transform.localScale = new Vector3(scale, scale, 1);
            groundSlamAlpha = GroundSlamAlphaAt(elapsed);
            groundSlamPropertyBlock ??= new MaterialPropertyBlock();
            groundSlamPropertyBlock.Clear();
            groundSlamPropertyBlock.SetColor(ColorId, new Color(1, 1, 1, groundSlamAlpha));
            groundSlamRenderer.SetPropertyBlock(groundSlamPropertyBlock);
        }
        void Track(GameObject item, float seconds) { transient.Add(item); StartCoroutine(ClearAfter(item, seconds)); }
        IEnumerator ClearAfter(GameObject item, float seconds) { yield return new WaitForSecondsRealtime(seconds); transient.Remove(item); if (item) Destroy(item); }
        void OnDisable() => Cleanup();
        void OnDestroy() => Cleanup();
    }

    public sealed class CameraFacingMarker : MonoBehaviour
    {
        void LateUpdate()
        {
            var camera = Camera.main;
            if (camera) transform.rotation = Quaternion.LookRotation(camera.transform.position - transform.position);
        }
    }
}
