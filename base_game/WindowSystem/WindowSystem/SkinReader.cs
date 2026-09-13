using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework.Content;

namespace WindowSystem;

internal class SkinReader : ContentTypeReader<Skin>
{
	protected override Skin Read(ContentReader input, Skin existingInstance)
	{
		Skin skin = new Skin();
		int num = input.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			UIComponentSkin uIComponentSkin = new UIComponentSkin();
			string typeName = input.ReadString();
			IDictionary<string, string> dictionary = RetrieveProperties(input);
			uIComponentSkin.ComponentType = Type.GetType(typeName);
			foreach (string key in dictionary.Keys)
			{
				PropertyInfo property = uIComponentSkin.ComponentType.GetProperty(key);
				if (property.GetCustomAttributes(typeof(SkinAttribute), inherit: true).Length != 0)
				{
					uIComponentSkin.Properties.Add(property);
					uIComponentSkin.Values.Add(dictionary[key]);
				}
			}
			skin.ComponentSkins.Add(uIComponentSkin);
		}
		return skin;
	}

	private IDictionary<string, string> RetrieveProperties(ContentReader input)
	{
		IDictionary<string, string> dictionary = new Dictionary<string, string>();
		int num = input.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			string key = input.ReadString();
			string value = input.ReadString();
			dictionary[key] = value;
		}
		return dictionary;
	}
}
