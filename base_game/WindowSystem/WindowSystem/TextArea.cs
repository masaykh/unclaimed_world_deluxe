using System.Collections.Generic;

namespace WindowSystem;

public class TextArea : ListBox
{
	public enum TextAreaType
	{
		HUD
	}

	private string text;

	private const char tagMarker = '§';

	private static char[] spaces = new char[1] { ' ' };

	public string Text
	{
		get
		{
			return text;
		}
		set
		{
			text = value;
			RefreshText();
		}
	}

	public override int Width
	{
		get
		{
			return base.Width;
		}
		set
		{
			if (base.Width != value)
			{
				base.Width = value;
				RefreshText();
				RefreshMargins();
			}
		}
	}

	public new int Height
	{
		get
		{
			return base.Height;
		}
		set
		{
			base.Height = value;
			RefreshMargins();
			RefreshText();
		}
	}

	public TextArea(GUIManager gui, ListBoxType type)
		: base(gui, type)
	{
		base.CanHaveFocus = false;
		text = "";
		base.CanGrowInHeight = true;
	}

	public void RefreshText()
	{
		Clear();
		List<string> list = BreakTextToEntries();
		if (list == null)
		{
			return;
		}
		BeginAddingEntries();
		foreach (string item in list)
		{
			AddEntry(item);
		}
		EndAddingEntries();
	}

	private void SplitTagFreeBlockIntoWords(string text, List<string> words)
	{
		string[] array = text.Split(' ');
		if (array.Length == 1)
		{
			_ = array[0] == "";
		}
		words.AddRange(array);
	}

	private List<string> SplitIntoWords(string text)
	{
		List<string> list = new List<string>();
		int? num = null;
		int num2 = 0;
		bool flag = false;
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] == '§')
			{
				if (!num.HasValue)
				{
					if (flag)
					{
						int num3 = i - num2;
						if (num3 > 0)
						{
							SplitBlock(text, list, num2, num3);
							num2 = i;
						}
					}
					num = i;
				}
				else
				{
					num = null;
					flag = false;
				}
			}
			else if (!num.HasValue)
			{
				flag = true;
			}
		}
		if (num2 == 0 || num2 < text.Length - 1)
		{
			SplitBlock(text, list, num2, text.Length - num2);
		}
		return list;
	}

	private void SplitBlock(string text, List<string> words, int beginningOfBlock, int length)
	{
		string text2 = text.Substring(beginningOfBlock, length);
		int num = text2.LastIndexOf('§');
		int count = words.Count;
		string text3 = null;
		if (num >= 0)
		{
			text3 = text2.Substring(0, num + 1);
			text2 = text2.Substring(num + 1, text2.Length - num - 1);
		}
		if (text2 != "")
		{
			SplitTagFreeBlockIntoWords(text2, words);
			if (num >= 0)
			{
				words[count] = text3 + words[count];
			}
		}
		else
		{
			words.Add(text3);
		}
	}

	private List<string> BreakTextToEntries()
	{
		if (string.IsNullOrEmpty(Text))
		{
			return null;
		}
		_ = Text;
		List<string> list = SplitIntoWords(Text);
		List<string> list2 = new List<string>();
		string text = "";
		int num = 0;
		int num2 = Width - base.HMargin * 2 - GapAndScrollBar;
		for (int i = 0; i < list.Count; i++)
		{
			string text2 = list[i];
			text2.StartsWith("§");
			string text4;
			if (text2.StartsWith("\n"))
			{
				text2 = text2.Replace("\n", "");
				list2.Add(text);
				string text3 = ((!(text2 != "")) ? text2 : (text2 + " "));
				text4 = text3;
				num = Label.GetParsedLineWidth(text4, guiManager, base.SpriteFont);
			}
			else
			{
				string text3 = text2 + " ";
				text4 = text + text3;
				int parsedLineWidth = Label.GetParsedLineWidth(text3, guiManager, base.SpriteFont);
				num += parsedLineWidth;
			}
			if (num < num2 || text == "")
			{
				text = text4;
			}
			else
			{
				list2.Add(text);
				text = text2 + " ";
				num = Label.GetParsedLineWidth(text, guiManager, base.SpriteFont);
			}
			if (text.TrimEnd(spaces).EndsWith("\n"))
			{
				text = text.Replace("\n", "");
				text.TrimEnd(spaces);
				list2.Add(text);
				text = "";
				num = 0;
			}
		}
		text = text.TrimEnd(spaces);
		list2.Add(text);
		return list2;
	}
}
