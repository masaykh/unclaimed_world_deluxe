using System;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Map;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class FramedCRT
{
	public enum State
	{
		Off,
		TurningOn,
		SwitchingOff,
		On,
		ChangingChannel
	}

	private State state;

	public CRTScreen CRTScreen;

	private Box frame;

	private Box frameDropShadow;

	public Window DisplayWindow;

	public Window cablesWindow;

	public CRTTextCharAnimator crtTextAnimatorCharacter;

	public CRTTextLineAnimator crtTextAnimatorLine;

	private UIComponent SurfacePanel;

	public Rectangle SurfaceRect;

	private AnimatedImage animationControl;

	private int edgeWidth = 37;

	public Image backgroundNoise;

	private float timeBetweenInterference;

	private float timeBetweenInterferencePassed;

	public CRTNoise CRTNoise;

	private CommonInterface intf;

	public bool ShowCables = true;

	public FramedCRT(CommonInterface intf, Rectangle dimension, Rectangle source, Level level)
	{
		this.intf = intf;
		GUIManager gui = intf.gui;
		CRTNoise = new CRTNoise(intf.gui);
		SurfaceRect = new Rectangle(source.X + edgeWidth, source.Y + edgeWidth, source.Width - 2 * edgeWidth, source.Height - 2 * edgeWidth);
		Rectangle destination = new Rectangle(dimension.X + edgeWidth, dimension.Y + edgeWidth, dimension.Width - 2 * edgeWidth, dimension.Height - 2 * edgeWidth);
		DisplayWindow = new Window(gui);
		DisplayWindow.Skin = gui.GUISpriteSheet.GetSourceRectangle("CRT_Frame_Scalable");
		DisplayWindow.CornerSize = 60;
		DisplayWindow.Margin = 0;
		DisplayWindow.Resizable = false;
		DisplayWindow.IsMovable = false;
		DisplayWindow.Level = level;
		DisplayWindow.Position = new Point(dimension.X, dimension.Y);
		DisplayWindow.WindowSize = new Vector2(dimension.Width, dimension.Height);
		DisplayWindow.HasCloseButton = false;
		DisplayWindow.HasCRTOrLCDComponents = true;
		DisplayWindow.HasOverlayComponents = true;
		DisplayWindow.ShowPanel = false;
		DisplayWindow.DebugTag = "crtWindow";
		_ = edgeWidth;
		_ = edgeWidth;
		SurfacePanel = new UIComponent(intf.gui);
		DisplayWindow.Add(SurfacePanel);
		SurfacePanel.ClipThis = false;
		SurfacePanel.Position = new Point(SurfaceRect.X - DisplayWindow.AbsolutePosition.X, SurfaceRect.Y - DisplayWindow.AbsolutePosition.Y);
		SurfacePanel.Width = SurfaceRect.Width;
		SurfacePanel.Height = SurfaceRect.Height;
		SurfacePanel.RenderType = RenderType.CRTAndLCD;
		animationControl = new AnimatedImage(intf.gui);
		DisplayWindow.Add(animationControl);
		animationControl.Texture = intf.gui.GUI_CRT_SpriteSheet.Texture;
		animationControl.RenderType = RenderType.CRTAndLCD;
		animationControl.ClipThis = false;
		animationControl.ScaleImageToSizeOfControl = true;
		animationControl.Width = SurfacePanel.Width;
		animationControl.Height = SurfacePanel.Height;
		animationControl.Position = SurfacePanel.Position;
		frame = new Box(gui);
		frame.RenderType = RenderType.Overlay;
		Rectangle sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("CRT_Frame_Scalable_NoLight");
		frame.SetSkinLocation(SkinState.Normal, sourceRectangle);
		frame.CornerSize = 60;
		frame.Position = new Point(0, 0);
		frame.Width = dimension.Width;
		frame.Height = dimension.Height;
		DisplayWindow.Add(frame);
		Panel.AddDustOnFrame(intf.gui, new Rectangle(frame.X, frame.Y, frame.Width, frame.Height), 37, DisplayWindow, RenderType.Overlay);
		DisplayWindow.Hide();
		CRTScreen = intf.DisplayPanelRenderer.AddCRT(SurfacePanel, SurfaceRect, destination, DisplayWindow.Level, DisplayWindow, ReflectionToUse.Big, isMonochrome: true);
		crtTextAnimatorCharacter = new CRTTextCharAnimator(gui);
		DisplayWindow.Add(crtTextAnimatorCharacter);
		crtTextAnimatorLine = new CRTTextLineAnimator(gui);
		DisplayWindow.Add(crtTextAnimatorLine);
	}

	public static void ClearContent(UIComponent crtContent)
	{
		for (int i = 0; i < crtContent.Controls.Count; i++)
		{
			if (crtContent.Controls[i].Name != "Noise")
			{
				crtContent.Controls.RemoveAt(i);
				i--;
			}
		}
	}

	public UIComponent GetNewSurfaceContent()
	{
		UIComponent obj = new UIComponent(intf.gui)
		{
			Width = SurfaceRect.Width,
			Height = SurfaceRect.Height,
			RenderType = RenderType.CRTAndLCD
		};
		backgroundNoise = new Image(intf.gui);
		Rectangle sourceRectangle = intf.gui.GUI_CRT_SpriteSheet.GetSourceRectangle("EmptyBG_Dark");
		backgroundNoise.SetSkinLocation(SkinState.Normal, sourceRectangle);
		backgroundNoise.Texture = intf.gui.GUI_CRT_SpriteSheet.Texture;
		obj.Add(backgroundNoise);
		backgroundNoise.Position = Point.Zero;
		backgroundNoise.Width = SurfaceRect.Width;
		backgroundNoise.Height = SurfaceRect.Height;
		backgroundNoise.ScaleImageToSizeOfControl = true;
		backgroundNoise.RenderType = RenderType.CRTAndLCD;
		backgroundNoise.Name = "Noise";
		return obj;
	}

	public void ChangeContent(UIComponent content)
	{
		SurfacePanel.Controls.Clear();
		SurfacePanel.Add(content);
		crtTextAnimatorCharacter.Clear();
		crtTextAnimatorLine.Clear();
		AddLabelsToAnimator(content);
		crtTextAnimatorCharacter.StartAnimating();
		crtTextAnimatorLine.StartAnimating();
	}

	private void AddLabelsToAnimator(UIComponent control)
	{
		foreach (UIComponent control2 in control.Controls)
		{
			if (control2 is Label label)
			{
				if (label.AnimateOnCRTScreen == Label.AnimationMode.Character)
				{
					crtTextAnimatorCharacter.Add(label);
				}
				else if (label.AnimateOnCRTScreen == Label.AnimationMode.Line)
				{
					crtTextAnimatorLine.Add(label);
				}
			}
			AddLabelsToAnimator(control2);
		}
	}

	public void PlayInterference()
	{
		animationControl.StartAnimation(CRTNoise.interference);
	}

	public void PlayNoReception()
	{
		animationControl.StartAnimation(CRTNoise.NoReception);
	}

	public void Switch()
	{
		NoLightOnFrame();
		animationControl.StartAnimation(CRTNoise.switchChannelBlackFrame);
		animationControl.Player.AnimationEndedEvent += SwitchChannelBlackFrameAnimationEnded;
	}

	private void SwitchChannelBlackFrameAnimationEnded()
	{
		animationControl.Player.AnimationEndedEvent -= SwitchChannelBlackFrameAnimationEnded;
		LightOnFrame();
		if (The.Client.ClientRandomGenerator.Next(5, "FramedCRT", saveMessage: false) == 4)
		{
			animationControl.StartAnimation(CRTNoise.switchChannel);
		}
		else
		{
			animationControl.StartAnimation(CRTNoise.interference);
		}
		animationControl.Player.AnimationEndedEvent += SwitchChannelAnimationEnded;
	}

	private void SwitchChannelAnimationEnded()
	{
		animationControl.Player.AnimationEndedEvent -= SwitchChannelAnimationEnded;
	}

	public void Show()
	{
		DisplayWindow.Show();
		if (ShowCables)
		{
			cablesWindow.Show();
		}
		intf.gui.BringToBottom(cablesWindow);
		if (state == State.Off)
		{
			intf.gui.PlaySound(GUIManager.CRTTurnOn);
			animationControl.StartAnimation(CRTNoise.turnOn);
			animationControl.Player.AnimationEndedEvent += TurnOn;
		}
	}

	public void TurnOn()
	{
		state = State.On;
		LightOnFrame();
		animationControl.Player.AnimationEndedEvent -= TurnOn;
	}

	private void LightOnFrame()
	{
		Rectangle sourceRectangle = intf.gui.GUISpriteSheet.GetSourceRectangle("CRT_Frame_Scalable");
		frame.SetSkinLocation(SkinState.Normal, sourceRectangle);
	}

	public void TurnOff()
	{
		state = State.Off;
		NoLightOnFrame();
		animationControl.Player.AnimationEndedEvent -= TurnOff;
	}

	private void NoLightOnFrame()
	{
		Rectangle sourceRectangle = intf.gui.GUISpriteSheet.GetSourceRectangle("CRT_Frame_Scalable_NoLight");
		frame.SetSkinLocation(SkinState.Normal, sourceRectangle);
	}

	public void Hide()
	{
		DisplayWindow.Hide();
		cablesWindow.Hide();
		if (state == State.On)
		{
			animationControl.Player.StartAnimation(CRTNoise.turnOff);
			animationControl.Player.AnimationEndedEvent += TurnOff;
		}
	}

	public static void RotateModel(GameTime gameTime, Entity vehicleToShow, ref float rotation, Vector3 location)
	{
		rotation += 0.5f * (float)gameTime.ElapsedGameTime.TotalSeconds;
		rotation %= (float)Math.PI * 2f;
		vehicleToShow.SetRotationAndDir(rotation);
	}

	public static void DrawModel(Entity entityToShow, Matrix view)
	{
		ModelData modelData = entityToShow.Renderable.RenderAsModel.ModelData;
		entityToShow.Renderable.RenderAsModel.ComputeMatricesForDrawing(AnimatedModel.Transformations.OnlyRotation, modelData.CRTDisplayScale);
		entityToShow.Renderable.Draw(GameWorldRenderer.RenderTechnique.StandardMonochrome, ref view, ref The.Client.PerspectiveProjection, 1f, modelData.CRTDisplayLightIntensity);
	}
}
