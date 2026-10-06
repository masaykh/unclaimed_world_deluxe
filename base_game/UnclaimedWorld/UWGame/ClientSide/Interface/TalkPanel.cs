using System.Collections.Generic;
using InputEventSystem;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.ClientSide.Log;
using UWGame.SimSide.Entities;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class TalkPanel
{
	public HUDTalkPanel HUDTalkPanel;

	private Window plasticPanel;

	private Window crtWindow;

	private Image faceImage;

	private CRTAnimator crtAnimator;

	private UIComponent crtContent;
	private double timeLeftToShowFace;

	/// <summary>
	/// HudMod's TALK PANEL, "when someone speaks": seconds since the last line was spoken AND its
	/// speaker's portrait went off the screen. The clock stands still while a portrait shows
	/// (timeLeftToShowFace, about 6 s from ShowSpeaker) - Kastuk: "let the timer start after
	/// portrait of the last talker is disappeared" - so the panel lingers a full
	/// HudMod.TalkPanelLingerSeconds after the face has gone, not after the line arrived.
	/// </summary>
	private double secondsSinceLastLine = double.MaxValue;

	private bool isShown = true;

	/// <summary>
	/// The newest line this panel knows about. A hidden panel has to look for new lines itself:
	/// InGameInterface refreshes only visible HUD windows, so HUDTalkPanel.Refresh - the only
	/// caller of <see cref="ShowSpeaker"/> - never runs while the panel is hidden, and in "when
	/// someone speaks" mode it never came back (Kastuk).
	/// </summary>
	private uint? lastSeenLine;

	public TalkPanel()
	{
		int num = 248;
		int num2 = 13;
		int num3 = 160;
		plasticPanel = new Window(The.InGameUI.gui);
		plasticPanel.Skin = The.InGameUI.gui.GUISpriteSheet.GetSourceRectangle("twitterpanel_frame");
		plasticPanel.CornerSize = 31;
		plasticPanel.Margin = 0;
		plasticPanel.IsMovable = false;
		plasticPanel.Resizable = false;
		plasticPanel.HasCloseButton = false;
		plasticPanel.Position = new Point(0, The.Client.ScreenHeight / 2 - num3 - 50);
		plasticPanel.WindowSize = new Vector2(32f, 370f);
		plasticPanel.Level = Level.Bottom;
		plasticPanel.Show();
		crtWindow = new Window(The.InGameUI.gui);
		crtWindow.CornerSize = 7;
		crtWindow.Margin = 0;
		crtWindow.IsMovable = false;
		crtWindow.Resizable = false;
		crtWindow.HasCloseButton = false;
		crtWindow.Position = new Point(num2 + 3, plasticPanel.Y + 28);
		crtWindow.WindowSize = new Vector2(num - 6, num3);
		crtWindow.Level = Level.Bottom;
		crtWindow.HasCRTOrLCDComponents = true;
		crtWindow.Show();
		HUDTalkPanel = new HUDTalkPanel(num2, plasticPanel.Y + 184, num, num3);
		crtAnimator = new CRTAnimator(crtWindow, crtWindow.AbsolutePosition, Point.Zero, crtWindow.Width, crtWindow.Height, ReflectionToUse.None, 0.8f);
		crtContent = new UIComponent(The.InGameUI.gui);
		crtContent.Width = crtAnimator.SurfacePanel.Width;
		crtContent.Height = crtAnimator.SurfacePanel.Height;
		crtContent.RenderType = RenderType.CRTAndLCD;
		faceImage = new Image(The.InGameUI.gui);
		faceImage.Texture = The.InGameUI.gui.GUI_CRT_SpriteSheet.Texture;
		faceImage.RenderType = RenderType.CRTAndLCD;
		faceImage.ScaleImageToSizeOfControl = true;
		faceImage.Width = num;
		faceImage.Height = num3;
		faceImage.X = 0;
		faceImage.Y = 0;
		crtAnimator.ChangeContent(crtContent);
	}

	public void Update(GameTime gameTime)
	{
		// MOD: HudMod's TALK PANEL - always (the studio's), when someone speaks, or hidden.
		string mode = UWGame.Mods.HudMod.TalkPanelMode();
		if (secondsSinceLastLine < double.MaxValue && timeLeftToShowFace <= 0.0)
		{
			secondsSinceLastLine += gameTime.ElapsedGameTime.TotalSeconds;
		}
		List<TalkEvent> lines = The.Client.Log?.TalkEvents;
		uint? newest = (lines != null && lines.Count > 0) ? lines[lines.Count - 1].ID : null;
		if (lastSeenLine == null)
		{
			lastSeenLine = newest ?? 0;   // what was said before this panel existed does not count
		}
		else if (newest > lastSeenLine)
		{
			lastSeenLine = newest;
			secondsSinceLastLine = 0.0;
		}
		// Kastuk: "do not hide, when cursor is over it. Restart time of hiding, when cursor is
		// away." The clock is held at 0 while the pointer is on the panel, so leaving it starts a
		// full HudMod.TalkPanelLingerSeconds.
		if (isShown && mode == UWGame.Mods.HudMod.TalkWhenSpoken && secondsSinceLastLine < double.MaxValue && PointerIsOver())
		{
			secondsSinceLastLine = 0.0;
		}
		bool wanted = mode == UWGame.Mods.HudMod.TalkAlways
			|| (mode == UWGame.Mods.HudMod.TalkWhenSpoken && secondsSinceLastLine < UWGame.Mods.HudMod.TalkPanelLingerSeconds);
		if (wanted != isShown)
		{
			if (wanted)
			{
				Show();
			}
			else
			{
				Hide();
			}
		}
		crtAnimator.Update(gameTime);
		if (timeLeftToShowFace > 0.0)
		{
			timeLeftToShowFace -= gameTime.ElapsedGameTime.TotalSeconds;
			if (timeLeftToShowFace <= 0.0)
			{
				timeLeftToShowFace = 0.0;
				crtAnimator.Switch();
				crtContent.Remove(faceImage);
			}
		}
	}

	/// <summary>Whether the pointer is on any of the panel's three windows.</summary>
	private bool PointerIsOver()
	{
		InputData input = The.InGameUI.gui.InputData;
		if (input == null)
		{
			return false;
		}
		Point pointer = new Point(input.mouseX, input.mouseY);
		return Contains(plasticPanel, pointer) || Contains(crtWindow, pointer) || Contains(HUDTalkPanel.DisplayWindow, pointer);
	}

	private static bool Contains(Window window, Point pointer)
	{
		return window != null && new Rectangle(window.AbsolutePosition.X, window.AbsolutePosition.Y, window.Width, window.Height).Contains(pointer);
	}

	public void ShowSpeaker(TalkEvent talkEvent)
	{
		Entity entity = Entity.FindByID(talkEvent.SpokenBy);
		if (entity != null)
		{
			crtAnimator.Switch();
			faceImage.SetSkinLocation(SkinState.Normal, entity.PersonEntity.GetPortraitForTalkDisplay(The.InGameUI.gui), null, null, flipHorizontally: true);
			crtContent.Add(faceImage);
			timeLeftToShowFace = The.Client.ClientRandomGenerator.RandomNormalDistribution(6.0, 0.4);
		}
		secondsSinceLastLine = 0.0;
	}

	public void Hide()
	{
		plasticPanel.Hide();
		HUDTalkPanel.Hide();
		crtWindow.Hide();
		isShown = false;
	}

	/// <summary>The studio wrote Hide and nothing to undo it; HudMod's TALK PANEL needs both.</summary>
	public void Show()
	{
		plasticPanel.Show();
		HUDTalkPanel.DisplayWindow.Show();
		crtWindow.Show();
		isShown = true;
	}
}
