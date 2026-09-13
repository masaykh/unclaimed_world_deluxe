using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities;

[Component]
public abstract class Component : ISnapshot
{
	public Entity Parent;

	private EntityID parentID;

	private Regulator updateRegulator;

	private Regulator updatePlaySiteRegulator;

	private double updateIntervalInSeconds;

	private Snapshotter.Version version;

	public bool IsSnapshotted { get; set; }

	public Component(Entity parent, double updateIntervalInSeconds)
	{
		this.updateIntervalInSeconds = updateIntervalInSeconds;
		Parent = parent;
		double intervalToUse = GetIntervalToUse(updateIntervalInSeconds);
		updateRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1.0 / intervalToUse, "Component");
		CreatePlaySiteRegulator();
	}

	private static double GetIntervalToUse(double updateIntervalInSeconds)
	{
		return updateIntervalInSeconds - 1.0 / 60.0;
	}

	private void CreatePlaySiteRegulator()
	{
		double intervalToUse = GetIntervalToUse(updateIntervalInSeconds);
		updatePlaySiteRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1.0 / intervalToUse, "Component");
	}

	public Component(Entity parent)
	{
		Parent = parent;
	}

	public Component()
	{
	}

	public virtual double? GetUpdateInterval()
	{
		return null;
	}

	public virtual void Update(GameTime gameTime)
	{
		bool flag = true;
		double? timeSinceLastUpdate = null;
		if (updateRegulator != null)
		{
			flag = updateRegulator.IsReadyGetTimeElapsedInSeconds(out var secondsSinceLastReady);
			timeSinceLastUpdate = secondsSinceLastReady;
		}
		if (flag)
		{
			UpdateRegulated(timeSinceLastUpdate);
		}
	}

	public virtual void UpdatePlaySite(GameTime gameTime)
	{
		bool flag = true;
		double? timeSinceLastUpdate = null;
		if (updatePlaySiteRegulator != null)
		{
			flag = updatePlaySiteRegulator.IsReadyGetTimeElapsedInSeconds(out var secondsSinceLastReady);
			timeSinceLastUpdate = secondsSinceLastReady;
		}
		if (flag)
		{
			UpdatePlaySiteRegulated(timeSinceLastUpdate);
		}
	}

	public virtual void ResetPlaySiteRegulators()
	{
		if (updatePlaySiteRegulator != null)
		{
			CreatePlaySiteRegulator();
		}
	}

	protected virtual void UpdateRegulated(double? timeSinceLastUpdate)
	{
	}

	protected virtual void UpdatePlaySiteRegulated(double? timeSinceLastUpdate)
	{
	}

	public virtual ISnapshot DoSnapshot(Snapshotter sn)
	{
		parentID = sn.SnapshotID<Entity, EntityID>(Parent).Value;
		updateIntervalInSeconds = sn.DoDouble(updateIntervalInSeconds);
		updateRegulator = (Regulator)sn.DoISnapshot(updateRegulator);
		updatePlaySiteRegulator = (Regulator)sn.DoISnapshot(updatePlaySiteRegulator);
		return this;
	}

	public virtual void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		Parent = Entity.FindByID(parentID);
		if (updateRegulator != null)
		{
			updateRegulator.LoadPostProcess(sn);
		}
		if (updatePlaySiteRegulator != null)
		{
			updatePlaySiteRegulator.LoadPostProcess(sn);
		}
	}

	public virtual Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}
}
