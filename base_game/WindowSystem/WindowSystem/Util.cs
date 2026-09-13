using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace WindowSystem;

public static class Util
{
	public static void AddToList<T>(ref List<T> list, T value)
	{
		if (list == null)
		{
			list = new List<T>();
		}
		list.Add(value);
	}

	public static int Clamp(int f1, int bottom, int top)
	{
		if (f1 <= bottom)
		{
			return bottom;
		}
		if (f1 >= top)
		{
			return top;
		}
		return f1;
	}

	public static float Clamp(float f1, float bottom, float top)
	{
		if (!(f1 > bottom))
		{
			return bottom;
		}
		if (!(f1 < top))
		{
			return top;
		}
		return f1;
	}

	public static Color? ColorFromHex(this string hexString)
	{
		if (hexString.StartsWith("#"))
		{
			hexString = hexString.Substring(1);
		}
		if (uint.TryParse(hexString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result))
		{
			Color white = Color.White;
			if (hexString.Length == 8)
			{
				white.A = (byte)(result >> 24);
				white.R = (byte)(result >> 16);
				white.G = (byte)(result >> 8);
				white.B = (byte)result;
			}
			else
			{
				if (hexString.Length != 6)
				{
					throw new InvalidOperationException("Invalid hex representation of an ARGB or RGB color value.");
				}
				white.R = (byte)(result >> 16);
				white.G = (byte)(result >> 8);
				white.B = (byte)result;
			}
			return white;
		}
		return null;
	}

	public static string ToHex(this Color color)
	{
		return string.Format("#{0}{1}{2}", (color.R.ToString("X").Length == 1) ? string.Format("0{0}", color.R.ToString("X")) : color.R.ToString("X"), (color.G.ToString("X").Length == 1) ? string.Format("0{0}", color.G.ToString("X")) : color.G.ToString("X"), (color.B.ToString("X").Length == 1) ? string.Format("0{0}", color.B.ToString("X")) : color.B.ToString("X"));
	}
}
