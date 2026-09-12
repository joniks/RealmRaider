using System.Collections.Generic;
using UnityEngine;

namespace RealmRaiders.Characters
{
    /// <summary>Owns visual-only children; never adds gameplay colliders, movement, health or controllers.</summary>
    [DisallowMultipleComponent]
    public sealed class CharacterVisualAssembler : MonoBehaviour
    {
        static readonly Dictionary<Color, Material> materials = new();
        public CharacterVisualRecipe Recipe { get; private set; }
        Transform visualRoot;
        Transform presentationPivot;

        public Transform VisualRoot => visualRoot;
        public Transform PresentationPivot => presentationPivot;

        public bool Assemble(CharacterVisualRecipe recipe)
        {
            Clear(); Recipe = recipe;
            if (!recipe || !recipe.IsValid) return false;
            var baseRenderer = GetComponent<Renderer>(); if (baseRenderer) baseRenderer.enabled = false;
            visualRoot = new GameObject("Character Visual Modules").transform; visualRoot.SetParent(transform, false);
            presentationPivot = new GameObject("Presentation Pivot").transform; presentationPivot.SetParent(visualRoot, false);
            var motion = GetComponent<CharacterVisualMotion>() ?? gameObject.AddComponent<CharacterVisualMotion>(); motion.Bind(presentationPivot);
            var effectiveRecipe = !recipe.BaseBodyPrefab && recipe.MissingBaseBodyFallback && recipe.MissingBaseBodyFallback.IsValid
                ? recipe.MissingBaseBodyFallback : recipe;
            Transform baseBody = null;
            var animated = recipe.Family == CharacterVisualFamily.LargeCreature && recipe.BaseBodyPrefab &&
                recipe.LargeCreatureMotion && recipe.LargeCreatureMotion.IsValid && GetComponent<CombatEntity>();
            if (animated)
            {
                baseBody = AddPrefab("Base Body", recipe.LargeCreatureMotion.VisualPrefab, Vector3.zero);
                ApplyBaseBodyRotation(recipe, baseBody);
                var adapter = GetComponent<LargeCreatureMotionAdapter>() ?? gameObject.AddComponent<LargeCreatureMotionAdapter>();
                if (!adapter.Bind(baseBody, recipe.LargeCreatureMotion))
                {
                    baseBody.gameObject.SetActive(false);
                    if (Application.isPlaying) Destroy(baseBody.gameObject); else DestroyImmediate(baseBody.gameObject);
                    baseBody = null;
                }
                else AlignBaseBodyToControllerSupportPlane(recipe, baseBody);
            }
            if (!baseBody) baseBody = BuildBase(effectiveRecipe);
            var proceduralMotion = GetComponent<CharacterProceduralMotionAdapter>() ?? gameObject.AddComponent<CharacterProceduralMotionAdapter>();
            proceduralMotion.Bind(baseBody, presentationPivot);
            BuildSlot("Head", effectiveRecipe.Head, effectiveRecipe.HeadPrefab, new Vector3(0, BodyHeight(effectiveRecipe) * .55f, 0), effectiveRecipe.AccentColor);
            BuildSlot("Back", effectiveRecipe.Back, effectiveRecipe.BackPrefab, new Vector3(0, .35f, -.28f), effectiveRecipe.Secondary);
            BuildSlot("Arms", effectiveRecipe.Arms, effectiveRecipe.ArmsPrefab, new Vector3(0, .05f, .1f), effectiveRecipe.Secondary);
            BuildSlot("Accent", effectiveRecipe.Accent, effectiveRecipe.AccentPrefab, new Vector3(0, .1f, .38f), effectiveRecipe.AccentColor);
            return true;
        }

        public void Clear()
        {
            GetComponent<LargeCreatureMotionAdapter>()?.Clear();
            GetComponent<CharacterProceduralMotionAdapter>()?.Clear();
            GetComponent<CharacterVisualMotion>()?.Bind(null);
            if (visualRoot) { visualRoot.gameObject.SetActive(false); if (Application.isPlaying) Destroy(visualRoot.gameObject); else DestroyImmediate(visualRoot.gameObject); } visualRoot = null; presentationPivot = null; Recipe = null;
            var baseRenderer = GetComponent<Renderer>(); if (baseRenderer) baseRenderer.enabled = true;
        }

