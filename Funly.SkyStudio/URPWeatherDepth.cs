using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Funly.SkyStudio;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(UniversalAdditionalCameraData))]
public class URPWeatherDepth : MonoBehaviour
{
	public RenderTexture renderTexture;

	private Camera m_Camera;

	private UniversalAdditionalCameraData m_CameraData;

	private void Start()
	{
		m_Camera = ((Component)this).GetComponent<Camera>();
		m_CameraData = ((Component)this).GetComponent<UniversalAdditionalCameraData>();
	}

	private void Update()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		m_CameraData.SetRenderer(1);
		Shader.SetGlobalTexture("_OverheadDepthTex", (Texture)(object)renderTexture);
		Shader.SetGlobalVector("_OverheadDepthPosition", Vector4.op_Implicit(((Component)m_Camera).transform.position));
		Shader.SetGlobalFloat("_OverheadDepthNearClip", m_Camera.nearClipPlane);
		Shader.SetGlobalFloat("_OverheadDepthFarClip", m_Camera.farClipPlane);
	}
}
