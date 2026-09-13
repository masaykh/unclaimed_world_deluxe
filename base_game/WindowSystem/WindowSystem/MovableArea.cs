using InputEventSystem;
using Microsoft.Xna.Framework;

namespace WindowSystem;

public class MovableArea : UIComponent
{
	private bool isDragging;

	private Point lastLocation;

	public event StartMovingHandler StartMoving;

	public event EndMovingHandler EndMoving;

	public MovableArea(GUIManager guiManager)
		: base(guiManager)
	{
		isDragging = false;
		lastLocation = Point.Zero;
	}

	protected override void OnMouseDown(MouseEventArgs args)
	{
		if (args.Button == MouseButtons.Left)
		{
			isDragging = true;
			lastLocation = args.Position;
			GetParentWindow().BringToTop();
			base.GUIManager.SetMouseCursor(MouseSprites.Moving);
			if (this.StartMoving != null)
			{
				this.StartMoving(this);
			}
		}
	}

	protected override void OnLoseFocus()
	{
		base.OnLoseFocus();
		if (isDragging)
		{
			isDragging = false;
			if (this.EndMoving != null)
			{
				this.EndMoving(this);
			}
		}
		base.GUIManager.SetMouseCursor(MouseSprites.Normal);
	}

	protected override void OnMouseUp(MouseEventArgs args)
	{
		if (args.Button == MouseButtons.Left)
		{
			isDragging = false;
			base.GUIManager.SetMouseCursor(MouseSprites.Normal);
			if (this.EndMoving != null)
			{
				this.EndMoving(this);
			}
		}
	}

	protected override void OnMouseMove(MouseEventArgs args)
	{
		if (isDragging)
		{
			int num = args.Position.X - lastLocation.X;
			int num2 = args.Position.Y - lastLocation.Y;
			if (Parent != null)
			{
				Parent.X += num;
				Parent.Y += num2;
			}
			lastLocation = args.Position;
		}
	}
}
