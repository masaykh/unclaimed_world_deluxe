using System.Collections.Generic;
using System.Linq;
using UWGame.Control.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Commands;

public class CreateStockpile : Command
{
	public Stockpile.TypesOfStockpiles TypeOfStockpile;

	public long EntityGroupID;

	public bool GiveClientFeedback;

	public long Structure;

	public ZoneCommand ZoneCommand;

	public SerializableDictionary<string, bool> MayStockpileCategory;

	public SerializableDictionary<string, int> MayStockpileItem;

	public string DefaultStorageSettings;

	public CreateStockpile()
	{
	}

	public CreateStockpile(SerializableDictionary<string, bool> mayStockpileCategory, SerializableDictionary<string, int> mayStockpileItem, EntityGroupID entityGroupID, DefaultStorageSettings defaultStorageSettings, ZoneID zoneID, bool giveClientFeedback)
	{
		ZoneCommand = new ZoneCommand(zoneID);
		TypeOfStockpile = Stockpile.TypesOfStockpiles.Normal;
		Init(mayStockpileCategory, mayStockpileItem, entityGroupID, defaultStorageSettings, giveClientFeedback);
	}

	public CreateStockpile(SerializableDictionary<string, bool> mayStockpileCategory, SerializableDictionary<string, int> mayStockpileItem, EntityGroupID entityGroupID, DefaultStorageSettings defaultStorageSettings, MapArea mapArea, bool giveClientFeedback)
	{
		ZoneCommand = new ZoneCommand(mapArea);
		TypeOfStockpile = Stockpile.TypesOfStockpiles.Normal;
		Init(mayStockpileCategory, mayStockpileItem, entityGroupID, defaultStorageSettings, giveClientFeedback);
	}

	public CreateStockpile(SerializableDictionary<string, bool> mayStockpileCategory, SerializableDictionary<string, int> mayStockpileItem, EntityGroupID entityGroupID, DefaultStorageSettings defaultStorageSettings, EntityID structure, Stockpile.TypesOfStockpiles typeOfStockpile, bool giveClientFeedback)
	{
		Structure = (long)structure;
		TypeOfStockpile = typeOfStockpile;
		Init(mayStockpileCategory, mayStockpileItem, entityGroupID, defaultStorageSettings, giveClientFeedback);
	}

	private void Init(SerializableDictionary<string, bool> mayStockpileCategory, SerializableDictionary<string, int> mayStockpileItem, EntityGroupID entityGroupID, DefaultStorageSettings defaultStorageSettings, bool giveClientFeedback)
	{
		MayStockpileCategory = mayStockpileCategory;
		MayStockpileItem = mayStockpileItem;
		GiveClientFeedback = giveClientFeedback;
		EntityGroupID = (long)entityGroupID;
		if (defaultStorageSettings != null)
		{
			DefaultStorageSettings = defaultStorageSettings.KeyName;
		}
	}

	public override void Execute(bool giveClientFeedback)
	{
		Zone zone = null;
		EntityGroup entityGroupToUse;
		if (ZoneCommand != null)
		{
			zone = ZoneCommand.RetrieveOrCreateZone(EntityGroupID, out entityGroupToUse);
		}
		else
		{
			entityGroupToUse = LookUp<EntityGroup, UWGame.SimSide.Entities.EntityGroupID>.FindByID((EntityGroupID)EntityGroupID);
		}
		bool flag = DoCreateStockpile(zone, entityGroupToUse);
		if (giveClientFeedback && GiveClientFeedback && flag)
		{
			The.Client.OnCreateStockpile(zone);
		}
	}

	private bool DoCreateStockpile(Zone zone, EntityGroup entityGroupToUse)
	{
		DefaultStorageSettings defaultSettings = null;
		if (DefaultStorageSettings != null)
		{
			defaultSettings = GameData.Instance.AllDefaultStorageSettings[DefaultStorageSettings];
		}
		Stockpile stockpile = new Stockpile(mayStockpileCategory: (MayStockpileCategory == null) ? new Dictionary<EntityCategory, bool>() : MayStockpileCategory.ToDictionary((KeyValuePair<string, bool> k) => GameData.Instance.AllEntityCategories[k.Key], (KeyValuePair<string, bool> k) => k.Value), mayStockpileItem: (MayStockpileItem == null) ? new Dictionary<EntityType, int>() : MayStockpileItem.ToDictionary((KeyValuePair<string, int> k) => GameData.Instance.AllEntityTypes[k.Key], (KeyValuePair<string, int> k) => k.Value), typeOfStockpile: TypeOfStockpile, defaultSettings: defaultSettings);
		if (zone != null)
		{
			zone.Stockpile = stockpile;
		}
		else if (TypeOfStockpile == Stockpile.TypesOfStockpiles.Normal)
		{
			entityGroupToUse.StructureStockpiles[(EntityID)Structure] = stockpile;
		}
		else
		{
			entityGroupToUse.TerminalTradeOffers[(EntityID)Structure] = stockpile;
		}
		return true;
	}
}
