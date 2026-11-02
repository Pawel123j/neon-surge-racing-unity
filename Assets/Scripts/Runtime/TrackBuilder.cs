using UnityEngine;
using UnityEngine.Rendering;

namespace NeonSurge
{
    /// <summary>Builds the complete circuit and car from primitive meshes at scene start.</summary>
    public static class TrackBuilder
    {
        private const float RadiusX = 25f;
        private const float RadiusZ = 16f;
        private const int OvalSegments = 160;
        public const float TrackHalfWidth = 4.8f;

        private static Material asphalt;
        private static Material infield;
        private static Material ground;
        private static Material cyan;
        private static Material blue;
        private static Material pink;
        private static Material lime;
        private static Material white;
        private static Material rubber;
        private static Material body;
        private static Material window;

        public static NeonCarController Build(RaceManager race)
        {
            CreateMaterials();
            BuildGroundAndTrack();
            BuildScenery();
            BuildCheckpoints(race);
            return BuildCar();
        }

        public static float DistanceFromRoad(Vector3 point)
        {
            var angle = Mathf.Atan2(point.z / RadiusZ, point.x / RadiusX);
            var center = EllipsePoint(angle);
            var normal = OutwardNormal(angle);
            var delta = new Vector3(point.x - center.x, 0f, point.z - center.z);
            return Mathf.Abs(Vector3.Dot(delta, normal));
        }

        private static void CreateMaterials()
        {
            asphalt = MakeMaterial("Midnight asphalt", new Color(0.035f, 0.07f, 0.13f), 0.2f, 0.28f);
            infield = MakeMaterial("Infield navy", new Color(0.022f, 0.055f, 0.09f), 0.1f, 0.32f);
            ground = MakeMaterial("Night terrain", new Color(0.012f, 0.025f, 0.05f), 0f, 0.4f);
            cyan = MakeMaterial("Ion cyan", new Color(0.045f, 0.62f, 0.72f), 0.65f, 0.2f, new Color(0.02f, 0.5f, 0.8f));
            blue = MakeMaterial("Deep cobalt", new Color(0.04f, 0.22f, 0.43f), 0.55f, 0.22f, new Color(0.025f, 0.15f, 0.4f));
            pink = MakeMaterial("Signal magenta", new Color(0.82f, 0.055f, 0.29f), 0.5f, 0.2f, new Color(0.8f, 0.015f, 0.2f));
            lime = MakeMaterial("Timing lime", new Color(0.55f, 0.82f, 0.17f), 0.35f, 0.25f, new Color(0.28f, 0.55f, 0.06f));
            white = MakeMaterial("Checkpoint porcelain", new Color(0.72f, 0.86f, 0.92f), 0.25f, 0.22f);
            rubber = MakeMaterial("Tire rubber", new Color(0.012f, 0.02f, 0.035f), 0f, 0.76f);
            body = MakeMaterial("Racer shell", new Color(0.09f, 0.55f, 0.67f), 0.75f, 0.19f, new Color(0.015f, 0.24f, 0.36f));
            window = MakeMaterial("Smoked glass", new Color(0.025f, 0.09f, 0.16f), 0.6f, 0.13f, new Color(0.01f, 0.1f, 0.2f));
        }

        private static Material MakeMaterial(string name, Color color, float metallic, float smoothness, Color? emission = null)
        {
            var shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Diffuse");
            var material = new Material(shader) { name = name };
            material.color = color;
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", smoothness);
            if (emission.HasValue && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission.Value);
            }
            return material;
        }

        private static void BuildGroundAndTrack()
        {
            CreateCube("Night ground", Vector3.zero + Vector3.down * 0.7f, Quaternion.identity,
                new Vector3(160f, 1f, 118f), ground, false);
            var center = new GameObject("Infield");
            center.transform.position = new Vector3(0f, -0.075f, 0f);
            AddMesh(center, BuildEllipseDisk(RadiusX - TrackHalfWidth, RadiusZ - TrackHalfWidth), infield);

            var road = new GameObject("Oval racing surface");
            road.transform.position = new Vector3(0f, 0f, 0f);
            AddMesh(road, BuildOvalRing(), asphalt);
            AddOvalLine("Inner cyan rail", -TrackHalfWidth + 0.2f, cyan, 0.15f);
            AddOvalLine("Outer blue rail", TrackHalfWidth - 0.2f, blue, 0.15f);
            AddLaneDashes();
        }

