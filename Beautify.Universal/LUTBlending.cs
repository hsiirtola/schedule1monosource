using UnityEngine;
using UnityEngine.Rendering;

namespace Beautify.Universal;

[ExecuteInEditMode]
public class LUTBlending : MonoBehaviour
{
	private static class ShaderParams
	{
		public static int LUT2 = Shader.PropertyToID("_LUT2");

		public static int Phase = Shader.PropertyToID("_Phase");
	}

	public Texture2D LUT1;

	public Texture2D LUT2;

	[Range(0f, 1f)]
	public float LUT1Intensity = 1f;

	[Range(0f, 1f)]
	public float LUT2Intensity = 1f;

	[Range(0f, 1f)]
	public float phase;

	public Shader lerpShader;

	private float oldPhase = -1f;

	private RenderTexture rt;

	private Material lerpMat;

	private void OnEnable()
	{
		UpdateBeautifyLUT();
	}

	private void OnValidate()
	{
		oldPhase = -1f;
		UpdateBeautifyLUT();
	}

	private void OnDestroy()
	{
		if ((Object)(object)rt != (Object)null)
		{
			rt.Release();
		}
	}

	private void LateUpdate()
	{
		UpdateBeautifyLUT();
	}

	private void UpdateBeautifyLUT()
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Expected O, but got Unknown
		if (oldPhase != phase && !((Object)(object)LUT1 == (Object)null) && !((Object)(object)LUT2 == (Object)null) && !((Object)(object)lerpShader == (Object)null))
		{
			oldPhase = phase;
			if ((Object)(object)rt == (Object)null)
			{
				rt = new RenderTexture(((Texture)LUT1).width, ((Texture)LUT1).height, 0, (RenderTextureFormat)0, (RenderTextureReadWrite)1);
				((Texture)rt).filterMode = (FilterMode)0;
			}
			if ((Object)(object)lerpMat == (Object)null)
			{
				lerpMat = new Material(lerpShader);
			}
			lerpMat.SetTexture(ShaderParams.LUT2, (Texture)(object)LUT2);
			lerpMat.SetFloat(ShaderParams.Phase, phase);
			Graphics.Blit((Texture)(object)LUT1, rt, lerpMat);
			((VolumeParameter<bool>)(object)BeautifySettings.settings.lut).Override(true);
			float num = Mathf.Lerp(LUT1Intensity, LUT2Intensity, phase);
			((VolumeParameter<float>)(object)BeautifySettings.settings.lutIntensity).Override(num);
			((VolumeParameter<Texture>)(object)BeautifySettings.settings.lutTexture).Override((Texture)(object)rt);
		}
	}
}
