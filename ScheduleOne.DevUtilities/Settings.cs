using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using ScheduleOne.Audio;
using ScheduleOne.Networking;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

namespace ScheduleOne.DevUtilities;

public class Settings : PersistentSingleton<Settings>
{
	public enum EUnitType
	{
		Metric,
		Imperial
	}

	public const float MinYPos = -20f;

	public const string BETA_ARG = "-beta";

	public List<string> LaunchArgs = new List<string>();

	public DisplaySettings DisplaySettings;

	public DisplaySettings UnappliedDisplaySettings;

	public GraphicsSettings GraphicsSettings = new GraphicsSettings();

	public AudioSettings AudioSettings = new AudioSettings();

	public InputSettings InputSettings = new InputSettings();

	public OtherSettings OtherSettings = new OtherSettings();

	public InputActionAsset InputActions;

	public GameInput GameInput;

	public ScriptableRendererFeature SSAO;

	public ScriptableRendererFeature GodRays;

	[Header("Camera")]
	public float LookSensitivity = 1f;

	public bool InvertMouse;

	public float CameraFOV = 75f;

	public InputSettings.EActionMode SprintMode = InputSettings.EActionMode.Hold;

	[Range(0f, 1f)]
	public float CameraBobIntensity = 1f;

	private InputActionMap playerControls;

	public Action onInputsApplied;

	public Action onDisplaySettingsApplied;

	public Action onUnappliedDisplayIndexChanged;

	public static bool ChristmasEventActive { get; private set; }

	public bool PausingFreezesTime
	{
		get
		{
			if (Player.PlayerList.Count <= 1)
			{
				return !Singleton<Lobby>.Instance.IsInLobby;
			}
			return false;
		}
	}

