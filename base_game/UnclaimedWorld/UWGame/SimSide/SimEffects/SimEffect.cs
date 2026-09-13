using System;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.SimEffects;

public class SimEffect : ISnapshot, ILookUp<SimEffect, SimEffectID>
{
	public EffectType EffectType;

	public double StartedOn;

	public float? Intensity;

	public double? ExpiresOn;

	private SimEffectID id = SimEffectID.Invalid;

	private static SimEffectID IDCounter = SimEffectID.First;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public SimEffectID ID
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

	public bool IsSnapshotted { get; set; }

	public SimEffect()
	{
	}

	public SimEffect(EffectType type)
	{
		AddToLookup();
		EffectType = type;
		Intensity = EffectType.GetIntensity();
		StartedOn = The.Sim.TotalUnPausedGameTimeInSeconds;
		if (EffectType.DurationInDays.HasValue)
		{
			ExpiresOn = StartedOn + DateAndTime.secondsPerDay * EffectType.DurationInDays.Value;
		}
		else if (EffectType.DynamicDurationInDays != null)
		{
			PropertyResult? propertyResult = EffectType.DynamicDurationInDays.Evaluate(null, null, null, null);
			if (propertyResult.HasValue)
			{
				ExpiresOn = StartedOn + DateAndTime.secondsPerDay * (double?)propertyResult.Value.NumberResult;
			}
		}
	}

	public void Destroy()
	{
		RemoveIDEntry();
	}

	public void UpdateExpiry(out bool wasDestroyed)
	{
		wasDestroyed = false;
		if (ExpiresOn.HasValue && The.Sim.TimepointReached(ExpiresOn.Value))
		{
			Destroy();
			wasDestroyed = true;
		}
	}

	public float GetValue()
	{
		return Intensity.Value;
	}

	public SimEffectID GetUniqueID()
	{
		IDCounter++;
		if ((ulong)IDCounter >= ulong.MaxValue)
		{
			throw new Exception("Astounding, SimEffectID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public SimEffectID SnapshotID(Snapshotter sn, SimEffectID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != SimEffectID.Invalid)
		{
			LookUp<SimEffect, SimEffectID>.Add(ID, this);
		}
	}

	public void SetInvalid()
	{
		id = SimEffectID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUp<SimEffect, SimEffectID>.Remove(this);
	}

	void ILookUp<SimEffect, SimEffectID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = SimEffectID.First;
	}

	void ILookUp<SimEffect, SimEffectID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<SimEffect, SimEffectID>.Create();
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = SnapshotID(sn, id);
		IDCounter = sn.DoEnum(IDCounter);
		EffectType = sn.DoGameData(EffectType);
		StartedOn = sn.DoDouble(StartedOn);
		Intensity = sn.DoFloatNullable(Intensity);
		ExpiresOn = sn.DoDoubleNullable(ExpiresOn);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
