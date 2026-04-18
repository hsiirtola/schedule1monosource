using System.Collections.Generic;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.Effects;
using UnityEngine;

namespace ScheduleOne.Weather;

public class WeatherEffectController : EffectController
{
	[Header("Components")]
	[SerializeField]
	protected List<ParticleEffectHandler> particleEffects;

	[SerializeField]
	protected List<VFXEffectHandler> visualEffects;

	[SerializeField]
	protected List<ShaderEffectHandler> shaderEffects;

	[SerializeField]
	protected List<AudioSourceController> _audioSources;

	[Header("Parameters: general")]
	[SerializeField]
	protected string _controllerId;

	[Header("Parameters: Audio")]
	[Tooltip("Min and max distance for audio effects. Max being the distance at which audio is inaudible, and min being the distance at which audio is at full volume")]
	[SerializeField]
	protected Vector2 _minMaxDistanceToPlayer;

	[Tooltip("Uses the blend value of weather volume to determine audio volume rather than distance to player")]
	[SerializeField]
	protected bool _useWeatherBlendForAudio;

	[Tooltip("Used to evaluate audio blending of audio volume (when using distance to player)")]
	[SerializeField]
	protected AnimationCurve _distanceCurve;

	[Tooltip("Used to evaluate audio blending from inside to outside")]
	[SerializeField]
	protected AnimationCurve _enclosureCurve;

	[Header("Parameters: Effects")]
	[Header("Settings: Player Following")]
	[SerializeField]
	protected List<EffectHandler> _effectsToFollowPlayer;

	[Header("Settings: Effects")]
	[SerializeField]
	protected List<EffectSettings> _effectSettings;

	[Header("Settings: Audio")]
	[SerializeField]
	protected List<ScheduleOne.Audio.AudioSettings> _audioSettings;

	[Header("Debugging & Development")]
	[SerializeField]
	protected bool _showGizmos = true;

	protected float _weatherBlend;

	protected WeatherVolume _mainVolume;

	protected WeatherVolume _neighbourVolume;

	private bool NetworkInitialize___EarlyScheduleOne_002EWeather_002EWeatherEffectControllerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EWeather_002EWeatherEffectControllerAssembly_002DCSharp_002Edll_Excuted;

