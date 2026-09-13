using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Overland.Missions;

public class MissionStop : ISnapshot, ILookUp<MissionStop, MissionStopID>
{
	public MissionStopTemplate MissionStopTemplate;

	private MissionStopTemplateID snapshotMissionStopTemplate;

	public Queue<MissionAction> Actions = new Queue<MissionAction>();

	public TravelAction TravelAction;

	public Mission mission;

	private MissionStopID id = MissionStopID.Invalid;

	private static MissionStopID IDCounter = MissionStopID.First;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public MissionStopID ID
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

	public MissionStop(Mission mission, MissionStopTemplate locationType)
	{
		AddToLookup();
		this.mission = mission;
		MissionStopTemplate = locationType;
		MissionAction missionAction = null;
		if (locationType.Actions != null)
		{
			foreach (MissionActionTemplate action in locationType.Actions)
			{
				missionAction = action.CreateMissionAction(mission);
				Actions.Enqueue(missionAction);
			}
		}
		if (locationType.TravelAction != null)
		{
			TravelAction = new TravelAction(mission, locationType.TravelAction);
		}
	}

	public MissionStop()
	{
	}

	public void Destroy()
	{
		foreach (MissionAction action in Actions)
		{
			action.Destroy();
		}
		if (TravelAction != null)
		{
			TravelAction.Destroy();
		}
		Actions.Clear();
		RemoveIDEntry();
	}

	public void StartMission()
	{
		foreach (MissionAction action in Actions)
		{
			action.StartMission();
		}
		if (TravelAction != null)
		{
			TravelAction.StartMission();
		}
	}

	public void Update(GameTime gameTime)
	{
		if (Actions.Count > 0)
		{
			if (Actions.Peek().Update(gameTime) && Actions.Count > 0)
			{
				Actions.Dequeue();
			}
		}
		else if (TravelAction != null && TravelAction.Update(gameTime))
		{
			TravelAction = null;
		}
	}

	public bool IsEndLocation()
	{
		return TravelAction == null;
	}

	public MissionStop GetEnd()
	{
		if (IsEndLocation())
		{
			return this;
		}
		return TravelAction.ToMissionStop.GetEnd();
	}

	public void SetParentPostLoad(Mission parent)
	{
		mission = parent;
		if (TravelAction != null)
		{
			TravelAction.SetParentPostLoad(parent);
		}
		foreach (MissionAction action in Actions)
		{
			action.parent = parent;
		}
	}

	public MissionStopID GetUniqueID()
	{
		IDCounter++;
		if ((ulong)IDCounter >= ulong.MaxValue)
		{
			throw new Exception("Astounding, MissionStopID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != MissionStopID.Invalid)
		{
			LookUp<MissionStop, MissionStopID>.Add(ID, this);
		}
	}

	public void SetInvalid()
	{
		id = MissionStopID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUp<MissionStop, MissionStopID>.Remove(this);
	}

	void ILookUp<MissionStop, MissionStopID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = MissionStopID.First;
	}

	void ILookUp<MissionStop, MissionStopID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<MissionStop, MissionStopID>.Create();
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = sn.DoEnum(id);
		IDCounter = sn.DoEnum(IDCounter);
		Actions = sn.DoQueue(Actions);
		snapshotMissionStopTemplate = sn.SnapshotID<MissionStopTemplate, MissionStopTemplateID>(MissionStopTemplate).Value;
		TravelAction = (TravelAction)sn.DoISnapshot(TravelAction);
		sn.Ignore(mission);
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
		MissionStopTemplate = LookUp<MissionStopTemplate, MissionStopTemplateID>.FindByID(snapshotMissionStopTemplate);
		if (TravelAction != null)
		{
			TravelAction.LoadPostProcess(sn);
		}
		foreach (MissionAction action in Actions)
		{
			action.LoadPostProcess(sn);
		}
	}
}
