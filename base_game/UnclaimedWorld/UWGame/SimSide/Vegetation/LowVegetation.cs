using System;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Soil;
using UWGame.SimSide.Trees;

namespace UWGame.SimSide.Vegetation;

public class LowVegetation : RenderedTerrainComponent, IHasCrops, ILookUp<IHasCrops, HasCropsID>, ILookUp<LowVegetation, LowVegetationID>
{
	public LowVegetationType LowVegetationType;

	public float Lushness;

	private LowVegetationID id = LowVegetationID.Invalid;

	private static LowVegetationID IDCounter = LowVegetationID.First;

	private HasCropsID hasCropsID;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public LowVegetationID ID
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

	public Point MapPosition => new Point(Parent.Parent.X, Parent.Parent.Y);

	public Vector3 AccessPoint => MapManager.TileToWorldPos(new Point(Parent.Parent.X, Parent.Parent.Y));

	public Vector3 Location => MapManager.TileToWorldPos(new Point(Parent.Parent.X, Parent.Parent.Y));

	HasCropsID ILookUp<IHasCrops, HasCropsID>.ID => hasCropsID;

	protected override float ComputeDisplayAmount()
	{
		float num = 1f;
		if (Parent.IsSubtileTerrain())
		{
			num = 9f;
		}
		return Common.ClampTop(num * 1.4f * base.Amount, 1f);
	}

	public LowVegetation()
	{
	}

	public LowVegetation(Terrain parent, LowVegetationType type)
		: base(parent)
	{
		AddToLookup();
		((ILookUp<IHasCrops, HasCropsID>)this).AddToLookup();
		LowVegetationType = type;
	}

	public void Destroy()
	{
		RemoveIDEntry();
	}

	public LowVegetationID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= LowVegetationID.Invalid)
		{
			throw new Exception("Astounding, LowVegetationID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public LowVegetationID SnapshotID(Snapshotter sn, LowVegetationID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != LowVegetationID.Invalid)
		{
			LookUpSortedDictionary<LowVegetation, LowVegetationID>.Add(ID, this);
		}
	}

	public void SetInvalid()
	{
		id = LowVegetationID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUpSortedDictionary<LowVegetation, LowVegetationID>.Remove(this);
	}

	void ILookUp<LowVegetation, LowVegetationID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = LowVegetationID.First;
	}

	void ILookUp<LowVegetation, LowVegetationID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUpSortedDictionary<LowVegetation, LowVegetationID>.Create();
	}

	HasCropsID ILookUp<IHasCrops, HasCropsID>.GetUniqueID()
	{
		return HasCrops.GetUniqueID();
	}

	void ILookUp<IHasCrops, HasCropsID>.AddToLookup()
	{
		hasCropsID = ((ILookUp<IHasCrops, HasCropsID>)this).GetUniqueID();
		if (hasCropsID != HasCropsID.Invalid)
		{
			LookUpIHasCrops.Add(hasCropsID, this);
		}
	}

	void ILookUp<IHasCrops, HasCropsID>.RemoveIDEntry()
	{
		LookUpIHasCrops.Remove(this);
	}

	void ILookUp<IHasCrops, HasCropsID>.ResetIDCounter()
	{
	}

	void ILookUp<IHasCrops, HasCropsID>.SetInvalid()
	{
		hasCropsID = HasCropsID.Invalid;
	}

	void ILookUp<IHasCrops, HasCropsID>.CreateLookupCollection()
	{
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		id = SnapshotID(sn, id);
		IDCounter = sn.DoEnum(IDCounter);
		hasCropsID = sn.DoEnum(hasCropsID);
		LowVegetationType = sn.DoGameData(LowVegetationType);
		Lushness = sn.DoFloat(Lushness);
		return this;
	}
}
