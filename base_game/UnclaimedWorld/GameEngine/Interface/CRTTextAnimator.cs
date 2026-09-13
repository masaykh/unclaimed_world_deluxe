using System.Collections.Generic;
using Microsoft.Xna.Framework;
using WindowSystem;

namespace GameEngine.Interface;

public class CRTTextAnimator
{
	private List<Label> labels;

	private List<string> originalTexts = new List<string>();

	public float TimeBetweenCharacters = 0.1f;

	private double timePassed;

	private bool isStarted;

	private int currentLabelIndex;

	public void Add(Label label)
	{
		labels.Add(label);
	}

	public void StartAnimating()
	{
		foreach (Label label in labels)
		{
			originalTexts.Add(label.Text);
			label.Text = "";
		}
		isStarted = true;
		timePassed = 0.0;
		currentLabelIndex = 0;
	}

	public void Update(GameTime gameTime)
	{
		if (isStarted)
		{
			timePassed += gameTime.ElapsedGameTime.TotalSeconds;
		}
		if (!(timePassed > (double)TimeBetweenCharacters))
		{
			return;
		}
		Label label = labels[currentLabelIndex];
		string text = originalTexts[currentLabelIndex];
		int length = label.Text.Length;
		if (length == text.Length)
		{
			currentLabelIndex++;
			if (currentLabelIndex == labels.Count)
			{
				isStarted = false;
			}
		}
		else
		{
			label.Text = originalTexts[currentLabelIndex].Substring(0, length + 1);
		}
		timePassed -= TimeBetweenCharacters;
	}
}
