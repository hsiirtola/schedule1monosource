namespace UnityEngine.PostProcessing;

public sealed class VignetteComponent : PostProcessingComponentRenderTexture<VignetteModel>
{
	private static class Uniforms
	{
		internal static readonly int _Vignette_Color = Shader.PropertyToID("_Vignette_Color");

		internal static readonly int _Vignette_Center = Shader.PropertyToID("_Vignette_Center");

		internal static readonly int _Vignette_Settings = Shader.PropertyToID("_Vignette_Settings");

		internal static readonly int _Vignette_Mask = Shader.PropertyToID("_Vignette_Mask");

		internal static readonly int _Vignette_Opacity = Shader.PropertyToID("_Vignette_Opacity");
	}

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

	public override void Prepare(Material uberMaterial)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		VignetteModel.Settings settings = base.model.settings;
		uberMaterial.SetColor(Uniforms._Vignette_Color, settings.color);
		if (settings.mode == VignetteModel.Mode.Classic)
		{
			uberMaterial.SetVector(Uniforms._Vignette_Center, Vector4.op_Implicit(settings.center));
			uberMaterial.EnableKeyword("VIGNETTE_CLASSIC");
			float num = (1f - settings.roundness) * 6f + settings.roundness;
			uberMaterial.SetVector(Uniforms._Vignette_Settings, new Vector4(settings.intensity * 3f, settings.smoothness * 5f, num, settings.rounded ? 1f : 0f));
		}
		else if (settings.mode == VignetteModel.Mode.Masked && (Object)(object)settings.mask != (Object)null && settings.opacity > 0f)
		{
			uberMaterial.EnableKeyword("VIGNETTE_MASKED");
			uberMaterial.SetTexture(Uniforms._Vignette_Mask, settings.mask);
			uberMaterial.SetFloat(Uniforms._Vignette_Opacity, settings.opacity);
		}
	}
}
