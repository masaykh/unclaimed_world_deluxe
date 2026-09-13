using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.Controls;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.ClientSide.Interface.LCD;
using UWGame.ClientSide.Log;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.Control.Commands;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Jobs.JobTypes;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Tasks;

public class JobsPanel : RosterPanel
{
	private enum ExpandCollapse
	{
		Expand,
		Collapse
	}

	private Expedition expedition = The.Sim.PlaySite.GetFirstPlayerExpedition();

	private Grid outerGrid;

	private LCDInnerPanel topPanel;

	private const int collapsedItemHeight = 35;

	private const int collapsedContentHeight = 31;

	private const int hyperLinkWidth = 15;

	private const int EntityTypeButtonEventArgsSize = 100;

	private const int XMoveRightLeftBar = 150;

	private const int priorityXpos = 418;

	private SortingButtons<TaskSettings.SortColumns> sortingButtons;

	private UIComponent sortingButtonsContainer;

	private ImageButton ibExpand;

	private ImageButton ibCompress;

	private List<Tuple<string, Job>> allJobsToShow = new List<Tuple<string, Job>>();

	private List<Job> tempJobsList = new List<Job>();

	private const int titleWidth = 150;

	private RadioGroup rgPriority;

	private RadioButton rbJobTypeNormal;

	private RadioButton rbJobTypeHigh;

	private RadioButton rbJobTypeLow;

	private ComboBox cbTaskType;

	private const string selectTaskTypePromptKey = "SELECTTASKTYPE";

	private TextArea taMessages;

	private Dictionary<string, JobsMessage> messages = new Dictionary<string, JobsMessage>();

	private const int orderedX = 200;

	private const string messagePrefix = "- ";

	private static Color notActiveColor = Color.Gray;

	private static Color activeColor = "EDFAFF".ColorFromHex();

	private string noTools = "Tools not in inventory";

	private string allToolsInUse = "Tools owned but currently in use";

	private string allToolsBroken = "Tools are broken and unusable";

	private const int paddedErrorHeight = 23;

	private const int firstColumnWidth = 230;

	private const int secondColumnWidth = 273;

	private const int columnGap = 10;

	private const int horizPadding = 6;

	private const int vertPadding = 8;

	private const int inputItemHeight = 18;

	private const string unassignedInputsGridKey = "unassigned";

	private const string assignedInputsGridKey = "assigned";

	public JobsPanel()
		: base("TASKS", 622, needBottomMarginForButtons: false)
	{
		CreateTopPanel();
		CreateGridHeaderButtons();
		int num = sortingButtonsContainer.Bottom + 2;
		outerGrid = new Grid(Interface.gui, ListBoxType.LCD, Label.LabelType.CRTBigGlow);
		outerGrid.FixedItemHeights = false;
		outerGrid.RenderType = RenderType.CRTAndLCD;
		lcdSurface.Add(outerGrid);
		outerGrid.HMargin = 0;
		outerGrid.VMargin = 0;
		outerGrid.Font = GUIManager.LCDandHUDBodyFontPath;
		outerGrid.Width = lcdSurface.Width;
		outerGrid.Height = lcdSurface.Height - num;
		outerGrid.ItemHeight = 35;
		outerGrid.Position = new Point(0, num);
		outerGrid.RowSpacing = 2;
		LoadUserSettings();
		The.Client.Log.NewEventAlert += Log_NewEventAlert;
	}

	private void Log_NewEventAlert(Event newEvent)
	{
		if (newEvent.EventType == The.Client.Log.EconomicEvent)
		{
			string plainText = Label.GetPlainText(newEvent.Text);
			if (!messages.ContainsKey(plainText))
			{
				messages.Add(plainText, new JobsMessage
				{
					Text = plainText
				});
			}
		}
	}

	private void CreateTopPanel()
	{
		topPanel = new LCDInnerPanel(Interface.gui, lcdSurface.Width, includeDecor: false);
		topPanel.HorizontalContentPadding = 6;
		topPanel.VerticalContentPadding = 8;
		lcdSurface.Add(topPanel.Panel);
		Box box = new Box(Interface.gui);
		topPanel.AddContent(box, -6, -8);
		box.CornerSize = 20;
		box.SetSkinLocation(SkinState.Normal, Interface.gui.GUISpriteSheet.GetSourceRectangle("lcd_panel_background"));
		box.Width = topPanel.Panel.Width;
		box.Height = 60;
		taMessages = new TextArea(Interface.gui, ListBoxType.LCD);
		taMessages.VMargin = 6;
		taMessages.HMargin = 6;
		box.Add(taMessages);
		taMessages.RenderType = RenderType.CRTAndLCD;
		taMessages.Init(Label.LabelType.LCDNormal);
		taMessages.CanGrowInHeight = false;
		taMessages.ScrollBarEnabled = true;
		taMessages.Height = 60;
		taMessages.Width = box.Width;
		taMessages.Color = UIComponent.errorColor;
		int num = 64;
		Label label = new Label(Interface.gui);
		topPanel.AddContent(label, 0, num);
		Label.LabelType type = Label.LabelType.LCDSmallHeadingBanner;
		label.Init(type);
		label.Text = "SET PRIORITY:";
		label.Width = 100;
		cbTaskType = new ComboBox(Interface.gui, ListBoxType.LCDCombo, isEditable: false);
		topPanel.AddContent(cbTaskType, 104, num - 6);
		cbTaskType.Init(ComboBoxTypes.LCD);
		cbTaskType.Width = 145;
		PopulateTaskTypeCombo();
		cbTaskType.SelectionChanged += cbTaskType_SelectionChanged;
		cbTaskType.ToolTip = "Select a task type to view or set its priority";
		rgPriority = new RadioGroup(Interface.gui);
		topPanel.AddContent(rgPriority, 270, num - 1);
		rgPriority.Width = 280;
		rgPriority.Height = 40;
		rbJobTypeLow = new RadioButton(Interface.gui);
		rgPriority.Add(rbJobTypeLow);
		rbJobTypeLow.Init(CheckBoxType.LCDRadioBanner);
		rbJobTypeLow.Text = Job.GetPriorityAsString(UWGame.SimSide.Jobs.Priority.Low);
		rbJobTypeLow.ToolTip = "Set to low priority. This will affect all current and future tasks of this type.";
		rbJobTypeLow.Tag1 = UWGame.SimSide.Jobs.Priority.Low;
		rbJobTypeLow.Width = 90;
		rbJobTypeNormal = new RadioButton(Interface.gui);
		rgPriority.Add(rbJobTypeNormal);
		rbJobTypeNormal.Init(CheckBoxType.LCDRadioBanner);
		rbJobTypeNormal.X = rbJobTypeLow.Right + 6;
		rbJobTypeNormal.Text = Job.GetPriorityAsString(UWGame.SimSide.Jobs.Priority.Normal);
		rbJobTypeNormal.ToolTip = "Set to normal priority. This will affect all current and future tasks of this type.";
		rbJobTypeNormal.IsChecked = true;
		rbJobTypeNormal.Tag1 = UWGame.SimSide.Jobs.Priority.Normal;
		rbJobTypeNormal.Width = rbJobTypeLow.Width;
		rbJobTypeHigh = new RadioButton(Interface.gui);
		rgPriority.Add(rbJobTypeHigh);
		rbJobTypeHigh.Init(CheckBoxType.LCDRadioBanner);
		rbJobTypeHigh.X = rbJobTypeNormal.Right + 6;
		rbJobTypeHigh.Text = Job.GetPriorityAsString(UWGame.SimSide.Jobs.Priority.High);
		rbJobTypeHigh.ToolTip = "Set to high priority. This will affect all current and future tasks of this type.";
		rbJobTypeHigh.Tag1 = UWGame.SimSide.Jobs.Priority.High;
		rbJobTypeHigh.Width = rbJobTypeLow.Width;
		rbJobTypeNormal.Click += btJobTypeNormal_Click;
		rbJobTypeHigh.Click += btJobTypeHigh_Click;
		rbJobTypeLow.Click += btJobTypeLow_Click;
		UpdateTaskTypeControls();
	}

	private void PopulateTaskTypeCombo()
	{
		cbTaskType.AddEntry("SELECTTASKTYPE", "Select task type:");
		foreach (KeyValuePair<string, JobType> allJobType in GameData.Instance.AllJobTypes)
		{
			cbTaskType.BeginAddingEntries();
			cbTaskType.AddEntry(allJobType.Value, allJobType.Value.GetDefaultDisplayName());
			cbTaskType.EndAddingEntries();
		}
		cbTaskType.SelectedKey = "SELECTTASKTYPE";
	}

