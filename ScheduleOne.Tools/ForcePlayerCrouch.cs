using FishNet.Object;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Tools;

public class ForcePlayerCrouch : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (((Component)other).gameObject.layer == LayerMask.NameToLayer("Player"))
		{
			Player componentInParent = ((Component)other).gameObject.GetComponentInParent<Player>();
			if ((Object)(object)componentInParent != (Object)null && ((NetworkBehaviour)componentInParent).IsOwner && !PlayerSingleton<PlayerMovement>.Instance.IsCrouched)
			{
				PlayerSingleton<PlayerMovement>.Instance.SetCrouched(c: true);
			}
		}
	}
}
