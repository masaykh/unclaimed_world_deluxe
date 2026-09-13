using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.LCD;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Controls;

public class ErrorAndMessagePanel
{
	private LCDInnerPanel lcdErrorMessagePanel;

	private ErrorsAndMessages errorsAndMessages;

	public int ContentHeight => lcdErrorMessagePanel.ContentHeight;

	public int Height => lcdErrorMessagePanel.Height;

	public bool IsShowingError => errorsAndMessages.IsShowingError;

	public ErrorAndMessagePanel(UIComponent lcdSurface)
	{
		lcdErrorMessagePanel = new LCDInnerPanel(lcdSurface.guiManager, lcdSurface.Width, includeDecor: true, 1f);
		lcdSurface.Add(lcdErrorMessagePanel.Panel);
		lcdErrorMessagePanel.ContentHeight = 30;
		lcdErrorMessagePanel.Panel.Y = lcdSurface.Height - 35;
		lcdErrorMessagePanel.VerticalContentPadding = 8;
		errorsAndMessages = new ErrorsAndMessages(lcdSurface, lcdSurface.guiManager, 10, lcdErrorMessagePanel.Panel.Y + 10);
	}

	public void ShowErrors(List<string> errors)
	{
		if (errors != null && errors.Count > 0)
		{
			ShowError(errors[0]);
		}
	}

	public void ShowError(string error, string errorTooltip = null)
	{
		errorsAndMessages.ShowError(error, errorTooltip);
		TintErrorPanel(Color.LightSalmon);
	}

	public void Clear()
	{
		TintErrorPanel(new Color(255, 255, 255, 255));
		errorsAndMessages.Hide();
	}

	private void TintErrorPanel(Color color)
	{
		lcdErrorMessagePanel.TintPanel(color);
	}
}
