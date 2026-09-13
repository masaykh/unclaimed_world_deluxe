using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RoundLineCode;

public class RoundLine
{
	private Vector2 p0;

	private Vector2 p1;

	private float rho;

	private float theta;

	public Vector2 P0
	{
		get
		{
			return p0;
		}
		set
		{
			p0 = value;
			RecalcRhoTheta();
		}
	}

	public Vector2 P1
	{
		get
		{
			return p1;
		}
		set
		{
			p1 = value;
			RecalcRhoTheta();
		}
	}

	public float Rho => rho;

	public float Theta => theta;

	public RoundLine(Vector2 p0, Vector2 p1)
	{
		this.p0 = p0;
		this.p1 = p1;
		RecalcRhoTheta();
	}

	public RoundLine(float x0, float y0, float x1, float y1)
	{
		p0 = new Vector2(x0, y0);
		p1 = new Vector2(x1, y1);
		RecalcRhoTheta();
	}

	protected void RecalcRhoTheta()
	{
		Vector2 vector = P1 - P0;
		rho = vector.Length();
		theta = (float)Math.Atan2(vector.Y, vector.X);
	}

	// NOTE: the studio extended this class with a collision/query half - CollideAndSlide,
	// MinDistanceSquaredDeviation, FindFirstIntersection, FindNearbyLines,
	// FindNearbyLinesWithClipping, DistanceSquaredPointToVirtualLine and the private
	// FindRadialT / FindLinearT / Rotate helpers (385 lines). It was character-movement
	// collision against walls, but it was never wired up: zero call sites in the game or in
	// WindowSystem, and the shipped game does its collision elsewhere. Removed so that this
	// rendering type stops implying a movement system the game does not have.
	// It remains in git history and in decomp/RoundLines/ if it is ever wanted.
}
