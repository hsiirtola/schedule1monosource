using FishNet.Managing.Transporting;
using FishNet.Transporting.Multipass;
using FishNet.Transporting.Yak;
using UnityEngine;

namespace ScheduleOne.Networking;

public class TransportInitializer : MonoBehaviour
{
	public void Awake()
	{
		((Component)this).GetComponent<TransportManager>().GetTransport<Multipass>().SetClientTransport<Yak>();
	}
}
