using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Kensei.Dev;

public static class Shape
{
	private static readonly float DefaultArrowHeadProportion = 0.3f;

	private static readonly float DefaultArrowHeadRadiusFactor = 1.5f;

	private static readonly int DefaultNumSegmentsLine = 12;

	private static readonly int DefaultNumSegmentsSolid = 12;

	private static Effect effect;

	private static List<VertexPositionColor> s_line2DVertices = new List<VertexPositionColor>();

	private static List<VertexPositionColor> s_triangle2DVertices = new List<VertexPositionColor>();

	private static List<VertexPositionColor> s_line3DVertices = new List<VertexPositionColor>();

	private static List<VertexPositionColor> s_triangle3DVertices = new List<VertexPositionColor>();

	private static VertexPositionColor[] s_triangle2DVerticesArray;

	private static int s_triangle2DVerticesArraySize;

	private static VertexPositionColor[] s_line2DVerticesArray;

	private static int s_line2DVerticesArraySize;

	private static VertexPositionColor[] s_triangle3DVerticesArray;

	private static int s_triangle3DVerticesArraySize;

	private static VertexPositionColor[] s_line3DVerticesArray;

	private static int s_line3DVerticesArraySize;

	private static int m_numSphereVertices = DefaultNumSegmentsLine * (DefaultNumSegmentsLine + 1);

	private static Vector3[] m_sphereVertices = new Vector3[m_numSphereVertices];

	private static int m_numCylinderSegments = DefaultNumSegmentsLine;

	private static Vector3[] m_cylinderBaseVertices = new Vector3[m_numCylinderSegments];

	private static Vector3[] m_cylinderTopVertices = new Vector3[m_numCylinderSegments];

	internal static void Initialise(ContentManager content, GraphicsDevice device)
	{
		effect = content.Load<Effect>("DevShape");
	}

	internal static void Draw(GraphicsDevice device, Matrix renderMatrix, float width, float height)
	{
		if (Options.GetOption("Overlays.ShowPrimitiveCount"))
		{
			DevText.Print("Drawing " + (s_line2DVertices.Count + s_line3DVertices.Count + s_triangle2DVertices.Count + s_triangle3DVertices.Count) + " Debug Primitives", Color.White);
		}
		device.RasterizerState = RasterizerState.CullNone;
		device.BlendState = BlendState.AlphaBlend;
		Render3DLines(device, renderMatrix);
		Render3DTriangles(device, renderMatrix);
		Render2DLines(device, width, height);
		Render2DTriangles(device, width, height);
	}

	public static void Line(Vector2 start, Vector2 end, Color startColour, Color endColour)
	{
		s_line2DVertices.Add(new VertexPositionColor(new Vector3(start, 0f), startColour));
		s_line2DVertices.Add(new VertexPositionColor(new Vector3(end, 0f), endColour));
	}

	public static void Line(Vector2 start, Vector2 end, Color colour)
	{
		Line(start, end, colour, colour);
	}

	public static void Box(Vector2 topLeft, Vector2 bottomRight, Color colour, bool solid)
	{
		Vector2 vector = new Vector2(topLeft.X, bottomRight.Y);
		Vector2 vector2 = new Vector2(bottomRight.X, topLeft.Y);
		if (solid)
		{
			s_triangle2DVertices.Add(new VertexPositionColor(new Vector3(vector, 0f), colour));
			s_triangle2DVertices.Add(new VertexPositionColor(new Vector3(vector2, 0f), colour));
			s_triangle2DVertices.Add(new VertexPositionColor(new Vector3(topLeft, 0f), colour));
			s_triangle2DVertices.Add(new VertexPositionColor(new Vector3(vector2, 0f), colour));
			s_triangle2DVertices.Add(new VertexPositionColor(new Vector3(vector, 0f), colour));
			s_triangle2DVertices.Add(new VertexPositionColor(new Vector3(bottomRight, 0f), colour));
		}
		else
		{
			Line(topLeft, vector2, colour);
			Line(vector2, bottomRight, colour);
			Line(bottomRight, vector, colour);
			Line(vector, topLeft, colour);
		}
	}

