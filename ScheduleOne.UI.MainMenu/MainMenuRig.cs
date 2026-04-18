using System.IO;
using ScheduleOne.AvatarFramework;
using ScheduleOne.AvatarFramework.Customization;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using ScheduleOne.Tools;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.UI.MainMenu;

public class MainMenuRig : MonoBehaviour
{
	public Avatar Avatar;

	public BasicAvatarSettings DefaultSettings;

	public CashPile[] CashPiles;

	public void Awake()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		Singleton<LoadManager>.Instance.onSaveInfoLoaded.AddListener(new UnityAction(LoadStuff));
	}

	private void LoadStuff()
	{
		bool flag = false;
		if (LoadManager.LastPlayedGame != null)
		{
			string text = Path.Combine(Path.Combine(LoadManager.LastPlayedGame.SavePath, "Players", "Player_0"), "Appearance.json");
			if (File.Exists(text))
			{
				string text2 = File.ReadAllText(text);
				BasicAvatarSettings basicAvatarSettings = ScriptableObject.CreateInstance<BasicAvatarSettings>();
				JsonUtility.FromJsonOverwrite(text2, (object)basicAvatarSettings);
				Avatar.LoadAvatarSettings(basicAvatarSettings.GetAvatarSettings());
				flag = true;
				Console.Log("Loaded player appearance from " + text);
			}
			float num = LoadManager.LastPlayedGame.Networth;
			for (int i = 0; i < CashPiles.Length; i++)
			{
				float displayedAmount = Mathf.Clamp(num, 0f, 100000f);
				CashPiles[i].SetDisplayedAmount(displayedAmount);
				num -= 100000f;
				if (num <= 0f)
				{
					break;
				}
			}
		}
		if (!flag)
		{
			((Component)Avatar).gameObject.SetActive(false);
		}
	}
}
