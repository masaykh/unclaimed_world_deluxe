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

	public bool FourSidedSymmetry;

	public float MeleeRadius;

	public LeggedLocomotorType LeggedLocomotorType;

	public BallisticLocomotorType BallisticLocomotorType;

	public CollisionResponderType CollisionResponderType;

	public float MaxAngularSpeed = 4.712389f;

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
