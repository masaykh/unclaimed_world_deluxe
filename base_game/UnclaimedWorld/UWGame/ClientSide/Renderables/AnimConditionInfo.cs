using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using UWGame.Client.Particles;
using Xclna.Xna.Animation;

namespace UWGame.ClientSide.Renderables;

[DebuggerDisplay("Conditions:{ConditionSet} Forbiddens:{Forbiddens.StateNames}")]
public class AnimConditionInfo
{
	public class TemporaryAttachable : IEquatable<TemporaryAttachable>
	{
		public string RenderableTypeKey;

		public AttacheePoint? AttacheePoint;

		public string AttachorTag;

		public bool Equals(TemporaryAttachable other)
		{
			if (RenderableTypeKey == other.RenderableTypeKey)
			{
				AttacheePoint? attacheePoint = AttacheePoint;
				AttacheePoint? attacheePoint2 = other.AttacheePoint;
				if (attacheePoint.GetValueOrDefault() == attacheePoint2.GetValueOrDefault() && attacheePoint.HasValue == attacheePoint2.HasValue && AttachorTag == other.AttachorTag)
				{
					return true;
				}
			}
			return false;
		}
	}

	public class AttachPointData
	{
		public AttacheePoint? AttacheePoint;

		public string RenderableTypeKey;

		public Vector3 Translation;

		public Vector3 Rotation;
	}

	public AnimConditions ConditionSet;

	public BitMask64 Forbiddens;

	public RandomSoundAndAnimationSet SoundAndAnimationSet;

	public Playback Playback;

	public StartingPoint StartingPoint;

	public float? StartingPointInSeconds;

	public BlendMode BlendMode;

	public Looping Looping;

	public float SpeedFactor;

	public string GaitSetKey;

	public AttachPointData[] AttachPoints;

	public ParticleEmitterEffect[] ParticleEmitters;

	public TemporaryAttachable[] TemporaryRenderablesToAttach;

	public AnimConditionInfo()
	{
		Playback = Playback.Forwards;
		StartingPoint = StartingPoint.FromBeginning;
		BlendMode = BlendMode.Normal;
		Looping = Looping.Yes;
		SpeedFactor = 1f;
	}

	public bool Equals(AnimConditionInfo other)
	{
		if (other != null)
		{
			bool flag = false;
			if (ConditionSet != null && other.ConditionSet != null)
			{
				flag = ConditionSet.Equals(other.ConditionSet);
			}
			else if (ConditionSet == null && other.ConditionSet == null)
			{
				flag = true;
			}
			bool flag2 = false;
			if (Forbiddens != null && other.Forbiddens != null)
			{
				flag2 = Forbiddens.Equals(other.Forbiddens);
			}
			else if (Forbiddens == null && other.Forbiddens == null)
			{
				flag2 = true;
			}
			return flag && flag2;
		}
		return false;
	}

	public AnimAction? GetAction()
	{
		if (ConditionSet != null)
		{
			return ConditionSet.Action;
		}
		return null;
	}

	public BitMask64 GetModifiers()
	{
		if (ConditionSet != null)
		{
			return ConditionSet.Modifiers;
		}
		return null;
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
		if (SoundAndAnimationSet != null)
		{
			SoundAndAnimationSet.Initialize();
		}
	}

	public void PostLoadContentValidate(List<string> listOfErrors, string modelName, AnimationInfoCollection animations)
	{
		if (SoundAndAnimationSet == null)
		{
			return;
		}
		if (SoundAndAnimationSet.BaseAnimations != null)
		{
			string[] baseAnimations = SoundAndAnimationSet.BaseAnimations;
			foreach (string key in baseAnimations)
			{
				listOfErrors = RenderAsModelType.ValidateThatAnimExists(listOfErrors, key, modelName, animations);
			}
		}
		if (SoundAndAnimationSet.AdditionalAnimations1 != null)
		{
			string[] baseAnimations = SoundAndAnimationSet.AdditionalAnimations1;
			foreach (string key2 in baseAnimations)
			{
				listOfErrors = RenderAsModelType.ValidateThatAnimExists(listOfErrors, key2, modelName, animations);
			}
		}
		if (SoundAndAnimationSet.AdditionalAnimations2 != null)
		{
			string[] baseAnimations = SoundAndAnimationSet.AdditionalAnimations2;
			foreach (string key3 in baseAnimations)
			{
				listOfErrors = RenderAsModelType.ValidateThatAnimExists(listOfErrors, key3, modelName, animations);
			}
		}
		if (SoundAndAnimationSet != null)
		{
			SoundAndAnimationSet.PostLoadContentValidate(ref listOfErrors);
		}
	}
}
