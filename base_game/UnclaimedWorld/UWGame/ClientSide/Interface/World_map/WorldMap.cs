using System;
using System.Collections.Generic;
using InputEventSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RoundLineCode;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Communication;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Overland.Locations;
using UWGame.SimSide.Overland.Missions;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.Snapshots;
using WindowSystem;

namespace UWGame.ClientSide.Interface.World_map;

public class WorldMap : UIComponent
{
	private enum WorldMapLayers
	{
		Map,
		Grid,
		SiteMarkers,
		MissionMarkers
	}

	private Box border;

	private Box ruler;

	private World world;

	private EntityGroupID? otherPartyID;

	private double longitudeStart;

	private double longitudeEnd;

	private double latitudeStart;

	private double latitudeEnd;

	private SiteWindow siteWindow;

	private bool gridHasChanged = true;

	private float yAxisSpacing;

	private float xAxisSpacing;

	private float minY;

	private float minX;

	private float maxY;

	private float maxX;

	private float tickStartY;

	private float tickStartX;

	private float tickEndY;

	private float tickEndX;

	private float xInterval;

	private float yInterval;

	public Color AxisColor = Color.SlateGray;

	private Dictionary<SiteID, SiteMarker> sitesOnMap = new Dictionary<SiteID, SiteMarker>();

	private Dictionary<JobID, JobID> missionsOnMap = new Dictionary<JobID, JobID>();

	private int canvasWidth;

	private int canvasHeight;

	private const int canvasMarginLeft = 3;

	private const int canvasMarginRight = 7;

	private const int canvasMarginTop = 11;

	private const int canvasMarginBottom = 12;

	private Dictionary<WorldMapLayers, UIComponent> layersToDraw = new Dictionary<WorldMapLayers, UIComponent>();

	private Image imCanvas;

	private RenderTarget2D renderTarget;

	private PrimitiveBatch primitiveBatch;

	private RoundLineManager RoundLineManager;

	private Matrix viewProj;

	private List<RoundLine> roundLines = new List<RoundLine>();

	private Label.LabelType axisType = Label.LabelType.LCDNormalDark;

	private Dictionary<MissionID, Image> missionMarkers = new Dictionary<MissionID, Image>();

	private Color missionColor = Color.Orange;

	private List<Bar> gridLines = new List<Bar>();

	private List<Label> gridLineLabels = new List<Label>();

	public event Action<TravelLocation> TerminalSelected
	{
		add
		{
			siteWindow.TerminalSelected += value;
		}
		remove
		{
			siteWindow.TerminalSelected -= value;
		}
	}

	public event Action ChildDialogDisplayed;

	public event Action ChildDialogClosed;

	public WorldMap(UIComponent surface, int canvasWidth, int canvasHeight)
		: base(surface.guiManager)
	{
		surface.Add(this);
		int num = Common.Min(canvasWidth, surface.Width - 3 - 7);
		int num2 = Common.Min(canvasHeight, surface.Height - 11 - 12);
		DebugTag = "worldmap";
		RenderType = RenderType.CRTAndLCD;
		this.canvasWidth = num;
		this.canvasHeight = num2;
		Width = canvasWidth + 3 + 7;
		Height = canvasHeight + 11 + 12;
		renderTarget = new RenderTarget2D(guiManager.Game.GraphicsDevice, canvasWidth, canvasHeight);
		primitiveBatch = new PrimitiveBatch(guiManager.ScreenWidth, guiManager.ScreenHeight, guiManager.Game.GraphicsDevice, canvasWidth, canvasHeight);
		viewProj = primitiveBatch.Projection;
		border = new Box(guiManager);
		Add(border);
		border.CornerSize = 20;
		border.SetSkinLocation(SkinState.Normal, guiManager.GUISpriteSheet.GetSourceRectangle("lcd_panel_background"));
		border.X = 1;
		border.Y = 9;
		ruler = new Box(guiManager);
		ruler.CornerSize = 4;
		ruler.SetSkinLocation(SkinState.Normal, guiManager.GUISpriteSheet.GetSourceRectangle("lcd_panel_background"));
		ruler.X = 1;
		ruler.Y = 9;
		imCanvas = new Image(guiManager);
		Add(imCanvas);
		imCanvas.Width = canvasWidth;
		imCanvas.Height = canvasHeight;
		imCanvas.X = 3;
		imCanvas.Y = 11;
		imCanvas.DebugTag = "canvas";
		siteWindow = new SiteWindow(guiManager, 410, 150);
		siteWindow.ChildDialogDisplayed += siteWindow_ChildWindowDisplayed;
		siteWindow.ChildDialogClosed += siteWindow_ChildWindowClosed;
		Add(siteWindow);
		siteWindow.Hide();
	}

