using System.Collections.Generic;

namespace UWGame.Control.Commands;

public class CommandInvoker
{
	private List<Command> commands = new List<Command>();

	public void Execute(Command command)
	{
		command.Execute();
	}

	public void Store(Command command)
	{
		commands.Add(command);
	}
}