	private void FillTaskTypeControls(JobType taskType)
	{
		if (!expedition.OwnedEntities.Policy.JobTypePriorities.TryGetValue(taskType, out var value))
		{
			value = UWGame.SimSide.Jobs.Priority.Normal;
		}
		switch (value)
		{
		case UWGame.SimSide.Jobs.Priority.Low:
			rgPriority.SelectMember(rbJobTypeLow);
			break;
		case UWGame.SimSide.Jobs.Priority.Normal:
			rgPriority.SelectMember(rbJobTypeNormal);
			break;
		case UWGame.SimSide.Jobs.Priority.High:
			rgPriority.SelectMember(rbJobTypeHigh);
			break;
		}
	}

	private void cbTaskType_SelectionChanged(UIComponent sender)
	{
		UpdateTaskTypeControls();
	}

	private void UpdateTaskTypeControls()
	{
		if (!cbTaskType.SelectedKey.Equals("SELECTTASKTYPE"))
		{
			rgPriority.Visible = true;
			JobType taskType = (JobType)cbTaskType.SelectedKey;
			FillTaskTypeControls(taskType);
		}
		else
		{
			rgPriority.Visible = false;
		}
	}

	public override void Refresh()
	{
		UpdateTopPanel();
		UpdateJobs(null);
		UpdateMessages();
		base.Refresh();
	}

	public override void Update(GameTime gameTime)
	{
		List<JobsMessage> list = null;
		foreach (KeyValuePair<string, JobsMessage> message in messages)
		{
			message.Value.Update(gameTime);
			if (message.Value.IsExpired())
			{
				Common.AddToList(ref list, message.Value);
			}
		}
		if (list != null)
		{
			foreach (JobsMessage item in list)
			{
				messages.Remove(item.Text);
			}
		}
		base.Update(gameTime);
	}

	private void LoadUserSettings()
	{
		TaskSettings taskSettings = The.InGameUI.TaskSettings;
		sortingButtons.Fill(taskSettings.SortingSettings);
	}

	private void GetAllJobsToShow()
	{
		allJobsToShow.Clear();
		tempJobsList.Clear();
		tempJobsList.AddRange(expedition.OwnedEntities.OtherJobs);
		foreach (KeyValuePair<EntityType, List<ProcessJob>> productionJob in expedition.OwnedEntities.ProductionJobs)
		{
			foreach (ProcessJob item in productionJob.Value)
			{
				ProcessJob processJob = item;
				if (processJob == null || processJob.HarvestJob == null)
				{
					tempJobsList.Add(item);
				}
			}
		}
		foreach (Zone zone in expedition.OwnedEntities.Zones)
		{
			if (zone.HarvestJobs == null)
			{
				continue;
			}
			foreach (KeyValuePair<ResourceType, List<ProcessJob>> harvestJob in zone.HarvestJobs)
			{
				if (harvestJob.Value.Count > 0)
				{
					tempJobsList.Add(harvestJob.Value[0]);
				}
			}
		}
		tempJobsList.AddRange(expedition.OwnedEntities.ScoutingJobs);
		tempJobsList.AddRange(expedition.OwnedEntities.FindPreyJobs);
		tempJobsList.AddRange(expedition.OwnedEntities.PatrolJobs);
		tempJobsList.AddRange(expedition.OwnedEntities.AttackAreaJobs);
		tempJobsList.AddRange(expedition.OwnedEntities.CheckProcessJobs);
		foreach (KeyValuePair<EntityID, List<ProcessJob>> repairJob in expedition.OwnedEntities.RepairJobs)
		{
			tempJobsList.AddRange(repairJob.Value);
		}
		foreach (Job tempJobs in tempJobsList)
		{
			allJobsToShow.Add(new Tuple<string, Job>(GetKey(tempJobs), tempJobs));
		}
		allJobsToShow = allJobsToShow.OrderBy((Tuple<string, Job> j) => j.Item2.Timestamp).ToList();
	}

	private bool GetProgressOfGroupedJobs(ProcessJob job, out float progress)
	{
		progress = 0f;
		if (job.HarvestJob.Zone.HarvestJobs.TryGetValue(job.HarvestJob.ResourceType, out var value))
		{
			float num = 0f;
			int num2 = 0;
			foreach (ProcessJob item in value)
			{
				if (item.GetKnownProgress(out var progress2))
				{
					num += progress2;
					num2++;
				}
			}
			if (num2 > 0)
			{
				progress = num / (float)num2;
				return true;
			}
			progress = 0f;
		}
		return true;
	}

	private bool GetProgress(ProcessJob job, out float thisProgress, out bool isMemory)
	{
		if (job.HarvestJob != null)
		{
			isMemory = false;
			return GetProgressOfGroupedJobs(job, out thisProgress);
		}
		IKnownProcess processData;
		bool knownProgress = job.GetKnownProgress(out thisProgress, out processData);
		if (processData is ProcessMemory)
		{
			isMemory = true;
			return knownProgress;
		}
		isMemory = false;
		return knownProgress;
	}

	private void UpdateTopPanel()
	{
		if (!cbTaskType.SelectedKey.Equals("SELECTTASKTYPE"))
		{
			JobType taskType = (JobType)cbTaskType.SelectedKey;
			FillTaskTypeControls(taskType);
		}
	}

	private void UpdateJobs(ExpandCollapse? expandOrCollapse)
	{
		GetAllJobsToShow();
		outerGrid.BeginAddingEntries();
		EntityGroup ownedEntities = The.Sim.PlaySite.GetFirstPlayerExpedition().OwnedEntities;
		foreach (Tuple<string, Job> item4 in allJobsToShow)
		{
			Job item = item4.Item2;
			string item2 = item4.Item1;
			Box box = null;
			box = (outerGrid.TryGetEntry(item2, out var item3) ? (item3 as Box) : AddItemRow(ownedEntities, item2, item));
			UpdateRow(box, item, ownedEntities, expandOrCollapse);
		}
		outerGrid.DeleteEntries((string j) => allJobsToShow.Exists((Tuple<string, Job> t) => t.Item1 == j));
		outerGrid.Sort((UIComponent i) => i.OrderByTag1, The.InGameUI.TaskSettings.SortingSettings.SortOrder);
		outerGrid.EndAddingEntries();
	}

	private string GetKey(Job job)
	{
		if (job is ProcessJob { HarvestJob: not null } processJob)
		{
			return processJob.HarvestJob.Zone.ID.ToString() + processJob.HarvestJob.ResourceType.KeyName;
		}
		return job.ID.ToString();
	}

	private void SetDataTypeButtonWidth(DataTypeButton button)
	{
		button.ScaleWidthToFitText();
		if (button.Width > 140)
		{
			button.Width = 140;
		}
	}