	public static void Box(Vector2 topLeft, Vector2 bottomRight, Color colour0, Color colour1, Color colour2, Color colour3, bool solid)
	{
		Vector2 vector = new Vector2(topLeft.X, bottomRight.Y);
		Vector2 vector2 = new Vector2(bottomRight.X, topLeft.Y);
		if (solid)
		{
			s_triangle2DVertices.Add(new VertexPositionColor(new Vector3(vector, 0f), colour2));
			s_triangle2DVertices.Add(new VertexPositionColor(new Vector3(vector2, 0f), colour1));
			s_triangle2DVertices.Add(new VertexPositionColor(new Vector3(topLeft, 0f), colour0));
			s_triangle2DVertices.Add(new VertexPositionColor(new Vector3(vector2, 0f), colour1));
			s_triangle2DVertices.Add(new VertexPositionColor(new Vector3(vector, 0f), colour2));
			s_triangle2DVertices.Add(new VertexPositionColor(new Vector3(bottomRight, 0f), colour3));
		}
		else
		{
			Line(topLeft, vector2, colour1);
			Line(vector2, bottomRight, colour3);
			Line(bottomRight, vector, colour2);
			Line(vector, topLeft, colour0);
		}
	}

	public static void Triangle(Vector2 point1, Color colour1, Vector2 point2, Color colour2, Vector2 point3, Color colour3)
	{
		s_triangle2DVertices.Add(new VertexPositionColor(new Vector3(point1, 0f), colour1));
		s_triangle2DVertices.Add(new VertexPositionColor(new Vector3(point2, 0f), colour2));
		s_triangle2DVertices.Add(new VertexPositionColor(new Vector3(point3, 0f), colour3));
	}

	public static void Triangle(Vector2 point1, Vector2 point2, Vector2 point3, Color colour)
	{
		Triangle(point1, colour, point2, colour, point3, colour);
	}

	public static void Circle(Vector2 center, float radius, Color color, bool dashed = false)
	{
		float num = (float)Math.PI / 10f * radius;
		float num2 = (float)Math.PI * 2f / num;
		if ((double)num2 > Math.PI / 6.0)
		{
			num2 = (float)Math.PI / 6f;
		}
		Vector2 vector = center;
		Vector2 vector2 = vector;
		Vector2? vector3 = null;
		for (float num3 = 0f; (double)num3 < Math.PI * 2.0; num3 += num2)
		{
			vector2.X = center.X + (float)Math.Sin(num3) * radius;
			vector2.Y = center.Y + (float)Math.Cos(num3) * radius;
			if (!vector3.HasValue)
			{
				vector3 = vector2;
			}
			else
			{
				if (dashed)
				{
					vector = (vector + vector2) * 0.5f;
				}
				Line(vector, vector2, color);
			}
			vector = vector2;
		}
		vector2 = vector3.Value;
		if (dashed)
		{
			vector = (vector + vector2) * 0.5f;
		}
		Line(vector, vector2, color);
	}

	public static void Line(Vector3 start, Vector3 end, Color startColour, Color endColour)
	{
		s_line3DVertices.Add(new VertexPositionColor(start, startColour));
		s_line3DVertices.Add(new VertexPositionColor(end, endColour));
	}

	public static void Line(Vector3 start, Vector3 end, Color colour)
	{
		Line(start, end, colour, colour);
	}

	public static void Line(Ray ray, Color colour)
	{
		Line(ray.Position, ray.Position + ray.Direction, colour);
	}

	public static void Triangle(Vector3 point1, Color colour1, Vector3 point2, Color colour2, Vector3 point3, Color colour3)
	{
		s_triangle3DVertices.Add(new VertexPositionColor(point1, colour1));
		s_triangle3DVertices.Add(new VertexPositionColor(point2, colour2));
		s_triangle3DVertices.Add(new VertexPositionColor(point3, colour3));
	}

	public static void Triangle(Vector3 point1, Vector3 point2, Vector3 point3, Color colour)
	{
		Triangle(point1, colour, point2, colour, point3, colour);
	}

