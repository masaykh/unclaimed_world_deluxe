using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems;

namespace UWGame.SimSide.Trees;

public class Tree : Component, IHasCrops, ILookUp<IHasCrops, HasCropsID>
{
	private enum SizeType
	{
		Young,
		Grown
	}

	private enum BlockedTransport
	{
		NotPlaced,
		None,
		Car,
		OffRoad,
		Foot
	}

	public Dictionary<ResourceType, Crop> Crops;

	private Dictionary<ResourceType, ResourceID> snapshotCrops;

	private InSeason inSeason;

	private float? ageInYears;

	private float? shapeFactor;

	private const float sizeForUsingMatureSprite = 1f;

	public const float sizeForUsingYoungSprite = 0.25f;

	private const float maxMatureSpriteScaling = 0.1f;

	private const float maxYoungSpriteScaling = 0.1f;

	private static readonly float[] ageProbabilities = new float[8] { 0.2f, 0.4f, 0.6f, 0.8f, 0.88f, 0.95f, 0.98f, 0.99f };

	private float? size;

	private int? flavour;

	private BlockedTransport blockedTransportState;

	private PlantResourceNeeds plantNeeds;

	private PlantResourceNeeds cropNeeds;

	private PlantResourceNeeds sustainNeeds;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	private HasCropsID hasCropsID;

	public InSeason InSeason
	{
		get
		{
			return inSeason;
		}
		set
		{
			if (inSeason != value)
			{
				inSeason = value;
				UpdateRenderableSeason();
			}
		}
	}

	public float AgeInYears
	{
		get
		{
			return ageInYears.Value;
		}
		set
		{
			ageInYears = value;
		}
	}

	public float ShapeFactor
	{
		get
		{
			return shapeFactor.Value;
		}
		set
		{
			shapeFactor = value;
		}
	}

	public float Size
	{
		get
		{
			return size.Value;
		}
		set
		{
			if (value != size)
			{
				size = value;
				UpdateRenderableSize();
			}
		}
	}

	public int Flavour
	{
		get
		{
			return flavour.Value;
		}
		set
		{
			if (flavour != value)
			{
				flavour = value;
				UpdateRenderableFlavour();
			}
		}
	}

	public Point MapPosition => Parent.MapPosition.Value;

	public Vector3 AccessPoint => Parent.AccessPoint.Value;

	public Vector3 Location => Parent.PlaySiteLocation;

	HasCropsID ILookUp<IHasCrops, HasCropsID>.ID => hasCropsID;

	int ILookUp<IHasCrops, HasCropsID>.LoadPostProcessOrder => 0;

	public Tree()
	{
	}

	public void UpdateRenderable()
	{
		UpdateRenderableSeason();
		UpdateRenderableSize();
		UpdateRenderableFlavour();
		if (Parent.EntityType.TreeType.CropTypes != null)
		{
			Parent.Renderable.SetPulsing(Renderable.AdditionalEffect.Outline);
		}
	}

	private void UpdateRenderableSeason()
	{
		if (Parent.Renderable != null)
		{
			Parent.Renderable.SetOrClearSpriteStateFlag(inSeason == InSeason.Winter, StateModifier.Winter);
		}
	}

	private void UpdateRenderableFlavour()
	{
		if (Parent.Renderable != null)
		{
			switch (flavour)
			{
			case 1:
				Parent.SetSpriteStateFlag(StateModifier.Flavour1);
				break;
			case 2:
				Parent.SetSpriteStateFlag(StateModifier.Flavour2);
				break;
			case 3:
				Parent.SetSpriteStateFlag(StateModifier.Flavour3);
				break;
			default:
				Parent.SetSpriteStateFlag(StateModifier.Flavour1);
				break;
			}
		}
	}

	private void UpdateRenderableSize()
	{
		if (Parent.Renderable != null)
		{
			if (GetSize() == SizeType.Young)
			{
				Parent.SetSpriteStateFlag(StateModifier.Young);
			}
			else
			{
				Parent.ClearSpriteStateFlag(StateModifier.Young);
			}
		}
	}

