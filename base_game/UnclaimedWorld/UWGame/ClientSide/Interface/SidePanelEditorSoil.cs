using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.Editor;
using UWGame.ClientSide.Interface.Editor.Controls;
using UWGame.ClientSide.Interface.Editor.MapTools;
using UWGame.SimSide;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Soil;
using UWGame.SimSide.Vegetation;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class SidePanelEditorSoil : RosterPanel, IEditorPanel
{
	private Grid outerGrid;

	private Label lblStatusInfo1;

	private Label lblStatusInfo2;

	private CollapsablePanel cpSoil;

	private CollapsablePanel cpVegetation;

	private Grid grdSoil;

	private Grid grdVegetation;

	private Toolbar tools;

	private RenderedTerrainType SelectedType;

	private static char[] stopChars = new char[11]
	{
		'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
		'_'
	};

	public SidePanelEditorSoil()
		: base(The.InGameUI.sidePanelFullHeight, isInfoPanel: true)
	{
		int value = 20;
		outerGrid = RosterPanel.CreateOuterGridForCollapsableLists(The.InGameUI.gui, lcdSurface, 150);
		RosterPanel.AddCollapsablePanelAndGrid(The.InGameUI.gui, outerGrid, "Soil", value, out cpSoil, out grdSoil);
		grdSoil.SelectedChanged += grdSoil_SelectedChanged;
		RosterPanel.AddCollapsablePanelAndGrid(The.InGameUI.gui, outerGrid, "Vegetation", value, out cpVegetation, out grdVegetation);
		grdVegetation.SelectedChanged += grdVegetation_SelectedChanged;
		PopulateGrid();
		InitStatusContentPanel();
		tools = new Toolbar(this, Toolbar.Resolution.Subtile);
		lcdSurface.Add(tools);
		tools.Y = outerGrid.Bottom + 12;
	}

	public void AffectMap(List<MapTool.TileAndChange> affectedSubtiles)
	{
	}

	public void AffectMap(List<MapTool.SubTileAndChange> affectedSubtiles)
	{
		foreach (MapTool.SubTileAndChange affectedSubtile in affectedSubtiles)
		{
			MapManager map = The.Map;
			SubtilePos subtilePos = affectedSubtile.SubtilePos;
			Terrain terrain = map.GetTerrain(subtilePos.ToPoint());
			float amount = ((!terrain.IsSubtileTerrain()) ? (affectedSubtile.Change / 9f) : affectedSubtile.Change);
			if (SelectedType is SoilComponentType)
			{
				terrain.AddSoilComponent((SoilComponentType)SelectedType, amount);
				terrain.NormalizeSoil();
			}
			else
			{
				terrain.AddVegetation((LowVegetationType)SelectedType, amount);
				terrain.NormalizeVegetation();
			}
			The.Client.Renderer.terrainSlicedMap.Redraw(MapManager.SubTileToWorldPos3(affectedSubtile.SubtilePos));
		}
	}

	private void cbDrawCoords_Click(UIComponent sender, EventArgs e)
	{
	}

	private void cbDrawResources_Click(UIComponent sender, EventArgs e)
	{
	}

	private void saveDialog_SaveOrLoadClick(object sender, EventArgs e)
	{
	}

	private void btClear_Click(UIComponent sender, EventArgs e)
	{
		intface.InterfaceMode = InGameInterface.InterfaceState.EditorClearTile;
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

	private void RefreshStatusScreen()
	{
	}

	private void PopulateGrid()
	{
		outerGrid.BeginAddingEntries();
		grdSoil.BeginAddingEntries();
		foreach (KeyValuePair<string, SoilComponentType> allSoilComponentType in GameData.Instance.AllSoilComponentTypes)
		{
			grdSoil.AddEntry(allSoilComponentType.Value, allSoilComponentType.Value.Name);
		}
		grdSoil.EndAddingEntries();
		grdVegetation.BeginAddingEntries();
		foreach (KeyValuePair<string, LowVegetationType> allLowVegetationType in GameData.Instance.AllLowVegetationTypes)
		{
			grdVegetation.AddEntry(allLowVegetationType.Value, allLowVegetationType.Value.Name);
		}
		grdVegetation.EndAddingEntries();
		outerGrid.EndAddingEntries();
	}

	private void grdSoil_SelectedChanged(UIComponent sender)
	{
		if (grdSoil.GetSelectedKey(out var key))
		{
			SelectedType = (RenderedTerrainType)key;
		}
	}

	private void grdVegetation_SelectedChanged(UIComponent sender)
	{
		if (grdVegetation.GetSelectedKey(out var key))
		{
			SelectedType = (RenderedTerrainType)key;
		}
	}

	public override void Hide()
	{
		base.Hide();
	}

	public override void Refresh()
	{
	}
}
