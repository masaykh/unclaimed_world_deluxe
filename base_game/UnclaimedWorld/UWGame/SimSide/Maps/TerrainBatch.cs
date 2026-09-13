using System;
using System.Collections.Generic;
using UWGame.ClientSide.Map;
using UWGame.SimSide.Vegetation;

namespace UWGame.SimSide.Maps;

public class TerrainBatch : IComparable
{
	public List<VertexMultitextured> terrainVerticesList;

	private VertexMultitextured[] terrainVerticesArray;

	public List<short> terrainIndicesList;

	private short[] terrainIndicesArray;

	public List<RenderedTerrainType> terrainInBatch = new List<RenderedTerrainType>();

	public bool RenderAsRocks;

	public VertexMultitextured[] TerrainVerticesArray
	{
		get
		{
			if (terrainVerticesArray == null)
			{
				terrainVerticesArray = terrainVerticesList.ToArray();
			}
			return terrainVerticesArray;
		}
	}

	public short[] TerrainIndicesArray
	{
		get
		{
			if (terrainIndicesArray == null)
			{
				terrainIndicesArray = terrainIndicesList.ToArray();
			}
			return terrainIndicesArray;
		}
	}

	public TerrainBatch()
	{
		terrainVerticesList = new List<VertexMultitextured>();
		terrainIndicesList = new List<short>();
	}

	public int CompareTo(object obj)
	{
		TerrainBatch terrainBatch = obj as TerrainBatch;
		return terrainInBatch[0].RenderOrder - terrainBatch.terrainInBatch[0].RenderOrder;
	}
}
