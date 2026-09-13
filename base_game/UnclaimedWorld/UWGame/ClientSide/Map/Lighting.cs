using System;
using System.Collections.Generic;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities;

namespace UWGame.ClientSide.Map;

public class Lighting
{
	public LightSource[] IndoorLightSources;

	public LightSource[] OutdoorLightSources;

	private Renderable Parent;

	private LightingType lightingType;

	public Lighting(LightingType lightingType, Renderable parent)
	{
		Parent = parent;
		this.lightingType = lightingType;
	}

	public void TurnOnDesiredShareOfLights(float fractionToTurnOn)
	{
		if (IndoorLightSources != null)
		{
			int num = CountLightsOn(IndoorLightSources);
			int num2 = (int)Math.Ceiling((float)IndoorLightSources.Length * fractionToTurnOn);
			if (num2 > num)
			{
				TurnOnLights(num2 - num, IndoorLightSources);
			}
		}
		if (OutdoorLightSources != null)
		{
			int num3 = CountLightsOn(OutdoorLightSources);
			int num4 = (int)Math.Ceiling((float)OutdoorLightSources.Length * fractionToTurnOn);
			if (num4 > num3)
			{
				TurnOnLights(num4 - num3, OutdoorLightSources);
			}
		}
	}

	public static void TurnOnLights(int noToTurnOn, LightSource[] list)
	{
		int num = The.Sim.GameplayRandomGenerator.Next(0, list.Length, "Structure");
		int i = 0;
		int num2 = 0;
		for (; i < noToTurnOn; i++)
		{
			if (num2 >= list.Length)
			{
				break;
			}
			while (list[num].LightIsOn)
			{
				num2++;
				num++;
				num %= list.Length;
			}
			list[num].LightIsOn = true;
		}
	}

	public void TurnOffTheLight()
	{
		if (IndoorLightSources != null)
		{
			for (int i = 0; i < IndoorLightSources.Length; i++)
			{
				IndoorLightSources[i].LightIsOn = false;
			}
		}
		if (OutdoorLightSources != null)
		{
			for (int j = 0; j < OutdoorLightSources.Length; j++)
			{
				OutdoorLightSources[j].LightIsOn = false;
			}
		}
	}

	public void GetLightSourcesForDrawing(List<LightSource> listOfObjectsToDraw)
	{
		if (IndoorLightSources != null)
		{
			for (int i = 0; i < IndoorLightSources.Length; i++)
			{
				if (IndoorLightSources[i] != null && IndoorLightSources[i].LightIsOn)
				{
					listOfObjectsToDraw.Add(IndoorLightSources[i]);
				}
			}
		}
		if (OutdoorLightSources == null)
		{
			return;
		}
		for (int j = 0; j < OutdoorLightSources.Length; j++)
		{
			if (OutdoorLightSources[j] != null && OutdoorLightSources[j].LightIsOn)
			{
				listOfObjectsToDraw.Add(OutdoorLightSources[j]);
			}
		}
	}

	public float GetOutdoorLightsShare()
	{
		if (OutdoorLightSources != null)
		{
			return (float)CountLightsOn(OutdoorLightSources) / (float)OutdoorLightSources.Length;
		}
		return -1f;
	}

	public int CountLightsOn(LightSource[] lights)
	{
		int num = 0;
		for (int i = 0; i < IndoorLightSources.Length; i++)
		{
			if (IndoorLightSources[i].LightIsOn)
			{
				num++;
			}
		}
		return num;
	}

	public float GetIndoorLightsShare()
	{
		if (IndoorLightSources != null)
		{
			return (float)CountLightsOn(IndoorLightSources) / (float)IndoorLightSources.Length;
		}
		return -1f;
	}
}
