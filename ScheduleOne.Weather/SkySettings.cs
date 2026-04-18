using System;
using UnityEngine;

namespace ScheduleOne.Weather;

[Serializable]
public class SkySettings
{
	[SerializeField]
	private DynamicGradient _skyUpperGradient = new DynamicGradient();

	[SerializeField]
	private DynamicGradient _skyMiddleGradient = new DynamicGradient();

	[SerializeField]
	private DynamicGradient _skyLowerGradient = new DynamicGradient();

	[SerializeField]
	private DynamicGradient _cloudDensityGradient = new DynamicGradient();

	[SerializeField]
	private DynamicGradient _cloudColorGradient = new DynamicGradient();

	[SerializeField]
	private DynamicGradient _sunLightGradient = new DynamicGradient();

	[SerializeField]
	private DynamicGradient _sunIntensityGradient = new DynamicGradient();

	[SerializeField]
	private DynamicGradient _sunColorGradient = new DynamicGradient();

	[SerializeField]
	private DynamicGradient _sunSizeGradient = new DynamicGradient();

	[SerializeField]
	private DynamicGradient _moonLightGradient = new DynamicGradient();

	[SerializeField]
	private DynamicGradient _moonIntensityGradient = new DynamicGradient();

	[SerializeField]
	private DynamicGradient _moonColorGradient = new DynamicGradient();

	[SerializeField]
	private DynamicGradient _moonSizeGradient = new DynamicGradient();

	[SerializeField]
	private DynamicGradient _ambientSkyGradient = new DynamicGradient();

	[SerializeField]
	private DynamicGradient _ambientEquatorGradient = new DynamicGradient();

	[SerializeField]
	private DynamicGradient _ambientGroundGradient = new DynamicGradient();

	[SerializeField]
	private DynamicGradient _fogColorGradient = new DynamicGradient();

	[SerializeField]
	private DynamicGradient _fogDensityGradient = new DynamicGradient();

	public Vector2 FogHeightFade = new Vector2(300f, 1000f);

	[SerializeField]
	private DynamicGradient _windIntensityGradient = new DynamicGradient();

	public DynamicGradient SkyUpperGradient => _skyUpperGradient;

	public DynamicGradient SkyMiddleGradient => _skyMiddleGradient;

	public DynamicGradient SkyLowerGradient => _skyLowerGradient;

	public DynamicGradient CloudDensityGradient => _cloudDensityGradient;

	public DynamicGradient CloudColorGradient => _cloudColorGradient;

	public DynamicGradient SunLightColorGradient => _sunLightGradient;

	public DynamicGradient SunDiscColorGradient => _sunColorGradient;

	public DynamicGradient MoonLightColorGradient => _moonLightGradient;

	public DynamicGradient MoonDiscColorGradient => _moonColorGradient;

	public DynamicGradient SunIntensityGradient => _sunIntensityGradient;

	public DynamicGradient MoonIntensityGradient => _moonIntensityGradient;

	public DynamicGradient AmbientSkyGradient => _ambientSkyGradient;

	public DynamicGradient AmbientEquatorGradient => _ambientEquatorGradient;

	public DynamicGradient AmbientGroundGradient => _ambientGroundGradient;

	public DynamicGradient FogColorGradient => _fogColorGradient;

	public DynamicGradient FogDensityGradient => _fogDensityGradient;

	public DynamicGradient WindIntensityGradient => _windIntensityGradient;

	public DynamicGradient SunSizeGradient => _sunSizeGradient;

	public DynamicGradient MoonSizeGradient => _moonSizeGradient;

	public void Set(SkySettings settings)
	{
		_skyUpperGradient = settings.SkyUpperGradient;
		_skyMiddleGradient = settings.SkyMiddleGradient;
		_skyLowerGradient = settings.SkyLowerGradient;
		_cloudDensityGradient = settings.CloudDensityGradient;
		_cloudColorGradient = settings.CloudColorGradient;
		_sunLightGradient = settings.SunLightColorGradient;
		_sunColorGradient = settings.SunDiscColorGradient;
		_sunIntensityGradient = settings.SunIntensityGradient;
		_moonLightGradient = settings.MoonLightColorGradient;
		_moonColorGradient = settings.MoonDiscColorGradient;
		_moonIntensityGradient = settings.MoonIntensityGradient;
		_ambientSkyGradient = settings.AmbientSkyGradient;
		_ambientEquatorGradient = settings.AmbientEquatorGradient;
		_ambientGroundGradient = settings.AmbientGroundGradient;
		_fogColorGradient = settings.FogColorGradient;
		_fogDensityGradient = settings.FogDensityGradient;
		_windIntensityGradient = settings.WindIntensityGradient;
		_sunSizeGradient = settings.SunSizeGradient;
		_moonSizeGradient = settings.MoonSizeGradient;
	}
}
