using UWGame.SimSide.AllGameData;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide;

public class Progress
{
	public const string FileName = "Progress.xml";

	public SerializableHashSet<string> DisplayedHints = new SerializableHashSet<string>();

	public void Write()
	{
		DataLoader.SerializeObject(this, "", "Progress.xml", Config.DataType.UserSettings);
	}
}
