using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.LCD;
using UWGame.SimSide;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.InGameEvents.SpecialEvents;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Tiers;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Policy;

public class TiersPage : TabPagePanel
{
	private class TierButtonArgs : EventArgs
	{
		public TierType Tier;

		public RatingTypes Rating;
	}

	private Grid grdTiers;

	private UIComponent tierHeaderContainer;

	private const int tierHeight = 128;

	private const int tierWidth = 129;

	private const int tierStartX = 51;

	private int ratingsBarWidth;

	private const int tiersToShow = 4;

	private int firstTier;

	private const int normalTooltipWidth = 200;

	private const int wideTooltipWidth = 360;

	private Dictionary<TierArea, string> tierButtonTooltips = new Dictionary<TierArea, string>();

	private Color comfortColor = Util.ColorFromHex("ECBCCF").Value;

	private Color securityColor = Util.ColorFromHex("A3F1FA").Value;

	private Color foodColor = Util.ColorFromHex("C0FDDD").Value;

	private Color readyToAdoptColor = "F5FEBD".ColorFromHex();

	private Color lockedColor = "BCE5EC".ColorFromHex();

	public TiersPage(TabControl parent)
		: base(parent)
	{
		GUIManager gui = parent.guiManager;
		parent.AddTabPage(this, "TECHNOLOGY TIERS", "Set/view current tiers");
		tierHeaderContainer = new UIComponent(gui);
		tierHeaderContainer.Width = Width;
		tierHeaderContainer.Height = 42;
		Add(tierHeaderContainer);
		grdTiers = FullLCDPanel.AddGridWithFixedItemHeights(gui, this, tierHeaderContainer.Height);
		grdTiers.ItemHeight = 128;
		grdTiers.Selectability = Grid.SelectabilityOptions.None;
		grdTiers.CanGrowInHeight = true;
		grdTiers.ScrollBarEnabled = false;
		grdTiers.RowSpacing = 6;
		PopulateStartingTiers();
	}

	private void PopulateStartingTiers()
	{
		GUIManager gUIManager = guiManager;
		grdTiers.BeginAddingEntries();
		tierHeaderContainer.Controls.Clear();
		grdTiers.Clear();
		Array values = Enum.GetValues(typeof(RatingTypes));
		Label label = new Label(guiManager);
		label.Init(Label.LabelType.LCDHeadingBlue);
		label.Text = "TECH:";
		tierHeaderContainer.Add(label);
		label.Width = 42;
		int headerXPos = 51;
		int tierGap = 11;
		IterateVisibleTiers(delegate(TierType tier)
		{
			Label label2 = new Label(guiManager);
			label2.Init(Label.LabelType.LCDHeadingSteelGrey);
			label2.Text = tier.Index + 1 + " - " + tier.Name.ToUpper(Config.Culture);
			tierHeaderContainer.Add(label2);
			label2.Y = 0;
			label2.X = headerXPos;
			label2.Width = 129 - tierGap;
			label2.ToolTip = Common.ComposeHeadingAndBlobText(tier.Name, tier.Description);
			Label label3 = new Label(guiManager);
			label3.Init(Label.LabelType.LCDNormal);
			label3.Text = Common.PercentageToString(tier.UpperEdge);
			tierHeaderContainer.Add(label3);
			label3.Y = label2.Bottom;
			int d = label2.Right + tierGap / 2 - label3.Width / 2;
			d = Common.ClampTop(d, tierHeaderContainer.Width - label3.Width);
			label3.X = d;
			headerXPos += 129;
		});
		foreach (object item in values)
		{
			RatingTypes ratingType = (RatingTypes)item;
			Color? color = null;
			switch (ratingType)
			{
			case RatingTypes.Comfort:
				color = GameData.Instance.GUIConstants.ComfortColor.ColorFromHex();
				break;
			case RatingTypes.Security:
				color = GameData.Instance.GUIConstants.SecurityColor.ColorFromHex();
				break;
			case RatingTypes.Food:
				color = GameData.Instance.GUIConstants.FoodColor.ColorFromHex();
				break;
			}
			LCDInnerPanel lCDInnerPanel = new LCDInnerPanel(gUIManager, grdTiers.Width, includeDecor: false, 0.5f, color);
			lCDInnerPanel.Panel.Height = 128;
			grdTiers.AddEntry(item, lCDInnerPanel.Panel);
			int num = 45;
			FillableBar fillableBar = new FillableBar(gUIManager, FillableBar.FillableBarType.ProgressBar, canGrow: false);
			lCDInnerPanel.AddContent(fillableBar, num, 6);
			fillableBar.Height = 12;
			fillableBar.ID = DataControlID.Rating;
			fillableBar.Color = color.Value;
			fillableBar.MaxValue = 100;
			fillableBar.DebugTag = "ratingsBar";
			fillableBar.ShowMaxValueLabelAtEnd = false;
			Icon icon = new Icon(gUIManager);
			lCDInnerPanel.AddContent(icon, 16, 3);
			icon.SetSkinLocation(SkinState.Normal, gUIManager.GUISpriteSheet.GetSourceRectangle(GetRowHeaderSprite(ratingType)));
			icon.ResizeControlToFitImage();
			icon.ToolTip = Common.ComposeHeadingAndBlobText(Statistic.RatingsTypeToString(ratingType), Statistic.RatingsTypeToDescription(ratingType));
			HorizontalList hzButtons = new HorizontalList(gUIManager);
			hzButtons.Height = 128;
			hzButtons.MinHeight = 128;
			hzButtons.MaxHeight = 128;
			hzButtons.ID = DataControlID.Actions;
			lCDInnerPanel.AddContent(hzButtons, num - 4, 21);
			hzButtons.BeginAddingEntries();
			IterateVisibleTiers(delegate(TierType t)
			{
				AddTierButton(hzButtons, ratingType, t);
			});
			hzButtons.EndAddingEntries();
			ratingsBarWidth = hzButtons.Width + 14;
			fillableBar.Width = ratingsBarWidth;
		}
		grdTiers.EndAddingEntries();
		Height = grdTiers.Bottom;
	}

