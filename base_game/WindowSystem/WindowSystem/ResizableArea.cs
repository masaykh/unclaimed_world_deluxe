using InputEventSystem;
using Microsoft.Xna.Framework;

namespace WindowSystem;

public class ResizableArea : UIComponent
{
	private bool isDragging;

	private Point lastLocation;

	private ResizeAreas resizeArea;

	public ResizeAreas ResizeArea
	{
		get
		{
			return resizeArea;
		}
		set
		{
			resizeArea = value;
		}
	}

	public event StartResizingHandler StartResizing;

	public event EndResizingHandler EndResizing;

	public ResizableArea(GUIManager guiManager)
		: base(guiManager)
	{
		isDragging = false;
		lastLocation = Point.Zero;
		ResizeArea = ResizeAreas.BottomRight;
		ToolTip = "Drag to resize.";
	}

	private void ShowResizeCursor()
	{
		switch (resizeArea)
		{
		case ResizeAreas.Top:
		case ResizeAreas.Bottom:
			base.GUIManager.SetMouseCursor(MouseSprites.ResizingNS);
			break;
		case ResizeAreas.Left:
		case ResizeAreas.Right:
			base.GUIManager.SetMouseCursor(MouseSprites.ResizingWE);
			break;
		case ResizeAreas.TopLeft:
		case ResizeAreas.BottomRight:
			base.GUIManager.SetMouseCursor(MouseSprites.ResizingNWSE);
			break;
		case ResizeAreas.TopRight:
		case ResizeAreas.BottomLeft:
			base.GUIManager.SetMouseCursor(MouseSprites.ResizingNESW);
			break;
		}
	}

	protected override void OnMouseOver(UIComponent sender, MouseEventArgs args)
	{
		base.OnMouseOver(sender, args);
		ShowResizeCursor();
	}

	protected override void OnMouseOut(UIComponent sender, MouseEventArgs args)
	{
		base.OnMouseOut(sender, args);
		base.GUIManager.SetMouseCursor(MouseSprites.Normal);
	}

	/// <summary>
	/// The pointer did not move - this area was hidden, moved or resized out from under it. The
	/// resize cursor has to be given back either way, and this is the only thing a ResizableArea
	/// does that outlives it being on screen.
	/// </summary>
	protected override void OnHoverReleased()
	{
		base.GUIManager.SetMouseCursor(MouseSprites.Normal);
	}

	protected override void OnMouseDown(MouseEventArgs args)
	{
		if (args.Button == MouseButtons.Left)
		{
			isDragging = true;
			lastLocation = args.Position;
			if (this.StartResizing != null)
			{
				this.StartResizing(this);
			}
		}
	}

	protected override void OnLoseFocus()
	{
		base.OnLoseFocus();
		if (isDragging)
		{
			isDragging = false;
			if (this.EndResizing != null)
			{
				this.EndResizing(this);
			}
		}
		base.GUIManager.SetMouseCursor(MouseSprites.Normal);
	}

	protected override void OnMouseUp(MouseEventArgs args)
	{
		if (args.Button == MouseButtons.Left)
		{
			isDragging = false;
			if (this.EndResizing != null)
			{
				this.EndResizing(this);
			}
		}
	}

	protected override void OnMouseMove(MouseEventArgs args)
	{
		if (isDragging && Parent != null)
		{
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			if (resizeArea == ResizeAreas.Top || resizeArea == ResizeAreas.TopLeft || resizeArea == ResizeAreas.TopRight)
			{
				flag = true;
			}
			else if (resizeArea == ResizeAreas.Bottom || resizeArea == ResizeAreas.BottomLeft || resizeArea == ResizeAreas.BottomRight)
			{
				flag2 = true;
			}
			if (resizeArea == ResizeAreas.Left || resizeArea == ResizeAreas.BottomLeft || resizeArea == ResizeAreas.TopLeft)
			{
				flag3 = true;
			}
			else if (resizeArea == ResizeAreas.Right || resizeArea == ResizeAreas.BottomRight || resizeArea == ResizeAreas.TopRight)
			{
				flag4 = true;
			}
			int width = Parent.Width;
			int height = Parent.Height;
			if (flag3)
			{
				Parent.Width -= args.Position.X - lastLocation.X;
				Parent.X += width - Parent.Width;
				lastLocation.X += width - Parent.Width;
			}
			else if (flag4)
			{
				Parent.Width += args.Position.X - lastLocation.X;
				lastLocation.X += Parent.Width - width;
			}
			if (flag)
			{
				Parent.Height -= args.Position.Y - lastLocation.Y;
				Parent.Y += height - Parent.Height;
				lastLocation.Y += height - Parent.Height;
			}
			else if (flag2)
			{
				Parent.Height += args.Position.Y - lastLocation.Y;
				lastLocation.Y += Parent.Height - height;
			}
		}
	}
}
