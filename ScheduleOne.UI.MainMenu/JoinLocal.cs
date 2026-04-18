using FishNet.Managing;
using UnityEngine;

namespace ScheduleOne.UI.MainMenu;

public class JoinLocal : MonoBehaviour
{
	private void Awake()
	{
		((Component)this).gameObject.SetActive(Application.isEditor || Debug.isDebugBuild);
	}

	public void Clicked()
	{
		Object.FindObjectOfType<NetworkManager>().ClientManager.StartConnection();
	}
}
