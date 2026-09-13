using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Kensei.Dev;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using SpriteSheetRuntime;
using UWGame.Client.Audio;
using UWGame.Client.Particles;
using UWGame.ClientSide.Map;
using UWGame.ClientSide.Particles;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Trees;
using WindowSystem;
using Xclna.Xna.Animation;

namespace UWGame.ClientSide.Renderables;

[DebuggerDisplay("{Entity}{MemoryFact}")]
public class Renderable : GameObject, ISleepingUpdatable
{
	public enum AdditionalEffect
	{
		Outline
	}

	[Flags]
	public enum TintStatus : uint
	{
		None = 0u,
		Disabled = 1u,
		Experience = 2u,
		Selected = 4u
	}

	[Flags]
	public enum LightAttenuationStatus : uint
	{
		None = 0u,
		Normal = 1u,
		Dark = 2u,
		Darkening = 4u,
		Lightening = 8u
	}

	public class RadiusDecalType
	{
	}

	private enum FadeStatus
	{
		None,
		FadeIn,
		FadeOut
	}

	private enum RenderEffect
	{
		Normal,
		Memory,
		Overlay,
		Stealth
	}

	public class SnapshotRenderable : ISnapshot
	{
		private AnimConditions animConditions;

		private BitMask64 spriteConditions;

		private bool flipHorizontally;

		private RenderEffect renderEffect;

		private List<Tuple<string, string, AttacheePoint>> attachables;

		private SnapshotRenderasModel SnapshotRenderAsModel;

		private Snapshotter.Version version = Snapshotter.Version.Original;

		public bool IsSnapshotted { get; set; }

		public SnapshotRenderable()
		{
		}

		public SnapshotRenderable(Renderable renderable)
		{
			spriteConditions = renderable.spriteConditions;
			animConditions = renderable.AnimConditions;
			flipHorizontally = renderable.flipHorizontally;
			renderEffect = renderable.renderEffect;
			attachables = renderable.snapshotAttachables;
			if (renderable.RenderAsModel != null)
			{
				SnapshotRenderAsModel = new SnapshotRenderasModel(renderable.RenderAsModel);
			}
		}

		public void LoadRenderableWithSnapshotData(Renderable renderable)
		{
			renderable.spriteConditions = spriteConditions;
			renderable.AnimConditions = animConditions;
			renderable.flipHorizontally = flipHorizontally;
			renderable.renderEffect = renderEffect;
			renderable.snapshotAttachables = attachables;
			if (SnapshotRenderAsModel == null)
			{
				return;
			}
			if (renderable.Parent is MemoryFact)
			{
				bool setAnimationFlagsDirty = true;
				renderable.RenderAsModel = new RenderAsModel(SnapshotRenderAsModel.CustomColor0, SnapshotRenderAsModel.CustomColor1, SnapshotRenderAsModel.CustomColor2, SnapshotRenderAsModel.CustomColor3, SnapshotRenderAsModel.LocalTransform, SnapshotRenderAsModel.Location, SnapshotRenderAsModel.FacingNormal, SnapshotRenderAsModel.FinalModelName, SnapshotRenderAsModel.FinalModelBasicTextureName, SnapshotRenderAsModel.FinalModelScale, null, renderable, setAnimationFlagsDirty, renderable.Parent);
				return;
			}
			renderable.RenderAsModel.CustomColor0 = SnapshotRenderAsModel.CustomColor0;
			renderable.RenderAsModel.CustomColor1 = SnapshotRenderAsModel.CustomColor1;
			renderable.RenderAsModel.CustomColor2 = SnapshotRenderAsModel.CustomColor2;
			renderable.RenderAsModel.CustomColor3 = SnapshotRenderAsModel.CustomColor3;
			renderable.RenderAsModel.Location = SnapshotRenderAsModel.Location;
			renderable.RenderAsModel.LocalTransform = SnapshotRenderAsModel.LocalTransform;
			renderable.RenderAsModel.FacingNormal = SnapshotRenderAsModel.FacingNormal;
			renderable.RenderAsModel.FinalModelScale = SnapshotRenderAsModel.FinalModelScale;
			renderable.RenderAsModel.FinalModelName = SnapshotRenderAsModel.FinalModelName;
			renderable.RenderAsModel.ModelData = GameData.Instance.AllModels[renderable.RenderAsModel.FinalModelName];
			renderable.RenderAsModel.FinalModelBasicTextureName = SnapshotRenderAsModel.FinalModelBasicTextureName;
			if (!string.IsNullOrEmpty(renderable.RenderAsModel.FinalModelBasicTextureName))
			{
				renderable.RenderAsModel.FinalModelBasicTexture = GameData.Instance.ExtraModelTextures[renderable.RenderAsModel.FinalModelBasicTextureName];
			}
		}

		public ISnapshot DoSnapshot(Snapshotter sn)
		{
			animConditions = (AnimConditions)sn.DoISnapshot(animConditions);
			spriteConditions = (BitMask64)sn.DoISnapshot(spriteConditions);
			flipHorizontally = sn.DoBool(flipHorizontally);
			renderEffect = sn.DoEnum(renderEffect);
			attachables = sn.DoList(attachables);
			SnapshotRenderAsModel = (SnapshotRenderasModel)sn.DoISnapshot(SnapshotRenderAsModel);
			return this;
		}

		public Snapshotter.Version DoVersion(Snapshotter sn)
		{
			version = sn.DoVersion(Snapshotter.Version.Original);
			return version;
		}

		public void LoadPostProcess(Snapshotter sn)
		{
			sn.RegisterLoadPostProcessCall(this);
			if (spriteConditions != null)
			{
				spriteConditions.LoadPostProcess(sn);
			}
			if (animConditions != null)
			{
				animConditions.LoadPostProcess(sn);
			}
			if (SnapshotRenderAsModel != null)
			{
				SnapshotRenderAsModel.LoadPostProcess(sn);
			}
		}
	}

	public class SnapshotRenderasModel : ISnapshot
	{
		public Vector3 CustomColor0;

		public Vector3 CustomColor1;

		public Vector3 CustomColor2;

		public Vector3 CustomColor3;

		public Vector3 FacingNormal = Vector3.UnitX;

		public float FinalModelScale = 1f;

		public string FinalModelName;

		public string FinalModelBasicTextureName;

		public Matrix LocalTransform = Matrix.Identity;

		public Vector3 Location = Vector3.Zero;

		private Snapshotter.Version version = Snapshotter.Version.Original;

		public bool IsSnapshotted { get; set; }

		public SnapshotRenderasModel(RenderAsModel renderAsModel)
		{
			CustomColor0 = renderAsModel.CustomColor0;
			CustomColor1 = renderAsModel.CustomColor1;
			CustomColor2 = renderAsModel.CustomColor2;
			CustomColor3 = renderAsModel.CustomColor3;
			FacingNormal = renderAsModel.FacingNormal;
			FinalModelScale = renderAsModel.FinalModelScale;
			FinalModelName = renderAsModel.FinalModelName;
			FinalModelBasicTextureName = renderAsModel.FinalModelBasicTextureName;
			LocalTransform = renderAsModel.LocalTransform;
			Location = renderAsModel.Location;
		}

		public SnapshotRenderasModel()
		{
		}

		public ISnapshot DoSnapshot(Snapshotter sn)
		{
			CustomColor0 = sn.DoVector3(CustomColor0);
			CustomColor1 = sn.DoVector3(CustomColor1);
			CustomColor2 = sn.DoVector3(CustomColor2);
			CustomColor3 = sn.DoVector3(CustomColor3);
			FacingNormal = sn.DoVector3(FacingNormal);
			Location = sn.DoVector3(Location);
			FinalModelScale = sn.DoFloat(FinalModelScale);
			FinalModelName = sn.DoString(FinalModelName);
			FinalModelBasicTextureName = sn.DoString(FinalModelBasicTextureName);
			LocalTransform = sn.DoMatrix(LocalTransform);
			return this;
		}

		public Snapshotter.Version DoVersion(Snapshotter sn)
		{
			version = sn.DoVersion(Snapshotter.Version.Original);
			return version;
		}

		public void LoadPostProcess(Snapshotter sn)
		{
			sn.RegisterLoadPostProcessCall(this);
		}
	}

	private double? nextSoundTimepoint;

	public double? ExpiryTimePointInSeconds;

	private double? timePointInSeconds;

	private SleepyUpdaterID sleepyUpdater;

	private double? updateInterval;

	private bool isOnScreen;

	private Effect<Color> tintEffect;

	private Effect<Animation2DPlayer> pulsingEffect;

	private Dictionary<AdditionalEffect, Effect<Animation2DPlayer>> AdditionalEffects;

	private Dictionary<AdditionalEffect, Effect<Color>> additionalTintEffects;

	private Effect<Color> overlayTintEffect;

	private Effect<Animation2DPlayer> overlayPulsingEffect;

	public AnimatedHead AnimatedHead;

	public List<RenderAsBillboard> RenderAsBillboard;

	public RenderAsModel RenderAsModel;

	public RenderAsIcon RenderAsIcon;

	public RenderAsGroundSprite RenderAsGroundSprite;

	public RenderAsConnectedGroundSprite RenderAsConnectedGroundSprite;

	public List<ParticleEmitter> ParticleEmitters;

	public List<LightSource> LightSources;

	public bool LerpableWasRenderedLastFrame;

	private bool spriteFlagsAreDirty = true;

	private BitMask64 spriteConditions;

	private ClientStateInfo selectedSpriteInfo;

	private bool flipHorizontally;

	public bool HideHandAttachments;

	private float? randomConstant;

	private TintStatus tintStatus;

	public TintEnvelope selectionFlashEnvelope;

	public TintEnvelope colorTintEnvelope;

	private LightAttenuationStatus lightAttenuationStatus;

	private TintEnvelope environmentLightAttenuation;

	private RadiusDecalType selectionDecalType;

	public bool IsDrawn = true;

	private uint fogOfWarClearFrame;

	private FadeStatus fadeStatus;

	private bool destroyAfterFadeOut;

	private float fadeProgress = 1f;

	private bool selected;

	public Tuple<SoundData, SoundEffectInstance> StateSoundPlaying;

	public List<Tuple<SoundData, SoundEffectInstance>> ActionSoundsPlaying;

	private static readonly Vector4 memoryFactGradient1 = Color.Cyan.ToVector4();

	private static readonly Vector4 memoryFactGradient2 = Color.LightSeaGreen.ToVector4();

	private static readonly Vector4 memoryFactGradient3 = Color.White.ToVector4();

	private Vector4 overlayGradient1 = Color.DarkGray.ToVector4();

	private Vector4 overlayGradient2 = Color.LightGray.ToVector4();

	private Vector4 overlayGradient3 = Color.White.ToVector4();

