using UnityEngine;

namespace Funly.SkyStudio;

public class TransitionBetweenProfiles : MonoBehaviour
{
	public SkyProfile daySkyProfile;

	public SkyProfile nightSkyProfile;

	[Tooltip("How long the transition animation will last.")]
	[Range(0.1f, 30f)]
	public float transitionDuration = 2f;

	public TimeOfDayController timeOfDayController;

	private SkyProfile m_CurrentSkyProfile;

	private void Start()
	{
		m_CurrentSkyProfile = daySkyProfile;
		if ((Object)(object)timeOfDayController == (Object)null)
		{
			timeOfDayController = TimeOfDayController.instance;
		}
		timeOfDayController.skyProfile = m_CurrentSkyProfile;
	}

	public void ToggleSkyProfiles()
	{
		m_CurrentSkyProfile = (((Object)(object)m_CurrentSkyProfile == (Object)(object)daySkyProfile) ? nightSkyProfile : daySkyProfile);
		timeOfDayController.StartSkyProfileTransition(m_CurrentSkyProfile, transitionDuration);
	}
}
