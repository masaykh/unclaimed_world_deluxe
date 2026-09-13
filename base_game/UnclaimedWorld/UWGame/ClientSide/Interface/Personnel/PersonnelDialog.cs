using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.Controls;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Personnel;

public class PersonnelDialog : Panel
{
	private LCDScreen lcdScreen;

	private UIComponent lcdSurface;

	private Box display;

	private PersonnelList PersonnelList;

	private ErrorAndMessagePanel errorAndMessagePanel;

	private TextButton btOK;

	private TextButton btCancel;

	public event EventHandler OKClick;

	public event EventHandler CancelClick;

	public PersonnelDialog(CommonInterface intf, Point position)
		: base(intf, "", position, new Vector2(468f, 500f), Level.StackedDialogs)
	{
		RosterPanel.CreateRosterStyleLCDPanel(intf, Window, out display, out lcdSurface, ref lcdScreen);
		errorAndMessagePanel = new ErrorAndMessagePanel(lcdSurface);
		PersonnelList = new PersonnelList(intf, lcdSurface, showSelectors: true, errorAndMessagePanel.Height, isRoster: false);
		btOK = AddLowerButton("OK", "Accepts the order and closes the dialog.", Align.Left);
		btOK.Click += btOK_Click;
		btCancel = AddLowerButton("CANCEL", "Cancels and closes the dialog.", Align.Right);
		btCancel.Click += btCancel_Click;
	}

	private void btCancel_Click(UIComponent sender, EventArgs e)
	{
		Window.Hide();
		if (this.CancelClick != null)
		{
			this.CancelClick(this, null);
		}
	}

	private void btOK_Click(UIComponent sender, EventArgs e)
	{
		errorAndMessagePanel.Clear();
		List<string> errors = null;
		if (ValidateSelectionCanEmbark(ref errors))
		{
			if (this.OKClick != null)
			{
				this.OKClick(this, null);
			}
			Hide();
		}
		else
		{
			errorAndMessagePanel.ShowErrors(errors);
		}
	}

	public void FillAndShow(Func<List<IKnownEntityData>> getEntities, Point dialogSourceAbsolutePosition, bool showSelectors, bool showMigrateRisk)
	{
		Fill(getEntities, showSelectors, showMigrateRisk);
		ShowInScreenSpace(dialogSourceAbsolutePosition.X - Window.Width / 2, dialogSourceAbsolutePosition.Y / 2);
	}

	private bool ValidateSelectionCanEmbark(ref List<string> errors)
	{
		List<EntityID> selectedEntities = GetSelectedEntities();
		int num = 0;
		if (selectedEntities != null)
		{
			foreach (EntityID item in selectedEntities)
			{
				Entity knownDataAsEntity = The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownDataAsEntity(item);
				if (knownDataAsEntity != null)
				{
					if (!knownDataAsEntity.Intelligence.IsReadyForEmbark(The.InGameUI.UIAllegiance))
					{
						Common.AddToList(ref errors, knownDataAsEntity.Name + " is not willing to embark now.");
					}
					num++;
				}
				else
				{
					Common.AddToList(ref errors, "The display is out of date. Please try again.");
				}
			}
		}
		if (errors == null && !The.InGameUI.UIAllegiance.IsWithinPopulationCap(num))
		{
			Common.AddToList(ref errors, "Exceeds max population! We do not accept that many newcomers!");
		}
		if (errors != null && errors.Count > 0)
		{
			return false;
		}
		return true;
	}

	public List<EntityID> GetSelectedEntities()
	{
		return PersonnelList.GetSelectedEntities();
	}

	public void Fill(Func<List<IKnownEntityData>> entities, bool showSelectors, bool showMigrationRisk)
	{
		PersonnelList.Fill(entities, showSelectors, showMigrationRisk);
		btOK.Visible = showSelectors;
	}

	public override void Refresh()
	{
		base.Refresh();
		PersonnelList.Populate();
	}
}
