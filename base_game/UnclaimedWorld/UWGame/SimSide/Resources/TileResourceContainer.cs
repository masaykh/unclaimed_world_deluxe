using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems;

namespace UWGame.SimSide.Resources;

public class TileResourceContainer : ResourceContainer, ISleepingUpdatable
{
	public TerrainTile TerrainTile;

	private TerrainTileID snapshotTerrainTile;

	private Renderable renderable;

	private Renderable.SnapshotRenderable snapshotRenderable;

	private List<IResourceItem> resourceItems = new List<IResourceItem>();

	private List<ResourceItemID> snapshotResourceItems;

	private float totalHarvestableBulk;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public override Renderable Renderable => renderable;

	public override EntityType EntityType => null;

	public override Entity ParentEntity => null;

	public override bool IsIntelligent => false;

	public override Vector3 AccessPoint => Location;

	public override Point MapPosition => new Point(TerrainTile.X, TerrainTile.Y);

	public override Vector3 Location => MapManager.TileToWorldPos(new Point(TerrainTile.X, TerrainTile.Y));

	public override List<IResourceItem> ResourceItems => resourceItems;

	public override float TotalHarvestableBulk => totalHarvestableBulk;

	public override int NoOfHarvestableItems => resourceItems.Count;

	public double? TimePointInSeconds { get; private set; }

	public double? UpdateInterval => GetUpdateInterval();

	public SleepyUpdaterID SleepyUpdater { get; set; }

	private bool IsRenderedWithSprite()
	{
		return resourceType.TileResourceType.IsRenderedWithSprites;
	}

	public TileResourceContainer(TerrainTile parent, ResourceType type)
		: base(type)
	{
		((ILookUp<IDetectable, DetectableID>)this).AddToLookup();
		TerrainTile = parent;
		CreateRenderable();
		if (base.ResourceType.CanReplenish())
		{
			The.Sim.PlaySite.PlaySite.AddTileResourceContainer(this);
		}
		parent.TileResources.Add(type, this);
		The.Sim.PlaySite.AddResourceContainer(this);
	}

	private void CreateRenderable()
	{
		renderable = RenderableFactory.Produce(null, base.ResourceType.TileResourceType.RenderableTypeMode, snapshotRenderable);
		Renderable.SetPulsing(Renderable.AdditionalEffect.Outline);
		Vector3 vector = MapManager.TileToWorldPos(new Point(TerrainTile.X, TerrainTile.Y));
		Vector3 vector2 = Vector3.Zero;
		if (IsRenderedWithSprite())
		{
			vector2 = new Vector3((float)Math.Sin(vector.Y) * 10f, (float)Math.Cos(vector.X) * 10f, 0f);
			Renderable.SetSpriteStateFlag(RenderableType.GetRandomFlavour(base.ResourceType.TileResourceType.MaxFlavours));
		}
		Renderable.Location = vector + vector2;
	}

	public TileResourceContainer()
	{
	}

	public override void Destroy()
	{
		base.Destroy();
		((ILookUp<IDetectable, DetectableID>)this).RemoveIDEntry();
	}

	public void SetTotalHarvestableBulk(float value)
	{
		float value2 = totalHarvestableBulk;
		totalHarvestableBulk = value;
		CreateResourceItemsFromTotals();
		UpdateSpritesToUse(value2);
	}

	private void UpdateSpritesToUse(float? oldValue)
	{
		if (!IsRenderedWithSprite())
		{
			return;
		}
		if (NoOfHarvestableItems == 0)
		{
			renderable.ClearSpriteStateFlag(StateModifier.Less);
			renderable.ClearSpriteStateFlag(StateModifier.More);
			return;
		}
		bool flag = false;
		bool flag2 = totalHarvestableBulk < base.ResourceType.TileResourceType.MoreSpriteLimit;
		if (oldValue.HasValue)
		{
			if (oldValue.Value < base.ResourceType.TileResourceType.MoreSpriteLimit != flag2)
			{
				flag = true;
			}
		}
		else
		{
			flag = true;
		}
		if (flag)
		{
			if (flag2)
			{
				renderable.SetSpriteStateFlag(StateModifier.Less);
			}
			else
			{
				renderable.SetSpriteStateFlag(StateModifier.More);
			}
		}
	}

	public override void SetResourceItems(int noOfItems)
	{
		int count = resourceItems.Count;
		if (noOfItems > count)
		{
			AddResourceItems(noOfItems - count);
		}
		else if (noOfItems < count)
		{
			int amounttoRemove = count - noOfItems;
			RemoveResourceItems(amounttoRemove);
		}
	}

