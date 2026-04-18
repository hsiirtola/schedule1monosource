using System;

namespace UnityEngine.PostProcessing;

public sealed class EyeAdaptationComponent : PostProcessingComponentRenderTexture<EyeAdaptationModel>
{
	private static class Uniforms
	{
		internal static readonly int _Params = Shader.PropertyToID("_Params");

		internal static readonly int _Speed = Shader.PropertyToID("_Speed");

		internal static readonly int _ScaleOffsetRes = Shader.PropertyToID("_ScaleOffsetRes");

		internal static readonly int _ExposureCompensation = Shader.PropertyToID("_ExposureCompensation");

		internal static readonly int _AutoExposure = Shader.PropertyToID("_AutoExposure");

		internal static readonly int _DebugWidth = Shader.PropertyToID("_DebugWidth");
	}

	private ComputeShader m_EyeCompute;

	private ComputeBuffer m_HistogramBuffer;

	private readonly RenderTexture[] m_AutoExposurePool = (RenderTexture[])(object)new RenderTexture[2];

	private int m_AutoExposurePingPing;

	private RenderTexture m_CurrentAutoExposure;

	private RenderTexture m_DebugHistogram;

	private static uint[] s_EmptyHistogramBuffer;

	private bool m_FirstFrame = true;

	private const int k_HistogramBins = 64;

	private const int k_HistogramThreadX = 16;

	private const int k_HistogramThreadY = 16;

	public override bool active
	{
		get
		{
			if (base.model.enabled && SystemInfo.supportsComputeShaders)
			{
				return !context.interrupted;
			}
			return false;
		}
	}

	public void ResetHistory()
	{
		m_FirstFrame = true;
	}

	public override void OnEnable()
	{
		m_FirstFrame = true;
	}

	public override void OnDisable()
	{
		RenderTexture[] autoExposurePool = m_AutoExposurePool;
		for (int i = 0; i < autoExposurePool.Length; i++)
		{
			GraphicsUtils.Destroy((Object)(object)autoExposurePool[i]);
		}
		if (m_HistogramBuffer != null)
		{
			m_HistogramBuffer.Release();
		}
		m_HistogramBuffer = null;
		if ((Object)(object)m_DebugHistogram != (Object)null)
		{
			m_DebugHistogram.Release();
		}
		m_DebugHistogram = null;
	}

