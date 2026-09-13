using System;
using System.Text;
using InputEventSystem;
using Microsoft.Xna.Framework;

namespace WindowSystem;

public class Hyperlink : TextButton
{
	public uint? TargetResourceContainerID;

	public uint? TargetEntityID;

	public Point? TargetMapPosition;

	public uint? TargetZoneID;

	private new const string toolTip = "LMB: Select the link target.\n RMB: Center on target.\n Double click: Select and center.";

	private Color normalColor = Color.Black;

	public static Color HoverColor = Color.Yellow;

	public static Color PressedColor = Color.DarkGray;

	public new Color NormalColor
	{
		get
		{
			return normalColor;
		}
		set
		{
			if (normalColor != value)
			{
				if (base.LabelColor == normalColor)
				{
					base.LabelColor = value;
				}
				normalColor = value;
				Rectangle sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("hyperlink");
				buttonBox.SetSkinLocation(SkinState.Normal, sourceRectangle, normalColor, normalColor);
			}
		}
	}

	public override bool Enabled
	{
		get
		{
			return base.Enabled;
		}
		set
		{
			if (base.Enabled != value)
			{
				if (!value)
				{
					buttonBox.Visible = false;
					ToolTip = null;
				}
				else
				{
					buttonBox.Visible = true;
					ToolTip = "LMB: Select the link target.\n RMB: Center on target.\n Double click: Select and center.";
				}
				base.Enabled = value;
			}
		}
	}

	public Hyperlink(GUIManager guiManager, RenderType renderType = RenderType.CRTAndLCD)
		: base(guiManager)
	{
		base.Type = TextButtonType.Hyperlink;
		base.Click += Hyperlink_Click;
		base.RightClick += Hyperlink_RightClick;
		DebugTag = "testHyper";
		buttonBox.DebugTag = "hyperBar";
		MinHeight = 1;
		Rectangle sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("hyperlink");
		buttonBox.SetSkinLocation(SkinState.Normal, sourceRectangle, NormalColor, NormalColor);
		buttonBox.SetSkinLocation(SkinState.Hover, sourceRectangle, HoverColor, HoverColor);
		buttonBox.SetSkinLocation(SkinState.Pressed, sourceRectangle, PressedColor, PressedColor);
		Height = sourceRectangle.Height;
		base.CornerSize = 4;
		base.Font = GUIManager.LCDandHUDFont;
		ToolTip = "LMB: Select the link target.\n RMB: Center on target.\n Double click: Select and center.";
		RenderType = renderType;
	}

	private void Hyperlink_RightClick(UIComponent sender, EventArgs e)
	{
		guiManager.HyperLinkClicked(TargetEntityID, TargetResourceContainerID, TargetZoneID, TargetMapPosition, GUIManager.MouseButtonClicked.Right);
	}

	private void Hyperlink_Click(UIComponent sender, EventArgs e)
	{
		guiManager.HyperLinkClicked(TargetEntityID, TargetResourceContainerID, TargetZoneID, TargetMapPosition, GUIManager.MouseButtonClicked.Left);
	}

	protected override void OnMouseOver(UIComponent sender, MouseEventArgs args)
	{
		base.LabelColor = HoverColor;
		buttonBox.CurrentSkinState = SkinState.Hover;
		base.OnMouseOver(sender, args);
	}

	protected override void OnMouseOut(UIComponent sender, MouseEventArgs args)
	{
		base.LabelColor = NormalColor;
		buttonBox.CurrentSkinState = SkinState.Normal;
		base.OnMouseOut(sender, args);
	}

	protected override void OnMouseDown(MouseEventArgs args)
	{
		if (args.Button == MouseButtons.Left)
		{
			buttonBox.CurrentSkinState = SkinState.Pressed;
			base.LabelColor = PressedColor;
		}
		base.OnMouseDown(args);
	}

	protected override void OnMouseUp(MouseEventArgs args)
	{
		if (args.Button == MouseButtons.Left)
		{
			if (CheckCoordinates(args.Position.X, args.Position.Y))
			{
				buttonBox.CurrentSkinState = SkinState.Hover;
				base.LabelColor = HoverColor;
			}
			else
			{
				buttonBox.CurrentSkinState = SkinState.Normal;
				base.LabelColor = NormalColor;
			}
		}
		base.OnMouseUp(args);
	}

	public static string ToLink(string name, long id, bool useUpperCase = false)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("§E");
		stringBuilder.Append(id.ToString());
		stringBuilder.Append("¤");
		if (!useUpperCase)
		{
			stringBuilder.Append(name);
		}
		stringBuilder.Append("§");
		return stringBuilder.ToString();
	}
}
