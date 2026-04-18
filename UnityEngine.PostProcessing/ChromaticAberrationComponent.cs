namespace UnityEngine.PostProcessing;

public sealed class ChromaticAberrationComponent : PostProcessingComponentRenderTexture<ChromaticAberrationModel>
{
	private static class Uniforms
	{
		internal static readonly int _ChromaticAberration_Amount = Shader.PropertyToID("_ChromaticAberration_Amount");

		internal static readonly int _ChromaticAberration_Spectrum = Shader.PropertyToID("_ChromaticAberration_Spectrum");
	}

	private Texture2D m_SpectrumLut;

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

	public override void OnDisable()
	{
		GraphicsUtils.Destroy((Object)(object)m_SpectrumLut);
		m_SpectrumLut = null;
	}

	public override void Prepare(Material uberMaterial)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		ChromaticAberrationModel.Settings settings = base.model.settings;
		Texture2D val = settings.spectralTexture;
		if ((Object)(object)val == (Object)null)
		{
			if ((Object)(object)m_SpectrumLut == (Object)null)
			{
				m_SpectrumLut = new Texture2D(3, 1, (TextureFormat)3, false)
				{
					name = "Chromatic Aberration Spectrum Lookup",
					filterMode = (FilterMode)1,
					wrapMode = (TextureWrapMode)1,
					anisoLevel = 0,
					hideFlags = (HideFlags)52
				};
				Color[] pixels = (Color[])(object)new Color[3]
				{
					new Color(1f, 0f, 0f),
					new Color(0f, 1f, 0f),
					new Color(0f, 0f, 1f)
				};
				m_SpectrumLut.SetPixels(pixels);
				m_SpectrumLut.Apply();
			}
			val = m_SpectrumLut;
		}
		uberMaterial.EnableKeyword("CHROMATIC_ABERRATION");
		uberMaterial.SetFloat(Uniforms._ChromaticAberration_Amount, settings.intensity * 0.03f);
		uberMaterial.SetTexture(Uniforms._ChromaticAberration_Spectrum, (Texture)(object)val);
	}
}
