using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;

namespace UWGame.SimSide.Resources;

public class CropType
{
	public float DetectionPulsingDuration = 2000f;

	public float CropItemGrowthPerDay;

	public float? RipeSpeed;

	public float MaxSizeShareOfWholePlant;

	public StateModifier? TreeSpriteFlag;

	public float BulkLimitToShowFlag;

	public Vector2[] AgeProduction;

	public void Initialize()
	{
	}
}
