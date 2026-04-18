using System;
using UnityEngine.Rendering;

namespace UnityEngine.PostProcessing;

public sealed class ScreenSpaceReflectionComponent : PostProcessingComponentCommandBuffer<ScreenSpaceReflectionModel>
{
	private static class Uniforms
	{
		internal static readonly int _RayStepSize = Shader.PropertyToID("_RayStepSize");

		internal static readonly int _AdditiveReflection = Shader.PropertyToID("_AdditiveReflection");

		internal static readonly int _BilateralUpsampling = Shader.PropertyToID("_BilateralUpsampling");

		internal static readonly int _TreatBackfaceHitAsMiss = Shader.PropertyToID("_TreatBackfaceHitAsMiss");

		internal static readonly int _AllowBackwardsRays = Shader.PropertyToID("_AllowBackwardsRays");

		internal static readonly int _TraceBehindObjects = Shader.PropertyToID("_TraceBehindObjects");

		internal static readonly int _MaxSteps = Shader.PropertyToID("_MaxSteps");

		internal static readonly int _FullResolutionFiltering = Shader.PropertyToID("_FullResolutionFiltering");

		internal static readonly int _HalfResolution = Shader.PropertyToID("_HalfResolution");

		internal static readonly int _HighlightSuppression = Shader.PropertyToID("_HighlightSuppression");

		internal static readonly int _PixelsPerMeterAtOneMeter = Shader.PropertyToID("_PixelsPerMeterAtOneMeter");

		internal static readonly int _ScreenEdgeFading = Shader.PropertyToID("_ScreenEdgeFading");

		internal static readonly int _ReflectionBlur = Shader.PropertyToID("_ReflectionBlur");

		internal static readonly int _MaxRayTraceDistance = Shader.PropertyToID("_MaxRayTraceDistance");

		internal static readonly int _FadeDistance = Shader.PropertyToID("_FadeDistance");

		internal static readonly int _LayerThickness = Shader.PropertyToID("_LayerThickness");

		internal static readonly int _SSRMultiplier = Shader.PropertyToID("_SSRMultiplier");

		internal static readonly int _FresnelFade = Shader.PropertyToID("_FresnelFade");

		internal static readonly int _FresnelFadePower = Shader.PropertyToID("_FresnelFadePower");

		internal static readonly int _ReflectionBufferSize = Shader.PropertyToID("_ReflectionBufferSize");

		internal static readonly int _ScreenSize = Shader.PropertyToID("_ScreenSize");

		internal static readonly int _InvScreenSize = Shader.PropertyToID("_InvScreenSize");

		internal static readonly int _ProjInfo = Shader.PropertyToID("_ProjInfo");

		internal static readonly int _CameraClipInfo = Shader.PropertyToID("_CameraClipInfo");

		internal static readonly int _ProjectToPixelMatrix = Shader.PropertyToID("_ProjectToPixelMatrix");

		internal static readonly int _WorldToCameraMatrix = Shader.PropertyToID("_WorldToCameraMatrix");

		internal static readonly int _CameraToWorldMatrix = Shader.PropertyToID("_CameraToWorldMatrix");

		internal static readonly int _Axis = Shader.PropertyToID("_Axis");

		internal static readonly int _CurrentMipLevel = Shader.PropertyToID("_CurrentMipLevel");

		internal static readonly int _NormalAndRoughnessTexture = Shader.PropertyToID("_NormalAndRoughnessTexture");

		internal static readonly int _HitPointTexture = Shader.PropertyToID("_HitPointTexture");

		internal static readonly int _BlurTexture = Shader.PropertyToID("_BlurTexture");

		internal static readonly int _FilteredReflections = Shader.PropertyToID("_FilteredReflections");

		internal static readonly int _FinalReflectionTexture = Shader.PropertyToID("_FinalReflectionTexture");

		internal static readonly int _TempTexture = Shader.PropertyToID("_TempTexture");
	}