        private static Mesh BuildOvalRing()
        {
            var mesh = new Mesh { name = "Procedural oval ring" };
            var vertices = new Vector3[(OvalSegments + 1) * 2];
            var triangles = new int[OvalSegments * 6];
            for (var index = 0; index <= OvalSegments; index++)
            {
                var angle = Mathf.PI * 2f * index / OvalSegments;
                var point = EllipsePoint(angle);
                var normal = OutwardNormal(angle);
                vertices[index * 2] = point - normal * TrackHalfWidth;
                vertices[index * 2 + 1] = point + normal * TrackHalfWidth;
                if (index == OvalSegments) continue;
                var vertex = index * 2;
                var tri = index * 6;
                triangles[tri] = vertex;
                triangles[tri + 1] = vertex + 2;
                triangles[tri + 2] = vertex + 1;
                triangles[tri + 3] = vertex + 1;
                triangles[tri + 4] = vertex + 2;
                triangles[tri + 5] = vertex + 3;
            }
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh BuildEllipseDisk(float radiusX, float radiusZ)
        {
            var mesh = new Mesh { name = "Procedural infield ellipse" };
            var vertices = new Vector3[OvalSegments + 2];
            var triangles = new int[OvalSegments * 3];
            vertices[0] = Vector3.zero;
            for (var index = 0; index <= OvalSegments; index++)
            {
                var angle = Mathf.PI * 2f * index / OvalSegments;
                vertices[index + 1] = new Vector3(Mathf.Cos(angle) * radiusX, 0f, Mathf.Sin(angle) * radiusZ);
                if (index == OvalSegments) continue;
                var tri = index * 3;
                triangles[tri] = 0;
                triangles[tri + 1] = index + 2;
                triangles[tri + 2] = index + 1;
            }
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddMesh(GameObject target, Mesh mesh, Material material)
        {
            target.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = target.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private static void AddOvalLine(string name, float offset, Material material, float width)
        {
            var lineObject = new GameObject(name);
            var line = lineObject.AddComponent<LineRenderer>();
            var positions = new Vector3[OvalSegments + 1];
            for (var index = 0; index <= OvalSegments; index++)
            {
                var angle = Mathf.PI * 2f * index / OvalSegments;
                positions[index] = EllipsePoint(angle) + OutwardNormal(angle) * offset + Vector3.up * 0.06f;
            }
            line.positionCount = positions.Length;
            line.SetPositions(positions);
            line.useWorldSpace = true;
            line.loop = false;
            line.startWidth = width;
            line.endWidth = width;
            line.material = material;
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.receiveShadows = false;
        }

        private static void AddLaneDashes()
        {
            for (var index = 0; index < 48; index++)
            {
                var angle = Mathf.PI * 2f * index / 48f;
                var point = EllipsePoint(angle) + Vector3.up * 0.035f;
                var tangent = EllipseTangent(angle).normalized;
                CreateCube("Lane dash", point, Quaternion.LookRotation(tangent, Vector3.up),
                    new Vector3(0.18f, 0.035f, 1.15f), white, false);
            }
        }

        private static void BuildScenery()
        {
            var blocks = new[]
            {
                new Vector3(-37f, 0f, -25f), new Vector3(-45f, 0f, -12f),
                new Vector3(-37f, 0f, 25f), new Vector3(-45f, 0f, 12f),
                new Vector3(37f, 0f, -25f), new Vector3(45f, 0f, -12f),
                new Vector3(37f, 0f, 25f), new Vector3(45f, 0f, 12f),
                new Vector3(-20f, 0f, 28f), new Vector3(20f, 0f, -28f),
                new Vector3(-18f, 0f, -28f), new Vector3(18f, 0f, 28f),
            };
            var random = new System.Random(6000);
            for (var index = 0; index < blocks.Length; index++)
            {
                var height = 1.5f + (float)random.NextDouble() * 4.2f;
                var width = 2.2f + (float)random.NextDouble() * 2.4f;
                var material = index % 3 == 0 ? blue : index % 3 == 1 ? pink : cyan;
                var tower = CreateCube("Infield skyline block", blocks[index] + Vector3.up * (height * 0.5f),
                    Quaternion.Euler(0f, index * 19f, 0f), new Vector3(width, height, width), material, false);
                CreateCube("Tower footing", blocks[index] + Vector3.up * 0.18f, Quaternion.identity,
                    new Vector3(width + 0.7f, 0.35f, width + 0.7f), asphalt, false);
                CreateCube("Tower light", tower.transform.position + Vector3.up * (height * 0.5f + 0.12f),
                    Quaternion.identity, new Vector3(width * 0.75f, 0.12f, 0.1f), lime, false);
            }

            CreateCube("Infield plinth", new Vector3(0f, 0.22f, 0f), Quaternion.identity,
                new Vector3(12f, 0.45f, 5.8f), asphalt, false);
            CreateCube("Infield display", new Vector3(0f, 0.57f, 0f), Quaternion.identity,
                new Vector3(10f, 0.2f, 4.3f), blue, false);
            CreateCube("Display cyan stripe", new Vector3(0f, 0.72f, -1.8f), Quaternion.identity,
                new Vector3(8.5f, 0.08f, 0.12f), cyan, false);
            CreateCube("Display pink stripe", new Vector3(0f, 0.72f, 1.8f), Quaternion.identity,
                new Vector3(6.5f, 0.08f, 0.12f), pink, false);
        }

        private static void BuildCheckpoints(RaceManager race)
        {
            for (var index = 0; index < RaceProgress.CheckpointCount; index++)
            {
                var angle = Mathf.PI * 0.5f * index;
                var point = EllipsePoint(angle);
                var tangent = EllipseTangent(angle).normalized;
                var normal = OutwardNormal(angle);
                var rotation = Quaternion.LookRotation(tangent, Vector3.up);

                var gate = new GameObject("Checkpoint " + (index + 1));
                gate.transform.position = point + Vector3.up * 1.6f;
                gate.transform.rotation = rotation;
                var trigger = gate.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
                trigger.size = new Vector3(TrackHalfWidth * 2.45f, 3.8f, 3.2f);
                gate.AddComponent<TrackCheckpoint>().Initialize(index, race);

                var outerPost = point + normal * (TrackHalfWidth + 0.15f);
                var innerPost = point - normal * (TrackHalfWidth + 0.15f);
                CreateCube("Gate outer pylon", outerPost + Vector3.up * 2.05f, rotation,
                    new Vector3(0.5f, 4.1f, 0.58f), index % 2 == 0 ? pink : cyan, false);
                CreateCube("Gate inner pylon", innerPost + Vector3.up * 2.05f, rotation,
                    new Vector3(0.5f, 4.1f, 0.58f), index % 2 == 0 ? cyan : pink, false);
                CreateCube("Gate overhead", point + Vector3.up * 4.05f, rotation,
                    new Vector3(TrackHalfWidth * 2.3f, 0.42f, 0.46f), blue, false);
                for (var tile = 0; tile < 9; tile++)
                {
                    var across = -TrackHalfWidth + (tile + 0.5f) * (TrackHalfWidth * 2f / 9f);
                    var tilePosition = point + normal * across + Vector3.up * 0.075f;
                    CreateCube("Timing line segment", tilePosition, rotation,
                        new Vector3(TrackHalfWidth * 2f / 9f, 0.08f, 0.42f),
                        tile % 2 == 0 ? white : pink, false);
                }
            }
        }

        private static NeonCarController BuildCar()
        {
            var root = new GameObject("Player | Neon GT");
            var controller = root.AddComponent<NeonCarController>();
            CreateCarPart(root.transform, "Lower chassis", new Vector3(0f, 0.34f, 0f),
                new Vector3(1.58f, 0.36f, 2.8f), rubber);
            CreateCarPart(root.transform, "Turquoise body", new Vector3(0f, 0.61f, 0.03f),
                new Vector3(1.52f, 0.48f, 2.55f), body);
            CreateCarPart(root.transform, "Cockpit canopy", new Vector3(0f, 0.91f, 0.1f),
                new Vector3(1.08f, 0.28f, 1.2f), window);
            CreateCarPart(root.transform, "Nose light", new Vector3(0f, 0.59f, 1.28f),
                new Vector3(0.82f, 0.1f, 0.12f), lime);
            CreateCarPart(root.transform, "Rear light", new Vector3(0f, 0.57f, -1.27f),
                new Vector3(1.1f, 0.12f, 0.1f), pink);
            CreateCarPart(root.transform, "Rear wing", new Vector3(0f, 0.83f, -1.05f),
                new Vector3(1.85f, 0.12f, 0.38f), blue);
            for (var side = -1; side <= 1; side += 2)
            for (var axle = -1; axle <= 1; axle += 2)
                CreateCarPart(root.transform, "Racing tire", new Vector3(side * 0.79f, 0.29f, axle * 0.83f),
                    new Vector3(0.31f, 0.44f, 0.63f), rubber);
            return controller;
        }

        private static void CreateCarPart(Transform parent, string name, Vector3 localPosition, Vector3 size, Material material)
        {
            var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = size;
            part.GetComponent<Renderer>().sharedMaterial = material;
            part.GetComponent<Collider>().enabled = false;
        }

        private static GameObject CreateCube(string name, Vector3 position, Quaternion rotation,
            Vector3 size, Material material, bool withCollider)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetPositionAndRotation(position, rotation);
            cube.transform.localScale = size;
            cube.GetComponent<Renderer>().sharedMaterial = material;
            if (!withCollider) cube.GetComponent<Collider>().enabled = false;
            return cube;
        }

        private static Vector3 EllipsePoint(float angle)
        {
            return new Vector3(Mathf.Cos(angle) * RadiusX, 0f, Mathf.Sin(angle) * RadiusZ);
        }

        private static Vector3 EllipseTangent(float angle)
        {
            return new Vector3(-RadiusX * Mathf.Sin(angle), 0f, RadiusZ * Mathf.Cos(angle));
        }

        private static Vector3 OutwardNormal(float angle)
        {
            return new Vector3(Mathf.Cos(angle) / RadiusX, 0f, Mathf.Sin(angle) / RadiusZ).normalized;
        }
    }
}
