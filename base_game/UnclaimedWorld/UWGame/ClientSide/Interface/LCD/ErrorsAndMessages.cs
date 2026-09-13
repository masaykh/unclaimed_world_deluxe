using WindowSystem;

namespace UWGame.ClientSide.Interface.LCD;

public class ErrorsAndMessages
{
	private Label lblMessages;

	private Label lblErrors;

	private UIComponent surface;

	public int Bottom => lblMessages.Bottom;

	public int Right => lblMessages.Right;

	public int Y => lblMessages.Y;

	public bool IsShowingError
	{
		get
		{
			if (surface.Contains(lblErrors))
			{
				return !string.IsNullOrEmpty(lblErrors.Text);
			}
			return false;
		}
	}

	public ErrorsAndMessages(UIComponent lcdSurface, GUIManager gui, int x, int y, Label.LabelType labelType = Label.LabelType.LCDNormal)
	{
		surface = lcdSurface;
		lblMessages = new Label(gui);
		lblMessages.Init(labelType);
		lblMessages.X = x;
		lblMessages.Y = y;
		lblErrors = new Label(gui);
		lblErrors.Init(Label.LabelType.LCDError);
		lblErrors.X = lblMessages.X;
		lblErrors.Y = lblMessages.Y;
	}

	public void ShowError(string text, string tooltip = null)
	{
		surface.Add(lblErrors);
		surface.Remove(lblMessages);
		lblErrors.Text = text;
		lblErrors.ToolTip = tooltip;
		lblErrors.FitToText();
	}

	public void ShowMessage(string text, string tooltip = null)
	{
		surface.Add(lblMessages);
		surface.Remove(lblErrors);
		lblMessages.Text = text;
		lblMessages.ToolTip = tooltip;
		lblMessages.FitToText();
	}

	public void Hide()
	{
		surface.Remove(lblErrors);
		surface.Remove(lblMessages);
	}
}
