using Microsoft.Xna.Framework;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using WindowSystem;

namespace UWGame.ClientSide.Interface.HUD_Windows;

public class EntityActivityHUDWindow : HUDWindow
{
	public EntityID? owner;

	private HorizontalList activityIcons;

	private Point ScreenPosition;

	private Point OffsetFromParent;

	public EntityActivityHUDWindow(Point offsetFromParent, int maxNumberOfIcons)
		: base(80, 24, hasSurface: false)
	{
		OffsetFromParent = offsetFromParent;
		activityIcons = new HorizontalList(The.InGameUI.gui, maxNumberOfIcons);
	}

	public override void Refresh()
	{
		Remove(activityIcons);
		if (owner.HasValue)
		{
			Entity knownDataAsEntity = The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownDataAsEntity(owner.Value);
			if (knownDataAsEntity != null && UpdateIcons(knownDataAsEntity))
			{
				Add(activityIcons);
			}
		}
	}

	public void SetPosition(Point parentScreenPosition)
	{
		ScreenPosition = parentScreenPosition;
		ScreenPosition.X += OffsetFromParent.X;
		ScreenPosition.Y += OffsetFromParent.Y;
	}

	private bool UpdateIcons(Entity entityWithStatus)
	{
		foreach (PresentationTypeCategory finalPresentationTypeCategory in GameData.Instance.CustomEntityActivityData.FinalPresentationTypeCategories)
		{
			int? numberOfItems = null;
			PresentationTypeCategoryProcessor.DisplayCategory(finalPresentationTypeCategory, entityWithStatus, activityIcons, ref numberOfItems);
		}
		DisplayWindow.Width = activityIcons.Width;
		DisplayWindow.CenterChildVertically(activityIcons);
		return true;
	}

	public void Show()
	{
		base.ShowOnPlayfield(ScreenPosition.X, ScreenPosition.Y);
	}

	public void Update()
	{
		base.ShowOnPlayfield(ScreenPosition.X, ScreenPosition.Y);
	}

	public override void Hide()
	{
		base.Hide();
	}
}
