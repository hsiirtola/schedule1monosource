using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.UI.Settings;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.MainMenu;

public class SettingsScreen : MainMenuScreen
{
	[Serializable]
	public class SettingsCategory
	{
		public Toggle Toggle;

		public GameObject Panel;
	}

	public SettingsCategory[] Categories;

	public Button ApplyDisplayButton;

	public ConfirmDisplaySettings ConfirmDisplaySettings;

	protected override void Awake()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		base.Awake();
		((UnityEvent)ApplyDisplayButton.onClick).AddListener(new UnityAction(ApplyDisplaySettings));
		((Component)ApplyDisplayButton).gameObject.SetActive(false);
	}

	protected void Start()
	{
		for (int i = 0; i < Categories.Length; i++)
		{
			int index = i;
			((UnityEvent<bool>)(object)Categories[i].Toggle.onValueChanged).AddListener((UnityAction<bool>)delegate(bool on)
			{
				if (on)
				{
					ShowCategory(index);
				}
			});
		}
		ShowCategory(0);
	}

	public void ShowCategory(int index)
	{
		for (int i = 0; i < Categories.Length; i++)
		{
			((Selectable)Categories[i].Toggle).interactable = i != index;
			Categories[i].Panel.SetActive(i == index);
		}
	}

	public void DisplayChanged()
	{
		((Component)ApplyDisplayButton).gameObject.SetActive(true);
	}

	private void ApplyDisplaySettings()
	{
		((Component)ApplyDisplayButton).gameObject.SetActive(false);
		DisplaySettings displaySettings = Singleton<ScheduleOne.DevUtilities.Settings>.Instance.DisplaySettings;
		DisplaySettings unappliedDisplaySettings = Singleton<ScheduleOne.DevUtilities.Settings>.Instance.UnappliedDisplaySettings;
		Singleton<ScheduleOne.DevUtilities.Settings>.Instance.ApplyDisplaySettings(unappliedDisplaySettings);
		ConfirmDisplaySettings.Open(displaySettings, unappliedDisplaySettings);
	}
}
