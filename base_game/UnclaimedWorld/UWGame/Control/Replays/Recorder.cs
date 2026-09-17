using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using UWGame.Contol;
using UWGame.Control.Commands;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Scenarios;

namespace UWGame.Control.Replays;

public class Recorder
{
	public bool isRecording;

	private BinaryWriter replayWriter;

	private StreamWriter randomWriter;

	private StreamWriter stateWriter;

	private PositionableStreamWriter commandWriter;

	private Controller controller;

	public int currentFrameIndex;

	private string closingTag;

	public bool RecordDuringPlay;

	private int savedMessageIndex;

	/// <summary>
	/// PORT DEVIATION 20. The draw-label trace. randomWriter and stateWriter above are the
	/// studio's own fields for this - declared, closed in StopRecording, and never once opened or
	/// written to. Rather than revive two half-specified streams this writes one file, under the
	/// name they chose for it, in a format something can diff. See ReplayTrace.
	/// </summary>
	private ReplayTrace trace;

	/// <summary>PORT DEVIATION 21. The per-frame count of time-sliced AI cycles. See CycleSchedule.</summary>
	private CycleSchedule cycleSchedule;

	/// <summary>The folder this recording is being written into, for the trace to share.</summary>
	private string replayFolderPath;

	public Recorder(Controller controller)
	{
		this.controller = controller;
	}

	public void RecordCommand(Command command)
	{
		command.frameCalled = currentFrameIndex;
		commandWriter.SetPositionFromEnd(-closingTag.Length);
		commandWriter.writer.Write(SerializeCommand(command));
		commandWriter.writer.Write(closingTag);
		commandWriter.writer.Flush();
	}