	public static void Cross(Vector3 position, float size, Color colour)
	{
		Line(new Vector3(position.X - size, position.Y, position.Z), new Vector3(position.X + size, position.Y, position.Z), colour);
		Line(new Vector3(position.X, position.Y - size, position.Z), new Vector3(position.X, position.Y + size, position.Z), colour);
		Line(new Vector3(position.X, position.Y, position.Z - size), new Vector3(position.X, position.Y, position.Z + size), colour);
	}

	public static void Axes(Vector3 position, float size, Quaternion rotation, Color xColour, Color yColour, Color zColour)
	{
		Line(position, position + Vector3.Transform(Vector3.UnitX * size, rotation), xColour);
		Line(position, position + Vector3.Transform(Vector3.UnitY * size, rotation), yColour);
		Line(position, position + Vector3.Transform(Vector3.UnitZ * size, rotation), zColour);
	}

	public static void Axes(Vector3 position, float size, Quaternion rotation)
	{
		Axes(position, size, rotation, Color.Blue, Color.Red, Color.Green);
	}

	public static void Axes(Vector3 position, float size)
	{
		Axes(position, size, Quaternion.Identity);
	}

	public static void Box(Vector3 min, Vector3 max, Color colour, bool solid)
	{
		Vector3 vector = new Vector3(min.X, min.Y, max.Z);
		Vector3 vector2 = new Vector3(min.X, max.Y, min.Z);
		Vector3 vector3 = new Vector3(max.X, min.Y, min.Z);
		Vector3 vector4 = new Vector3(min.X, max.Y, max.Z);
		Vector3 vector5 = new Vector3(max.X, min.Y, max.Z);
		Vector3 vector6 = new Vector3(max.X, max.Y, min.Z);
		if (solid)
		{
			Triangle(min, vector5, vector, colour);
			Triangle(min, vector5, vector3, colour);
			Triangle(min, vector4, vector, colour);
			Triangle(min, vector4, vector2, colour);
			Triangle(min, vector6, vector3, colour);
			Triangle(min, vector6, vector2, colour);
			Triangle(max, vector3, vector6, colour);
			Triangle(max, vector3, vector5, colour);
			Triangle(max, vector, vector4, colour);
			Triangle(max, vector, vector5, colour);
			Triangle(max, vector2, vector4, colour);
			Triangle(max, vector2, vector6, colour);
		}
		else
		{
			Line(min, vector, colour);
			Line(vector, vector5, colour);
			Line(vector5, vector3, colour);
			Line(vector3, min, colour);
			Line(min, vector2, colour);
			Line(vector, vector4, colour);
			Line(vector5, max, colour);
			Line(vector3, vector6, colour);
			Line(max, vector6, colour);
			Line(vector6, vector2, colour);
			Line(vector2, vector4, colour);
			Line(vector4, max, colour);
		}
	}

	public static void Box(Vector3 centre, Vector3 size, Quaternion rotation, Color colour, bool solid)
	{
		Vector3 vector = centre + Vector3.Transform(-size, rotation);
		Vector3 vector2 = centre + Vector3.Transform(new Vector3(0f - size.X, 0f - size.Y, size.Z), rotation);
		Vector3 vector3 = centre + Vector3.Transform(new Vector3(0f - size.X, size.Y, 0f - size.Z), rotation);
		Vector3 vector4 = centre + Vector3.Transform(new Vector3(size.X, 0f - size.Y, 0f - size.Z), rotation);
		Vector3 vector5 = centre + Vector3.Transform(new Vector3(0f - size.X, size.Y, size.Z), rotation);
		Vector3 vector6 = centre + Vector3.Transform(new Vector3(size.X, 0f - size.Y, size.Z), rotation);
		Vector3 vector7 = centre + Vector3.Transform(new Vector3(size.X, size.Y, 0f - size.Z), rotation);
		Vector3 vector8 = centre + Vector3.Transform(size, rotation);
		if (solid)
		{
			Triangle(vector, vector6, vector2, colour);
			Triangle(vector, vector6, vector4, colour);
			Triangle(vector, vector5, vector2, colour);
			Triangle(vector, vector5, vector3, colour);
			Triangle(vector, vector7, vector4, colour);
			Triangle(vector, vector7, vector3, colour);
			Triangle(vector8, vector4, vector7, colour);
			Triangle(vector8, vector4, vector6, colour);
			Triangle(vector8, vector2, vector5, colour);
			Triangle(vector8, vector2, vector6, colour);
			Triangle(vector8, vector3, vector5, colour);
			Triangle(vector8, vector3, vector7, colour);
		}
		else
		{
			Line(vector, vector2, colour);
			Line(vector2, vector6, colour);
			Line(vector6, vector4, colour);
			Line(vector4, vector, colour);
			Line(vector, vector3, colour);
			Line(vector2, vector5, colour);
			Line(vector6, vector8, colour);
			Line(vector4, vector7, colour);
			Line(vector8, vector7, colour);
			Line(vector7, vector3, colour);
			Line(vector3, vector5, colour);
			Line(vector5, vector8, colour);
		}
	}

