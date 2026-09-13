using System;

namespace UWGame.SimSide.Entities;

[AttributeUsage(AttributeTargets.Class)]
public class ComponentAttribute : Attribute
{
	private static int counter;

	public static int Counter { get; set; }

	static ComponentAttribute()
	{
		Counter++;
	}
}
