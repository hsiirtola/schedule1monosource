namespace UnityEngine.PostProcessing;

public sealed class FxaaComponent : PostProcessingComponentRenderTexture<AntialiasingModel>
{
	private static class Uniforms
	{
		internal static readonly int _QualitySettings = Shader.PropertyToID("_QualitySettings");

		internal static readonly int _ConsoleSettings = Shader.PropertyToID("_ConsoleSettings");
	}

	public override bool active
	{
		get
		{
			if (base.model.enabled && base.model.settings.method == AntialiasingModel.Method.Fxaa)
			{
				return !context.interrupted;
			}
			return false;
		}
	}

	public void Render(RenderTexture source, RenderTexture destination)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		AntialiasingModel.FxaaSettings fxaaSettings = base.model.settings.fxaaSettings;
		Material val = context.materialFactory.Get("Hidden/Post FX/FXAA");
		AntialiasingModel.FxaaQualitySettings fxaaQualitySettings = AntialiasingModel.FxaaQualitySettings.presets[(int)fxaaSettings.preset];
		AntialiasingModel.FxaaConsoleSettings fxaaConsoleSettings = AntialiasingModel.FxaaConsoleSettings.presets[(int)fxaaSettings.preset];
		val.SetVector(Uniforms._QualitySettings, Vector4.op_Implicit(new Vector3(fxaaQualitySettings.subpixelAliasingRemovalAmount, fxaaQualitySettings.edgeDetectionThreshold, fxaaQualitySettings.minimumRequiredLuminance)));
		val.SetVector(Uniforms._ConsoleSettings, new Vector4(fxaaConsoleSettings.subpixelSpreadAmount, fxaaConsoleSettings.edgeSharpnessAmount, fxaaConsoleSettings.edgeDetectionThreshold, fxaaConsoleSettings.minimumRequiredLuminance));
		Graphics.Blit((Texture)(object)source, destination, val, 0);
	}
}
