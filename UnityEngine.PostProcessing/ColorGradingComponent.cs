namespace UnityEngine.PostProcessing;

public sealed class ColorGradingComponent : PostProcessingComponentRenderTexture<ColorGradingModel>
{
	private static class Uniforms
	{
		internal static readonly int _LutParams = Shader.PropertyToID("_LutParams");

		internal static readonly int _NeutralTonemapperParams1 = Shader.PropertyToID("_NeutralTonemapperParams1");

		internal static readonly int _NeutralTonemapperParams2 = Shader.PropertyToID("_NeutralTonemapperParams2");

		internal static readonly int _HueShift = Shader.PropertyToID("_HueShift");

		internal static readonly int _Saturation = Shader.PropertyToID("_Saturation");

		internal static readonly int _Contrast = Shader.PropertyToID("_Contrast");

		internal static readonly int _Balance = Shader.PropertyToID("_Balance");

		internal static readonly int _Lift = Shader.PropertyToID("_Lift");

		internal static readonly int _InvGamma = Shader.PropertyToID("_InvGamma");

		internal static readonly int _Gain = Shader.PropertyToID("_Gain");

		internal static readonly int _Slope = Shader.PropertyToID("_Slope");

		internal static readonly int _Power = Shader.PropertyToID("_Power");

		internal static readonly int _Offset = Shader.PropertyToID("_Offset");

		internal static readonly int _ChannelMixerRed = Shader.PropertyToID("_ChannelMixerRed");

		internal static readonly int _ChannelMixerGreen = Shader.PropertyToID("_ChannelMixerGreen");

		internal static readonly int _ChannelMixerBlue = Shader.PropertyToID("_ChannelMixerBlue");

		internal static readonly int _Curves = Shader.PropertyToID("_Curves");

		internal static readonly int _LogLut = Shader.PropertyToID("_LogLut");

		internal static readonly int _LogLut_Params = Shader.PropertyToID("_LogLut_Params");

		internal static readonly int _ExposureEV = Shader.PropertyToID("_ExposureEV");
	}

	private const int k_InternalLogLutSize = 32;

	private const int k_CurvePrecision = 128;

	private const float k_CurveStep = 1f / 128f;

	private Texture2D m_GradingCurves;

	private Color[] m_pixels = (Color[])(object)new Color[256];

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

	private float StandardIlluminantY(float x)
	{
		return 2.87f * x - 3f * x * x - 0.27509508f;
	}

