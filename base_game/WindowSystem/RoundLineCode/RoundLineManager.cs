using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace RoundLineCode;

public class RoundLineManager
{
	private GraphicsDevice device;

	private Effect effect;

	private EffectParameter viewProjMatrixParameter;

	private EffectParameter instanceDataParameter;

	private EffectParameter timeParameter;

	private EffectParameter lineRadiusParameter;

	private EffectParameter lineColorParameter;

	private EffectParameter blurThresholdParameter;

	private VertexBuffer vb;

	private IndexBuffer ib;

	private int numInstances;

	private int numVertices;

	private int numIndices;

	private int numPrimitivesPerInstance;

	private int numPrimitives;

	private Vector4[] translationData;

	public int NumLinesDrawn;

	public float BlurThreshold = 0.97f;

	public string[] TechniqueNames
	{
		get
		{
			string[] array = new string[effect.Techniques.Count];
			int num = 0;
			foreach (EffectTechnique technique in effect.Techniques)
			{
				array[num++] = technique.Name;
			}
			return array;
		}
	}

	public void LoadContent(GraphicsDevice device, ContentManager content)
	{
		this.device = device;
		effect = content.Load<Effect>("RoundLine");
		viewProjMatrixParameter = effect.Parameters["viewProj"];
		instanceDataParameter = effect.Parameters["instanceData"];
		timeParameter = effect.Parameters["time"];
		lineRadiusParameter = effect.Parameters["lineRadius"];
		lineColorParameter = effect.Parameters["lineColor"];
		blurThresholdParameter = effect.Parameters["blurThreshold"];
		CreateRoundLineMesh();
	}

