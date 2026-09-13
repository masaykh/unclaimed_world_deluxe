using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

namespace UWGame.SimSide.Entities;

[DebuggerDisplay("{KeyName}")]
public class DetectionType : IGameData
{
	public string Comments;

	public DetectionFactor[] DetectionFactors;

	public bool DetectionDisabled;

	[XmlIgnore]
	public Dictionary<IDetectableType, DetectionFactor> DetectFactors = new Dictionary<IDetectableType, DetectionFactor>();

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public void Initialize()
	{
	}

	public void PreInitValidate(ref List<string> errors)
	{
	}

	public void PostInitValidate(ref List<string> errors)
	{
	}

	public void PostLoadContentInitialize(ref List<string> errors)
	{
		if (DetectionFactors == null)
		{
			return;
		}
		DetectionFactor[] detectionFactors = DetectionFactors;
		foreach (DetectionFactor detectionFactor in detectionFactors)
		{
			detectionFactor.PostLoadContentInitialize(ref errors);
			foreach (IDetectableType detectableType in detectionFactor.DetectableTypes)
			{
				if (!DetectFactors.ContainsKey(detectableType))
				{
					DetectFactors.Add(detectableType, detectionFactor);
				}
				else
				{
					EntityType.CreateValidationError(ref errors, $"An entry for the detectableType {detectableType.KeyName} already exists in the list of detect factors.");
				}
			}
		}
	}

	public void PostDataCompleteInitialize()
	{
	}

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
	}
}
