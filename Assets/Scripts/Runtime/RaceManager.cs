using UnityEngine;

namespace NeonSurge
{
    public sealed class RaceManager : MonoBehaviour
    {
        private const string BestTimeKey = "neon-surge-best-time-v1";
        private static readonly Color Ink = new Color(0.025f, 0.045f, 0.09f, 0.96f);
        private static readonly Color Muted = new Color(0.55f, 0.68f, 0.78f);
        private static readonly Color Cyan = new Color(0.2f, 0.94f, 0.96f);
        private static readonly Color Pink = new Color(1f, 0.24f, 0.55f);

        private readonly RaceProgress progress = new RaceProgress();
        private NeonCarController car;
        private bool armed;
        private bool paused;
        private float bestTime;
        private int styleWidth = -1;
        private GUIStyle titleStyle;
        private GUIStyle labelStyle;
        private GUIStyle smallStyle;
        private GUIStyle timerStyle;
        private GUIStyle buttonStyle;

        public bool CanDrive { get { return armed && !paused && !progress.Finished; } }
        public bool CanRegisterCheckpoints { get { return CanDrive; } }
        public RaceProgress Progress { get { return progress; } }

        private void Awake()
        {
            bestTime = PlayerPrefs.GetFloat(BestTimeKey, 0f);
            Application.runInBackground = true;
        }

        private void Update()
        {
            if (armed && !paused && !progress.Finished)
            {
                progress.Tick(Time.unscaledDeltaTime);
                if (progress.Finished) StoreBestTime();
            }
        }

        public void Initialize(NeonCarController controller)
        {
            car = controller;
            car.Initialize(this, new Vector3(25f, 0.05f, -7f), Quaternion.identity);
        }

        public bool RegisterCheckpoint(int checkpointIndex)
        {
            if (!CanRegisterCheckpoints) return false;
            var accepted = progress.RegisterCheckpoint(checkpointIndex);
            if (accepted && progress.Finished) StoreBestTime();
            return accepted;
        }

        public void BeginTrial()
        {
            if (progress.Finished) RestartTrial();
            armed = true;
            paused = false;
        }

        public void TogglePause()
        {
            if (!armed || progress.Finished) return;
            paused = !paused;
        }

        public void ResumeTrial()
        {
            if (armed && !progress.Finished) paused = false;
        }

        public void RestartTrial()
        {
            progress.Reset();
            armed = false;
            paused = false;
            if (car != null) car.ResetVehicle();
        }

        private void StoreBestTime()
        {
            if (bestTime <= 0f || progress.TotalTime < bestTime)
            {
                bestTime = progress.TotalTime;
                PlayerPrefs.SetFloat(BestTimeKey, bestTime);
                PlayerPrefs.Save();
            }
        }

        private void OnGUI()
        {
            EnsureStyles();
            var compact = Screen.width < 760;
            var scale = Mathf.Clamp(Screen.width / 1440f, 0.62f, 1.15f);
            var margin = 18f * scale;
            var panelWidth = compact ? Screen.width - margin * 2f : 380f * scale;
            var panelHeight = compact ? 75f * scale : 83f * scale;
            var hudTop = 15f * scale;
            var rightWidth = compact ? panelWidth : 292f * scale;
            var rightX = compact ? margin : Screen.width - rightWidth - margin;
            var rightY = compact ? hudTop + panelHeight + 8f * scale : hudTop;

            DrawRect(new Rect(margin, hudTop, panelWidth, panelHeight), Ink);
            DrawText(new Rect(margin + 16f * scale, hudTop + 10f * scale, panelWidth - 24f * scale, 23f * scale),
                "NEON SURGE  /  NIGHT CIRCUIT", labelStyle, Cyan);
            DrawText(new Rect(margin + 16f * scale, hudTop + 39f * scale, panelWidth - 24f * scale, 23f * scale),
                "WASD OR ARROWS TO DRIVE", smallStyle, Muted);

            DrawRect(new Rect(rightX, rightY, rightWidth, 88f * scale), Ink);
            DrawText(new Rect(rightX + 13f * scale, rightY + 8f * scale, rightWidth - 20f * scale, 18f * scale),
                progress.Started ? "TIME TRIAL" : "BEST TIME", smallStyle, Muted);
            var displayTime = progress.Started ? progress.TotalTime : bestTime;
            DrawText(new Rect(rightX + 12f * scale, rightY + 27f * scale, rightWidth - 24f * scale, 50f * scale),
                !progress.Started && bestTime <= 0f ? "--:--.--" : FormatTime(displayTime), timerStyle, Color.white);

            var progressTop = compact ? rightY + 99f * scale : hudTop + panelHeight + 12f * scale;
            DrawRaceStrip(progressTop, scale, compact, margin, rightWidth, rightX);

            if (!armed)
                DrawOverlay("NEON SURGE", "NIGHT CIRCUIT / 03 LAPS", "Drive through the start gate and follow each checkpoint in order.\nSteer left through the course and ease off in the long bends. The clock starts at the line.", "BEGIN TIME TRIAL", BeginTrial, scale);
            else if (paused)
                DrawOverlay("PAUSED", "TAKE A BREATHER", "Your time is stopped. Resume when you are ready, or reset the trial.", "RESUME RACE", ResumeTrial, scale, true);
            else if (progress.Finished)
                DrawResults(scale);
            else
                DrawText(new Rect(margin, Screen.height - 38f * scale, Screen.width - margin * 2f, 24f * scale),
                    "WASD / ARROWS DRIVE    |    ESC PAUSE    |    R RESTART", smallStyle, Muted);
        }

