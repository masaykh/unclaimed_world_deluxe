using System;
using InputEventSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowSystem;

public class Spinner : UIComponent
{
	public delegate void CountChangedHandler(int newCount, EventArgs e);

	public enum SpinnerType
	{
		LCD,
		HUD
	}

	protected ImageButton btIncrease;

	protected ImageButton btDecrease;

	protected TextBox tbNumber;

	protected int noOfDigits = 3;

	protected int defaultWidth = 40;

	private double seconds;

	private bool increaseIsPressed;

	private bool decreaseIsPressed;

	private int maxValue = 10;

	private float increaseFraction;

	private bool changedCounter;

	private float currentSpeed;

	private const float acceleration = 8f;

	private const float maxSpeed = 100f;

	private const float minSpeed = 3f;

	public override string ToolTip
	{
		get
		{
			return base.ToolTip;
		}
		set
		{
			base.ToolTip = value;
			btIncrease.ToolTip = value;
			btDecrease.ToolTip = value;
		}
	}

	public int NoOfDigits
	{
		get
		{
			return noOfDigits;
		}
		set
		{
			noOfDigits = value;
			string text = "";
			tbNumber.Width = tbNumber.GetTextWidth(text.PadRight(noOfDigits, '9'));
			maxValue = (int)Math.Pow(10.0, noOfDigits) - 1;
			Width = tbNumber.Width + btIncrease.Width;
		}
	}

	public int Count
	{
		get
		{
			return int.Parse(tbNumber.Text);
		}
		set
		{
			tbNumber.Text = value.ToString();
		}
	}

	public SpriteFont Font
	{
		set
		{
			tbNumber.Font = value;
		}
	}

	public event CountChangedHandler CountChanged;

	public Spinner(GUIManager guiManager)
		: base(guiManager)
	{
		btIncrease = new ImageButton(guiManager);
		btDecrease = new ImageButton(guiManager);
		tbNumber = new TextBox(guiManager);
		Add(btIncrease);
		Add(btDecrease);
		Add(tbNumber);
		Width = defaultWidth;
		tbNumber.IsEditable = true;
		btIncrease.MouseDown += btIncrease_MouseDown;
		btIncrease.MouseUp += btIncrease_MouseUp;
		btDecrease.MouseDown += btDecrease_MouseDown;
		btDecrease.MouseUp += btDecrease_MouseUp;
		NoOfDigits = 3;
	}

	private void btDecrease_MouseUp(MouseEventArgs args)
	{
		increaseIsPressed = false;
		decreaseIsPressed = false;
		currentSpeed = 3f;
		increaseFraction = 0f;
		if (!changedCounter)
		{
			Increase(-1);
		}
		changedCounter = false;
	}

	private void btIncrease_MouseUp(MouseEventArgs args)
	{
		increaseIsPressed = false;
		decreaseIsPressed = false;
		currentSpeed = 3f;
		increaseFraction = 0f;
		if (!changedCounter)
		{
			Increase(1);
		}
		changedCounter = false;
	}

	private void btDecrease_MouseDown(MouseEventArgs args)
	{
		decreaseIsPressed = true;
	}

	private void btIncrease_MouseDown(MouseEventArgs args)
	{
		increaseIsPressed = true;
	}

	private void Increase(int amount)
	{
		changedCounter = true;
		int count = Count;
		int num = count + amount;
		if (num > maxValue)
		{
			num = maxValue;
		}
		else if (num < 0)
		{
			num = 0;
		}
		if (num != count)
		{
			Count = num;
			if (this.CountChanged != null)
			{
				this.CountChanged(Count, EventArgs);
			}
		}
	}

	public void Init(SpinnerType type)
	{
		switch (type)
		{
		case SpinnerType.LCD:
			btIncrease.Init(ImageButtonType.LCDIncrease);
			btDecrease.Init(ImageButtonType.LCDDecrease);
			Font = GUIManager.LCDandHUDFont;
			tbNumber.IsEditable = false;
			tbNumber.Y = 1;
			Height = btIncrease.Height + btDecrease.Height;
			break;
		case SpinnerType.HUD:
			btIncrease.Init(ImageButtonType.HUDIncrease);
			btDecrease.Init(ImageButtonType.HUDDecrease);
			Font = GUIManager.LCDandHUDFont;
			tbNumber.IsEditable = true;
			tbNumber.Y = 1;
			tbNumber.Init(TextBox.TextBoxType.HUD);
			Height = btIncrease.Height + btDecrease.Height;
			break;
		}
	}

	public override void Update(GameTime gameTime)
	{
		if (increaseIsPressed)
		{
			increaseFraction += (float)(gameTime.ElapsedGameTime.TotalSeconds * (double)currentSpeed);
			if (increaseFraction >= 1f)
			{
				Increase((int)increaseFraction);
				increaseFraction = Math.Max(0f, increaseFraction - (float)(int)increaseFraction);
			}
			if (currentSpeed < 100f)
			{
				currentSpeed += (float)(gameTime.ElapsedGameTime.TotalSeconds * 8.0);
			}
			if (currentSpeed > 100f)
			{
				currentSpeed = 100f;
			}
		}
		else if (decreaseIsPressed)
		{
			increaseFraction += (float)(gameTime.ElapsedGameTime.TotalSeconds * (double)currentSpeed);
			if (increaseFraction >= 1f)
			{
				Increase(-(int)increaseFraction);
				increaseFraction = Math.Max(0f, increaseFraction - (float)(int)increaseFraction);
			}
			if (currentSpeed < 100f)
			{
				currentSpeed += (float)(gameTime.ElapsedGameTime.TotalSeconds * 8.0);
			}
			if (currentSpeed > 100f)
			{
				currentSpeed = 100f;
			}
		}
		base.Update(gameTime);
	}

	protected override void OnResize(UIComponent sender)
	{
		base.OnResize(sender);
		if (btIncrease != null && tbNumber != null)
		{
			tbNumber.Width = Width - btIncrease.Width;
			tbNumber.Height = Height;
			btIncrease.X = tbNumber.Width;
			btDecrease.X = tbNumber.Width;
			btIncrease.Y = 0;
			btDecrease.Y = btIncrease.Height;
		}
	}
}
