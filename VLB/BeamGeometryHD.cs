using UnityEngine;
using UnityEngine.Rendering;

namespace VLB;

[AddComponentMenu("")]
[ExecuteInEditMode]
[HelpURL("http://saladgamer.com/vlb-doc/comp-lightbeam-hd/")]
public class BeamGeometryHD : BeamGeometryAbstractBase
{
	public enum InvalidTexture
	{
		Null,
		NoDepth
	}

	private VolumetricLightBeamHD m_Master;

	private VolumetricCookieHD m_Cookie;

	private VolumetricShadowHD m_Shadow;

	private Camera m_CurrentCameraRenderingSRP;

	private DirtyProps m_DirtyProps;

	public bool visible
	{
		set
		{
			if (Object.op_Implicit((Object)(object)base.meshRenderer))
			{
				((Renderer)base.meshRenderer).enabled = value;
			}
		}
	}

	public int sortingLayerID
	{
		set
		{
			if (Object.op_Implicit((Object)(object)base.meshRenderer))
			{
				((Renderer)base.meshRenderer).sortingLayerID = value;
			}
		}
	}

	public int sortingOrder
	{
		set
		{
			if (Object.op_Implicit((Object)(object)base.meshRenderer))
			{
				((Renderer)base.meshRenderer).sortingOrder = value;
			}
		}
	}

	public static bool isCustomRenderPipelineSupported => true;

	private bool shouldUseGPUInstancedMaterial
	{
		get
		{
			if (Config.Instance.GetActualRenderingMode(ShaderMode.HD) == RenderingMode.GPUInstancing)
			{
				if ((Object)(object)m_Cookie == (Object)null)
				{
					return (Object)(object)m_Shadow == (Object)null;
				}
				return false;
			}
			return false;
		}
	}

	private bool isNoiseEnabled
	{
		get
		{
			if (m_Master.isNoiseEnabled && m_Master.noiseIntensity > 0f)
			{
				return Noise3D.isSupported;
			}
			return false;
		}
	}

	protected override VolumetricLightBeamAbstractBase GetMaster()
	{
		return m_Master;
	}

	private void OnDisable()
	{
		SRPHelper.UnregisterOnBeginCameraRendering(OnBeginCameraRenderingSRP);
		m_CurrentCameraRenderingSRP = null;
	}

	private void OnEnable()
	{
		SRPHelper.RegisterOnBeginCameraRendering(OnBeginCameraRenderingSRP);
	}

