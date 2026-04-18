using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace VLB;

[HelpURL("http://saladgamer.com/vlb-doc/config/")]
public class Config : ScriptableObject
{
	public const string ClassName = "Config";

	public const string kAssetName = "VLBConfigOverride";

	public const string kAssetNameExt = ".asset";

	public bool geometryOverrideLayer = true;

	public int geometryLayerID = 1;

	public string geometryTag = "Untagged";

	public int geometryRenderQueue = 3000;

	public int geometryRenderQueueHD = 3100;

	[FormerlySerializedAs("renderPipeline")]
	[FormerlySerializedAs("_RenderPipeline")]
	[SerializeField]
	private RenderPipeline m_RenderPipeline;

	[FormerlySerializedAs("renderingMode")]
	[FormerlySerializedAs("_RenderingMode")]
	[SerializeField]
	private RenderingMode m_RenderingMode = RenderingMode.Default;

	public float ditheringFactor;

	public bool useLightColorTemperature = true;

	public int sharedMeshSides = 24;

	public int sharedMeshSegments = 5;

	public float hdBeamsCameraBlendingDistance = 0.5f;

	public int urpDepthCameraScriptableRendererIndex = -1;

	[Range(0.01f, 2f)]
	public float globalNoiseScale = 0.5f;

	public Vector3 globalNoiseVelocity = Consts.Beam.NoiseVelocityDefault;

	public string fadeOutCameraTag = "MainCamera";

	[HighlightNull]
	public Texture3D noiseTexture3D;

	[HighlightNull]
	public ParticleSystem dustParticlesPrefab;

	[HighlightNull]
	public Texture2D ditheringNoiseTexture;

	[HighlightNull]
	public Texture2D jitteringNoiseTexture;

	public FeatureEnabledColorGradient featureEnabledColorGradient = FeatureEnabledColorGradient.HighOnly;

	public bool featureEnabledDepthBlend = true;

	public bool featureEnabledNoise3D = true;

	public bool featureEnabledDynamicOcclusion = true;

	public bool featureEnabledMeshSkewing = true;

	public bool featureEnabledShaderAccuracyHigh = true;

	public bool featureEnabledShadow = true;

	public bool featureEnabledCookie = true;

	[SerializeField]
	private RaymarchingQuality[] m_RaymarchingQualities;

	[SerializeField]
	private int m_DefaultRaymarchingQualityUniqueID;

	[SerializeField]
	private int pluginVersion = -1;

	[SerializeField]
	private Material _DummyMaterial;

	[SerializeField]
	private Material _DummyMaterialHD;

	[SerializeField]
	private Shader _BeamShader;

	[SerializeField]
	private Shader _BeamShaderHD;

	private Transform m_CachedFadeOutCamera;

	private static Config ms_Instance;

	public RenderPipeline renderPipeline
	{
		get
		{
			return m_RenderPipeline;
		}
		set
		{
			Debug.LogError((object)"Modifying the RenderPipeline in standalone builds is not permitted");
		}
	}

	public RenderingMode renderingMode
	{
		get
		{
			return m_RenderingMode;
		}
		set
		{
			Debug.LogError((object)"Modifying the RenderingMode in standalone builds is not permitted");
		}
	}

	public bool SD_useSinglePassShader => GetActualRenderingMode(ShaderMode.SD) != RenderingMode.MultiPass;

	public bool SD_requiresDoubleSidedMesh => SD_useSinglePassShader;

	public Transform fadeOutCameraTransform
	{
		get
		{
			if ((Object)(object)m_CachedFadeOutCamera == (Object)null)
			{
				ForceUpdateFadeOutCamera();
			}
			return m_CachedFadeOutCamera;
		}
	}

	public int defaultRaymarchingQualityUniqueID => m_DefaultRaymarchingQualityUniqueID;

	public int raymarchingQualitiesCount => Mathf.Max(1, (m_RaymarchingQualities == null) ? 1 : m_RaymarchingQualities.Length);

	public bool isHDRPExposureWeightSupported => renderPipeline == RenderPipeline.HDRP;

	public bool hasRenderPipelineMismatch => SRPHelper.projectRenderPipeline == RenderPipeline.BuiltIn != (m_RenderPipeline == RenderPipeline.BuiltIn);

	public static Config Instance => GetInstance(assertIfNotFound: true);

	public bool IsSRPBatcherSupported()
	{
		if (renderPipeline == RenderPipeline.BuiltIn)
		{
			return false;
		}
		RenderPipeline projectRenderPipeline = SRPHelper.projectRenderPipeline;
		if (projectRenderPipeline != RenderPipeline.URP)
		{
			return projectRenderPipeline == RenderPipeline.HDRP;
		}
		return true;
	}

