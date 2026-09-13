using System;
using Microsoft.Xna.Framework;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class StatusScreen
{
	public Window DisplayWindow;

	private Image plastic;

	private Image fingerprint;

	private Box plasticEdge;

	public const int HeightOfStatusImage = 130;

	private CRTAnimator crtAnimator;

	private Window centerWindow;

	private ImageButton btTrack;

	private int crtHeight = 214;

	private int crtWidth = 277;

	public const int CRTPlasticEdgeCenter = 11;

	public bool IsOn => crtAnimator.IsOn;

	public StatusScreen()
	{
		GUIManager gui = The.InGameUI.gui;
		_ = The.Sim.Controller.Game;
		_ = The.InGameUI;
		int num = 41;
		centerWindow = new Window(The.InGameUI.gui);
		centerWindow.Position = new Point(The.Client.Controller.DrawArea.Width - num, 235);
		centerWindow.WindowSize = new Vector2(num, 33f);
		centerWindow.Level = Level.Middle;
		centerWindow.IsMovable = false;
		centerWindow.Resizable = false;
		centerWindow.Margin = 0;
		centerWindow.HasCloseButton = false;
		centerWindow.Skin = The.InGameUI.gui.GUISpriteSheet.GetSourceRectangle("centerentitybutton_basepatch");
		centerWindow.CornerSize = 5;
		centerWindow.Hide();
		btTrack = new ImageButton(The.InGameUI.gui);
		centerWindow.Add(btTrack);
		btTrack.Position = new Point(3, 1);
		btTrack.Init(ImageButtonType.CenterOnEntity);
		btTrack.Click += center_Click;
		btTrack.RightClick += center_RightClick;
		btTrack.ZOrder = 1f;
		btTrack.ToolTip = "Left click to center on the selected entity. Right click to track the entity.";
		DisplayWindow = new Window(gui);
		DisplayWindow.Skin = gui.GUISpriteSheet.GetSourceRectangle("TV_panel");
		DisplayWindow.CornerSize = 7;
		DisplayWindow.Margin = 0;
		DisplayWindow.Resizable = false;
		DisplayWindow.IsMovable = false;
		DisplayWindow.Position = new Point(The.InGameUI.MainLeft, 0);
		DisplayWindow.WindowSize = new Vector2(303f, 246f);
		DisplayWindow.HasCloseButton = false;
		DisplayWindow.HasCRTOrLCDComponents = true;
		DisplayWindow.HasOverlayComponents = true;
		DisplayWindow.Level = Level.Bottom;
		DisplayWindow.Show();
		DisplayWindow.DebugTag = "tvpanel";
		Point point = new Point(20, 20);
		AddCRTPlasticFrame(gui, DisplayWindow, point, crtWidth, crtHeight, out plasticEdge);
		Image image = Panel.AddDust(gui, DisplayWindow);
		image.RenderType = RenderType.Overlay;
		image.DebugTag = "event_dust";
		crtAnimator = new CRTAnimator(DisplayWindow, point, point, crtWidth, crtHeight, ReflectionToUse.Small);
	}

	public UIComponent GetNewSurfaceContent()
	{
		GUIManager gui = The.InGameUI.gui;
		UIComponent uIComponent = new UIComponent(The.InGameUI.gui);
		uIComponent.Width = crtAnimator.SurfacePanel.Width;
		uIComponent.Height = crtAnimator.SurfacePanel.Height;
		uIComponent.RenderType = RenderType.CRTAndLCD;
		Image image = new Image(The.InGameUI.gui);
		Rectangle sourceRectangle = gui.GUI_CRT_SpriteSheet.GetSourceRectangle("EmptyBG_Dark");
		image.SetSkinLocation(SkinState.Normal, sourceRectangle);
		image.Texture = gui.GUI_CRT_SpriteSheet.Texture;
		uIComponent.Add(image);
		image.Position = new Point(0, 0);
		image.Width = uIComponent.Width;
		image.Height = uIComponent.Height;
		image.ScaleImageToSizeOfControl = true;
		image.RenderType = RenderType.CRTAndLCD;
		return uIComponent;
	}

	public void ChangeContent(UIComponent content)
	{
		crtAnimator.ChangeContent(content);
	}

	public void Update(GameTime gameTime)
	{
		crtAnimator.Update(gameTime);
	}

	public void Refresh()
	{
	}

	private void center_Click(UIComponent sender, EventArgs e)
	{
		The.InGameUI.EnableTracking(enable: false);
		if (The.InGameUI.SelectedEntity.HasValue)
		{
			The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(The.InGameUI.SelectedEntity.Value, out var data);
			if (data != null)
			{
				The.InGameUI.ZoomToEntity(data);
			}
		}
	}

	private void center_RightClick(UIComponent sender, EventArgs e)
	{
		if (The.InGameUI.SelectedEntity.HasValue)
		{
			The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(The.InGameUI.SelectedEntity.Value, out var data);
			if (data != null)
			{
				The.InGameUI.ZoomToEntity(data);
				The.InGameUI.EnableTracking(!The.InGameUI.TrackSelectedEntity);
			}
		}
	}

	public static void AddCRTPlasticFrame(GUIManager gui, Window form, Point crtPos, int crtWidth, int crtHeight, out Box plasticEdge)
	{
		plasticEdge = new Box(gui);
		Rectangle sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("TV_plastic_edge");
		plasticEdge.SetSkinLocation(SkinState.Normal, sourceRectangle);
		plasticEdge.CornerSize = 30;
		plasticEdge.Position = new Point(crtPos.X - 11, crtPos.Y - 11);
		plasticEdge.Width = crtWidth + 22;
		plasticEdge.Height = crtHeight + 22;
		plasticEdge.RenderType = RenderType.Overlay;
		form.Add(plasticEdge);
		plasticEdge.DebugTag = "TV_plastic_edge";
	}

	public static int GetPlasticFrameHeight()
	{
		return 22;
	}

	public static int GetPlasticFrameWidth()
	{
		return 22;
	}

	public static void action_Click(UIComponent sender, EventArgs e)
	{
		if (The.InGameUI.SelectedEntity.HasValue)
		{
			The.InGameUI.HUDActionPanel.ShowInScreenSpace(sender.AbsolutePosition.X - The.InGameUI.HUDActionPanel.DisplayWindow.Width, sender.AbsolutePosition.Y);
		}
	}

	public void ShowCenterButton(bool show)
	{
		btTrack.Visible = show;
	}

	public void Show()
	{
		centerWindow.Show();
		DisplayWindow.Show();
	}

	public void Hide()
	{
		centerWindow.Hide();
		DisplayWindow.Hide();
	}

	public void TurnOn()
	{
		The.InGameUI.gui.PlaySound(GUIManager.CRTTurnOn);
		crtAnimator.TurnOn();
	}

	public void TurnOff()
	{
		crtAnimator.TurnOff();
	}

	public void Switch()
	{
		crtAnimator.Switch();
	}

	private void DrawRubber(GUIManager gui, Game game)
	{
		int i = 0;
		int num = 0;
		for (Rectangle sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("TV_rubber_texture"); i < DisplayWindow.Width; i += sourceRectangle.Width)
		{
			for (num = 0; num < DisplayWindow.Height; num += sourceRectangle.Height)
			{
				Image image = new Image(gui);
				image.SetSkinLocation(SkinState.Normal, sourceRectangle);
				image.Alpha = 0.2f;
				DisplayWindow.Add(image);
				image.Position = new Point(i, num);
				image.ResizeControlToFitImage();
			}
		}
	}

	public void SetCenterButtonChecked(bool enable)
	{
		btTrack.IsChecked = enable;
	}
}
