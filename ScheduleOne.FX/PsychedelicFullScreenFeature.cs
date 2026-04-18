using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ScheduleOne.FX;

public class PsychedelicFullScreenFeature : ScriptableRendererFeature
{
	[Serializable]
	public class Settings
	{
		public string profilerTag = "Custom Full Screen Pass";

		public RenderPassEvent renderPassEvent = (RenderPassEvent)550;

		public Material passMaterial;

		[Header("Active Properties")]
		public MaterialProperties ActiveProperties;

		[Header("Presets")]
		public List<MaterialPropertyPreset> MaterialPresets;
	}

	[Serializable]
	public class MaterialPropertyPreset
	{
		public string Name;

		public PsychedelicFullScreenData Data;
	}

	[Serializable]
	public class MaterialProperties
	{
		public float NoiseScale = 15f;

		public float Blend = 0.016f;

		public Vector2 PanSpeed = new Vector2(0.05f, 0.05f);

		public bool DoesBounce;

		public float Amplitude = 0.19f;

		public MaterialProperties Clone()
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			return new MaterialProperties
			{
				NoiseScale = NoiseScale,
				Blend = Blend,
				PanSpeed = PanSpeed,
				DoesBounce = DoesBounce,
				Amplitude = Amplitude
			};
		}
	}

	[Header("Settings")]
	[SerializeField]
	private Settings _settings = new Settings();

	private static readonly int BLEND_ID = Shader.PropertyToID("_Blend");

	private static readonly int NOISE_SCALE_ID = Shader.PropertyToID("_NoiseScale");

	private static readonly int PAN_SPEED_ID = Shader.PropertyToID("_PanSpeed");

	private static readonly int DOES_BOUNCE_ID = Shader.PropertyToID("_DoesBounce");

	private static readonly int AMPLITUDE_ID = Shader.PropertyToID("_Amplitude");

	private PsychedelicFullScreenPass _psychedelicPass;

	public Settings FeatureSettings => _settings;

	public MaterialProperties ActiveMaterialProperties => _settings.ActiveProperties;

	public override void Create()
	{
		_psychedelicPass = new PsychedelicFullScreenPass(_settings);
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_settings.passMaterial == (Object)null)
		{
			Debug.LogWarning((object)"CustomFullScreenFeature: Missing Material. Pass will not be added.");
			return;
		}
		((ScriptableRenderPass)_psychedelicPass).renderPassEvent = _settings.renderPassEvent;
		renderer.EnqueuePass((ScriptableRenderPass)(object)_psychedelicPass);
	}

	public void SetActiveMaterialProperties(MaterialProperties properties)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		_settings.ActiveProperties.Blend = properties.Blend;
		_settings.ActiveProperties.NoiseScale = properties.NoiseScale;
		_settings.ActiveProperties.PanSpeed = properties.PanSpeed;
		_settings.ActiveProperties.DoesBounce = properties.DoesBounce;
		_settings.ActiveProperties.Amplitude = properties.Amplitude;
	}

	public void PrintMaterialValue()
	{
		float num = _settings.passMaterial.GetFloat("_Blend");
		Debug.Log((object)$"Psychedelic material Blend Value: {num}");
	}

	public PsychedelicFullScreenData GetMaterialPreset(string presetName)
	{
		foreach (MaterialPropertyPreset materialPreset in _settings.MaterialPresets)
		{
			if (materialPreset.Name == presetName)
			{
				return materialPreset.Data;
			}
		}
		return null;
	}
}
