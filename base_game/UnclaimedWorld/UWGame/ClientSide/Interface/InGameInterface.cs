using System;
using System.Collections.Generic;
using System.Text;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.Client.Interface.HUD_Windows;
using UWGame.ClientSide.HelpTopics;
using UWGame.ClientSide.Interface.BuyAndSell;
using UWGame.ClientSide.Interface.Controls;
using UWGame.ClientSide.Interface.DateAndWeatherPanel;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.ClientSide.Interface.Ledger;
using UWGame.ClientSide.Interface.MapGUI;
using UWGame.ClientSide.Interface.Missions;
using UWGame.ClientSide.Interface.Overlays;
using UWGame.ClientSide.Interface.Personnel;
using UWGame.ClientSide.Interface.Policy;
using UWGame.ClientSide.Interface.Tasks;
using UWGame.ClientSide.Interface.World_map;
using UWGame.Control;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Policies;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.XmlCollections;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class InGameInterface : CommonInterface
{
	public enum InterfaceState
	{
		None,
		Build,
		Threat,
		BlockSubtile,
		UnblockSubtile,
		Hunt,
		FindPrey,
		Salvage,
		PlaceExpeditionCenter,
		EditorPlaceEntity,
		EditorDeleteEntities,
		EditorClearTile,
		Launch,
		EditorTool
	}

	public delegate void SelectedEntityChanged(EntityID? oldEntity, EntityID? newEntity);

	public struct EntityPosition
	{
		public Vector2 Position;

		public Entity Entity;
	}

	private Allegiance uiAllegiance;

	public ExpeditionID? UIExpedition;

	public EntityGroupID? UIOwner;

	private HashSet<IKnownEntityData> entitiesToShowMarkerWindowsFor = new HashSet<IKnownEntityData>();

	private List<object> markersToRemove = new List<object>();

	public OverlaySettings OverlaySettings;

	public InventorySettings InventorySettings;

	public FoodProductionSettings FoodProductionSettings;

	public ProductionSettings ProductionSettings;

	public KillsSettings KillsSettings;

	public NutrientSheetSettings NutrientSheetSettings;

	public TaskSettings TaskSettings;

	public BuySellPanelSettings BuySettings;

	public BuySellPanelSettings SellSettings;

	private InterfaceState interfaceMode;

	public TerrainTile HoverTile;

	public MapArea SelectedTiles;

	public MapArea SelectedTilesPreview;

	private Zone selectedZone;

	public bool TrackSelectedEntity;

	private bool showOverlaysAndMarkerWindows = true;

	public Selection Selection;

	public TerrainBlockingRender TerrainBlockingRender;

	public ThreatRender ThreatRender;

	private EntityID? selectedEntity;

	public List<EntityPosition> EntitiesBeingPlaced = new List<EntityPosition>();

	public int leftMargin = 20;

	public int topMargin = 60;

	private int maxPanelHeight = 800;

	public const int InterfaceWidth = 309;

	public const int BottomAreaExcludedFromHUD = 160;

	public int MainLeft;

	public int MainTop;

	public const int SmallPanelTop = 213;

	private const int rosterPanelBottomMargin = 100;

	public int expandedInterfaceLeft;

	public const int rosterPanelTop = 18;

	public int expandedInterfaceWidth = 800;

	public int rosterPanelHeight;

	public int sidePanelHeight;

	public int sidePanelFullHeight;

	private RosterPanel displayedRosterPanel;

	public InventoryPanel InventoryPanel;

	public JobsPanel JobsPanel;

	public GraphPanel GraphPanel;

	public LedgerPanel LedgerPanel;

	public DiplomacyPanel DiplomacyPanel;

	public WorldMapRosterPanel WorldMapPanel;

	public MissionsPanel MissionsPanel;

	public EventArchivePanel EventArchivePanel;

	public PersonnelRosterPanel PersonnelRosterPanel;

	public PolicyPanel PolicyPanel;

	public CreateMissionPanel CreateMissionPanel;

	public MessageBox MessageBox;

	public SaveLoadMessageBox SaveLoadMessageBox;

	public InGameMenuDialog MenuDialog;

	public BuySellPanel BuySellDialog;

	public PersonnelDialog PersonnelDialog;

	public WorldMapDialog WorldMapDialog;

	public FramedCRT framedCRT;

	public SpriteFont InterfaceFont;

	public HUDOverlayPanel HUDOverlayPanel;

	public HUDHelpPanel HUDHelpPanel;

	public HUDEntityContextMenu HUDActionPanel;

	public SidePanelEmpty SidePanelEmpty;

	public SidePanelEntity SidePanelEntity;

	public SidePanelMapArea SidePanelMapArea;

	public SidePanelEditorEntity SidePanelEditorEntity;

	public SidePanelEditorSoil SidePanelEditorSoil;

	public SidePanelEditorTerrainHeight SidePanelEditorTerrainHeight;

	public LogPanel LogPanel;

	public RosterAccessPanel RosterAccessPanel;

	public MainPanel MainPanel;

	public Minimap Minimap;

	public OverlayPanel OverlayPanel;

	public FogMap FogMap;

	private List<HUDWindow> hudWindows = new List<HUDWindow>();

	private Pool<SpokenLine> poolOfSpokenLines;

	private List<SpokenLine> activeSpokenLines = new List<SpokenLine>();

	private Pool<MarkerWindow> poolOfMarkerWindows;

	private Dictionary<object, MarkerWindow> activeMarkerWindows = new Dictionary<object, MarkerWindow>();

	private float screenBoundsExtension = 100f;

	public ContextMenuOpener ContextMenuOpener;

	public TileSelectionContextMenu ContextMenu;

	public List<DataSheet> EntityTypeTooltipsStack;

	public HashSet<DataSheet> PinnedDataTypeTooltips;

	public HashSet<DataSheet> UnpinnedDataTypeTooltipsOutsideStack;

	public new Tooltip Tooltip;

	public Pool<EntityDataSheet> poolOfEntityTypeTooltips;

	public Pool<ProcessTypeDataSheet> poolOfProcessTypeTooltips;

	public EventDialog EventDialog;

	public TalkPanel TalkPanel;

	public Dictionary<HelpTopic, HelpTopicDialog> HelpTopicDialogs = new Dictionary<HelpTopic, HelpTopicDialog>();

	public SetTileResourcesWindow SetTileResources;

	public EntityListWindow EntityListWindow;

	public SiteWindow SiteWindow;

	public RatingsPanel RatingsPanel;

	public CounterPanel CounterPanel;

	private Rectangle characterNearbyBounds;

	public SelectRectangle SelectRectangle;

	public StatusScreen StatusScreen;

	// PORT DEVIATION 1 (see PORTING-NOTES.md).
	// The original IL declares this field as System.Windows.Forms.Help - the game has no type
	// of its own called Help, so a `using System.Windows.Forms;` (since removed) silently bound it
	// BCL type. On .NET Framework 4.5 that was a plain instantiable class; on .NET 8 it is a
	// `static class`, and C# forbids a field of static type (CS0723).
	// The field is dead: nothing in the entire 2170-type assembly ever reads or writes it.
	// Retyped to object to keep the member (and its name) present for anything that walks
	// fields reflectively, without pulling in the now-static BCL type.
	public object Help;

	private DateAndWeather dateAndWeather;

	private Window window;

	public Animation2D SelectedCycleAnimationQuick;

	public Animation2DPlayer SelectedCyclePlayerFlashing;

	public Animation2D SelectedCycleAnimation;

	public Animation2DPlayer SelectedCyclePlayer;

	public const int minimapMinWidth = 200;

	public const int minimapMaxWidth = 280;

	private TextWindow replayTime;

	private Regulator ownerRegulator = new Regulator(The.Client.ClientRandomGenerator, 2.0, "InGameInterfaceOwner");

	public Allegiance UIAllegiance
	{
		get
		{
			if (uiAllegiance == null)
			{
				uiAllegiance = The.Sim.PlaySite.PlayerAllegiance;
			}
			return uiAllegiance;
		}
		set
		{
			uiAllegiance = value;
		}
	}

	public InterfaceState InterfaceMode
	{
		get
		{
			return interfaceMode;
		}
		set
		{
			DestroyEntityBeingPlaced();
			interfaceMode = value;
		}
	}

	public EntityID? HoverEntity { get; private set; }

	public Zone SelectedZone
	{
		get
		{
			return selectedZone;
		}
		set
		{
			if (selectedZone != value)
			{
				if (selectedZone != null)
				{
					selectedZone.MapArea.MapAreaRender.IsSelected = false;
				}
				selectedZone = value;
				if (selectedZone != null)
				{
					selectedZone.MapArea.MapAreaRender.IsSelected = true;
					RemoveSelectedTiles();
				}
			}
		}
	}

	public EntityID? SelectedEntity
	{
		get
		{
			return selectedEntity;
		}
		private set
		{
			if (selectedEntity != value)
			{
				EnableTracking(enable: false);
				EntityID? oldEntity = selectedEntity;
				selectedEntity = value;
				if (selectedEntity.HasValue)
				{
					ChangeRosterPanel(SidePanelEntity, refreshCurrentPanelWithNewContent: true);
				}
				if (this.SelectedEntityChangedEvent != null)
				{
					this.SelectedEntityChangedEvent(oldEntity, selectedEntity);
				}
			}
		}
	}

	public bool ShowOverlaysAndMarkerWindows
	{
		get
		{
			return showOverlaysAndMarkerWindows;
		}
		set
		{
			if (showOverlaysAndMarkerWindows == value)
			{
				return;
			}
			showOverlaysAndMarkerWindows = value;
			if (!showOverlaysAndMarkerWindows)
			{
				foreach (KeyValuePair<object, MarkerWindow> activeMarkerWindow in activeMarkerWindows)
				{
					activeMarkerWindow.Value.Hide();
				}
				return;
			}
			foreach (KeyValuePair<object, MarkerWindow> activeMarkerWindow2 in activeMarkerWindows)
			{
				activeMarkerWindow2.Value.Show();
			}
		}
	}

	public event SelectedEntityChanged SelectedEntityChangedEvent;

	public void SetHoverEntity(EntityID? value, Color? hoverTintingColor = null)
	{
		EntityID? hoverEntity = HoverEntity;
		EntityID? entityID = value;
		if (hoverEntity.GetValueOrDefault() != entityID.GetValueOrDefault() || hoverEntity.HasValue != entityID.HasValue || The.InGameUI.Selection.HoverTintingColor != hoverTintingColor)
		{
			HoverEntity = value;
			Selection.StartHoverOverEntity(hoverTintingColor);
		}
	}

	public Expedition GetExpedition()
	{
		if (The.InGameUI.UIExpedition.HasValue)
		{
			return Expedition.FindByID(The.InGameUI.UIExpedition.Value);
		}
		return null;
	}

	public void RemoveSelectedTiles()
	{
		SelectedTiles.Clear();
		if (ContextMenuOpener != null)
		{
			ContextMenuOpener.Hide();
		}
	}

	public void SetExpeditionMarkerPositions()
	{
		if (!ShowOverlaysAndMarkerWindows)
		{
			return;
		}
		foreach (KeyValuePair<object, MarkerWindow> activeMarkerWindow in activeMarkerWindows)
		{
			if (activeMarkerWindow.Value.Expedition != null)
			{
				activeMarkerWindow.Value.Show();
			}
		}
	}

	public OwnerID? GetUIOwnerID()
	{
		if (UIExpedition.HasValue)
		{
			Expedition expedition = LookUp<Expedition, ExpeditionID>.FindByID(UIExpedition);
			if (expedition != null)
			{
				return ((ILookUp<IOwner, OwnerID>)expedition).ID;
			}
		}
		return null;
	}

	public InGameInterface(UnclaimedWorld game, SerializableDictionary<string, string> customColors)
		: base(game)
	{
		if (customColors == null)
		{
			return;
		}
		foreach (KeyValuePair<string, string> customColor in customColors)
		{
			gui.CustomColors.Add(customColor.Key, customColor.Value.ColorFromHex());
		}
	}

	public void AddHudWindow(HUDWindow window)
	{
		hudWindows.Add(window);
	}

	public void RemoveSpokenLine(SpokenLine line)
	{
		if (line != null && activeSpokenLines.Remove(line))
		{
			line.Speaker = null;
			line.Parent.ShowWhileLineIsSpoken = false;
			line.Parent = null;
			line.Hide();
			poolOfSpokenLines.Retire(line);
		}
	}

	public void ShowSpokenLine(Entity speaker, string text, float spokenLineDisplaySecondsLeft)
	{
		if (!activeMarkerWindows.TryGetValue(speaker.EntityID, out var value))
		{
			TryToAddEntityMarkerWindow(speaker.EntityID, out value);
		}
		if (value != null)
		{
			SpokenLine spokenLine = activeSpokenLines.Find((SpokenLine s) => s.Speaker == speaker.EntityID);
			if (spokenLine == null)
			{
				spokenLine = poolOfSpokenLines.Get();
				activeSpokenLines.Add(spokenLine);
			}
			spokenLine.Init(speaker, text, activeMarkerWindows[speaker.EntityID]);
			spokenLine.Show();
		}
	}

	public void FillAndShowBuySellDialog(Point absolutePosition, BuySellPanel.BuySellDialogMode mode, Dictionary<EntityType, List<EntityID>> currentOrders, BuySellPanel.CanTradeDelegate canTrade, EntityGroup ownerOfItems, EntityGroup buyerOfItems, BuySellPanel.Func<EntityType, bool, List<EntityID>> getItems)
	{
		BuySellPanelSettings settings = null;
		switch (mode)
		{
		case BuySellPanel.BuySellDialogMode.ViewBuyAtNPC:
		case BuySellPanel.BuySellDialogMode.ActionBuyAtNPC:
			settings = The.InGameUI.BuySettings;
			break;
		case BuySellPanel.BuySellDialogMode.ViewSellAtNPC:
		case BuySellPanel.BuySellDialogMode.ActionSellAtPlayer:
			settings = The.InGameUI.SellSettings;
			break;
		}
		EntityGroupID? buyerID = null;
		if (buyerOfItems != null)
		{
			buyerID = buyerOfItems.ID;
		}
		EntityGroupID? ownerID = null;
		if (ownerOfItems != null)
		{
			ownerID = ownerOfItems.ID;
		}
		BuySellDialog.Fill(mode, getItems, canTrade, ownerID, buyerID, currentOrders, settings);
		BuySellDialog.ShowInScreenSpace(absolutePosition.X - BuySellDialog.Window.Width / 2, absolutePosition.Y / 2);
	}

	public static bool CanTrade(Expedition expedition, EntityType entityType, out TierOrAreaType unavailablePolicy)
	{
		unavailablePolicy = null;
		if (expedition != null && !expedition.Policy.CanProduceOrTrade(entityType.TierOrAreaType))
		{
			unavailablePolicy = entityType.TierOrAreaType;
		}
		return unavailablePolicy == null;
	}

	public override void LoadContent()
	{
		base.LoadContent();
		InterfaceFont = The.Client.Content.Load<SpriteFont>("Arial");
		if (!Snapshotter.IsSnapshotting && The.Sim.StartGameParams.StartScenarioParams != null)
		{
			The.Sim.StartGameParams.StartScenarioParams.Scenario.ScenarioData.LoadContent(The.Client.Content);
		}
	}

	public void OnSetProduction(string entityTypeKey)
	{
		InventoryPanel.OnSetProduction(entityTypeKey);
		foreach (DataSheet item in EntityTypeTooltipsStack)
		{
			item.OnSetProduction();
		}
		foreach (DataSheet pinnedDataTypeTooltip in PinnedDataTypeTooltips)
		{
			pinnedDataTypeTooltip.OnSetProduction();
		}
		foreach (DataSheet item2 in UnpinnedDataTypeTooltipsOutsideStack)
		{
			item2.OnSetProduction();
		}
	}

	public void SetUIAllegianceToSelectedEntity()
	{
		if (!SelectedEntity.HasValue)
		{
			return;
		}
		Entity entity = Entity.FindByID(SelectedEntity.Value);
		if (entity == null)
		{
			return;
		}
		UIAllegiance = entity.Intelligence.Allegiance;
		List<EntityID> list = null;
		foreach (KeyValuePair<EntityID, MemoryFact> memoryFact in UIAllegiance.SharedKnowledge.MemoryFacts)
		{
			Entity entity2 = Entity.FindByID(memoryFact.Key);
			if (entity2 != null)
			{
				The.Client.SetRenderableOnMemoryFact(memoryFact.Value, entity2, UIAllegiance);
			}
			else
			{
				Common.AddToList(ref list, memoryFact.Key);
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (EntityID item in list)
		{
			UIAllegiance.SharedKnowledge.DeleteMemoryOfEntity(item, null, removeAllKnowledge: true);
		}
	}

	public void ResetUIAllegiance()
	{
		UIAllegiance = The.Sim.PlaySite.PlayerAllegiance;
	}

	private void UpdateMarkerWindows(GameTime elapsedTime, CollideShape2D screenBounds)
	{
		RefreshEntityMarkers(elapsedTime, screenBounds);
		RefreshExpeditionMarkers(screenBounds);
		RefreshZoneMarkers(screenBounds);
		foreach (KeyValuePair<object, MarkerWindow> activeMarkerWindow in activeMarkerWindows)
		{
			if (activeMarkerWindow.Value.GetCurrentType() == MarkerWindow.MarkerType.Entity && !activeMarkerWindow.Value.EntityIsValid())
			{
				markersToRemove.Add(activeMarkerWindow.Key);
				continue;
			}
			if (activeMarkerWindow.Value.GetCurrentType() != MarkerWindow.MarkerType.Entity && !screenBounds.ContainsPoint(activeMarkerWindow.Value.MarkerWorldPosition))
			{
				markersToRemove.Add(activeMarkerWindow.Key);
				continue;
			}
			if (!activeMarkerWindow.Value.IsRenewedThisFrame && !activeMarkerWindow.Value.ShowWhileLineIsSpoken)
			{
				markersToRemove.Add(activeMarkerWindow.Key);
			}
			activeMarkerWindow.Value.Update();
			activeMarkerWindow.Value.IsRenewedThisFrame = false;
		}
		foreach (object item in markersToRemove)
		{
			MarkerWindow markerWindow = activeMarkerWindows[item];
			activeMarkerWindows.Remove(item);
			markerWindow.Reset();
			markerWindow.Hide();
			poolOfMarkerWindows.Retire(markerWindow);
		}
		markersToRemove.Clear();
	}

	public void UpdateSpokenLines(CollideShape2D screenBounds)
	{
		foreach (SpokenLine activeSpokenLine in activeSpokenLines)
		{
			screenBounds.ContainsPoint(activeSpokenLine.GetScreenPosition().ToVector2());
		}
	}

	public DataSheet GetAnySpawnedTooltip(DataTypeButton spawningButton)
	{
		DataSheet dataSheet = EntityTypeTooltipsStack.Find((DataSheet tt) => tt.SpawningControl == spawningButton);
		if (dataSheet == null)
		{
			foreach (DataSheet pinnedDataTypeTooltip in PinnedDataTypeTooltips)
			{
				if (spawningButton.ShowsData(pinnedDataTypeTooltip))
				{
					dataSheet = pinnedDataTypeTooltip;
					break;
				}
			}
			foreach (DataSheet item in UnpinnedDataTypeTooltipsOutsideStack)
			{
				if (spawningButton.ShowsData(item))
				{
					dataSheet = item;
					break;
				}
			}
		}
		return dataSheet;
	}

	public bool TryToAddZoneMarker(Zone zone, CollideShape2D screenBounds)
	{
		try
		{
			if (zone.MapArea.BottomLeftTile == null)
			{
				return false;
			}
			if (screenBounds.ContainsPoint(MapManager.TilePosToWorldPos(zone.MapArea.BottomLeftTile.TilePos).ToVector2()))
			{
				MarkerWindow markerWindow = poolOfMarkerWindows.Get();
				if (markerWindow != null)
				{
					markerWindow.FillWithZoneInfo(zone);
					activeMarkerWindows.Add(zone, markerWindow);
					markerWindow.Show();
					return true;
				}
			}
			return false;
		}
		catch (Exception ex)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(ex.Message);
			stringBuilder.AppendLine("Info: ");
			if (screenBounds == null)
			{
				stringBuilder.AppendLine("Screenbounds is null");
			}
			if (zone == null)
			{
				stringBuilder.AppendLine("Zone is null");
			}
			else if (zone.MapArea == null)
			{
				stringBuilder.AppendLine("zone.MapArea is null");
			}
			else if (zone.MapArea.BottomLeftTile == null)
			{
				stringBuilder.AppendLine("zone.MapArea.BottomLeftTile is null");
			}
			throw new Exception(stringBuilder.ToString());
		}
	}

	private void RefreshExpeditionMarkers(CollideShape2D screenBounds)
	{
		if (!showOverlaysAndMarkerWindows || UIAllegiance.HumanActivities == null)
		{
			return;
		}
		foreach (Expedition expedition in UIAllegiance.Expeditions)
		{
			if (!activeMarkerWindows.TryGetValue(expedition, out var value))
			{
				if (TryToAddExpeditionMarker(expedition, screenBounds))
				{
					activeMarkerWindows[expedition].IsRenewedThisFrame = true;
				}
			}
			else
			{
				value.IsRenewedThisFrame = true;
			}
		}
	}

	private void RefreshZoneMarkers(CollideShape2D screenBounds)
	{
		if (!The.InGameUI.UIExpedition.HasValue)
		{
			return;
		}
		Expedition expedition = Expedition.FindByID(The.InGameUI.UIExpedition.Value);
		if (expedition == null)
		{
			return;
		}
		foreach (Zone zone in expedition.OwnedEntities.Zones)
		{
			if (!activeMarkerWindows.TryGetValue(zone, out var value))
			{
				if (TryToAddZoneMarker(zone, screenBounds))
				{
					activeMarkerWindows[zone].IsRenewedThisFrame = true;
				}
			}
			else
			{
				value.IsRenewedThisFrame = true;
			}
		}
	}

	public bool TryToAddExpeditionMarker(Expedition expedition, CollideShape2D screenBounds)
	{
		if (screenBounds.ContainsPoint(expedition.Location.Value.ToVector2()))
		{
			MarkerWindow markerWindow = poolOfMarkerWindows.Get();
			if (markerWindow != null)
			{
				markerWindow.FillWithExpeditionInfo(expedition);
				activeMarkerWindows.Add(expedition, markerWindow);
				markerWindow.Show();
				return true;
			}
		}
		return false;
	}

	private void RefreshEntityMarkers(GameTime elapsedTime, CollideShape2D screenBounds)
	{
		List<Pair<EntityID, Vector2>> resultsList = new List<Pair<EntityID, Vector2>>();
		UIAllegiance.SharedKnowledge.PlaySiteKnowledge.KnownEntityDataTree.GetObjectsIntersectingBounds(screenBounds, null, ref resultsList);
		MarkerWindow markerWindow = null;
		foreach (Pair<EntityID, Vector2> item in resultsList)
		{
			bool flag = false;
			markerWindow = null;
			The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(item.First, out var data);
			if (data != null)
			{
				if (data.PartOfID.HasValue)
				{
					continue;
				}
				activeMarkerWindows.TryGetValue(data.EntityID, out markerWindow);
				if (data.EntityID == SelectedEntity)
				{
					flag = true;
				}
				else
				{
					switch (data.EntityType.GetShowMarkerWindowMode())
					{
					case EntityType.ShowMarkerWindowMode.Always:
						if (showOverlaysAndMarkerWindows)
						{
							flag = true;
						}
						break;
					case EntityType.ShowMarkerWindowMode.ByStatus:
						if (showOverlaysAndMarkerWindows)
						{
							if (data.IsTimeToShowStatusMarkerWindow())
							{
								flag = (MarkerWindow.HasStatusIconsToShow(data) ? true : false);
							}
							else if (markerWindow != null)
							{
								flag = true;
							}
						}
						break;
					}
				}
			}
			if (flag)
			{
				if (markerWindow == null && !TryToAddEntityMarkerWindow(item.First, out markerWindow))
				{
					break;
				}
				if (!markerWindow.DisplayWindow.Visible)
				{
					markerWindow.Show();
				}
				markerWindow.IsRenewedThisFrame = true;
			}
		}
	}

	private bool TryToAddEntityMarkerWindow(EntityID entityID, out MarkerWindow activeMarkerWindow)
	{
		activeMarkerWindow = poolOfMarkerWindows.Get();
		if (activeMarkerWindow == null)
		{
			return false;
		}
		activeMarkerWindow.FillWithEntityInfo(entityID);
		activeMarkerWindows.Add(entityID, activeMarkerWindow);
		activeMarkerWindow.Show();
		return true;
	}

	public void Initialize()
	{
		expandedInterfaceWidth = Math.Min(750, The.MapUI.mapWindowWidth);
		The.MapUI.noOfTilesLeftOfDetailsInterface = Math.Max((The.MapUI.mapWindowWidth - expandedInterfaceWidth) / 48, 0);
		The.MapUI.widthOfWindowLeftOfDetailsInterface = Math.Max(The.MapUI.mapWindowWidth - expandedInterfaceWidth, 0);
		MainLeft = The.MapUI.mapWindowWidth - 309;
		MainTop = The.MapUI.mapWindowHeight;
		sidePanelHeight = Math.Min(The.MapUI.mapWindowHeight - 213 - 100, maxPanelHeight);
		sidePanelFullHeight = The.MapUI.mapWindowHeight - 213 - 10;
		expandedInterfaceLeft = Math.Max(The.MapUI.mapWindowWidth - expandedInterfaceWidth, 0);
		rosterPanelHeight = The.MapUI.mapWindowHeight - 110;
		PinnedDataTypeTooltips = new HashSet<DataSheet>();
		UnpinnedDataTypeTooltipsOutsideStack = new HashSet<DataSheet>();
		gui.HyperlinkClicked += gui_HyperlinkClicked;
		gui.MouseOverWindow += gui_MouseOverWindow;
		gui.MouseOutOfWindow += gui_MouseOutOfWindow;
		Selection = new Selection();
		SelectRectangle = new SelectRectangle();
		TerrainBlockingRender = new TerrainBlockingRender();
		SelectedCycleAnimationQuick = new Animation2D(null, 0.15f, isLooping: true)
		{
			DoColorInterpolation = true,
			Cells = new List<Cell>
			{
				new Cell
				{
					Color = Color.White
				},
				new Cell
				{
					Color = Animation2D.halfTransp
				},
				new Cell
				{
					Color = Color.White
				}
			}
		};
		SelectedCyclePlayerFlashing = new Animation2DPlayer();
		SelectedCyclePlayerFlashing.StartAnimation(SelectedCycleAnimationQuick);
		SelectedCycleAnimation = new Animation2D(null, 0.6f, isLooping: true)
		{
			DoColorInterpolation = true,
			Cells = new List<Cell>
			{
				new Cell
				{
					Color = Color.White
				},
				new Cell
				{
					Color = Animation2D.halfTransp
				},
				new Cell
				{
					Color = Color.White
				}
			}
		};
		SelectedCyclePlayer = new Animation2DPlayer();
		SelectedCyclePlayer.StartAnimation(SelectedCycleAnimation);
		SelectedTiles = new MapArea();
		SelectedTiles.Initialize();
		SelectedTiles.MapAreaRender.IsSelected = true;
		SelectedTiles.MapAreaRender.Color = Color.Yellow;
		SelectedTilesPreview = new MapArea();
		SelectedTilesPreview.Initialize();
		SelectedTilesPreview.MapAreaRender.IsSelected = false;
		SelectedTilesPreview.MapAreaRender.Color = Color.Gray;
		MenuDialog = new InGameMenuDialog();
		poolOfEntityTypeTooltips = new Pool<EntityDataSheet>(1, 15);
		poolOfProcessTypeTooltips = new Pool<ProcessTypeDataSheet>(1, 5);
		poolOfMarkerWindows = new Pool<MarkerWindow>(20, 1);
		poolOfSpokenLines = new Pool<SpokenLine>(1, 5);
		EventDialog = new EventDialog(this);
		BuySellDialog = new BuySellPanel(this, Point.Zero);
		MessageBox = new MessageBox(this);
		SaveLoadMessageBox = new SaveLoadMessageBox(this);
		PersonnelDialog = new PersonnelDialog(this, Point.Zero);
		WorldMapDialog = new WorldMapDialog(this, Point.Zero);
		HelpTopicDialogs = new Dictionary<HelpTopic, HelpTopicDialog>();
		foreach (KeyValuePair<string, HelpTopic> allHelpTopic in GameData.Instance.AllHelpTopics)
		{
			HelpTopicDialog value = new HelpTopicDialog(allHelpTopic.Value);
			HelpTopicDialogs.Add(allHelpTopic.Value, value);
		}
		foreach (KeyValuePair<string, HelpTopic> allTutorialTopic in GameData.Instance.AllTutorialTopics)
		{
			HelpTopicDialog value2 = new HelpTopicDialog(allTutorialTopic.Value);
			HelpTopicDialogs.Add(allTutorialTopic.Value, value2);
		}
		replayTime = new TextWindow(hasSurface: false);
		replayTime.ShowInScreenSpace(10, 100);
		if (The.Sim.Mode == Sim.EngineMode.Game)
		{
			InventorySettings = new InventorySettings();
			OverlaySettings = new OverlaySettings();
			BuySettings = new BuySellPanelSettings();
			SellSettings = new BuySellPanelSettings();
			TaskSettings = new TaskSettings();
			FoodProductionSettings = new FoodProductionSettings();
			ProductionSettings = new ProductionSettings();
			KillsSettings = new KillsSettings();
			NutrientSheetSettings = new NutrientSheetSettings();
			ThreatRender = new ThreatRender();
		}
	}

	private void gui_MouseOutOfWindow(Window obj)
	{
	}

	private void gui_MouseOverWindow(Window obj)
	{
	}

	public void ShowInGameMenu()
	{
		The.InGameUI.MenuDialog.ShowDialog(modal: true);
		MainPanel.tbMain.IsChecked = true;
	}

	public void HideInGameMenu()
	{
		The.InGameUI.MenuDialog.Hide();
		The.Client.SetModal(value: false);
		MainPanel.tbMain.IsChecked = false;
	}

	public void ShowContextMenu(int screenX, int screenY, Window parentWindow)
	{
		ContextMenu.OpeningWindow = parentWindow;
		if (!ContextMenu.DisplayWindow.IsVisibleAndActive)
		{
			ContextMenu.RefreshThisPanel();
			ContextMenu.ShowOnPlayfield(screenX, screenY);
		}
	}

	public void DestroyEntityBeingPlaced()
	{
		if (EntitiesBeingPlaced != null)
		{
			foreach (EntityPosition item in EntitiesBeingPlaced)
			{
				item.Entity.Destroy();
			}
		}
		EntitiesBeingPlaced.Clear();
	}

	private void gui_HyperlinkClicked(uint? entityID, uint? containerID, uint? zoneID, Point? mapPosition, GUIManager.MouseButtonClicked button)
	{
		if (entityID.HasValue)
		{
			UIAllegiance.SharedKnowledge.GetKnownData((EntityID)entityID.Value, out var data);
			if (CanSelectEntity(data))
			{
				if (button == GUIManager.MouseButtonClicked.Left)
				{
					if (selectedEntity == data.EntityID)
					{
						ZoomToEntity(data);
					}
					else
					{
						SelectEntity(data);
					}
				}
				else
				{
					ZoomToEntity(data);
				}
			}
		}
		if (mapPosition.HasValue)
		{
			The.MapUI.ZoomToMapPosition(mapPosition.Value.X, mapPosition.Value.Y);
		}
		if (zoneID.HasValue)
		{
			Zone zone = LookUp<Zone, ZoneID>.FindByID((ZoneID)zoneID.Value);
			if (zone != null)
			{
				The.MapUI.ZoomToMapPosition(zone.MapArea.UpperLeftTile.TilePos.ToPoint());
			}
		}
		if (containerID.HasValue)
		{
			ResourceContainer resourceContainer = LookUp<ResourceContainer, ResourceID>.FindByID((ResourceID)containerID.Value);
			if (resourceContainer != null)
			{
				resourceContainer.FlashWhenClicked();
				The.MapUI.ZoomToMapPosition(resourceContainer.MapPosition.X, resourceContainer.MapPosition.Y);
			}
		}
	}

	public void EnableTracking(bool enable)
	{
		if (StatusScreen != null)
		{
			StatusScreen.SetCenterButtonChecked(enable);
		}
		TrackSelectedEntity = enable;
	}

	public void ZoomToEntity(IKnownEntityData entity)
	{
		The.MapUI.ZoomToMapPosition(entity.PlaySiteLocation);
	}

	public static void PlaceWindowInsideViewableArea(UIComponent window, UIComponent boundingArea)
	{
		int width = boundingArea.Width;
		if (window.X < 0)
		{
			window.X = 0;
		}
		if (window.Right > width)
		{
			window.X = width - window.Width;
		}
		int height = boundingArea.Height;
		if (window.Y < 0)
		{
			window.Y = 0;
		}
		if (window.Bottom > height)
		{
			window.Y = height - window.Height;
		}
	}

	public void HideMapInterface()
	{
		InterfaceMode = InterfaceState.None;
		ChangeRosterPanel(The.InGameUI.SidePanelEmpty);
		SelectedEntity = null;
		RemoveSelectedTiles();
		CloseRosterPanel();
		foreach (HUDWindow hudWindow in hudWindows)
		{
			if (hudWindow.HideOnRightClick && hudWindow.DisplayWindow.IsVisibleAndActive)
			{
				hudWindow.Hide();
			}
		}
		ShowOverlaysAndMarkerWindows = false;
	}

	public void CloseRosterPanel()
	{
		if (displayedRosterPanel != null)
		{
			displayedRosterPanel.Hide();
			displayedRosterPanel = null;
		}
		HideStatusScreen();
	}

	public void PostLoadContent()
	{
		if (The.Sim.Mode != Sim.EngineMode.Edit)
		{
			FogMap = new FogMap();
		}
		framedCRT = new FramedCRT(source: new Rectangle(The.InGameUI.expandedInterfaceLeft, 18, 533, 400), intf: this, dimension: new Rectangle(The.InGameUI.expandedInterfaceLeft, 18, 533, 400), level: Level.Middle);
		framedCRT.crtTextAnimatorCharacter.TimeBetweenUpdates = 0.03f;
		framedCRT.crtTextAnimatorLine.TimeBetweenUpdates = 0.12f;
		StatusScreen = new StatusScreen();
		HUDOverlayPanel = new HUDOverlayPanel();
		if (The.Sim.Mode != Sim.EngineMode.Edit)
		{
			HUDActionPanel = new HUDEntityContextMenu();
			HUDHelpPanel = new HUDHelpPanel();
		}
		SidePanelEmpty = new SidePanelEmpty();
		ChangeRosterPanel(SidePanelEmpty);
		SidePanelEntity = new SidePanelEntity();
		SidePanelMapArea = new SidePanelMapArea();
		if (The.Sim.Mode == Sim.EngineMode.Game)
		{
			InventoryPanel = new InventoryPanel();
			JobsPanel = new JobsPanel();
			GraphPanel = new GraphPanel();
			LedgerPanel = new LedgerPanel();
			WorldMapPanel = new WorldMapRosterPanel();
			MissionsPanel = new MissionsPanel();
			CreateMissionPanel = new CreateMissionPanel();
			EventArchivePanel = new EventArchivePanel();
			PersonnelRosterPanel = new PersonnelRosterPanel();
			PolicyPanel = new PolicyPanel();
		}
		else
		{
			SidePanelEditorEntity = new SidePanelEditorEntity();
			SidePanelEditorSoil = new SidePanelEditorSoil();
			SidePanelEditorTerrainHeight = new SidePanelEditorTerrainHeight();
			SetTileResources = new SetTileResourcesWindow();
		}
		RosterAccessPanel = new RosterAccessPanel();
		MainPanel = new MainPanel();
		int num = (int)Common.Clamp(0.22 * (double)The.MapUI.mapWindowWidth, 200.0, 280.0);
		int screenHeight = (int)(0.75 * (double)num);
		int num2 = ((num == 280) ? 15 : 0);
		OverlayPanel = new OverlayPanel(num2 + Minimap.GetWidth(num));
		Minimap = new Minimap(num, screenHeight, num2);
		EntityTypeTooltipsStack = new List<DataSheet>();
		Tooltip = new Tooltip(The.InGameUI);
		if (The.Sim.Mode == Sim.EngineMode.Game)
		{
			SetupEventLogPanel();
			SetupCountersAndDateTimePanels();
			TalkPanel = new TalkPanel();
			ContextMenuOpener = new ContextMenuOpener();
			ContextMenu = new TileSelectionContextMenu();
			EntityListWindow = new EntityListWindow();
			RatingsPanel = new RatingsPanel();
			CounterPanel = new CounterPanel(400);
			SetCounterPanelPosition();
			if (The.Sim.StartGameParams.StartScenarioParams != null)
			{
				EnableScenarioUI(The.Sim.StartGameParams.StartScenarioParams.Scenario.ScenarioData);
			}
		}
		Selection.PostLoadContent();
		SelectedTiles.PostLoadContent();
		TerrainBlockingRender.PostLoadContent();
		ThreatRender.PostLoadContent();
		ArrangeAccessPanels();
		ArrangeTopPanels();
		The.InGameUI.HideMapInterface();
	}

	private void SetCounterPanelPosition()
	{
		CounterPanel.Window.X = RatingsPanel.Window.Right - 2;
	}

	private void ArrangeTopPanels()
	{
		if (CounterPanel != null && CounterPanel.Window.Right > dateAndWeather.PlasticPanel.X)
		{
			RatingsPanel.Window.X = MainPanel.DisplayWindow.Right;
			SetCounterPanelPosition();
			dateAndWeather.SetPosition(CounterPanel.Window.Right);
		}
	}

	private void ArrangeAccessPanels()
	{
	}

	public override void Destroy()
	{
		if (Tooltip != null)
		{
			Tooltip.Destroy();
		}
		gui.HyperlinkClicked -= gui_HyperlinkClicked;
		base.Destroy();
	}

	public void CloseAllEntityTooltips()
	{
		for (int num = EntityTypeTooltipsStack.Count - 1; num >= 0; num--)
		{
			EntityTypeTooltipsStack[num].Hide();
		}
	}

	public DataSheet GetChildTooltip(DataSheet tooltip)
	{
		int num = EntityTypeTooltipsStack.IndexOf(tooltip);
		if (num > -1 && EntityTypeTooltipsStack.Count > num + 1)
		{
			return EntityTypeTooltipsStack[num + 1];
		}
		return null;
	}

	public DataSheet GetParentTooltip(DataSheet tooltip)
	{
		int num = EntityTypeTooltipsStack.IndexOf(tooltip);
		if (num > 0)
		{
			return EntityTypeTooltipsStack[num - 1];
		}
		return null;
	}

	private void EnableScenarioUI(ScenarioData scenario)
	{
		if (!scenario.EnableMissions)
		{
			RosterAccessPanel.DisableMissions();
		}
		if (!scenario.EnableGraphs)
		{
			RosterAccessPanel.DisableGraphs();
		}
		if (!scenario.EnablePersonell)
		{
			RosterAccessPanel.DisablePersonell();
		}
		if (!scenario.EnableWorldMap)
		{
			RosterAccessPanel.DisableWorldMap();
		}
		if (!scenario.EnableLedger)
		{
			RosterAccessPanel.DisableLedger();
		}
		if (!scenario.EnablePolicy)
		{
			RosterAccessPanel.DisablePolicy();
		}
	}

	private void SetupEventLogPanel()
	{
		int width = 459;
		int xPos = (int)Common.Clamp(0.3 * (double)The.MapUI.mapWindowWidth, OverlayPanel.DisplayWindow.Right + 115, 615.0);
		LogPanel = new LogPanel(xPos, width);
	}

	private void SetupCountersAndDateTimePanels()
	{
		int num = The.MapUI.mapWindowWidth - 445;
		int num2 = 300;
		int xPos;
		if (num < num2)
		{
			xPos = num;
		}
		else
		{
			num -= num2;
			xPos = (num2 + num) / 2;
		}
		dateAndWeather = new DateAndWeather(xPos);
	}

	public void SelectNextEntityOfType(string typeKey)
	{
		EntityID entityID = ((!SelectedEntity.HasValue) ? EntityID.Invalid : SelectedEntity.Value);
		if (entityID == EntityID.Invalid)
		{
			entityID = EntityID.First;
		}
		int num = (int)Entity.LastUsedID;
		for (EntityID entityID2 = entityID + 1; entityID2 != entityID; entityID2++)
		{
			if (entityID2 > Entity.LastUsedID)
			{
				entityID2 = EntityID.First;
			}
			Entity entity = Entity.FindByID(entityID2);
			if (entity != null && entity.IsOnPlaySite() && entity.EntityType.KeyName == typeKey)
			{
				SelectEntity(entity, centerInView: true);
				break;
			}
			if (num-- <= 0)
			{
				break;
			}
		}
	}

	public static bool CanSelectEntity(IKnownEntityData entityData)
	{
		if (entityData != null)
		{
			return Entity.IsOnPlaySite(entityData);
		}
		return false;
	}

	public void SelectEntity(EntityID entityID, bool centerInView = false)
	{
		The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID, out var data);
		if (data != null && CanSelectEntity(data))
		{
			SelectEntity(data, centerInView);
		}
		else
		{
			SelectEntity(null);
		}
	}

	public void SelectEntity(IKnownEntityData selectedEntity, bool centerInView = false)
	{
		if (selectedEntity != null)
		{
			SelectedEntity = selectedEntity.EntityID;
			Selection.SelectEntity(selectedEntity);
		}
		else
		{
			SelectedEntity = null;
		}
		if (centerInView && SelectedEntity.HasValue)
		{
			Point value = selectedEntity.MapPosition.Value;
			The.MapUI.ZoomToMapPosition(value.X, value.Y);
		}
	}

	public void SelectTile(TerrainTile clickedTile)
	{
		SelectedEntity = null;
		SelectedZone = null;
		SelectedTiles.Clear();
		SelectedTiles.Add(clickedTile);
		Rectangle value = new Rectangle(clickedTile.X, clickedTile.Y, 1, 1);
		SelectedTiles.StartDragTile = new Point(clickedTile.X, clickedTile.Y);
		SelectedTiles.BoundingRectangle = value;
	}

	public void Select()
	{
	}

	public void ShowInventoryPanel()
	{
		if (!IsDisplayed(InventoryPanel))
		{
			ChangeRosterPanel(InventoryPanel);
		}
	}

	public void ChangeRosterPanel(RosterPanel panel)
	{
		ChangeRosterPanel(panel, refreshCurrentPanelWithNewContent: false);
	}

	private bool IsDisplayed(RosterPanel rosterPanel)
	{
		return rosterPanel == displayedRosterPanel;
	}

	public void ChangeRosterPanel(RosterPanel panel, bool refreshCurrentPanelWithNewContent)
	{
		if (!refreshCurrentPanelWithNewContent && IsDisplayed(panel))
		{
			CloseRosterPanel();
			return;
		}
		if (refreshCurrentPanelWithNewContent)
		{
			IsDisplayed(panel);
		}
		else
			_ = 0;
		if (panel != displayedRosterPanel && displayedRosterPanel != null)
		{
			displayedRosterPanel.Hide();
		}
		if (!IsDisplayed(panel))
		{
			displayedRosterPanel = panel;
			displayedRosterPanel.Show();
		}
		else if (displayedRosterPanel != null)
		{
			displayedRosterPanel.Refresh();
		}
		if (displayedRosterPanel != null && displayedRosterPanel.HasStatusCRT)
		{
			if (!StatusScreen.DisplayWindow.IsVisibleAndActive)
			{
				StatusScreen.Show();
			}
			if (!StatusScreen.IsOn)
			{
				StatusScreen.TurnOn();
			}
			else
			{
				StatusScreen.Switch();
			}
		}
		else
		{
			HideStatusScreen();
		}
		if (displayedRosterPanel.AccessButton != null)
		{
			RosterAccessPanel.CheckAccessButton(displayedRosterPanel.AccessButton);
		}
	}

	public bool RosterIsDisplayed(RosterPanel roster)
	{
		return roster == displayedRosterPanel;
	}

	private void HideStatusScreen()
	{
		if (StatusScreen != null && StatusScreen.DisplayWindow.IsVisibleAndActive)
		{
			if (StatusScreen.IsOn)
			{
				StatusScreen.TurnOff();
			}
			StatusScreen.Hide();
		}
	}

	public void StocksExpandedPanel_DrawContentEvent(Window sender, SpriteBatch formSpriteBatch)
	{
	}

	public bool IsShowingMapEditor()
	{
		return displayedRosterPanel == SidePanelEditorEntity;
	}

	public bool IsMouseInsideInterface()
	{
		return false;
	}

	public void MoveHUDWindows()
	{
		foreach (HUDWindow hudWindow in hudWindows)
		{
			_ = hudWindow is EntityListWindow;
			if (hudWindow.DisplayWindow.Visible && hudWindow.WorldPosition.HasValue)
			{
				hudWindow.DisplayWindow.Position = The.MapUI.WorldPosToScreenPoint(hudWindow.WorldPosition.Value);
			}
		}
	}

	public void MoveFogMap()
	{
		if (FogMap != null)
		{
			FogMap.MoveToPosition(new Point(-(int)The.MapUI.MapWindowWorldPosition.X, -(int)The.MapUI.MapWindowWorldPosition.Y));
		}
	}

	private void UpdateUISimPerspective()
	{
		if (The.Sim.Mode == Sim.EngineMode.Game && (!UIOwner.HasValue || ownerRegulator.IsReady()))
		{
			Vector2 centerOfScreenWorldLocation = The.MapUI.GetCenterOfScreenWorldLocation();
			Expedition closestExpedition = The.Map.GetClosestExpedition(centerOfScreenWorldLocation.ToVector3());
			if (closestExpedition != null)
			{
				UIOwner = closestExpedition.OwnedEntities.ID;
				UIExpedition = closestExpedition.ID;
			}
			else
			{
				UIOwner = null;
				UIExpedition = null;
			}
		}
	}

	public virtual void SetWeatherNow(string cloudCover, string wind)
	{
		dateAndWeather.HUDPanel.SetWeatherNow(cloudCover, wind);
	}

	public virtual void SetTimeAndDate(string timeOfDayString, string season, string date)
	{
		dateAndWeather.HUDPanel.SetTime(timeOfDayString);
		dateAndWeather.HUDPanel.SetSeason(season);
		dateAndWeather.HUDPanel.SetDate(date);
	}

	public override void Update(GameTime gameTime)
	{
		_ = The.Client.IsExiting;
		base.Update(gameTime);
		_ = The.Client.IsExiting;
		if (The.Sim == null)
		{
			return;
		}
		UpdateUISimPerspective();
		string text = The.Client.Controller.GetReplayTime();
		if (text != null)
		{
			replayTime.Text = text;
		}
		replayTime.Update(gameTime);
		if (FogMap != null)
		{
			FogMap.Update(gameTime);
		}
		Selection.Update(gameTime);
		StatusScreen.Update(gameTime);
		if (TalkPanel != null)
		{
			TalkPanel.Update(gameTime);
		}
		if ((InventoryPanel != null && IsDisplayed(InventoryPanel)) || (ContextMenu != null && ContextMenu.ZoneGatherResourcesWindow.DisplayWindow.IsVisibleAndActive) || (HUDActionPanel != null && HUDActionPanel.UpgradeWindow.DisplayWindow.IsVisibleAndActive) || EntityTypeTooltipsStack.Count > 0)
		{
			InventorySettings.Update(gameTime);
		}
		Minimap.Update(gameTime);
		SelectedCyclePlayer.Update(gameTime);
		SelectedCyclePlayerFlashing.Update(gameTime);
		Vector2 vector = new Vector2(screenBoundsExtension, screenBoundsExtension);
		CollideShape2D screenBounds = new CollideShape2D(The.MapUI.MapWindowWorldPosition - vector, The.MapUI.MapWindowWorldPosition + new Vector2(The.MapUI.mapWindowWidth, The.MapUI.mapWindowHeight) + vector);
		UpdateSpokenLines(screenBounds);
		UpdateMarkerWindows(gameTime, screenBounds);
		if (SaveLoadMessageBox.Window.IsVisibleAndActive)
		{
			SaveLoadMessageBox.Update(gameTime);
		}
		if (!The.Sim.IsPaused)
		{
			if (displayedRosterPanel != null)
			{
				displayedRosterPanel.Update(gameTime);
			}
			int num = 0;
			foreach (HUDWindow hudWindow in hudWindows)
			{
				if (hudWindow.DisplayWindow.IsVisibleAndActive)
				{
					num++;
					hudWindow.UpdateContent(gameTime);
				}
			}
			if (Game.Controller.GraphicsLevelSetting == Controller.GraphicsLevel.High)
			{
				DisplayPanelRenderer.Update(gameTime);
			}
			if (LogPanel != null)
			{
				LogPanel.Update(gameTime);
			}
			if (RatingsPanel != null)
			{
				RatingsPanel.Update(gameTime);
			}
		}
		if (CounterPanel != null)
		{
			CounterPanel.Update(gameTime);
		}
		for (int num2 = hudWindows.Count - 1; num2 >= 0; num2--)
		{
			HUDWindow hUDWindow = hudWindows[num2];
			if (hUDWindow.DisplayWindow.IsVisibleAndActive || hUDWindow.UpdateWhileHidden)
			{
				hUDWindow.Update(gameTime);
			}
		}
		if (InterfaceMode == InterfaceState.Build || InterfaceMode == InterfaceState.EditorPlaceEntity)
		{
			UpdateEntitiesBeingPlacedPositions();
		}
	}

	public void UpdateEntitiesBeingPlacedPositions()
	{
		foreach (EntityPosition item in EntitiesBeingPlaced)
		{
			Vector3 mouseWorldLocation = The.MapUI.MouseWorldLocation;
			mouseWorldLocation += item.Position.ToVector3();
			mouseWorldLocation = The.Map.ClampWorldPosition(mouseWorldLocation);
			if (item.Entity.EntityType.StructureType != null)
			{
				item.Entity.SetPosition(mouseWorldLocation);
				if (!item.Entity.Structure.IsPlacementValid(mouseWorldLocation))
				{
					item.Entity.Renderable.SetOverlayGradientColors(Color.DarkRed, Color.OrangeRed, Color.White);
				}
				else
				{
					item.Entity.Renderable.SetOverlayGradientColors(Color.DeepSkyBlue, Color.Blue, Color.White);
				}
			}
			else
			{
				item.Entity.SetPosition(mouseWorldLocation);
			}
		}
	}

	public void DrawText(string text, Vector2 pos, Color color)
	{
		The.Client.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
		The.Client.spriteBatch.DrawString(The.InGameUI.InterfaceFont, text, pos, color);
		The.Client.spriteBatch.End();
	}

	public void DrawPauseIcon(bool isPaused)
	{
		if (isPaused && The.Sim.Mode != Sim.EngineMode.Edit)
		{
			MainPanel.DrawPauseIcon();
		}
	}

	public void ShowSelectedEntityPanel(bool refreshCurrentPanel = true)
	{
		ChangeRosterPanel(SidePanelEntity, refreshCurrentPanel);
	}

	public void ShowSelectedMapAreaPanel(bool refreshCurrentPanel = true)
	{
		ChangeRosterPanel(SidePanelMapArea, refreshCurrentPanel);
	}
}
