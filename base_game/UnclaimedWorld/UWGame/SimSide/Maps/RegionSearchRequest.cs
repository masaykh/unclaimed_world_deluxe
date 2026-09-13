using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps;

public class RegionSearchRequest : ISnapshot, ILookUp<RegionSearchRequest, RegionSearchRequestID>
{
	public EntityID? Entity;

	public Point FromSubtile;

	public Point ToSubtile;

	public List<Tuple<double, string>> Log = new List<Tuple<double, string>>();

	public bool UseClosestRegionToFromSubtile;

	private MethodID? notifyWhenFinishedMethodID;

	public bool SendMessageToEntityWhenDone = true;

	private RegionSearchRequestID id = RegionSearchRequestID.Invalid;

	private static RegionSearchRequestID IDCounter;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public RegionSearchRequestID ID
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

	public RegionSearchRequest()
	{
	}

	public RegionSearchRequest(EntityID? entityID, Point fromSubtile, Point toSubtile, bool sendMessageToEntity, MethodID? notifyWhenFinished)
	{
		AddToLookup();
		if (The.Sim.TotalUnPausedGameTimeInSeconds > 35.0 && fromSubtile.X == 118)
		{
			_ = fromSubtile.Y;
			_ = 61;
		}
		Entity = entityID;
		FromSubtile = fromSubtile;
		ToSubtile = toSubtile;
		SendMessageToEntityWhenDone = sendMessageToEntity;
		notifyWhenFinishedMethodID = notifyWhenFinished;
		AddLog("Created");
	}

	public void AddLog(string text)
	{
	}

	public void Notify(Entity entity, RegionMap.Result result, float distance)
	{
		if (entity != null && SendMessageToEntityWhenDone)
		{
			switch (result)
			{
			case RegionMap.Result.OK:
				entity.SendMessage(new Message(Message.MessageTypes.DistanceFound)
				{
					OtherInfo = distance
				});
				break;
			case RegionMap.Result.NoAccess:
				entity.SendMessage(new Message(Message.MessageTypes.DistanceFoundNoAccess)
				{
					OtherInfo = distance
				});
				break;
			}
		}
		if (notifyWhenFinishedMethodID.HasValue)
		{
			ActionLookup.FindByID(notifyWhenFinishedMethodID.Value)?.Invoke();
		}
	}

	public void Destroy()
	{
		RemoveIDEntry();
	}

	public RegionSearchRequestID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= RegionSearchRequestID.Invalid)
		{
			throw new Exception("Astounding, RegionSearchRequestID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public RegionSearchRequestID SnapshotID(Snapshotter sn, RegionSearchRequestID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != RegionSearchRequestID.Invalid)
		{
			LookUp<RegionSearchRequest, RegionSearchRequestID>.Add(ID, this);
		}
	}

	public void SetInvalid()
	{
		id = RegionSearchRequestID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUp<RegionSearchRequest, RegionSearchRequestID>.Remove(this);
	}

	void ILookUp<RegionSearchRequest, RegionSearchRequestID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = RegionSearchRequestID.First;
	}

	void ILookUp<RegionSearchRequest, RegionSearchRequestID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<RegionSearchRequest, RegionSearchRequestID>.Create();
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		IDCounter = sn.DoEnum(IDCounter);
		id = SnapshotID(sn, id);
		Entity = sn.DoEnumNullable(Entity);
		FromSubtile = sn.DoPoint(FromSubtile);
		ToSubtile = sn.DoPoint(ToSubtile);
		UseClosestRegionToFromSubtile = sn.DoBool(UseClosestRegionToFromSubtile);
		SendMessageToEntityWhenDone = sn.DoBool(SendMessageToEntityWhenDone);
		notifyWhenFinishedMethodID = sn.DoEnumNullable(notifyWhenFinishedMethodID);
		sn.Ignore(Log);
		return this;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}

	private void sn_FinalLoadProcess(Snapshotter sn)
	{
	}
}
