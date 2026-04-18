using System;
using Beautify.Universal;
using FishNet.Object;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace ScheduleOne.FX;

public class PlayerHealthVisuals : MonoBehaviour
{
	[Header("References")]
	public Volume[] PPVolumes;

	[Header("Vignette")]
	public float VignetteAlpha_MaxHealth;

	public float VignetteAlpha_MinHealth;

	public AnimationCurve OuterRingCurve;

	[Header("Saturation")]
	public float Saturation_MaxHealth = 0.5f;

	public float Saturation_MinHealth = -2f;

	[Header("Chromatic Abberation")]
	public float ChromAb_MaxHealth;

	public float ChromAb_MinHealth = 0.02f;

	[Header("Lens Dirt")]
	public float LensDirt_MaxHealth;

	public float LensDirt_MinHealth = 1f;

	private Beautify[] _beautifySettings;

	private void Awake()
	{
		Player.onLocalPlayerSpawned = (Action)Delegate.Remove(Player.onLocalPlayerSpawned, new Action(Spawned));
		Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(Spawned));
		NetworkSingleton<TimeManager>.Instance.onMinutePass -= new Action(MinPass);
		NetworkSingleton<TimeManager>.Instance.onMinutePass += new Action(MinPass);
		_beautifySettings = (Beautify[])(object)new Beautify[PPVolumes.Length];
		Beautify val = default(Beautify);
		for (int i = 0; i < PPVolumes.Length; i++)
		{
			PPVolumes[i].sharedProfile.TryGet<Beautify>(ref val);
			_beautifySettings[i] = val;
		}
	}

	private void Spawned()
	{
		if (((NetworkBehaviour)Player.Local).Owner.IsLocalClient)
		{
			UpdateEffects(Player.Local.Health.CurrentHealth);
			Player.Local.Health.onHealthChanged.AddListener((UnityAction<float>)UpdateEffects);
		}
	}

	private void MinPass()
	{
		Beautify[] beautifySettings = _beautifySettings;
		for (int i = 0; i < beautifySettings.Length; i++)
		{
			((VolumeParameter<float>)(object)beautifySettings[i].vignettingOuterRing).value = OuterRingCurve.Evaluate(NetworkSingleton<TimeManager>.Instance.NormalizedTimeOfDay);
		}
	}

	private void UpdateEffects(float newHealth)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		Beautify[] beautifySettings = _beautifySettings;
		foreach (Beautify val in beautifySettings)
		{
			((VolumeParameter<Color>)(object)val.vignettingColor).value = new Color(((VolumeParameter<Color>)(object)val.vignettingColor).value.r, ((VolumeParameter<Color>)(object)val.vignettingColor).value.g, ((VolumeParameter<Color>)(object)val.vignettingColor).value.b, Mathf.Lerp(VignetteAlpha_MinHealth, VignetteAlpha_MaxHealth, newHealth / 100f));
			((VolumeParameter<float>)(object)val.saturate).value = Mathf.Lerp(Saturation_MinHealth, Saturation_MaxHealth, newHealth / 100f);
			((VolumeParameter<float>)(object)val.chromaticAberrationIntensity).value = Mathf.Lerp(ChromAb_MinHealth, ChromAb_MaxHealth, newHealth / 100f);
			((VolumeParameter<float>)(object)val.lensDirtIntensity).value = Mathf.Lerp(LensDirt_MinHealth, LensDirt_MaxHealth, newHealth / 100f);
		}
	}
}
