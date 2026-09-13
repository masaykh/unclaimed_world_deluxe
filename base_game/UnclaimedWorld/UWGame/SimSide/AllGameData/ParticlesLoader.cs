using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.ClientSide.Particles;

namespace UWGame.SimSide.AllGameData;

public class ParticlesLoader
{
	public static List<ParticleSystemType> Init()
	{
		List<ParticleSystemType> obj = new List<ParticleSystemType>
		{
			new ParticleSystemType("smallFire", 10)
			{
				minInitialSpeed = 0f,
				maxInitialSpeed = 0f,
				minAcceleration = 0f,
				maxAcceleration = 0f,
				StartMeanMoveDirectionInDegrees = 0f,
				StartMaxMoveDirectionDifferenceInDegrees = 0f,
				TextureFilename = "explosion",
				IsAffectedByWind = false,
				rotateRandomlyAtStart = false,
				minRotationSpeed = 0f,
				maxRotationSpeed = 0f,
				minScale = 1f,
				maxScale = 1f,
				MinNumParticles = 1,
				MaxNumParticles = 1,
				ParticlesNeverExpire = true,
				spriteBlendState = BlendState.Additive,
				AnimationKey = "campfireSmall"
			},
			new ParticleSystemType("tinyFire", 10)
			{
				minInitialSpeed = 1f,
				maxInitialSpeed = 3f,
				minAcceleration = 0f,
				maxAcceleration = 0f,
				StartMeanMoveDirectionInDegrees = 270f,
				StartMaxMoveDirectionDifferenceInDegrees = 3f,
				TextureFilename = "explosion",
				MaxWindAccelerationFactor = 0.5f,
				MinWindAccelerationFactor = 1f,
				IsAffectedByWind = true,
				minRotationSpeed = -(float)Math.PI / 4f,
				maxRotationSpeed = -(float)Math.PI,
				minScale = 0.005f,
				maxScale = 0.02f,
				MinNumParticles = 10,
				MaxNumParticles = 15,
				minLifetime = 1f,
				maxLifetime = 2f,
				spriteBlendState = BlendState.Additive,
				TimeBetweenEmitting = 0.1f
			},
			new ParticleSystemType("bonfire", 10)
			{
				minInitialSpeed = 0f,
				maxInitialSpeed = 0f,
				minAcceleration = 0f,
				maxAcceleration = 0f,
				StartMeanMoveDirectionInDegrees = 0f,
				StartMaxMoveDirectionDifferenceInDegrees = 0f,
				TextureFilename = "explosion",
				IsAffectedByWind = false,
				rotateRandomlyAtStart = false,
				minRotationSpeed = 0f,
				maxRotationSpeed = 0f,
				minScale = 1f,
				maxScale = 1f,
				MinNumParticles = 1,
				MaxNumParticles = 1,
				ParticlesNeverExpire = true,
				spriteBlendState = BlendState.Additive,
				AnimationKey = "campfireFast"
			},
			new ParticleSystemType("explosion", 2)
			{
				SystemScale = 0.3f,
				minInitialSpeed = 40f,
				maxInitialSpeed = 500f,
				minAcceleration = 0f,
				maxAcceleration = 0f,
				StartMeanMoveDirectionInDegrees = 90f,
				StartMaxMoveDirectionDifferenceInDegrees = 10f,
				TextureFilename = "explosion",
				RemoveEmittersAfterLastParticleExpires = true,
				IsAffectedByWind = false,
				DecelerateParticlesToZeroBeforeTheyDie = true,
				minRotationSpeed = -(float)Math.PI / 4f,
				maxRotationSpeed = (float)Math.PI / 4f,
				minScale = 0.3f,
				maxScale = 1f,
				MinNumParticles = 5,
				MaxNumParticles = 10,
				minLifetime = 0.5f,
				maxLifetime = 1f,
				spriteBlendState = BlendState.Additive
			},
			new ParticleSystemType("explosionSmokeCloud", 8)
			{
				SystemScale = 0.3f,
				minInitialSpeed = 20f,
				maxInitialSpeed = 200f,
				minAcceleration = -10f,
				maxAcceleration = -50f,
				TextureFilename = "smoke",
				RemoveEmittersAfterLastParticleExpires = true,
				IsAffectedByWind = true,
				minRotationSpeed = -(float)Math.PI / 4f,
				maxRotationSpeed = (float)Math.PI / 4f,
				minScale = 1f,
				maxScale = 2f,
				MinNumParticles = 10,
				MaxNumParticles = 20,
				minLifetime = 1f,
				maxLifetime = 2.5f,
				spriteBlendState = BlendState.AlphaBlend
			}
		};
		ParticleSystemType particleSystemType = new ParticleSystemType("dustStorm", 180);
		particleSystemType.StartMeanMoveDirectionInDegrees = 90f;
		particleSystemType.StartMaxMoveDirectionDifferenceInDegrees = 10f;
		particleSystemType.TextureFilename = "smoke";
		particleSystemType.StartColor = new Color(247, 226, 181);
		particleSystemType.endColor = particleSystemType.StartColor;
		particleSystemType.minInitialSpeed = 10f;
		particleSystemType.maxInitialSpeed = 30f;
		particleSystemType.minRotationSpeed = -(float)Math.PI / 20f;
		particleSystemType.maxRotationSpeed = (float)Math.PI / 20f;
		particleSystemType.minLifetime = 4f;
		particleSystemType.maxLifetime = 8f;
		obj.Add(particleSystemType);
		obj.Add(new ParticleSystemType("flamePlume", 40)
		{
			minInitialSpeed = 240f,
			maxInitialSpeed = 300f,
			minAcceleration = 0f,
			maxAcceleration = 0f,
			StartMeanMoveDirectionInDegrees = 90f,
			StartMaxMoveDirectionDifferenceInDegrees = 10f,
			TextureFilename = "explosion",
			MaxWindAccelerationFactor = 0.5f,
			MinWindAccelerationFactor = 1f,
			IsAffectedByWind = true,
			minRotationSpeed = -(float)Math.PI / 4f,
			maxRotationSpeed = -(float)Math.PI,
			minScale = 0.3f,
			maxScale = 0.5f,
			MinNumParticles = 5,
			MaxNumParticles = 10,
			minLifetime = 2f,
			maxLifetime = 3f,
			spriteBlendState = BlendState.Additive,
			TimeBetweenEmitting = 0.1f
		});
		obj.Add(new ParticleSystemType("fireExtinguisherPoison", 20)
		{
			minInitialSpeed = 240f,
			maxInitialSpeed = 300f,
			minAcceleration = -30f,
			maxAcceleration = -50f,
			StartMaxMoveDirectionDifferenceInDegrees = 26f,
			TextureFilename = "smokeWhite",
			MaxWindAccelerationFactor = 0.5f,
			MinWindAccelerationFactor = 1f,
			IsAffectedByWind = false,
			minRotationSpeed = -(float)Math.PI / 4f,
			maxRotationSpeed = -(float)Math.PI,
			minScale = 0.05f,
			maxScale = 0.15f,
			MinNumParticles = 10,
			MaxNumParticles = 15,
			StartColor = new Color(235, 255, 200),
			endColor = new Color(255, 0, 0),
			minLifetime = 0.5f,
			maxLifetime = 0.7f,
			TimeBetweenEmitting = 0.1f
		});
		obj.Add(new ParticleSystemType("bushDragonSpray", 20)
		{
			minInitialSpeed = 100f,
			maxInitialSpeed = 140f,
			minAcceleration = -40f,
			maxAcceleration = -50f,
			StartMaxMoveDirectionDifferenceInDegrees = 26f,
			TextureFilename = "smokeWhite",
			MaxWindAccelerationFactor = 0.5f,
			MinWindAccelerationFactor = 1f,
			IsAffectedByWind = false,
			minRotationSpeed = -(float)Math.PI / 4f,
			maxRotationSpeed = -(float)Math.PI,
			minScale = 0.05f,
			maxScale = 0.15f,
			MinNumParticles = 10,
			MaxNumParticles = 15,
			StartColor = new Color(235, 255, 200),
			endColor = new Color(255, 0, 0),
			minLifetime = 0.5f,
			maxLifetime = 0.7f,
			TimeBetweenEmitting = 0.3f
		});
		particleSystemType = new ParticleSystemType("dust", 40);
		particleSystemType.minInitialSpeed = 0f;
		particleSystemType.maxInitialSpeed = 20f;
		particleSystemType.minAcceleration = 0f;
		particleSystemType.maxAcceleration = 0f;
		particleSystemType.StartMeanMoveDirectionInDegrees = 90f;
		particleSystemType.StartMaxMoveDirectionDifferenceInDegrees = 10f;
		particleSystemType.TextureFilename = "smoke";
		particleSystemType.EmitParticlesManually = true;
		particleSystemType.MaxWindAccelerationFactor = 0.5f;
		particleSystemType.MinWindAccelerationFactor = 1f;
		particleSystemType.IsAffectedByWind = true;
		particleSystemType.minRotationSpeed = -(float)Math.PI / 20f;
		particleSystemType.maxRotationSpeed = (float)Math.PI / 20f;
		particleSystemType.minScale = 1f;
		particleSystemType.maxScale = 3f;
		particleSystemType.MinNumParticles = 2;
		particleSystemType.MaxNumParticles = 3;
		particleSystemType.StartColor = new Color(196, 122, 96);
		particleSystemType.endColor = particleSystemType.StartColor;
		particleSystemType.minLifetime = 4f;
		particleSystemType.maxLifetime = 6f;
		obj.Add(particleSystemType);
		particleSystemType = new ParticleSystemType("sulphurousSmoke", 40);
		particleSystemType.StartMeanMoveDirectionInDegrees = 90f;
		particleSystemType.StartMaxMoveDirectionDifferenceInDegrees = 10f;
		particleSystemType.minInitialSpeed = 40f;
		particleSystemType.maxInitialSpeed = 60f;
		particleSystemType.TextureFilename = "smoke";
		particleSystemType.SystemScale = 0.3f;
		particleSystemType.MaxWindAccelerationFactor = 1f;
		particleSystemType.MinWindAccelerationFactor = 0.5f;
		particleSystemType.minRotationSpeed = -(float)Math.PI / 8f;
		particleSystemType.maxRotationSpeed = (float)Math.PI / 8f;
		particleSystemType.minScale = 0.5f;
		particleSystemType.maxScale = 1f;
		particleSystemType.MinNumParticles = 3;
		particleSystemType.MaxNumParticles = 6;
		particleSystemType.StartColor = new Color(238, 218, 126);
		particleSystemType.endColor = particleSystemType.StartColor;
		particleSystemType.minLifetime = 5f;
		particleSystemType.maxLifetime = 7f;
		particleSystemType.TimeBetweenEmitting = 0.5f;
		obj.Add(particleSystemType);
		particleSystemType = new ParticleSystemType("sulfurBomb", 4);
		particleSystemType.StartMeanMoveDirectionInDegrees = 90f;
		particleSystemType.StartMaxMoveDirectionDifferenceInDegrees = 10f;
		particleSystemType.minInitialSpeed = 40f;
		particleSystemType.maxInitialSpeed = 60f;
		particleSystemType.TextureFilename = "smoke";
		particleSystemType.SystemScale = 0.3f;
		particleSystemType.MaxWindAccelerationFactor = 1f;
		particleSystemType.MinWindAccelerationFactor = 0.5f;
		particleSystemType.minRotationSpeed = -(float)Math.PI / 8f;
		particleSystemType.maxRotationSpeed = (float)Math.PI / 8f;
		particleSystemType.minScale = 0.5f;
		particleSystemType.maxScale = 1f;
		particleSystemType.MinNumParticles = 3;
		particleSystemType.MaxNumParticles = 6;
		particleSystemType.StartColor = new Color(238, 218, 126);
		particleSystemType.endColor = particleSystemType.StartColor;
		particleSystemType.minLifetime = 6f;
		particleSystemType.maxLifetime = 8f;
		obj.Add(particleSystemType);
		particleSystemType = new ParticleSystemType("smallSmoke", 40);
		particleSystemType.StartMeanMoveDirectionInDegrees = 90f;
		particleSystemType.StartMaxMoveDirectionDifferenceInDegrees = 10f;
		particleSystemType.minInitialSpeed = 40f;
		particleSystemType.maxInitialSpeed = 60f;
		particleSystemType.TextureFilename = "smoke";
		particleSystemType.SystemScale = 0.3f;
		particleSystemType.MaxWindAccelerationFactor = 1f;
		particleSystemType.MinWindAccelerationFactor = 0.5f;
		particleSystemType.minRotationSpeed = -(float)Math.PI / 8f;
		particleSystemType.maxRotationSpeed = (float)Math.PI / 8f;
		particleSystemType.minScale = 0.5f;
		particleSystemType.maxScale = 1f;
		particleSystemType.MinNumParticles = 3;
		particleSystemType.MaxNumParticles = 6;
		particleSystemType.StartColor = new Color(255, 255, 255);
		particleSystemType.endColor = particleSystemType.StartColor;
		particleSystemType.minLifetime = 5f;
		particleSystemType.maxLifetime = 7f;
		obj.Add(particleSystemType);
		particleSystemType = new ParticleSystemType("smallerSmoke", 8);
		particleSystemType.StartMeanMoveDirectionInDegrees = 90f;
		particleSystemType.StartMaxMoveDirectionDifferenceInDegrees = 0.8f;
		particleSystemType.minInitialSpeed = 40f;
		particleSystemType.maxInitialSpeed = 60f;
		particleSystemType.TextureFilename = "smoke";
		particleSystemType.SystemScale = 0.15f;
		particleSystemType.MaxWindAccelerationFactor = 1f;
		particleSystemType.MinWindAccelerationFactor = 0.5f;
		particleSystemType.minRotationSpeed = -(float)Math.PI / 8f;
		particleSystemType.maxRotationSpeed = (float)Math.PI / 8f;
		particleSystemType.minScale = 0.3f;
		particleSystemType.maxScale = 1.5f;
		particleSystemType.MinNumParticles = 3;
		particleSystemType.MaxNumParticles = 6;
		particleSystemType.StartColor = new Color(255, 255, 255);
		particleSystemType.endColor = particleSystemType.StartColor;
		particleSystemType.minLifetime = 10f;
		particleSystemType.maxLifetime = 20f;
		particleSystemType.TimeBetweenEmitting = 0.5f;
		obj.Add(particleSystemType);
		particleSystemType = new ParticleSystemType("smallestSmoke", 8);
		particleSystemType.StartMeanMoveDirectionInDegrees = 90f;
		particleSystemType.StartMaxMoveDirectionDifferenceInDegrees = 0.8f;
		particleSystemType.minInitialSpeed = 40f;
		particleSystemType.maxInitialSpeed = 60f;
		particleSystemType.TextureFilename = "smoke";
		particleSystemType.SystemScale = 0.15f;
		particleSystemType.MaxWindAccelerationFactor = 1f;
		particleSystemType.MinWindAccelerationFactor = 0.5f;
		particleSystemType.minRotationSpeed = -(float)Math.PI / 8f;
		particleSystemType.maxRotationSpeed = (float)Math.PI / 8f;
		particleSystemType.minScale = 0.2f;
		particleSystemType.maxScale = 1.2f;
		particleSystemType.MinNumParticles = 2;
		particleSystemType.MaxNumParticles = 4;
		particleSystemType.StartColor = new Color(255, 255, 255);
		particleSystemType.endColor = particleSystemType.StartColor;
		particleSystemType.minLifetime = 4f;
		particleSystemType.maxLifetime = 10f;
		particleSystemType.TimeBetweenEmitting = 0.5f;
		obj.Add(particleSystemType);
		particleSystemType = new ParticleSystemType("foodSteam", 4);
		particleSystemType.StartMeanMoveDirectionInDegrees = 90f;
		particleSystemType.StartMaxMoveDirectionDifferenceInDegrees = 0.8f;
		particleSystemType.minInitialSpeed = 40f;
		particleSystemType.maxInitialSpeed = 60f;
		particleSystemType.TextureFilename = "smoke";
		particleSystemType.SystemScale = 0.05f;
		particleSystemType.MaxWindAccelerationFactor = 1f;
		particleSystemType.MinWindAccelerationFactor = 0.5f;
		particleSystemType.minRotationSpeed = -(float)Math.PI / 8f;
		particleSystemType.maxRotationSpeed = (float)Math.PI / 8f;
		particleSystemType.minScale = 0.2f;
		particleSystemType.maxScale = 1.2f;
		particleSystemType.MinNumParticles = 2;
		particleSystemType.MaxNumParticles = 4;
		particleSystemType.StartColor = new Color(255, 255, 255);
		particleSystemType.endColor = particleSystemType.StartColor;
		particleSystemType.minLifetime = 2f;
		particleSystemType.maxLifetime = 4f;
		particleSystemType.TimeBetweenEmitting = 0.5f;
		obj.Add(particleSystemType);
		particleSystemType = new ParticleSystemType("signalSmoke", 40);
		particleSystemType.StartMeanMoveDirectionInDegrees = 90f;
		particleSystemType.StartMaxMoveDirectionDifferenceInDegrees = 0.8f;
		particleSystemType.minInitialSpeed = 40f;
		particleSystemType.maxInitialSpeed = 60f;
		particleSystemType.TextureFilename = "smoke";
		particleSystemType.SystemScale = 0.15f;
		particleSystemType.MaxWindAccelerationFactor = 1f;
		particleSystemType.MinWindAccelerationFactor = 0.5f;
		particleSystemType.minRotationSpeed = -(float)Math.PI / 8f;
		particleSystemType.maxRotationSpeed = (float)Math.PI / 8f;
		particleSystemType.minScale = 0.5f;
		particleSystemType.maxScale = 1f;
		particleSystemType.MinNumParticles = 3;
		particleSystemType.MaxNumParticles = 6;
		particleSystemType.StartColor = new Color(0, 0, 0);
		particleSystemType.endColor = particleSystemType.StartColor;
		particleSystemType.minLifetime = 10f;
		particleSystemType.maxLifetime = 20f;
		particleSystemType.TimeBetweenEmitting = 0.5f;
		obj.Add(particleSystemType);
		obj.Add(new ParticleSystemType("whiteSignalSmoke", 40)
		{
			StartMeanMoveDirectionInDegrees = 90f,
			StartMaxMoveDirectionDifferenceInDegrees = 0.8f,
			minInitialSpeed = 40f,
			maxInitialSpeed = 60f,
			TextureFilename = "smoke",
			SystemScale = 0.15f,
			MaxWindAccelerationFactor = 1f,
			MinWindAccelerationFactor = 0.5f,
			minRotationSpeed = -(float)Math.PI / 8f,
			maxRotationSpeed = (float)Math.PI / 8f,
			minScale = 0.5f,
			maxScale = 1f,
			MinNumParticles = 3,
			MaxNumParticles = 6,
			StartColor = new Color(255, 255, 255),
			endColor = new Color(255, 255, 255),
			minLifetime = 10f,
			maxLifetime = 20f,
			TimeBetweenEmitting = 0.5f
		});
		obj.Add(new ParticleSystemType("fog", 30)
		{
			TextureFilename = "smoke",
			SystemScale = 0.3f,
			StartMeanMoveDirectionInDegrees = 90f,
			StartMaxMoveDirectionDifferenceInDegrees = 0f,
			IsAffectedByWind = true,
			MinWindAccelerationFactor = 0.1f,
			MaxWindAccelerationFactor = 0.1f,
			StartColor = new Color(174, 227, 236),
			endColor = new Color(174, 227, 236),
			minInitialSpeed = 0f,
			maxInitialSpeed = 0f,
			minRotationSpeed = 0f,
			maxRotationSpeed = 0f,
			minLifetime = 12f,
			maxLifetime = 24f,
			TimeBetweenEmitting = 3f,
			MinNumParticles = 0,
			MaxNumParticles = 1,
			minScale = 0.75f,
			maxScale = 2.25f
		});
		particleSystemType = new ParticleSystemType("groundFog", 30);
		particleSystemType.TextureFilename = "fog";
		particleSystemType.SystemScale = 0.3f;
		particleSystemType.StartMeanMoveDirectionInDegrees = 0f;
		particleSystemType.StartMaxMoveDirectionDifferenceInDegrees = 0f;
		particleSystemType.IsAffectedByWind = false;
		particleSystemType.MinWindAccelerationFactor = 0f;
		particleSystemType.MaxWindAccelerationFactor = 0f;
		particleSystemType.StartColor = new Color(174, 227, 236);
		particleSystemType.endColor = new Color(174, 227, 236);
		particleSystemType.minInitialSpeed = 0f;
		particleSystemType.maxInitialSpeed = 0f;
		particleSystemType.minRotationSpeed = 0f;
		particleSystemType.maxRotationSpeed = 0f;
		particleSystemType.rotateRandomlyAtStart = false;
		particleSystemType.minLifetime = 12f;
		particleSystemType.maxLifetime = 22f;
		particleSystemType.TimeBetweenEmitting = 1f;
		particleSystemType.MinNumParticles = 0;
		particleSystemType.MaxNumParticles = 1;
		particleSystemType.minScale = 1f;
		particleSystemType.maxScale = 2f;
		particleSystemType.rotateRandomlyAtStart = false;
		obj.Add(particleSystemType);
		obj.Add(new ParticleSystemType("kitchenSteam", 3)
		{
			TextureFilename = "smoke",
			SystemScale = 0.1f,
			StartMeanMoveDirectionInDegrees = 90f,
			StartMaxMoveDirectionDifferenceInDegrees = 0f,
			IsAffectedByWind = true,
			MaxWindAccelerationFactor = 1f,
			MinWindAccelerationFactor = 0.5f,
			StartColor = new Color(174, 227, 236),
			endColor = new Color(174, 227, 236),
			minInitialSpeed = 40f,
			maxInitialSpeed = 60f,
			minRotationSpeed = -(float)Math.PI / 8f,
			maxRotationSpeed = (float)Math.PI / 8f,
			minLifetime = 2f,
			maxLifetime = 4f,
			TimeBetweenEmitting = 0.5f,
			MinNumParticles = 0,
			MaxNumParticles = 1,
			minScale = 0.75f,
			maxScale = 1.25f
		});
		obj.Add(new ParticleSystemType("smallFog", 30)
		{
			TextureFilename = "smoke",
			SystemScale = 0.3f,
			StartMeanMoveDirectionInDegrees = 90f,
			StartMaxMoveDirectionDifferenceInDegrees = 0f,
			IsAffectedByWind = true,
			MinWindAccelerationFactor = 0.1f,
			MaxWindAccelerationFactor = 0.1f,
			StartColor = new Color(174, 227, 236),
			endColor = new Color(174, 227, 236),
			minInitialSpeed = 0f,
			maxInitialSpeed = 0f,
			minRotationSpeed = 0f,
			maxRotationSpeed = 0f,
			minLifetime = 12f,
			maxLifetime = 24f,
			TimeBetweenEmitting = 3f,
			MinNumParticles = 0,
			MaxNumParticles = 1,
			minScale = 0.75f,
			maxScale = 1.25f
		});
		obj.Add(new ParticleSystemType("haze", 30)
		{
			TextureFilename = "smoke",
			SystemScale = 0.3f,
			StartMeanMoveDirectionInDegrees = 90f,
			StartMaxMoveDirectionDifferenceInDegrees = 0f,
			IsAffectedByWind = true,
			MinWindAccelerationFactor = 0.1f,
			MaxWindAccelerationFactor = 0.1f,
			StartColor = new Color(247, 226, 181),
			endColor = new Color(247, 226, 181),
			minInitialSpeed = 0f,
			maxInitialSpeed = 0f,
			minRotationSpeed = 0f,
			maxRotationSpeed = 0f,
			minLifetime = 12f,
			maxLifetime = 24f,
			TimeBetweenEmitting = 3f,
			MinNumParticles = 0,
			MaxNumParticles = 1,
			minScale = 0.75f,
			maxScale = 2.25f
		});
		obj.Add(new ParticleSystemType("pollen", 30)
		{
			TextureFilename = "pollen",
			SystemScale = 0.3f,
			StartMeanMoveDirectionInDegrees = 90f,
			StartMaxMoveDirectionDifferenceInDegrees = 0f,
			IsAffectedByWind = true,
			MinWindAccelerationFactor = 0.7f,
			MaxWindAccelerationFactor = 1f,
			StartColor = new Color(255, 255, 255),
			endColor = new Color(255, 255, 255),
			minInitialSpeed = 1f,
			maxInitialSpeed = 1.9f,
			minRotationSpeed = -(float)Math.PI / 6f,
			maxRotationSpeed = (float)Math.PI / 8f,
			minLifetime = 4f,
			maxLifetime = 25f,
			TimeBetweenEmitting = 5f,
			MinNumParticles = 0,
			MaxNumParticles = 3,
			minScale = 0.5f,
			maxScale = 1f
		});
		obj.Add(new ParticleSystemType("fireSparks", 30)
		{
			TextureFilename = "pollen",
			SystemScale = 0.3f,
			StartMeanMoveDirectionInDegrees = 90f,
			StartMaxMoveDirectionDifferenceInDegrees = 0f,
			IsAffectedByWind = false,
			MinWindAccelerationFactor = 0.1f,
			MaxWindAccelerationFactor = 0.3f,
			StartColor = new Color(255, 244, 168),
			endColor = new Color(255, 83, 15),
			minInitialSpeed = 8f,
			maxInitialSpeed = 12f,
			minRotationSpeed = -(float)Math.PI / 6f,
			maxRotationSpeed = (float)Math.PI / 8f,
			minLifetime = 0.1f,
			maxLifetime = 1.5f,
			TimeBetweenEmitting = 0.5f,
			MinNumParticles = 0,
			MaxNumParticles = 3,
			minScale = 0.2f,
			maxScale = 0.2f
		});
		obj.Add(new ParticleSystemType("poisonPlume", 40)
		{
			minInitialSpeed = 240f,
			maxInitialSpeed = 300f,
			minAcceleration = 0f,
			maxAcceleration = 0f,
			StartMeanMoveDirectionInDegrees = 90f,
			StartMaxMoveDirectionDifferenceInDegrees = 10f,
			TextureFilename = "explosion",
			MaxWindAccelerationFactor = 0.5f,
			MinWindAccelerationFactor = 1f,
			StartColor = new Color(0, 255, 0),
			endColor = new Color(255, 255, 0),
			IsAffectedByWind = true,
			minRotationSpeed = -(float)Math.PI / 4f,
			maxRotationSpeed = -(float)Math.PI,
			minScale = 0.3f,
			maxScale = 0.5f,
			MinNumParticles = 5,
			MaxNumParticles = 10,
			minLifetime = 2f,
			maxLifetime = 3f,
			spriteBlendState = BlendState.Additive
		});
		obj.Add(new ParticleSystemType("mineExplosion", 2)
		{
			SystemScale = 0.3f,
			minInitialSpeed = 5f,
			maxInitialSpeed = 20f,
			minAcceleration = 0f,
			maxAcceleration = 0f,
			StartMeanMoveDirectionInDegrees = 90f,
			StartMaxMoveDirectionDifferenceInDegrees = 10f,
			TextureFilename = "smokeWhite",
			RemoveEmittersAfterLastParticleExpires = true,
			IsAffectedByWind = false,
			DecelerateParticlesToZeroBeforeTheyDie = true,
			minRotationSpeed = -(float)Math.PI / 4f,
			maxRotationSpeed = (float)Math.PI / 4f,
			minScale = 0.3f,
			maxScale = 0.75f,
			MinNumParticles = 5,
			MaxNumParticles = 10,
			minLifetime = 0.1f,
			maxLifetime = 0.4f,
			spriteBlendState = BlendState.Additive
		});
		obj.Add(new ParticleSystemType("bigBombExplosion1", 2)
		{
			SystemScale = 0.5f,
			minInitialSpeed = 9f,
			maxInitialSpeed = 25f,
			minAcceleration = 0f,
			maxAcceleration = 0f,
			StartMeanMoveDirectionInDegrees = 90f,
			StartMaxMoveDirectionDifferenceInDegrees = 10f,
			TextureFilename = "smokeWhite",
			RemoveEmittersAfterLastParticleExpires = true,
			IsAffectedByWind = false,
			DecelerateParticlesToZeroBeforeTheyDie = true,
			minRotationSpeed = -(float)Math.PI / 4f,
			maxRotationSpeed = (float)Math.PI / 4f,
			minScale = 0.5f,
			maxScale = 1f,
			MinNumParticles = 5,
			MaxNumParticles = 10,
			minLifetime = 0.1f,
			maxLifetime = 0.4f,
			spriteBlendState = BlendState.Additive
		});
		obj.Add(new ParticleSystemType("bigBombExplosion2", 2)
		{
			SystemScale = 0.5f,
			minInitialSpeed = 40f,
			maxInitialSpeed = 500f,
			minAcceleration = 0f,
			maxAcceleration = 0f,
			StartMeanMoveDirectionInDegrees = 90f,
			StartMaxMoveDirectionDifferenceInDegrees = 10f,
			TextureFilename = "explosion",
			RemoveEmittersAfterLastParticleExpires = true,
			IsAffectedByWind = false,
			DecelerateParticlesToZeroBeforeTheyDie = true,
			minRotationSpeed = -(float)Math.PI / 4f,
			maxRotationSpeed = (float)Math.PI / 4f,
			minScale = 0.5f,
			maxScale = 1.3f,
			MinNumParticles = 5,
			MaxNumParticles = 10,
			minLifetime = 0.5f,
			maxLifetime = 1f,
			spriteBlendState = BlendState.Additive
		});
		obj.Add(new ParticleSystemType("gunpowderSmoke", 2)
		{
			TextureFilename = "smoke",
			RemoveEmittersAfterLastParticleExpires = true,
			SystemScale = 0.3f,
			StartMeanMoveDirectionInDegrees = 90f,
			StartMaxMoveDirectionDifferenceInDegrees = 0f,
			IsAffectedByWind = true,
			MaxWindAccelerationFactor = 1f,
			MinWindAccelerationFactor = 0.5f,
			StartColor = new Color(174, 227, 236),
			endColor = new Color(174, 227, 236),
			minInitialSpeed = 30f,
			maxInitialSpeed = 50f,
			minRotationSpeed = -(float)Math.PI / 8f,
			maxRotationSpeed = (float)Math.PI / 8f,
			minLifetime = 0.5f,
			maxLifetime = 5f,
			MinNumParticles = 10,
			MaxNumParticles = 11,
			minScale = 0.75f,
			maxScale = 1.5f
		});
		obj.Add(new ParticleSystemType("explosionSmokeCloudLong", 8)
		{
			SystemScale = 0.5f,
			minInitialSpeed = 5f,
			maxInitialSpeed = 5f,
			minAcceleration = 5f,
			maxAcceleration = 10f,
			TextureFilename = "smoke",
			RemoveEmittersAfterLastParticleExpires = true,
			IsAffectedByWind = true,
			minRotationSpeed = -(float)Math.PI / 4f,
			maxRotationSpeed = (float)Math.PI / 4f,
			minScale = 0.75f,
			maxScale = 1f,
			MinNumParticles = 10,
			MaxNumParticles = 20,
			TimeBetweenEmitting = 20f,
			minLifetime = 12f,
			maxLifetime = 12f,
			spriteBlendState = BlendState.AlphaBlend
		});
		obj.Add(new ParticleSystemType("buckShotCloud", 20)
		{
			minInitialSpeed = 600f,
			maxInitialSpeed = 800f,
			minAcceleration = 0f,
			maxAcceleration = -10f,
			StartMaxMoveDirectionDifferenceInDegrees = 10f,
			TextureFilename = "pollen",
			RemoveEmittersAfterLastParticleExpires = true,
			IsAffectedByWind = false,
			minRotationSpeed = 0f,
			maxRotationSpeed = 0f,
			MinNumParticles = 5,
			MaxNumParticles = 7,
			minScale = 0.18f,
			maxScale = 0.18f,
			StartColor = new Color(0, 0, 0),
			endColor = new Color(0f, 0f, 0f, 0.5f),
			minLifetime = 0.15f,
			maxLifetime = 0.25f
		});
		obj.Add(new ParticleSystemType("goldBuckShotCloud", 20)
		{
			minInitialSpeed = 600f,
			maxInitialSpeed = 800f,
			minAcceleration = 0f,
			maxAcceleration = -10f,
			StartMaxMoveDirectionDifferenceInDegrees = 10f,
			TextureFilename = "pollen",
			RemoveEmittersAfterLastParticleExpires = true,
			IsAffectedByWind = false,
			minRotationSpeed = 0f,
			maxRotationSpeed = 0f,
			MinNumParticles = 5,
			MaxNumParticles = 7,
			minScale = 0.18f,
			maxScale = 0.18f,
			StartColor = new Color(255, 215, 0),
			endColor = new Color(255f, 215f, 0f, 0.5f),
			minLifetime = 0.15f,
			maxLifetime = 0.25f
		});
		obj.Add(new ParticleSystemType("gunSmoke", 8)
		{
			SystemScale = 0.05f,
			minInitialSpeed = 490f,
			maxInitialSpeed = 590f,
			minAcceleration = -15f,
			maxAcceleration = -20f,
			TextureFilename = "smoke",
			RemoveEmittersAfterLastParticleExpires = true,
			IsAffectedByWind = true,
			MinWindAccelerationFactor = 3f,
			MaxWindAccelerationFactor = 3f,
			minRotationSpeed = -(float)Math.PI / 4f,
			maxRotationSpeed = (float)Math.PI / 4f,
			StartColor = new Color(200, 200, 200),
			endColor = new Color(200f, 200f, 200f, 0.5f),
			minScale = 1f,
			maxScale = 6f,
			MinNumParticles = 8,
			MaxNumParticles = 14,
			minLifetime = 3f,
			maxLifetime = 6f,
			spriteBlendState = BlendState.AlphaBlend
		});
		return obj;
	}
}
