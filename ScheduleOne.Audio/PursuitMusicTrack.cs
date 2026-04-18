using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Audio;

public class PursuitMusicTrack : MusicTrack
{
	private const float OutOfSightTimeToDipMusic = 8f;

	private const float MinMusicVolume = 0.6f;

	private const float MusicChangeRate_Down = 0.04f;

	private const float MusicChangeRate_Up = 2f;

	[SerializeField]
	private PlayerCrimeData.EPursuitLevel _pursuitLevelToActivate;

	protected virtual void Start()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(OnLoadComplete));
		OnLoadComplete();
	}

	private void OnLoadComplete()
	{
		if ((Object)(object)Player.Local != (Object)null)
		{
			RegisterEvent();
		}
		else
		{
			Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(RegisterEvent));
		}
	}

	private void RegisterEvent()
	{
		Debug.Log((object)"Registering pursuit level change event");
		PlayerCrimeData crimeData = Player.Local.CrimeData;
		crimeData.onPursuitLevelChange = (Action<PlayerCrimeData.EPursuitLevel, PlayerCrimeData.EPursuitLevel>)Delegate.Combine(crimeData.onPursuitLevelChange, new Action<PlayerCrimeData.EPursuitLevel, PlayerCrimeData.EPursuitLevel>(PursuitLevelChange));
	}

	protected override void Update()
	{
		base.Update();
		_volumeMultiplier = GetNewVolume();
	}

	private void PursuitLevelChange(PlayerCrimeData.EPursuitLevel oldLevel, PlayerCrimeData.EPursuitLevel newLevel)
	{
		if (oldLevel != newLevel)
		{
			if (newLevel >= _pursuitLevelToActivate)
			{
				Enable();
				return;
			}
			Disable();
			Stop();
		}
	}

	private float GetNewVolume()
	{
		if ((Object)(object)Player.Local == (Object)null)
		{
			return _volumeMultiplier;
		}
		float volumeMultiplier = _volumeMultiplier;
		volumeMultiplier = ((!(Player.Local.CrimeData.TimeSinceSighted > 8f)) ? (volumeMultiplier + 2f * Time.deltaTime) : (volumeMultiplier - 0.04f * Time.deltaTime));
		return Mathf.Clamp(volumeMultiplier, 0.6f, 1f);
	}
}
