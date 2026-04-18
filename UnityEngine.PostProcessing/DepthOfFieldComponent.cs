using System;

namespace UnityEngine.PostProcessing;

public sealed class DepthOfFieldComponent : PostProcessingComponentRenderTexture<DepthOfFieldModel>
{
	private static class Uniforms
	{
		internal static readonly int _DepthOfFieldTex = Shader.PropertyToID("_DepthOfFieldTex");

		internal static readonly int _DepthOfFieldCoCTex = Shader.PropertyToID("_DepthOfFieldCoCTex");

		internal static readonly int _Distance = Shader.PropertyToID("_Distance");

		internal static readonly int _LensCoeff = Shader.PropertyToID("_LensCoeff");

		internal static readonly int _MaxCoC = Shader.PropertyToID("_MaxCoC");

		internal static readonly int _RcpMaxCoC = Shader.PropertyToID("_RcpMaxCoC");

		internal static readonly int _RcpAspect = Shader.PropertyToID("_RcpAspect");

		internal static readonly int _MainTex = Shader.PropertyToID("_MainTex");

		internal static readonly int _CoCTex = Shader.PropertyToID("_CoCTex");

		internal static readonly int _TaaParams = Shader.PropertyToID("_TaaParams");

		internal static readonly int _DepthOfFieldParams = Shader.PropertyToID("_DepthOfFieldParams");
	}

	private const string k_ShaderString = "Hidden/Post FX/Depth Of Field";

	private RenderTexture m_CoCHistory;

	private const float k_FilmHeight = 0.024f;

	public override bool active
	{
		get
		{
			if (base.model.enabled)
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

	private float CalculateFocalLength()
	{
		DepthOfFieldModel.Settings settings = base.model.settings;
		if (!settings.useCameraFov)
		{
			return settings.focalLength / 1000f;
		}
		float num = context.camera.fieldOfView * ((float)Math.PI / 180f);
		return 0.012f / Mathf.Tan(0.5f * num);
	}

	private float CalculateMaxCoCRadius(int screenHeight)
	{
		float num = (float)base.model.settings.kernelSize * 4f + 6f;
		return Mathf.Min(0.05f, num / (float)screenHeight);
	}

	private bool CheckHistory(int width, int height)
	{
		if ((Object)(object)m_CoCHistory != (Object)null && m_CoCHistory.IsCreated() && ((Texture)m_CoCHistory).width == width)
		{
			return ((Texture)m_CoCHistory).height == height;
		}
		return false;
	}

	private RenderTextureFormat SelectFormat(RenderTextureFormat primary, RenderTextureFormat secondary)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (SystemInfo.SupportsRenderTextureFormat(primary))
		{
			return primary;
		}
		if (SystemInfo.SupportsRenderTextureFormat(secondary))
		{
			return secondary;
		}
		return (RenderTextureFormat)7;
	}

	public void Prepare(RenderTexture source, Material uberMaterial, bool antialiasCoC, Vector2 taaJitter, float taaBlending)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		DepthOfFieldModel.Settings settings = base.model.settings;
		RenderTextureFormat format = (RenderTextureFormat)9;
		RenderTextureFormat val = SelectFormat((RenderTextureFormat)16, (RenderTextureFormat)15);
		float num = CalculateFocalLength();
		float num2 = Mathf.Max(settings.focusDistance, num);
		float num3 = (float)((Texture)source).width / (float)((Texture)source).height;
		float num4 = num * num / (settings.aperture * (num2 - num) * 0.024f * 2f);
		float num5 = CalculateMaxCoCRadius(((Texture)source).height);
		Material val2 = context.materialFactory.Get("Hidden/Post FX/Depth Of Field");
		val2.SetFloat(Uniforms._Distance, num2);
		val2.SetFloat(Uniforms._LensCoeff, num4);
		val2.SetFloat(Uniforms._MaxCoC, num5);
		val2.SetFloat(Uniforms._RcpMaxCoC, 1f / num5);
		val2.SetFloat(Uniforms._RcpAspect, 1f / num3);
		RenderTexture val3 = context.renderTextureFactory.Get(context.width, context.height, 0, val, (RenderTextureReadWrite)1, (FilterMode)1, (TextureWrapMode)1);
		Graphics.Blit((Texture)null, val3, val2, 0);
		if (antialiasCoC)
		{
			val2.SetTexture(Uniforms._CoCTex, (Texture)(object)val3);
			float num6 = (CheckHistory(context.width, context.height) ? taaBlending : 0f);
			val2.SetVector(Uniforms._TaaParams, Vector4.op_Implicit(new Vector3(taaJitter.x, taaJitter.y, num6)));
			RenderTexture temporary = RenderTexture.GetTemporary(context.width, context.height, 0, val);
			Graphics.Blit((Texture)(object)m_CoCHistory, temporary, val2, 1);
			context.renderTextureFactory.Release(val3);
			if ((Object)(object)m_CoCHistory != (Object)null)
			{
				RenderTexture.ReleaseTemporary(m_CoCHistory);
			}
			val3 = (m_CoCHistory = temporary);
		}
		RenderTexture val4 = context.renderTextureFactory.Get(context.width / 2, context.height / 2, 0, format, (RenderTextureReadWrite)0, (FilterMode)1, (TextureWrapMode)1);
		val2.SetTexture(Uniforms._CoCTex, (Texture)(object)val3);
		Graphics.Blit((Texture)(object)source, val4, val2, 2);
		RenderTexture val5 = context.renderTextureFactory.Get(context.width / 2, context.height / 2, 0, format, (RenderTextureReadWrite)0, (FilterMode)1, (TextureWrapMode)1);
		Graphics.Blit((Texture)(object)val4, val5, val2, (int)(3 + settings.kernelSize));
		Graphics.Blit((Texture)(object)val5, val4, val2, 7);
		uberMaterial.SetVector(Uniforms._DepthOfFieldParams, Vector4.op_Implicit(new Vector3(num2, num4, num5)));
		if (context.profile.debugViews.IsModeActive(BuiltinDebugViewsModel.Mode.FocusPlane))
		{
			uberMaterial.EnableKeyword("DEPTH_OF_FIELD_COC_VIEW");
			context.Interrupt();
		}
		else
		{
			uberMaterial.SetTexture(Uniforms._DepthOfFieldTex, (Texture)(object)val4);
			uberMaterial.SetTexture(Uniforms._DepthOfFieldCoCTex, (Texture)(object)val3);
			uberMaterial.EnableKeyword("DEPTH_OF_FIELD");
		}
		context.renderTextureFactory.Release(val5);
	}

	public override void OnDisable()
	{
		if ((Object)(object)m_CoCHistory != (Object)null)
		{
			RenderTexture.ReleaseTemporary(m_CoCHistory);
		}
		m_CoCHistory = null;
	}
}
