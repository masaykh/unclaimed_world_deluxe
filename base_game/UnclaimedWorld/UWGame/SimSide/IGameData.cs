using System.Collections.Generic;

namespace UWGame.SimSide;

public interface IGameData
{
	string KeyName { get; }

	string Name { get; }

	bool DeleteRecord { get; }

	void PreInitValidate(ref List<string> errors);

	void Initialize();

	void PostInitValidate(ref List<string> errors);

	void PreDataCompleteValidate(ref List<string> listOfErrors);

	void PostDataCompleteInitialize();

	void PostDataCompleteValidate(ref List<string> listOfErrors);
}
