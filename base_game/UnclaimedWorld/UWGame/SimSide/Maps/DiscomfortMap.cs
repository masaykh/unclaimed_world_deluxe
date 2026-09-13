using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps;

public class DiscomfortMap : IMap, ILookUp<IMap, IMapID>, ISnapshot
{
	private enum Phase
	{
		InitSectors,
		ComputeChildMaps,
		GetSectorsToAdd,
		ClearSectors,
		AddChildMaps,
		RemoveEmptySectors
	}

	public byte MaxValue = 200;

	public ProtectionLevel ProtectionLevel;

	public string IDName;

	private List<Dependence> children = new List<Dependence>();

	private Phase phase;

	private HashSet<Point> childSectorsThatHaveChangedCC = new HashSet<Point>();

	private HashSet<Point> childSectorsThatHaveBeenRemovedCR = new HashSet<Point>();

	private HashSet<Point> sectorsToRemove = new HashSet<Point>();

	private HashSet<Point> sectorsToReAdd = new HashSet<Point>();

	private HashSet<Point> previousChildSectors = new HashSet<Point>();

	private int childMapIndex;

	private IMapID id = IMapID.Invalid;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsReady { get; set; }

	public TileLayer Map { get; private set; }

	public List<Dependence> Children
	{
		get
		{
			return children;
		}
		set
		{
			children = value;
		}
	}

	public IMapID ID
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

	public DiscomfortMap()
	{
	}

	public DiscomfortMap(ProtectionLevel protectionLevel, ThreatMap childThreatMap)
	{
		((ILookUp<IMap, IMapID>)this).AddToLookup();
		ProtectionLevel = protectionLevel;
		Map = new TileLayer((ushort)The.Map.mapTileWidth, (ushort)The.Map.mapTileHeight);
		Dependence item = new Dependence(childThreatMap, 1f);
		Children.Add(item);
	}

	public IMap GetCurrent()
	{
		return this;
	}

	public void Destroy()
	{
		RemoveIDEntry();
	}

	private void GetSectorsToReAdd()
	{
		sectorsToReAdd.Clear();
		sectorsToReAdd.UnionWith(childSectorsThatHaveChangedCC);
	}

	private void ClearSectorsBeforeReadding()
	{
		Parallel.ForEach(sectorsToReAdd, delegate(Point s)
		{
			Map.Sectors[s.X][s.Y]?.ClearMap();
		});
	}

	private void SetDirtyFlagToNotifyMoveMap()
	{
		foreach (Point item in childSectorsThatHaveChangedCC)
		{
			TileSector orCreateSector = Map.GetOrCreateSector(item.X, item.Y);
			if (orCreateSector != null)
			{
				orCreateSector.IsDirty = true;
			}
		}
	}

	private void AddSectors()
	{
		foreach (Point item in sectorsToReAdd)
		{
			Map.GetOrCreateSector(item.X, item.Y);
		}
		Parallel.ForEach(sectorsToReAdd, delegate(Point sector)
		{
			TileSector sector2 = Map.GetSector(sector.X, sector.Y);
			foreach (Dependence child in Children)
			{
				TileSector tileSector = child.Child.Map.Sectors[sector.X][sector.Y];
				if (tileSector != null)
				{
					sector2.AddSector(tileSector, child.Weight);
				}
			}
		});
	}

	public bool DoCycle()
	{
		switch (phase)
		{
		case Phase.InitSectors:
			Map.IterateSectors(delegate(TileSector s)
			{
				s.IsDirty = false;
			});
			phase = Phase.ComputeChildMaps;
			return false;
		case Phase.ComputeChildMaps:
			if (Children[childMapIndex].Child.DoCycle())
			{
				childMapIndex++;
				if (childMapIndex == Children.Count)
				{
					phase = Phase.GetSectorsToAdd;
					childMapIndex = 0;
				}
			}
			return false;
		case Phase.GetSectorsToAdd:
			GetChildSectorsThatHaveChanged();
			GetChildSectorsThatHaveBeenRemoved();
			phase = Phase.ClearSectors;
			return false;
		case Phase.ClearSectors:
			GetSectorsToReAdd();
			ClearSectorsBeforeReadding();
			phase = Phase.AddChildMaps;
			return false;
		case Phase.AddChildMaps:
			AddSectors();
			phase = Phase.RemoveEmptySectors;
			return false;
		case Phase.RemoveEmptySectors:
			RemoveEmptySectors();
			SetDirtyFlagToNotifyMoveMap();
			phase = Phase.InitSectors;
			return true;
		default:
			return true;
		}
	}

	private void RemoveEmptySectors()
	{
		HashSet<Point> allSectors = Map.GetAllSectors();
		sectorsToRemove.Clear();
		foreach (Point item in allSectors)
		{
			if (Children.TrueForAll((Dependence d) => d.Child.Map.GetSector(item.X, item.Y) == null))
			{
				sectorsToRemove.Add(item);
			}
		}
		Map.RemoveSectors(sectorsToRemove);
	}

	private void GetChildSectorsThatHaveChanged()
	{
		childSectorsThatHaveChangedCC.Clear();
		foreach (Dependence child in Children)
		{
			child.Child.Map.GetDirtySectors(ref childSectorsThatHaveChangedCC);
		}
	}

	private void GetChildSectorsThatHaveBeenRemoved()
	{
		childSectorsThatHaveBeenRemovedCR.Clear();
		childSectorsThatHaveBeenRemovedCR.UnionWith(previousChildSectors);
		previousChildSectors.Clear();
		foreach (Dependence child in Children)
		{
			HashSet<Point> allSectors = child.Child.Map.GetAllSectors();
			childSectorsThatHaveBeenRemovedCR.ExceptWith(allSectors);
			previousChildSectors.UnionWith(allSectors);
		}
	}

	public IMapID SnapshotID(Snapshotter sn, IMapID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != IMapID.Invalid)
		{
			LookUp<IMap, IMapID>.Add(ID, this);
		}
	}

	public IMapID GetUniqueID()
	{
		return IMapCounter.GetUniqueID();
	}

	public void SetInvalid()
	{
		id = IMapID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUp<IMap, IMapID>.Remove(this);
	}

	void ILookUp<IMap, IMapID>.ResetIDCounter()
	{
	}

	void ILookUp<IMap, IMapID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<IMap, IMapID>.Create();
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = SnapshotID(sn, id);
		Map = (TileLayer)sn.DoISnapshot(Map);
		childMapIndex = sn.DoInt32(childMapIndex);
		children = sn.DoList(children);
		IDName = sn.DoString(IDName);
		previousChildSectors = sn.DoHashSet(previousChildSectors);
		MaxValue = sn.DoByte(MaxValue);
		phase = sn.DoEnum(phase);
		ProtectionLevel = sn.DoEnum(ProtectionLevel);
		IsReady = sn.DoBool(IsReady);
		sectorsToReAdd = sn.DoHashSet(sectorsToReAdd);
		childSectorsThatHaveChangedCC = sn.DoHashSet(childSectorsThatHaveChangedCC);
		childSectorsThatHaveBeenRemovedCR = sn.DoHashSet(childSectorsThatHaveBeenRemovedCR);
		sectorsToRemove = sn.DoHashSet(sectorsToRemove);
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
		foreach (Dependence child in Children)
		{
			child.LoadPostProcess(sn);
		}
		Map.LoadPostProcess(sn);
	}
}
