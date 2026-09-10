using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealmRaiders.Core
{
    public enum PrototypeArenaBoundaryStyle
    {
        NeutralStone,
        SylvanRoots,
        InfernalBasalt
    }

    public readonly struct ArenaCircleFootprint
    {
        public readonly Vector2 Center;
        public readonly float Radius;

        public ArenaCircleFootprint(Vector2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }
    }

    public readonly struct ArenaPathFootprint
    {
        public readonly Vector2 Center;
        public readonly Vector2 Size;
        public readonly float Yaw;

        public ArenaPathFootprint(Vector2 center, Vector2 size, float yaw = 0)
        {
            Center = center;
            Size = size;
            Yaw = yaw;
        }
    }

    [DisallowMultipleComponent]
    sealed class PrototypeArenaBoundaryHost : MonoBehaviour
    {
        [NonSerialized] public GameObject Boundary;
        [NonSerialized] public Mesh Mesh;

        void OnDestroy()
        {
            if (Mesh)
            {
                if (Application.isPlaying) Destroy(Mesh);
                else DestroyImmediate(Mesh);
            }
            Boundary = null;
            Mesh = null;
        }
    }

    /// <summary>Build-once, scene-local visual and collision closure for explicit prototype footprints.</summary>
    public static class PrototypeArenaBoundaryBuilder
    {
        public const string BoundaryName = "Prototype Arena Boundary";
        public const string VisualName = "Boundary Visual";
        public const float CornerOverlap = .12f;
        public const int SylvanColliderBudget = 56;
        public const string SylvanAlbedoResource = "Art/WorldSurfaces/MWS09-SylvanBoundary/sylvan-living-root-boundary-edge-hardened-candidate";
        public const string SylvanNormalResource = "Art/WorldSurfaces/MWS09-SylvanBoundary/sylvan-living-root-mobile-normal-rgb-candidate";
        public const float SylvanNormalStrength = .35f;
        const int CircleSides = 8;
        const float GeometryEpsilon = .002f;
        const float EndpointTolerance = .01f;
        const float MaximumContourSimplification = .24f;
        const float SurfaceTileScale = .22f;

        static Material neutralMaterial;
        static Material sylvanMaterial;
        static Material infernalMaterial;
        static Texture2D sylvanAlbedo;
        static Texture2D sylvanNormal;
        static bool sylvanSurfaceResolved;
        static Func<string, Texture2D> textureLoader = LoadTexture;

        struct Segment
        {
            public Vector2 A;
            public Vector2 B;

            public Segment(Vector2 a, Vector2 b) { A = a; B = b; }
        }

        public static GameObject BuildRectangle(Transform owner, Vector3 center, Vector2 size, float yaw,
            PrototypeArenaBoundaryStyle style, float visibleHeight, float visibleThickness,
            float collisionHeight, float inset)
        {
            var existing = Existing(owner);
            if (existing) return existing;
            ValidateCommon(owner, visibleHeight, visibleThickness, collisionHeight, inset);
            if (size.x <= inset * 2 || size.y <= inset * 2) throw new ArgumentOutOfRangeException(nameof(size));

            var rotation = Quaternion.Euler(0, yaw, 0);
            var right3 = rotation * Vector3.right;
            var forward3 = rotation * Vector3.forward;
            var right = new Vector2(right3.x, right3.z);
            var forward = new Vector2(forward3.x, forward3.z);
            var origin = new Vector2(center.x, center.z);
            var halfRight = size.x * .5f - inset;
            var halfForward = size.y * .5f - inset;
            var nearLeft = origin - right * halfRight - forward * halfForward;
            var nearRight = origin + right * halfRight - forward * halfForward;
            var farRight = origin + right * halfRight + forward * halfForward;
            var farLeft = origin - right * halfRight + forward * halfForward;
            var sides = new[]
            {
                new Segment(nearLeft, nearRight),
                new Segment(nearRight, farRight),
                new Segment(farRight, farLeft),
                new Segment(farLeft, nearLeft)
            };

            var boundary = CreateBoundary(owner, center.y, style, visibleHeight, visibleThickness,
                collisionHeight, sides, true, right, forward, new[] { nearLeft, nearRight, farRight, farLeft });
            Remember(owner, boundary);
            return boundary;
        }

        public static GameObject BuildSylvan(Transform owner, ArenaCircleFootprint[] nodes,
            ArenaPathFootprint[] paths, float visibleHeight = .95f, float visibleThickness = .75f,
            float collisionHeight = 5f, float inset = .25f)
        {
            var existing = Existing(owner);
            if (existing) return existing;
            ValidateCommon(owner, visibleHeight, visibleThickness, collisionHeight, inset);
            ValidateFootprints(nodes, paths);
            var components = CountFootprintComponents(nodes, paths);
            if (components != 1)
                throw new InvalidOperationException($"Sylvan floor footprints must form one factual union; found {components} components.");

            var effective = CreatePolygons(nodes, paths, inset);
            var effectiveComponents = CountPolygonComponents(effective);
            if (effectiveComponents != 1)
                throw new InvalidOperationException($"Sylvan inset footprints must remain one truthful union; found {effectiveComponents} components.");
            var loops = BuildLoops(ExposedSegments(effective));
            SimplifyToBudget(loops, SylvanColliderBudget);
            var contour = Flatten(loops);
            if (contour.Count > SylvanColliderBudget)
                throw new InvalidOperationException($"Sylvan contour needs {contour.Count} colliders; budget is {SylvanColliderBudget}.");

            var boundary = CreateBoundary(owner, 0, PrototypeArenaBoundaryStyle.SylvanRoots,
                visibleHeight, visibleThickness, collisionHeight, contour.ToArray(), false,
                Vector2.right, Vector2.up, null);
            Remember(owner, boundary);
            return boundary;
        }

        public static int CountFootprintComponents(ArenaCircleFootprint[] nodes, ArenaPathFootprint[] paths)
        {
            ValidateFootprints(nodes, paths);
            var polygons = CreatePolygons(nodes, paths, 0);
            return CountPolygonComponents(polygons);
        }

        static int CountPolygonComponents(List<Vector2[]> polygons)
        {
            var visited = new bool[polygons.Count];
            var stack = new Stack<int>();
            var components = 0;
            for (var start = 0; start < polygons.Count; start++)
            {
                if (visited[start]) continue;
                components++;
                visited[start] = true;
                stack.Push(start);
                while (stack.Count > 0)
                {
                    var current = stack.Pop();
                    for (var candidate = 0; candidate < polygons.Count; candidate++)
                    {
                        if (visited[candidate] || !PolygonsOverlap(polygons[current], polygons[candidate])) continue;
                        visited[candidate] = true;
                        stack.Push(candidate);
                    }
                }
            }
            return components;
        }

        static GameObject CreateBoundary(Transform owner, float groundY, PrototypeArenaBoundaryStyle style,
            float visibleHeight, float visibleThickness, float collisionHeight, Segment[] segments,
            bool addRectangleCorners, Vector2 rectangleRight, Vector2 rectangleForward, Vector2[] corners)
        {
            var root = new GameObject(BoundaryName); root.transform.SetParent(owner, false); root.isStatic = true;
            var visual = new GameObject(VisualName, typeof(MeshFilter), typeof(MeshRenderer)); visual.transform.SetParent(root.transform, false); visual.isStatic = true;
            var vertices = new List<Vector3>(segments.Length * 8 + (addRectangleCorners ? 32 : 0));
            var triangles = new List<int>(segments.Length * 36 + (addRectangleCorners ? 144 : 0));
            for (var i = 0; i < segments.Length; i++)
            {
                var factor = HeightFactor(style, i);
                AddRun(vertices, triangles, segments[i], groundY, visibleHeight * factor, visibleThickness, Lean(style));
                AddStyleAccent(vertices, triangles, segments[i], groundY, visibleHeight, visibleThickness, style, i);
                AddCollider(root.transform, segments[i], groundY, collisionHeight, visibleThickness, i);
            }
            if (addRectangleCorners)
            {
                var outwardRight = new[] { -rectangleRight, rectangleRight, rectangleRight, -rectangleRight };
                var outwardForward = new[] { -rectangleForward, -rectangleForward, rectangleForward, rectangleForward };
                for (var i = 0; i < corners.Length; i++)
                {
                    if (style == PrototypeArenaBoundaryStyle.SylvanRoots)
                    {
                        var center = corners[i] + (outwardRight[i] + outwardForward[i]) * (visibleThickness * .45f);
                        AddRoundAnchor(vertices, triangles, center, groundY,
                            visibleHeight * CornerHeightFactor(style, i), visibleThickness * .58f);
                    }
                    else AddCorner(vertices, triangles, corners[i], outwardRight[i], outwardForward[i], groundY,
                        visibleHeight * CornerHeightFactor(style, i), visibleThickness, Lean(style));
                }
            }

            var mesh = new Mesh { name = $"{style} Boundary Mesh" };
            mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.SetUVs(0, SurfaceUvs(vertices));
            mesh.RecalculateNormals(); mesh.RecalculateTangents(); mesh.RecalculateBounds();
            visual.GetComponent<MeshFilter>().sharedMesh = mesh;
            visual.GetComponent<MeshRenderer>().sharedMaterial = SharedMaterial(style);
            return root;
        }

        static List<Vector2> SurfaceUvs(List<Vector3> vertices)
        {
            var result = new List<Vector2>(vertices.Count);
            foreach (var vertex in vertices)
            {
                // One oblique world-space projection gives both horizontal crowns and vertical roots useful,
                // deterministic coverage without splitting or moving any existing visual-mesh vertex.
                result.Add(new Vector2(
                    (vertex.x + vertex.z * .37f) * SurfaceTileScale,
                    (vertex.y * 1.15f + vertex.x * .17f - vertex.z * .29f) * SurfaceTileScale));
            }
            return result;
        }

        static void AddCollider(Transform root, Segment segment, float groundY, float height, float thickness, int index)
        {
            var direction = segment.B - segment.A;
            var length = direction.magnitude;
            direction /= length;
            var outward = new Vector2(direction.y, -direction.x);
            var center = (segment.A + segment.B) * .5f + outward * (thickness * .5f);
            var child = new GameObject($"Boundary Collider {index + 1}", typeof(BoxCollider));
            child.transform.SetParent(root, false); child.transform.localPosition = new Vector3(center.x, groundY + height * .5f, center.y);
            child.transform.localRotation = Quaternion.Euler(0, Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg, 0);
            child.isStatic = true;
            var collider = child.GetComponent<BoxCollider>();
            collider.size = new Vector3(thickness, height, length + CornerOverlap * 2); collider.isTrigger = false;
        }

        static void AddRun(List<Vector3> vertices, List<int> triangles, Segment segment, float groundY,
            float height, float thickness, float lean)
        {
            var direction = (segment.B - segment.A).normalized;
            var outward = new Vector2(direction.y, -direction.x);
            var outerA = segment.A + outward * thickness;
            var outerB = segment.B + outward * thickness;
            var topShift = outward * lean;
            AddPrism(vertices, triangles,
                segment.A, segment.B, outerB, outerA,
                segment.A + topShift, segment.B + topShift, outerB + topShift, outerA + topShift,
                groundY, groundY + height);
        }

        static void AddCorner(List<Vector3> vertices, List<int> triangles, Vector2 corner,
            Vector2 outwardRight, Vector2 outwardForward, float groundY, float height, float thickness, float lean)
        {
            var a = corner;
            var b = corner + outwardRight * thickness;
            var c = b + outwardForward * thickness;
            var d = corner + outwardForward * thickness;
            var diagonal = (outwardRight + outwardForward).normalized * lean;
            AddPrism(vertices, triangles, a, b, c, d, a + diagonal, b + diagonal, c + diagonal, d + diagonal,
                groundY, groundY + height);
        }

        static void AddStyleAccent(List<Vector3> vertices, List<int> triangles, Segment segment,
            float groundY, float height, float thickness, PrototypeArenaBoundaryStyle style, int index)
        {
            var direction = (segment.B - segment.A).normalized;
            var outward = new Vector2(direction.y, -direction.x);
            var midpoint = (segment.A + segment.B) * .5f;
            if (style == PrototypeArenaBoundaryStyle.SylvanRoots && index % 6 == 0)
            {
                AddRoundAnchor(vertices, triangles, midpoint + outward * (thickness * .7f), groundY,
                    height * 1.08f, thickness * .42f);
            }
            else if (style == PrototypeArenaBoundaryStyle.InfernalBasalt && index % 3 == 0)
            {
                var halfLength = Mathf.Min(.55f, Vector2.Distance(segment.A, segment.B) * .2f);
                var innerA = midpoint - direction * halfLength;
                var innerB = midpoint + direction * halfLength;
                var outerA = innerA + outward * (thickness * .72f);
                var outerB = innerB + outward * (thickness * 1.28f);
                var topShift = outward * (Lean(style) * 1.2f);
                AddPrism(vertices, triangles, innerA, innerB, outerB, outerA,
                    innerA + topShift, innerB + topShift, outerB + topShift, outerA + topShift,
                    groundY, groundY + height * 1.08f);
            }
        }

        static void AddRoundAnchor(List<Vector3> vertices, List<int> triangles, Vector2 center,
            float groundY, float height, float radius)
        {
            const int sides = 8;
            var start = vertices.Count;
            for (var side = 0; side < sides; side++)
            {
                var angle = Mathf.PI * 2 * side / sides;
                var point = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                vertices.Add(new Vector3(point.x, groundY, point.y));
            }
            for (var side = 0; side < sides; side++)
            {
                var angle = Mathf.PI * 2 * side / sides;
                var point = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                vertices.Add(new Vector3(point.x, groundY + height, point.y));
            }
            for (var side = 0; side < sides; side++)
            {
                var next = (side + 1) % sides;
                triangles.Add(start + side); triangles.Add(start + next); triangles.Add(start + sides + next);
                triangles.Add(start + side); triangles.Add(start + sides + next); triangles.Add(start + sides + side);
            }
            for (var side = 1; side < sides - 1; side++)
            {
                triangles.Add(start); triangles.Add(start + side + 1); triangles.Add(start + side);
                triangles.Add(start + sides); triangles.Add(start + sides + side); triangles.Add(start + sides + side + 1);
            }
        }

        static void AddPrism(List<Vector3> vertices, List<int> triangles,
            Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 topA, Vector2 topB, Vector2 topC, Vector2 topD,
            float bottom, float top)
        {
            var start = vertices.Count;
            vertices.Add(new Vector3(a.x, bottom, a.y)); vertices.Add(new Vector3(b.x, bottom, b.y));
            vertices.Add(new Vector3(c.x, bottom, c.y)); vertices.Add(new Vector3(d.x, bottom, d.y));
            vertices.Add(new Vector3(topA.x, top, topA.y)); vertices.Add(new Vector3(topB.x, top, topB.y));
            vertices.Add(new Vector3(topC.x, top, topC.y)); vertices.Add(new Vector3(topD.x, top, topD.y));
            var faces = new[]
            {
                0, 2, 1, 0, 3, 2, 4, 5, 6, 4, 6, 7,
                0, 1, 5, 0, 5, 4, 1, 2, 6, 1, 6, 5,
                2, 3, 7, 2, 7, 6, 3, 0, 4, 3, 4, 7
            };
            for (var i = 0; i < faces.Length; i++) triangles.Add(start + faces[i]);
        }

        static List<Vector2[]> CreatePolygons(ArenaCircleFootprint[] nodes, ArenaPathFootprint[] paths, float pathInset)
        {
            var polygons = new List<Vector2[]>(nodes.Length + paths.Length);
            foreach (var node in nodes)
            {
                var radius = node.Radius - pathInset;
                if (radius <= 0) throw new ArgumentOutOfRangeException(nameof(nodes));
                var polygon = new Vector2[CircleSides];
                for (var side = 0; side < CircleSides; side++)
                {
                    var angle = Mathf.PI * 2 * side / CircleSides;
                    polygon[side] = node.Center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                }
                polygons.Add(polygon);
            }
            foreach (var path in paths)
            {
                var width = path.Size.x - pathInset * 2;
                if (width <= 0 || path.Size.y <= 0) throw new ArgumentOutOfRangeException(nameof(paths));
                var rotation = Quaternion.Euler(0, path.Yaw, 0);
                var right3 = rotation * Vector3.right;
                var forward3 = rotation * Vector3.forward;
                var right = new Vector2(right3.x, right3.z) * (width * .5f);
                var forward = new Vector2(forward3.x, forward3.z) * (path.Size.y * .5f);
                polygons.Add(new[]
                {
                    path.Center - right - forward,
                    path.Center + right - forward,
                    path.Center + right + forward,
                    path.Center - right + forward
                });
            }
            return polygons;
        }

        static List<Segment> ExposedSegments(List<Vector2[]> polygons)
        {
            var result = new List<Segment>();
            for (var polygonIndex = 0; polygonIndex < polygons.Count; polygonIndex++)
            {
                var polygon = polygons[polygonIndex];
                for (var edgeIndex = 0; edgeIndex < polygon.Length; edgeIndex++)
                {
                    var a = polygon[edgeIndex];
                    var b = polygon[(edgeIndex + 1) % polygon.Length];
                    var parameters = new List<float> { 0, 1 };
                    for (var otherIndex = 0; otherIndex < polygons.Count; otherIndex++)
                    {
                        if (otherIndex == polygonIndex) continue;
                        var other = polygons[otherIndex];
                        for (var otherEdge = 0; otherEdge < other.Length; otherEdge++)
                            AddIntersectionParameter(a, b, other[otherEdge], other[(otherEdge + 1) % other.Length], parameters);
                    }
                    parameters.Sort();
                    for (var parameterIndex = 0; parameterIndex < parameters.Count - 1; parameterIndex++)
                    {
                        var from = parameters[parameterIndex];
                        var to = parameters[parameterIndex + 1];
                        if (to - from <= .00001f) continue;
                        var start = Vector2.Lerp(a, b, from);
                        var end = Vector2.Lerp(a, b, to);
                        var direction = end - start;
                        if (direction.sqrMagnitude <= .000001f) continue;
                        direction.Normalize();
                        var outward = new Vector2(direction.y, -direction.x);
                        var midpoint = (start + end) * .5f;
                        if (!InsideUnion(midpoint + outward * GeometryEpsilon, polygons)
                            && InsideUnion(midpoint - outward * GeometryEpsilon, polygons))
                            result.Add(new Segment(start, end));
                    }
                }
            }
            return result;
        }

        static void AddIntersectionParameter(Vector2 a, Vector2 b, Vector2 c, Vector2 d, List<float> parameters)
        {
            var first = b - a;
            var second = d - c;
            var denominator = Cross(first, second);
            if (Mathf.Abs(denominator) <= .000001f) return;
            var offset = c - a;
            var t = Cross(offset, second) / denominator;
            var u = Cross(offset, first) / denominator;
            if (t > .00001f && t < .99999f && u >= -.00001f && u <= 1.00001f)
                AddUnique(parameters, t);
        }

        static List<List<Vector2>> BuildLoops(List<Segment> segments)
        {
            var remaining = new List<Segment>(segments);
            var loops = new List<List<Vector2>>();
            while (remaining.Count > 0)
            {
                var current = remaining[remaining.Count - 1];
                remaining.RemoveAt(remaining.Count - 1);
                var points = new List<Vector2> { current.A };
                var end = current.B;
                var guard = segments.Count + 1;
                while ((end - points[0]).sqrMagnitude > EndpointTolerance * EndpointTolerance && guard-- > 0)
                {
                    points.Add(end);
                    var nextIndex = FindStartingAt(remaining, end, out var reverse);
                    if (nextIndex < 0) throw new InvalidOperationException("Footprint union produced an open contour.");
                    current = remaining[nextIndex];
                    remaining.RemoveAt(nextIndex);
                    end = reverse ? current.A : current.B;
                }
                if (guard <= 0 || points.Count < 3) throw new InvalidOperationException("Footprint union contour is invalid.");
                loops.Add(points);
            }
            return loops;
        }

        static int FindStartingAt(List<Segment> segments, Vector2 endpoint, out bool reverse)
        {
            var tolerance = EndpointTolerance * EndpointTolerance;
            for (var index = 0; index < segments.Count; index++)
            {
                if ((segments[index].A - endpoint).sqrMagnitude <= tolerance) { reverse = false; return index; }
            }
            for (var index = 0; index < segments.Count; index++)
            {
                if ((segments[index].B - endpoint).sqrMagnitude <= tolerance) { reverse = true; return index; }
            }
            reverse = false;
            return -1;
        }

        static void SimplifyToBudget(List<List<Vector2>> loops, int budget)
        {
            var total = 0;
            foreach (var loop in loops) total += loop.Count;
            while (total > budget)
            {
                var bestLoop = -1;
                var bestVertex = -1;
                var bestDeviation = float.MaxValue;
                for (var loopIndex = 0; loopIndex < loops.Count; loopIndex++)
                {
                    var loop = loops[loopIndex];
                    if (loop.Count <= 3) continue;
                    for (var vertex = 0; vertex < loop.Count; vertex++)
                    {
                        var previous = loop[(vertex - 1 + loop.Count) % loop.Count];
                        var current = loop[vertex];
                        var next = loop[(vertex + 1) % loop.Count];
                        if (Cross(current - previous, next - current) < -.0001f) continue;
                        var deviation = DistanceToSegment(current, previous, next);
                        if (deviation >= bestDeviation) continue;
                        bestDeviation = deviation;
                        bestLoop = loopIndex;
                        bestVertex = vertex;
                    }
                }
                if (bestLoop < 0 || bestDeviation > MaximumContourSimplification)
                    throw new InvalidOperationException($"Sylvan contour cannot meet its collider budget without exceeding {MaximumContourSimplification:0.00} inward simplification.");
                loops[bestLoop].RemoveAt(bestVertex);
                total--;
            }
        }

        static List<Segment> Flatten(List<List<Vector2>> loops)
        {
            var result = new List<Segment>();
            foreach (var loop in loops)
                for (var index = 0; index < loop.Count; index++)
                    result.Add(new Segment(loop[index], loop[(index + 1) % loop.Count]));
            return result;
        }

        static bool PolygonsOverlap(Vector2[] first, Vector2[] second)
        {
            if (PointInPolygon(first[0], second) || PointInPolygon(second[0], first)) return true;
            for (var a = 0; a < first.Length; a++)
                for (var b = 0; b < second.Length; b++)
                    if (SegmentsIntersect(first[a], first[(a + 1) % first.Length], second[b], second[(b + 1) % second.Length])) return true;
            return false;
        }

        static bool SegmentsIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            var first = b - a;
            var second = d - c;
            var denominator = Cross(first, second);
            if (Mathf.Abs(denominator) <= .000001f) return false;
            var offset = c - a;
            var t = Cross(offset, second) / denominator;
            var u = Cross(offset, first) / denominator;
            return t >= -.00001f && t <= 1.00001f && u >= -.00001f && u <= 1.00001f;
        }

        static bool InsideUnion(Vector2 point, List<Vector2[]> polygons)
        {
            foreach (var polygon in polygons) if (PointInPolygon(point, polygon)) return true;
            return false;
        }

        static bool PointInPolygon(Vector2 point, Vector2[] polygon)
        {
            var inside = false;
            for (int index = 0, previous = polygon.Length - 1; index < polygon.Length; previous = index++)
            {
                var a = polygon[previous];
                var b = polygon[index];
                var edge = b - a;
                var offset = point - a;
                if (Mathf.Abs(Cross(edge, offset)) <= .00001f)
                {
                    var projection = Vector2.Dot(offset, edge);
                    if (projection >= -.00001f && projection <= edge.sqrMagnitude + .00001f) return true;
                }
                if ((a.y > point.y) != (b.y > point.y)
                    && point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x) inside = !inside;
            }
            return inside;
        }

        static float DistanceToSegment(Vector2 point, Vector2 start, Vector2 end)
        {
            var segment = end - start;
            var denominator = segment.sqrMagnitude;
            if (denominator <= .000001f) return Vector2.Distance(point, start);
            var t = Mathf.Clamp01(Vector2.Dot(point - start, segment) / denominator);
            return Vector2.Distance(point, start + segment * t);
        }

        static void AddUnique(List<float> values, float value)
        {
            foreach (var existing in values) if (Mathf.Abs(existing - value) <= .00001f) return;
            values.Add(value);
        }

        static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

        static void ValidateFootprints(ArenaCircleFootprint[] nodes, ArenaPathFootprint[] paths)
        {
            if (nodes == null || nodes.Length == 0) throw new ArgumentException("At least one node footprint is required.", nameof(nodes));
            if (paths == null) throw new ArgumentNullException(nameof(paths));
            foreach (var node in nodes) if (node.Radius <= 0) throw new ArgumentOutOfRangeException(nameof(nodes));
            foreach (var path in paths) if (path.Size.x <= 0 || path.Size.y <= 0) throw new ArgumentOutOfRangeException(nameof(paths));
        }

        static void ValidateCommon(Transform owner, float visibleHeight, float visibleThickness, float collisionHeight, float inset)
        {
            if (!owner) throw new ArgumentNullException(nameof(owner));
            if (visibleHeight <= 0) throw new ArgumentOutOfRangeException(nameof(visibleHeight));
            if (visibleThickness <= 0) throw new ArgumentOutOfRangeException(nameof(visibleThickness));
            if (collisionHeight <= 0) throw new ArgumentOutOfRangeException(nameof(collisionHeight));
            if (inset < 0) throw new ArgumentOutOfRangeException(nameof(inset));
        }

        static GameObject Existing(Transform owner)
        {
            if (!owner) return null;
            var host = owner.GetComponent<PrototypeArenaBoundaryHost>();
            return host && host.Boundary ? host.Boundary : null;
        }

        static void Remember(Transform owner, GameObject boundary)
        {
            var host = owner.GetComponent<PrototypeArenaBoundaryHost>() ?? owner.gameObject.AddComponent<PrototypeArenaBoundaryHost>();
            host.Boundary = boundary;
            host.Mesh = boundary.GetComponentInChildren<MeshFilter>().sharedMesh;
        }

        static float HeightFactor(PrototypeArenaBoundaryStyle style, int index)
        {
            return style switch
            {
                PrototypeArenaBoundaryStyle.NeutralStone => index % 2 == 0 ? .84f : 1f,
                PrototypeArenaBoundaryStyle.SylvanRoots => .84f + index % 3 * .08f,
                PrototypeArenaBoundaryStyle.InfernalBasalt => .82f + index % 3 * .1f,
                _ => 1
            };
        }

        static float CornerHeightFactor(PrototypeArenaBoundaryStyle style, int index)
        {
            return style switch
            {
                PrototypeArenaBoundaryStyle.NeutralStone => index % 2 == 0 ? 1.12f : 1.02f,
                PrototypeArenaBoundaryStyle.SylvanRoots => index % 2 == 0 ? 1.08f : .96f,
                PrototypeArenaBoundaryStyle.InfernalBasalt => index % 2 == 0 ? 1.12f : 1.02f,
                _ => 1
            };
        }

        static float Lean(PrototypeArenaBoundaryStyle style)
        {
            return style switch
            {
                PrototypeArenaBoundaryStyle.SylvanRoots => .07f,
                PrototypeArenaBoundaryStyle.InfernalBasalt => .16f,
                _ => 0
            };
        }

        static Material SharedMaterial(PrototypeArenaBoundaryStyle style)
        {
            switch (style)
            {
                case PrototypeArenaBoundaryStyle.NeutralStone:
                    return neutralMaterial ? neutralMaterial : neutralMaterial = CreateMaterial("Neutral Boundary Stone", new Color(.17f, .22f, .21f));
                case PrototypeArenaBoundaryStyle.SylvanRoots:
                    return sylvanMaterial ? sylvanMaterial : sylvanMaterial = CreateSylvanMaterial();
                case PrototypeArenaBoundaryStyle.InfernalBasalt:
                    return infernalMaterial ? infernalMaterial : infernalMaterial = CreateMaterial("Infernal Boundary Basalt", new Color(.12f, .075f, .06f));
                default:
                    throw new ArgumentOutOfRangeException(nameof(style), style, null);
            }
        }

        static Material CreateSylvanMaterial()
        {
            var material = CreateMaterial("Sylvan Boundary Roots", new Color(.25f, .22f, .105f));
            if (!ResolveSylvanSurface() || !material.HasProperty("_BumpMap")) return material;
            material.color = Color.white;
            material.mainTexture = sylvanAlbedo;
            material.SetTexture("_BumpMap", sylvanNormal);
            if (material.HasProperty("_BumpScale")) material.SetFloat("_BumpScale", SylvanNormalStrength);
            material.EnableKeyword("_NORMALMAP");
            return material;
        }

        static bool ResolveSylvanSurface()
        {
            if (sylvanSurfaceResolved) return sylvanAlbedo && sylvanNormal;
            sylvanSurfaceResolved = true;
            try
            {
                sylvanAlbedo = textureLoader?.Invoke(SylvanAlbedoResource);
                sylvanNormal = textureLoader?.Invoke(SylvanNormalResource);
            }
            catch (Exception)
            {
                sylvanAlbedo = null;
                sylvanNormal = null;
            }
            if (sylvanAlbedo && sylvanNormal) return true;
            sylvanAlbedo = null;
            sylvanNormal = null;
            return false;
        }

        static Texture2D LoadTexture(string resourcePath) => Resources.Load<Texture2D>(resourcePath);

        public static void ConfigureTextureLoaderForTests(Func<string, Texture2D> loader)
        {
            ResetSylvanSurfaceCache();
            textureLoader = loader ?? (_ => null);
        }

        public static void ResetTextureLoaderForTests()
        {
            ResetSylvanSurfaceCache();
            textureLoader = LoadTexture;
        }

        static void ResetSylvanSurfaceCache()
        {
            if (sylvanMaterial)
            {
                if (Application.isPlaying) UnityEngine.Object.Destroy(sylvanMaterial);
                else UnityEngine.Object.DestroyImmediate(sylvanMaterial);
            }
            sylvanMaterial = null;
            sylvanAlbedo = null;
            sylvanNormal = null;
            sylvanSurfaceResolved = false;
        }

        static Material CreateMaterial(string name, Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var material = new Material(shader) { name = name, color = color };
            return material;
        }
    }
}
