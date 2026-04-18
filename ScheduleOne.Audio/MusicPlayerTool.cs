using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Audio;

public class MusicPlayerTool : MonoBehaviour
{
	public void PlayTrack(string trackName)
	{
		Singleton<MusicManager>.Instance.SetTrackEnabled(trackName, enabled: true);
	}

	public void StopTracks()
	{
		Singleton<MusicManager>.Instance.StopAndDisableTracks();
	}
}
