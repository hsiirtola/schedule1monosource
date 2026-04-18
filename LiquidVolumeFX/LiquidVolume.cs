using System;
using System.Collections.Generic;
using UnityEngine;

namespace LiquidVolumeFX;

[ExecuteInEditMode]
[HelpURL("https://kronnect.com/support")]
[AddComponentMenu("Effects/Liquid Volume")]
[DisallowMultipleComponent]
public class LiquidVolume : MonoBehaviour
{
	private struct MeshCache
	{
		public Vector3[] verticesSorted;

		public Vector3[] verticesUnsorted;

		public int[] indices;
	}

	private delegate float MeshVolumeCalcFunction(float level01, float yExtent);

	private static class ShaderParams
	{
		public static int PointLightInsideAtten = Shader.PropertyToID("_PointLightInsideAtten");

		public static int PointLightColorArray = Shader.PropertyToID("_PointLightColor");

		public static int PointLightPositionArray = Shader.PropertyToID("_PointLightPosition");

		public static int PointLightCount = Shader.PropertyToID("_PointLightCount");

		public static int GlossinessInt = Shader.PropertyToID("_GlossinessInternal");

		public static int DoubleSidedBias = Shader.PropertyToID("_DoubleSidedBias");

		public static int BackDepthBias = Shader.PropertyToID("_BackDepthBias");

		public static int Muddy = Shader.PropertyToID("_Muddy");

		public static int Alpha = Shader.PropertyToID("_Alpha");

		public static int AlphaCombined = Shader.PropertyToID("_AlphaCombined");

		public static int SparklingIntensity = Shader.PropertyToID("_SparklingIntensity");

		public static int SparklingThreshold = Shader.PropertyToID("_SparklingThreshold");

		public static int DepthAtten = Shader.PropertyToID("_DeepAtten");

		public static int SmokeColor = Shader.PropertyToID("_SmokeColor");

		public static int SmokeAtten = Shader.PropertyToID("_SmokeAtten");

		public static int SmokeSpeed = Shader.PropertyToID("_SmokeSpeed");

		public static int SmokeHeightAtten = Shader.PropertyToID("_SmokeHeightAtten");

		public static int SmokeRaySteps = Shader.PropertyToID("_SmokeRaySteps");

		public static int LiquidRaySteps = Shader.PropertyToID("_LiquidRaySteps");

		public static int FlaskBlurIntensity = Shader.PropertyToID("_FlaskBlurIntensity");

		public static int FoamColor = Shader.PropertyToID("_FoamColor");

		public static int FoamRaySteps = Shader.PropertyToID("_FoamRaySteps");

		public static int FoamDensity = Shader.PropertyToID("_FoamDensity");

		public static int FoamWeight = Shader.PropertyToID("_FoamWeight");

		public static int FoamBottom = Shader.PropertyToID("_FoamBottom");

		public static int FoamTurbulence = Shader.PropertyToID("_FoamTurbulence");

		public static int RefractTex = Shader.PropertyToID("_RefractTex");

		public static int FlaskThickness = Shader.PropertyToID("_FlaskThickness");

		public static int Size = Shader.PropertyToID("_Size");

		public static int Scale = Shader.PropertyToID("_Scale");

		public static int Center = Shader.PropertyToID("_Center");

		public static int SizeWorld = Shader.PropertyToID("_SizeWorld");

		public static int DepthAwareOffset = Shader.PropertyToID("_DepthAwareOffset");

		public static int Turbulence = Shader.PropertyToID("_Turbulence");

		public static int TurbulenceSpeed = Shader.PropertyToID("_TurbulenceSpeed");

		public static int MurkinessSpeed = Shader.PropertyToID("_MurkinessSpeed");

		public static int Color1 = Shader.PropertyToID("_Color1");

		public static int Color2 = Shader.PropertyToID("_Color2");

		public static int EmissionColor = Shader.PropertyToID("_EmissionColor");

		public static int LightColor = Shader.PropertyToID("_LightColor");

		public static int LightDir = Shader.PropertyToID("_LightDir");

		public static int LevelPos = Shader.PropertyToID("_LevelPos");

		public static int UpperLimit = Shader.PropertyToID("_UpperLimit");

		public static int LowerLimit = Shader.PropertyToID("_LowerLimit");

		public static int FoamMaxPos = Shader.PropertyToID("_FoamMaxPos");

		public static int CullMode = Shader.PropertyToID("_CullMode");

		public static int ZTestMode = Shader.PropertyToID("_ZTestMode");

		public static int NoiseTex = Shader.PropertyToID("_NoiseTex");

		public static int NoiseTexUnwrapped = Shader.PropertyToID("_NoiseTexUnwrapped");

		public static int GlobalRefractionTexture = Shader.PropertyToID("_VLGrabBlurTexture");

		public static int RotationMatrix = Shader.PropertyToID("_Rot");

		public static int QueueOffset = Shader.PropertyToID("_QueueOffset");

		public static int PreserveSpecular = Shader.PropertyToID("_BlendModePreserveSpecular");
	}

	public static bool FORCE_GLES_COMPATIBILITY = false;

	[SerializeField]
	private TOPOLOGY _topology;

	[SerializeField]
	private DETAIL _detail = DETAIL.Default;

	[SerializeField]
	[Range(0f, 1f)]
	private float _level = 0.5f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _levelMultiplier = 1f;

	[SerializeField]
	[Tooltip("Uses directional light color")]
	private bool _useLightColor;

	[SerializeField]
	[Tooltip("Uses directional light direction for day/night cycle")]
	private bool _useLightDirection;

	[SerializeField]
	private Light _directionalLight;

	[SerializeField]
	[ColorUsage(true)]
	private Color _liquidColor1 = new Color(0f, 1f, 0f, 0.1f);

	[SerializeField]
	[Range(0.1f, 4.85f)]
	private float _liquidScale1 = 1f;

	[SerializeField]
	[ColorUsage(true)]
	private Color _liquidColor2 = new Color(1f, 0f, 0f, 0.3f);

	[SerializeField]
	[Range(2f, 4.85f)]
	private float _liquidScale2 = 5f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _alpha = 1f;

	[SerializeField]
	[ColorUsage(true)]
	private Color _emissionColor = new Color(0f, 0f, 0f);

	[SerializeField]
	private bool _ditherShadows = true;

	[SerializeField]
	[Range(0f, 1f)]
	private float _murkiness = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _turbulence1 = 0.5f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _turbulence2 = 0.2f;

	[SerializeField]
	private float _frecuency = 1f;

	[SerializeField]
	[Range(0f, 2f)]
	private float _speed = 1f;

	[SerializeField]
	[Range(0f, 5f)]
	private float _sparklingIntensity = 0.1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _sparklingAmount = 0.2f;

	[SerializeField]
	[Range(0f, 10f)]
	private float _deepObscurance = 2f;

	[SerializeField]
	[ColorUsage(true)]
	private Color _foamColor = new Color(1f, 1f, 1f, 0.65f);

	[SerializeField]
	[Range(0.01f, 1f)]
	private float _foamScale = 0.2f;

	[SerializeField]
	[Range(0f, 0.1f)]
	private float _foamThickness = 0.04f;

	[SerializeField]
	[Range(-1f, 1f)]
	private float _foamDensity = 0.5f;

	[SerializeField]
	[Range(4f, 100f)]
	private float _foamWeight = 10f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _foamTurbulence = 1f;

	[SerializeField]
	private bool _foamVisibleFromBottom = true;

	[SerializeField]
	private bool _smokeEnabled = true;

	[ColorUsage(true)]
	[SerializeField]
	private Color _smokeColor = new Color(0.7f, 0.7f, 0.7f, 0.25f);

	[SerializeField]
	[Range(0.01f, 1f)]
	private float _smokeScale = 0.25f;

	[SerializeField]
	[Range(0f, 10f)]
	private float _smokeBaseObscurance = 2f;

	[SerializeField]
	[Range(0f, 10f)]
	private float _smokeHeightAtten;

	[SerializeField]
	[Range(0f, 20f)]
	private float _smokeSpeed = 5f;

	[SerializeField]
	private bool _fixMesh;

	public Mesh originalMesh;

	public Vector3 originalPivotOffset;

	[SerializeField]
	private Vector3 _pivotOffset;

	[SerializeField]
	private bool _limitVerticalRange;

	[SerializeField]
	[Range(0f, 1.5f)]
	private float _upperLimit = 1.5f;

	[SerializeField]
	[Range(-1.5f, 1.5f)]
	private float _lowerLimit = -1.5f;

	[SerializeField]
	private int _subMeshIndex = -1;

	[SerializeField]
	private Material _flaskMaterial;

	[SerializeField]
	[Range(0f, 1f)]
	private float _flaskThickness = 0.03f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _glossinessInternal = 0.3f;

	[SerializeField]
	private bool _scatteringEnabled;

	[SerializeField]
	[Range(1f, 16f)]
	private int _scatteringPower = 5;

	[SerializeField]
	[Range(0f, 10f)]
	private float _scatteringAmount = 0.3f;

	[SerializeField]
	private bool _refractionBlur = true;

	[SerializeField]
	[Range(0f, 1f)]
	private float _blurIntensity = 0.75f;

	[SerializeField]
	private int _liquidRaySteps = 10;

	[SerializeField]
	private int _foamRaySteps = 7;

	[SerializeField]
	private int _smokeRaySteps = 5;

	[SerializeField]
	private Texture2D _bumpMap;

	[SerializeField]
	[Range(0f, 1f)]
	private float _bumpStrength = 1f;

	[SerializeField]
	[Range(0f, 10f)]
	private float _bumpDistortionScale = 1f;

	[SerializeField]
	private Vector2 _bumpDistortionOffset;

	[SerializeField]
	private Texture2D _distortionMap;

	[SerializeField]
	private Texture2D _texture;

	[SerializeField]
	private Vector2 _textureScale = Vector2.one;

	[SerializeField]
	private Vector2 _textureOffset;

	[SerializeField]
	[Range(0f, 10f)]
	private float _distortionAmount = 1f;

	[SerializeField]
	private bool _depthAware;

	[SerializeField]
	private float _depthAwareOffset;

	[SerializeField]
	private bool _irregularDepthDebug;

	[SerializeField]
	private bool _depthAwareCustomPass;

	[SerializeField]
	private bool _depthAwareCustomPassDebug;

	[SerializeField]
	[Range(0f, 5f)]
	private float _doubleSidedBias;

	[SerializeField]
	private float _backDepthBias;

	[SerializeField]
	private LEVEL_COMPENSATION _rotationLevelCompensation;

	[SerializeField]
	private bool _ignoreGravity;

	[SerializeField]
	private bool _reactToForces;

	[SerializeField]
	private Vector3 _extentsScale = Vector3.one;

