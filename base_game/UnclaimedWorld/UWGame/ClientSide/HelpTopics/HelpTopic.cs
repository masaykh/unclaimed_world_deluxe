using System.Collections.Generic;
using UWGame.ClientSide.Interface.Layout;
using UWGame.SimSide;

namespace UWGame.ClientSide.HelpTopics;

public class HelpTopic : IGameData
{
	public LayoutElement[] FlowElements;

	public const string ColorHeader = "#COLORHEADER";

	public const string ColorHeaderOnLightBG = "#COLORHEADERDARK";

	public const string ColorMember = "#COLORMEMBER";

	public const string ColorDate = "#COLORDATE";

	public const string ColorItem = "#COLORTYPE";

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public void PreInitValidate(ref List<string> errors)
	{
	}

	public void Initialize()
	{
	}

	public void PostInitValidate(ref List<string> errors)
	{
	}

	public void PostDataCompleteInitialize()
	{
	}

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
	}
}
