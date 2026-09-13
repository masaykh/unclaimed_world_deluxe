using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WindowSystem;

namespace UWGame.ClientSide;

public abstract class QuadBase
{
	public Animation2DPlayer Player = new Animation2DPlayer();

	public Rectangle StaticSourceRectangle => Player.GetFrame();

	public Texture2D Texture
	{
		get
		{
			if (Player != null && Player.Animation != null)
			{
				return Player.Animation.Texture;
			}
			return null;
		}
	}

	public bool RequiresUpdate
	{
		get
		{
			if (Player != null)
			{
				return Player.RequiresUpdate;
			}
			return false;
		}
	}

	public QuadBase()
	{
	}

	public QuadBase(QuadBase original)
	{
		Player = new Animation2DPlayer(original.Player);
	}

	public void Update(GameTime gameTime)
	{
		if (Player != null && Player.RequiresUpdate)
		{
			Player.Update(gameTime);
		}
	}

	public void SetStaticFrame(Rectangle? rectangle, Texture2D texture)
	{
		if (rectangle.HasValue)
		{
			Player.StartAnimation(new Animation2D(texture, 0.1f, isLooping: false)
			{
				Cells = new List<Cell>
				{
					new Cell(rectangle.Value)
				}
			});
			return;
		}
		Animation2DPlayer player = Player;
		Animation2D animation2D = new Animation2D(texture, 0.1f, isLooping: false);
		List<Cell> list = new List<Cell>();
		Cell item = new Cell(default(Rectangle))
		{
			Color = Color.Transparent
		};
		list.Add(item);
		animation2D.Cells = list;
		player.StartAnimation(animation2D);
	}

	public void SetAnimationFrames(Animation2D animation)
	{
		Player.StartAnimation(animation);
	}
}