	public static void Box(BoundingBox boundingBox, Color colour, bool solid)
	{
		Box(boundingBox.Min, boundingBox.Max, colour, solid);
	}

	public static void Frustum(Vector3 focalPoint, float yaw, float pitch, float roll, float verticalAngle, float aspectRatio, float nearClip, float farClip, Color colour, bool solid)
	{
		float num = (float)Math.Atan((float)Math.Tan(verticalAngle * 0.5f) * aspectRatio);
		float num2 = nearClip * (float)Math.Tan(verticalAngle * 0.5f);
		float num3 = nearClip * (float)Math.Tan(num);
		Vector3 value = new Vector3(0f - num3, num2, nearClip);
		Vector3 value2 = new Vector3(num3, num2, nearClip);
		Vector3 value3 = new Vector3(0f - num3, 0f - num2, nearClip);
		Vector3 value4 = new Vector3(num3, 0f - num2, nearClip);
		num2 = farClip * (float)Math.Tan(verticalAngle * 0.5f);
		num3 = farClip * (float)Math.Tan(num);
		Vector3 value5 = new Vector3(0f - num3, num2, farClip);
		Vector3 value6 = new Vector3(num3, num2, farClip);
		Vector3 value7 = new Vector3(0f - num3, 0f - num2, farClip);
		Vector3 value8 = new Vector3(num3, 0f - num2, farClip);
		Quaternion rotation = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
		value = Vector3.Transform(value, rotation) + focalPoint;
		value2 = Vector3.Transform(value2, rotation) + focalPoint;
		value3 = Vector3.Transform(value3, rotation) + focalPoint;
		value4 = Vector3.Transform(value4, rotation) + focalPoint;
		value5 = Vector3.Transform(value5, rotation) + focalPoint;
		value6 = Vector3.Transform(value6, rotation) + focalPoint;
		value7 = Vector3.Transform(value7, rotation) + focalPoint;
		value8 = Vector3.Transform(value8, rotation) + focalPoint;
		if (solid)
		{
			Triangle(value3, value8, value7, colour);
			Triangle(value3, value8, value4, colour);
			Triangle(value3, value5, value7, colour);
			Triangle(value3, value5, value, colour);
			Triangle(value3, value2, value4, colour);
			Triangle(value3, value2, value, colour);
			Triangle(value6, value4, value2, colour);
			Triangle(value6, value4, value8, colour);
			Triangle(value6, value7, value5, colour);
			Triangle(value6, value7, value8, colour);
			Triangle(value6, value, value5, colour);
			Triangle(value6, value, value2, colour);
		}
		else
		{
			Line(value, value2, colour);
			Line(value2, value4, colour);
			Line(value4, value3, colour);
			Line(value3, value, colour);
			Line(value, value5, colour);
			Line(value2, value6, colour);
			Line(value4, value8, colour);
			Line(value3, value7, colour);
			Line(value5, value6, colour);
			Line(value6, value8, colour);
			Line(value8, value7, colour);
			Line(value7, value5, colour);
		}
	}

