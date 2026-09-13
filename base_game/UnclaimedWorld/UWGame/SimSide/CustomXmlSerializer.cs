using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace UWGame.SimSide;

public class CustomXmlSerializer
{
	public interface ISerializationProxy
	{
		object UnderlyingObject { get; set; }

		void Copy(object source, object dest);

		ISerializationProxy CreateInstance(object o);
	}

	public abstract class XmlTypeMappingBase
	{
		public abstract Type ProxiedType { get; }

		public abstract Type ProxyType { get; }
	}

	public class XmlTypeMapping<TProxied, TProxy> : XmlTypeMappingBase
	{
		public override Type ProxiedType => typeof(TProxied);

		public override Type ProxyType => typeof(TProxy);

		public Func<TProxy, TProxied> SetterMethod { get; set; }

		public Func<TProxied, TProxy> GetterMethod { get; set; }
	}

	public class XmlProxyData
	{
		public Type ProxiedType { get; set; }

		public List<XmlTypeMappingBase> TypeMappings { get; set; }

		public XmlProxyData(Type proxiedType)
		{
			ProxiedType = proxiedType;
			TypeMappings = new List<XmlTypeMappingBase>();
		}
	}

	private class ProxyAssemblyData
	{
		public XmlProxyData XmlProxyData { get; set; }

		public Type ProxyType { get; set; }

		public Assembly ProxyAssembly { get; set; }

		public ISerializationProxy ProxyInstance { get; set; }
	}

	private static Dictionary<Type, ProxyAssemblyData> _proxyAssemblyDataMap = new Dictionary<Type, ProxyAssemblyData>();

	/// <summary>
	/// PORT DEVIATION 12 (see PORTING-NOTES.md). Namespace the generated proxies are emitted
	/// into. Was <c>"dummy.d" + Guid.NewGuid()</c> - a fresh namespace per run, which was fine
	/// when the proxy was compiled into a throwaway in-memory assembly but makes the source
	/// impossible to pre-generate. tools/XmlProxyGen asserts this value matches what it emits.
	/// </summary>
	public const string GeneratedProxyNamespace = "UWGame.Generated.XmlProxies";

	public static XmlProxyData GetXmlProxyData(Type proxiedType)
	{
		if (_proxyAssemblyDataMap.ContainsKey(proxiedType))
		{
			return _proxyAssemblyDataMap[proxiedType].XmlProxyData;
		}
		return null;
	}

	public static void WriteXmlSerialize(object o, XmlWriter writer, XmlProxyData proxyData)
	{
		if (o == null || writer == null || proxyData == null)
		{
			throw new NullReferenceException();
		}
		Type proxiedType = proxyData.ProxiedType;
		if (!_proxyAssemblyDataMap.ContainsKey(proxiedType))
		{
			GenerateProxyAssembly(proxyData);
		}
		ProxyAssemblyData proxyAssemblyData = _proxyAssemblyDataMap[proxiedType];
		XmlSerializer xmlSerializer = new XmlSerializer(proxyAssemblyData.ProxyType);
		XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
		xmlSerializerNamespaces.Add("", "");
		object o2 = proxyAssemblyData.ProxyInstance.CreateInstance(o);
		StringBuilder stringBuilder = new StringBuilder();
		using (StringWriter w = new StringWriter(stringBuilder))
		{
			using XmlTextWriter xmlWriter = new XmlTextWriter(w);
			xmlSerializer.Serialize(xmlWriter, o2, xmlSerializerNamespaces);
		}
		using StringReader input = new StringReader(stringBuilder.ToString());
		using XmlTextReader xmlTextReader = new XmlTextReader(input);
		while (xmlTextReader.Read() && xmlTextReader.NodeType != XmlNodeType.Element)
		{
		}
		WriteDeepNode(xmlTextReader, writer, skipStartElement: true);
	}