        Transform BuildBase(CharacterVisualRecipe recipe)
        {
            Transform baseBody;
            if (recipe.BaseBodyPrefab) baseBody = AddPrefab("Base Body", recipe.BaseBodyPrefab, Vector3.zero);
            else
            {
                var type = recipe.Family == CharacterVisualFamily.Humanoid ? PrimitiveType.Capsule : recipe.Family == CharacterVisualFamily.LargeCreature ? PrimitiveType.Cube : PrimitiveType.Sphere;
                var scale = recipe.Family == CharacterVisualFamily.Humanoid ? new Vector3(.75f, 1.25f, .55f) : recipe.Family == CharacterVisualFamily.LargeCreature ? new Vector3(1.45f, 1.25f, .85f) : new Vector3(1.15f, .7f, .75f);
                baseBody = AddPrimitive("Base Body", type, Vector3.zero, scale, recipe.Primary);
            }
            ApplyBaseBodyRotation(recipe, baseBody);
            AlignBaseBodyToControllerSupportPlane(recipe, baseBody);
            return baseBody;
        }
        static void ApplyBaseBodyRotation(CharacterVisualRecipe recipe, Transform baseBody)
        {
            baseBody.localRotation = Quaternion.Euler(recipe.BaseBodyLocalEulerAngles) * baseBody.localRotation;
        }
        void AlignBaseBodyToControllerSupportPlane(CharacterVisualRecipe recipe, Transform baseBody)
        {
            if (!recipe.AlignBaseBodyToControllerSupportPlane || !TryGetVisualFootPlane(recipe, baseBody, out var footPlane)) return;
            var motor = GetComponent<CharacterController>();
            if (!motor) return;
            var supportPoint = transform.TransformPoint(motor.center + Vector3.down * (motor.height * .5f));
            var supportPlane = presentationPivot.InverseTransformPoint(supportPoint).y;
            baseBody.localPosition += Vector3.up * (supportPlane - footPlane);
        }

        bool TryGetVisualFootPlane(CharacterVisualRecipe recipe, Transform baseBody, out float footPlane)
        {
            footPlane = float.PositiveInfinity;
            if (recipe.BaseBodyGroundingAnchorNames != null && recipe.BaseBodyGroundingAnchorNames.Length > 0)
            {
                var matched = 0;
                foreach (var anchorName in recipe.BaseBodyGroundingAnchorNames)
                {
                    Transform match = null;
                    foreach (var candidate in baseBody.GetComponentsInChildren<Transform>(true))
                        if (candidate != baseBody && candidate.name == anchorName) { match = candidate; break; }
                    if (!match) return false;
                    footPlane = Mathf.Min(footPlane, presentationPivot.InverseTransformPoint(match.position).y);
                    matched++;
                }
                return matched == recipe.BaseBodyGroundingAnchorNames.Length;
            }
            var found = false;
            foreach (var renderer in baseBody.GetComponentsInChildren<Renderer>(true))
            {
                // Skinned localBounds is a conservative animation AABB, not a factual foot plane.
                // Imported skinned bodies must provide exact contact anchors above.
                if (renderer is SkinnedMeshRenderer) continue;
                var filter = renderer.GetComponent<MeshFilter>();
                if (!filter || !filter.sharedMesh) continue;
                var bounds = filter.sharedMesh.bounds;
                var center = bounds.center;
                var extents = bounds.extents;
                for (var x = -1; x <= 1; x += 2)
                for (var y = -1; y <= 1; y += 2)
                for (var z = -1; z <= 1; z += 2)
                {
                    var localCorner = center + Vector3.Scale(extents, new Vector3(x, y, z));
                    var pivotCorner = presentationPivot.InverseTransformPoint(renderer.transform.TransformPoint(localCorner));
                    footPlane = Mathf.Min(footPlane, pivotCorner.y);
                    found = true;
                }
            }
            return found;
        }
        float BodyHeight(CharacterVisualRecipe recipe) => recipe.Family == CharacterVisualFamily.LargeCreature ? 1.35f : recipe.Family == CharacterVisualFamily.Humanoid ? 1.2f : .65f;
        void BuildSlot(string slot, VisualModuleStyle style, GameObject prefab, Vector3 position, Color color)
        {
            if (prefab) { AddPrefab(slot, prefab, position); return; }
            if (style == VisualModuleStyle.None) return;
            var type = style is VisualModuleStyle.Blade or VisualModuleStyle.Spikes ? PrimitiveType.Cube : PrimitiveType.Sphere;
            var scale = style is VisualModuleStyle.Blade or VisualModuleStyle.Claws ? new Vector3(.18f, .7f, .18f) : style == VisualModuleStyle.ShoulderPads ? new Vector3(1.3f, .28f, .5f) : new Vector3(.42f, .42f, .42f);
            if (slot == "Arms") { AddPrimitive(slot + " Left", type, position + Vector3.left * .7f, scale, color); AddPrimitive(slot + " Right", type, position + Vector3.right * .7f, scale, color); }
            else AddPrimitive(slot, type, position, scale, color);
        }
        Transform AddPrefab(string name, GameObject prefab, Vector3 position)
        { var item = Instantiate(prefab, presentationPivot); item.name = name; item.transform.localPosition = position; DisableColliders(item); return item.transform; }
        Transform AddPrimitive(string name, PrimitiveType type, Vector3 position, Vector3 scale, Color color)
        {
            var item = GameObject.CreatePrimitive(type); item.name = name; item.transform.SetParent(presentationPivot, false); item.transform.localPosition = position; item.transform.localScale = scale;
            var collider = item.GetComponent<Collider>(); if (collider) collider.enabled = false;
            item.GetComponent<Renderer>().sharedMaterial = Material(color);
            return item.transform;
        }
        static void DisableColliders(GameObject item) { foreach (var collider in item.GetComponentsInChildren<Collider>(true)) collider.enabled = false; }
        static Material Material(Color color)
        {
            if (materials.TryGetValue(color, out var material) && material) return material;
            material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard")); material.name = "Character Visual Shared"; material.color = color; materials[color] = material; return material;
        }
        void OnDestroy() => Clear();
    }
}
