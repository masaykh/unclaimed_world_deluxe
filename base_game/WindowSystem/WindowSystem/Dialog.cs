namespace WindowSystem;

public class Dialog : Window
{
	private DialogResult result;

	public DialogResult DialogResult => result;

	public Dialog(GUIManager guiManager)
		: base(guiManager)
	{
		result = DialogResult.Cancel;
	}

	protected void SetDialogResult(DialogResult result)
	{
		this.result = result;
	}
}
