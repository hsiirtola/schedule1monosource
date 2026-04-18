using System;
using UnityEngine;

namespace VLB;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[RequireComponent(typeof(VolumetricLightBeamHD))]
[HelpURL("http://saladgamer.com/vlb-doc/comp-shadow-hd/")]
public class VolumetricShadowHD : MonoBehaviour
{
	private enum ProcessOcclusionSource
	{
		RenderLoop,
		OnEnable,
		EditorUpdate,
		User
	}

	public const string ClassName = "VolumetricShadowHD";

	[SerializeField]
	private float m_Strength = 1f;

	[SerializeField]
	private ShadowUpdateRate m_UpdateRate = ShadowUpdateRate.EveryXFrames;

	[SerializeField]
	private int m_WaitXFrames = 3;

	[SerializeField]
	private LayerMask m_LayerMask = Consts.Shadow.LayerMaskDefault;

	[SerializeField]
	private bool m_UseOcclusionCulling = true;

	[SerializeField]
	private int m_DepthMapResolution = 128;

	private VolumetricLightBeamHD m_Master;

	private TransformUtils.Packed m_TransformPacked;

	private int m_LastFrameRendered = int.MinValue;

	private Camera m_DepthCamera;

	private bool m_NeedToUpdateOcclusionNextFrame;

	public static bool _INTERNAL_ApplyRandomFrameOffset = true;

	public float strength
	{
		get
		{
			return m_Strength;
		}
		set
		{
			if (m_Strength != value)
			{
				m_Strength = value;
				SetDirty();
			}
		}
	}

	public ShadowUpdateRate updateRate
	{
		get
		{
			return m_UpdateRate;
		}
		set
		{
			m_UpdateRate = value;
		}
	}

	public int waitXFrames
	{
		get
		{
			return m_WaitXFrames;
		}
		set
		{
			m_WaitXFrames = value;
		}
	}