	private void IterateVisibleTiers(Action<TierType> iterator)
	{
		int val = GameData.Instance.Tiers.Length;
		int val2 = firstTier + 4 - 1;
		val2 = Math.Min(val2, val);
		for (int i = firstTier; i <= val2; i++)
		{
			TierType obj = GameData.Instance.Tiers[i];
			iterator(obj);
		}
	}

	private void AddTierButton(HorizontalList buttonList, RatingTypes rating, TierType tier)
	{
		GUIManager gUIManager = buttonList.guiManager;
		UIComponent uIComponent = new UIComponent(gUIManager);
		buttonList.AddEntry(tier, uIComponent);
		ImageButton imageButton = new ImageButton(gUIManager);
		uIComponent.Add(imageButton);
		imageButton.Init(GetActionButtonType(rating));
		imageButton.Tag1 = tier;
		imageButton.Click += btAction_Click;
		imageButton.EventArgs = new TierButtonArgs
		{
			Tier = tier,
			Rating = rating
		};
		imageButton.ID = DataControlID.Action;
		imageButton.ScaleImageToSizeOfControl = false;
		imageButton.CheckedMode = CheckedModes.CannotBeChecked;
		imageButton.TooltipExpires = false;
		imageButton.TooltipWidth = 240;
		TierArea tierArea = GetTierArea(rating, tier);
		string value = Common.ComposeHeadingAndBlobText(tierArea.ToString(), tierArea.Description);
		tierButtonTooltips.Add(tierArea, value);
		imageButton.ToolTip = value;
		uIComponent.Width = imageButton.Width;
		uIComponent.Height = imageButton.Height;
		int y = 20;
		Label label = new Label(buttonList.guiManager);
		label.Init(Label.LabelType.LCDNormal);
		label.ID = DataControlID.VotersFor;
		uIComponent.Add(label);
		label.X = 12;
		label.Y = y;
		Icon icon = new Icon(gUIManager);
		icon.SetSkinLocation(SkinState.Normal, gUIManager.GUISpriteSheet.GetSourceRectangle("lcd_icon_thumbsUp"), UIComponent.LCDNormal, UIComponent.LCDNormal);
		icon.ResizeControlToFitImage();
		icon.ID = DataControlID.VotersForIcon;
		uIComponent.Add(icon);
		icon.Y = y;
		icon.X = 36;
		icon.CanHaveFocus = false;
		Label label2 = new Label(buttonList.guiManager);
		label2.Init(Label.LabelType.LCDNormal);
		label2.ID = DataControlID.VotersAgainst;
		uIComponent.Add(label2);
		label2.X = 53;
		label2.Y = y;
		icon = new Icon(gUIManager);
		icon.SetSkinLocation(SkinState.Normal, gUIManager.GUISpriteSheet.GetSourceRectangle("lcd_icon_thumbsDown"), UIComponent.errorColor, UIComponent.errorColor);
		icon.ResizeControlToFitImage();
		icon.ID = DataControlID.VotersAgainstIcon;
		uIComponent.Add(icon);
		icon.Y = y;
		icon.X = 85;
		icon.CanHaveFocus = false;
		Label label3 = new Label(buttonList.guiManager);
		label3.Init(Label.LabelType.LCDNormal);
		label3.ID = DataControlID.Prompt;
		uIComponent.Add(label3);
		label3.X = 53;
		label3.Y = label2.Bottom;
		label3.TooltipWidth = 200;
	}

