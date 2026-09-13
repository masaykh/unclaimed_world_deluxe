using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;
using UWGame.Client.Audio;
using UWGame.Client.Particles;
using UWGame.ClientSide.Renderables;

namespace UWGame.SimSide.Entities;

[DebuggerDisplay("Conditions:{Conditions} Forbiddens:{Forbiddens.StateNames}")]
public class ClientStateInfo : IStateInfo
{
	public RenderAsBillboardType[] RenderAsBillboardType;

	public RenderAsGroundSpriteType RenderAsGroundSpriteType;

	public ParticleEmitterEffect[] ParticleEmitters;

	public LightingType[] LightingTypes;

	public NormalDistribution DelayBetweenSounds;

	public string[] Sounds;

	public BitMask64 Conditions { get; set; }

	public BitMask64 Forbiddens { get; set; }

	[XmlIgnore]
	public SoundData[] SoundDatas { get; private set; }

	public bool Test(StateModifier state)
	{
		if (Conditions != null && Conditions.Test(state))
		{
			return true;
		}
		return false;
	}

	internal void Initialize()
	{
		if (ParticleEmitters != null)
		{
			ParticleEmitterEffect[] particleEmitters = ParticleEmitters;
			for (int i = 0; i < particleEmitters.Length; i++)
			{
				particleEmitters[i].Initialize();
			}
		}
		if (Sounds != null)
		{
			SoundDatas = new SoundData[Sounds.Length];
			for (int j = 0; j < Sounds.Length; j++)
			{
				SoundDatas[j] = GameData.Instance.AllSoundData[Sounds[j]];
			}
		}
	}

	private bool RequiresGhostedImage(EntityType entityType)
	{
		if (entityType.GetUsesMemory() || entityType.StructureType != null)
		{
			return true;
		}
		return false;
	}

	internal void PostLoadContentValidate(EntityType parent, ref List<string> listOfErrors)
	{
		if (RenderAsBillboardType != null)
		{
			RenderAsBillboardType[] renderAsBillboardType = RenderAsBillboardType;
			foreach (RenderAsBillboardType renderAsBillboardType2 in renderAsBillboardType)
			{
				if (!string.IsNullOrEmpty(renderAsBillboardType2.AssetName))
				{
					if (!GameData.Instance.BillboardSpriteSheet.TryGetSourceRectangle(renderAsBillboardType2.AssetName, out var spriteRect))
					{
						EntityType.CreateValidationError(ref listOfErrors, "Billboard asset " + renderAsBillboardType2.AssetName + " not found in BillboardSpriteSheet.");
					}
					if (RequiresGhostedImage(parent) && !The.Client.Renderer.GhostedStructuresSpriteSheet.TryGetSourceRectangle(renderAsBillboardType2.AssetName, out spriteRect))
					{
						EntityType.CreateValidationError(ref listOfErrors, "Billboard asset " + renderAsBillboardType2.AssetName + " not found in GhostedStructuresSpriteSheet.");
					}
				}
			}
		}
		if (RenderAsGroundSpriteType != null && !The.Client.FlatSpriteSheet.TryGetSourceRectangle(RenderAsGroundSpriteType.AssetName, out var _))
		{
			EntityType.CreateValidationError(ref listOfErrors, "Flat sprite asset " + RenderAsGroundSpriteType.AssetName + " not found.");
		}
	}
}
