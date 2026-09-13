using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kensei.Dev;

public static class DevText
{
	public enum Alignment
	{
		Left,
		Centre,
		Right
	}

	private struct TextInfo2D
	{
		public Vector2 m_position;

		public string m_text;

		public Color m_colour;
	}

	private struct TextInfo3D
	{
		public Vector3 m_position;

		public string m_text;

		public Color m_colour;
	}

	private struct TextInfoStack
	{
		public string m_text;

		public Color m_colour;
	}

	private static List<TextInfo2D> s_printList = new List<TextInfo2D>();

	private static List<TextInfo3D> s_print3DList = new List<TextInfo3D>();

	private static List<TextInfoStack> s_printStackList = new List<TextInfoStack>();

	private static Rectangle s_safeArea;

	private static Color s_defaultColour = Color.LimeGreen;

	public static Color DefaultColour
	{
		get
		{
			return s_defaultColour;
		}
		set
		{
			s_defaultColour = value;
		}
	}

	internal static void Initialise(GraphicsDevice device, int drawableAreaX, int drawableAreaY, int drawableAreaWidth, int drawableAreaHeight)
	{
		s_safeArea = new Rectangle(drawableAreaX, drawableAreaY, drawableAreaWidth, drawableAreaHeight);
	}

	internal static void Draw(Matrix renderMatrix, float width, float height)
	{
		if (s_printList.Count > 0 || s_printStackList.Count > 0 || s_print3DList.Count > 0)
		{
			Manager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
			try
			{
				Render2D();
				RenderStack();
				Render3D(ref renderMatrix, width, height);
			}
			catch (ArgumentException)
			{
			}
			Manager.SpriteBatch.End();
			s_printList.Clear();
			s_print3DList.Clear();
			s_printStackList.Clear();
		}
	}

	public static void Print(Vector2 position, string text)
	{
		Print(position, text, DefaultColour);
	}

	public static void Print(Vector2 position, string text, Color colour)
	{
		Print(position, text, colour, Alignment.Left);
	}

	public static void Print(Vector2 position, string text, Alignment align)
	{
		Print(position, text, align);
	}

	public static void Print(Vector2 position, string text, Color colour, Alignment align)
	{
		TextInfo2D item = default(TextInfo2D);
		item.m_position = position;
		item.m_text = text;
		item.m_colour = colour;
		switch (align)
		{
		case Alignment.Centre:
			item.m_position.X -= GetDims(item.m_text).X / 2f;
			break;
		case Alignment.Right:
			item.m_position.X -= GetDims(item.m_text).X;
			break;
		}
		s_printList.Add(item);
	}

	public static void Print(Vector3 position, string text)
	{
		Print(position, text, DefaultColour);
	}

	public static void Print(Vector3 position, string text, Color colour)
	{
		TextInfo3D item = default(TextInfo3D);
		item.m_position = position;
		item.m_text = text;
		item.m_colour = colour;
		s_print3DList.Add(item);
	}

	public static void Print(string text)
	{
		Print(text, DefaultColour);
	}

	public static void Print(string text, Color colour)
	{
		TextInfoStack item = default(TextInfoStack);
		item.m_colour = colour;
		item.m_text = text;
		s_printStackList.Add(item);
	}

	public static Vector2 GetDims(string text)
	{
		return Manager.SpriteFont.MeasureString(text);
	}

	private static void Render3D(ref Matrix renderMatrix, float width, float height)
	{
		foreach (TextInfo3D s_print3D in s_print3DList)
		{
			Vector4 value = new Vector4(s_print3D.m_position, 1f);
			Vector4.Transform(ref value, ref renderMatrix, out value);
			value.X = value.X * 0.5f / value.W + 0.5f;
			value.Y = 1f - (value.Y * 0.5f / value.W + 0.5f);
			value.X *= width;
			value.Y *= height;
			if (value.X >= 0f && value.X < width && value.Y >= 0f && value.Y < height && value.Z > 0f)
			{
				Manager.SpriteBatch.DrawString(Manager.SpriteFont, s_print3D.m_text, new Vector2(value.X, value.Y), s_print3D.m_colour);
			}
		}
	}

	private static void RenderStack()
	{
		Vector2 position = new Vector2(s_safeArea.Left, s_safeArea.Top);
		for (int i = 0; i < s_printStackList.Count; i++)
		{
			Manager.SpriteBatch.DrawString(Manager.SpriteFont, s_printStackList[i].m_text, position, s_printStackList[i].m_colour);
			position.Y += Manager.SpriteFont.MeasureString(s_printStackList[i].m_text).Y;
		}
	}

	private static void Render2D()
	{
		for (int i = 0; i < s_printList.Count; i++)
		{
			Manager.SpriteBatch.DrawString(Manager.SpriteFont, s_printList[i].m_text, s_printList[i].m_position, s_printList[i].m_colour);
		}
	}
}
