using UWGame.Port;

namespace UnclaimedWorld.Test
{
    /// <summary>
    /// PORT DEVIATION 22: the simulation's fixed step. The arithmetic only - what the game does with
    /// the steps is checked on a real save by dataexport --save-steprate (gate 80, case 32d).
    /// </summary>
    [TestClass]
    public sealed class FixedSimStepTests
    {
        /// <summary>Feeds <paramref name="seconds"/> of frames at <paramref name="fps"/>, in whole ticks that add up exactly.</summary>
        private static long Run(FixedSimStep step, double fps, double seconds, double speed = 1)
        {
            long frames = (long)Math.Round(seconds * fps), previous = 0, steps = 0;
            for (long f = 0; f < frames; f++)
            {
                long ticks = (long)Math.Round((f + 1) * TimeSpan.TicksPerSecond / fps);
                steps += step.Advance(new TimeSpan((long)((ticks - previous) * speed)));
                previous = ticks;
            }
            return steps;
        }

        [TestMethod]
        [DataRow(30.0)]
        [DataRow(60.0)]
        [DataRow(120.0)]
        [DataRow(144.0)]
        [DataRow(165.0)]
        public void EveryFrameRateRunsTheSameSteps(double fps)
        {
            Assert.AreEqual(600L, Run(new FixedSimStep(60), fps, 10));
            Assert.AreEqual(300L, Run(new FixedSimStep(30), fps, 10));
        }

        [TestMethod]
        public void GameSpeedRunsProportionallyMoreSteps()
        {
            Assert.AreEqual(1200L, Run(new FixedSimStep(60), 144, 10, speed: 2));
            Assert.AreEqual(2400L, Run(new FixedSimStep(60), 60, 10, speed: 4));
        }

        [TestMethod]
        public void TheCapSlowsTheGameRatherThanOwingTime()
        {
            var step = new FixedSimStep(60);
            Assert.AreEqual(FixedSimStep.MaxStepsPerFrame, step.Advance(TimeSpan.FromSeconds(1)));
            Assert.IsTrue(step.Pending < step.Step, "what is left over is less than a step");
            Assert.AreEqual(TimeSpan.FromSeconds(1).Ticks - (FixedSimStep.MaxStepsPerFrame * step.Step.Ticks) - step.Pending.Ticks, step.Dropped.Ticks);
            // The next ordinary frame is ordinary: the dropped time is not run later.
            Assert.AreEqual(1, step.Advance(step.Step));
        }

        [TestMethod]
        public void OffRunsNoFixedSteps()
        {
            var step = new FixedSimStep(0);
            Assert.IsFalse(step.IsOn);
            Assert.AreEqual(0, step.Advance(TimeSpan.FromSeconds(1)));
        }

        [TestMethod]
        public void ChangingTheRateOrResettingForgetsWhatIsOwed()
        {
            var step = new FixedSimStep(60);
            step.Advance(TimeSpan.FromMilliseconds(10));
            Assert.AreNotEqual(TimeSpan.Zero, step.Pending);
            step.SetRate(30);
            Assert.AreEqual(TimeSpan.Zero, step.Pending);
            step.Advance(TimeSpan.FromMilliseconds(10));
            step.Reset();
            Assert.AreEqual(TimeSpan.Zero, step.Pending);
        }

        [TestMethod]
        public void AFrameShorterThanAStepRunsNoneUntilAStepIsOwed()
        {
            var step = new FixedSimStep(60);
            Assert.AreEqual(0, step.Advance(TimeSpan.FromTicks(step.Step.Ticks / 2)));
            Assert.AreEqual(1, step.Advance(TimeSpan.FromTicks(step.Step.Ticks / 2 + 1)));
        }
    }
}
