using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UWGame.SimSide.AllGameData.Scenarios;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Scenarios;

public class StartScenarioParams : ISnapshot
{
	[XmlIgnore]
	public Scenario Scenario;

	private string scenarioName;

	public Source Source;

	public SerializableDictionary<string, Option> Options;

	private Dictionary<string, string> snapshotOptions;

	public string MainDifficultyKey;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public string ScenarioName
	{
		get
		{
			return scenarioName;
		}
		set
		{
			scenarioName = value;
		}
	}

	public bool IsSnapshotted { get; set; }

	public override string ToString()
	{
		return ScenarioName;
	}

	public string GetLoadingDialogText()
	{
		string text = Scenario.ScenarioData.LoadingDialogText ?? "";
		foreach (Option item in from o in Options.Values.ToList()
			orderby o.LoadingDialogTextOrder ?? 0
			select o)
		{
			if (item.LoadingDialogText != null)
			{
				text = ((item.LoadingDialogTextMode != Option.LoadingDialogTextModes.Append) ? item.LoadingDialogText : (text + item.LoadingDialogText));
				text += "\n\n";
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		return text;
	}

	public void LoadScenarioFromName()
	{
		Scenario = AllScenarioLoader.LoadScenarioHeader(ScenarioName, Source);
		Scenario.ScenarioData = AllScenarioLoader.LoadScenarioData(Scenario);
		if (The.Client != null)
		{
			Scenario.ScenarioData.LoadContent(The.Client.Content);
		}
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		ScenarioName = sn.DoString(ScenarioName);
		Source = sn.DoEnum(Source);
		if (version >= (Snapshotter.Version)2u)
		{
			MainDifficultyKey = sn.DoString(MainDifficultyKey);
		}
		else
		{
			MainDifficultyKey = null;
		}
		if (sn.mode != Snapshotter.Mode.Load)
		{
			snapshotOptions = Options.ToDictionary((KeyValuePair<string, Option> k) => k.Key, (KeyValuePair<string, Option> k) => k.Value.KeyName);
		}
		snapshotOptions = sn.DoDictionary(snapshotOptions);
		sn.Ignore(Scenario);
		sn.Ignore(Options);
		return this;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion((Snapshotter.Version)2u);
		return version;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		LoadScenarioFromName();
		Options = new SerializableDictionary<string, Option>();
		foreach (KeyValuePair<string, string> kvp in snapshotOptions)
		{
			Option value = Scenario.ScenarioData.OptionSets.First((OptionSet s) => s.KeyName == kvp.Key).Options.First((Option o) => o.KeyName == kvp.Value);
			Options.Add(kvp.Key, value);
		}
	}
}