	[SerializeField]
	[Range(1f, 3f)]
	private int _noiseVariation = 1;

	[SerializeField]
	private bool _allowViewFromInside;

	[SerializeField]
	private bool _debugSpillPoint;

	[SerializeField]
	private int _renderQueue = 3001;

	[SerializeField]
	private Cubemap _reflectionTexture;

	[SerializeField]
	[Range(0.1f, 5f)]
	private float _physicsMass = 1f;

	[SerializeField]
	[Range(0f, 0.2f)]
	private float _physicsAngularDamp = 0.02f;

	private const int SHADER_KEYWORD_DEPTH_AWARE_INDEX = 0;

	private const int SHADER_KEYWORD_DEPTH_AWARE_CUSTOM_PASS_INDEX = 1;

	private const int SHADER_KEYWORD_IGNORE_GRAVITY_INDEX = 2;

	private const int SHADER_KEYWORD_NON_AABB_INDEX = 3;

	private const int SHADER_KEYWORD_TOPOLOGY_INDEX = 4;

	private const int SHADER_KEYWORD_REFRACTION_INDEX = 5;

	private const string SHADER_KEYWORD_DEPTH_AWARE = "LIQUID_VOLUME_DEPTH_AWARE";

	private const string SHADER_KEYWORD_DEPTH_AWARE_CUSTOM_PASS = "LIQUID_VOLUME_DEPTH_AWARE_PASS";

	private const string SHADER_KEYWORD_NON_AABB = "LIQUID_VOLUME_NON_AABB";

	private const string SHADER_KEYWORD_IGNORE_GRAVITY = "LIQUID_VOLUME_IGNORE_GRAVITY";

	private const string SHADER_KEYWORD_SPHERE = "LIQUID_VOLUME_SPHERE";

	private const string SHADER_KEYWORD_CUBE = "LIQUID_VOLUME_CUBE";

	private const string SHADER_KEYWORD_CYLINDER = "LIQUID_VOLUME_CYLINDER";

	private const string SHADER_KEYWORD_IRREGULAR = "LIQUID_VOLUME_IRREGULAR";

	private const string SHADER_KEYWORD_FP_RENDER_TEXTURE = "LIQUID_VOLUME_FP_RENDER_TEXTURES";

	private const string SHADER_KEYWORD_USE_REFRACTION = "LIQUID_VOLUME_USE_REFRACTION";

	private const string SPILL_POINT_GIZMO = "SpillPointGizmo";

	[NonSerialized]
	public Material liqMat;

	private Material liqMatSimple;

	private Material liqMatDefaultNoFlask;

	private Mesh mesh;

	[NonSerialized]
	public Renderer mr;

	private static readonly List<Material> mrSharedMaterials = new List<Material>();

	private Vector3 lastPosition;

	private Vector3 lastScale;

	private Quaternion lastRotation;

	private string[] shaderKeywords;

	private bool camInside;

	private float lastDistanceToCam;

	private DETAIL currentDetail;

	private Vector4 turb;

	private Vector4 shaderTurb;

	private float turbulenceSpeed;

	private float murkinessSpeed;

	private float liquidLevelPos;

	private bool shouldUpdateMaterialProperties;

	private int currentNoiseVariation;

	private float levelMultipled;

	private Texture2D noise3DUnwrapped;

	private Texture3D[] noise3DTex;

	private Color[][] colors3D;

	private Vector3[] verticesUnsorted;

	private Vector3[] verticesSorted;

	private static Vector3[] rotatedVertices;

	private int[] verticesIndices;

	private float volumeRef;

	private float lastLevelVolumeRef;

	private Vector3 inertia;

	private Vector3 lastAvgVelocity;

	private float angularVelocity;

	private float angularInertia;

	private float turbulenceDueForces;

	private Quaternion liquidRot;

	private float prevThickness;

	private GameObject spillPointGizmo;

	private static string[] defaultContainerNames = new string[6] { "GLASS", "CONTAINER", "BOTTLE", "POTION", "FLASK", "LIQUID" };

	private Color[] pointLightColorBuffer;

	private Vector4[] pointLightPositionBuffer;

	private int lastPointLightCount;

	private static readonly Dictionary<Mesh, MeshCache> meshCache = new Dictionary<Mesh, MeshCache>();

	private readonly List<Vector3> verts = new List<Vector3>();

	private readonly List<Vector3> cutPoints = new List<Vector3>();

	private Vector3 cutPlaneCenter;

	[SerializeField]
	private Mesh fixedMesh;

	public TOPOLOGY topology
	{
		get
		{
			return _topology;
		}
		set
		{
			if (_topology != value)
			{
				_topology = value;
				UpdateMaterialProperties();
			}
		}
	}

