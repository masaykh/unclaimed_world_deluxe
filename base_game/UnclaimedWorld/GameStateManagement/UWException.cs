using System;

namespace GameStateManagement;

public class UWException : Exception
{
	public string Title;

	public bool IncludePasteInstructions = true;

	public UWException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
