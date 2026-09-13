using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.ClientSide.Log;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class LogPanel
{
	public HUDLog HUDEventLog;

	public Window PlasticPanel;

	private List<LogAlert> activeAlerts = new List<LogAlert>();

	private List<LogAlert> inactiveAlerts = new List<LogAlert>();

	public const int AlertWindowOverlap = 4;

	private const int plasticEdgeWidth = 26;

	private int maxWidth = 100;

	public LogPanel(int xPos, int width)
	{
		int num = 43;
		PlasticPanel = new Window(The.InGameUI.gui);
		PlasticPanel.Skin = The.InGameUI.gui.GUISpriteSheet.GetSourceRectangle("log_panel_frame");
		PlasticPanel.CornerSize = 36;
		PlasticPanel.Margin = 0;
		PlasticPanel.IsMovable = false;
		PlasticPanel.Resizable = false;
		PlasticPanel.HasCloseButton = false;
		PlasticPanel.Position = new Point(xPos, The.Client.Controller.DrawArea.Height - num);
		PlasticPanel.WindowSize = new Vector2(width + 55, num);
		PlasticPanel.Level = Level.Bottom;
		PlasticPanel.Show();
		maxWidth = width + 100;
		HUDEventLog = new HUDLog(xPos + 26, width);
		HUDEventLog.DisplayWindow.Resize += HUDEventLog_Resize;
		HUDEventLog.DisplayWindow.Move += HUDEventLog_Move;
		ResizePlasticPart();
		for (int i = 0; i < The.Client.Controller.Options.MaxAlerts; i++)
		{
			inactiveAlerts.Add(new LogAlert());
		}
		The.Client.Log.NewEventAlert += Log_NewEventAlert;
	}

	private void HUDEventLog_Move(UIComponent sender)
	{
		ArrangeAlertWindows();
	}

	private void HUDEventLog_Resize(UIComponent sender)
	{
		ArrangeAlertWindows();
		ResizePlasticPart();
	}

	private void ResizePlasticPart()
	{
		PlasticPanel.Width = HUDEventLog.DisplayWindow.Width + 52;
		if (PlasticPanel.Width > maxWidth)
		{
			PlasticPanel.Width = maxWidth;
		}
		if (HUDEventLog.DisplayWindow.Width > maxWidth - 58)
		{
			HUDEventLog.DisplayWindow.Width = maxWidth - 58;
		}
	}

	private void Log_NewEventAlert(Event newEvent)
	{
		string text = HUDLog.FormatLogEntryText(newEvent, includeTime: false);
		string plainText = Label.GetPlainText(text);
		if (!activeAlerts.Exists((LogAlert a) => a.Text == plainText))
		{
			LogAlert logAlert;
			if (inactiveAlerts.Count > 0)
			{
				logAlert = inactiveAlerts[0];
				inactiveAlerts.RemoveAt(0);
			}
			else
			{
				logAlert = activeAlerts[0];
				activeAlerts.RemoveAt(0);
			}
			logAlert.Show(text);
			activeAlerts.Add(logAlert);
			ArrangeAlertWindows();
		}
	}

	public int GetTopOfAlertWindows()
	{
		if (activeAlerts.Count > 0)
		{
			return activeAlerts[activeAlerts.Count - 1].DisplayWindow.Y;
		}
		return GetFirstAlertPosition();
	}

	private int GetFirstAlertPosition()
	{
		return HUDEventLog.DisplayWindow.Y - 28 + 5;
	}

	public void Update(GameTime gameTime)
	{
		int count = activeAlerts.Count;
		for (int num = activeAlerts.Count - 1; num >= 0; num--)
		{
			LogAlert logAlert = activeAlerts[num];
			if (logAlert.IsExpired())
			{
				logAlert.Hide();
			}
		}
		if (count != activeAlerts.Count)
		{
			ArrangeAlertWindows();
		}
	}

	private void ArrangeAlertWindows()
	{
		int num = GetFirstAlertPosition();
		for (int i = 0; i < activeAlerts.Count; i++)
		{
			LogAlert logAlert = activeAlerts[i];
			logAlert.DisplayWindow.Y = num;
			logAlert.DisplayWindow.Width = HUDEventLog.DisplayWindow.Width;
			num -= 4;
		}
	}

	public void Retire(LogAlert alert)
	{
		activeAlerts.Remove(alert);
		inactiveAlerts.Add(alert);
	}

	public void Hide()
	{
		PlasticPanel.Hide();
		HUDEventLog.Hide();
	}
}
