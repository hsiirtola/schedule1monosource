using Funly.SkyStudio;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Tools;
using ScheduleOne.Weather;
using UnityEngine;
using VolumetricFogAndMist2;

namespace ScheduleOne.FX;

public class EnvironmentFX : Singleton<EnvironmentFX>
{
	[Header("References")]
	[SerializeField]
	protected TimeOfDayController timeOfDayController;

	public VolumetricFog VolumetricFog;

	public Light SunLight;

	public Light MoonLight;

	[Header("Height Fog")]
	[SerializeField]
	protected Gradient HeightFogColor;

	[SerializeField]
	protected AnimationCurve HeightFogIntensityCurve;

	[SerializeField]
	protected float HeightFogIntensityMultiplier = 0.5f;

	[SerializeField]
	protected AnimationCurve HeightFogDirectionalIntensityCurve;

	[Header("Volumetric Fog")]
	[SerializeField]
	protected AnimationCurve VolumetricFogIntensityCurve;

	[SerializeField]
	protected float VolumetricFogIntensityMultiplier = 0.5f;

	[SerializeField]
	protected float VolumetricFogSaturationMultiplier = 1f;

	[Header("Fog")]
	[SerializeField]
	private float fogEndDistanceMultiplier = 250f;

	[Header("God rays")]
	[SerializeField]
	protected AnimationCurve godRayIntensityCurve;

	[Header("Contrast")]
	[SerializeField]
	protected AnimationCurve contrastCurve;

	[SerializeField]
	protected float contractMultiplier = 1f;

	[Header("Saturation")]
	[SerializeField]
	protected AnimationCurve saturationCurve;

	[SerializeField]
	protected float saturationMultiplier = 1f;

	[Header("Grass")]
	[SerializeField]
	protected Material grassMat;

	[SerializeField]
	protected Gradient grassColorGradient;

	[Header("Trees")]
	public Material distanceTreeMat;

	public AnimationCurve distanceTreeColorCurve;

	[Header("Stealth settings")]
	public AnimationCurve environmentalBrightnessCurve;

	[Header("Bloom")]
	public AnimationCurve bloomThreshholdCurve;

	[Header("Gloabl Shader Properties")]
	[SerializeField]
	private float _environmentScrollSpeed;

	[SerializeField]
	private float _testPercentage;

	public FloatSmoother FogEndDistanceController;

	private float _scrollTime;

	private float _scrollValue;

	private bool _scrollTActive;

	private Color _defaultDistantTreeMatColor;

	private Color _defaultGrassMatColor;

	public float normalizedEnvironmentalBrightness => environmentalBrightnessCurve.Evaluate(NetworkSingleton<TimeManager>.Instance.NormalizedTimeOfDay);

	public float FogEndDistanceMultiplier => fogEndDistanceMultiplier * FogEndDistanceController.CurrentValue;

	protected override void Awake()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		FogEndDistanceController = new FloatSmoother();
		FogEndDistanceController.Initialize();
		FogEndDistanceController.SetSmoothingSpeed(0.2f);
		FogEndDistanceController.SetDefault(1f);
		_defaultDistantTreeMatColor = distanceTreeMat.GetColor("_TintColor");
		_defaultGrassMatColor = grassMat.GetColor("_BaseColor");
		((MonoBehaviour)this).InvokeRepeating("UpdateVisuals", 0f, 0.1f);
		Shader.SetGlobalFloat("_ScrollTime", _scrollTime);
	}

	protected override void Start()
	{
		base.Start();
		UpdateVisuals();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void Update()
	{
		if (_scrollTActive)
		{
			_scrollTime += Time.deltaTime * _scrollValue;
			Shader.SetGlobalFloat("_ScrollTime", _scrollTime);
		}
	}

	private void UpdateVisuals()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		if (Application.isPlaying)
		{
			timeOfDayController.skyTime = NetworkSingleton<TimeManager>.Instance.NormalizedTimeOfDay;
			Color val = ((NetworkSingleton<EnvironmentManager>.Instance.CurrentSkyState != null) ? NetworkSingleton<EnvironmentManager>.Instance.CurrentSkyState.FogColor : Color.clear);
			float num = default(float);
			float num3 = default(float);
			float num2 = default(float);
			Color.RGBToHSV(val, ref num, ref num2, ref num3);
			num2 *= VolumetricFogSaturationMultiplier;
			val = Color.HSVToRGB(num, num2, num3);
			val.a = VolumetricFogIntensityCurve.Evaluate(timeOfDayController.skyTime) * VolumetricFogIntensityMultiplier;
			VolumetricFog.profile.albedo = val;
			byte b = (byte)distanceTreeColorCurve.Evaluate(NetworkSingleton<TimeManager>.Instance.NormalizedTimeOfDay);
			distanceTreeMat.SetColor("_TintColor", Color32.op_Implicit(new Color32(b, b, b, byte.MaxValue)));
			grassMat.SetColor("_BaseColor", grassColorGradient.Evaluate(timeOfDayController.skyTime));
			Singleton<PostProcessingManager>.Instance.SetGodRayIntensity(godRayIntensityCurve.Evaluate(timeOfDayController.skyTime));
			Singleton<PostProcessingManager>.Instance.SetContrast(contrastCurve.Evaluate(timeOfDayController.skyTime) * contractMultiplier);
			Singleton<PostProcessingManager>.Instance.SetSaturation(saturationCurve.Evaluate(timeOfDayController.skyTime) * saturationMultiplier);
			Singleton<PostProcessingManager>.Instance.SetBloomThreshold(bloomThreshholdCurve.Evaluate(timeOfDayController.skyTime));
		}
	}

	public void SetEnvironmentScrollingActive(bool active)
	{
		_scrollTActive = active;
	}

	public void SetEnvironmentScrollingSpeedByPercentage(float percentage)
	{
		_scrollValue = percentage;
	}
}
