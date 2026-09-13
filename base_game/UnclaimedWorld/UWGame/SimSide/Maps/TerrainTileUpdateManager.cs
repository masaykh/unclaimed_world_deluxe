using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;

namespace UWGame.SimSide.Maps;

public class TerrainTileUpdateManager : ICyclable, ILookUp<ICyclable, CyclableID>, ISnapshot
{
	private int rowCounter;

	private double deltaTimeInSeconds;

	private Regulator regulator;

	private const double oneOverThousand = 0.001;

	private static double totalComputationAllInstancesInSeconds;

	private CyclableID id = CyclableID.Invalid;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsPaused { get; set; }

	public double? UpdateInterval => 10.0;

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

	public TerrainTileUpdateManager()
	{
		if (!Snapshotter.IsSnapshotting)
		{
			AddToLookup();
			CreateRegulators();
		}
	}

	private void CreateRegulators()
	{
		regulator = new Regulator(The.Sim.GameplayRandomGenerator, 1.0 / UpdateInterval.Value, ToString());
	}

	public void Update(GameTime gameTime)
	{
		if (!The.Sim.CycleManager.IsRegistered(this))
		{
			double millisecondsSinceLastReady = 0.0;
			if (regulator.IsReady(ref millisecondsSinceLastReady))
			{
				deltaTimeInSeconds = 0.001 * millisecondsSinceLastReady;
				rowCounter = 0;
				The.Sim.CycleManager.Register(this, CycleManager.Priority.Medium);
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
		text.Append($"TerrainTileUpdateManager: ");
	}

	public bool CycleOnce()
	{
		TerrainTile[][] tileMap = The.Map.TileMap;
		int jaggedArrayWidth = Common.GetJaggedArrayWidth(tileMap);
		Parallel.For(0, jaggedArrayWidth, delegate(int x)
		{
			tileMap[x][rowCounter].UpdateSimulationInParallel(deltaTimeInSeconds);
		});
		rowCounter++;
		if (rowCounter == Common.GetJaggedArrayHeight(tileMap))
		{
			rowCounter = 0;
			return true;
		}
		return false;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		sn.Ignore(totalComputationAllInstancesInSeconds);
		sn.Ignore(ComputationTimeSpentInSeconds);
		sn.Ignore(StartedOnTimeInSeconds);
		sn.Ignore(regulator);
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
		CreateRegulators();
	}
}
