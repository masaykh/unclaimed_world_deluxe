using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Xclna.Xna.Animation;

public class ModelAnimator
{
	public delegate void IterateMethod(IAttachable attachable);

	private Matrix world = Matrix.Identity;

	private readonly Model model;

	private readonly EffectParameter[] worldParams;

	private readonly EffectParameter[] matrixPaletteParams;

	private Effect[] modelEffects;

	private ReadOnlyCollection<Effect> effectCollection;

	private BonePoseCollection bonePoses;

	private AnimationInfoCollection animations;

	private Dictionary<string, List<IAttachable>> attachedObjects = new Dictionary<string, List<IAttachable>>();

	private readonly int numMeshes;

	private readonly int numEffects;

	private static Matrix skinTransform;

	private Matrix[] pose;

	private Matrix[][] palette;

	private SkinInfoCollection[] skinInfo;

	private Dictionary<string, bool> MeshesToHide;

	public Dictionary<string, AnimationController> AnimationControllers = new Dictionary<string, AnimationController>();

	public Matrix World
	{
		get
		{
			return world;
		}
		set
		{
			world = value;
		}
	}

	public Matrix UncorrectedWorld { get; set; }

	protected int EffectCount => numEffects;

	public Model Model => model;

	public AnimationInfoCollection Animations => animations;

	public ReadOnlyCollection<Effect> Effects => effectCollection;

	public Dictionary<string, List<IAttachable>> AttachedObjects => attachedObjects;

	public BonePoseCollection BonePoses => bonePoses;

	public ModelAnimator(Model model)
	{
		this.model = model;
		animations = AnimationInfoCollection.FromModel(model);
		bonePoses = BonePoseCollection.FromModelBoneCollection(model.Bones);
		numMeshes = model.Meshes.Count;
		numEffects = 0;
		foreach (ModelMesh mesh in model.Meshes)
		{
			foreach (Effect effect in mesh.Effects)
			{
				_ = effect;
				numEffects++;
			}
		}
		modelEffects = new Effect[numEffects];
		worldParams = new EffectParameter[numEffects];
		matrixPaletteParams = new EffectParameter[numEffects];
		InitializeEffectParams();
		pose = new Matrix[model.Bones.Count];
		model.CopyAbsoluteBoneTransformsTo(pose);
		Dictionary<string, object> dictionary = (Dictionary<string, object>)model.Tag;
		if (dictionary == null)
		{
			throw new Exception("Model Processor must subclass AnimatedModelProcessor.");
		}
		skinInfo = (SkinInfoCollection[])dictionary["SkinInfo"];
		if (skinInfo == null)
		{
			throw new Exception("Model processor must pass skinning info through the tag.");
		}
		palette = new Matrix[model.Meshes.Count][];
		for (int i = 0; i < skinInfo.Length; i++)
		{
			if (Util.IsSkinned(model.Meshes[i]))
			{
				palette[i] = new Matrix[skinInfo[i].Count];
			}
			else
			{
				palette[i] = null;
			}
		}
		for (int j = 0; j < model.Meshes.Count; j++)
		{
			if (palette[j] != null && matrixPaletteParams[j] != null)
			{
				Matrix[] value = palette[j];
				try
				{
					matrixPaletteParams[j].SetValue(value);
				}
				catch
				{
					throw new Exception("Model has too many skinned bones for the matrix palette.");
				}
			}
		}
		foreach (KeyValuePair<string, AnimationInfo> animation in Animations)
		{
			AnimationController value2 = new InterpolationController(animation.Value)
			{
				SpeedFactor = 0.0,
				IsLooping = false
			};
			AnimationControllers.Add(animation.Key, value2);
		}
	}

	public ModelAnimator(ModelAnimator original, bool canAnimate)
	{
		model = original.model;
		numMeshes = original.numMeshes;
		numEffects = original.numEffects;
		pose = new Matrix[original.pose.Length];
		Array.Copy(original.pose, pose, original.pose.Length);
		bonePoses = BonePoseCollection.FromModelBoneCollection(model.Bones);
		if (original.palette[0] != null)
		{
			InitJaggedArray(ref palette, original.palette.Length, original.palette[0].Length);
			CopyJaggedArray(original.palette, palette);
		}
		else
		{
			palette = new Matrix[original.palette.Length][];
		}
		if (original.MeshesToHide != null)
		{
			MeshesToHide = original.MeshesToHide.ToDictionary((KeyValuePair<string, bool> k) => k.Key, (KeyValuePair<string, bool> k) => k.Value);
		}
		skinInfo = original.skinInfo;
		modelEffects = new Effect[original.modelEffects.Length];
		Array.Copy(original.modelEffects, modelEffects, original.modelEffects.Length);
		matrixPaletteParams = new EffectParameter[original.matrixPaletteParams.Length];
		Array.Copy(original.matrixPaletteParams, matrixPaletteParams, original.matrixPaletteParams.Length);
		worldParams = new EffectParameter[original.worldParams.Length];
		Array.Copy(original.worldParams, worldParams, original.worldParams.Length);
		if (canAnimate)
		{
			animations = original.animations;
			AnimationControllers = original.AnimationControllers;
		}
	}

