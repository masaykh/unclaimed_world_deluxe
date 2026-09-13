using System;
using System.Collections.Generic;
using System.Reflection;

namespace WindowSystem;

public class UIComponentSkin
{
	public Type ComponentType;

	public List<PropertyInfo> Properties = new List<PropertyInfo>();

	public List<string> Values = new List<string>();
}
