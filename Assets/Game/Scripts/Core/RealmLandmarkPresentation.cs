using UnityEngine;

namespace RealmRaiders.Core
{
    public enum RealmLandmarkRecipe
    {
        SylvanHeartTree,
        SylvanRootTrap,
        InfernalHeart,
        InfernalFlameTrap
    }

    /// <summary>Builds the four approved primitive-only realm silhouettes beneath existing gameplay roots.</summary>
    public static class RealmLandmarkPresentation
    {
        public const string RootName = "Realm Landmark Presentation";

        static Material sylvanWood;
        static Material sylvanCrown;
        static Material sylvanTrap;
        static Material infernalStone;
        static Material infernalFissure;

        public static Transform Build(Transform authoritativeRoot, RealmLandmarkRecipe recipe)
        {
            if (!authoritativeRoot) return null;
            var existing = authoritativeRoot.Find(RootName);
            if (existing) return existing;

            var presentation = new GameObject(RootName).transform;
            presentation.SetParent(authoritativeRoot, false);
            presentation.localPosition = Vector3.zero;
            presentation.localRotation = Quaternion.identity;
            presentation.localScale = Reciprocal(authoritativeRoot.lossyScale);

            switch (recipe)
            {
                case RealmLandmarkRecipe.SylvanHeartTree:
                    BuildHeartTree(presentation);
                    break;
                case RealmLandmarkRecipe.SylvanRootTrap:
                    SetRootMaterial(authoritativeRoot, SylvanTrap);
                    BuildRootTrap(presentation);
                    break;
                case RealmLandmarkRecipe.InfernalHeart:
                    BuildInfernalHeart(presentation);
                    break;
                case RealmLandmarkRecipe.InfernalFlameTrap:
                    SetRootMaterial(authoritativeRoot, InfernalFissure);
                    BuildFlameTrap(presentation);
                    break;
            }
            return presentation;
        }

        public static int RendererCeiling(RealmLandmarkRecipe recipe)
        {
            return recipe == RealmLandmarkRecipe.SylvanRootTrap || recipe == RealmLandmarkRecipe.InfernalFlameTrap ? 6 : 8;
        }

        static Material SylvanWood => Shared(ref sylvanWood, new Color(.27f, .15f, .07f));
        static Material SylvanCrown => Shared(ref sylvanCrown, new Color(.16f, .7f, .3f));
        static Material SylvanTrap => Shared(ref sylvanTrap, new Color(.2f, .75f, .28f));
        static Material InfernalStone => Shared(ref infernalStone, new Color(.12f, .035f, .025f));
        static Material InfernalFissure => Shared(ref infernalFissure, new Color(.9f, .16f, .025f));

        static Material Shared(ref Material material, Color color)
        {
            if (!material) material = PrototypeRuntimeFactory.Material(color);
            return material;
        }

        static void BuildHeartTree(Transform root)
        {
            Part(root, "Trunk Axis", PrimitiveType.Cylinder, new Vector3(0, 0, 0), new Vector3(1.65f, 2.4f, 1.65f), Quaternion.identity, SylvanWood);
            Part(root, "Wide Crown", PrimitiveType.Sphere, new Vector3(0, 3.5f, 0), new Vector3(4.6f, 2.8f, 3.8f), Quaternion.identity, SylvanCrown);
            Part(root, "Branch Left", PrimitiveType.Cylinder, new Vector3(-1.35f, 2.35f, 0), new Vector3(.42f, 1.65f, .42f), Quaternion.Euler(0, 0, -52), SylvanWood);
            Part(root, "Branch Right", PrimitiveType.Cylinder, new Vector3(1.45f, 2.55f, .15f), new Vector3(.4f, 1.75f, .4f), Quaternion.Euler(0, 0, 50), SylvanWood);
            Part(root, "Radial Root Left", PrimitiveType.Cylinder, new Vector3(-1.25f, -2.15f, 0), new Vector3(.34f, 1.45f, .34f), Quaternion.Euler(0, 0, 90), SylvanWood);
            Part(root, "Radial Root Right", PrimitiveType.Cylinder, new Vector3(1.25f, -2.15f, 0), new Vector3(.34f, 1.45f, .34f), Quaternion.Euler(0, 0, 90), SylvanWood);
            Part(root, "Radial Root Front", PrimitiveType.Cylinder, new Vector3(0, -2.15f, -1.25f), new Vector3(.34f, 1.45f, .34f), Quaternion.Euler(90, 0, 0), SylvanWood);
            Part(root, "Radial Root Rear", PrimitiveType.Cylinder, new Vector3(0, -2.15f, 1.25f), new Vector3(.34f, 1.45f, .34f), Quaternion.Euler(90, 0, 0), SylvanWood);
        }

