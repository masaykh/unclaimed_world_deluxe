using System;
using InputEventSystem;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using WindowSystem;

namespace UWGame.ClientSide.Interface.HUD_Windows;

public class ContextMenuOpener : HUDWindow
{
	private ImageButton btCycle;

	private TextButton btExpand;

	private UIComponent pnZones;

	private Grid grdZones;

	public int CycleButtonXPos => btCycle.X;

	public ContextMenuOpener()
		: base(96, 60)
	{
		DisplayWindow.DebugTag = "contextMenuOpener";
		btExpand = new TextButton(gui);
		Add(btExpand);
		btExpand.Text = "NEW";
		btExpand.Init(TextButton.TextButtonType.HUD);
		btExpand.MouseOver += bt_MouseOver;
		btExpand.ScaleWidthToFitText();
		btExpand.Y = 6;
		btExpand.X = 6;
		btExpand.DebugTag = "context";
		btExpand.ZOrder = 1f;
		TileSelectionContextMenu.CreateCycleButton(ref btCycle, gui, btExpand.Right + 6);
		Add(btCycle);
		btCycle.X = btExpand.Right + 6;
		btCycle.Click += btCycle_Click;
		DisplayWindow.Height = btCycle.Bottom + 6;
		DisplayWindow.Width = btCycle.Right + 6;
	}

	private void pnZones_HeightResize(UIComponent sender)
	{
		DisplayWindow.Height = pnZones.Y + pnZones.Height;
	}

	private void grdSkills_HeightResize(UIComponent sender)
	{
		pnZones.Height = grdZones.Y + grdZones.Height + 10;
	}

	private void btModify_Click(UIComponent sender, EventArgs e)
	{
	}

	private void btCycle_Click(UIComponent sender, EventArgs e)
	{
		CycleEntities(The.InGameUI.SelectedTiles);
	}

	public static void CycleEntities(MapArea mapArea)
	{
		EntityID? previousID = null;
		IKnownEntityData data = null;
		if (The.InGameUI.SelectedEntity.HasValue)
		{
			previousID = The.InGameUI.SelectedEntity.Value;
			The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(The.InGameUI.SelectedEntity.Value, out data);
		}
		bool returnFirstMatch = false;
		bool foundEntities = false;
		bool foundEntities2 = false;
		bool foundEntities3 = false;
		bool foundEntities4 = false;
		while (true)
		{
			if (data == null || (data != null && data.EntityType.Person != null))
			{
				EntityID? entityID = mapArea.CycleKnownEntities(previousID, The.InGameUI.UIAllegiance.SharedKnowledge, (IKnownEntityData entity) => entity.EntityType.Person != null, out foundEntities, returnFirstMatch);
				if (entityID.HasValue)
				{
					The.InGameUI.SelectEntity(entityID.Value);
					break;
				}
				data = null;
				returnFirstMatch = true;
			}
			if (data == null || (data != null && data.EntityType.IntelligenceType != null && data.EntityType.Person == null))
			{
				EntityID? entityID = mapArea.CycleKnownEntities(previousID, The.InGameUI.UIAllegiance.SharedKnowledge, (IKnownEntityData entity) => entity.EntityType.IntelligenceType != null && entity.EntityType.Person == null, out foundEntities2, returnFirstMatch);
				if (entityID.HasValue)
				{
					The.InGameUI.SelectEntity(entityID.Value);
					break;
				}
				data = null;
				returnFirstMatch = true;
			}
			if (data == null || (data != null && data.EntityType.StructureType != null))
			{
				EntityID? entityID = mapArea.CycleKnownEntities(previousID, The.InGameUI.UIAllegiance.SharedKnowledge, (IKnownEntityData entity) => entity.EntityType.StructureType != null, out foundEntities3, returnFirstMatch);
				if (entityID.HasValue)
				{
					The.InGameUI.SelectEntity(entityID.Value);
					break;
				}
				data = null;
				returnFirstMatch = true;
			}
			if (data == null || (data != null && data.EntityType.ItemType != null))
			{
				EntityID? entityID = mapArea.CycleKnownEntities(previousID, The.InGameUI.UIAllegiance.SharedKnowledge, (IKnownEntityData entity) => entity.EntityType.ItemType != null, out foundEntities4, returnFirstMatch);
				if (entityID.HasValue)
				{
					The.InGameUI.SelectEntity(entityID.Value);
					break;
				}
				data = null;
				returnFirstMatch = true;
			}
			if (foundEntities || foundEntities2 || foundEntities3 || foundEntities4)
			{
				returnFirstMatch = true;
				previousID = null;
				continue;
			}
			break;
		}
	}

	private void ViewPort_MouseOut(MouseEventArgs args)
	{
	}

	private void bt_MouseOver(UIComponent sender, MouseEventArgs args)
	{
		if (btExpand.Enabled)
		{
			The.InGameUI.ShowContextMenu(DisplayWindow.X, DisplayWindow.Y, DisplayWindow);
		}
	}

	public void Populate()
	{
		if (TileSelectionContextMenu.GetMapArea().Count == 0)
		{
			btExpand.Enabled = false;
		}
		else
		{
			btExpand.Enabled = true;
		}
	}

	private void PopulateZoneList()
	{
	}

	public override void Hide()
	{
		base.Hide();
		The.InGameUI.ContextMenu.Hide();
	}
}
