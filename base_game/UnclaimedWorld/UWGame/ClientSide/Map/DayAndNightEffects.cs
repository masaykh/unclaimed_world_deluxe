using System;
using Microsoft.Xna.Framework;

namespace UWGame.ClientSide.Map;

public class DayAndNightEffects
{
	public enum SunAnimations
	{
		Night,
		MorningBeforeSunrise,
		MorningAfterSunrise,
		Day,
		EveningBeforeSunset,
		EveningAfterSunset
	}

	public Curve TimeOfDayAlpha;

	public Curve TimeOfDayReds;

	public Curve TimeOfDayGreens;

	public Curve TimeOfDayBlues;

	public SunAnimations SunAnimation;

	private const float sunriseOnCurve = 42f;

	private const float sunriseOnCurveEnds = 80f;

	private const float sunsetOnCurveStarts = 191f;

	private const float sunsetOnCurve = 242f;

	private const float sundiskHitsHorizon = 0.04f;

	private Matrix shadowWarping = Matrix.CreateScale(new Vector3(1.2f, 0.8f, 1f));

	public Matrix SunShadowRotationMatrix;

	public Matrix ShadowScaling;

	public float ShadowLength;

	public float ShadowXAlignment;

	private const float maxShadowLengthScaling = 10f;

	private Matrix? shadowMatrix;

	private Plane lightPlane = new Plane(-Vector3.UnitZ, 0f);

	public Matrix GroundObjectsShadowMatrix
	{
		get
		{
			if (shadowMatrix.HasValue)
			{
				return shadowMatrix.Value;
			}
			shadowMatrix = Matrix.CreateShadow(The.Sim.DateAndTime.SunPosition, lightPlane);
			return shadowMatrix.Value;
		}
	}

	public void LoadContent()
	{
		TimeOfDayAlpha = The.Client.Content.Load<Curve>("TimeOfDay_Alpha");
		TimeOfDayReds = The.Client.Content.Load<Curve>("TimeOfDay_Reds");
		TimeOfDayGreens = The.Client.Content.Load<Curve>("TimeOfDay_Greens");
		TimeOfDayBlues = The.Client.Content.Load<Curve>("TimeOfDay_Blue");
	}

	public void Recompute()
	{
		shadowMatrix = null;
		SunAnimation = GetAnimation();
		if (The.Sim.DateAndTime.SunIsUp)
		{
			ShadowXAlignment = MathHelper.SmoothStep(0f, 1f, Math.Abs(The.Sim.DateAndTime.UnshiftedAzimuth) / ((float)Math.PI / 2f));
			SunShadowRotationMatrix = Matrix.CreateRotationZ(The.Sim.DateAndTime.SunAzimuth - (float)Math.PI) * shadowWarping;
			ShadowLength = MathHelper.Clamp((float)(1.0 / Math.Tan(The.Sim.DateAndTime.SunElevation)), 0.1f, 10f);
			ShadowLength = MathHelper.SmoothStep(0.1f, 10f, ShadowLength / 10f);
			ShadowScaling = Matrix.CreateScale(1f - 0.5f * (ShadowLength / 10f), ShadowLength, 1f);
		}
	}

	public Color? GetTimeOfDayColor()
	{
		float position = 120f;
		switch (SunAnimation)
		{
		case SunAnimations.MorningBeforeSunrise:
		{
			float amount = The.Sim.DateAndTime.GetProgress(-0.209f);
			position = amount * 42f;
			break;
		}
		case SunAnimations.MorningAfterSunrise:
		{
			float amount = The.Sim.DateAndTime.SunElevation / 0.104f;
			position = MathHelper.Lerp(42f, 80f, amount);
			break;
		}
		case SunAnimations.Day:
			return null;
		case SunAnimations.EveningBeforeSunset:
		{
			float amount = MathHelper.Distance(The.Sim.DateAndTime.SunElevation, 0.104f) / 0.104f;
			position = MathHelper.Lerp(191f, 242f, amount);
			break;
		}
		case SunAnimations.EveningAfterSunset:
		{
			float amount = Math.Abs(The.Sim.DateAndTime.SunElevation / -0.314f);
			position = MathHelper.Lerp(242f, 255f, amount);
			break;
		}
		case SunAnimations.Night:
			position = 255f;
			break;
		}
		return new Color((byte)TimeOfDayReds.Evaluate(position), (byte)TimeOfDayGreens.Evaluate(position), (byte)TimeOfDayBlues.Evaluate(position), (byte)TimeOfDayAlpha.Evaluate(position));
	}