	public static void Frustum(BoundingFrustum boundingFrustum, Color colour, bool solid)
	{
		Vector3[] corners = boundingFrustum.GetCorners();
		if (solid)
		{
			Triangle(corners[3], corners[6], corners[7], colour);
			Triangle(corners[3], corners[6], corners[2], colour);
			Triangle(corners[3], corners[4], corners[7], colour);
			Triangle(corners[3], corners[4], corners[0], colour);
			Triangle(corners[3], corners[1], corners[2], colour);
			Triangle(corners[3], corners[1], corners[0], colour);
			Triangle(corners[5], corners[2], corners[1], colour);
			Triangle(corners[5], corners[2], corners[6], colour);
			Triangle(corners[5], corners[7], corners[4], colour);
			Triangle(corners[5], corners[7], corners[6], colour);
			Triangle(corners[5], corners[0], corners[4], colour);
			Triangle(corners[5], corners[0], corners[1], colour);
		}
		else
		{
			Line(corners[0], corners[1], colour);
			Line(corners[1], corners[2], colour);
			Line(corners[2], corners[3], colour);
			Line(corners[3], corners[0], colour);
			Line(corners[0], corners[4], colour);
			Line(corners[1], corners[5], colour);
			Line(corners[2], corners[6], colour);
			Line(corners[3], corners[7], colour);
			Line(corners[4], corners[5], colour);
			Line(corners[5], corners[6], colour);
			Line(corners[6], corners[7], colour);
			Line(corners[7], corners[4], colour);
		}
	}

	public static void Sphere(Vector3 centre, float radius, Color colour, bool solid, int numSegments, int numSlices)
	{
		int num = numSegments * (numSlices + 1);
		if (num > m_numSphereVertices)
		{
			m_sphereVertices = new Vector3[num];
			m_numSphereVertices = num;
		}
		for (int i = 0; i <= numSlices; i++)
		{
			float num2 = (float)Math.PI * (float)i / (float)numSlices;
			float num3 = radius * (float)Math.Sin(num2);
			float y = centre.Y + radius * (float)Math.Cos(num2);
			for (int j = 0; j < numSegments; j++)
			{
				int num4 = i * numSegments + j;
				float num5 = (float)Math.PI * 2f * (float)j / (float)numSegments;
				m_sphereVertices[num4].X = centre.X + num3 * (float)Math.Sin(num5);
				m_sphereVertices[num4].Y = y;
				m_sphereVertices[num4].Z = centre.Z + num3 * (float)Math.Cos(num5);
			}
		}
		if (solid)
		{
			for (int k = 0; k < numSegments; k++)
			{
				for (int l = 0; l < numSlices; l++)
				{
					int num6 = l * numSegments + k;
					int num7 = (l + 1) * numSegments + k;
					int num8 = l * numSegments + (k + 1) % numSegments;
					int num9 = (l + 1) * numSegments + (k + 1) % numSegments;
					Triangle(m_sphereVertices[num6], m_sphereVertices[num8], m_sphereVertices[num7], colour);
					Triangle(m_sphereVertices[num9], m_sphereVertices[num8], m_sphereVertices[num7], colour);
				}
			}
			return;
		}
		for (int m = 1; m < numSlices; m++)
		{
			for (int n = 0; n < numSegments; n++)
			{
				int num10 = m * numSegments + n;
				int num11 = m * numSegments + (n + 1) % numSegments;
				Line(m_sphereVertices[num10], m_sphereVertices[num11], colour);
			}
		}
		for (int num12 = 0; num12 < numSegments; num12++)
		{
			for (int num13 = 0; num13 < numSlices; num13++)
			{
				int num14 = num13 * numSegments + num12;
				int num15 = (num13 + 1) * numSegments + num12;
				Line(m_sphereVertices[num14], m_sphereVertices[num15], colour);
			}
		}
	}

	public static void Sphere(Vector3 centre, float radius, Color colour, bool solid)
	{
		Sphere(centre, radius, colour, solid, solid ? DefaultNumSegmentsSolid : DefaultNumSegmentsLine, solid ? DefaultNumSegmentsSolid : DefaultNumSegmentsLine);
	}

