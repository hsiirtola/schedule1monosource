using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Management.UI;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Tools;
using ScheduleOne.UI;
using ScheduleOne.UI.Management;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.Management;

public class ManagementInterface : Singleton<ManagementInterface>
{
	[Serializable]
	public class ConfigurableTypePanel
	{
		public EConfigurableType Type;

		public ConfigPanel Panel;
	}

	public const float PANEL_SLIDE_TIME = 0f;

	[Header("References")]
	public Canvas Canvas;

	public TextMeshProUGUI NothingSelectedLabel;

	public TextMeshProUGUI DifferentTypesSelectedLabel;

	public RectTransform PanelContainer;

	public ClipboardScreen MainScreen;

	public ItemSelector ItemSelectorScreen;

	public NPCSelector NPCSelector;

	public ObjectSelector ObjectSelector;

	public RecipeSelector RecipeSelectorScreen;

	public TransitEntitySelector TransitEntitySelector;

	public StringSetter StringSetterScreen;

	public Button RenameButton;

	[Header("Screen")]
	public UIScreen UIScreen;

	[SerializeField]
	protected ConfigurableTypePanel[] ConfigPanelPrefabs;

	public List<IConfigurable> Configurables = new List<IConfigurable>();

	private bool areConfigurablesUniform;

	private ConfigPanel loadedPanel;

	public ManagementClipboard_Equippable EquippedClipboard { get; protected set; }

	protected override void Awake()
	{
		base.Awake();
		EConfigurableType[] array = (EConfigurableType[])Enum.GetValues(typeof(EConfigurableType));
		foreach (EConfigurableType eConfigurableType in array)
		{
			if ((Object)(object)GetConfigPanelPrefab(eConfigurableType) == (Object)null)
			{
				Console.LogError($"No config panel prefab assigned for configurable type {eConfigurableType}");
			}
		}
	}

	public void Open(List<IConfigurable> configurables, ManagementClipboard_Equippable _equippedClipboard)
	{
		Configurables = new List<IConfigurable>();
		Configurables.AddRange(configurables);
		EquippedClipboard = _equippedClipboard;
		areConfigurablesUniform = true;
		if (Configurables.Count > 1)
		{
			for (int i = 0; i < Configurables.Count - 1; i++)
			{
				if (Configurables[i].ConfigurableType != Configurables[i + 1].ConfigurableType)
				{
					areConfigurablesUniform = false;
					break;
				}
			}
		}
		((Component)RenameButton).gameObject.SetActive(areConfigurablesUniform && configurables.Count > 0 && configurables[0].Configuration.AllowRename());
		UpdateMainLabels();
		InitializeConfigPanel();
		Singleton<InputPromptsCanvas>.Instance.LoadModule("backonly_rightclick");
	}

	public void Close(bool preserveState = false)
	{
		if (ItemSelectorScreen.IsOpen)
		{
			ItemSelectorScreen.Close();
		}
		if (RecipeSelectorScreen.IsOpen)
		{
			RecipeSelectorScreen.Close();
		}
		if (StringSetterScreen.IsOpen)
		{
			StringSetterScreen.Close();
		}
		if (Singleton<InputPromptsCanvas>.Instance.currentModuleLabel == "exitonly")
		{
			Singleton<InputPromptsCanvas>.Instance.UnloadModule();
		}
		DestroyConfigPanel();
	}

	private void UpdateMainLabels()
	{
		((Component)NothingSelectedLabel).gameObject.SetActive(Configurables.Count == 0);
		((Component)DifferentTypesSelectedLabel).gameObject.SetActive(!areConfigurablesUniform);
	}

	private void InitializeConfigPanel()
	{
		if ((Object)(object)loadedPanel != (Object)null)
		{
			Console.LogWarning("InitializeConfigPanel called when there is an existing config panel. Destroying existing.");
			DestroyConfigPanel();
		}
		if (areConfigurablesUniform && Configurables.Count != 0)
		{
			Singleton<UIScreenManager>.Instance.AddScreen(UIScreen);
			PlayerSingleton<PlayerInventory>.Instance.HolsterEnabled = false;
			ConfigPanel configPanelPrefab = GetConfigPanelPrefab(Configurables[0].ConfigurableType);
			loadedPanel = ((Component)Object.Instantiate<ConfigPanel>(configPanelPrefab, (Transform)(object)PanelContainer)).GetComponent<ConfigPanel>();
			loadedPanel.Bind(Configurables.Select((IConfigurable x) => x.Configuration).ToList(), UIScreen);
		}
	}

	private void DestroyConfigPanel()
	{
		if ((Object)(object)loadedPanel != (Object)null)
		{
			UIScreen.ClearPanels();
			Singleton<UIScreenManager>.Instance.RemoveScreen(UIScreen);
			PlayerSingleton<PlayerInventory>.Instance.HolsterEnabled = true;
			Object.Destroy((Object)(object)((Component)loadedPanel).gameObject);
			loadedPanel = null;
		}
	}

	public ConfigPanel GetConfigPanelPrefab(EConfigurableType type)
	{
		return ConfigPanelPrefabs.FirstOrDefault((ConfigurableTypePanel x) => x.Type == type)?.Panel;
	}

	public void RenameButtonClicked()
	{
		bool flag = true;
		if (Configurables.Count > 1)
		{
			for (int i = 0; i < Configurables.Count - 1; i++)
			{
				if (Configurables[i].Configuration.Name.Value != Configurables[i + 1].Configuration.Name.Value)
				{
					flag = false;
					break;
				}
			}
		}
		StringSetterScreen.Initialize("Rename", flag ? Configurables[0].Configuration.Name.Value : "Mixed", Configurables[0].Configuration.Name.CharacterLimit, allowEmpty: false, CompleteRename);
		StringSetterScreen.Open();
		void CompleteRename(string newName)
		{
			for (int j = 0; j < Configurables.Count; j++)
			{
				Configurables[j].Configuration.Name.SetValue(newName, network: true);
			}
			Singleton<ManagementClipboard>.Instance.SelectionInfo.Set(Configurables);
			EquippedClipboard.SelectionInfo.Set(Configurables);
		}
	}
}
