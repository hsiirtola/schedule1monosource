using UnityEngine;

namespace Funly.SkyStudio;

[RequireComponent(typeof(Camera))]
public class WeatherDepthCamera : MonoBehaviour
{
	private Camera m_DepthCamera;

	[Tooltip("Shader used to render out depth + normal texture. This should be the sky studio depth shader.")]
	public Shader depthShader;

	[HideInInspector]
	public RenderTexture overheadDepthTexture;

	[Tooltip("You can help increase performance by only rendering periodically some number of frames.")]
	[Range(1f, 60f)]
	public int renderFrameInterval = 5;

	[Tooltip("The resolution of the texture. Higher resolution uses more rendering time but makes more precise weather along edges.")]
	[Range(128f, 8192f)]
	public int textureResolution = 1024;

	private void Start()
	{
		m_DepthCamera = ((Component)this).GetComponent<Camera>();
		((Behaviour)m_DepthCamera).enabled = false;
	}

	private void Update()
	{
		if (((Behaviour)m_DepthCamera).enabled)
		{
			((Behaviour)m_DepthCamera).enabled = false;
		}
		if (Time.frameCount % renderFrameInterval == 0)
		{
			RenderOverheadCamera();
		}
	}

	private void RenderOverheadCamera()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		PrepareRenderTexture();
		if ((Object)(object)depthShader == (Object)null)
		{
			Debug.LogError((object)"Can't render depth since depth shader is missing.");
			return;
		}
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = overheadDepthTexture;
		GL.Clear(true, true, Color.black);
		m_DepthCamera.RenderWithShader(depthShader, "RenderType");
		RenderTexture.active = active;
		Shader.SetGlobalTexture("_OverheadDepthTex", (Texture)(object)overheadDepthTexture);
		Shader.SetGlobalVector("_OverheadDepthPosition", Vector4.op_Implicit(((Component)m_DepthCamera).transform.position));
		Shader.SetGlobalFloat("_OverheadDepthNearClip", m_DepthCamera.nearClipPlane);
		Shader.SetGlobalFloat("_OverheadDepthFarClip", m_DepthCamera.farClipPlane);
	}

	private void PrepareRenderTexture()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		if ((Object)(object)overheadDepthTexture == (Object)null)
		{
			int num = Mathf.ClosestPowerOfTwo(Mathf.FloorToInt((float)textureResolution));
			RenderTextureFormat val = (RenderTextureFormat)0;
			overheadDepthTexture = new RenderTexture(num, num, 24, val, (RenderTextureReadWrite)1);
			overheadDepthTexture.useMipMap = false;
			overheadDepthTexture.autoGenerateMips = false;
			((Texture)overheadDepthTexture).filterMode = (FilterMode)0;
			overheadDepthTexture.antiAliasing = 2;
		}
		if (!overheadDepthTexture.IsCreated())
		{
			overheadDepthTexture.Create();
		}
		if ((Object)(object)m_DepthCamera.targetTexture != (Object)(object)overheadDepthTexture)
		{
			m_DepthCamera.targetTexture = overheadDepthTexture;
		}
	}
}
