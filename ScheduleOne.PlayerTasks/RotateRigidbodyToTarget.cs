using UnityEngine;

namespace ScheduleOne.PlayerTasks;

public class RotateRigidbodyToTarget : MonoBehaviour
{
	public Rigidbody Rigidbody;

	public Vector3 TargetRotation;

	public float RotationForce = 1f;

	public Transform Bitch;

	public void FixedUpdate()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		Bitch.localRotation = Quaternion.Euler(TargetRotation);
		Quaternion rotation = Bitch.rotation;
		Quaternion val = rotation * Quaternion.Inverse(((Component)this).transform.rotation);
		Vector3 val2 = Vector3.Normalize(new Vector3(val.x, val.y, val.z)) * RotationForce;
		float num = Mathf.Clamp01(Quaternion.Angle(((Component)this).transform.rotation, rotation) / 90f);
		Rigidbody.AddTorque(val2 * num, (ForceMode)5);
	}
}