	private Vector3 CIExyToLMS(float x, float y)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f;
		float num2 = num * x / y;
		float num3 = num * (1f - x - y) / y;
		float num4 = 0.7328f * num2 + 0.4296f * num - 0.1624f * num3;
		float num5 = -0.7036f * num2 + 1.6975f * num + 0.0061f * num3;
		float num6 = 0.003f * num2 + 0.0136f * num + 0.9834f * num3;
		return new Vector3(num4, num5, num6);
	}

	private Vector3 CalculateColorBalance(float temperature, float tint)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		float num = temperature / 55f;
		float num2 = tint / 55f;
		float x = 0.31271f - num * ((num < 0f) ? 0.1f : 0.05f);
		float y = StandardIlluminantY(x) + num2 * 0.05f;
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(0.949237f, 1.03542f, 1.08728f);
		Vector3 val2 = CIExyToLMS(x, y);
		return new Vector3(val.x / val2.x, val.y / val2.y, val.z / val2.z);
	}

	private static Color NormalizeColor(Color c)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		float num = (c.r + c.g + c.b) / 3f;
		if (Mathf.Approximately(num, 0f))
		{
			return new Color(1f, 1f, 1f, c.a);
		}
		return new Color
		{
			r = c.r / num,
			g = c.g / num,
			b = c.b / num,
			a = c.a
		};
	}

	private static Vector3 ClampVector(Vector3 v, float min, float max)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(Mathf.Clamp(v.x, min, max), Mathf.Clamp(v.y, min, max), Mathf.Clamp(v.z, min, max));
	}

	public static Vector3 GetLiftValue(Color lift)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		Color val = NormalizeColor(lift);
		float num = (val.r + val.g + val.b) / 3f;
		float num2 = (val.r - num) * 0.1f + lift.a;
		float num3 = (val.g - num) * 0.1f + lift.a;
		float num4 = (val.b - num) * 0.1f + lift.a;
		return ClampVector(new Vector3(num2, num3, num4), -1f, 1f);
	}

	public static Vector3 GetGammaValue(Color gamma)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		Color val = NormalizeColor(gamma);
		float num = (val.r + val.g + val.b) / 3f;
		gamma.a *= ((gamma.a < 0f) ? 0.8f : 5f);
		float num2 = Mathf.Pow(2f, (val.r - num) * 0.5f) + gamma.a;
		float num3 = Mathf.Pow(2f, (val.g - num) * 0.5f) + gamma.a;
		float num4 = Mathf.Pow(2f, (val.b - num) * 0.5f) + gamma.a;
		float num5 = 1f / Mathf.Max(0.01f, num2);
		float num6 = 1f / Mathf.Max(0.01f, num3);
		float num7 = 1f / Mathf.Max(0.01f, num4);
		return ClampVector(new Vector3(num5, num6, num7), 0f, 5f);
	}

	public static Vector3 GetGainValue(Color gain)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		Color val = NormalizeColor(gain);
		float num = (val.r + val.g + val.b) / 3f;
		gain.a *= ((gain.a > 0f) ? 3f : 1f);
		float num2 = Mathf.Pow(2f, (val.r - num) * 0.5f) + gain.a;
		float num3 = Mathf.Pow(2f, (val.g - num) * 0.5f) + gain.a;
		float num4 = Mathf.Pow(2f, (val.b - num) * 0.5f) + gain.a;
		return ClampVector(new Vector3(num2, num3, num4), 0f, 4f);
	}

	public static void CalculateLiftGammaGain(Color lift, Color gamma, Color gain, out Vector3 outLift, out Vector3 outGamma, out Vector3 outGain)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		outLift = GetLiftValue(lift);
		outGamma = GetGammaValue(gamma);
		outGain = GetGainValue(gain);
	}

	public static Vector3 GetSlopeValue(Color slope)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		Color val = NormalizeColor(slope);
		float num = (val.r + val.g + val.b) / 3f;
		slope.a *= 0.5f;
		float num2 = (val.r - num) * 0.1f + slope.a + 1f;
		float num3 = (val.g - num) * 0.1f + slope.a + 1f;
		float num4 = (val.b - num) * 0.1f + slope.a + 1f;
		return ClampVector(new Vector3(num2, num3, num4), 0f, 2f);
	}

	public static Vector3 GetPowerValue(Color power)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		Color val = NormalizeColor(power);
		float num = (val.r + val.g + val.b) / 3f;
		power.a *= 0.5f;
		float num2 = (val.r - num) * 0.1f + power.a + 1f;
		float num3 = (val.g - num) * 0.1f + power.a + 1f;
		float num4 = (val.b - num) * 0.1f + power.a + 1f;
		float num5 = 1f / Mathf.Max(0.01f, num2);
		float num6 = 1f / Mathf.Max(0.01f, num3);
		float num7 = 1f / Mathf.Max(0.01f, num4);
		return ClampVector(new Vector3(num5, num6, num7), 0.5f, 2.5f);
	}

	public static Vector3 GetOffsetValue(Color offset)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		Color val = NormalizeColor(offset);
		float num = (val.r + val.g + val.b) / 3f;
		offset.a *= 0.5f;
		float num2 = (val.r - num) * 0.05f + offset.a;
		float num3 = (val.g - num) * 0.05f + offset.a;
		float num4 = (val.b - num) * 0.05f + offset.a;
		return ClampVector(new Vector3(num2, num3, num4), -0.8f, 0.8f);
	}

	public static void CalculateSlopePowerOffset(Color slope, Color power, Color offset, out Vector3 outSlope, out Vector3 outPower, out Vector3 outOffset)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		outSlope = GetSlopeValue(slope);
		outPower = GetPowerValue(power);
		outOffset = GetOffsetValue(offset);
	}

	private TextureFormat GetCurveFormat()
	{
		if (!SystemInfo.SupportsTextureFormat((TextureFormat)17))
		{
			return (TextureFormat)4;
		}
		return (TextureFormat)17;
	}

	private Texture2D GetCurveTexture()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_GradingCurves == (Object)null)
		{
			m_GradingCurves = new Texture2D(128, 2, GetCurveFormat(), false, true)
			{
				name = "Internal Curves Texture",
				hideFlags = (HideFlags)52,
				anisoLevel = 0,
				wrapMode = (TextureWrapMode)1,
				filterMode = (FilterMode)1
			};
		}
		ColorGradingModel.CurvesSettings curves = base.model.settings.curves;
		curves.hueVShue.Cache();
		curves.hueVSsat.Cache();
		for (int i = 0; i < 128; i++)
		{
			float t = (float)i * (1f / 128f);
			float num = curves.hueVShue.Evaluate(t);
			float num2 = curves.hueVSsat.Evaluate(t);
			float num3 = curves.satVSsat.Evaluate(t);
			float num4 = curves.lumVSsat.Evaluate(t);
			m_pixels[i] = new Color(num, num2, num3, num4);
			float num5 = curves.master.Evaluate(t);
			float num6 = curves.red.Evaluate(t);
			float num7 = curves.green.Evaluate(t);
			float num8 = curves.blue.Evaluate(t);
			m_pixels[i + 128] = new Color(num6, num7, num8, num5);
		}
		m_GradingCurves.SetPixels(m_pixels);
		m_GradingCurves.Apply(false, false);
		return m_GradingCurves;
	}

	private bool IsLogLutValid(RenderTexture lut)
	{
		if ((Object)(object)lut != (Object)null && lut.IsCreated())
		{
			return ((Texture)lut).height == 32;
		}
		return false;
	}

	private RenderTextureFormat GetLutFormat()
	{
		if (!SystemInfo.SupportsRenderTextureFormat((RenderTextureFormat)2))
		{
			return (RenderTextureFormat)0;
		}
		return (RenderTextureFormat)2;
	}

	private void GenerateLut()
	{
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_0356: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0371: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		ColorGradingModel.Settings settings = base.model.settings;
		if (!IsLogLutValid(base.model.bakedLut))
		{
			GraphicsUtils.Destroy((Object)(object)base.model.bakedLut);
			base.model.bakedLut = new RenderTexture(1024, 32, 0, GetLutFormat())
			{
				name = "Color Grading Log LUT",
				hideFlags = (HideFlags)52,
				filterMode = (FilterMode)1,
				wrapMode = (TextureWrapMode)1,
				anisoLevel = 0
			};
		}
		Material val = context.materialFactory.Get("Hidden/Post FX/Lut Generator");
		val.SetVector(Uniforms._LutParams, new Vector4(32f, 0.00048828125f, 1f / 64f, 1.032258f));
		val.shaderKeywords = null;
		ColorGradingModel.TonemappingSettings tonemapping = settings.tonemapping;
		switch (tonemapping.tonemapper)
		{
		case ColorGradingModel.Tonemapper.Neutral:
		{
			val.EnableKeyword("TONEMAPPING_NEUTRAL");
			float num = tonemapping.neutralBlackIn * 20f + 1f;
			float num2 = tonemapping.neutralBlackOut * 10f + 1f;
			float num3 = tonemapping.neutralWhiteIn / 20f;
			float num4 = 1f - tonemapping.neutralWhiteOut / 20f;
			float num5 = num / num2;
			float num6 = num3 / num4;
			float num7 = Mathf.Max(0f, Mathf.LerpUnclamped(0.57f, 0.37f, num5));
			float num8 = Mathf.LerpUnclamped(0.01f, 0.24f, num6);
			float num9 = Mathf.Max(0f, Mathf.LerpUnclamped(0.02f, 0.2f, num5));
			val.SetVector(Uniforms._NeutralTonemapperParams1, new Vector4(0.2f, num7, num8, num9));
			val.SetVector(Uniforms._NeutralTonemapperParams2, new Vector4(0.02f, 0.3f, tonemapping.neutralWhiteLevel, tonemapping.neutralWhiteClip / 10f));
			break;
		}
		case ColorGradingModel.Tonemapper.ACES:
			val.EnableKeyword("TONEMAPPING_FILMIC");
			break;
		}
		val.SetFloat(Uniforms._HueShift, settings.basic.hueShift / 360f);
		val.SetFloat(Uniforms._Saturation, settings.basic.saturation);
		val.SetFloat(Uniforms._Contrast, settings.basic.contrast);
		val.SetVector(Uniforms._Balance, Vector4.op_Implicit(CalculateColorBalance(settings.basic.temperature, settings.basic.tint)));
		CalculateLiftGammaGain(settings.colorWheels.linear.lift, settings.colorWheels.linear.gamma, settings.colorWheels.linear.gain, out var outLift, out var outGamma, out var outGain);
		val.SetVector(Uniforms._Lift, Vector4.op_Implicit(outLift));
		val.SetVector(Uniforms._InvGamma, Vector4.op_Implicit(outGamma));
		val.SetVector(Uniforms._Gain, Vector4.op_Implicit(outGain));
		CalculateSlopePowerOffset(settings.colorWheels.log.slope, settings.colorWheels.log.power, settings.colorWheels.log.offset, out var outSlope, out var outPower, out var outOffset);
		val.SetVector(Uniforms._Slope, Vector4.op_Implicit(outSlope));
		val.SetVector(Uniforms._Power, Vector4.op_Implicit(outPower));
		val.SetVector(Uniforms._Offset, Vector4.op_Implicit(outOffset));
		val.SetVector(Uniforms._ChannelMixerRed, Vector4.op_Implicit(settings.channelMixer.red));
		val.SetVector(Uniforms._ChannelMixerGreen, Vector4.op_Implicit(settings.channelMixer.green));
		val.SetVector(Uniforms._ChannelMixerBlue, Vector4.op_Implicit(settings.channelMixer.blue));
		val.SetTexture(Uniforms._Curves, (Texture)(object)GetCurveTexture());
		Graphics.Blit((Texture)null, base.model.bakedLut, val, 0);
	}

	public override void Prepare(Material uberMaterial)
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		if (base.model.isDirty || !IsLogLutValid(base.model.bakedLut))
		{
			GenerateLut();
			base.model.isDirty = false;
		}
		uberMaterial.EnableKeyword(context.profile.debugViews.IsModeActive(BuiltinDebugViewsModel.Mode.PreGradingLog) ? "COLOR_GRADING_LOG_VIEW" : "COLOR_GRADING");
		RenderTexture bakedLut = base.model.bakedLut;
		uberMaterial.SetTexture(Uniforms._LogLut, (Texture)(object)bakedLut);
		uberMaterial.SetVector(Uniforms._LogLut_Params, Vector4.op_Implicit(new Vector3(1f / (float)((Texture)bakedLut).width, 1f / (float)((Texture)bakedLut).height, (float)((Texture)bakedLut).height - 1f)));
		float num = Mathf.Exp(base.model.settings.basic.postExposure * 0.6931472f);
		uberMaterial.SetFloat(Uniforms._ExposureEV, num);
	}

	public void OnGUI()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		RenderTexture bakedLut = base.model.bakedLut;
		Rect viewport = context.viewport;
		GUI.DrawTexture(new Rect(((Rect)(ref viewport)).x * (float)Screen.width + 8f, 8f, (float)((Texture)bakedLut).width, (float)((Texture)bakedLut).height), (Texture)(object)bakedLut);
	}

	public override void OnDisable()
	{
		GraphicsUtils.Destroy((Object)(object)m_GradingCurves);
		GraphicsUtils.Destroy((Object)(object)base.model.bakedLut);
		m_GradingCurves = null;
		base.model.bakedLut = null;
	}
}
