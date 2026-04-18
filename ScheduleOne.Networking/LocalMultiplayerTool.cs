using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using UnityEngine;

namespace ScheduleOne.Networking;

public class LocalMultiplayerTool : MonoBehaviour
{
	private void Update()
	{
		if (!Singleton<LoadManager>.Instance.IsLoading && (Application.isEditor || Debug.isDebugBuild) && Input.GetKeyDown((KeyCode)287))
		{
			Singleton<LoadManager>.Instance.LoadAsClient("localhost");
		}
	}
}
