using System;
using Steamworks;
using UnityEngine;

namespace ScheduleOne;

public static class OnScreenKeyboard
{
	private static uint s_charLimit;

	private static Action<string> s_onSubmit;

	private static Action s_onCancel;

	private static Callback<GamepadTextInputDismissed_t> s_onGamepadTextInputDismissed;

	public static void Show(Action<string> onSubmit, Action onCancel = null, string description = "", uint charMax = 32u, string defaultText = "")
	{
		if (SteamManager.Initialized)
		{
			if (s_onGamepadTextInputDismissed == null)
			{
				s_onGamepadTextInputDismissed = Callback<GamepadTextInputDismissed_t>.Create((DispatchDelegate<GamepadTextInputDismissed_t>)OnGamepadTextInputDismissed);
			}
			if (SteamUtils.IsSteamRunningOnSteamDeck() || SteamUtils.IsSteamInBigPictureMode())
			{
				SteamUtils.ShowGamepadTextInput((EGamepadTextInputMode)0, (EGamepadTextInputLineMode)0, description, charMax, defaultText);
				Debug.LogWarning((object)"[OnScreenKeyboard] Showing OSK");
			}
			else
			{
				Debug.LogWarning((object)"[OnScreenKeyboard] OSK not supported on this platform/mode");
			}
		}
		else
		{
			Debug.LogWarning((object)"[OnScreenKeyboard] SteamManager is not initialized");
		}
		s_onSubmit = onSubmit;
		s_onCancel = onCancel;
		s_charLimit = charMax;
	}

	private static void OnGamepadTextInputDismissed(GamepadTextInputDismissed_t param)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		string obj = default(string);
		SteamUtils.GetEnteredGamepadTextInput(ref obj, s_charLimit);
		if (param.m_bSubmitted && s_onSubmit != null)
		{
			s_onSubmit(obj);
		}
		else if (!param.m_bSubmitted && s_onCancel != null)
		{
			s_onCancel();
		}
		s_onSubmit = null;
		s_onCancel = null;
		s_charLimit = 0u;
	}
}
