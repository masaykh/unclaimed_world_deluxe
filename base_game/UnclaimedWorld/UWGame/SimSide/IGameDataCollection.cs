using System.Collections.Generic;

namespace UWGame.SimSide;

public interface IGameDataCollection
{
	int Order { get; }

	void PostDataCompleteInitialize();

	void PostDataCompleteValidate(Dictionary<string, List<string>> allPostInitValidationErrors);

	void PreDataCompleteValidate(Dictionary<string, List<string>> allPostInitValidationErrors);

	IGameData Get(string key);
}