	public DETAIL detail
	{
		get
		{
			return _detail;
		}
		set
		{
			if (_detail != value)
			{
				_detail = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float level
	{
		get
		{
			return _level;
		}
		set
		{
			if (_level != value)
			{
				_level = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float levelMultiplier
	{
		get
		{
			return _levelMultiplier;
		}
		set
		{
			if (_levelMultiplier != value)
			{
				_levelMultiplier = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool useLightColor
	{
		get
		{
			return _useLightColor;
		}
		set
		{
			if (_useLightColor != value)
			{
				_useLightColor = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool useLightDirection
	{
		get
		{
			return _useLightDirection;
		}
		set
		{
			if (_useLightDirection != value)
			{
				_useLightDirection = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Light directionalLight
	{
		get
		{
			return _directionalLight;
		}
		set
		{
			if ((Object)(object)_directionalLight != (Object)(object)value)
			{
				_directionalLight = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Color liquidColor1
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _liquidColor1;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			if (_liquidColor1 != value)
			{
				_liquidColor1 = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float liquidScale1
	{
		get
		{
			return _liquidScale1;
		}
		set
		{
			if (_liquidScale1 != value)
			{
				_liquidScale1 = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Color liquidColor2
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _liquidColor2;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			if (_liquidColor2 != value)
			{
				_liquidColor2 = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float liquidScale2
	{
		get
		{
			return _liquidScale2;
		}
		set
		{
			if (_liquidScale2 != value)
			{
				_liquidScale2 = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float alpha
	{
		get
		{
			return _alpha;
		}
		set
		{
			if (_alpha != Mathf.Clamp01(value))
			{
				_alpha = Mathf.Clamp01(value);
				UpdateMaterialProperties();
			}
		}
	}

	public Color emissionColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _emissionColor;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			if (_emissionColor != value)
			{
				_emissionColor = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool ditherShadows
	{
		get
		{
			return _ditherShadows;
		}
		set
		{
			if (_ditherShadows != value)
			{
				_ditherShadows = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float murkiness
	{
		get
		{
			return _murkiness;
		}
		set
		{
			if (_murkiness != value)
			{
				_murkiness = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float turbulence1
	{
		get
		{
			return _turbulence1;
		}
		set
		{
			if (_turbulence1 != value)
			{
				_turbulence1 = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float turbulence2
	{
		get
		{
			return _turbulence2;
		}
		set
		{
			if (_turbulence2 != value)
			{
				_turbulence2 = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float frecuency
	{
		get
		{
			return _frecuency;
		}
		set
		{
			if (_frecuency != value)
			{
				_frecuency = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float speed
	{
		get
		{
			return _speed;
		}
		set
		{
			if (_speed != value)
			{
				_speed = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sparklingIntensity
	{
		get
		{
			return _sparklingIntensity;
		}
		set
		{
			if (_sparklingIntensity != value)
			{
				_sparklingIntensity = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sparklingAmount
	{
		get
		{
			return _sparklingAmount;
		}
		set
		{
			if (_sparklingAmount != value)
			{
				_sparklingAmount = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float deepObscurance
	{
		get
		{
			return _deepObscurance;
		}
		set
		{
			if (_deepObscurance != value)
			{
				_deepObscurance = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Color foamColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _foamColor;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			if (_foamColor != value)
			{
				_foamColor = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float foamScale
	{
		get
		{
			return _foamScale;
		}
		set
		{
			if (_foamScale != value)
			{
				_foamScale = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float foamThickness
	{
		get
		{
			return _foamThickness;
		}
		set
		{
			if (_foamThickness != value)
			{
				_foamThickness = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float foamDensity
	{
		get
		{
			return _foamDensity;
		}
		set
		{
			if (_foamDensity != value)
			{
				_foamDensity = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float foamWeight
	{
		get
		{
			return _foamWeight;
		}
		set
		{
			if (_foamWeight != value)
			{
				_foamWeight = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float foamTurbulence
	{
		get
		{
			return _foamTurbulence;
		}
		set
		{
			if (_foamTurbulence != value)
			{
				_foamTurbulence = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool foamVisibleFromBottom
	{
		get
		{
			return _foamVisibleFromBottom;
		}
		set
		{
			if (_foamVisibleFromBottom != value)
			{
				_foamVisibleFromBottom = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool smokeEnabled
	{
		get
		{
			return _smokeEnabled;
		}
		set
		{
			if (_smokeEnabled != value)
			{
				_smokeEnabled = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Color smokeColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _smokeColor;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			if (_smokeColor != value)
			{
				_smokeColor = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float smokeScale
	{
		get
		{
			return _smokeScale;
		}
		set
		{
			if (_smokeScale != value)
			{
				_smokeScale = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float smokeBaseObscurance
	{
		get
		{
			return _smokeBaseObscurance;
		}
		set
		{
			if (_smokeBaseObscurance != value)
			{
				_smokeBaseObscurance = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float smokeHeightAtten
	{
		get
		{
			return _smokeHeightAtten;
		}
		set
		{
			if (_smokeHeightAtten != value)
			{
				_smokeHeightAtten = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float smokeSpeed
	{
		get
		{
			return _smokeSpeed;
		}
		set
		{
			if (_smokeSpeed != value)
			{
				_smokeSpeed = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool fixMesh
	{
		get
		{
			return _fixMesh;
		}
		set
		{
			if (_fixMesh != value)
			{
				_fixMesh = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Vector3 pivotOffset
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _pivotOffset;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			if (_pivotOffset != value)
			{
				_pivotOffset = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool limitVerticalRange
	{
		get
		{
			return _limitVerticalRange;
		}
		set
		{
			if (_limitVerticalRange != value)
			{
				_limitVerticalRange = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float upperLimit
	{
		get
		{
			return _upperLimit;
		}
		set
		{
			if (_upperLimit != value)
			{
				_upperLimit = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float lowerLimit
	{
		get
		{
			return _lowerLimit;
		}
		set
		{
			if (_lowerLimit != value)
			{
				_lowerLimit = value;
				UpdateMaterialProperties();
			}
		}
	}

	public int subMeshIndex
	{
		get
		{
			return _subMeshIndex;
		}
		set
		{
			if (_subMeshIndex != value)
			{
				_subMeshIndex = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Material flaskMaterial
	{
		get
		{
			return _flaskMaterial;
		}
		set
		{
			if ((Object)(object)_flaskMaterial != (Object)(object)value)
			{
				_flaskMaterial = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float flaskThickness
	{
		get
		{
			return _flaskThickness;
		}
		set
		{
			if (_flaskThickness != value)
			{
				_flaskThickness = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float glossinessInternal
	{
		get
		{
			return _glossinessInternal;
		}
		set
		{
			if (_glossinessInternal != value)
			{
				_glossinessInternal = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool scatteringEnabled
	{
		get
		{
			return _scatteringEnabled;
		}
		set
		{
			if (_scatteringEnabled != value)
			{
				_scatteringEnabled = value;
				UpdateMaterialProperties();
			}
		}
	}

	public int scatteringPower
	{
		get
		{
			return _scatteringPower;
		}
		set
		{
			if (_scatteringPower != value)
			{
				_scatteringPower = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float scatteringAmount
	{
		get
		{
			return _scatteringAmount;
		}
		set
		{
			if (_scatteringAmount != value)
			{
				_scatteringAmount = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool refractionBlur
	{
		get
		{
			return _refractionBlur;
		}
		set
		{
			if (_refractionBlur != value)
			{
				_refractionBlur = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float blurIntensity
	{
		get
		{
			return _blurIntensity;
		}
		set
		{
			if (_blurIntensity != Mathf.Clamp01(value))
			{
				_blurIntensity = Mathf.Clamp01(value);
				UpdateMaterialProperties();
			}
		}
	}

	public int liquidRaySteps
	{
		get
		{
			return _liquidRaySteps;
		}
		set
		{
			if (_liquidRaySteps != value)
			{
				_liquidRaySteps = value;
				UpdateMaterialProperties();
			}
		}
	}

	public int foamRaySteps
	{
		get
		{
			return _foamRaySteps;
		}
		set
		{
			if (_foamRaySteps != value)
			{
				_foamRaySteps = value;
				UpdateMaterialProperties();
			}
		}
	}

	public int smokeRaySteps
	{
		get
		{
			return _smokeRaySteps;
		}
		set
		{
			if (_smokeRaySteps != value)
			{
				_smokeRaySteps = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Texture2D bumpMap
	{
		get
		{
			return _bumpMap;
		}
		set
		{
			if ((Object)(object)_bumpMap != (Object)(object)value)
			{
				_bumpMap = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float bumpStrength
	{
		get
		{
			return _bumpStrength;
		}
		set
		{
			if (_bumpStrength != value)
			{
				_bumpStrength = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float bumpDistortionScale
	{
		get
		{
			return _bumpDistortionScale;
		}
		set
		{
			if (_bumpDistortionScale != value)
			{
				_bumpDistortionScale = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Vector2 bumpDistortionOffset
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _bumpDistortionOffset;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			if (_bumpDistortionOffset != value)
			{
				_bumpDistortionOffset = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Texture2D distortionMap
	{
		get
		{
			return _distortionMap;
		}
		set
		{
			if ((Object)(object)_distortionMap != (Object)(object)value)
			{
				_distortionMap = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Texture2D texture
	{
		get
		{
			return _texture;
		}
		set
		{
			if ((Object)(object)_texture != (Object)(object)value)
			{
				_texture = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Vector2 textureScale
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _textureScale;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			if (_textureScale != value)
			{
				_textureScale = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Vector2 textureOffset
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _textureOffset;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			if (_textureOffset != value)
			{
				_textureOffset = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float distortionAmount
	{
		get
		{
			return _distortionAmount;
		}
		set
		{
			if (_distortionAmount != value)
			{
				_distortionAmount = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool depthAware
	{
		get
		{
			return _depthAware;
		}
		set
		{
			if (_depthAware != value)
			{
				_depthAware = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float depthAwareOffset
	{
		get
		{
			return _depthAwareOffset;
		}
		set
		{
			if (_depthAwareOffset != value)
			{
				_depthAwareOffset = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool irregularDepthDebug
	{
		get
		{
			return _irregularDepthDebug;
		}
		set
		{
			if (_irregularDepthDebug != value)
			{
				_irregularDepthDebug = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool depthAwareCustomPass
	{
		get
		{
			return _depthAwareCustomPass;
		}
		set
		{
			if (_depthAwareCustomPass != value)
			{
				_depthAwareCustomPass = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool depthAwareCustomPassDebug
	{
		get
		{
			return _depthAwareCustomPassDebug;
		}
		set
		{
			if (_depthAwareCustomPassDebug != value)
			{
				_depthAwareCustomPassDebug = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float doubleSidedBias
	{
		get
		{
			return _doubleSidedBias;
		}
		set
		{
			if (_doubleSidedBias != value)
			{
				_doubleSidedBias = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float backDepthBias
	{
		get
		{
			return _backDepthBias;
		}
		set
		{
			if (value < 0f)
			{
				value = 0f;
			}
			if (_backDepthBias != value)
			{
				_backDepthBias = value;
				UpdateMaterialProperties();
			}
		}
	}

	public LEVEL_COMPENSATION rotationLevelCompensation
	{
		get
		{
			return _rotationLevelCompensation;
		}
		set
		{
			if (_rotationLevelCompensation != value)
			{
				_rotationLevelCompensation = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool ignoreGravity
	{
		get
		{
			return _ignoreGravity;
		}
		set
		{
			if (_ignoreGravity != value)
			{
				_ignoreGravity = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool reactToForces
	{
		get
		{
			return _reactToForces;
		}
		set
		{
			if (_reactToForces != value)
			{
				_reactToForces = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Vector3 extentsScale
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _extentsScale;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			if (_extentsScale != value)
			{
				_extentsScale = value;
				UpdateMaterialProperties();
			}
		}
	}

	public int noiseVariation
	{
		get
		{
			return _noiseVariation;
		}
		set
		{
			if (_noiseVariation != value)
			{
				_noiseVariation = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool allowViewFromInside
	{
		get
		{
			return _allowViewFromInside;
		}
		set
		{
			if (_allowViewFromInside != value)
			{
				_allowViewFromInside = value;
				lastDistanceToCam = -1f;
				CheckInsideOut();
			}
		}
	}

	public bool debugSpillPoint
	{
		get
		{
			return _debugSpillPoint;
		}
		set
		{
			if (_debugSpillPoint != value)
			{
				_debugSpillPoint = value;
			}
		}
	}

	public int renderQueue
	{
		get
		{
			return _renderQueue;
		}
		set
		{
			if (_renderQueue != value)
			{
				_renderQueue = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Cubemap reflectionTexture
	{
		get
		{
			return _reflectionTexture;
		}
		set
		{
			if ((Object)(object)_reflectionTexture != (Object)(object)value)
			{
				_reflectionTexture = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float physicsMass
	{
		get
		{
			return _physicsMass;
		}
		set
		{
			if (_physicsMass != value)
			{
				_physicsMass = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float physicsAngularDamp
	{
		get
		{
			return _physicsAngularDamp;
		}
		set
		{
			if (_physicsAngularDamp != value)
			{
				_physicsAngularDamp = value;
				UpdateMaterialProperties();
			}
		}
	}

	public static bool useFPRenderTextures => true;

	public float liquidSurfaceYPosition => liquidLevelPos;

	public event PropertiesChangedEvent onPropertiesChanged;

	private void OnEnable()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		if (((Component)this).gameObject.activeInHierarchy)
		{
			levelMultipled = _level * _levelMultiplier;
			turb.z = 1f;
			turbulenceDueForces = 0f;
			turbulenceSpeed = 1f;
			liquidRot = ((Component)this).transform.rotation;
			currentDetail = _detail;
			currentNoiseVariation = -1;
			lastPosition = ((Component)this).transform.position;
			lastRotation = ((Component)this).transform.rotation;
			lastScale = ((Component)this).transform.localScale;
			prevThickness = _flaskThickness;
			if (_depthAwareCustomPass && (Object)(object)((Component)this).transform.parent == (Object)null)
			{
				_depthAwareCustomPass = false;
			}
			UpdateMaterialPropertiesNow();
			if (!Application.isPlaying)
			{
				shouldUpdateMaterialProperties = true;
			}
		}
	}

	private void Reset()
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)mesh == (Object)null)
		{
			return;
		}
		if (mesh.vertexCount == 24)
		{
			topology = TOPOLOGY.Cube;
			return;
		}
		Renderer component = ((Component)this).GetComponent<Renderer>();
		Bounds bounds;
		if ((Object)(object)component == (Object)null)
		{
			bounds = mesh.bounds;
			float y = ((Bounds)(ref bounds)).extents.y;
			bounds = mesh.bounds;
			if (y > ((Bounds)(ref bounds)).extents.x)
			{
				topology = TOPOLOGY.Cylinder;
			}
			return;
		}
		bounds = component.bounds;
		float y2 = ((Bounds)(ref bounds)).extents.y;
		bounds = component.bounds;
		if (!(y2 > ((Bounds)(ref bounds)).extents.x))
		{
			return;
		}
		topology = TOPOLOGY.Cylinder;
		if (Application.isPlaying)
		{
			return;
		}
		Quaternion rotation = ((Component)this).transform.rotation;
		if (!(((Quaternion)(ref rotation)).eulerAngles != Vector3.zero))
		{
			return;
		}
		bounds = mesh.bounds;
		float y3 = ((Bounds)(ref bounds)).extents.y;
		bounds = mesh.bounds;
		if (!(y3 <= ((Bounds)(ref bounds)).extents.x))
		{
			bounds = mesh.bounds;
			float y4 = ((Bounds)(ref bounds)).extents.y;
			bounds = mesh.bounds;
			if (!(y4 <= ((Bounds)(ref bounds)).extents.z))
			{
				return;
			}
		}
		Debug.LogWarning((object)"Intrinsic model rotation detected. Consider using the Bake Transform and/or Center Pivot options in Advanced section.");
	}

	private void OnDestroy()
	{
		RestoreOriginalMesh();
		liqMat = null;
		if ((Object)(object)liqMatSimple != (Object)null)
		{
			Object.DestroyImmediate((Object)(object)liqMatSimple);
			liqMatSimple = null;
		}
		if ((Object)(object)liqMatDefaultNoFlask != (Object)null)
		{
			Object.DestroyImmediate((Object)(object)liqMatDefaultNoFlask);
			liqMatDefaultNoFlask = null;
		}
		if (noise3DTex != null)
		{
			for (int i = 0; i < noise3DTex.Length; i++)
			{
				Texture3D val = noise3DTex[i];
				if ((Object)(object)val != (Object)null && ((Object)val).name.Contains("Clone"))
				{
					Object.DestroyImmediate((Object)(object)val);
					noise3DTex[i] = null;
				}
			}
		}
		LiquidVolumeDepthPrePassRenderFeature.RemoveLiquidFromBackRenderers(this);
		LiquidVolumeDepthPrePassRenderFeature.RemoveLiquidFromFrontRenderers(this);
	}

	private void RenderObject()
	{
		bool flag = ((Component)this).gameObject.activeInHierarchy && ((Behaviour)this).enabled;
		if (shouldUpdateMaterialProperties || !Application.isPlaying)
		{
			shouldUpdateMaterialProperties = false;
			UpdateMaterialPropertiesNow();
		}
		if (flag && _allowViewFromInside)
		{
			CheckInsideOut();
		}
		UpdateAnimations();
		if (!flag || _topology != TOPOLOGY.Irregular)
		{
			LiquidVolumeDepthPrePassRenderFeature.RemoveLiquidFromBackRenderers(this);
		}
		else if (_topology == TOPOLOGY.Irregular)
		{
			LiquidVolumeDepthPrePassRenderFeature.AddLiquidToBackRenderers(this);
		}
		if ((Object)(object)((Component)this).transform.parent != (Object)null)
		{
			((Component)this).GetComponentInParent<Renderer>();
			if (!flag || !_depthAwareCustomPass)
			{
				LiquidVolumeDepthPrePassRenderFeature.RemoveLiquidFromFrontRenderers(this);
			}
			else if (_depthAwareCustomPass)
			{
				LiquidVolumeDepthPrePassRenderFeature.AddLiquidToFrontRenderers(this);
			}
		}
		if (_debugSpillPoint)
		{
			UpdateSpillPointGizmo();
		}
	}

	public void OnWillRenderObject()
	{
		RenderObject();
	}

	private void FixedUpdate()
	{
		turbulenceSpeed += Time.deltaTime * 3f * _speed;
		liqMat.SetFloat(ShaderParams.TurbulenceSpeed, turbulenceSpeed * 4f);
		murkinessSpeed += Time.deltaTime * 0.05f * (shaderTurb.x + shaderTurb.y);
		liqMat.SetFloat(ShaderParams.MurkinessSpeed, murkinessSpeed);
	}

	private void OnDidApplyAnimationProperties()
	{
		shouldUpdateMaterialProperties = true;
	}

	public void ClearMeshCache()
	{
		meshCache.Clear();
	}

	private void ReadVertices()
	{
		if ((Object)(object)mesh == (Object)null)
		{
			return;
		}
		if (!meshCache.TryGetValue(mesh, out var value))
		{
			if (!mesh.isReadable)
			{
				Debug.LogError((object)("Mesh " + ((Object)mesh).name + " is not readable. Please select your mesh and enable the Read/Write Enabled option."));
			}
			verticesUnsorted = mesh.vertices;
			verticesIndices = mesh.triangles;
			int num = verticesUnsorted.Length;
			if (verticesSorted == null || verticesSorted.Length != num)
			{
				verticesSorted = (Vector3[])(object)new Vector3[num];
			}
			Array.Copy(verticesUnsorted, verticesSorted, num);
			Array.Sort(verticesSorted, vertexComparer);
			value.verticesUnsorted = verticesUnsorted;
			value.indices = verticesIndices;
			value.verticesSorted = verticesSorted;
			if (meshCache.Count > 64)
			{
				ClearMeshCache();
			}
			meshCache[mesh] = value;
		}
		else
		{
			verticesUnsorted = value.verticesUnsorted;
			verticesIndices = value.indices;
			verticesSorted = value.verticesSorted;
		}
	}

	private int vertexComparer(Vector3 v0, Vector3 v1)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (v1.y < v0.y)
		{
			return -1;
		}
		if (v1.y > v0.y)
		{
			return 1;
		}
		return 0;
	}

	private void UpdateAnimations()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0409: Unknown result type (might be due to invalid IL or missing references)
		//IL_040f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_042a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0430: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0442: Unknown result type (might be due to invalid IL or missing references)
		//IL_0448: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_038f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		switch (topology)
		{
		case TOPOLOGY.Sphere:
			if (((Component)this).transform.localScale.y != ((Component)this).transform.localScale.x || ((Component)this).transform.localScale.z != ((Component)this).transform.localScale.x)
			{
				((Component)this).transform.localScale = new Vector3(((Component)this).transform.localScale.x, ((Component)this).transform.localScale.x, ((Component)this).transform.localScale.x);
			}
			break;
		case TOPOLOGY.Cylinder:
			if (((Component)this).transform.localScale.z != ((Component)this).transform.localScale.x)
			{
				((Component)this).transform.localScale = new Vector3(((Component)this).transform.localScale.x, ((Component)this).transform.localScale.y, ((Component)this).transform.localScale.x);
			}
			break;
		}
		if ((Object)(object)liqMat != (Object)null)
		{
			Vector3 val = Vector3.right;
			Quaternion rotation = ((Component)this).transform.rotation;
			if (_reactToForces)
			{
				Quaternion val2 = ((Component)this).transform.rotation;
				float deltaTime = Time.deltaTime;
				if (Application.isPlaying && deltaTime > 0f)
				{
					Vector3 val3 = (((Component)this).transform.position - lastPosition) / deltaTime;
					Vector3 val4 = val3 - lastAvgVelocity;
					lastAvgVelocity = val3;
					inertia += val3;
					float num = Mathf.Max(((Vector3)(ref val4)).magnitude / _physicsMass - _physicsAngularDamp * 150f * deltaTime, 0f);
					angularInertia += num;
					angularVelocity += angularInertia;
					if (angularVelocity > 0f)
					{
						angularInertia -= Mathf.Abs(angularVelocity) * deltaTime * _physicsMass;
					}
					else if (angularVelocity < 0f)
					{
						angularInertia += Mathf.Abs(angularVelocity) * deltaTime * _physicsMass;
					}
					float num2 = 1f - _physicsAngularDamp;
					angularInertia *= num2;
					inertia *= num2;
					float num3 = Mathf.Clamp(angularVelocity, -90f, 90f);
					float magnitude = ((Vector3)(ref inertia)).magnitude;
					if (magnitude > 0f)
					{
						val = inertia / magnitude;
					}
					Vector3 val5 = Vector3.Cross(val, Vector3.down);
					val2 = Quaternion.AngleAxis(num3, val5);
					float num4 = Mathf.Abs(angularInertia) + Mathf.Abs(angularVelocity);
					turbulenceDueForces = Mathf.Min(0.5f / _physicsMass, turbulenceDueForces + num4 / 1000f);
					turbulenceDueForces *= num2;
				}
				else
				{
					turbulenceDueForces = 0f;
				}
				if (_topology == TOPOLOGY.Sphere)
				{
					liquidRot = Quaternion.Lerp(liquidRot, val2, 0.1f);
					rotation = liquidRot;
				}
			}
			else if (turbulenceDueForces > 0f)
			{
				turbulenceDueForces *= 0.1f;
			}
			Matrix4x4 val6 = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
			liqMat.SetMatrix(ShaderParams.RotationMatrix, ((Matrix4x4)(ref val6)).inverse);
			if (_topology != TOPOLOGY.Sphere)
			{
				float x = val.x;
				val.x += (val.z - val.x) * 0.25f;
				val.z += (x - val.z) * 0.25f;
			}
			turb.z = val.x;
			turb.w = val.z;
		}
		bool flag = ((Component)this).transform.rotation != lastRotation;
		if (_reactToForces || flag || ((Component)this).transform.position != lastPosition || ((Component)this).transform.localScale != lastScale)
		{
			UpdateLevels(flag);
		}
	}

	public void UpdateMaterialProperties()
	{
		if (Application.isPlaying)
		{
			shouldUpdateMaterialProperties = true;
		}
		else
		{
			UpdateMaterialPropertiesNow();
		}
	}

	private void UpdateMaterialPropertiesNow()
	{
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_037d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0357: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0437: Unknown result type (might be due to invalid IL or missing references)
		//IL_0553: Unknown result type (might be due to invalid IL or missing references)
		//IL_0558: Unknown result type (might be due to invalid IL or missing references)
		//IL_055d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0589: Unknown result type (might be due to invalid IL or missing references)
		//IL_0609: Unknown result type (might be due to invalid IL or missing references)
		//IL_060e: Unknown result type (might be due to invalid IL or missing references)
		if (!((Component)this).gameObject.activeInHierarchy)
		{
			return;
		}
		DETAIL dETAIL = _detail;
		if ((uint)dETAIL <= 1u)
		{
			if ((Object)(object)liqMatSimple == (Object)null)
			{
				liqMatSimple = Object.Instantiate<Material>(Resources.Load<Material>("Materials/LiquidVolumeSimple"));
			}
			liqMat = liqMatSimple;
		}
		else
		{
			if ((Object)(object)liqMatDefaultNoFlask == (Object)null)
			{
				liqMatDefaultNoFlask = Object.Instantiate<Material>(Resources.Load<Material>("Materials/LiquidVolumeDefaultNoFlask"));
			}
			liqMat = liqMatDefaultNoFlask;
		}
		if ((Object)(object)_flaskMaterial == (Object)null)
		{
			_flaskMaterial = Object.Instantiate<Material>(Resources.Load<Material>("Materials/Flask"));
		}
		if ((Object)(object)liqMat == (Object)null)
		{
			return;
		}
		CheckMeshDisplacement();
		if (currentDetail != _detail)
		{
			currentDetail = _detail;
		}
		UpdateLevels();
		if ((Object)(object)mr == (Object)null)
		{
			return;
		}
		mr.GetSharedMaterials(mrSharedMaterials);
		int count = mrSharedMaterials.Count;
		if (_subMeshIndex < 0)
		{
			for (int i = 0; i < defaultContainerNames.Length; i++)
			{
				if (_subMeshIndex >= 0)
				{
					break;
				}
				for (int j = 0; j < count; j++)
				{
					if ((Object)(object)mrSharedMaterials[j] != (Object)null && (Object)(object)mrSharedMaterials[j] != (Object)(object)_flaskMaterial && ((Object)mrSharedMaterials[j]).name.ToUpper().Contains(defaultContainerNames[i]))
					{
						_subMeshIndex = j;
						break;
					}
				}
			}
		}
		if (_subMeshIndex < 0)
		{
			_subMeshIndex = 0;
		}
		if (count > 1 && _subMeshIndex >= 0 && _subMeshIndex < count)
		{
			mrSharedMaterials[_subMeshIndex] = liqMat;
		}
		else
		{
			mrSharedMaterials.Clear();
			mrSharedMaterials.Add(liqMat);
		}
		if ((Object)(object)_flaskMaterial != (Object)null)
		{
			bool flag = _detail.usesFlask();
			if (flag && !mrSharedMaterials.Contains(_flaskMaterial))
			{
				for (int k = 0; k < mrSharedMaterials.Count; k++)
				{
					if ((Object)(object)mrSharedMaterials[k] == (Object)null)
					{
						mrSharedMaterials[k] = _flaskMaterial;
						flag = false;
					}
				}
				if (flag)
				{
					mrSharedMaterials.Add(_flaskMaterial);
				}
			}
			else if (!flag && mrSharedMaterials.Contains(_flaskMaterial))
			{
				mrSharedMaterials.Remove(_flaskMaterial);
			}
			_flaskMaterial.SetFloat(ShaderParams.QueueOffset, (float)(_renderQueue - 3000));
			_flaskMaterial.SetFloat(ShaderParams.PreserveSpecular, 0f);
		}
		mr.sharedMaterials = mrSharedMaterials.ToArray();
		liqMat.SetColor(ShaderParams.Color1, ApplyGlobalAlpha(_liquidColor1));
		liqMat.SetColor(ShaderParams.Color2, ApplyGlobalAlpha(_liquidColor2));
		liqMat.SetColor(ShaderParams.EmissionColor, _emissionColor);
		if (_useLightColor && (Object)(object)_directionalLight != (Object)null)
		{
			Color color = _directionalLight.color;
			liqMat.SetColor(ShaderParams.LightColor, color);
		}
		else
		{
			liqMat.SetColor(ShaderParams.LightColor, Color.white);
		}
		if (_useLightDirection && (Object)(object)_directionalLight != (Object)null)
		{
			liqMat.SetVector(ShaderParams.LightDir, Vector4.op_Implicit(-((Component)_directionalLight).transform.forward));
		}
		else
		{
			liqMat.SetVector(ShaderParams.LightDir, Vector4.op_Implicit(Vector3.up));
		}
		int num = _scatteringPower;
		float num2 = _scatteringAmount;
		if (!_scatteringEnabled)
		{
			num = 0;
			num2 = 0f;
		}
		liqMat.SetVector(ShaderParams.GlossinessInt, new Vector4((1f - _glossinessInternal) * 96f + 1f, Mathf.Pow(2f, (float)num), num2, _glossinessInternal));
		liqMat.SetFloat(ShaderParams.DoubleSidedBias, _doubleSidedBias);
		liqMat.SetFloat(ShaderParams.BackDepthBias, 0f - _backDepthBias);
		liqMat.SetFloat(ShaderParams.Muddy, _murkiness);
		liqMat.SetFloat(ShaderParams.Alpha, _alpha);
		float num3 = _alpha * Mathf.Clamp01((_liquidColor1.a + _liquidColor2.a) * 4f);
		if (_ditherShadows)
		{
			liqMat.SetFloat(ShaderParams.AlphaCombined, num3);
		}
		else
		{
			liqMat.SetFloat(ShaderParams.AlphaCombined, (num3 > 0f) ? 1000f : 0f);
		}
		liqMat.SetFloat(ShaderParams.SparklingIntensity, _sparklingIntensity * 250f);
		liqMat.SetFloat(ShaderParams.SparklingThreshold, 1f - _sparklingAmount);
		liqMat.SetFloat(ShaderParams.DepthAtten, _deepObscurance);
		Color val = ApplyGlobalAlpha(_smokeColor);
		int num4 = _smokeRaySteps;
		if (!_smokeEnabled)
		{
			val.a = 0f;
			num4 = 1;
		}
		liqMat.SetColor(ShaderParams.SmokeColor, val);
		liqMat.SetFloat(ShaderParams.SmokeAtten, _smokeBaseObscurance);
		liqMat.SetFloat(ShaderParams.SmokeHeightAtten, _smokeHeightAtten);
		liqMat.SetFloat(ShaderParams.SmokeSpeed, _smokeSpeed);
		liqMat.SetFloat(ShaderParams.SmokeRaySteps, (float)num4);
		liqMat.SetFloat(ShaderParams.LiquidRaySteps, (float)_liquidRaySteps);
		liqMat.SetColor(ShaderParams.FoamColor, ApplyGlobalAlpha(_foamColor));
		liqMat.SetFloat(ShaderParams.FoamRaySteps, (float)((!(_foamThickness > 0f)) ? 1 : _foamRaySteps));
		liqMat.SetFloat(ShaderParams.FoamDensity, (_foamThickness > 0f) ? _foamDensity : (-1f));
		liqMat.SetFloat(ShaderParams.FoamWeight, _foamWeight);
		liqMat.SetFloat(ShaderParams.FoamBottom, _foamVisibleFromBottom ? 1f : 0f);
		liqMat.SetFloat(ShaderParams.FoamTurbulence, _foamTurbulence);
		if (_noiseVariation != currentNoiseVariation)
		{
			currentNoiseVariation = _noiseVariation;
			if (noise3DTex == null || noise3DTex.Length != 4)
			{
				noise3DTex = (Texture3D[])(object)new Texture3D[4];
			}
			if ((Object)(object)noise3DTex[currentNoiseVariation] == (Object)null)
			{
				noise3DTex[currentNoiseVariation] = Resources.Load<Texture3D>("Textures/Noise3D" + currentNoiseVariation);
			}
			Texture3D val2 = noise3DTex[currentNoiseVariation];
			if ((Object)(object)val2 != (Object)null)
			{
				liqMat.SetTexture(ShaderParams.NoiseTex, (Texture)(object)val2);
			}
		}
		liqMat.renderQueue = _renderQueue;
		UpdateInsideOut();
		if (_topology == TOPOLOGY.Irregular && prevThickness != _flaskThickness)
		{
			prevThickness = _flaskThickness;
		}
		this.onPropertiesChanged?.Invoke(this);
	}

	private Color ApplyGlobalAlpha(Color originalColor)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		return new Color(originalColor.r, originalColor.g, originalColor.b, originalColor.a * _alpha);
	}

	private void GetRenderer()
	{
		MeshFilter component = ((Component)this).GetComponent<MeshFilter>();
		if ((Object)(object)component != (Object)null)
		{
			mesh = component.sharedMesh;
			mr = (Renderer)(object)((Component)this).GetComponent<MeshRenderer>();
			return;
		}
		SkinnedMeshRenderer component2 = ((Component)this).GetComponent<SkinnedMeshRenderer>();
		if ((Object)(object)component2 != (Object)null)
		{
			mesh = component2.sharedMesh;
			mr = (Renderer)(object)component2;
		}
	}

	private void UpdateLevels(bool updateShaderKeywords = true)
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_047b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0480: Unknown result type (might be due to invalid IL or missing references)
		//IL_0484: Unknown result type (might be due to invalid IL or missing references)
		//IL_048e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0523: Unknown result type (might be due to invalid IL or missing references)
		//IL_0528: Unknown result type (might be due to invalid IL or missing references)
		//IL_052c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0605: Unknown result type (might be due to invalid IL or missing references)
		//IL_061a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0637: Unknown result type (might be due to invalid IL or missing references)
		//IL_0640: Unknown result type (might be due to invalid IL or missing references)
		//IL_064c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0661: Unknown result type (might be due to invalid IL or missing references)
		//IL_0666: Unknown result type (might be due to invalid IL or missing references)
		//IL_066a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0675: Unknown result type (might be due to invalid IL or missing references)
		//IL_067a: Unknown result type (might be due to invalid IL or missing references)
		//IL_067e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0698: Unknown result type (might be due to invalid IL or missing references)
		//IL_069e: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_070c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0711: Unknown result type (might be due to invalid IL or missing references)
		//IL_0402: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_035e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_07dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_08bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_08df: Unknown result type (might be due to invalid IL or missing references)
		//IL_08e4: Unknown result type (might be due to invalid IL or missing references)
		_level = Mathf.Clamp01(_level);
		levelMultipled = _level * _levelMultiplier;
		if ((Object)(object)liqMat == (Object)null)
		{
			return;
		}
		if ((Object)(object)mesh == (Object)null)
		{
			GetRenderer();
			ReadVertices();
		}
		else if ((Object)(object)mr == (Object)null)
		{
			GetRenderer();
		}
		if ((Object)(object)mesh == (Object)null || (Object)(object)mr == (Object)null)
		{
			return;
		}
		Bounds bounds = mesh.bounds;
		float num = ((Bounds)(ref bounds)).extents.x * 2f * ((Component)this).transform.lossyScale.x;
		bounds = mesh.bounds;
		float num2 = ((Bounds)(ref bounds)).extents.y * 2f * ((Component)this).transform.lossyScale.y;
		bounds = mesh.bounds;
		Vector4 val = default(Vector4);
		((Vector4)(ref val))._002Ector(num, num2, ((Bounds)(ref bounds)).extents.z * 2f * ((Component)this).transform.lossyScale.z, 0f);
		val.x *= _extentsScale.x;
		val.y *= _extentsScale.y;
		val.z *= _extentsScale.z;
		float num3 = Mathf.Max(val.x, val.z);
		_003F val2;
		if (!_ignoreGravity)
		{
			bounds = mr.bounds;
			val2 = ((Bounds)(ref bounds)).extents;
		}
		else
		{
			val2 = new Vector3(val.x * 0.5f, val.y * 0.5f, val.z * 0.5f);
		}
		Vector3 val3 = (Vector3)val2;
		val3 *= 1f - _flaskThickness;
		val3.x *= _extentsScale.x;
		val3.y *= _extentsScale.y;
		val3.z *= _extentsScale.z;
		float num4;
		if (_upperLimit < 1f && !_ignoreGravity)
		{
			float y = ((Component)this).transform.TransformPoint(Vector3.up * val3.y).y;
			num4 = Mathf.Max(((Component)this).transform.TransformPoint(Vector3.up * (val3.y * _upperLimit)).y - y, 0f);
		}
		else
		{
			num4 = 0f;
		}
		float num5 = levelMultipled;
		if (_rotationLevelCompensation != LEVEL_COMPENSATION.None && !_ignoreGravity && num5 > 0f)
		{
			MeshVolumeCalcFunction meshVolumeCalcFunction;
			int num6;
			if (_rotationLevelCompensation == LEVEL_COMPENSATION.Fast)
			{
				meshVolumeCalcFunction = GetMeshVolumeUnderLevelFast;
				num6 = 8;
			}
			else
			{
				meshVolumeCalcFunction = GetMeshVolumeUnderLevel;
				num6 = 10;
			}
			if (lastLevelVolumeRef != num5)
			{
				lastLevelVolumeRef = num5;
				if (_topology == TOPOLOGY.Cylinder)
				{
					float num7 = val.x * 0.5f;
					float num8 = val.y * num5;
					volumeRef = (float)Math.PI * num7 * num7 * num8;
				}
				else
				{
					Quaternion rotation = ((Component)this).transform.rotation;
					((Component)this).transform.rotation = Quaternion.identity;
					float num9;
					if (!_ignoreGravity)
					{
						bounds = mr.bounds;
						num9 = ((Bounds)(ref bounds)).extents.y;
					}
					else
					{
						num9 = val.y * 0.5f;
					}
					float num10 = num9;
					num10 *= 1f - _flaskThickness;
					num10 *= _extentsScale.y;
					RotateVertices();
					volumeRef = meshVolumeCalcFunction(num5, num10);
					((Component)this).transform.rotation = rotation;
				}
			}
			RotateVertices();
			float num11 = num5;
			float num12 = float.MaxValue;
			float num13 = Mathf.Clamp01(num5 + 0.5f);
			float num14 = Mathf.Clamp01(num5 - 0.5f);
			for (int i = 0; i < 12; i++)
			{
				num5 = (num14 + num13) * 0.5f;
				float num15 = meshVolumeCalcFunction(num5, val3.y);
				float num16 = Mathf.Abs(volumeRef - num15);
				if (num16 < num12)
				{
					num12 = num16;
					num11 = num5;
				}
				if (num15 < volumeRef)
				{
					num14 = num5;
					continue;
				}
				if (i >= num6)
				{
					break;
				}
				num13 = num5;
			}
			num5 = num11 * _levelMultiplier;
		}
		else if (levelMultipled <= 0f)
		{
			num5 = -0.001f;
		}
		bounds = mr.bounds;
		liquidLevelPos = ((Bounds)(ref bounds)).center.y - val3.y;
		liquidLevelPos += val3.y * 2f * num5 + num4;
		liqMat.SetFloat(ShaderParams.LevelPos, liquidLevelPos);
		bounds = mesh.bounds;
		float num17 = ((Bounds)(ref bounds)).extents.y * _extentsScale.y * _upperLimit;
		liqMat.SetFloat(ShaderParams.UpperLimit, _limitVerticalRange ? num17 : float.MaxValue);
		bounds = mesh.bounds;
		float num18 = ((Bounds)(ref bounds)).extents.y * _extentsScale.y * _lowerLimit;
		liqMat.SetFloat(ShaderParams.LowerLimit, _limitVerticalRange ? num18 : float.MinValue);
		float num19 = ((levelMultipled <= 0f || levelMultipled >= 1f) ? 0f : 1f);
		UpdateTurbulence();
		bounds = mr.bounds;
		float num20 = ((Bounds)(ref bounds)).center.y - val3.y + (num4 + val3.y * 2f * (num5 + _foamThickness)) * num19;
		liqMat.SetFloat(ShaderParams.FoamMaxPos, num20);
		Vector4 val4 = default(Vector4);
		((Vector4)(ref val4))._002Ector(1f - _flaskThickness, 1f - _flaskThickness * num3 / val.z, 1f - _flaskThickness * num3 / val.z, 0f);
		liqMat.SetVector(ShaderParams.FlaskThickness, val4);
		val.w = val.x * 0.5f * val4.x;
		bounds = mr.bounds;
		Vector3 max = ((Bounds)(ref bounds)).max;
		bounds = mr.bounds;
		val.x = Vector3.Distance(max, ((Bounds)(ref bounds)).min);
		liqMat.SetVector(ShaderParams.Size, val);
		float num21 = val.y * 0.5f * (1f - _flaskThickness * num3 / val.y);
		liqMat.SetVector(ShaderParams.Scale, new Vector4(_smokeScale / num21, _foamScale / num21, _liquidScale1 / num21, _liquidScale2 / num21));
		liqMat.SetVector(ShaderParams.Center, Vector4.op_Implicit(((Component)this).transform.position));
		if (shaderKeywords == null || shaderKeywords.Length != 6)
		{
			shaderKeywords = new string[6];
		}
		for (int j = 0; j < shaderKeywords.Length; j++)
		{
			shaderKeywords[j] = null;
		}
		if (_depthAware)
		{
			shaderKeywords[0] = "LIQUID_VOLUME_DEPTH_AWARE";
			liqMat.SetFloat(ShaderParams.DepthAwareOffset, _depthAwareOffset);
		}
		if (_depthAwareCustomPass)
		{
			shaderKeywords[1] = "LIQUID_VOLUME_DEPTH_AWARE_PASS";
		}
		if (_reactToForces && _topology == TOPOLOGY.Sphere)
		{
			shaderKeywords[2] = "LIQUID_VOLUME_IGNORE_GRAVITY";
		}
		else if (_ignoreGravity)
		{
			shaderKeywords[2] = "LIQUID_VOLUME_IGNORE_GRAVITY";
		}
		else
		{
			Quaternion rotation2 = ((Component)this).transform.rotation;
			if (((Quaternion)(ref rotation2)).eulerAngles != Vector3.zero)
			{
				shaderKeywords[3] = "LIQUID_VOLUME_NON_AABB";
			}
		}
		switch (_topology)
		{
		case TOPOLOGY.Sphere:
			shaderKeywords[4] = "LIQUID_VOLUME_SPHERE";
			break;
		case TOPOLOGY.Cube:
			shaderKeywords[4] = "LIQUID_VOLUME_CUBE";
			break;
		case TOPOLOGY.Cylinder:
			shaderKeywords[4] = "LIQUID_VOLUME_CYLINDER";
			break;
		default:
			shaderKeywords[4] = "LIQUID_VOLUME_IRREGULAR";
			break;
		}
		if (_refractionBlur && _detail.allowsRefraction())
		{
			liqMat.SetFloat(ShaderParams.FlaskBlurIntensity, _blurIntensity * (_refractionBlur ? 1f : 0f));
			shaderKeywords[5] = "LIQUID_VOLUME_USE_REFRACTION";
		}
		if (updateShaderKeywords)
		{
			liqMat.shaderKeywords = shaderKeywords;
		}
		lastPosition = ((Component)this).transform.position;
		lastScale = ((Component)this).transform.localScale;
		lastRotation = ((Component)this).transform.rotation;
	}

	private void RotateVertices()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		int num = verticesUnsorted.Length;
		if (rotatedVertices == null || rotatedVertices.Length != num)
		{
			rotatedVertices = (Vector3[])(object)new Vector3[num];
		}
		for (int i = 0; i < num; i++)
		{
			rotatedVertices[i] = ((Component)this).transform.TransformPoint(verticesUnsorted[i]);
		}
	}

	private float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 zeroPoint)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		p1.x -= zeroPoint.x;
		p1.y -= zeroPoint.y;
		p1.z -= zeroPoint.z;
		p2.x -= zeroPoint.x;
		p2.y -= zeroPoint.y;
		p2.z -= zeroPoint.z;
		p3.x -= zeroPoint.x;
		p3.y -= zeroPoint.y;
		p3.z -= zeroPoint.z;
		float num = p3.x * p2.y * p1.z;
		float num2 = p2.x * p3.y * p1.z;
		float num3 = p3.x * p1.y * p2.z;
		float num4 = p1.x * p3.y * p2.z;
		float num5 = p2.x * p1.y * p3.z;
		float num6 = p1.x * p2.y * p3.z;
		return 1f / 6f * (0f - num + num2 + num3 - num4 - num5 + num6);
	}

	public float GetMeshVolumeUnderLevelFast(float level01, float yExtent)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		Bounds bounds = mr.bounds;
		float num = ((Bounds)(ref bounds)).center.y - yExtent;
		num += yExtent * 2f * level01;
		return GetMeshVolumeUnderLevelWSFast(num);
	}

	public float GetMeshVolumeWSFast()
	{
		return GetMeshVolumeUnderLevelWSFast(float.MaxValue);
	}

	public float GetMeshVolumeUnderLevelWSFast(float level)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		Bounds bounds = mr.bounds;
		Vector3 center = ((Bounds)(ref bounds)).center;
		float num = 0f;
		for (int i = 0; i < verticesIndices.Length; i += 3)
		{
			Vector3 val = rotatedVertices[verticesIndices[i]];
			Vector3 val2 = rotatedVertices[verticesIndices[i + 1]];
			Vector3 val3 = rotatedVertices[verticesIndices[i + 2]];
			if (val.y > level)
			{
				val.y = level;
			}
			if (val2.y > level)
			{
				val2.y = level;
			}
			if (val3.y > level)
			{
				val3.y = level;
			}
			num += SignedVolumeOfTriangle(val, val2, val3, center);
		}
		return Mathf.Abs(num);
	}

	private Vector3 ClampVertexToSlicePlane(Vector3 p, Vector3 q, float level)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = q - p;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		float num = p.y - level;
		return p + normalized * num / (0f - normalized.y);
	}

	public float GetMeshVolumeUnderLevel(float level01, float yExtent)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		Bounds bounds = mr.bounds;
		float num = ((Bounds)(ref bounds)).center.y - yExtent;
		num += yExtent * 2f * level01;
		return GetMeshVolumeUnderLevelWS(num);
	}

	public float GetMeshVolumeWS()
	{
		return GetMeshVolumeUnderLevelWS(float.MaxValue);
	}

	public float GetMeshVolumeUnderLevelWS(float level)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0524: Unknown result type (might be due to invalid IL or missing references)
		//IL_0529: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_054c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0551: Unknown result type (might be due to invalid IL or missing references)
		//IL_0539: Unknown result type (might be due to invalid IL or missing references)
		//IL_053e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_055a: Unknown result type (might be due to invalid IL or missing references)
		//IL_056a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0577: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0357: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_0371: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		//IL_038b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0401: Unknown result type (might be due to invalid IL or missing references)
		//IL_0404: Unknown result type (might be due to invalid IL or missing references)
		//IL_0409: Unknown result type (might be due to invalid IL or missing references)
		//IL_0411: Unknown result type (might be due to invalid IL or missing references)
		//IL_041e: Unknown result type (might be due to invalid IL or missing references)
		//IL_042b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0438: Unknown result type (might be due to invalid IL or missing references)
		//IL_0445: Unknown result type (might be due to invalid IL or missing references)
		//IL_0452: Unknown result type (might be due to invalid IL or missing references)
		//IL_045f: Unknown result type (might be due to invalid IL or missing references)
		//IL_046c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0475: Unknown result type (might be due to invalid IL or missing references)
		//IL_047a: Unknown result type (might be due to invalid IL or missing references)
		//IL_047c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0481: Unknown result type (might be due to invalid IL or missing references)
		//IL_0488: Unknown result type (might be due to invalid IL or missing references)
		//IL_048d: Unknown result type (might be due to invalid IL or missing references)
		//IL_048f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0494: Unknown result type (might be due to invalid IL or missing references)
		Bounds bounds = mr.bounds;
		Vector3 center = ((Bounds)(ref bounds)).center;
		cutPlaneCenter = Vector3.zero;
		cutPoints.Clear();
		verts.Clear();
		int num = verticesIndices.Length;
		for (int i = 0; i < num; i += 3)
		{
			Vector3 val = rotatedVertices[verticesIndices[i]];
			Vector3 val2 = rotatedVertices[verticesIndices[i + 1]];
			Vector3 val3 = rotatedVertices[verticesIndices[i + 2]];
			if (val.y > level && val2.y > level && val3.y > level)
			{
				continue;
			}
			if (val.y < level && val2.y > level && val3.y > level)
			{
				val2 = ClampVertexToSlicePlane(val2, val, level);
				val3 = ClampVertexToSlicePlane(val3, val, level);
				cutPoints.Add(val2);
				cutPoints.Add(val3);
				cutPlaneCenter += val2;
				cutPlaneCenter += val3;
			}
			else if (val2.y < level && val.y > level && val3.y > level)
			{
				val = ClampVertexToSlicePlane(val, val2, level);
				val3 = ClampVertexToSlicePlane(val3, val2, level);
				cutPoints.Add(val);
				cutPoints.Add(val3);
				cutPlaneCenter += val;
				cutPlaneCenter += val3;
			}
			else if (val3.y < level && val.y > level && val2.y > level)
			{
				val = ClampVertexToSlicePlane(val, val3, level);
				val2 = ClampVertexToSlicePlane(val2, val3, level);
				cutPoints.Add(val);
				cutPoints.Add(val2);
				cutPlaneCenter += val;
				cutPlaneCenter += val2;
			}
			else
			{
				if (val.y > level && val2.y < level && val3.y < level)
				{
					Vector3 val4 = ClampVertexToSlicePlane(val, val2, level);
					Vector3 val5 = ClampVertexToSlicePlane(val, val3, level);
					verts.Add(val4);
					verts.Add(val2);
					verts.Add(val3);
					verts.Add(val5);
					verts.Add(val4);
					verts.Add(val3);
					cutPoints.Add(val4);
					cutPoints.Add(val5);
					cutPlaneCenter += val4;
					cutPlaneCenter += val5;
					continue;
				}
				if (val2.y > level && val.y < level && val3.y < level)
				{
					Vector3 val6 = ClampVertexToSlicePlane(val2, val, level);
					Vector3 val7 = ClampVertexToSlicePlane(val2, val3, level);
					verts.Add(val);
					verts.Add(val6);
					verts.Add(val3);
					verts.Add(val6);
					verts.Add(val7);
					verts.Add(val3);
					cutPoints.Add(val6);
					cutPoints.Add(val7);
					cutPlaneCenter += val6;
					cutPlaneCenter += val7;
					continue;
				}
				if (val3.y > level && val.y < level && val2.y < level)
				{
					Vector3 val8 = ClampVertexToSlicePlane(val3, val, level);
					Vector3 val9 = ClampVertexToSlicePlane(val3, val2, level);
					verts.Add(val8);
					verts.Add(val);
					verts.Add(val2);
					verts.Add(val9);
					verts.Add(val8);
					verts.Add(val2);
					cutPoints.Add(val8);
					cutPoints.Add(val9);
					cutPlaneCenter += val8;
					cutPlaneCenter += val9;
					continue;
				}
			}
			verts.Add(val);
			verts.Add(val2);
			verts.Add(val3);
		}
		int count = cutPoints.Count;
		if (cutPoints.Count >= 3)
		{
			cutPlaneCenter /= (float)count;
			cutPoints.Sort(PolygonSortOnPlane);
			for (int j = 0; j < count; j++)
			{
				Vector3 item = cutPoints[j];
				Vector3 item2 = ((j != count - 1) ? cutPoints[j + 1] : cutPoints[0]);
				verts.Add(cutPlaneCenter);
				verts.Add(item);
				verts.Add(item2);
			}
		}
		int count2 = verts.Count;
		float num2 = 0f;
		for (int k = 0; k < count2; k += 3)
		{
			num2 += SignedVolumeOfTriangle(verts[k], verts[k + 1], verts[k + 2], center);
		}
		return Mathf.Abs(num2);
	}

	private int PolygonSortOnPlane(Vector3 p1, Vector3 p2)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Atan2(p1.x - cutPlaneCenter.x, p1.z - cutPlaneCenter.z);
		float num2 = Mathf.Atan2(p2.x - cutPlaneCenter.x, p2.z - cutPlaneCenter.z);
		if (num < num2)
		{
			return -1;
		}
		if (num > num2)
		{
			return 1;
		}
		return 0;
	}

	private void UpdateTurbulence()
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)liqMat == (Object)null))
		{
			float num = ((levelMultipled > 0f) ? 1f : 0f);
			float num2 = ((camInside && _allowViewFromInside) ? 0f : 1f);
			turb.x = _turbulence1 * num * num2;
			turb.y = Mathf.Max(_turbulence2, turbulenceDueForces) * num * num2;
			shaderTurb = turb;
			shaderTurb.z *= (float)Math.PI * _frecuency * 4f;
			shaderTurb.w *= (float)Math.PI * _frecuency * 4f;
			liqMat.SetVector(ShaderParams.Turbulence, shaderTurb);
		}
	}

	private void CheckInsideOut()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		Camera current = Camera.current;
		if ((Object)(object)current == (Object)null || (Object)(object)mr == (Object)null)
		{
			if (!_allowViewFromInside)
			{
				UpdateInsideOut();
			}
			return;
		}
		Vector3 val = ((Component)current).transform.position + ((Component)current).transform.forward * current.nearClipPlane;
		Vector3 val2 = val - ((Component)this).transform.position;
		float sqrMagnitude = ((Vector3)(ref val2)).sqrMagnitude;
		if (sqrMagnitude != lastDistanceToCam)
		{
			lastDistanceToCam = sqrMagnitude;
			bool flag = false;
			switch (_topology)
			{
			case TOPOLOGY.Cube:
				flag = PointInAABB(val);
				break;
			case TOPOLOGY.Cylinder:
				flag = PointInCylinder(val);
				break;
			default:
			{
				Bounds bounds = mesh.bounds;
				float num = ((Bounds)(ref bounds)).extents.x * 2f;
				val2 = val - ((Component)this).transform.position;
				flag = ((Vector3)(ref val2)).sqrMagnitude < num * num;
				break;
			}
			}
			if (flag != camInside)
			{
				camInside = flag;
				UpdateInsideOut();
			}
		}
	}

	private bool PointInAABB(Vector3 point)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		point = ((Component)this).transform.InverseTransformPoint(point);
		Bounds bounds = mesh.bounds;
		Vector3 extents = ((Bounds)(ref bounds)).extents;
		if (point.x < extents.x && point.x > 0f - extents.x && point.y < extents.y && point.y > 0f - extents.y && point.z < extents.z && point.z > 0f - extents.z)
		{
			return true;
		}
		return false;
	}

	private bool PointInCylinder(Vector3 point)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		point = ((Component)this).transform.InverseTransformPoint(point);
		Bounds bounds = mesh.bounds;
		Vector3 extents = ((Bounds)(ref bounds)).extents;
		if (point.x < extents.x && point.x > 0f - extents.x && point.y < extents.y && point.y > 0f - extents.y && point.z < extents.z && point.z > 0f - extents.z)
		{
			point.y = 0f;
			Vector3 position = ((Component)this).transform.position;
			position.y = 0f;
			Vector3 val = point - position;
			return ((Vector3)(ref val)).sqrMagnitude < extents.x * extents.x;
		}
		return false;
	}

	private void UpdateInsideOut()
	{
		if ((Object)(object)liqMat == (Object)null)
		{
			return;
		}
		if (_allowViewFromInside && camInside)
		{
			liqMat.SetInt(ShaderParams.CullMode, 1);
			liqMat.SetInt(ShaderParams.ZTestMode, 8);
			if ((Object)(object)_flaskMaterial != (Object)null)
			{
				_flaskMaterial.SetInt(ShaderParams.CullMode, 1);
				_flaskMaterial.SetInt(ShaderParams.ZTestMode, 8);
			}
		}
		else
		{
			liqMat.SetInt(ShaderParams.CullMode, 2);
			liqMat.SetInt(ShaderParams.ZTestMode, 4);
			if ((Object)(object)_flaskMaterial != (Object)null)
			{
				_flaskMaterial.SetInt(ShaderParams.CullMode, 2);
				_flaskMaterial.SetInt(ShaderParams.ZTestMode, 4);
			}
		}
		UpdateTurbulence();
	}

	public bool GetSpillPoint(out Vector3 spillPosition, float apertureStart = 1f)
	{
		float spillAmount;
		return GetSpillPoint(out spillPosition, out spillAmount, apertureStart);
	}

	public bool GetSpillPoint(out Vector3 spillPosition, out float spillAmount, float apertureStart = 1f, LEVEL_COMPENSATION rotationCompensation = LEVEL_COMPENSATION.None)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		spillPosition = Vector3.zero;
		spillAmount = 0f;
		if ((Object)(object)mesh == (Object)null || verticesSorted == null || levelMultipled <= 0f)
		{
			return false;
		}
		float num = float.MinValue;
		for (int i = 0; i < verticesSorted.Length; i++)
		{
			Vector3 val = verticesSorted[i];
			if (val.y > num)
			{
				num = val.y;
			}
		}
		float num2 = num * apertureStart * 0.99f;
		Vector3 val2 = ((Component)this).transform.position;
		bool flag = false;
		float num3 = float.MaxValue;
		for (int j = 0; j < verticesSorted.Length; j++)
		{
			Vector3 val3 = verticesSorted[j];
			if (val3.y < num2)
			{
				break;
			}
			val3 = ((Component)this).transform.TransformPoint(val3);
			if (val3.y < liquidLevelPos && val3.y < num3)
			{
				num3 = val3.y;
				val2 = val3;
				flag = true;
			}
		}
		if (!flag)
		{
			return false;
		}
		spillPosition = val2;
		switch (rotationCompensation)
		{
		case LEVEL_COMPENSATION.Accurate:
			spillAmount = GetMeshVolumeUnderLevelWS(liquidLevelPos) - GetMeshVolumeUnderLevelWS(val2.y);
			break;
		case LEVEL_COMPENSATION.Fast:
			spillAmount = GetMeshVolumeUnderLevelWSFast(liquidLevelPos) - GetMeshVolumeUnderLevelWSFast(val2.y);
			break;
		default:
		{
			float num4 = liquidLevelPos - val2.y;
			Bounds bounds = mr.bounds;
			spillAmount = num4 / (((Bounds)(ref bounds)).extents.y * 2f);
			break;
		}
		}
		return true;
	}

	private void UpdateSpillPointGizmo()
	{
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		if (!_debugSpillPoint)
		{
			if ((Object)(object)spillPointGizmo != (Object)null)
			{
				Object.DestroyImmediate((Object)(object)spillPointGizmo.gameObject);
				spillPointGizmo = null;
			}
			return;
		}
		if ((Object)(object)spillPointGizmo == (Object)null)
		{
			Transform val = ((Component)this).transform.Find("SpillPointGizmo");
			if ((Object)(object)val != (Object)null)
			{
				Object.DestroyImmediate((Object)(object)((Component)val).gameObject);
			}
			spillPointGizmo = GameObject.CreatePrimitive((PrimitiveType)0);
			((Object)spillPointGizmo).name = "SpillPointGizmo";
			spillPointGizmo.transform.SetParent(((Component)this).transform, true);
			Collider component = spillPointGizmo.GetComponent<Collider>();
			if ((Object)(object)component != (Object)null)
			{
				Object.DestroyImmediate((Object)(object)component);
			}
			MeshRenderer component2 = spillPointGizmo.GetComponent<MeshRenderer>();
			if ((Object)(object)component2 != (Object)null)
			{
				((Renderer)component2).sharedMaterial = Object.Instantiate<Material>(((Renderer)component2).sharedMaterial);
				((Object)((Renderer)component2).sharedMaterial).hideFlags = (HideFlags)52;
				((Renderer)component2).sharedMaterial.color = Color.yellow;
			}
		}
		if (GetSpillPoint(out var spillPosition))
		{
			spillPointGizmo.transform.position = spillPosition;
			if ((Object)(object)mesh != (Object)null)
			{
				Bounds bounds = mesh.bounds;
				Vector3 val2 = ((Bounds)(ref bounds)).extents * 0.2f;
				float num = ((val2.x > val2.y) ? val2.x : val2.z);
				num = ((num > val2.z) ? num : val2.z);
				spillPointGizmo.transform.localScale = new Vector3(num, num, num);
			}
			else
			{
				spillPointGizmo.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
			}
			spillPointGizmo.SetActive(true);
		}
		else
		{
			spillPointGizmo.SetActive(false);
		}
	}

	public void BakeRotation()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		if (((Component)this).transform.localRotation == ((Component)this).transform.rotation)
		{
			return;
		}
		MeshFilter component = ((Component)this).GetComponent<MeshFilter>();
		Mesh sharedMesh = component.sharedMesh;
		if (!((Object)(object)sharedMesh == (Object)null))
		{
			sharedMesh = Object.Instantiate<Mesh>(sharedMesh);
			Vector3[] vertices = sharedMesh.vertices;
			Vector3 localScale = ((Component)this).transform.localScale;
			Vector3 localPosition = ((Component)this).transform.localPosition;
			((Component)this).transform.localScale = Vector3.one;
			Transform parent = ((Component)this).transform.parent;
			if ((Object)(object)parent != (Object)null)
			{
				((Component)this).transform.SetParent((Transform)null, false);
			}
			for (int i = 0; i < vertices.Length; i++)
			{
				vertices[i] = ((Component)this).transform.TransformVector(vertices[i]);
			}
			sharedMesh.vertices = vertices;
			sharedMesh.RecalculateBounds();
			sharedMesh.RecalculateNormals();
			component.sharedMesh = sharedMesh;
			if ((Object)(object)parent != (Object)null)
			{
				((Component)this).transform.SetParent(parent, false);
				((Component)this).transform.localPosition = localPosition;
			}
			((Component)this).transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			((Component)this).transform.localScale = localScale;
			RefreshMeshAndCollider();
		}
	}

	public void CenterPivot()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		CenterPivot(Vector3.zero);
	}

	public void CenterPivot(Vector3 offset)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		MeshFilter component = ((Component)this).GetComponent<MeshFilter>();
		Mesh sharedMesh = component.sharedMesh;
		if (!((Object)(object)sharedMesh == (Object)null))
		{
			sharedMesh = Object.Instantiate<Mesh>(sharedMesh);
			((Object)sharedMesh).name = ((Object)component.sharedMesh).name;
			Vector3[] vertices = sharedMesh.vertices;
			Vector3 val = Vector3.zero;
			for (int i = 0; i < vertices.Length; i++)
			{
				val += vertices[i];
			}
			val /= (float)vertices.Length;
			val += offset;
			for (int j = 0; j < vertices.Length; j++)
			{
				ref Vector3 reference = ref vertices[j];
				reference -= val;
			}
			sharedMesh.vertices = vertices;
			sharedMesh.RecalculateBounds();
			component.sharedMesh = sharedMesh;
			fixedMesh = sharedMesh;
			Vector3 localScale = ((Component)this).transform.localScale;
			val.x *= localScale.x;
			val.y *= localScale.y;
			val.z *= localScale.z;
			Transform transform = ((Component)this).transform;
			transform.localPosition += val;
			RefreshMeshAndCollider();
		}
	}

	public void RefreshMeshAndCollider()
	{
		ClearMeshCache();
		MeshCollider component = ((Component)this).GetComponent<MeshCollider>();
		if ((Object)(object)component != (Object)null)
		{
			Mesh sharedMesh = component.sharedMesh;
			component.sharedMesh = null;
			component.sharedMesh = sharedMesh;
		}
	}

	public void Redraw()
	{
		UpdateMaterialProperties();
	}

	private void CheckMeshDisplacement()
	{
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		MeshFilter component = ((Component)this).GetComponent<MeshFilter>();
		if ((Object)(object)component == (Object)null)
		{
			originalMesh = null;
			return;
		}
		Mesh sharedMesh = component.sharedMesh;
		if ((Object)(object)sharedMesh == (Object)null)
		{
			if (!_fixMesh)
			{
				originalMesh = null;
				return;
			}
			if ((Object)(object)fixedMesh != (Object)null)
			{
				component.sharedMesh = fixedMesh;
				return;
			}
			if ((Object)(object)originalMesh != (Object)null)
			{
				component.sharedMesh = originalMesh;
			}
			sharedMesh = component.sharedMesh;
		}
		if (!_fixMesh)
		{
			RestoreOriginalMesh();
			originalMesh = null;
			return;
		}
		if ((Object)(object)originalMesh == (Object)null || !((Object)originalMesh).name.Equals(((Object)sharedMesh).name))
		{
			originalMesh = component.sharedMesh;
		}
		if ((Object)(object)sharedMesh != (Object)(object)originalMesh)
		{
			RestoreOriginalMesh();
		}
		Vector3 localPosition = ((Component)this).transform.localPosition;
		CenterPivot(_pivotOffset);
		originalPivotOffset = ((Component)this).transform.localPosition - localPosition;
	}

	private void RestoreOriginalMesh()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		fixedMesh = null;
		if (!((Object)(object)originalMesh == (Object)null))
		{
			MeshFilter component = ((Component)this).GetComponent<MeshFilter>();
			if (!((Object)(object)component == (Object)null))
			{
				component.sharedMesh = originalMesh;
				Transform transform = ((Component)this).transform;
				transform.localPosition -= originalPivotOffset;
				RefreshMeshAndCollider();
			}
		}
	}

	public void CopyFrom(LiquidVolume lv)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)lv == (Object)null))
		{
			_allowViewFromInside = lv._allowViewFromInside;
			_alpha = lv._alpha;
			_backDepthBias = lv._backDepthBias;
			_blurIntensity = lv._blurIntensity;
			_bumpDistortionOffset = lv._bumpDistortionOffset;
			_bumpDistortionScale = lv._bumpDistortionScale;
			_bumpMap = lv._bumpMap;
			_bumpStrength = lv._bumpStrength;
			_debugSpillPoint = lv._debugSpillPoint;
			_deepObscurance = lv._deepObscurance;
			_depthAware = lv._depthAware;
			_depthAwareCustomPass = lv._depthAwareCustomPass;
			_depthAwareCustomPassDebug = lv._depthAwareCustomPassDebug;
			_depthAwareOffset = lv._depthAwareOffset;
			_detail = lv._detail;
			_distortionAmount = lv._distortionAmount;
			_distortionMap = lv._distortionMap;
			_ditherShadows = lv._ditherShadows;
			_doubleSidedBias = lv._doubleSidedBias;
			_emissionColor = lv._emissionColor;
			_extentsScale = lv._extentsScale;
			_fixMesh = lv._fixMesh;
			_flaskThickness = lv._flaskThickness;
			_foamColor = lv._foamColor;
			_foamDensity = lv._foamDensity;
			_foamRaySteps = lv._foamRaySteps;
			_foamScale = lv._foamScale;
			_foamThickness = lv._foamThickness;
			_foamTurbulence = lv._foamTurbulence;
			_foamVisibleFromBottom = lv._foamVisibleFromBottom;
			_foamWeight = lv._foamWeight;
			_frecuency = lv._frecuency;
			_ignoreGravity = lv._ignoreGravity;
			_irregularDepthDebug = lv._irregularDepthDebug;
			_level = lv._level;
			_levelMultiplier = lv._levelMultiplier;
			_liquidColor1 = lv._liquidColor1;
			_liquidColor2 = lv._liquidColor2;
			_liquidRaySteps = lv._liquidRaySteps;
			_liquidScale1 = lv._liquidScale1;
			_liquidScale2 = lv._liquidScale2;
			_lowerLimit = lv._lowerLimit;
			_murkiness = lv._murkiness;
			_noiseVariation = lv._noiseVariation;
			_physicsAngularDamp = lv._physicsAngularDamp;
			_physicsMass = lv._physicsMass;
			_pivotOffset = lv._pivotOffset;
			_reactToForces = lv._reactToForces;
			_reflectionTexture = lv._reflectionTexture;
			_refractionBlur = lv._refractionBlur;
			_renderQueue = lv._renderQueue;
			_scatteringAmount = lv._scatteringAmount;
			_scatteringEnabled = lv._scatteringEnabled;
			_scatteringPower = lv._scatteringPower;
			_smokeBaseObscurance = lv._smokeBaseObscurance;
			_smokeColor = lv._smokeColor;
			_smokeEnabled = lv._smokeEnabled;
			_smokeHeightAtten = lv._smokeHeightAtten;
			_smokeRaySteps = lv._smokeRaySteps;
			_smokeScale = lv._smokeScale;
			_smokeSpeed = lv._smokeSpeed;
			_sparklingAmount = lv._sparklingAmount;
			_sparklingIntensity = lv._sparklingIntensity;
			_speed = lv._speed;
			_subMeshIndex = lv._subMeshIndex;
			_texture = lv._texture;
			_textureOffset = lv._textureOffset;
			_textureScale = lv._textureScale;
			_topology = lv._topology;
			_turbulence1 = lv._turbulence1;
			_turbulence2 = lv._turbulence2;
			_upperLimit = lv._upperLimit;
			shouldUpdateMaterialProperties = true;
		}
	}
}
