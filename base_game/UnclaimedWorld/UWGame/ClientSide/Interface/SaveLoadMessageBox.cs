using System;
using System.Threading;
using Kensei.Dev;
using Microsoft.Xna.Framework;

namespace UWGame.ClientSide.Interface;

public class SaveLoadMessageBox : MessageBox
{
	public enum Mode
	{
		None,
		Save,
		LoadAfterSave,
		Load
	}

	private Thread saveThread;

	private Mode mode;

	private bool saveThreadFinished;

	private string savePath;

	private TimeSpan startedTimeStamp;

	public SaveLoadMessageBox(CommonInterface intf)
		: base(intf)
	{
	}

	public void StartLoad(bool isLoadAfterSave)
	{
		SetStartTimestamp();
		if (isLoadAfterSave)
		{
			mode = Mode.LoadAfterSave;
		}
		else
		{
			mode = Mode.Load;
		}
	}

	private void SetStartTimestamp()
	{
		startedTimeStamp = The.Client.GameTime.TotalGameTime;
	}

	public void StartSave(string savePath)
	{
		this.savePath = savePath;
		if (mode != Mode.Save)
		{
			SetStartTimestamp();
			saveThreadFinished = false;
			saveThread = new Thread(SaveInThread);
			saveThread.IsBackground = true;
			saveThread.Start();
			mode = Mode.Save;
			ShowMessage("Saving. Please wait...", null, modal: true, ButtonOptions.None);
		}
	}

	private void SaveInThread()
	{
		The.Sim.DoSave(savePath);
		saveThreadFinished = true;
	}

	public override void Update(GameTime elapsed)
	{
		base.Update(elapsed);
		if (mode == Mode.Save)
		{
			UpdateProgress(elapsed);
			if (saveThreadFinished)
			{
				if (saveThread != null && saveThread.IsAlive && Thread.CurrentThread != saveThread)
				{
					saveThread.Join();
				}
				LoadGameDirectlyInGame(savePath, isLoadAfterSave: true);
				mode = Mode.None;
			}
		}
		else if (mode == Mode.Load || mode == Mode.LoadAfterSave)
		{
			UpdateProgress(elapsed);
		}
	}

	private void UpdateProgress(GameTime elapsed)
	{
		TimeSpan timeSpan = elapsed.TotalGameTime - startedTimeStamp;
		string text = "";
		switch (mode)
		{
		case Mode.Save:
			text = "Saving (1/2). Please wait... ";
			break;
		case Mode.LoadAfterSave:
			text = "Saving (2/2). Please wait... ";
			break;
		case Mode.Load:
			text = "Loading. Please wait... ";
			break;
		}
		int count = Common.ClampBottom((int)timeSpan.TotalSeconds, 0);
		base.Text = text + new string('|', count);
	}

	public static void LoadGameDirectlyInGame(string savePath, bool isLoadAfterSave)
	{
		Kensei.Dev.Options.Destroy();
		The.IngameLoadScreen.Interface.SaveLoadMessageBox.StartLoad(isLoadAfterSave);
		The.Sim.LoadSavedGame(savePath);
	}
}
