using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using WindowSystem;

namespace UWGame.ClientSide.Interface.EntityPanel;

public class EntityCRTContent
{
	private Dictionary<EntityType, Entity> modelsToPlay = new Dictionary<EntityType, Entity>();

	private UIComponent crtContent;

	private UIComponent modelRenderer;

	private GUIManager gui;

	private EntityPanel entityPanel;

	private FramedCRT framedCRT;

	private Dictionary<EntityType, Entity> screenModels = new Dictionary<EntityType, Entity>();

	private Entity currentScreenModel;

	private float rotation;

	private Vector3 modelLocation;

	private Vector3 realLocation;

	private Matrix perspectiveView;

	private const int leftMargin = 34;

	private const int captionWidth = 100;

	private const int lineHeight = 18;

	protected UIComponent pnHeading;

	protected Label lblHeading;

	protected Label lblSubHeading;

	protected Label lblProducer;

	protected Label lblFlavourLine1;

	protected Label lblFlavourLine2;

	protected Bar underline;

	protected Image imCRT;

	protected UIComponent pnBillboards;

	protected UIComponent pnPerson;

	protected Box boxPersonImageBorder;

	protected Label lblPersonHeading;

	protected Label lblPersonSex;

	protected Label lblPersonAge;

	protected Label lblPersonOccupation;

	protected Label lblPersonFamily;

	protected Label lblPersonHealth;

	protected Label lblPersonHome;

	protected Label lblPersonHeight;

	protected Label lblPersonWeight;

	protected UIComponent pnStructure;

	protected Box boxStructureImageBorder;

	protected Label lblStructureHeading;

	protected Label lblStructureWorkRequired;

	protected Label lblStructureMaterialsRequired;

	protected Label lblStructureCondition;

	protected Label lblStructureEnergy;

	protected TextArea taStructureDescription;

	protected UIComponent pnVehicle;

	protected Label lblVehicleHeading;

	protected Label lblVehicleSubHeading;

	protected Label lblVehicleProducer;

	protected Label lblVehicleWorkRequired;

	protected Label lblVehicleMaterialsRequired;

	protected Label lblVehicleCapacity;

	protected Label lblVehicleRange;

	protected Label lblVehicleSpeed;

	protected Label lblVehicleCondition;

	protected Label lblVehicleEnergy;

	protected TextArea taVehicleDescription;

	private int lineNo;

	private const int emptyLine = 6;

	public EntityCRTContent(GUIManager gui, EntityPanel entityPanel, FramedCRT framedCRT)
	{
		this.gui = gui;
		this.entityPanel = entityPanel;
		this.framedCRT = framedCRT;
		crtContent = framedCRT.GetNewSurfaceContent();
		InitModelRenderer();
		pnHeading = new UIComponent(gui);
		pnHeading.Width = crtContent.Width;
		pnHeading.Height = crtContent.Height;
		pnHeading.RenderType = RenderType.CRTAndLCD;
		InitHeading(ref lblHeading, ref lblSubHeading, ref lblProducer, pnHeading);
		imCRT = new Image(gui);
		InitImageFrame(ref boxPersonImageBorder, imCRT);
		InitCRTPersonTemplate();
		InitCRTStructureTemplate();
		InitCRTVehicleTemplate();
	}

	public void AddCRTCaptionAndLabel(UIComponent pnPanel, string caption, ref Label lblValue, ref int yPos)
	{
		AddCRTCaptionAndLabel(pnPanel, caption, ref lblValue, 34, 100, ref yPos);
	}

	public void AddCRTCaptionAndLabel(UIComponent pnPanel, string caption, ref Label lblValue, int xPos, int captionWidth, ref int yPos)
	{
		Label label = new Label(gui);
		pnPanel.Add(label);
		label.Text = caption;
		label.Init(Label.LabelType.CRTSmall);
		label.Position = new Point(xPos, yPos);
		label.AnimateOnCRTScreen = Label.AnimationMode.Line;
		label.AnimateOnCRTScreenLineNo = lineNo;
		lblValue = new Label(gui);
		pnPanel.Add(lblValue);
		lblValue.Init(Label.LabelType.CRTSmall);
		lblValue.Position = new Point(xPos + Common.Max(captionWidth, label.Width), yPos);
		lblValue.Width = 280;
		lblValue.AnimateOnCRTScreen = Label.AnimationMode.Line;
		lblValue.AnimateOnCRTScreenLineNo = lineNo;
		lineNo++;
		yPos += 18;
	}

	private void InitImageFrame(ref Box box, Image image)
	{
		box = new Box(gui);
		Rectangle sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("CRT_LayoutBox");
		box.SetSkinLocation(SkinState.Normal, sourceRectangle);
		box.RenderType = RenderType.CRTAndLCD;
		box.CornerSize = 11;
	}