	public AnimationController GetAnimControllerFromKey(string animKey)
	{
		if (!AnimationControllers.TryGetValue(animKey, out var value))
		{
			throw new Exception("Animation " + animKey + " not found... check MergeAnimations property and spelling. Also check CreatureLoader  reference");
		}
		return value;
	}

	private static void InitJaggedArray<T>(ref T[][] map, int width, int height)
	{
		map = new T[width][];
		for (int i = 0; i < width; i++)
		{
			map[i] = new T[height];
		}
	}

	private static void CopyJaggedArray<T>(T[][] fromArray, T[][] toArray)
	{
		Parallel.For(0, fromArray.Length, delegate(int x)
		{
			T[] array = fromArray[x];
			T[] destinationArray = toArray[x];
			Array.Copy(array, destinationArray, array.Length);
		});
	}

	public SkinInfoCollection GetMeshSkinInfo(int index)
	{
		return skinInfo[index];
	}

	public void ShowMesh(string name, bool show)
	{
		if (show)
		{
			if (MeshesToHide != null && MeshesToHide.Count > 0 && MeshesToHide.ContainsKey(name))
			{
				MeshesToHide.Remove(name);
			}
			return;
		}
		if (MeshesToHide == null)
		{
			MeshesToHide = new Dictionary<string, bool>();
		}
		if (!MeshesToHide.ContainsKey(name))
		{
			MeshesToHide.Add(name, value: true);
		}
	}

	protected virtual IList<Effect> CreateEffectList()
	{
		List<Effect> list = new List<Effect>();
		foreach (ModelMesh mesh in model.Meshes)
		{
			foreach (ModelMeshPart meshPart in mesh.MeshParts)
			{
				list.Add(meshPart.Effect);
			}
		}
		return list;
	}

	public void InitializeEffectParams()
	{
		IList<Effect> list = CreateEffectList();
		if (list.Count != numEffects)
		{
			throw new Exception("The number of effects in the list returned by CreateEffectList must be equal to the number of ModelMeshParts.");
		}
		list.CopyTo(modelEffects, 0);
		effectCollection = new ReadOnlyCollection<Effect>(modelEffects);
		for (int i = 0; i < numEffects; i++)
		{
			worldParams[i] = modelEffects[i].Parameters["World"];
			matrixPaletteParams[i] = modelEffects[i].Parameters["MatrixPalette"];
		}
	}

	public void Update(GameTime gameTime)
	{
		CopyAbsoluteTransforms();
	}

	public void CopyAbsoluteTransforms()
	{
		bonePoses.CopyAbsoluteTransformsTo(pose);
		for (int i = 0; i < skinInfo.Length; i++)
		{
			if (palette[i] == null)
			{
				continue;
			}
			foreach (SkinInfo item in skinInfo[i])
			{
				skinTransform = item.InverseBindPoseTransform;
				Matrix.Multiply(ref skinTransform, ref pose[item.BoneIndex], out palette[i][item.PaletteIndex]);
			}
		}
	}

	public void UpdateModelBones(AnimationTrack track)
	{
		foreach (BonePose bonePose in BonePoses)
		{
			bonePose.UpdateAnimControllerStatus(track);
		}
	}

	public void IterateAttachedEntities(IterateMethod iterateMethod)
	{
		foreach (KeyValuePair<string, List<IAttachable>> attachedObject in attachedObjects)
		{
			foreach (IAttachable item in attachedObject.Value)
			{
				iterateMethod(item);
			}
		}
	}

