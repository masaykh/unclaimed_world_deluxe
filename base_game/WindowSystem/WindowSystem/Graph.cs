using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RoundLineCode;

namespace WindowSystem;

public class Graph : UIComponent
{
	private Label lblYAxisMax;

	private Label lblYAxisMin;

	private Label lblXAxisMax;

	private Label lblXAxisMin;

	private Image imCanvas;

	private RenderTarget2D renderTarget;

	private PrimitiveBatch primitiveBatch;

	private RoundLineManager RoundLineManager;

	private Matrix viewProj;

	private List<Legend> unusedLegends = new List<Legend>();

	private Dictionary<object, Legend> legendsInUse = new Dictionary<object, Legend>();

	private List<object> legendsToDraw = new List<object>();

	private bool isAddingLegends;

	private float lineRadius = 1f;

	public const float DefaultLineRadius = 1f;

	private string roundLineTechniqueName = "Standard";

	private float minX;

	private float maxX;

	private float xInterval;

	private float yInterval;

	private float chartWidth;

	private float chartHeight;

	private float tickSpacing;

	private float minY;

	private float maxY;

	public Color AxisColor = Color.SlateGray;

	private int axisPaddingRight = 20;

	private int axisPaddingTop = 20;

	private int axisPaddingBottom = 60;

	private int axisPaddingLeft = 60;

	private Vector2? previousPlotPoint;

	private Color plotColor;

	private List<RoundLine> roundLines = new List<RoundLine>();

	private bool useFixedYRanges;

	private const int legendIconSpacing = 4;

	private const int legendStartX = 24;

	private const int legendLineHeight = 20;

	public const double epsilon = 1E-05;

	public const float floatEpsilon = 1E-05f;

	public int AxisPaddingRight
	{
		get
		{
			return axisPaddingRight;
		}
		set
		{
			axisPaddingRight = value;
			UpdateCanvasSize();
		}
	}

	public int AxisPaddingTop
	{
		get
		{
			return axisPaddingTop;
		}
		set
		{
			axisPaddingTop = value;
			UpdateCanvasSize();
		}
	}

	public int AxisPaddingBottom
	{
		get
		{
			return axisPaddingBottom;
		}
		set
		{
			axisPaddingBottom = value;
			UpdateCanvasSize();
		}
	}

	public int AxisPaddingLeft
	{
		get
		{
			return axisPaddingLeft;
		}
		set
		{
			axisPaddingLeft = value;
			UpdateCanvasSize();
		}
	}

	public event Action LegendClicked;

	public Graph(GUIManager guiManager, int canvasWidth, int canvasHeight)
		: base(guiManager)
	{
		renderTarget = new RenderTarget2D(guiManager.Game.GraphicsDevice, canvasWidth, canvasHeight);
		primitiveBatch = new PrimitiveBatch(guiManager.ScreenWidth, guiManager.ScreenHeight, guiManager.Game.GraphicsDevice, canvasWidth, canvasHeight);
		viewProj = primitiveBatch.Projection;
		RoundLineManager = new RoundLineManager();
		RoundLineManager.LoadContent(guiManager.Game.GraphicsDevice, guiManager.Game.Content);
		imCanvas = new Image(guiManager);
		imCanvas.Texture = renderTarget;
		Add(imCanvas);
		imCanvas.Position = new Point(0, 0);
		imCanvas.Width = canvasWidth;
		imCanvas.Height = canvasHeight;
		imCanvas.ScaleImageToSizeOfControl = false;
		Width = canvasWidth;
		Height = canvasHeight;
		UpdateCanvasSize();
		lblYAxisMax = new Label(guiManager);
		Add(lblYAxisMax);
		lblYAxisMax.Init(Label.LabelType.LCDNormal);
		lblYAxisMin = new Label(guiManager);
		Add(lblYAxisMin);
		lblYAxisMin.Init(Label.LabelType.LCDNormal);
		lblYAxisMin.Text = "0";
		lblYAxisMin.FitToText();
		PositionYAxisMinLabel();
		int yPosToCenterTo = (int)((float)axisPaddingTop + chartHeight + 10f);
		lblXAxisMax = new Label(guiManager);
		Add(lblXAxisMax);
		lblXAxisMax.Init(Label.LabelType.LCDNormal);
		lblXAxisMax.CenterThisVertically(yPosToCenterTo);
		lblXAxisMin = new Label(guiManager);
		Add(lblXAxisMin);
		lblXAxisMin.Init(Label.LabelType.LCDNormal);
		lblXAxisMin.CenterThisVertically(yPosToCenterTo);
		for (int i = 0; i < 15; i++)
		{
			Legend item = new Legend(guiManager, this);
			unusedLegends.Add(item);
		}
	}

