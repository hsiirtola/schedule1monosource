using UnityEngine.Rendering;

namespace UnityEngine.PostProcessing;

public sealed class AmbientOcclusionComponent : PostProcessingComponentCommandBuffer<AmbientOcclusionModel>
{
	private static class Uniforms
	{
		internal static readonly int _Intensity = Shader.PropertyToID("_Intensity");

		internal static readonly int _Radius = Shader.PropertyToID("_Radius");

		internal static readonly int _FogParams = Shader.PropertyToID("_FogParams");

		internal static readonly int _Downsample = Shader.PropertyToID("_Downsample");

		internal static readonly int _SampleCount = Shader.PropertyToID("_SampleCount");

		internal static readonly int _OcclusionTexture1 = Shader.PropertyToID("_OcclusionTexture1");

		internal static readonly int _OcclusionTexture2 = Shader.PropertyToID("_OcclusionTexture2");

		internal static readonly int _OcclusionTexture = Shader.PropertyToID("_OcclusionTexture");

		internal static readonly int _MainTex = Shader.PropertyToID("_MainTex");

		internal static readonly int _TempRT = Shader.PropertyToID("_TempRT");
	}

	private enum OcclusionSource
	{
		DepthTexture,
		DepthNormalsTexture,
		GBuffer
	}

	private const string k_BlitShaderString = "Hidden/Post FX/Blit";

	private const string k_ShaderString = "Hidden/Post FX/Ambient Occlusion";