	private static TierArea GetTierArea(RatingTypes rating, TierType tier)
	{
		return GameData.Instance.AllTierAreas.FirstOrDefault((KeyValuePair<string, TierArea> a) => a.Value.TierType == tier && a.Value.Area == rating).Value;
	}

	private bool CanAdopt(List<Entity> membersFor, List<Entity> membersAgainst)
	{
		return membersFor.Count > membersAgainst.Count;
	}

	private void btAction_Click(UIComponent sender, EventArgs e)
	{
		TierButtonArgs tierButtonArgs = e as TierButtonArgs;
		LookUp<Expedition, ExpeditionID>.FindByID(The.InGameUI.UIExpedition);
		TierArea tierArea = GetTierArea(tierButtonArgs.Rating, tierButtonArgs.Tier);
		The.InGameUI.UIAllegiance.GetMembersInTierRange(tierButtonArgs.Rating, tierButtonArgs.Tier, GameData.Instance.AIConstants.AgentCanVote, out var membersAboveRange, out var membersBelowRange, out var membersUnqualified, out var _);
		if (CanAdopt(membersAboveRange, membersBelowRange))
		{
			AdoptTierPolicy command = new AdoptTierPolicy(The.InGameUI.UIExpedition.Value, tierButtonArgs.Tier, tierButtonArgs.Rating, giveClientFeedback: true);
			sender.DebugTag = "adopted";
			The.Client.Controller.StoreAndExecuteCommand(command);
			PopulateTiers();
			new PolicyAdoptedByVoting().ShowPolicyAdoption(tierArea, membersAboveRange, membersBelowRange, membersUnqualified);
		}
	}

	private string GetRowHeaderSprite(RatingTypes rating)
	{
		return rating switch
		{
			RatingTypes.Comfort => "policy_banner_comfort", 
			RatingTypes.Security => "policy_banner_security", 
			RatingTypes.Food => "policy_banner_food", 
			_ => "policy_banner_comfort", 
		};
	}

	private ImageButtonType GetActionButtonType(RatingTypes rating)
	{
		return rating switch
		{
			RatingTypes.Comfort => ImageButtonType.ComfortPolicy, 
			RatingTypes.Security => ImageButtonType.SecurityPolicy, 
			RatingTypes.Food => ImageButtonType.FoodPolicy, 
			_ => ImageButtonType.ComfortPolicy, 
		};
	}

	private Color GetButtonColor(RatingTypes rating)
	{
		return rating switch
		{
			RatingTypes.Comfort => comfortColor, 
			RatingTypes.Security => securityColor, 
			RatingTypes.Food => foodColor, 
			_ => comfortColor, 
		};
	}

	private void UpdateTierButton(Expedition expedition, HorizontalList hzButtons, TierType tier, RatingTypes ratingType, float rating)
	{
		hzButtons.TryGetEntry(tier, out var entry);
		ImageButton imageButton = (ImageButton)entry.FindChildById(DataControlID.Action, firstLevelOnly: true);
		Color color;
		Color disabledColor;
		float requiredRating;
		if (expedition.Policy.TierIsUnlocked(ratingType, tier))
		{
			UpdateUnlockedTierArea(ratingType, tier, entry, imageButton, out color, out disabledColor);
		}
		else if (expedition.Policy.IsLaterTier(ratingType, tier))
		{
			UpdateLaterTierArea(ratingType, tier, entry, imageButton, out color, out disabledColor);
		}
		else if (!expedition.Policy.RatingIsInsideOrAbovePreviousTier(ratingType, rating, tier, out requiredRating))
		{
			UpdateTierAreaAboveRating(ratingType, tier, requiredRating, entry, imageButton, out color, out disabledColor);
		}
		else
		{
			UpdateVotes(ratingType, tier, rating, entry, out color);
			disabledColor = color;
		}
		imageButton.SetSkinLocation(SkinState.Normal, null, color, color);
		imageButton.SetSkinLocation(SkinState.Disabled, null, disabledColor, disabledColor);
	}