	private RenderEffect renderEffect;

	private float strobe;

	private List<Tuple<string, string, AttacheePoint>> snapshotAttachables;

	public Entity Entity { get; set; }

	public MemoryFact MemoryFact { get; set; }

	public RenderableType RenderableType { get; set; }

	public override Renderable AsRenderable => this;

	public IKnownEntityData Parent
	{
		get
		{
			if (Entity != null)
			{
				return Entity;
			}
			if (MemoryFact != null)
			{
				return MemoryFact;
			}
			return null;
		}
	}

	public AnimConditions AnimConditions { get; set; }

	public double? TimePointInSeconds => timePointInSeconds;

	public SleepyUpdaterID SleepyUpdater
	{
		get
		{
			return sleepyUpdater;
		}
		set
		{
			sleepyUpdater = value;
		}
	}

	public double? UpdateInterval
	{
		get
		{
			return updateInterval;
		}
		private set
		{
			if (!Common.IsEqual(updateInterval, value))
			{
				updateInterval = value;
				LookUpSleepyUpdater<Renderable>.FindByID(SleepyUpdater)?.NotifyUpdateIntervalChanged(this);
			}
		}
	}

	public bool IsOnScreen
	{
		get
		{
			return isOnScreen;
		}
		set
		{
			if (value != isOnScreen)
			{
				isOnScreen = value;
				if (isOnScreen)
				{
					RecomputeUpdateInterval();
				}
			}
		}
	}

	public ClientStateInfo SelectedSpriteInfo
	{
		get
		{
			if (spriteFlagsAreDirty || selectedSpriteInfo == null)
			{
				ReplaceSpriteConditionState();
				spriteFlagsAreDirty = false;
			}
			return selectedSpriteInfo;
		}
		private set
		{
			selectedSpriteInfo = value;
		}
	}

	public bool FlipHorizontally
	{
		get
		{
			return flipHorizontally;
		}
		set
		{
			if (flipHorizontally != value)
			{
				flipHorizontally = value;
				SetSpriteQuadsDirty();
			}
		}
	}

	public float RandomConstant
	{
		get
		{
			if (!randomConstant.HasValue)
			{
				randomConstant = (float)The.Sim.GameplayRandomGenerator.NextDouble("Tree") * 2f - 1f;
			}
			return randomConstant.Value;
		}
	}

	public Point MapPosition
	{
		get
		{
			if (Entity != null)
			{
				return Entity.MapPosition.Value;
			}
			if (MemoryFact != null)
			{
				return MemoryFact.MapPosition.Value;
			}
			return MapManager.WorldPosToTile(Location.Value);
		}
	}

	public override Vector3? Location
	{
		get
		{
			if (Entity != null)
			{
				return Entity.Location;
			}
			if (MemoryFact != null)
			{
				return MemoryFact.Location;
			}
			return base.Location;
		}
		set
		{
			base.Location = value;
		}
	}

	public bool DrawAsNonPhysical => renderEffect != RenderEffect.Normal;

	public bool DrawAsOverlay
	{
		get
		{
			if (renderEffect != RenderEffect.Overlay)
			{
				return renderEffect == RenderEffect.Memory;
			}
			return true;
		}
	}

	public void SetNextTimepoint(double? timepoint)
	{
		timePointInSeconds = timepoint;
	}

	void ISleepingUpdatable.CreateSleepyLookupCollection()
	{
	}

	public static void CreateSleepyLookupCollection()
	{
		LookUpSleepyUpdater<Renderable>.Create();
	}

	public Renderable(Entity entity, RenderableType type)
	{
		RenderableType = type;
		Entity = entity;
		spriteConditions = new BitMask64(typeof(StateModifier));
		if (RenderableType.RenderAsIconType != null)
		{
			RenderAsIcon = new RenderAsIcon();
			RenderAsIcon.IconToRender = RenderableType.RenderAsIconType.IconToRender;
		}
		if (Parent != null && RenderableType.AnimatedHeadType != null)
		{
			AnimatedHead = new AnimatedHead(this);
		}
		if (RenderableType.RenderAsModelType != null)
		{
			RenderAsModel = new RenderAsModel(Entity, this);
			AnimConditions = new AnimConditions();
		}
		if (RenderableType.RenderAsConnectedGroundSpriteType != null)
		{
			RenderAsConnectedGroundSprite = new RenderAsConnectedGroundSprite(Entity, this);
		}
		if (RenderableType.ParticleEmitterTypes != null)
		{
			ParticleEmitterType[] particleEmitterTypes = RenderableType.ParticleEmitterTypes;
			foreach (ParticleEmitterType particleEmitterType in particleEmitterTypes)
			{
				The.Client.ParticleManager.AddEmitter(particleEmitterType.ParticleSystemKey, this);
			}
		}
	}

	public Renderable(Renderable original, MemoryFact memoryFact)
	{
		MemoryFact = memoryFact;
		InitCopy(original, memoryFact);
		if (MemoryFact.EntityType.RenderWithOverlayWhenMemoryFact())
		{
			renderEffect = RenderEffect.Memory;
		}
	}

	public Renderable(Renderable original, Entity entity)
	{
		Entity = entity;
		InitCopy(original, entity);
	}

	public Renderable(MemoryFact memoryFact)
	{
		MemoryFact = memoryFact;
		RenderableType = memoryFact.EntityType.RenderableTypeMode;
		if (MemoryFact.EntityType.RenderWithOverlayWhenMemoryFact())
		{
			renderEffect = RenderEffect.Memory;
		}
	}

	public virtual SnapshotRenderable GetFieldsToSnapshot()
	{
		return new SnapshotRenderable(this);
	}

	private void InitCopy(Renderable original, IKnownEntityData entityData)
	{
		location = original.location;
		RenderableType = original.RenderableType;
		spriteConditions = new BitMask64(original.spriteConditions);
		if (original.RenderAsModel != null)
		{
			AnimConditions = new AnimConditions(original.AnimConditions);
			bool setAnimationFlagsDirty = !(entityData is MemoryFact);
			RenderAsModel = new RenderAsModel(original.RenderAsModel.CustomColor0, original.RenderAsModel.CustomColor1, original.RenderAsModel.CustomColor2, original.RenderAsModel.CustomColor3, original.RenderAsModel.LocalTransform, original.RenderAsModel.Location, original.RenderAsModel.FacingNormal, original.RenderAsModel.FinalModelName, original.RenderAsModel.FinalModelBasicTextureName, original.RenderAsModel.FinalModelScale, original.RenderAsModel.AnimatedModel, this, setAnimationFlagsDirty, entityData);
		}
		if (original.RenderAsConnectedGroundSprite != null)
		{
			RenderAsConnectedGroundSprite = new RenderAsConnectedGroundSprite(original.RenderAsConnectedGroundSprite, this);
			RenderAsConnectedGroundSprite.Parent = entityData;
		}
		IsDrawn = original.IsDrawn;
	}

	private void UpdateSettingsFromEntity()
	{
		if (Entity != null)
		{
			FlipHorizontally = Entity.FlipHorizontally;
			if (Entity.EntityType.TreeType != null)
			{
				Entity.Find<Tree>(out var c);
				c.UpdateRenderable();
			}
			if (Entity.EntityType.StructureType != null)
			{
				Entity.Structure.UpdateRenderable();
			}
			if (Entity.EntityType.BodyType != null)
			{
				Entity.Body.UpdateRenderable();
			}
			if (Entity.EntityType.NonLivingType != null)
			{
				Entity.NonLivingEntity.UpdateRenderable();
			}
			if (Entity.EntityType.StructureType != null)
			{
				Entity.Structure.UpdateRenderable();
			}
		}
	}

	public void Initialize(bool updatePropertiesFromEntity)
	{
		if (updatePropertiesFromEntity)
		{
			UpdateSettingsFromEntity();
			if (RenderAsModel != null)
			{
				UpdateModelSettingsFromEntity();
			}
		}
		if (RenderAsModel != null)
		{
			RenderAsModel.Initialize();
			if (snapshotAttachables != null)
			{
				foreach (Tuple<string, string, AttacheePoint> snapshotAttachable in snapshotAttachables)
				{
					AttachPoint attachPointFromTag = RenderAsModel.ModelData.GetAttachPointFromTag(snapshotAttachable.Item2);
					AttachPooledObjectIfPossible(snapshotAttachable.Item1, attachPointFromTag, snapshotAttachable.Item3);
				}
			}
		}
		RecomputeUpdateInterval();
	}

	private void UpdateModelSettingsFromEntity()
	{
		RenderAsModel.FinalModelName = RenderableType.RenderAsModelType.AssetName;
		RenderAsModel.FinalModelScale = RenderableType.RenderAsModelType.ModelScale;
		RenderAsModel.FinalModelBasicTextureName = RenderableType.RenderAsModelType.ModelBasicTextureName;
		if (Parent == null)
		{
			return;
		}
		if (Parent.EntityType.Person != null)
		{
			Parent.EntityType.Person.UpdateRenderableRandomColors(this);
		}
		if (Parent.EntityType.BiologicalType != null)
		{
			BiologicalEntity biologicalEntity = Entity.BiologicalEntity;
			RenderAsModel.CustomColor3 = Parent.EntityType.BiologicalType.GetPrimaryColor(biologicalEntity.CasteType, biologicalEntity.RaceType, biologicalEntity.AgeGroup.Age);
			RenderAsModel.CustomColor2 = Parent.EntityType.BiologicalType.GetSecondaryColor(biologicalEntity.CasteType, biologicalEntity.RaceType, biologicalEntity.AgeGroup.Age);
			RenderAsModel.CustomColor1 = Parent.EntityType.BiologicalType.GetTertiaryColor(biologicalEntity.CasteType, biologicalEntity.RaceType, biologicalEntity.AgeGroup.Age);
			RenderAsModel.CustomColor0 = Parent.EntityType.BiologicalType.GetQuaternaryColor(biologicalEntity.CasteType, biologicalEntity.RaceType, biologicalEntity.AgeGroup.Age);
			string modelName = Parent.EntityType.BiologicalType.GetModelName(biologicalEntity.CasteType, biologicalEntity.RaceType, biologicalEntity.AgeGroup.Age);
			if (!string.IsNullOrEmpty(modelName))
			{
				RenderAsModel.FinalModelName = modelName;
			}
			RenderAsModel.FinalModelScale = Parent.EntityType.BiologicalType.GetModelScale(biologicalEntity.CasteType, biologicalEntity.RaceType, biologicalEntity.AgeGroup.Age, RenderAsModel.FinalModelScale);
			string text = (string.IsNullOrEmpty(biologicalEntity.ModelTextureName) ? Parent.EntityType.BiologicalType.GetModelBasicTexture(biologicalEntity.CasteType, biologicalEntity.RaceType, biologicalEntity.AgeGroup.Age) : biologicalEntity.ModelTextureName);
			if (!string.IsNullOrEmpty(text))
			{
				RenderAsModel.FinalModelBasicTextureName = text;
			}
		}
	}

