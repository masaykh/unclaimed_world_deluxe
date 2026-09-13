using System;
using System.Collections.Generic;
using InputEventSystem;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Resources;
using WindowSystem;

namespace UWGame.ClientSide.Interface.HUD_Windows;

public class MarkerWindow : HUDWindow
{
	public enum MarkerType
	{
		Zone,
		Expedition,
		Entity,
		None
	}

	private static int WindowHeight = 24;

	private Label markerLabel;

	private ImageButton buttonExpand;

	private Point? screenPosition;

	private bool showWhileLineIsSpoken;

	private Vector2 markerWorldPosition;

	private MarkerType currentType = MarkerType.None;

	private UIComponent zonePanel;

	private UIComponent entityPanel;

	private Image imGather;

	private Image imStockpile;

	private Image imScout;

	private Image imPatrol;

	private Image imHunt;

	private Image imForage;

	private Image imAttack;

	public Zone Zone;

	private const int iconSpacing = 2;

	private EntityID? entityID;

	private HorizontalList entityHorizontalList;

	private EntityActivityHUDWindow activityWindow;

	private const string personBackgroundSprite = "HUD_windowCharacter_base_small";

	private const string otherAllegianceBackgroundSprite = "HUD_windowRed_base_small";

	private const string specialSiteBackgroundSprite = "HUD_windowOrange_base_small";

	private const string defaultBackgroundSprite = "HUD_window_base_small";

	private Point offsetFromEntity = new Point(0, 90);

	public bool IsRenewedThisFrame;

	public Expedition Expedition;

	public bool ShowWhileLineIsSpoken
	{
		get
		{
			return showWhileLineIsSpoken;
		}
		set
		{
			showWhileLineIsSpoken = value;
			if (showWhileLineIsSpoken)
			{
				ShowWindow();
			}
			else if (!DisplayWindow.IsVisibleAndActive)
			{
				HideWindow();
			}
		}
	}

	public Vector2 MarkerWorldPosition => markerWorldPosition;

	public EntityID? EntityID
	{
		set
		{
			if (entityID != value)
			{
				entityID = value;
				activityWindow.owner = value;
				Refresh();
			}
		}
	}

	public MarkerWindow()
		: base(100, WindowHeight)
	{
		markerLabel = new Label(gui);
		Add(markerLabel);
		buttonExpand = new ImageButton(gui);
		buttonExpand.Init(ImageButtonType.HUDArrowRight);
		buttonExpand.Click += expandButton_Click;
		buttonExpand.X = DisplayWindow.Width - buttonExpand.Width - 6;
		buttonExpand.ToolTip = "Click to see the actions that can be taken";
		buttonExpand.ZOrder = 1f;
		DisplayWindow.CenterChildVertically(buttonExpand);
		DisplayWindow.ViewPort.MouseOver += ViewPort_MouseOver;
		DisplayWindow.ViewPort.MouseOut += ViewPort_MouseOut;
	}

	private void ViewPort_MouseOut(UIComponent sender, MouseEventArgs args)
	{
		The.InGameUI.SetHoverEntity(null);
	}