	public SunAnimations GetAnimation()
	{
		if (The.Sim.DateAndTime.TimeOfDay < 0.5)
		{
			if (The.Sim.DateAndTime.SunElevation > -0.209f)
			{
				if (The.Sim.DateAndTime.SunElevation < 0f)
				{
					return SunAnimations.MorningBeforeSunrise;
				}
				if (The.Sim.DateAndTime.SunElevation < 0.104f)
				{
					return SunAnimations.MorningAfterSunrise;
				}
				return SunAnimations.Day;
			}
			return SunAnimations.Night;
		}
		if (The.Sim.DateAndTime.SunElevation < 0.104f)
		{
			if (The.Sim.DateAndTime.SunElevation > 0f)
			{
				return SunAnimations.EveningBeforeSunset;
			}
			if (The.Sim.DateAndTime.SunElevation > -0.314f)
			{
				return SunAnimations.EveningAfterSunset;
			}
			return SunAnimations.Night;
		}
		return SunAnimations.Day;
	}

	public Vector3? GetDropShadowEstimate()
	{
		SunAnimations sunAnimation = SunAnimation;
		if ((uint)(sunAnimation - 2) <= 2u)
		{
			return Vector3.Transform(-Vector3.UnitZ * ShadowLength, GroundObjectsShadowMatrix);
		}
		return null;
	}

	public float GetDropShadowAlphaFactor()
	{
		switch (SunAnimation)
		{
		case SunAnimations.MorningBeforeSunrise:
			return 0f;
		case SunAnimations.MorningAfterSunrise:
		{
			if (The.Sim.DateAndTime.SunElevation > 0.04f)
			{
				return 1f;
			}
			float amount = MathHelper.Distance(The.Sim.DateAndTime.SunElevation, 0.04f) / 0.04f;
			return MathHelper.Lerp(1f, 0f, amount);
		}
		case SunAnimations.Day:
			return 1f;
		case SunAnimations.EveningBeforeSunset:
		{
			if (The.Sim.DateAndTime.SunElevation > 0.04f)
			{
				return 1f;
			}
			float amount = MathHelper.Distance(The.Sim.DateAndTime.SunElevation, 0.04f) / 0.04f;
			return MathHelper.Lerp(1f, 0f, amount);
		}
		case SunAnimations.EveningAfterSunset:
			return 0f;
		case SunAnimations.Night:
			return 0f;
		default:
			return 0f;
		}
	}

	public float GetOwnShadowFactor()
	{
		switch (SunAnimation)
		{
		case SunAnimations.MorningBeforeSunrise:
		{
			float amount = MathHelper.Distance(The.Sim.DateAndTime.SunElevation, -0.209f) / Math.Abs(-0.209f);
			return MathHelper.SmoothStep(0f, 0.2f, amount);
		}
		case SunAnimations.MorningAfterSunrise:
		{
			float amount = The.Sim.DateAndTime.SunElevation / 0.104f;
			return MathHelper.SmoothStep(0.2f, 1f, amount);
		}
		case SunAnimations.Day:
			return 1f;
		case SunAnimations.EveningBeforeSunset:
		{
			float amount = MathHelper.Distance(The.Sim.DateAndTime.SunElevation, 0.104f) / 0.104f;
			return MathHelper.SmoothStep(1f, 0.4f, amount);
		}
		case SunAnimations.EveningAfterSunset:
		{
			float amount = Math.Abs(The.Sim.DateAndTime.SunElevation / -0.314f);
			return MathHelper.SmoothStep(0.4f, 0f, amount);
		}
		case SunAnimations.Night:
			return 0f;
		default:
			return 0f;
		}
	}
}