	public void SetAnimationActionStateFlag(AnimAction state)
	{
		if (AnimConditions != null && AnimConditions.Action != state)
		{
			AnimConditions.Action = state;
			SetAnimFlagsDirty();
		}
	}

	public void SetAnimationStateFlags(AnimModifier[] states)
	{
		if (AnimConditions != null && states != null)
		{
			foreach (AnimModifier animationStateFlag in states)
			{
				SetAnimationStateFlag(animationStateFlag);
			}
		}
	}

	public void ClearAnimationStateFlags(AnimModifier[] states)
	{
		if (AnimConditions != null && states != null)
		{
			foreach (AnimModifier state in states)
			{
				ClearAnimationStateFlag(state);
			}
		}
	}

	public void SetAnimationStateFlag(AnimModifier state)
	{
		if (AnimConditions != null && !AnimConditions.Modifiers.Test(state))
		{
			AnimConditions.Modifiers.Set(state);
			switch (state)
			{
			case AnimModifier.High:
				AnimConditions.Modifiers.Clear(AnimModifier.Low);
				break;
			case AnimModifier.Low:
				AnimConditions.Modifiers.Clear(AnimModifier.High);
				break;
			case AnimModifier.Fast:
				AnimConditions.Modifiers.Clear(AnimModifier.Slow);
				break;
			case AnimModifier.Slow:
				AnimConditions.Modifiers.Clear(AnimModifier.Fast);
				break;
			case AnimModifier.Left:
				AnimConditions.Modifiers.Clear(AnimModifier.Right);
				break;
			case AnimModifier.Right:
				AnimConditions.Modifiers.Clear(AnimModifier.Left);
				break;
			case AnimModifier.Near:
				AnimConditions.Modifiers.Clear(AnimModifier.Far);
				break;
			case AnimModifier.Far:
				AnimConditions.Modifiers.Clear(AnimModifier.Near);
				break;
			case AnimModifier.Happy:
				AnimConditions.Modifiers.Clear(AnimModifier.Trouble);
				break;
			case AnimModifier.Trouble:
				AnimConditions.Modifiers.Clear(AnimModifier.Happy);
				break;
			case AnimModifier.Pre:
				AnimConditions.Modifiers.Clear(AnimModifier.Post);
				break;
			case AnimModifier.Post:
				AnimConditions.Modifiers.Clear(AnimModifier.Pre);
				break;
			case AnimModifier.Sitting:
				AnimConditions.Modifiers.Clear(AnimModifier.Kneeling);
				AnimConditions.Modifiers.Clear(AnimModifier.Lying);
				break;
			case AnimModifier.Kneeling:
				AnimConditions.Modifiers.Clear(AnimModifier.Sitting);
				AnimConditions.Modifiers.Clear(AnimModifier.Lying);
				break;
			case AnimModifier.Lying:
				AnimConditions.Modifiers.Clear(AnimModifier.Sitting);
				AnimConditions.Modifiers.Clear(AnimModifier.Kneeling);
				break;
			case AnimModifier.Spear:
			case AnimModifier.Rifle:
			case AnimModifier.Watergun:
			case AnimModifier.Knife:
			case AnimModifier.Hammer:
			case AnimModifier.Machete:
			case AnimModifier.Axe:
			case AnimModifier.Pickaxe:
			case AnimModifier.Bow:
			case AnimModifier.Hoe:
			case AnimModifier.Shovel:
				MutexOtherItemStates(state);
				break;
			}
			SetAnimFlagsDirty();
		}
	}

	public void ClearAnimationActionStateFlag(AnimAction state)
	{
		if (AnimConditions != null && AnimConditions.Action == state)
		{
			AnimConditions.Action = null;
			SetAnimFlagsDirty();
		}
	}

	public void ClearAnimationStateFlag(AnimModifier state)
	{
		if (AnimConditions != null && AnimConditions.Modifiers != null && AnimConditions.Modifiers.Test(state))
		{
			AnimConditions.Modifiers.Clear(state);
			SetAnimFlagsDirty();
		}
	}

	private void MutexOtherItemStates(AnimModifier state)
	{
		if (state != AnimModifier.Spear)
		{
			AnimConditions.Modifiers.Clear(AnimModifier.Spear);
		}
		if (state != AnimModifier.Rifle)
		{
			AnimConditions.Modifiers.Clear(AnimModifier.Rifle);
		}
		if (state != AnimModifier.Knife)
		{
			AnimConditions.Modifiers.Clear(AnimModifier.Knife);
		}
		if (state != AnimModifier.Machete)
		{
			AnimConditions.Modifiers.Clear(AnimModifier.Machete);
		}
		if (state != AnimModifier.Axe)
		{
			AnimConditions.Modifiers.Clear(AnimModifier.Axe);
		}
		if (state != AnimModifier.Pickaxe)
		{
			AnimConditions.Modifiers.Clear(AnimModifier.Pickaxe);
		}
		if (state != AnimModifier.Watergun)
		{
			AnimConditions.Modifiers.Clear(AnimModifier.Watergun);
		}
		if (state != AnimModifier.Bow)
		{
			AnimConditions.Modifiers.Clear(AnimModifier.Bow);
		}
		if (state != AnimModifier.Hammer)
		{
			AnimConditions.Modifiers.Clear(AnimModifier.Hammer);
		}
		if (state != AnimModifier.Hoe)
		{
			AnimConditions.Modifiers.Clear(AnimModifier.Hoe);
		}
		if (state != AnimModifier.Shovel)
		{
			AnimConditions.Modifiers.Clear(AnimModifier.Shovel);
		}
	}

	private void ReplaceSpriteConditionState()
	{
		IStateInfo bestMatch = null;
		RenderableType.FindBestStaticInfo(spriteConditions, RenderableType.ClientStateConditions, RenderableType.DefaultClientState, out bestMatch);
		if (bestMatch != null)
		{
			AdoptSpriteInfo((ClientStateInfo)bestMatch);
		}
	}

	private void AdoptSpriteInfo(ClientStateInfo newInfo)
	{
		if (newInfo == null)
		{
			newInfo = RenderableType.DefaultClientState;
		}
		ClientStateInfo clientStateInfo = selectedSpriteInfo;
		SelectedSpriteInfo = newInfo;
		if (clientStateInfo != selectedSpriteInfo)
		{
			AdoptSpriteInfo(selectedSpriteInfo, clientStateInfo);
		}
	}

	public void SetOrClearSpriteStateFlag(bool setFlag, StateModifier state)
	{
		if (setFlag)
		{
			SetSpriteStateFlag(state);
		}
		else
		{
			ClearSpriteStateFlag(state);
		}
	}

	public void SetSpriteStateFlag(StateModifier state)
	{
		bool flag = SetSpriteStateFlag(spriteConditions, state);
		spriteFlagsAreDirty |= flag;
	}

	public static bool SetSpriteStateFlag(BitMask64 spriteConditions, StateModifier state)
	{
		if (spriteConditions.Test(state))
		{
			return false;
		}
		spriteConditions.Set(state);
		switch (state)
		{
		case StateModifier.BeingBuilt:
			spriteConditions.Clear(StateModifier.Ordered);
			break;
		case StateModifier.Less:
			spriteConditions.Clear(StateModifier.More);
			break;
		case StateModifier.More:
			spriteConditions.Clear(StateModifier.Less);
			break;
		case StateModifier.HalfFull:
			spriteConditions.Clear(StateModifier.Full);
			break;
		case StateModifier.Full:
			spriteConditions.Clear(StateModifier.HalfFull);
			break;
		case StateModifier.Flavour1:
		case StateModifier.Flavour2:
		case StateModifier.Flavour3:
		case StateModifier.Flavour4:
		case StateModifier.Flavour5:
		case StateModifier.Flavour6:
		case StateModifier.Flavour7:
			MutexOtherFlavourStates(spriteConditions, state);
			break;
		}
		return true;
	}

	public void ClearSpriteStateFlag(StateModifier state)
	{
		if (ClearSpriteStateFlag(spriteConditions, state))
		{
			spriteFlagsAreDirty = true;
		}
	}

	public static bool ClearSpriteStateFlag(BitMask64 spriteConditions, StateModifier state)
	{
		if (!spriteConditions.Test(state))
		{
			return false;
		}
		spriteConditions.Clear(state);
		return true;
	}

	private static void MutexOtherFlavourStates(BitMask64 spriteConditions, StateModifier state)
	{
		if (state != StateModifier.Flavour1)
		{
			spriteConditions.Clear(StateModifier.Flavour1);
		}
		if (state != StateModifier.Flavour2)
		{
			spriteConditions.Clear(StateModifier.Flavour2);
		}
		if (state != StateModifier.Flavour3)
		{
			spriteConditions.Clear(StateModifier.Flavour3);
		}
		if (state != StateModifier.Flavour4)
		{
			spriteConditions.Clear(StateModifier.Flavour4);
		}
		if (state != StateModifier.Flavour5)
		{
			spriteConditions.Clear(StateModifier.Flavour5);
		}
		if (state != StateModifier.Flavour6)
		{
			spriteConditions.Clear(StateModifier.Flavour6);
		}
		if (state != StateModifier.Flavour7)
		{
			spriteConditions.Clear(StateModifier.Flavour7);
		}
	}

	public void AddParticleEmitter(ParticleEmitter emitter)
	{
		if (ParticleEmitters == null)
		{
			ParticleEmitters = new List<ParticleEmitter>();
		}
		ParticleEmitters.Add(emitter);
	}

	public void RemoveParticleEmitters(string systemKey)
	{
		if (ParticleEmitters != null)
		{
			ParticleEmitters.RemoveAll((ParticleEmitter p) => p.System.ParticleSystemType.KeyName == systemKey);
		}
	}

	public void RemoveParticleEmitter(ParticleEmitter emitter)
	{
		if (ParticleEmitters != null)
		{
			ParticleEmitters.Remove(emitter);
		}
	}

	public void AdoptParticleEffects(ParticleEmitterEffect[] newEffects, ParticleEmitterEffect[] oldEffects)
	{
		if (oldEffects != null)
		{
			ParticleEmitterEffect[] array = oldEffects;
			foreach (ParticleEmitterEffect particleEmitterEffect in array)
			{
				RemoveParticleEmitters(particleEmitterEffect.ParticleSystemKey);
			}
		}
		if (newEffects != null)
		{
			ParticleEmitterEffect[] array = newEffects;
			foreach (ParticleEmitterEffect particleEmitterEffect2 in array)
			{
				The.Client.ParticleManager.AddEmitter(particleEmitterEffect2.ParticleSystemKey, this, null, null, particleEmitterEffect2.EmitParticlesInParentDirection, null, particleEmitterEffect2.Offset);
			}
		}
	}

