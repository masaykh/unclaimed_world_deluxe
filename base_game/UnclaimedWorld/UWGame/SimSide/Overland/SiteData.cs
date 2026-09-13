using System.Collections.Generic;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Overland.Locations;

namespace UWGame.SimSide.Overland;

public class SiteData : IGameData
{
	public string Description;

	public bool IsPlaySite;

	public GeodeticCoordinate Coords;

	public bool ShowLabel = true;

	public bool ShowTallPin = true;

	public int SiteMarkerOrder;

	public StringChance[] SiteTemplates;

	public string AllegianceKeyName;

	public string ExpeditionKeyName;

	public string Name { get; set; }

	public string KeyName { get; set; }

	public bool DeleteRecord { get; set; }

	public void PreInitValidate(ref List<string> errors)
	{
	}

	public void Initialize()
	{
	}

	public void PostInitValidate(ref List<string> errors)
	{
	}

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public void PostDataCompleteInitialize()
	{
	}

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
	}
}
