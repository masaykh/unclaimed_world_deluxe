using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.InGameEvents.Actions;

public class SpawnStockpileAction : EventActionType
{
	public AllegianceAndExpedition OwnedBy;

	public Point[] CoveredArea;

	public Point StartDragTilePosition;

	public SerializableDictionary<string, bool> MayStockpileCategory;

	public SerializableDictionary<string, int> MayStockpileItem;

	public SpawnStockpileAction(string keyName)
		: base(keyName)
	{
	}

	public SpawnStockpileAction()
	{
	}

	public override bool Execute(EventAction action, ref string failReason)
	{
		MapArea mapArea = new MapArea();
		Point[] coveredArea = CoveredArea;
		foreach (Point pos in coveredArea)
		{
			mapArea.Add(The.Map.GetTile(pos));
		}
		mapArea.StartDragTile = StartDragTilePosition;
		if (!OwnedBy.Resolve(action, out var _, out var expedition, ref failReason))
		{
			return false;
		}
		new CreateStockpile(MayStockpileCategory, MayStockpileItem, expedition.OwnedEntities.ID, null, mapArea, giveClientFeedback: true).Execute(giveClientFeedback: false);
		return true;
	}

	public override void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
		if (MayStockpileCategory != null)
		{
			foreach (KeyValuePair<string, bool> item in MayStockpileCategory)
			{
				EntityType.ValidateGameDataTypeExists(ref listOfErrors, item.Key, GameData.Instance.AllEntityCategories, out var _);
			}
		}
		if (MayStockpileItem == null)
		{
			return;
		}
		foreach (KeyValuePair<string, int> item2 in MayStockpileItem)
		{
			EntityType.ValidateEntityTypeKeyExists(ref listOfErrors, item2.Key);
		}
	}

	public override void PreInitValidate(ref List<string> listOfErrors)
	{
		base.PreInitValidate(ref listOfErrors);
		if (!CoveredArea.Contains(StartDragTilePosition))
		{
			EntityType.CreateValidationError(ref listOfErrors, "StartDragTilePosition is not in the list of Covered tiles");
		}
	}

	public override string ToString()
	{
		return "Spawn stockpile: " + OwnedBy.ToString();
	}
}