	private void AdoptSpriteInfo(ClientStateInfo newInfo, ClientStateInfo oldInfo)
	{
		AdoptParticleEffects(newInfo.ParticleEmitters, oldInfo?.ParticleEmitters);
		AdoptBillboards(newInfo);
		AdoptGroundSprite(newInfo);
		AdoptLighting(newInfo);
		AdoptSound(newInfo);
	}

	private void AdoptBillboards(ClientStateInfo newInfo)
	{
		if (newInfo.RenderAsBillboardType != null)
		{
			if (RenderAsBillboard == null)
			{
				RenderAsBillboard = new List<RenderAsBillboard>();
			}
			while (RenderAsBillboard.Count < newInfo.RenderAsBillboardType.Length)
			{
				RenderAsBillboard item = new RenderAsBillboard(this, null);
				RenderAsBillboard.Add(item);
			}
			SpriteSheet spritesheet = GameData.Instance.BillboardSpriteSheet;
			bool flag = newInfo.Test(StateModifier.Ordered) || DrawAsOverlay;
			if (flag)
			{
				spritesheet = The.Client.Renderer.GhostedStructuresSpriteSheet;
			}
			for (int i = 0; i < newInfo.RenderAsBillboardType.Length; i++)
			{
				RenderAsBillboard renderAsBillboard = RenderAsBillboard[i];
				RenderAsBillboardType billboardType = newInfo.RenderAsBillboardType[i];
				renderAsBillboard.Redraw(spritesheet, billboardType, flag);
				renderAsBillboard.ComputeMapPosition();
			}
			int count = RenderAsBillboard.Count;
			for (int j = newInfo.RenderAsBillboardType.Length; j < count; j++)
			{
				RenderAsBillboard.RemoveAt(j);
			}
		}
		else if (RenderAsBillboard != null)
		{
			RenderAsBillboard.Clear();
			RenderAsBillboard = null;
		}
		RecomputeUpdateInterval();
	}

	public void PrintStaticStates(StringBuilder states)
	{
		if (spriteConditions == null)
		{
			return;
		}
		states.Append("\nActual SpriteStateFlags:\n");
		states.Append(spriteConditions.StateNames);
		if (selectedSpriteInfo != null)
		{
			states.Append("\nBest Match Conditions: ");
			if (spriteConditions.Equals(selectedSpriteInfo.Conditions))
			{
				states.Append("PERFECT");
			}
			states.Append("\n");
			if (selectedSpriteInfo.Conditions != null)
			{
				states.Append(selectedSpriteInfo.Conditions.StateNames);
			}
			if (selectedSpriteInfo.Forbiddens != null && selectedSpriteInfo.Forbiddens.Any())
			{
				states.Append("\nBest Match Forbiddens:\n     ");
				states.Append(selectedSpriteInfo.Forbiddens.StateNames);
			}
			states.Append("\n");
		}
		else
		{
			states.Append("\n\n   MATCH FAILED -- selectedSpriteInfo is null -- this is bad.\n\n");
		}
	}

	public virtual void LocationChanged()
	{
		SetSpriteQuadsDirty();
		if (RenderAsBillboard != null)
		{
			for (int i = 0; i < RenderAsBillboard.Count; i++)
			{
				RenderAsBillboard[i].ComputeMapPosition();
			}
		}
	}

	private void AdoptSound(ClientStateInfo newInfo)
	{
		Looping loopSound = Looping.No;
		SoundData randomSound = GetRandomSound();
		AdoptStateSound(randomSound, null, loopSound, PickNewRandomSound);
	}

	public void PickNewRandomSound(PlayedSound soundThatEnded)
	{
		soundThatEnded.SoundEndedEvent -= PickNewRandomSound;
		if (selectedSpriteInfo.SoundDatas != null && selectedSpriteInfo.SoundDatas.Any((SoundData s) => s.KeyName == soundThatEnded.SoundData.KeyName))
		{
			Looping loopSound = Looping.No;
			if (selectedSpriteInfo.DelayBetweenSounds != null)
			{
				nextSoundTimepoint = UpdateTimePoints.ComputeTimePointFromInterval(selectedSpriteInfo.DelayBetweenSounds.GetRandomValue(The.Client.ClientRandomGenerator));
				RecomputeUpdateInterval();
			}
			else
			{
				nextSoundTimepoint = null;
				SoundData randomSound = GetRandomSound();
				AdoptStateSound(randomSound, null, loopSound, PickNewRandomSound);
			}
		}
	}

	private SoundData GetRandomSound()
	{
		if (selectedSpriteInfo.SoundDatas != null)
		{
			return Common.GetRandomListMember(selectedSpriteInfo.SoundDatas, The.Client.ClientRandomGenerator);
		}
		return null;
	}

	private void AdoptLighting(ClientStateInfo newInfo)
	{
		if (newInfo.LightingTypes != null)
		{
			if (LightSources == null)
			{
				LightSources = new List<LightSource>();
			}
			LightingType[] lightingTypes = newInfo.LightingTypes;
			foreach (LightingType item in lightingTypes)
			{
				if (!LightSources.Exists((LightSource l) => l.LightingType == item))
				{
					LightSource item2 = new LightSource(item, this);
					LightSources.Add(item2);
				}
			}
			LightSources.RemoveAll((LightSource p) => !newInfo.LightingTypes.Any((LightingType l) => l == p.LightingType));
		}
		else if (LightSources != null)
		{
			LightSources = null;
		}
	}

	private void AdoptGroundSprite(ClientStateInfo newInfo)
	{
		if (newInfo.RenderAsGroundSpriteType != null)
		{
			if (RenderAsGroundSprite == null)
			{
				RenderAsGroundSprite = new RenderAsGroundSprite(Entity, newInfo.RenderAsGroundSpriteType, this);
			}
			if (RenderAsGroundSprite != null)
			{
				string assetName = newInfo.RenderAsGroundSpriteType.AssetName;
				Rectangle? spriteRect = (string.IsNullOrEmpty(assetName) ? ((Rectangle?)null) : new Rectangle?(The.Client.FlatSpriteSheet.GetSourceRectangle(assetName)));
				RenderAsGroundSprite.Redraw(spriteRect);
			}
		}
		else if (RenderAsGroundSprite != null)
		{
			RenderAsGroundSprite = null;
		}
	}

	public void AdoptStateSound(SoundData newSound, SoundData oldSound, Looping loopSound, Action<PlayedSound> soundEndedCallback = null)
	{
		if (newSound != null && newSound == oldSound)
		{
			return;
		}
		if (StateSoundPlaying != null && StateSoundPlaying.Item2 != null && (newSound == null || !newSound.PlayMaxOneInstance || StateSoundPlaying.Item1 != newSound) && StateSoundPlaying.Item2.State == SoundState.Playing)
		{
			The.Client.AudioManager.StopSound(StateSoundPlaying.Item1, StateSoundPlaying.Item2);
			StateSoundPlaying = null;
		}
		if (newSound != null)
		{
			SoundEffectInstance soundEffectInstance = The.Client.AudioManager.PlayWorldSound(newSound, new WorldLocation(Location.Value), fadeProgress, loopSound == Looping.Yes, soundEndedCallback);
			if (soundEffectInstance != null)
			{
				StateSoundPlaying = new Tuple<SoundData, SoundEffectInstance>(newSound, soundEffectInstance);
			}
			else
			{
				StateSoundPlaying = null;
			}
		}
	}

	public virtual void PlayActionSound(SoundData sound)
	{
		if (Parent != null && The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(Parent.EntityID, out var _) != EntityResult.SeenDirectly)
		{
			return;
		}
		if (ActionSoundsPlaying == null)
		{
			ActionSoundsPlaying = new List<Tuple<SoundData, SoundEffectInstance>>();
		}
		for (int num = ActionSoundsPlaying.Count - 1; num >= 0; num--)
		{
			if (ActionSoundsPlaying[num].Item2 != null && ActionSoundsPlaying[num].Item2.State == SoundState.Stopped)
			{
				ActionSoundsPlaying.RemoveAt(num);
			}
		}
		SoundEffectInstance soundEffectInstance = The.Client.AudioManager.PlayWorldSound(sound, new WorldLocation(Location.Value), fadeProgress);
		if (soundEffectInstance != null)
		{
			ActionSoundsPlaying.Add(new Tuple<SoundData, SoundEffectInstance>(sound, soundEffectInstance));
		}
	}

	public bool CanLerpLocation()
	{
		if (Entity != null && Entity.EntityType.LocomotorType != null && Entity.IsStarted() != false)
		{
			return true;
		}
		return false;
	}

	public bool CanFade()
	{
		if (!RenderableType.CanFade() && (StateSoundPlaying == null || StateSoundPlaying.Item2.State != SoundState.Playing) && (ActionSoundsPlaying == null || ActionSoundsPlaying.Count <= 0 || !ActionSoundsPlaying.Exists((Tuple<SoundData, SoundEffectInstance> s) => s.Item2.State == SoundState.Playing)))
		{
			return ParticleEmitters != null;
		}
		return true;
	}

	public virtual void FadeIn()
	{
		if ((fadeStatus != FadeStatus.None || !(fadeProgress >= 1f)) && !destroyAfterFadeOut && fadeStatus != FadeStatus.FadeIn)
		{
			fadeStatus = FadeStatus.FadeIn;
			RecomputeUpdateInterval();
		}
	}

	public virtual void FadeOut(bool destroyAfterFadeOut = false)
	{
		if (fadeStatus != FadeStatus.FadeOut || this.destroyAfterFadeOut != destroyAfterFadeOut)
		{
			fadeStatus = FadeStatus.FadeOut;
			this.destroyAfterFadeOut = destroyAfterFadeOut;
			RecomputeUpdateInterval();
		}
	}

	public bool IsFlashing(AdditionalEffect effectID)
	{
		if (AdditionalEffects.TryGetValue(effectID, out var value))
		{
			return value.GetNumberOfEnvelopes() > 0;
		}
		return false;
	}

	public bool IsResourceContainerFlashing()
	{
		if (RenderAsIcon != null)
		{
			if (pulsingEffect != null)
			{
				return pulsingEffect.GetValue() == The.InGameUI.SelectedCyclePlayerFlashing;
			}
			return false;
		}
		if (AdditionalEffects != null && AdditionalEffects.TryGetValue(AdditionalEffect.Outline, out var value))
		{
			return value.GetValue() == The.InGameUI.SelectedCyclePlayerFlashing;
		}
		return false;
	}

