using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide;
using WindowSystem;

namespace UWGame.ClientSide.Particles;

[DebuggerDisplay("{KeyName}")]
public class ParticleSystemType : IGameData
{
	[XmlIgnore]
	public Texture2D Texture;

	public Vector2 Origin;

	public const int AlphaBlendDrawOrder = 100;

	public const int AdditiveDrawOrder = 200;

	public float SystemScale = 1f;

	public bool DecelerateParticlesToZeroBeforeTheyDie;

	public float? TimeBetweenEmitting;

	public bool EmitParticlesManually;

	public bool RemoveEmittersAfterLastParticleExpires;

	public bool ParticlesNeverExpire;

	public bool IsAffectedByWind = true;

	public float MinWindAccelerationFactor = 0.5f;

	public float MaxWindAccelerationFactor = 1f;

	public bool rotateRandomlyAtStart = true;

	public int MinNumParticles;

	public int MaxNumParticles;

	public Color StartColor = Color.White;

	public Color endColor = Color.White;

	public string AnimationKey;

	public string TextureFilename;

	public float minInitialSpeed;

	public float maxInitialSpeed;

	public float minAcceleration;

	public float maxAcceleration;

	public float minRotationSpeed;

	public float maxRotationSpeed;

	public float minLifetime;

	public float maxLifetime;

	public float minScale;

	public float maxScale;

	public BlendState spriteBlendState;

	public float StartMeanMoveDirectionInDegrees;

	public float StartMaxMoveDirectionDifferenceInDegrees;

	public int NoOfEmitters { get; private set; }

	[XmlIgnore]
	public Animation2D Animation { get; private set; }

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	[XmlIgnore]
	public float? MeanDirectionInRadians { get; private set; }

	[XmlIgnore]
	public float? MaxDirectionDifferenceInRadians { get; private set; }

	public ParticleSystemType()
	{
	}

	public ParticleSystemType(string key, int howManyEmitters)
	{
		KeyName = key;
		NoOfEmitters = howManyEmitters;
	}

	public void LoadContent(ContentManager content)
	{
		if (TextureFilename != null)
		{
			Texture = content.Load<Texture2D>(TextureFilename);
			Origin.X = Texture.Width / 2;
			Origin.Y = Texture.Height / 2;
		}
	}

	public void PreInitValidate(ref List<string> errors)
	{
	}

	public void PostLoadContentInitialize()
	{
		if (!string.IsNullOrEmpty(AnimationKey))
		{
			Animation = GameData.Instance.Animation2Ds[AnimationKey];
		}
	}

	public void Initialize()
	{
		MeanDirectionInRadians = MathHelper.ToRadians(StartMeanMoveDirectionInDegrees);
		MaxDirectionDifferenceInRadians = MathHelper.ToRadians(StartMaxMoveDirectionDifferenceInDegrees);
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