	private enum PassIndex
	{
		RayTraceStep,
		CompositeFinal,
		Blur,
		CompositeSSR,
		MinMipGeneration,
		HitPointToReflections,
		BilateralKeyPack,
		BlitDepthAsCSZ,
		PoissonBlur
	}

	private bool k_HighlightSuppression;

	private bool k_TraceBehindObjects = true;

	private bool k_TreatBackfaceHitAsMiss;

	private bool k_BilateralUpsample = true;

	private readonly int[] m_ReflectionTextures = new int[5];

	public override bool active
	{
		get
		{
			if (base.model.enabled && context.isGBufferAvailable)
			{
				return !context.interrupted;
			}
			return false;
		}
	}

	public override DepthTextureMode GetCameraFlags()
	{
		return (DepthTextureMode)1;
	}

	public override void OnEnable()
	{
		m_ReflectionTextures[0] = Shader.PropertyToID("_ReflectionTexture0");
		m_ReflectionTextures[1] = Shader.PropertyToID("_ReflectionTexture1");
		m_ReflectionTextures[2] = Shader.PropertyToID("_ReflectionTexture2");
		m_ReflectionTextures[3] = Shader.PropertyToID("_ReflectionTexture3");
		m_ReflectionTextures[4] = Shader.PropertyToID("_ReflectionTexture4");
	}

	public override string GetName()
	{
		return "Screen Space Reflection";
	}

	public override CameraEvent GetCameraEvent()
	{
		return (CameraEvent)9;
	}