	private void UpdateTierAreaAboveRating(RatingTypes rating, TierType tier, float requiredRating, UIComponent tierComponent, ImageButton btTier, out Color color, out Color disabledColor)
	{
		TierArea tierArea = GetTierArea(rating, tier);
		HideVotes(tierComponent);
		btTier.Enabled = false;
		color = lockedColor;
		disabledColor = lockedColor;
		StringBuilder stringBuilder = new StringBuilder();
		Common.Append(stringBuilder, tierButtonTooltips[tierArea]);
		Common.AppendDividerOnOwnLine(stringBuilder);
		Common.AppendImpossibleActionText(stringBuilder, "Cannot adopt: ");
		Common.Append(stringBuilder, "The colony rating has to reach ");
		Common.AppendFormat(stringBuilder, "{0}", true, Common.PercentageToString(requiredRating));
		string text = (btTier.ToolTip = stringBuilder.ToString());
		Label label = (Label)tierComponent.FindChildById(DataControlID.Prompt, firstLevelOnly: true);
		label.Visible = true;
		label.ToolTip = text;
		label.Text = "CANNOT VOTE";
		label.FitToText();
		tierComponent.CenterChildHorizontally(label);
	}

	private void UpdateLaterTierArea(RatingTypes rating, TierType tier, UIComponent tierComponent, ImageButton btTier, out Color color, out Color disabledColor)
	{
		TierArea tierArea = GetTierArea(rating, tier);
		HideVotes(tierComponent);
		btTier.Enabled = false;
		color = lockedColor;
		disabledColor = lockedColor;
		StringBuilder stringBuilder = new StringBuilder();
		Common.Append(stringBuilder, tierButtonTooltips[tierArea]);
		Common.AppendDividerOnOwnLine(stringBuilder);
		Common.AppendImpossibleActionText(stringBuilder, "Cannot adopt: ");
		Common.Append(stringBuilder, "We need to adopt the lower tier areas first.");
		btTier.ToolTip = stringBuilder.ToString();
	}

	private void UpdateUnlockedTierArea(RatingTypes ratingType, TierType tier, UIComponent tierComponent, ImageButton btTier, out Color color, out Color disabledColor)
	{
		TierArea tierArea = GetTierArea(ratingType, tier);
		HideVotes(tierComponent);
		color = GetButtonColor(ratingType);
		btTier.Enabled = false;
		disabledColor = color;
		StringBuilder stringBuilder = new StringBuilder();
		Common.Append(stringBuilder, tierButtonTooltips[tierArea]);
		Common.AppendDividerOnOwnLine(stringBuilder);
		Common.AppendImpossibleActionText(stringBuilder, "Cannot adopt: ");
		Common.Append(stringBuilder, "We have already adopted this policy.");
		btTier.ToolTip = stringBuilder.ToString();
	}

	private int MapRatingToFillableBar(float rating)
	{
		int stairstep;
		TierType stairStepIndex = Common.GetStairStepIndex(rating, GameData.Instance.Tiers, out stairstep);
		if (stairStepIndex.Index < firstTier)
		{
			return 0;
		}
		if (stairStepIndex.Index >= firstTier + 4)
		{
			return 100;
		}
		TierType.GetTierBelow(stairstep, out var _, out var lowerTierEdge);
		stairstep -= firstTier;
		float amount = (rating - lowerTierEdge) / (stairStepIndex.UpperEdge - lowerTierEdge);
		float num = MathHelper.Lerp(stairstep * 129, (stairstep + 1) * 129, amount);
		num /= (float)ratingsBarWidth;
		return Common.Clamp((int)Math.Round(100f * num), 0, 100);
	}

	private void PopulateTiers()
	{
		Expedition expedition = LookUp<Expedition, ExpeditionID>.FindByID(The.InGameUI.UIExpedition);
		Allegiance uIAllegiance = The.InGameUI.UIAllegiance;
		if (expedition == null)
		{
			return;
		}
		foreach (object value in Enum.GetValues(typeof(RatingTypes)))
		{
			RatingTypes ratingType = (RatingTypes)value;
			if (grdTiers.TryGetEntry(value, out var item))
			{
				FillableBar obj = (FillableBar)item.FindChildById(DataControlID.Rating, firstLevelOnly: true);
				float rating = uIAllegiance.Statistics.GetRating(ratingType);
				obj.Value = MapRatingToFillableBar(rating);
				obj.ToolTip = Common.ComposeHeadingAndBlobText("Colony " + Statistic.RatingsTypeToString(ratingType).ToLower(Config.Culture) + " conditions", Common.PercentageToString(rating));
				HorizontalList hzButtons = (HorizontalList)item.FindChildById(DataControlID.Actions, firstLevelOnly: true);
				IterateVisibleTiers(delegate(TierType t)
				{
					UpdateTierButton(expedition, hzButtons, t, ratingType, rating);
				});
			}
		}
	}