	public void UpdateBulk()
	{
		ComputeSizeFromBulk();
		BlockedTransport blockedTransport = ((Parent.Bulk >= GameData.Instance.Constants.BulkOfTreeBlockingFootTransport) ? BlockedTransport.Foot : ((Parent.Bulk >= GameData.Instance.Constants.BulkOfTreeBlockingATVTransport) ? BlockedTransport.OffRoad : ((!(Parent.Bulk >= GameData.Instance.Constants.BulkOfTreeBlockingCarTransport)) ? BlockedTransport.None : BlockedTransport.Car)));
		if (blockedTransport != blockedTransportState)
		{
			blockedTransportState = blockedTransport;
			if (Parent.HasBeenPlaced() && Parent.PointLayout != null)
			{
				Parent.PointLayout.ClearTerrainCosts(redraw: true);
			}
		}
	}

	private SizeType GetSize()
	{
		if (Size < 0.9f)
		{
			return SizeType.Young;
		}
		return SizeType.Grown;
	}

	private void ComputeSizeFromBulk()
	{
		Size = Parent.Bulk / Parent.EntityType.TreeType.BulkPerSize;
	}

	public Tree(Entity parent)
		: base(parent)
	{
		if (The.Sim.GameplayRandomGenerator.Next(100, "Tree") > 50)
		{
			Parent.FlipHorizontally = true;
		}
		((ILookUp<IHasCrops, HasCropsID>)this).AddToLookup();
	}

	public void SetAgePreInit(float age)
	{
		AgeInYears = age;
	}

	public void SetAgePreInit(AgeGroup ageGroup)
	{
		SetRandomAge(ageGroup);
	}

	public void SetAgePreInit()
	{
		SetRandomAge();
	}

	public override void UpdatePlaySite(GameTime gameTime)
	{
		if (Crops == null)
		{
			return;
		}
		foreach (KeyValuePair<ResourceType, Crop> crop in Crops)
		{
			crop.Value.Update(gameTime);
		}
	}

	public override double? GetUpdateInterval()
	{
		double? currentInterval = null;
		if (Crops != null)
		{
			foreach (KeyValuePair<ResourceType, Crop> crop in Crops)
			{
				UpdateTimePoints.GetSoonestInterval(crop.Value.GetUpdateInterval(), ref currentInterval);
			}
		}
		return currentInterval;
	}

	public void Initialize()
	{
		if (!ageInYears.HasValue)
		{
			SetRandomAge();
		}
		if (!size.HasValue)
		{
			SetRandomSize(AgeInYears);
		}
		if (!flavour.HasValue)
		{
			SetRandomFlavour();
		}
		if (!shapeFactor.HasValue)
		{
			SetRandomShapeFactor();
		}
		Parent.Bulk = Parent.EntityType.TreeType.BulkPerSize * Size;
		if (Parent.EntityType.TreeType.CropTypes != null)
		{
			Crops = new Dictionary<ResourceType, Crop>();
			foreach (ResourceType cropType in Parent.EntityType.TreeType.CropTypes)
			{
				Crop value = new Crop(this, cropType);
				Crops.Add(cropType, value);
			}
		}
		if (Crops == null)
		{
			return;
		}
		foreach (KeyValuePair<ResourceType, Crop> crop in Crops)
		{
			Parent.Site.AddResourceContainer(crop.Value);
		}
	}

	private void SetRandomFlavour()
	{
		if (Parent.EntityType.TreeType.MaxFlavours > 1)
		{
			Flavour = The.Sim.GameplayRandomGenerator.Next(1, Parent.EntityType.TreeType.MaxFlavours + 1, "Tree");
		}
		else
		{
			Flavour = 1;
		}
	}

