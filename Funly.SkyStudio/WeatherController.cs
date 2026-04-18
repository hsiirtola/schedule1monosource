using System;
using UnityEngine;

namespace Funly.SkyStudio;

public class WeatherController : MonoBehaviour
{
	private WeatherEnclosure m_Enclosure;

	private MeshRenderer m_EnclosureMeshRenderer;

	private WeatherEnclosureDetector detector;

	private SkyProfile m_Profile;

	private float m_TimeOfDay;

	public RainDownfallController rainDownfallController { get; protected set; }

	public RainSplashController rainSplashController { get; protected set; }

	public LightningController lightningController { get; protected set; }

	public WeatherDepthCamera weatherDepthCamera { get; protected set; }

	private void Awake()
	{
		DiscoverWeatherControllers();
	}

	private void Start()
	{
		DiscoverWeatherControllers();
	}

	private void OnEnable()
	{
		DiscoverWeatherControllers();
		if ((Object)(object)detector == (Object)null)
		{
			Debug.LogError((object)"Can't register for enclosure callbacks since there's no WeatherEnclosureDetector on any children");
			return;
		}
		WeatherEnclosureDetector weatherEnclosureDetector = detector;
		weatherEnclosureDetector.enclosureChangedCallback = (Action<WeatherEnclosure>)Delegate.Combine(weatherEnclosureDetector.enclosureChangedCallback, new Action<WeatherEnclosure>(OnEnclosureDidChange));
	}

	private void DiscoverWeatherControllers()
	{
		rainDownfallController = ((Component)this).GetComponentInChildren<RainDownfallController>();
		rainSplashController = ((Component)this).GetComponentInChildren<RainSplashController>();
		lightningController = ((Component)this).GetComponentInChildren<LightningController>();
		weatherDepthCamera = ((Component)this).GetComponentInChildren<WeatherDepthCamera>();
		detector = ((Component)this).GetComponentInChildren<WeatherEnclosureDetector>();
	}

	private void OnDisable()
	{
		if (!((Object)(object)detector == (Object)null))
		{
			WeatherEnclosureDetector weatherEnclosureDetector = detector;
			weatherEnclosureDetector.enclosureChangedCallback = (Action<WeatherEnclosure>)Delegate.Remove(weatherEnclosureDetector.enclosureChangedCallback, new Action<WeatherEnclosure>(OnEnclosureDidChange));
		}
	}

	public void UpdateForTimeOfDay(SkyProfile skyProfile, float timeOfDay)
	{
		if (Object.op_Implicit((Object)(object)skyProfile))
		{
			m_Profile = skyProfile;
			m_TimeOfDay = timeOfDay;
			if ((Object)(object)weatherDepthCamera != (Object)null)
			{
				((Behaviour)weatherDepthCamera).enabled = skyProfile.IsFeatureEnabled("RainSplashFeature");
			}
			if ((Object)(object)rainDownfallController != (Object)null)
			{
				rainDownfallController.UpdateForTimeOfDay(skyProfile, timeOfDay);
			}
			if ((Object)(object)rainSplashController != (Object)null)
			{
				rainSplashController.UpdateForTimeOfDay(skyProfile, timeOfDay);
			}
			if ((Object)(object)lightningController != (Object)null)
			{
				lightningController.UpdateForTimeOfDay(skyProfile, timeOfDay);
			}
		}
	}

	private void LateUpdate()
	{
		if (!((Object)(object)m_Profile == (Object)null))
		{
			if (Object.op_Implicit((Object)(object)m_EnclosureMeshRenderer) && Object.op_Implicit((Object)(object)rainDownfallController) && m_Profile.IsFeatureEnabled("RainFeature"))
			{
				((Renderer)m_EnclosureMeshRenderer).enabled = true;
			}
			else
			{
				((Renderer)m_EnclosureMeshRenderer).enabled = false;
			}
		}
	}

	private void OnEnclosureDidChange(WeatherEnclosure enclosure)
	{
		m_Enclosure = enclosure;
		if ((Object)(object)m_Enclosure != (Object)null)
		{
			m_EnclosureMeshRenderer = ((Component)m_Enclosure).GetComponentInChildren<MeshRenderer>();
		}
		rainDownfallController.SetWeatherEnclosure(m_Enclosure);
		UpdateForTimeOfDay(m_Profile, m_TimeOfDay);
	}
}