	private void InitCRTPersonTemplate()
	{
		pnPerson = new UIComponent(gui);
		pnPerson.Width = crtContent.Width;
		pnPerson.Height = crtContent.Height;
		pnPerson.RenderType = RenderType.CRTAndLCD;
		int yPos = 10;
		AddCRTCaptionAndLabel(pnPerson, "AGE:", ref lblPersonAge, ref yPos);
		AddCRTCaptionAndLabel(pnPerson, "SEX:", ref lblPersonSex, ref yPos);
		AddCRTCaptionAndLabel(pnPerson, "HEIGHT:", ref lblPersonHeight, ref yPos);
		AddCRTCaptionAndLabel(pnPerson, "WEIGHT:", ref lblPersonWeight, ref yPos);
		yPos += 6;
		AddCRTCaptionAndLabel(pnPerson, "FAMILY:", ref lblPersonFamily, ref yPos);
		AddCRTCaptionAndLabel(pnPerson, "HOME:", ref lblPersonHome, ref yPos);
		AddCRTCaptionAndLabel(pnPerson, "HEALTH:", ref lblPersonHealth, ref yPos);
		yPos += 6;
		AddCRTCaptionAndLabel(pnPerson, "CURRENT JOB:", ref lblPersonOccupation, ref yPos);
		yPos += 6;
		lblFlavourLine1 = new Label(gui);
		pnPerson.Add(lblFlavourLine1);
		lblFlavourLine1.Text = "";
		lblFlavourLine1.Init(Label.LabelType.CRTSmall);
		lblFlavourLine1.Position = new Point(34, yPos);
		lblFlavourLine1.AnimateOnCRTScreen = Label.AnimationMode.Line;
		lblFlavourLine1.AnimateOnCRTScreenLineNo = lineNo;
		yPos += 18;
		yPos += 6;
		lineNo++;
		lblFlavourLine2 = new Label(gui);
		pnPerson.Add(lblFlavourLine2);
		lblFlavourLine2.Text = "";
		lblFlavourLine2.Init(Label.LabelType.CRTSmall);
		lblFlavourLine2.Position = new Point(34, yPos);
		lblFlavourLine2.AnimateOnCRTScreen = Label.AnimationMode.Line;
		lblFlavourLine2.AnimateOnCRTScreenLineNo = lineNo;
		yPos += 18;
	}

	private void InitCRTStructureTemplate()
	{
		pnStructure = new UIComponent(gui);
		pnStructure.Width = crtContent.Width;
		pnStructure.Height = crtContent.Height;
		pnStructure.RenderType = RenderType.CRTAndLCD;
		pnBillboards = new UIComponent(gui);
		pnBillboards.Width = crtContent.Width;
		pnBillboards.Height = crtContent.Height;
		int yPos = 10;
		AddCRTCaptionAndLabel(pnStructure, "MATERIALS:", ref lblStructureMaterialsRequired, ref yPos);
		yPos += 6;
		AddCRTCaptionAndLabel(pnStructure, "CONDITION:", ref lblStructureCondition, ref yPos);
		AddCRTCaptionAndLabel(pnStructure, "ENERGY:", ref lblStructureEnergy, ref yPos);
		yPos += 6;
		yPos += 6;
		yPos += 6;
		yPos += 6;
		taStructureDescription = new TextArea(gui, ListBoxType.Main);
		pnStructure.Add(taStructureDescription);
		taStructureDescription.Position = new Point(34, yPos);
		taStructureDescription.Width = pnStructure.Width - 34;
		taStructureDescription.Height = pnStructure.Height - taStructureDescription.Position.Y;
		taStructureDescription.HMargin = 0;
		taStructureDescription.Init(Label.LabelType.CRTSmall);
	}

	private void InitCRTVehicleTemplate()
	{
		pnVehicle = new UIComponent(gui);
		pnVehicle.Width = crtContent.Width;
		pnVehicle.Height = crtContent.Height;
		pnVehicle.RenderType = RenderType.CRTAndLCD;
		int yPos = 10;
		AddCRTCaptionAndLabel(pnVehicle, "WORK REQD.:", ref lblVehicleWorkRequired, ref yPos);
		yPos += 6;
		AddCRTCaptionAndLabel(pnVehicle, "CAPACITY:", ref lblVehicleCapacity, ref yPos);
		yPos += 6;
		AddCRTCaptionAndLabel(pnVehicle, "SPEED:", ref lblVehicleSpeed, ref yPos);
		AddCRTCaptionAndLabel(pnVehicle, "RANGE:", ref lblVehicleRange, ref yPos);
		yPos += 6;
		AddCRTCaptionAndLabel(pnVehicle, "CONDITION:", ref lblVehicleCondition, ref yPos);
		AddCRTCaptionAndLabel(pnVehicle, "ENERGY:", ref lblVehicleEnergy, ref yPos);
		yPos += 6;
		yPos += 6;
		yPos += 6;
		yPos += 6;
		taVehicleDescription = new TextArea(gui, ListBoxType.Main);
		pnVehicle.Add(taVehicleDescription);
		taVehicleDescription.Position = new Point(34, yPos);
		taVehicleDescription.Width = pnVehicle.Width - 34;
		taVehicleDescription.Height = pnVehicle.Height - taVehicleDescription.Position.Y;
		taVehicleDescription.HMargin = 0;
		taVehicleDescription.Init(Label.LabelType.CRTSmall);
	}