	public static void ReadXmlDeserialize(object o, XmlReader reader, XmlProxyData proxyData)
	{
		if (o == null || reader == null || proxyData == null)
		{
			throw new NullReferenceException();
		}
		Type proxiedType = proxyData.ProxiedType;
		if (!_proxyAssemblyDataMap.ContainsKey(proxiedType))
		{
			GenerateProxyAssembly(proxyData);
		}
		ProxyAssemblyData proxyAssemblyData = _proxyAssemblyDataMap[proxiedType];
		if (reader.LocalName == proxyAssemblyData.ProxyType.Name)
		{
			ISerializationProxy obj = (ISerializationProxy)new XmlSerializer(proxyAssemblyData.ProxyType).Deserialize(reader);
			obj.Copy(obj.UnderlyingObject, o);
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		using (StringWriter w = new StringWriter(stringBuilder))
		{
			using XmlTextWriter xmlTextWriter = new XmlTextWriter(w);
			xmlTextWriter.WriteStartElement(proxyAssemblyData.ProxyType.Name);
			bool num = reader.NodeType == XmlNodeType.Element && !reader.IsEmptyElement;
			WriteDeepNode(reader, xmlTextWriter, skipStartElement: true);
			if (num)
			{
				xmlTextWriter.WriteFullEndElement();
			}
			else
			{
				xmlTextWriter.WriteEndElement();
			}
			reader.Read();
		}
		using StringReader input = new StringReader(stringBuilder.ToString());
		using XmlTextReader xmlReader = new XmlTextReader(input);
		ISerializationProxy obj2 = (ISerializationProxy)new XmlSerializer(proxyAssemblyData.ProxyType).Deserialize(xmlReader);
		obj2.Copy(obj2.UnderlyingObject, o);
	}

	/// <summary>
	/// PORT DEVIATION 12 (see PORTING-NOTES.md).
	///
	/// Was: generate a proxy class for <paramref name="proxyData"/>.ProxiedType with CodeDom and
	/// compile it into an in-memory assembly via
	/// <c>CodeDomProvider.CompileAssemblyFromDom</c>. That method throws
	/// <c>PlatformNotSupportedException</c> on .NET 8 - runtime C# compilation was removed from
	/// the framework, and the System.CodeDom package keeps only the code-GENERATION half.
	///
	/// The proxies are now generated at build time by tools/XmlProxyGen, whose
	/// ProxyCodeGenerator IS this class's former CodeDom half, moved rather than rewritten - so
	/// the emitted proxies are the same source the retail build compiled, and XmlSerializer sees
	/// an identical shape. That keeps the on-disk XML format of every data/ file byte-compatible,
	/// which is the whole point: these proxies are what the data/BaseData and data/Scenarios mod
	/// hooks deserialize through (deviation 13).
	///
	/// Moving it also drops System.CodeDom from the game's references. The WindowsDX target got
	/// that assembly free from the Microsoft.WindowsDesktop.App shared framework, but the
	/// DesktopGL target builds as net8.0, where it is a NuGet package - so the portable build no
	/// longer carries a package for code it could never execute.
	///
	/// <c>ProxyAssembly</c> stays null - there is no longer a separate assembly - and nothing
	/// reads it.
	/// </summary>
	private static void GenerateProxyAssembly(XmlProxyData proxyData)
	{
		ProxyAssemblyData proxyAssemblyData = new ProxyAssemblyData();
		proxyAssemblyData.XmlProxyData = proxyData;
		ProxyAssemblyData proxyAssemblyData2 = proxyAssemblyData;
		_proxyAssemblyDataMap[proxyData.ProxiedType] = proxyAssemblyData2;

		if (!Generated.XmlProxies.XmlProxyRegistry.ProxyTypes.TryGetValue(proxyData.ProxiedType, out Type proxyType))
		{
			// Reachable only if a type gained a _proxyData field without the proxies being
			// regenerated. Say which type and how to fix it, rather than letting a null
			// ProxyType surface as a NullReferenceException inside XmlSerializer.
			_proxyAssemblyDataMap.Remove(proxyData.ProxiedType);
			throw new InvalidOperationException(
				"No pre-generated XML serialization proxy for " + proxyData.ProxiedType.FullName +
				". Run build/20-generate-xml-proxies.sh and rebuild. (Proxies cannot be compiled " +
				"at runtime on .NET 8 - see PORT DEVIATION 12 in PORTING-NOTES.md.)");
		}

		proxyAssemblyData2.ProxyType = proxyType;
		ConstructorInfo constructor = proxyType.GetConstructor(new Type[0]);
		proxyAssemblyData2.ProxyInstance = (ISerializationProxy)constructor.Invoke(new object[0]);
	}

	private static void WriteDeepNode(XmlReader reader, XmlWriter writer, bool skipStartElement)
	{
		int num = 1;
		string localName = reader.LocalName;
		WriteShallowNode(reader, writer, skipStartElement);
		if (reader.NodeType != XmlNodeType.Element || reader.IsEmptyElement)
		{
			return;
		}
		bool flag = false;
		while (reader.Read())
		{
			if (reader.LocalName == localName)
			{
				if (reader.NodeType == XmlNodeType.Element && !reader.IsEmptyElement)
				{
					num++;
				}
				else if (reader.NodeType == XmlNodeType.EndElement)
				{
					num--;
					if (num == 0)
					{
						if (skipStartElement)
						{
							break;
						}
						flag = true;
					}
				}
			}
			WriteShallowNode(reader, writer, skipStartElement: false);
			if (flag)
			{
				break;
			}
		}
	}

	private static void WriteShallowNode(XmlReader reader, XmlWriter writer, bool skipStartElement)
	{
		switch (reader.NodeType)
		{
		case XmlNodeType.Element:
			if (!skipStartElement)
			{
				writer.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
			}
			writer.WriteAttributes(reader, defattr: true);
			if (!skipStartElement && reader.IsEmptyElement)
			{
				writer.WriteEndElement();
			}
			break;
		case XmlNodeType.Text:
			writer.WriteString(reader.Value);
			break;
		case XmlNodeType.Whitespace:
		case XmlNodeType.SignificantWhitespace:
			writer.WriteWhitespace(reader.Value);
			break;
		case XmlNodeType.CDATA:
			writer.WriteCData(reader.Value);
			break;
		case XmlNodeType.EntityReference:
			writer.WriteEntityRef(reader.Name);
			break;
		case XmlNodeType.ProcessingInstruction:
		case XmlNodeType.XmlDeclaration:
			writer.WriteProcessingInstruction(reader.Name, reader.Value);
			break;
		case XmlNodeType.DocumentType:
			writer.WriteDocType(reader.Name, reader.GetAttribute("PUBLIC"), reader.GetAttribute("SYSTEM"), reader.Value);
			break;
		case XmlNodeType.Comment:
			writer.WriteComment(reader.Value);
			break;
		case XmlNodeType.EndElement:
			writer.WriteFullEndElement();
			break;
		case XmlNodeType.Attribute:
		case XmlNodeType.Entity:
		case XmlNodeType.Document:
		case XmlNodeType.DocumentFragment:
		case XmlNodeType.Notation:
		case XmlNodeType.EndEntity:
			break;
		}
	}
}
