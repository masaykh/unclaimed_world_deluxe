using System.Xml.Serialization;

namespace UWGame.SimSide.Scenarios;

public class Scenario
{
	public string Name;

	public string DisplayName;

	public string SummaryDescription;

	public string Description;

	public string ThumbnailImage;

	public bool Allow32Bit;

	public string Image;

	public string MapKey;

	public DateAndTime.TimeDateYear TimeDateYear;

	public bool IsInDevelopment;

	public int SortOrder;

	public MapSize MapSize;

	[XmlIgnore]
	public ScenarioData ScenarioData;

	[XmlIgnore]
	public Source Source { get; private set; }

	public static string GetMapSizeAsString(MapSize size)
	{
		return size switch
		{
			MapSize.Small => "Small", 
			MapSize.Medium => "Medium", 
			MapSize.Large => "Large", 
			_ => "", 
		};
	}

	public void SetRGSource()
	{
		Source = Source.RefactoredGames;
	}

	public void RegisterEvents()
	{
		if (ScenarioData.ConditionalEvents != null)
		{
			string[] conditionalEvents = ScenarioData.ConditionalEvents;
			foreach (string eventKey in conditionalEvents)
			{
				The.Sim.PlaySite.EventManager.AddPolledEvent(eventKey);
			}
		}
	}
}