	private void HideVotes(UIComponent item)
	{
		((Label)item.FindChildById(DataControlID.VotersFor, firstLevelOnly: true)).Visible = false;
		((Icon)item.FindChildById(DataControlID.VotersForIcon, firstLevelOnly: true)).Visible = false;
		((Label)item.FindChildById(DataControlID.VotersAgainst, firstLevelOnly: true)).Visible = false;
		((Icon)item.FindChildById(DataControlID.VotersAgainstIcon, firstLevelOnly: true)).Visible = false;
		((Label)item.FindChildById(DataControlID.Prompt, firstLevelOnly: true)).Visible = false;
	}

	private List<Entity> GetUnavailableVoters(List<Entity> voters)
	{
		List<Entity> list = null;
		foreach (Entity voter in voters)
		{
			if (!GameData.Instance.AIConstants.PolicyCanBeAdopted.IsFulfilled(voter))
			{
				Common.AddToList(ref list, voter);
			}
		}
		return list;
	}

	private void UpdateVotes(RatingTypes rating, TierType tier, float currentRating, UIComponent item, out Color color)
	{
		TierArea tierArea = GetTierArea(rating, tier);
		The.InGameUI.UIAllegiance.GetMembersInTierRange(rating, tier, GameData.Instance.AIConstants.AgentCanVote, out var membersAboveRange, out var membersBelowRange, out var _, out var _);
		int num = membersAboveRange?.Count ?? 0;
		int num2 = membersBelowRange?.Count ?? 0;
		List<Entity> list = null;
		Common.AddRangeToList(ref list, membersAboveRange);
		Common.AddRangeToList(ref list, membersBelowRange);
		List<Entity> unavailableVoters = GetUnavailableVoters(list);
		((Icon)item.FindChildById(DataControlID.VotersForIcon, firstLevelOnly: true)).Visible = true;
		((Icon)item.FindChildById(DataControlID.VotersAgainstIcon, firstLevelOnly: true)).Visible = true;
		Label label = (Label)item.FindChildById(DataControlID.VotersFor, firstLevelOnly: true);
		label.Text = num + "X";
		label.AlignRight(32);
		label.Visible = true;
		if (membersAboveRange != null)
		{
			label.ToolTip = "Members for: \n" + Common.ListToCommaSeparatedString(membersAboveRange, (Entity e) => e.GetDisplayName());
		}
		else
		{
			label.ToolTip = "No one is for adopting this.";
		}
		Label label2 = (Label)item.FindChildById(DataControlID.VotersAgainst, firstLevelOnly: true);
		label2.Text = num2 + "X";
		label2.AlignRight(82);
		label2.Visible = true;
		if (membersBelowRange != null)
		{
			label2.ToolTip = "Members against: \n" + Common.ListToCommaSeparatedString(membersBelowRange, (Entity e) => e.GetDisplayName());
		}
		else
		{
			label2.ToolTip = "No one is against adopting this.";
		}
		ImageButton imageButton = (ImageButton)item.FindChildById(DataControlID.Action);
		Label label3 = (Label)item.FindChildById(DataControlID.Prompt, firstLevelOnly: true);
		label3.Visible = true;
		if (unavailableVoters == null || unavailableVoters.Count == 0)
		{
			if (num > num2)
			{
				label3.Text = "DECIDE NOW?";
				label3.ToolTip = $"Click to adopt this policy. We will then be able to buy and produce items from this tier. \n \nNOTE: When adopting this policy, those who are against will have their principles raised to this level ({Common.PercentageToString(GameData.Instance.Tiers[tier.Index - 1].UpperEdge)}). This can increase their unhappiness unless conditions are quickly improved.";
				label3.TooltipWidth = 200;
				label3.TooltipExpires = true;
				StringBuilder stringBuilder = new StringBuilder();
				Common.Append(stringBuilder, tierButtonTooltips[tierArea]);
				Common.AppendDividerOnOwnLine(stringBuilder);
				Common.AppendPossibleActionText(stringBuilder, "CLICK TO ADOPT THIS POLICY");
				imageButton.ToolTip = stringBuilder.ToString();
				imageButton.Enabled = true;
				color = readyToAdoptColor;
			}
			else
			{
				int num3 = num2 + num;
				int num4 = (int)Math.Ceiling((float)num3 / 2f);
				if (num3 % 2 == 0)
				{
					num4++;
				}
				int neededVotes = num4 - num;
				label3.Text = "NO MAJORITY";
				label3.ToolTip = ComposeNoMajorityTooltip(tier, rating, currentRating, neededVotes, membersBelowRange);
				label3.TooltipWidth = 360;
				label3.TooltipExpires = false;
				StringBuilder stringBuilder2 = new StringBuilder();
				Common.Append(stringBuilder2, tierButtonTooltips[tierArea]);
				Common.AppendDividerOnOwnLine(stringBuilder2);
				Common.AppendImpossibleActionText(stringBuilder2, "Cannot adopt: ");
				Common.Append(stringBuilder2, "No majority. Learn more by hovering on the NO MAJORITY text.");
				imageButton.ToolTip = stringBuilder2.ToString();
				imageButton.Enabled = false;
				color = lockedColor;
			}
		}
		else
		{
			label3.Text = "NOT READY";
			label3.ToolTip = "The following members are not ready to vote right now: \n" + Common.ListToCommaSeparatedString(unavailableVoters, (Entity e) => e.GetDisplayName());
			label3.TooltipWidth = 200;
			label3.TooltipExpires = true;
			StringBuilder stringBuilder3 = new StringBuilder();
			Common.Append(stringBuilder3, tierButtonTooltips[tierArea]);
			Common.AppendDividerOnOwnLine(stringBuilder3);
			Common.AppendImpossibleActionText(stringBuilder3, "Cannot adopt: ");
			Common.Append(stringBuilder3, "Some members are busy or sleeping. Everyone must be able to participate in the vote.");
			imageButton.ToolTip = stringBuilder3.ToString();
			imageButton.Enabled = false;
			color = lockedColor;
		}
		label3.FitToText();
		item.CenterChildHorizontally(label3);
	}

