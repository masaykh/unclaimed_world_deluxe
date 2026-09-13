using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Xclna.Xna.Animation;

public sealed class Util
{
	public const long TICKS_PER_60FPS = 166666L;

	public const long TICKS_PER_30FPS = 333333L;

	public const long TICKS_PER_24FPS = 416666L;

	public const long SampleDistance = 333333L;

	private static Quaternion qStart;

	private static Quaternion qEnd;

	private static Quaternion qResult;

	private static Vector3 curTrans;

	private static Vector3 nextTrans;

	private static Vector3 lerpedTrans;

	private static Vector3 curScale;

	private static Vector3 nextScale;

	private static Vector3 lerpedScale;

	private static Matrix startRotation;

	private static Matrix endRotation;

	private static Matrix returnMatrix;

	public static SkinningType GetSkinningType(VertexElement[] elements)
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < elements.Length; i++)
		{
			VertexElement vertexElement = elements[i];
			if (vertexElement.VertexElementUsage == VertexElementUsage.BlendIndices)
			{
				num++;
			}
			else if (vertexElement.VertexElementUsage == VertexElementUsage.BlendWeight)
			{
				num2++;
			}
		}
		if (num == 3 || num2 == 3)
		{
			return SkinningType.TwelveBonesPerVertex;
		}
		if (num == 2 || num2 == 2)
		{
			return SkinningType.EightBonesPerVertex;
		}
		if (num == 1 || num2 == 1)
		{
			return SkinningType.FourBonesPerVertex;
		}
		return SkinningType.None;
	}

	public static void ReflectMatrix(ref Matrix m)
	{
		m.M13 *= -1f;
		m.M23 *= -1f;
		m.M33 *= -1f;
		m.M43 *= -1f;
		m.M31 *= -1f;
		m.M32 *= -1f;
		m.M33 *= -1f;
		m.M34 *= -1f;
	}

	private static T Max<T>(params T[] items) where T : IComparable
	{
		IComparable comparable = null;
		foreach (IComparable comparable2 in items)
		{
			if (comparable == null)
			{
				comparable = comparable2;
			}
			else if (comparable2.CompareTo(comparable) > 0)
			{
				comparable = comparable2;
			}
		}
		return (T)comparable;
	}

	public static T[] Convert<T>(byte[] data, int vertexSize, GraphicsDevice device) where T : struct
	{
		T[] array = new T[data.Length / vertexSize];
		VertexDeclaration vertexDeclaration = new VertexDeclaration(array.Length);
		using VertexBuffer vertexBuffer = new VertexBuffer(device, vertexDeclaration, data.Length, BufferUsage.None);
		vertexBuffer.SetData(data);
		vertexBuffer.GetData(array);
		return array;
	}

	public static Matrix SlerpMatrix(Matrix start, Matrix end, float slerpAmount)
	{
		if (start == end)
		{
			return start;
		}
		Quaternion.CreateFromRotationMatrix(ref start, out qStart);
		Quaternion.CreateFromRotationMatrix(ref end, out qEnd);
		Quaternion.Lerp(ref qStart, ref qEnd, slerpAmount, out qResult);
		curTrans.X = start.M41;
		curTrans.Y = start.M42;
		curTrans.Z = start.M43;
		nextTrans.X = end.M41;
		nextTrans.Y = end.M42;
		nextTrans.Z = end.M43;
		Vector3.Lerp(ref curTrans, ref nextTrans, slerpAmount, out lerpedTrans);
		Matrix.CreateFromQuaternion(ref qStart, out startRotation);
		Matrix.CreateFromQuaternion(ref qEnd, out endRotation);
		curScale.X = start.M11 - startRotation.M11;
		curScale.Y = start.M22 - startRotation.M22;
		curScale.Z = start.M33 - startRotation.M33;
		nextScale.X = end.M11 - endRotation.M11;
		nextScale.Y = end.M22 - endRotation.M22;
		nextScale.Z = end.M33 - endRotation.M33;
		Vector3.Lerp(ref curScale, ref nextScale, slerpAmount, out lerpedScale);
		Matrix.CreateFromQuaternion(ref qResult, out returnMatrix);
		returnMatrix.M41 = lerpedTrans.X;
		returnMatrix.M42 = lerpedTrans.Y;
		returnMatrix.M43 = lerpedTrans.Z;
		returnMatrix.M11 += lerpedScale.X;
		returnMatrix.M22 += lerpedScale.Y;
		returnMatrix.M33 += lerpedScale.Z;
		return returnMatrix;
	}

	public static void SlerpMatrix(ref Matrix start, ref Matrix end, float slerpAmount, out Matrix result)
	{
		start.Decompose(out var scale, out var rotation, out var translation);
		end.Decompose(out var scale2, out var rotation2, out var translation2);
		Quaternion.Lerp(ref rotation, ref rotation2, slerpAmount, out var result2);
		Vector3.Lerp(ref translation, ref translation2, slerpAmount, out var result3);
		Vector3.Lerp(ref scale, ref scale2, slerpAmount, out var result4);
		result = Matrix.CreateScale(result4) * Matrix.CreateFromQuaternion(result2) * Matrix.CreateTranslation(result3);
	}

	public static void SlerpDecomposedMatrixValues(Quaternion currentRotation, Quaternion nextRotation, Vector3 currentTranslation, Vector3 nextTranslation, Vector3 currentScaling, Vector3 nextScaling, float slerpAmount, out Matrix result)
	{
		Quaternion.Lerp(ref currentRotation, ref nextRotation, slerpAmount, out var result2);
		Vector3.Lerp(ref currentTranslation, ref nextTranslation, slerpAmount, out var result3);
		Vector3.Lerp(ref currentScaling, ref nextScaling, slerpAmount, out var result4);
		result = Matrix.CreateScale(result4) * Matrix.CreateFromQuaternion(result2) * Matrix.CreateTranslation(result3);
	}

	public static bool IsSkinned(ModelMeshPart meshPart)
	{
		VertexElement[] vertexElements = meshPart.VertexBuffer.VertexDeclaration.GetVertexElements();
		for (int i = 0; i < vertexElements.Length; i++)
		{
			VertexElement vertexElement = vertexElements[i];
			if (vertexElement.VertexElementUsage == VertexElementUsage.BlendIndices && vertexElement.UsageIndex == 0)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsSkinned(ModelMesh mesh)
	{
		foreach (ModelMeshPart meshPart in mesh.MeshParts)
		{
			if (IsSkinned(meshPart))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsSkinned(Model model)
	{
		foreach (ModelMesh mesh in model.Meshes)
		{
			if (IsSkinned(mesh))
			{
				return true;
			}
		}
		return false;
	}
}
