using Microsoft.Xna.Framework;

namespace UWGame.ClientSide.Renderables;

public class RenderAsBillboardType
{
	public string AnimationAssetName;

	public Vector2 Offset;

	public float Bendyness;

	public Vector2 BaseCenter;

	public float AspectRatio = 1f;

	public string SpriteSheet;

	public string AssetName { get; set; }
}