	public override void Destroy()
	{
		base.Destroy();
		renderTarget.Dispose();
	}

	private void siteWindow_ChildWindowClosed()
	{
		if (this.ChildDialogClosed != null)
		{
			this.ChildDialogClosed();
		}
	}

	private void siteWindow_ChildWindowDisplayed()
	{
		if (this.ChildDialogDisplayed != null)
		{
			this.ChildDialogDisplayed();
		}
	}

	public void Fill(World world, EntityGroupID? otherPartyID, bool isMissionAction)
	{
		latitudeStart = world.ViewLatitudeStart;
		latitudeEnd = world.ViewLatitudeEnd;
		longitudeStart = world.ViewLongitudeStart;
		longitudeEnd = world.ViewLongitudeEnd;
		this.otherPartyID = otherPartyID;
		if (this.world != world)
		{
			this.world = world;
		}
		siteWindow.Hide();
		SetRanges();
		DrawMap();
		DrawGridAndAxis();
		DrawBorder();
		UpdateChangingData();
		UpdateCanvas();
	}

	public void HideOpenDialogs()
	{
		siteWindow.ResetAfterBuySellDialog();
		siteWindow.ResetAfterPersonnelDialog();
		The.InGameUI.BuySellDialog.Hide();
		The.InGameUI.PersonnelDialog.Hide();
	}

	private void UpdateCanvas()
	{
		foreach (KeyValuePair<WorldMapLayers, UIComponent> item in layersToDraw)
		{
			int layerIndex = getLayerIndex(item.Key);
			if (layerIndex > imCanvas.Controls.Count - 1)
			{
				imCanvas.Add(item.Value);
			}
			else if (imCanvas.Controls[layerIndex] != item.Value)
			{
				imCanvas.Controls[layerIndex] = item.Value;
			}
		}
	}

	private int getLayerIndex(WorldMapLayers drawingLayers)
	{
		return drawingLayers switch
		{
			WorldMapLayers.Map => 0, 
			WorldMapLayers.Grid => 1, 
			WorldMapLayers.SiteMarkers => 2, 
			WorldMapLayers.MissionMarkers => 3, 
			_ => 0, 
		};
	}

	private void UpdateChangingData()
	{
		UpdateSites();
		UpdateMissions();
	}

	public void Update()
	{
		UpdateChangingData();
		UpdateCanvas();
		if (siteWindow.Visible)
		{
			siteWindow.Refresh();
		}
		if (The.InGameUI.PersonnelDialog.Window.IsVisibleAndActive)
		{
			The.InGameUI.PersonnelDialog.Refresh();
		}
		if (The.InGameUI.BuySellDialog.Window.IsVisibleAndActive)
		{
			The.InGameUI.BuySellDialog.Refresh();
		}
	}

	private void UpdateMissions()
	{
		foreach (KeyValuePair<string, Site> allSite in The.Sim.World.AllSites)
		{
			foreach (Allegiance allegiance in allSite.Value.Allegiances)
			{
				if (allegiance.Missions == null)
				{
					continue;
				}
				foreach (Mission mission in allegiance.Missions)
				{
					if (!missionMarkers.TryGetValue(mission.ID, out var value))
					{
						value = AddMissionMarker(mission);
					}
					UpdateMissionMarker(mission, value);
				}
			}
		}
		DeleteMissionMarkers();
	}