	private void InitModelRenderer()
	{
		float num = 0f;
		float num2 = -180f;
		num = (framedCRT.DisplayWindow.AbsolutePosition.X + 390 - gui.ScreenWidth / 2) / 2;
		modelLocation = new Vector3(num, num2, 0f);
		perspectiveView = Matrix.CreateLookAt(new Vector3(num, num2 - 420f, -420f), new Vector3(num, num2, 0f), Vector3.UnitY);
		modelRenderer = new UIComponent(gui);
		modelRenderer.DrawContentEvent += ModelRenderer_DrawContentEvent;
		modelRenderer.UpdateEvent += ModelRenderer_UpdateEvent;
		modelRenderer.Width = crtContent.Width;
		modelRenderer.Height = crtContent.Height;
	}

	private void InitHeading(ref Label lblHeading, ref Label lblSubHeading, ref Label lblProducer, UIComponent pnTemplate)
	{
		lblHeading = new Label(gui);
		pnTemplate.Add(lblHeading);
		lblHeading.Init(Label.LabelType.CRTBigGlow);
		lblHeading.Position = new Point(34, 35);
		lblHeading.AnimateOnCRTScreen = Label.AnimationMode.Character;
		lblHeading.Width = 400;
		lblSubHeading = new Label(gui);
		pnTemplate.Add(lblSubHeading);
		lblSubHeading.Init(Label.LabelType.CRTSmall);
		lblSubHeading.Position = new Point(34, 64);
		lblSubHeading.AnimateOnCRTScreen = Label.AnimationMode.Character;
		lblSubHeading.Width = 400;
		underline = new Bar(gui);
		pnTemplate.Add(underline);
		underline.Position = new Point(34, 88);
		underline.EdgeSize = 6;
		Rectangle sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("CRT_LayoutLine");
		underline.SetSkinLocation(SkinState.Normal, sourceRectangle);
		underline.Width = 290;
		underline.Height = sourceRectangle.Height;
		underline.RenderType = RenderType.CRTAndLCD;
		underline.DebugTag = "underline";
		lblProducer = new Label(gui);
		pnTemplate.Add(lblProducer);
		lblProducer.Init(Label.LabelType.CRTSmall);
		lblProducer.Position = new Point(34, 106);
		lblProducer.Width = 400;
	}

	private void DisplayHeading(string heading, string summary, string producer)
	{
		lblHeading.Text = heading;
		if (string.IsNullOrEmpty(summary))
		{
			lblSubHeading.Text = "";
			underline.Y = lblHeading.Y + lblHeading.Height + 2;
			underline.Width = lblHeading.Width;
		}
		else
		{
			lblSubHeading.Text = summary;
			underline.Y = lblSubHeading.Y + lblSubHeading.Height + 2;
			underline.Width = lblSubHeading.Width;
		}
		lblProducer.Y = underline.Y + underline.Height;
		if (string.IsNullOrEmpty(producer))
		{
			lblProducer.Text = "";
			pnHeading.Height = underline.Y + underline.Height + 4;
		}
		else
		{
			lblProducer.Text = producer;
			pnHeading.Height = lblProducer.Y + lblProducer.Height + 4;
		}
	}

	private void ModelRenderer_UpdateEvent(GameTime gameTime)
	{
		if (currentScreenModel != null)
		{
			FramedCRT.RotateModel(gameTime, currentScreenModel, ref rotation, modelLocation);
		}
	}

	private void ModelRenderer_DrawContentEvent()
	{
		if (currentScreenModel != null)
		{
			gui.EndSpriteBatch();
			FramedCRT.DrawModel(currentScreenModel, perspectiveView);
			gui.BeginSpriteBatch();
		}
	}

