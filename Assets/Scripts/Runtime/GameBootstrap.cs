using UnityEngine;

namespace NeonSurge
{
    /// <summary>Builds the playable circuit and camera from the single saved scene object.</summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class GameBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            Application.runInBackground = true;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.19f, 0.24f, 0.33f);
            RenderSettings.ambientIntensity = 0.8f;

            BuildLighting();
            BuildCamera();
            var race = new GameObject("Race System").AddComponent<RaceManager>();
            var car = TrackBuilder.Build(race);
            race.Initialize(car);
        }

        private static void BuildLighting()
        {
            var sun = new GameObject("Moonlight");
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.48f, 0.7f, 1f);
            light.intensity = 0.82f;
            light.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(48f, -32f, 0f);

            var fill = new GameObject("Circuit fill light");
            var fillLight = fill.AddComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.color = new Color(1f, 0.18f, 0.42f);
            fillLight.intensity = 0.28f;
            fillLight.shadows = LightShadows.None;
            fill.transform.rotation = Quaternion.Euler(28f, 150f, 0f);
        }

        private static void BuildCamera()
        {
            var cameraObject = new GameObject("Race Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 47f, -38f);
            cameraObject.transform.LookAt(Vector3.zero, Vector3.up);

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 30f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 180f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.008f, 0.02f, 0.045f);
            camera.allowHDR = true;
            cameraObject.AddComponent<AudioListener>();
        }
    }
}
