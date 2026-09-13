using UWGame.ClientSide.Interface;
using UWGame.ClientSide.Interface.HUD_Windows;
using WindowSystem;

namespace UWGame.ClientSide.MainMenu.Credits;

public class CreditsPanel : HUDWindow
{
	private const int minHeight = 26;

	private const int defaultHeight = 260;

	private TextArea text;

	public CreditsPanel(CommonInterface intf)
		: base(intf, 400, 600, hasSurface: true, hasCloseButton: true)
	{
		HideOnRightClick = false;
		DisplayWindow.CenterWindow();
		DisplayWindow.Resizable = false;
		DisplayWindow.Level = Level.Bottom;
		DisplayWindow.Show();
		text = new TextArea(gui, ListBoxType.HUDAndLCD);
		DisplayWindow.Add(text);
		text.Init(Label.LabelType.HUDWindow);
		text.CanGrowInHeight = false;
		text.X = 12;
		text.Y = base.TitleBarHeight;
		text.Width = DisplayWindow.Width - 24;
		text.Height = DisplayWindow.Height - text.Y - 12;
		text.Text = "MADE BY REFACTORED GAMES \n/////////////////////////// \n \nLEAD PROGRAMMING: Lars Pedersen \n \nART DIRECTION: Morten Pedersen \n \nPROJECT MANAGEMENT ON PROTOTYPE: \nHans von Knut Skovfoged \n \nPROGRAMMING: Mark Lorenzen \n \nJUNIOR PROGRAMMER: Andreas Broqvist \n \n3D MODEL PIPELINE: Tobias Jacobsen \n \nINTERNS// \n \nMUSIC: Martin Hasseldam, Jesper Lundager \n \nPROGRAMMING: Finn Axel Simon Broman, Martin Juul Petersen, Jakob Sigvard, Olivér Árnits \n \nSCRIPTING: Thor-Bjørn Böhme, Benjamin Størup Olsen \n \n3D ART: Anchelika Skjødt \n \nGAME DESIGN: Stefan Ort Mortensen, Nicklas Andersen, Aleksander Fjellvang \n \nLEVEL DESIGN: Matteo Martinelli \n \nSOUND DESIGN: Morten Skouboe \n \nIN-HOUSE TESTING: Lukasz Maliglowka, Daniel Voss, Tommy Christensen \n \nTHANK YOU: Lau Korsgaard (GAME DESIGN), Chris Correia (VFX), Calvin Riedy, Samuel Blantz, Bjarke Larsen (WRITING), Carl Trägårdh (3D), Leo Claxton (SOUND), Gavin 'DrTssha' Craig and Daniel Pena \n \n";
	}
}