	public override void RemoveResourceItems(int amounttoRemove)
	{
		while (amounttoRemove > 0 && resourceItems.Count > 0)
		{
			resourceItems[resourceItems.Count - 1].Destroy();
			resourceItems.RemoveAt(resourceItems.Count - 1);
			amounttoRemove--;
		}
		UpdateBulkAndSprites(resourceItems.Count);
	}

	public override void AddResourceItems(int noOfItemsToAdd)
	{
		for (int i = 0; i < noOfItemsToAdd; i++)
		{
			resourceItems.Add(new TileResourceItem(this));
		}
		UpdateBulkAndSprites(resourceItems.Count);
	}

	protected override void UpdateBulkAndSprites(int noOfItems)
	{
		base.UpdateBulkAndSprites(noOfItems);
		SetTotalHarvestableBulk(base.ResourceType.ResourceItemType.ItemType.MaximumBulk.Value * (float)resourceItems.Count);
		UpdateSpritesToUse(null);
	}

	public override IResourceItem FindHarvestableItem()
	{
		return resourceItems[0];
	}

	public override bool IsDestroyed(SharedKnowledge knowledge)
	{
		return false;
	}

	public override bool GetClosestAccessibleHarvestLocation(SubtileLayers movemap, Vector3 fromLocation, out Vector3? closestLocation)
	{
		if (The.Map.GetClosestAccessiblePoint(movemap, fromLocation, Location, stayInsideTile: false, out var closestSubtile))
		{
			closestLocation = MapManager.SubTileToWorldPos3(closestSubtile.Value);
			closestLocation = MapManager.VaryLocationWithinSubtile(closestLocation.Value);
			return true;
		}
		closestLocation = null;
		return false;
	}

	public override bool GatherResource(IResourceItem resourceItem)
	{
		if (resourceItems.Contains(resourceItem))
		{
			resourceItems.Remove(resourceItem);
			SetTotalHarvestableBulk((float)resourceItems.Count * base.ResourceType.ResourceItemType.ItemType.MaximumBulk.Value);
			return true;
		}
		return false;
	}

	private void CreateResourceItemsFromTotals()
	{
		int harvestableItemsFromBulk = base.ResourceType.GetHarvestableItemsFromBulk(totalHarvestableBulk);
		if (resourceItems.Count < harvestableItemsFromBulk)
		{
			for (int i = 0; i < harvestableItemsFromBulk - resourceItems.Count; i++)
			{
				resourceItems.Add(new TileResourceItem(this));
			}
		}
		else
		{
			_ = resourceItems.Count;
		}
	}

	public override bool UsesMemory(SharedKnowledge sharedKnowledge)
	{
		return true;
	}

	public override bool RequiresRollToDetect()
	{
		return true;
	}

	public void SetNextTimepoint(double? timepoint)
	{
		TimePointInSeconds = timepoint;
	}

	void ISleepingUpdatable.CreateSleepyLookupCollection()
	{
	}

	public static void CreateSleepyLookupCollection()
	{
		LookUpSleepyUpdater<TileResourceContainer>.Create();
	}

	public void Update(GameTime gameTime, out bool wasDestroyed)
	{
		base.Update(gameTime);
		wasDestroyed = false;
	}

	public void RecomputeUpdateInterval(out bool intervalChanged)
	{
		intervalChanged = false;
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		snapshotResourceItems = resourceItems.Select((IResourceItem r) => r.ID).ToList();
		snapshotResourceItems = sn.DoList(snapshotResourceItems);
		snapshotTerrainTile = sn.SnapshotID<TerrainTile, TerrainTileID>(TerrainTile).Value;
		totalHarvestableBulk = sn.DoFloat(totalHarvestableBulk);
		TimePointInSeconds = sn.DoDoubleNullable(TimePointInSeconds);
		SleepyUpdater = sn.DoEnum(SleepyUpdater);
		if (sn.mode != Snapshotter.Mode.Load && The.Client != null && Renderable != null)
		{
			snapshotRenderable = Renderable.GetFieldsToSnapshot();
		}
		snapshotRenderable = (Renderable.SnapshotRenderable)sn.DoISnapshot(snapshotRenderable);
		sn.Ignore(resourceItems);
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
		sn.RegisterLoadPostProcessCall(this);
		base.LoadPostProcess(sn);
		resourceItems = snapshotResourceItems.Select((ResourceItemID r) => LookUp<IResourceItem, ResourceItemID>.FindByID(r)).ToList();
		TerrainTile = LookUpSortedDictionary<TerrainTile, TerrainTileID>.FindByID(snapshotTerrainTile);
		CreateRenderable();
	}
}
