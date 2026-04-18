using FishNet;
using ScheduleOne.DevUtilities;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.UI.Settings;

public class GameSettingsWindow : MonoBehaviour
{
	public UIToggle ConsoleToggle;

	public UIToggle RandomMixMapsToggle;

	public GameObject Blocker;

	public UIPanel uiPanel;

	private void Awake()
	{
		ConsoleToggle.OnChanged.AddListener((UnityAction<bool>)ConsoleToggled);
		RandomMixMapsToggle.OnChanged.AddListener((UnityAction<bool>)RandomMixMapsToggled);
	}

	public void Start()
	{
		ApplySettings(NetworkSingleton<GameManager>.Instance.Settings);
		Blocker.SetActive(!InstanceFinder.IsServer);
		if (!Blocker.activeSelf)
		{
			uiPanel.SelectSelectable(null);
		}
	}

	public void ApplySettings(GameSettings settings)
	{
		ConsoleToggle.SetStateWithoutNotify(settings.ConsoleEnabled);
		RandomMixMapsToggle.SetStateWithoutNotify(settings.UseRandomizedMixMaps);
	}

	private void ConsoleToggled(bool value)
	{
		NetworkSingleton<GameManager>.Instance.Settings.ConsoleEnabled = value;
	}

	private void RandomMixMapsToggled(bool value)
	{
		NetworkSingleton<GameManager>.Instance.Settings.UseRandomizedMixMaps = value;
	}
}