	public void Refresh(Entity entity)
	{
		if (entity == null || framedCRT.crtTextAnimatorCharacter.IsStarted || framedCRT.crtTextAnimatorLine.IsStarted)
		{
			return;
		}
		FramedCRT.ClearContent(crtContent);
		if (entity.Renderable.RenderAsModel != null && entity.PersonEntity == null)
		{
			screenModels.TryGetValue(entity.EntityType, out currentScreenModel);
			crtContent.Add(modelRenderer);
		}
		else
		{
			currentScreenModel = null;
		}
		crtContent.Add(pnHeading);
		string text = ((entity.PersonEntity == null) ? (entity.EntityType.FormalName ?? entity.EntityType.Name) : entity.Name);
		DisplayHeading(text.ToUpper(Config.Culture), GetUpperCase(entity.EntityType.SummaryDescription), "");
		if (entity.PersonEntity != null)
		{
			crtContent.Add(pnPerson);
			pnPerson.Y = pnHeading.Y + pnHeading.Height;
			crtContent.Add(imCRT);
			Rectangle portraitForEntityPanel = entity.PersonEntity.GetPortraitForEntityPanel(gui);
			imCRT.Position = new Point(290, 14);
			imCRT.Texture = gui.GUI_CRT_SpriteSheet.Texture;
			imCRT.SetSkinLocation(SkinState.Normal, portraitForEntityPanel);
			imCRT.ScaleImageToSizeOfControl = false;
			imCRT.ResizeControlToFitImage();
			crtContent.Add(boxPersonImageBorder);
			boxPersonImageBorder.Position = new Point(imCRT.X - 6, imCRT.Y - 6);
			boxPersonImageBorder.Width = imCRT.Width + 12;
			boxPersonImageBorder.Height = imCRT.Height + 12;
			lblPersonSex.Text = entity.BiologicalEntity.CasteType.Reproduction.ToString().ToUpper(Config.Culture);
			if (entity.BiologicalEntity.AgeGroup.AgeGroupType.AIAgeGroup == AIAgeGroup.Adult || entity.BiologicalEntity.AgeGroup.AgeGroupType.AIAgeGroup == AIAgeGroup.Old)
			{
				if (entity.BiologicalEntity.Mate != null)
				{
					lblPersonFamily.Text = "MARRIED";
				}
				else
				{
					lblPersonFamily.Text = "SINGLE";
				}
			}
			else
			{
				lblPersonFamily.Text = "-";
			}
			lblPersonAge.Text = ((int)entity.BiologicalEntity.AgeGroup.Age).ToString();
			lblPersonHeight.Text = (int)(100f * entity.BiologicalEntity.AdultTargetHeight) + " CM";
			lblPersonWeight.Text = (int)entity.BiologicalEntity.AdultTargetWeight + " KG";
			lblPersonHealth.Text = "EXCELLENT";
			StringBuilder stringBuilder = new StringBuilder("SKILLS: ");
			string value = "";
			int num = 0;
			foreach (KeyValuePair<SkillType, Skill> skill in entity.Intelligence.Skills)
			{
				stringBuilder.Append(value);
				stringBuilder.Append(skill.Key.Name.ToUpper(Config.Culture));
				value = ", ";
				num++;
				if (num > 1)
				{
					break;
				}
			}
			lblFlavourLine1.Text = stringBuilder.ToString();
			lblFlavourLine1.Width = pnPerson.Width - 68;
			lblFlavourLine2.Text = "WEALTH: 12000c";
			lblFlavourLine2.Text += ", CHILDREN: 0";
			lblFlavourLine2.Text += ", CRIMES: NONE";
		}
		else if (entity.Structure != null)
		{
			crtContent.Add(pnStructure);
			pnStructure.Y = pnHeading.Y + pnHeading.Height;
			if (entity.Renderable.RenderAsBillboard != null)
			{
				crtContent.Add(pnBillboards);
			}
			lblStructureCondition.Text = "GOOD";
			taStructureDescription.Text = entity.EntityType.Description.ToUpper(Config.Culture);
		}
		else if (entity.Vehicle != null)
		{
			crtContent.Add(pnVehicle);
			pnVehicle.Y = pnHeading.Y + pnHeading.Height;
			taVehicleDescription.Text = entity.EntityType.Description.ToUpper(Config.Culture);
			lblVehicleWorkRequired.Text = "2000 MAN HRS";
			lblVehicleCapacity.Text = "3 SEATS / MAX 500 KG";
			lblVehicleCondition.Text = "GOOD";
			lblVehicleRange.Text = "600 KM";
			lblVehicleSpeed.Text = "280 KM/H";
		}
	}

	public void Show(bool showFrame)
	{
		if (showFrame)
		{
			framedCRT.TurnOn();
			framedCRT.Show();
		}
		framedCRT.ChangeContent(crtContent);
	}

	public void Hide()
	{
		framedCRT.TurnOff();
		framedCRT.Hide();
	}

	private string GetUpperCase(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		return text.ToUpper(Config.Culture);
	}
}