	public void Initialize(VolumetricLightBeamHD master)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		HideFlags proceduralObjectsHideFlags = Consts.Internal.ProceduralObjectsHideFlags;
		m_Master = master;
		((Component)this).transform.SetParent(((Component)master).transform, false);
		base.meshRenderer = ((Component)this).gameObject.GetOrAddComponent<MeshRenderer>();
		((Object)base.meshRenderer).hideFlags = proceduralObjectsHideFlags;
		((Renderer)base.meshRenderer).shadowCastingMode = (ShadowCastingMode)0;
		((Renderer)base.meshRenderer).receiveShadows = false;
		((Renderer)base.meshRenderer).reflectionProbeUsage = (ReflectionProbeUsage)0;
		((Renderer)base.meshRenderer).lightProbeUsage = (LightProbeUsage)0;
		m_Cookie = m_Master.GetAdditionalComponentCookie();
		m_Shadow = m_Master.GetAdditionalComponentShadow();
		if (!shouldUseGPUInstancedMaterial)
		{
			m_CustomMaterial = Config.Instance.NewMaterialTransient(ShaderMode.HD, gpuInstanced: false);
			ApplyMaterial();
		}
		if (m_Master.DoesSupportSorting2D())
		{
			if (SortingLayer.IsValid(m_Master.GetSortingLayerID()))
			{
				sortingLayerID = m_Master.GetSortingLayerID();
			}
			else
			{
				Debug.LogError((object)$"Beam '{Utils.GetPath(((Component)m_Master).transform)}' has an invalid sortingLayerID ({m_Master.GetSortingLayerID()}). Please fix it by setting a valid layer.");
			}
			sortingOrder = m_Master.GetSortingOrder();
		}
		base.meshFilter = ((Component)this).gameObject.GetOrAddComponent<MeshFilter>();
		((Object)base.meshFilter).hideFlags = proceduralObjectsHideFlags;
		((Object)((Component)this).gameObject).hideFlags = proceduralObjectsHideFlags;
	}

	public void RegenerateMesh()
	{
		if (Config.Instance.geometryOverrideLayer)
		{
			((Component)this).gameObject.layer = Config.Instance.geometryLayerID;
		}
		else
		{
			((Component)this).gameObject.layer = ((Component)m_Master).gameObject.layer;
		}
		((Component)this).gameObject.tag = Config.Instance.geometryTag;
		base.coneMesh = GlobalMeshHD.Get();
		base.meshFilter.sharedMesh = base.coneMesh;
		UpdateMaterialAndBounds();
	}

	private Vector3 ComputeLocalMatrix()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Max(m_Master.coneRadiusStart, m_Master.coneRadiusEnd);
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(num, num, m_Master.maxGeometryDistance);
		if (!m_Master.scalable)
		{
			val = val.Divide(m_Master.GetLossyScale());
		}
		((Component)this).transform.localScale = val;
		((Component)this).transform.localRotation = m_Master.beamInternalLocalRotation;
		return val;
	}

	private MaterialManager.StaticPropertiesHD ComputeMaterialStaticProperties()
	{
		MaterialManager.ColorGradient colorGradient = MaterialManager.ColorGradient.Off;
		if (m_Master.colorMode == ColorMode.Gradient)
		{
			colorGradient = ((Utils.GetFloatPackingPrecision() != Utils.FloatPackingPrecision.High) ? MaterialManager.ColorGradient.MatrixLow : MaterialManager.ColorGradient.MatrixHigh);
		}
		return new MaterialManager.StaticPropertiesHD
		{
			blendingMode = (MaterialManager.BlendingMode)m_Master.blendingMode,
			attenuation = ((m_Master.attenuationEquation != AttenuationEquationHD.Linear) ? MaterialManager.HD.Attenuation.Quadratic : MaterialManager.HD.Attenuation.Linear),
			noise3D = (isNoiseEnabled ? MaterialManager.Noise3D.On : MaterialManager.Noise3D.Off),
			colorGradient = colorGradient,
			shadow = (((Object)(object)m_Shadow != (Object)null) ? MaterialManager.HD.Shadow.On : MaterialManager.HD.Shadow.Off),
			cookie = (((Object)(object)m_Cookie != (Object)null) ? ((m_Cookie.channel != CookieChannel.RGBA) ? MaterialManager.HD.Cookie.SingleChannel : MaterialManager.HD.Cookie.RGBA) : MaterialManager.HD.Cookie.Off),
			raymarchingQualityIndex = m_Master.raymarchingQualityIndex
		};
	}

	private bool ApplyMaterial()
	{
		MaterialManager.StaticPropertiesHD staticProps = ComputeMaterialStaticProperties();
		Material val = null;
		if (!shouldUseGPUInstancedMaterial)
		{
			val = m_CustomMaterial;
			if (Object.op_Implicit((Object)(object)val))
			{
				staticProps.ApplyToMaterial(val);
			}
		}
		else
		{
			val = MaterialManager.GetInstancedMaterial(m_Master._INTERNAL_InstancedMaterialGroupID, ref staticProps);
		}
		((Renderer)base.meshRenderer).material = val;
		return (Object)(object)val != (Object)null;
	}

	public void SetMaterialProp(int nameID, float value)
	{
		if (Object.op_Implicit((Object)(object)m_CustomMaterial))
		{
			m_CustomMaterial.SetFloat(nameID, value);
		}
		else
		{
			MaterialManager.materialPropertyBlock.SetFloat(nameID, value);
		}
	}

	public void SetMaterialProp(int nameID, Vector4 value)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)m_CustomMaterial))
		{
			m_CustomMaterial.SetVector(nameID, value);
		}
		else
		{
			MaterialManager.materialPropertyBlock.SetVector(nameID, value);
		}
	}

	public void SetMaterialProp(int nameID, Color value)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)m_CustomMaterial))
		{
			m_CustomMaterial.SetColor(nameID, value);
		}
		else
		{
			MaterialManager.materialPropertyBlock.SetColor(nameID, value);
		}
	}

	public void SetMaterialProp(int nameID, Matrix4x4 value)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)m_CustomMaterial))
		{
			m_CustomMaterial.SetMatrix(nameID, value);
		}
		else
		{
			MaterialManager.materialPropertyBlock.SetMatrix(nameID, value);
		}
	}

	public void SetMaterialProp(int nameID, Texture value)
	{
		if (Object.op_Implicit((Object)(object)m_CustomMaterial))
		{
			m_CustomMaterial.SetTexture(nameID, value);
		}
	}

	public void SetMaterialProp(int nameID, InvalidTexture invalidTexture)
	{
		if (Object.op_Implicit((Object)(object)m_CustomMaterial))
		{
			Texture val = null;
			if (invalidTexture == InvalidTexture.NoDepth)
			{
				val = (Texture)(object)(SystemInfo.usesReversedZBuffer ? Texture2D.blackTexture : Texture2D.whiteTexture);
			}
			m_CustomMaterial.SetTexture(nameID, val);
		}
	}

	private void MaterialChangeStart()
	{
		if ((Object)(object)m_CustomMaterial == (Object)null)
		{
			((Renderer)base.meshRenderer).GetPropertyBlock(MaterialManager.materialPropertyBlock);
		}
	}

	private void MaterialChangeStop()
	{
		if ((Object)(object)m_CustomMaterial == (Object)null)
		{
			((Renderer)base.meshRenderer).SetPropertyBlock(MaterialManager.materialPropertyBlock);
		}
	}

	public void SetPropertyDirty(DirtyProps prop)
	{
		m_DirtyProps |= prop;
		if (prop.HasAtLeastOneFlag(DirtyProps.OnlyMaterialChangeOnly))
		{
			UpdateMaterialAndBounds();
		}
	}

	private void UpdateMaterialAndBounds()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (ApplyMaterial())
		{
			MaterialChangeStart();
			m_DirtyProps = DirtyProps.All;
			if (isNoiseEnabled)
			{
				Noise3D.LoadIfNeeded();
			}
			ComputeLocalMatrix();
			UpdateMatricesPropertiesForGPUInstancingSRP();
			MaterialChangeStop();
		}
	}

	private void UpdateMatricesPropertiesForGPUInstancingSRP()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (SRPHelper.IsUsingCustomRenderPipeline() && Config.Instance.GetActualRenderingMode(ShaderMode.HD) == RenderingMode.GPUInstancing)
		{
			SetMaterialProp(ShaderProperties.LocalToWorldMatrix, ((Component)this).transform.localToWorldMatrix);
			SetMaterialProp(ShaderProperties.WorldToLocalMatrix, ((Component)this).transform.worldToLocalMatrix);
		}
	}

	private void OnBeginCameraRenderingSRP(ScriptableRenderContext context, Camera cam)
	{
		m_CurrentCameraRenderingSRP = cam;
	}

	private void OnWillRenderObject()
	{
		Camera val = null;
		val = ((!SRPHelper.IsUsingCustomRenderPipeline()) ? Camera.current : m_CurrentCameraRenderingSRP);
		OnWillCameraRenderThisBeam(val);
	}

	private void OnWillCameraRenderThisBeam(Camera cam)
	{
		if (Object.op_Implicit((Object)(object)m_Master) && Object.op_Implicit((Object)(object)cam) && ((Behaviour)cam).enabled)
		{
			UpdateMaterialPropertiesForCamera(cam);
			if (Object.op_Implicit((Object)(object)m_Shadow))
			{
				m_Shadow.OnWillCameraRenderThisBeam(cam, this);
			}
		}
	}

	private void UpdateDirtyMaterialProperties()
	{
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		if (m_DirtyProps == DirtyProps.None)
		{
			return;
		}
		if (m_DirtyProps.HasFlag(DirtyProps.Intensity))
		{
			SetMaterialProp(ShaderProperties.HD.Intensity, m_Master.intensity);
		}
		if (m_DirtyProps.HasFlag(DirtyProps.HDRPExposureWeight) && Config.Instance.isHDRPExposureWeightSupported)
		{
			SetMaterialProp(ShaderProperties.HDRPExposureWeight, m_Master.hdrpExposureWeight);
		}
		if (m_DirtyProps.HasFlag(DirtyProps.SideSoftness))
		{
			SetMaterialProp(ShaderProperties.HD.SideSoftness, m_Master.sideSoftness);
		}
		if (m_DirtyProps.HasFlag(DirtyProps.Color))
		{
			if (m_Master.colorMode == ColorMode.Flat)
			{
				SetMaterialProp(ShaderProperties.ColorFlat, m_Master.colorFlat);
			}
			else
			{
				Utils.FloatPackingPrecision floatPackingPrecision = Utils.GetFloatPackingPrecision();
				m_ColorGradientMatrix = m_Master.colorGradient.SampleInMatrix((int)floatPackingPrecision);
			}
		}
		if (m_DirtyProps.HasFlag(DirtyProps.Cone))
		{
			Vector2 val = default(Vector2);
			((Vector2)(ref val))._002Ector(Mathf.Max(m_Master.coneRadiusStart, 0.0001f), Mathf.Max(m_Master.coneRadiusEnd, 0.0001f));
			SetMaterialProp(ShaderProperties.ConeRadius, Vector4.op_Implicit(val));
			float coneApexOffsetZ = m_Master.GetConeApexOffsetZ(counterApplyScaleForUnscalableBeam: false);
			float num = Mathf.Sign(coneApexOffsetZ) * Mathf.Max(Mathf.Abs(coneApexOffsetZ), 0.0001f);
			SetMaterialProp(ShaderProperties.ConeGeomProps, Vector4.op_Implicit(new Vector2(num, (float)Config.Instance.sharedMeshSides)));
			SetMaterialProp(ShaderProperties.DistanceFallOff, Vector4.op_Implicit(new Vector3(m_Master.fallOffStart, m_Master.fallOffEnd, m_Master.maxGeometryDistance)));
			ComputeLocalMatrix();
		}
		if (m_DirtyProps.HasFlag(DirtyProps.Jittering))
		{
			this.SetMaterialProp(ShaderProperties.HD.Jittering, new Vector4(m_Master.jitteringFactor, (float)m_Master.jitteringFrameRate, m_Master.jitteringLerpRange.minValue, m_Master.jitteringLerpRange.maxValue));
		}
		if (isNoiseEnabled)
		{
			if (m_DirtyProps.HasFlag(DirtyProps.NoiseMode) || m_DirtyProps.HasFlag(DirtyProps.NoiseIntensity))
			{
				SetMaterialProp(ShaderProperties.NoiseParam, Vector4.op_Implicit(new Vector2(m_Master.noiseIntensity, (m_Master.noiseMode == NoiseMode.WorldSpace) ? 0f : 1f)));
			}
			if (m_DirtyProps.HasFlag(DirtyProps.NoiseVelocityAndScale))
			{
				Vector3 val2 = (m_Master.noiseVelocityUseGlobal ? Config.Instance.globalNoiseVelocity : m_Master.noiseVelocityLocal);
				float num2 = (m_Master.noiseScaleUseGlobal ? Config.Instance.globalNoiseScale : m_Master.noiseScaleLocal);
				this.SetMaterialProp(ShaderProperties.NoiseVelocityAndScale, new Vector4(val2.x, val2.y, val2.z, num2));
			}
		}
		if (m_DirtyProps.HasFlag(DirtyProps.CookieProps))
		{
			VolumetricCookieHD.ApplyMaterialProperties(m_Cookie, this);
		}
		if (m_DirtyProps.HasFlag(DirtyProps.ShadowProps))
		{
			VolumetricShadowHD.ApplyMaterialProperties(m_Shadow, this);
		}
		m_DirtyProps = DirtyProps.None;
	}

	private void UpdateMaterialPropertiesForCamera(Camera cam)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)cam) && Object.op_Implicit((Object)(object)m_Master))
		{
			MaterialChangeStart();
			SetMaterialProp(ShaderProperties.HD.TransformScale, Vector4.op_Implicit(m_Master.scalable ? m_Master.GetLossyScale() : Vector3.one));
			Vector3 val = ((Component)this).transform.InverseTransformDirection(((Component)cam).transform.forward);
			Vector3 normalized = ((Vector3)(ref val)).normalized;
			SetMaterialProp(ShaderProperties.HD.CameraForwardOS, Vector4.op_Implicit(normalized));
			SetMaterialProp(ShaderProperties.HD.CameraForwardWS, Vector4.op_Implicit(((Component)cam).transform.forward));
			UpdateDirtyMaterialProperties();
			if (m_Master.colorMode == ColorMode.Gradient)
			{
				SetMaterialProp(ShaderProperties.ColorGradientMatrix, m_ColorGradientMatrix);
			}
			UpdateMatricesPropertiesForGPUInstancingSRP();
			MaterialChangeStop();
			cam.depthTextureMode = (DepthTextureMode)(cam.depthTextureMode | 1);
		}
	}
}
