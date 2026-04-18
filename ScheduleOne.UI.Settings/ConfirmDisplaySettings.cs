using ScheduleOne.DevUtilities;
using TMPro;
using UnityEngine;

namespace ScheduleOne.UI.Settings;

public class ConfirmDisplaySettings : MonoBehaviour
{
	public const float RevertTime = 15f;

	public TextMeshProUGUI SubtitleLabel;

	private float timeUntilRevert;

	private DisplaySettings oldSettings;

	private DisplaySettings newSettings;

	public bool IsOpen
	{
		get
		{
			if ((Object)(object)this != (Object)null && (Object)(object)((Component)this).gameObject != (Object)null)
			{
				return ((Component)this).gameObject.activeSelf;
			}
			return false;
		}
	}

	public void Awake()
	{
		GameInput.RegisterExitListener(Exit, 6);
	}

	public void Open(DisplaySettings _oldSettings, DisplaySettings _newSettings)
	{
		((Component)this).gameObject.SetActive(true);
		oldSettings = _oldSettings;
		newSettings = _newSettings;
		timeUntilRevert = 15f;
		Update();
	}

	public void Exit(ExitAction action)
	{
		if (!action.Used && IsOpen && action.exitType == ExitType.Escape)
		{
			action.Used = true;
			Close(revert: true);
		}
	}

	public void Update()
	{
		timeUntilRevert -= Time.unscaledDeltaTime;
		((TMP_Text)SubtitleLabel).text = $"Reverting in {timeUntilRevert:0.0} seconds";
		if (timeUntilRevert <= 0f)
		{
			Close(revert: true);
		}
	}

	public void Close(bool revert)
	{
		if (revert)
		{
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.ApplyDisplaySettings(oldSettings);
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.DisplaySettings = oldSettings;
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.UnappliedDisplaySettings = oldSettings;
		}
		else
		{
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.WriteDisplaySettings(newSettings);
		}
		((Component)((Component)this).transform.parent).gameObject.SetActive(false);
		((Component)((Component)this).transform.parent).gameObject.SetActive(true);
		((Component)this).gameObject.SetActive(false);
	}
}
