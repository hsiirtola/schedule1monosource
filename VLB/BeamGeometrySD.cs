using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace VLB;

[AddComponentMenu("")]
[ExecuteInEditMode]
[HelpURL("http://saladgamer.com/vlb-doc/comp-lightbeam-sd/")]
public class BeamGeometrySD : BeamGeometryAbstractBase, MaterialModifier.Interface
{
	private VolumetricLightBeamSD m_Master;

	private MeshType m_CurrentMeshType;

	private MaterialModifier.Callback m_MaterialModifierCallback;

	private Coroutine m_CoFadeOut;

	private Camera m_CurrentCameraRenderingSRP;

	private bool visible
	{
		get
		{
			return ((Renderer)base.meshRenderer).enabled;
		}
		set
		{
			((Renderer)base.meshRenderer).enabled = value;
		}
	}

	public int sortingLayerID
	{
		get
		{
			return ((Renderer)base.meshRenderer).sortingLayerID;
		}
		set
		{
			((Renderer)base.meshRenderer).sortingLayerID = value;
		}
	}

	public int sortingOrder
	{
		get
		{
			return ((Renderer)base.meshRenderer).sortingOrder;
		}
		set
		{
			((Renderer)base.meshRenderer).sortingOrder = value;
		}
	}

	public bool _INTERNAL_IsFadeOutCoroutineRunning => m_CoFadeOut != null;

	public static bool isCustomRenderPipelineSupported => true;

	private bool shouldUseGPUInstancedMaterial
	{
		get
		{
			if (m_Master._INTERNAL_DynamicOcclusionMode != MaterialManager.SD.DynamicOcclusion.DepthTexture)
			{
				return Config.Instance.GetActualRenderingMode(ShaderMode.SD) == RenderingMode.GPUInstancing;
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

	private bool isDepthBlendEnabled
	{
		get
		{
			if (!BatchingHelper.forceEnableDepthBlend)
			{
				return m_Master.depthBlendDistance > 0f;
			}
			return true;
		}
	}

	protected override VolumetricLightBeamAbstractBase GetMaster()
	{
		return m_Master;
	}

	private float ComputeFadeOutFactor(Transform camTransform)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (m_Master.isFadeOutEnabled)
		{
			Bounds bounds = ((Renderer)base.meshRenderer).bounds;
			float num = Vector3.SqrMagnitude(((Bounds)(ref bounds)).center - camTransform.position);
			return Mathf.InverseLerp(m_Master.fadeOutEnd * m_Master.fadeOutEnd, m_Master.fadeOutBegin * m_Master.fadeOutBegin, num);
		}
		return 1f;
	}

	private IEnumerator CoUpdateFadeOut()
	{
		while (m_Master.isFadeOutEnabled)
		{
			ComputeFadeOutFactor();
			yield return null;
		}
		SetFadeOutFactorProp(1f);
		m_CoFadeOut = null;
	}

	private void ComputeFadeOutFactor()
	{
		Transform fadeOutCameraTransform = Config.Instance.fadeOutCameraTransform;
		if (Object.op_Implicit((Object)(object)fadeOutCameraTransform))
		{
			float fadeOutFactorProp = ComputeFadeOutFactor(fadeOutCameraTransform);
			SetFadeOutFactorProp(fadeOutFactorProp);
		}
		else
		{
			SetFadeOutFactorProp(1f);
		}
	}

	private void SetFadeOutFactorProp(float value)
	{
		if (value > 0f)
		{
			((Renderer)base.meshRenderer).enabled = true;
			MaterialChangeStart();
			SetMaterialProp(ShaderProperties.SD.FadeOutFactor, value);
			MaterialChangeStop();
		}
		else
		{
			((Renderer)base.meshRenderer).enabled = false;
		}
	}

	private void StopFadeOutCoroutine()
	{
		if (m_CoFadeOut != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_CoFadeOut);
			m_CoFadeOut = null;
		}
	}

	public void RestartFadeOutCoroutine()
	{
		StopFadeOutCoroutine();
		if (Object.op_Implicit((Object)(object)m_Master) && m_Master.isFadeOutEnabled)
		{
			m_CoFadeOut = ((MonoBehaviour)this).StartCoroutine(CoUpdateFadeOut());
		}
	}

	public void OnMasterEnable()
	{
		visible = true;
		RestartFadeOutCoroutine();
	}

	public void OnMasterDisable()
	{
		StopFadeOutCoroutine();
		visible = false;
	}

	private void OnDisable()
	{
		SRPHelper.UnregisterOnBeginCameraRendering(OnBeginCameraRenderingSRP);
		m_CurrentCameraRenderingSRP = null;
	}

	private void OnEnable()
	{
		RestartFadeOutCoroutine();
		SRPHelper.RegisterOnBeginCameraRendering(OnBeginCameraRenderingSRP);
	}

	public void Initialize(VolumetricLightBeamSD master)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		HideFlags proceduralObjectsHideFlags = Consts.Internal.ProceduralObjectsHideFlags;
		m_Master = master;
		((Component)this).transform.SetParent(((Component)master).transform, false);
		base.meshRenderer = ((Component)this).gameObject.GetOrAddComponent<MeshRenderer>();
		((Object)base.meshRenderer).hideFlags = proceduralObjectsHideFlags;
		((Renderer)base.meshRenderer).shadowCastingMode = (ShadowCastingMode)0;
		((Renderer)base.meshRenderer).receiveShadows = false;
		((Renderer)base.meshRenderer).reflectionProbeUsage = (ReflectionProbeUsage)0;
		((Renderer)base.meshRenderer).lightProbeUsage = (LightProbeUsage)0;
		if (!shouldUseGPUInstancedMaterial)
		{
			m_CustomMaterial = Config.Instance.NewMaterialTransient(ShaderMode.SD, gpuInstanced: false);
			ApplyMaterial();
		}
		if (SortingLayer.IsValid(m_Master.sortingLayerID))
		{
			sortingLayerID = m_Master.sortingLayerID;
		}
		else
		{
			Debug.LogError((object)$"Beam '{Utils.GetPath(((Component)m_Master).transform)}' has an invalid sortingLayerID ({m_Master.sortingLayerID}). Please fix it by setting a valid layer.");
		}
		sortingOrder = m_Master.sortingOrder;
		base.meshFilter = ((Component)this).gameObject.GetOrAddComponent<MeshFilter>();
		((Object)base.meshFilter).hideFlags = proceduralObjectsHideFlags;
		((Object)((Component)this).gameObject).hideFlags = proceduralObjectsHideFlags;
		RestartFadeOutCoroutine();
	}