	private void DeleteMissionMarkers()
	{
		List<MissionID> list = null;
		foreach (KeyValuePair<MissionID, Image> missionMarker in missionMarkers)
		{
			if (LookUp<Mission, MissionID>.FindByID(missionMarker.Key) == null)
			{
				Common.AddToList(ref list, missionMarker.Key);
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (MissionID item in list)
		{
			RemoveMissionMarker(item);
		}
	}

	private void UpdateMissionMarker(Mission mission, Image missionMarker)
	{
		CommunicationMethod? workingMethod;
		bool num = Communicates.IsInCommunicationRange(The.InGameUI.UIAllegiance, mission, out workingMethod);
		layersToDraw[WorldMapLayers.MissionMarkers].Controls.IndexOf(missionMarker);
		if (num)
		{
			missionMarker.Y = GetVertexYPosFromValueAsInt((float)mission.Coords.Value.Latitude) - missionMarker.Height / 2;
			missionMarker.X = GetVertexXPosFromValueAsInt((float)mission.Coords.Value.Longitude) - missionMarker.Width / 2;
			missionMarker.SetSkinLocation(SkinState.Normal, null, missionColor, missionColor);
			missionMarker.ToolTip = mission.GetTooltip();
		}
		else
		{
			missionMarker.SetSkinLocation(SkinState.Normal, null, Color.Gray, Color.Gray);
			missionMarker.ToolTip = "No communication";
		}
	}

	private Image AddMissionMarker(Mission mission)
	{
		string missionMarker = mission.GetMissionMarker();
		Image image = new Image(guiManager);
		image.SetSkinLocation(SkinState.Normal, guiManager.GUISpriteSheet.GetSourceRectangle(missionMarker), Color.Orange, Color.Orange);
		image.ResizeControlToFitImage();
		image.Visible = true;
		missionMarkers.Add(mission.ID, image);
		image.Visible = true;
		image.ToolTip = mission.GetTooltip();
		if (!layersToDraw.ContainsKey(WorldMapLayers.MissionMarkers))
		{
			UIComponent uIComponent = new UIComponent(guiManager)
			{
				Height = Height,
				Width = Width
			};
			uIComponent.CanHaveFocus = false;
			layersToDraw.Add(WorldMapLayers.MissionMarkers, uIComponent);
		}
		layersToDraw[WorldMapLayers.MissionMarkers].Add(image);
		return image;
	}

	private void AddForegroundSprite(UIComponent component)
	{
	}

	private void RemoveForegroundSprite(UIComponent component)
	{
	}

	private void RemoveMissionMarker(MissionID id)
	{
		Image image = missionMarkers[id];
		RemoveForegroundSprite(image);
		layersToDraw[WorldMapLayers.MissionMarkers].Controls.Remove(image);
		missionMarkers.Remove(id);
	}

	private void DrawMap()
	{
		if (imCanvas.FindChildById(DataControlID.MapTexture) != null)
		{
			return;
		}
		Image image = new Image(guiManager);
		if (The.Sim.StartGameParams.StartScenarioParams != null)
		{
			Scenario scenario = The.Sim.StartGameParams.StartScenarioParams.Scenario;
			if (scenario.ScenarioData.WorldMapTexture != null)
			{
				image.Texture = scenario.ScenarioData.WorldMapTexture;
			}
		}
		image.ID = DataControlID.MapTexture;
		image.Visible = true;
		image.ResizeControlToFitImage();
		image.X = 0;
		image.Y = 0;
		layersToDraw.Add(WorldMapLayers.Map, image);
	}

	private void DrawBorder()
	{
		border.Width = canvasWidth + 4;
		border.Height = canvasHeight + 4;
	}

	private void DrawRuler(UIComponent gridLayer)
	{
		float num = (float)The.Sim.World.GetAirDistance(new GeodeticCoordinate(minX, minY), new GeodeticCoordinate(maxX, minY));
		NiceScale niceScale = new NiceScale(0.0, 0.5f * num);
		_ = niceScale.tickSpacing;
		_ = niceScale.niceMin;
		float num2 = (float)niceScale.niceMax;
		int width = (int)(num2 / num * (float)canvasWidth);
		ruler.Width = width;
		ruler.Height = 9;
		ruler.X = canvasWidth - ruler.Width + 2;
		ruler.Y = -2;
		gridLayer.Add(ruler);
		int bottom = ruler.Bottom;
		Label label = new Label(guiManager);
		gridLayer.Add(label);
		gridLineLabels.Add(label);
		label.Init(axisType);
		label.Text = GetRulerLabelText(0f);
		label.FitToText();
		label.Y = bottom;
		label.X = ruler.X;
		label = new Label(guiManager);
		gridLayer.Add(label);
		gridLineLabels.Add(label);
		label.Init(axisType);
		label.Text = GetRulerLabelText(num2);
		label.FitToText();
		label.Y = bottom;
		label.X = ruler.Right - label.Width;
	}

	private void DrawGridAndAxis()
	{
		if (!gridHasChanged)
		{
			return;
		}
		if (layersToDraw.TryGetValue(WorldMapLayers.Grid, out var value))
		{
			value.Controls.Clear();
		}
		else
		{
			value = new UIComponent(guiManager)
			{
				Height = Height,
				Width = Width
			};
			layersToDraw.Add(WorldMapLayers.Grid, value);
		}
		gridLines.Clear();
		gridLineLabels.Clear();
		float num = tickStartY;
		int x = 5;
		int? num2 = null;
		int num3 = 166;
		for (; num < tickEndY; num += yAxisSpacing)
		{
			Bar bar = new Bar(guiManager);
			bar.EdgeSize = 1;
			bar.SetSkinLocation(SkinState.Normal, guiManager.GUISpriteSheet.GetSourceRectangle("gridline_horiz"));
			gridLines.Add(bar);
			bar.Height = 3;
			bar.Width = canvasWidth;
			bar.X = 0;
			int vertexYPosFromValueAsInt = GetVertexYPosFromValueAsInt(num);
			bar.CenterThisVertically(vertexYPosFromValueAsInt);
			value.Add(bar);
			if (canvasHeight - vertexYPosFromValueAsInt > 20 && (!num2.HasValue || Math.Abs(vertexYPosFromValueAsInt - num2.Value) > num3))
			{
				Label label = new Label(guiManager);
				Add(label);
				gridLineLabels.Add(label);
				label.Init(axisType);
				label.Text = GetTickLabelText(num);
				label.FitToText();
				num2 = GetVertexYPosFromValueAsInt(num, addMargin: true);
				label.CenterThisVertically(num2.Value);
				label.X = x;
			}
		}
		int y = Height - 27;
		num = tickStartX;
		num2 = null;
		for (; num < tickEndX; num += xAxisSpacing)
		{
			Bar bar2 = new Bar(guiManager);
			bar2.EdgeSize = 1;
			bar2.SetSkinLocation(SkinState.Normal, guiManager.GUISpriteSheet.GetSourceRectangle("gridline_vert"));
			gridLines.Add(bar2);
			bar2.IsVertical = true;
			bar2.Width = 3;
			bar2.Height = canvasHeight;
			bar2.Y = 0;
			int vertexYPosFromValueAsInt = GetVertexXPosFromValueAsInt(num);
			bar2.CenterThisHorizontally(vertexYPosFromValueAsInt);
			value.Add(bar2);
			if (vertexYPosFromValueAsInt > 20 && (!num2.HasValue || Math.Abs(vertexYPosFromValueAsInt - num2.Value) > num3))
			{
				Label label2 = new Label(guiManager);
				Add(label2);
				gridLineLabels.Add(label2);
				label2.Init(axisType);
				label2.Text = GetTickLabelText(num);
				label2.FitToText();
				num2 = GetVertexXPosFromValueAsInt(num, addMargin: true);
				label2.CenterThisHorizontally(num2.Value);
				label2.Y = y;
				label2.X = bar2.X + bar2.Width + 2;
			}
		}
		DrawRuler(value);
		gridHasChanged = false;
	}

	private SiteMarker AddSite(UIComponent layer, Site site)
	{
		SiteMarker.MarkerType markerType = SiteMarker.MarkerType.Short;
		if (site.ShowTallPin)
		{
			markerType = SiteMarker.MarkerType.Tall;
		}
		SiteMarker siteMarker = new SiteMarker(guiManager, site, The.Sim.PlaySite, btSite_Click, markerType, site.SiteMarkerOrder);
		siteMarker.X = GetVertexXPosFromValueAsInt((float)site.Coords.Longitude) - 29;
		int num = ((markerType != SiteMarker.MarkerType.Tall) ? (-31) : (-42));
		siteMarker.Y = GetVertexYPosFromValueAsInt((float)site.Coords.Latitude) + num;
		sitesOnMap[site.ID] = siteMarker;
		layer.Add(siteMarker);
		return siteMarker;
	}

	private void UpdateSite(SiteMarker siteMarker, Site site)
	{
		Allegiance uIAllegiance = The.InGameUI.UIAllegiance;
		bool canCommunicate = false;
		foreach (Allegiance allegiance in site.Allegiances)
		{
			if (allegiance.RepresentativeEntityType.Person != null && Communicates.IsInCommunicationRange(uIAllegiance, allegiance, out var _))
			{
				canCommunicate = true;
				break;
			}
		}
		bool isStart = The.InGameUI.CreateMissionPanel.start.HasValue && site.ID == (SiteID)The.InGameUI.CreateMissionPanel.start.Value.SiteID;
		bool isDestination = The.InGameUI.CreateMissionPanel.destination.HasValue && site.ID == (SiteID)The.InGameUI.CreateMissionPanel.destination.Value.SiteID;
		siteMarker.UpdateMarker(canCommunicate, isStart, isDestination, site.Name, site.ShowLabel);
	}

	public void UpdateSites()
	{
		if (!layersToDraw.TryGetValue(WorldMapLayers.SiteMarkers, out var value))
		{
			value = new UIComponent(guiManager)
			{
				Height = Height,
				Width = Width
			};
			layersToDraw.Add(WorldMapLayers.SiteMarkers, value);
		}
		bool flag = false;
		foreach (KeyValuePair<string, Site> allSite in world.AllSites)
		{
			if (!sitesOnMap.TryGetValue(allSite.Value.ID, out var value2))
			{
				value2 = AddSite(value, allSite.Value);
				flag = true;
			}
			UpdateSite(value2, allSite.Value);
		}
		if (flag)
		{
			value.SortControls((UIComponent c) => (int)c.OrderByTag1);
		}
	}

	public string GetSiteName(SiteID siteID)
	{
		return sitesOnMap[siteID].lblName.Text;
	}

	private void ChooseAxisDivision(float min, float max, out float tickSpacing, out float niceMin, out float niceMax)
	{
		NiceScale niceScale = new NiceScale(min, max);
		tickSpacing = (float)niceScale.tickSpacing;
		niceMin = (float)niceScale.niceMin;
		niceMax = (float)niceScale.niceMax;
	}

	public void DrawFatLine(float x, float y, float toX, float toY)
	{
		GetVertexPosFromValues(toX, toY, out var xPos, out var yPos);
		Vector2 p = new Vector2(xPos, yPos);
		GetVertexPosFromValues(x, y, out xPos, out yPos);
		Vector2 p2 = new Vector2(xPos, yPos);
		roundLines.Add(new RoundLine(p2, p));
	}

	public void BeginDraw()
	{
		guiManager.Game.GraphicsDevice.SetRenderTarget(renderTarget);
		guiManager.Game.GraphicsDevice.Clear(Color.Transparent);
		primitiveBatch.Begin(PrimitiveType.LineStrip);
	}

	public void EndDraw(RenderTarget2D previousRenderTarget = null)
	{
		primitiveBatch.End();
		roundLines.Clear();
		guiManager.Game.GraphicsDevice.SetRenderTarget(previousRenderTarget);
		imCanvas.Texture = renderTarget;
	}

	private void GetVertexPosFromValues(float x, float y, out float xPos, out float yPos)
	{
		xPos = GetVertexXPosFromValue(x);
		yPos = GetVertexYPosFromValue(y);
	}

	private float GetVertexXPosFromValue(float longitudeXpos)
	{
		return (longitudeXpos - minX) / xInterval * (float)canvasWidth;
	}

	private int GetVertexXPosFromValueAsInt(float longitudeXpos, bool addMargin = false)
	{
		int num = (int)Math.Round(GetVertexXPosFromValue(longitudeXpos), MidpointRounding.AwayFromZero);
		if (addMargin)
		{
			num += 11;
		}
		return num;
	}

	private int GetVertexYPosFromValueAsInt(float latitudeYPos, bool addMargin = false)
	{
		int num = (int)Math.Round(GetVertexYPosFromValue(latitudeYPos), MidpointRounding.AwayFromZero);
		if (addMargin)
		{
			num += 3;
		}
		return num;
	}

	private float GetVertexYPosFromValue(float latitudeYPos)
	{
		float num = (latitudeYPos - minY) / yInterval * (float)canvasHeight;
		return (float)canvasHeight - num;
	}

	private void SetRanges()
	{
		ChooseAxisDivision((float)latitudeStart, (float)latitudeEnd, out yAxisSpacing, out tickStartY, out tickEndY);
		ChooseAxisDivision((float)longitudeStart, (float)longitudeEnd, out xAxisSpacing, out tickStartX, out tickEndX);
		minY = (float)latitudeStart;
		maxY = (float)latitudeEnd;
		minX = (float)longitudeStart;
		maxX = (float)longitudeEnd;
		xInterval = maxX - minX;
		yInterval = maxY - minY;
	}

	private string GetRulerLabelText(float tickPos)
	{
		return tickPos.ToString("N0") + "KM";
	}

	private string GetTickLabelText(float tickPos)
	{
		return tickPos.ToString("G6") + "°";
	}

	public void UnCheckSiteMarkerButtons()
	{
		foreach (KeyValuePair<SiteID, SiteMarker> item in sitesOnMap)
		{
			item.Value.btSite.IsChecked = false;
		}
	}

	private void SiteWindow_MouseOut(MouseEventArgs args)
	{
		siteWindow.Visible = false;
	}

	private void btSite_Click(UIComponent sender, EventArgs e)
	{
		siteWindow.Fill((Site)sender.Tag1, otherPartyID);
		PlaceSiteWindow(sender);
		siteWindow.Show();
	}

	private void PlaceSiteWindow(UIComponent itemButton)
	{
		siteWindow.Position = itemButton.Parent.Position;
		siteWindow.X -= siteWindow.Width;
		if (siteWindow.X < 0)
		{
			siteWindow.X = 0;
		}
		if (siteWindow.X + siteWindow.Width > Width)
		{
			siteWindow.X = Width - siteWindow.Width;
		}
		if (siteWindow.Y < 0)
		{
			siteWindow.Y = 0;
		}
		if (siteWindow.Y + siteWindow.Height > Height)
		{
			siteWindow.Y = Height - siteWindow.Height - 60;
		}
	}
}
