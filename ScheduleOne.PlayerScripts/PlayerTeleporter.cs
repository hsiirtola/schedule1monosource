using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.PlayerScripts;

public class PlayerTeleporter : MonoBehaviour
{
	public void Teleport(Transform destination)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		PlayerSingleton<PlayerMovement>.Instance.Teleport(destination.position);
		((Component)Player.Local).transform.rotation = destination.rotation;
		((Component)Player.Local).transform.eulerAngles = new Vector3(0f, ((Component)Player.Local).transform.eulerAngles.y, 0f);
	}
}