	private void AddControlsToCommonSection(LCDInnerPanel panel, EntityGroup owner, string key, Job job)
	{
		EntityType entityType = null;
		ProcessType processType = null;
		if (job is ProcessJob processJob)
		{
			if (processJob.ProcessType.IsSalvageProcess)
			{
				if (processJob.ProcessType.Inputs != null && processJob.ProcessType.Inputs.Length != 0)
				{
					entityType = processJob.ProcessType.Inputs[0].EntityType;
				}
			}
			else if (processJob.ReplenishJob != null)
			{
				Entity entity = Entity.FindByID(processJob.ReplenishJob.EntityToReplenish);
				if (entity != null)
				{
					entityType = ((!entity.ParentEntityID.HasValue) ? entity.EntityType : Entity.FindByID(entity.ParentEntityID).EntityType);
				}
			}
			else if (processJob.ProcessType.HasOutput)
			{
				entityType = processJob.ProcessType.Outputs[0].FinalEntityTypeToCreate;
			}
			else
			{
				processType = processJob.ProcessType;
			}
		}
		JobType jobType = job.GetJobType();
		Label label = new Label(Interface.gui);
		panel.AddContent(label, 0, -1);
		Label.LabelType labelType = GetLabelType(job);
		label.Init(labelType);
		SetTitle(job, jobType, label);
		label.ID = UIComponent.DataControlID.JobType;
		label.Width = 150;
		if (entityType != null)
		{
			DataTypeButton dataTypeButton = new DataTypeButton(Interface.gui, DataSheet.InfoToShow.Data, entityType, owner.ID, useUIOwner: false);
			dataTypeButton.Init(TextButton.TextButtonType.LCDToolTipBlack);
			dataTypeButton.ID = UIComponent.DataControlID.Caption;
			dataTypeButton.IsRoot = true;
			panel.AddContent(dataTypeButton);
			dataTypeButton.TextAlignment = TextButton.TextAlign.Left;
			SetDataTypeButtonWidth(dataTypeButton);
			dataTypeButton.X = label.Right + 6;
			dataTypeButton.CenterThisVertically(label.MiddleVertical);
			dataTypeButton.Tag1 = key;
			dataTypeButton.LabelColor = dataTypeButton.GetNormalColor();
		}
		else if (processType != null)
		{
			DataTypeButton dataTypeButton2 = new DataTypeButton(Interface.gui, DataSheet.InfoToShow.Data, processType, owner.ID, useUIOwner: false);
			dataTypeButton2.Init(TextButton.TextButtonType.LCDToolTipBlack);
			dataTypeButton2.ID = UIComponent.DataControlID.Caption;
			dataTypeButton2.IsRoot = true;
			panel.AddContent(dataTypeButton2);
			dataTypeButton2.TextAlignment = TextButton.TextAlign.Left;
			SetDataTypeButtonWidth(dataTypeButton2);
			dataTypeButton2.X = label.Right + 6;
			dataTypeButton2.CenterThisVertically(label.MiddleVertical);
			dataTypeButton2.Tag1 = key;
			dataTypeButton2.LabelColor = dataTypeButton2.GetNormalColor();
		}
		ImageButton imageButton = new ImageButton(Interface.gui);
		panel.AddContent(imageButton, 300, -4);
		imageButton.Init(ImageButtonType.LCDExpandWithUpAndDownArrows);
		imageButton.CheckedMode = CheckedModes.SwitchCheckedStateOnClick;
		imageButton.ToolTip = "Show more details";
		imageButton.Click += btnExpand_Click;
		imageButton.Tag1 = key;
		imageButton.Width = 28;
		imageButton.ID = UIComponent.DataControlID.Expand;
		FillableBar fillableBar = new FillableBar(Interface.gui, FillableBar.FillableBarType.ProgressBar);
		fillableBar.ShowMaxValueLabelAtEnd = false;
		panel.AddContent(fillableBar, imageButton.Right + 4, 4);
		fillableBar.ID = UIComponent.DataControlID.Progress;
		fillableBar.Width = 70;
		fillableBar.MaxValue = 100;
		fillableBar.Height = 12;
		fillableBar.BarColor = BaseDataLoader.ProgressColor;
		ComboBox comboBox = CreatePriorityComboBox();
		comboBox.ID = UIComponent.DataControlID.Priority;
		comboBox.Tag1 = GetKey(job);
		comboBox.SelectionChanged += cbPriority_SelectionChanged;
		panel.AddContent(comboBox, 418, -4);
		comboBox.ToolTip = "Set the priority for this task";
		ImageButton imageButton2 = new ImageButton(Interface.gui);
		panel.AddContent(imageButton2, 516, -2);
		imageButton2.InitWithIcon(ImageButtonType.LCD, "lcd_icon_asterisk", hasCheckedState: false, UIComponent.LCDNormal);
		imageButton2.Click += btApplyPriorityToAll_Click;
		imageButton2.Tag1 = key;
		imageButton2.Width = 28;
		imageButton2.ID = UIComponent.DataControlID.ApplyPriority;
		if (jobType == null)
		{
			imageButton2.Enabled = false;
			imageButton2.ToolTip = "Not possible to set default priority for this task type";
		}
		else
		{
			imageButton2.ToolTip = $"Click to set this priority as the default for all current and future tasks of this type ({jobType.GetDefaultDisplayName()})";
		}
		Icon icon = new Icon(Interface.gui);
		panel.AddContent(icon, -5);
		icon.SetSkinLocation(SkinState.Normal, Interface.gui.GUISpriteSheet.GetSourceRectangle("lcd_icon_person"), UIComponent.LCDNormal, UIComponent.LCDNormal);
		icon.ResizeControlToFitImage();
		icon.Y = 40;
		icon.ID = UIComponent.DataControlID.Skill;
		LinkOrLabel linkOrLabel = new LinkOrLabel(Interface.gui, Label.LabelType.LCDNormal);
		panel.AddContent(linkOrLabel, icon.Right - 7);
		linkOrLabel.Hyperlink.NormalColor = UIComponent.LCDNormal;
		linkOrLabel.Width = 120;
		linkOrLabel.Y = 42;
		linkOrLabel.ID = UIComponent.DataControlID.JobsPanelAssignedWorker;
		linkOrLabel.Text = "H";
		linkOrLabel.DebugTag = "lolAssignedWorker";
		Label label2 = new Label(Interface.gui);
		label2.Init(Label.LabelType.LCDNormal);
		panel.AddContent(label2);
		label2.Text = "LOCATION:";
		label2.FitToText();
		label2.Y = linkOrLabel.Y;
		label2.X = 180;
		Hyperlink hyperlink = new Hyperlink(Interface.gui);
		panel.AddContent(hyperlink);
		hyperlink.Initialize();
		hyperlink.NormalColor = UIComponent.LCDNormal;
		hyperlink.ID = UIComponent.DataControlID.Location;
		hyperlink.Y = label2.Y;
		hyperlink.X = label2.Right + 2;
		Label label3 = new Label(Interface.gui);
		label3.Init(Label.LabelType.LCDNormal);
		panel.AddContent(label3);
		label3.Text = "STATUS:";
		label3.FitToText();
		label3.Y = linkOrLabel.Bottom + 8;
		label3.ID = UIComponent.DataControlID.JobStatus;
		string tooltip;
		bool flag = job.UserCanCancel(out tooltip);
		ImageButton imageButton3 = new ImageButton(Interface.gui);
		panel.AddContent(imageButton3);
		imageButton3.InitWithIcon(ImageButtonType.LCD, "lcd_icon_trash", hasCheckedState: false);
		imageButton3.ToolTip = (flag ? "Cancel this task" : tooltip);
		imageButton3.X = 370;
		imageButton3.Y = linkOrLabel.Y - 4;
		imageButton3.Click += btCancel_Click;
		imageButton3.Tag1 = key;
		imageButton3.Enabled = flag;
		imageButton3.ID = UIComponent.DataControlID.Cancel;
		imageButton3.Width = 24;
	}

	private static void SetTitle(Job job, JobType jobType, Label lblJobType)
	{
		if (jobType != null)
		{
			lblJobType.Text = jobType.GetDefaultDisplayName().ToUpper(Config.Culture);
		}
		else
		{
			lblJobType.Text = job.GetName().ToUpper(Config.Culture);
		}
	}

	private void btApplyPriorityToAll_Click(UIComponent sender, EventArgs e)
	{
		Job jobFromKey = GetJobFromKey((string)sender.Tag1);
		JobType jobType = jobFromKey.GetJobType();
		Command command = new SetJobTypePriority(expedition.OwnedEntities, jobType.KeyName, jobFromKey.Priority, updateInstances: true);
		The.Client.Controller.StoreAndExecuteCommand(command);
		UpdateJobs(null);
		UpdateTopPanel();
	}

	private void UpdateMessages()
	{
		taMessages.BeginAddingEntries();
		taMessages.Text = null;
		if (ProductionOrderControl.TotalJobsRequireWarning(expedition.OwnedEntities))
		{
			foreach (KeyValuePair<EntityType, List<ProcessJob>> productionJob in expedition.OwnedEntities.ProductionJobs)
			{
				if (productionJob.Value.Count > GameData.Instance.GUIConstants.OrderedJobsWithSameOutputToTriggerWarning)
				{
					TextArea textArea = taMessages;
					textArea.Text = textArea.Text + "- " + productionJob.Key.PluralName + ": Ordering many single items can take a while to produce. Try to look for ways to produce in larger batches, as this cuts down on the production time. \n";
				}
			}
		}
		foreach (KeyValuePair<string, JobsMessage> message in messages)
		{
			TextArea textArea2 = taMessages;
			textArea2.Text = textArea2.Text + "- " + message.Value.Text + " \n";
		}
		taMessages.EndAddingEntries();
	}

