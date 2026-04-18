using ScheduleOne.Map;
using UnityEngine;

namespace ScheduleOne.Audio;

public class SewerAmbientSound : MonoBehaviour
{
	public SewerCameraPresense SewerCameraPresense;

	public AudioSourceController SewerAmbienceSource;

	private void Awake()
	{
	}

	private void Update()
	{
		SewerAmbienceSource.VolumeMultiplier = SewerCameraPresense.SmoothedCameraPresenceInSewerArea;
		if (SewerAmbienceSource.VolumeMultiplier > 0f && !SewerAmbienceSource.IsPlaying)
		{
			SewerAmbienceSource.Play();
		}
		else if (SewerAmbienceSource.VolumeMultiplier <= 0f && SewerAmbienceSource.IsPlaying)
		{
			SewerAmbienceSource.Stop();
		}
	}
}
