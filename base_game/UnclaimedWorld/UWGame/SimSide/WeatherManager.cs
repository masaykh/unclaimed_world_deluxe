using System;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;

namespace UWGame.SimSide;

public class WeatherManager : ICyclable, ILookUp<ICyclable, CyclableID>, ISnapshot
{
	private enum Phase
	{
		Temperature,
		Moisture
	}

	public Vector2 WindDirection;

	private Vector2 WindDirectionTarget;

	public float windSpeed = 1.5f;

	public float SunIntensity = 1f;

	private float globalTemperature;

	private float temperatureBeingAssigned;

	public float Temperature;

	private float cloudCover = 0.2f;

	private float cloudCoverTarget;

	private float cloudCoverSpeedOfChange = 1f;

	private static double totalComputationAllInstancesInSeconds;

	private float[] cloudEdges;

	private string[] cloudCoverTerms;

	private float[] windEdges;

	private string[] windTerms;

	public Vector2 CloudPosition = Vector2.Zero;

	private Vector2 cloudDrift;

	private Phase phase;

	private int cycleTileY;

	private Regulator regulator;

	private CyclableID id = CyclableID.Invalid;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsPaused { get; set; }

	public double StartedOnTimeInSeconds { get; set; }

	public double TotalComputationAllInstancesInSeconds
	{
		get
		{
			return totalComputationAllInstancesInSeconds;
		}
		set
		{
			totalComputationAllInstancesInSeconds = value;
		}
	}

	public double ComputationTimeSpentInSeconds { get; set; }

	public double? UpdateInterval => 2.0;

	public float CloudCover
	{
		get
		{
			return cloudCover;
		}
		set
		{
			if (cloudCover != value)
			{
				cloudCover = value;
				UpdateWeatherDisplay();
			}
		}
	}

	public float WindSpeed
	{
		get
		{
			return windSpeed;
		}
		set
		{
			if (windSpeed != value)
			{
				windSpeed = value;
				UpdateWeatherDisplay();
			}
		}
	}

	public CyclableID ID
	{
		get
		{
			return id;
		}
		private set
		{
			id = value;
		}
	}

	public int LoadPostProcessOrder => 0;

	public bool UnregisterBeforeSnapshot => false;

	public bool IsSnapshotted { get; set; }

	public WeatherManager()
	{
		InitConstants();
		if (!Snapshotter.IsSnapshotting)
		{
			AddToLookup();
			WindDirection = new Vector2(5f, -1f);
			WindDirection.Normalize();
			WindDirectionTarget = WindDirection;
			cloudCoverTarget = cloudCover;
			cloudDrift = WindDirection * WindSpeed / 600f;
			cloudDrift.X *= -1f;
			CreateRegulators();
		}
	}

	private void CreateRegulators()
	{
		regulator = new Regulator(The.Sim.GameplayRandomGenerator, 1.0 / UpdateInterval.Value, "WeatherManager");
	}

	private void UpdateWeatherDisplay()
	{
		int stairStepIndex = Common.GetStairStepIndex(cloudCover, cloudEdges);
		string text = cloudCoverTerms[stairStepIndex];
		stairStepIndex = Common.GetStairStepIndex(WindSpeed, windEdges);
		string wind = windTerms[stairStepIndex];
		The.InGameUI.SetWeatherNow(text, wind);
	}

