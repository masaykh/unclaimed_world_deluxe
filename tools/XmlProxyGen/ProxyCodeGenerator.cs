using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using Microsoft.CSharp;
using UWGame.SimSide;

// Brings CustomXmlSerializer's nested types - XmlProxyData, XmlTypeMapping<,>,
// XmlTypeMappingBase, ISerializationProxy - and GeneratedProxyNamespace into scope unqualified,
// so the code below is the game's original generator verbatim rather than a reworded copy.
using static UWGame.SimSide.CustomXmlSerializer;

namespace UW.Tools.XmlProxyGen;

/// <summary>
/// The CodeDom half of the game's CustomXmlSerializer, moved out of the game (PORT DEVIATION 12).
///
/// This is the retail code that built an XML serialization proxy per data type, unchanged apart
/// from being lifted into its own class. It ran at runtime in the retail build; on .NET 8 it runs
/// here, at build time, and the game compiles the result as ordinary source.
///
/// Moving it rather than copying it is what lets the game drop its System.CodeDom reference.
/// That matters for the DesktopGL target specifically: it builds as net8.0, where System.CodeDom
/// is a NuGet package, whereas the WindowsDX target's net8.0-windows gets it free from the
/// Microsoft.WindowsDesktop.App shared framework. So the move is the difference between the
/// portable build carrying a package it only needs for code it can never execute, and not.
///
/// The proof that the move changed nothing is that regenerating still produces byte-identical
/// proxies - build/20-generate-xml-proxies.sh --check.
/// </summary>
internal sealed class ProxyCodeGenerator
{
	private class TypeData
	{
		public int Index { get; set; }

		public XmlTypeMappingBase TypeMapping { get; set; }
	}

	private class MemberData
	{
		public bool IsField { get; set; }

		public Type MemberType { get; set; }

		public string Name { get; set; }

		public object[] CustomAttributes { get; set; }
	}

	private static readonly Dictionary<Type, object> _xmlCustomAttributesInstances = new Dictionary<Type, object>();

	private const string UNDERLYING_INSTANCE_NAME = "_underlyingInstance";

	private const string PROXY_DATA_NAME = "_proxyData";

	private CodeCompileUnit _unit;

	private CodeNamespace _ns;

	private XmlProxyData _proxyData;

	private CodeTypeDeclaration _cl;

	private Dictionary<Type, TypeData> _typeDataMap;

	private List<MemberData> _memberList;
	public CodeCompileUnit GenerateXmlProxy(XmlProxyData proxyData)
	{
		_proxyData = proxyData;
		_unit = new CodeCompileUnit();
		_ns = new CodeNamespace(GeneratedProxyNamespace);
		_unit.Namespaces.Add(_ns);
		_cl = new CodeTypeDeclaration(proxyData.ProxiedType.Name);
		_cl.IsClass = true;
		_cl.Attributes = MemberAttributes.Public;
		_cl.BaseTypes.Add(typeof(ISerializationProxy));
		_ns.Types.Add(_cl);
		GenerateMembers();
		return _unit;
	}

	public string GetProxyCode(XmlProxyData proxyData)
	{
		return GenerateCode(GenerateXmlProxy(proxyData));
	}

	public void SaveProxyCodeToFile(XmlProxyData proxyData, string filePath)
	{
		GenerateXmlProxy(proxyData);
		SaveFile(filePath);
	}

	private void GenerateMembers()
	{
		InitTypeAndFieldData();
		CreateConstructorAndFields();
		CreateProxyInterfaceMembers();
		CreateProperties();
	}

	private void InitTypeAndFieldData()
	{
		int num = 0;
		_typeDataMap = new Dictionary<Type, TypeData>();
		foreach (XmlTypeMappingBase typeMapping in _proxyData.TypeMappings)
		{
			_typeDataMap[typeMapping.ProxiedType] = new TypeData
			{
				Index = num++,
				TypeMapping = typeMapping
			};
		}
		_memberList = new List<MemberData>();
		_memberList.AddRange(from f in _proxyData.ProxiedType.GetFields(BindingFlags.Instance | BindingFlags.Public)
			select new MemberData
			{
				IsField = true,
				MemberType = f.FieldType,
				Name = f.Name,
				CustomAttributes = f.GetCustomAttributes(inherit: true)
			});
		_memberList.AddRange(from p in _proxyData.ProxiedType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
			where p.GetGetMethod() != null && p.GetSetMethod() != null
			select new MemberData
			{
				IsField = false,
				MemberType = p.PropertyType,
				Name = p.Name,
				CustomAttributes = p.GetCustomAttributes(inherit: true)
			});
	}

