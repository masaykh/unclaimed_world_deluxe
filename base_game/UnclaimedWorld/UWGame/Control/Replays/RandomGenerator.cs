using System;
using UWGame.SimSide.Snapshots;

namespace UWGame.Control.Replays;

public class RandomGenerator : ISnapshot
{
	public enum GeneratorType
	{
		Client,
		Sim
	}

	public GeneratorType Type;

	private static GeneratorType latestTypeInUse;

	private static string lastMessage;

	private int numberOfCalls;

	/// <summary>
	/// How many values have been drawn from <see cref="randomGenerator"/> - not how many methods
	/// were called on it.
	///
	/// The two differ: NextBytes consumes ONE value per byte, and SimplexNoise.CreateSeedNumbers
	/// asks it for 512 at a time, once per Personality. Counting calls would under-count by five
	/// hundred every time a colonist is created, and a fast-forward built on that number would
	/// land in the wrong place - which is worse than not resuming at all, because it looks right.
	/// </summary>
	private long internalSamples;

	private Random randomGenerator;

	private Snapshotter.Version version = Snapshotter.Version.RandomStreamPosition;

	public int? RandomSeed { get; private set; }

	public Random Random => randomGenerator;

	public bool IsSnapshotted { get; set; }

	public RandomGenerator()
	{
	}

	public RandomGenerator(int randomSeed, GeneratorType type)
	{
		RandomSeed = randomSeed;
		randomGenerator = new Random(RandomSeed.Value);
		Type = type;
	}

	public RandomGenerator(GeneratorType type)
	{
		randomGenerator = new Random();
		Type = type;
	}

	public int Next(string getterMessage, bool saveMessage = true)
	{
		if (saveMessage)
		{
			The.Sim.Controller.SaveOrVerifyRandomGet(getterMessage);
		}
		lastMessage = getterMessage;
		latestTypeInUse = Type;
		numberOfCalls++;
		internalSamples++;
		return randomGenerator.Next();
	}

	public int Next(int maximumValue, string getterMessage, bool saveMessage = true)
	{
		if (saveMessage)
		{
			The.Sim.Controller.SaveOrVerifyRandomGet(getterMessage);
		}
		lastMessage = getterMessage;
		latestTypeInUse = Type;
		numberOfCalls++;
		internalSamples++;
		return randomGenerator.Next(maximumValue);
	}

	public int Next(int minimumValue, int maximumValue, string getterMessage, bool saveMessage = true)
	{
		if (saveMessage)
		{
			The.Sim.Controller.SaveOrVerifyRandomGet(getterMessage);
		}
		lastMessage = getterMessage;
		latestTypeInUse = Type;
		numberOfCalls++;
		// One sample for any range this game uses. Random.Next(min, max) takes a SECOND sample
		// only when the range exceeds int.MaxValue, which nothing here does - counted anyway, so
		// that a future caller with a huge range cannot silently desynchronise the fast-forward.
		internalSamples += (((long)maximumValue - minimumValue) > int.MaxValue) ? 2 : 1;
		return randomGenerator.Next(minimumValue, maximumValue);
	}

	public void NextBytes(byte[] values, string getterMessage, bool saveMessage = true)
	{
		if (saveMessage && The.Sim != null)
		{
			The.Sim.Controller.SaveOrVerifyRandomGet(getterMessage);
		}
		lastMessage = getterMessage;
		latestTypeInUse = Type;
		numberOfCalls++;
		internalSamples += values?.Length ?? 0;
		randomGenerator.NextBytes(values);
	}

	public double NextDouble(string getterMessage, bool saveMessage = true)
	{
		if (saveMessage)
		{
			The.Sim.Controller.SaveOrVerifyRandomGet(getterMessage);
		}
		lastMessage = getterMessage;
		latestTypeInUse = Type;
		double result = randomGenerator.NextDouble();
		numberOfCalls++;
		internalSamples++;
		return result;
	}

	public float RandomBetween(float min, float max)
	{
		bool saveMessage = Type == GeneratorType.Sim;
		return min + (float)NextDouble("Common - RandomBetween", saveMessage) * (max - min);
	}

	public int RandomBetween(int min, int max)
	{
		bool saveMessage = Type == GeneratorType.Sim;
		return Next(min, max, "Common - RandomBetween", saveMessage);
	}

	public int RandomSign()
	{
		bool saveMessage = Type == GeneratorType.Sim;
		return 2 * Next(0, 1, "Common - RandomSign", saveMessage) - 1;
	}

	public double RandomNormalDistribution(double mean, double stdDev)
	{
		bool saveMessage = Type == GeneratorType.Sim;
		double d = NextDouble("Common - RandomNormalDistribution", saveMessage);
		double num = NextDouble("Common - RandomNormalDistribution", saveMessage);
		return mean + stdDev * (Math.Sqrt(-2.0 * Math.Log(d)) * Math.Cos(6.28 * num));
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.RandomStreamPosition);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		RandomSeed = sn.DoInt32Nullable(RandomSeed);
		Type = sn.DoEnum(Type);
		// PORT FIX. Saving the POSITION in the stream, not only the seed it started from.
		//
		// This used to rebuild the generator from RandomSeed and nothing else, so every load
		// rewound the random stream to the beginning and the game dealt out numbers it had
		// already used. A save made ten hours in resumed with the first ten hours' worth of
		// randomness ahead of it again.
		//
		// That is a determinism bug in its own right, and it is the reason a replay cannot be
		// trusted across a save. It is also a candidate for "when I did save-load, they got
		// Superficial injuries" - a reloaded game is not a continuation of the one that was
		// saved, it is one that draws different numbers from the same point.
		//
		// System.Random has no way to read or write its internal state, so the position is
		// restored by drawing that many values and discarding them. Random.Next() consumes
		// exactly one, which is why the count above is of VALUES rather than of calls.
		// A long game reaches a few million; at roughly two nanoseconds a draw that is tens of
		// milliseconds, once, on load.
		if (version >= Snapshotter.Version.RandomStreamPosition)
		{
			internalSamples = sn.DoInt64(internalSamples);
		}
		if (sn.mode == Snapshotter.Mode.Load)
		{
			if (RandomSeed.HasValue)
			{
				randomGenerator = new Random(RandomSeed.Value);
				// An unseeded generator has no reproducible stream to resume, and a save from
				// before this version has no recorded position - both leave internalSamples at 0
				// and behave exactly as they did before.
				for (long i = 0L; i < internalSamples; i++)
				{
					randomGenerator.Next();
				}
			}
			else
			{
				randomGenerator = new Random();
			}
		}
		sn.Ignore(randomGenerator);
		sn.Ignore(numberOfCalls);
		sn.Ignore(lastMessage);
		sn.Ignore(latestTypeInUse);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
