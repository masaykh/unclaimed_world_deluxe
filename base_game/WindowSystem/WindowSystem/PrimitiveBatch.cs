using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowSystem;

public class PrimitiveBatch : IDisposable
{
	private const int DefaultBufferSize = 500;

	private VertexPositionColor[] vertices = new VertexPositionColor[500];

	private int[] indexData = new int[500];

	private int positionInBuffer;

	private BasicEffect basicEffect;

	private GraphicsDevice device;

	private PrimitiveType primitiveType;

	private int numVertsPerPrimitive;

	private bool hasBegun;

	private bool isDisposed;

	public Matrix Projection;

	private int drawAreaWidth;

	private int drawAreaHeight;

	public Matrix View => basicEffect.View;

	public PrimitiveBatch(int drawAreaWidth, int drawAreaHeight, GraphicsDevice graphicsDevice, int? renderTargetWidth = null, int? renderTargetHeight = null)
	{
		if (graphicsDevice == null)
		{
			throw new ArgumentNullException("graphicsDevice");
		}
		device = graphicsDevice;
		basicEffect = new BasicEffect(graphicsDevice);
		basicEffect.VertexColorEnabled = true;
		basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0f, renderTargetWidth ?? drawAreaWidth, renderTargetHeight ?? drawAreaHeight, 0f, 0f, 1f);
		Projection = basicEffect.Projection;
		for (int i = 0; i < 500; i++)
		{
			indexData[i] = i;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing && !isDisposed)
		{
			if (basicEffect != null)
			{
				basicEffect.Dispose();
			}
			isDisposed = true;
		}
	}

	public void Begin(PrimitiveType primitiveType)
	{
		if (hasBegun)
		{
			throw new InvalidOperationException("End must be called before Begin can be called again.");
		}
		if (primitiveType == PrimitiveType.TriangleStrip)
		{
			throw new NotSupportedException("The specified primitiveType is not supported by PrimitiveBatch.");
		}
		this.primitiveType = primitiveType;
		numVertsPerPrimitive = NumVertsPerPrimitive(primitiveType);
		basicEffect.CurrentTechnique.Passes[0].Apply();
		hasBegun = true;
	}

	public void AddVertex(Vector2 vertex, Color color)
	{
		if (!hasBegun)
		{
			throw new InvalidOperationException("Begin must be called before AddVertex can be called.");
		}
		if (positionInBuffer % numVertsPerPrimitive == 0 && positionInBuffer + numVertsPerPrimitive >= vertices.Length)
		{
			Flush();
		}
		vertices[positionInBuffer].Position = new Vector3(vertex, 0f);
		vertices[positionInBuffer].Color = color;
		positionInBuffer++;
	}

	public void End()
	{
		if (!hasBegun)
		{
			throw new InvalidOperationException("Begin must be called before End can be called.");
		}
		Flush();
		hasBegun = false;
	}

	private void Flush()
	{
		if (!hasBegun)
		{
			throw new InvalidOperationException("Begin must be called before Flush can be called.");
		}
		if (positionInBuffer == 0)
		{
			return;
		}
		int num = positionInBuffer / numVertsPerPrimitive;
		if (primitiveType == PrimitiveType.LineStrip)
		{
			num--;
			if (num > 0)
			{
				device.DrawUserIndexedPrimitives(primitiveType, vertices, 0, num + 1, indexData, 0, num);
			}
		}
		else
		{
			device.DrawUserPrimitives(primitiveType, vertices, 0, num);
		}
		positionInBuffer = 0;
	}

	private static int NumVertsPerPrimitive(PrimitiveType primitive)
	{
		return primitive switch
		{
			PrimitiveType.LineList => 2, 
			PrimitiveType.TriangleList => 3, 
			PrimitiveType.LineStrip => 1, 
			_ => throw new InvalidOperationException("primitive is not valid"), 
		};
	}
}