	public RenderingMode GetActualRenderingMode(ShaderMode shaderMode)
	{
		if (renderingMode == RenderingMode.SRPBatcher && !IsSRPBatcherSupported())
		{
			return RenderingMode.Default;
		}
		if (renderPipeline != RenderPipeline.BuiltIn && renderingMode == RenderingMode.MultiPass)
		{
			return RenderingMode.Default;
		}
		if (shaderMode == ShaderMode.HD && renderingMode == RenderingMode.MultiPass)
		{
			return RenderingMode.Default;
		}
		return renderingMode;
	}

	public Shader GetBeamShader(ShaderMode mode)
	{
		return GetBeamShaderInternal(mode);
	}

	private ref Shader GetBeamShaderInternal(ShaderMode mode)
	{
		if (mode == ShaderMode.SD)
		{
			return ref _BeamShader;
		}
		return ref _BeamShaderHD;
	}

	private int GetRenderQueueInternal(ShaderMode mode)
	{
		if (mode == ShaderMode.SD)
		{
			return geometryRenderQueue;
		}
		return geometryRenderQueueHD;
	}

	public Material NewMaterialTransient(ShaderMode mode, bool gpuInstanced)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		Material val = MaterialManager.NewMaterialPersistent(GetBeamShader(mode), gpuInstanced);
		if (Object.op_Implicit((Object)(object)val))
		{
			((Object)val).hideFlags = Consts.Internal.ProceduralObjectsHideFlags;
			val.renderQueue = GetRenderQueueInternal(mode);
		}
		return val;
	}

	public void SetURPScriptableRendererIndexToDepthCamera(Camera camera)
	{
		if (urpDepthCameraScriptableRendererIndex >= 0)
		{
			UniversalAdditionalCameraData universalAdditionalCameraData = CameraExtensions.GetUniversalAdditionalCameraData(camera);
			if (Object.op_Implicit((Object)(object)universalAdditionalCameraData))
			{
				universalAdditionalCameraData.SetRenderer(urpDepthCameraScriptableRendererIndex);
			}
		}
	}

	public void ForceUpdateFadeOutCamera()
	{
		GameObject val = GameObject.FindGameObjectWithTag(fadeOutCameraTag);
		if (Object.op_Implicit((Object)(object)val))
		{
			m_CachedFadeOutCamera = val.transform;
		}
	}

	public RaymarchingQuality GetRaymarchingQualityForIndex(int index)
	{
		return m_RaymarchingQualities[index];
	}

	public RaymarchingQuality GetRaymarchingQualityForUniqueID(int id)
	{
		int raymarchingQualityIndexForUniqueID = GetRaymarchingQualityIndexForUniqueID(id);
		if (raymarchingQualityIndexForUniqueID >= 0)
		{
			return GetRaymarchingQualityForIndex(raymarchingQualityIndexForUniqueID);
		}
		return null;
	}

	public int GetRaymarchingQualityIndexForUniqueID(int id)
	{
		for (int i = 0; i < m_RaymarchingQualities.Length; i++)
		{
			RaymarchingQuality raymarchingQuality = m_RaymarchingQualities[i];
			if (raymarchingQuality != null && raymarchingQuality.uniqueID == id)
			{
				return i;
			}
		}
		Debug.LogErrorFormat("Failed to find RaymarchingQualityIndex for Unique ID {0}", new object[1] { id });
		return -1;
	}

	public bool IsRaymarchingQualityUniqueIDValid(int id)
	{
		return GetRaymarchingQualityIndexForUniqueID(id) >= 0;
	}

	private void CreateDefaultRaymarchingQualityPreset(bool onlyIfNeeded)
	{
		if (m_RaymarchingQualities == null || m_RaymarchingQualities.Length == 0 || !onlyIfNeeded)
		{
			m_RaymarchingQualities = new RaymarchingQuality[3];
			m_RaymarchingQualities[0] = RaymarchingQuality.New("Fast", 1, 5);
			m_RaymarchingQualities[1] = RaymarchingQuality.New("Balanced", 2, 10);
			m_RaymarchingQualities[2] = RaymarchingQuality.New("High", 3, 20);
			m_DefaultRaymarchingQualityUniqueID = m_RaymarchingQualities[1].uniqueID;
		}
	}

	[RuntimeInitializeOnLoadMethod]
	private static void OnStartup()
	{
		Instance.m_CachedFadeOutCamera = null;
		Instance.RefreshGlobalShaderProperties();
		if (Instance.hasRenderPipelineMismatch)
		{
			Debug.LogError((object)"It looks like the 'Render Pipeline' is not correctly set in the config. Please make sure to select the proper value depending on your pipeline in use.", (Object)(object)Instance);
		}
	}

	public void Reset()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		geometryOverrideLayer = true;
		geometryLayerID = 1;
		geometryTag = "Untagged";
		geometryRenderQueue = 3000;
		geometryRenderQueueHD = 3100;
		sharedMeshSides = 24;
		sharedMeshSegments = 5;
		globalNoiseScale = 0.5f;
		globalNoiseVelocity = Consts.Beam.NoiseVelocityDefault;
		renderPipeline = RenderPipeline.BuiltIn;
		renderingMode = RenderingMode.Default;
		ditheringFactor = 0f;
		useLightColorTemperature = true;
		fadeOutCameraTag = "MainCamera";
		featureEnabledColorGradient = FeatureEnabledColorGradient.HighOnly;
		featureEnabledDepthBlend = true;
		featureEnabledNoise3D = true;
		featureEnabledDynamicOcclusion = true;
		featureEnabledMeshSkewing = true;
		featureEnabledShaderAccuracyHigh = true;
		hdBeamsCameraBlendingDistance = 0.5f;
		urpDepthCameraScriptableRendererIndex = -1;
		CreateDefaultRaymarchingQualityPreset(onlyIfNeeded: false);
		ResetInternalData();
	}

	private void RefreshGlobalShaderProperties()
	{
		Shader.SetGlobalFloat(ShaderProperties.GlobalUsesReversedZBuffer, SystemInfo.usesReversedZBuffer ? 1f : 0f);
		Shader.SetGlobalFloat(ShaderProperties.GlobalDitheringFactor, ditheringFactor);
		Shader.SetGlobalTexture(ShaderProperties.GlobalDitheringNoiseTex, (Texture)(object)ditheringNoiseTexture);
		Shader.SetGlobalFloat(ShaderProperties.HD.GlobalCameraBlendingDistance, hdBeamsCameraBlendingDistance);
		Shader.SetGlobalTexture(ShaderProperties.HD.GlobalJitteringNoiseTex, (Texture)(object)jitteringNoiseTexture);
	}

	public void ResetInternalData()
	{
		Object obj = Resources.Load("Noise3D_64x64x64");
		noiseTexture3D = (Texture3D)(object)((obj is Texture3D) ? obj : null);
		Object obj2 = Resources.Load("DustParticles", typeof(ParticleSystem));
		dustParticlesPrefab = (ParticleSystem)(object)((obj2 is ParticleSystem) ? obj2 : null);
		Object obj3 = Resources.Load("VLBDitheringNoise", typeof(Texture2D));
		ditheringNoiseTexture = (Texture2D)(object)((obj3 is Texture2D) ? obj3 : null);
		Object obj4 = Resources.Load("VLBBlueNoise", typeof(Texture2D));
		jitteringNoiseTexture = (Texture2D)(object)((obj4 is Texture2D) ? obj4 : null);
	}

	public ParticleSystem NewVolumetricDustParticles()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)dustParticlesPrefab))
		{
			if (Application.isPlaying)
			{
				Debug.LogError((object)"Failed to instantiate VolumetricDustParticles prefab.");
			}
			return null;
		}
		ParticleSystem obj = Object.Instantiate<ParticleSystem>(dustParticlesPrefab);
		obj.useAutoRandomSeed = false;
		((Object)obj).name = "Dust Particles";
		((Object)((Component)obj).gameObject).hideFlags = Consts.Internal.ProceduralObjectsHideFlags;
		((Component)obj).gameObject.SetActive(true);
		return obj;
	}

	private void OnEnable()
	{
		CreateDefaultRaymarchingQualityPreset(onlyIfNeeded: true);
		HandleBackwardCompatibility(pluginVersion, 20100);
		pluginVersion = 20100;
	}

	private void HandleBackwardCompatibility(int serializedVersion, int newVersion)
	{
	}

	private static Config LoadAssetInternal(string assetName)
	{
		return Resources.Load<Config>(assetName);
	}

	private static Config GetInstance(bool assertIfNotFound)
	{
		if ((Object)(object)ms_Instance == (Object)null)
		{
			Config config = LoadAssetInternal("VLBConfigOverride" + PlatformHelper.GetCurrentPlatformSuffix());
			if ((Object)(object)config == (Object)null)
			{
				config = LoadAssetInternal("VLBConfigOverride");
			}
			ms_Instance = config;
			_ = (Object)(object)ms_Instance == (Object)null;
		}
		return ms_Instance;
	}
}
