using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Equipping;
using ScheduleOne.Heatmap;
using ScheduleOne.Interaction;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.Misc;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Property;
using ScheduleOne.Temperature;
using ScheduleOne.UI;
using ScheduleOne.UI.Management;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Tools;

public class ManagementClipboard_Equippable : Equippable_Viewmodel
{
	[Header("References")]
	public Transform Clipboard;

	public Transform LoweredPosition;

	public Transform RaisedPosition;

	public ToggleableLight Light;

	public SelectionInfoUI SelectionInfo;

	public TextMeshProUGUI OverrideText;

	private static bool _heatmapToggledOn;

	private ScheduleOne.Property.Property _propertyWithHeatmapShown;

	private static bool _canToggleHeatmap => TemperatureUtility.TemperatureSystemEnabled;

	public static bool ResetHeatmapToggle()
	{
		return _heatmapToggledOn = false;
	}

	public override void Equip(ItemInstance item)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected O, but got Unknown
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		base.Equip(item);
		Singleton<ManagementWorldspaceCanvas>.Instance.Open();
		((Component)Clipboard).transform.position = LoweredPosition.position;
		((Component)OverrideText).gameObject.SetActive(false);
		((Component)SelectionInfo).gameObject.SetActive(true);
		Singleton<ManagementClipboard>.Instance.IsEquipped = true;
		Singleton<ManagementClipboard>.Instance.onOpened.AddListener(new UnityAction(FullscreenEnter));
		Singleton<ManagementClipboard>.Instance.onClosed.AddListener(new UnityAction(FullscreenExit));
		ShowInputPrompts();
		if (Singleton<ManagementClipboard>.Instance.onClipboardEquipped != null)
		{
			Singleton<ManagementClipboard>.Instance.onClipboardEquipped.Invoke();
		}
	}

	private void ShowInputPrompts()
	{
		Singleton<InputPromptsCanvas>.Instance.LoadModule(_canToggleHeatmap ? "clipboard_heatmaptoggle" : "clipboard");
	}

	private void HideInputPrompts()
	{
		if (Singleton<InputPromptsCanvas>.Instance.currentModuleLabel == "clipboard" || Singleton<InputPromptsCanvas>.Instance.currentModuleLabel == "clipboard_heatmaptoggle")
		{
			Singleton<InputPromptsCanvas>.Instance.UnloadModule();
		}
	}

	public override void Unequip()
	{
		base.Unequip();
		if (Singleton<ManagementClipboard>.Instance.IsOpen)
		{
			Singleton<ManagementClipboard>.Instance.Close();
		}
		Singleton<ManagementWorldspaceCanvas>.Instance.Close();
		Singleton<ManagementClipboard>.Instance.IsEquipped = false;
		if (Singleton<ManagementClipboard>.Instance.onClipboardUnequipped != null)
		{
			Singleton<ManagementClipboard>.Instance.onClipboardUnequipped.Invoke();
		}
		HideInputPrompts();
		ClearPropertyWithHeatmapShown();
	}

	protected override void Update()
	{
		base.Update();
		if (GameInput.GetButtonDown(GameInput.ButtonCode.Interact) && CanToggleClipboard())
		{
			if (Singleton<ManagementClipboard>.Instance.IsOpen)
			{
				Singleton<ManagementClipboard>.Instance.Close();
			}
			else
			{
				List<IConfigurable> list = new List<IConfigurable>();
				list.AddRange(Singleton<ManagementWorldspaceCanvas>.Instance.SelectedConfigurables);
				if (Singleton<ManagementWorldspaceCanvas>.Instance.HoveredConfigurable != null && !list.Contains(Singleton<ManagementWorldspaceCanvas>.Instance.HoveredConfigurable))
				{
					list.Add(Singleton<ManagementWorldspaceCanvas>.Instance.HoveredConfigurable);
				}
				Singleton<ManagementClipboard>.Instance.Open(list, this);
			}
		}
		UpdateHeatmap();
	}

	private bool CanToggleClipboard()
	{
		if (Singleton<ManagementInterface>.InstanceExists)
		{
			if (Singleton<ManagementInterface>.Instance.ObjectSelector.IsOpen)
			{
				return false;
			}
			if (Singleton<ManagementInterface>.Instance.TransitEntitySelector.IsOpen)
			{
				return false;
			}
			if (Singleton<ManagementInterface>.Instance.NPCSelector.IsOpen)
			{
				return false;
			}
		}
		if (!GameInput.IsTyping)
		{
			return (Object)(object)Singleton<InteractionManager>.Instance.HoveredValidInteractableObject == (Object)null;
		}
		return false;
	}

	private void UpdateHeatmap()
	{
		if (_canToggleHeatmap && GameInput.GetButtonDown(GameInput.ButtonCode.VehicleToggleLights) && !GameInput.IsTyping)
		{
			_heatmapToggledOn = !_heatmapToggledOn;
		}
		if (_heatmapToggledOn)
		{
			ScheduleOne.Property.Property property = ScheduleOne.Property.Property.OwnedProperties.OrderBy((ScheduleOne.Property.Property x) => Vector3.SqrMagnitude(x.SpawnPoint.position - ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position)).FirstOrDefault();
			if ((Object)(object)property != (Object)(object)_propertyWithHeatmapShown)
			{
				ClearPropertyWithHeatmapShown();
				_propertyWithHeatmapShown = property;
				if (Singleton<HeatmapManager>.InstanceExists)
				{
					Singleton<HeatmapManager>.Instance.SetHeatmapActive(_propertyWithHeatmapShown, isActive: true);
				}
			}
		}
		else
		{
			ClearPropertyWithHeatmapShown();
		}
	}

	private void ClearPropertyWithHeatmapShown()
	{
		if ((Object)(object)_propertyWithHeatmapShown != (Object)null)
		{
			if (Singleton<HeatmapManager>.InstanceExists)
			{
				Singleton<HeatmapManager>.Instance.SetHeatmapActive(_propertyWithHeatmapShown, isActive: false);
			}
			_propertyWithHeatmapShown = null;
		}
	}

	private void FullscreenEnter()
	{
		Singleton<ManagementWorldspaceCanvas>.Instance.Close(preserveSelection: true);
		((Component)Clipboard).gameObject.SetActive(false);
		HideInputPrompts();
	}

	private void FullscreenExit()
	{
		((Component)Clipboard).gameObject.SetActive(true);
		if (!Singleton<ManagementClipboard>.Instance.IsOpen && !Singleton<ManagementClipboard>.Instance.StatePreserved)
		{
			Singleton<ManagementWorldspaceCanvas>.Instance.Open();
			ShowInputPrompts();
		}
	}

	public void OverrideClipboardText(string overriddenText)
	{
		((TMP_Text)OverrideText).text = overriddenText;
		((Component)OverrideText).gameObject.SetActive(true);
		((Component)SelectionInfo).gameObject.SetActive(false);
	}

	public void EndOverride()
	{
		((Component)OverrideText).gameObject.SetActive(false);
		((Component)SelectionInfo).gameObject.SetActive(true);
	}
}
