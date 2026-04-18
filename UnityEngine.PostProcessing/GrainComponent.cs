namespace UnityEngine.PostProcessing;

public sealed class GrainComponent : PostProcessingComponentRenderTexture<GrainModel>
{
	private static class Uniforms
	{
		internal static readonly int _Grain_Params1 = Shader.PropertyToID("_Grain_Params1");

		internal static readonly int _Grain_Params2 = Shader.PropertyToID("_Grain_Params2");

		internal static readonly int _GrainTex = Shader.PropertyToID("_GrainTex");

		internal static readonly int _Phase = Shader.PropertyToID("_Phase");
	}

	private RenderTexture m_GrainLookupRT;

	public override bool active
	{
		get
		{
			if (base.model.enabled && base.model.settings.intensity > 0f && SystemInfo.SupportsRenderTextureFormat((RenderTextureFormat)2))
			{
				return !context.interrupted;
			}
			return false;
		}
	}

	public override void OnDisable()
	{
		GraphicsUtils.Destroy((Object)(object)m_GrainLookupRT);
		m_GrainLookupRT = null;
	}

	public override void Prepare(Material uberMaterial)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		GrainModel.Settings settings = base.model.settings;
		uberMaterial.EnableKeyword("GRAIN");
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		float value = Random.value;
		float value2 = Random.value;
		if ((Object)(object)m_GrainLookupRT == (Object)null || !m_GrainLookupRT.IsCreated())
		{
			GraphicsUtils.Destroy((Object)(object)m_GrainLookupRT);
			m_GrainLookupRT = new RenderTexture(192, 192, 0, (RenderTextureFormat)2)
			{
				filterMode = (FilterMode)1,
				wrapMode = (TextureWrapMode)0,
				anisoLevel = 0,
				name = "Grain Lookup Texture"
			};
			m_GrainLookupRT.Create();
		}
		Material val = context.materialFactory.Get("Hidden/Post FX/Grain Generator");
		val.SetFloat(Uniforms._Phase, realtimeSinceStartup / 20f);
		Graphics.Blit((Texture)null, m_GrainLookupRT, val, settings.colored ? 1 : 0);
		uberMaterial.SetTexture(Uniforms._GrainTex, (Texture)(object)m_GrainLookupRT);
		uberMaterial.SetVector(Uniforms._Grain_Params1, Vector4.op_Implicit(new Vector2(settings.luminanceContribution, settings.intensity * 20f)));
		uberMaterial.SetVector(Uniforms._Grain_Params2, new Vector4((float)context.width / (float)((Texture)m_GrainLookupRT).width / settings.size, (float)context.height / (float)((Texture)m_GrainLookupRT).height / settings.size, value, value2));
	}
}
