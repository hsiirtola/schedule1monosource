using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScheduleOneFogFeature : ScriptableRendererFeature
{
	[Serializable]
	public class Settings
	{
		public RenderPassEvent RenderPassEvent = (RenderPassEvent)550;

		public Shader Shader;

		public Color Color = new Color(0.5f, 0.5f, 0.5f, 1f);

		[Range(0f, 100f)]
		public float Start;

		[Range(0f, 5000f)]
		public float End = 50f;

		[Range(0f, 1f)]
		public float Density = 1f;

		[Range(0f, 10f)]
		public float BlurStrength = 2f;

		public float StartHeightFade = 300f;

		public float EndHeightFade = 1000f;
	}

	public Settings _settings = new Settings();

	private ScheduleOneFogPass _pass;

	private Material _material;

	public override void Create()
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_settings.Shader == (Object)null)
		{
			_settings.Shader = Shader.Find("Hidden/ScheduleOneFog");
		}
		if ((Object)(object)_settings.Shader != (Object)null)
		{
			_material = CoreUtils.CreateEngineMaterial(_settings.Shader);
		}
		if ((Object)(object)_material != (Object)null)
		{
			_pass = new ScheduleOneFogPass(_material);
			((ScriptableRenderPass)_pass).renderPassEvent = _settings.RenderPassEvent;
		}
	}

	public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
	{
		if (_pass != null && !((Object)(object)_material == (Object)null))
		{
			_pass.Setup(_settings, renderer.cameraColorTargetHandle);
		}
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		if (_pass != null && !((Object)(object)_material == (Object)null))
		{
			renderer.EnqueuePass((ScriptableRenderPass)(object)_pass);
		}
	}

	protected override void Dispose(bool disposing)
	{
		_pass?.Dispose();
		CoreUtils.Destroy((Object)(object)_material);
	}
}