	public EUnitType UnitType { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		if ((Object)(object)Singleton<Settings>.Instance == (Object)null || (Object)(object)Singleton<Settings>.Instance != (Object)(object)this)
		{
			return;
		}
		playerControls = InputActions.FindActionMap("Generic", false);
		DisplaySettings = ReadDisplaySettings();
		UnappliedDisplaySettings = ReadDisplaySettings();
		GraphicsSettings = ReadGraphicsSettings();
		AudioSettings = ReadAudioSettings();
		InputSettings = ReadInputSettings();
		OtherSettings = ReadOtherSettings();
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			LaunchArgs.Add(commandLineArgs[i]);
			if (commandLineArgs[i] == "-beta")
			{
				GameManager.IS_BETA = true;
			}
		}
		if (DateTime.Now.Month == 12 && DateTime.Now.Day >= 20)
		{
			ChristmasEventActive = true;
			Console.Log("Christmas event is active!");
		}
	}

	protected override void Start()
	{
		base.Start();
		ApplyDisplaySettings(DisplaySettings);
		ApplyGraphicsSettings(GraphicsSettings);
		ApplyAudioSettings(AudioSettings);
		ApplyInputSettings(InputSettings);
		ApplyOtherSettings(OtherSettings);
	}

	public void ApplyDisplaySettings(DisplaySettings settings)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		Resolution[] array = DisplaySettings.GetResolutions().ToArray();
		Resolution resolution = array[Mathf.Clamp(settings.ResolutionIndex, 0, array.Length - 1)];
		FullScreenMode mode = (FullScreenMode)3;
		switch (settings.DisplayMode)
		{
		case DisplaySettings.EDisplayMode.Windowed:
			mode = (FullScreenMode)3;
			break;
		case DisplaySettings.EDisplayMode.FullscreenWindow:
			mode = (FullScreenMode)1;
			break;
		case DisplaySettings.EDisplayMode.ExclusiveFullscreen:
			mode = (FullScreenMode)0;
			break;
		}
		List<DisplayInfo> list = new List<DisplayInfo>();
		Screen.GetDisplayLayout(list);
		DisplayInfo val = list[Mathf.Clamp(settings.ActiveDisplayIndex, 0, list.Count - 1)];
		val.refreshRate = ((Resolution)(ref resolution)).refreshRateRatio;
		MoveMainWindowTo(val);
		Console.Log("Active display set to: " + val.name + "(" + settings.ActiveDisplayIndex + ") with resolution " + val.width + "x" + val.height);
		QualitySettings.vSyncCount = (settings.VSync ? 1 : 0);
		Application.targetFrameRate = settings.TargetFPS;
		CanvasScaler.SetScaleFactor(settings.UIScale);
		Singleton<Settings>.Instance.CameraBobIntensity = settings.CameraBobbing;
		UnitType = settings.UnitType;
		((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Wait());
		IEnumerator Wait()
		{
			yield return null;
			Console.Log("Setting resolution to: " + ((Resolution)(ref resolution)).width + "x" + ((Resolution)(ref resolution)).height + " @ " + ((object)System.Runtime.CompilerServices.Unsafe.As<FullScreenMode, FullScreenMode>(ref mode)/*cast due to .constrained prefix*/).ToString());
			Screen.SetResolution(((Resolution)(ref resolution)).width, ((Resolution)(ref resolution)).height, mode, ((Resolution)(ref resolution)).refreshRateRatio);
			if (onDisplaySettingsApplied != null)
			{
				onDisplaySettingsApplied();
			}
		}
	}

	private void MoveMainWindowTo(DisplayInfo displayInfo)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		Screen.MoveMainWindowTo(ref displayInfo, new Vector2Int(displayInfo.width / 2, displayInfo.height / 2));
	}

	public void ReloadGraphicsSettings()
	{
		ApplyGraphicsSettings(GraphicsSettings);
	}

	public void ApplyGraphicsSettings(GraphicsSettings settings)
	{
		QualitySettings.SetQualityLevel((int)settings.GraphicsQuality);
		PlayerCamera.SetAntialiasingMode(settings.AntiAliasingMode);
		CameraFOV = settings.FOV;
		SSAO.SetActive(settings.SSAO);
		GodRays.SetActive(settings.GodRays);
	}

	public void ReloadAudioSettings()
	{
		ApplyAudioSettings(AudioSettings);
	}

	public void ApplyAudioSettings(AudioSettings settings)
	{
		Singleton<AudioManager>.Instance.SetMasterVolume(settings.MasterVolume);
		Singleton<AudioManager>.Instance.SetVolume(EAudioType.Ambient, settings.AmbientVolume);
		Singleton<AudioManager>.Instance.SetVolume(EAudioType.Music, settings.MusicVolume);
		Singleton<AudioManager>.Instance.SetVolume(EAudioType.FX, settings.SFXVolume);
		Singleton<AudioManager>.Instance.SetVolume(EAudioType.UI, settings.UIVolume);
		Singleton<AudioManager>.Instance.SetVolume(EAudioType.Voice, settings.DialogueVolume);
		Singleton<AudioManager>.Instance.SetVolume(EAudioType.Footsteps, settings.FootstepsVolume);
		Singleton<AudioManager>.Instance.SetVolume(EAudioType.Weather, settings.WeatherVolume);
	}

	public void ReloadInputSettings()
	{
		ApplyInputSettings(InputSettings);
	}

	public void ApplyInputSettings(InputSettings settings)
	{
		InputSettings = settings;
		LookSensitivity = settings.MouseSensitivity;
		InvertMouse = settings.InvertMouse;
		SprintMode = settings.SprintMode;
		InputActions.Disable();
		InputActionRebindingExtensions.LoadBindingOverridesFromJson((IInputActionCollection2)(object)InputActions, settings.BindingOverrides, true);
		InputActions.Enable();
		GameInput.PlayerInput.actions = InputActions;
		onInputsApplied?.Invoke();
	}

	public void ReloadOtherSettings()
	{
		ApplyOtherSettings(OtherSettings);
	}

	public void ApplyOtherSettings(OtherSettings settings)
	{
		OtherSettings = settings;
	}

	public void WriteDisplaySettings(DisplaySettings settings)
	{
		DisplaySettings = settings;
		UnappliedDisplaySettings = settings;
		PlayerPrefs.SetInt("ResolutionIndex", settings.ResolutionIndex);
		PlayerPrefs.SetInt("DisplayMode", (int)settings.DisplayMode);
		PlayerPrefs.SetInt("VSync", settings.VSync ? 1 : 0);
		PlayerPrefs.SetInt("TargetFPS", settings.TargetFPS);
		PlayerPrefs.SetFloat("UIScale", settings.UIScale);
		PlayerPrefs.SetFloat("CameraBobbing", settings.CameraBobbing);
		PlayerPrefs.SetInt("ActiveDisplayIndex", settings.ActiveDisplayIndex);
		PlayerPrefs.SetInt("UnitType", (int)settings.UnitType);
	}

	public DisplaySettings ReadDisplaySettings()
	{
		DisplaySettings result = new DisplaySettings
		{
			ResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", Screen.resolutions.Length - 1),
			DisplayMode = (DisplaySettings.EDisplayMode)PlayerPrefs.GetInt("DisplayMode", 2),
			VSync = (PlayerPrefs.GetInt("VSync", 1) == 1),
			TargetFPS = PlayerPrefs.GetInt("TargetFPS", 90),
			UIScale = PlayerPrefs.GetFloat("UIScale", 1f),
			CameraBobbing = PlayerPrefs.GetFloat("CameraBobbing", 0.7f),
			ActiveDisplayIndex = PlayerPrefs.GetInt("ActiveDisplayIndex", 0)
		};
		EUnitType defaultUnitTypeForPlayer = GetDefaultUnitTypeForPlayer();
		Debug.Log((object)$"Default unit type for player region is {defaultUnitTypeForPlayer}");
		result.UnitType = (EUnitType)PlayerPrefs.GetInt("UnitType", (int)defaultUnitTypeForPlayer);
		return result;
	}

	public void WriteGraphicsSettings(GraphicsSettings settings)
	{
		GraphicsSettings = settings;
		PlayerPrefs.SetInt("QualityLevel", (int)settings.GraphicsQuality);
		PlayerPrefs.SetInt("AntiAliasing", (int)settings.AntiAliasingMode);
		PlayerPrefs.SetFloat("FOV", settings.FOV);
		PlayerPrefs.SetInt("SSAO", settings.SSAO ? 1 : 0);
		PlayerPrefs.SetInt("GodRays", settings.GodRays ? 1 : 0);
	}

	public GraphicsSettings ReadGraphicsSettings()
	{
		return new GraphicsSettings
		{
			GraphicsQuality = (GraphicsSettings.EGraphicsQuality)PlayerPrefs.GetInt("QualityLevel", 2),
			AntiAliasingMode = (GraphicsSettings.EAntiAliasingMode)PlayerPrefs.GetInt("AntiAliasing", 2),
			FOV = PlayerPrefs.GetFloat("FOV", 80f),
			SSAO = (PlayerPrefs.GetInt("SSAO", 1) == 1),
			GodRays = (PlayerPrefs.GetInt("GodRays", 1) == 1)
		};
	}

	public void WriteAudioSettings(AudioSettings settings)
	{
		AudioSettings = settings;
		PlayerPrefs.SetFloat("MasterVolume", settings.MasterVolume);
		PlayerPrefs.SetFloat("AmbientVolume", settings.AmbientVolume);
		PlayerPrefs.SetFloat("MusicVolume", settings.MusicVolume);
		PlayerPrefs.SetFloat("SFXVolume", settings.SFXVolume);
		PlayerPrefs.SetFloat("UIVolume", settings.UIVolume);
		PlayerPrefs.SetFloat("DialogueVolume", settings.DialogueVolume);
		PlayerPrefs.SetFloat("FootstepsVolume", settings.FootstepsVolume);
		PlayerPrefs.SetFloat("WeatherVolume", settings.WeatherVolume);
	}

	public AudioSettings ReadAudioSettings()
	{
		return new AudioSettings
		{
			MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f),
			AmbientVolume = PlayerPrefs.GetFloat("AmbientVolume", 1f),
			MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f),
			SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f),
			UIVolume = PlayerPrefs.GetFloat("UIVolume", 1f),
			DialogueVolume = PlayerPrefs.GetFloat("DialogueVolume", 1f),
			FootstepsVolume = PlayerPrefs.GetFloat("FootstepsVolume", 1f),
			WeatherVolume = PlayerPrefs.GetFloat("WeatherVolume", 1f)
		};
	}

	public void WriteInputSettings(InputSettings settings)
	{
		InputSettings = settings;
		PlayerPrefs.SetFloat("MouseSensitivity", settings.MouseSensitivity);
		PlayerPrefs.SetInt("InvertMouse", settings.InvertMouse ? 1 : 0);
		PlayerPrefs.SetInt("SprintMode", (int)settings.SprintMode);
		string text = InputActionRebindingExtensions.SaveBindingOverridesAsJson((IInputActionCollection2)(object)GameInput.PlayerInput.actions);
		PlayerPrefs.SetString("BindingOverrides", text);
	}

	public InputSettings ReadInputSettings()
	{
		return new InputSettings
		{
			MouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f),
			InvertMouse = (PlayerPrefs.GetInt("InvertMouse", 0) == 1),
			SprintMode = (InputSettings.EActionMode)PlayerPrefs.GetInt("SprintMode", 0),
			BindingOverrides = PlayerPrefs.GetString("BindingOverrides", InputActionRebindingExtensions.SaveBindingOverridesAsJson((IInputActionCollection2)(object)GameInput.PlayerInput.actions))
		};
	}

	public void WriteOtherSettings(OtherSettings settings)
	{
		OtherSettings = settings;
		PlayerPrefs.SetInt("AutoBackupSaves", settings.AutoBackupSaves ? 1 : 0);
	}

	public OtherSettings ReadOtherSettings()
	{
		return new OtherSettings
		{
			AutoBackupSaves = (PlayerPrefs.GetInt("AutoBackupSaves", 1) == 1)
		};
	}

	public string GetActionControlPath(string actionName)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		InputAction val = playerControls.FindAction(actionName, false);
		if (val == null)
		{
			Console.LogError("Could not find action with name '" + actionName + "'");
			return string.Empty;
		}
		return val.controls[0].path;
	}

	private EUnitType GetDefaultUnitTypeForPlayer()
	{
		RegionInfo regionInfo = new RegionInfo(CultureInfo.CurrentCulture.LCID);
		if (regionInfo != null && (regionInfo.TwoLetterISORegionName == "US" || regionInfo.TwoLetterISORegionName == "LR" || regionInfo.TwoLetterISORegionName == "MM"))
		{
			return EUnitType.Imperial;
		}
		return EUnitType.Metric;
	}
}
