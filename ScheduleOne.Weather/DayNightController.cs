using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Weather;

public class DayNightController : MonoBehaviour
{
	private const float SunShadowStrength = 0.95f;

	private const float MoonShadowStrength = 0.8f;

	[Header("Components")]
	[SerializeField]
	private GameObject _lightPivot;

	[SerializeField]
	private MeshRenderer _skyRenderer;

	[Header("Lights")]
	[SerializeField]
	private Light _sunLight;

	[SerializeField]
	private Light _moonLight;

	[SerializeField]
	private Light _ambientLight;

	[SerializeField]
	private AnimationCurve _fadeInCurve;

	[SerializeField]
	private AnimationCurve _fadeOutCurve;

	[Header("Debugging & Development")]
	[SerializeField]
	private float _debugRotationSpeed = 5f;

	[SerializeField]
	private float _debugTimeSpeed = 3f;

	[SerializeField]
	private bool _enableDebugTimeControl;

	[SerializeField]
	private bool _debugAutoUpdateTime;

	[SerializeField]
	[Range(0f, 24f)]
	private float _timeInHours;

	private float _timePercentage;

	private bool _isDay;

	private Quaternion _currentSunRotation;

	private Quaternion _currentMoonRotation;

	[SerializeField]
	private DayNightPhaseTimes _dayNightPhaseTimes = new DayNightPhaseTimes
	{
		MinDawnHour = 5,
		SunRiseHour = 6,
		MaxDawnHour = 7,
		MinDuskHour = 17,
		SunSetHour = 19,
		MaxDuskHour = 20
	};

	public const float MAX_LIGHT_INTENSITY = 4f;

	public bool EnableDebugTimeControl => _enableDebugTimeControl;

	private void Update()
	{
		if (_debugAutoUpdateTime)
		{
			_timeInHours += Time.deltaTime * _debugTimeSpeed / 20f;
			_timeInHours %= 24f;
		}
		if (_enableDebugTimeControl)
		{
			_timePercentage = _timeInHours / 24f;
			Shader.SetGlobalFloat("_TimeOfDay", _timePercentage);
			SetRotation();
		}
		UpdateRotation();
	}

	public SkyState EvaluateSky(SkySettings activeSettings, SkySettings neighbourSettings, float blend, SkySettings overrideSkySettings = null, float overrideBlend = 0f)
	{
		if (activeSettings == null)
		{
			return new SkyState();
		}
		float timeInTwentyFourHour = _timePercentage * 24f;
		_isDay = IsDay(timeInTwentyFourHour);
		SetLights(_isDay);
		SkyState skyState = EvaluateSky(new SkyState(), activeSettings, neighbourSettings, blend, timeInTwentyFourHour, _timePercentage);
		if (overrideSkySettings != null)
		{
			SkyState to = EvaluateSky(new SkyState(), overrideSkySettings, null, 0f, _timePercentage * 24f, _timePercentage);
			skyState = BlendSky(skyState, to, overrideBlend);
		}
		UpdateSky(skyState);
		return skyState;
	}