	public void RegenerateMesh(bool masterEnabled)
	{
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		if (Config.Instance.geometryOverrideLayer)
		{
			((Component)this).gameObject.layer = Config.Instance.geometryLayerID;
		}
		else
		{
			((Component)this).gameObject.layer = ((Component)m_Master).gameObject.layer;
		}
		((Component)this).gameObject.tag = Config.Instance.geometryTag;
		if (Object.op_Implicit((Object)(object)base.coneMesh) && m_CurrentMeshType == MeshType.Custom)
		{
			Object.DestroyImmediate((Object)(object)base.coneMesh);
		}
		m_CurrentMeshType = m_Master.geomMeshType;
		switch (m_Master.geomMeshType)
		{
		case MeshType.Custom:
			base.coneMesh = MeshGenerator.GenerateConeZ_Radii(1f, 1f, 1f, m_Master.geomCustomSides, m_Master.geomCustomSegments, m_Master.geomCap, Config.Instance.SD_requiresDoubleSidedMesh);
			((Object)base.coneMesh).hideFlags = Consts.Internal.ProceduralObjectsHideFlags;
			base.meshFilter.mesh = base.coneMesh;
			break;
		case MeshType.Shared:
			base.coneMesh = GlobalMeshSD.Get();
			base.meshFilter.sharedMesh = base.coneMesh;
			break;
		default:
			Debug.LogError((object)"Unsupported MeshType");
			break;
		}
		UpdateMaterialAndBounds();
		visible = masterEnabled;
	}

