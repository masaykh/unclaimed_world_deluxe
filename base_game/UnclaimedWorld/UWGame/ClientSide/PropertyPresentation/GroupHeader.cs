using System.Collections.Generic;

namespace UWGame.ClientSide.PropertyPresentation;

public class GroupHeader
{
	public Presentation Presentation;

	public GroupNode.HeaderType GetHeaderType()
	{
		if (Presentation.Caption != null && Presentation.Caption.StaticString != null && Presentation.PropertyNameForValue == null)
		{
			return GroupNode.HeaderType.Static;
		}
		return GroupNode.HeaderType.Dynamic;
	}

	public void PreInitValidate(ref List<string> listOfErrors)
	{
		Presentation.PreInitValidate(ref listOfErrors);
	}
}