	public string SerializeCommand(Command toSerialize)
	{
		List<Command> list = new List<Command>();
		list.Add(toSerialize);
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.OmitXmlDeclaration = true;
		xmlWriterSettings.Indent = true;
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<Command>));
		StringWriter stringWriter = new StringWriter();
		XmlWriter xmlWriter = XmlWriter.Create(stringWriter, xmlWriterSettings);
		xmlSerializer.Serialize(xmlWriter, list);
		string text = stringWriter.ToString();
		return text.Substring(text.IndexOf('>') + 1).Replace("</ArrayOfCommand>", "") + "\r\n";
	}

	private void OpenNewReplayFiles(StartGameParams startGameParams)
	{
		string dataFolderPath = Config.GetDataFolderPath(Config.DataType.Replays);
		if (!Directory.Exists(dataFolderPath))
		{
			Directory.CreateDirectory(dataFolderPath);
		}
		string text = DateTime.Now.ToString("yyyyMMdd");
		string text2 = DateTime.Now.TimeOfDay.Hours.ToString();
		string text3 = DateTime.Now.TimeOfDay.Minutes.ToString();
		string text4 = startGameParams.ToString() + " " + text + " " + text2 + " " + text3 + " " + UnclaimedWorld.GetVersionAsString();
		string text5 = Config.GetDataFolderPath(Config.DataType.Replays, text4);
		if (Directory.Exists(text5))
		{
			string text6 = text5;
			int num = 1;
			while (Directory.Exists(text6))
			{
				num++;
				text6 = text5 + num;
			}
			text5 = text6;
			text4 += num;
		}
		Directory.CreateDirectory(text5);
		string dataFolderPath2 = Config.GetDataFolderPath(Config.DataType.Replays, text4, "Replay.UWRep");
		string dataFolderPath3 = Config.GetDataFolderPath(Config.DataType.Replays, text4, "Commands.xml");
		// Was: both of these called, and both results DISCARDED. The paths were computed and
		// thrown away, which is the clearest sign that the trace was cut rather than never
		// planned. RandomCalls.UWRepRand is written now; AIStates.UWRepStates is still unwritten -
		// see Recorder.SaveEntityAIState.
		replayFolderPath = Config.GetDataFolderPath(Config.DataType.Replays, text4);
		Config.GetDataFolderPath(Config.DataType.Replays, text4, "AIStates.UWRepStates");
		DataLoader.SerializeObject(startGameParams, text4, "GameParams.xml", Config.DataType.Replays);
		FileStream output = File.Create(dataFolderPath2);
		replayWriter = new BinaryWriter(output);
		commandWriter = new PositionableStreamWriter(dataFolderPath3);
		closingTag = "</ArrayOfCommand>";
		commandWriter.writer.Write("<ArrayOfCommand xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">");
		commandWriter.writer.Write(closingTag);
		commandWriter.writer.Flush();
	}

	public void SaveStartGameParams(StartGameParams startGameParams)
	{
		if (RecordDuringPlay)
		{
			OpenNewReplayFiles(startGameParams);
		}
	}

	public void SaveEntityAIState(string entityAIState)
	{
	}

	/// <summary>
	/// One random draw, by the label its call site passed. Was empty; every RandomGenerator method
	/// has been calling it, with a useful label, the whole time.
	/// </summary>
	public void SaveRandomGet(string getMessage)
	{
		trace?.Draw(getMessage);
	}

	public void Initialize()
	{
	}

	public void Update(GameTime gameTime)
	{
		if (isRecording)
		{
			SaveFrame(gameTime);
		}
	}

	public void AdvanceFrame()
	{
		if (isRecording)
		{
			currentFrameIndex++;
		}
	}

	public void StartRecording(int? randomSeed)
	{
		// PORT FIX. replayWriter is opened by SaveStartGameParams, and
		// Controller.SaveScenarioForReplay SKIPS that when startGameParams.StartGameEditorParams
		// is set - the map editor and the TEST MAP button. Those start with Sim.Mode still Game,
		// so Controller.LoadingFinished came straight here and dereferenced a null writer:
		//
		//   Object reference not set to an instance of an object.
		//      at UWGame.Control.Replays.Recorder.StartRecording(Nullable`1 randomSeed)
		//      at UWGame.Control.Controller.LoadingFinished()
		//
		// It could not happen in the retail game because neither Options.RecordGame nor the TEST
		// MAP button was reachable; making both reachable made this reachable too. Nor can it be
		// fixed by opening the files here: an editor start has no scenario to write into
		// GameParams.xml, so the recording could never be loaded back. The session is not
		// recorded, and says so once rather than taking the game down.
		if (replayWriter == null)
		{
			isRecording = false;
			UnclaimedWorld.LogError(
				"This session is not being recorded. A recording needs a scenario to write into "
				+ "GameParams.xml so the replay can be started again, and a game opened from the "
				+ "map editor or the TEST MAP button has none. Start from NEW GAME, or from the "
				+ "dev panel's TEST, to record.",
				"Replay recording");
			return;
		}
		int backBufferHeight = The.Sim.Controller.GraphicsDevice.PresentationParameters.BackBufferHeight;
		int backBufferWidth = The.Sim.Controller.GraphicsDevice.PresentationParameters.BackBufferWidth;
		isRecording = true;
		currentFrameIndex = 0;
		if (replayFolderPath != null)
		{
			trace = new ReplayTrace();
			trace.BeginRecording(replayFolderPath);
			cycleSchedule = new CycleSchedule();
			cycleSchedule.BeginRecording(replayFolderPath);
		}
		replayWriter.Write(backBufferHeight);
		replayWriter.Write(backBufferWidth);
		replayWriter.Write(randomSeed ?? 0);
		replayWriter.Write(MainMenuInterface.GetMajor());
		replayWriter.Write(MainMenuInterface.GetMinor());
		replayWriter.Write(MainMenuInterface.GetBuild());
		replayWriter.Write(MainMenuInterface.GetRevision());
		replayWriter.Flush();
	}

	public void CleanupOldRecordedFiles()
	{
		if (controller.Options.MaxNoOfRecordedGamesToKeep < 0)
		{
			return;
		}
		GetListOfReplayFolders(Config.GetDataFolderPath(Config.DataType.Replays), out var directories);
		IOrderedEnumerable<DirectoryInfo> orderedEnumerable = directories.OrderByDescending((DirectoryInfo i) => i.CreationTimeUtc);
		int num = 0;
		foreach (DirectoryInfo item in orderedEnumerable)
		{
			if (num >= controller.Options.MaxNoOfRecordedGamesToKeep)
			{
				try
				{
					item.Delete(recursive: true);
				}
				catch (Exception)
				{
				}
			}
			num++;
		}
	}

	public static List<string> GetListOfReplayFolders(string replayFolderPath, out List<DirectoryInfo> directories)
	{
		List<string> list = new List<string>();
		directories = new List<DirectoryInfo>();
		string[] directories2 = Directory.GetDirectories(replayFolderPath);
		for (int i = 0; i < directories2.Length; i++)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(directories2[i]);
			string name = directoryInfo.Name;
			list.Add(name);
			directories.Add(directoryInfo);
		}
		return list;
	}

	public void StopRecording()
	{
		savedMessageIndex = 0;
		isRecording = false;
		trace?.Dispose();
		trace = null;
		cycleSchedule?.Dispose();
		cycleSchedule = null;
		if (replayWriter != null)
		{
			replayWriter.Close();
		}
		if (stateWriter != null)
		{
			stateWriter.Close();
		}
		if (randomWriter != null)
		{
			randomWriter.Close();
		}
		if (commandWriter != null)
		{
			commandWriter.Close();
		}
	}

	public void SaveFrame(GameTime gameTime)
	{
		replayWriter.Write(gameTime.TotalGameTime.Ticks);
		replayWriter.Write(gameTime.ElapsedGameTime.Ticks);
		replayWriter.Write(controller.InputData.mouseX);
		replayWriter.Write(controller.InputData.mouseY);
		replayWriter.Write(controller.InputData.LeftButtonDown);
		replayWriter.Write(controller.InputData.RightButtonDown);
		foreach (Keys value in Enum.GetValues(typeof(Keys)))
		{
			if (controller.InputData.IsKeyDown(value) != controller.InputData.WasKeyDown(value))
			{
				replayWriter.Write(value: true);
				replayWriter.Write((int)value);
			}
		}
		replayWriter.Write(value: false);
		ReplayVerificationData verificationData = controller.RetrieveVerificationData();
		verificationData.Write(replayWriter);
		replayWriter.Flush();
		cycleSchedule?.Record(currentFrameIndex, The.Sim?.CycleManager == null ? 0 : The.Sim.CycleManager.CyclesLastUpdate);
		trace?.EndFrame(currentFrameIndex, verificationData);
	}
}
