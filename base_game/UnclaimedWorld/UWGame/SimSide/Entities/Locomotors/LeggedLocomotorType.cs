namespace UWGame.SimSide.Entities.Locomotors;

public class LeggedLocomotorType
{
	/// <summary>
	/// A value from 0 to 1: 0 applies the full negative terrain movement effect, while 1 ignores terrain effects.
	/// </summary>
	public float TerrainNegateFactor;

	/// <summary>
	/// Speeds are measured in pixels per second.
	/// </summary>
	public float WalkNormalSpeed;

	public float WalkSlowSpeed;

	public float WalkFastSpeed;

	public float RunSpeed;

	public float HaulSpeed;
}
