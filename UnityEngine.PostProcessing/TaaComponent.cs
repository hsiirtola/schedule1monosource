using System;

namespace UnityEngine.PostProcessing;

public sealed class TaaComponent : PostProcessingComponentRenderTexture<AntialiasingModel>
{
	private static class Uniforms
	{
		internal static int _Jitter = Shader.PropertyToID("_Jitter");

		internal static int _SharpenParameters = Shader.PropertyToID("_SharpenParameters");

		internal static int _FinalBlendParameters = Shader.PropertyToID("_FinalBlendParameters");

		internal static int _HistoryTex = Shader.PropertyToID("_HistoryTex");

		internal static int _MainTex = Shader.PropertyToID("_MainTex");
	}

	private const string k_ShaderString = "Hidden/Post FX/Temporal Anti-aliasing";

	private const int k_SampleCount = 8;

	private readonly RenderBuffer[] m_MRT = (RenderBuffer[])(object)new RenderBuffer[2];

	private int m_SampleIndex;

	private bool m_ResetHistory = true;

	private RenderTexture m_HistoryTexture;

	public override bool active
	{
		get
		{
			if (base.model.enabled && base.model.settings.method == AntialiasingModel.Method.Taa && SystemInfo.supportsMotionVectors && SystemInfo.supportedRenderTargetCount >= 2)
			{
				return !context.interrupted;
			}
			return false;
		}
	}

	public Vector2 jitterVector { get; private set; }

	public override DepthTextureMode GetCameraFlags()
	{
		return (DepthTextureMode)5;
	}

	public void ResetHistory()
	{
		m_ResetHistory = true;
	}