	public static void Sphere(BoundingSphere boundingSphere, Color colour, bool solid)
	{
		Sphere(boundingSphere.Center, boundingSphere.Radius, colour, solid);
	}

	public static void Cylinder(Vector3 start, Vector3 end, float radius, Color colour, bool solid, int numSegments)
	{
		if (numSegments > m_numCylinderSegments)
		{
			m_cylinderBaseVertices = new Vector3[numSegments];
			m_cylinderTopVertices = new Vector3[numSegments];
			m_numCylinderSegments = numSegments;
		}
		Quaternion rotation = GetRotateFromYAxis(start, end);
		for (int i = 0; i < numSegments; i++)
		{
			float num = (float)Math.PI * 2f * (float)i / (float)numSegments;
			m_cylinderBaseVertices[i].X = radius * (float)Math.Sin(num);
			m_cylinderBaseVertices[i].Y = 0f;
			m_cylinderBaseVertices[i].Z = radius * (float)Math.Cos(num);
			Vector3.Transform(ref m_cylinderBaseVertices[i], ref rotation, out m_cylinderBaseVertices[i]);
			m_cylinderTopVertices[i] = m_cylinderBaseVertices[i] + end;
			m_cylinderBaseVertices[i] += start;
		}
		if (solid)
		{
			for (int j = 0; j < numSegments; j++)
			{
				int num2 = (j + 1) % numSegments;
				Triangle(m_cylinderBaseVertices[j], m_cylinderTopVertices[num2], m_cylinderBaseVertices[num2], colour);
				Triangle(m_cylinderBaseVertices[j], m_cylinderTopVertices[num2], m_cylinderTopVertices[j], colour);
				Triangle(start, m_cylinderBaseVertices[j], m_cylinderBaseVertices[num2], colour);
				Triangle(end, m_cylinderTopVertices[j], m_cylinderTopVertices[num2], colour);
			}
			return;
		}
		for (int k = 0; k < numSegments; k++)
		{
			int num3 = (k + 1) % numSegments;
			Line(m_cylinderBaseVertices[k], m_cylinderTopVertices[k], colour);
			Line(start, m_cylinderBaseVertices[k], colour);
			Line(end, m_cylinderTopVertices[k], colour);
			Line(m_cylinderBaseVertices[k], m_cylinderBaseVertices[num3], colour);
			Line(m_cylinderTopVertices[k], m_cylinderTopVertices[num3], colour);
		}
	}

	public static void Cylinder(Vector3 start, Vector3 end, float radius, Color colour, bool solid)
	{
		Cylinder(start, end, radius, colour, solid, solid ? DefaultNumSegmentsSolid : DefaultNumSegmentsLine);
	}

	public static void Cone(Vector3 baseCentre, Vector3 point, float radius, Color colour, bool solid, int numSegments)
	{
		if (numSegments > m_numCylinderSegments)
		{
			m_cylinderBaseVertices = new Vector3[numSegments];
			m_cylinderTopVertices = new Vector3[numSegments];
			m_numCylinderSegments = numSegments;
		}
		Quaternion rotation = GetRotateFromYAxis(baseCentre, point);
		for (int i = 0; i < numSegments; i++)
		{
			float num = (float)Math.PI * 2f * (float)i / (float)numSegments;
			m_cylinderBaseVertices[i].X = radius * (float)Math.Sin(num);
			m_cylinderBaseVertices[i].Y = 0f;
			m_cylinderBaseVertices[i].Z = radius * (float)Math.Cos(num);
			Vector3.Transform(ref m_cylinderBaseVertices[i], ref rotation, out m_cylinderBaseVertices[i]);
			m_cylinderBaseVertices[i] += baseCentre;
		}
		if (solid)
		{
			for (int j = 0; j < numSegments; j++)
			{
				int num2 = (j + 1) % numSegments;
				Triangle(m_cylinderBaseVertices[j], m_cylinderBaseVertices[num2], point, colour);
				Triangle(baseCentre, m_cylinderBaseVertices[j], m_cylinderBaseVertices[num2], colour);
			}
			return;
		}
		for (int k = 0; k < numSegments; k++)
		{
			int num3 = (k + 1) % numSegments;
			Line(m_cylinderBaseVertices[k], point, colour);
			Line(m_cylinderBaseVertices[k], baseCentre, colour);
			Line(m_cylinderBaseVertices[k], m_cylinderBaseVertices[num3], colour);
		}
	}

