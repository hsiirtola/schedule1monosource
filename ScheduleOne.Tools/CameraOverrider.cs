using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Tools;

public class CameraOverrider : MonoBehaviour
{
	public float FOV = 70f;

	public void LateUpdate()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(((Component)this).transform.position, ((Component)this).transform.rotation, 0f);
		PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(FOV, 0f);
	}
}