	public void Update(GameTime gameTime)
	{
		if (The.Sim.DateAndTime.SunIsUp)
		{
			SunIntensity = Common.Clamp((1f - 0.5f * CloudCover) * (float)Math.Sin(The.Sim.DateAndTime.SunElevation), 0f, 1f);
		}
		else
		{
			SunIntensity = 0f;
		}
		CloudPosition += cloudDrift * (float)gameTime.ElapsedGameTime.TotalSeconds;
		CloudPosition.X %= 1f;
		CloudPosition.Y %= 1f;
		globalTemperature = 278f + 20f * SunIntensity;
		WindDirection = WindDirection * 0.99f + WindDirectionTarget * 0.01f;
		WindDirection.Normalize();
		if (!Common.IsEqual(cloudCover, cloudCoverSpeedOfChange))
		{
			CloudCover = CloudCover * (1f - cloudCoverSpeedOfChange) + cloudCoverTarget * cloudCoverSpeedOfChange;
		}
		cloudDrift = WindDirection * WindSpeed / 600f;
		cloudDrift.X *= -1f;
		if (The.Sim.CycleManager.IsRegistered(this))
		{
			return;
		}
		double millisecondsSinceLastReady = 0.0;
		if (regulator.IsReady(ref millisecondsSinceLastReady))
		{
			temperatureBeingAssigned = globalTemperature;
			phase = Phase.Temperature;
			cycleTileY = 0;
			The.Sim.CycleManager.Register(this, CycleManager.Priority.Medium);
			if (The.Sim.GameplayRandomGenerator.Next(50, "Weather") == 2)
			{
				WindDirectionTarget = new Vector2((float)(The.Sim.GameplayRandomGenerator.NextDouble("Weather") - 0.5), (float)(The.Sim.GameplayRandomGenerator.NextDouble("Weather") - 0.5));
				WindDirectionTarget.Normalize();
			}
			if (The.Sim.GameplayRandomGenerator.Next(80, "Weather") == 1)
			{
				cloudCoverTarget = (float)The.Sim.GameplayRandomGenerator.NextDouble("Weather");
				cloudCoverSpeedOfChange = (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution(1.0, 0.20000000298023224);
				cloudCoverSpeedOfChange = 0.01f * cloudCoverSpeedOfChange;
			}
		}
	}

	public CyclableID GetUniqueID()
	{
		return Cyclable.GetUniqueID();
	}

	public CyclableID SnapshotID(Snapshotter sn, CyclableID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != CyclableID.Invalid)
		{
			LookUp<ICyclable, CyclableID>.Add(ID, this);
		}
	}

	public void RemoveIDEntry()
	{
		LookUp<ICyclable, CyclableID>.Remove(this);
	}

	public void SetInvalid()
	{
		id = CyclableID.Invalid;
	}

	public void ResetIDCounter()
	{
	}

	void ILookUp<ICyclable, CyclableID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<ICyclable, CyclableID>.Create();
	}

	public void PrintInfo(StringBuilder text)
	{
		text.Append($"Weather");
	}

	public bool CycleOnce()
	{
		switch (phase)
		{
		case Phase.Temperature:
			_ = The.Map.mapTileWidth;
			_ = The.Map.TileMap;
			cycleTileY++;
			if (cycleTileY == The.Map.mapTileHeight)
			{
				cycleTileY = 0;
				phase = Phase.Moisture;
			}
			return false;
		case Phase.Moisture:
			return true;
		default:
			return true;
		}
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = SnapshotID(sn, id);
		cycleTileY = sn.DoInt32(cycleTileY);
		phase = sn.DoEnum(phase);
		cloudCover = sn.DoFloat(cloudCover);
		cloudCoverSpeedOfChange = sn.DoFloat(cloudCoverSpeedOfChange);
		cloudCoverTarget = sn.DoFloat(cloudCoverTarget);
		cloudDrift = sn.DoVector2(cloudDrift);
		CloudPosition = sn.DoVector2(CloudPosition);
		globalTemperature = sn.DoFloat(globalTemperature);
		SunIntensity = sn.DoFloat(SunIntensity);
		Temperature = sn.DoFloat(Temperature);
		WindDirection = sn.DoVector2(WindDirection);
		WindDirectionTarget = sn.DoVector2(WindDirectionTarget);
		windSpeed = sn.DoFloat(windSpeed);
		IsPaused = sn.DoBool(IsPaused);
		sn.Ignore(windTerms);
		sn.Ignore(cloudCoverTerms);
		sn.Ignore(windEdges);
		sn.Ignore(cloudEdges);
		sn.Ignore(totalComputationAllInstancesInSeconds);
		sn.Ignore(ComputationTimeSpentInSeconds);
		sn.Ignore(StartedOnTimeInSeconds);
		return this;
	}

	private void InitConstants()
	{
		cloudEdges = new float[4] { 0.1f, 0.4f, 0.96f, 1f };
		cloudCoverTerms = new string[4] { "Clear", "Partly cloudy", "Cloudy", "Overcast" };
		windEdges = new float[10] { 0.5f, 3f, 5f, 11f, 14f, 17f, 20f, 24f, 28f, 100f };
		windTerms = new string[10] { "Calm", "Light breeze", "Gentle breeze", "Fresh breeze", "Strong breeze", "High wind", "Gale", "Strong gale", "Storm", "Hurricane" };
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		CreateRegulators();
	}
}
