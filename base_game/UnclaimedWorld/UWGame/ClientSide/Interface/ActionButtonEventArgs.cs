using System;
using UWGame.SimSide.Processes;

namespace UWGame.ClientSide.Interface;

public class ActionButtonEventArgs : EventArgs
{
	public ProcessType ProcessType;

	public ActionButtonEventArgs(ProcessType item)
	{
		ProcessType = item;
	}
}
