using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class VFXRainFeature : ScriptableRendererFeature
{
	[Serializable]
	public class Settings
	{
		public RenderPassEvent RenderPassEvent = (RenderPassEvent)550;

		public LayerMask LayerMask = LayerMask.op_Implicit(0);
	}

	public Settings _settings = new Settings();

	private VFXRainPass _pass;

	public override void Create()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		_pass = new VFXRainPass();
		((ScriptableRenderPass)_pass).renderPassEvent = _settings.RenderPassEvent;
	}

	public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
	{
		if (_pass != null)
		{
			_pass.Setup(_settings, renderer.cameraColorTargetHandle, renderer.cameraDepthTargetHandle);
		}
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		if (_pass != null)
		{
			renderer.EnqueuePass((ScriptableRenderPass)(object)_pass);
		}
	}

	protected override void Dispose(bool disposing)
	{
		_pass?.Dispose();
	}
}
