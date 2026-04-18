using UnityEngine.Rendering;

namespace UnityEngine.PostProcessing;

public sealed class FogComponent : PostProcessingComponentCommandBuffer<FogModel>
{
	private static class Uniforms
	{
		internal static readonly int _FogColor = Shader.PropertyToID("_FogColor");

		internal static readonly int _Density = Shader.PropertyToID("_Density");

		internal static readonly int _Start = Shader.PropertyToID("_Start");

		internal static readonly int _End = Shader.PropertyToID("_End");

		internal static readonly int _TempRT = Shader.PropertyToID("_TempRT");
	}

	private const string k_ShaderString = "Hidden/Post FX/Fog";

	public override bool active
	{
		get
		{
			if (base.model.enabled && context.isGBufferAvailable && RenderSettings.fog)
			{
				return !context.interrupted;
			}
			return false;
		}
	}

	public override string GetName()
	{
		return "Fog";
	}

	public override DepthTextureMode GetCameraFlags()
	{
		return (DepthTextureMode)1;
	}

	public override CameraEvent GetCameraEvent()
	{
		return (CameraEvent)13;
	}

	public override void PopulateCommandBuffer(CommandBuffer cb)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Expected I4, but got Unknown
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		FogModel.Settings settings = base.model.settings;
		Material val = context.materialFactory.Get("Hidden/Post FX/Fog");
		val.shaderKeywords = null;
		Color val2;
		if (!GraphicsUtils.isLinearColorSpace)
		{
			val2 = RenderSettings.fogColor;
		}
		else
		{
			Color fogColor = RenderSettings.fogColor;
			val2 = ((Color)(ref fogColor)).linear;
		}
		Color val3 = val2;
		val.SetColor(Uniforms._FogColor, val3);
		val.SetFloat(Uniforms._Density, RenderSettings.fogDensity);
		val.SetFloat(Uniforms._Start, RenderSettings.fogStartDistance);
		val.SetFloat(Uniforms._End, RenderSettings.fogEndDistance);
		FogMode fogMode = RenderSettings.fogMode;
		switch (fogMode - 1)
		{
		case 0:
			val.EnableKeyword("FOG_LINEAR");
			break;
		case 1:
			val.EnableKeyword("FOG_EXP");
			break;
		case 2:
			val.EnableKeyword("FOG_EXP2");
			break;
		}
		RenderTextureFormat val4 = (RenderTextureFormat)(context.isHdr ? 9 : 7);
		cb.GetTemporaryRT(Uniforms._TempRT, context.width, context.height, 24, (FilterMode)1, val4);
		cb.Blit(RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)2), RenderTargetIdentifier.op_Implicit(Uniforms._TempRT));
		cb.Blit(RenderTargetIdentifier.op_Implicit(Uniforms._TempRT), RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)2), val, settings.excludeSkybox ? 1 : 0);
		cb.ReleaseTemporaryRT(Uniforms._TempRT);
	}
}