	public void SetProjectionMatrix(Func<Vector2, Matrix4x4> jitteredFunc)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		AntialiasingModel.TaaSettings taaSettings = base.model.settings.taaSettings;
		Vector2 val = GenerateRandomOffset();
		val *= taaSettings.jitterSpread;
		context.camera.nonJitteredProjectionMatrix = context.camera.projectionMatrix;
		if (jitteredFunc != null)
		{
			context.camera.projectionMatrix = jitteredFunc(val);
		}
		else
		{
			context.camera.projectionMatrix = (context.camera.orthographic ? GetOrthographicProjectionMatrix(val) : GetPerspectiveProjectionMatrix(val));
		}
		context.camera.useJitteredProjectionMatrixForTransparentRendering = false;
		val.x /= context.width;
		val.y /= context.height;
		context.materialFactory.Get("Hidden/Post FX/Temporal Anti-aliasing").SetVector(Uniforms._Jitter, Vector4.op_Implicit(val));
		jitterVector = val;
	}

	public void Render(RenderTexture source, RenderTexture destination)
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		Material val = context.materialFactory.Get("Hidden/Post FX/Temporal Anti-aliasing");
		val.shaderKeywords = null;
		AntialiasingModel.TaaSettings taaSettings = base.model.settings.taaSettings;
		if (m_ResetHistory || (Object)(object)m_HistoryTexture == (Object)null || ((Texture)m_HistoryTexture).width != ((Texture)source).width || ((Texture)m_HistoryTexture).height != ((Texture)source).height)
		{
			if (Object.op_Implicit((Object)(object)m_HistoryTexture))
			{
				RenderTexture.ReleaseTemporary(m_HistoryTexture);
			}
			m_HistoryTexture = RenderTexture.GetTemporary(((Texture)source).width, ((Texture)source).height, 0, source.format);
			((Object)m_HistoryTexture).name = "TAA History";
			Graphics.Blit((Texture)(object)source, m_HistoryTexture, val, 2);
		}
		val.SetVector(Uniforms._SharpenParameters, new Vector4(taaSettings.sharpen, 0f, 0f, 0f));
		val.SetVector(Uniforms._FinalBlendParameters, new Vector4(taaSettings.stationaryBlending, taaSettings.motionBlending, 6000f, 0f));
		val.SetTexture(Uniforms._MainTex, (Texture)(object)source);
		val.SetTexture(Uniforms._HistoryTex, (Texture)(object)m_HistoryTexture);
		RenderTexture temporary = RenderTexture.GetTemporary(((Texture)source).width, ((Texture)source).height, 0, source.format);
		((Object)temporary).name = "TAA History";
		m_MRT[0] = destination.colorBuffer;
		m_MRT[1] = temporary.colorBuffer;
		Graphics.SetRenderTarget(m_MRT, source.depthBuffer);
		GraphicsUtils.Blit(val, context.camera.orthographic ? 1 : 0);
		RenderTexture.ReleaseTemporary(m_HistoryTexture);
		m_HistoryTexture = temporary;
		m_ResetHistory = false;
	}

	private float GetHaltonValue(int index, int radix)
	{
		float num = 0f;
		float num2 = 1f / (float)radix;
		while (index > 0)
		{
			num += (float)(index % radix) * num2;
			index /= radix;
			num2 /= (float)radix;
		}
		return num;
	}

	private Vector2 GenerateRandomOffset()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		Vector2 result = new Vector2(GetHaltonValue(m_SampleIndex & 0x3FF, 2), GetHaltonValue(m_SampleIndex & 0x3FF, 3));
		if (++m_SampleIndex >= 8)
		{
			m_SampleIndex = 0;
		}
		return result;
	}

	private Matrix4x4 GetPerspectiveProjectionMatrix(Vector2 offset)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Tan((float)Math.PI / 360f * context.camera.fieldOfView);
		float num2 = num * context.camera.aspect;
		offset.x *= num2 / (0.5f * (float)context.width);
		offset.y *= num / (0.5f * (float)context.height);
		float num3 = (offset.x - num2) * context.camera.nearClipPlane;
		float num4 = (offset.x + num2) * context.camera.nearClipPlane;
		float num5 = (offset.y + num) * context.camera.nearClipPlane;
		float num6 = (offset.y - num) * context.camera.nearClipPlane;
		Matrix4x4 result = default(Matrix4x4);
		((Matrix4x4)(ref result))[0, 0] = 2f * context.camera.nearClipPlane / (num4 - num3);
		((Matrix4x4)(ref result))[0, 1] = 0f;
		((Matrix4x4)(ref result))[0, 2] = (num4 + num3) / (num4 - num3);
		((Matrix4x4)(ref result))[0, 3] = 0f;
		((Matrix4x4)(ref result))[1, 0] = 0f;
		((Matrix4x4)(ref result))[1, 1] = 2f * context.camera.nearClipPlane / (num5 - num6);
		((Matrix4x4)(ref result))[1, 2] = (num5 + num6) / (num5 - num6);
		((Matrix4x4)(ref result))[1, 3] = 0f;
		((Matrix4x4)(ref result))[2, 0] = 0f;
		((Matrix4x4)(ref result))[2, 1] = 0f;
		((Matrix4x4)(ref result))[2, 2] = (0f - (context.camera.farClipPlane + context.camera.nearClipPlane)) / (context.camera.farClipPlane - context.camera.nearClipPlane);
		((Matrix4x4)(ref result))[2, 3] = (0f - 2f * context.camera.farClipPlane * context.camera.nearClipPlane) / (context.camera.farClipPlane - context.camera.nearClipPlane);
		((Matrix4x4)(ref result))[3, 0] = 0f;
		((Matrix4x4)(ref result))[3, 1] = 0f;
		((Matrix4x4)(ref result))[3, 2] = -1f;
		((Matrix4x4)(ref result))[3, 3] = 0f;
		return result;
	}

	private Matrix4x4 GetOrthographicProjectionMatrix(Vector2 offset)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		float orthographicSize = context.camera.orthographicSize;
		float num = orthographicSize * context.camera.aspect;
		offset.x *= num / (0.5f * (float)context.width);
		offset.y *= orthographicSize / (0.5f * (float)context.height);
		float num2 = offset.x - num;
		float num3 = offset.x + num;
		float num4 = offset.y + orthographicSize;
		float num5 = offset.y - orthographicSize;
		return Matrix4x4.Ortho(num2, num3, num5, num4, context.camera.nearClipPlane, context.camera.farClipPlane);
	}

	public override void OnDisable()
	{
		if ((Object)(object)m_HistoryTexture != (Object)null)
		{
			RenderTexture.ReleaseTemporary(m_HistoryTexture);
		}
		m_HistoryTexture = null;
		m_SampleIndex = 0;
		ResetHistory();
	}
}