	public LayerMask layerMask
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_LayerMask;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			m_LayerMask = value;
			UpdateDepthCameraProperties();
		}
	}

	public bool useOcclusionCulling
	{
		get
		{
			return m_UseOcclusionCulling;
		}
		set
		{
			m_UseOcclusionCulling = value;
			UpdateDepthCameraProperties();
		}
	}

	public int depthMapResolution
	{
		get
		{
			return m_DepthMapResolution;
		}
		set
		{
			if ((Object)(object)m_DepthCamera != (Object)null && Application.isPlaying)
			{
				Debug.LogErrorFormat(Consts.Shadow.GetErrorChangeRuntimeDepthMapResolution(this), Array.Empty<object>());
			}
			m_DepthMapResolution = value;
		}
	}

	public int _INTERNAL_LastFrameRendered => m_LastFrameRendered;

	public void ProcessOcclusionManually()
	{
		ProcessOcclusion(ProcessOcclusionSource.User);
	}

	public void UpdateDepthCameraProperties()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)m_DepthCamera))
		{
			m_DepthCamera.cullingMask = LayerMask.op_Implicit(layerMask);
			m_DepthCamera.useOcclusionCulling = useOcclusionCulling;
		}
	}

	private void ProcessOcclusion(ProcessOcclusionSource source)
	{
		if (Config.Instance.featureEnabledShadow && (m_LastFrameRendered != Time.frameCount || !Application.isPlaying || source != ProcessOcclusionSource.OnEnable))
		{
			if (SRPHelper.IsUsingCustomRenderPipeline())
			{
				m_NeedToUpdateOcclusionNextFrame = true;
			}
			else
			{
				ProcessOcclusionInternal();
			}
			SetDirty();
			if (updateRate.HasFlag(ShadowUpdateRate.OnBeamMove))
			{
				m_TransformPacked = ((Component)this).transform.GetWorldPacked();
			}
			bool num = m_LastFrameRendered < 0;
			m_LastFrameRendered = Time.frameCount;
			if (num && _INTERNAL_ApplyRandomFrameOffset)
			{
				m_LastFrameRendered += Random.Range(0, waitXFrames);
			}
		}
	}

	public static void ApplyMaterialProperties(VolumetricShadowHD instance, BeamGeometryHD geom)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)instance) && ((Behaviour)instance).enabled)
		{
			geom.SetMaterialProp(ShaderProperties.HD.ShadowDepthTexture, (Texture)(object)instance.m_DepthCamera.targetTexture);
			Vector3 val = (instance.m_Master.scalable ? instance.m_Master.GetLossyScale() : Vector3.one);
			geom.SetMaterialProp(ShaderProperties.HD.ShadowProps, new Vector4(Mathf.Sign(val.x) * Mathf.Sign(val.z), Mathf.Sign(val.y), instance.m_Strength, instance.m_DepthCamera.orthographic ? 0f : 1f));
		}
		else
		{
			geom.SetMaterialProp(ShaderProperties.HD.ShadowDepthTexture, BeamGeometryHD.InvalidTexture.NoDepth);
		}
	}

	private void Awake()
	{
		m_Master = ((Component)this).GetComponent<VolumetricLightBeamHD>();
	}

	private void OnEnable()
	{
		OnValidateProperties();
		InstantiateOrActivateDepthCamera();
		OnBeamEnabled();
	}

	private void OnDisable()
	{
		if (Object.op_Implicit((Object)(object)m_DepthCamera))
		{
			((Component)m_DepthCamera).gameObject.SetActive(false);
		}
		SetDirty();
	}

	private void OnDestroy()
	{
		DestroyDepthCamera();
	}

	private void ProcessOcclusionInternal()
	{
		UpdateDepthCameraPropertiesAccordingToBeam();
		m_DepthCamera.Render();
	}

	private void OnBeamEnabled()
	{
		if (((Behaviour)this).enabled && !updateRate.HasFlag(ShadowUpdateRate.Never))
		{
			ProcessOcclusion(ProcessOcclusionSource.OnEnable);
		}
	}

	public void OnWillCameraRenderThisBeam(Camera cam, BeamGeometryHD beamGeom)
	{
		if (((Behaviour)this).enabled && (Object)(object)cam != (Object)null && ((Behaviour)cam).enabled && Time.frameCount != m_LastFrameRendered && updateRate != ShadowUpdateRate.Never)
		{
			bool flag = false;
			if (!flag && updateRate.HasFlag(ShadowUpdateRate.OnBeamMove) && !m_TransformPacked.IsSame(((Component)this).transform))
			{
				flag = true;
			}
			if (!flag && updateRate.HasFlag(ShadowUpdateRate.EveryXFrames) && Time.frameCount >= m_LastFrameRendered + waitXFrames)
			{
				flag = true;
			}
			if (flag)
			{
				ProcessOcclusion(ProcessOcclusionSource.RenderLoop);
			}
		}
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
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		Utils.SetupDepthCamera(m_DepthCamera, m_Master.GetConeApexOffsetZ(counterApplyScaleForUnscalableBeam: true), m_Master.maxGeometryDistance, m_Master.coneRadiusStart, m_Master.coneRadiusEnd, m_Master.beamLocalForward, m_Master.GetLossyScale(), m_Master.scalable, m_Master.beamInternalLocalRotation, shouldScaleMinNearClipPlane: false);
	}

	private void InstantiateOrActivateDepthCamera()
	{
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Expected O, but got Unknown
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
			UpdateDepthCameraProperties();
			m_DepthCamera.clearFlags = (CameraClearFlags)3;
			m_DepthCamera.depthTextureMode = (DepthTextureMode)1;
			m_DepthCamera.renderingPath = (RenderingPath)1;
			((Object)((Component)m_DepthCamera).gameObject).hideFlags = Consts.Internal.ProceduralObjectsHideFlags;
			((Component)m_DepthCamera).transform.SetParent(((Component)this).transform, false);
			Config.Instance.SetURPScriptableRendererIndexToDepthCamera(m_DepthCamera);
			RenderTexture targetTexture = new RenderTexture(depthMapResolution, depthMapResolution, 16, (RenderTextureFormat)1);
			m_DepthCamera.targetTexture = targetTexture;
			UpdateDepthCameraPropertiesAccordingToBeam();
		}
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

	private void OnValidateProperties()
	{
		m_WaitXFrames = Mathf.Clamp(m_WaitXFrames, 1, 60);
		m_DepthMapResolution = Mathf.Clamp(Mathf.NextPowerOfTwo(m_DepthMapResolution), 8, 2048);
	}

	private void SetDirty()
	{
		if (Object.op_Implicit((Object)(object)m_Master))
		{
			m_Master.SetPropertyDirty(DirtyProps.ShadowProps);
		}
	}
}
