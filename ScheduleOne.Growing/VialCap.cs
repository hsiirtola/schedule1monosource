using ScheduleOne.PlayerTasks;
using UnityEngine;

namespace ScheduleOne.Growing;

public class VialCap : Clickable
{
	public Collider Collider;

	private Rigidbody RigidBody;

	public bool Removed { get; protected set; }

	public override void StartClick(RaycastHit hit)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		base.StartClick(hit);
		Pop();
	}

	private void Pop()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		RigidBody = ((Component)this).gameObject.AddComponent<Rigidbody>();
		Removed = true;
		Collider.enabled = false;
		RigidBody.isKinematic = false;
		RigidBody.useGravity = true;
		((Component)this).transform.SetParent((Transform)null);
		RigidBody.AddRelativeForce(Vector3.forward * 1.5f, (ForceMode)2);
		RigidBody.AddRelativeForce(Vector3.up * 0.5f, (ForceMode)2);
		RigidBody.AddTorque(Vector3.up * 1.5f, (ForceMode)2);
		Object.Destroy((Object)(object)((Component)this).gameObject, 3f);
		((Behaviour)this).enabled = false;
	}
}