	private readonly RenderTargetIdentifier[] m_MRT = (RenderTargetIdentifier[])(object)new RenderTargetIdentifier[2]
	{
		RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)10),
		RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)2)
	};

	private OcclusionSource occlusionSource
	{
		get
		{
			if (context.isGBufferAvailable && !base.model.settings.forceForwardCompatibility)
			{
				return OcclusionSource.GBuffer;
			}
			if (base.model.settings.highPrecision && (!context.isGBufferAvailable || base.model.settings.forceForwardCompatibility))
			{
				return OcclusionSource.DepthTexture;
			}
			return OcclusionSource.DepthNormalsTexture;
		}
	}

	private bool ambientOnlySupported
	{
		get
		{
			if (context.isHdr && base.model.settings.ambientOnly && context.isGBufferAvailable)
			{
				return !base.model.settings.forceForwardCompatibility;
			}
			return false;
		}
	}

	public override bool active
	{
		get
		{
			if (base.model.enabled && base.model.settings.intensity > 0f)
			{
				return !context.interrupted;
			}
			return false;
		}
	}

	public override DepthTextureMode GetCameraFlags()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		DepthTextureMode val = (DepthTextureMode)0;
		if (occlusionSource == OcclusionSource.DepthTexture)
		{
			val = (DepthTextureMode)(val | 1);
		}
		if (occlusionSource != OcclusionSource.GBuffer)
		{
			val = (DepthTextureMode)(val | 2);
		}
		return val;
	}

	public override string GetName()
	{
		return "Ambient Occlusion";
	}

	public override CameraEvent GetCameraEvent()
	{
		if (ambientOnlySupported && !context.profile.debugViews.IsModeActive(BuiltinDebugViewsModel.Mode.AmbientOcclusion))
		{
			return (CameraEvent)21;
		}
		return (CameraEvent)12;
	}

	public override void PopulateCommandBuffer(CommandBuffer cb)
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Expected I4, but got Unknown
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		AmbientOcclusionModel.Settings settings = base.model.settings;
		Material val = context.materialFactory.Get("Hidden/Post FX/Blit");
		Material val2 = context.materialFactory.Get("Hidden/Post FX/Ambient Occlusion");
		val2.shaderKeywords = null;
		val2.SetFloat(Uniforms._Intensity, settings.intensity);
		val2.SetFloat(Uniforms._Radius, settings.radius);
		val2.SetFloat(Uniforms._Downsample, settings.downsampling ? 0.5f : 1f);
		val2.SetInt(Uniforms._SampleCount, (int)settings.sampleCount);
		if (!context.isGBufferAvailable && RenderSettings.fog)
		{
			val2.SetVector(Uniforms._FogParams, Vector4.op_Implicit(new Vector3(RenderSettings.fogDensity, RenderSettings.fogStartDistance, RenderSettings.fogEndDistance)));
			FogMode fogMode = RenderSettings.fogMode;
			switch (fogMode - 1)
			{
			case 0:
				val2.EnableKeyword("FOG_LINEAR");
				break;
			case 1:
				val2.EnableKeyword("FOG_EXP");
				break;
			case 2:
				val2.EnableKeyword("FOG_EXP2");
				break;
			}
		}
		else
		{
			val2.EnableKeyword("FOG_OFF");
		}
		int width = context.width;
		int height = context.height;
		int num = ((!settings.downsampling) ? 1 : 2);
		int occlusionTexture = Uniforms._OcclusionTexture1;
		cb.GetTemporaryRT(occlusionTexture, width / num, height / num, 0, (FilterMode)1, (RenderTextureFormat)0, (RenderTextureReadWrite)1);
		cb.Blit((Texture)null, RenderTargetIdentifier.op_Implicit(occlusionTexture), val2, (int)occlusionSource);
		int occlusionTexture2 = Uniforms._OcclusionTexture2;
		cb.GetTemporaryRT(occlusionTexture2, width, height, 0, (FilterMode)1, (RenderTextureFormat)0, (RenderTextureReadWrite)1);
		cb.SetGlobalTexture(Uniforms._MainTex, RenderTargetIdentifier.op_Implicit(occlusionTexture));
		cb.Blit(RenderTargetIdentifier.op_Implicit(occlusionTexture), RenderTargetIdentifier.op_Implicit(occlusionTexture2), val2, (occlusionSource == OcclusionSource.GBuffer) ? 4 : 3);
		cb.ReleaseTemporaryRT(occlusionTexture);
		occlusionTexture = Uniforms._OcclusionTexture;
		cb.GetTemporaryRT(occlusionTexture, width, height, 0, (FilterMode)1, (RenderTextureFormat)0, (RenderTextureReadWrite)1);
		cb.SetGlobalTexture(Uniforms._MainTex, RenderTargetIdentifier.op_Implicit(occlusionTexture2));
		cb.Blit(RenderTargetIdentifier.op_Implicit(occlusionTexture2), RenderTargetIdentifier.op_Implicit(occlusionTexture), val2, 5);
		cb.ReleaseTemporaryRT(occlusionTexture2);
		if (context.profile.debugViews.IsModeActive(BuiltinDebugViewsModel.Mode.AmbientOcclusion))
		{
			cb.SetGlobalTexture(Uniforms._MainTex, RenderTargetIdentifier.op_Implicit(occlusionTexture));
			cb.Blit(RenderTargetIdentifier.op_Implicit(occlusionTexture), RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)2), val2, 8);
			context.Interrupt();
		}
		else if (ambientOnlySupported)
		{
			cb.SetRenderTarget(m_MRT, RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)2));
			cb.DrawMesh(GraphicsUtils.quad, Matrix4x4.identity, val2, 0, 7);
		}
		else
		{
			RenderTextureFormat val3 = (RenderTextureFormat)(context.isHdr ? 9 : 7);
			int tempRT = Uniforms._TempRT;
			cb.GetTemporaryRT(tempRT, context.width, context.height, 0, (FilterMode)1, val3);
			cb.Blit(RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)2), RenderTargetIdentifier.op_Implicit(tempRT), val, 0);
			cb.SetGlobalTexture(Uniforms._MainTex, RenderTargetIdentifier.op_Implicit(tempRT));
			cb.Blit(RenderTargetIdentifier.op_Implicit(tempRT), RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)2), val2, 6);
			cb.ReleaseTemporaryRT(tempRT);
		}
		cb.ReleaseTemporaryRT(occlusionTexture);
	}
}
