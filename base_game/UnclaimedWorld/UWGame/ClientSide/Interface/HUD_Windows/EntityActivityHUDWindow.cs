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

	/// <summary>
	/// PORT FIX: below the parent window, never under it. The studio's offset is 20 from the top of
	/// a name marker 24 high, so the activity line - "Spear (flint-tipped)" and its progress bar -
	/// sat 4 pixels under the colonist's name, and the name, drawn later, covered it (Kastuk, "Too
	/// much info on the screen", with a screenshot). The offset still applies when it is the lower.
	/// </summary>
	public void SetPositionBelow(Point parentScreenPosition, int parentHeight)
	{
		SetPosition(parentScreenPosition);
		ScreenPosition.Y = System.Math.Max(ScreenPosition.Y, parentScreenPosition.Y + parentHeight + ActivityGap);
	}

	/// <summary>Pixels between the name marker and the activity line under it.</summary>
	public const int ActivityGap = 1;

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
