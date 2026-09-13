using Microsoft.Xna.Framework;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class CRTAnimator
{
	public UIComponent SurfacePanel;

	private AnimatedImage animationControl;

	private CRTScreen crtScreen;

	private Point absolutePosition;

	private Point relativePosition;

	private int width;

	private int height;

	private bool isOn;

	private bool microSwitchIsPlaying;

	private bool longShakeIsPlaying;

	public Animation2D switchChannelSmall;

	public Animation2D switchChannelBlackFrameSmall;

	public Animation2D interferenceSmall;

	public Animation2D microSwitch;

	public Animation2D NoReceptionSmall;

	private static float[] longShakeEdges = new float[9] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f };

	private static float[] longShakeYPositions = new float[9] { 0.25f, -0.1f, 0.2f, 0.1f, 0.3f, 0f, 0.2f, -0.1f, 0f };

	private bool shortShakeIsPlaying;

	private static float[] shortShakeEdges = new float[3] { 0.1f, 0.5f, 0.8f };

	private static float[] shortShakeYPositions = new float[3] { 0.25f, -0.15f, 0f };

	private Window DisplayWindow;

	public bool IsOn => isOn;

	public CRTAnimator(Window window, Point absolutePosition, Point relativePosition, int width, int height, ReflectionToUse reflection, float alpha = 1f)
	{
		this.absolutePosition = absolutePosition;
		this.relativePosition = relativePosition;
		this.width = width;
		this.height = height;
		DisplayWindow = window;
		InitDisplay(reflection, alpha);
		InitAnims();
	}

	private void InitDisplay(ReflectionToUse reflection, float alpha)
	{
		GUIManager gui = The.InGameUI.gui;
		_ = The.Sim.Controller.Game;
		SurfacePanel = new UIComponent(gui);
		DisplayWindow.Add(SurfacePanel);
		SetSurfacePanelPosition();
		SurfacePanel.Width = width;
		SurfacePanel.Height = height - 6;
		SurfacePanel.RenderType = RenderType.CRTAndLCD;
		SurfacePanel.DebugTag = "statusSurface";
		animationControl = new AnimatedImage(gui);
		DisplayWindow.Add(animationControl);
		animationControl.Texture = gui.GUI_CRT_SpriteSheet.Texture;
		animationControl.Position = SurfacePanel.Position;
		animationControl.ScaleImageToSizeOfControl = true;
		animationControl.Width = SurfacePanel.Width;
		animationControl.Height = SurfacePanel.Height;
		animationControl.RenderType = RenderType.CRTAndLCD;
		animationControl.DebugTag = "animationControl";
		crtScreen = The.InGameUI.DisplayPanelRenderer.AddCRT(SurfacePanel, new Point(absolutePosition.X + 4, absolutePosition.Y + 4), width - 8, height - 8, DisplayWindow.Level, DisplayWindow, reflection, isMonochrome: true, alpha);
	}

	private void InitAnims()
	{
		GUIManager gui = The.InGameUI.gui;
		switchChannelSmall = new Animation2D(gui.GUI_CRT_SpriteSheet.Texture, 0.08f, isLooping: false);
		switchChannelBlackFrameSmall = new Animation2D(gui.GUI_CRT_SpriteSheet.Texture, 0.08f, isLooping: false);
		switchChannelBlackFrameSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame1_small"))
		{
			Color = new Color(0f, 0f, 0f, 1f)
		});
		microSwitch = new Animation2D(gui.GUI_CRT_SpriteSheet.Texture, 0.05f, isLooping: false);
		microSwitch.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame5_small"))
		{
			Color = new Color(0.1f, 0.1f, 0.1f, 0.1f)
		});
		microSwitch.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame6_small"))
		{
			Color = new Color(0f, 0f, 0f, 0f)
		});
		switchChannelSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame1_small")));
		switchChannelSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame2_small")));
		switchChannelSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame3_small")));
		switchChannelSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame3_small"))
		{
			Color = new Color(0f, 0f, 0f, 1f)
		});
		switchChannelSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame4_small"))
		{
			Color = new Color(0.67f, 0.67f, 0.67f, 0.67f)
		});
		switchChannelSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame5_small"))
		{
			Color = new Color(0.5f, 0.5f, 0.5f, 0.5f)
		});
		switchChannelSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame6_small"))
		{
			Color = new Color(0.27f, 0.27f, 0.27f, 0.27f)
		});
		switchChannelSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame6_small"))
		{
			Color = new Color(0f, 0f, 0f, 0f)
		});
		interferenceSmall = new Animation2D(gui.GUI_CRT_SpriteSheet.Texture, 0.08f, isLooping: false);
		interferenceSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame4_small"))
		{
			Color = new Color(0.67f, 0.67f, 0.67f, 0.67f)
		});
		interferenceSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame5_small"))
		{
			Color = new Color(0.5f, 0.5f, 0.5f, 0.5f)
		});
		interferenceSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame6_small"))
		{
			Color = new Color(0.27f, 0.27f, 0.27f, 0.27f)
		});
		interferenceSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame6_small"))
		{
			Color = new Color(0f, 0f, 0f, 0f)
		});
		NoReceptionSmall = new Animation2D(gui.GUI_CRT_SpriteSheet.Texture, 0.05f, isLooping: true);
		NoReceptionSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame1_small")));
		NoReceptionSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame1_small")));
		NoReceptionSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame2_small")));
		NoReceptionSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame2_small")));
		NoReceptionSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame3_small")));
		NoReceptionSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame3_small")));
		NoReceptionSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame4_small")));
		NoReceptionSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame4_small")));
		NoReceptionSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame5_small")));
		NoReceptionSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame5_small")));
		NoReceptionSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame6_small")));
		NoReceptionSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame6_small")));
		NoReceptionSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame7_small")));
		NoReceptionSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame7_small")));
		NoReceptionSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame7_small"))
		{
			Color = new Color(0f, 0f, 0f, 1f)
		});
	}

	public void ChangeContent(UIComponent content)
	{
		if (SurfacePanel.Controls.Contains(content))
		{
			return;
		}
		for (int i = 0; i < SurfacePanel.Controls.Count; i++)
		{
			if (SurfacePanel.Controls[i] != animationControl)
			{
				SurfacePanel.Controls.Remove(SurfacePanel.Controls[i]);
				i--;
			}
		}
		SurfacePanel.Add(content);
	}

	public void Update(GameTime gameTime)
	{
		if (longShakeIsPlaying || shortShakeIsPlaying)
		{
			float progress = animationControl.Player.GetProgress();
			float num = 0f;
			if (longShakeIsPlaying)
			{
				int stairStepIndex = Common.GetStairStepIndex(progress, longShakeEdges);
				num = longShakeYPositions[stairStepIndex];
			}
			else if (shortShakeIsPlaying)
			{
				int stairStepIndex2 = Common.GetStairStepIndex(progress, shortShakeEdges);
				num = shortShakeYPositions[stairStepIndex2];
			}
			SetSurfaceYPosition((int)(num * (float)SurfacePanel.Height));
		}
	}

	private void SetSurfaceYPosition(int position)
	{
		SurfacePanel.Controls[0].Y = position;
	}

	private void SetSurfacePanelPosition()
	{
		SurfacePanel.Position = relativePosition;
	}

	public void TurnOn()
	{
		animationControl.Player.StartAnimation(The.InGameUI.framedCRT.CRTNoise.turnOn);
		animationControl.Player.AnimationEndedEvent += TurnOnAnimFinished;
	}

	public void TurnOff()
	{
		animationControl.Player.StartAnimation(The.InGameUI.framedCRT.CRTNoise.turnOff);
		animationControl.Player.AnimationEndedEvent += TurnOffAnimFinished;
	}

	private void TurnOffAnimFinished()
	{
		isOn = false;
		animationControl.Player.AnimationEndedEvent -= TurnOffAnimFinished;
	}

	private void TurnOnAnimFinished()
	{
		isOn = true;
		animationControl.Player.AnimationEndedEvent -= TurnOnAnimFinished;
	}

	public void Switch()
	{
		int num = The.Client.ClientRandomGenerator.Next(10, "CRTAnimator", saveMessage: false);
		if (num <= 1)
		{
			longShakeIsPlaying = true;
			The.InGameUI.gui.PlaySound(GUIManager.WhiteNoise);
		}
		else if (num <= 3)
		{
			shortShakeIsPlaying = true;
		}
		else
		{
			microSwitchIsPlaying = true;
		}
		animationControl.StartAnimation(switchChannelBlackFrameSmall);
		animationControl.Player.AnimationEndedEvent += SwitchChannelBlackFrameAnimationEnded;
	}

	private void SwitchChannelBlackFrameAnimationEnded()
	{
		animationControl.Player.AnimationEndedEvent -= SwitchChannelBlackFrameAnimationEnded;
		if (longShakeIsPlaying)
		{
			animationControl.StartAnimation(switchChannelSmall);
		}
		else if (shortShakeIsPlaying)
		{
			animationControl.StartAnimation(interferenceSmall);
		}
		else
		{
			animationControl.StartAnimation(microSwitch);
		}
		animationControl.Player.AnimationEndedEvent += SwitchChannelAnimationEnded;
	}

	private void SwitchChannelAnimationEnded()
	{
		SetSurfacePanelPosition();
		longShakeIsPlaying = false;
		shortShakeIsPlaying = false;
		microSwitchIsPlaying = false;
		SetSurfaceYPosition(0);
		animationControl.Player.AnimationEndedEvent -= SwitchChannelAnimationEnded;
	}
}
