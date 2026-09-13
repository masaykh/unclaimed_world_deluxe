using System;
using System.Collections.Generic;
using System.Linq;
using UWGame.ClientSide.GameEvents;
using UWGame.ClientSide.Screens;
using UWGame.Control;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Scenarios;
using UWGame.Steam;

namespace UWGame.SimSide.InGameEvents.Actions;

public class WinGameAction : EventActionType, IGameData
{
	public DynamicText ModalDialogText;

	public string ModalDialogImage;

	public string WinScreenText;

	public string ContinueGameTooltip;

	public string EndGameTooltip;

	public bool AllowContinueGame = true;

	public ActionSets ContinueActions;

	public EventActionType[] EndGameActions;

	private Controller controller;

	public WinGameAction(string keyName)
		: base(keyName)
	{
	}

	public WinGameAction()
	{
	}

	public override bool Execute(EventAction action, ref string failReason)
	{
		controller = The.Sim.Controller;
		CheckAchievements();
		if (The.Client != null)
		{
			The.Client.PauseGame();
			if (ModalDialogText != null && !string.IsNullOrEmpty(ModalDialogText.Text))
			{
				EventActionDialog.ShowAndSaveEventDialog(ModalDialogImage, "", ModalDialogText.GetSubstitutedText(action), modal: true, showOkButton: true, "EXIT SCENARIO", EndGameTooltip, null, AllowContinueGame, "CONTINUE PLAYING", ContinueGameTooltip).ButtonClicked += dialog_ButtonClicked;
			}
			else
			{
				GoToWinGameScreen();
			}
		}
		return true;
	}

	private void dialog_ButtonClicked(string arg1, int index)
	{
		if (index == 0)
		{
			GoToWinGameScreen();
		}
		else
		{
			ContinuePlaying();
		}
	}

	public override void ExtractNestedActionTypes(ref List<string> duplicateKeyErrors)
	{
		base.ExtractNestedActionTypes(ref duplicateKeyErrors);
		if (ContinueActions != null)
		{
			ContinueActions.ExtractNestedGameData(ref duplicateKeyErrors);
		}
		if (EndGameActions != null)
		{
			EventActionType[] endGameActions = EndGameActions;
			for (int i = 0; i < endGameActions.Length; i++)
			{
				endGameActions[i].ExtractNestedActionTypes(ref duplicateKeyErrors);
			}
		}
	}

	private void GoToWinGameScreen()
	{
		The.Sim.IsGameOver = true;
		if (EndGameActions != null)
		{
			EventActionType[] endGameActions = EndGameActions;
			for (int i = 0; i < endGameActions.Length; i++)
			{
				new EventAction(endGameActions[i], null).Execute();
			}
		}
		The.InGameUI.EventDialog.ButtonClicked -= dialog_ButtonClicked;
		LoadingScreen.StartTransition(The.Sim.Controller, LoadWinGameScreen, loadingIsSlow: false);
	}

	private void CheckAchievements()
	{
		StatsAndAchievements statsAndAchievements = The.Sim.Controller.StatsAndAchievements;
		if (The.Sim.StartGameParams.StartScenarioParams == null || The.Sim.StartGameParams.StartScenarioParams.Scenario.Source != Source.RefactoredGames)
		{
			return;
		}
		string scenarioName = The.Sim.StartGameParams.StartScenarioParams.ScenarioName;
		if (!statsAndAchievements.IsAchievementUnlocked(AchievementID.tutorialCompleted) && scenarioName == "TUTORIAL - Castaways")
		{
			statsAndAchievements.UnlockAchievement(AchievementID.tutorialCompleted);
		}
		if (!statsAndAchievements.IsAchievementUnlocked(AchievementID.clayPitCompleted) && scenarioName == "The Clay Pit")
		{
			statsAndAchievements.UnlockAchievement(AchievementID.clayPitCompleted);
		}
		if (scenarioName == "Making Headway")
		{
			if (!statsAndAchievements.IsAchievementUnlocked(AchievementID.headwayCompleted))
			{
				statsAndAchievements.UnlockAchievement(AchievementID.headwayCompleted);
			}
			if (!statsAndAchievements.IsAchievementUnlocked(AchievementID.headwayAlternative) && (!The.Sim.PlaySite.PlayerAllegiance.Statistics.ProductionStatistics.Totals[ProductionStatistics.StatTypes.Produced].TryGetValue(GameData.Instance.AllEntityTypes["item:gaskets"], out var value) || value == 0))
			{
				statsAndAchievements.UnlockAchievement(AchievementID.headwayAlternative);
			}
		}
		if (!statsAndAchievements.IsAchievementUnlocked(AchievementID.twinklerIslandNoDeaths) && scenarioName == "Twinkler Island")
		{
			bool flag = true;
			if (!(The.Sim.StartGameParams.StartScenarioParams.MainDifficultyKey == "normal"))
			{
				flag = The.Sim.StartGameParams.StartScenarioParams.MainDifficultyKey == "easy";
			}
			else if (The.Sim.PlaySite.PlayerAllegiance.IndependentMembers.Count < 5)
			{
				flag = false;
			}
			if (flag)
			{
				PopulationStatistics populationStatistics = The.Sim.PlaySite.PlayerAllegiance.Statistics.PopulationStatistics;
				if (populationStatistics.IndependentDeaths.Count == 0 && populationStatistics.Emigration.Count == 0)
				{
					statsAndAchievements.UnlockAchievement(AchievementID.twinklerIslandNoDeaths);
				}
			}
		}
		if (!statsAndAchievements.IsAchievementUnlocked(AchievementID.twinklerIslandNoKills) && scenarioName == "Twinkler Island" && (The.Sim.StartGameParams.StartScenarioParams.MainDifficultyKey == "normal" || The.Sim.StartGameParams.StartScenarioParams.MainDifficultyKey == "easy") && !The.Sim.PlaySite.PlayerAllegiance.Statistics.KillStatistics.Kills.Any((KeyValuePair<EntityType, int> kvp) => kvp.Value > 0))
		{
			statsAndAchievements.UnlockAchievement(AchievementID.twinklerIslandNoKills);
		}
	}

	private void ContinuePlaying()
	{
		if (ContinueActions != null)
		{
			ContinueActions.Fire(null, null, null, out var _);
		}
		The.InGameUI.EventDialog.ButtonClicked -= dialog_ButtonClicked;
	}

	public void LoadWinGameScreen(object sender, EventArgs e)
	{
		GameScreen[] screens = controller.GetScreens();
		for (int i = 0; i < screens.Length; i++)
		{
			screens[i].ExitScreen();
		}
		controller.AddScreen(new BackgroundScreen(BackgroundScreen.Background.BlueTint));
		controller.AddScreen(new WinGameScreen(controller, WinScreenText));
	}
}
