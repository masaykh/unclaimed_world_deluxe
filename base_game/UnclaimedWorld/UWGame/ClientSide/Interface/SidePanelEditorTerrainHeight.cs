using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.Editor;
using UWGame.ClientSide.Interface.Editor.Controls;
using UWGame.ClientSide.Interface.Editor.MapTools;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Maps.MapEditor;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class SidePanelEditorTerrainHeight : RosterPanel, IEditorPanel
{
	private Label lblStatusInfo1;

	private Label lblStatusInfo2;

	private Toolbar tools;

	public SidePanelEditorTerrainHeight()
		: base(The.InGameUI.sidePanelFullHeight, isInfoPanel: true)
	{
		InitStatusContentPanel();
		tools = new Toolbar(this, Toolbar.Resolution.Subtile);
		lcdSurface.Add(tools);
		tools.Y = 400;
	}

	public void AffectMap(List<MapTool.TileAndChange> affectedTiles)
	{
	}

	public void AffectMap(List<MapTool.SubTileAndChange> affectedTiles)
	{
		int x = affectedTiles.Min((MapTool.SubTileAndChange t) => t.SubtilePos.X);
		ushort x2 = affectedTiles.Max((MapTool.SubTileAndChange t) => t.SubtilePos.X);
		int y = affectedTiles.Min((MapTool.SubTileAndChange t) => t.SubtilePos.Y);
		int y2 = affectedTiles.Max((MapTool.SubTileAndChange t) => t.SubtilePos.Y);
		Point point = MapManager.SubTileToTilePos(new Point(x, y));
		Point point2 = MapManager.SubTileToTilePos(new Point(x2, y2));
		point2.X = Common.ClampBottom(point2.X, point.X + 1);
		point2.Y = Common.ClampBottom(point2.Y, point.Y + 1);
		Rectangle tileArea = new Rectangle(point.X, point.Y, point2.X - point.X, point2.Y - point.Y);
		MapLoader.CreateTerrainSubtiles(tileArea);
		float waterLevelBelowTerrain = The.Client.Renderer.Water.WaterHeight - 1400f;
		foreach (MapTool.SubTileAndChange affectedTile in affectedTiles)
		{
			MapManager map = The.Map;
			SubtilePos subtilePos = affectedTile.SubtilePos;
			Terrain terrain = map.GetTerrain(subtilePos.ToPoint());
			float num = 0f - affectedTile.Change;
			float terrainDepth = terrain.TerrainDepth + num;
			MapLoader.SetTerrainDepth(waterLevelBelowTerrain, terrain, terrainDepth);
			MapManager map2 = The.Map;
			subtilePos = affectedTile.SubtilePos;
			map2.SetSubtileCostToSurfaceType(subtilePos.ToPoint());
			The.Client.Renderer.terrainSlicedMap.Redraw(MapManager.SubTileToWorldPos3(affectedTile.SubtilePos));
		}
		MapLoader.RecomputeSubdivision(tileArea);
		The.Map.IterateTileArea(tileArea, The.Client.Renderer.RecomputeTerrainTilePositions);
	}

	private void InitStatusContentPanel()
	{
		statusContent = The.InGameUI.StatusScreen.GetNewSurfaceContent();
		_ = The.Sim.Controller.Game;
		GUIManager gui = The.InGameUI.gui;
		InitStatusImage();
		InitBillboardPanel();
		RosterPanel.InitStatusCRTHeader(gui, statusContent, 8, out lblStatusHeading, out crtUnderline);
		lblStatusInfo1 = new Label(gui);
		statusContent.Add(lblStatusInfo1);
		lblStatusInfo1.Position = new Point(8, 80);
		lblStatusInfo1.Init(Label.LabelType.CRTSmall);
		lblStatusInfo1.Width = 200;
		lblStatusInfo2 = new Label(gui);
		statusContent.Add(lblStatusInfo2);
		lblStatusInfo2.Position = new Point(8, 100);
		lblStatusInfo2.Init(Label.LabelType.CRTSmall);
		lblStatusInfo2.Width = 200;
	}

	public override void Hide()
	{
		base.Hide();
	}

	public override void Refresh()
	{
	}
}
