using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Processes;

public class ProcessMemory : IKnownProcess, ILookUp<ProcessMemory, ProcessMemoryID>, ISnapshot
{
	public bool RealProcessIsCompleted;

	private Vector3? computedProductionSiteLocation;

	private Vector3? computedCurrentLocation;

	private Vector3? computedFixedJobLocation;

	private float progress;

	private bool isCompleted;

	private bool outputExists;

	private ProcessMemoryID id = ProcessMemoryID.Invalid;

	private static ProcessMemoryID IDCounter;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public SimProcessID ProcessID { get; set; }

	public Point? MapPosition { get; set; }

	public bool IsStarted { get; set; }

	public ProcessType ProcessType { get; private set; }

	public Dictionary<EntityType, List<Tuple<EntityID, WorldLocation>>> AssignedInputs { get; set; }

	public EntityID? ImmovableTool { get; set; }

	public EntityID? ImmovableInput { get; set; }

	public Vector3? GroundLocation { get; set; }

	public EntityID? ContainerToPlaceOutputsIn { get; set; }

	public List<EntityID> OutputEntities { get; private set; }

	public UpgradeCategory UpgradeCategory { get; set; }

	public List<EntityID> StationaryTools { get; set; }

	public EntityAndRoot? ActingOnEntity { get; set; }

	public float ProgressSpeed { get; set; }

	public Productivity Productivity { get; set; }

	public ProcessMemoryID ID
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

	public ProcessMemory()
	{
	}

	public ProcessMemory(SimProcess process)
	{
		AddToLookup();
		ProcessID = process.ID;
	}

	public bool Init(SimProcess process, SharedKnowledge sharedKnowledge)
	{
		ImmovableTool = process.ImmovableTool;
		ImmovableInput = process.ImmovableInput;
		ActingOnEntity = process.ActingOnEntity;
		GroundLocation = process.GroundLocation;
		MapPosition = process.MapPosition;
		IsStarted = process.IsStarted;
		ProcessType = process.ProcessType;
		UpgradeCategory = process.UpgradeCategory;
		StationaryTools = process.StationaryTools;
		if (process.OutputEntities != null)
		{
			OutputEntities = new List<EntityID>(process.OutputEntities);
		}
		if (process.AssignedInputs != null)
		{
			AssignedInputs = new Dictionary<EntityType, List<Tuple<EntityID, WorldLocation>>>();
			foreach (KeyValuePair<EntityType, List<Tuple<EntityID, WorldLocation>>> assignedInput in process.AssignedInputs)
			{
				AssignedInputs.Add(assignedInput.Key, new List<Tuple<EntityID, WorldLocation>>(assignedInput.Value));
			}
		}
		ProgressSpeed = process.ProgressSpeed;
		if (process.Productivity != null)
		{
			Productivity = new Productivity(process.Productivity);
		}
		if (!process.GetProductionSiteLocation(out computedProductionSiteLocation, sharedKnowledge))
		{
			return false;
		}
		if (!process.GetCurrentLocation(out computedCurrentLocation, sharedKnowledge))
		{
			return false;
		}
		if (!process.GetFixedJobLocation(out computedFixedJobLocation, sharedKnowledge))
		{
			return false;
		}
		if (!process.IsCompleted(out isCompleted, sharedKnowledge))
		{
			return false;
		}
		if (!process.OutputExists(out outputExists, sharedKnowledge))
		{
			return false;
		}
		if (!process.GetKnownProgress(sharedKnowledge, out progress))
		{
			return false;
		}
		return true;
	}

	public bool GetKnownProgress(SharedKnowledge sharedKnowledge, out float progress)
	{
		progress = this.progress;
		return true;
	}

	public bool GetProductionSiteLocation(out Vector3? location, SharedKnowledge sharedKnowledge, bool ignoreKnowledge)
	{
		location = computedProductionSiteLocation;
		return true;
	}

	public bool GetCurrentLocation(out Vector3? location, SharedKnowledge sharedKnowledge, bool ignoreKnowledge)
	{
		location = computedCurrentLocation;
		return true;
	}

	public bool GetFixedJobLocation(out Vector3? location, SharedKnowledge sharedKnowledge, bool ignoreKnowledge)
	{
		location = computedFixedJobLocation;
		return true;
	}

	public bool IsCompleted(out bool isCompleted, SharedKnowledge sharedKnowledge)
	{
		isCompleted = this.isCompleted;
		return true;
	}

	public bool OutputExists(out bool outputExists, SharedKnowledge sharedKnowledge)
	{
		outputExists = this.outputExists;
		return true;
	}

	public bool HasFixedLocation()
	{
		return SimProcess.HasFixedLocation(GroundLocation);
	}

	public void Destroy()
	{
		RemoveIDEntry();
	}

	public void SetCompletedProcess(SimProcess process)
	{
		RealProcessIsCompleted = true;
		Productivity = new Productivity(process.Productivity);
	}

	public ProcessMemoryID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= ProcessMemoryID.Invalid)
		{
			throw new Exception("Astounding, ProcessMemoryID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public ProcessMemoryID SnapshotID(Snapshotter sn, ProcessMemoryID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != ProcessMemoryID.Invalid)
		{
			LookUp<ProcessMemory, ProcessMemoryID>.Add(ID, this);
		}
	}

	public void SetInvalid()
	{
		id = ProcessMemoryID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUp<ProcessMemory, ProcessMemoryID>.Remove(this);
	}

	void ILookUp<ProcessMemory, ProcessMemoryID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = ProcessMemoryID.First;
	}

	void ILookUp<ProcessMemory, ProcessMemoryID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<ProcessMemory, ProcessMemoryID>.Create();
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		IDCounter = sn.DoEnum(IDCounter);
		id = SnapshotID(sn, id);
		ProcessID = sn.DoEnum(ProcessID);
		ProcessType = sn.DoGameData(ProcessType);
		ImmovableTool = sn.DoEnumNullable(ImmovableTool);
		ImmovableInput = sn.DoEnumNullable(ImmovableInput);
		ContainerToPlaceOutputsIn = sn.DoEnumNullable(ContainerToPlaceOutputsIn);
		ActingOnEntity = sn.DoEntityAndRootNullable(ActingOnEntity);
		GroundLocation = sn.DoVector3Nullable(GroundLocation);
		IsStarted = sn.DoBool(IsStarted);
		RealProcessIsCompleted = sn.DoBool(RealProcessIsCompleted);
		OutputEntities = sn.DoList(OutputEntities);
		AssignedInputs = sn.DoMultiMap(AssignedInputs);
		StationaryTools = sn.DoList(StationaryTools);
		MapPosition = sn.DoPointNullable(MapPosition);
		UpgradeCategory = sn.DoGameData(UpgradeCategory);
		computedCurrentLocation = sn.DoVector3Nullable(computedCurrentLocation);
		computedFixedJobLocation = sn.DoVector3Nullable(computedFixedJobLocation);
		computedProductionSiteLocation = sn.DoVector3Nullable(computedProductionSiteLocation);
		isCompleted = sn.DoBool(isCompleted);
		outputExists = sn.DoBool(outputExists);
		progress = sn.DoFloat(progress);
		ProgressSpeed = sn.DoFloat(ProgressSpeed);
		Productivity = (Productivity)sn.DoISnapshot(Productivity);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