	public string ControllerId => _controllerId;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EWeather_002EWeatherEffectController_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected virtual void Update()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		if (_effectsToFollowPlayer == null || _effectsToFollowPlayer.Count == 0 || (Object)(object)_mainVolume == (Object)null)
		{
			return;
		}
		Vector3 position = default(Vector3);
		foreach (EffectHandler item in _effectsToFollowPlayer)
		{
			float num = Mathf.Clamp(_playerPosition.x, _mainVolume.MinBounds.x, _mainVolume.MaxBounds.x);
			float num2 = Mathf.Clamp(_playerPosition.z, _mainVolume.MinBounds.z, _mainVolume.MaxBounds.z);
			((Vector3)(ref position))._002Ector(num, ((Component)item).transform.position.y, num2);
			item.SetPosition(position);
		}
	}

	public void Initialise(WeatherVolume mainVolume)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		_mainVolume = mainVolume;
		_distanceToPlayerNormalised = -1f;
		_enclosureBlend = -1f;
		Vector3 volumeSize = _mainVolume.VolumeSize;
		volumeSize.x += _mainVolume.BlendAmount;
		volumeSize.y = 1f;
		List<EffectHandler> list = new List<EffectHandler>();
		list.AddRange(particleEffects);
		list.AddRange(visualEffects);
		list.AddRange(shaderEffects);
		foreach (EffectHandler item in list)
		{
			item.Initialise();
			if (item.ScaleToParent)
			{
				item.SetSize(volumeSize);
			}
			if (item.PositionToParent)
			{
				item.SetPosition(((Component)_mainVolume).transform.position);
			}
			item.SetVectorParameterForAll("WeatherOverlapSize", volumeSize);
			item.SetVectorParameterForAll("WeatherBounds", _mainVolume.WeatherBounds);
		}
		UpdateAudio();
	}

	public void SetNeighbourVolume(WeatherVolume neighbourVolume)
	{
		_neighbourVolume = neighbourVolume;
	}

	public override void Activate()
	{
		if (particleEffects != null)
		{
			base.IsActive = true;
		}
	}

	public override void Deactivate()
	{
		if (particleEffects != null)
		{
			base.IsActive = false;
		}
	}

	public void BlendEffects(float blend, AnimationCurve curve)
	{
		_weatherBlend = blend;
		particleEffects.ForEach(delegate(ParticleEffectHandler e)
		{
			SetEffectParamters(e, blend, curve);
		});
	}

	private void SetEffectParamters(EffectHandler effectHandler, float blend, AnimationCurve curve)
	{
		EffectSettings fromEffectSettings = GetFromEffectSettings(effectHandler.Id);
		EffectSettings effectSettings = _effectSettings.Find((EffectSettings s) => s.Id == effectHandler.Id + "Active");
		if ((Object)(object)fromEffectSettings == (Object)null || (Object)(object)effectSettings == (Object)null)
		{
			return;
		}
		foreach (EffectItem toEffectItem in effectSettings.EffectItems)
		{
			EffectItem effectItem = fromEffectSettings.EffectItems.Find((EffectItem i) => i.Name == toEffectItem.Name);
			if (effectItem == null)
			{
				Debug.LogWarning((object)("Effect item '" + toEffectItem.Name + "' not found in fromSettings for effect handler '" + effectHandler.Id + "'"));
				continue;
			}
			foreach (NumericParameter numericParameter2 in toEffectItem.Wrapper.NumericParameters)
			{
				float numericParameter = effectItem.Wrapper.GetNumericParameter(numericParameter2.Variable);
				float value = numericParameter2.Value;
				float value2 = Mathf.Lerp(numericParameter, value, curve.Evaluate(blend));
				effectHandler.SetNumericParameter(toEffectItem.Name, numericParameter2.Variable, value2);
			}
		}
	}

	public void SetShaderNumericParameter(string paramater, float value)
	{
		shaderEffects.ForEach(delegate(ShaderEffectHandler e)
		{
			e.SetNumericParameterForAll(paramater, value);
		});
	}

	public void SetVisualEffectNumericParameter(string paramater, float value)
	{
		visualEffects.ForEach(delegate(VFXEffectHandler e)
		{
			e.SetNumericParameterForAll(paramater, value);
		});
	}

	public void SetShaderColorParameter(string paramater, Color value)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		shaderEffects.ForEach(delegate(ShaderEffectHandler e)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			e.SetColorParameterForAll(paramater, value);
		});
	}

	public EffectSettings FindEffectSettings(string handlerId)
	{
		return _effectSettings.Find((EffectSettings s) => s.Id == handlerId + "Active");
	}

	protected virtual EffectSettings GetFromEffectSettings(string handlerId)
	{
		EffectSettings effectSettings = _effectSettings.Find((EffectSettings s) => s.Id == handlerId + "Inactive");
		if ((Object)(object)_neighbourVolume == (Object)null)
		{
			return effectSettings;
		}
		WeatherEffectController weatherEffectController = _neighbourVolume.EffectControllers.Find((WeatherEffectController e) => e.ControllerId == _controllerId);
		if ((Object)(object)weatherEffectController == (Object)null)
		{
			return effectSettings;
		}
		return weatherEffectController.FindEffectSettings(handlerId) ?? effectSettings;
	}

	public virtual void UpdateAudio()
	{
		foreach (AudioSourceController audioSource in _audioSources)
		{
			ScheduleOne.Audio.AudioSettings audioSettings = _audioSettings.Find((ScheduleOne.Audio.AudioSettings s) => s.Id == audioSource.Id + "Outside");
			ScheduleOne.Audio.AudioSettings audioSettings2 = _audioSettings.Find((ScheduleOne.Audio.AudioSettings s) => s.Id == audioSource.Id + "Inside");
			AudioSettingsWrapper audioSettingsWrapper = audioSource.ExtractAudioSettings();
			if (!((Object)(object)audioSettings2 == (Object)null) && !((Object)(object)audioSettings == (Object)null))
			{
				float num = (_useWeatherBlendForAudio ? _weatherBlend : _distanceToPlayerNormalised);
				audioSettingsWrapper.Volume = Mathf.Lerp(audioSettings.Wrapper.Volume, audioSettings2.Wrapper.Volume, _enclosureCurve.Evaluate(_enclosureBlend)) * _distanceCurve.Evaluate(num);
				audioSettingsWrapper.VolumeMultiplier = Mathf.Lerp(audioSettings.Wrapper.VolumeMultiplier, audioSettings2.Wrapper.VolumeMultiplier, _enclosureCurve.Evaluate(_enclosureBlend));
				audioSettingsWrapper.PitchMultiplier = Mathf.Lerp(audioSettings.Wrapper.PitchMultiplier, audioSettings2.Wrapper.PitchMultiplier, _enclosureCurve.Evaluate(_enclosureBlend));
				audioSettingsWrapper.LowPassCutoffFrequency = (int)MathUtility.LogLerp(audioSettings.Wrapper.LowPassCutoffFrequency, audioSettings2.Wrapper.LowPassCutoffFrequency, _enclosureBlend);
				audioSource.ApplyAudioSettings(audioSettingsWrapper);
			}
		}
	}

	public override void UpdateProperties(Vector3 anchoredPosition, Vector3 playerPosition, float sqrDistanceToPlayer, float enclosureBlend)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		base.UpdateProperties(anchoredPosition, playerPosition, sqrDistanceToPlayer, enclosureBlend);
		float num = Mathf.InverseLerp(_minMaxDistanceToPlayer.y * _minMaxDistanceToPlayer.y, _minMaxDistanceToPlayer.x * _minMaxDistanceToPlayer.x, sqrDistanceToPlayer);
		bool num2 = !MathUtility.NearlyEqual(num, _distanceToPlayerNormalised, 0.01f);
		bool flag = !MathUtility.NearlyEqual(enclosureBlend, _enclosureBlend, 0.01f);
		if (flag)
		{
			_enclosureBlend = enclosureBlend;
		}
		if (num2)
		{
			_distanceToPlayerNormalised = num;
		}
		if (num2 || flag)
		{
			UpdateAudio();
		}
	}

	private void OnDrawGizmos()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (_showGizmos)
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireSphere(_anchoredPosition, _minMaxDistanceToPlayer.x);
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(_anchoredPosition, _minMaxDistanceToPlayer.y);
		}
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002EWeather_002EWeatherEffectControllerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EWeather_002EWeatherEffectControllerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EWeather_002EWeatherEffectControllerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EWeather_002EWeatherEffectControllerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	protected virtual void Awake_UserLogic_ScheduleOne_002EWeather_002EWeatherEffectController_Assembly_002DCSharp_002Edll()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		BlendEffects(0f, AnimationCurve.Linear(0f, 0f, 1f, 1f));
		UpdateProperties(Vector3.zero, Vector3.zero, 0f, 0f);
	}
}
