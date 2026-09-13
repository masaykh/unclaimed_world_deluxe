using System;
using System.Collections.Generic;
using GameStateManagement;
using Steamworks;
using UWGame.Control;

namespace UWGame.Steam;

public class StatsAndAchievements
{
	private class Achievement
	{
		public AchievementID m_eAchievementID;

		public string m_strName;

		public string m_strDescription;

		public bool m_bAchieved;

		public Achievement(AchievementID achievementID)
		{
			m_eAchievementID = achievementID;
			m_bAchieved = false;
		}

		public Achievement(AchievementID achievementID, string name, string desc)
		{
			m_eAchievementID = achievementID;
			m_strName = name;
			m_strDescription = desc;
			m_bAchieved = false;
		}
	}

	private Dictionary<AchievementID, Achievement> achievements;

	private CGameID m_GameID;

	private bool m_bRequestedStats;

	private bool m_bStatsValid;

	private bool m_bStoreStats;

	protected Callback<UserStatsReceived_t> m_UserStatsReceived;

	protected Callback<UserStatsStored_t> m_UserStatsStored;

	protected Callback<UserAchievementStored_t> m_UserAchievementStored;

	private Controller controller;

	public StatsAndAchievements(Controller controller)
	{
		this.controller = controller;
		achievements = new Dictionary<AchievementID, Achievement>();
		foreach (AchievementID value in Enum.GetValues(typeof(AchievementID)))
		{
			achievements.Add(value, new Achievement(value));
		}
	}

	public void Initialize()
	{
		if (controller.SteamManager.IsInitialized)
		{
			m_GameID = new CGameID(SteamUtils.GetAppID());
			m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
			m_UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
			m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);
			m_bRequestedStats = false;
			m_bStatsValid = false;
		}
	}

	public void Update()
	{
		if (controller.SteamManager.IsInitialized)
		{
			if (!m_bRequestedStats)
			{
				bool bRequestedStats = SteamUserStats.RequestCurrentStats();
				m_bRequestedStats = bRequestedStats;
			}
			if (m_bStatsValid && m_bStoreStats)
			{
				bool flag = SteamUserStats.StoreStats();
				m_bStoreStats = !flag;
			}
		}
	}

	public void UnlockAchievement(AchievementID achievement)
	{
		Achievement achievement2 = achievements[achievement];
		achievement2.m_bAchieved = true;
		if (!controller.SteamManager.IsInitialized)
		{
			string title = "Achievement could not be unlocked. Steam error.";
			UnclaimedWorld.LogError($"Achievement {achievement.ToString()} failed to be unlocked.", title);
			return;
		}
		try
		{
			SteamUserStats.SetAchievement(achievement2.m_eAchievementID.ToString());
		}
		catch (InvalidOperationException ex)
		{
			string title2 = "Achievement could not be unlocked. Steam error.";
			UnclaimedWorld.LogError($"Achievement {achievement.ToString()} failed to be unlocked. Error message: {ex.Message}", title2);
			return;
		}
		m_bStoreStats = true;
	}

	public bool IsAchievementUnlocked(AchievementID achievement)
	{
		return achievements[achievement].m_bAchieved;
	}

	private void OnUserStatsReceived(UserStatsReceived_t pCallback)
	{
		if (!controller.SteamManager.IsInitialized || (ulong)m_GameID != pCallback.m_nGameID)
		{
			return;
		}
		if (EResult.k_EResultOK == pCallback.m_eResult)
		{
			Console.WriteLine("Received stats and achievements from Steam\n");
			m_bStatsValid = true;
			{
				foreach (KeyValuePair<AchievementID, Achievement> achievement2 in achievements)
				{
					string text = achievement2.Value.m_eAchievementID.ToString();
					bool pbAchieved;
					bool achievement = SteamUserStats.GetAchievement(text, out pbAchieved);
					achievement2.Value.m_bAchieved = pbAchieved;
					if (achievement)
					{
						achievement2.Value.m_strName = SteamUserStats.GetAchievementDisplayAttribute(text, "name");
						achievement2.Value.m_strDescription = SteamUserStats.GetAchievementDisplayAttribute(text, "desc");
					}
					else
					{
						Console.WriteLine("SteamUserStats.GetAchievement failed for Achievement " + text + "\nIs it registered in the Steam Partner site?");
					}
				}
				return;
			}
		}
		Console.WriteLine("RequestStats - failed, " + pCallback.m_eResult);
	}

	private void OnUserStatsStored(UserStatsStored_t pCallback)
	{
		if ((ulong)m_GameID == pCallback.m_nGameID)
		{
			if (EResult.k_EResultOK == pCallback.m_eResult)
			{
				Console.WriteLine("StoreStats - success");
			}
			else if (EResult.k_EResultInvalidParam == pCallback.m_eResult)
			{
				Console.WriteLine("StoreStats - some failed to validate");
				OnUserStatsReceived(new UserStatsReceived_t
				{
					m_eResult = EResult.k_EResultOK,
					m_nGameID = (ulong)m_GameID
				});
			}
			else
			{
				Console.WriteLine("StoreStats - failed, " + pCallback.m_eResult);
			}
		}
	}

	private void OnAchievementStored(UserAchievementStored_t pCallback)
	{
		if (pCallback.m_nGameID == (ulong)m_GameID)
		{
			if (pCallback.m_nMaxProgress == 0)
			{
				Console.WriteLine("Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!");
				return;
			}
			Console.WriteLine("Achievement '" + pCallback.m_rgchAchievementName + "' progress callback, (" + pCallback.m_nCurProgress + "," + pCallback.m_nMaxProgress + ")");
		}
	}
}