        static void BuildRootTrap(Transform root)
        {
            for (var index = 0; index < 6; index++)
            {
                var angle = index * 60f;
                var direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
                Part(root, $"Inward Root {index + 1}", PrimitiveType.Cube, direction * 1.65f + Vector3.up * .12f, new Vector3(.34f, .18f, 1.35f), Quaternion.Euler(0, angle, 0), SylvanWood);
            }
        }

        static void BuildInfernalHeart(Transform root)
        {
            Part(root, "Heavy Core", PrimitiveType.Sphere, new Vector3(0, .15f, 0), new Vector3(3.2f, 2.8f, 2.8f), Quaternion.identity, InfernalFissure);
            Part(root, "Fractured Base", PrimitiveType.Cube, new Vector3(0, -2.15f, 0), new Vector3(4.2f, .65f, 3.6f), Quaternion.Euler(0, 45, 0), InfernalStone);
            Part(root, "Claw Left", PrimitiveType.Cube, new Vector3(-2.2f, 1.15f, 0), new Vector3(.72f, 3.2f, .72f), Quaternion.Euler(0, 0, -14), InfernalStone);
            Part(root, "Claw Right", PrimitiveType.Cube, new Vector3(2.2f, 1.15f, 0), new Vector3(.72f, 3.2f, .72f), Quaternion.Euler(0, 0, 14), InfernalStone);
            Part(root, "Claw Front", PrimitiveType.Cube, new Vector3(0, 1.05f, -2f), new Vector3(.68f, 2.9f, .68f), Quaternion.Euler(-14, 0, 0), InfernalStone);
            Part(root, "Claw Rear", PrimitiveType.Cube, new Vector3(0, 1.05f, 2f), new Vector3(.68f, 2.9f, .68f), Quaternion.Euler(14, 0, 0), InfernalStone);
        }

        static void BuildFlameTrap(Transform root)
        {
            for (var row = 0; row < 3; row++)
            {
                var z = -1.5f + row * 1.5f;
                Part(root, $"Chevron {row + 1} Left", PrimitiveType.Cube, new Vector3(-.68f, .12f, z), new Vector3(.24f, .16f, 1.35f), Quaternion.Euler(0, -36, 0), InfernalFissure);
                Part(root, $"Chevron {row + 1} Right", PrimitiveType.Cube, new Vector3(.68f, .12f, z), new Vector3(.24f, .16f, 1.35f), Quaternion.Euler(0, 36, 0), InfernalFissure);
            }
        }

        static void SetRootMaterial(Transform root, Material material)
        {
            var renderer = root.GetComponent<Renderer>();
            if (renderer) renderer.sharedMaterial = material;
        }

        static Transform Part(Transform parent, string name, PrimitiveType type, Vector3 position, Vector3 scale, Quaternion rotation, Material material)
        {
            var part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localRotation = rotation;
            part.transform.localScale = scale;
            var collider = part.GetComponent<Collider>();
            if (collider)
            {
                collider.enabled = false;
                if (Application.isPlaying) Object.Destroy(collider); else Object.DestroyImmediate(collider);
            }
            part.GetComponent<Renderer>().sharedMaterial = material;
            return part.transform;
        }

        static Vector3 Reciprocal(Vector3 value)
        {
            return new Vector3(SafeReciprocal(value.x), SafeReciprocal(value.y), SafeReciprocal(value.z));
        }

        static float SafeReciprocal(float value) => Mathf.Abs(value) < .0001f ? 1 : 1 / value;
    }
}