	private Vector3 ComputeLocalMatrix()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Max(m_Master.coneRadiusStart, m_Master.coneRadiusEnd);
		((Component)this).transform.localScale = new Vector3(num, num, m_Master.maxGeometryDistance);
		((Component)this).transform.localRotation = m_Master.beamInternalLocalRotation;
		return ((Component)this).transform.localScale;
	}

	private MaterialManager.StaticPropertiesSD ComputeMaterialStaticProperties()
	{
		MaterialManager.ColorGradient colorGradient = MaterialManager.ColorGradient.Off;
		if (m_Master.colorMode == ColorMode.Gradient)
		{
			colorGradient = ((Utils.GetFloatPackingPrecision() != Utils.FloatPackingPrecision.High) ? MaterialManager.ColorGradient.MatrixLow : MaterialManager.ColorGradient.MatrixHigh);
		}
		return new MaterialManager.StaticPropertiesSD
		{
			blendingMode = (MaterialManager.BlendingMode)m_Master.blendingMode,
			noise3D = (isNoiseEnabled ? MaterialManager.Noise3D.On : MaterialManager.Noise3D.Off),
			depthBlend = (isDepthBlendEnabled ? MaterialManager.SD.DepthBlend.On : MaterialManager.SD.DepthBlend.Off),
			colorGradient = colorGradient,
			dynamicOcclusion = m_Master._INTERNAL_DynamicOcclusionMode_Runtime,
			meshSkewing = (m_Master.hasMeshSkewing ? MaterialManager.SD.MeshSkewing.On : MaterialManager.SD.MeshSkewing.Off),
			shaderAccuracy = ((m_Master.shaderAccuracy != ShaderAccuracy.Fast) ? MaterialManager.SD.ShaderAccuracy.High : MaterialManager.SD.ShaderAccuracy.Fast)
		};
	}

	private bool ApplyMaterial()
	{
		MaterialManager.StaticPropertiesSD staticProps = ComputeMaterialStaticProperties();
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
		else
		{
			Debug.LogError((object)"Setting a Texture property to a GPU instanced material is not supported");
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

	public void SetDynamicOcclusionCallback(string shaderKeyword, MaterialModifier.Callback cb)
	{
		m_MaterialModifierCallback = cb;
		if (Object.op_Implicit((Object)(object)m_CustomMaterial))
		{
			m_CustomMaterial.SetKeywordEnabled(shaderKeyword, cb != null);
			cb?.Invoke(this);
		}
		else
		{
			UpdateMaterialAndBounds();
		}
	}

	public void UpdateMaterialAndBounds()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Unknown result type (might be due to invalid IL or missing references)
		//IL_037d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_038a: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0402: Unknown result type (might be due to invalid IL or missing references)
		//IL_0407: Unknown result type (might be due to invalid IL or missing references)
		//IL_040b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0410: Unknown result type (might be due to invalid IL or missing references)
		//IL_0414: Unknown result type (might be due to invalid IL or missing references)
		//IL_0419: Unknown result type (might be due to invalid IL or missing references)
		//IL_041b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0446: Unknown result type (might be due to invalid IL or missing references)
		//IL_0432: Unknown result type (might be due to invalid IL or missing references)
		//IL_044f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_047a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0466: Unknown result type (might be due to invalid IL or missing references)
		//IL_0485: Unknown result type (might be due to invalid IL or missing references)
		//IL_048e: Unknown result type (might be due to invalid IL or missing references)
		//IL_049b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0350: Unknown result type (might be due to invalid IL or missing references)
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		if (!ApplyMaterial())
		{
			return;
		}
		MaterialChangeStart();
		if ((Object)(object)m_CustomMaterial == (Object)null && m_MaterialModifierCallback != null)
		{
			m_MaterialModifierCallback(this);
		}
		float num = m_Master.coneAngle * ((float)Math.PI / 180f) / 2f;
		SetMaterialProp(ShaderProperties.SD.ConeSlopeCosSin, Vector4.op_Implicit(new Vector2(Mathf.Cos(num), Mathf.Sin(num))));
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(Mathf.Max(m_Master.coneRadiusStart, 0.0001f), Mathf.Max(m_Master.coneRadiusEnd, 0.0001f));
		SetMaterialProp(ShaderProperties.ConeRadius, Vector4.op_Implicit(val));
		float num2 = Mathf.Sign(m_Master.coneApexOffsetZ) * Mathf.Max(Mathf.Abs(m_Master.coneApexOffsetZ), 0.0001f);
		SetMaterialProp(ShaderProperties.ConeGeomProps, Vector4.op_Implicit(new Vector2(num2, (float)m_Master.geomSides)));
		if (m_Master.usedColorMode == ColorMode.Flat)
		{
			SetMaterialProp(ShaderProperties.ColorFlat, m_Master.color);
		}
		else
		{
			Utils.FloatPackingPrecision floatPackingPrecision = Utils.GetFloatPackingPrecision();
			m_ColorGradientMatrix = m_Master.colorGradient.SampleInMatrix((int)floatPackingPrecision);
		}
		m_Master.GetInsideAndOutsideIntensity(out var inside, out var outside);
		SetMaterialProp(ShaderProperties.SD.AlphaInside, inside);
		SetMaterialProp(ShaderProperties.SD.AlphaOutside, outside);
		SetMaterialProp(ShaderProperties.SD.AttenuationLerpLinearQuad, m_Master.attenuationLerpLinearQuad);
		SetMaterialProp(ShaderProperties.DistanceFallOff, Vector4.op_Implicit(new Vector3(m_Master.fallOffStart, m_Master.fallOffEnd, m_Master.maxGeometryDistance)));
		SetMaterialProp(ShaderProperties.SD.DistanceCamClipping, m_Master.cameraClippingDistance);
		SetMaterialProp(ShaderProperties.SD.FresnelPow, Mathf.Max(0.001f, m_Master.fresnelPow));
		SetMaterialProp(ShaderProperties.SD.GlareBehind, m_Master.glareBehind);
		SetMaterialProp(ShaderProperties.SD.GlareFrontal, m_Master.glareFrontal);
		SetMaterialProp(ShaderProperties.SD.DrawCap, (float)(m_Master.geomCap ? 1 : 0));
		SetMaterialProp(ShaderProperties.SD.TiltVector, Vector4.op_Implicit(m_Master.tiltFactor));
		SetMaterialProp(ShaderProperties.SD.AdditionalClippingPlaneWS, m_Master.additionalClippingPlane);
		if (Config.Instance.isHDRPExposureWeightSupported)
		{
			SetMaterialProp(ShaderProperties.HDRPExposureWeight, m_Master.hdrpExposureWeight);
		}
		if (isDepthBlendEnabled)
		{
			SetMaterialProp(ShaderProperties.SD.DepthBlendDistance, m_Master.depthBlendDistance);
		}
		if (isNoiseEnabled)
		{
			Noise3D.LoadIfNeeded();
			Vector3 val2 = (m_Master.noiseVelocityUseGlobal ? Config.Instance.globalNoiseVelocity : m_Master.noiseVelocityLocal);
			float num3 = (m_Master.noiseScaleUseGlobal ? Config.Instance.globalNoiseScale : m_Master.noiseScaleLocal);
			this.SetMaterialProp(ShaderProperties.NoiseVelocityAndScale, new Vector4(val2.x, val2.y, val2.z, num3));
			SetMaterialProp(ShaderProperties.NoiseParam, Vector4.op_Implicit(new Vector2(m_Master.noiseIntensity, (m_Master.noiseMode == NoiseMode.WorldSpace) ? 0f : 1f)));
		}
		Vector3 val3 = ComputeLocalMatrix();
		if (m_Master.hasMeshSkewing)
		{
			Vector3 skewingLocalForwardDirectionNormalized = m_Master.skewingLocalForwardDirectionNormalized;
			SetMaterialProp(ShaderProperties.SD.LocalForwardDirection, Vector4.op_Implicit(skewingLocalForwardDirectionNormalized));
			if ((Object)(object)base.coneMesh != (Object)null)
			{
				Vector3 val4 = skewingLocalForwardDirectionNormalized;
				val4 /= val4.z;
				val4 *= m_Master.fallOffEnd;
				val4.x /= val3.x;
				val4.y /= val3.y;
				Bounds bounds = MeshGenerator.ComputeBounds(1f, 1f, 1f);
				Vector3 min = ((Bounds)(ref bounds)).min;
				Vector3 max = ((Bounds)(ref bounds)).max;
				if (val4.x > 0f)
				{
					max.x += val4.x;
				}
				else
				{
					min.x += val4.x;
				}
				if (val4.y > 0f)
				{
					max.y += val4.y;
				}
				else
				{
					min.y += val4.y;
				}
				((Bounds)(ref bounds)).min = min;
				((Bounds)(ref bounds)).max = max;
				base.coneMesh.bounds = bounds;
			}
		}
		UpdateMatricesPropertiesForGPUInstancingSRP();
		MaterialChangeStop();
	}

	private void UpdateMatricesPropertiesForGPUInstancingSRP()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (SRPHelper.IsUsingCustomRenderPipeline() && Config.Instance.GetActualRenderingMode(ShaderMode.SD) == RenderingMode.GPUInstancing)
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
			UpdateCameraRelatedProperties(cam);
			m_Master._INTERNAL_OnWillCameraRenderThisBeam(cam);
		}
	}

	private void UpdateCameraRelatedProperties(Camera cam)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)cam) && Object.op_Implicit((Object)(object)m_Master))
		{
			MaterialChangeStart();
			Vector3 posOS = ((Component)m_Master).transform.InverseTransformPoint(((Component)cam).transform.position);
			Vector3 val = ((Component)this).transform.InverseTransformDirection(((Component)cam).transform.forward);
			Vector3 normalized = ((Vector3)(ref val)).normalized;
			float num = (cam.orthographic ? (-1f) : m_Master.GetInsideBeamFactorFromObjectSpacePos(posOS));
			this.SetMaterialProp(ShaderProperties.SD.CameraParams, new Vector4(normalized.x, normalized.y, normalized.z, num));
			UpdateMatricesPropertiesForGPUInstancingSRP();
			if (m_Master.usedColorMode == ColorMode.Gradient)
			{
				SetMaterialProp(ShaderProperties.ColorGradientMatrix, m_ColorGradientMatrix);
			}
			MaterialChangeStop();
			if (m_Master.depthBlendDistance > 0f)
			{
				cam.depthTextureMode = (DepthTextureMode)(cam.depthTextureMode | 1);
			}
		}
	}
}
