namespace ScheduleOne.Audio;

public class LegacyAudioSourceController : AudioSourceController
{
	private void FixedUpdate()
	{
		if (base.IsPlaying)
		{
			ApplyVolume();
		}
	}
}
