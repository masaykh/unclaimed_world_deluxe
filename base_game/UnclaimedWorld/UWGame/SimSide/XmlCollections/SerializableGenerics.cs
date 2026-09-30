using System;

namespace UWGame.SimSide.XmlCollections;

public static class SerializableGenerics
{

	public static string GetTypeName(Type type)
	{
		string text;
		if (type.IsGenericType)
		{
			text = type.Name.Substring(0, type.Name.Length - 2);
			text += "Of";
			Type[] genericArguments = type.GetGenericArguments();
			foreach (Type type2 in genericArguments)
			{
				text += GetTypeName(type2);
			}
		}
		else if (type.IsArray)
		{
			text = type.BaseType.Name;
			text += "Of";
			text += GetTypeName(type.GetElementType());
		}
		else
		{
			text = type.Name;
		}
		return text;
	}
}
