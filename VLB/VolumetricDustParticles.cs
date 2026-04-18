using System;
using UnityEngine;

namespace VLB;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[RequireComponent(typeof(VolumetricLightBeamAbstractBase))]
[HelpURL("http://saladgamer.com/vlb-doc/comp-dustparticles/")]
public class VolumetricDustParticles : MonoBehaviour
{
	public const string ClassName = "VolumetricDustParticles";

	[Range(0f, 1f)]
	public float alpha = 0.5f;

	[Range(0.0001f, 0.1f)]
	public float size = 0.01f;

	public ParticlesDirection direction;

	public Vector3 velocity = Consts.DustParticles.VelocityDefault;

	[Obsolete("Use 'velocity' instead")]
	public float speed = 0.03f;

	public float density = 5f;

	[MinMaxRange(0f, 1f)]
	public MinMaxRangeFloat spawnDistanceRange = Consts.DustParticles.SpawnDistanceRangeDefault;

	[Obsolete("Use 'spawnDistanceRange' instead")]
	public float spawnMinDistance;

	[Obsolete("Use 'spawnDistanceRange' instead")]
	public float spawnMaxDistance = 0.7f;

	public bool cullingEnabled;

	public float cullingMaxDistance = 10f;

	[SerializeField]
	private float m_AlphaAdditionalRuntime = 1f;

	private ParticleSystem m_Particles;

	private ParticleSystemRenderer m_Renderer;

	private Material m_Material;

	private Gradient m_GradientCached = new Gradient();

	private bool m_RuntimePropertiesDirty = true;

	private static bool ms_NoMainCameraLogged;

	private static Camera ms_MainCamera;

	private VolumetricLightBeamAbstractBase m_Master;

	public bool isCulled { get; private set; }

	public float alphaAdditionalRuntime
	{
		get
		{
			return m_AlphaAdditionalRuntime;
		}
		set
		{
			if (m_AlphaAdditionalRuntime != value)
			{
				m_AlphaAdditionalRuntime = value;
				m_RuntimePropertiesDirty = true;
			}
		}
	}

	public bool particlesAreInstantiated => Object.op_Implicit((Object)(object)m_Particles);