	public void ComputeTransformsForAttachedObjects(float scale)
	{
		if (attachedObjects.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<string, List<IAttachable>> attachedObject in attachedObjects)
		{
			foreach (IAttachable item in attachedObject.Value)
			{
				Model model = item.ModelAnimator.model;
				Matrix[] array = new Matrix[model.Bones.Count];
				model.CopyAbsoluteBoneTransformsTo(array);
				if (item.AttacheeBone != null)
				{
					item.CombinedTransform = item.LocalTransform * Matrix.Invert(array[model.Meshes[0].ParentBone.Index]) * array[item.AttacheeBone.Index] * Matrix.Invert(pose[this.model.Root.Index]) * pose[item.AttachorBone.Index] * world;
				}
			}
		}
	}

	public void AttachObject(IAttachable attachable, AttachPoint attachorPoint, AttachPoint attachee, AttacheePoint? attacheePoint, BonePose attacheeBone, object attachorEntity)
	{
		BonePose bonePose = bonePoses[attachorPoint.BoneName];
		if (!attachedObjects.TryGetValue(bonePose.Name, out var value))
		{
			value = new List<IAttachable>();
			attachedObjects.Add(bonePose.Name, value);
		}
		value.Add(attachable);
		attachable.AttachedTo = attachorEntity;
		attachable.AttachorPoint = attachorPoint;
		attachable.AttachorBone = bonePose;
		attachable.AttacheeBone = attacheeBone;
		attachable.AttacheePointValue = attacheePoint;
	}

	public void DeattachObject(IAttachable attachable, string attachorBoneName)
	{
		if (attachedObjects.TryGetValue(attachorBoneName, out var value))
		{
			value.Remove(attachable);
			attachable.AttachedTo = null;
			attachable.AttacheePointValue = null;
			attachable.AttacheeBone = null;
			attachable.AttachorPoint = null;
			attachable.AttachorBone = null;
		}
	}

	public bool HasAttachedObject(string attachorBoneName)
	{
		if (attachedObjects.TryGetValue(attachorBoneName, out var value))
		{
			return value.Count > 0;
		}
		return false;
	}

	public Matrix GetAbsoluteTransform(int boneIndex)
	{
		return pose[boneIndex];
	}

	public void Draw(GameTime gameTime)
	{
		try
		{
			int num = 0;
			bool flag = MeshesToHide != null && MeshesToHide.Count > 0;
			for (int i = 0; i < numMeshes; i++)
			{
				ModelMesh modelMesh = model.Meshes[i];
				int num2 = num;
				if (matrixPaletteParams[num] != null)
				{
					foreach (Effect effect in modelMesh.Effects)
					{
						_ = effect;
						worldParams[num].SetValue(world);
						matrixPaletteParams[num].SetValue(palette[i]);
						num++;
					}
				}
				else
				{
					foreach (Effect effect2 in modelMesh.Effects)
					{
						_ = effect2;
						worldParams[num].SetValue(pose[modelMesh.ParentBone.Index] * world);
						num++;
					}
				}
				if (flag && MeshesToHide.ContainsKey(modelMesh.Name))
				{
					continue;
				}
				int count = modelMesh.MeshParts.Count;
				GraphicsDevice graphicsDevice = modelMesh.MeshParts[0].VertexBuffer.GraphicsDevice;
				graphicsDevice.Indices = modelMesh.MeshParts[0].IndexBuffer;
				for (int j = 0; j < count; j++)
				{
					ModelMeshPart modelMeshPart = modelMesh.MeshParts[j];
					if (!((string)modelMeshPart.Tag == "skip") && modelMeshPart.NumVertices != 0 && modelMeshPart.PrimitiveCount != 0)
					{
						Effect obj = modelEffects[num2 + j];
						graphicsDevice.SetVertexBuffer(modelMeshPart.VertexBuffer);
						EffectPassCollection passes = obj.CurrentTechnique.Passes;
						int count2 = passes.Count;
						for (int k = 0; k < count2; k++)
						{
							passes[k].Apply();
							graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, modelMeshPart.VertexOffset, 0, modelMeshPart.NumVertices, modelMeshPart.StartIndex, modelMeshPart.PrimitiveCount);
						}
					}
				}
			}
		}
		catch (NullReferenceException)
		{
			throw new InvalidOperationException("The effects on the model for a ModelAnimator were changed without calling ModelAnimator.InitializeEffectParams().");
		}
		catch (InvalidCastException)
		{
			throw new InvalidCastException("ModelAnimator has thrown an InvalidCastException.  This is likely because the model uses too many bones for the matrix palette.  The default palette size is 56 for windows and 40 for Xbox.");
		}
	}
}
