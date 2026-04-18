using UnityEngine;

namespace Funly.SkyStudio;

[RequireComponent(typeof(AudioSource))]
public class RainDownfallController : MonoBehaviour, ISkyModule
{
	public MeshRenderer rainMeshRenderer;

	public Material rainMaterial;

	private MaterialPropertyBlock m_PropertyBlock;

	private AudioSource m_RainAudioSource;

	private float m_TimeOfDay;

	private SkyProfile m_SkyProfile;

	public void SetWeatherEnclosure(WeatherEnclosure enclosure)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		if ((Object)(object)rainMeshRenderer != (Object)null)
		{
			((Renderer)rainMeshRenderer).enabled = false;
			rainMeshRenderer = null;
		}
		if (!Object.op_Implicit((Object)(object)enclosure))
		{
			return;
		}
		rainMeshRenderer = ((Component)enclosure).GetComponentInChildren<MeshRenderer>();
		if (!Object.op_Implicit((Object)(object)rainMeshRenderer))
		{
			Debug.LogError((object)"Can't render rain since there's no MeshRenderer on the WeatherEnclosure");
			return;
		}
		m_PropertyBlock = new MaterialPropertyBlock();
		if (Object.op_Implicit((Object)(object)rainMaterial))
		{
			((Renderer)rainMeshRenderer).material = rainMaterial;
			((Renderer)rainMeshRenderer).enabled = true;
			UpdateForTimeOfDay(m_SkyProfile, m_TimeOfDay);
		}
	}

	private void Update()
	{
		if (!((Object)(object)m_SkyProfile == (Object)null))
		{
			UpdateForTimeOfDay(m_SkyProfile, m_TimeOfDay);
		}
	}

	public void UpdateForTimeOfDay(SkyProfile skyProfile, float timeOfDay)
	{
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected O, but got Unknown
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		m_SkyProfile = skyProfile;
		m_TimeOfDay = timeOfDay;
		if (!Object.op_Implicit((Object)(object)skyProfile))
		{
			return;
		}
		if ((Object)(object)m_RainAudioSource == (Object)null)
		{
			m_RainAudioSource = ((Component)this).GetComponent<AudioSource>();
		}
		if ((Object)(object)skyProfile == (Object)null || !m_SkyProfile.IsFeatureEnabled("RainFeature"))
		{
			if ((Object)(object)m_RainAudioSource != (Object)null)
			{
				((Behaviour)m_RainAudioSource).enabled = false;
			}
			return;
		}
		if (!Object.op_Implicit((Object)(object)rainMaterial))
		{
			Debug.LogError((object)"Can't render rain without a rain material");
			return;
		}
		if (!Object.op_Implicit((Object)(object)rainMeshRenderer))
		{
			Debug.LogError((object)"Can't show rain without an enclosure mesh renderer.");
			return;
		}
		if (m_PropertyBlock == null)
		{
			m_PropertyBlock = new MaterialPropertyBlock();
		}
		((Renderer)rainMeshRenderer).enabled = true;
		((Renderer)rainMeshRenderer).material = rainMaterial;
		((Renderer)rainMeshRenderer).GetPropertyBlock(m_PropertyBlock);
		float numberPropertyValue = skyProfile.GetNumberPropertyValue("RainNearIntensityKey", timeOfDay);
		float numberPropertyValue2 = skyProfile.GetNumberPropertyValue("RainFarIntensityKey", timeOfDay);
		Texture texturePropertyValue = skyProfile.GetTexturePropertyValue("RainNearTextureKey", timeOfDay);
		Texture texturePropertyValue2 = skyProfile.GetTexturePropertyValue("RainFarTextureKey", timeOfDay);
		float numberPropertyValue3 = skyProfile.GetNumberPropertyValue("RainNearSpeedKey", timeOfDay);
		float numberPropertyValue4 = skyProfile.GetNumberPropertyValue("RainFarSpeedKey", timeOfDay);
		Color colorPropertyValue = m_SkyProfile.GetColorPropertyValue("RainTintColorKey", m_TimeOfDay);
		float numberPropertyValue5 = m_SkyProfile.GetNumberPropertyValue("RainWindTurbulenceKey", m_TimeOfDay);
		float numberPropertyValue6 = m_SkyProfile.GetNumberPropertyValue("RainWindTurbulenceSpeedKey", m_TimeOfDay);
		float numberPropertyValue7 = m_SkyProfile.GetNumberPropertyValue("RainNearTextureTiling", m_TimeOfDay);
		float numberPropertyValue8 = m_SkyProfile.GetNumberPropertyValue("RainFarTextureTiling", m_TimeOfDay);
		if ((Object)(object)texturePropertyValue != (Object)null)
		{
			m_PropertyBlock.SetTexture("_NearTex", texturePropertyValue);
			m_PropertyBlock.SetVector("_NearTex_ST", new Vector4(numberPropertyValue7, numberPropertyValue7, numberPropertyValue7, 1f));
		}
		m_PropertyBlock.SetFloat("_NearDensity", numberPropertyValue);
		m_PropertyBlock.SetFloat("_NearRainSpeed", numberPropertyValue3);
		if ((Object)(object)texturePropertyValue2 != (Object)null)
		{
			m_PropertyBlock.SetTexture("_FarTex", texturePropertyValue2);
			m_PropertyBlock.SetVector("_FarTex_ST", new Vector4(numberPropertyValue8, numberPropertyValue8, numberPropertyValue8, 1f));
		}
		m_PropertyBlock.SetFloat("_FarDensity", numberPropertyValue2);
		m_PropertyBlock.SetFloat("_FarRainSpeed", numberPropertyValue4);
		m_PropertyBlock.SetColor("_TintColor", colorPropertyValue);
		m_PropertyBlock.SetFloat("_Turbulence", numberPropertyValue5);
		m_PropertyBlock.SetFloat("_TurbulenceSpeed", numberPropertyValue6);
		((Renderer)rainMeshRenderer).SetPropertyBlock(m_PropertyBlock);
		if (skyProfile.IsFeatureEnabled("RainSoundFeature"))
		{
			((Behaviour)m_RainAudioSource).enabled = true;
			m_RainAudioSource.volume = skyProfile.GetNumberPropertyValue("RainSoundVolume", timeOfDay);
		}
		else
		{
			((Behaviour)m_RainAudioSource).enabled = false;
		}
	}
}