	public new void Destroy()
	{
		renderTarget.Dispose();
	}

	private void PositionYAxisMinLabel()
	{
		lblYAxisMin.X = axisPaddingLeft - lblYAxisMin.Width - 2;
		lblYAxisMin.CenterThisVertically((int)((float)axisPaddingTop + chartHeight));
	}

	private void UpdateCanvasSize()
	{
		chartWidth = Width - axisPaddingLeft - axisPaddingRight;
		chartHeight = Height - axisPaddingTop - axisPaddingBottom;
	}

	public void LegendCheckBox_Click(UIComponent sender, EventArgs e)
	{
		if (this.LegendClicked != null)
		{
			this.LegendClicked();
		}
	}

	private void SetXAxisLabels(string xMinLabel, string xMaxLabel)
	{
		lblXAxisMin.Text = xMinLabel;
		lblXAxisMin.FitToText();
		lblXAxisMin.X = axisPaddingLeft;
		lblXAxisMax.Text = xMaxLabel;
		lblXAxisMax.FitToText();
		lblXAxisMax.X = (int)((float)axisPaddingLeft + chartWidth - (float)lblXAxisMax.Width);
	}

	private void SetYAxisLabels(string yMinLabel, string yMaxLabel)
	{
		lblYAxisMin.Text = yMinLabel;
		lblYAxisMin.FitToText();
		PositionYAxisMinLabel();
		lblYAxisMax.Text = yMaxLabel;
		lblYAxisMax.FitToText();
		PositionYAxisMaxLabel(axisPaddingTop);
	}

	public void SetFixedRanges(float minX, float maxX, float minY, float maxY, float tickSpacing, string xMinLabel, string xMaxLabel, string yMinLabel = null, string yMaxLabel = null)
	{
		useFixedYRanges = true;
		this.minY = minY;
		this.maxY = maxY;
		this.minX = minX;
		this.maxX = maxX;
		xInterval = this.maxX - this.minX;
		yInterval = this.maxY - this.minY;
		this.tickSpacing = tickSpacing;
		SetYAxisLabels(yMinLabel, yMaxLabel);
		SetXAxisLabels(xMinLabel, xMaxLabel);
	}

	public void SetRanges(float minX, float maxX, float minY, float maxY, string xMinLabel, string xMaxLabel)
	{
		useFixedYRanges = false;
		ChooseAxisDivision(maxY, minY);
		this.minX = minX;
		this.maxX = maxX;
		xInterval = this.maxX - this.minX;
		yInterval = this.maxY - this.minY;
		lblYAxisMin.Text = "0";
		lblYAxisMin.FitToText();
		PositionYAxisMinLabel();
		SetXAxisLabels(xMinLabel, xMaxLabel);
	}

	public void BeginDraw()
	{
		guiManager.Game.GraphicsDevice.SetRenderTarget(renderTarget);
		guiManager.Game.GraphicsDevice.Clear(Color.Transparent);
	}

	public void EndDraw(RenderTarget2D previousRenderTarget = null)
	{
		guiManager.Game.GraphicsDevice.SetRenderTarget(previousRenderTarget);
		imCanvas.Texture = renderTarget;
	}

	public void BeginConnectedPlot(Color color, float lineRadius)
	{
		plotColor = color;
		this.lineRadius = lineRadius;
		roundLines.Clear();
		previousPlotPoint = null;
	}

	public void AddVertex(float x, float y, Color color)
	{
		primitiveBatch.AddVertex(new Vector2(x, y), color);
	}

