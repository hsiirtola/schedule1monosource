using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.FX;
using ScheduleOne.Weather;
using UnityEngine;
using UnityEngine.Rendering;

namespace Funly.SkyStudio;

[ExecuteInEditMode]
public class TimeOfDayController : MonoBehaviour
{
	public delegate void TimeOfDayDidChange(TimeOfDayController tc, float timeOfDay);

	[Tooltip("Sky profile defines the skyColors configuration for times of day. This script will animate between those skyColors values based on the time of day.")]
	[SerializeField]
	private SkyProfile m_SkyProfile;

	[Tooltip("Time is expressed in a fractional number of days that have completed.")]
	[SerializeField]
	private float m_SkyTime;

	[Tooltip("Automatically advance time at fixed speed.")]
	public bool automaticTimeIncrement;

	[Tooltip("Create a copy of the sky profile at runtime, so modifications don't affect the original Sky Profile in your project.")]
	public bool copySkyProfile;

	private SkyMaterialController m_SkyMaterialController;

	[Tooltip("Speed at which to advance time by if in automatic increment is enabled.")]
	[Range(0f, 1f)]
	public float automaticIncrementSpeed = 0.01f;

	[Tooltip("Sun orbit.")]
	public OrbitingBody sunOrbit;

	[Tooltip("Moon orbit.")]
	public OrbitingBody moonOrbit;

	[Tooltip("Controller for managing weather effects")]
	public WeatherController weatherController;

	[Tooltip("If true we'll invoke DynamicGI.UpdateEnvironment() when skybox changes. This is an expensive operation.")]
	public bool updateGlobalIllumination;

	[Tooltip("Configurable prefab that determines how to animate between 2 sky profiles. You can override individual feature animations, ex: 'skyBlender', to create a custom sky blending effect.")]
	public BlendSkyProfiles skyProfileTransitionPrefab;

	[Header("Overrides")]
	public SkyProfileOverride[] SkyProfileOverrideStack;

	private bool m_DidInitialUpdate;

	private SkyProfileFrame _skyProfileFrame;

	public static TimeOfDayController instance { get; private set; }

	public SkyProfile skyProfile
	{
		get
		{
			return m_SkyProfile;
		}
		set
		{
			if ((Object)(object)value != (Object)null && copySkyProfile)
			{
				m_SkyProfile = Object.Instantiate<SkyProfile>(value);
			}
			else
			{
				m_SkyProfile = value;
			}
			m_SkyMaterialController = null;
			UpdateSkyForCurrentTime();
			SynchronizeAllShaderKeywords();
		}
	}

	public float skyTime
	{
		get
		{
			return m_SkyTime;
		}
		set
		{
			m_SkyTime = Mathf.Abs(value);
			UpdateSkyForCurrentTime();
		}
	}

	public SkyMaterialController SkyMaterial => m_SkyMaterialController;

	public bool UseEnvironmentProfileStack { get; set; }

	public SkyProfileFrame SkyProfileFrame
	{
		get
		{
			return _skyProfileFrame;
		}
		set
		{
			_skyProfileFrame = value;
		}
	}

	public float timeOfDay => m_SkyTime - (float)(int)m_SkyTime;

	public int daysElapsed => (int)m_SkyTime;

	public event TimeOfDayDidChange timeChangedCallback;

	private void Awake()
	{
		instance = this;
	}

	private void OnEnabled()
	{
		skyTime = m_SkyTime;
	}

	private void OnValidate()
	{
		if (((Component)this).gameObject.activeInHierarchy)
		{
			skyTime = m_SkyTime;
			skyProfile = m_SkyProfile;
		}
	}

	private void WarnInvalidSkySetup()
	{
		Debug.LogError((object)"Your SkySystemController has an old or invalid prefab layout! Please run the upgrade tool in 'Windows -> Sky Studio -> Upgrade Sky System Controller'. Do not rename or modify any of the children in the SkySystemController hierarchy.");
	}

