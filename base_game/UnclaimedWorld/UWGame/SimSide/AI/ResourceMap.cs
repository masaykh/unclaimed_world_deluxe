using System;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;

namespace UWGame.SimSide.AI;

public class ResourceMap : ICyclable, ILookUp<ICyclable, CyclableID>, ISnapshot, IIDEventSubscriber
{
	private enum Phase
	{
		Clear,
		Draw
	}

	public enum Result
	{
		OK,
		Wait
	}

	private byte[][] values;

	private byte[][] newValues;

	private int cropsToDrawPerCycle = 10;

	private int cropCounter;

	private Phase phase;

	public const byte MaxValue = 100;

	private ResourceType ResourceType;

	private bool isDirty = true;

	private Regulator regulator;

	private MethodID cropsMap_ListItemRemovedMethodID;

	public IDActionEvent FinishedEvent;

	private static double totalComputationAllInstancesInSeconds;

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

	public double? UpdateInterval => 30.0;

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

	public ResourceMap()
	{
	}

	public ResourceMap(ResourceType resourceType)
	{
		ResourceType = resourceType;
		if (!The.Sim.PlaySite.Resources.TryGetValue(resourceType, out var value))
		{
			value = new ObservableList<ResourceContainer>();
			The.Sim.PlaySite.Resources.Add(resourceType, value);
		}
		value.ListMemberRemoved.AddAndRegister((Action<int>)CropsMap_ListItemRemoved, (IIDEventSubscriber)this, out cropsMap_ListItemRemovedMethodID);
		AddToLookup();
		CreateRegulators();
	}

	private void CreateRegulators()
	{
		regulator = new Regulator(The.Sim.GameplayRandomGenerator, 1.0 / UpdateInterval.Value, "ResourceMap");
	}

	private void CropsMap_ListItemRemoved(int indexOfRemovedItem)
	{
		ObservableList<Entity>.UpdateCounterWhenItemIsRemoved(ref cropCounter, indexOfRemovedItem);
	}

	public void Update(GameTime gameTime)
	{
		if (regulator.IsReady())
		{
			isDirty = true;
		}
	}

	public void Destroy()
	{
		if (The.Sim.CycleManager.IsRegistered(this))
		{
			The.Sim.CycleManager.UnRegister(this);
		}
		ActionLookup<int>.Remove(cropsMap_ListItemRemovedMethodID);
		RemoveIDEntry();
	}

	public Result GetMap(ref byte[][] outValues)
	{
		if (isDirty)
		{
			if (!The.Sim.CycleManager.IsRegistered(this))
			{
				cropCounter = 0;
				if (values == null)
				{
					int mapTileWidth = The.Map.mapTileWidth;
					int mapTileHeight = The.Map.mapTileHeight;
					Common.InitJaggedArray(ref values, mapTileWidth, mapTileHeight);
					Common.InitJaggedArray(ref newValues, mapTileWidth, mapTileHeight);
				}
				else
				{
					Common.ClearJaggedArray(newValues);
				}
				The.Sim.CycleManager.Register(this, CycleManager.Priority.Medium);
			}
			return Result.Wait;
		}
		outValues = values;
		return Result.OK;
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
		text.Append($"ResourceMap {ID}:");
	}

	public bool CycleOnce()
	{
		switch (phase)
		{
		case Phase.Clear:
			Common.ClearMap(values);
			phase = Phase.Draw;
			return false;
		case Phase.Draw:
		{
			ObservableList<ResourceContainer> observableList = The.Sim.PlaySite.Resources[ResourceType];
			int num = cropCounter;
			int num2 = Common.Min(cropCounter + cropsToDrawPerCycle, observableList.Count);
			Point zero = Point.Zero;
			for (int i = num; i < num2; i++)
			{
				ResourceContainer resourceContainer = observableList[i];
				zero = resourceContainer.MapPosition;
				int val = (int)(8f * resourceContainer.TotalHarvestableBulk);
				val = Math.Min(val, 100);
				if (resourceContainer.TotalHarvestableBulk > 0f)
				{
					val = Math.Max(1, val);
				}
				if (val > 0)
				{
					InfluenceMap.DrawLinearInfluenceCircle(newValues, zero, val, InfluenceMap.Operation.AddToExisting, InfluenceMap.Falloff.Yes, InfluenceMap.CircleParameter.FalloffEachTile, 2);
				}
			}
			cropCounter = num2;
			if (num2 == observableList.Count)
			{
				Common.CopyJaggedArray(newValues, values);
				cropCounter = 0;
				phase = Phase.Clear;
				isDirty = false;
				if (FinishedEvent != null)
				{
					FinishedEvent.Invoke();
				}
				return true;
			}
			return false;
		}
		default:
			return false;
		}
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = SnapshotID(sn, id);
		phase = sn.DoEnum(phase);
		IsPaused = sn.DoBool(IsPaused);
		cropsMap_ListItemRemovedMethodID = sn.DoEnum(cropsMap_ListItemRemovedMethodID);
		cropCounter = sn.DoInt32(cropCounter);
		cropsToDrawPerCycle = sn.DoInt32(cropsToDrawPerCycle);
		FinishedEvent = (IDActionEvent)sn.DoISnapshot(FinishedEvent);
		isDirty = sn.DoBool(isDirty);
		newValues = sn.DoJaggedArray(newValues);
		values = sn.DoJaggedArray(values);
		ResourceType = sn.DoGameData(ResourceType);
		sn.Ignore(regulator);
		sn.Ignore(totalComputationAllInstancesInSeconds);
		sn.Ignore(ComputationTimeSpentInSeconds);
		sn.Ignore(StartedOnTimeInSeconds);
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
		LoadPostProcessRegisterMethodIDs();
		CreateRegulators();
	}

	public void LoadPostProcessRegisterMethodIDs()
	{
		ActionLookup<int>.Add(cropsMap_ListItemRemovedMethodID, CropsMap_ListItemRemoved);
	}
}