	public override void PopulateCommandBuffer(CommandBuffer cb)
	{
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0389: Unknown result type (might be due to invalid IL or missing references)
		//IL_038b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0397: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_040b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0415: Unknown result type (might be due to invalid IL or missing references)
		//IL_0417: Unknown result type (might be due to invalid IL or missing references)
		//IL_0419: Unknown result type (might be due to invalid IL or missing references)
		//IL_041e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_0436: Unknown result type (might be due to invalid IL or missing references)
		//IL_0448: Unknown result type (might be due to invalid IL or missing references)
		//IL_044d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0451: Unknown result type (might be due to invalid IL or missing references)
		//IL_046c: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0509: Unknown result type (might be due to invalid IL or missing references)
		//IL_0512: Unknown result type (might be due to invalid IL or missing references)
		//IL_0519: Unknown result type (might be due to invalid IL or missing references)
		//IL_0528: Unknown result type (might be due to invalid IL or missing references)
		//IL_052f: Unknown result type (might be due to invalid IL or missing references)
		//IL_053e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0545: Unknown result type (might be due to invalid IL or missing references)
		//IL_0555: Unknown result type (might be due to invalid IL or missing references)
		//IL_0562: Unknown result type (might be due to invalid IL or missing references)
		//IL_059c: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_05de: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_060c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0624: Unknown result type (might be due to invalid IL or missing references)
		//IL_062b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0657: Unknown result type (might be due to invalid IL or missing references)
		//IL_065e: Unknown result type (might be due to invalid IL or missing references)
		//IL_067c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0685: Unknown result type (might be due to invalid IL or missing references)
		//IL_068c: Unknown result type (might be due to invalid IL or missing references)
		//IL_069c: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a2: Unknown result type (might be due to invalid IL or missing references)
		ScreenSpaceReflectionModel.Settings settings = base.model.settings;
		Camera camera = context.camera;
		int num = ((settings.reflection.reflectionQuality == ScreenSpaceReflectionModel.SSRResolution.High) ? 1 : 2);
		int num2 = context.width / num;
		int num3 = context.height / num;
		float num4 = context.width;
		float num5 = context.height;
		float num6 = num4 / 2f;
		float num7 = num5 / 2f;
		Material val = context.materialFactory.Get("Hidden/Post FX/Screen Space Reflection");
		val.SetInt(Uniforms._RayStepSize, settings.reflection.stepSize);
		val.SetInt(Uniforms._AdditiveReflection, (settings.reflection.blendType == ScreenSpaceReflectionModel.SSRReflectionBlendType.Additive) ? 1 : 0);
		val.SetInt(Uniforms._BilateralUpsampling, k_BilateralUpsample ? 1 : 0);
		val.SetInt(Uniforms._TreatBackfaceHitAsMiss, k_TreatBackfaceHitAsMiss ? 1 : 0);
		val.SetInt(Uniforms._AllowBackwardsRays, settings.reflection.reflectBackfaces ? 1 : 0);
		val.SetInt(Uniforms._TraceBehindObjects, k_TraceBehindObjects ? 1 : 0);
		val.SetInt(Uniforms._MaxSteps, settings.reflection.iterationCount);
		val.SetInt(Uniforms._FullResolutionFiltering, 0);
		val.SetInt(Uniforms._HalfResolution, (settings.reflection.reflectionQuality != ScreenSpaceReflectionModel.SSRResolution.High) ? 1 : 0);
		val.SetInt(Uniforms._HighlightSuppression, k_HighlightSuppression ? 1 : 0);
		float num8 = num4 / (-2f * Mathf.Tan(camera.fieldOfView / 180f * (float)Math.PI * 0.5f));
		val.SetFloat(Uniforms._PixelsPerMeterAtOneMeter, num8);
		val.SetFloat(Uniforms._ScreenEdgeFading, settings.screenEdgeMask.intensity);
		val.SetFloat(Uniforms._ReflectionBlur, settings.reflection.reflectionBlur);
		val.SetFloat(Uniforms._MaxRayTraceDistance, settings.reflection.maxDistance);
		val.SetFloat(Uniforms._FadeDistance, settings.intensity.fadeDistance);
		val.SetFloat(Uniforms._LayerThickness, settings.reflection.widthModifier);
		val.SetFloat(Uniforms._SSRMultiplier, settings.intensity.reflectionMultiplier);
		val.SetFloat(Uniforms._FresnelFade, settings.intensity.fresnelFade);
		val.SetFloat(Uniforms._FresnelFadePower, settings.intensity.fresnelFadePower);
		Matrix4x4 projectionMatrix = camera.projectionMatrix;
		Vector4 val2 = default(Vector4);
		((Vector4)(ref val2))._002Ector(-2f / (num4 * ((Matrix4x4)(ref projectionMatrix))[0]), -2f / (num5 * ((Matrix4x4)(ref projectionMatrix))[5]), (1f - ((Matrix4x4)(ref projectionMatrix))[2]) / ((Matrix4x4)(ref projectionMatrix))[0], (1f + ((Matrix4x4)(ref projectionMatrix))[6]) / ((Matrix4x4)(ref projectionMatrix))[5]);
		Vector3 val3 = (float.IsPositiveInfinity(camera.farClipPlane) ? new Vector3(camera.nearClipPlane, -1f, 1f) : new Vector3(camera.nearClipPlane * camera.farClipPlane, camera.nearClipPlane - camera.farClipPlane, camera.farClipPlane));
		val.SetVector(Uniforms._ReflectionBufferSize, Vector4.op_Implicit(new Vector2((float)num2, (float)num3)));
		val.SetVector(Uniforms._ScreenSize, Vector4.op_Implicit(new Vector2(num4, num5)));
		val.SetVector(Uniforms._InvScreenSize, Vector4.op_Implicit(new Vector2(1f / num4, 1f / num5)));
		val.SetVector(Uniforms._ProjInfo, val2);
		val.SetVector(Uniforms._CameraClipInfo, Vector4.op_Implicit(val3));
		Matrix4x4 val4 = default(Matrix4x4);
		((Matrix4x4)(ref val4)).SetRow(0, new Vector4(num6, 0f, 0f, num6));
		((Matrix4x4)(ref val4)).SetRow(1, new Vector4(0f, num7, 0f, num7));
		((Matrix4x4)(ref val4)).SetRow(2, new Vector4(0f, 0f, 1f, 0f));
		((Matrix4x4)(ref val4)).SetRow(3, new Vector4(0f, 0f, 0f, 1f));
		Matrix4x4 val5 = val4 * projectionMatrix;
		val.SetMatrix(Uniforms._ProjectToPixelMatrix, val5);
		val.SetMatrix(Uniforms._WorldToCameraMatrix, camera.worldToCameraMatrix);
		int cameraToWorldMatrix = Uniforms._CameraToWorldMatrix;
		Matrix4x4 worldToCameraMatrix = camera.worldToCameraMatrix;
		val.SetMatrix(cameraToWorldMatrix, ((Matrix4x4)(ref worldToCameraMatrix)).inverse);
		RenderTextureFormat val6 = (RenderTextureFormat)(context.isHdr ? 2 : 0);
		int normalAndRoughnessTexture = Uniforms._NormalAndRoughnessTexture;
		int hitPointTexture = Uniforms._HitPointTexture;
		int blurTexture = Uniforms._BlurTexture;
		int filteredReflections = Uniforms._FilteredReflections;
		int finalReflectionTexture = Uniforms._FinalReflectionTexture;
		int tempTexture = Uniforms._TempTexture;
		cb.GetTemporaryRT(normalAndRoughnessTexture, -1, -1, 0, (FilterMode)0, (RenderTextureFormat)0, (RenderTextureReadWrite)1);
		cb.GetTemporaryRT(hitPointTexture, num2, num3, 0, (FilterMode)1, (RenderTextureFormat)2, (RenderTextureReadWrite)1);
		for (int i = 0; i < 5; i++)
		{
			cb.GetTemporaryRT(m_ReflectionTextures[i], num2 >> i, num3 >> i, 0, (FilterMode)1, val6);
		}
		cb.GetTemporaryRT(filteredReflections, num2, num3, 0, (FilterMode)(!k_BilateralUpsample), val6);
		cb.GetTemporaryRT(finalReflectionTexture, num2, num3, 0, (FilterMode)0, val6);
		cb.Blit(RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)2), RenderTargetIdentifier.op_Implicit(normalAndRoughnessTexture), val, 6);
		cb.Blit(RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)2), RenderTargetIdentifier.op_Implicit(hitPointTexture), val, 0);
		cb.Blit(RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)2), RenderTargetIdentifier.op_Implicit(filteredReflections), val, 5);
		cb.Blit(RenderTargetIdentifier.op_Implicit(filteredReflections), RenderTargetIdentifier.op_Implicit(m_ReflectionTextures[0]), val, 8);
		for (int j = 1; j < 5; j++)
		{
			int num9 = m_ReflectionTextures[j - 1];
			int num10 = j;
			cb.GetTemporaryRT(blurTexture, num2 >> num10, num3 >> num10, 0, (FilterMode)1, val6);
			cb.SetGlobalVector(Uniforms._Axis, new Vector4(1f, 0f, 0f, 0f));
			cb.SetGlobalFloat(Uniforms._CurrentMipLevel, (float)j - 1f);
			cb.Blit(RenderTargetIdentifier.op_Implicit(num9), RenderTargetIdentifier.op_Implicit(blurTexture), val, 2);
			cb.SetGlobalVector(Uniforms._Axis, new Vector4(0f, 1f, 0f, 0f));
			num9 = m_ReflectionTextures[j];
			cb.Blit(RenderTargetIdentifier.op_Implicit(blurTexture), RenderTargetIdentifier.op_Implicit(num9), val, 2);
			cb.ReleaseTemporaryRT(blurTexture);
		}
		cb.Blit(RenderTargetIdentifier.op_Implicit(m_ReflectionTextures[0]), RenderTargetIdentifier.op_Implicit(finalReflectionTexture), val, 3);
		cb.GetTemporaryRT(tempTexture, camera.pixelWidth, camera.pixelHeight, 0, (FilterMode)1, val6);
		cb.Blit(RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)2), RenderTargetIdentifier.op_Implicit(tempTexture), val, 1);
		cb.Blit(RenderTargetIdentifier.op_Implicit(tempTexture), RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)2));
		cb.ReleaseTemporaryRT(tempTexture);
	}
}
