using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace SpriteSheetRuntime;

public class BuildingTypeData
{
	public Point BaseCenter;

	public int Width;

	public int Height;

	public int WidthInTiles;

	public int HeightInTiles;

	public Point FrontDoorRelativeLocation = new Point(-1, -1);

	public Point BackDoorRelativeLocation = new Point(-1, -1);

	public Dictionary<AddonSize, List<Point>> AddonSlots;

	public List<Point> WindowPositions;

	public byte[] DiscomfortValues;

	public Dictionary<string, Point> LightSourceOffsets;

	public void CreateAddonSlotsIfNotExist()
	{
		if (AddonSlots == null)
		{
			AddonSlots = new Dictionary<AddonSize, List<Point>>
			{
				{
					AddonSize.Tiny,
					new List<Point>()
				},
				{
					AddonSize.Small,
					new List<Point>()
				},
				{
					AddonSize.Big,
					new List<Point>()
				}
			};
		}
	}
}
