using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps;

public abstract class InfluenceMap : IMap, ILookUp<IMap, IMapID>, ISnapshot
{
	public class DiscomfortTileIsBelowValueParameters
	{
		public DiscomfortMap map;

		public byte belowOrEqualToValue;

		public DiscomfortTileIsBelowValueParameters(DiscomfortMap map, byte belowOrEqualToValue)
		{
			this.map = map;
			this.belowOrEqualToValue = belowOrEqualToValue;
		}
	}

	public enum GradientDirection
	{
		LeftToRight,
		TopToBottom
	}

	public enum CircleParameter
	{
		FalloffEachTile,
		Radius
	}

	public enum Operation
	{
		SetValue,
		AddToExisting
	}

	public enum Falloff
	{
		Yes,
		No
	}

	public string IDName;

	private List<Dependence> children = new List<Dependence>();

	private const float subTileWidthReciprocal = 0.33333f;

	public byte BlockingLimit;

	private static string spacer = "\t";

	private IMapID id = IMapID.Invalid;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public List<Dependence> Children
	{
		get
		{
			return children;
		}
		set
		{
			children = value;
		}
	}

	public bool IsReady { get; set; }

	public TileLayer Map { get; protected set; }

	public IMapID ID
	{
		get
		{
			return id;
		}
		private set
		{
			id = value;
		}
	}

	public int LoadPostProcessOrder => 0;

	public bool IsSnapshotted { get; set; }

	public InfluenceMap()
	{
	}

	public InfluenceMap(int mapWidth, int mapHeight)
	{
		AddToLookup();
	}

	public static bool DiscomfortTileIsFree(DiscomfortMap map, int subtileX, int subtileY)
	{
		Point point = MapManager.SubTileToTilePos(new Point(subtileX, subtileY));
		return !map.Map.GetIsBlocked(point.X, point.Y);
	}

	public static bool DiscomfortTileIsBelowValue(DiscomfortTileIsBelowValueParameters parameters, int subtileX, int subtileY)
	{
		Point pos = MapManager.SubTileToTilePos(new Point(subtileX, subtileY));
		return parameters.map.Map.GetValue(pos) <= parameters.belowOrEqualToValue;
	}

	public static void AddTilesInSector(Rectangle sectorArea, float weight, byte[][] map, bool[][] isBlocked, byte[][] mapToAdd, bool[][] isBlockedToAdd)
	{
		for (int i = sectorArea.Left; i < sectorArea.Right; i++)
		{
			byte[] array = map[i];
			bool[] array2 = isBlocked[i];
			byte[] array3 = mapToAdd[i];
			bool[] array4 = isBlockedToAdd[i];
			for (int j = sectorArea.Top; j < sectorArea.Bottom; j++)
			{
				array[j] = (byte)Common.ClampTop((float)(int)array[j] + weight * (float)(int)array3[j], 255f);
				array2[j] = array2[j] || array4[j];
			}
		}
	}

	public void Destroy()
	{
		RemoveIDEntry();
	}

	public IMap GetCurrent()
	{
		return this;
	}

	public override string ToString()
	{
		return IDName;
	}

