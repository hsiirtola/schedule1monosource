using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using Steamworks;
using UnityEngine;

namespace ScheduleOne;

public static class AchievementManager
{
	public enum EAchievement
	{
		COMPLETE_PROLOGUE,
		RV_DESTROYED,
		DEALER_RECRUITED,
		MASTER_CHEF,
		BUSINESSMAN,
		BIGWIG,
		MAGNATE,
		UPSTANDING_CITIZEN,
		ROLLING_IN_STYLE,
		LONG_ARM_OF_THE_LAW,
		INDIAN_DEALER,
		URBAN_ARTIST,
		FINISHING_THE_JOB
	}

	private static EAchievement[] achievements;

	private static Dictionary<EAchievement, bool> achievementUnlocked = new Dictionary<EAchievement, bool>();

	[RuntimeInitializeOnLoadMethod(/*Could not decode attribute arguments.*/)]
	private static void Init()
	{
		achievements = (EAchievement[])Enum.GetValues(typeof(EAchievement));
		EAchievement[] array = achievements;
		foreach (EAchievement key in array)
		{
			achievementUnlocked.Add(key, value: false);
		}
		if (SteamManager.Initialized)
		{
			PullAchievements();
		}
		else
		{
			SteamManager.OnSteamInitialized = (Action)Delegate.Combine(SteamManager.OnSteamInitialized, new Action(PullAchievements));
		}
	}

	private static void PullAchievements()
	{
		if (!SteamManager.Initialized)
		{
			Debug.LogWarning((object)"Steamworks not initialized, cannot pull achievement stats");
			return;
		}
		EAchievement[] array = achievements;
		bool value = default(bool);
		for (int i = 0; i < array.Length; i++)
		{
			EAchievement key = array[i];
			SteamUserStats.GetAchievement(key.ToString(), ref value);
			achievementUnlocked[key] = value;
		}
		Debug.Log((object)("Pulled achievement stats. Unlocked achievement count: " + achievementUnlocked.Where((KeyValuePair<EAchievement, bool> kvp) => kvp.Value).Count()));
	}

	public static void UnlockAchievement(EAchievement achievement)
	{
		if (!SteamManager.Initialized)
		{
			Debug.LogWarning((object)"Steamworks not initialized, cannot unlock achievement");
		}
		else if (GameManager.IS_BETA)
		{
			Debug.Log((object)"Blocking achievement unlock in beta");
		}
		else if (!achievementUnlocked[achievement])
		{
			Debug.Log((object)$"Unlocking achievement: {achievement}");
			SteamUserStats.SetAchievement(achievement.ToString());
			SteamUserStats.StoreStats();
			achievementUnlocked[achievement] = true;
		}
	}
}
