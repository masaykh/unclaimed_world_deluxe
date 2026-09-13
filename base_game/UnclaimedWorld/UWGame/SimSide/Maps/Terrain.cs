using System;
using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Soil;
using UWGame.SimSide.Vegetation;

namespace UWGame.SimSide.Maps;

public class Terrain : ILookUp<Terrain, TerrainID>, ISnapshot
{
	public Dictionary<LowVegetationType, LowVegetation> Vegetation;

	private Dictionary<string, LowVegetationID> snapshotVegetation;

	public Dictionary<SoilComponentType, SoilComponent> SoilComponents;

	public float TerrainDepth;

	public float LevelBelowWater;

	public SurfaceType SurfaceType;

	private bool snapshotSurfaceTypeIsPlains;

	public TerrainTile Parent;

	private TerrainTileID snapshotParent;

	private const float maxAmountPerSubtile = 1f / 9f;

	private TerrainID id = TerrainID.Invalid;

	private static TerrainID IDCounter = TerrainID.First;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public TerrainID ID
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

	public Terrain(TerrainTile parent)
	{
		AddToLookup();
		Parent = parent;
	}

	public Terrain()
	{
	}

	public bool IsUnderWater()
	{
		return LevelBelowWater > 0f;
	}

	public void AddVegetation(LowVegetationType vegType, float amount)
	{
		if (Vegetation == null)
		{
			Vegetation = new Dictionary<LowVegetationType, LowVegetation>();
		}
		if (!Vegetation.TryGetValue(vegType, out var value))
		{
			value = new LowVegetation(this, vegType);
			Vegetation.Add(vegType, value);
		}
		value.Amount += amount;
		value.Amount = Common.ClampBottom(value.Amount, 0f);
	}

	public void AddSoilComponent(SoilComponentType soilType, float amount)
	{
		if (SoilComponents == null)
		{
			SoilComponents = new Dictionary<SoilComponentType, SoilComponent>();
		}
		if (!SoilComponents.TryGetValue(soilType, out var value))
		{
			value = new SoilComponent(this, soilType);
			SoilComponents.Add(soilType, value);
		}
		float f = value.Amount + amount;
		f = Common.ClampBottom(f, 0f);
		value.Amount = f;
	}

	public float GetRoughness()
	{
		float num = 0f;
		if (SoilComponents != null)
		{
			num = SoilComponents.Sum((KeyValuePair<SoilComponentType, SoilComponent> s) => s.Value.SoilComponentType.MoveFactor * s.Value.Amount);
		}
		float num2 = 0f;
		if (Vegetation != null)
		{
			num2 = Vegetation.Sum((KeyValuePair<LowVegetationType, LowVegetation> s) => s.Value.LowVegetationType.MoveFactor * s.Value.Amount);
		}
		float num3 = num + num2;
		if (IsSubtileTerrain())
		{
			num3 *= 9f;
		}
		return Common.ClampTop(num3, 1f);
	}

	private float GetMaxAmountForResources()
	{
		if (IsSubtileTerrain())
		{
			return 1f / 9f;
		}
		return 1f;
	}

	public void RecomputeDisplayAmounts()
	{
		if (Vegetation != null)
		{
			foreach (KeyValuePair<LowVegetationType, LowVegetation> item in Vegetation)
			{
				item.Value.RecomputeDisplayAmount();
			}
		}
		if (SoilComponents == null)
		{
			return;
		}
		foreach (KeyValuePair<SoilComponentType, SoilComponent> soilComponent in SoilComponents)
		{
			soilComponent.Value.RecomputeDisplayAmount();
		}
	}

	public void NormalizeVegetation()
	{
		if (Vegetation == null)
		{
			return;
		}
		float num = 0f;
		float maxAmountForResources = GetMaxAmountForResources();
		foreach (KeyValuePair<LowVegetationType, LowVegetation> item in Vegetation)
		{
			num += item.Value.Amount;
		}
		if (!(num > maxAmountForResources))
		{
			return;
		}
		float num2 = maxAmountForResources / num;
		foreach (KeyValuePair<LowVegetationType, LowVegetation> item2 in Vegetation)
		{
			item2.Value.Amount = num2 * item2.Value.Amount;
		}
	}

