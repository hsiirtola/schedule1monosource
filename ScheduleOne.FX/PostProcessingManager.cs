using Beautify.Universal;
using CorgiGodRays;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Tools;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ScheduleOne.FX;

public class PostProcessingManager : Singleton<PostProcessingManager>
{
	[Header("References")]
	public UniversalRendererData rendererData;

	public Volume GlobalVolume;

	[Header("Vignette")]
	public float Vig_DefaultIntensity = 0.25f;

	public float Vig_DefaultSmoothness = 0.3f;

	[Header("Blur")]
	public float MinBlur;

	public float MaxBlur = 1f;

	[Header("Post exposre")]
	public AnimationCurve PostExposureCurve;

	public float PostExposureMultiplier = 0.1f;

	[Header("Bloom")]
	public AnimationCurve BloomIntensityCurve;

	[Header("Smoothers")]
	public FloatSmoother ChromaticAberrationController;

	public FloatSmoother SaturationController;

	public FloatSmoother BloomController;

	public HDRColorSmoother ColorFilterController;

	private Vignette vig;

	private DepthOfField DoF;

	private GodRaysVolume GodRays;

	private ColorAdjustments ColorAdjustments;

	private Beautify beautifySettings;

	private Bloom bloom;

	private ChromaticAberration chromaticAberration;

	private ColorAdjustments colorAdjustments;

	private PsychedelicFullScreenFeature _psychedelicFullScreenFeature;

	protected override void Awake()
	{
		base.Awake();
		((Behaviour)GlobalVolume).enabled = true;
		GlobalVolume.sharedProfile.TryGet<Vignette>(ref vig);
		ResetVignette();
		GlobalVolume.sharedProfile.TryGet<DepthOfField>(ref DoF);
		((VolumeComponent)DoF).active = false;
		GlobalVolume.sharedProfile.TryGet<GodRaysVolume>(ref GodRays);
		GlobalVolume.sharedProfile.TryGet<ColorAdjustments>(ref ColorAdjustments);
		GlobalVolume.sharedProfile.TryGet<Beautify>(ref beautifySettings);
		GlobalVolume.sharedProfile.TryGet<Bloom>(ref bloom);
		GlobalVolume.sharedProfile.TryGet<ChromaticAberration>(ref chromaticAberration);
		GlobalVolume.sharedProfile.TryGet<ColorAdjustments>(ref colorAdjustments);
		ChromaticAberrationController.Initialize();
		SaturationController.Initialize();
		BloomController.Initialize();
		ColorFilterController.Initialize();
		SetBlur(0f);
		_psychedelicFullScreenFeature = ((ScriptableRendererData)rendererData).rendererFeatures.Find((ScriptableRendererFeature x) => ((Object)x).name == "PsychedelicFullScreenFeature") as PsychedelicFullScreenFeature;
		if ((Object)(object)_psychedelicFullScreenFeature == (Object)null)
		{
			Debug.LogError((object)"[PostProcessingManager] Could not find PsychedelicFullScreenFeature in the renderer features!");
		}
		SetPsychedelicEffectProperties(_psychedelicFullScreenFeature.GetMaterialPreset("Default"));
	}

	public void Update()
	{
		UpdateEffects();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		SetPsychedelicEffectActive(isActive: false);
	}

	private void UpdateEffects()
	{
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Lerp(0f, 1f, PlayerSingleton<PlayerCamera>.InstanceExists ? PlayerSingleton<PlayerCamera>.Instance.FovJitter : 0f);
		((VolumeParameter<float>)(object)chromaticAberration.intensity).value = ChromaticAberrationController.CurrentValue * (1f + num);
		((VolumeParameter<float>)(object)ColorAdjustments.saturation).value = SaturationController.CurrentValue;
		((VolumeParameter<float>)(object)ColorAdjustments.postExposure).value = PostExposureMultiplier * PostExposureCurve.Evaluate(NetworkSingleton<TimeManager>.Instance.NormalizedTimeOfDay) + num;
		((VolumeParameter<float>)(object)bloom.intensity).value = BloomController.CurrentValue * (1f + num) * BloomIntensityCurve.Evaluate(NetworkSingleton<TimeManager>.Instance.NormalizedTimeOfDay);
		((VolumeParameter<Color>)(object)colorAdjustments.colorFilter).value = ColorFilterController.CurrentValue;
	}

	public void OverrideVignette(float intensity, float smoothness)
	{
		((VolumeParameter<float>)(object)vig.intensity).value = intensity;
		((VolumeParameter<float>)(object)vig.smoothness).value = smoothness;
	}

	public void ResetVignette()
	{
		((VolumeParameter<float>)(object)vig.intensity).value = Vig_DefaultIntensity;
		((VolumeParameter<float>)(object)vig.smoothness).value = Vig_DefaultSmoothness;
	}

	public void SetGodRayIntensity(float intensity)
	{
		((VolumeParameter<float>)(object)GodRays.MainLightIntensity).value = intensity;
	}

	public void SetContrast(float value)
	{
		((VolumeParameter<float>)(object)ColorAdjustments.contrast).value = value;
	}

	public void SetSaturation(float value)
	{
		SaturationController.SetDefault(value, apply: false);
	}

	public void SetBloomThreshold(float threshold)
	{
		((VolumeParameter<float>)(object)bloom.threshold).value = threshold;
	}

	public void SetBlur(float blurLevel)
	{
		((VolumeParameter<float>)(object)beautifySettings.blurIntensity).value = Mathf.Lerp(MinBlur, MaxBlur, blurLevel);
	}

	public void SetPsychedelicEffectActive(bool isActive)
	{
		if (!((Object)(object)_psychedelicFullScreenFeature == (Object)null))
		{
			((ScriptableRendererFeature)_psychedelicFullScreenFeature).SetActive(isActive);
		}
	}

	public void SetPsychedelicEffectProperties(PsychedelicFullScreenData data)
	{
		SetPsychedelicEffectProperties(data.ConvertToMaterialProperties());
	}

	public void SetPsychedelicEffectProperties(PsychedelicFullScreenFeature.MaterialProperties properties)
	{
		_psychedelicFullScreenFeature.SetActiveMaterialProperties(properties);
	}

	public PsychedelicFullScreenFeature.MaterialProperties GetActivePsychedelicEffectProperties()
	{
		return _psychedelicFullScreenFeature.ActiveMaterialProperties;
	}

	public PsychedelicFullScreenData GetPsychedelicEffectDataPreset(string presetName)
	{
		return _psychedelicFullScreenFeature.GetMaterialPreset(presetName);
	}

	public void PrintValueOfPsychedelicEffectBlend()
	{
		_psychedelicFullScreenFeature.PrintMaterialValue();
	}
}