        private void DrawRaceStrip(float top, float scale, bool compact, float margin, float rightWidth, float rightX)
        {
            var stripWidth = compact ? Screen.width - margin * 2f : rightWidth;
            var left = compact ? margin : rightX;
            var height = 58f * scale;
            DrawRect(new Rect(left, top, stripWidth, height), Ink);
            var lap = progress.Started ? progress.CurrentLap : 0;
            DrawText(new Rect(left + 12f * scale, top + 5f * scale, stripWidth * 0.43f, 20f * scale),
                lap == 0 ? "LAP  -- / 03" : "LAP  " + lap.ToString("00") + " / 03", labelStyle, Color.white);
            var checkpointText = progress.Started
                ? "GATE " + progress.CheckpointsPassed.ToString("0") + " / 4"
                : "START LINE AHEAD";
            DrawText(new Rect(left + stripWidth * 0.53f, top + 6f * scale, stripWidth * 0.43f, 19f * scale),
                checkpointText, smallStyle, Muted);

            var gap = 5f * scale;
            var barWidth = (stripWidth - 24f * scale - gap * 3f) / 4f;
            for (var index = 0; index < RaceProgress.CheckpointCount; index++)
            {
                var filled = index < progress.CheckpointsPassed;
                var active = progress.Started && index == progress.NextCheckpoint;
                var color = filled ? Cyan : active ? Pink : new Color(0.18f, 0.26f, 0.36f);
                DrawRect(new Rect(left + 12f * scale + index * (barWidth + gap), top + 33f * scale, barWidth, 10f * scale), color);
            }
        }

        private void DrawOverlay(string heading, string eyebrow, string copy, string action,
            System.Action callback, float scale, bool showRestart = false)
        {
            DrawRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0.01f, 0.02f, 0.05f, 0.68f));
            var width = Mathf.Min(610f * scale, Screen.width - 30f);
            var height = 350f * scale;
            var rect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
            DrawRect(rect, new Color(0.035f, 0.065f, 0.11f, 0.99f));
            DrawText(new Rect(rect.x + 28f * scale, rect.y + 28f * scale, width - 56f * scale, 24f * scale), eyebrow, labelStyle, Cyan);
            DrawText(new Rect(rect.x + 28f * scale, rect.y + 69f * scale, width - 56f * scale, 57f * scale), heading, titleStyle, Color.white);
            DrawText(new Rect(rect.x + 28f * scale, rect.y + 137f * scale, width - 56f * scale, 92f * scale), copy, labelStyle, Muted);

            var buttonRect = new Rect(rect.x + 28f * scale, rect.y + height - 77f * scale, width - 56f * scale, 48f * scale);
            GUI.backgroundColor = Cyan;
            GUI.contentColor = new Color(0.02f, 0.05f, 0.08f);
            if (GUI.Button(buttonRect, action, buttonStyle)) callback();
            GUI.backgroundColor = Color.white;
            GUI.contentColor = Color.white;
            if (showRestart)
            {
                var restartRect = new Rect(rect.x + 28f * scale, rect.y + height - 20f * scale, width - 56f * scale, 24f * scale);
                if (GUI.Button(restartRect, "RESTART TRIAL (R)", smallStyle)) RestartTrial();
            }
        }

        private void DrawResults(float scale)
        {
            DrawOverlay("TRIAL COMPLETE", "THREE LAPS / FINISHED", "TOTAL   " + FormatTime(progress.TotalTime) +
                "\nBEST     " + FormatTime(bestTime) + "\nLAP 1   " + FormatTime(progress.LapTimes[0]) +
                "     LAP 2   " + FormatTime(progress.LapTimes[1]) + "     LAP 3   " + FormatTime(progress.LapTimes[2]),
                "RACE AGAIN", BeginTrial, scale);
        }

        private void EnsureStyles()
        {
            if (styleWidth == Screen.width) return;
            styleWidth = Screen.width;
            var scale = Mathf.Clamp(Screen.width / 1440f, 0.62f, 1.15f);
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(16f * scale),
                wordWrap = true,
                alignment = TextAnchor.UpperLeft
            };
            smallStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(11f * scale),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(43f * scale),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            timerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(34f * scale),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.RoundToInt(15f * scale),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
        }

        private static void DrawText(Rect rect, string text, GUIStyle style, Color color)
        {
            var previous = GUI.color;
            GUI.color = color;
            GUI.Label(rect, text, style);
            GUI.color = previous;
        }

        private static void DrawRect(Rect rect, Color color)
        {
            var previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private static string FormatTime(float seconds)
        {
            var hundredths = Mathf.Max(0, Mathf.FloorToInt(seconds * 100f));
            var minutes = hundredths / 6000;
            var wholeSeconds = (hundredths / 100) % 60;
            var fraction = hundredths % 100;
            return minutes.ToString("00") + ":" + wholeSeconds.ToString("00") + "." + fraction.ToString("00");
        }
    }
}