	public static string VisualizeByteArray(byte[,] array, Point from, Point to)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("(x, y)\t");
		for (int i = from.X; i <= to.X; i++)
		{
			stringBuilder.Append(i);
			stringBuilder.Append("\t");
		}
		stringBuilder.Append("\r\n");
		stringBuilder.Append("---------------------------------------------------------------------------------------------------------------------------");
		stringBuilder.Append("\r\n");
		for (int j = from.Y; j <= to.Y; j++)
		{
			stringBuilder.Append(j);
			stringBuilder.Append("\t");
			stringBuilder.Append("|");
			for (int k = from.X; k <= to.X; k++)
			{
				stringBuilder.Append(array[k, j]);
				stringBuilder.Append("\t");
			}
			stringBuilder.Append("\r\n");
		}
		return stringBuilder.ToString();
	}

	public static string VisualizeByteArray<T>(T[][] array, Point? from = null, Point? to = null)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num;
		int num2;
		if (to.HasValue)
		{
			num = to.Value.Y;
			num2 = to.Value.X;
		}
		else
		{
			num = array[0].GetUpperBound(0);
			num2 = array.GetUpperBound(0);
		}
		int num3;
		int num4;
		if (from.HasValue)
		{
			num3 = from.Value.Y;
			num4 = from.Value.X;
		}
		else
		{
			num4 = 0;
			num3 = 0;
		}
		stringBuilder.Append("(x, y)\t");
		for (int i = num3; i <= num2; i++)
		{
			stringBuilder.Append(i);
			stringBuilder.Append(spacer);
		}
		stringBuilder.Append("\r\n");
		stringBuilder.Append("---------------------------------------------------------------------------------------------------------------------------");
		stringBuilder.Append("\r\n");
		for (int j = num4; j <= num; j++)
		{
			stringBuilder.Append(j);
			stringBuilder.Append(spacer);
			stringBuilder.Append("|");
			for (int k = num3; k <= num2; k++)
			{
				stringBuilder.Append(array[k][j]);
				stringBuilder.Append(spacer);
			}
			stringBuilder.Append("\r\n");
		}
		return stringBuilder.ToString();
	}

	public static void DrawGradientRectangle(byte[][] map, Point topLeft, Point bottomRight, bool addToExistingValues, byte startValue, byte endValue, int maxRandomToAdd, GradientDirection dir)
	{
		double num = (int)startValue;
		double num2;
		if (dir == GradientDirection.LeftToRight)
		{
			num2 = (double)(endValue - startValue) / Math.Abs((double)(bottomRight.X - topLeft.X));
			for (int i = topLeft.X; i <= bottomRight.X; i++)
			{
				for (int j = topLeft.Y; j <= bottomRight.Y; j++)
				{
					byte b = ((maxRandomToAdd == 0) ? ((byte)num) : ((byte)(num + (double)The.Sim.GameplayRandomGenerator.Next(maxRandomToAdd + 1, "InfluenceMap"))));
					if (!addToExistingValues)
					{
						map[i][j] = b;
					}
					else
					{
						map[i][j] = (byte)Common.Clamp(map[i][j] + b, 0, 255);
					}
				}
				num += num2;
			}
			return;
		}
		num2 = (double)(endValue - startValue) / Math.Abs((double)(bottomRight.Y - topLeft.Y));
		for (int k = topLeft.Y; k <= bottomRight.Y; k++)
		{
			for (int l = topLeft.X; l <= bottomRight.X; l++)
			{
				byte b = ((maxRandomToAdd == 0) ? ((byte)num) : ((byte)(num + (double)The.Sim.GameplayRandomGenerator.Next(maxRandomToAdd + 1, "InfluenceMap"))));
				if (!addToExistingValues)
				{
					map[l][k] = b;
				}
				else
				{
					map[l][k] = (byte)Common.Clamp(map[l][k] + b, 0, 255);
				}
			}
			num += num2;
		}
	}

	public static void DrawLinearInfluenceCircle(byte[][] map, Point pos, int centerValue, Operation operation, Falloff falloffYesNo, CircleParameter circleParam, int paramValue, HashSet<Point> affectedSectors = null, int? sectorSize = null, int? maxRadius = null)
	{
		int jaggedArrayWidth = Common.GetJaggedArrayWidth(map);
		int jaggedArrayHeight = Common.GetJaggedArrayHeight(map);
		pos = ComputeLinearCircle(jaggedArrayWidth, jaggedArrayHeight, pos, centerValue, falloffYesNo, circleParam, paramValue, maxRadius, out var isDrawingAPositiveCircle, out var falloffEachTile, out var minX, out var minY, out var maxX, out var maxY);
		double num = centerValue;
		for (int i = minX; i <= maxX; i++)
		{
			for (int j = minY; j <= maxY; j++)
			{
				if (falloffYesNo == Falloff.Yes)
				{
					float num2 = Common.DistanceOctile(new Point(i, j), pos);
					num = (double)centerValue + (double)num2 * falloffEachTile;
				}
				num = (isDrawingAPositiveCircle ? Common.ClampBottom(num, 0.0) : Common.ClampTop(num, 0.0));
				if (operation == Operation.SetValue)
				{
					map[i][j] = (byte)Common.Clamp(num, 0.0, 255.0);
				}
				else if (num != 0.0)
				{
					map[i][j] = (byte)Common.Clamp((double)(int)map[i][j] + num, 0.0, 255.0);
				}
			}
		}
		if (!sectorSize.HasValue)
		{
			return;
		}
		int num3 = minX / sectorSize.Value;
		int num4 = maxX / sectorSize.Value;
		int num5 = minY / sectorSize.Value;
		int num6 = maxY / sectorSize.Value;
		for (int k = num3; k <= num4; k++)
		{
			for (int l = num5; l <= num6; l++)
			{
				affectedSectors.Add(new Point(k, l));
			}
		}
	}

	public static Point ComputeLinearCircle(int mapWidth, int mapHeight, Point pos, int centerValue, Falloff falloffYesNo, CircleParameter circleParam, int paramValue, int? maxRadius, out bool isDrawingAPositiveCircle, out double falloffEachTile, out int minX, out int minY, out int maxX, out int maxY)
	{
		isDrawingAPositiveCircle = centerValue > 0;
		falloffEachTile = 0.0;
		int num;
		if (circleParam == CircleParameter.FalloffEachTile)
		{
			num = Math.Abs(centerValue / paramValue);
			falloffEachTile = -paramValue;
			if (maxRadius.HasValue)
			{
				num = Common.ClampTop(num, maxRadius.Value);
			}
		}
		else
		{
			num = paramValue;
			if (falloffYesNo == Falloff.Yes)
			{
				if (isDrawingAPositiveCircle)
				{
					falloffEachTile = (double)centerValue / (double)(num + 1);
					falloffEachTile = 0.0 - falloffEachTile;
				}
				else
				{
					falloffEachTile = (0.0 - (double)centerValue) / (double)(num + 1);
				}
			}
		}
		minX = Math.Max(0, pos.X - num);
		minY = Math.Max(0, pos.Y - num);
		maxX = Math.Min(mapWidth - 1, pos.X + num);
		maxY = Math.Min(mapHeight - 1, pos.Y + num);
		return pos;
	}

	public static int GetBestSubtileLocationThatIsntBlocked(byte[][] subtileMap, bool[][] isBlockedMap, out Point bestSubtilePoint)
	{
		bestSubtilePoint = new Point(-1, -1);
		int num = -1;
		int jaggedArrayWidth = Common.GetJaggedArrayWidth(subtileMap);
		int jaggedArrayHeight = Common.GetJaggedArrayHeight(subtileMap);
		for (int i = 0; i < jaggedArrayWidth; i++)
		{
			for (int j = 0; j < jaggedArrayHeight; j++)
			{
				if (!isBlockedMap[i][j] && subtileMap[i][j] > num)
				{
					bestSubtilePoint = new Point(i, j);
					num = subtileMap[i][j];
				}
			}
		}
		return num;
	}

	public static int GetBestSubtileLocationThatIsntBlocked(byte[][] subtileMap, Tuple<bool, Vector2>[][] isBlockedData, out Vector2 bestSubtileWorldPosition)
	{
		bestSubtileWorldPosition = new Vector2(-1f, -1f);
		int num = -1;
		int jaggedArrayWidth = Common.GetJaggedArrayWidth(isBlockedData);
		int jaggedArrayHeight = Common.GetJaggedArrayHeight(isBlockedData);
		for (int i = 0; i < jaggedArrayWidth; i++)
		{
			for (int j = 0; j < jaggedArrayHeight; j++)
			{
				if (!isBlockedData[i][j].Item1 && subtileMap[i][j] > num)
				{
					num = subtileMap[i][j];
					bestSubtileWorldPosition = isBlockedData[i][j].Item2;
				}
			}
		}
		return num;
	}

	public static int GetBestSubtileLocationThatIsntBlocked(byte[][] subtileMap, out Point bestSubtilePoint)
	{
		bestSubtilePoint = new Point(-1, -1);
		int num = -1;
		int jaggedArrayWidth = Common.GetJaggedArrayWidth(subtileMap);
		int jaggedArrayHeight = Common.GetJaggedArrayHeight(subtileMap);
		for (int i = 0; i < jaggedArrayWidth; i++)
		{
			for (int j = 0; j < jaggedArrayHeight; j++)
			{
				byte b = subtileMap[i][j];
				if (b != 0 && b > num)
				{
					bestSubtilePoint = new Point(i, j);
					num = b;
				}
			}
		}
		return num;
	}

	public static void DrawLinearInfluenceCircle(ushort[][] map, Point pos, int centerValue, Operation operation, Falloff falloffYesNo, CircleParameter circleParam, int paramValue)
	{
		bool flag = centerValue > 0;
		double num = 0.0;
		int num2;
		if (circleParam == CircleParameter.FalloffEachTile)
		{
			num2 = Math.Abs(centerValue / paramValue);
			num = -paramValue;
		}
		else
		{
			num2 = paramValue;
			if (falloffYesNo == Falloff.Yes)
			{
				if (flag)
				{
					num = (double)centerValue / (double)(num2 + 1);
					num = 0.0 - num;
				}
				else
				{
					num = (0.0 - (double)centerValue) / (double)(num2 + 1);
				}
			}
		}
		int num3 = Math.Max(0, pos.X - num2);
		int num4 = Math.Max(0, pos.Y - num2);
		int jaggedArrayWidth = Common.GetJaggedArrayWidth(map);
		int jaggedArrayHeight = Common.GetJaggedArrayHeight(map);
		int num5 = Math.Min(jaggedArrayWidth - 1, pos.X + num2);
		int num6 = Math.Min(jaggedArrayHeight - 1, pos.Y + num2);
		double num7 = centerValue;
		for (int i = num3; i <= num5; i++)
		{
			for (int j = num4; j <= num6; j++)
			{
				if (falloffYesNo == Falloff.Yes)
				{
					float num8 = Common.DistanceOctile(new Point(i, j), pos);
					num7 = (double)centerValue + (double)num8 * num;
				}
				num7 = (flag ? Common.ClampBottom(num7, 0.0) : Common.ClampTop(num7, 0.0));
				if (num7 != 0.0)
				{
					if (operation == Operation.SetValue)
					{
						map[i][j] = (ushort)Common.Clamp(num7, 0.0, 65535.0);
					}
					else
					{
						map[i][j] = (ushort)Common.Clamp((double)(int)map[i][j] + num7, 0.0, 65535.0);
					}
				}
			}
		}
	}

	public static void DrawLinearInfluenceCircle(bool[][] map, Point pos, bool value, int radius)
	{
		int num = Math.Max(0, pos.X - radius);
		int num2 = Math.Max(0, pos.Y - radius);
		int num3 = Math.Min(Common.GetJaggedArrayWidth(map) - 1, pos.X + radius);
		int num4 = Math.Min(Common.GetJaggedArrayHeight(map) - 1, pos.Y + radius);
		for (int i = num; i <= num3; i++)
		{
			for (int j = num2; j <= num4; j++)
			{
				map[i][j] = value;
			}
		}
	}

	public abstract bool DoCycle();

	public IMapID GetUniqueID()
	{
		return IMapCounter.GetUniqueID();
	}

	public IMapID SnapshotID(Snapshotter sn, IMapID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != IMapID.Invalid)
		{
			LookUp<IMap, IMapID>.Add(ID, this);
		}
	}

	public void RemoveIDEntry()
	{
		LookUp<IMap, IMapID>.Remove(this);
	}

	public void SetInvalid()
	{
		id = IMapID.Invalid;
	}

	public void ResetIDCounter()
	{
	}

	void ILookUp<IMap, IMapID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<IMap, IMapID>.Create();
	}

	public virtual ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = SnapshotID(sn, id);
		children = sn.DoList(children);
		IDName = sn.DoString(IDName);
		BlockingLimit = sn.DoByte(BlockingLimit);
		Map = (TileLayer)sn.DoISnapshot(Map);
		IsReady = sn.DoBool(IsReady);
		sn.Ignore(spacer);
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
		foreach (Dependence child in children)
		{
			child.LoadPostProcess(sn);
		}
		Map.LoadPostProcess(sn);
	}
}