	private void CreateRoundLineMesh()
	{
		numInstances = 200;
		numVertices = 60 * numInstances;
		numPrimitivesPerInstance = 28;
		numPrimitives = numPrimitivesPerInstance * numInstances;
		numIndices = 3 * numPrimitives;
		short[] array = new short[numIndices];
		RoundLineVertex[] array2 = new RoundLineVertex[numVertices];
		translationData = new Vector4[numInstances];
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < numInstances; i++)
		{
			int num3 = num;
			array2[num++] = new RoundLineVertex(new Vector3(0f, -1f, 0f), new Vector2(1f, 4.712389f), new Vector2(0f, 0f), i);
			array2[num++] = new RoundLineVertex(new Vector3(0f, -1f, 0f), new Vector2(1f, 4.712389f), new Vector2(0f, 1f), i);
			array2[num++] = new RoundLineVertex(new Vector3(0f, 0f, 0f), new Vector2(0f, 4.712389f), new Vector2(0f, 1f), i);
			array2[num++] = new RoundLineVertex(new Vector3(0f, 0f, 0f), new Vector2(0f, 4.712389f), new Vector2(0f, 0f), i);
			array2[num++] = new RoundLineVertex(new Vector3(0f, 0f, 0f), new Vector2(0f, (float)Math.PI / 2f), new Vector2(0f, 1f), i);
			array2[num++] = new RoundLineVertex(new Vector3(0f, 0f, 0f), new Vector2(0f, (float)Math.PI / 2f), new Vector2(0f, 0f), i);
			array2[num++] = new RoundLineVertex(new Vector3(0f, 1f, 0f), new Vector2(1f, (float)Math.PI / 2f), new Vector2(0f, 1f), i);
			array2[num++] = new RoundLineVertex(new Vector3(0f, 1f, 0f), new Vector2(1f, (float)Math.PI / 2f), new Vector2(0f, 0f), i);
			array[num2++] = (short)num3;
			array[num2++] = (short)(num3 + 1);
			array[num2++] = (short)(num3 + 2);
			array[num2++] = (short)(num3 + 2);
			array[num2++] = (short)(num3 + 3);
			array[num2++] = (short)num3;
			array[num2++] = (short)(num3 + 4);
			array[num2++] = (short)(num3 + 6);
			array[num2++] = (short)(num3 + 5);
			array[num2++] = (short)(num3 + 6);
			array[num2++] = (short)(num3 + 7);
			array[num2++] = (short)(num3 + 5);
			num3 = num;
			int num4 = num2;
			for (int j = 0; j < 13; j++)
			{
				float num5 = (float)Math.PI / 12f;
				float num6 = (float)Math.PI / 2f + (float)j * num5;
				float y = num6 + num5 / 2f;
				array2[num3] = new RoundLineVertex(new Vector3(0f, 0f, 0f), new Vector2(0f, y), new Vector2(0f, 0f), i);
				float x = (float)Math.Cos(num6);
				float y2 = (float)Math.Sin(num6);
				array2[num3 + 1] = new RoundLineVertex(new Vector3(x, y2, 0f), new Vector2(1f, num6), new Vector2(1f, 0f), i);
				if (j < 12)
				{
					array[num4] = (short)num3;
					array[num4 + 1] = (short)(num3 + 1);
					array[num4 + 2] = (short)(num3 + 3);
					num4 += 3;
					num2 += 3;
				}
				num3 += 2;
				num += 2;
			}
			for (int k = 0; k < 13; k++)
			{
				float num7 = (float)Math.PI / 12f;
				float num8 = 4.712389f + (float)k * num7;
				float y3 = num8 + num7 / 2f;
				array2[num3] = new RoundLineVertex(new Vector3(0f, 0f, 0f), new Vector2(0f, y3), new Vector2(0f, 1f), i);
				float x2 = (float)Math.Cos(num8);
				float y4 = (float)Math.Sin(num8);
				array2[num3 + 1] = new RoundLineVertex(new Vector3(x2, y4, 0f), new Vector2(1f, num8), new Vector2(1f, 1f), i);
				if (k < 12)
				{
					array[num4] = (short)num3;
					array[num4 + 1] = (short)(num3 + 1);
					array[num4 + 2] = (short)(num3 + 3);
					num4 += 3;
					num2 += 3;
				}
				num3 += 2;
				num += 2;
			}
		}
		vb = new VertexBuffer(device, typeof(RoundLineVertex), numVertices, BufferUsage.None);
		vb.SetData(array2);
		ib = new IndexBuffer(device, IndexElementSize.SixteenBits, numIndices, BufferUsage.None);
		ib.SetData(array);
	}

	public float ComputeBlurThreshold(float lineRadius, Matrix viewProjMatrix, float viewportWidth)
	{
		Vector4 vector = new Vector4(0f, 0f, 0f, 1f);
		Vector4 vector2 = Vector4.Transform(new Vector4(lineRadius, 0f, 0f, 1f) - vector, viewProjMatrix);
		vector2.X *= viewportWidth;
		return MathHelper.Clamp((float)(0.125 * Math.Log(vector2.X) + 0.4), 0.5f, 0.99f);
	}

	public void Draw(RoundLine roundLine, float lineRadius, Color lineColor, Matrix viewProjMatrix, float time, string techniqueName)
	{
		device.SetVertexBuffer(vb);
		device.Indices = ib;
		viewProjMatrixParameter.SetValue(viewProjMatrix);
		timeParameter.SetValue(time);
		lineColorParameter.SetValue(lineColor.ToVector4());
		lineRadiusParameter.SetValue(lineRadius);
		blurThresholdParameter.SetValue(BlurThreshold);
		int iData = 0;
		PopulateTranslationData(ref iData, roundLine);
		instanceDataParameter.SetValue(translationData);
		if (techniqueName == null)
		{
			effect.CurrentTechnique = effect.Techniques[0];
		}
		else
		{
			effect.CurrentTechnique = effect.Techniques[techniqueName];
		}
		effect.CurrentTechnique.Passes[0].Apply();
		int num = 1;
		device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, numVertices, 0, numPrimitivesPerInstance * num);
		NumLinesDrawn += num;
	}

	public void Draw(List<RoundLine> roundLines, float lineRadius, Color lineColor, Matrix viewProjMatrix, float time, string techniqueName)
	{
		device.SetVertexBuffer(vb);
		device.Indices = ib;
		viewProjMatrixParameter.SetValue(viewProjMatrix);
		timeParameter.SetValue(time);
		lineColorParameter.SetValue(lineColor.ToVector4());
		lineRadiusParameter.SetValue(lineRadius);
		blurThresholdParameter.SetValue(BlurThreshold);
		if (techniqueName == null)
		{
			effect.CurrentTechnique = effect.Techniques[0];
		}
		else
		{
			effect.CurrentTechnique = effect.Techniques[techniqueName];
		}
		EffectPass effectPass = effect.CurrentTechnique.Passes[0];
		effectPass.Apply();
		int iData = 0;
		int num = 0;
		foreach (RoundLine roundLine in roundLines)
		{
			PopulateTranslationData(ref iData, roundLine);
			num++;
			if (num == numInstances)
			{
				instanceDataParameter.SetValue(translationData);
				effectPass.Apply();
				device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, numVertices, 0, numPrimitivesPerInstance * num);
				NumLinesDrawn += num;
				num = 0;
				iData = 0;
			}
		}
		if (num > 0)
		{
			instanceDataParameter.SetValue(translationData);
			effectPass.Apply();
			device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, numVertices, 0, numPrimitivesPerInstance * num);
			NumLinesDrawn += num;
		}
	}

	private void PopulateTranslationData(ref int iData, RoundLine roundLine)
	{
		translationData[iData++] = new Vector4(roundLine.P0.X, roundLine.P0.Y, roundLine.Rho, roundLine.Theta);
	}
}
