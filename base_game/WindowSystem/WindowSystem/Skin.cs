using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace WindowSystem;

public class Skin
{
	public List<UIComponentSkin> ComponentSkins = new List<UIComponentSkin>();

	private object ParseValue(PropertyInfo property, string value)
	{
		object obj = null;
		if (property.PropertyType == typeof(string))
		{
			return value;
		}
		if (property.PropertyType == typeof(Rectangle) || property.PropertyType == typeof(Color))
		{
			char[] separator = new char[1] { ',' };
			int[] array = new int[4];
			string[] array2 = value.Split(separator);
			for (int i = 0; i < 4; i++)
			{
				array[i] = int.Parse(array2[i]);
			}
			if (property.PropertyType == typeof(Rectangle))
			{
				return new Rectangle(array[0], array[1], array[2], array[3]);
			}
			return new Color((byte)array[0], (byte)array[1], (byte)array[2], (byte)array[3]);
		}
		if (property.PropertyType == typeof(Point))
		{
			char[] separator2 = new char[1] { ',' };
			string[] array3 = value.Split(separator2);
			int x = int.Parse(array3[0]);
			int y = int.Parse(array3[1]);
			return new Point(x, y);
		}
		try
		{
			return Convert.ChangeType(value, property.PropertyType);
		}
		catch
		{
			return null;
		}
	}
}
