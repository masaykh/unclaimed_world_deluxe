using System;
using WindowSystem;

namespace UWGame.ClientSide.PropertyPresentation;

[System.Xml.Serialization.XmlType("PresentationSorting")]
public class Sorting
{
	public Grid.Sorting? SortingDirection;

	public SortingMethod SortingMethod;

	public static Func<UIComponent, object> GetSelector(SortingMethod method)
	{
		Func<UIComponent, object> result = null;
		switch (method)
		{
		case SortingMethod.StaticSortOrder:
			result = (UIComponent i) => i.OrderByTag2;
			break;
		case SortingMethod.NumberResult:
			result = (UIComponent i) => i.OrderByTag1 ?? ((object)0f);
			break;
		case SortingMethod.NumberResultMiddleDistance:
			result = (UIComponent i) => GetMiddleDistanceForValue((float)(i.OrderByTag1 ?? ((object)0.5f)));
			break;
		}
		return result;
	}

	public Func<UIComponent, object> GetSelector()
	{
		return GetSelector(SortingMethod);
	}

	private static float GetMiddleDistanceForValue(float value)
	{
		return Math.Abs(value - 0.5f);
	}
}
