using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace VLB;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[SelectionBase]
[HelpURL("http://saladgamer.com/vlb-doc/comp-lightbeam-sd/")]
public class VolumetricLightBeamSD : VolumetricLightBeamAbstractBase
{
	public delegate void OnWillCameraRenderCB(Camera cam);

	public delegate void OnBeamGeometryInitialized();

	public new const string ClassName = "VolumetricLightBeamSD";

	public bool colorFromLight = true;

	public ColorMode colorMode;

	[ColorUsage(false, true)]
	[FormerlySerializedAs("colorValue")]
	public Color color = Consts.Beam.FlatColor;

	public Gradient colorGradient;

	public bool intensityFromLight = true;

	public bool intensityModeAdvanced;

	[FormerlySerializedAs("alphaInside")]
	[Min(0f)]
	public float intensityInside = 1f;

	[FormerlySerializedAs("alphaOutside")]
	[FormerlySerializedAs("alpha")]
	[Min(0f)]
	public float intensityOutside = 1f;

	[Min(0f)]
	public float intensityMultiplier = 1f;

	[Range(0f, 1f)]
	public float hdrpExposureWeight;

	public BlendingMode blendingMode;

	[FormerlySerializedAs("angleFromLight")]
	public bool spotAngleFromLight = true;

	[Range(0.1f, 179.9f)]
	public float spotAngle = 35f;

	[Min(0f)]
	public float spotAngleMultiplier = 1f;

	[FormerlySerializedAs("radiusStart")]
	public float coneRadiusStart = 0.1f;

	public ShaderAccuracy shaderAccuracy;

	public MeshType geomMeshType;

	[FormerlySerializedAs("geomSides")]
	public int geomCustomSides = 18;

	public int geomCustomSegments = 5;

	public Vector3 skewingLocalForwardDirection = Consts.Beam.SD.SkewingLocalForwardDirectionDefault;

	public Transform clippingPlaneTransform;

	public bool geomCap;

	public AttenuationEquation attenuationEquation = AttenuationEquation.Quadratic;

	[Range(0f, 1f)]
	public float attenuationCustomBlending = 0.5f;

	[FormerlySerializedAs("fadeStart")]
	public float fallOffStart;

	[FormerlySerializedAs("fadeEnd")]
	public float fallOffEnd = 3f;

	[FormerlySerializedAs("fadeEndFromLight")]
	public bool fallOffEndFromLight = true;

	[Min(0f)]
	public float fallOffEndMultiplier = 1f;

	public float depthBlendDistance = 2f;

	public float cameraClippingDistance = 0.5f;

	[Range(0f, 1f)]
	public float glareFrontal = 0.5f;

	[Range(0f, 1f)]
	public float glareBehind = 0.5f;

	[FormerlySerializedAs("fresnelPowOutside")]
	public float fresnelPow = 8f;

	public NoiseMode noiseMode;

	[Range(0f, 1f)]
	public float noiseIntensity = 0.5f;

	public bool noiseScaleUseGlobal = true;

	[Range(0.01f, 2f)]
	public float noiseScaleLocal = 0.5f;

	public bool noiseVelocityUseGlobal = true;

	public Vector3 noiseVelocityLocal = Consts.Beam.NoiseVelocityDefault;

	public Dimensions dimensions;

	public Vector2 tiltFactor = Consts.Beam.SD.TiltDefault;

	private MaterialManager.SD.DynamicOcclusion m_INTERNAL_DynamicOcclusionMode;

	private bool m_INTERNAL_DynamicOcclusionMode_Runtime;

	private OnBeamGeometryInitialized m_OnBeamGeometryInitialized;

	[FormerlySerializedAs("trackChangesDuringPlaytime")]
	[SerializeField]
	private bool _TrackChangesDuringPlaytime;

	[SerializeField]
	private int _SortingLayerID;

	[SerializeField]
	private int _SortingOrder;

	[FormerlySerializedAs("fadeOutBegin")]
	[SerializeField]
	private float _FadeOutBegin = -150f;

	[FormerlySerializedAs("fadeOutEnd")]
	[SerializeField]
	private float _FadeOutEnd = -200f;

	private BeamGeometrySD m_BeamGeom;

	private Coroutine m_CoPlaytimeUpdate;

