using System;
using System.Globalization;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using UWGame.SimSide.AllGameData;

namespace UWGame.ClientSide;

public class Options
{
	public const string FileName = "Options.xml";

	public bool MusicEnabled = true;

	public float MusicVolume = 0.1f;

	public bool SoundEnabled = true;

	public float SoundFXVolume = 0.45f;

	public bool FullScreen = true;

	public bool Borderless;

	public bool HardwareModeSwitch;

	public int ResolutionWidth;

	public int ResolutionHeight;

	public bool EnableSteam = true;

	public string CultureString = CultureInfo.CurrentCulture.ToString();

	[XmlIgnore]
	public CultureInfo Culture;

	public Color ProductionStatusNoToolsAndNoInputsColor = "#e9a042".ColorFromHex();

	public Color ProductionStatusNoInputsOrNoToolsColor = "#ffcd76".ColorFromHex();

	public bool ShowAllTools;

	public int MinimumLogMessagesPerPage = 20;

	public int TalkLogMessagesToShow = 60;

	public int MaxAlerts = 5;

	public float AlertLifetime = 5f;

	public bool PlayVideo = true;

	public bool RecordGame;

	public int MaxNoOfRecordedGamesToKeep = 20;

	public bool LimitFramerateWhenPaused = true;

	public int TargetFramerateWhenPaused = 25;

	public bool SynchronizeWithVerticalRetrace = true;

	public float KeyScrollSpeedPerSecond = 1200f;

	public float ZoomFactor = 1f;

	public Keys KeyScrollLeft = Keys.A;

	public Keys KeyScrollUp = Keys.W;

	public Keys KeyScrollRight = Keys.D;

	public Keys KeyScrollDown = Keys.S;

	public Keys KeyPause1 = Keys.Pause;

	public Keys KeyPause2 = Keys.Space;

	public Keys KeyPause3 = Keys.P;

	public Keys KeyGameSpeed1 = Keys.D1;

	public Keys KeyGameSpeed2 = Keys.D2;

	public Keys KeyGameSpeed3 = Keys.D3;

	public Keys ToggleIngameMenu = Keys.M;

	public Keys CloseRosterPanel = Keys.Escape;

	public bool ShowPerformanceWarning = true;

	public void Write()
	{
		DataLoader.SerializeObject(this, "", "Options.xml", Config.DataType.UserSettings);
	}

	public void SetDefaultResolution()
	{
		try
		{
			ResolutionWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
			ResolutionHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
		}
		catch (Exception)
		{
			ResolutionWidth = 1024;
			ResolutionHeight = 800;
			FullScreen = false;
		}
	}

	public bool Validate()
	{
		if (MaxAlerts > 0)
		{
			return TalkLogMessagesToShow > 0;
		}
		return false;
	}
}