	private SkyState EvaluateSky(SkyState state, SkySettings activeSettings, SkySettings neighbourSettings, float blend, float timeInTwentyFourHour, float timePercentage)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Unknown result type (might be due to invalid IL or missing references)
		//IL_0356: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0374: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		//IL_038d: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0392: Unknown result type (might be due to invalid IL or missing references)
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0405: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_040a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0414: Unknown result type (might be due to invalid IL or missing references)
		//IL_0432: Unknown result type (might be due to invalid IL or missing references)
		//IL_043f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0444: Unknown result type (might be due to invalid IL or missing references)
		//IL_0451: Unknown result type (might be due to invalid IL or missing references)
		//IL_0449: Unknown result type (might be due to invalid IL or missing references)
		//IL_0456: Unknown result type (might be due to invalid IL or missing references)
		//IL_0460: Unknown result type (might be due to invalid IL or missing references)
		//IL_047e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0492: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0510: Unknown result type (might be due to invalid IL or missing references)
		//IL_0512: Unknown result type (might be due to invalid IL or missing references)
		//IL_0516: Unknown result type (might be due to invalid IL or missing references)
		//IL_051b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0521: Unknown result type (might be due to invalid IL or missing references)
		//IL_0523: Unknown result type (might be due to invalid IL or missing references)
		//IL_0527: Unknown result type (might be due to invalid IL or missing references)
		//IL_052c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0532: Unknown result type (might be due to invalid IL or missing references)
		//IL_0534: Unknown result type (might be due to invalid IL or missing references)
		//IL_0538: Unknown result type (might be due to invalid IL or missing references)
		//IL_053d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0543: Unknown result type (might be due to invalid IL or missing references)
		//IL_0545: Unknown result type (might be due to invalid IL or missing references)
		//IL_0549: Unknown result type (might be due to invalid IL or missing references)
		//IL_054e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0554: Unknown result type (might be due to invalid IL or missing references)
		//IL_0556: Unknown result type (might be due to invalid IL or missing references)
		//IL_055a: Unknown result type (might be due to invalid IL or missing references)
		//IL_055f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0565: Unknown result type (might be due to invalid IL or missing references)
		//IL_0567: Unknown result type (might be due to invalid IL or missing references)
		//IL_056b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0570: Unknown result type (might be due to invalid IL or missing references)
		//IL_0576: Unknown result type (might be due to invalid IL or missing references)
		//IL_0578: Unknown result type (might be due to invalid IL or missing references)
		//IL_057c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0581: Unknown result type (might be due to invalid IL or missing references)
		//IL_0598: Unknown result type (might be due to invalid IL or missing references)
		//IL_059a: Unknown result type (might be due to invalid IL or missing references)
		//IL_059e: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a3: Unknown result type (might be due to invalid IL or missing references)
		bool num = neighbourSettings != null;
		bool flag = IsDay(timeInTwentyFourHour);
		float r = activeSettings.SunIntensityGradient.Evaluate(timePercentage).r;
		float num2 = (num ? neighbourSettings.SunIntensityGradient.Evaluate(timePercentage).r : 0f);
		state.SunIntensity = Mathf.Lerp(r, num2, blend);
		float r2 = activeSettings.MoonIntensityGradient.Evaluate(timePercentage).r;
		float num3 = (num ? neighbourSettings.MoonIntensityGradient.Evaluate(timePercentage).r : 0f);
		state.MoonIntensity = Mathf.Lerp(r2, num3, blend);
		state.SunShadowStrength = (flag ? 0.95f : 0f);
		state.MoonShadowStrength = ((!flag) ? 0.8f : 0f);
		if (MathUtility.BetweenValues(timeInTwentyFourHour, _dayNightPhaseTimes.MinDawnHour, _dayNightPhaseTimes.MaxDawnHour))
		{
			float num4 = Mathf.InverseLerp((float)_dayNightPhaseTimes.SunRiseHour, (float)_dayNightPhaseTimes.MaxDawnHour, timeInTwentyFourHour);
			float num5 = 1f - Mathf.InverseLerp((float)_dayNightPhaseTimes.MinDawnHour, (float)_dayNightPhaseTimes.SunRiseHour, timeInTwentyFourHour);
			num4 = _fadeInCurve.Evaluate(num4);
			num5 = _fadeOutCurve.Evaluate(num5);
			state.SunIntensity *= num4;
			state.MoonIntensity *= num5;
		}
		else if (MathUtility.BetweenValues(timeInTwentyFourHour, _dayNightPhaseTimes.MinDuskHour, _dayNightPhaseTimes.MaxDuskHour))
		{
			float num6 = 1f - Mathf.InverseLerp((float)_dayNightPhaseTimes.MinDuskHour, (float)_dayNightPhaseTimes.SunSetHour, timeInTwentyFourHour);
			float num7 = Mathf.InverseLerp((float)_dayNightPhaseTimes.SunSetHour, (float)_dayNightPhaseTimes.MaxDuskHour, timeInTwentyFourHour);
			num7 = _fadeInCurve.Evaluate(num7);
			num6 = _fadeOutCurve.Evaluate(num6);
			state.MoonIntensity *= num7;
			state.SunIntensity *= num6;
		}
		Color val = activeSettings.SunLightColorGradient.Evaluate(timePercentage);
		Color val2 = (num ? neighbourSettings.SunLightColorGradient.Evaluate(timePercentage) : Color.black);
		Color val3 = activeSettings.SunDiscColorGradient.Evaluate(timePercentage);
		Color val4 = (num ? neighbourSettings.SunDiscColorGradient.Evaluate(timePercentage) : Color.black);
		state.SunLightColor = Color.Lerp(val, val2, blend);
		state.SunColor = Color.Lerp(val3, val4, blend);
		Color val5 = activeSettings.MoonLightColorGradient.Evaluate(timePercentage);
		Color val6 = (num ? neighbourSettings.MoonLightColorGradient.Evaluate(timePercentage) : Color.black);
		Color val7 = activeSettings.MoonDiscColorGradient.Evaluate(timePercentage);
		Color val8 = (num ? neighbourSettings.MoonDiscColorGradient.Evaluate(timePercentage) : Color.black);
		state.MoonLightColor = Color.Lerp(val5, val6, blend);
		state.MoonColor = Color.Lerp(val7, val8, blend);
		Color val9 = activeSettings.SkyUpperGradient.Evaluate(timePercentage);
		Color val10 = (num ? neighbourSettings.SkyUpperGradient.Evaluate(timePercentage) : Color.black);
		Color val11 = activeSettings.SkyMiddleGradient.Evaluate(timePercentage);
		Color val12 = (num ? neighbourSettings.SkyMiddleGradient.Evaluate(timePercentage) : Color.black);
		Color val13 = activeSettings.SkyLowerGradient.Evaluate(timePercentage);
		Color val14 = (num ? neighbourSettings.SkyLowerGradient.Evaluate(timePercentage) : Color.black);
		Color val15 = activeSettings.AmbientSkyGradient.Evaluate(timePercentage);
		Color val16 = (num ? neighbourSettings.AmbientSkyGradient.Evaluate(timePercentage) : Color.black);
		Color val17 = activeSettings.AmbientEquatorGradient.Evaluate(timePercentage);
		Color val18 = (num ? neighbourSettings.AmbientEquatorGradient.Evaluate(timePercentage) : Color.black);
		Color val19 = activeSettings.AmbientGroundGradient.Evaluate(timePercentage);
		Color val20 = (num ? neighbourSettings.AmbientGroundGradient.Evaluate(timePercentage) : Color.black);
		Color val21 = activeSettings.FogColorGradient.Evaluate(timePercentage);
		Color val22 = (num ? neighbourSettings.FogColorGradient.Evaluate(timePercentage) : Color.black);
		float r3 = activeSettings.FogDensityGradient.Evaluate(timePercentage).r;
		float num8 = (num ? neighbourSettings.FogDensityGradient.Evaluate(timePercentage).r : 0f);
		Vector2 fogHeightFade = activeSettings.FogHeightFade;
		Vector2 val23 = (num ? neighbourSettings.FogHeightFade : Vector2.zero);
		float r4 = activeSettings.WindIntensityGradient.Evaluate(timePercentage).r;
		float num9 = (num ? neighbourSettings.WindIntensityGradient.Evaluate(timePercentage).r : 0f);
		float r5 = activeSettings.SunSizeGradient.Evaluate(timePercentage).r;
		float num10 = (num ? neighbourSettings.SunSizeGradient.Evaluate(timePercentage).r : 0f);
		float r6 = activeSettings.MoonSizeGradient.Evaluate(timePercentage).r;
		float num11 = (num ? neighbourSettings.MoonSizeGradient.Evaluate(timePercentage).r : 0f);
		state.SunSize = Mathf.Lerp(r5, num10, blend);
		state.MoonSize = Mathf.Lerp(r6, num11, blend);
		state.SkyUpperColor = Color.Lerp(val9, val10, blend);
		state.SkyMiddleColor = Color.Lerp(val11, val12, blend);
		state.SkyLowerColor = Color.Lerp(val13, val14, blend);
		state.AmbientSkyColor = Color.Lerp(val15, val16, blend);
		state.AmbientEquatorColor = Color.Lerp(val17, val18, blend);
		state.AmbientGroundColor = Color.Lerp(val19, val20, blend);
		state.FogColor = Color.Lerp(val21, val22, blend);
		state.FogDensity = Mathf.Lerp(r3, num8, blend);
		state.FogHeightFade = Vector2.Lerp(fogHeightFade, val23, blend);
		state.WindIntensity = Mathf.Lerp(r4, num9, blend);
		return state;
	}

	private SkyState BlendSky(SkyState from, SkyState to, float blend)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		from.SkyLowerColor = Color.Lerp(from.SkyLowerColor, to.SkyLowerColor, blend);
		from.SkyMiddleColor = Color.Lerp(from.SkyMiddleColor, to.SkyMiddleColor, blend);
		from.SkyUpperColor = Color.Lerp(from.SkyUpperColor, to.SkyUpperColor, blend);
		from.FogDensity = Mathf.Lerp(from.FogDensity, to.FogDensity, blend);
		from.FogColor = Color.Lerp(from.FogColor, to.FogColor, blend);
		from.SunColor = Color.Lerp(from.SunColor, to.SunColor, blend);
		from.SunLightColor = Color.Lerp(from.SunLightColor, to.SunLightColor, blend);
		from.SunIntensity = Mathf.Lerp(from.SunIntensity, to.SunIntensity, blend);
		from.SunSize = Mathf.Lerp(from.SunSize, to.SunSize, blend);
		from.SunShadowStrength = Mathf.Lerp(from.SunShadowStrength, to.SunShadowStrength, blend);
		from.MoonColor = Color.Lerp(from.MoonColor, to.MoonColor, blend);
		from.MoonLightColor = Color.Lerp(from.MoonLightColor, to.MoonLightColor, blend);
		from.MoonIntensity = Mathf.Lerp(from.MoonIntensity, to.MoonIntensity, blend);
		from.MoonSize = Mathf.Lerp(from.MoonSize, to.MoonSize, blend);
		from.MoonShadowStrength = Mathf.Lerp(from.MoonShadowStrength, to.MoonShadowStrength, blend);
		from.FogHeightFade = Vector2.Lerp(from.FogHeightFade, to.FogHeightFade, blend);
		from.AmbientSkyColor = Color.Lerp(from.AmbientSkyColor, to.AmbientSkyColor, blend);
		from.AmbientEquatorColor = Color.Lerp(from.AmbientEquatorColor, to.AmbientEquatorColor, blend);
		from.AmbientGroundColor = Color.Lerp(from.AmbientGroundColor, to.AmbientGroundColor, blend);
		return from;
	}

	public float EvaluateFloatByTimeOfDay(DynamicGradient gradient)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return gradient.Evaluate(_timePercentage).r;
	}

	public Color EvaluateColorByTimeOfDay(DynamicGradient gradient)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return gradient.Evaluate(_timePercentage);
	}

	private void UpdateSky(SkyState skyState)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		_sunLight.color = skyState.SunLightColor;
		_moonLight.color = skyState.MoonLightColor;
		_sunLight.intensity = skyState.SunIntensity * 4f;
		_moonLight.intensity = skyState.MoonIntensity * 4f;
		_sunLight.shadowStrength = skyState.SunShadowStrength;
		_moonLight.shadowStrength = skyState.MoonShadowStrength;
	}

	private void SetLights(bool isDay)
	{
		if (((Behaviour)_moonLight).enabled != !isDay)
		{
			((Behaviour)_moonLight).enabled = !isDay;
		}
		if (((Behaviour)_sunLight).enabled != isDay)
		{
			((Behaviour)_sunLight).enabled = isDay;
		}
	}

	private void UpdateRotation()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		((Component)_sunLight).transform.localRotation = Quaternion.Slerp(((Component)_sunLight).transform.localRotation, _currentSunRotation, Time.deltaTime * _debugRotationSpeed);
		((Component)_moonLight).transform.localRotation = Quaternion.Slerp(((Component)_moonLight).transform.localRotation, _currentMoonRotation, Time.deltaTime * _debugRotationSpeed);
		Shader.SetGlobalVector("_SunDirection", Vector4.op_Implicit(((Component)_sunLight).transform.forward));
		Shader.SetGlobalVector("_MoonDirection", Vector4.op_Implicit(((Component)_moonLight).transform.forward));
	}

	private void SnapRotation()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		((Component)_sunLight).transform.localRotation = _currentSunRotation;
		((Component)_moonLight).transform.localRotation = _currentMoonRotation;
		Shader.SetGlobalVector("_SunDirection", Vector4.op_Implicit(((Component)_sunLight).transform.forward));
		Shader.SetGlobalVector("_MoonDirection", Vector4.op_Implicit(((Component)_moonLight).transform.forward));
	}

	public void SetRotation()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		float num = _timePercentage * 360f;
		_currentSunRotation = Quaternion.Euler(num - 90f, 0f, 0f);
		_currentMoonRotation = Quaternion.Euler(num + 90f, 0f, 0f);
	}

	public void UpdateTime(float normalisedTime)
	{
		if (!EnableDebugTimeControl)
		{
			_timePercentage = normalisedTime;
			_timeInHours = _timePercentage * 24f;
			Shader.SetGlobalFloat("_TimeOfDay", _timePercentage);
		}
	}

	public void OnTick()
	{
		SetRotation();
	}

	public void OnTimeSet(float normalisedTime)
	{
		if (!EnableDebugTimeControl)
		{
			_timePercentage = normalisedTime;
			_timeInHours = _timePercentage * 24f;
			Shader.SetGlobalFloat("_TimeOfDay", _timePercentage);
			SetRotation();
			SnapRotation();
		}
	}

	private bool IsDay(float timeInTwentyFourHour)
	{
		if (timeInTwentyFourHour > (float)_dayNightPhaseTimes.SunRiseHour)
		{
			return timeInTwentyFourHour < (float)_dayNightPhaseTimes.SunSetHour;
		}
		return false;
	}
}
