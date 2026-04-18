namespace UnityEngine.PostProcessing;

public sealed class UserLutComponent : PostProcessingComponentRenderTexture<UserLutModel>
{
	private static class Uniforms
	{
		internal static readonly int _UserLut = Shader.PropertyToID("_UserLut");

		internal static readonly int _UserLut_Params = Shader.PropertyToID("_UserLut_Params");
	}

	public override bool active
	{
		get
		{
			UserLutModel.Settings settings = base.model.settings;
			if (base.model.enabled && (Object)(object)settings.lut != (Object)null && settings.contribution > 0f && ((Texture)settings.lut).height == (int)Mathf.Sqrt((float)((Texture)settings.lut).width))
			{
				return !context.interrupted;
			}
			return false;
		}
	}

	public override void Prepare(Material uberMaterial)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		UserLutModel.Settings settings = base.model.settings;
		uberMaterial.EnableKeyword("USER_LUT");
		uberMaterial.SetTexture(Uniforms._UserLut, (Texture)(object)settings.lut);
		uberMaterial.SetVector(Uniforms._UserLut_Params, new Vector4(1f / (float)((Texture)settings.lut).width, 1f / (float)((Texture)settings.lut).height, (float)((Texture)settings.lut).height - 1f, settings.contribution));
	}

	public void OnGUI()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		UserLutModel.Settings settings = base.model.settings;
		Rect viewport = context.viewport;
		GUI.DrawTexture(new Rect(((Rect)(ref viewport)).x * (float)Screen.width + 8f, 8f, (float)((Texture)settings.lut).width, (float)((Texture)settings.lut).height), (Texture)(object)settings.lut);
	}
}