	public void EndPlot()
	{
		if (roundLines.Count > 0)
		{
			RoundLineManager.BlurThreshold = RoundLineManager.ComputeBlurThreshold(lineRadius, viewProj, imCanvas.Width);
			float time = 0f;
			RoundLineManager.Draw(roundLines, lineRadius, plotColor, viewProj, time, roundLineTechniqueName);
		}
		else if (previousPlotPoint.HasValue)
		{
			Disc roundLine = new Disc(previousPlotPoint.Value);
			RoundLineManager.Draw(roundLine, lineRadius, plotColor, viewProj, 0f, roundLineTechniqueName);
		}
	}

	public void AddDataPoint(float x, float y)
	{
		x = Math.Max(x, 0f);
		y = Math.Max(y, 0f);
		GetVertexPosFromValues(x, y, out var xPos, out var yPos);
		Vector2 vector = new Vector2(xPos, yPos);
		if (previousPlotPoint.HasValue)
		{
			roundLines.Add(new RoundLine(previousPlotPoint.Value, vector));
		}
		previousPlotPoint = vector;
	}

	private void GetVertexPosFromValues(float x, float y, out float xPos, out float yPos)
	{
		xPos = GetVertexXPosFromValue(x);
		yPos = GetVertexYPosFromValue(y);
	}

	private float GetVertexXPosFromValue(float x)
	{
		x = Util.Clamp(x, minX, maxX);
		return (float)axisPaddingLeft + (x - minX) / xInterval * chartWidth;
	}

	private float GetVertexYPosFromValue(float y)
	{
		y = Util.Clamp(y, minY, maxY);
		float num = (y - minY) / yInterval * chartHeight;
		return (float)axisPaddingTop + chartHeight - num;
	}

	public void DrawPoint(Vector2 where, Color color)
	{
		primitiveBatch.AddVertex(where, color);
		where.X -= 1f;
		primitiveBatch.AddVertex(where, color);
		where.X += 2f;
		primitiveBatch.AddVertex(where, color);
		where.X -= 1f;
		where.Y -= 1f;
		primitiveBatch.AddVertex(where, color);
		where.Y += 2f;
		primitiveBatch.AddVertex(where, color);
	}

	public void DrawAxis()
	{
		DrawAxisAndBorder();
		float? maxValue = DrawAxisDivisionLines();
		if (!useFixedYRanges)
		{
			PositionAxisLabels(maxValue);
		}
	}

	private float? DrawAxisDivisionLines()
	{
		primitiveBatch.Begin(PrimitiveType.LineList);
		float? num = null;
		float num2 = minY;
		float? result = null;
		for (; IsLessThanOrEqual(num2, maxY); num2 += tickSpacing)
		{
			num = GetVertexYPosFromValue(num2);
			primitiveBatch.AddVertex(new Vector2(axisPaddingLeft, num.Value), AxisColor);
			primitiveBatch.AddVertex(new Vector2((float)axisPaddingLeft + chartWidth, num.Value), AxisColor);
			result = num2;
		}
		primitiveBatch.End();
		return result;
	}

	private void PositionAxisLabels(float? maxValue)
	{
		if (maxValue.HasValue)
		{
			Add(lblYAxisMax);
			int roundingPosition;
			string text = RoundSignificantDigits(maxValue.Value, 4, out roundingPosition).ToString();
			lblYAxisMax.Text = text;
			lblYAxisMax.FitToText();
			int yPos = (int)GetVertexYPosFromValue(maxValue.Value);
			PositionYAxisMaxLabel(yPos);
		}
		else
		{
			Remove(lblYAxisMax);
		}
	}

	private void PositionYAxisMaxLabel(int yPos)
	{
		lblYAxisMax.CenterThisVertically(yPos);
		lblYAxisMax.X = axisPaddingLeft - lblYAxisMax.Width - 2;
	}

	private void DrawAxisAndBorder()
	{
		primitiveBatch.Begin(PrimitiveType.LineStrip);
		primitiveBatch.AddVertex(new Vector2(axisPaddingLeft, axisPaddingTop), AxisColor);
		primitiveBatch.AddVertex(new Vector2(axisPaddingLeft, (float)axisPaddingTop + chartHeight), AxisColor);
		primitiveBatch.AddVertex(new Vector2(Width - axisPaddingRight, (float)axisPaddingTop + chartHeight), AxisColor);
		primitiveBatch.AddVertex(new Vector2(Width - axisPaddingRight, axisPaddingTop), AxisColor);
		primitiveBatch.AddVertex(new Vector2(axisPaddingLeft, axisPaddingTop), AxisColor);
		primitiveBatch.End();
	}

