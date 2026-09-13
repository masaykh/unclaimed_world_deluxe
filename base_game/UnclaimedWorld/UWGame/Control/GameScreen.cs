using System;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;

namespace UWGame.Control;

public abstract class GameScreen
{
	private bool isPopup;

	private TimeSpan transitionOnTime = TimeSpan.Zero;

	private TimeSpan transitionOffTime = TimeSpan.Zero;

	private float transitionPosition = 1f;

	private ScreenState screenState;

	private bool isExiting;

	private bool otherScreenHasFocus;

	private Controller controller;

	public bool IsPopup
	{
		get
		{
			return isPopup;
		}
		protected set
		{
			isPopup = value;
		}
	}

	public TimeSpan TransitionOnTime
	{
		get
		{
			return transitionOnTime;
		}
		protected set
		{
			transitionOnTime = value;
		}
	}

	public TimeSpan TransitionOffTime
	{
		get
		{
			return transitionOffTime;
		}
		protected set
		{
			transitionOffTime = value;
		}
	}

	public float TransitionPosition
	{
		get
		{
			return transitionPosition;
		}
		protected set
		{
			transitionPosition = value;
		}
	}

	public byte TransitionAlpha => (byte)(255f - TransitionPosition * 255f);

	public ScreenState ScreenState
	{
		get
		{
			return screenState;
		}
		protected set
		{
			screenState = value;
		}
	}

	public bool IsExiting
	{
		get
		{
			return isExiting;
		}
		protected set
		{
			isExiting = value;
		}
	}

	public bool IsActive
	{
		get
		{
			if (!otherScreenHasFocus)
			{
				if (screenState != ScreenState.TransitionOn)
				{
					return screenState == ScreenState.Active;
				}
				return true;
			}
			return false;
		}
	}

	public Controller Controller
	{
		get
		{
			return controller;
		}
		internal set
		{
			controller = value;
		}
	}

	public virtual void LoadContent()
	{
	}

	protected virtual float getStringEnlargementFactor()
	{
		return 1f;
	}

	public virtual void UnloadContent()
	{
	}

	public virtual void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		this.otherScreenHasFocus = otherScreenHasFocus;
		if (isExiting)
		{
			screenState = ScreenState.TransitionOff;
			if (!UpdateTransition(gameTime, transitionOffTime, 1))
			{
				Controller.RemoveScreenNow(this);
				isExiting = false;
			}
		}
		else if (coveredByOtherScreen)
		{
			if (UpdateTransition(gameTime, transitionOffTime, 1))
			{
				screenState = ScreenState.TransitionOff;
			}
			else
			{
				screenState = ScreenState.Hidden;
			}
		}
		else if (UpdateTransition(gameTime, transitionOnTime, -1))
		{
			screenState = ScreenState.TransitionOn;
		}
		else
		{
			screenState = ScreenState.Active;
		}
	}

	private bool UpdateTransition(GameTime gameTime, TimeSpan time, int direction)
	{
		float num = ((!(time == TimeSpan.Zero)) ? ((float)(gameTime.ElapsedGameTime.TotalMilliseconds / time.TotalMilliseconds)) : 1f);
		transitionPosition += num * (float)direction;
		if (transitionPosition <= 0f || transitionPosition >= 1f)
		{
			transitionPosition = MathHelper.Clamp(transitionPosition, 0f, 1f);
			return false;
		}
		return true;
	}

	public virtual void HandleInput()
	{
	}

	public abstract void Draw(GameTime gameTime);

	public virtual void ExitScreen()
	{
		if (TransitionOffTime == TimeSpan.Zero)
		{
			Controller.RemoveScreenNow(this);
		}
		else
		{
			isExiting = true;
		}
	}

	public virtual void Destroy()
	{
	}

	public virtual void Init()
	{
	}

	public void IgnoreSnapshotFields(Snapshotter sn)
	{
		sn.Ignore(isPopup);
		sn.Ignore(otherScreenHasFocus);
		sn.Ignore(isExiting);
		sn.Ignore(transitionOffTime);
		sn.Ignore(transitionOnTime);
		sn.Ignore(screenState);
		sn.Ignore(transitionPosition);
	}
}
