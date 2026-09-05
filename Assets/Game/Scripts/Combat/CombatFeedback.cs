using System.Collections;
using RealmRaiders.Characters;
using UnityEngine;

namespace RealmRaiders.Combat
{
    /// <summary>Deliberately compact greybox readability feedback for CombatEntity actions.</summary>
    [DisallowMultipleComponent]
    public sealed class CombatFeedback : MonoBehaviour
    {
        static readonly int ColorId = Shader.PropertyToID("_BaseColor");
        static readonly System.Collections.Generic.Dictionary<Color, Material> materials = new();
        GameObject telegraph;
        readonly System.Collections.Generic.List<GameObject> transient = new();
        Renderer[] renderers;

        void Awake() => renderers = GetComponentsInChildren<Renderer>();
        public void ShowTelegraph(AbilityDefinition ability, Vector3 direction)
        {
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
            var marker = new GameObject("Combat Damage", typeof(TextMesh), typeof(CameraFacingMarker));
            marker.transform.position = point + Vector3.up * 1.35f;
            var text = marker.GetComponent<TextMesh>(); text.text = $"-{damage:0}"; text.anchor = TextAnchor.MiddleCenter; text.characterSize = .09f; text.fontSize = 54; text.color = new Color(1f, .86f, .25f);
            Track(marker, .65f);
        }

        public void ShowImpact()
        {
            var pulse = FlatPrimitive("Ability Impact", PrimitiveType.Cylinder, transform.position + Vector3.up * .05f, new Vector3(1.45f, .02f, 1.45f), new Color(.95f, 1f, .5f, .5f));
            Track(pulse, .2f);
        }

        public void Cleanup()
        {
            StopAllCoroutines(); ClearTelegraph();
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