	private void SetRandomShapeFactor()
	{
		ShapeFactor = (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution(1.0, 0.03);
		ShapeFactor = MathHelper.Clamp(ShapeFactor, 0.9f, 1.1f);
	}

	private void SetRandomAge()
	{
		int stairStepIndex = Common.GetStairStepIndex((float)The.Sim.GameplayRandomGenerator.NextDouble("Tree"), ageProbabilities);
		float num = Parent.EntityType.TreeType.MaxAge / (float)ageProbabilities.Length;
		AgeInYears = ((float)stairStepIndex + (float)The.Sim.GameplayRandomGenerator.NextDouble("Tree")) * num;
	}

	private void SetRandomAge(AgeGroup ageGroup)
	{
		float matureAge = Parent.EntityType.TreeType.MatureAge;
		float maxAge = Parent.EntityType.TreeType.MaxAge;
		float f;
		if (ageGroup == AgeGroup.Young)
		{
			f = (float)The.Sim.GameplayRandomGenerator.NextDouble("Tree") * matureAge;
			f = Common.Clamp(f, 0.0001f, matureAge - 0.0001f);
		}
		else
		{
			float num = maxAge / (float)ageProbabilities.Length;
			int num2 = (int)(matureAge / num);
			f = ((float)Common.GetStairStepIndex(MathHelper.Lerp(ageProbabilities[num2], 1f, (float)The.Sim.GameplayRandomGenerator.NextDouble("Tree")), ageProbabilities) + (float)The.Sim.GameplayRandomGenerator.NextDouble("Tree")) * num;
			f = Common.Clamp(f, matureAge + 0.0001f, maxAge);
		}
		AgeInYears = f;
	}

	public void SetRandomSize(float age)
	{
		if (age < Parent.EntityType.TreeType.MatureAge)
		{
			Size = MathHelper.SmoothStep(0.01f, 1f, age / Parent.EntityType.TreeType.MatureAge);
		}
		else
		{
			Size = 0.05f * age + 0.94f;
		}
	}

	public void GetSizeScaling(ref float width, ref float height)
	{
	}

	public void Place()
	{
		The.Map.GetTile(Parent.MapPosition.Value).AddTree(Parent);
	}

	public void Destroy()
	{
		The.Map.GetTile(Parent.MapPosition.Value).RemoveTree(Parent);
		if (Crops != null)
		{
			foreach (KeyValuePair<ResourceType, Crop> crop in Crops)
			{
				Parent.Site.Resources[crop.Key].Remove(crop.Value);
				crop.Value.Destroy();
			}
		}
		((ILookUp<IHasCrops, HasCropsID>)this).RemoveIDEntry();
	}

	public void GetGrowthNeeds(double deltaTimeInSeconds, out float water, out float nitrogen, out float phosphorous)
	{
		float water2 = 0f;
		float nitrogen2 = 0f;
		float phosphorous2 = 0f;
		if (Crops != null)
		{
			GetCropGrowthNeeds(deltaTimeInSeconds, out water2, out nitrogen2, out phosphorous2);
			cropNeeds = new PlantResourceNeeds
			{
				Water = water2,
				Nitrogen = nitrogen2,
				Phosphorous = phosphorous2
			};
		}
		else
		{
			cropNeeds = null;
		}
		water = water2;
		nitrogen = nitrogen2;
		phosphorous = phosphorous2;
		GetPlantGrowthNeeds(deltaTimeInSeconds, out water2, out nitrogen2, out phosphorous2);
		plantNeeds = new PlantResourceNeeds
		{
			Water = water2,
			Nitrogen = nitrogen2,
			Phosphorous = phosphorous2
		};
		water += water2;
		nitrogen += nitrogen2;
		phosphorous += phosphorous2;
		GetNeedsForPestsAndSelfSustainment(deltaTimeInSeconds, out water2, out nitrogen2, out phosphorous2);
		sustainNeeds = new PlantResourceNeeds
		{
			Water = water2,
			Nitrogen = nitrogen2,
			Phosphorous = phosphorous2
		};
		water += water2;
		nitrogen += nitrogen2;
		phosphorous += phosphorous2;
	}

	private void GetPlantGrowthNeeds(double deltaTimeInSeconds, out float water, out float nitrogen, out float phosphorous)
	{
		float num = (float)(deltaTimeInSeconds * (double)Parent.EntityType.TreeType.BulkGrowthSpeedInSeconds);
		water = num * Parent.EntityType.TreeType.WaterNeedsPerBulk;
		nitrogen = num * Parent.EntityType.TreeType.NitrogenNeedsPerBulk;
		phosphorous = num * Parent.EntityType.TreeType.PhosphorousNeedsPerBulk;
	}

	private void GetNeedsForPestsAndSelfSustainment(double deltaTimeInSeconds, out float water, out float nitrogen, out float phosphorous)
	{
		float num = (float)(deltaTimeInSeconds * (double)Parent.EntityType.TreeType.SelfSustainmentNeedsInBulkPercentagePerSecond * (double)Parent.Bulk);
		water = num * Parent.EntityType.TreeType.WaterNeedsPerBulk;
		nitrogen = num * Parent.EntityType.TreeType.NitrogenNeedsPerBulk;
		phosphorous = num * Parent.EntityType.TreeType.PhosphorousNeedsPerBulk;
	}

	private void GetCropGrowthNeeds(double deltaTimeInSeconds, out float water, out float nitrogen, out float phosphorous)
	{
		water = 0f;
		nitrogen = 0f;
		phosphorous = 0f;
		foreach (KeyValuePair<ResourceType, Crop> crop in Crops)
		{
			crop.Value.GetGrowthNeeds(deltaTimeInSeconds, out var water2, out var nitrogen2, out var phosphorous2, AgeInYears, Parent.Bulk);
			water += water2;
			nitrogen += nitrogen2;
			phosphorous += phosphorous2;
		}
	}

	public void Grow(ref float water, ref float nitrogen, ref float phosphorous)
	{
		Parent.Bulk += ProducePlantBulk(ref water, ref nitrogen, ref phosphorous);
		if (Crops == null)
		{
			return;
		}
		foreach (KeyValuePair<ResourceType, Crop> crop in Crops)
		{
			crop.Value.GrowAndRipen(AgeInYears, Parent.Bulk);
		}
	}

	private float ProducePlantBulk(ref float water, ref float nitrogen, ref float phosphorous)
	{
		float num = phosphorous / Parent.EntityType.TreeType.WaterNeedsPerBulk;
		float num2 = water / Parent.EntityType.TreeType.PhosphorousNeedsPerBulk;
		float num3 = nitrogen / Parent.EntityType.TreeType.NitrogenNeedsPerBulk;
		float num4 = 0f;
		num4 = ((num < num2) ? ((!(num < num3)) ? num3 : num) : ((!(num2 < num3)) ? num3 : num2));
		Consume(num4, ref water, ref nitrogen, ref phosphorous);
		return num4;
	}

	private void Consume(float bulkToGrow, ref float water, ref float nitrogen, ref float phosphorous)
	{
		water -= bulkToGrow * Parent.EntityType.TreeType.WaterNeedsPerBulk;
		nitrogen -= bulkToGrow * Parent.EntityType.TreeType.NitrogenNeedsPerBulk;
		phosphorous -= bulkToGrow * Parent.EntityType.TreeType.PhosphorousNeedsPerBulk;
	}

	public void LoseMass(double deltaTimeInSeconds, out float fibrousMaterial, out float nonFibrousMaterial)
	{
		float num = The.Sim.GameplayRandomGenerator.RandomBetween(0.5f, 1.2f);
		float num2 = (float)(deltaTimeInSeconds * (double)num * (double)Parent.EntityType.TreeType.MassLossPercentagePerSecond * (double)Parent.Bulk);
		if (Parent.EntityType.TreeType.FibrousPercentageOfTotalMass > 0f)
		{
			The.Sim.GameplayRandomGenerator.RandomBetween(0.5f, 1.2f);
			fibrousMaterial = Parent.EntityType.TreeType.FibrousPercentageOfTotalMass * num2;
		}
		else
		{
			fibrousMaterial = 0f;
		}
		nonFibrousMaterial = num2 - fibrousMaterial;
		Parent.Bulk -= num2;
	}

	public void RedrawTerrainCosts()
	{
		TerrainTile tile = The.Map.GetTile(Parent.MapPosition.Value);
		switch (blockedTransportState)
		{
		case BlockedTransport.Foot:
			The.Map.SetSubtileCost(Parent.PlaySiteLocation, SurfaceType.TransportType.Foot, 0);
			The.Map.SetSubtileCost(Parent.PlaySiteLocation, SurfaceType.TransportType.OffRoad, 0);
			The.Map.SetSubtileCost(Parent.PlaySiteLocation, SurfaceType.TransportType.Car, 0);
			break;
		case BlockedTransport.OffRoad:
			The.Map.SetSubtileCost(Parent.PlaySiteLocation, SurfaceType.TransportType.Foot, tile.GetCost(SurfaceType.TransportType.Foot, SurfaceType.TerrainFeatures.Obstacle));
			The.Map.SetSubtileCost(Parent.PlaySiteLocation, SurfaceType.TransportType.OffRoad, 0);
			The.Map.SetSubtileCost(Parent.PlaySiteLocation, SurfaceType.TransportType.Car, 0);
			break;
		case BlockedTransport.Car:
			The.Map.SetSubtileCost(Parent.PlaySiteLocation, SurfaceType.TransportType.Foot, tile.GetCost(SurfaceType.TransportType.Foot, SurfaceType.TerrainFeatures.Obstacle));
			The.Map.SetSubtileCost(Parent.PlaySiteLocation, SurfaceType.TransportType.OffRoad, tile.GetCost(SurfaceType.TransportType.OffRoad, SurfaceType.TerrainFeatures.Obstacle));
			The.Map.SetSubtileCost(Parent.PlaySiteLocation, SurfaceType.TransportType.Car, 0);
			break;
		case BlockedTransport.None:
			The.Map.SetSubtileCost(Parent.PlaySiteLocation, SurfaceType.TransportType.Foot, tile.GetCost(SurfaceType.TransportType.Foot, SurfaceType.TerrainFeatures.Obstacle));
			The.Map.SetSubtileCost(Parent.PlaySiteLocation, SurfaceType.TransportType.OffRoad, tile.GetCost(SurfaceType.TransportType.OffRoad, SurfaceType.TerrainFeatures.Obstacle));
			The.Map.SetSubtileCost(Parent.PlaySiteLocation, SurfaceType.TransportType.Car, tile.GetCost(SurfaceType.TransportType.Car, SurfaceType.TerrainFeatures.Obstacle));
			break;
		}
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		hasCropsID = sn.DoEnum(hasCropsID);
		if (sn.mode != Snapshotter.Mode.Load && Crops != null)
		{
			snapshotCrops = Crops.ToDictionary((KeyValuePair<ResourceType, Crop> k) => k.Key, (KeyValuePair<ResourceType, Crop> k) => k.Value.ID);
		}
		ageInYears = sn.DoFloatNullable(ageInYears);
		blockedTransportState = sn.DoEnum(blockedTransportState);
		snapshotCrops = sn.DoDictionary(snapshotCrops);
		flavour = sn.DoInt32Nullable(flavour);
		inSeason = sn.DoEnum(inSeason);
		shapeFactor = sn.DoFloatNullable(shapeFactor);
		size = sn.DoFloatNullable(size);
		sn.Ignore(cropNeeds);
		sn.Ignore(plantNeeds);
		sn.Ignore(sustainNeeds);
		sn.Ignore(ageProbabilities);
		sn.Ignore(Crops);
		return this;
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
		if (snapshotCrops != null)
		{
			Crops = snapshotCrops.ToDictionary((KeyValuePair<ResourceType, ResourceID> k) => k.Key, (KeyValuePair<ResourceType, ResourceID> k) => (Crop)LookUp<ResourceContainer, ResourceID>.FindByID(k.Value));
			snapshotCrops = null;
		}
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

	void ILookUp<IHasCrops, HasCropsID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUpIHasCrops.Create();
	}

	void ILookUp<IHasCrops, HasCropsID>.SetInvalid()
	{
		hasCropsID = HasCropsID.Invalid;
	}
}