	private ComboBox CreatePriorityComboBox()
	{
		ComboBox comboBox = new ComboBox(Interface.gui, ListBoxType.LCDCombo, isEditable: false);
		comboBox.Init(ComboBoxTypes.LCD);
		comboBox.Width = 100;
		comboBox.BeginAddingEntries();
		comboBox.AddEntry(UWGame.SimSide.Jobs.Priority.Low, Job.GetPriorityAsString(UWGame.SimSide.Jobs.Priority.Low));
		comboBox.AddEntry(UWGame.SimSide.Jobs.Priority.Normal, Job.GetPriorityAsString(UWGame.SimSide.Jobs.Priority.Normal));
		comboBox.AddEntry(UWGame.SimSide.Jobs.Priority.High, Job.GetPriorityAsString(UWGame.SimSide.Jobs.Priority.High));
		comboBox.EndAddingEntries();
		return comboBox;
	}

	private void cbPriority_SelectionChanged(UIComponent sender)
	{
		Job jobFromKey = GetJobFromKey((string)sender.Tag1);
		if (jobFromKey.ID == JobID.Invalid)
		{
			return;
		}
		ProcessJob processJob = jobFromKey as ProcessJob;
		List<Job> list = null;
		if (processJob != null && processJob.HarvestJob != null)
		{
			if (processJob.HarvestJob.Zone.HarvestJobs.TryGetValue(processJob.HarvestJob.ResourceType, out var value))
			{
				list = new List<Job>();
				list.AddRange(value);
			}
		}
		else
		{
			Common.AddToList(ref list, jobFromKey);
		}
		UWGame.SimSide.Jobs.Priority jobPriority = (UWGame.SimSide.Jobs.Priority)((ComboBox)sender).SelectedKey;
		foreach (Job item in list)
		{
			Command command = new SetTaskPriority(item.ID, jobPriority);
			The.Client.Controller.StoreAndExecuteCommand(command);
		}
	}

	private void CreateGridHeaderButtons()
	{
		sortingButtonsContainer = new UIComponent(Interface.gui);
		lcdSurface.Add(sortingButtonsContainer);
		sortingButtonsContainer.Width = lcdSurface.Width;
		sortingButtonsContainer.Height = 28;
		sortingButtonsContainer.Position = new Point(0, topPanel.Panel.Bottom);
		sortingButtons = new SortingButtons<TaskSettings.SortColumns>(Interface.gui);
		sortingButtons.Width = lcdSurface.Width;
		sortingButtons.Height = 50;
		sortingButtons.Position = new Point(60, 0);
		sortingButtonsContainer.Add(sortingButtons);
		sortingButtons.SortClicked += tbSort_Click;
		ibCompress = new ImageButton(Interface.gui);
		sortingButtonsContainer.Add(ibCompress);
		ibCompress.InitWithIcon(ImageButtonType.LCD, "lcd_icon_allCollapse", hasCheckedState: true);
		ibCompress.CheckedMode = CheckedModes.CannotBeChecked;
		ibCompress.Click += ibCompress_Click;
		ibCompress.Y = 0;
		ibCompress.X = 0;
		ibCompress.ToolTip = "Collapse all";
		ibCompress.Width = 30;
		ibCompress.Height = 30;
		ibCompress.RecalculateIconPosition();
		ibExpand = new ImageButton(Interface.gui);
		sortingButtonsContainer.Add(ibExpand);
		ibExpand.InitWithIcon(ImageButtonType.LCD, "lcd_icon_allExpand", hasCheckedState: true);
		ibExpand.CheckedMode = CheckedModes.CannotBeChecked;
		ibExpand.Click += ibExpand_Click;
		ibExpand.ToolTip = "Expand all";
		ibExpand.Width = 30;
		ibExpand.Y = 0;
		ibExpand.X = 30;
		ibExpand.Height = 30;
		ibExpand.RecalculateIconPosition();
		sortingButtons.CreateTextButton(0, 248, "TASK TYPE", TaskSettings.SortColumns.TaskType);
		sortingButtons.CreateTextButton(244, 124, "COMPLETION", TaskSettings.SortColumns.Completion);
		sortingButtons.CreateTextButton(364, 136, "PRIORITY", TaskSettings.SortColumns.Priority);
	}

	private void ibExpand_Click(UIComponent sender, EventArgs e)
	{
		UpdateJobs(ExpandCollapse.Expand);
	}

	private void ibCompress_Click(UIComponent sender, EventArgs e)
	{
		UpdateJobs(ExpandCollapse.Collapse);
	}

	private void tbSort_Click()
	{
		UpdateJobs(null);
	}

	private RadioGroup CreatePriorityButtons(out RadioButton btNormal, out RadioButton btHigh, out RadioButton btLow)
	{
		RadioGroup obj = new RadioGroup(Interface.gui)
		{
			Width = 100,
			Height = 80
		};
		btNormal = new RadioButton(Interface.gui);
		btLow = new RadioButton(Interface.gui);
		btHigh = new RadioButton(Interface.gui);
		obj.Add(btLow);
		btLow.Init(CheckBoxType.LCDRadioBanner);
		btLow.X = 0;
		btLow.Y = 5;
		btLow.Text = "LOW";
		btLow.ToolTip = "Set to low priority";
		btLow.Tag1 = UWGame.SimSide.Jobs.Priority.Low;
		btLow.Width = 90;
		btLow.ID = UIComponent.DataControlID.PriorityLow;
		obj.Add(btNormal);
		btNormal.Init(CheckBoxType.LCDRadioBanner);
		btNormal.X = btLow.X;
		btNormal.Y = btLow.Bottom + 4;
		btNormal.Text = "NORMAL";
		btNormal.ToolTip = "Set to normal priority";
		btNormal.IsChecked = true;
		btNormal.Tag1 = UWGame.SimSide.Jobs.Priority.Normal;
		btNormal.Width = btLow.Width;
		btNormal.ID = UIComponent.DataControlID.PriorityNormal;
		obj.Add(btHigh);
		btHigh.Init(CheckBoxType.LCDRadioBanner);
		btHigh.X = btLow.X;
		btHigh.Y = btNormal.Bottom + 4;
		btHigh.Text = "HIGH";
		btHigh.ToolTip = "Set to high priority";
		btHigh.Tag1 = UWGame.SimSide.Jobs.Priority.High;
		btHigh.Width = btLow.Width;
		btHigh.ID = UIComponent.DataControlID.PriorityHigh;
		return obj;
	}

	private void btnExpand_Click(UIComponent sender, EventArgs e)
	{
		ImageButton imageButton = sender as ImageButton;
		outerGrid.TryGetEntry(sender.Tag1, out var item);
		ExpandOrCollapseRow(imageButton.IsChecked, item);
	}

	private void ExpandOrCollapseRow(bool expand, UIComponent rowPanel)
	{
		ImageButton imageButton = (ImageButton)rowPanel.FindChildById(UIComponent.DataControlID.Expand);
		if (expand)
		{
			ResizeExpandedPanel(rowPanel);
			imageButton.SetIconSkinState(1);
		}
		else
		{
			rowPanel.Height = 35;
			imageButton.SetIconSkinState(0);
		}
	}

	private Label.LabelType GetLabelType(Job job)
	{
		JobLabelTypes jobLabelTypes = JobLabelTypes.SteelGrey;
		JobType jobType = job.GetJobType();
		if (jobType != null && jobType.LabelType.HasValue)
		{
			jobLabelTypes = jobType.LabelType.Value;
		}
		else if (job is ProcessJob processJob)
		{
			jobLabelTypes = ((processJob.SalvageJob != null) ? JobLabelTypes.Red : ((processJob.HarvestJob != null) ? JobLabelTypes.Green : ((processJob.BuildingJob == null) ? JobLabelTypes.Grey : JobLabelTypes.Blue)));
		}
		return jobLabelTypes switch
		{
			JobLabelTypes.Blue => Label.LabelType.LCDHeadingBlue, 
			JobLabelTypes.Brown => Label.LabelType.LCDHeadingBrown, 
			JobLabelTypes.Green => Label.LabelType.LCDHeadingGreen, 
			JobLabelTypes.Grey => Label.LabelType.LCDHeadingGrey, 
			JobLabelTypes.Red => Label.LabelType.LCDHeadingRed, 
			JobLabelTypes.SteelGrey => Label.LabelType.LCDHeadingSteelGrey, 
			_ => Label.LabelType.LCDHeadingBrown, 
		};
	}

