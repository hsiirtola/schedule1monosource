using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ScheduleOne.Weather;

public class MaskController : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private ComputeShader _wetMaskShader;

	[SerializeField]
	private ComputeShader _maskDownsampleShader;

	[SerializeField]
	private RenderTexture _wetMaskTexture;

	[Header("General Settings")]
	[SerializeField]
	private int _worldSize = 512;

	[Header("Wet Mask Settings")]
	[SerializeField]
	private int _wetMaskResolution = 512;

	[SerializeField]
	private float _wetGrowthRate = 0.5f;

	[SerializeField]
	private float _wetDecayRate = 0.5f;

	[SerializeField]
	private float _sunEvapMultiplier = 0.5f;

	[SerializeField]
	private AnimationCurve _wetnessGrowthCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[Header("Height Settings")]
	[SerializeField]
	private Texture2D _heightMask;

	[SerializeField]
	private int _downsampledResolution = 256;

	[SerializeField]
	private Vector2 _minMaxHeight = new Vector2(-6.5f, 80f);

	[Header("Debugging & Development")]
	[SerializeField]
	private RenderTexture _debugTexture;

	private Vector2[] _weatherVolumeOrigins;

	private float[] _weatherRainValues;

	private float[] _weatherSunValues;

	private ComputeBuffer _volumeOriginsBuffer;

	private ComputeBuffer _volumeRainBuffer;

	private ComputeBuffer _volumeSunBuffer;

	private Coroutine _heightConversionCo;

	private float[] _heightMap;

	public float WorldSize => _worldSize;

	public int HeightMapResolution => _downsampledResolution;

	public float[] HeightMap => _heightMap;

	public Vector2 MinMaxHeight => _minMaxHeight;

	public void Initialise(int weatherVolumeCount, float blendAmount, Vector3 weatherVolumeSize)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Expected O, but got Unknown
		_weatherVolumeOrigins = (Vector2[])(object)new Vector2[weatherVolumeCount];
		_weatherRainValues = new float[weatherVolumeCount];
		_weatherSunValues = new float[weatherVolumeCount];
		_volumeOriginsBuffer = new ComputeBuffer(weatherVolumeCount, 8);
		_volumeRainBuffer = new ComputeBuffer(weatherVolumeCount, 4);
		_volumeSunBuffer = new ComputeBuffer(weatherVolumeCount, 4);
		_wetMaskShader.SetInt("_Resolution", _wetMaskResolution);
		_wetMaskShader.SetFloat("_BlendAmount", blendAmount);
		_wetMaskShader.SetVector("_WorldSize", Vector4.op_Implicit(new Vector2((float)_worldSize, (float)_worldSize)));
		_wetMaskShader.SetVector("_WorldOrigin", Vector4.op_Implicit(new Vector2(((Component)this).transform.position.x, ((Component)this).transform.position.z)));
		_wetMaskShader.SetVector("_WeatherVolumeSize", Vector4.op_Implicit(new Vector2(weatherVolumeSize.x, weatherVolumeSize.z)));
		_wetMaskShader.SetTexture(0, "Result", (Texture)(object)_wetMaskTexture);
		_heightMap = new float[_downsampledResolution * _downsampledResolution];
		_debugTexture = new RenderTexture(_downsampledResolution, _downsampledResolution, 0, (RenderTextureFormat)14);
		_debugTexture.enableRandomWrite = true;
		_debugTexture.Create();
		_maskDownsampleShader.SetInt("_NewResolution", _downsampledResolution);
		_maskDownsampleShader.SetInt("_OriginalResolution", ((Texture)_heightMask).width);
		_maskDownsampleShader.SetTexture(0, "HeightMask", (Texture)(object)_heightMask);
		_maskDownsampleShader.SetTexture(0, "Debug", (Texture)(object)_debugTexture);
	}

	public void RunWetMaskShader(List<WeatherVolume> weatherVolumes)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < weatherVolumes.Count; i++)
		{
			WeatherVolume weatherVolume = weatherVolumes[i];
			WeatherProfile weatherProfile = weatherVolume.WeatherProfile;
			_weatherVolumeOrigins[i] = new Vector2(weatherVolume.Center.x, weatherVolume.Center.z);
			_weatherRainValues[i] = _wetnessGrowthCurve.Evaluate(weatherProfile.Conditions.Rainy);
			_weatherSunValues[i] = weatherProfile.Conditions.Sunny;
		}
		_volumeOriginsBuffer.SetData((Array)_weatherVolumeOrigins);
		_volumeRainBuffer.SetData((Array)_weatherRainValues);
		_volumeSunBuffer.SetData((Array)_weatherSunValues);
		_wetMaskShader.SetFloat("_GrowthRate", _wetGrowthRate);
		_wetMaskShader.SetFloat("_DecayRate", _wetDecayRate);
		_wetMaskShader.SetFloat("_SunEvapMultiplier", _sunEvapMultiplier);
		_wetMaskShader.SetFloat("_DeltaTime", Time.deltaTime);
		_wetMaskShader.SetBuffer(0, "VolumeRain", _volumeRainBuffer);
		_wetMaskShader.SetBuffer(0, "VolumeSun", _volumeSunBuffer);
		_wetMaskShader.SetBuffer(0, "VolumeOrigins", _volumeOriginsBuffer);
		_wetMaskShader.Dispatch(0, _wetMaskResolution / 8, _wetMaskResolution / 8, 1);
	}

	public void ConvertHeightToArray()
	{
		if (_heightConversionCo != null)
		{
			((MonoBehaviour)this).StopCoroutine(_heightConversionCo);
		}
		_heightConversionCo = ((MonoBehaviour)this).StartCoroutine(DoHeightConversionRoutine());
	}

	private IEnumerator DoHeightConversionRoutine()
	{
		ComputeBuffer heightBuffer = new ComputeBuffer(((Texture)_heightMask).width * ((Texture)_heightMask).height, 4);
		_maskDownsampleShader.SetBuffer(0, "HeightBuffer", heightBuffer);
		_maskDownsampleShader.Dispatch(0, _downsampledResolution / 8, _downsampledResolution / 8, 1);
		AsyncGPUReadbackRequest request = AsyncGPUReadback.Request(heightBuffer, (Action<AsyncGPUReadbackRequest>)null);
		yield return (object)new WaitUntil((Func<bool>)(() => ((AsyncGPUReadbackRequest)(ref request)).done));
		if (((AsyncGPUReadbackRequest)(ref request)).hasError)
		{
			Debug.LogError((object)"GPU readback error while converting heightmap.");
			yield break;
		}
		_heightMap = ((AsyncGPUReadbackRequest)(ref request)).GetData<float>(0).ToArray();
		_heightConversionCo = null;
		heightBuffer.Release();
	}

	private void OnDestroy()
	{
		if ((Object)(object)_wetMaskTexture != (Object)null)
		{
			_wetMaskTexture.Release();
		}
	}
}