	public int particlesCurrentCount
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)m_Particles))
			{
				return 0;
			}
			return m_Particles.particleCount;
		}
	}

	public int particlesMaxCount
	{
		get
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			if (!Object.op_Implicit((Object)(object)m_Particles))
			{
				return 0;
			}
			MainModule main = m_Particles.main;
			return ((MainModule)(ref main)).maxParticles;
		}
	}

	public Camera mainCamera
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)ms_MainCamera))
			{
				ms_MainCamera = Camera.main;
				if (!Object.op_Implicit((Object)(object)ms_MainCamera) && !ms_NoMainCameraLogged)
				{
					Debug.LogErrorFormat((Object)(object)((Component)this).gameObject, "In order to use 'VolumetricDustParticles' culling, you must have a MainCamera defined in your scene.", Array.Empty<object>());
					ms_NoMainCameraLogged = true;
				}
			}
			return ms_MainCamera;
		}
	}

	private void Start()
	{
		isCulled = false;
		m_Master = ((Component)this).GetComponent<VolumetricLightBeamAbstractBase>();
		HandleBackwardCompatibility(m_Master._INTERNAL_pluginVersion, 20100);
		InstantiateParticleSystem();
		SetActiveAndPlay();
	}

	private void InstantiateParticleSystem()
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		((Component)this).gameObject.ForeachComponentsInDirectChildrenOnly<ParticleSystem>((Action<ParticleSystem>)delegate(ParticleSystem ps)
		{
			Object.DestroyImmediate((Object)(object)((Component)ps).gameObject);
		}, true);
		m_Particles = Config.Instance.NewVolumetricDustParticles();
		if (Object.op_Implicit((Object)(object)m_Particles))
		{
			((Component)m_Particles).transform.SetParent(((Component)this).transform, false);
			m_Renderer = ((Component)m_Particles).GetComponent<ParticleSystemRenderer>();
			m_Material = new Material(((Renderer)m_Renderer).sharedMaterial);
			((Renderer)m_Renderer).material = m_Material;
		}
	}

	private void OnEnable()
	{
		SetActiveAndPlay();
	}

	private void SetActive(bool active)
	{
		if (Object.op_Implicit((Object)(object)m_Particles))
		{
			((Component)m_Particles).gameObject.SetActive(active);
		}
	}

	private void SetActiveAndPlay()
	{
		SetActive(active: true);
		Play();
	}

	private void Play()
	{
		if (Object.op_Implicit((Object)(object)m_Particles))
		{
			SetParticleProperties();
			m_Particles.Simulate(0f);
			m_Particles.Play(true);
		}
	}

	private void OnDisable()
	{
		SetActive(active: false);
	}

	private void OnDestroy()
	{
		if (Object.op_Implicit((Object)(object)m_Particles))
		{
			Object.DestroyImmediate((Object)(object)((Component)m_Particles).gameObject);
			m_Particles = null;
		}
		if (Object.op_Implicit((Object)(object)m_Material))
		{
			Object.DestroyImmediate((Object)(object)m_Material);
			m_Material = null;
		}
	}

	private void Update()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		UpdateCulling();
		if (UtilsBeamProps.CanChangeDuringPlaytime(m_Master))
		{
			SetParticleProperties();
		}
		if (m_RuntimePropertiesDirty && (Object)(object)m_Material != (Object)null)
		{
			m_Material.SetColor(ShaderProperties.ParticlesTintColor, new Color(1f, 1f, 1f, alphaAdditionalRuntime));
			m_RuntimePropertiesDirty = false;
		}
	}

	private void SetParticleProperties()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0350: Unknown result type (might be due to invalid IL or missing references)
		//IL_0387: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_0395: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)m_Particles) || !((Component)m_Particles).gameObject.activeSelf)
		{
			return;
		}
		((Component)m_Particles).transform.localRotation = UtilsBeamProps.GetInternalLocalRotation(m_Master);
		((Component)m_Particles).transform.localScale = (m_Master.IsScalable() ? Vector3.one : Vector3.one.Divide(m_Master.GetLossyScale()));
		float num = UtilsBeamProps.GetFallOffEnd(m_Master) * (spawnDistanceRange.maxValue - spawnDistanceRange.minValue);
		float num2 = num * density;
		int maxParticles = (int)(num2 * 4f);
		MainModule main = m_Particles.main;
		MinMaxCurve startLifetime = ((MainModule)(ref main)).startLifetime;
		((MinMaxCurve)(ref startLifetime)).mode = (ParticleSystemCurveMode)3;
		((MinMaxCurve)(ref startLifetime)).constantMin = 4f;
		((MinMaxCurve)(ref startLifetime)).constantMax = 6f;
		((MainModule)(ref main)).startLifetime = startLifetime;
		MinMaxCurve startSize = ((MainModule)(ref main)).startSize;
		((MinMaxCurve)(ref startSize)).mode = (ParticleSystemCurveMode)3;
		((MinMaxCurve)(ref startSize)).constantMin = size * 0.9f;
		((MinMaxCurve)(ref startSize)).constantMax = size * 1.1f;
		((MainModule)(ref main)).startSize = startSize;
		MinMaxGradient startColor = ((MainModule)(ref main)).startColor;
		if (UtilsBeamProps.GetColorMode(m_Master) == ColorMode.Flat)
		{
			((MinMaxGradient)(ref startColor)).mode = (ParticleSystemGradientMode)0;
			Color colorFlat = UtilsBeamProps.GetColorFlat(m_Master);
			colorFlat.a *= alpha;
			((MinMaxGradient)(ref startColor)).color = colorFlat;
		}
		else
		{
			((MinMaxGradient)(ref startColor)).mode = (ParticleSystemGradientMode)1;
			Gradient colorGradient = UtilsBeamProps.GetColorGradient(m_Master);
			GradientColorKey[] colorKeys = colorGradient.colorKeys;
			GradientAlphaKey[] alphaKeys = colorGradient.alphaKeys;
			for (int i = 0; i < alphaKeys.Length; i++)
			{
				alphaKeys[i].alpha *= alpha;
			}
			m_GradientCached.SetKeys(colorKeys, alphaKeys);
			((MinMaxGradient)(ref startColor)).gradient = m_GradientCached;
		}
		((MainModule)(ref main)).startColor = startColor;
		MinMaxCurve startSpeed = ((MainModule)(ref main)).startSpeed;
		((MinMaxCurve)(ref startSpeed)).constant = ((direction == ParticlesDirection.Random) ? Mathf.Abs(velocity.z) : 0f);
		((MainModule)(ref main)).startSpeed = startSpeed;
		VelocityOverLifetimeModule velocityOverLifetime = m_Particles.velocityOverLifetime;
		((VelocityOverLifetimeModule)(ref velocityOverLifetime)).enabled = direction != ParticlesDirection.Random;
		((VelocityOverLifetimeModule)(ref velocityOverLifetime)).space = (ParticleSystemSimulationSpace)(direction != ParticlesDirection.LocalSpace);
		((VelocityOverLifetimeModule)(ref velocityOverLifetime)).xMultiplier = velocity.x;
		((VelocityOverLifetimeModule)(ref velocityOverLifetime)).yMultiplier = velocity.y;
		((VelocityOverLifetimeModule)(ref velocityOverLifetime)).zMultiplier = velocity.z;
		((MainModule)(ref main)).maxParticles = maxParticles;
		float thickness = UtilsBeamProps.GetThickness(m_Master);
		float fallOffEnd = UtilsBeamProps.GetFallOffEnd(m_Master);
		ShapeModule shape = m_Particles.shape;
		((ShapeModule)(ref shape)).shapeType = (ParticleSystemShapeType)8;
		float num3 = UtilsBeamProps.GetConeAngle(m_Master) * Mathf.Lerp(0.7f, 1f, thickness);
		((ShapeModule)(ref shape)).angle = num3 * 0.5f;
		float num4 = UtilsBeamProps.GetConeRadiusStart(m_Master) * Mathf.Lerp(0.3f, 1f, thickness);
		float num5 = Utils.ComputeConeRadiusEnd(fallOffEnd, num3);
		((ShapeModule)(ref shape)).radius = Mathf.Lerp(num4, num5, spawnDistanceRange.minValue);
		((ShapeModule)(ref shape)).length = num;
		float num6 = fallOffEnd * spawnDistanceRange.minValue;
		((ShapeModule)(ref shape)).position = new Vector3(0f, 0f, num6);
		((ShapeModule)(ref shape)).arc = 360f;
		((ShapeModule)(ref shape)).randomDirectionAmount = ((direction == ParticlesDirection.Random) ? 1f : 0f);
		EmissionModule emission = m_Particles.emission;
		MinMaxCurve rateOverTime = ((EmissionModule)(ref emission)).rateOverTime;
		((MinMaxCurve)(ref rateOverTime)).constant = num2;
		((EmissionModule)(ref emission)).rateOverTime = rateOverTime;
		if (Object.op_Implicit((Object)(object)m_Renderer))
		{
			((Renderer)m_Renderer).sortingLayerID = UtilsBeamProps.GetSortingLayerID(m_Master);
			((Renderer)m_Renderer).sortingOrder = UtilsBeamProps.GetSortingOrder(m_Master);
		}
	}

	private void HandleBackwardCompatibility(int serializedVersion, int newVersion)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (serializedVersion == -1 || serializedVersion == newVersion)
		{
			return;
		}
		if (serializedVersion < 1880)
		{
			if (direction == ParticlesDirection.Random)
			{
				direction = ParticlesDirection.LocalSpace;
			}
			else
			{
				direction = ParticlesDirection.Random;
			}
			velocity = new Vector3(0f, 0f, speed);
		}
		if (serializedVersion < 1940)
		{
			spawnDistanceRange = new MinMaxRangeFloat(spawnMinDistance, spawnMaxDistance);
		}
		Utils.MarkCurrentSceneDirty();
	}

	private void UpdateCulling()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)m_Particles))
		{
			return;
		}
		bool flag = true;
		bool fadeOutEnabled = UtilsBeamProps.GetFadeOutEnabled(m_Master);
		if ((cullingEnabled || fadeOutEnabled) && m_Master.hasGeometry)
		{
			if (Object.op_Implicit((Object)(object)mainCamera))
			{
				float num = cullingMaxDistance;
				if (fadeOutEnabled)
				{
					num = Mathf.Min(num, UtilsBeamProps.GetFadeOutEnd(m_Master));
				}
				float num2 = num * num;
				Bounds bounds = m_Master.bounds;
				flag = ((Bounds)(ref bounds)).SqrDistance(((Component)mainCamera).transform.position) <= num2;
			}
			else
			{
				cullingEnabled = false;
			}
		}
		if (((Component)m_Particles).gameObject.activeSelf != flag)
		{
			SetActive(flag);
			isCulled = !flag;
		}
		if (flag && !m_Particles.isPlaying)
		{
			m_Particles.Play();
		}
	}
}
