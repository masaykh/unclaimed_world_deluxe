using Microsoft.Xna.Framework;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class CRTNoise
{
	public Animation2D turnOn;

	public Animation2D turnOff;

	public Animation2D switchChannel;

	public Animation2D switchChannelBlackFrame;

	public Animation2D interference;

	public Animation2D NoReception;

	public CRTNoise(GUIManager gui)
	{
		turnOn = new Animation2D(gui.GUI_CRT_SpriteSheet.Texture, 0.17f, isLooping: false);
		turnOn.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("CRT-turnon-frame1")));
		turnOn.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("CRT-turnon-frame2")));
		turnOn.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("CRT-turnon-frame3")));
		turnOn.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("CRT-turnon-frame3"))
		{
			Color = Animation2D.transp
		});
		turnOff = new Animation2D(gui.GUI_CRT_SpriteSheet.Texture, 0.1f, isLooping: false);
		turnOff.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("CRT-turnon-frame3"))
		{
			Color = Animation2D.halfTransp
		});
		turnOff.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("CRT-turnon-frame2"))
		{
			Color = Animation2D.halfTransp
		});
		turnOff.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("CRT-turnon-frame1")));
		turnOff.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("CRT-turnon-frame1"))
		{
			Color = Color.FromNonPremultiplied(new Vector4(0.5f, 0.5f, 0.5f, 1f))
		});
		turnOff.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("CRT-turnon-frame1"))
		{
			Color = Animation2D.black
		});
		switchChannel = new Animation2D(gui.GUI_CRT_SpriteSheet.Texture, 0.08f, isLooping: false);
		switchChannelBlackFrame = new Animation2D(gui.GUI_CRT_SpriteSheet.Texture, 0.08f, isLooping: false);
		switchChannelBlackFrame.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame1"))
		{
			Color = Color.FromNonPremultiplied(new Vector4(0f, 0f, 0f, 1f))
		});
		switchChannel.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame1")));
		switchChannel.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame2")));
		switchChannel.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame3")));
		switchChannel.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame4"))
		{
			Color = new Color(0.67f, 0.67f, 0.67f, 0.67f)
		});
		switchChannel.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame5"))
		{
			Color = new Color(0.5f, 0.5f, 0.5f, 0.5f)
		});
		switchChannel.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame6"))
		{
			Color = new Color(0.27f, 0.27f, 0.27f, 0.27f)
		});
		switchChannel.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame6"))
		{
			Color = new Color(0f, 0f, 0f, 0f)
		});
		interference = new Animation2D(gui.GUI_CRT_SpriteSheet.Texture, 0.08f, isLooping: false);
		interference.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame4"))
		{
			Color = new Color(0.67f, 0.67f, 0.67f, 0.67f)
		});
		interference.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame5"))
		{
			Color = new Color(0.5f, 0.5f, 0.5f, 0.5f)
		});
		interference.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame6"))
		{
			Color = new Color(0.27f, 0.27f, 0.27f, 0.27f)
		});
		interference.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame6"))
		{
			Color = new Color(0f, 0f, 0f, 0f)
		});
		NoReception = new Animation2D(gui.GUI_CRT_SpriteSheet.Texture, 0.05f, isLooping: true);
		NoReception.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame1")));
		NoReception.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame1")));
		NoReception.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame2")));
		NoReception.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame2")));
		NoReception.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame3")));
		NoReception.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame3")));
		NoReception.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame4")));
		NoReception.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame4")));
		NoReception.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame5")));
		NoReception.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame5")));
		NoReception.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame6")));
		NoReception.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame6")));
		NoReception.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame7")));
		NoReception.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame7")));
		NoReception.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame7"))
		{
			Color = Color.FromNonPremultiplied(new Vector4(0f, 0f, 0f, 1f))
		});
	}
}
