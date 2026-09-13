using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using UWGame.SimSide;

namespace UWGame.Client.Audio;

[DebuggerDisplay("{KeyName}")]
public class SoundData : IGameData
{
	public string Sound;

	public float Volume;

	public NormalDistribution RandomPitchChange;

	public bool PlayMaxOneInstance;

	[XmlIgnore]
	public SoundEffect SoundEffect;

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public void LoadContent(ContentManager content)
	{
		SoundEffect = content.Load<SoundEffect>("Sounds\\" + Sound);
	}

	public void PreInitValidate(ref List<string> errors)
	{
	}

	public void Initialize()
	{
	}

	public void PostInitValidate(ref List<string> errors)
	{
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
