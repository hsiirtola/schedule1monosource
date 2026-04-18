using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ScheduleOne.FX;

public class PsychedelicFullScreenPass : ScriptableRenderPass
{
	private PsychedelicFullScreenFeature.Settings _settings;

	private RTHandle _source;

	private RTHandle _tempTexture;

	private Material _material;

	private static readonly int BLEND_ID = Shader.PropertyToID("_Blend");

	private static readonly int NOISE_SCALE_ID = Shader.PropertyToID("_NoiseScale");

	private static readonly int PAN_SPEED_ID = Shader.PropertyToID("_PanSpeed");

	private static readonly int DOES_BOUNCE_ID = Shader.PropertyToID("_DoesBounce");

	private static readonly int AMPLITUDE_ID = Shader.PropertyToID("_Amplitude");

	public PsychedelicFullScreenPass(PsychedelicFullScreenFeature.Settings settings)
	{
		_settings = settings;
		_material = settings.passMaterial;
	}

	public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		ScriptableRenderer renderer = renderingData.cameraData.renderer;
		_source = renderer.cameraColorTargetHandle;
		RenderTextureDescriptor cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
		((RenderTextureDescriptor)(ref cameraTargetDescriptor)).depthBufferBits = 0;
		RenderingUtils.ReAllocateIfNeeded(ref _tempTexture, ref cameraTargetDescriptor, (FilterMode)1, (TextureWrapMode)1, false, 1, 0f, "_TempEffectTexture");
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_material == (Object)null))
		{
			CommandBuffer val = CommandBufferPool.Get(_settings.profilerTag);
			_material.SetFloat(NOISE_SCALE_ID, _settings.ActiveProperties.NoiseScale);
			_material.SetFloat(BLEND_ID, _settings.ActiveProperties.Blend);
			_material.SetVector(PAN_SPEED_ID, Vector4.op_Implicit(_settings.ActiveProperties.PanSpeed));
			_material.SetInt(DOES_BOUNCE_ID, Convert.ToInt32(_settings.ActiveProperties.DoesBounce));
			_material.SetFloat(AMPLITUDE_ID, _settings.ActiveProperties.Amplitude);
			Blitter.BlitCameraTexture(val, _source, _tempTexture, _material, 0);
			Blitter.BlitCameraTexture(val, _tempTexture, _source, 0f, false);
			((ScriptableRenderContext)(ref context)).ExecuteCommandBuffer(val);
			CommandBufferPool.Release(val);
		}
	}

	public override void OnCameraCleanup(CommandBuffer cmd)
	{
	}
}
