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
		Config.GetDataFolderPath(Config.DataType.Replays, text4, "RandomCalls.UWRepRand");
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

	public void SaveRandomGet(string getMessage)
	{
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
		int backBufferHeight = The.Sim.Controller.GraphicsDevice.PresentationParameters.BackBufferHeight;
		int backBufferWidth = The.Sim.Controller.GraphicsDevice.PresentationParameters.BackBufferWidth;
		isRecording = true;
		currentFrameIndex = 0;
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
		controller.RetrieveVerificationData().Write(replayWriter);
		replayWriter.Flush();
	}
}
