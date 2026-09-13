using System.Collections.Generic;
using UWGame.ClientSide.HelpTopics;
using UWGame.ClientSide.Interface.Layout;
using WindowSystem;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_6.Data;

public class TutorialLoader
{
	public static List<HelpTopic> Init()
	{
		List<HelpTopic> list = new List<HelpTopic>();
		list.Add(new HelpTopic
		{
			KeyName = "tutorialClayScenario6_1",
			Name = "GUIDE #1 - BUILD A KILN",
			FlowElements = new LayoutElement[7]
			{
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = Label.ToLabel("GUIDE #1 - BUILD A KILN", "#COLORHEADER") + " \n \n -Scout the surroundings until you find 'Stones'. \n -In the 'Production Manager', click the 'ATTAINABLE' button."
					}
				},
				new LayoutElement
				{
					Image = new ImageElement
					{
						Image = "tutClayPit_attainableButton"
					}
				},
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = " \n -Then, find 'Kiln' in the Production Manager, click the name, then click the 'Pin' button. RMB- click to close the other windows. \n-Place the window where you like and resize it so that you can see which materials are needed to make the Kiln. \n-Gather the needed materials (clay and stone)."
					}
				},
				new LayoutElement
				{
					Image = new ImageElement
					{
						Image = "tutClayPit_kilnTooltip"
					}
				},
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = " \n -When the materials have been gathered, a Hammer button appears in the Kiln window. Click the Hammer button and place the kiln near the camp. \n \n"
					}
				},
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = Label.ToLabel("HINTS:////////////////////////////////////////////////", "#COLORTYPE") + "\n -Open the Resource Viewer next to the Mini-map and check the box where it says 'Clay'. This will indicate all the locations where clay has been found."
					}
				},
				new LayoutElement
				{
					Image = new ImageElement
					{
						Image = "tutClayPit_viewClayResources"
					}
				}
			}
		});
		list.Add(new HelpTopic
		{
			KeyName = "tutorialClayScenario6_2",
			Name = "GUIDE #2 - MAKE 3X MUDBRICKS IN THE KILN",
			FlowElements = new LayoutElement[2]
			{
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = Label.ToLabel("GUIDE #2 - MAKE 3X MUDBRICKS IN THE KILN.", "#COLORHEADER") + "\n \n -Gather 10X 'Clay' \n -Gather 10X 'Firewood' \n -Make 3X 'Mudbricks (wet)' \n -Then order 3X 'Mudbricks'. \n "
					}
				},
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = Label.ToLabel("HINTS:////////////////////////////////////////////////", "#COLORTYPE") + " \n -Shaping wet mudbricks requires the tool 'Brick mold'. To speed up production, make 2 additional Brick molds so that the workers don't need to share the tool. The brick molds are made from sticks. \n-Mudbricks can also be made without a kiln since they will dry on their own, but it takes much longer."
					}
				}
			}
		});
		list.Add(new HelpTopic
		{
			KeyName = "tutorialClayScenario6_3",
			Name = "GUIDE #3 - PLACE MUDBRICKS IN PORT. STORE 40X MUDBRICKS",
			FlowElements = new LayoutElement[4]
			{
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = Label.ToLabel("GUIDE #3 - PLACE MUDBRICKS IN PORT.", "#COLORHEADER") + " \n \n STORE 40X MUDBRICKS \n \n -Click on the structure Canopy port, click TRADE, find mudbricks and check the box:"
					}
				},
				new LayoutElement
				{
					Image = new ImageElement
					{
						Image = "tutClayPit_tradeAllowMudbricks"
					}
				},
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = " \n -Gather more clay and firewood, while you bit by bit, shape 40 X 'Mudbricks (wet)' and make 40 X 'Mudbricks' from them. \n \nOur goal is to store 40 mudbricks in our camp! \n \n "
					}
				},
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = Label.ToLabel("HINTS:////////////////////////////////////////////////", "#COLORTYPE") + " \n -Build more kilns to speed up production as you see fit. \n -Attempt to make the kilns produce mudbricks constantly. \n -Use the Task manager screen to keep track of your progress. It will display a warning if you order too many at a time. \n-In the Task Manager you can increase priority on HAULING; this will make camp members carry items to storage quicker. Look in the dropdown menu at the top for the 'Haul to storage' task type."
					}
				}
			}
		});
		list.Add(new HelpTopic
		{
			KeyName = "tutorialClayScenario6_4",
			Name = "GUIDE #4 - SELL MUDBRICKS. BUY OIL TUBERS.",
			FlowElements = new LayoutElement[12]
			{
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = Label.ToLabel("GUIDE #4 - SELL MUDBRICKS. BUY OIL TUBERS.", "#COLORHEADER") + "\n \n First, click on the structure Canopy port and confirm that it contains 40X mudbricks in its Trade area. If not, click TRADE, find 'mudbricks' and check the box, then wait till the mudbricks have been placed:"
					}
				},
				new LayoutElement
				{
					Image = new ImageElement
					{
						Image = "tutClayPit_tradeAllowMudbricks_SMALL"
					}
				},
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = " \n -When the bricks have been placed there, click the 'MISSIONS' button:"
					}
				},
				new LayoutElement
				{
					Image = new ImageElement
					{
						Image = "comm_button_out"
					}
				},
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = " \n -Click 'NEW RUN' \n -then click the 'Globe' button next to 'START':"
					}
				},
				new LayoutElement
				{
					Image = new ImageElement
					{
						Image = "tutRubber_startRun"
					}
				},
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = " \n -Click the 'Tellus' button, \n -then SELECT:"
					}
				},
				new LayoutElement
				{
					Image = new ImageElement
					{
						Image = "tutClayPit_selectTellus"
					}
				},
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = " \n \n Back on the 'CREATE NEW RUN' screen, \n -click the 'Globe' button next to 'DESTINATION'. \n -Click 'Clay Pit', \n-then SELECT \n \n -Now select 'Small barge (Hired)' from the drop-down menu next to 'TRANSPORT'. \n \n -Next to 'Clay Pit', select 'ADD ACTION', \n -then click 'SELL' \n -Find 'Mudbricks' and drag the slider to sell all. \n -Next to 'Tellus', select 'ADD ACTION' \n -then click 'BUY' \n -Find 'Common oil tubers' and order as many as you can afford. This depends on the amount of mud bricks you sell! \n -Click 'START RUN'."
					}
				},
				new LayoutElement
				{
					Image = new ImageElement
					{
						Image = "tutClayPit_createRun"
					}
				},
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = " \n \n On the 'MISSIONS' screen, you can see ETA (estimated time of arrival) for the Barge. \n \n You can also click the 'WORLD MAP' screen to see how far the barge has come on its journey:"
					}
				},
				new LayoutElement
				{
					Image = new ImageElement
					{
						Image = "tutClayPit_bargeMap"
					}
				}
			}
		});
		list.Add(new HelpTopic
		{
			KeyName = "tutorialClayScenario6_5",
			Name = "GUIDE #5 - CONTINUE MAKING AND TRADING MUDBRICKS",
			FlowElements = new LayoutElement[5]
			{
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = Label.ToLabel("GUIDE #5 - CONTINUE MAKING AND TRADING MUDBRICKS", "#COLORHEADER") + "\n \n Make more mudbricks. When the oil tubers arrive, make food for the workers. \n \n"
					}
				},
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = Label.ToLabel("HINTS:////////////////////////////////////////////////", "#COLORTYPE") + "\n -In the 'Production Manager', 'Track' Firewood and other important materials with a color marker to better keep an eye on your supply:"
					}
				},
				new LayoutElement
				{
					Image = new ImageElement
					{
						Image = "tutClayPit_trackFirewood"
					}
				},
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = " \n -The 'Task Manager' screen gives warnings when you run out of fuel. \n -The 'Task Manager' also tells you if you order too many tasks at a time, which can cause bottlenecks. \n -Keep an eye on the tools needed for your tasks - if you don't have enough campfires for example, cooking will take a long time. \n -Experiment with the 'padlock' button which appears next to the order controls in the Gather window and Production Manager. This button allows you to set standing orders and automate production, reducing the micromanagement."
					}
				},
				new LayoutElement
				{
					Image = new ImageElement
					{
						Image = "tutClayPit_standingProduction"
					}
				}
			}
		});
		list.Add(new HelpTopic
		{
			KeyName = "tutorialClayScenario6_6",
			Name = "GUIDE #6 - SALVAGE 'ABATIS'. BUILD A SETTLEMENT",
			FlowElements = new LayoutElement[5]
			{
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = Label.ToLabel("GUIDE #6 - SALVAGE 'ABATIS'. BUILD A SETTLEMENT", "#COLORHEADER") + "\n \n -Click on one of the abatis that block the cliff opening, select 'SALVAGE'. \n -Fight off any patricians near Blue Creek to the north. \n -Use the resources you find to build up your settlement. \n \nThis concludes the tutorial. More hints below. Good luck! \n \n"
					}
				},
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = Label.ToLabel("HINTS:////////////////////////////////////////////////", "#COLORTYPE") + " \n -Examine the river banks and set up fish weirs. \n -Build smoke ovens for smoking the fish. \n -Dig bog ore and set up a smithy. Get a blacksmith from Tellus to increase productivity. \n -Make tools to sell, thereby financing the colony's growth: \n -Raise your colony's conditions within all the areas FOOD, SECURITY and COMFORT too keep your settlers happy."
					}
				},
				new LayoutElement
				{
					Image = new ImageElement
					{
						Image = "tutClayPit_standingProduction"
					}
				},
				new LayoutElement
				{
					Text = new TextElement
					{
						Text = Label.ToLabel("HINT: Standing Orders/////////////////", "#COLORTYPE") + "\n Click the padlock icon next to the order controls in the Production Manager and the Gather window (see images). \n This allows you to automate gathering and production: When your stock falls below the specified amount, the colonists will automatically resume production or gathering."
					}
				},
				new LayoutElement
				{
					Image = new ImageElement
					{
						Image = "tutClayPit_standingGather"
					}
				}
			}
		});
		return list;
	}
}
