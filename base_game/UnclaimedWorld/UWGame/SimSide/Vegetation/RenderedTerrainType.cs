using System;
using System.Collections.Generic;

namespace UWGame.SimSide.Vegetation;

public abstract class RenderedTerrainType : IGameData, IComparable
{
	public float NoiseScaling = 1f;

	public string TextureName;

	private bool invertNoise;

	public int RenderOrder;

	public static int HighestOrder;

	public bool IsBaseTerrain;

	private RenderPerlinNoise renderWithPerlinNoise;

	private float renderPerlinNoiseSharpness = 1f;

	public string Name { get; set; }

	public string KeyName { get; set; }

	public bool DeleteRecord { get; set; }

	public bool InvertNoise
	{
		get
		{
			return invertNoise;
		}
		set
		{
			if (value)
			{
				renderPerlinNoiseSharpness = -1f * Math.Abs(renderPerlinNoiseSharpness);
			}
			else
			{
				renderPerlinNoiseSharpness = Math.Abs(renderPerlinNoiseSharpness);
			}
			invertNoise = value;
		}
	}

	public RenderPerlinNoise RenderWithPerlinNoise
	{
		get
		{
			return renderWithPerlinNoise;
		}
		set
		{
			if (value == RenderPerlinNoise.None)
			{
				RenderPerlinNoiseSharpness = 0f;
			}
			renderWithPerlinNoise = value;
		}
	}

	public float RenderPerlinNoiseSharpness
	{
		get
		{
			return renderPerlinNoiseSharpness;
		}
		set
		{
			if (value != 0f && renderWithPerlinNoise == RenderPerlinNoise.None)
			{
				throw new Exception("Sharpness must be 0 when RenderWithPerlinNoise = None is set.");
			}
			renderPerlinNoiseSharpness = value;
		}
	}

	public int GetPerlinNoiseChannel()
	{
		return renderWithPerlinNoise switch
		{
			RenderPerlinNoise.ChannelRed => 0, 
			RenderPerlinNoise.ChannelGreen => 1, 
			RenderPerlinNoise.ChannelBlue => 2, 
			_ => 0, 
		};
	}

	public void Initialize()
	{
	}

	public void PreInitValidate(ref List<string> errors)
	{
	}

	public void PostInitValidate(ref List<string> errors)
	{
	}

	public void PostDataCompleteInitialize()
	{
	}

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public int CompareTo(object obj)
	{
		RenderedTerrainType renderedTerrainType = obj as RenderedTerrainType;
		return RenderOrder - renderedTerrainType.RenderOrder;
	}
}
