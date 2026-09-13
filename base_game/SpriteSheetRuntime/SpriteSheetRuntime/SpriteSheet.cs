using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SpriteSheetRuntime;

public class SpriteSheet
{
	public Texture2D Texture;

	public List<Rectangle> spriteRectangles;

	public Dictionary<string, int> spriteNames;

	protected internal virtual void ReadContent(ContentReader input)
	{
		Texture = input.ReadObject<Texture2D>();
		spriteRectangles = input.ReadObject<List<Rectangle>>();
		spriteNames = input.ReadObject<Dictionary<string, int>>();
	}

	public Rectangle GetSourceRectangle(string spriteName)
	{
		int index = GetIndex(spriteName);
		return spriteRectangles[index];
	}

	public bool TryGetSourceRectangle(string spriteName, out Rectangle? spriteRect)
	{
		if (spriteNames.TryGetValue(spriteName, out var value))
		{
			if (value >= 0)
			{
				spriteRect = spriteRectangles[value];
				return true;
			}
			spriteRect = null;
			return false;
		}
		spriteRect = null;
		return false;
	}

	public Rectangle SourceRectangle(int spriteIndex)
	{
		if (spriteIndex < 0 || spriteIndex >= spriteRectangles.Count)
		{
			throw new ArgumentOutOfRangeException("spriteIndex");
		}
		return spriteRectangles[spriteIndex];
	}

	public int GetIndex(string spriteName)
	{
		if (!spriteNames.TryGetValue(spriteName, out var value))
		{
			throw new KeyNotFoundException($"SpriteSheet does not contain a sprite named '{spriteName}'.");
		}
		return value;
	}
}
