using System.Collections.Generic;

namespace NeonSurge
{
    /// <summary>Deterministic three-lap time-trial rules, kept independent of the scene and physics.</summary>
    public sealed class RaceProgress
    {
        public const int TotalLaps = 3;
        public const int CheckpointCount = 4;

        private readonly List<float> lapTimes = new List<float>(TotalLaps);

        public bool Started { get; private set; }
        public bool Finished { get; private set; }
        public int LapsCompleted { get; private set; }
        public int CurrentLap { get; private set; }
        public int NextCheckpoint { get; private set; }
        public float TotalTime { get; private set; }
        public float CurrentLapTime { get; private set; }
        public IReadOnlyList<float> LapTimes { get { return lapTimes; } }

        /// <summary>Number of ordered gates already passed on the current lap.</summary>
        public int CheckpointsPassed
        {
            get
            {
                if (!Started) return 0;
                return NextCheckpoint == 0 ? CheckpointCount : NextCheckpoint;
            }
        }

        public void Tick(float deltaSeconds)
        {
            if (!Started || Finished || deltaSeconds <= 0f) return;
            TotalTime += deltaSeconds;
            CurrentLapTime += deltaSeconds;
        }

        /// <summary>Accepts only the next ordered gate. Gate zero starts the clock and finishes each lap.</summary>
        public bool RegisterCheckpoint(int index)
        {
            if (index < 0 || index >= CheckpointCount || Finished) return false;

            if (!Started)
            {
                if (index != 0) return false;
                Started = true;
                CurrentLap = 1;
                NextCheckpoint = 1;
                CurrentLapTime = 0f;
                return true;
            }

            if (index != NextCheckpoint) return false;
            if (index != 0)
            {
                NextCheckpoint = (index + 1) % CheckpointCount;
                return true;
            }

            lapTimes.Add(CurrentLapTime);
            LapsCompleted++;
            if (LapsCompleted >= TotalLaps)
            {
                Finished = true;
                CurrentLap = TotalLaps;
                return true;
            }

            CurrentLap = LapsCompleted + 1;
            NextCheckpoint = 1;
            CurrentLapTime = 0f;
            return true;
        }

        public void Reset()
        {
            Started = false;
            Finished = false;
            LapsCompleted = 0;
            CurrentLap = 0;
            NextCheckpoint = 0;
            TotalTime = 0f;
            CurrentLapTime = 0f;
            lapTimes.Clear();
        }
    }
}
