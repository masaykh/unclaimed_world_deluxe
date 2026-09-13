using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Overland;
using WindowSystem;

namespace UWGame.ClientSide.Interface.World_map;

internal class SiteMarker : UIComponent
{
	public enum MarkerType
	{
		Tall,
		Short
	}

	public ImageButton btSite;

	public Label lblName;

	public Image imRadio;

	public SiteMarker(GUIManager gui, Site site, Site playSite, ClickHandler siteClickAction, MarkerType markerType, int order)
		: base(gui)
	{
		RenderType = RenderType.CRTAndLCD;
		Width = 190;
		OrderByTag1 = order;
		int num = 0;
		ImageButtonType type;
		if (markerType == MarkerType.Short)
		{
			Height = 33;
			type = ImageButtonType.SiteMarkerShortPin;
		}
		else
		{
			Height = 45;
			type = ImageButtonType.SiteMarkerTallPin;
		}
		double num2 = ((site == playSite) ? 0.0 : The.Sim.World.GetAirDistance(playSite.Coords, site.Coords));
		StringBuilder stringBuilder = new StringBuilder();
		Common.AppendLine(stringBuilder, site.Name);
		Common.AppendDivider(stringBuilder);
		stringBuilder.Append("Distance: ");
		Common.AppendLine(stringBuilder, $"{num2:N1} km");
		btSite = new ImageButton(guiManager);
		if (site.Allegiances.Contains(The.InGameUI.UIAllegiance))
		{
			Common.AppendLine(stringBuilder, "This is where we are.");
			Common.AppendLine(stringBuilder, "Click to view options and details.");
			btSite.InitWithIcon(type, "HUD_icon_structure", hasCheckedState: false);
			btSite.SetIconTooltip(stringBuilder.ToString());
			btSite.SetIconTint(Color.LightGreen);
			btSite.Tag1 = site;
			btSite.SetIconClick(siteClickAction);
		}
		else
		{
			Common.AppendLine(stringBuilder, "Click to view options and details.");
			btSite.Init(type);
			btSite.Tag1 = site;
		}
		btSite.ToolTip = stringBuilder.ToString();
		Add(btSite);
		btSite.X = 17;
		btSite.Y = num;
		btSite.Click += siteClickAction;
		imRadio = new Image(guiManager);
		imRadio.SetSkinLocation(SkinState.Normal, guiManager.GUISpriteSheet.GetSourceRectangle("antenna_icon"), Color.Yellow, Color.Yellow);
		imRadio.ResizeControlToFitImage();
		Add(imRadio);
		imRadio.X = 0;
		imRadio.Y = num;
		lblName = new Label(guiManager);
		Add(lblName);
		lblName.Init(Label.LabelType.LCDRadioBannerTinted);
		lblName.TintLabelBackground("#6E8284".ColorFromHex());
		lblName.X = btSite.Right - 2;
		lblName.Y = num + 4;
	}

	public void UpdateMarker(bool canCommunicate, bool isStart, bool isDestination, string siteName, bool showLabel)
	{
		btSite.Enabled = true;
		string text;
		Color value;
		if (canCommunicate)
		{
			text = "Communication is established with this location.";
			value = Color.Yellow;
		}
		else
		{
			text = "No communication with this location. Both locations need a functioning radio or satellite station.";
			value = Color.Red;
		}
		imRadio.SetSkinLocation(SkinState.Normal, guiManager.GUISpriteSheet.GetSourceRectangle("antenna_icon"), value, value);
		imRadio.ResizeControlToFitImage();
		imRadio.ToolTip = text;
		if (isStart)
		{
			lblName.TintLabelBackground("#3B6C75".ColorFromHex());
		}
		else if (isDestination)
		{
			lblName.TintLabelBackground("#735538".ColorFromHex());
		}
		else
		{
			lblName.TintLabelBackground("#6E8284".ColorFromHex());
		}
		lblName.Text = siteName;
		lblName.FitToText();
		lblName.Visible = showLabel;
	}
}