	public virtual void SetPulsing()
	{
		Animation2DPlayer selectedCyclePlayer = The.InGameUI.SelectedCyclePlayer;
		if (pulsingEffect == null)
		{
			pulsingEffect = new Effect<Animation2DPlayer>(this, selectedCyclePlayer);
		}
		else
		{
			pulsingEffect.SetDefaultValue(selectedCyclePlayer);
		}
		RecomputeUpdateInterval();
	}

	public virtual void SetPulsing(AdditionalEffect effectID)
	{
		Animation2DPlayer selectedCyclePlayer = The.InGameUI.SelectedCyclePlayer;
		if (AdditionalEffects == null)
		{
			AdditionalEffects = new Dictionary<AdditionalEffect, Effect<Animation2DPlayer>>();
		}
		if (!AdditionalEffects.TryGetValue(effectID, out var value))
		{
			value = new Effect<Animation2DPlayer>(this, selectedCyclePlayer);
			AdditionalEffects.Add(effectID, value);
		}
		else
		{
			value.SetDefaultValue(selectedCyclePlayer);
		}
		RecomputeUpdateInterval();
	}

	public virtual void SetFlashing(AdditionalEffect effectID, float duration)
	{
		if (AdditionalEffects == null)
		{
			AdditionalEffects = new Dictionary<AdditionalEffect, Effect<Animation2DPlayer>>();
		}
		Animation2DPlayer selectedCyclePlayerFlashing = The.InGameUI.SelectedCyclePlayerFlashing;
		if (!AdditionalEffects.TryGetValue(effectID, out var value))
		{
			value = new Effect<Animation2DPlayer>(this, selectedCyclePlayerFlashing);
			AdditionalEffects.Add(effectID, value);
		}
		value.AddNewEnvelope(duration, selectedCyclePlayerFlashing);
		RecomputeUpdateInterval();
	}

	public virtual void SetOverlayPulsing()
	{
		Animation2DPlayer selectedCyclePlayer = The.InGameUI.SelectedCyclePlayer;
		if (overlayPulsingEffect == null)
		{
			overlayPulsingEffect = new Effect<Animation2DPlayer>(this, selectedCyclePlayer);
		}
		else
		{
			overlayPulsingEffect.SetDefaultValue(selectedCyclePlayer);
		}
		RecomputeUpdateInterval();
	}

	public virtual void SetFlashing(float duration)
	{
		if (pulsingEffect == null)
		{
			pulsingEffect = new Effect<Animation2DPlayer>(this, The.InGameUI.SelectedCyclePlayer);
		}
		Animation2DPlayer selectedCyclePlayerFlashing = The.InGameUI.SelectedCyclePlayerFlashing;
		pulsingEffect.AddNewEnvelope(duration, selectedCyclePlayerFlashing);
		RecomputeUpdateInterval();
	}

	public virtual void SetOverlayFlashing(float duration)
	{
		if (overlayPulsingEffect == null)
		{
			overlayPulsingEffect = new Effect<Animation2DPlayer>(this, The.InGameUI.SelectedCyclePlayer);
		}
		Animation2DPlayer selectedCyclePlayerFlashing = The.InGameUI.SelectedCyclePlayerFlashing;
		overlayPulsingEffect.AddNewEnvelope(duration, selectedCyclePlayerFlashing);
		RecomputeUpdateInterval();
	}

	public virtual void SetResourceContainerTintColor(Color color)
	{
		if (RenderAsIcon != null)
		{
			SetTintColor(color);
		}
		else
		{
			SetAdditionalTintColor(AdditionalEffect.Outline, color);
		}
	}

	public virtual void SetResourceContainerColorFlashing(Color color, float duration)
	{
		if (RenderAsIcon != null)
		{
			SetTintForDurationOfTime(color, duration);
			SetFlashing(duration);
		}
		else
		{
			SetAdditionalTintForDurationOfTime(AdditionalEffect.Outline, color, duration);
			SetFlashing(AdditionalEffect.Outline, duration);
		}
	}

	public Color GetOverlayTintColor()
	{
		if (overlayTintEffect != null)
		{
			return overlayTintEffect.GetValue();
		}
		return Color.White;
	}

	public Color GetTintColor()
	{
		if (tintEffect != null)
		{
			return tintEffect.GetValue();
		}
		return Color.White;
	}

	public void SetOverlayTintColor(Color color)
	{
		if (overlayTintEffect == null)
		{
			overlayTintEffect = new Effect<Color>(this, color);
		}
		else
		{
			overlayTintEffect.SetDefaultValue(color);
		}
		RecomputeUpdateInterval();
	}

	public void SetTintColor(Color color)
	{
		if (tintEffect == null)
		{
			tintEffect = new Effect<Color>(this, color);
		}
		else
		{
			tintEffect.SetDefaultValue(color);
		}
		RecomputeUpdateInterval();
	}

	public void SetAdditionalTintColor(AdditionalEffect effectID, Color color)
	{
		if (additionalTintEffects == null)
		{
			additionalTintEffects = new Dictionary<AdditionalEffect, Effect<Color>>();
		}
		if (!additionalTintEffects.TryGetValue(effectID, out var value))
		{
			value = new Effect<Color>(this, color);
			additionalTintEffects.Add(effectID, value);
		}
		else
		{
			value.SetDefaultValue(color);
		}
		RecomputeUpdateInterval();
	}

	public void SetTintForDurationOfTime(Color color, float time)
	{
		if (tintEffect == null)
		{
			tintEffect = new Effect<Color>(this, Color.White);
		}
		tintEffect.AddNewEnvelope(time, color);
		RecomputeUpdateInterval();
	}

	public void SetOverlayTintForDurationOfTime(Color color, float time)
	{
		if (overlayTintEffect == null)
		{
			overlayTintEffect = new Effect<Color>(this, Color.White);
		}
		overlayTintEffect.AddNewEnvelope(time, color);
		RecomputeUpdateInterval();
	}

	public void SetAdditionalTintForDurationOfTime(AdditionalEffect effectID, Color color, float time)
	{
		if (additionalTintEffects == null)
		{
			additionalTintEffects = new Dictionary<AdditionalEffect, Effect<Color>>();
		}
		if (!additionalTintEffects.TryGetValue(effectID, out var value))
		{
			value = new Effect<Color>(this, Color.White);
			additionalTintEffects.Add(effectID, value);
		}
		else
		{
			value.SetDefaultValue(Color.White);
		}
		value.AddNewEnvelope(time, color);
		RecomputeUpdateInterval();
	}

	public Vector4 GetCombinedEffects(AdditionalEffect effectID)
	{
		Vector4 vector = Vector4.One;
		if (AdditionalEffects != null)
		{
			vector = AdditionalEffects[effectID].GetValue().GetCurrentColor(null).ToVector4();
		}
		Vector4 vector2 = Vector4.One;
		if (additionalTintEffects != null)
		{
			vector2 = additionalTintEffects[effectID].GetValue().ToVector4();
		}
		return vector * vector2;
	}

	public Vector4 GetCombinedEffects()
	{
		return GetCommonEffects(pulsingEffect, tintEffect, fadeProgress);
	}

	public Color GetCombinedEffectsAsColor()
	{
		return new Color(GetCommonEffects(pulsingEffect, tintEffect, fadeProgress));
	}

	private static Vector4 GetCommonEffects(Effect<Animation2DPlayer> pulsingEffect, Effect<Color> tintingEffect, float fadeProgress)
	{
		Color color = pulsingEffect?.GetValue().GetCurrentColor(null) ?? Color.White;
		Color color2 = tintingEffect?.GetValue() ?? Color.White;
		return color.ToVector4() * color2.ToVector4() * fadeProgress;
	}

	public Color GetCombinedOverlayEffectsAsColor()
	{
		return new Color(GetCommonEffects(overlayPulsingEffect, overlayTintEffect, fadeProgress));
	}

	public Vector4 GetCombinedOverlayEffects()
	{
		return GetCommonEffects(overlayPulsingEffect, overlayTintEffect, fadeProgress);
	}

	public void GetOverlayGradientColors(out Vector4 gradient1, out Vector4 gradient2, out Vector4 gradient3)
	{
		if (renderEffect == RenderEffect.Memory)
		{
			gradient1 = memoryFactGradient1;
			gradient2 = memoryFactGradient2;
			gradient3 = memoryFactGradient3;
		}
		else
		{
			gradient1 = overlayGradient1;
			gradient2 = overlayGradient2;
			gradient3 = overlayGradient3;
		}
	}

	public void Destroy()
	{
		if (ParticleEmitters != null)
		{
			for (int num = ParticleEmitters.Count - 1; num >= 0; num--)
			{
				ParticleEmitter emitter = ParticleEmitters[num];
				The.Client.ParticleManager.RemoveEmitter(emitter);
			}
		}
		RenderableFactory.Remove(this);
	}

	private void BindToEntityID(EntityID id)
	{
	}

	public void spawnAttachableRenderable(string asset, bool randomlyRotate, int expireTimer)
	{
	}

	public void setTintStatus(TintStatus statusBits)
	{
		tintStatus |= statusBits;
	}

	public void clearTintStatus(TintStatus statusBits)
	{
		tintStatus = TintStatus.None;
	}

	public bool testTintStatus(TintStatus statusBits)
	{
		return (tintStatus & statusBits) != 0;
	}

	public void setTintColor(Color tintColor, uint preColorTime, uint postColorTime, uint sustainedColorTime, float tintFrequency, float tintAmplitude)
	{
	}

	public TintEnvelope getColorTintEnvelope()
	{
		return colorTintEnvelope;
	}

	public void setColorTintEnvelope(ref TintEnvelope source)
	{
		if (colorTintEnvelope != null)
		{
			colorTintEnvelope = source;
		}
	}

	public void setLightAttenuationStatus(LightAttenuationStatus statusBits)
	{
		lightAttenuationStatus |= statusBits;
	}

	public void clearLightAttenuationStatus(LightAttenuationStatus statusBits)
	{
		lightAttenuationStatus = LightAttenuationStatus.None;
	}

	public bool testLightAttenuationStatus(LightAttenuationStatus statusBits)
	{
		return (lightAttenuationStatus & statusBits) != 0;
	}

	public void setEnvironmentLightAttenuation(ref TintEnvelope source)
	{
		if (environmentLightAttenuation != null)
		{
			environmentLightAttenuation = source;
		}
	}

	public TintEnvelope getEnvironmentLightAttenuation()
	{
		return environmentLightAttenuation;
	}

	public void createSelectionDecal(uint numSelectedUnits, Color color)
	{
	}

	public RadiusDecalType getSelectionDecalTemplate()
	{
		return selectionDecalType;
	}

	public void assignSelectionDecalType(ref RadiusDecalType decalType)
	{
	}

