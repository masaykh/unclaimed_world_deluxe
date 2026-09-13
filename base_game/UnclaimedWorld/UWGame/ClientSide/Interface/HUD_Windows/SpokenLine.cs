using Microsoft.Xna.Framework;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;
using WindowSystem;

namespace UWGame.ClientSide.Interface.HUD_Windows;

public class SpokenLine : HUDWindow
{
	public EntityID? Speaker;

	private TextArea textArea;

	public MarkerWindow Parent;

	public string Text
	{
		set
		{
			textArea.Text = value;
			textArea.X = 6;
			DisplayWindow.Width = textArea.Width + 12;
			DisplayWindow.Height = textArea.Height;
			DisplayWindow.CenterChildVertically(textArea);
		}
	}

	public void Init(Entity speaker, string text, MarkerWindow parent)
	{
		textArea.Width = DisplayWindow.Width;
		Parent = parent;
		Parent.ShowWhileLineIsSpoken = true;
		Speaker = speaker.EntityID;
		speaker.Intelligence.TalkActionEnded += Intelligence_TalkActionEnded;
		string text2 = "";
		string text3 = "";
		_ = speaker.EntityType.Person;
		Text = text2 + text3 + text;
	}

	private void Intelligence_TalkActionEnded()
	{
		The.InGameUI.RemoveSpokenLine(this);
	}

	public SpokenLine()
		: base(120, 24, hasSurface: true, hasCloseButton: false, isMovable: false, "HUD_windowCharacter_base")
	{
		textArea = new TextArea(gui, ListBoxType.HUDAndLCD);
		Add(textArea);
		textArea.Init(Label.LabelType.HUDWindow);
	}

	public void Show()
	{
		Entity speakingEntity;
		Point? point = UpdatePosition(out speakingEntity);
		if (point.HasValue)
		{
			base.ShowOnPlayfield(point.Value.X, point.Value.Y);
		}
		else
		{
			The.InGameUI.RemoveSpokenLine(this);
		}
	}

	private Point? UpdatePosition(out Entity speakingEntity)
	{
		Point? result = null;
		speakingEntity = null;
		if (The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(Speaker.Value, out var data) == EntityResult.SeenDirectly)
		{
			speakingEntity = (Entity)data;
			if (!speakingEntity.Location.HasValue)
			{
				return null;
			}
			Point screenPosition = Parent.GetScreenPosition();
			screenPosition.X += Parent.DisplayWindow.Width;
			result = screenPosition;
		}
		return result;
	}

	public override void Update(GameTime elapsed)
	{
		base.Update(elapsed);
		if (Speaker.HasValue)
		{
			Entity speakingEntity;
			Point? point = UpdatePosition(out speakingEntity);
			if (speakingEntity != null && point.HasValue)
			{
				SetScreenPosition(new Point(point.Value.X, point.Value.Y));
				SetWorldPosition(new Point(point.Value.X, point.Value.Y));
			}
			else
			{
				The.InGameUI.RemoveSpokenLine(this);
			}
		}
	}

	public override void Hide()
	{
		if (!Speaker.HasValue)
		{
			base.Hide();
		}
	}
}
