using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.Editor.MapTools;
using UWGame.SimSide.Maps;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Editor.Controls;

public class Toolbar : UIComponent
{
	public enum PaintTools
	{
		None,
		Pencil,
		Brush,
		Eraser
	}

	public enum Resolution
	{
		Subtile,
		Tile
	}

	private Paintbrush paintBrush = new Paintbrush();

	private Pencil pencil = new Pencil();

	private Eraser eraser = new Eraser();

	private HorizontalList hzButtons;

	private IEditorPanel parent;

	private MapTool selectedTool;

	private Resolution resolution;

	private RadiusOption radiusOption;

	private AlphaOption alphaOption;

	private Grid grdOptions;

	public Toolbar(IEditorPanel parent, Resolution resolution)
		: base(((RosterPanel)parent).Window.guiManager)
	{
		this.resolution = resolution;
		GUIManager gui = ((RosterPanel)parent).Window.guiManager;
		base.Width = 224;
		this.parent = parent;
		hzButtons = new HorizontalList(guiManager);
		Add(hzButtons);
		hzButtons.X = 6;
		hzButtons.Height = 36;
		RadioGroup radioGroup = new RadioGroup(gui);
		radioGroup.NewMemberChecked += rgNewToolSelected;
		radioGroup.UnChecked += rg_UnChecked;
		ImageButton imageButton = new ImageButton(gui);
		imageButton.InitWithIcon(ImageButtonType.LCD, "basic_icon_category", hasCheckedState: true);
		hzButtons.AddEntry(pencil, imageButton);
		radioGroup.Add(imageButton, addAsControl: false);
		imageButton.ToolTip = "Pencil. This tool draws with a sharp edge.";
		imageButton = new ImageButton(gui);
		imageButton.InitWithIcon(ImageButtonType.LCD, "basic_icon_category", hasCheckedState: true);
		hzButtons.AddEntry(paintBrush, imageButton);
		radioGroup.Add(imageButton, addAsControl: false);
		imageButton.ToolTip = "Paintbrush. This tool draws with a soft edge.";
		imageButton = new ImageButton(gui);
		imageButton.InitWithIcon(ImageButtonType.LCD, "basic_icon_category", hasCheckedState: true);
		hzButtons.AddEntry(eraser, imageButton);
		radioGroup.Add(imageButton, addAsControl: false);
		imageButton.ToolTip = "Eraser. This tool removes/erases.";
		grdOptions = new Grid(gui, ListBoxType.LCD, Label.LabelType.CRTBigGlow);
		grdOptions.FixedItemHeights = false;
		grdOptions.RenderType = RenderType.CRTAndLCD;
		grdOptions.CanGrowInHeight = true;
		Add(grdOptions);
		grdOptions.Font = GUIManager.LCDandHUDBodyFontPath;
		grdOptions.Width = Width;
		grdOptions.Position = new Point(0, hzButtons.Bottom + 6);
		_ = hzButtons.Bottom;
		radiusOption = new RadiusOption(gui);
		alphaOption = new AlphaOption(gui);
		base.Height = grdOptions.Bottom;
		The.MapUI.LeftMouseDownInMap += MapUI_LeftMouseDownInMap;
		The.MapUI.LeftMouseReleasedInMap += MapUI_LeftMouseReleasedInMap;
		if (resolution == Resolution.Subtile)
		{
			The.MapUI.LeftMouseSubtileDragInMap += MapUI_LeftMouseSubtileDragInMap;
		}
		else
		{
			The.MapUI.LeftMouseTileDragInMap += MapUI_LeftMouseTileDragInMap;
		}
	}

	private void MapUI_LeftMouseReleasedInMap(Vector3 obj)
	{
		The.MapUI.ResetDragging();
	}

	private void MapUI_LeftMouseTileDragInMap(TilePos obj)
	{
		UseTool(obj);
	}

