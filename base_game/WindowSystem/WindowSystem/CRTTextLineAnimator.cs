using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace WindowSystem;

public class CRTTextLineAnimator : CRTTextAnimator
{
	private SortedDictionary<int, List<Label>> labelsByLineNumber = new SortedDictionary<int, List<Label>>();

	private Dictionary<Label, string> originalTextsByLabel = new Dictionary<Label, string>();

	private int[] lineNumbers;

	public CRTTextLineAnimator(GUIManager gui)
		: base(gui)
	{
	}

	public override void Add(Label label)
	{
		if (labelsByLineNumber.TryGetValue(label.AnimateOnCRTScreenLineNo.Value, out var value))
		{
			value.Add(label);
			return;
		}
		value = new List<Label>();
		value.Add(label);
		labelsByLineNumber.Add(label.AnimateOnCRTScreenLineNo.Value, value);
	}

	public override void Clear()
	{
		foreach (KeyValuePair<int, List<Label>> item in labelsByLineNumber)
		{
			foreach (Label item2 in item.Value)
			{
				item2.Text = originalTextsByLabel[item2];
			}
		}
		labelsByLineNumber.Clear();
		originalTextsByLabel.Clear();
		Stop();
	}

	public override int Add(UIComponent control)
	{
		throw new Exception("Can only add labels...");
	}

	public override void StartAnimating()
	{
		if (isStarted || labelsByLineNumber.Count <= 0)
		{
			return;
		}
		originalTextsByLabel.Clear();
		foreach (KeyValuePair<int, List<Label>> item in labelsByLineNumber)
		{
			foreach (Label item2 in item.Value)
			{
				originalTextsByLabel.Add(item2, item2.Text);
				item2.Text = "";
			}
		}
		lineNumbers = labelsByLineNumber.Keys.ToArray();
		StartIt();
	}

	public override void Update(GameTime gameTime)
	{
		if (labelsByLineNumber.Count <= 0)
		{
			return;
		}
		if (isStarted)
		{
			timePassed += gameTime.ElapsedGameTime.TotalSeconds;
		}
		if (!(timePassed > (double)TimeBetweenUpdates))
		{
			return;
		}
		int key = lineNumbers[currentIndex];
		foreach (Label item in labelsByLineNumber[key])
		{
			item.Text = originalTextsByLabel[item];
		}
		currentIndex++;
		if (currentIndex == lineNumbers.Length)
		{
			Stop();
		}
		timePassed -= TimeBetweenUpdates;
	}
}