	public ColorMode usedColorMode
	{
		get
		{
			if (Config.Instance.featureEnabledColorGradient == FeatureEnabledColorGradient.Off)
			{
				return ColorMode.Flat;
			}
			return colorMode;
		}
	}

	private bool useColorFromAttachedLightSpot
	{
		get
		{
			if (colorFromLight)
			{
				return (Object)(object)base.lightSpotAttached != (Object)null;
			}
			return false;
		}
	}

	private bool useColorTemperatureFromAttachedLightSpot
	{
		get
		{
			if (useColorFromAttachedLightSpot && base.lightSpotAttached.useColorTemperature)
			{
				return Config.Instance.useLightColorTemperature;
			}
			return false;
		}
	}

	[Obsolete("Use 'intensityGlobal' or 'intensityInside' instead")]
	public float alphaInside
	{
		get
		{
			return intensityInside;
		}
		set
		{
			intensityInside = value;
		}
	}

	[Obsolete("Use 'intensityGlobal' or 'intensityOutside' instead")]
	public float alphaOutside
	{
		get
		{
			return intensityOutside;
		}
		set
		{
			intensityOutside = value;
		}
	}

	public float intensityGlobal
	{
		get
		{
			return intensityOutside;
		}
		set
		{
			intensityInside = value;
			intensityOutside = value;
		}
	}

	public bool useIntensityFromAttachedLightSpot
	{
		get
		{
			if (intensityFromLight)
			{
				return (Object)(object)base.lightSpotAttached != (Object)null;
			}
			return false;
		}
	}

	public bool useSpotAngleFromAttachedLightSpot
	{
		get
		{
			if (spotAngleFromLight)
			{
				return (Object)(object)base.lightSpotAttached != (Object)null;
			}
			return false;
		}
	}

	public float coneAngle => Mathf.Atan2(coneRadiusEnd - coneRadiusStart, maxGeometryDistance) * 57.29578f * 2f;

	public float coneRadiusEnd
	{
		get
		{
			return Utils.ComputeConeRadiusEnd(maxGeometryDistance, spotAngle);
		}
		set
		{
			spotAngle = Utils.ComputeSpotAngle(maxGeometryDistance, value);
		}
	}

	public float coneVolume
	{
		get
		{
			float num = coneRadiusStart;
			float num2 = coneRadiusEnd;
			return (float)Math.PI / 3f * (num * num + num * num2 + num2 * num2) * fallOffEnd;
		}
	}

	public float coneApexOffsetZ
	{
		get
		{
			float num = coneRadiusStart / coneRadiusEnd;
			if (num != 1f)
			{
				return maxGeometryDistance * num / (1f - num);
			}
			return float.MaxValue;
		}
	}

	public Vector3 coneApexPositionLocal => new Vector3(0f, 0f, 0f - coneApexOffsetZ);

