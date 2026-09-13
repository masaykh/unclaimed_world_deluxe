using System.Collections.Generic;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.AllGameData;

public class ToolsLoader
{
	private const float HighDegrade = 0.05f;

	private const float MediumDegrade = 0.02f;

	private const float LowDegrade = 0.005f;

	private const float NoDegrade = 0f;

	public static List<ProcessToolSet> InitProcessToolSets()
	{
		ToolAlternatives toolAlternatives = new ToolAlternatives();
		toolAlternatives.Tools = new Tool[8]
		{
			new Tool
			{
				UsesToolKeyName = "item:weedingRobotTool",
				ProductivityFactor = 1f,
				DegradePerSecondOfUse = 0f
			},
			new Tool
			{
				UsesToolKeyName = "item:advancedMachete",
				ProductivityFactor = 0.5f,
				DegradePerSecondOfUse = 0f
			},
			new Tool
			{
				UsesToolKeyName = "item:steelMachete",
				ProductivityFactor = 0.3f,
				DegradePerSecondOfUse = 0f
			},
			new Tool
			{
				UsesToolKeyName = "item:improvisedKnife",
				ProductivityFactor = 0.05f,
				DegradePerSecondOfUse = 0f
			},
			new Tool
			{
				UsesToolKeyName = "item:steelKnife",
				ProductivityFactor = 0.1f,
				DegradePerSecondOfUse = 0f
			},
			new Tool
			{
				UsesToolKeyName = "item:advancedKnife",
				ProductivityFactor = 0.15f,
				DegradePerSecondOfUse = 0f
			},
			new Tool
			{
				UsesToolKeyName = "item:steelHandAxe",
				ProductivityFactor = 0.08f,
				DegradePerSecondOfUse = 0f
			},
			new Tool
			{
				UsesToolKeyName = "item:improvisedHandAxe",
				ProductivityFactor = 0.06f,
				DegradePerSecondOfUse = 0f
			}
		};
		ToolAlternatives toolAlternatives2 = toolAlternatives;
		toolAlternatives = new ToolAlternatives();
		toolAlternatives.Tools = new Tool[5]
		{
			new Tool
			{
				UsesToolKeyName = "item:steelHoe",
				ProductivityFactor = 0.6f,
				DegradePerSecondOfUse = 0f
			},
			new Tool
			{
				UsesToolKeyName = "item:farmingHoe",
				ProductivityFactor = 0.5f,
				DegradePerSecondOfUse = 0f
			},
			new Tool
			{
				UsesToolKeyName = "item:weedingRobotTool",
				ProductivityFactor = 1f,
				DegradePerSecondOfUse = 0f
			},
			new Tool
			{
				UsesToolKeyName = "item:steelSpade",
				ProductivityFactor = 0.15f,
				DegradePerSecondOfUse = 0f
			},
			new Tool
			{
				UsesToolKeyName = "item:improvisedSpade",
				ProductivityFactor = 0.1f,
				DegradePerSecondOfUse = 0f
			}
		};
		ToolAlternatives toolAlternatives3 = toolAlternatives;
		toolAlternatives = new ToolAlternatives();
		toolAlternatives.Tools = new Tool[5]
		{
			new Tool
			{
				UsesToolKeyName = "item:flintKnife",
				ProductivityFactor = 0.3f,
				DegradePerSecondOfUse = 0f
			},
			new Tool
			{
				UsesToolKeyName = "item:improvisedKnife",
				ProductivityFactor = 0.65f,
				DegradePerSecondOfUse = 0f
			},
			new Tool
			{
				UsesToolKeyName = "item:steelKnife",
				ProductivityFactor = 0.8f,
				DegradePerSecondOfUse = 0f
			},
			new Tool
			{
				UsesToolKeyName = "item:advancedKnife",
				ProductivityFactor = 0.85f,
				DegradePerSecondOfUse = 0f
			},
			new Tool
			{
				UsesToolKeyName = "item:improvisedTrowel",
				ProductivityFactor = 0.4f,
				DegradePerSecondOfUse = 0f
			}
		};
		ToolAlternatives toolAlternatives4 = toolAlternatives;
		List<ProcessToolSet> list = new List<ProcessToolSet>();
		list.Add(new ProcessToolSet
		{
			KeyName = "toolSetPseudoFarmplot",
			Tools = new ToolAlternatives[3]
			{
				new ToolAlternatives
				{
					Tools = new Tool[2]
					{
						new Tool
						{
							UsesToolKeyName = "structure:smallPlot",
							ProductivityFactor = 0.5f,
							DegradePerSecondOfUse = 0f
						},
						new Tool
						{
							UsesToolKeyName = "structure:largePlot",
							ProductivityFactor = 0.5f,
							DegradePerSecondOfUse = 0f
						}
					}
				},
				toolAlternatives3,
				toolAlternatives2
			}
		});
		list.Add(new ProcessToolSet
		{
			KeyName = "toolSetPseudoGreenhouse",
			Tools = new ToolAlternatives[2]
			{
				new ToolAlternatives
				{
					Tools = new Tool[2]
					{
						new Tool
						{
							UsesToolKeyName = "structure:improvisedGreenhouse",
							ProductivityFactor = 0.6f,
							DegradePerSecondOfUse = 0f
						},
						new Tool
						{
							UsesToolKeyName = "structure:greenhouse",
							ProductivityFactor = 0.7f,
							DegradePerSecondOfUse = 0f
						}
					}
				},
				toolAlternatives4
			}
		});
		list.Add(new ProcessToolSet
		{
			KeyName = "toolSetCarbonTailTraps",
			Tools = new ToolAlternatives[1]
			{
				new ToolAlternatives
				{
					Tools = new Tool[4]
					{
						new Tool
						{
							UsesToolKeyName = "structure:fishTrapCreekSticks",
							ProductivityFactor = 0.6f,
							DegradePerSecondOfUse = 0f
						},
						new Tool
						{
							UsesToolKeyName = "structure:fishTrapCreekNet",
							ProductivityFactor = 0.8f,
							DegradePerSecondOfUse = 0f
						},
						new Tool
						{
							UsesToolKeyName = "structure:fishTrapShoreBasket",
							ProductivityFactor = 0.5f,
							DegradePerSecondOfUse = 0f
						},
						new Tool
						{
							UsesToolKeyName = "structure:fishTrapShoreHoopNet",
							ProductivityFactor = 0.6f,
							DegradePerSecondOfUse = 0f
						}
					}
				}
			}
		});
		list.Add(new ProcessToolSet
		{
			KeyName = "toolSetStreakFinTraps",
			Tools = new ToolAlternatives[1]
			{
				new ToolAlternatives
				{
					Tools = new Tool[1]
					{
						new Tool
						{
							UsesToolKeyName = "structure:fishTrapCoast",
							ProductivityFactor = 0.6f,
							DegradePerSecondOfUse = 0f
						}
					}
				}
			}
		});
		ProcessToolSet processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetButcherTurnip";
		processToolSet.Tools = new ToolAlternatives[2]
		{
			new ToolAlternatives
			{
				Tools = new Tool[2]
				{
					new Tool
					{
						UsesToolKeyName = "item:turnipCracker",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:metalWorkersToolbox",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[6]
				{
					new Tool
					{
						UsesToolKeyName = "item:flintKnife",
						ProductivityFactor = 0.2f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedKnife",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelKnife",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedKnife",
						ProductivityFactor = 0.7f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedMachete",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelHandAxe",
						ProductivityFactor = 0.2f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		ProcessToolSet item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetButcherTwinkler";
		processToolSet.Tools = new ToolAlternatives[2]
		{
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolTag = "cutThinShell",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[6]
				{
					new Tool
					{
						UsesToolKeyName = "item:flintKnife",
						ProductivityFactor = 0.2f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedKnife",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelKnife",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedKnife",
						ProductivityFactor = 0.7f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedMachete",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelHandAxe",
						ProductivityFactor = 0.2f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetPrepareMeal";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolTag = "knife",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetButcherFlesh";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[6]
				{
					new Tool
					{
						UsesToolKeyName = "item:flintKnife",
						ProductivityFactor = 0.2f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedKnife",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelKnife",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedKnife",
						ProductivityFactor = 0.7f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedMachete",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelHandAxe",
						ProductivityFactor = 0.2f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetClimbingRope";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolKeyName = "item:vine",
						ProductivityFactor = 0.2f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetStrongBugNet";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[2]
				{
					new Tool
					{
						UsesToolKeyName = "item:strongBugNet",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:shadeleafResin",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetCatchStinkpup";
		processToolSet.Tools = new ToolAlternatives[3]
		{
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolTag = "fishingHook",
						ProductivityFactor = 0.7f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolKeyName = "item:neonHornetsLive",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[4]
				{
					new Tool
					{
						UsesToolKeyName = "item:advancedString",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:superconductingWire",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:rawhideString",
						ProductivityFactor = 0.2f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:cottonString",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetFishMinnows";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[2]
				{
					new Tool
					{
						UsesToolKeyName = "item:strongBugNet",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:ursinixVenomGland",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetFishAlabasterRay";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[4]
				{
					new Tool
					{
						UsesToolKeyName = "item:improvisedBasicSpear",
						ProductivityFactor = 0.2f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedFlintSpear",
						ProductivityFactor = 0.35f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedGoodSpear",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:ironSpear",
						ProductivityFactor = 0.7f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetFishStreakFin";
		processToolSet.Tools = new ToolAlternatives[3]
		{
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolTag = "fishingHook",
						ProductivityFactor = 0.7f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[2]
				{
					new Tool
					{
						UsesToolKeyName = "item:pigFliesLive",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:pigFliesDead",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[3]
				{
					new Tool
					{
						UsesToolKeyName = "item:advancedString",
						ProductivityFactor = 0.65f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:superconductingWire",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:cottonString",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetFishCarbonTail";
		processToolSet.Tools = new ToolAlternatives[3]
		{
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolTag = "fishingHook",
						ProductivityFactor = 0.7f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[4]
				{
					new Tool
					{
						UsesToolKeyName = "item:pigFliesLive",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:pigFliesDead",
						ProductivityFactor = 0.2f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:neonHornetsLive",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:neonHornetsDead",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[3]
				{
					new Tool
					{
						UsesToolKeyName = "item:advancedString",
						ProductivityFactor = 0.65f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:superconductingWire",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:cottonString",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetSharpenBlade";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[4]
				{
					new Tool
					{
						UsesToolKeyName = "item:smoothSandstone",
						ProductivityFactor = 0.15f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:file",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:blacksmithsToolbox",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:metalWorkersToolbox",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetSharpenBladeAndtoolSetAttachWoodAndMetal";
		processToolSet.Tools = new ToolAlternatives[2]
		{
			new ToolAlternatives
			{
				Tools = new Tool[4]
				{
					new Tool
					{
						UsesToolKeyName = "item:smoothSandstone",
						ProductivityFactor = 0.15f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:file",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:blacksmithsToolbox",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:metalWorkersToolbox",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[6]
				{
					new Tool
					{
						UsesToolTag = "improvisedGlue",
						ProductivityFactor = 0.1f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedString",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:exaGlue",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:metalWire",
						ProductivityFactor = 0.15f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:cottonString",
						ProductivityFactor = 0.15f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:rawhideString",
						ProductivityFactor = 0.1f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetMold";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolKeyName = "item:brickMold",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetDryingPeat";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolKeyName = "structure:peatStack",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetDryingFirewood";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolKeyName = "structure:firewoodStack",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetKilnBigAndSmall";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[2]
				{
					new Tool
					{
						UsesToolKeyName = "structure:kiln",
						ProductivityFactor = 0.7f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:kilnImprovisedSmall",
						ProductivityFactor = 0.45f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetKilnBig";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolKeyName = "structure:kiln",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetOven";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[3]
				{
					new Tool
					{
						UsesToolKeyName = "structure:kilnImprovisedSmall",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:fieldKitchen",
						ProductivityFactor = 0.7f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:simpleStoveUpgrade",
						ProductivityFactor = 0.7f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetCordage";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[6]
				{
					new Tool
					{
						UsesToolKeyName = "item:advancedString",
						ProductivityFactor = 0.55f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:superconductingWire",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:metalWire",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:vine",
						ProductivityFactor = 0.45f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:rawhideString",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:cottonString",
						ProductivityFactor = 0.45f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetLightString";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[5]
				{
					new Tool
					{
						UsesToolKeyName = "item:advancedString",
						ProductivityFactor = 0.55f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:superconductingWire",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:metalWire",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:rawhideString",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:cottonString",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetChopWeakWood";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[8]
				{
					new Tool
					{
						UsesToolKeyName = "item:flintKnife",
						ProductivityFactor = 0.08f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedKnife",
						ProductivityFactor = 0.15f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelKnife",
						ProductivityFactor = 0.2f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedKnife",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelHandAxe",
						ProductivityFactor = 0.35f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedHandAxe",
						ProductivityFactor = 0.25f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedMachete",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelMachete",
						ProductivityFactor = 0.45f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetChopToughWood";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[7]
				{
					new Tool
					{
						UsesToolKeyName = "item:improvisedKnife",
						ProductivityFactor = 0.06f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelKnife",
						ProductivityFactor = 0.1f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedKnife",
						ProductivityFactor = 0.2f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedMachete",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelMachete",
						ProductivityFactor = 0.15f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelHandAxe",
						ProductivityFactor = 0.7f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedHandAxe",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetHarvestSmallBranches";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[7]
				{
					new Tool
					{
						UsesToolKeyName = "item:advancedMachete",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelMachete",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedKnife",
						ProductivityFactor = 0.05f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelKnife",
						ProductivityFactor = 0.1f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedKnife",
						ProductivityFactor = 0.15f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelHandAxe",
						ProductivityFactor = 0.1f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedHandAxe",
						ProductivityFactor = 0.08f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		list.Add(new ProcessToolSet
		{
			KeyName = "toolSetHarvestFieldCrops",
			Tools = new ToolAlternatives[1] { toolAlternatives2 }
		});
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetShapenSmallWoodImprovisedWithFire";
		processToolSet.Tools = new ToolAlternatives[2]
		{
			new ToolAlternatives
			{
				Tools = new Tool[7]
				{
					new Tool
					{
						UsesToolKeyName = "structure:campfire",
						ProductivityFactor = 0.35f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:fieldKitchen",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:improvisedKitchen",
						ProductivityFactor = 0.35f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:mudBrickKitchen",
						ProductivityFactor = 0.35f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:improvisedWorkbench",
						ProductivityFactor = 0.7f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:mudBrickWorkbench",
						ProductivityFactor = 0.7f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:carpenterWorkshopUpgrade",
						ProductivityFactor = 0.85f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[9]
				{
					new Tool
					{
						UsesToolKeyName = "item:carpentersToolbox",
						ProductivityFactor = 0.7f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:flintKnife",
						ProductivityFactor = 0.05f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedKnife",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelKnife",
						ProductivityFactor = 0.45f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedKnife",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedMachete",
						ProductivityFactor = 0.2f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelMachete",
						ProductivityFactor = 0.15f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelHandAxe",
						ProductivityFactor = 0.08f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedHandAxe",
						ProductivityFactor = 0.05f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetMakeSpoakShingles";
		processToolSet.Tools = new ToolAlternatives[2]
		{
			new ToolAlternatives
			{
				Tools = new Tool[3]
				{
					new Tool
					{
						UsesToolKeyName = "item:carpenterWorkshopUpgrade",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:mudBrickWorkbench",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:improvisedWorkbench",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolKeyName = "item:shadeleafResin",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetShapenSmallWood";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[9]
				{
					new Tool
					{
						UsesToolKeyName = "item:carpentersToolbox",
						ProductivityFactor = 0.65f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:flintKnife",
						ProductivityFactor = 0.05f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedKnife",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelKnife",
						ProductivityFactor = 0.35f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedKnife",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedMachete",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelMachete",
						ProductivityFactor = 0.2f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelHandAxe",
						ProductivityFactor = 0.18f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedHandAxe",
						ProductivityFactor = 0.15f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetCombineLightImprovisedObjects";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[7]
				{
					new Tool
					{
						UsesToolTag = "improvisedGlue",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedString",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:exaGlue",
						ProductivityFactor = 0.9f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:superconductingWire",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:metalWire",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:rawhideString",
						ProductivityFactor = 0.2f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:cottonString",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetAttachLightObjectsAndShapeSmallWood";
		processToolSet.Tools = new ToolAlternatives[2]
		{
			new ToolAlternatives
			{
				Tools = new Tool[7]
				{
					new Tool
					{
						UsesToolTag = "improvisedGlue",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedString",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:exaGlue",
						ProductivityFactor = 0.9f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:superconductingWire",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:metalWire",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:rawhideString",
						ProductivityFactor = 0.2f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:cottonString",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[10]
				{
					new Tool
					{
						UsesToolKeyName = "item:carpentersToolbox",
						ProductivityFactor = 0.65f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:flintKnife",
						ProductivityFactor = 0.05f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedKnife",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelKnife",
						ProductivityFactor = 0.35f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedKnife",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedMachete",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelMachete",
						ProductivityFactor = 0.2f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelHandAxe",
						ProductivityFactor = 0.18f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedHandAxe",
						ProductivityFactor = 0.15f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:file",
						ProductivityFactor = 0.1f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		list.Add(new ProcessToolSet
		{
			KeyName = "toolSetMakeImprovisedTool",
			Tools = new ToolAlternatives[2]
			{
				new ToolAlternatives
				{
					Tools = new Tool[4]
					{
						new Tool
						{
							UsesToolKeyName = "item:smoothSandstone",
							ProductivityFactor = 0.15f,
							DegradePerSecondOfUse = 0f
						},
						new Tool
						{
							UsesToolKeyName = "item:file",
							ProductivityFactor = 0.4f,
							DegradePerSecondOfUse = 0f
						},
						new Tool
						{
							UsesToolKeyName = "item:blacksmithsToolbox",
							ProductivityFactor = 0.5f,
							DegradePerSecondOfUse = 0f
						},
						new Tool
						{
							UsesToolKeyName = "item:metalWorkersToolbox",
							ProductivityFactor = 0.6f,
							DegradePerSecondOfUse = 0f
						}
					}
				},
				new ToolAlternatives
				{
					Tools = new Tool[6]
					{
						new Tool
						{
							UsesToolTag = "improvisedGlue",
							ProductivityFactor = 0.1f,
							DegradePerSecondOfUse = 0f
						},
						new Tool
						{
							UsesToolKeyName = "item:advancedString",
							ProductivityFactor = 0.3f,
							DegradePerSecondOfUse = 0f
						},
						new Tool
						{
							UsesToolKeyName = "item:exaGlue",
							ProductivityFactor = 1f,
							DegradePerSecondOfUse = 0f
						},
						new Tool
						{
							UsesToolKeyName = "item:metalWire",
							ProductivityFactor = 0.15f,
							DegradePerSecondOfUse = 0f
						},
						new Tool
						{
							UsesToolKeyName = "item:rawhideString",
							ProductivityFactor = 0.1f,
							DegradePerSecondOfUse = 0f
						},
						new Tool
						{
							UsesToolKeyName = "item:cottonString",
							ProductivityFactor = 0.4f,
							DegradePerSecondOfUse = 0f
						}
					}
				}
			}
		});
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetAttachWoodAndMetal";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[6]
				{
					new Tool
					{
						UsesToolTag = "improvisedGlue",
						ProductivityFactor = 0.1f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedString",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:exaGlue",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:metalWire",
						ProductivityFactor = 0.15f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:cottonString",
						ProductivityFactor = 0.15f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:rawhideString",
						ProductivityFactor = 0.1f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetNone";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolKeyName = "item:advancedString",
						ProductivityFactor = 0.1f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetCutMetal";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[3]
				{
					new Tool
					{
						UsesToolKeyName = "item:metalCutter",
						ProductivityFactor = 2f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedSnips",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:tinnerSnips",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetShapenSimpleSmallMetal";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[7]
				{
					new Tool
					{
						UsesToolKeyName = "item:advancedSnips",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:smoothSandstone",
						ProductivityFactor = 0.08f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:tinnerSnips",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelHandAxe",
						ProductivityFactor = 0.08f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:file",
						ProductivityFactor = 0.2f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:blacksmithsToolbox",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:metalWorkersToolbox",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetShapenSimpleSmallMetalAndtoolSetAttachWoodAndMetal";
		processToolSet.Tools = new ToolAlternatives[2]
		{
			new ToolAlternatives
			{
				Tools = new Tool[7]
				{
					new Tool
					{
						UsesToolKeyName = "item:advancedSnips",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:smoothSandstone",
						ProductivityFactor = 0.08f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:tinnerSnips",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelHandAxe",
						ProductivityFactor = 0.08f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:file",
						ProductivityFactor = 0.2f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:blacksmithsToolbox",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:metalWorkersToolbox",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[6]
				{
					new Tool
					{
						UsesToolTag = "improvisedGlue",
						ProductivityFactor = 0.1f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedString",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:exaGlue",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:metalWire",
						ProductivityFactor = 0.15f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:rawhideString",
						ProductivityFactor = 0.1f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:cottonString",
						ProductivityFactor = 0.15f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetShapenSimpleSmallMetalAndCombineLightImprovisedObjects";
		processToolSet.Tools = new ToolAlternatives[2]
		{
			new ToolAlternatives
			{
				Tools = new Tool[7]
				{
					new Tool
					{
						UsesToolKeyName = "item:advancedSnips",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:smoothSandstone",
						ProductivityFactor = 0.08f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:tinnerSnips",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelHandAxe",
						ProductivityFactor = 0.08f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:file",
						ProductivityFactor = 0.2f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:blacksmithsToolbox",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:metalWorkersToolbox",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[7]
				{
					new Tool
					{
						UsesToolTag = "improvisedGlue",
						ProductivityFactor = 0.2f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedString",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:exaGlue",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:superconductingWire",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:metalWire",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:rawhideString",
						ProductivityFactor = 0.1f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:cottonString",
						ProductivityFactor = 0.15f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetFireplace";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[5]
				{
					new Tool
					{
						UsesToolKeyName = "item:simpleStoveUpgrade",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:fieldKitchen",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:campfire",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:improvisedKitchen",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:mudBrickKitchen",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetBluntTool";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[4]
				{
					new Tool
					{
						UsesToolKeyName = "item:blacksmithsToolbox",
						ProductivityFactor = 0.85f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:metalWorkersToolbox",
						ProductivityFactor = 0.85f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:hammer",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:stoneHammer",
						ProductivityFactor = 0.2f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetImprovisedForging";
		processToolSet.Tools = new ToolAlternatives[3]
		{
			new ToolAlternatives
			{
				Tools = new Tool[2]
				{
					new Tool
					{
						UsesToolKeyName = "structure:improvisedSmithy",
						ProductivityFactor = 0.1f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:simpleSmithy",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[2]
				{
					new Tool
					{
						UsesToolKeyName = "item:bellows",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:blowpipe",
						ProductivityFactor = 0.15f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[4]
				{
					new Tool
					{
						UsesToolKeyName = "item:blacksmithsToolbox",
						ProductivityFactor = 0.85f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:metalWorkersToolbox",
						ProductivityFactor = 0.9f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:hammer",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:stoneHammer",
						ProductivityFactor = 0.1f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetHarderImprovisedForging";
		processToolSet.Tools = new ToolAlternatives[3]
		{
			new ToolAlternatives
			{
				Tools = new Tool[2]
				{
					new Tool
					{
						UsesToolKeyName = "structure:improvisedSmithy",
						ProductivityFactor = 0.1f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:simpleSmithy",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[2]
				{
					new Tool
					{
						UsesToolKeyName = "item:bellows",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:blowpipe",
						ProductivityFactor = 0.15f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[2]
				{
					new Tool
					{
						UsesToolKeyName = "item:blacksmithsToolbox",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:metalWorkersToolbox",
						ProductivityFactor = 0.9f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetSimpleForging";
		processToolSet.Tools = new ToolAlternatives[3]
		{
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolKeyName = "structure:simpleSmithy",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[2]
				{
					new Tool
					{
						UsesToolKeyName = "item:bellows",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:blowpipe",
						ProductivityFactor = 0.15f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[2]
				{
					new Tool
					{
						UsesToolKeyName = "item:blacksmithsToolbox",
						ProductivityFactor = 0.7f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:metalWorkersToolbox",
						ProductivityFactor = 0.9f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetBarrelBoring";
		processToolSet.Tools = new ToolAlternatives[2]
		{
			new ToolAlternatives
			{
				Tools = new Tool[2]
				{
					new Tool
					{
						UsesToolKeyName = "structure:simpleSmithy",
						ProductivityFactor = 0.13f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:metalLatheShopHumanPoweredUpgrade",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolKeyName = "item:metalWorkersToolbox",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetMetalLathe";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolKeyName = "item:metalLatheShopHumanPoweredUpgrade",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetBulletCasting";
		processToolSet.Tools = new ToolAlternatives[2]
		{
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolKeyName = "structure:goldFurnace",
						ProductivityFactor = 0.45f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolKeyName = "item:bulletMold",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetPrimitiveCasting";
		processToolSet.Tools = new ToolAlternatives[3]
		{
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolKeyName = "structure:goldFurnace",
						ProductivityFactor = 0.45f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolKeyName = "item:sandMold",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[3]
				{
					new Tool
					{
						UsesToolKeyName = "item:file",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:blacksmithsToolbox",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:metalWorkersToolbox",
						ProductivityFactor = 0.75f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetSimpleCasting";
		processToolSet.Tools = new ToolAlternatives[3]
		{
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolKeyName = "structure:goldFurnace",
						ProductivityFactor = 0.45f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolKeyName = "item:sandMold",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolKeyName = "item:metalWorkersToolbox",
						ProductivityFactor = 0.75f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetAssembleMetalMachine";
		processToolSet.Tools = new ToolAlternatives[2]
		{
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolKeyName = "item:metalWorkersToolbox",
						ProductivityFactor = 0.75f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[5]
				{
					new Tool
					{
						UsesToolKeyName = "item:advancedString",
						ProductivityFactor = 0.7f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:superconductingWire",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolTag = "improvisedGlue",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:exaGlue",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:metalWire",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetPlowingTools";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[4]
				{
					new Tool
					{
						UsesToolKeyName = "item:steelSpade",
						ProductivityFactor = 0.75f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedSpade",
						ProductivityFactor = 0.7f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelHoe",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:farmingHoe",
						ProductivityFactor = 0.45f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetSowingWeedingFertilizingFarmPlot";
		processToolSet.Comments = "Used for sowing, weeding and fertilizing in farm plots.";
		processToolSet.Tools = new ToolAlternatives[1] { toolAlternatives3 };
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetUnPlowingTools";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolTag = "plowingTools",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetClayPit";
		processToolSet.Comments = "the spade is best because it can break and move the material";
		processToolSet.Tools = new ToolAlternatives[2]
		{
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolKeyName = "structure:clayPit",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[7]
				{
					new Tool
					{
						UsesToolKeyName = "item:improvisedTrowel",
						ProductivityFactor = 0.15f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:farmingHoe",
						ProductivityFactor = 0.27f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelHoe",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedPickaxe",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedSpade",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelPickaxe",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelSpade",
						ProductivityFactor = 0.9f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetSaltMine";
		processToolSet.Comments = "salt is harder to break so the pickaxe is more suited, even though it cannot move the material";
		processToolSet.Tools = new ToolAlternatives[2]
		{
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolKeyName = "structure:saltMine",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[7]
				{
					new Tool
					{
						UsesToolKeyName = "item:improvisedTrowel",
						ProductivityFactor = 0.15f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:farmingHoe",
						ProductivityFactor = 0.27f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelHoe",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedSpade",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedPickaxe",
						ProductivityFactor = 0.55f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelSpade",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelPickaxe",
						ProductivityFactor = 0.9f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetBogOrePit";
		processToolSet.Tools = new ToolAlternatives[2]
		{
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolKeyName = "structure:bogOrePit",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[7]
				{
					new Tool
					{
						UsesToolKeyName = "item:improvisedTrowel",
						ProductivityFactor = 0.15f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedPickaxe",
						ProductivityFactor = 0.28f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelPickaxe",
						ProductivityFactor = 0.33f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelHoe",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:farmingHoe",
						ProductivityFactor = 0.27f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedSpade",
						ProductivityFactor = 0.75f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelSpade",
						ProductivityFactor = 0.9f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetRareMetalOrePit";
		processToolSet.Tools = new ToolAlternatives[2]
		{
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolKeyName = "structure:rareMetalOrePit1",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolKeyName = "item:diggingRobotTool",
						ProductivityFactor = 0.15f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetRareMetalOrePit2";
		processToolSet.Tools = new ToolAlternatives[2]
		{
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolKeyName = "structure:rareMetalOrePit2",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolKeyName = "item:diggingRobotTool",
						ProductivityFactor = 0.15f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetPeatBank";
		processToolSet.Tools = new ToolAlternatives[2]
		{
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolKeyName = "structure:peatBank",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[4]
				{
					new Tool
					{
						UsesToolKeyName = "item:steelSpade",
						ProductivityFactor = 0.75f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedSpade",
						ProductivityFactor = 0.7f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelHoe",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:farmingHoe",
						ProductivityFactor = 0.45f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetFavorbreadFarm";
		processToolSet.Tools = new ToolAlternatives[2]
		{
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolKeyName = "structure:favorbreadFarm",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[5]
				{
					new Tool
					{
						UsesToolKeyName = "item:flintKnife",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedKnife",
						ProductivityFactor = 0.65f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelKnife",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedKnife",
						ProductivityFactor = 0.85f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedTrowel",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetSmokeOven";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[2]
				{
					new Tool
					{
						UsesToolKeyName = "structure:smokeOven",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:smokeOvenUpgrade",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetKitchen";
		processToolSet.Tools = new ToolAlternatives[2]
		{
			new ToolAlternatives
			{
				Tools = new Tool[4]
				{
					new Tool
					{
						UsesToolKeyName = "item:simpleStoveUpgrade",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:fieldKitchen",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:improvisedKitchen",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:mudBrickKitchen",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[5]
				{
					new Tool
					{
						UsesToolKeyName = "item:improvisedKnife",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelKnife",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedKnife",
						ProductivityFactor = 0.7f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedMachete",
						ProductivityFactor = 0.1f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelMachete",
						ProductivityFactor = 0.06f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetFermentWithKitchenAndJar";
		processToolSet.Tools = new ToolAlternatives[2]
		{
			new ToolAlternatives
			{
				Tools = new Tool[4]
				{
					new Tool
					{
						UsesToolKeyName = "item:simpleStoveUpgrade",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:fieldKitchen",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:improvisedKitchen",
						ProductivityFactor = 0.7f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:mudBrickKitchen",
						ProductivityFactor = 0.7f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[2]
				{
					new Tool
					{
						UsesToolKeyName = "item:clayJar",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedPlasticJar",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetMakeVinegar";
		processToolSet.Tools = new ToolAlternatives[3]
		{
			new ToolAlternatives
			{
				Tools = new Tool[5]
				{
					new Tool
					{
						UsesToolKeyName = "item:simpleStoveUpgrade",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:fieldKitchen",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:improvisedKitchen",
						ProductivityFactor = 0.25f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:mudBrickKitchen",
						ProductivityFactor = 0.25f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:fieldLab",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[2]
				{
					new Tool
					{
						UsesToolKeyName = "item:clayJar",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedPlasticJar",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[4]
				{
					new Tool
					{
						UsesToolKeyName = "item:vinegar",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedKnife",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedKnife",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelKnife",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetJarNoHeating";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[2]
				{
					new Tool
					{
						UsesToolKeyName = "item:clayJar",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedPlasticJar",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetMakeBrandy";
		processToolSet.Tools = new ToolAlternatives[2]
		{
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolKeyName = "structure:still",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[2]
				{
					new Tool
					{
						UsesToolKeyName = "item:clayJar",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedPlasticJar",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetMeatDrying";
		processToolSet.Tools = new ToolAlternatives[2]
		{
			new ToolAlternatives
			{
				Tools = new Tool[3]
				{
					new Tool
					{
						UsesToolKeyName = "structure:meatDryingRack",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:dryingShed",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:dryingShedUpgrade",
						ProductivityFactor = 0.9f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[5]
				{
					new Tool
					{
						UsesToolKeyName = "item:improvisedKnife",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelKnife",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedKnife",
						ProductivityFactor = 0.7f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedMachete",
						ProductivityFactor = 0.1f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelMachete",
						ProductivityFactor = 0.06f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetSalamiDrying";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[3]
				{
					new Tool
					{
						UsesToolKeyName = "structure:meatDryingRack",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:dryingShed",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:dryingShedUpgrade",
						ProductivityFactor = 0.9f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetHarvestSap";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[6]
				{
					new Tool
					{
						UsesToolKeyName = "item:plasticTappingBucket",
						ProductivityFactor = 0.9f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:tappingBucket",
						ProductivityFactor = 0.9f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:clayJar",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedPlasticJar",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:vat",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:woodenCookingPot",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetMakeSulfurSmokeBomb";
		processToolSet.Tools = new ToolAlternatives[2]
		{
			new ToolAlternatives
			{
				Tools = new Tool[5]
				{
					new Tool
					{
						UsesToolKeyName = "item:simpleStoveUpgrade",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:campfire",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:fieldKitchen",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:improvisedKitchen",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:mudBrickKitchen",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[4]
				{
					new Tool
					{
						UsesToolKeyName = "item:goldPot",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedCookingPot",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedCookingPot",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:clayPotUnglazed",
						ProductivityFactor = 0.35f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		list.Add(new ProcessToolSet
		{
			KeyName = "toolSetMakeRubber",
			Tools = new ToolAlternatives[2]
			{
				new ToolAlternatives
				{
					Tools = new Tool[1]
					{
						new Tool
						{
							UsesToolKeyName = "item:polymerWorkshopUpgrade",
							ProductivityFactor = 0.7f,
							DegradePerSecondOfUse = 0f
						}
					}
				},
				new ToolAlternatives
				{
					Tools = new Tool[1]
					{
						new Tool
						{
							UsesToolKeyName = "item:vinegar",
							ProductivityFactor = 0.7f,
							DegradePerSecondOfUse = 0f
						}
					}
				}
			}
		});
		list.Add(new ProcessToolSet
		{
			KeyName = "toolSetMakeTextile",
			Tools = new ToolAlternatives[1]
			{
				new ToolAlternatives
				{
					Tools = new Tool[1]
					{
						new Tool
						{
							UsesToolKeyName = "item:textileWorkshopUpgrade",
							ProductivityFactor = 0.7f,
							DegradePerSecondOfUse = 0f
						}
					}
				}
			}
		});
		list.Add(new ProcessToolSet
		{
			KeyName = "toolSetCarpentry",
			Tools = new ToolAlternatives[2]
			{
				new ToolAlternatives
				{
					Tools = new Tool[1]
					{
						new Tool
						{
							UsesToolKeyName = "item:carpenterWorkshopUpgrade",
							ProductivityFactor = 0.7f,
							DegradePerSecondOfUse = 0f
						}
					}
				},
				new ToolAlternatives
				{
					Tools = new Tool[1]
					{
						new Tool
						{
							UsesToolKeyName = "item:carpentersToolbox",
							ProductivityFactor = 0.7f,
							DegradePerSecondOfUse = 0f
						}
					}
				}
			}
		});
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetMakeStew";
		processToolSet.Tools = new ToolAlternatives[2]
		{
			new ToolAlternatives
			{
				Tools = new Tool[5]
				{
					new Tool
					{
						UsesToolKeyName = "structure:campfire",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:fieldKitchen",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:improvisedKitchen",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "structure:mudBrickKitchen",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:simpleStoveUpgrade",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[5]
				{
					new Tool
					{
						UsesToolKeyName = "item:goldPot",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedCookingPot",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedCookingPot",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:clayPotUnglazed",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:woodenCookingPot",
						ProductivityFactor = 0.15f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetMakeBlendedFood";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[5]
				{
					new Tool
					{
						UsesToolKeyName = "item:goldPot",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedCookingPot",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedCookingPot",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:clayPotUnglazed",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:woodenCookingPot",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetDissolveInedibleMatter";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolKeyName = "item:vat",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetShapePottery";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[4]
				{
					new Tool
					{
						UsesToolKeyName = "item:bluntKnife",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedKnife",
						ProductivityFactor = 0.45f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelKnife",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedKnife",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetMakeEnzyme";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolKeyName = "structure:fieldLab",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		list.Add(new ProcessToolSet
		{
			KeyName = "toolSetUseAssemblerPlateA",
			Tools = new ToolAlternatives[2]
			{
				new ToolAlternatives
				{
					Tools = new Tool[1]
					{
						new Tool
						{
							UsesToolKeyName = "structure:molecularAssembler",
							ProductivityFactor = 1f,
							DegradePerSecondOfUse = 0f
						}
					}
				},
				new ToolAlternatives
				{
					Tools = new Tool[1]
					{
						new Tool
						{
							UsesToolKeyName = "item:assemblerPlateA",
							ProductivityFactor = 1f,
							DegradePerSecondOfUse = 0f
						}
					}
				}
			}
		});
		list.Add(new ProcessToolSet
		{
			KeyName = "toolSetUseMasterAssemblerPlateA",
			Tools = new ToolAlternatives[2]
			{
				new ToolAlternatives
				{
					Tools = new Tool[1]
					{
						new Tool
						{
							UsesToolKeyName = "structure:molecularAssembler",
							ProductivityFactor = 1f,
							DegradePerSecondOfUse = 0f
						}
					}
				},
				new ToolAlternatives
				{
					Tools = new Tool[1]
					{
						new Tool
						{
							UsesToolKeyName = "item:masterAssemblerPlateA",
							ProductivityFactor = 1f,
							DegradePerSecondOfUse = 0f
						}
					}
				}
			}
		});
		list.Add(new ProcessToolSet
		{
			KeyName = "toolSetUseAssemblerPlateB",
			Tools = new ToolAlternatives[2]
			{
				new ToolAlternatives
				{
					Tools = new Tool[1]
					{
						new Tool
						{
							UsesToolKeyName = "structure:molecularAssembler",
							ProductivityFactor = 1f,
							DegradePerSecondOfUse = 0f
						}
					}
				},
				new ToolAlternatives
				{
					Tools = new Tool[1]
					{
						new Tool
						{
							UsesToolKeyName = "item:assemblerPlateB",
							ProductivityFactor = 1f,
							DegradePerSecondOfUse = 0f
						}
					}
				}
			}
		});
		list.Add(new ProcessToolSet
		{
			KeyName = "toolSetUseMasterAssemblerPlateB",
			Tools = new ToolAlternatives[2]
			{
				new ToolAlternatives
				{
					Tools = new Tool[1]
					{
						new Tool
						{
							UsesToolKeyName = "structure:molecularAssembler",
							ProductivityFactor = 1f,
							DegradePerSecondOfUse = 0f
						}
					}
				},
				new ToolAlternatives
				{
					Tools = new Tool[1]
					{
						new Tool
						{
							UsesToolKeyName = "item:masterAssemblerPlateB",
							ProductivityFactor = 1f,
							DegradePerSecondOfUse = 0f
						}
					}
				}
			}
		});
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetMixingBlackPowder";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[3]
				{
					new Tool
					{
						UsesToolKeyName = "item:woodenCookingPot",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:clayPotUnglazed",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:vat",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetCleanHide";
		processToolSet.Tools = new ToolAlternatives[3]
		{
			new ToolAlternatives
			{
				Tools = new Tool[5]
				{
					new Tool
					{
						UsesToolKeyName = "item:flintKnife",
						ProductivityFactor = 0.15f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedKnife",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelKnife",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:advancedKnife",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:bluntKnife",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolTag = "hideRack",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[4]
				{
					new Tool
					{
						UsesToolKeyName = "item:turnipBrain",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:thunderChickenBrain",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:megapodBrain",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:whipjawBrain",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetRefineRareMetal";
		processToolSet.Tools = new ToolAlternatives[2]
		{
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolKeyName = "item:steelSpade",
						ProductivityFactor = 0.15f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolTag = "rareMetalRefinery",
						ProductivityFactor = 1f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetDiggingConstruction";
		processToolSet.Tools = new ToolAlternatives[2]
		{
			new ToolAlternatives
			{
				Tools = new Tool[6]
				{
					new Tool
					{
						UsesToolKeyName = "item:advancedString",
						ProductivityFactor = 0.55f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:superconductingWire",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:metalWire",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:vine",
						ProductivityFactor = 0.45f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:rawhideString",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:cottonString",
						ProductivityFactor = 0.45f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[4]
				{
					new Tool
					{
						UsesToolKeyName = "item:improvisedPickaxe",
						ProductivityFactor = 0.28f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelPickaxe",
						ProductivityFactor = 0.33f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedSpade",
						ProductivityFactor = 0.75f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelSpade",
						ProductivityFactor = 0.9f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetSmallDiggingConstruction";
		processToolSet.Tools = new ToolAlternatives[2]
		{
			new ToolAlternatives
			{
				Tools = new Tool[6]
				{
					new Tool
					{
						UsesToolKeyName = "item:advancedString",
						ProductivityFactor = 0.55f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:superconductingWire",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:metalWire",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:vine",
						ProductivityFactor = 0.45f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:rawhideString",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:cottonString",
						ProductivityFactor = 0.45f,
						DegradePerSecondOfUse = 0f
					}
				}
			},
			new ToolAlternatives
			{
				Tools = new Tool[1]
				{
					new Tool
					{
						UsesToolKeyName = "item:improvisedTrowel",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.Comments = "digging holes, collecting grass sod, guano etc.  A tool is needed to loosen the soil, then it can either be moved with hands or with a shovel. A spade can do both.";
		processToolSet.KeyName = "toolSetDiggingSoil";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[7]
				{
					new Tool
					{
						UsesToolKeyName = "item:improvisedTrowel",
						ProductivityFactor = 0.15f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedPickaxe",
						ProductivityFactor = 0.28f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelPickaxe",
						ProductivityFactor = 0.33f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelHoe",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:farmingHoe",
						ProductivityFactor = 0.27f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedSpade",
						ProductivityFactor = 0.75f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelSpade",
						ProductivityFactor = 0.9f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetDiggingClay";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[7]
				{
					new Tool
					{
						UsesToolKeyName = "item:improvisedTrowel",
						ProductivityFactor = 0.15f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:farmingHoe",
						ProductivityFactor = 0.27f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelHoe",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedPickaxe",
						ProductivityFactor = 0.4f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedSpade",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelPickaxe",
						ProductivityFactor = 0.6f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelSpade",
						ProductivityFactor = 0.9f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetDiggingSalt";
		processToolSet.Tools = new ToolAlternatives[1]
		{
			new ToolAlternatives
			{
				Tools = new Tool[7]
				{
					new Tool
					{
						UsesToolKeyName = "item:improvisedTrowel",
						ProductivityFactor = 0.15f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:farmingHoe",
						ProductivityFactor = 0.27f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelHoe",
						ProductivityFactor = 0.3f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedSpade",
						ProductivityFactor = 0.5f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:improvisedPickaxe",
						ProductivityFactor = 0.55f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelSpade",
						ProductivityFactor = 0.8f,
						DegradePerSecondOfUse = 0f
					},
					new Tool
					{
						UsesToolKeyName = "item:steelPickaxe",
						ProductivityFactor = 0.9f,
						DegradePerSecondOfUse = 0f
					}
				}
			}
		};
		item = processToolSet;
		list.Add(item);
		processToolSet = new ProcessToolSet();
		processToolSet.KeyName = "toolSetGreenhouseHarvest";
		processToolSet.Tools = new ToolAlternatives[1] { toolAlternatives4 };
		item = processToolSet;
		list.Add(item);
		return list;
	}
}
