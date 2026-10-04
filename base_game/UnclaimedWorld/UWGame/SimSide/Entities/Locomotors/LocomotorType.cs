using System.Collections.Generic;
using System.Xml.Serialization;
using UWGame.SimSide.Entities.Locomotors.Stances;

namespace UWGame.SimSide.Entities.Locomotors;

public class LocomotorType
{
	public enum Surface
	{
		Ground,
		WaterFloat,
		WaterFord,
		Air,
		Obstacle
	}

	public enum Appearance
	{
		Biped,
		Quadruped,
		Car,
		Bike,
		Treads,
		Hover,
		Wings,
		Boat,
		Thrown
	}

	public enum ZBehavior
	{
		Ground,
		SeaLevel,
		SurfaceRelative,
		Ballistic,
		Bounce
	}

	public enum AxialBehavior
	{
		Plumb,
		Tumble,
		Roll,
		Bank,
		Fletch,
		Corkscrew
	}

	public bool CanRun;

	/// <summary>
	/// Do not use directly; this value may be overridden by <c>BiologicalEntityType</c>.
	/// </summary>
	public bool FourSidedSymmetry;

	/// <summary>
	/// Can differ from the bounding radius. It is the circular tabletop-game base that must touch another unit's base for melee.
	/// </summary>
	public float MeleeRadius;

	public LeggedLocomotorType LeggedLocomotorType;

	public BallisticLocomotorType BallisticLocomotorType;

	public CollisionResponderType CollisionResponderType;

	public float MaxAngularSpeed = 4.712389f;

	/// <summary>
	/// When present, allows part of the entity to rotate during <c>GoalTurnToFace</c> instead of rotating the whole body.
	/// </summary>
	public RotatorType RotatorType;

	public string Stances;

	[XmlIgnore]
	public StancesType StancesType;

	public Surface surface;

	public Appearance appearance;

	public ZBehavior zBehavior;

	public AxialBehavior axialBehavior;

	public void Initialize()
	{
		if (Stances != null)
		{
			StancesType = GameData.Instance.AllStancesTypes[Stances];
		}
	}

	public void PreInitValidate(List<string> listOfErrors)
	{
	}

	public void PostInitValidate(List<string> listOfErrors)
	{
	}
}