	public Vector3 coneApexPositionGlobal
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			Matrix4x4 localToWorldMatrix = ((Component)this).transform.localToWorldMatrix;
			return ((Matrix4x4)(ref localToWorldMatrix)).MultiplyPoint(coneApexPositionLocal);
		}
	}

	public int geomSides
	{
		get
		{
			if (geomMeshType != MeshType.Custom)
			{
				return Config.Instance.sharedMeshSides;
			}
			return geomCustomSides;
		}
		set
		{
			geomCustomSides = value;
			Debug.LogWarningFormat("The setter VLB.{0}.geomSides is OBSOLETE and has been renamed to geomCustomSides.", new object[1] { "VolumetricLightBeamSD" });
		}
	}

	public int geomSegments
	{
		get
		{
			if (geomMeshType != MeshType.Custom)
			{
				return Config.Instance.sharedMeshSegments;
			}
			return geomCustomSegments;
		}
		set
		{
			geomCustomSegments = value;
			Debug.LogWarningFormat("The setter VLB.{0}.geomSegments is OBSOLETE and has been renamed to geomCustomSegments.", new object[1] { "VolumetricLightBeamSD" });
		}
	}

	public Vector3 skewingLocalForwardDirectionNormalized
	{
		get
		{
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			if (Mathf.Approximately(skewingLocalForwardDirection.z, 0f))
			{
				Debug.LogErrorFormat("Beam {0} has a skewingLocalForwardDirection with a null Z, which is forbidden", new object[1] { ((Object)this).name });
				return Vector3.forward;
			}
			return ((Vector3)(ref skewingLocalForwardDirection)).normalized;
		}
	}

	public bool canHaveMeshSkewing => geomMeshType == MeshType.Custom;

	public bool hasMeshSkewing
	{
		get
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			if (!Config.Instance.featureEnabledMeshSkewing)
			{
				return false;
			}
			if (!canHaveMeshSkewing)
			{
				return false;
			}
			if (Mathf.Approximately(Vector3.Dot(skewingLocalForwardDirectionNormalized, Vector3.forward), 1f))
			{
				return false;
			}
			return true;
		}
	}

	public Vector4 additionalClippingPlane
	{
		get
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)clippingPlaneTransform == (Object)null))
			{
				return Utils.PlaneEquation(clippingPlaneTransform.forward, clippingPlaneTransform.position);
			}
			return Vector4.zero;
		}
	}

	public float attenuationLerpLinearQuad
	{
		get
		{
			if (attenuationEquation == AttenuationEquation.Linear)
			{
				return 0f;
			}
			if (attenuationEquation == AttenuationEquation.Quadratic)
			{
				return 1f;
			}
			return attenuationCustomBlending;
		}
	}

	[Obsolete("Use 'fallOffStart' instead")]
	public float fadeStart
	{
		get
		{
			return fallOffStart;
		}
		set
		{
			fallOffStart = value;
		}
	}

	[Obsolete("Use 'fallOffEnd' instead")]
	public float fadeEnd
	{
		get
		{
			return fallOffEnd;
		}
		set
		{
			fallOffEnd = value;
		}
	}

	[Obsolete("Use 'fallOffEndFromLight' instead")]
	public bool fadeEndFromLight
	{
		get
		{
			return fallOffEndFromLight;
		}
		set
		{
			fallOffEndFromLight = value;
		}
	}

	public bool useFallOffEndFromAttachedLightSpot
	{
		get
		{
			if (fallOffEndFromLight)
			{
				return (Object)(object)base.lightSpotAttached != (Object)null;
			}
			return false;
		}
	}

	public float maxGeometryDistance => fallOffEnd + Mathf.Max(Mathf.Abs(tiltFactor.x), Mathf.Abs(tiltFactor.y));

	public bool isNoiseEnabled => noiseMode != NoiseMode.Disabled;

	[Obsolete("Use 'noiseMode' instead")]
	public bool noiseEnabled
	{
		get
		{
			return isNoiseEnabled;
		}
		set
		{
			noiseMode = (value ? NoiseMode.WorldSpace : NoiseMode.Disabled);
		}
	}

	public float fadeOutBegin
	{
		get
		{
			return _FadeOutBegin;
		}
		set
		{
			SetFadeOutValue(ref _FadeOutBegin, value);
		}
	}

	public float fadeOutEnd
	{
		get
		{
			return _FadeOutEnd;
		}
		set
		{
			SetFadeOutValue(ref _FadeOutEnd, value);
		}
	}

	public bool isFadeOutEnabled
	{
		get
		{
			if (_FadeOutBegin >= 0f)
			{
				return _FadeOutEnd >= 0f;
			}
			return false;
		}
	}

	public bool isTilted => !tiltFactor.Approximately(Vector2.zero);

	public int sortingLayerID
	{
		get
		{
			return _SortingLayerID;
		}
		set
		{
			_SortingLayerID = value;
			if (Object.op_Implicit((Object)(object)m_BeamGeom))
			{
				m_BeamGeom.sortingLayerID = value;
			}
		}
	}

	public string sortingLayerName
	{
		get
		{
			return SortingLayer.IDToName(sortingLayerID);
		}
		set
		{
			sortingLayerID = SortingLayer.NameToID(value);
		}
	}

	public int sortingOrder
	{
		get
		{
			return _SortingOrder;
		}
		set
		{
			_SortingOrder = value;
			if (Object.op_Implicit((Object)(object)m_BeamGeom))
			{
				m_BeamGeom.sortingOrder = value;
			}
		}
	}

	public bool trackChangesDuringPlaytime
	{
		get
		{
			return _TrackChangesDuringPlaytime;
		}
		set
		{
			_TrackChangesDuringPlaytime = value;
			StartPlaytimeUpdateIfNeeded();
		}
	}

	public bool isCurrentlyTrackingChanges => m_CoPlaytimeUpdate != null;

	public int blendingModeAsInt => Mathf.Clamp((int)blendingMode, 0, Enum.GetValues(typeof(BlendingMode)).Length);

	public Quaternion beamInternalLocalRotation
	{
		get
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			if (dimensions != Dimensions.Dim3D)
			{
				return Quaternion.LookRotation(Vector3.right, Vector3.up);
			}
			return Quaternion.identity;
		}
	}

	public Vector3 beamLocalForward
	{
		get
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			if (dimensions != Dimensions.Dim3D)
			{
				return Vector3.right;
			}
			return Vector3.forward;
		}
	}

	public Vector3 beamGlobalForward => ((Component)this).transform.TransformDirection(beamLocalForward);

	public float raycastDistance
	{
		get
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			if (!hasMeshSkewing)
			{
				return maxGeometryDistance;
			}
			float z = skewingLocalForwardDirectionNormalized.z;
			if (!Mathf.Approximately(z, 0f))
			{
				return maxGeometryDistance / z;
			}
			return maxGeometryDistance;
		}
	}

	public Vector3 raycastGlobalForward => ComputeRaycastGlobalVector(hasMeshSkewing ? skewingLocalForwardDirectionNormalized : Vector3.forward);

	public Vector3 raycastGlobalUp => ComputeRaycastGlobalVector(Vector3.up);

	public Vector3 raycastGlobalRight => ComputeRaycastGlobalVector(Vector3.right);

	public MaterialManager.SD.DynamicOcclusion _INTERNAL_DynamicOcclusionMode
	{
		get
		{
			if (!Config.Instance.featureEnabledDynamicOcclusion)
			{
				return MaterialManager.SD.DynamicOcclusion.Off;
			}
			return m_INTERNAL_DynamicOcclusionMode;
		}
		set
		{
			m_INTERNAL_DynamicOcclusionMode = value;
		}
	}

	public MaterialManager.SD.DynamicOcclusion _INTERNAL_DynamicOcclusionMode_Runtime
	{
		get
		{
			if (!m_INTERNAL_DynamicOcclusionMode_Runtime)
			{
				return MaterialManager.SD.DynamicOcclusion.Off;
			}
			return _INTERNAL_DynamicOcclusionMode;
		}
	}

	public uint _INTERNAL_InstancedMaterialGroupID { get; protected set; }

	public string meshStats
	{
		get
		{
			Mesh val = (Object.op_Implicit((Object)(object)m_BeamGeom) ? m_BeamGeom.coneMesh : null);
			if (Object.op_Implicit((Object)(object)val))
			{
				return $"Cone angle: {coneAngle:0.0} degrees\nMesh: {val.vertexCount} vertices, {val.triangles.Length / 3} triangles";
			}
			return "no mesh available";
		}
	}

	public int meshVerticesCount
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)m_BeamGeom) || !Object.op_Implicit((Object)(object)m_BeamGeom.coneMesh))
			{
				return 0;
			}
			return m_BeamGeom.coneMesh.vertexCount;
		}
	}

	public int meshTrianglesCount
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)m_BeamGeom) || !Object.op_Implicit((Object)(object)m_BeamGeom.coneMesh))
			{
				return 0;
			}
			return m_BeamGeom.coneMesh.triangles.Length / 3;
		}
	}

	public event OnWillCameraRenderCB onWillCameraRenderThisBeam;

	public void GetInsideAndOutsideIntensity(out float inside, out float outside)
	{
		if (intensityModeAdvanced)
		{
			inside = intensityInside;
			outside = intensityOutside;
		}
		else
		{
			inside = (outside = intensityOutside);
		}
	}

	public override bool IsScalable()
	{
		return true;
	}

	public override BeamGeometryAbstractBase GetBeamGeometry()
	{
		return m_BeamGeom;
	}

	protected override void SetBeamGeometryNull()
	{
		m_BeamGeom = null;
	}

	public override Vector3 GetLossyScale()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (dimensions != Dimensions.Dim3D)
		{
			return new Vector3(((Component)this).transform.lossyScale.z, ((Component)this).transform.lossyScale.y, ((Component)this).transform.lossyScale.x);
		}
		return ((Component)this).transform.lossyScale;
	}

	private Vector3 ComputeRaycastGlobalVector(Vector3 localVec)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.rotation * beamInternalLocalRotation * localVec;
	}

	public void _INTERNAL_SetDynamicOcclusionCallback(string shaderKeyword, MaterialModifier.Callback cb)
	{
		m_INTERNAL_DynamicOcclusionMode_Runtime = cb != null;
		if (Object.op_Implicit((Object)(object)m_BeamGeom))
		{
			m_BeamGeom.SetDynamicOcclusionCallback(shaderKeyword, cb);
		}
	}

	public void _INTERNAL_OnWillCameraRenderThisBeam(Camera cam)
	{
		if (this.onWillCameraRenderThisBeam != null)
		{
			this.onWillCameraRenderThisBeam(cam);
		}
	}

	public void RegisterOnBeamGeometryInitializedCallback(OnBeamGeometryInitialized cb)
	{
		m_OnBeamGeometryInitialized = (OnBeamGeometryInitialized)Delegate.Combine(m_OnBeamGeometryInitialized, cb);
		if (Object.op_Implicit((Object)(object)m_BeamGeom))
		{
			CallOnBeamGeometryInitializedCallback();
		}
	}

	private void CallOnBeamGeometryInitializedCallback()
	{
		if (m_OnBeamGeometryInitialized != null)
		{
			m_OnBeamGeometryInitialized();
			m_OnBeamGeometryInitialized = null;
		}
	}

	private void SetFadeOutValue(ref float propToChange, float value)
	{
		bool flag = isFadeOutEnabled;
		propToChange = value;
		if (isFadeOutEnabled != flag)
		{
			OnFadeOutStateChanged();
		}
	}

	private void OnFadeOutStateChanged()
	{
		if (isFadeOutEnabled && Object.op_Implicit((Object)(object)m_BeamGeom))
		{
			m_BeamGeom.RestartFadeOutCoroutine();
		}
	}

	public float GetInsideBeamFactor(Vector3 posWS)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return GetInsideBeamFactorFromObjectSpacePos(((Component)this).transform.InverseTransformPoint(posWS));
	}

	public float GetInsideBeamFactorFromObjectSpacePos(Vector3 posOS)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (dimensions == Dimensions.Dim2D)
		{
			((Vector3)(ref posOS))._002Ector(posOS.z, posOS.y, posOS.x);
		}
		if (posOS.z < 0f)
		{
			return -1f;
		}
		Vector2 val = posOS.xy();
		if (hasMeshSkewing)
		{
			Vector3 val2 = skewingLocalForwardDirectionNormalized;
			val -= val2.xy() * (posOS.z / val2.z);
		}
		Vector2 val3 = new Vector2(((Vector2)(ref val)).magnitude, posOS.z + coneApexOffsetZ);
		Vector2 normalized = ((Vector2)(ref val3)).normalized;
		return Mathf.Clamp((Mathf.Abs(Mathf.Sin(coneAngle * ((float)Math.PI / 180f) / 2f)) - Mathf.Abs(normalized.x)) / 0.1f, -1f, 1f);
	}

	[Obsolete("Use 'GenerateGeometry()' instead")]
	public void Generate()
	{
		GenerateGeometry();
	}

	public virtual void GenerateGeometry()
	{
		HandleBackwardCompatibility(pluginVersion, 20100);
		pluginVersion = 20100;
		ValidateProperties();
		if ((Object)(object)m_BeamGeom == (Object)null)
		{
			m_BeamGeom = Utils.NewWithComponent<BeamGeometrySD>("Beam Geometry");
			m_BeamGeom.Initialize(this);
			CallOnBeamGeometryInitializedCallback();
		}
		m_BeamGeom.RegenerateMesh(((Behaviour)this).enabled);
	}

	public virtual void UpdateAfterManualPropertyChange()
	{
		ValidateProperties();
		if (Object.op_Implicit((Object)(object)m_BeamGeom))
		{
			m_BeamGeom.UpdateMaterialAndBounds();
		}
	}

	private void Start()
	{
		InitLightSpotAttachedCached();
		GenerateGeometry();
	}

	private void OnEnable()
	{
		if (Object.op_Implicit((Object)(object)m_BeamGeom))
		{
			m_BeamGeom.OnMasterEnable();
		}
		StartPlaytimeUpdateIfNeeded();
	}

	private void OnDisable()
	{
		if (Object.op_Implicit((Object)(object)m_BeamGeom))
		{
			m_BeamGeom.OnMasterDisable();
		}
		m_CoPlaytimeUpdate = null;
	}

	private void StartPlaytimeUpdateIfNeeded()
	{
		if (Application.isPlaying && trackChangesDuringPlaytime && m_CoPlaytimeUpdate == null)
		{
			m_CoPlaytimeUpdate = ((MonoBehaviour)this).StartCoroutine(CoPlaytimeUpdate());
		}
	}

	private IEnumerator CoPlaytimeUpdate()
	{
		while (trackChangesDuringPlaytime && ((Behaviour)this).enabled)
		{
			UpdateAfterManualPropertyChange();
			yield return null;
		}
		m_CoPlaytimeUpdate = null;
	}

	private void AssignPropertiesFromAttachedSpotLight()
	{
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		Light val = base.lightSpotAttached;
		if (!Object.op_Implicit((Object)(object)val))
		{
			return;
		}
		if (intensityFromLight)
		{
			intensityModeAdvanced = false;
			intensityGlobal = SpotLightHelper.GetIntensity(val) * intensityMultiplier;
		}
		if (fallOffEndFromLight)
		{
			fallOffEnd = SpotLightHelper.GetFallOffEnd(val) * fallOffEndMultiplier;
		}
		if (spotAngleFromLight)
		{
			spotAngle = Mathf.Clamp(SpotLightHelper.GetSpotAngle(val) * spotAngleMultiplier, 0.1f, 179.9f);
		}
		if (colorFromLight)
		{
			colorMode = ColorMode.Flat;
			if (useColorTemperatureFromAttachedLightSpot)
			{
				Color val2 = Mathf.CorrelatedColorTemperatureToRGB(val.colorTemperature);
				Color val3 = val.color;
				Color val4 = ((Color)(ref val3)).linear * val2;
				color = ((Color)(ref val4)).gamma;
			}
			else
			{
				color = val.color;
			}
		}
	}

	private void ClampProperties()
	{
		intensityInside = Mathf.Max(intensityInside, 0f);
		intensityOutside = Mathf.Max(intensityOutside, 0f);
		intensityMultiplier = Mathf.Max(intensityMultiplier, 0f);
		attenuationCustomBlending = Mathf.Clamp(attenuationCustomBlending, 0f, 1f);
		fallOffEnd = Mathf.Max(0.01f, fallOffEnd);
		fallOffStart = Mathf.Clamp(fallOffStart, 0f, fallOffEnd - 0.01f);
		fallOffEndMultiplier = Mathf.Max(fallOffEndMultiplier, 0f);
		spotAngle = Mathf.Clamp(spotAngle, 0.1f, 179.9f);
		spotAngleMultiplier = Mathf.Max(spotAngleMultiplier, 0f);
		coneRadiusStart = Mathf.Max(coneRadiusStart, 0f);
		depthBlendDistance = Mathf.Max(depthBlendDistance, 0f);
		cameraClippingDistance = Mathf.Max(cameraClippingDistance, 0f);
		geomCustomSides = Mathf.Clamp(geomCustomSides, 3, 256);
		geomCustomSegments = Mathf.Clamp(geomCustomSegments, 0, 64);
		fresnelPow = Mathf.Max(0f, fresnelPow);
		glareBehind = Mathf.Clamp(glareBehind, 0f, 1f);
		glareFrontal = Mathf.Clamp(glareFrontal, 0f, 1f);
		noiseIntensity = Mathf.Clamp(noiseIntensity, 0f, 1f);
	}

	private void ValidateProperties()
	{
		AssignPropertiesFromAttachedSpotLight();
		ClampProperties();
	}

	private void HandleBackwardCompatibility(int serializedVersion, int newVersion)
	{
		if (serializedVersion != -1 && serializedVersion != newVersion)
		{
			if (serializedVersion < 1301)
			{
				attenuationEquation = AttenuationEquation.Linear;
			}
			if (serializedVersion < 1501)
			{
				geomMeshType = MeshType.Custom;
				geomCustomSegments = 5;
			}
			if (serializedVersion < 1610)
			{
				intensityFromLight = false;
				intensityModeAdvanced = !Mathf.Approximately(intensityInside, intensityOutside);
			}
			if (serializedVersion < 1910 && !intensityModeAdvanced && !Mathf.Approximately(intensityInside, intensityOutside))
			{
				intensityInside = intensityOutside;
			}
			Utils.MarkCurrentSceneDirty();
		}
	}
}
