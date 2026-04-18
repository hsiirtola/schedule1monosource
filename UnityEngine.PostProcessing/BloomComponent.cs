namespace UnityEngine.PostProcessing;

public sealed class BloomComponent : PostProcessingComponentRenderTexture<BloomModel>
{
	private static class Uniforms
	{
		internal static readonly int _AutoExposure = Shader.PropertyToID("_AutoExposure");

		internal static readonly int _Threshold = Shader.PropertyToID("_Threshold");

		internal static readonly int _Curve = Shader.PropertyToID("_Curve");

		internal static readonly int _PrefilterOffs = Shader.PropertyToID("_PrefilterOffs");

		internal static readonly int _SampleScale = Shader.PropertyToID("_SampleScale");

		internal static readonly int _BaseTex = Shader.PropertyToID("_BaseTex");

		internal static readonly int _BloomTex = Shader.PropertyToID("_BloomTex");

		internal static readonly int _Bloom_Settings = Shader.PropertyToID("_Bloom_Settings");

		internal static readonly int _Bloom_DirtTex = Shader.PropertyToID("_Bloom_DirtTex");

		internal static readonly int _Bloom_DirtIntensity = Shader.PropertyToID("_Bloom_DirtIntensity");
	}

	private const int k_MaxPyramidBlurLevel = 16;

	private readonly RenderTexture[] m_BlurBuffer1 = (RenderTexture[])(object)new RenderTexture[16];

	private readonly RenderTexture[] m_BlurBuffer2 = (RenderTexture[])(object)new RenderTexture[16];

	public override bool active
	{
		get
		{
			if (base.model.enabled && base.model.settings.bloom.intensity > 0f)
			{
				return !context.interrupted;
			}
			return false;
		}
	}

	public void Prepare(RenderTexture source, Material uberMaterial, Texture autoExposure)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		BloomModel.BloomSettings bloom = base.model.settings.bloom;
		BloomModel.LensDirtSettings lensDirt = base.model.settings.lensDirt;
		Material val = context.materialFactory.Get("Hidden/Post FX/Bloom");
		val.shaderKeywords = null;
		val.SetTexture(Uniforms._AutoExposure, autoExposure);
		int width = context.width / 2;
		int num = context.height / 2;
		RenderTextureFormat format = (RenderTextureFormat)(Application.isMobilePlatform ? 7 : 9);
		float num2 = Mathf.Log((float)num, 2f) + bloom.radius - 8f;
		int num3 = (int)num2;
		int num4 = Mathf.Clamp(num3, 1, 16);
		float thresholdLinear = bloom.thresholdLinear;
		val.SetFloat(Uniforms._Threshold, thresholdLinear);
		float num5 = thresholdLinear * bloom.softKnee + 1E-05f;
		Vector3 val2 = default(Vector3);
		((Vector3)(ref val2))._002Ector(thresholdLinear - num5, num5 * 2f, 0.25f / num5);
		val.SetVector(Uniforms._Curve, Vector4.op_Implicit(val2));
		val.SetFloat(Uniforms._PrefilterOffs, bloom.antiFlicker ? (-0.5f) : 0f);
		float num6 = 0.5f + num2 - (float)num3;
		val.SetFloat(Uniforms._SampleScale, num6);
		if (bloom.antiFlicker)
		{
			val.EnableKeyword("ANTI_FLICKER");
		}
		RenderTexture val3 = context.renderTextureFactory.Get(width, num, 0, format, (RenderTextureReadWrite)0, (FilterMode)1, (TextureWrapMode)1);
		Graphics.Blit((Texture)(object)source, val3, val, 0);
		RenderTexture val4 = val3;
		for (int i = 0; i < num4; i++)
		{
			m_BlurBuffer1[i] = context.renderTextureFactory.Get(((Texture)val4).width / 2, ((Texture)val4).height / 2, 0, format, (RenderTextureReadWrite)0, (FilterMode)1, (TextureWrapMode)1);
			int num7 = ((i == 0) ? 1 : 2);
			Graphics.Blit((Texture)(object)val4, m_BlurBuffer1[i], val, num7);
			val4 = m_BlurBuffer1[i];
		}
		for (int num8 = num4 - 2; num8 >= 0; num8--)
		{
			RenderTexture val5 = m_BlurBuffer1[num8];
			val.SetTexture(Uniforms._BaseTex, (Texture)(object)val5);
			m_BlurBuffer2[num8] = context.renderTextureFactory.Get(((Texture)val5).width, ((Texture)val5).height, 0, format, (RenderTextureReadWrite)0, (FilterMode)1, (TextureWrapMode)1);
			Graphics.Blit((Texture)(object)val4, m_BlurBuffer2[num8], val, 3);
			val4 = m_BlurBuffer2[num8];
		}
		RenderTexture val6 = val4;
		for (int j = 0; j < 16; j++)
		{
			if ((Object)(object)m_BlurBuffer1[j] != (Object)null)
			{
				context.renderTextureFactory.Release(m_BlurBuffer1[j]);
			}
			if ((Object)(object)m_BlurBuffer2[j] != (Object)null && (Object)(object)m_BlurBuffer2[j] != (Object)(object)val6)
			{
				context.renderTextureFactory.Release(m_BlurBuffer2[j]);
			}
			m_BlurBuffer1[j] = null;
			m_BlurBuffer2[j] = null;
		}
		context.renderTextureFactory.Release(val3);
		uberMaterial.SetTexture(Uniforms._BloomTex, (Texture)(object)val6);
		uberMaterial.SetVector(Uniforms._Bloom_Settings, Vector4.op_Implicit(new Vector2(num6, bloom.intensity)));
		if (lensDirt.intensity > 0f && (Object)(object)lensDirt.texture != (Object)null)
		{
			uberMaterial.SetTexture(Uniforms._Bloom_DirtTex, lensDirt.texture);
			uberMaterial.SetFloat(Uniforms._Bloom_DirtIntensity, lensDirt.intensity);
			uberMaterial.EnableKeyword("BLOOM_LENS_DIRT");
		}
		else
		{
			uberMaterial.EnableKeyword("BLOOM");
		}
	}
}
