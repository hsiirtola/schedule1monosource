using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Equipping;

public class Equippable_PumpShotgun : Equippable_RangedWeapon
{
	[Header("Shotgun Settings")]
	public int PelletCount = 8;

	protected override Vector3[] GetBulletDirections()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		float spreadAngle = GetSpreadAngle();
		Vector3 forward = ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.forward;
		Vector3[] array = (Vector3[])(object)new Vector3[PelletCount];
		for (int i = 0; i < PelletCount; i++)
		{
			array[i] = Equippable_RangedWeapon.SpreadDirection(forward, spreadAngle);
		}
		return array;
	}
}
