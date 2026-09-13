using WindowSystem;

namespace UWGame.ClientSide.Interface.Editor.MapTools;

public abstract class ToolOption : UIComponent
{
	public abstract int Order { get; }

	public ToolOption(GUIManager gui)
		: base(gui)
	{
	}
}
