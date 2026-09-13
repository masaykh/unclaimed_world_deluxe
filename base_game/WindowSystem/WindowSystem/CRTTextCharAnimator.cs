using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace WindowSystem;

public class CRTTextCharAnimator : CRTTextAnimator
{
	private List<Label> labels = new List<Label>();

	private List<string> originalTexts = new List<string>();

	private string cursorString = "I";

	public CRTTextCharAnimator(GUIManager gui)
		: base(gui)
	{
	}

	public override void Add(Label label)
	{
		labels.Add(label);
	}

	public override void Clear()
	{
		for (int i = 0; i < labels.Count; i++)
		{
			labels[i].Text = originalTexts[i];
		}
		labels.Clear();
		originalTexts.Clear();
		Stop();
	}

	public override int Add(UIComponent control)
	{
		throw new Exception("Can only add labels...");
	}

	public override void StartAnimating()
	{
		if (isStarted || labels.Count <= 0)
		{
			return;
		}
		originalTexts.Clear();
		foreach (Label label in labels)
		{
			originalTexts.Add(label.Text);
			label.Text = "";
		}
		StartIt();
	}

	public override void Update(GameTime gameTime)
	{
		if (labels.Count <= 0 || !isStarted)
		{
			return;
		}
		timePassed += gameTime.ElapsedGameTime.TotalSeconds;
		if (!(timePassed > (double)TimeBetweenUpdates))
		{
			return;
		}
		Label label = labels[currentIndex];
		string text = originalTexts[currentIndex];
		int num = label.Text.Length - cursorString.Length;
		if (text == "" || num == text.Length)
		{
			label.Text = text;
			currentIndex++;
			if (currentIndex == labels.Count)
			{
				Stop();
			}
		}
		else
		{
			label.Text = text.Substring(0, num + 1) + cursorString;
		}
		timePassed -= TimeBetweenUpdates;
	}
}
