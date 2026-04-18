using System;
using System.Collections.Generic;
using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Law;

public class LawController : Singleton<LawController>, IBaseSaveable, ISaveable
{
	public const float DAILY_INTENSITY_DRAIN = 0.05f;

	[Range(1f, 10f)]
	public int LE_Intensity = 1;

	private float internalLawIntensity;

	[Header("Settings")]
	public LawActivitySettings MondaySettings;

	public LawActivitySettings TuesdaySettings;

	public LawActivitySettings WednesdaySettings;

	public LawActivitySettings ThursdaySettings;

	public LawActivitySettings FridaySettings;

	public LawActivitySettings SaturdaySettings;

	public LawActivitySettings SundaySettings;

	[Header("Demo Settings")]
	public float IntensityIncreasePerDay = 1.5f;

	private LawLoader loader = new LawLoader();

	public bool OverrideSettings { get; protected set; }

	public LawActivitySettings OverriddenSettings { get; protected set; }

	public LawActivitySettings CurrentSettings { get; protected set; }

	public string SaveFolderName => "Law";

	public string SaveFileName => "Law";

	public Loader Loader => loader;

	public bool ShouldSaveUnderFolder => false;

	public List<string> LocalExtraFiles { get; set; } = new List<string>();

	public List<string> LocalExtraFolders { get; set; } = new List<string>();

	public bool HasChanged { get; set; }

	public int LoadOrder { get; }

	protected override void Awake()
	{
		base.Awake();
		InitializeSaveable();
	}

	public virtual void InitializeSaveable()
	{
		Singleton<SaveManager>.Instance.RegisterSaveable(this);
	}

	protected override void Start()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		base.Start();
		NetworkSingleton<TimeManager>.Instance.onUncappedMinutePass += new Action(OnUncappedMinPass);
		TimeManager timeManager = NetworkSingleton<TimeManager>.Instance;
		timeManager.onDayPass = (Action)Delegate.Combine(timeManager.onDayPass, new Action(DayPass));
		Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(OnLoadComplete));
	}

	protected override void OnDestroy()
	{
		if (NetworkSingleton<TimeManager>.InstanceExists)
		{
			NetworkSingleton<TimeManager>.Instance.onUncappedMinutePass -= new Action(OnUncappedMinPass);
			TimeManager timeManager = NetworkSingleton<TimeManager>.Instance;
			timeManager.onDayPass = (Action)Delegate.Remove(timeManager.onDayPass, new Action(DayPass));
		}
		base.OnDestroy();
	}

	private void OnLoadComplete()
	{
		GetSettings().OnLoaded();
	}

	private void OnUncappedMinPass()
	{
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		LawActivitySettings settings = GetSettings();
		if (settings != CurrentSettings)
		{
			if (CurrentSettings != null)
			{
				CurrentSettings.End();
			}
			CurrentSettings = settings;
		}
		CurrentSettings.Evaluate();
	}

	private void DayPass()
	{
		if (InstanceFinder.IsServer)
		{
			ChangeInternalIntensity(IntensityIncreasePerDay / 10f);
		}
	}

	public LawActivitySettings GetSettings()
	{
		if (OverrideSettings && OverriddenSettings != null)
		{
			return OverriddenSettings;
		}
		return GetSettings(NetworkSingleton<TimeManager>.Instance.CurrentDay);
	}

	public LawActivitySettings GetSettings(EDay day)
	{
		return day switch
		{
			EDay.Monday => MondaySettings, 
			EDay.Tuesday => TuesdaySettings, 
			EDay.Wednesday => WednesdaySettings, 
			EDay.Thursday => ThursdaySettings, 
			EDay.Friday => FridaySettings, 
			EDay.Saturday => SaturdaySettings, 
			EDay.Sunday => SundaySettings, 
			_ => null, 
		};
	}

	public void OverrideSetings(LawActivitySettings settings)
	{
		OverrideSettings = true;
		OverriddenSettings = settings;
	}

	public void EndOverride()
	{
		OverrideSettings = false;
		OverriddenSettings = null;
	}

	public void ChangeInternalIntensity(float change)
	{
		internalLawIntensity = Mathf.Clamp01(internalLawIntensity + change);
		LE_Intensity = Mathf.RoundToInt(Mathf.Lerp(1f, 10f, internalLawIntensity));
		HasChanged = true;
	}

	public void SetInternalIntensity(float intensity)
	{
		internalLawIntensity = Mathf.Clamp01(intensity);
		LE_Intensity = Mathf.RoundToInt(Mathf.Lerp(1f, 10f, internalLawIntensity));
		HasChanged = true;
	}

	public virtual string GetSaveString()
	{
		return new LawData(internalLawIntensity).GetJson();
	}

	public void Load(LawData data)
	{
		SetInternalIntensity(data.InternalLawIntensity);
	}
}
