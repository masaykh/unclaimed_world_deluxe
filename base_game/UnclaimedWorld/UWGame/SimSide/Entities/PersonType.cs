using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;

namespace UWGame.SimSide.Entities;

public class PersonType : IXmlSerializable
{
	public enum SkinColors
	{
		Celtic,
		LightEuropean,
		AverageCaucasian,
		OliveSkin,
		Dark,
		Black
	}

	public enum HairColors
	{
		Platinum,
		LightBlonde,
		Red,
		MediumBlonde,
		DarkBlonde,
		LightBrown,
		DarkBrown,
		DarkestBrown,
		Black,
		Grey,
		White,
		Bald
	}

	public List<Vector3> ShirtColors = new List<Vector3>();

	public List<Vector3> PantsColors = new List<Vector3>();

	public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(PersonType))
	{
		TypeMappings = new List<CustomXmlSerializer.XmlTypeMappingBase>
		{
			new CustomXmlSerializer.XmlTypeMapping<List<Vector3>, string[]>
			{
				GetterMethod = delegate(List<Vector3> t)
				{
					if (t == null)
					{
						return (string[])null;
					}
					string[] array = new string[t.Count];
					int num = 0;
					foreach (Vector3 item in t)
					{
						array[num] = Vector3ToHexString(item);
						num++;
					}
					return array;
				},
				SetterMethod = delegate(string[] t)
				{
					if (t == null)
					{
						return (List<Vector3>)null;
					}
					List<Vector3> list = new List<Vector3>();
					foreach (string hex in t)
					{
						list.Add(HexStringToVector3(hex));
					}
					return list;
				}
			}
		}
	};

	public void Initialize()
	{
	}

	public void UpdateRenderableRandomColors(Renderable renderable)
	{
		renderable.RenderAsModel.CustomColor0 = GetRandomPants();
		renderable.RenderAsModel.CustomColor1 = GetRandomShirt();
	}

	public static string FormatVector3(Vector3 v)
	{
		return $"new Vector3({v.X}f# {v.Y}f# {v.Z}f)".Replace(",", ".").Replace("#", ",");
	}

	public static Vector3 HexStringToVector3(string hex)
	{
		int num = int.Parse(hex.Substring(0, 2), NumberStyles.AllowHexSpecifier);
		int num2 = int.Parse(hex.Substring(2, 2), NumberStyles.AllowHexSpecifier);
		return new Vector3(z: (float)int.Parse(hex.Substring(4, 2), NumberStyles.AllowHexSpecifier) / 255f, x: (float)num / 255f, y: (float)num2 / 255f);
	}

	public static string Vector3ToHexString(Vector3 vector)
	{
		return string.Format("{0}{1}{2}", ((int)(255f * vector.X)).ToString("X2"), ((int)(255f * vector.Y)).ToString("X2"), ((int)(255f * vector.Z)).ToString("X2"));
	}

	public Vector3 GetRandomShirt()
	{
		return ShirtColors[The.Sim.GameplayRandomGenerator.Next(ShirtColors.Count, "PersonType")];
	}

	public Vector3 GetRandomPants()
	{
		return PantsColors[The.Sim.GameplayRandomGenerator.Next(PantsColors.Count, "PersonType")];
	}

	public XmlSchema GetSchema()
	{
		return null;
	}

	public void ReadXml(XmlReader reader)
	{
		CustomXmlSerializer.ReadXmlDeserialize(this, reader, _proxyData);
	}

	public void WriteXml(XmlWriter writer)
	{
		CustomXmlSerializer.WriteXmlSerialize(this, writer, _proxyData);
	}
}