	public void setSelectionDecalPosition(ref Vector3 position, ref Vector3 normal)
	{
	}

	public void setSelectionDecalColor(Color color)
	{
	}

	public void removeSelectionDecal()
	{
	}

	public void SetMemoryRendering(bool value)
	{
		if (value && renderEffect == RenderEffect.Normal)
		{
			renderEffect = RenderEffect.Memory;
		}
		else
		{
			renderEffect = RenderEffect.Normal;
		}
	}

	public void SetOverlayRendering(bool value)
	{
		if (value)
		{
			renderEffect = RenderEffect.Overlay;
		}
		else
		{
			renderEffect = RenderEffect.Normal;
		}
	}

	public void UpdateIsDrawnStatus()
	{
		bool flag = false;
		if (RenderAsModel != null && RenderAsModel.AttachedTo != null)
		{
			flag = true;
		}
		if (!Entity.GetContainedBy(out Container container))
		{
			IsDrawn = false;
		}
		else
		{
			IsDrawn = Entity.PartOf == null && (container == null || container.IsOpenContainer() || container.IsVisible(Entity)) && !flag && !Entity.DrivingVehicle.HasValue && !Entity.PassengerInVehicle.HasValue;
		}
	}

	public void SetNormalRendering()
	{
		renderEffect = RenderEffect.Normal;
	}

	public void setSelectable(bool selectable)
	{
	}

	public bool isSelectable()
	{
		return true;
	}

	public void reactToGeometryChange()
	{
	}

	public float getScale()
	{
		return 1f;
	}

	private void setFogOfWarClearFrame(uint frame)
	{
		fogOfWarClearFrame = frame;
	}

	private uint getFogOfWarClearFrame()
	{
		return fogOfWarClearFrame;
	}

	public void setFullyObscuredByShroud(bool fullyObscured)
	{
	}

	public void colorFlash(Color color, uint decayFrames = 16u, uint attackFrames = 0u, uint sustainAtPeak = 0u)
	{
	}

	public void colorTint(Color color)
	{
	}

	public void setTintEnvelope(Color color, float attack, float decay)
	{
	}

	public void flashAsSelected(Color? color = null)
	{
	}

	public void updateLightAttenuation()
	{
	}

	public bool isSelected()
	{
		return selected;
	}

	public void onSelected()
	{
	}

	public void onUnselected()
	{
	}

	public void recalcCollisionType()
	{
	}

	public void setTimeOfDay(GameTime tod)
	{
	}

	public virtual void AttachPooledObjectIfPossible(string attachRenderableKey, AttachPoint attachor, AttacheePoint attacheePoint, bool saveToSnapshot = false)
	{
		if (RenderAsModel != null && !RenderAsModel.AnimatedModel.ModelAnimator.HasAttachedObject(attachor.BoneName))
		{
			if (saveToSnapshot)
			{
				Common.AddToList(ref snapshotAttachables, new Tuple<string, string, AttacheePoint>(attachRenderableKey, attachor.Tag, attacheePoint));
			}
			Renderable freeAttachableRenderable = The.Client.Renderer.GetFreeAttachableRenderable(attachRenderableKey);
			RenderAsModel.GetAttachTransformations(freeAttachableRenderable, attachor, attacheePoint, RenderAsModel.SelectedAnimInfo, out var attacheePoint2, out var translation, out var rotation);
			if (!AttachObject(freeAttachableRenderable.RenderAsModel, attachor, attacheePoint2, attacheePoint, translation, rotation, testIfAlreadyAttached: false))
			{
				The.Client.Renderer.RetireAttachableRenderable(freeAttachableRenderable);
			}
		}
	}

	public virtual void RemoveAttachedMountedRenderables(string renderableKey)
	{
		if (RenderAsModel != null)
		{
			RemoveAndRetireAllAttachedModels(RenderAsModel.ModelData.RightHandAttachor, renderableKey);
			RemoveAndRetireAllAttachedModels(RenderAsModel.ModelData.LeftHandAttachor, renderableKey);
		}
	}

	public virtual void AttachPooledObjectIfPossible(string attachRenderableKey, string attachorTag, AttacheePoint attacheePoint, bool saveToSnapshot = false)
	{
		if (RenderAsModel != null)
		{
			AttachPoint attachPointFromTag = RenderAsModel.ModelData.GetAttachPointFromTag(attachorTag);
			if (attachPointFromTag != null)
			{
				AttachPooledObjectIfPossible(attachRenderableKey, attachPointFromTag, attacheePoint, saveToSnapshot);
			}
		}
	}

	public virtual void DeattachAndRetireObject(IAttachable attachedObject, string attachorBoneName)
	{
		RenderAsModel.AnimatedModel.ModelAnimator.DeattachObject(attachedObject, attachorBoneName);
		The.Client.Renderer.RetireAttachableRenderable(((RenderAsModel)attachedObject).Renderable);
	}

	public virtual void RemoveAndRetireAllAttachedModels(AttachPoint attachor, string renderableKey)
	{
		if (RenderAsModel.AnimatedModel.ModelAnimator.AttachedObjects.TryGetValue(attachor.BoneName, out var value))
		{
			int num;
			for (num = 0; num < value.Count; num++)
			{
				IAttachable attachedObject = value[num];
				ClearSnapshotAttachables(renderableKey, attachor);
				DeattachAndRetireObject(attachedObject, attachor.BoneName);
				num--;
			}
		}
	}

	public virtual void RemoveAttachables(string renderableKey, AttachPoint attachor)
	{
		if (!RenderAsModel.AnimatedModel.ModelAnimator.AttachedObjects.TryGetValue(attachor.BoneName, out var value))
		{
			return;
		}
		for (int num = value.Count - 1; num >= 0; num--)
		{
			IAttachable attachable = value[num];
			ClearSnapshotAttachables(renderableKey, attachor);
			if (((RenderAsModel)attachable).Renderable.RenderableType.KeyName == renderableKey)
			{
				DeattachAndRetireObject(attachable, attachor.BoneName);
			}
		}
	}

	private void ClearSnapshotAttachables(string renderableKey, AttachPoint attachor)
	{
		if (snapshotAttachables != null)
		{
			snapshotAttachables.RemoveAll((Tuple<string, string, AttacheePoint> t) => (renderableKey == null || t.Item1 == renderableKey) && t.Item2 == attachor.Tag);
		}
	}

	public virtual void UpdateAttachBoxModelToHand()
	{
		AnimModifier? burdenAnimStateFlag = GetBurdenAnimStateFlag(Entity.AgentStorage.GetHaulingPercentageOfCapacity());
		AttachBoxModelToHand(burdenAnimStateFlag);
	}

	public virtual void AttachBoxModelToHand(AnimModifier? burdenFlag)
	{
		BoxHandlingWhenHauling boxHandlingWhenHauling = RenderableType.BoxHandlingWhenHauling;
		if (boxHandlingWhenHauling != null)
		{
			if (boxHandlingWhenHauling.ShowBoxInHand.HasValue && (burdenFlag == AnimModifier.Heavy || boxHandlingWhenHauling.BoxHandling == BoxHandlingWhenHauling.BoxHandlingType.AlwaysOnBack))
			{
				AttachPooledObjectIfPossible("box", RenderAsModel.ModelData.GetAttachPointFromTag(boxHandlingWhenHauling.AttachorWhenBoxIsInHand), boxHandlingWhenHauling.ShowBoxInHand.Value, saveToSnapshot: true);
			}
			if (boxHandlingWhenHauling.BoxHandling == BoxHandlingWhenHauling.BoxHandlingType.OnlyOnBackWhenHeavyAndHaulingFar)
			{
				RemoveAttachedHauledObjectsFromBack("backpackHeavy");
			}
		}
	}

	public virtual void AttachModelToBack(string boxModelKey, AttacheePoint attacheePoint)
	{
		AttachPooledObjectIfPossible(boxModelKey, RenderAsModel.ModelData.BackAttachor, attacheePoint, saveToSnapshot: true);
		RemoveAttachedBoxModelsFromHand();
	}

	public virtual void RemoveHauledObjects()
	{
		RemoveAttachedBoxModelsFromHand();
		RemoveAttachedHauledObjectsFromBack("backpackHeavy");
		RemoveAttachedHauledObjectsFromBack("box");
	}

	public virtual void UpdateBurdenState(AgentStorage.BurdenState burdenState)
	{
		BoxHandlingWhenHauling boxHandlingWhenHauling = Parent.EntityType.RenderableTypeMode.BoxHandlingWhenHauling;
		if (boxHandlingWhenHauling == null)
		{
			return;
		}
		if (boxHandlingWhenHauling.BoxHandling == BoxHandlingWhenHauling.BoxHandlingType.AlwaysOnBack)
		{
			if (burdenState == AgentStorage.BurdenState.None)
			{
				RemoveHauledObjects();
			}
		}
		else if (burdenState != AgentStorage.BurdenState.HaulHeavy)
		{
			RemoveHauledObjects();
		}
	}

	public virtual void RemoveAttachedHauledObjectsFromBack(string objectKey)
	{
		if (RenderAsModel != null)
		{
			AttachPoint backAttachor = RenderAsModel.ModelData.BackAttachor;
			if (backAttachor != null)
			{
				RemoveAttachables(objectKey, backAttachor);
			}
		}
	}

	public virtual void RemoveAttachedBoxModelsFromHand()
	{
		if (RenderAsModel != null)
		{
			AttachPoint rightHandAttachor = RenderAsModel.ModelData.RightHandAttachor;
			if (rightHandAttachor != null)
			{
				RemoveAttachables("box", rightHandAttachor);
			}
			rightHandAttachor = RenderAsModel.ModelData.LeftHandAttachor;
			if (rightHandAttachor != null)
			{
				RemoveAttachables("box", rightHandAttachor);
			}
		}
	}

	public virtual void RemoveAttachables(string renderableKey, string attachorTag)
	{
		if (RenderAsModel != null)
		{
			AttachPoint attachPointFromTag = RenderAsModel.ModelData.GetAttachPointFromTag(attachorTag);
			if (attachPointFromTag != null)
			{
				RemoveAttachables(renderableKey, attachPointFromTag);
			}
		}
	}

	public virtual bool AttachObject(IAttachable objectToAttach, AttachPoint attachor, AttachPoint attachee, AttacheePoint? attacheePoint, Vector3 translation, Vector3 rotation, bool testIfAlreadyAttached = true)
	{
		if (RenderAsModel == null)
		{
			return false;
		}
		if (testIfAlreadyAttached && RenderAsModel.AnimatedModel.ModelAnimator.HasAttachedObject(attachor.BoneName))
		{
			return false;
		}
		Renderable renderable = ((RenderAsModel)objectToAttach).Renderable;
		AttachEntityAndCreateLocalTransform(renderable, renderable.RenderAsModel.FinalModelScale, attachor, attachee, attacheePoint, translation, rotation);
		return true;
	}

