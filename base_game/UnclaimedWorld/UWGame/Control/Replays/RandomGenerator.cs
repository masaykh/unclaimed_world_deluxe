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

	private Random randomGenerator;

	private Snapshotter.Version version = Snapshotter.Version.Original;

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
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		RandomSeed = sn.DoInt32Nullable(RandomSeed);
		Type = sn.DoEnum(Type);
		if (sn.mode == Snapshotter.Mode.Load)
		{
			if (RandomSeed.HasValue)
			{
				randomGenerator = new Random(RandomSeed.Value);
			}
			else
			{
				randomGenerator = new Random();
			}
		}
		sn.Ignore(randomGenerator);
		sn.Ignore(lastMessage);
		sn.Ignore(latestTypeInUse);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
