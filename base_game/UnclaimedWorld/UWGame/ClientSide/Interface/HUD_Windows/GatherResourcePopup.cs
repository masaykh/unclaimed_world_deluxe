using System;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Resources;
using WindowSystem;

namespace UWGame.ClientSide.Interface.HUD_Windows;

public class GatherResourcePopup : HUDPopup
{
	private FillableBar fillableBar;

	private ResourceType resourceType;

	public GatherResourcePopup()
		: base(200, 160)
	{
		DisplayWindow.Hide();
		fillableBar = new FillableBar(gui, FillableBar.FillableBarType.HUDSlider, canGrow: false, includeButtons: true, 0.5f, 0.1f)
		{
			Position = new Point(10, 20),
			Width = 100
		};
		Add(fillableBar);
		fillableBar.SliderMouseUp += fillableBar_SliderMouseUp;
	}

	private void fillableBar_SliderMouseUp(object sender, EventArgs e)
	{
		DisplayWindow.Hide();
		The.InGameUI.ContextMenu.ZoneGatherResourcesWindow.SaveJobChanges(resourceType, fillableBar.Value);
	}

	public void Fill(ResourceType resourceType, int currentOrders, int max)
	{
		this.resourceType = resourceType;
		fillableBar.Value = currentOrders;
		fillableBar.MaxValue = max;
		fillableBar.UpdateSliderPosition();
	}
}