	public virtual void AttachEntityAndCreateLocalTransform(Renderable renderableToAttach, float scaling, AttachPoint attachorPoint, AttachPoint attachee, AttacheePoint? attacheePoint, Vector3 translation, Vector3 rotation)
	{
		Matrix localTransform = CreateTransformForAttachedEntity(scaling, attachorPoint, attachee, translation, rotation);
		renderableToAttach.RenderAsModel.LocalTransform = localTransform;
		if (!string.IsNullOrEmpty(attachorPoint.BoneName))
		{
			BonePose attacheeBone = renderableToAttach.RenderAsModel.AnimatedModel.ModelAnimator.BonePoses[attachee.BoneName];
			RenderAsModel.AnimatedModel.ModelAnimator.AttachObject(renderableToAttach.RenderAsModel, attachorPoint, attachee, attacheePoint, attacheeBone, Parent);
		}
	}

	public static Matrix CreateTransformForAttachedEntity(float scaling, AttachPoint attachorPoint, AttachPoint attachee, Vector3 translation, Vector3 rotation)
	{
		Matrix matrix = Matrix.CreateScale(scaling);
		Matrix identity = Matrix.Identity;
		Matrix identity2 = Matrix.Identity;
		identity2 = Matrix.CreateTranslation(translation);
		if (rotation != Vector3.Zero)
		{
			identity *= Matrix.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(rotation.X));
			identity *= Matrix.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(rotation.Y));
			identity *= Matrix.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(rotation.Z));
		}
		return matrix * identity * identity2;
	}

	public virtual void UpdateMainAnimationTimeScalar(double scalar)
	{
		if (RenderAsModel != null)
		{
			RenderAsModel.UpdateAnimationManuallyByTimeScalar(scalar);
		}
	}

	public virtual void SetAnimFlagsDirty()
	{
		if (Entity != null && Entity.Name != null)
		{
			Entity.Name.Contains("Pezal");
		}
		if (RenderAsModel != null)
		{
			RenderAsModel.SetAnimFlagsDirty();
		}
	}

	public virtual void SetStaticFlagsDirty()
	{
	}

	public void UpdateAnimationConditionState()
	{
		if (RenderAsModel != null)
		{
			RenderAsModel.UpdateAnimationConditionState();
		}
	}

	private void RecomputeUpdateInterval()
	{
		RecomputeUpdateInterval(out var _);
	}

	public void RecomputeUpdateInterval(out bool intervalChanged)
	{
		intervalChanged = false;
		double? currentInterval = null;
		if (Location.HasValue || Parent == null || !(Parent is Entity))
		{
			currentInterval = GetOverlayEnvelopeEffectsUpdateInterval();
			UpdateTimePoints.GetSoonestInterval(GetAdditionalEffectsUpdateInterval(), ref currentInterval);
			UpdateTimePoints.GetSoonestInterval(GetFadingUpdateInterval(), ref currentInterval);
			UpdateTimePoints.GetSoonestInterval(GetIntervalForExpiry(), ref currentInterval);
			UpdateTimePoints.GetSoonestInterval(GetRenderAsModelInterval(), ref currentInterval);
			UpdateTimePoints.GetSoonestInterval(GetRenderAsBillboardInterval(), ref currentInterval);
			UpdateTimePoints.GetSoonestInterval(GetNextSoundInterval(), ref currentInterval);
		}
		if (!Common.IsEqual(UpdateInterval, currentInterval))
		{
			UpdateInterval = currentInterval;
			intervalChanged = true;
		}
	}

	public virtual void SetToParentLocation()
	{
		if (RenderAsModel != null)
		{
			RenderAsModel.SetToParentLocation();
		}
	}

	public virtual void UpdateLerpingBackSpine(GameTime gameTime)
	{
		if (AnimatedHead != null)
		{
			AnimatedHead.UpdateLerpingBackSpine(gameTime);
		}
	}

	public virtual void StartLerpingBackSpine()
	{
		if (AnimatedHead != null)
		{
			AnimatedHead.StartLerpingBackSpine();
		}
	}

	public virtual void TurnToLook(Vector2 lookTarget, float lookAngle, float lookTargetAngle)
	{
		if (AnimatedHead != null)
		{
			AnimatedHead.TurnToLook(lookTarget, lookAngle, lookTargetAngle);
		}
	}

	public void Update(GameTime gameTime, out bool wasDestroyed)
	{
		wasDestroyed = false;
		if (RenderAsModel != null)
		{
			RenderAsModel.Update(gameTime);
		}
		if (RenderAsBillboard != null)
		{
			foreach (RenderAsBillboard item in RenderAsBillboard)
			{
				item.Update(gameTime);
			}
		}
		bool effectsWereUpdated = false;
		UpdateFading(gameTime, ref effectsWereUpdated);
		UpdateEnvelopeEffects(gameTime, ref effectsWereUpdated);
		if (effectsWereUpdated)
		{
			SetSpritePropertiesDirty();
		}
		bool effectsWereUpdated2 = false;
		UpdateOverlayEnvelopeEffects(gameTime, ref effectsWereUpdated2);
		if (effectsWereUpdated2)
		{
			SetOverlaySpritePropertiesDirty();
		}
		UpdateAdditionalEffects(gameTime);
		UpdateSoundDelay();
		UpdateExpiry(out wasDestroyed);
	}

	private void SetSpritePropertiesDirty()
	{
		if (RenderAsGroundSprite != null)
		{
			RenderAsGroundSprite.SetPropertiesAreDirty();
		}
		if (RenderAsBillboard == null)
		{
			return;
		}
		foreach (RenderAsBillboard item in RenderAsBillboard)
		{
			item.SetPropertiesAreDirty();
		}
	}

	private void SetOverlaySpritePropertiesDirty()
	{
		if (RenderAsBillboard != null)
		{
			foreach (RenderAsBillboard item in RenderAsBillboard)
			{
				item.SetOverlayPropertiesAreDirty();
			}
		}
		if (RenderAsGroundSprite != null)
		{
			RenderAsGroundSprite.SetOverlayPropertiesAreDirty();
		}
	}

	private void SetSpriteQuadsDirty()
	{
		if (RenderAsGroundSprite != null)
		{
			RenderAsGroundSprite.SetIsDirty();
		}
		if (RenderAsBillboard != null)
		{
			foreach (RenderAsBillboard item in RenderAsBillboard)
			{
				item.SetIsDirty();
			}
		}
		if (LightSources != null)
		{
			LightSources.ForEach(delegate(LightSource l)
			{
				l.SetIsDirty();
			});
		}
	}

	private double? GetRenderAsModelInterval()
	{
		if (RenderAsModel != null)
		{
			return RenderAsModel.GetUpdateInterval();
		}
		return null;
	}

	private double? GetRenderAsBillboardInterval()
	{
		if (RenderAsBillboard != null)
		{
			double? currentInterval = null;
			{
				foreach (RenderAsBillboard item in RenderAsBillboard)
				{
					UpdateTimePoints.GetSoonestInterval(item.GetUpdateInterval(), ref currentInterval);
				}
				return currentInterval;
			}
		}
		return null;
	}

	private void UpdateSoundDelay()
	{
		if (nextSoundTimepoint.HasValue && The.Sim.TimepointReached(nextSoundTimepoint.Value))
		{
			AdoptSound(selectedSpriteInfo);
			nextSoundTimepoint = null;
		}
	}

	private void UpdateExpiry(out bool wasDestroyed)
	{
		wasDestroyed = false;
		if (ExpiryTimePointInSeconds.HasValue && The.Sim.TimepointReached(ExpiryTimePointInSeconds.Value))
		{
			if (RenderableType.FadeOutWhenDestroyed)
			{
				FadeOut(destroyAfterFadeOut: true);
			}
			else
			{
				Destroy();
				wasDestroyed = true;
			}
			ExpiryTimePointInSeconds = null;
		}
	}

	private double? GetIntervalForExpiry()
	{
		return UpdateTimePoints.ComputeIntervalFromTimepoint(ExpiryTimePointInSeconds);
	}

	private double? GetOverlayEnvelopeEffectsUpdateInterval()
	{
		double? currentInterval = null;
		if (overlayTintEffect != null)
		{
			UpdateTimePoints.GetSoonestInterval(overlayTintEffect.GetUpdateInterval(), ref currentInterval);
		}
		if (overlayPulsingEffect != null)
		{
			UpdateTimePoints.GetSoonestInterval(overlayPulsingEffect.GetUpdateInterval(), ref currentInterval);
		}
		return currentInterval;
	}

	private double? GetEnvelopeEffectsUpdateInterval()
	{
		double? currentInterval = null;
		if (tintEffect != null)
		{
			UpdateTimePoints.GetSoonestInterval(tintEffect.GetUpdateInterval(), ref currentInterval);
		}
		if (pulsingEffect != null)
		{
			UpdateTimePoints.GetSoonestInterval(pulsingEffect.GetUpdateInterval(), ref currentInterval);
		}
		return currentInterval;
	}

	private double? GetAdditionalEffectsUpdateInterval()
	{
		double? currentInterval = null;
		if (AdditionalEffects != null)
		{
			foreach (KeyValuePair<AdditionalEffect, Effect<Animation2DPlayer>> additionalEffect in AdditionalEffects)
			{
				UpdateTimePoints.GetSoonestInterval(additionalEffect.Value.GetUpdateInterval(), ref currentInterval);
			}
		}
		if (additionalTintEffects != null)
		{
			foreach (KeyValuePair<AdditionalEffect, Effect<Color>> additionalTintEffect in additionalTintEffects)
			{
				UpdateTimePoints.GetSoonestInterval(additionalTintEffect.Value.GetUpdateInterval(), ref currentInterval);
			}
		}
		return currentInterval;
	}

	private void UpdateOverlayEnvelopeEffects(GameTime gameTime, ref bool effectsWereUpdated)
	{
		if (overlayTintEffect != null)
		{
			overlayTintEffect.Update(gameTime);
			effectsWereUpdated = true;
		}
		if (overlayPulsingEffect != null)
		{
			overlayPulsingEffect.Update(gameTime);
			effectsWereUpdated = true;
		}
	}

	private void UpdateAdditionalEffects(GameTime gameTime)
	{
		if (AdditionalEffects != null)
		{
			foreach (KeyValuePair<AdditionalEffect, Effect<Animation2DPlayer>> additionalEffect in AdditionalEffects)
			{
				additionalEffect.Value.Update(gameTime);
			}
		}
		if (additionalTintEffects == null)
		{
			return;
		}
		foreach (KeyValuePair<AdditionalEffect, Effect<Color>> additionalTintEffect in additionalTintEffects)
		{
			additionalTintEffect.Value.Update(gameTime);
		}
	}

	private void UpdateEnvelopeEffects(GameTime gameTime, ref bool effectsWereUpdated)
	{
		if (tintEffect != null)
		{
			tintEffect.Update(gameTime);
			effectsWereUpdated = true;
		}
		if (pulsingEffect != null)
		{
			pulsingEffect.Update(gameTime);
			effectsWereUpdated = true;
		}
	}

	private void UpdateFading(GameTime gameTime, ref bool effectsWereUpdated)
	{
		if (fadeStatus == FadeStatus.None)
		{
			return;
		}
		effectsWereUpdated = true;
		float num = 2f;
		float num2 = (float)(gameTime.ElapsedGameTime.TotalSeconds / (double)num);
		if (fadeStatus == FadeStatus.FadeIn)
		{
			fadeProgress += num2;
		}
		else
		{
			fadeProgress -= num2;
		}
		fadeProgress = Common.Clamp(fadeProgress, 0f, 1f);
		UpdateSoundDirectionAndDistance();
		if ((double)fadeProgress >= 1.0)
		{
			fadeStatus = FadeStatus.None;
			fadeProgress = 1f;
		}
		else if (fadeProgress <= 0f)
		{
			The.Client.Renderer.renderablesFadingOut.Remove(this);
			fadeStatus = FadeStatus.None;
			fadeProgress = 0f;
			if (destroyAfterFadeOut)
			{
				Destroy();
			}
		}
	}

	private double? GetNextSoundInterval()
	{
		if (nextSoundTimepoint.HasValue)
		{
			return UpdateTimePoints.ComputeIntervalFromTimepoint(nextSoundTimepoint.Value);
		}
		return null;
	}

	private double? GetFadingUpdateInterval()
	{
		if (fadeStatus != FadeStatus.None)
		{
			return 0.0;
		}
		return null;
	}

	private void UpdateSoundDirectionAndDistance()
	{
		UpdateSoundDirectionAndDistance(StateSoundPlaying);
		if (ActionSoundsPlaying == null)
		{
			return;
		}
		foreach (Tuple<SoundData, SoundEffectInstance> item in ActionSoundsPlaying)
		{
			UpdateSoundDirectionAndDistance(item);
		}
	}

	private void UpdateSoundDirectionAndDistance(Tuple<SoundData, SoundEffectInstance> sound)
	{
		if (sound != null && sound.Item2.State == SoundState.Playing)
		{
			The.Client.AudioManager.SetSoundLocation(sound.Item2, sound.Item1, fadeProgress, Location.Value);
		}
	}

	private void StopSounds()
	{
		StopSound(StateSoundPlaying);
		if (ActionSoundsPlaying == null)
		{
			return;
		}
		foreach (Tuple<SoundData, SoundEffectInstance> item in ActionSoundsPlaying)
		{
			StopSound(item);
		}
	}

	private void StopSound(Tuple<SoundData, SoundEffectInstance> sound)
	{
		if (sound != null && sound.Item2.State == SoundState.Playing)
		{
			The.Client.AudioManager.StopSound(sound.Item1, sound.Item2);
		}
	}

	public void Draw(GameWorldRenderer.RenderTechnique technique, ref Matrix view, ref Matrix projection, float drawWithAlpha = 1f, float lightIntensity = 1f, Color? tintColor = null)
	{
		if (IsDrawn)
		{
			Color combinedEffectsAsColor = GetCombinedEffectsAsColor();
			DrawModel(technique, ref view, ref projection, drawWithAlpha, lightIntensity, ref tintColor);
			if (technique == GameWorldRenderer.RenderTechnique.Standard)
			{
				DrawParticleEmitters(combinedEffectsAsColor);
				UpdateSoundDirectionAndDistance();
			}
		}
	}

	private void DrawModel(GameWorldRenderer.RenderTechnique technique, ref Matrix view, ref Matrix projection, float drawWithAlpha, float lightIntensity, ref Color? tintColor)
	{
		if (RenderAsModel == null)
		{
			return;
		}
		RenderAsModel.UpdateAnimationConditionState();
		float dirtLevel = 0f;
		if (Parent != null && Parent.Condition.HasValue)
		{
			dirtLevel = 1f - (float)Parent.Condition.Value;
		}
		RenderAsModel renderAsModel = RenderAsModel;
		if (renderEffect == RenderEffect.Memory && technique == GameWorldRenderer.RenderTechnique.StandardOverlay)
		{
			tintColor = Color.Cyan;
		}
		renderAsModel.AnimatedModel.DrawStandard(technique, view, projection, Color.Cyan.ToVector3(), Color.Magenta.ToVector3(), Color.Yellow.ToVector3(), Color.Red.ToVector3(), drawWithAlpha, lightIntensity, dirtLevel, renderAsModel.FinalModelBasicTexture, tintColor);
		foreach (KeyValuePair<string, List<IAttachable>> attachedObject in RenderAsModel.AnimatedModel.ModelAnimator.AttachedObjects)
		{
			foreach (IAttachable item in attachedObject.Value)
			{
				if (!HideHandAttachments || !item.AttachorPoint.IsHand)
				{
					RenderAsModel renderAsModel2 = item as RenderAsModel;
					renderAsModel2.AnimatedModel.StandardDrawingWorldTransformation = item.CombinedTransform;
					renderAsModel2.AnimatedModel.CreateBoneTransformMatrixArray();
					dirtLevel = 0f;
					if (renderAsModel2.Parent != null && renderAsModel2.Parent.Condition.HasValue)
					{
						dirtLevel = 1f - (float)renderAsModel2.Parent.Condition.Value;
					}
					renderAsModel2.AnimatedModel.DrawStandard(technique, view, projection, renderAsModel2.CustomColor0, renderAsModel2.CustomColor1, renderAsModel2.CustomColor2, renderAsModel2.CustomColor3, drawWithAlpha, lightIntensity, dirtLevel, renderAsModel2.FinalModelBasicTexture, tintColor);
				}
			}
		}
	}

	private void DrawParticleEmitters(Color combinedEffects)
	{
		if (ParticleEmitters == null)
		{
			return;
		}
		foreach (ParticleEmitter particleEmitter in ParticleEmitters)
		{
			particleEmitter.Draw(combinedEffects);
		}
		The.Client.GraphicsDevice.BlendState = BlendState.AlphaBlend;
	}

	public void SetOverlayGradientColors(Color gradient1, Color gradient2, Color gradient3)
	{
		overlayGradient1 = gradient1.ToVector4();
		overlayGradient2 = gradient2.ToVector4();
		overlayGradient3 = gradient3.ToVector4();
		SetOverlaySpritePropertiesDirty();
	}

	public virtual void FlashAsDetected()
	{
	}

	public virtual void ComputeMatricesForDrawing()
	{
		if (RenderAsModel != null)
		{
			RenderAsModel.ComputeMatricesForDrawing(AnimatedModel.Transformations.All, RenderAsModel.FinalModelScale);
			RenderAsModel.CopyAbsoluteTransforms();
		}
	}

	public static AnimModifier? GetBurdenAnimStateFlag(float storedPercentage)
	{
		return (!(storedPercentage > GameData.Instance.Constants.StoredPercentageMeansHeavyHaul)) ? ((AnimModifier?)null) : new AnimModifier?(AnimModifier.Heavy);
	}

	public virtual void StartAdditionalAnimation(string animKey, Playback playback, StartingPoint startingPoint, BlendMode mode, float speedFactor = 1f, Looping looping = Looping.No, float? startOffsetToAdd = null)
	{
		RenderAsModel.StartAdditionalAnimation(animKey, playback, startingPoint, mode, speedFactor, looping, startOffsetToAdd);
	}

	public virtual void SetModelBoneRotation(string boneName, Matrix rotation)
	{
		BonePose bonePose = RenderAsModel.ModelAnimator.BonePoses[boneName];
		bonePose.DefaultTransform = rotation * Matrix.CreateTranslation(bonePose.DefaultTransform.Translation);
		bonePose.UseSpecialTransform = true;
	}

	public virtual void StopOverridingAnimTransforms(string boneName)
	{
		RenderAsModel.ModelAnimator.BonePoses[boneName].UseSpecialTransform = false;
	}

	private void DrawEntityWaypoints(GameWorldRenderer.RenderTechnique technique)
	{
		Color pink = Color.Pink;
		if (technique != GameWorldRenderer.RenderTechnique.Standard || Entity == null || !Entity.Find<Intelligence>(out var c))
		{
			return;
		}
		The.Client.DrawPoint(The.MapUI.WorldPosToScreen(Location.Value), Color.White);
		if (c.GroupMoveAssignedWaypoint != null)
		{
			The.Client.DrawPoint(The.MapUI.WorldPosToScreen(c.GroupMoveAssignedWaypoint.Location), pink);
		}
		List<Waypoint> waypointPath = c.Brain.GetWaypointPath();
		if (waypointPath != null)
		{
			Vector2? vector = The.MapUI.WorldPosToScreen(Location.Value);
			foreach (Waypoint item in waypointPath)
			{
				if (vector.HasValue)
				{
					Shape.Line(vector.Value, The.MapUI.WorldPosToScreen(item.Location), pink, pink);
				}
				vector = The.MapUI.WorldPosToScreen(item.Location);
				Shape.Box(new Vector2(vector.Value.X - 2f, vector.Value.Y - 2f), new Vector2(vector.Value.X + 2f, vector.Value.Y + 2f), Color.Black, solid: true);
			}
		}
		if (Entity.Find<Locomotor>(out var c2))
		{
			The.Client.DrawPoint(The.MapUI.WorldPosToScreen(c2.CurrentMoveTarget), Color.Red);
		}
	}

	public static void GetExcludedStanceFlags(AnimModifier? stanceFlag, List<AnimModifier> excludedFlagsResult)
	{
		if (stanceFlag.HasValue)
		{
			switch (stanceFlag.Value)
			{
			case AnimModifier.Lying:
				excludedFlagsResult.Add(AnimModifier.Kneeling);
				excludedFlagsResult.Add(AnimModifier.Sitting);
				break;
			case AnimModifier.Sitting:
				excludedFlagsResult.Add(AnimModifier.Kneeling);
				excludedFlagsResult.Add(AnimModifier.Lying);
				break;
			case AnimModifier.Kneeling:
				excludedFlagsResult.Add(AnimModifier.Lying);
				excludedFlagsResult.Add(AnimModifier.Sitting);
				break;
			}
		}
		else
		{
			excludedFlagsResult.Add(AnimModifier.Lying);
			excludedFlagsResult.Add(AnimModifier.Sitting);
			excludedFlagsResult.Add(AnimModifier.Kneeling);
		}
	}
}