	private void ViewPort_MouseOver(UIComponent sender, MouseEventArgs args)
	{
		if (entityID.HasValue)
		{
			The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID.Value, out var data);
			if (data != null)
			{
				The.InGameUI.SetHoverEntity(data.EntityID);
			}
		}
	}

	public void Reset()
	{
		ChangeTypeToNone();
		showWhileLineIsSpoken = false;
		EntityID = null;
		Zone = null;
		Expedition = null;
		WorldPosition = null;
	}

	public void ChangeTypeToNone()
	{
		ChangeType(MarkerType.None);
	}

	public void FillWithZoneInfo(Zone zone)
	{
		ChangeType(MarkerType.Zone);
		FillFromZone(zone);
		ChangeBackground();
		Add(zonePanel);
		Refresh();
	}

	public void FillWithEntityInfo(EntityID entityID)
	{
		ChangeType(MarkerType.Entity);
		FillFromEntity(entityID);
		ChangeBackground();
		Add(entityPanel);
		Refresh();
	}

	public bool EntityIsValid()
	{
		The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID.Value, out var data);
		if (data != null)
		{
			if (GoalEvaluator.IsOnPlaySite(data))
			{
				return true;
			}
			return false;
		}
		return false;
	}

	private void ChangeBackground()
	{
		string surfaceSpriteName = "HUD_window_base_small";
		switch (currentType)
		{
		case MarkerType.Entity:
		{
			The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID.Value, out var data);
			if (data != null)
			{
				surfaceSpriteName = ((data.AllegianceID == The.InGameUI.UIAllegiance.ID) ? "HUD_windowCharacter_base_small" : ((data.EntityType.IntelligenceType == null && data.EntityType.ThreatType == null) ? ((data.EntityType.TerrainType == null) ? "HUD_window_base_small" : "HUD_windowOrange_base_small") : "HUD_windowRed_base_small"));
			}
			break;
		}
		case MarkerType.Zone:
			surfaceSpriteName = "HUD_window_base_small";
			break;
		case MarkerType.Expedition:
			surfaceSpriteName = "HUD_window_base_small";
			break;
		}
		ChangeSurface(surfaceSpriteName);
	}

	public void FillWithExpeditionInfo(Expedition expedition)
	{
		ChangeType(MarkerType.Expedition);
		FillFromExpedition(expedition);
		ChangeBackground();
		Refresh();
	}

	private void ChangeType(MarkerType typeToChangeTo)
	{
		if (currentType == MarkerType.Entity)
		{
			Remove(entityPanel);
			Remove(buttonExpand);
			if (The.InGameUI.HUDActionPanel.IsShowingEntity(entityID.Value))
			{
				The.InGameUI.HUDActionPanel.Hide();
			}
			activityWindow.Hide();
		}
		else if (currentType == MarkerType.Expedition)
		{
			Remove(buttonExpand);
		}
		else if (currentType == MarkerType.Zone)
		{
			Remove(zonePanel);
			Remove(buttonExpand);
		}
		currentType = typeToChangeTo;
	}

	private void FillFromZone(Zone zone)
	{
		Zone = zone;
		Zone.ZoneOrdersChanged += Zone_ZoneOrdersChanged;
		if (markerLabel.Text != "")
		{
			markerLabel.Text = "";
		}
		TerrainTile bottomLeftTile = Zone.MapArea.BottomLeftTile;
		markerWorldPosition = MapManager.TilePosToWorldPos(bottomLeftTile.TilePos).ToVector2();
		markerWorldPosition.X -= 24f;
		markerWorldPosition.Y += 24f;
		Vector2 vector = The.MapUI.TilePosToScreen(new Point(bottomLeftTile.X, bottomLeftTile.Y));
		vector.X -= 24f;
		vector.Y += 24f;
		screenPosition = vector.ToPoint();
		if (zonePanel == null)
		{
			zonePanel = new UIComponent(gui);
			zonePanel.DebugTag = "zonePanel";
			imGather = new Image(gui);
			imGather.Texture = gui.GUISpriteSheet.Texture;
			Rectangle sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_gather");
			imGather.SetSkinLocation(SkinState.Normal, sourceRectangle);
			imGather.ResizeControlToFitImage();
			DisplayWindow.CenterChildVertically(imGather);
			imStockpile = new Image(gui);
			imStockpile.Texture = gui.GUISpriteSheet.Texture;
			sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_stockpile");
			imStockpile.SetSkinLocation(SkinState.Normal, sourceRectangle);
			imStockpile.ResizeControlToFitImage();
			DisplayWindow.CenterChildVertically(imStockpile);
			imHunt = new Image(gui);
			imHunt.Texture = gui.GUISpriteSheet.Texture;
			sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_hunt");
			imHunt.SetSkinLocation(SkinState.Normal, sourceRectangle);
			imHunt.ResizeControlToFitImage();
			DisplayWindow.CenterChildVertically(imHunt);
			imScout = new Image(gui);
			imScout.Texture = gui.GUISpriteSheet.Texture;
			sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_scout");
			imScout.SetSkinLocation(SkinState.Normal, sourceRectangle);
			imScout.ResizeControlToFitImage();
			DisplayWindow.CenterChildVertically(imScout);
			imForage = new Image(gui);
			imForage.Texture = gui.GUISpriteSheet.Texture;
			sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_forage");
			imForage.SetSkinLocation(SkinState.Normal, sourceRectangle);
			imForage.ResizeControlToFitImage();
			DisplayWindow.CenterChildVertically(imForage);
			imPatrol = new Image(gui);
			imPatrol.Texture = gui.GUISpriteSheet.Texture;
			sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_patrol");
			imPatrol.SetSkinLocation(SkinState.Normal, sourceRectangle);
			imPatrol.ResizeControlToFitImage();
			DisplayWindow.CenterChildVertically(imPatrol);
			imAttack = new Image(gui);
			imAttack.Texture = gui.GUISpriteSheet.Texture;
			sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_sword");
			imAttack.SetSkinLocation(SkinState.Normal, sourceRectangle);
			imAttack.ResizeControlToFitImage();
			DisplayWindow.CenterChildVertically(imAttack);
		}
		Refresh();
	}

	private void FillFromEntity(EntityID entityID)
	{
		if (entityPanel == null)
		{
			entityPanel = new UIComponent(gui);
			entityHorizontalList = new HorizontalList(The.InGameUI.gui, 2);
			entityHorizontalList.CenterItemsVertically = true;
			entityHorizontalList.HorizontalSpacing = -2;
			entityHorizontalList.Y = 0;
			entityPanel.Add(entityHorizontalList);
			entityPanel.Height = WindowHeight;
			entityHorizontalList.Height = WindowHeight;
			entityHorizontalList.MinHeight = WindowHeight;
			entityHorizontalList.MaxHeight = WindowHeight;
			entityHorizontalList.DebugTag = "statusIcons";
			activityWindow = new EntityActivityHUDWindow(new Point(0, 20), 3);
		}
		EntityID = entityID;
	}

	private void FillFromExpedition(Expedition expedition)
	{
		Expedition = expedition;
		markerWorldPosition = Expedition.Location.Value.ToVector2();
		screenPosition = The.MapUI.WorldPosToScreenPoint(markerWorldPosition);
		int x = 6;
		Add(markerLabel);
		markerLabel.Init(Label.LabelType.HUDWindow);
		markerLabel.X = x;
		markerLabel.Text = expedition.Name;
		markerLabel.FitToText();
		Add(buttonExpand);
		buttonExpand.X = markerLabel.Right;
		DisplayWindow.Width = markerLabel.Width + 12 + buttonExpand.Width;
		DisplayWindow.CenterChildVertically(markerLabel);
	}

	private void Zone_ZoneOrdersChanged(object sender, EventArgs e)
	{
		Refresh();
	}

	public void Show()
	{
		ShowWindow();
	}

	private void ShowWindow()
	{
		switch (currentType)
		{
		case MarkerType.Entity:
		{
			DisplayWindow.ViewPort.Click += OnMarkerClick;
			UpdatePosition(out var _);
			activityWindow.Show();
			break;
		}
		case MarkerType.Expedition:
			markerWorldPosition = Expedition.Location.Value.ToVector2();
			screenPosition = The.MapUI.WorldPosToScreenPoint(markerWorldPosition);
			break;
		case MarkerType.Zone:
		{
			TerrainTile bottomLeftTile = Zone.MapArea.BottomLeftTile;
			Vector2 vector = The.MapUI.TilePosToScreen(new Point(bottomLeftTile.X, bottomLeftTile.Y));
			vector.X -= 24f;
			vector.Y += 24f;
			screenPosition = vector.ToPoint();
			break;
		}
		}
		base.ShowOnPlayfield(screenPosition.Value.X, screenPosition.Value.Y);
	}

	private void UpdatePosition(out IKnownEntityData entityData)
	{
		if (entityID.HasValue)
		{
			The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID.Value, out entityData);
			if (entityData != null)
			{
				markerWorldPosition = entityData.RenderedLocation.ToVector2();
				markerWorldPosition.X -= offsetFromEntity.X;
				markerWorldPosition.Y -= offsetFromEntity.Y;
				Point value = The.MapUI.WorldPosToScreenPoint(markerWorldPosition);
				screenPosition = value;
			}
		}
		else
		{
			entityData = null;
		}
	}

	public override void Refresh()
	{
		if (currentType == MarkerType.Zone)
		{
			RefreshZoneMarker();
		}
		else if (currentType == MarkerType.Entity)
		{
			RefreshEntityMarker();
		}
	}

	private void RefreshEntityMarker()
	{
		activityWindow.Refresh();
		entityPanel.X = 6;
		entityPanel.Y = 6;
		markerLabel.X = 0;
		markerLabel.Y = 0;
		The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID.Value, out var data);
		if (data != null)
		{
			RefreshIconList(data);
			entityPanel.Width = entityHorizontalList.Width;
			DisplayWindow.CenterChildVertically(entityPanel);
			RefreshLabelWithEntity(data);
			if (HUDEntityContextMenu.EntityHasActions(out var _, out var _, out var _, out var _, out var _, out var _, out var _, out var _, out var _, entityID))
			{
				AddExpandButtonForEntityMarker();
			}
			else
			{
				Remove(buttonExpand);
				DisplayWindow.Width = markerLabel.Right + 6;
			}
			offsetFromEntity.X = DisplayWindow.Width / 2;
		}
		else
		{
			Remove(buttonExpand);
			DisplayWindow.ViewPort.Click -= OnMarkerClick;
		}
	}

	private void AddExpandButtonForEntityMarker()
	{
		Add(buttonExpand);
		DisplayWindow.CenterChildVertically(buttonExpand);
		buttonExpand.X = markerLabel.X + markerLabel.TextWidth;
		DisplayWindow.Width = buttonExpand.Right + 6;
	}

	public void RefreshIconList(IKnownEntityData entityWithStatus)
	{
		IHasExposedProperties hasExposedProperties = null;
		hasExposedProperties = entityWithStatus;
		foreach (PresentationTypeCategory finalPresentationTypeCategory in GameData.Instance.CustomStatusIconData.FinalPresentationTypeCategories)
		{
			int? numberOfItems = null;
			PresentationTypeCategoryProcessor.DisplayCategory(finalPresentationTypeCategory, hasExposedProperties, entityHorizontalList, ref numberOfItems);
		}
	}

	public static bool HasStatusIconsToShow(IHasExposedProperties hasExposedProperties)
	{
		foreach (PresentationTypeCategory finalPresentationTypeCategory in GameData.Instance.CustomStatusIconData.FinalPresentationTypeCategories)
		{
			if (PresentationTypeCategoryProcessor.KeyedEntryComponentHasDataToShow(finalPresentationTypeCategory, hasExposedProperties, HorizontalList.CanProcessEntryDataStatic))
			{
				return true;
			}
		}
		return false;
	}

	public void RefreshLabelWithEntity(IKnownEntityData entityWithStatus)
	{
		markerLabel.X += entityPanel.Position.X + entityPanel.Width;
		if (The.InGameUI.SelectedEntity.HasValue)
		{
			if (The.InGameUI.SelectedEntity.Value == entityWithStatus.EntityID)
			{
				markerLabel.Init(Label.LabelType.HUDWindowHeader);
			}
			else
			{
				markerLabel.Init(Label.LabelType.HUDWindow);
			}
		}
		else
		{
			markerLabel.Init(Label.LabelType.HUDWindow);
		}
		string text = entityWithStatus.GetDisplayName();
		if (entityWithStatus.EntityType.IntelligenceType != null && entityWithStatus is Entity entity && entity.Intelligence.LastName != null)
		{
			text = entity.Intelligence.LastName;
		}
		markerLabel.Text = text;
		markerLabel.FitToText();
		DisplayWindow.CenterChildVertically(markerLabel);
	}

	private void OnMarkerClick(UIComponent sender, EventArgs e)
	{
		if (currentType == MarkerType.Entity)
		{
			The.InGameUI.SelectEntity(entityID.Value);
		}
		DisplayWindow.BringToTop();
	}

	public void RefreshZoneMarker()
	{
		int x = 12;
		zonePanel.X = x;
		x = 0;
		zonePanel.Remove(imStockpile);
		zonePanel.Remove(imGather);
		zonePanel.Remove(imHunt);
		zonePanel.Remove(imScout);
		zonePanel.Remove(imForage);
		zonePanel.Remove(imPatrol);
		zonePanel.Remove(imAttack);
		zonePanel.Width = 0;
		if (Zone.Stockpile != null)
		{
			zonePanel.Add(imStockpile);
			imStockpile.X = x;
			x = imStockpile.Right + 2;
			zonePanel.Width += imStockpile.Width + 2;
		}
		if (Zone.HasHarvestJobs() || Zone.AllowStandingOrderHarvest.Count > 0)
		{
			zonePanel.Add(imGather);
			if (Zone.HasHarvestJobs())
			{
				SetJobIconColor(Zone.HarvestJobs, imGather);
			}
			imGather.X = x;
			x = imGather.Right + 2;
			zonePanel.Width += imGather.Width + 2;
		}
		if (Zone.ZoneHunt.HasFindPreyJobs() || Zone.ZoneHunt.HasHuntOrders())
		{
			zonePanel.Add(imHunt);
			SetJobIconColor(Zone.ZoneHunt.FindPreyJobs, imHunt);
			imHunt.X = x;
			x = imHunt.Right + 2;
			zonePanel.Width += imHunt.Width + 2;
		}
		if (Zone.PatrolJob != null)
		{
			zonePanel.Add(imPatrol);
			SetJobIconColor(Zone.PatrolJob.ID, imPatrol);
			imPatrol.X = x;
			x = imPatrol.Right + 2;
			zonePanel.Width += imPatrol.Width + 2;
		}
		if (Zone.AttackAreaJob != null)
		{
			zonePanel.Add(imAttack);
			SetJobIconColor(Zone.AttackAreaJob.ID, imAttack);
			imAttack.X = x;
			x = imAttack.Right + 2;
			zonePanel.Width += imAttack.Width + 2;
		}
		if (Zone.ExamineJob != null)
		{
			zonePanel.Add(imForage);
			SetJobIconColor(Zone.ExamineJob.ID, imForage);
			imForage.X = x;
			x = imForage.Right + 2;
			zonePanel.Width += imForage.Width + 2;
		}
		if (Zone.ScoutingJob != null)
		{
			zonePanel.Add(imScout);
			SetJobIconColor(Zone.ScoutingJob.ID, imScout);
			imScout.X = x;
			x = imScout.Right + 2;
			zonePanel.Width += imScout.Width + 2;
		}
		zonePanel.Height = WindowHeight;
		x = zonePanel.Right + 2;
		buttonExpand.X = x;
		Add(buttonExpand);
		DisplayWindow.Width = buttonExpand.Right + 1;
	}

	private void SetJobIconColor(Dictionary<ResourceType, List<ProcessJob>> jobs, Image image)
	{
		bool aJobWasBlockedByThreat = false;
		bool aJobWasBlockedByStance = false;
		bool jobIsInaccessible = false;
		foreach (KeyValuePair<ResourceType, List<ProcessJob>> job in jobs)
		{
			foreach (ProcessJob item in job.Value)
			{
				if (!GetJobStatus(item, ref aJobWasBlockedByThreat, ref aJobWasBlockedByStance))
				{
					image.ToolTip = null;
					image.Color = Color.White;
					return;
				}
			}
		}
		SetJobIconColor(jobIsInaccessible, aJobWasBlockedByStance, aJobWasBlockedByThreat, image);
	}

	private void SetJobIconColor(List<FindPreyJob> jobs, Image image)
	{
		if (jobs != null)
		{
			bool aJobWasBlockedByThreat = false;
			bool aJobWasBlockedByStance = false;
			bool jobIsInaccessible = false;
			foreach (FindPreyJob job in jobs)
			{
				if (!GetJobStatus(job, ref aJobWasBlockedByThreat, ref aJobWasBlockedByStance))
				{
					image.ToolTip = null;
					image.Color = Color.White;
					return;
				}
			}
			SetJobIconColor(jobIsInaccessible, aJobWasBlockedByStance, aJobWasBlockedByThreat, image);
		}
		else
		{
			image.ToolTip = null;
			image.Color = Color.White;
		}
	}

	private bool GetJobStatus(Job job, ref bool aJobWasBlockedByThreat, ref bool aJobWasBlockedByStance)
	{
		bool isInAccessible = false;
		bool isBlockedByThreat = false;
		bool isBlockedDueToBoldStanceRequired = false;
		bool tooFarFromExpedition = false;
		bool huntingNotFeasible = false;
		bool areaNotCleared = false;
		The.Client.GetFeedback(job.ID, out isInAccessible, out isBlockedByThreat, out isBlockedDueToBoldStanceRequired, out tooFarFromExpedition, out huntingNotFeasible, out areaNotCleared);
		if (!isInAccessible && !isBlockedByThreat && !isBlockedDueToBoldStanceRequired)
		{
			return false;
		}
		if (isBlockedDueToBoldStanceRequired)
		{
			aJobWasBlockedByStance = true;
		}
		if (isBlockedByThreat)
		{
			aJobWasBlockedByThreat = true;
		}
		return true;
	}

	private void SetJobIconColor(bool jobIsInaccessible, bool blockedByThreat, bool blockedByStance, Image image)
	{
		if (jobIsInaccessible || blockedByStance || blockedByThreat)
		{
			if (blockedByThreat)
			{
				image.ToolTip = "Dangerous area. To enter the area, a person must have Fearless stance. (Use a PATROL zone to clear the area of threats.)";
			}
			else if (jobIsInaccessible)
			{
				image.ToolTip = "Task cannot be completed because area is inaccessible - due to terrain or obstacles blocking the way";
			}
			else
			{
				image.ToolTip = "No camp members can do this dangerous task (Required stance: Fearless)";
			}
			image.Color = Color.Red;
		}
		else
		{
			image.ToolTip = null;
			image.Color = Color.White;
		}
	}

	private void SetJobIconColor(JobID jobID, Image image)
	{
		The.Client.GetFeedback(jobID, out var isInAccessible, out var isBlockedByThreat, out var isBlockedDueToBoldStanceRequired, out var _, out var _, out var _);
		SetJobIconColor(isInAccessible, isBlockedByThreat, isBlockedDueToBoldStanceRequired, image);
	}

	private void expandButton_Click(UIComponent sender, EventArgs e)
	{
		if (currentType == MarkerType.Zone)
		{
			The.InGameUI.SelectedZone = Zone;
			The.InGameUI.ShowContextMenu(DisplayWindow.X, DisplayWindow.Y, DisplayWindow);
			The.InGameUI.ShowSelectedMapAreaPanel();
		}
		else if (currentType == MarkerType.Entity)
		{
			if (The.InGameUI.HUDActionPanel.IsShowingEntity(entityID.Value))
			{
				The.InGameUI.HUDActionPanel.Hide();
			}
			else
			{
				The.InGameUI.HUDActionPanel.ShowOnPlayfield(DisplayWindow.AbsolutePosition.X + DisplayWindow.Width - 4, DisplayWindow.AbsolutePosition.Y, modal: false, entityID.Value);
			}
		}
		else if (currentType == MarkerType.Expedition)
		{
			if (The.InGameUI.HUDActionPanel.IsShowingExpedition(Expedition))
			{
				The.InGameUI.HUDActionPanel.Hide();
			}
			else
			{
				The.InGameUI.HUDActionPanel.ShowOnPlayfield(DisplayWindow.AbsolutePosition.X + DisplayWindow.Width - 4, DisplayWindow.AbsolutePosition.Y, modal: false, Expedition);
			}
		}
	}

	private void bt_MouseOver(UIComponent sender, MouseEventArgs args)
	{
	}

	public void Update()
	{
		if ((DisplayWindow.IsVisibleAndActive || showWhileLineIsSpoken) && currentType == MarkerType.Entity)
		{
			UpdateEntityMarkerPosition();
		}
	}

	private void UpdateEntityMarkerPosition()
	{
		if (entityID.HasValue)
		{
			UpdatePosition(out var entityData);
			if (entityData != null)
			{
				SetScreenPosition(new Point(screenPosition.Value.X, screenPosition.Value.Y));
				SetWorldPosition(new Point(screenPosition.Value.X, screenPosition.Value.Y));
				activityWindow.SetPosition(screenPosition.Value);
				activityWindow.Update();
			}
		}
	}

	public override void Hide()
	{
		HideWindow();
	}

	private void HideWindow()
	{
		DisplayWindow.ViewPort.Click -= OnMarkerClick;
		if (!showWhileLineIsSpoken)
		{
			base.Hide();
		}
	}

	public MarkerType GetCurrentType()
	{
		return currentType;
	}
}
