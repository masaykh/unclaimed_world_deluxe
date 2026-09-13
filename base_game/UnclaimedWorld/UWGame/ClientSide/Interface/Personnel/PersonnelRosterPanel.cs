using System;
using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;

namespace UWGame.ClientSide.Interface.Personnel;

public class PersonnelRosterPanel : RosterPanel
{
	private PersonnelList PersonnelList;

	public PersonnelRosterPanel()
		: base("PERSONNEL", 615, The.InGameUI.rosterPanelHeight, needBottomMarginForButton: false)
	{
		PersonnelList = new PersonnelList(intface, lcdSurface, showSelectors: false, 0, isRoster: true);
	}

	public override void Show()
	{
		PersonnelList.Fill(GetPeople, showSelectors: false, showMigrationRisk: true);
		base.Show();
	}

	public static List<IKnownEntityData> GetPeople()
	{
		return ((IEnumerable<Entity>)The.InGameUI.UIAllegiance.MembersList).Select((Func<Entity, IKnownEntityData>)((Entity e) => e)).ToList();
	}

	public override void Refresh()
	{
		base.Refresh();
		PersonnelList.Populate();
	}
}