	private bool JobHasProblems(Job job, EntityGroup owner, out bool isInaccessible, out bool blockedByThreat, out bool isBlockedDueToBoldStanceRequired, out bool? hasTools, out bool? hasSkill, out bool? hasWeapon, out bool tooFarAwayFromExpedition, out bool huntingNotFeasible, out bool areaNotCleared)
	{
		bool result = false;
		The.Client.GetFeedback(job.ID, out isInaccessible, out blockedByThreat, out isBlockedDueToBoldStanceRequired, out tooFarAwayFromExpedition, out huntingNotFeasible, out areaNotCleared);
		hasTools = null;
		hasSkill = null;
		hasWeapon = null;
		if (isInaccessible | tooFarAwayFromExpedition | huntingNotFeasible | areaNotCleared)
		{
			result = true;
		}
		if (job is ProcessJob processJob)
		{
			if (!processJob.ToolsAreAvailable || !InventoryPanel.HasToolsForProcess(processJob.ProcessType, owner))
			{
				hasTools = false;
				result = true;
			}
			else
			{
				hasTools = true;
			}
			if (owner.GetExpedition().HasSkill(processJob.ProcessType.RequiredSkillType))
			{
				hasSkill = true;
			}
			else
			{
				hasSkill = false;
				result = true;
			}
		}
		if (job is IRequiresWeapon requiresWeapon)
		{
			if (!requiresWeapon.WeaponsAreAvailable)
			{
				hasWeapon = false;
				result = true;
			}
			else
			{
				hasWeapon = true;
			}
		}
		return result;
	}

