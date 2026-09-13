namespace WindowSystem;

public interface ICanBeChecked
{
	bool IsChecked { get; set; }

	CheckedModes CheckedMode { get; set; }

	event ClickHandler Click;
}