	private void Update()
	{
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)skyProfile))
		{
			return;
		}
		if (automaticTimeIncrement && Application.isPlaying)
		{
			skyTime += automaticIncrementSpeed * Time.deltaTime;
		}
		if ((Object)(object)sunOrbit == (Object)null || (Object)(object)moonOrbit == (Object)null || (Object)(object)sunOrbit.rotateBody == (Object)null || (Object)(object)moonOrbit.rotateBody == (Object)null || (Object)(object)sunOrbit.positionTransform == (Object)null || (Object)(object)moonOrbit.positionTransform == (Object)null)
		{
			WarnInvalidSkySetup();
			return;
		}
		if (!m_DidInitialUpdate)
		{
			UpdateSkyForCurrentTime();
			m_DidInitialUpdate = true;
		}
		UpdateSkyForCurrentTime();
		if ((Object)(object)weatherController != (Object)null)
		{
			weatherController.UpdateForTimeOfDay(skyProfile, timeOfDay);
		}
		if (skyProfile.IsFeatureEnabled("SunFeature"))
		{
			if (Object.op_Implicit((Object)(object)sunOrbit.positionTransform))
			{
				m_SkyMaterialController.SunWorldToLocalMatrix = sunOrbit.positionTransform.worldToLocalMatrix;
			}
			if (skyProfile.IsFeatureEnabled("SunCustomTextureFeature"))
			{
				if (skyProfile.IsFeatureEnabled("SunRotationFeature"))
				{
					sunOrbit.rotateBody.AllowSpinning = true;
					sunOrbit.rotateBody.SpinSpeed = skyProfile.GetNumberPropertyValue("SunRotationSpeedKey", timeOfDay);
				}
				else
				{
					sunOrbit.rotateBody.AllowSpinning = false;
				}
			}
		}
		if (!skyProfile.IsFeatureEnabled("MoonFeature"))
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)moonOrbit.positionTransform))
		{
			m_SkyMaterialController.MoonWorldToLocalMatrix = moonOrbit.positionTransform.worldToLocalMatrix;
		}
		if (skyProfile.IsFeatureEnabled("MoonCustomTextureFeature"))
		{
			if (skyProfile.IsFeatureEnabled("MoonRotationFeature"))
			{
				moonOrbit.rotateBody.AllowSpinning = true;
				moonOrbit.rotateBody.SpinSpeed = skyProfile.GetNumberPropertyValue("MoonRotationSpeedKey", timeOfDay);
			}
			else
			{
				moonOrbit.rotateBody.AllowSpinning = false;
			}
		}
	}

	public void UpdateGlobalIllumination()
	{
		DynamicGI.UpdateEnvironment();
	}

	private void SynchronizeAllShaderKeywords()
	{
		if ((Object)(object)m_SkyProfile == (Object)null)
		{
			return;
		}
		ProfileFeatureSection[] features = m_SkyProfile.profileDefinition.features;
		for (int i = 0; i < features.Length; i++)
		{
			ProfileFeatureDefinition[] featureDefinitions = features[i].featureDefinitions;
			foreach (ProfileFeatureDefinition profileFeatureDefinition in featureDefinitions)
			{
				if (profileFeatureDefinition.featureType == ProfileFeatureDefinition.FeatureType.ShaderKeyword)
				{
					SynchronizedShaderKeyword(profileFeatureDefinition.featureKey, profileFeatureDefinition.shaderKeyword);
				}
				else if (profileFeatureDefinition.featureType == ProfileFeatureDefinition.FeatureType.ShaderKeywordDropdown)
				{
					for (int k = 0; k < profileFeatureDefinition.featureKeys.Length; k++)
					{
						SynchronizedShaderKeyword(profileFeatureDefinition.featureKeys[k], profileFeatureDefinition.shaderKeywords[k]);
					}
				}
			}
		}
	}

	private void SynchronizedShaderKeyword(string featureKey, string shaderKeyword)
	{
		if ((Object)(object)skyProfile == (Object)null || (Object)(object)skyProfile.skyboxMaterial == (Object)null)
		{
			return;
		}
		if (skyProfile.IsFeatureEnabled(featureKey))
		{
			if (!skyProfile.skyboxMaterial.IsKeywordEnabled(shaderKeyword))
			{
				skyProfile.skyboxMaterial.EnableKeyword(shaderKeyword);
			}
		}
		else if (skyProfile.skyboxMaterial.IsKeywordEnabled(shaderKeyword))
		{
			skyProfile.skyboxMaterial.DisableKeyword(shaderKeyword);
		}
	}

	private Vector3 GetPrimaryLightDirection()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		Vector3 bodyGlobalDirection = default(Vector3);
		if (skyProfile.IsFeatureEnabled("SunFeature") && Object.op_Implicit((Object)(object)sunOrbit))
		{
			bodyGlobalDirection = sunOrbit.BodyGlobalDirection;
			return bodyGlobalDirection;
		}
		if (skyProfile.IsFeatureEnabled("MoonFeature") && Object.op_Implicit((Object)(object)moonOrbit))
		{
			bodyGlobalDirection = moonOrbit.BodyGlobalDirection;
			return bodyGlobalDirection;
		}
		((Vector3)(ref bodyGlobalDirection))._002Ector(0f, 1f, 0f);
		return bodyGlobalDirection;
	}

	public bool StartSkyProfileTransition(SkyProfile toProfile, float duration = 1f)
	{
		CancelSkyProfileTransition();
		if ((Object)(object)skyProfileTransitionPrefab == (Object)null)
		{
			Debug.LogWarning((object)"Can't transition since the skyProfileTransitionPrefab is null");
			return false;
		}
		if ((Object)(object)toProfile == (Object)null)
		{
			Debug.LogWarning((object)"Can't transition to null profile");
			return false;
		}
		if ((Object)(object)this.skyProfile == (Object)null)
		{
			Debug.LogWarning((object)"Can't transition to a SkyProfile without a current profile to start from.");
			this.skyProfile = toProfile;
			return false;
		}
		BlendSkyProfiles blendSkyProfiles = Object.Instantiate<BlendSkyProfiles>(skyProfileTransitionPrefab);
		blendSkyProfiles.onBlendComplete = (Action<BlendSkyProfiles>)Delegate.Combine(blendSkyProfiles.onBlendComplete, new Action<BlendSkyProfiles>(OnBlendComplete));
		SkyProfile skyProfile = blendSkyProfiles.StartBlending(this, this.skyProfile, toProfile, duration);
		if ((Object)(object)skyProfile == (Object)null)
		{
			Debug.LogWarning((object)"Failed to create blending profile, check your from/to args.");
			return false;
		}
		this.skyProfile = skyProfile;
		return true;
	}

	public void CancelSkyProfileTransition()
	{
		BlendSkyProfiles component = ((Component)this).GetComponent<BlendSkyProfiles>();
		if (!((Object)(object)component == (Object)null))
		{
			component.CancelBlending();
		}
	}

	public void OnBlendComplete(BlendSkyProfiles blender)
	{
		skyProfile = blender.toProfile;
		skyTime = 0f;
	}

	public bool IsBlendingInProgress()
	{
		if (!Object.op_Implicit((Object)(object)((Component)this).GetComponent<BlendSkyProfiles>()))
		{
			return false;
		}
		return true;
	}

	public void UpdateSkyForCurrentTime()
	{
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Invalid comparison between Unknown and I4
		//IL_07a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0734: Unknown result type (might be due to invalid IL or missing references)
		//IL_0739: Unknown result type (might be due to invalid IL or missing references)
		//IL_0740: Unknown result type (might be due to invalid IL or missing references)
		//IL_0391: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0472: Unknown result type (might be due to invalid IL or missing references)
		//IL_056f: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_050c: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_06cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0828: Unknown result type (might be due to invalid IL or missing references)
		//IL_0849: Unknown result type (might be due to invalid IL or missing references)
		//IL_08fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ae7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b08: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e14: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bdb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ea8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a42: Unknown result type (might be due to invalid IL or missing references)
		//IL_106d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d01: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)skyProfile == (Object)null)
		{
			return;
		}
		if ((Object)(object)skyProfile.skyboxMaterial == (Object)null)
		{
			Debug.LogError((object)"Your sky profile is missing a reference to the skybox material.");
			return;
		}
		if (m_SkyMaterialController == null)
		{
			m_SkyMaterialController = new SkyMaterialController();
		}
		m_SkyMaterialController.SkyboxMaterial = skyProfile.skyboxMaterial;
		if ((Object)(object)RenderSettings.skybox == (Object)null || ((Object)RenderSettings.skybox).GetInstanceID() != ((Object)skyProfile.skyboxMaterial).GetInstanceID())
		{
			RenderSettings.skybox = skyProfile.skyboxMaterial;
		}
		SynchronizeAllShaderKeywords();
		if (!UseEnvironmentProfileStack)
		{
			_skyProfileFrame = new SkyProfileFrame(skyProfile, timeOfDay);
		}
		SkyProfileOutput skyProfileOutput = new SkyProfileOutput(skyProfile, timeOfDay);
		if (SkyProfileOverrideStack != null)
		{
			for (int i = 0; i < SkyProfileOverrideStack.Length; i++)
			{
				SkyProfileOverrideStack[i].Apply(skyProfileOutput, timeOfDay);
			}
		}
		SkyMaterialController skyMaterialController = m_SkyMaterialController;
		Texture texturePropertyValue = skyProfile.GetTexturePropertyValue("SkyCubemapKey", timeOfDay);
		skyMaterialController.BackgroundCubemap = (Cubemap)(object)((texturePropertyValue is Cubemap) ? texturePropertyValue : null);
		m_SkyMaterialController.SkyColor = _skyProfileFrame.SkyUpperColor;
		m_SkyMaterialController.SkyMiddleColor = _skyProfileFrame.SkyMiddleColor;
		m_SkyMaterialController.HorizonColor = _skyProfileFrame.SkyLowerColor;
		m_SkyMaterialController.GradientFadeBegin = _skyProfileFrame.HorizonTrasitionStart;
		m_SkyMaterialController.GradientFadeLength = _skyProfileFrame.HorizonTransitionLength;
		m_SkyMaterialController.SkyMiddlePosition = _skyProfileFrame.SkyMiddleColorPosition;
		m_SkyMaterialController.StarFadeBegin = _skyProfileFrame.StarTransitionStart;
		m_SkyMaterialController.StarFadeLength = _skyProfileFrame.StarTransitionLength;
		m_SkyMaterialController.HorizonDistanceScale = _skyProfileFrame.HorizonStarScale;
		if (skyProfile.IsFeatureEnabled("AmbientLightGradient"))
		{
			if ((int)RenderSettings.ambientMode != 1)
			{
				Debug.Log((object)"Sky Profile is using Ambient Light feature, however Unity scene isn't configured for environment gradient. Changing environment to trilight gradient...");
				RenderSettings.ambientMode = (AmbientMode)1;
			}
			if (UseEnvironmentProfileStack)
			{
				RenderSettings.ambientSkyColor = _skyProfileFrame.AmbientLightSkyColor;
				RenderSettings.ambientEquatorColor = _skyProfileFrame.AmbientLightEquatorColor;
				RenderSettings.ambientGroundColor = _skyProfileFrame.AmbientLightGroundColor;
			}
			else
			{
				RenderSettings.ambientSkyColor = skyProfileOutput.ambientSkyColor;
				RenderSettings.ambientEquatorColor = skyProfileOutput.ambientEquatorColor;
				RenderSettings.ambientGroundColor = skyProfileOutput.ambientGroundColor;
			}
		}
		if (skyProfile.IsFeatureEnabled("CloudFeature"))
		{
			m_SkyMaterialController.CloudAlpha = skyProfile.GetNumberPropertyValue("CloudAlphaKey", timeOfDay);
			if (skyProfile.IsFeatureEnabled("NoiseCloudFeature"))
			{
				m_SkyMaterialController.CloudTexture = skyProfile.GetTexturePropertyValue("CloudNoiseTextureKey", timeOfDay);
				m_SkyMaterialController.CloudTextureTiling = skyProfile.GetNumberPropertyValue("CloudTextureTiling", timeOfDay);
				m_SkyMaterialController.CloudDensity = skyProfile.GetNumberPropertyValue("CloudDensityKey", timeOfDay);
				m_SkyMaterialController.CloudSpeed = skyProfile.GetNumberPropertyValue("CloudSpeedKey", timeOfDay);
				m_SkyMaterialController.CloudDirection = skyProfile.GetNumberPropertyValue("CloudDirectionKey", timeOfDay);
				m_SkyMaterialController.CloudHeight = skyProfile.GetNumberPropertyValue("CloudHeightKey", timeOfDay);
				m_SkyMaterialController.CloudColor1 = skyProfile.GetColorPropertyValue("CloudColor1Key", timeOfDay);
				m_SkyMaterialController.CloudColor2 = skyProfile.GetColorPropertyValue("CloudColor2Key", timeOfDay);
				m_SkyMaterialController.CloudFadePosition = skyProfile.GetNumberPropertyValue("CloudFadePositionKey", timeOfDay);
				m_SkyMaterialController.CloudFadeAmount = skyProfile.GetNumberPropertyValue("CloudFadeAmountKey", timeOfDay);
			}
			else if (skyProfile.IsFeatureEnabled("CubemapCloudFeature"))
			{
				m_SkyMaterialController.CloudCubemap = skyProfile.GetTexturePropertyValue("CloudCubemapTextureKey", timeOfDay);
				m_SkyMaterialController.CloudCubemapRotationSpeed = skyProfile.GetNumberPropertyValue("CloudCubemapRotationSpeedKey", timeOfDay);
				m_SkyMaterialController.CloudCubemapTintColor = skyProfile.GetColorPropertyValue("CloudCubemapTintColorKey", timeOfDay);
				m_SkyMaterialController.CloudCubemapHeight = skyProfile.GetNumberPropertyValue("CloudCubemapHeightKey", timeOfDay);
				if (skyProfile.IsFeatureEnabled("CubemapCloudDoubleLayerFeature"))
				{
					m_SkyMaterialController.CloudCubemapDoubleLayerHeight = skyProfile.GetNumberPropertyValue("CloudCubemapDoubleLayerHeightKey", timeOfDay);
					m_SkyMaterialController.CloudCubemapDoubleLayerRotationSpeed = skyProfile.GetNumberPropertyValue("CloudCubemapDoubleLayerRotationSpeedKey", timeOfDay);
					m_SkyMaterialController.CloudCubemapDoubleLayerTintColor = skyProfile.GetColorPropertyValue("CloudCubemapDoubleLayerTintColorKey", timeOfDay);
					if (skyProfile.IsFeatureEnabled("CubemapCloudDoubleLayerCubemap"))
					{
						m_SkyMaterialController.CloudCubemapDoubleLayerCustomTexture = skyProfile.GetTexturePropertyValue("CloudCubemapDoubleLayerCustomTextureKey", timeOfDay);
					}
				}
			}
			else if (skyProfile.IsFeatureEnabled("CubemapNormalCloudFeature"))
			{
				m_SkyMaterialController.CloudCubemapNormalLightDirection = GetPrimaryLightDirection();
				m_SkyMaterialController.CloudCubemapNormalTexture = skyProfile.GetTexturePropertyValue("CloudCubemapNormalTextureKey", timeOfDay);
				m_SkyMaterialController.CloudCubemapNormalLitColor = skyProfile.GetColorPropertyValue("CloudCubemapNormalLitColorKey", timeOfDay);
				m_SkyMaterialController.CloudCubemapNormalShadowColor = skyProfile.GetColorPropertyValue("CloudCubemapNormalShadowColorKey", timeOfDay);
				m_SkyMaterialController.CloudCubemapNormalAmbientIntensity = skyProfile.GetNumberPropertyValue("CloudCubemapNormalAmbientIntensityKey", timeOfDay);
				m_SkyMaterialController.CloudCubemapNormalHeight = skyProfile.GetNumberPropertyValue("CloudCubemapNormalHeightKey", timeOfDay);
				m_SkyMaterialController.CloudCubemapNormalRotationSpeed = skyProfile.GetNumberPropertyValue("CloudCubemapNormalRotationSpeedKey", timeOfDay);
				if (skyProfile.IsFeatureEnabled("CubemapNormalCloudDoubleLayerFeature"))
				{
					m_SkyMaterialController.CloudCubemapNormalDoubleLayerHeight = skyProfile.GetNumberPropertyValue("CloudCubemapNormalDoubleLayerHeightKey", timeOfDay);
					m_SkyMaterialController.CloudCubemapNormalDoubleLayerRotationSpeed = skyProfile.GetNumberPropertyValue("CloudCubemapNormalDoubleLayerRotationSpeedKey", timeOfDay);
					m_SkyMaterialController.CloudCubemapNormalDoubleLayerLitColor = skyProfile.GetColorPropertyValue("CloudCubemapNormalDoubleLayerLitColorKey", timeOfDay);
					m_SkyMaterialController.CloudCubemapNormalDoubleLayerShadowColor = skyProfile.GetColorPropertyValue("CloudCubemapNormalDoubleLayerShadowKey", timeOfDay);
					if (skyProfile.IsFeatureEnabled("CubemapNormalCloudDoubleLayerCubemap"))
					{
						m_SkyMaterialController.CloudCubemapNormalDoubleLayerCustomTexture = skyProfile.GetTexturePropertyValue("CloudCubemapNormalDoubleLayerCustomTextureKey", timeOfDay);
					}
				}
			}
		}
		if (skyProfile.IsFeatureEnabled("FogFeature"))
		{
			Color colorPropertyValue = skyProfile.GetColorPropertyValue("FogColorKey", timeOfDay);
			m_SkyMaterialController.FogColor = colorPropertyValue;
			m_SkyMaterialController.FogDensity = skyProfile.GetNumberPropertyValue("FogEndDistanceKey", timeOfDay);
			m_SkyMaterialController.FogHeight = skyProfile.GetNumberPropertyValue("FogLengthKey", timeOfDay);
			if (skyProfile.GetBoolPropertyValue("FogSyncWithGlobal", timeOfDay))
			{
				RenderSettings.fogColor = colorPropertyValue;
			}
		}
		RenderSettings.fogColor = skyProfileOutput.fogColor;
		RenderSettings.fogEndDistance = skyProfileOutput.fogEndDistance * (Singleton<EnvironmentFX>.InstanceExists ? Singleton<EnvironmentFX>.Instance.FogEndDistanceMultiplier : 250f);
		if (skyProfile.IsFeatureEnabled("SunFeature") && Object.op_Implicit((Object)(object)sunOrbit))
		{
			sunOrbit.Point = skyProfile.GetSpherePointPropertyValue("SunPositionKey", timeOfDay);
			m_SkyMaterialController.SunDirection = sunOrbit.BodyGlobalDirection;
			m_SkyMaterialController.SunColor = skyProfile.GetColorPropertyValue("SunColorKey", timeOfDay);
			m_SkyMaterialController.SunSize = skyProfile.GetNumberPropertyValue("SunSizeKey", timeOfDay);
			m_SkyMaterialController.SunEdgeFeathering = skyProfile.GetNumberPropertyValue("SunEdgeFeatheringKey", timeOfDay);
			m_SkyMaterialController.SunBloomFilterBoost = skyProfile.GetNumberPropertyValue("SunColorIntensityKey", timeOfDay);
			m_SkyMaterialController.SunAlpha = skyProfile.GetNumberPropertyValue("SunAlphaKey", timeOfDay);
			if (skyProfile.IsFeatureEnabled("SunCustomTextureFeature"))
			{
				m_SkyMaterialController.SunWorldToLocalMatrix = sunOrbit.positionTransform.worldToLocalMatrix;
				m_SkyMaterialController.SunTexture = skyProfile.GetTexturePropertyValue("SunTextureKey", timeOfDay);
				if (skyProfile.IsFeatureEnabled("SunRotationFeature"))
				{
					sunOrbit.rotateBody.SpinSpeed = skyProfile.GetNumberPropertyValue("SunRotationSpeedKey", timeOfDay);
				}
			}
			if (skyProfile.IsFeatureEnabled("SunSpriteSheetFeature"))
			{
				m_SkyMaterialController.SetSunSpriteDimensions((int)skyProfile.GetNumberPropertyValue("SunSpriteColumnCountKey", timeOfDay), (int)skyProfile.GetNumberPropertyValue("SunSpriteRowCountKey", timeOfDay));
				m_SkyMaterialController.SunSpriteItemCount = (int)skyProfile.GetNumberPropertyValue("SunSpriteItemCount", timeOfDay);
				m_SkyMaterialController.SunSpriteAnimationSpeed = skyProfile.GetNumberPropertyValue("SunSpriteAnimationSpeed", timeOfDay);
			}
			if (Object.op_Implicit((Object)(object)sunOrbit.BodyLight))
			{
				if (!((Behaviour)sunOrbit.BodyLight).enabled)
				{
					((Behaviour)sunOrbit.BodyLight).enabled = true;
				}
				RenderSettings.sun = sunOrbit.BodyLight;
				sunOrbit.BodyLight.color = skyProfileOutput.sunLightColor;
				sunOrbit.BodyLight.intensity = skyProfileOutput.sunLightIntensity;
			}
		}
		else if (Object.op_Implicit((Object)(object)sunOrbit) && Object.op_Implicit((Object)(object)sunOrbit.BodyLight))
		{
			((Behaviour)sunOrbit.BodyLight).enabled = false;
		}
		if (skyProfile.IsFeatureEnabled("MoonFeature") && Object.op_Implicit((Object)(object)moonOrbit))
		{
			moonOrbit.Point = skyProfile.GetSpherePointPropertyValue("MoonPositionKey", timeOfDay);
			m_SkyMaterialController.MoonDirection = moonOrbit.BodyGlobalDirection;
			m_SkyMaterialController.MoonColor = skyProfile.GetColorPropertyValue("MoonColorKey", timeOfDay);
			m_SkyMaterialController.MoonSize = skyProfile.GetNumberPropertyValue("MoonSizeKey", timeOfDay);
			m_SkyMaterialController.MoonEdgeFeathering = skyProfile.GetNumberPropertyValue("MoonEdgeFeatheringKey", timeOfDay);
			m_SkyMaterialController.MoonBloomFilterBoost = skyProfile.GetNumberPropertyValue("MoonColorIntensityKey", timeOfDay);
			m_SkyMaterialController.MoonAlpha = skyProfile.GetNumberPropertyValue("MoonAlphaKey", timeOfDay);
			if (skyProfile.IsFeatureEnabled("MoonCustomTextureFeature"))
			{
				m_SkyMaterialController.MoonTexture = skyProfile.GetTexturePropertyValue("MoonTextureKey", timeOfDay);
				m_SkyMaterialController.MoonWorldToLocalMatrix = moonOrbit.positionTransform.worldToLocalMatrix;
				if (skyProfile.IsFeatureEnabled("MoonRotationFeature"))
				{
					moonOrbit.rotateBody.SpinSpeed = skyProfile.GetNumberPropertyValue("MoonRotationSpeedKey", timeOfDay);
				}
			}
			if (skyProfile.IsFeatureEnabled("MoonSpriteSheetFeature"))
			{
				m_SkyMaterialController.SetMoonSpriteDimensions((int)skyProfile.GetNumberPropertyValue("MoonSpriteColumnCountKey", timeOfDay), (int)skyProfile.GetNumberPropertyValue("MoonSpriteRowCountKey", timeOfDay));
				m_SkyMaterialController.MoonSpriteItemCount = (int)skyProfile.GetNumberPropertyValue("MoonSpriteItemCount", timeOfDay);
				m_SkyMaterialController.MoonSpriteAnimationSpeed = skyProfile.GetNumberPropertyValue("MoonSpriteAnimationSpeed", timeOfDay);
			}
			if (Object.op_Implicit((Object)(object)moonOrbit.BodyLight))
			{
				if (!((Behaviour)moonOrbit.BodyLight).enabled)
				{
					((Behaviour)moonOrbit.BodyLight).enabled = true;
				}
				moonOrbit.BodyLight.color = skyProfile.GetColorPropertyValue("MoonLightColorKey", timeOfDay);
				moonOrbit.BodyLight.intensity = skyProfile.GetNumberPropertyValue("MoonLightIntensityKey", timeOfDay);
			}
		}
		else if (Object.op_Implicit((Object)(object)moonOrbit) && Object.op_Implicit((Object)(object)moonOrbit.BodyLight))
		{
			((Behaviour)moonOrbit.BodyLight).enabled = false;
		}
		if (skyProfile.IsFeatureEnabled("StarBasicFeature"))
		{
			m_SkyMaterialController.StarBasicCubemap = skyProfile.GetTexturePropertyValue("StarBasicCubemapKey", timeOfDay);
			m_SkyMaterialController.StarBasicTwinkleSpeed = skyProfile.GetNumberPropertyValue("StarBasicTwinkleSpeedKey", timeOfDay);
			m_SkyMaterialController.StarBasicTwinkleAmount = skyProfile.GetNumberPropertyValue("StarBasicTwinkleAmountKey", timeOfDay);
			m_SkyMaterialController.StarBasicOpacity = skyProfile.GetNumberPropertyValue("StarBasicOpacityKey", timeOfDay);
			m_SkyMaterialController.StarBasicTintColor = skyProfile.GetColorPropertyValue("StarBasicTintColorKey", timeOfDay);
			m_SkyMaterialController.StarBasicExponent = skyProfile.GetNumberPropertyValue("StarBasicExponentKey", timeOfDay);
			m_SkyMaterialController.StarBasicIntensity = skyProfile.GetNumberPropertyValue("StarBasicIntensityKey", timeOfDay);
		}
		else
		{
			if (skyProfile.IsFeatureEnabled("StarLayer1Feature"))
			{
				m_SkyMaterialController.StarLayer1DataTexture = skyProfile.starLayer1DataTexture;
				m_SkyMaterialController.StarLayer1Color = skyProfile.GetColorPropertyValue("Star1ColorKey", timeOfDay);
				m_SkyMaterialController.StarLayer1MaxRadius = skyProfile.GetNumberPropertyValue("Star1SizeKey", timeOfDay);
				m_SkyMaterialController.StarLayer1Texture = skyProfile.GetTexturePropertyValue("Star1TextureKey", timeOfDay);
				m_SkyMaterialController.StarLayer1TwinkleAmount = skyProfile.GetNumberPropertyValue("Star1TwinkleAmountKey", timeOfDay);
				m_SkyMaterialController.StarLayer1TwinkleSpeed = skyProfile.GetNumberPropertyValue("Star1TwinkleSpeedKey", timeOfDay);
				m_SkyMaterialController.StarLayer1RotationSpeed = skyProfile.GetNumberPropertyValue("Star1RotationSpeed", timeOfDay);
				m_SkyMaterialController.StarLayer1EdgeFeathering = skyProfile.GetNumberPropertyValue("Star1EdgeFeathering", timeOfDay);
				m_SkyMaterialController.StarLayer1BloomFilterBoost = skyProfile.GetNumberPropertyValue("Star1ColorIntensityKey", timeOfDay);
				if (skyProfile.IsFeatureEnabled("StarLayer1SpriteSheetFeature"))
				{
					m_SkyMaterialController.StarLayer1SpriteItemCount = (int)skyProfile.GetNumberPropertyValue("Star1SpriteItemCount", timeOfDay);
					m_SkyMaterialController.StarLayer1SpriteAnimationSpeed = (int)skyProfile.GetNumberPropertyValue("Star1SpriteAnimationSpeed", timeOfDay);
					m_SkyMaterialController.SetStarLayer1SpriteDimensions((int)skyProfile.GetNumberPropertyValue("Star1SpriteColumnCountKey", timeOfDay), (int)skyProfile.GetNumberPropertyValue("Star1SpriteRowCountKey", timeOfDay));
				}
			}
			if (skyProfile.IsFeatureEnabled("StarLayer2Feature"))
			{
				m_SkyMaterialController.StarLayer2DataTexture = skyProfile.starLayer2DataTexture;
				m_SkyMaterialController.StarLayer2Color = skyProfile.GetColorPropertyValue("Star2ColorKey", timeOfDay);
				m_SkyMaterialController.StarLayer2MaxRadius = skyProfile.GetNumberPropertyValue("Star2SizeKey", timeOfDay);
				m_SkyMaterialController.StarLayer2Texture = skyProfile.GetTexturePropertyValue("Star2TextureKey", timeOfDay);
				m_SkyMaterialController.StarLayer2TwinkleAmount = skyProfile.GetNumberPropertyValue("Star2TwinkleAmountKey", timeOfDay);
				m_SkyMaterialController.StarLayer2TwinkleSpeed = skyProfile.GetNumberPropertyValue("Star2TwinkleSpeedKey", timeOfDay);
				m_SkyMaterialController.StarLayer2RotationSpeed = skyProfile.GetNumberPropertyValue("Star2RotationSpeed", timeOfDay);
				m_SkyMaterialController.StarLayer2EdgeFeathering = skyProfile.GetNumberPropertyValue("Star2EdgeFeathering", timeOfDay);
				m_SkyMaterialController.StarLayer2BloomFilterBoost = skyProfile.GetNumberPropertyValue("Star2ColorIntensityKey", timeOfDay);
				if (skyProfile.IsFeatureEnabled("StarLayer2SpriteSheetFeature"))
				{
					m_SkyMaterialController.StarLayer2SpriteItemCount = (int)skyProfile.GetNumberPropertyValue("Star2SpriteItemCount", timeOfDay);
					m_SkyMaterialController.StarLayer2SpriteAnimationSpeed = (int)skyProfile.GetNumberPropertyValue("Star2SpriteAnimationSpeed", timeOfDay);
					m_SkyMaterialController.SetStarLayer2SpriteDimensions((int)skyProfile.GetNumberPropertyValue("Star2SpriteColumnCountKey", timeOfDay), (int)skyProfile.GetNumberPropertyValue("Star2SpriteRowCountKey", timeOfDay));
				}
			}
			if (skyProfile.IsFeatureEnabled("StarLayer3Feature"))
			{
				m_SkyMaterialController.StarLayer3DataTexture = skyProfile.starLayer3DataTexture;
				m_SkyMaterialController.StarLayer3Color = skyProfile.GetColorPropertyValue("Star3ColorKey", timeOfDay);
				m_SkyMaterialController.StarLayer3MaxRadius = skyProfile.GetNumberPropertyValue("Star3SizeKey", timeOfDay);
				m_SkyMaterialController.StarLayer3Texture = skyProfile.GetTexturePropertyValue("Star3TextureKey", timeOfDay);
				m_SkyMaterialController.StarLayer3TwinkleAmount = skyProfile.GetNumberPropertyValue("Star3TwinkleAmountKey", timeOfDay);
				m_SkyMaterialController.StarLayer3TwinkleSpeed = skyProfile.GetNumberPropertyValue("Star3TwinkleSpeedKey", timeOfDay);
				m_SkyMaterialController.StarLayer3RotationSpeed = skyProfile.GetNumberPropertyValue("Star3RotationSpeed", timeOfDay);
				m_SkyMaterialController.StarLayer3EdgeFeathering = skyProfile.GetNumberPropertyValue("Star3EdgeFeathering", timeOfDay);
				m_SkyMaterialController.StarLayer3BloomFilterBoost = skyProfile.GetNumberPropertyValue("Star3ColorIntensityKey", timeOfDay);
				if (skyProfile.IsFeatureEnabled("StarLayer3SpriteSheetFeature"))
				{
					m_SkyMaterialController.StarLayer3SpriteItemCount = (int)skyProfile.GetNumberPropertyValue("Star3SpriteItemCount", timeOfDay);
					m_SkyMaterialController.StarLayer3SpriteAnimationSpeed = (int)skyProfile.GetNumberPropertyValue("Star3SpriteAnimationSpeed", timeOfDay);
					m_SkyMaterialController.SetStarLayer3SpriteDimensions((int)skyProfile.GetNumberPropertyValue("Star3SpriteColumnCountKey", timeOfDay), (int)skyProfile.GetNumberPropertyValue("Star3SpriteRowCountKey", timeOfDay));
				}
			}
		}
		if (updateGlobalIllumination)
		{
			UpdateGlobalIllumination();
		}
		if (this.timeChangedCallback != null)
		{
			this.timeChangedCallback(this, timeOfDay);
		}
	}
}
