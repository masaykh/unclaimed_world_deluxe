using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowSystem;

public class Animation2D
{
	public Vector2 Origin;

	public static Color transp = new Color(0, 0, 0, 0);

	public static Color halfTransp = new Color(128, 128, 128, 128);

	public static Color thirdTransp = new Color(85, 85, 85, 85);

	public static Color black = Color.FromNonPremultiplied(new Vector4(0f, 0f, 0f, 1f));

	public bool DoColorInterpolation = true;

	private Texture2D texture;

	private float frameTime;

	private bool isLooping;

	public List<Cell> Cells = new List<Cell>();

	[XmlIgnore]
	public Texture2D Texture => texture;

	public float FrameTime
	{
		get
		{
			return frameTime;
		}
		set
		{
			frameTime = value;
		}
	}

	public bool IsLooping => isLooping;

	public int FrameCount => Cells.Count;

	public Animation2D()
	{
	}

	public Animation2D(Texture2D texture, float frameTime, bool isLooping)
	{
		this.texture = texture;
		this.frameTime = frameTime;
		this.isLooping = isLooping;
	}
}