	private Vector4 GetHistogramScaleOffsetRes()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		EyeAdaptationModel.Settings settings = base.model.settings;
		float num = settings.logMax - settings.logMin;
		float num2 = 1f / num;
		float num3 = (float)(-settings.logMin) * num2;
		return new Vector4(num2, num3, Mathf.Floor((float)context.width / 2f), Mathf.Floor((float)context.height / 2f));
	}

	public Texture Prepare(RenderTexture source, Material uberMaterial)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Expected O, but got Unknown
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Expected O, but got Unknown
		EyeAdaptationModel.Settings settings = base.model.settings;
		if ((Object)(object)m_EyeCompute == (Object)null)
		{
			m_EyeCompute = Resources.Load<ComputeShader>("Shaders/EyeHistogram");
		}
		Material val = context.materialFactory.Get("Hidden/Post FX/Eye Adaptation");
		val.shaderKeywords = null;
		if (m_HistogramBuffer == null)
		{
			m_HistogramBuffer = new ComputeBuffer(64, 4);
		}
		if (s_EmptyHistogramBuffer == null)
		{
			s_EmptyHistogramBuffer = new uint[64];
		}
		Vector4 histogramScaleOffsetRes = GetHistogramScaleOffsetRes();
		RenderTexture val2 = context.renderTextureFactory.Get((int)histogramScaleOffsetRes.z, (int)histogramScaleOffsetRes.w, 0, source.format, (RenderTextureReadWrite)0, (FilterMode)1, (TextureWrapMode)1);
		Graphics.Blit((Texture)(object)source, val2);
		if ((Object)(object)m_AutoExposurePool[0] == (Object)null || !m_AutoExposurePool[0].IsCreated())
		{
			m_AutoExposurePool[0] = new RenderTexture(1, 1, 0, (RenderTextureFormat)14);
		}
		if ((Object)(object)m_AutoExposurePool[1] == (Object)null || !m_AutoExposurePool[1].IsCreated())
		{
			m_AutoExposurePool[1] = new RenderTexture(1, 1, 0, (RenderTextureFormat)14);
		}
		m_HistogramBuffer.SetData((Array)s_EmptyHistogramBuffer);
		int num = m_EyeCompute.FindKernel("KEyeHistogram");
		m_EyeCompute.SetBuffer(num, "_Histogram", m_HistogramBuffer);
		m_EyeCompute.SetTexture(num, "_Source", (Texture)(object)val2);
		m_EyeCompute.SetVector("_ScaleOffsetRes", histogramScaleOffsetRes);
		m_EyeCompute.Dispatch(num, Mathf.CeilToInt((float)((Texture)val2).width / 16f), Mathf.CeilToInt((float)((Texture)val2).height / 16f), 1);
		context.renderTextureFactory.Release(val2);
		settings.highPercent = Mathf.Clamp(settings.highPercent, 1.01f, 99f);
		settings.lowPercent = Mathf.Clamp(settings.lowPercent, 1f, settings.highPercent - 0.01f);
		val.SetBuffer("_Histogram", m_HistogramBuffer);
		val.SetVector(Uniforms._Params, new Vector4(settings.lowPercent * 0.01f, settings.highPercent * 0.01f, Mathf.Exp(settings.minLuminance * 0.6931472f), Mathf.Exp(settings.maxLuminance * 0.6931472f)));
		val.SetVector(Uniforms._Speed, Vector4.op_Implicit(new Vector2(settings.speedDown, settings.speedUp)));
		val.SetVector(Uniforms._ScaleOffsetRes, histogramScaleOffsetRes);
		val.SetFloat(Uniforms._ExposureCompensation, settings.keyValue);
		if (settings.dynamicKeyValue)
		{
			val.EnableKeyword("AUTO_KEY_VALUE");
		}
		if (m_FirstFrame || !Application.isPlaying)
		{
			m_CurrentAutoExposure = m_AutoExposurePool[0];
			Graphics.Blit((Texture)null, m_CurrentAutoExposure, val, 1);
			Graphics.Blit((Texture)(object)m_AutoExposurePool[0], m_AutoExposurePool[1]);
		}
		else
		{
			int autoExposurePingPing = m_AutoExposurePingPing;
			RenderTexture obj = m_AutoExposurePool[++autoExposurePingPing % 2];
			RenderTexture val3 = m_AutoExposurePool[++autoExposurePingPing % 2];
			Graphics.Blit((Texture)(object)obj, val3, val, (int)settings.adaptationType);
			m_AutoExposurePingPing = ++autoExposurePingPing % 2;
			m_CurrentAutoExposure = val3;
		}
		if (context.profile.debugViews.IsModeActive(BuiltinDebugViewsModel.Mode.EyeAdaptation))
		{
			if ((Object)(object)m_DebugHistogram == (Object)null || !m_DebugHistogram.IsCreated())
			{
				m_DebugHistogram = new RenderTexture(256, 128, 0, (RenderTextureFormat)0)
				{
					filterMode = (FilterMode)0,
					wrapMode = (TextureWrapMode)1
				};
			}
			val.SetFloat(Uniforms._DebugWidth, (float)((Texture)m_DebugHistogram).width);
			Graphics.Blit((Texture)null, m_DebugHistogram, val, 2);
		}
		m_FirstFrame = false;
		return (Texture)(object)m_CurrentAutoExposure;
	}

	public void OnGUI()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)m_DebugHistogram == (Object)null) && m_DebugHistogram.IsCreated())
		{
			Rect viewport = context.viewport;
			GUI.DrawTexture(new Rect(((Rect)(ref viewport)).x * (float)Screen.width + 8f, 8f, (float)((Texture)m_DebugHistogram).width, (float)((Texture)m_DebugHistogram).height), (Texture)(object)m_DebugHistogram);
		}
	}
}