	public static void Cone(Vector3 baseCentre, Vector3 point, float radius, Color colour, bool solid)
	{
		Cone(baseCentre, point, radius, colour, solid, solid ? DefaultNumSegmentsSolid : DefaultNumSegmentsLine);
	}

	public static void Arrow(Vector3 begin, Vector3 end, float shaftRadius, float headProportion, float headRadiusFactor, Color colour, bool solid, int numSegments)
	{
		Vector3 vector = Vector3.Lerp(end, begin, headProportion);
		Cylinder(begin, vector, shaftRadius, colour, solid, numSegments);
		Cone(vector, end, shaftRadius * headRadiusFactor, colour, solid, numSegments);
	}

	public static void Arrow(Vector3 begin, Vector3 end, float shaftRadius, float headProportion, float headRadiusFactor, Color colour, bool solid)
	{
		Arrow(begin, end, shaftRadius, headProportion, headRadiusFactor, colour, solid, solid ? DefaultNumSegmentsSolid : DefaultNumSegmentsLine);
	}

	public static void Arrow(Vector3 begin, Vector3 end, float shaftRadius, Color colour, bool solid, int numSegments)
	{
		Arrow(begin, end, shaftRadius, DefaultArrowHeadProportion, DefaultArrowHeadRadiusFactor, colour, solid, numSegments);
	}

	public static void Arrow(Vector3 begin, Vector3 end, float shaftRadius, Color colour, bool solid)
	{
		Arrow(begin, end, shaftRadius, DefaultArrowHeadProportion, DefaultArrowHeadRadiusFactor, colour, solid);
	}

	public static void Capsule(Vector3 begin, Vector3 end, float radius, Color colour, bool solid, int numSegments)
	{
		Sphere(begin, radius, colour, solid, numSegments, numSegments);
		Cylinder(begin, end, radius, colour, solid, numSegments);
		Sphere(end, radius, colour, solid, numSegments, numSegments);
	}

	public static void Capsule(Vector3 begin, Vector3 end, float radius, Color colour, bool solid)
	{
		Capsule(begin, end, radius, colour, solid, solid ? DefaultNumSegmentsSolid : DefaultNumSegmentsLine);
	}

	private static void Render2DLines(GraphicsDevice device, float width, float height)
	{
		if (s_line2DVertices.Count > 0)
		{
			if (s_line2DVertices.Count > s_line2DVerticesArraySize)
			{
				s_line2DVerticesArraySize = s_line2DVertices.Count;
				s_line2DVerticesArray = new VertexPositionColor[s_line2DVerticesArraySize];
			}
			s_line2DVertices.CopyTo(s_line2DVerticesArray);
			for (int i = 0; i < s_line2DVertices.Count; i++)
			{
				s_line2DVerticesArray[i].Position.X = -1f + 2f * s_line2DVerticesArray[i].Position.X / width;
				s_line2DVerticesArray[i].Position.Y = 0f - (-1f + 2f * s_line2DVerticesArray[i].Position.Y / height);
			}
			device.DepthStencilState = DepthStencilState.None;
			effect.CurrentTechnique = effect.Techniques["LineRendering2D"];
			for (int j = 0; j < effect.CurrentTechnique.Passes.Count; j++)
			{
				effect.CurrentTechnique.Passes[j].Apply();
				device.DrawUserPrimitives(PrimitiveType.LineList, s_line2DVerticesArray, 0, s_line2DVertices.Count / 2);
			}
			s_line2DVertices.Clear();
		}
	}

