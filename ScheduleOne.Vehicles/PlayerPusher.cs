using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Vehicles;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class PlayerPusher : MonoBehaviour
{
	private LandVehicle veh;

	[Header("Settings")]
	public float MinSpeedToPush = 3f;

	public float MaxPushSpeed = 20f;

	public float MinPushForce = 0.5f;

	public float MaxPushForce = 5f;

	private Collider collider;

	private void Awake()
	{
		veh = ((Component)this).GetComponentInParent<LandVehicle>();
		veh.RegisterPusher(this);
		collider = ((Component)this).GetComponent<Collider>();
		LayerUtility.SetLayerRecursively(((Component)this).gameObject, LayerMask.NameToLayer("Ignore Raycast"));
	}

	private void OnDestroy()
	{
		veh.DeregisterPusher(this);
	}

	public void SetEnabled(bool isEnabled)
	{
		collider.enabled = isEnabled;
	}

	private void OnTriggerStay(Collider other)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		if (!(veh.Speed_Kmh < MinSpeedToPush))
		{
			Player componentInParent = ((Component)other).GetComponentInParent<Player>();
			if ((Object)(object)componentInParent != (Object)null && (Object)(object)componentInParent == (Object)(object)Player.Local && (Object)(object)componentInParent.CurrentVehicle == (Object)null)
			{
				Vector3 val = ((Component)componentInParent).transform.position - ((Component)this).transform.position;
				val = Vector3.Project(((Vector3)(ref val)).normalized, ((Component)this).transform.right);
				Vector3 normalized = ((Vector3)(ref val)).normalized;
				float num = MinPushForce + Mathf.Clamp((veh.Speed_Kmh - MinSpeedToPush) / MaxPushSpeed, 0f, 1f) * (MaxPushSpeed - MinPushForce);
				PlayerSingleton<PlayerMovement>.Instance.Controller.Move(normalized * num * Time.fixedDeltaTime);
			}
		}
	}
}
