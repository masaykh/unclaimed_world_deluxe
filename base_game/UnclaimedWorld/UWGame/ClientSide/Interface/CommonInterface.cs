using System;
using System.IO;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.Control;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public abstract class CommonInterface
{
	public enum AnchorSide
	{
		Left,
		Right
	}


	public UnclaimedWorld Game;

	public GUIManager gui;

	// PORT DEVIATION 9 (see PORTING-NOTES.md). Was `private Form windowForm;`, whose ONLY
	// use in this class was assigning WinForms Cursors. MonoGame's MouseCursor/Mouse.SetCursor
	// replaces it, so the last WinForms dependency here is gone. We cache the applied cursor
	// to preserve the original "only assign when it actually changed" behaviour.
	private MouseCursor currentCursor;

	public DisplayPanelRenderer DisplayPanelRenderer;

	public const int framedCRTWidth = 533;

	public const int framedCRTHeight = 400;

	private Microsoft.Xna.Framework.Rectangle upperLeftQuadrant;

	private Microsoft.Xna.Framework.Rectangle upperRightQuadrant;

	private Microsoft.Xna.Framework.Rectangle lowerLeftQuadrant;

	private Microsoft.Xna.Framework.Rectangle lowerRightQuadrant;

	protected MouseCursor fingerCursor;

	protected MouseCursor fingerUpDown;

	protected MouseCursor fingerLeftRight;

	protected MouseCursor fingerUpRight;

	protected MouseCursor fingerDownRight;

	protected MouseCursor fingerMove;

	protected MouseCursor worldCursor;

	protected MouseCursor worldUpDown;

	protected MouseCursor worldLeftRight;

	protected MouseCursor worldUpRight;

	protected MouseCursor worldDownRight;

	protected MouseCursor worldMove;

	protected Tooltip Tooltip;

	private const int tooltipOverlap = 8;

	public CommonInterface(UnclaimedWorld game, bool addGuiManagerNow = true, bool addTooltip = false)
	{
		Game = game;
		gui = new GUIManager(game, game.Controller.DrawArea.Width, game.Controller.DrawArea.Height, game.Controller.InputData, game.Controller.Content, addGuiManagerNow);
		gui.SetMouseCursorEvent += gui_SetMouseCursorEvent;
		worldCursor = CreateCursor("Content/GUI/Cursors/HUD_cursor.png", new Microsoft.Xna.Framework.Point(5, 5));
		worldDownRight = CreateCursor("Content/GUI/Cursors/HUD_cursor_scale_downright.png", new Microsoft.Xna.Framework.Point(15, 15));
		worldLeftRight = CreateCursor("Content/GUI/Cursors/HUD_cursor_scale_horiz.png", new Microsoft.Xna.Framework.Point(16, 15));
		worldUpRight = CreateCursor("Content/GUI/Cursors/HUD_cursor_scale_upright.png", new Microsoft.Xna.Framework.Point(15, 15));
		worldUpDown = CreateCursor("Content/GUI/Cursors/HUD_cursor_scale_vertical.png", new Microsoft.Xna.Framework.Point(16, 16));
		worldMove = CreateCursor("Content/GUI/Cursors/HUD_cursor_move.png", new Microsoft.Xna.Framework.Point(16, 15));
		DisplayPanelRenderer = new DisplayPanelRenderer(game);
		DisplayPanelRenderer.Initialize();
		if (addTooltip)
		{
			Tooltip = new Tooltip(this);
		}
	}

	private void InitScreenQuadrants()
	{
		Dimension drawArea = ((UnclaimedWorld)gui.Game).Controller.DrawArea;
		int num = drawArea.Width / 2;
		int num2 = drawArea.Height / 2;
		upperLeftQuadrant = new Microsoft.Xna.Framework.Rectangle(0, 0, num, num2);
		upperRightQuadrant = new Microsoft.Xna.Framework.Rectangle(num, 0, num, num2);
		lowerLeftQuadrant = new Microsoft.Xna.Framework.Rectangle(0, num2, num, num2);
		lowerRightQuadrant = new Microsoft.Xna.Framework.Rectangle(num, num2, num, num2);
	}

	public void SelectAnchorPoint(UIComponent control, Window windowToAnchor, AnchorSide? sideToAnchorOn, int heightToUse, bool doOverlap, int yOffset, out int x, out int y, int? overlapToUse = null)
	{
		int x2 = control.AbsolutePosition.X;
		int num = control.AbsolutePosition.X + control.Width;
		int y2 = control.AbsolutePosition.Y;
		int y3 = control.AbsolutePosition.Y + control.Height;
		overlapToUse = ((!doOverlap) ? new int?(overlapToUse ?? 0) : new int?(overlapToUse ?? 8));
		AnchorSide anchorSide;
		if (upperLeftQuadrant.Contains(num, y2))
		{
			anchorSide = AnchorSide.Right;
			y = y2 + yOffset;
		}
		else if (upperRightQuadrant.Contains(x2, y2))
		{
			anchorSide = AnchorSide.Left;
			y = y2 + yOffset;
		}
		else if (lowerLeftQuadrant.Contains(num, y3))
		{
			anchorSide = AnchorSide.Right;
			y = y2 + yOffset;
		}
		else if (lowerRightQuadrant.Contains(x2, y3))
		{
			anchorSide = AnchorSide.Left;
			y = y2 + yOffset;
		}
		else
		{
			anchorSide = AnchorSide.Right;
			y = y2 + yOffset;
		}
		if (sideToAnchorOn.HasValue)
		{
			anchorSide = sideToAnchorOn.Value;
		}
		if (anchorSide == AnchorSide.Left)
		{
			x = x2 - windowToAnchor.Width + overlapToUse.Value;
		}
		else
		{
			x = num - overlapToUse.Value;
		}
		y = Common.ClampTop(y, lowerLeftQuadrant.Bottom - heightToUse - 36);
	}

	public virtual void Destroy()
	{
		gui.SetMouseCursorEvent -= gui_SetMouseCursorEvent;
		gui.Destroy();
		gui = null;
		if (Tooltip != null)
		{
			Tooltip.Destroy();
		}
		DisplayPanelRenderer.Destroy();
	}

	private void gui_SetMouseCursorEvent(MouseSprites mouseSprite)
	{
		SetCursor(gui.MouseSprite);
	}

	public virtual void LoadContent()
	{
		DisplayPanelRenderer.LoadContent();
		InitScreenQuadrants();
	}

	public virtual void UnloadContent()
	{
		worldCursor.Dispose();
		worldDownRight.Dispose();
		worldLeftRight.Dispose();
		worldUpRight.Dispose();
		worldUpDown.Dispose();
		worldMove.Dispose();
	}

	public void SetInterfaceCursor()
	{
		ApplyCursor(worldCursor);
	}

	public void SetCursor(MouseSprites sprite)
	{
		ApplyCursor(sprite switch
		{
			MouseSprites.Normal => worldCursor,
			MouseSprites.Moving => worldMove,
			MouseSprites.ResizingNESW => worldUpRight,
			MouseSprites.ResizingNWSE => worldDownRight,
			MouseSprites.ResizingWE => worldLeftRight,
			MouseSprites.ResizingNS => worldUpDown,
			_ => worldCursor,
		});
	}

	public void SetFingerCursor(MouseSprites sprite)
	{
		// NOTE: none of the finger* fields is ever assigned - not here and not in any subclass -
		// so every branch of this method has always applied a null cursor, i.e. the default
		// arrow. Preserved exactly (ApplyCursor maps null to MouseCursor.Arrow, which is what
		// assigning a null Form.Cursor did). Left in place rather than deleted because the
		// method is called from the interface code and the fields look like an unfinished
		// feature, not dead weight to silently discard.
		ApplyCursor(sprite switch
		{
			MouseSprites.Normal => fingerCursor,
			MouseSprites.Moving => fingerMove,
			MouseSprites.ResizingNESW => fingerUpRight,
			MouseSprites.ResizingNWSE => fingerDownRight,
			MouseSprites.ResizingWE => fingerLeftRight,
			MouseSprites.ResizingNS => fingerUpDown,
			_ => fingerCursor,
		});
	}

	/// <summary>
	/// PORT DEVIATION 9 (see PORTING-NOTES.md). Replaces `windowForm.Cursor = x`.
	/// A null cursor means "the default arrow", which is what assigning null to
	/// Form.Cursor used to do. The cached comparison keeps the original behaviour of only
	/// touching the cursor when it actually changes.
	/// </summary>
	private void ApplyCursor(MouseCursor cursor)
	{
		cursor ??= MouseCursor.Arrow;
		if (currentCursor == cursor)
		{
			return;
		}
		currentCursor = cursor;
		UWGame.Port.PlatformCursor.Set(cursor);
	}

	/// <summary>
	/// PORT DEVIATION 9 (see PORTING-NOTES.md).
	/// Was: load the PNG into a GDI+ Bitmap, GetHicon(), GetIconInfo(), overwrite the hotspot,
	/// CreateIconIndirect(), and wrap the resulting HICON in a WinForms Cursor - two user32
	/// P/Invokes plus a System.Drawing dependency. It also leaked both icon handles: nothing
	/// ever called DestroyIcon on the bitmap's HICON or on the one CreateIconIndirect returned.
	///
	/// MonoGame does this natively and takes the hotspot directly, so the whole thing collapses
	/// to two calls, works on DesktopGL as well as WindowsDX, and leaks nothing.
	/// </summary>
	private MouseCursor CreateCursor(string cursorImage, Microsoft.Xna.Framework.Point hotspot)
	{
		using FileStream stream = File.OpenRead(cursorImage);
		Texture2D texture = PadToSquare(Texture2D.FromStream(Game.GraphicsDevice, stream));
		return MouseCursor.FromTexture2D(texture, hotspot.X, hotspot.Y);
	}

	/// <summary>
	/// Works around a bug in MonoGame's WindowsDX MouseCursor implementation, still present as
	/// of 3.8.5.1.
	/// <c>MouseCursor.PlatformFromTexture2D</c> builds its GDI+ bitmap with
	/// <c>new Bitmap(w, h, h * 4, PixelFormat.Format32bppArgb, ...)</c> - but that third
	/// argument is the STRIDE, i.e. bytes per row, which is <c>w * 4</c>, not <c>h * 4</c>.
	/// The two only coincide when the image is square, so every non-square cursor comes out
	/// skewed and garbled. All six of this game's cursors are non-square (25x27 and 32x31),
	/// so all six were affected.
	///
	/// Padding the image out to a square with transparent pixels makes <c>w == h</c>, which
	/// makes MonoGame's stride arithmetic accidentally correct. The padding is added on the
	/// right and bottom only, and hotspots are measured from the top-left, so cursor hotspots
	/// are unaffected. It is also harmless on the SDL/DesktopGL path, which passes an explicit
	/// pitch and was never affected.
	///
	/// Remove this once MonoGame fixes the stride, or if the cursor art is ever re-exported
	/// square (32x32 is the conventional Windows cursor size anyway).
	/// </summary>
	private Texture2D PadToSquare(Texture2D texture)
	{
		int side = Math.Max(texture.Width, texture.Height);
		if (texture.Width == side && texture.Height == side)
		{
			return texture;
		}

		Color[] source = new Color[texture.Width * texture.Height];
		texture.GetData(source);

		// Color default is (0,0,0,0), so the padding is fully transparent.
		Color[] padded = new Color[side * side];
		for (int y = 0; y < texture.Height; y++)
		{
			Array.Copy(source, y * texture.Width, padded, y * side, texture.Width);
		}

		Texture2D square = new Texture2D(Game.GraphicsDevice, side, side, false, SurfaceFormat.Color);
		square.SetData(padded);
		texture.Dispose();
		return square;
	}

	public void Draw(GameTime gameTime)
	{
		for (Level level = Level.Min; level < Level.Max; level++)
		{
			DrawWindowLevel(gameTime, level);
		}
	}

	public virtual void Update(GameTime gameTime)
	{
		gui.Update(gameTime);
		if (Tooltip != null)
		{
			Tooltip.Update(gameTime);
		}
	}

	private void DrawWindowLevel(GameTime gameTime, Level levelToDraw)
	{
		gui.Draw(gameTime, RenderType.Normal, levelToDraw);
		if (Game.Controller.GraphicsLevelSetting == Controller.GraphicsLevel.High)
		{
			DisplayPanelRenderer.StartPanelRendering();
			gui.Draw(gameTime, RenderType.CRTAndLCD, levelToDraw);
			DisplayPanelRenderer.Draw(levelToDraw);
		}
		else
		{
			gui.Draw(gameTime, RenderType.CRTAndLCD, levelToDraw);
		}
		gui.Draw(gameTime, RenderType.Overlay, levelToDraw);
	}
}
