using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;

namespace UWGame.SimSide.Entities.Containers.Components;

public class TerminalContainerType : ContainerType, IHasItemStorageType
{
	public Vector2[] Doors;

	public bool HasRallyPointInCourtyard;

	public ItemStorageType OfferedForTradeStorageType;

	public string DefaultStorageSettings;

	public bool AllowStockpiling = true;

	public ItemStorageType ItemStorageType { get; set; }

	[XmlIgnore]
	public DefaultStorageSettings DefaultStorageSettingsFinal { get; private set; }

	public bool AllowsStockpiling => AllowStockpiling;

	public override float? FullStatePercentage => OfferedForTradeStorageType.FullStatePercentage;

	public override float? HalfFullStatePercentage => OfferedForTradeStorageType.HalfFullStatePercentage;

	public override Container CreateContainer(Entity parent)
	{
		return new TerminalContainer(parent);
	}

	public override DefaultStorageSettings GetDefaultStorageSettings()
	{
		return DefaultStorageSettingsFinal;
	}

	public override void Initialize()
	{
		base.Initialize();
		if (ItemStorageType != null)
		{
			ItemStorageType.Initialize();
		}
		if (DefaultStorageSettings != null)
		{
			DefaultStorageSettingsFinal = GameData.Instance.AllDefaultStorageSettings[DefaultStorageSettings];
		}
	}

	public override void PostInitValidate(EntityType parent, ref List<string> listOfErrors)
	{
		base.PostInitValidate(parent, ref listOfErrors);
		if (Doors == null && CanBeEnteredByTags != null)
		{
			EntityType.CreateValidationError(ref listOfErrors, "CanBeEnteredByTags requires Doors to be specified also.");
		}
	}

	public override void PostLoadContentInitialize(EntityType parent)
	{
		base.PostLoadContentInitialize(parent);
	}
}
