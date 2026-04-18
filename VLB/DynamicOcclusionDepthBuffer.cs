using System;
using UnityEngine;

namespace VLB;

[ExecuteInEditMode]
[HelpURL("http://saladgamer.com/vlb-doc/comp-dynocclusion-sd-depthbuffer/")]
public class DynamicOcclusionDepthBuffer : DynamicOcclusionAbstractBase
{
	public new const string ClassName = "DynamicOcclusionDepthBuffer";

	public LayerMask layerMask = Consts.DynOcclusion.LayerMaskDefault;

	public bool useOcclusionCulling = true;

	public int depthMapResolution = 128;

	public float fadeDistanceToSurface;

	private Camera m_DepthCamera;

	private bool m_NeedToUpdateOcclusionNextFrame;

	protected override string GetShaderKeyword()
	{
		return "VLB_OCCLUSION_DEPTH_TEXTURE";
	}

	protected override MaterialManager.SD.DynamicOcclusion GetDynamicOcclusionMode()
	{
		return MaterialManager.SD.DynamicOcclusion.DepthTexture;
	}

	private void ProcessOcclusionInternal()
	{
		UpdateDepthCameraPropertiesAccordingToBeam();
		m_DepthCamera.Render();
	}

	protected override bool OnProcessOcclusion(ProcessOcclusionSource source)
	{
		if (SRPHelper.IsUsingCustomRenderPipeline())
		{
			m_NeedToUpdateOcclusionNextFrame = true;
		}
		else
		{
			ProcessOcclusionInternal();
		}
		return true;
	}

	private void Update()
	{
		if (m_NeedToUpdateOcclusionNextFrame && Object.op_Implicit((Object)(object)m_Master) && Object.op_Implicit((Object)(object)m_DepthCamera) && Time.frameCount > 1)
		{
			ProcessOcclusionInternal();
			m_NeedToUpdateOcclusionNextFrame = false;
		}
	}

	private void UpdateDepthCameraPropertiesAccordingToBeam()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		Utils.SetupDepthCamera(m_DepthCamera, m_Master.coneApexOffsetZ, m_Master.maxGeometryDistance, m_Master.coneRadiusStart, m_Master.coneRadiusEnd, m_Master.beamLocalForward, m_Master.GetLossyScale(), m_Master.IsScalable(), m_Master.beamInternalLocalRotation, shouldScaleMinNearClipPlane: true);
	}

	public bool HasLayerMaskIssues()
	{
		if (Config.Instance.geometryOverrideLayer)
		{
			int num = 1 << Config.Instance.geometryLayerID;
			return (((LayerMask)(ref layerMask)).value & num) == num;
		}
		return false;
	}

	protected override void OnValidateProperties()
	{
		base.OnValidateProperties();
		depthMapResolution = Mathf.Clamp(Mathf.NextPowerOfTwo(depthMapResolution), 8, 2048);
		fadeDistanceToSurface = Mathf.Max(fadeDistanceToSurface, 0f);
	}

	private void InstantiateOrActivateDepthCamera()
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Expected O, but got Unknown
		if ((Object)(object)m_DepthCamera != (Object)null)
		{
			((Component)m_DepthCamera).gameObject.SetActive(true);
			return;
		}
		((Component)this).gameObject.ForeachComponentsInDirectChildrenOnly<Camera>((Action<Camera>)delegate(Camera cam)
		{
			Object.DestroyImmediate((Object)(object)((Component)cam).gameObject);
		}, true);
		m_DepthCamera = Utils.NewWithComponent<Camera>("Depth Camera");
		if (Object.op_Implicit((Object)(object)m_DepthCamera) && Object.op_Implicit((Object)(object)m_Master))
		{
			((Behaviour)m_DepthCamera).enabled = false;
			m_DepthCamera.cullingMask = LayerMask.op_Implicit(layerMask);
			m_DepthCamera.clearFlags = (CameraClearFlags)3;
			m_DepthCamera.depthTextureMode = (DepthTextureMode)1;
			m_DepthCamera.renderingPath = (RenderingPath)0;
			m_DepthCamera.useOcclusionCulling = useOcclusionCulling;
			((Object)((Component)m_DepthCamera).gameObject).hideFlags = Consts.Internal.ProceduralObjectsHideFlags;
			((Component)m_DepthCamera).transform.SetParent(((Component)this).transform, false);
			Config.Instance.SetURPScriptableRendererIndexToDepthCamera(m_DepthCamera);
			RenderTexture targetTexture = new RenderTexture(depthMapResolution, depthMapResolution, 16, (RenderTextureFormat)1);
			m_DepthCamera.targetTexture = targetTexture;
			UpdateDepthCameraPropertiesAccordingToBeam();
		}
	}

	protected override void OnEnablePostValidate()
	{
		InstantiateOrActivateDepthCamera();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (Object.op_Implicit((Object)(object)m_DepthCamera))
		{
			((Component)m_DepthCamera).gameObject.SetActive(false);
		}
	}

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		DestroyDepthCamera();
	}

	private void DestroyDepthCamera()
	{
		if (Object.op_Implicit((Object)(object)m_DepthCamera))
		{
			if (Object.op_Implicit((Object)(object)m_DepthCamera.targetTexture))
			{
				m_DepthCamera.targetTexture.Release();
				Object.DestroyImmediate((Object)(object)m_DepthCamera.targetTexture);
				m_DepthCamera.targetTexture = null;
			}
			Object.DestroyImmediate((Object)(object)((Component)m_DepthCamera).gameObject);
			m_DepthCamera = null;
		}
	}

	protected override void OnModifyMaterialCallback(MaterialModifier.Interface owner)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		owner.SetMaterialProp(ShaderProperties.SD.DynamicOcclusionDepthTexture, (Texture)(object)m_DepthCamera.targetTexture);
		Vector3 lossyScale = m_Master.GetLossyScale();
		owner.SetMaterialProp(ShaderProperties.SD.DynamicOcclusionDepthProps, new Vector4(Mathf.Sign(lossyScale.x) * Mathf.Sign(lossyScale.z), Mathf.Sign(lossyScale.y), fadeDistanceToSurface, m_DepthCamera.orthographic ? 0f : 1f));
	}
}