	private string ComposeNoMajorityTooltip(TierType tier, RatingTypes rating, float currentRating, int neededVotes, List<Entity> membersBelowRange)
	{
		DateAndTime.TimeDateYear timeToReachMajority = The.InGameUI.UIAllegiance.GetTimeToReachMajority(rating, tier, neededVotes, membersBelowRange);
		StringBuilder stringBuilder = new StringBuilder();
		Common.AppendHeaderOnLightBG(stringBuilder, "No majority");
		Common.Append(stringBuilder, "There is no majority for this policy yet.");
		Common.AppendLine(stringBuilder);
		Common.AppendLine(stringBuilder);
		Common.Append(stringBuilder, "To adopt this policy, more colonists with higher principles are needed");
		Common.AppendLine(stringBuilder);
		Common.AppendLine(stringBuilder);
		Common.AppendHeaderOnLightBG(stringBuilder, "SUGGESTED STRATEGY:");
		Common.AppendLine(stringBuilder, "Do one or both of the following:");
		Common.AppendLine(stringBuilder);
		Common.AppendLine(stringBuilder, "1. Let the inhabitants grow accustomed to a higher " + Statistic.AppendRatingsTypeToStringAndIcon(rating) + " rating.");
		Common.AppendIndentedLine(stringBuilder, "They will slowly change their principles above the rating.");
		float tierEdgeBelow = tier.GetTierEdgeBelow();
		string text = timeToReachMajority.ToIntervalString();
		if (currentRating < tierEdgeBelow)
		{
			Common.AppendIndentedLine(stringBuilder, "After increasing " + Statistic.AppendRatingsTypeToStringAndIcon(rating) + " to " + Common.PercentageToString(tierEdgeBelow, includePlusPrefix: false, useColoring: false, Common.ValueTint.Neutral) + " the estimated time to reach majority is: " + text);
		}
		else
		{
			Common.AppendIndentedLine(stringBuilder, "Current time to reach majority: " + text);
		}
		Common.AppendLine(stringBuilder, "2. Attract immigrants with high " + Statistic.AppendRatingsTypeToStringAndIcon(rating) + " principles.");
		Common.AppendIndentedLine(stringBuilder, "This will more quickly gain a majority.");
		return stringBuilder.ToString();
	}

	public override void Refresh()
	{
		Populate();
	}

	private void Populate()
	{
		PopulateTiers();
	}
}