	public void BeginAddingLegends()
	{
		isAddingLegends = true;
	}

	public void AddLegend(string name, Color color, object key)
	{
		if (!isAddingLegends)
		{
			throw new Exception("Call Begin first!");
		}
		legendsToDraw.Add(key);
		if (!legendsInUse.ContainsKey(key))
		{
			Legend legend;
			if (unusedLegends.Count == 0)
			{
				legend = new Legend(guiManager, this);
			}
			else
			{
				legend = unusedLegends[unusedLegends.Count - 1];
				unusedLegends.RemoveAt(unusedLegends.Count - 1);
			}
			legend.CheckBox.Tag1 = key;
			legend.CheckBox.IsChecked = true;
			legendsInUse.Add(key, legend);
			legend.CheckBox.Text = name;
			legend.CheckBox.BackColor = color;
			Add(legend.CheckBox);
		}
	}

	public void EndAddingLegends()
	{
		isAddingLegends = false;
		List<CheckBox> list = null;
		foreach (KeyValuePair<object, Legend> item in legendsInUse)
		{
			if (!legendsToDraw.Contains(item.Key))
			{
				Util.AddToList(ref list, item.Value.CheckBox);
			}
		}
		if (list != null)
		{
			foreach (CheckBox item2 in list)
			{
				Remove(item2);
				legendsInUse.Remove(item2.Tag1);
			}
		}
		legendsToDraw.Clear();
	}

	public void DrawLegends()
	{
		int num = 24;
		int num2 = (int)((float)axisPaddingTop + chartHeight + 30f);
		foreach (KeyValuePair<object, Legend> item in legendsInUse)
		{
			if (item.Value.CheckBox.Width + num > Width - 6)
			{
				num2 += 20;
				num = 24;
			}
			item.Value.CheckBox.X = num;
			item.Value.CheckBox.CenterThisVertically(num2);
			num = item.Value.CheckBox.Right + 8;
		}
	}

	public void Clear()
	{
		Remove(lblYAxisMax);
		BeginDraw();
		DrawAxisAndBorder();
		EndDraw();
	}

	private void ChooseAxisDivision(float max, float min)
	{
		NiceScale niceScale = new NiceScale(min, max);
		tickSpacing = (float)niceScale.tickSpacing;
		minY = (float)niceScale.niceMin;
		maxY = (float)niceScale.niceMax;
	}

	private static double RoundSignificantDigits(double value, int significantDigits, out int roundingPosition)
	{
		roundingPosition = 0;
		if (IsEqual(value, 0.0))
		{
			roundingPosition = significantDigits - 1;
			return 0.0;
		}
		if (double.IsNaN(value))
		{
			return double.NaN;
		}
		if (double.IsPositiveInfinity(value))
		{
			return double.PositiveInfinity;
		}
		if (double.IsNegativeInfinity(value))
		{
			return double.NegativeInfinity;
		}
		if (significantDigits < 1 || significantDigits > 15)
		{
			throw new ArgumentOutOfRangeException("significantDigits", value, "The significantDigits argument must be between 1 and 15.");
		}
		roundingPosition = significantDigits - 1 - (int)Math.Floor(Math.Log10(Math.Abs(value)));
		if (roundingPosition > 0 && roundingPosition < 16)
		{
			return Math.Round(value, roundingPosition, MidpointRounding.AwayFromZero);
		}
		double num = Math.Pow(10.0, Math.Ceiling(Math.Log10(Math.Abs(value))));
		return Math.Round(value / num, significantDigits, MidpointRounding.AwayFromZero) * num;
	}

	public static bool IsLessThanOrEqual(double valueToTest, double valueToTestWith)
	{
		if (IsEqual(valueToTest, valueToTestWith))
		{
			return true;
		}
		return valueToTest < valueToTestWith;
	}

	public static bool IsEqual(double d1, double d2)
	{
		if (d1 < d2 + 1E-05)
		{
			return d1 > d2 - 1E-05;
		}
		return false;
	}

	public bool IsChecked(object p)
	{
		if (legendsInUse.TryGetValue(p, out var value))
		{
			return value.CheckBox.IsChecked;
		}
		return false;
	}
}
