using System;
using System.Collections.Generic;

namespace WindowSystem;

public class RadioGroup : UIComponent
{
	private bool firstButtonClicked;

	public int ButtonMargin;

	private List<ICanBeChecked> abstractMembers = new List<ICanBeChecked>();

	public event Action<ICanBeChecked, EventArgs> NewMemberChecked;

	public event Action<EventArgs> UnChecked;

	public RadioGroup(GUIManager guiManager)
		: base(guiManager)
	{
		firstButtonClicked = false;
		base.CanHaveFocus = false;
	}

	public override int Add(UIComponent control)
	{
		return 0;
	}

	public virtual void Add(ICanBeChecked control, bool addAsControl = true, bool setHorizPosition = false)
	{
		control.Click += OnClick;
		if (addAsControl)
		{
			UIComponent uIComponent = (UIComponent)control;
			base.Add(uIComponent);
			if (setHorizPosition)
			{
				uIComponent.X = GetNextXPos();
			}
		}
		Util.AddToList(ref abstractMembers, control);
	}

	public int GetNextXPos()
	{
		if (abstractMembers.Count > 0)
		{
			return ((UIComponent)abstractMembers[abstractMembers.Count - 1]).Right + ButtonMargin;
		}
		return 0;
	}

	public void Clear()
	{
		base.Controls.Clear();
	}

	public void SelectMember(ICanBeChecked member)
	{
		member.IsChecked = true;
		foreach (ICanBeChecked abstractMember in abstractMembers)
		{
			if (abstractMember != member)
			{
				abstractMember.IsChecked = false;
			}
		}
	}

	public RadioButton GetSelected()
	{
		if (abstractMembers.Count > 0)
		{
			ICanBeChecked canBeChecked = abstractMembers.Find((ICanBeChecked u) => u.IsChecked);
			if (canBeChecked != null)
			{
				return (RadioButton)canBeChecked;
			}
			return null;
		}
		return null;
	}

	protected void OnClick(UIComponent sender, EventArgs e)
	{
		ICanBeChecked canBeChecked = (ICanBeChecked)sender;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		foreach (ICanBeChecked abstractMember in abstractMembers)
		{
			if (abstractMember != canBeChecked)
			{
				if (abstractMember.IsChecked)
				{
					flag2 = true;
				}
				abstractMember.IsChecked = false;
			}
		}
		if (canBeChecked.IsChecked && !flag2 && canBeChecked.CheckedMode == CheckedModes.SwitchCheckedStateOnClick)
		{
			flag = true;
		}
		foreach (ICanBeChecked abstractMember2 in abstractMembers)
		{
			if (abstractMember2.IsChecked)
			{
				flag3 = true;
			}
		}
		if (!firstButtonClicked || canBeChecked.IsChecked)
		{
			firstButtonClicked = true;
		}
		if ((flag2 || flag) && this.NewMemberChecked != null)
		{
			this.NewMemberChecked(canBeChecked, EventArgs);
		}
		if (!flag3 && this.UnChecked != null)
		{
			this.UnChecked(EventArgs);
		}
	}
}