	public void NormalizeSoil()
	{
		if (SoilComponents == null)
		{
			return;
		}
		float num = 0f;
		float maxAmountForResources = GetMaxAmountForResources();
		foreach (KeyValuePair<SoilComponentType, SoilComponent> soilComponent in SoilComponents)
		{
			num += soilComponent.Value.Amount;
		}
		if (!(num > maxAmountForResources))
		{
			return;
		}
		float num2 = maxAmountForResources / num;
		foreach (KeyValuePair<SoilComponentType, SoilComponent> soilComponent2 in SoilComponents)
		{
			soilComponent2.Value.Amount = soilComponent2.Value.Amount * num2;
		}
	}

	public bool IsSubtileTerrain()
	{
		return Parent.Terrain != this;
	}

	private void GetSubtileCoords(out int? sx, out int? sy)
	{
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				if (Parent.TerrainSubtiles[i][j] == this)
				{
					sx = i;
					sy = j;
					return;
				}
			}
		}
		sx = null;
		sy = null;
	}

	public TerrainID GetUniqueID()
	{
		IDCounter++;
		if ((ulong)IDCounter >= ulong.MaxValue)
		{
			throw new Exception("Astounding, TerrainID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public TerrainID SnapshotID(Snapshotter sn, TerrainID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != TerrainID.Invalid)
		{
			LookUpSortedDictionary<Terrain, TerrainID>.Add(ID, this);
		}
	}

	public void SetInvalid()
	{
		id = TerrainID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUpSortedDictionary<Terrain, TerrainID>.Remove(this);
	}

	void ILookUp<Terrain, TerrainID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = TerrainID.First;
	}

	void ILookUp<Terrain, TerrainID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUpSortedDictionary<Terrain, TerrainID>.Create();
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = SnapshotID(sn, id);
		IDCounter = sn.DoEnum(IDCounter);
		if (sn.mode != Snapshotter.Mode.Load && Vegetation != null)
		{
			snapshotVegetation = Vegetation.ToDictionary((KeyValuePair<LowVegetationType, LowVegetation> v) => v.Key.KeyName, (KeyValuePair<LowVegetationType, LowVegetation> v) => v.Value.ID);
		}
		LevelBelowWater = sn.DoFloat(LevelBelowWater);
		SoilComponents = sn.DoDictionary(SoilComponents);
		TerrainDepth = sn.DoFloat(TerrainDepth);
		snapshotVegetation = sn.DoDictionary(snapshotVegetation);
		snapshotParent = sn.SnapshotID<TerrainTile, TerrainTileID>(Parent).Value;
		if (sn.mode != Snapshotter.Mode.Load)
		{
			if (SurfaceType.Name.Contains("Water"))
			{
				snapshotSurfaceTypeIsPlains = false;
			}
			else
			{
				snapshotSurfaceTypeIsPlains = true;
			}
		}
		snapshotSurfaceTypeIsPlains = sn.DoBool(snapshotSurfaceTypeIsPlains);
		sn.Ignore(SurfaceType);
		sn.Ignore(Vegetation);
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
		Parent = LookUpSortedDictionary<TerrainTile, TerrainTileID>.FindByID(snapshotParent);
		if (snapshotVegetation != null)
		{
			Vegetation = snapshotVegetation.ToDictionary((KeyValuePair<string, LowVegetationID> v) => GameData.Instance.AllLowVegetationTypes[v.Key], (KeyValuePair<string, LowVegetationID> v) => LookUpSortedDictionary<LowVegetation, LowVegetationID>.FindByID(v.Value));
		}
		snapshotVegetation = null;
		if (snapshotSurfaceTypeIsPlains)
		{
			SurfaceType = PlainsType.Instance;
		}
		else
		{
			SurfaceType = WaterType.Instance;
		}
		if (SoilComponents == null)
		{
			return;
		}
		foreach (KeyValuePair<SoilComponentType, SoilComponent> soilComponent in SoilComponents)
		{
			soilComponent.Value.LoadPostProcess(sn);
		}
	}
}