	private void UpdateCollapsedPart(Box cpJob, Job job, EntityGroup owner, out bool hasProblems, out bool? hasTools, out float? progress)
	{
		progress = null;
		cpJob.FindChildById(UIComponent.DataControlID.JobType);
		ProcessJob processJob = job as ProcessJob;
		hasProblems = JobHasProblems(job, owner, out var isInaccessible, out var blockedByThreat, out var isBlockedDueToBoldStanceRequired, out hasTools, out var hasSkill, out var hasWeapon, out var tooFarAwayFromExpedition, out var huntingNotFeasible, out var areaNotCleared);
		UpdateWorkerAndSkill(cpJob, job, hasSkill);
		((ComboBox)cpJob.FindChildById(UIComponent.DataControlID.Priority)).SelectedKey = job.Priority;
		Label label = cpJob.FindChildById(UIComponent.DataControlID.JobStatus) as Label;
		string text = null;
		string text2 = null;
		Color normalColor = label.GetNormalColorForType();
		if (hasWeapon == false)
		{
			text = "No weapon available";
			text2 = "No suitable weapon is available for this task.";
		}
		else if (isInaccessible)
		{
			if (blockedByThreat)
			{
				text = "Dangerous area";
				text2 = "The task is located in a dangerous spot. Suggestion: First secure the area with PATROL/ATTACK zones. Use the THREAT overlay (next to the minimap) to highlight dangerous areas.";
			}
			else if (isBlockedDueToBoldStanceRequired)
			{
				text = "Noone dares go near";
				text2 = "No workers are currently willing to move near danger, perhaps because of injuries and/or low morale";
			}
			else
			{
				text = "Area not accessible";
				text2 = "The area is geographically inaccessible because of terrain or structures blocking the way";
			}
			normalColor = Label.LCDErrorColor;
		}
		else if (tooFarAwayFromExpedition)
		{
			text = "Too far away from camp";
			text2 = "The target is too far away from the camp.";
			normalColor = Label.LCDErrorColor;
		}
		else if (huntingNotFeasible)
		{
			text = "Ranged weapon needed";
			text2 = "No workers have the speed to hunt down this animal after it has spotted us.";
			normalColor = Label.LCDErrorColor;
		}
		else if (areaNotCleared)
		{
			text = "Area not cleared";
			text2 = "The construction area contains items that must be removed first.";
			normalColor = Label.LCDErrorColor;
		}
		else
		{
			label.NormalColor = label.GetNormalColorForType();
		}
		FillableBar fillableBar = cpJob.FindChildById(UIComponent.DataControlID.Progress) as FillableBar;
		fillableBar.Visible = false;
		if (processJob != null)
		{
			if (text == null && GetProgress(processJob, out var thisProgress, out var isMemory))
			{
				progress = thisProgress;
				fillableBar.Visible = true;
				fillableBar.Value = (int)(100f * progress.Value);
				PresentationType presentationType = GameData.Instance.AllPresentationTypes["ProcessProgress"];
				if (isMemory)
				{
					fillableBar.BarColor = Color.Gray;
					fillableBar.ToolTip = presentationType.GetValueTerm(progress.Value, null, null) + " (last known status)";
				}
				else
				{
					fillableBar.BarColor = BaseDataLoader.ProgressColor;
					fillableBar.ToolTip = presentationType.GetValueTerm(progress.Value, null, null);
				}
				fillableBar.UnderBarColor = GetProgressTint(job, out var _);
				text = "";
			}
			if (cpJob.FindChildById(UIComponent.DataControlID.Cancel) is TextButton textButton && processJob.CumulativelyEstimatedProgress > 0f && textButton.Enabled)
			{
				textButton.Enabled = false;
				textButton.ToolTip = "Cannot cancel this ongoing task.";
			}
		}
		if (text != null)
		{
			label.Text = "STATUS: " + text;
			if (text2 != null)
			{
				label.ToolTip = text2;
			}
		}
		else
		{
			label.Text = "";
			label.ToolTip = "";
		}
		label.NormalColor = normalColor;
		Hyperlink hyperlink = cpJob.FindChildById(UIComponent.DataControlID.Location) as Hyperlink;
		string text3 = null;
		job.GetLocation(out var tilePos, out var targetEntity, out var zoneID);
		hyperlink.TargetMapPosition = null;
		hyperlink.TargetEntityID = null;
		hyperlink.TargetZoneID = null;
		if (tilePos.HasValue)
		{
			hyperlink.TargetMapPosition = tilePos.Value;
			text3 = MapManager.TilePosToString(tilePos.Value);
		}
		else if (targetEntity.HasValue)
		{
			hyperlink.TargetEntityID = (uint)targetEntity.Value;
			if (!GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(targetEntity.Value, out var data)))
			{
				text3 = data.GetDisplayName();
			}
		}
		else if (zoneID.HasValue)
		{
			hyperlink.TargetZoneID = (uint)zoneID.Value;
			Zone zone = LookUp<Zone, ZoneID>.FindByID(zoneID);
			if (zone != null)
			{
				text3 = zone.GetDisplayName();
			}
		}
		if (text3 != null)
		{
			hyperlink.Text = text3;
			hyperlink.Enabled = true;
		}
		else
		{
			hyperlink.Text = "NONE";
			hyperlink.ToolTip = "The task has no location yet.";
			hyperlink.Enabled = false;
		}
		hyperlink.ScaleWidthToFitText();
		int num = 115;
		if (hyperlink.Width > num)
		{
			hyperlink.Width = Math.Min(hyperlink.Width, num);
		}
	}

	public static Color GetProgressTint(Job job, out bool isActive)
	{
		bool flag = job.TakenBy.Count > 0;
		if (job is ProcessJob processJob)
		{
			if (processJob.IsStarted(out var isStarted) && isStarted)
			{
				if (processJob.ProcessType.WorkNeeded == WorkerNeededOptions.WorkerNeeded && !flag)
				{
					isActive = false;
					return notActiveColor;
				}
				isActive = true;
				return activeColor;
			}
			if (!flag)
			{
				isActive = false;
				return notActiveColor;
			}
			isActive = true;
			return activeColor;
		}
		if (!flag)
		{
			isActive = false;
			return notActiveColor;
		}
		isActive = true;
		return activeColor;
	}

	private static void UpdateWorkerAndSkill(Box cpJob, Job job, bool? hasSkill)
	{
		Icon icon = cpJob.FindChildById(UIComponent.DataControlID.Skill) as Icon;
		LinkOrLabel linkOrLabel = cpJob.FindChildById(UIComponent.DataControlID.JobsPanelAssignedWorker) as LinkOrLabel;
		linkOrLabel.Label.NormalColor = linkOrLabel.Label.GetNormalColorForType();
		linkOrLabel.ToolTip = null;
		Entity assignedWorker = job.GetAssignedWorker();
		string text = null;
		string toolTip = null;
		if (job is ProcessJob processJob && processJob.ProcessType.RequiredSkillType != null)
		{
			text = processJob.ProcessType.RequiredSkillType.Name;
			toolTip = processJob.ProcessType.RequiredSkillType.Description;
		}
		if (assignedWorker != null)
		{
			linkOrLabel.Mode = LinkOrLabel.Modes.Link;
			linkOrLabel.Text = assignedWorker.Name ?? assignedWorker.EntityType.Name;
			linkOrLabel.Hyperlink.TargetEntityID = (uint)assignedWorker.ID;
			if (text != null)
			{
				icon.ToolTip = $"This worker supplies the needed '{text}' skill";
			}
			else
			{
				icon.ToolTip = null;
			}
			return;
		}
		linkOrLabel.Mode = LinkOrLabel.Modes.Label;
		linkOrLabel.Hyperlink.TargetEntityID = null;
		if (text != null)
		{
			linkOrLabel.Text = text;
			icon.ToolTip = $"A worker with '{text}' skill is needed";
			if (hasSkill == false)
			{
				linkOrLabel.Label.NormalColor = Label.LCDErrorColor;
				linkOrLabel.ToolTip = $"There are no colony members with this skill (Requirement: Skill level above {GameData.Instance.Constants.MinimumSkillValueToUse:N1})";
			}
			else
			{
				linkOrLabel.ToolTip = toolTip;
			}
		}
		else
		{
			linkOrLabel.Text = "NO ONE ASSIGNED";
		}
	}

	private UIComponent AddAssignedItem(Grid grid, IKnownEntityData itemData)
	{
		UIComponent uIComponent = new UIComponent(The.InGameUI.gui);
		Hyperlink hyperlink = AddHyperLink(The.InGameUI.gui, (uint)itemData.EntityID, itemData.EntityType.Name, 0);
		uIComponent.Add(hyperlink);
		hyperlink.MaxWidth = 147;
		Label label = new Label(The.InGameUI.gui);
		label.Init(Label.LabelType.LCDNormal);
		uIComponent.Add(label);
		label.X = 150;
		label.ID = UIComponent.DataControlID.Status;
		grid.AddEntry(itemData.EntityID, uIComponent);
		return uIComponent;
	}

	private void UpdateAllAssignedItems(Grid grid, ProcessJob pJob, EntityGroup owner)
	{
		if (!pJob.GetAssignedInputs(out var inputs))
		{
			return;
		}
		if (inputs != null && inputs.Count > 0)
		{
			foreach (KeyValuePair<EntityType, List<Tuple<EntityID, WorldLocation>>> item2 in inputs)
			{
				for (int num = item2.Value.Count - 1; num >= 0; num--)
				{
					EntityID item = item2.Value[num].Item1;
					if (GoalEvaluator.HandleOwnerDataResult(The.InGameUI.UIAllegiance.SharedKnowledge, item, owner, out var entityData))
					{
						UpdateAssignedInputs(grid, entityData, "On site");
					}
					else
					{
						item2.Value.RemoveAt(num);
					}
				}
			}
		}
		if (pJob.InputsBeingHauled.Count > 0)
		{
			foreach (KeyValuePair<EntityType, List<EntityID>> item3 in pJob.InputsBeingHauled)
			{
				for (int num2 = item3.Value.Count - 1; num2 >= 0; num2--)
				{
					EntityID item = item3.Value[num2];
					if (GoalEvaluator.HandleOwnerDataResult(The.InGameUI.UIAllegiance.SharedKnowledge, item, owner, out var entityData2))
					{
						UpdateAssignedInputs(grid, entityData2, "In transit");
					}
					else
					{
						item3.Value.RemoveAt(num2);
					}
				}
			}
		}
		grid.DeleteEntries((EntityID j) => IsAssignedAndOnSite(j, inputs) || Common.MultiListContains(pJob.InputsBeingHauled, null, j));
	}

	private bool IsAssignedAndOnSite(EntityID entityID, Dictionary<EntityType, List<Tuple<EntityID, WorldLocation>>> inputs)
	{
		foreach (KeyValuePair<EntityType, List<Tuple<EntityID, WorldLocation>>> input in inputs)
		{
			if (input.Value.Exists((Tuple<EntityID, WorldLocation> t) => t.Item1 == entityID))
			{
				return true;
			}
		}
		return false;
	}

	private void GetInputStatus(ProcessJob pJob, Input input, out int unassigned, out int inTransit, out int onSite)
	{
		if (!pJob.GetAssignedInputs(out var inputs))
		{
			unassigned = 0;
			inTransit = 0;
			onSite = 0;
			return;
		}
		if (inputs.TryGetValue(input.EntityType, out var value))
		{
			onSite = value.Count;
		}
		else
		{
			onSite = 0;
		}
		if (pJob.InputsBeingHauled.TryGetValue(input.EntityType, out var value2))
		{
			inTransit = value2.Count;
		}
		else
		{
			inTransit = 0;
		}
		if (pJob.IsStarted(out var isStarted) && isStarted)
		{
			unassigned = 0;
		}
		else
		{
			unassigned = input.Amount.NoOfItems.Value - inTransit - onSite;
		}
	}

	private bool UpdateHarvestJobResources(EntityGroup owner, Grid grid, ProcessJob pJob, ref bool setTitleColorToRed)
	{
		int num = 0;
		int num2 = 0;
		Zone zone = pJob.HarvestJob.Zone;
		if (zone != null && zone.HasHarvestJobs())
		{
			foreach (ProcessJob item2 in pJob.HarvestJob.Zone.HarvestJobs[pJob.HarvestJob.ResourceType])
			{
				if (The.Client.GetIsInaccessible(item2.ID))
				{
					num++;
				}
				else
				{
					num2++;
				}
			}
		}
		if (num2 <= 0 && num > 0)
		{
			setTitleColorToRed = true;
		}
		string key = "accessible";
		string key2 = "inaccessible";
		UIComponent item;
		if (num2 > 0)
		{
			if (!grid.TryGetEntry(key, out item))
			{
				item = AddEntityTypeRow(pJob.HarvestJob.ResourceType.ResourceItemType, key, owner, grid);
			}
			UpdateEntityTypeRow(item, num2, "Accessible", highlightMoreInfo: false);
		}
		else
		{
			grid.TryRemoveEntry(key);
		}
		if (num > 0)
		{
			if (!grid.TryGetEntry(key2, out item))
			{
				item = AddEntityTypeRow(pJob.HarvestJob.ResourceType.ResourceItemType, key2, owner, grid);
			}
			UpdateEntityTypeRow(item, num, "Inaccessible", highlightMoreInfo: true);
		}
		else
		{
			grid.TryRemoveEntry(key2);
		}
		return true;
	}

	private void UpdateProcessJobInputs(Box cPanel, ProcessJob pJob, EntityGroup owner, Grid grid, ref bool setTitlebarColorToRed)
	{
		grid.TryGetEntry("unassigned", out var item);
		Grid grid2 = item as Grid;
		grid2.BeginAddingEntries();
		grid.TryGetEntry("assigned", out var item2);
		Grid grid3 = item2 as Grid;
		grid3.BeginAddingEntries();
		UpdateAllUnassignedInputs(pJob, owner, ref setTitlebarColorToRed, grid2);
		UpdateAllAssignedItems(grid3, pJob, owner);
		grid2.EndAddingEntries();
		grid3.EndAddingEntries();
	}

	private void UpdateAllUnassignedInputs(ProcessJob pJob, EntityGroup owner, ref bool setTitlebarColorToRed, Grid grdUnassignedInputs)
	{
		Input[] inputs = pJob.ProcessType.Inputs;
		foreach (Input input in inputs)
		{
			UpdateUnassignedInputs(grdUnassignedInputs, input, pJob, owner, ref setTitlebarColorToRed);
		}
	}

	private string GetMaterialStatus(int takenBy, bool hasInputs)
	{
		if (takenBy > 0)
		{
			return "On site";
		}
		if (hasInputs)
		{
			return "In inventory";
		}
		return "Not in inventory";
	}

	private void UpdateLeftColumn(Box cPanel, ProcessJob pJob, EntityGroup owner, ref bool hasProblems)
	{
		bool setTitlebarColorToRed = false;
		Label label = (Label)cPanel.FindChildById(UIComponent.DataControlID.InputHeading);
		if (pJob.HarvestJob != null)
		{
			label.Text = "RESOURCES";
			label.ToolTip = "This column shows a summary of the resources being gathered";
			Grid grid = (Grid)cPanel.FindChildById(UIComponent.DataControlID.LeftGrid);
			grid.BeginAddingEntries();
			UpdateHarvestJobResources(owner, grid, pJob, ref setTitlebarColorToRed);
			grid.EndAddingEntries();
		}
		else if (pJob != null && pJob.ProcessType.Inputs != null)
		{
			label.Text = "MATERIALS";
			label.ToolTip = "This column shows the status of the needed input materials";
			Grid grid2 = (Grid)cPanel.FindChildById(UIComponent.DataControlID.LeftGrid);
			grid2.BeginAddingEntries();
			UpdateProcessJobInputs(cPanel, pJob, owner, grid2, ref setTitlebarColorToRed);
			grid2.EndAddingEntries();
		}
		label.Width = 230;
		if (setTitlebarColorToRed)
		{
			hasProblems = true;
		}
	}

	private void ResizeExpandedPanel(UIComponent itemRow)
	{
		int num = 0;
		foreach (UIComponent control in itemRow.Controls)
		{
			if (control.Visible)
			{
				num = Common.Max(num, control.Bottom);
			}
		}
		itemRow.Height = num;
	}

	private void UpdateToolsColumn(UIComponent itemRow, ProcessJob pJob, EntityGroup owner, bool? hasTools)
	{
		EntityID entityID = EntityID.Invalid;
		Grid grid = (Grid)itemRow.FindChildById(UIComponent.DataControlID.Tools);
		Label label = (Label)itemRow.FindChildById(UIComponent.DataControlID.ToolsError);
		grid.BeginAddingEntries();
		List<EntityID> AssignedTools = new List<EntityID>();
		if (pJob.HarvestJob != null)
		{
			foreach (KeyValuePair<ResourceType, List<ProcessJob>> harvestJob in pJob.HarvestJob.Zone.HarvestJobs)
			{
				if (harvestJob.Key != pJob.HarvestJob.ResourceType)
				{
					continue;
				}
				foreach (ProcessJob item2 in harvestJob.Value)
				{
					foreach (EntityID assignedTool in item2.AssignedTools)
					{
						AssignedTools.Add(assignedTool);
					}
				}
			}
		}
		else
		{
			AssignedTools = pJob.AssignedTools;
		}
		bool? flag = hasTools;
		bool flag2 = true;
		if (flag == true == flag2 && flag.HasValue && pJob.ToolsAreAvailable)
		{
			label.Visible = false;
			grid.Visible = true;
			for (int num = AssignedTools.Count - 1; num >= 0; num--)
			{
				entityID = AssignedTools[num];
				if (GoalEvaluator.HandleOwnerDataResult(The.InGameUI.UIAllegiance.SharedKnowledge, entityID, owner, out var entityData) && !grid.TryGetEntry(entityID, out var _))
				{
					AddToolEntry(grid, entityData);
				}
			}
		}
		else
		{
			label.Visible = true;
			grid.Visible = false;
			if (pJob.AllToolsInUse)
			{
				label.Text = allToolsInUse;
			}
			else if (pJob.AllToolsAreBroken)
			{
				label.Text = allToolsBroken;
			}
			else
			{
				label.Text = noTools;
			}
			label.Height = 23;
		}
		grid.DeleteEntries((EntityID e) => AssignedTools.Contains(e));
		grid.EndAddingEntries();
	}

	private void AddToolEntry(Grid toolGrid, IKnownEntityData tool)
	{
		UIComponent uIComponent = new UIComponent(Interface.gui);
		Hyperlink control = AddHyperLink(Interface.gui, (uint)tool.EntityID, tool.EntityType.Name, 0);
		uIComponent.Add(control);
		toolGrid.AddEntry(tool.EntityID, uIComponent);
	}

	private void AddLabelEntry(Grid grid, string labelText, Color color, string key)
	{
		UIComponent uIComponent = new UIComponent(Interface.gui);
		Label label = new Label(Interface.gui);
		label.Init(Label.LabelType.LCDNormal);
		label.NormalColor = color;
		label.Text = labelText;
		label.ID = UIComponent.DataControlID.Status;
		uIComponent.Add(label);
		grid.AddEntry(key, uIComponent);
	}

	private Hyperlink AddHyperLink(GUIManager guiManager, uint targetID, string text, int xOffSet)
	{
		Hyperlink hyperlink = new Hyperlink(guiManager, RenderType.Normal);
		hyperlink.Initialize();
		hyperlink.NormalColor = UIComponent.LCDNormal;
		hyperlink.Text = text;
		hyperlink.X += xOffSet;
		hyperlink.ID = UIComponent.DataControlID.Caption;
		hyperlink.TargetEntityID = targetID;
		return hyperlink;
	}

	private UIComponent AddEntityTypeRow(EntityType typeToShow, object key, EntityGroup owner, Grid materialsGrid)
	{
		UIComponent uIComponent = new UIComponent(The.InGameUI.gui);
		Label label = new Label(uIComponent.guiManager);
		label.Init(Label.LabelType.LCDNormal);
		uIComponent.Add(label);
		label.FitToText();
		label.X = 0;
		label.ID = UIComponent.DataControlID.Amount;
		DataTypeButton dataTypeButton = new DataTypeButton(uIComponent.guiManager, DataSheet.InfoToShow.Production, typeToShow, owner.ID, useUIOwner: false);
		dataTypeButton.Init(TextButton.TextButtonType.LCDToolTipBlack);
		dataTypeButton.IsRoot = true;
		uIComponent.Add(dataTypeButton);
		dataTypeButton.TextAlignment = TextButton.TextAlign.Left;
		dataTypeButton.Width = 100;
		dataTypeButton.X = 20;
		dataTypeButton.LabelColor = dataTypeButton.GetNormalColor();
		dataTypeButton.Tag1 = typeToShow;
		dataTypeButton.Width = 128;
		Label label2 = new Label(uIComponent.guiManager);
		label2.Init(Label.LabelType.LCDNormal);
		uIComponent.Add(label2);
		label2.FitToText();
		label2.X = 150;
		label2.ID = UIComponent.DataControlID.Status;
		materialsGrid.AddEntry(key, uIComponent);
		return uIComponent;
	}

	private void UpdateEntityTypeRow(UIComponent row, int amount, string moreInfo, bool highlightMoreInfo)
	{
		((Label)row.FindChildById(UIComponent.DataControlID.Amount)).Text = amount.ToString();
		Label label = (Label)row.FindChildById(UIComponent.DataControlID.Status);
		label.Text = moreInfo;
		if (highlightMoreInfo)
		{
			label.NormalColor = Label.LCDErrorColor;
		}
		else
		{
			label.NormalColor = label.GetNormalColorForType();
		}
	}

	private void UpdateUnassignedInputs(Grid grdUnassigned, Input input, ProcessJob pJob, EntityGroup owner, ref bool highlightParent)
	{
		GetInputStatus(pJob, input, out var unassigned, out var _, out var _);
		if (unassigned > 0)
		{
			if (!grdUnassigned.TryGetEntry(input.EntityType, out var item))
			{
				item = AddEntityTypeRow(input.EntityType, input.EntityType, owner, grdUnassigned);
			}
			UpdateUnassignedInputRow(item, input, pJob, owner, unassigned, ref highlightParent);
		}
		else
		{
			grdUnassigned.TryRemoveEntry(input.EntityType);
		}
	}

	private void UpdateUnassignedInputRow(UIComponent row, Input input, ProcessJob pJob, EntityGroup owner, int amount, ref bool highlightParent)
	{
		bool hasInputs = false;
		InventoryPanel.HasInputForProcess(pJob.ProcessType, owner, out hasInputs, out var _, out var _, out var _, out var _, input.EntityType);
		string materialStatus = GetMaterialStatus(pJob.TakenBy.Count, hasInputs);
		bool flag = !hasInputs;
		UpdateEntityTypeRow(row, amount, materialStatus, flag);
		if (flag)
		{
			highlightParent = true;
		}
	}

	private void UpdateAssignedInputs(Grid grid, IKnownEntityData itemData, string statusText)
	{
		if (!grid.TryGetEntry(itemData.EntityID, out var item))
		{
			item = AddAssignedItem(grid, itemData);
		}
		((Label)item.FindChildById(UIComponent.DataControlID.Status)).Text = statusText;
	}

	private Box AddItemRow(EntityGroup owner, string jobKey, Job job)
	{
		LCDInnerPanel rowPanel = null;
		AddRowPanel(ref rowPanel, jobKey);
		AddControlsToCommonSection(rowPanel, owner, jobKey, job);
		AddToolsAndMaterialsControls(job, rowPanel);
		return rowPanel.Panel;
	}

	private void AddToolsAndMaterialsControls(Job job, LCDInnerPanel collapsablePanel)
	{
		if (job is ProcessJob)
		{
			int height = 20;
			UIComponent uIComponent = new UIComponent(Interface.gui);
			uIComponent.Height = height;
			uIComponent.Width = collapsablePanel.Panel.Width;
			collapsablePanel.AddContent(uIComponent, 0, 82);
			Label label = new Label(uIComponent.guiManager);
			label.Init(Label.LabelType.LCDSmallHeadingBanner);
			uIComponent.Add(label);
			label.Width = 230;
			label.ID = UIComponent.DataControlID.InputHeading;
			Label label2 = new Label(uIComponent.guiManager);
			label2.Init(Label.LabelType.LCDSmallHeadingBanner);
			uIComponent.Add(label2);
			label2.Text = "TOOLS";
			label2.Width = 297;
			label2.X = label.Right + 10;
			Grid grid = null;
			int num = uIComponent.Bottom - 6;
			ProcessJob processJob = job as ProcessJob;
			if (processJob.HarvestJob != null)
			{
				grid = AddColumnGrid(collapsablePanel, 0, num, 0, 230, fixedItemHeight: true);
				grid.ID = UIComponent.DataControlID.LeftGrid;
			}
			else if (processJob.ProcessType.Inputs != null)
			{
				grid = AddColumnGrid(collapsablePanel, 0, num, 0, 230, fixedItemHeight: false);
				grid.ID = UIComponent.DataControlID.LeftGrid;
				grid.BeginAddingEntries();
				Grid item = CreateFixedItemHeightGrid(grid.Width);
				grid.AddEntry("unassigned", item);
				item = CreateFixedItemHeightGrid(grid.Width);
				grid.AddEntry("assigned", item);
				grid.EndAddingEntries();
			}
			int num2 = 240;
			AddColumnGrid(collapsablePanel, num2, num, 1, 273, fixedItemHeight: true).ID = UIComponent.DataControlID.Tools;
			Label label3 = new Label(Interface.gui);
			collapsablePanel.AddContent(label3, num2, num);
			label3.Init(Label.LabelType.LCDNormal);
			label3.NormalColor = Label.LCDErrorColor;
			label3.ID = UIComponent.DataControlID.ToolsError;
			label3.Visible = false;
			label3.Height = 23;
		}
	}

	private void AddProcessJobHeader(LCDInnerPanel cPanel, int headerHeight)
	{
	}

	private void AddRowPanel(ref LCDInnerPanel rowPanel, object key)
	{
		rowPanel = new LCDInnerPanel(Interface.gui, outerGrid.Width, includeDecor: false);
		rowPanel.HorizontalContentPadding = 6;
		rowPanel.VerticalContentPadding = 8;
		outerGrid.AddEntry(key, rowPanel.Panel);
	}

	private Grid AddColumnGrid(LCDInnerPanel cPanel, int xPos, int yPos, int i, int width, bool fixedItemHeight)
	{
		Grid grid;
		if (fixedItemHeight)
		{
			grid = CreateFixedItemHeightGrid(width);
		}
		else
		{
			grid = new Grid(Interface.gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
			grid.Width = width;
			grid.ScrollBarEnabled = false;
			grid.CanGrowInHeight = true;
			grid.Font = GUIManager.LCDandHUDBodyFontPath;
			grid.IsOuterGrid = false;
			grid.Tag1 = i;
		}
		grid.FixedItemHeights = fixedItemHeight;
		grid.BottomMargin = 4;
		cPanel.AddContent(grid, xPos, yPos);
		return grid;
	}

	private Grid CreateFixedItemHeightGrid(int width)
	{
		Grid grid = new Grid(Interface.gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
		grid.FixedItemHeights = true;
		grid.Width = width;
		grid.ScrollBarEnabled = false;
		grid.ItemHeight = 18;
		grid.RowSpacing = 4;
		grid.FixedItemHeights = true;
		grid.CanGrowInHeight = true;
		grid.Font = GUIManager.LCDandHUDBodyFontPath;
		grid.IsOuterGrid = false;
		grid.Tag1 = 0;
		return grid;
	}

	private void UpdateRow(Box itemRow, Job job, EntityGroup owner, ExpandCollapse? expandOrCollapse)
	{
		UpdateCollapsedPart(itemRow, job, owner, out var hasProblems, out var hasTools, out var progress);
		if (job is ProcessJob pJob)
		{
			UpdateLeftColumn(itemRow, pJob, owner, ref hasProblems);
			UpdateToolsColumn(itemRow, pJob, owner, hasTools);
		}
		UpdateTitleColor(itemRow, hasProblems);
		ImageButton imageButton = (ImageButton)itemRow.FindChildById(UIComponent.DataControlID.Expand);
		bool expand;
		if (expandOrCollapse.HasValue)
		{
			if (expandOrCollapse.Value == ExpandCollapse.Expand)
			{
				imageButton.IsChecked = true;
				expand = true;
			}
			else
			{
				imageButton.IsChecked = false;
				expand = false;
			}
		}
		else
		{
			expand = imageButton.IsChecked;
		}
		ExpandOrCollapseRow(expand, itemRow);
		switch (The.InGameUI.TaskSettings.SortingSettings.SortedBy)
		{
		case TaskSettings.SortColumns.TaskType:
			itemRow.OrderByTag1 = job.GetName();
			break;
		case TaskSettings.SortColumns.Completion:
			itemRow.OrderByTag1 = progress ?? (-1f);
			break;
		case TaskSettings.SortColumns.Priority:
		{
			int num = 0;
			switch (job.Priority)
			{
			case UWGame.SimSide.Jobs.Priority.Low:
				num = 0;
				break;
			case UWGame.SimSide.Jobs.Priority.Normal:
				num = 1;
				break;
			case UWGame.SimSide.Jobs.Priority.High:
				num = 2;
				break;
			}
			itemRow.OrderByTag1 = num;
			break;
		}
		}
	}

	private static void UpdateTitleColor(Box itemRow, bool hasProblems)
	{
		Label label = itemRow.FindChildById(UIComponent.DataControlID.JobType) as Label;
		if (hasProblems)
		{
			label.NormalColor = Label.LCDErrorColor;
		}
		else
		{
			label.NormalColor = label.GetNormalColorForType();
		}
	}

	private void btJobTypeHigh_Click(UIComponent sender, EventArgs e)
	{
		jobTypePriorityButtonClick(UWGame.SimSide.Jobs.Priority.High);
	}

	private void btJobTypeNormal_Click(UIComponent sender, EventArgs e)
	{
		jobTypePriorityButtonClick(UWGame.SimSide.Jobs.Priority.Normal);
	}

	private void btJobTypeLow_Click(UIComponent sender, EventArgs e)
	{
		jobTypePriorityButtonClick(UWGame.SimSide.Jobs.Priority.Low);
	}

	private void jobTypePriorityButtonClick(UWGame.SimSide.Jobs.Priority priority)
	{
		if (!cbTaskType.SelectedKey.Equals("SELECTTASKTYPE"))
		{
			JobType jobType = (JobType)cbTaskType.SelectedKey;
			Command command = new SetJobTypePriority(expedition.OwnedEntities, jobType.KeyName, priority, updateInstances: true);
			The.Client.Controller.StoreAndExecuteCommand(command);
			UpdateJobs(null);
		}
	}

	private Job GetJobFromKey(string key)
	{
		return allJobsToShow.Find((Tuple<string, Job> t) => t.Item1 == key).Item2;
	}

	private void btCancel_Click(UIComponent sender, EventArgs e)
	{
		Job jobFromKey = GetJobFromKey((string)sender.Tag1);
		if (jobFromKey.ID != JobID.Invalid)
		{
			Command command = new CancelJob(jobFromKey.ID);
			The.Client.Controller.StoreAndExecuteCommand(command);
		}
		UpdateJobs(null);
	}

	public override void Hide()
	{
		base.Hide();
	}
}