	private static void Render2DTriangles(GraphicsDevice device, float width, float height)
	{
		if (s_triangle2DVertices.Count > 0)
		{
			if (s_triangle2DVertices.Count > s_triangle2DVerticesArraySize)
			{
				s_triangle2DVerticesArraySize = s_triangle2DVertices.Count;
				s_triangle2DVerticesArray = new VertexPositionColor[s_triangle2DVerticesArraySize];
			}
			s_triangle2DVertices.CopyTo(s_triangle2DVerticesArray);
			for (int i = 0; i < s_triangle2DVertices.Count; i++)
			{
				s_triangle2DVerticesArray[i].Position.X = -1f + 2f * s_triangle2DVerticesArray[i].Position.X / width;
				s_triangle2DVerticesArray[i].Position.Y = 0f - (-1f + 2f * s_triangle2DVerticesArray[i].Position.Y / height);
			}
			device.DepthStencilState = DepthStencilState.None;
			effect.CurrentTechnique = effect.Techniques["LineRendering2D"];
			for (int j = 0; j < effect.CurrentTechnique.Passes.Count; j++)
			{
				effect.CurrentTechnique.Passes[j].Apply();
				device.DrawUserPrimitives(PrimitiveType.TriangleList, s_triangle2DVerticesArray, 0, s_triangle2DVertices.Count / 3);
			}
			s_triangle2DVertices.Clear();
		}
	}

	private static void Render3DLines(GraphicsDevice device, Matrix renderMatrix)
	{
		if (s_line3DVertices.Count > 0)
		{
			if (s_line3DVertices.Count > s_line3DVerticesArraySize)
			{
				s_line3DVerticesArraySize = s_line3DVertices.Count;
				s_line3DVerticesArray = new VertexPositionColor[s_line3DVerticesArraySize];
			}
			s_line3DVertices.CopyTo(s_line3DVerticesArray);
			device.DepthStencilState = DepthStencilState.Default;
			effect.Parameters["renderMatrix"].SetValue(renderMatrix);
			effect.CurrentTechnique = effect.Techniques["LineRendering3D"];
			for (int i = 0; i < effect.CurrentTechnique.Passes.Count; i++)
			{
				effect.CurrentTechnique.Passes[i].Apply();
				device.DrawUserPrimitives(PrimitiveType.LineList, s_line3DVerticesArray, 0, s_line3DVertices.Count / 2);
			}
			s_line3DVertices.Clear();
		}
	}

	private static void Render3DTriangles(GraphicsDevice device, Matrix renderMatrix)
	{
		if (s_triangle3DVertices.Count > 0)
		{
			if (s_triangle3DVertices.Count > s_triangle3DVerticesArraySize)
			{
				s_triangle3DVerticesArraySize = s_triangle3DVertices.Count;
				s_triangle3DVerticesArray = new VertexPositionColor[s_triangle3DVerticesArraySize];
			}
			s_triangle3DVertices.CopyTo(s_triangle3DVerticesArray);
			device.DepthStencilState = DepthStencilState.Default;
			effect.Parameters["renderMatrix"].SetValue(renderMatrix);
			effect.CurrentTechnique = effect.Techniques["LineRendering3D"];
			for (int i = 0; i < effect.CurrentTechnique.Passes.Count; i++)
			{
				effect.CurrentTechnique.Passes[i].Apply();
				device.DrawUserPrimitives(PrimitiveType.TriangleList, s_triangle3DVerticesArray, 0, s_triangle3DVertices.Count / 3);
			}
			s_triangle3DVertices.Clear();
		}
	}

	private static Quaternion GetRotateFromYAxis(Vector3 start, Vector3 end)
	{
		Vector3 vector = end - start;
		vector.Normalize();
		if (vector == Vector3.UnitY)
		{
			return Quaternion.Identity;
		}
		if (vector == -Vector3.UnitY)
		{
			return Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)Math.PI);
		}
		Matrix matrix = default(Matrix);
		matrix.Up = vector;
		matrix.Right = Vector3.Cross(Vector3.UnitY, vector);
		matrix.Forward = Vector3.Cross(vector, matrix.Right);
		return Quaternion.CreateFromRotationMatrix(matrix);
	}
}
