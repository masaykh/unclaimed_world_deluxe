using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UWGame.Control.Commands;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Scenarios;

namespace UWGame.Control.Replays;

public class ReplayData
{
	private List<ReplayFrame> frames;

	private int startIndex = -1;

	public int currentFrameIndex;

	public int? randomSeed;

	public int screenHeight;

	public int screenWidth;

	public StartGameParams StartGameParams;

	private int commandIndex;

	private List<Command> commandsToReplay = new List<Command>();

	public string GameVersion;

	private float? TimeLeftToPause;

	private Controller controller;

	public ReplayData(Controller controller)
	{
		this.controller = controller;
		currentFrameIndex = startIndex;
	}

	public double GetStartingSeconds()
	{
		return frames[0].GameTime.TotalGameTime.TotalSeconds;
	}

	private void serializer_UnknownNode(object sender, XmlNodeEventArgs e)
	{
		Console.WriteLine("UnknownNode Name: {0}", e.Name);
		Console.WriteLine("UnknownNode LocalName: {0}", e.LocalName);
		Console.WriteLine("UnknownNode Namespace URI: {0}", e.NamespaceURI);
		Console.WriteLine("UnknownNode Text: {0}", e.Text);
		XmlNodeType nodeType = e.NodeType;
		Console.WriteLine("NodeType: {0}", nodeType);
	}

	public void LoadReplay(string replayFolderPath, string replayFilePath, string commandFilePath, string gameParamsPath, float? timeToPause)
	{
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<Command>));
		xmlSerializer.UnknownNode += serializer_UnknownNode;
		using (TextReader textReader = new StreamReader(commandFilePath))
		{
			commandsToReplay = (List<Command>)xmlSerializer.Deserialize(textReader);
		}
		xmlSerializer.UnknownNode -= serializer_UnknownNode;
		frames = new List<ReplayFrame>();
		DataLoader.DeserializeObject<StartGameParams>(gameParamsPath, out StartGameParams);
		if (StartGameParams.StartScenarioParams != null)
		{
			StartGameParams.StartScenarioParams.LoadScenarioFromName();
		}
		BinaryFileReader binaryFileReader = new BinaryFileReader(replayFilePath);
		screenHeight = binaryFileReader.ReadInt();
		screenWidth = binaryFileReader.ReadInt();
		int num = binaryFileReader.ReadInt();
		if (num == 0)
		{
			randomSeed = null;
		}
		else
		{
			randomSeed = num;
		}
		int num2 = binaryFileReader.ReadInt();
		int num3 = binaryFileReader.ReadInt();
		int num4 = binaryFileReader.ReadInt();
		int num5 = binaryFileReader.ReadInt();
		GameVersion = num2 + "." + num3 + "." + num4 + "." + num5;
		while (!binaryFileReader.HasReachedEndOfFile())
		{
			ReplayFrame replayFrame = new ReplayFrame();
			frames.Add(replayFrame);
			replayFrame.LoadFrame(binaryFileReader);
		}
		binaryFileReader.Close();
		TimeLeftToPause = timeToPause;
	}

	public bool Update(Replayer.ReplayingMode mode)
	{
		if (mode != Replayer.ReplayingMode.InterfaceMode)
		{
			while (commandIndex < commandsToReplay.Count)
			{
				Command command = commandsToReplay[commandIndex];
				if (command.frameCalled != currentFrameIndex)
				{
					break;
				}
				command.Execute(giveClientFeedback: false);
				commandIndex++;
			}
		}
		if (TimeLeftToPause.HasValue)
		{
			TimeLeftToPause -= (float)frames[currentFrameIndex].GameTime.ElapsedGameTime.TotalSeconds;
			if (TimeLeftToPause < 0f)
			{
				TimeLeftToPause = null;
				controller.PauseReplay();
			}
		}
		return true;
	}

	public bool ReplayEnded()
	{
		if (currentFrameIndex < frames.Count)
		{
			return false;
		}
		return true;
	}

	public ReplayFrame GetCurrentFrame()
	{
		return frames[currentFrameIndex];
	}

	public void Reset()
	{
		currentFrameIndex = startIndex;
		commandIndex = 0;
	}
}
