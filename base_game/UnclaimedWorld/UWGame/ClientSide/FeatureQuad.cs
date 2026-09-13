using Microsoft.Xna.Framework;

namespace UWGame.ClientSide;

public class FeatureQuad : QuadBase
{
	protected VertexFeatureQuad[] quad;

	private Vector4 Tint = Vector4.One;

	private float Bendyness;

	private float RandomValue;

	public float WidthHeightRatio = 1f;

	public void SetProperties(float bendyness, float randomValue, Vector4 tint)
	{
		Bendyness = bendyness;
		RandomValue = randomValue;
		Tint = tint;
		quad[0].Bendyness = Bendyness;
		quad[1].Bendyness = Bendyness;
		quad[2].Bendyness = 0f;
		quad[3].Bendyness = 0f;
		for (int i = 0; i < quad.Length; i++)
		{
			VertexFeatureQuad vertexFeatureQuad = quad[i];
			vertexFeatureQuad.Random = RandomValue;
			vertexFeatureQuad.Tint = Tint;
			quad[i] = vertexFeatureQuad;
		}
	}

	public void SetTint(Vector4 tint)
	{
		Tint = tint;
		for (int i = 0; i < quad.Length; i++)
		{
			VertexFeatureQuad vertexFeatureQuad = quad[i];
			vertexFeatureQuad.Tint = Tint;
			quad[i] = vertexFeatureQuad;
		}
	}

	public bool CopyQuadToVertexBuffer(VertexFeatureQuad[] featureVertices, int index)
	{
		index *= 4;
		if (index + 4 >= featureVertices.Length)
		{
			return false;
		}
		for (int i = index; i < index + 4; i++)
		{
			featureVertices[i] = quad[i - index];
		}
		return true;
	}

	public void SetupQuadVertices(Vector3 worldPosition, float posLeft, float posTop, float posRight, float posBottom, bool isFlipped)
	{
		float z = 0f;
		quad = new VertexFeatureQuad[4];
		float num = (float)base.StaticSourceRectangle.X / (float)base.Texture.Width;
		float num2 = (float)base.StaticSourceRectangle.Y / (float)base.Texture.Height;
		float textureRight = num + (float)base.StaticSourceRectangle.Width / (float)base.Texture.Width;
		float textureBottom = num2 + (float)base.StaticSourceRectangle.Height / (float)base.Texture.Height;
		worldPosition = SetupFourCornerVertices(worldPosition, posLeft, posTop, posRight, posBottom, z, num, num2, textureRight, textureBottom, 1f, isFlipped);
	}

	public void SetupQuadVerticesFromBaseCenter(Vector3 worldPosition, float posLeft, float posTop, float posRight, float posBottom, Vector2 baseCenter, bool isFlipped)
	{
		float z = 0f;
		quad = new VertexFeatureQuad[4];
		float num = (float)base.StaticSourceRectangle.X / (float)base.Texture.Width;
		float num2 = (float)base.StaticSourceRectangle.Y / (float)base.Texture.Height;
		float textureRight = num + (float)base.StaticSourceRectangle.Width / (float)base.Texture.Width;
		float textureBottom = num2 + baseCenter.Y / (float)base.Texture.Height;
		worldPosition = SetupFourCornerVertices(worldPosition, posLeft, posTop, posRight, posBottom, z, num, num2, textureRight, textureBottom, 0.75f, isFlipped);
	}

	private Vector3 SetupFourCornerVertices(Vector3 worldPosition, float posLeft, float posTop, float posRight, float posBottom, float z, float textureLeft, float textureTop, float textureRight, float textureBottom, float squashBottomFactor, bool flipSprite)
	{
		bool flag = flipSprite;
		quad[0] = default(VertexFeatureQuad);
		quad[0].Position = new Vector3(posLeft, posTop, z);
		quad[0].WorldPosition = worldPosition;
		quad[0].TextureCoordinate = (flipSprite ? new Vector2(textureRight, textureTop) : new Vector2(textureLeft, textureTop));
		quad[0].NormalTextureCoordinate = (flag ? new Vector2(textureRight, textureTop) : new Vector2(textureLeft, textureTop));
		quad[0].Bendyness = Bendyness;
		quad[1] = default(VertexFeatureQuad);
		quad[1].Position = new Vector3(posRight, posTop, z);
		quad[1].WorldPosition = worldPosition;
		quad[1].TextureCoordinate = (flipSprite ? new Vector2(textureLeft, textureTop) : new Vector2(textureRight, textureTop));
		quad[1].NormalTextureCoordinate = (flag ? new Vector2(textureLeft, textureTop) : new Vector2(textureRight, textureTop));
		quad[1].Bendyness = Bendyness;
		quad[2] = default(VertexFeatureQuad);
		quad[2].Position = new Vector3(posRight * squashBottomFactor, posBottom, z);
		quad[2].WorldPosition = worldPosition;
		quad[2].TextureCoordinate = (flipSprite ? new Vector2(textureLeft, textureBottom) : new Vector2(textureRight, textureBottom));
		quad[2].NormalTextureCoordinate = (flag ? new Vector2(textureLeft, textureBottom) : new Vector2(textureRight, textureBottom));
		quad[2].Bendyness = 0f;
		quad[3] = default(VertexFeatureQuad);
		quad[3].Position = new Vector3(posLeft * squashBottomFactor, posBottom, z);
		quad[3].WorldPosition = worldPosition;
		quad[3].TextureCoordinate = (flipSprite ? new Vector2(textureRight, textureBottom) : new Vector2(textureLeft, textureBottom));
		quad[3].NormalTextureCoordinate = (flag ? new Vector2(textureRight, textureBottom) : new Vector2(textureLeft, textureBottom));
		quad[3].Bendyness = 0f;
		for (int i = 0; i < quad.Length; i++)
		{
			quad[i].Random = RandomValue;
			quad[i].FlipNormals = (flag ? 1f : 0f);
			quad[i].WidthHeightRatio = WidthHeightRatio;
			quad[i].Tint = Tint;
		}
		return worldPosition;
	}
}