	private void CreateProperties()
	{
		foreach (MemberData member in _memberList)
		{
			CodeMemberProperty codeMemberProperty = AddProperty(member.MemberType, member.Name);
			if (_typeDataMap.ContainsKey(member.MemberType))
			{
				TypeData typeData = _typeDataMap[member.MemberType];
				codeMemberProperty.Type = new CodeTypeReference(typeData.TypeMapping.ProxyType);
				CodeExpression targetObject = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(_cl.Name), "_proxyData");
				targetObject = new CodeFieldReferenceExpression(targetObject, "TypeMappings");
				targetObject = new CodeIndexerExpression(targetObject, new CodePrimitiveExpression(typeData.Index));
				targetObject = new CodeCastExpression(typeData.TypeMapping.GetType(), targetObject);
				codeMemberProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(targetObject, "GetterMethod"), new CodeFieldReferenceExpression(GetThisField("_underlyingInstance"), member.Name))));
				codeMemberProperty.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(GetThisField("_underlyingInstance"), member.Name), new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(targetObject, "SetterMethod"), new CodePropertySetValueReferenceExpression())));
			}
			else
			{
				codeMemberProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(GetThisField("_underlyingInstance"), member.Name)));
				codeMemberProperty.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(GetThisField("_underlyingInstance"), member.Name), new CodePropertySetValueReferenceExpression()));
			}
			AddCustomAttributes(codeMemberProperty, member);
		}
	}

	private void AddCustomAttributes(CodeMemberProperty p, MemberData data)
	{
		if (data.CustomAttributes == null)
		{
			return;
		}
		object[] customAttributes = data.CustomAttributes;
		foreach (object obj in customAttributes)
		{
			Type type = obj.GetType();
			if (!type.Name.StartsWith("Xml"))
			{
				continue;
			}
			CodeAttributeDeclaration codeAttributeDeclaration = new CodeAttributeDeclaration(new CodeTypeReference(type));
			PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (propertyInfo.DeclaringType == type && (propertyInfo.PropertyType.IsPrimitive || propertyInfo.PropertyType == typeof(string) || propertyInfo.PropertyType == typeof(Type)))
				{
					object value = propertyInfo.GetValue(obj, null);
					if (!IsCustomAttributeDefaultValue(type, propertyInfo, value))
					{
						codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument(propertyInfo.Name, (propertyInfo.PropertyType == typeof(Type) && value != null) ? ((CodeExpression)new CodeTypeOfExpression((Type)value)) : ((CodeExpression)new CodePrimitiveExpression(value))));
					}
				}
			}
			p.CustomAttributes.Add(codeAttributeDeclaration);
		}
	}

	private bool IsCustomAttributeDefaultValue(Type attrType, PropertyInfo pi, object value)
	{
		if (!_xmlCustomAttributesInstances.ContainsKey(attrType))
		{
			ConstructorInfo constructor = attrType.GetConstructor(new Type[0]);
			if (constructor == null)
			{
				return false;
			}
			_xmlCustomAttributesInstances[attrType] = constructor.Invoke(new object[0]);
		}
		return pi.GetValue(_xmlCustomAttributesInstances[attrType], null)?.Equals(value) ?? (value == null);
	}

	private void CreateProxyInterfaceMembers()
	{
		CodeMemberProperty codeMemberProperty = AddProperty(typeof(object), "UnderlyingObject");
		codeMemberProperty.GetStatements.Add(new CodeMethodReturnStatement(GetThisField("_underlyingInstance")));
		codeMemberProperty.SetStatements.Add(new CodeAssignStatement(GetThisField("_underlyingInstance"), new CodeCastExpression(new CodeTypeReference(_proxyData.ProxiedType), new CodePropertySetValueReferenceExpression())));
		codeMemberProperty.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(XmlIgnoreAttribute))));
		CodeMemberMethod codeMemberMethod = AddMethod(typeof(ISerializationProxy), "CreateInstance");
		codeMemberMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "o"));
		codeMemberMethod.Statements.Add(new CodeMethodReturnStatement(new CodeObjectCreateExpression(new CodeTypeReference(_cl.Name), GetVar("o"))));
		codeMemberMethod = AddMethod(typeof(void), "Copy");
		codeMemberMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "source"));
		codeMemberMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "dest"));
		codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(_proxyData.ProxiedType, "s", new CodeCastExpression(_proxyData.ProxiedType, GetVar("source"))));
		codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(_proxyData.ProxiedType, "d", new CodeCastExpression(_proxyData.ProxiedType, GetVar("dest"))));
		foreach (MemberData member in _memberList)
		{
			if (member.IsField)
			{
				codeMemberMethod.Statements.Add(new CodeAssignStatement(GetFieldRef("d", member.Name), GetFieldRef("s", member.Name)));
			}
			else
			{
				codeMemberMethod.Statements.Add(new CodeAssignStatement(GetPropRef("d", member.Name), GetPropRef("s", member.Name)));
			}
		}
	}

	private void CreateConstructorAndFields()
	{
		AddField(_cl, _proxyData.ProxiedType, "_underlyingInstance");
		CodeMemberField codeMemberField = AddField(_cl, typeof(XmlProxyData), "_proxyData");
		codeMemberField.Attributes |= MemberAttributes.Static;
		codeMemberField.InitExpression = GetStaticMethodInvoke(GetSelfType(), "GetXmlProxyData", new CodeTypeOfExpression(new CodeTypeReference(_proxyData.ProxiedType)));
		CodeConstructor codeConstructor = new CodeConstructor();
		codeConstructor.Attributes = MemberAttributes.Public;
		codeConstructor.Statements.Add(new CodeAssignStatement(GetThisField("_underlyingInstance"), new CodeObjectCreateExpression(new CodeTypeReference(_proxyData.ProxiedType))));
		_cl.Members.Add(codeConstructor);
		codeConstructor = new CodeConstructor();
		codeConstructor.Attributes = MemberAttributes.Public;
		codeConstructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "_underlyingInstance"));
		codeConstructor.Statements.Add(new CodeAssignStatement(GetThisField("_underlyingInstance"), new CodeCastExpression(_proxyData.ProxiedType, GetVar("_underlyingInstance"))));
		_cl.Members.Add(codeConstructor);
	}

	private CodeVariableReferenceExpression GetVar(string varName)
	{
		return new CodeVariableReferenceExpression(varName);
	}

	private CodeTypeReference GetSelfType()
	{
		return new CodeTypeReference(typeof(CustomXmlSerializer));
	}

	private CodeExpression GetThis()
	{
		return new CodeThisReferenceExpression();
	}

	private CodeExpression GetThisField(string fieldName)
	{
		return new CodeFieldReferenceExpression(GetThis(), fieldName);
	}

	private CodeMemberField AddField(CodeTypeDeclaration cl, Type type, string name)
	{
		CodeMemberField codeMemberField = new CodeMemberField(type, name);
		codeMemberField.Attributes = MemberAttributes.Private;
		cl.Members.Add(codeMemberField);
		return codeMemberField;
	}

	private CodeMethodInvokeExpression GetMethodInvoke(string varName, string methodName, params CodeExpression[] parameters)
	{
		return new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(varName), methodName), parameters);
	}

	private CodeMethodInvokeExpression GetStaticMethodInvoke(CodeTypeReference type, string methodName, params CodeExpression[] parameters)
	{
		return new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(type), methodName), parameters);
	}

	private CodeMemberProperty AddProperty(Type type, string name)
	{
		CodeMemberProperty codeMemberProperty = new CodeMemberProperty();
		codeMemberProperty.Name = name;
		codeMemberProperty.Type = new CodeTypeReference(type);
		codeMemberProperty.Attributes = (MemberAttributes)24578;
		_cl.Members.Add(codeMemberProperty);
		return codeMemberProperty;
	}

	private CodeMemberMethod AddMethod(Type returnType, string name)
	{
		CodeMemberMethod codeMemberMethod = new CodeMemberMethod();
		codeMemberMethod.Name = name;
		codeMemberMethod.ReturnType = new CodeTypeReference(returnType);
		codeMemberMethod.Attributes = (MemberAttributes)24578;
		CodeMemberMethod codeMemberMethod2 = codeMemberMethod;
		_cl.Members.Add(codeMemberMethod2);
		return codeMemberMethod2;
	}

	private CodeFieldReferenceExpression GetFieldRef(string varName, string fieldName)
	{
		return new CodeFieldReferenceExpression(GetVar(varName), fieldName);
	}

	private CodePropertyReferenceExpression GetPropRef(string varName, string propName)
	{
		return new CodePropertyReferenceExpression(GetVar(varName), propName);
	}

	private CodeExpression GetFieldOrPropRef(string varName, string name, bool isField)
	{
		if (isField)
		{
			return GetFieldRef(varName, name);
		}
		return GetPropRef(varName, name);
	}

	private void SaveFile(string filePath)
	{
		using FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
		using StreamWriter streamWriter = new StreamWriter(stream);
		string value = GenerateCode(_unit);
		streamWriter.Write(value);
		streamWriter.Flush();
	}

	private string GenerateCode(CodeCompileUnit unit)
	{
		CSharpCodeProvider cSharpCodeProvider = new CSharpCodeProvider();
		StringBuilder stringBuilder = new StringBuilder();
		using (StringWriter writer = new StringWriter(stringBuilder))
		{
			using IndentedTextWriter writer2 = new IndentedTextWriter(writer);
			CodeGeneratorOptions codeGeneratorOptions = new CodeGeneratorOptions();
			codeGeneratorOptions.BlankLinesBetweenMembers = true;
			codeGeneratorOptions.ElseOnClosing = false;
			codeGeneratorOptions.VerbatimOrder = false;
			cSharpCodeProvider.GenerateCodeFromCompileUnit(unit, writer2, codeGeneratorOptions);
		}
		return stringBuilder.ToString();
	}
}
