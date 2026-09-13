using Microsoft.Xna.Framework;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI;

internal abstract class GoapAction : ISnapshot
{
	protected Allegiance allegiance;

	private AllegianceID snapshotAllegiance;

	protected Expedition expedition;

	private ExpeditionID snapshotExpedition;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public GoapAction(Allegiance allegiance, Expedition expedition)
	{
		this.allegiance = allegiance;
		this.expedition = expedition;
	}

	public GoapAction()
	{
	}

	public abstract bool Init();

	public abstract bool MonitorAction();

	public abstract void Destroy();

	public static MapArea GetRandomMapArea(Expedition expedition, Allegiance allegiance)
	{
		MapArea mapArea = new MapArea();
		Point point = expedition.Location.Value.ToPoint();
		int forageAndHuntingRadius = allegiance.GetForageAndHuntingRadius();
		int num = 0;
		do
		{
			int num2 = The.Sim.GameplayRandomGenerator.RandomBetween(point.X - forageAndHuntingRadius, point.X + forageAndHuntingRadius);
			int num3 = The.Sim.GameplayRandomGenerator.RandomBetween(point.Y - forageAndHuntingRadius, point.Y + forageAndHuntingRadius);
			Vector3 position = new Vector3(num2, num3, 0f);
			position = The.Map.ClampWorldPosition(position);
			Point point2 = MapManager.WorldPosToTilePos(position).ToPoint();
			num++;
			Point sourceSubtile = MapManager.WorldPosToSubtile(point.ToVector2());
			Point destinationSubtile = MapManager.TileToCenterSubtile(point2);
			switch (IsPathAccessible(sourceSubtile, destinationSubtile, allegiance))
			{
			case RegionMap.Result.Wait:
				return null;
			case RegionMap.Result.OK:
				mapArea.Add(The.Map.GetTile(point2));
				mapArea.StartDragTile = The.Map.GetTile(31, 40).TilePos.ToPoint();
				return mapArea;
			}
		}
		while (num <= 1000);
		return null;
	}

	private static RegionMap.Result IsPathAccessible(Point sourceSubtile, Point destinationSubtile, Allegiance allegiance)
	{
		ThreatStance a = ThreatStance.Cautious;
		if (allegiance.RepresentativeEntityType.IntelligenceType.AttackTypes != null)
		{
			a = ThreatStance.Normal;
		}
		RegionMap regionMap = allegiance.SharedKnowledge.GetMovementMap(ProtectionLevel.Exposed, allegiance.RepresentativeEntityType, a).Layers[SurfaceType.TransportType.Foot].RegionMap;
		float distance = 0f;
		return regionMap.GetDistance(null, sourceSubtile, destinationSubtile, ref distance, sendMessageToEntity: false);
	}

	public virtual ISnapshot DoSnapshot(Snapshotter sn)
	{
		snapshotAllegiance = sn.SnapshotID<Allegiance, AllegianceID>(allegiance).Value;
		snapshotExpedition = sn.SnapshotID<Expedition, ExpeditionID>(expedition).Value;
		return this;
	}

	public virtual Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public virtual void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		allegiance = LookUp<Allegiance, AllegianceID>.FindByID(snapshotAllegiance);
		expedition = LookUp<Expedition, ExpeditionID>.FindByID(snapshotExpedition);
	}
}