	private void MapUI_LeftMouseSubtileDragInMap(SubtilePos obj)
	{
		UseTool(obj);
	}

	private void MapUI_LeftMouseDownInMap(Vector3 location)
	{
		if (resolution == Resolution.Subtile)
		{
			UseTool(MapManager.WorldPosToSubtilePos(location));
		}
		else
		{
			UseTool(MapManager.WorldPosToTilePos(location));
		}
	}

	private void rg_UnChecked(EventArgs obj)
	{
		The.InGameUI.InterfaceMode = InGameInterface.InterfaceState.None;
	}

	private void UseTool(TilePos tilePos)
	{
		if (selectedTool != null)
		{
			List<MapTool.TileAndChange> affectedTiles = selectedTool.GetAffectedTiles(tilePos);
			parent.AffectMap(affectedTiles);
		}
	}

	private void UseTool(SubtilePos subtilePos)
	{
		if (selectedTool != null)
		{
			List<MapTool.SubTileAndChange> affectedSubtiles = selectedTool.GetAffectedSubtiles(subtilePos);
			parent.AffectMap(affectedSubtiles);
		}
	}

	private void rgNewToolSelected(ICanBeChecked arg1, EventArgs arg2)
	{
		MapTool mapTool = (selectedTool = (MapTool)(arg1 as ImageButton).Tag1);
		grdOptions.BeginAddingEntries();
		IHasRadiusOption hasRadiusOption = mapTool as IHasRadiusOption;
		if (hasRadiusOption != null)
		{
			radiusOption.Set(hasRadiusOption.RadiusSetting);
		}
		AddOrRemoveOption(hasRadiusOption, radiusOption);
		IHasAlphaOption hasAlphaOption = mapTool as IHasAlphaOption;
		if (hasAlphaOption != null)
		{
			alphaOption.Set(hasAlphaOption.AlphaSetting);
		}
		AddOrRemoveOption(hasAlphaOption, alphaOption);
		grdOptions.Sort((UIComponent u) => ((ToolOption)u).Order, Grid.Sorting.Ascending);
		grdOptions.EndAddingEntries();
		The.InGameUI.InterfaceMode = InGameInterface.InterfaceState.EditorTool;
		Height = grdOptions.Bottom;
	}

	private void AddOrRemoveOption(object hasOption, ToolOption option)
	{
		if (hasOption != null)
		{
			if (!grdOptions.EntriesByKey.ContainsKey(option))
			{
				grdOptions.AddEntry(option, option);
			}
		}
		else
		{
			// PORT DEVIATION 19 (see PORTING-NOTES.md). Was grdOptions.RemoveEntry(option).
			//
			// Grid.RemoveEntry(key) looks the key up with entriesByKey[key], which throws
			// KeyNotFoundException when the option is not currently in the grid - and this is
			// called for EVERY option on every tool change, including ones the newly selected
			// tool never had. Selecting a tool without a radius option after one that has none
			// crashes the editor:
			//
			//   The given key 'UWGame.ClientSide.Interface.Editor.MapTools.RadiusOption' was
			//   not present in the dictionary.
			//     at WindowSystem.Grid.RemoveEntry(Object key)
			//     at ...Editor.Controls.Toolbar.AddOrRemoveOption(Object, ToolOption)
			//
			// TryRemoveEntry is the studio's OWN guarded variant (Grid.cs:648) - it does exactly
			// the ContainsKey check and delegates - so this is the call that was always meant to
			// be here. Note the add branch above already guards with ContainsKey; only the
			// remove branch did not.
			//
			// WHY THE STUDIO NEVER HIT IT: the map editor is unreachable in the retail game.
			// MainMenuScreen.EditMap and TestMap are fully implemented but no button was ever
			// wired to them - see PORT DEVIATION 16's note. The bundled Unhidden Mod adds those
			// buttons, which is what made this reachable. Reported by a modder, with this fix.
			grdOptions.TryRemoveEntry(option);
		}
	}
}
