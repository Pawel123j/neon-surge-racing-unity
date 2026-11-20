using NUnit.Framework;

namespace NeonSurge.Tests
{
    public sealed class RaceProgressTests
    {
        [Test]
        public void OnlyStartFinishGateStartsTheClock()
        {
            var race = new RaceProgress();
            Assert.That(race.RegisterCheckpoint(2), Is.False);
            race.Tick(4f);
            Assert.That(race.TotalTime, Is.Zero);
            Assert.That(race.RegisterCheckpoint(0), Is.True);
            Assert.That(race.Started, Is.True);
            Assert.That(race.CurrentLap, Is.EqualTo(1));
            Assert.That(race.NextCheckpoint, Is.EqualTo(1));
        }

        [Test]
        public void WrongOrderAndDuplicateGatesDoNotAdvanceProgress()
        {
            var race = new RaceProgress();
            race.RegisterCheckpoint(0);
            Assert.That(race.RegisterCheckpoint(3), Is.False);
            Assert.That(race.RegisterCheckpoint(1), Is.True);
            Assert.That(race.RegisterCheckpoint(1), Is.False);
            Assert.That(race.NextCheckpoint, Is.EqualTo(2));
            Assert.That(race.CheckpointsPassed, Is.EqualTo(2));
        }

        [Test]
        public void OrderedGatesRecordThreeLapsAndFinishTheTrial()
        {
            var race = new RaceProgress();
            race.RegisterCheckpoint(0);
            for (var lap = 0; lap < RaceProgress.TotalLaps; lap++)
            {
                race.Tick(2f);
                Assert.That(race.RegisterCheckpoint(1), Is.True);
                race.Tick(2f);
                Assert.That(race.RegisterCheckpoint(2), Is.True);
                race.Tick(2f);
                Assert.That(race.RegisterCheckpoint(3), Is.True);
                race.Tick(2f);
                Assert.That(race.RegisterCheckpoint(0), Is.True);
            }

            Assert.That(race.Finished, Is.True);
            Assert.That(race.LapsCompleted, Is.EqualTo(3));
            Assert.That(race.LapTimes.Count, Is.EqualTo(3));
            Assert.That(race.LapTimes[0], Is.EqualTo(8f));
            Assert.That(race.TotalTime, Is.EqualTo(24f));
            Assert.That(race.RegisterCheckpoint(1), Is.False);
        }

        [Test]
        public void RestartClearsThePreviousTrialAndIgnoresNegativeTime()
        {
            var race = new RaceProgress();
            race.RegisterCheckpoint(0);
            race.Tick(10f);
            race.Tick(-3f);
            race.Reset();
            Assert.That(race.Started, Is.False);
            Assert.That(race.Finished, Is.False);
            Assert.That(race.LapsCompleted, Is.Zero);
            Assert.That(race.CurrentLap, Is.Zero);
            Assert.That(race.TotalTime, Is.Zero);
            Assert.That(race.LapTimes, Is.Empty);
        }
    }
}
