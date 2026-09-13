using Microsoft.Xna.Framework;

namespace WindowSystem;

public class RadioButton : CheckBox
{
	private static Rectangle defaultSkin = new Rectangle(1, 59, 15, 15);

	private static Rectangle defaultHoverSkin = new Rectangle(17, 59, 15, 15);

	private static Rectangle defaultPressedSkin = new Rectangle(33, 59, 15, 15);

	private static Rectangle defaultCheckedSkin = new Rectangle(1, 75, 15, 15);

	private static Rectangle defaultCheckedHoverSkin = new Rectangle(17, 75, 15, 15);

	private static Rectangle defaultCheckedPressedSkin = new Rectangle(33, 75, 15, 15);

	public new static Rectangle DefaultSkin
	{
		set
		{
			defaultSkin = value;
		}
	}

	public new static Rectangle DefaultHoverSkin
	{
		set
		{
			defaultHoverSkin = value;
		}
	}

	public new static Rectangle DefaultPressedSkin
	{
		set
		{
			defaultPressedSkin = value;
		}
	}

	public new static Rectangle DefaultCheckedSkin
	{
		set
		{
			defaultCheckedSkin = value;
		}
	}

	public new static Rectangle DefaultCheckedHoverSkin
	{
		set
		{
			defaultCheckedHoverSkin = value;
		}
	}

	public new static Rectangle DefaultCheckedPressedSkin
	{
		set
		{
			defaultCheckedPressedSkin = value;
		}
	}

	public RadioButton(GUIManager guiManager)
		: base(guiManager)
	{
		base.Skin = defaultSkin;
		base.HoverSkin = defaultHoverSkin;
		base.PressedSkin = defaultPressedSkin;
		base.CheckedSkin = defaultCheckedSkin;
		base.CheckedHoverSkin = defaultCheckedHoverSkin;
		base.CheckedPressedSkin = defaultCheckedPressedSkin;
		base.SwitchStateOnClick = false;
	}
}
