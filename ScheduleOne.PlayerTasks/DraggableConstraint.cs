using UnityEngine;

namespace ScheduleOne.PlayerTasks;

public class DraggableConstraint : MonoBehaviour
{
	public Transform Container;

	public Rigidbody Anchor;

	public bool ProportionalZClamp;

	public bool AlignUpToContainerPlane;

	[Header("Up Direction Clamping")]
	public bool ClampUpDirection;

	public float UpDirectionMaxDifference = 45f;

	private Vector3 startLocalPos;

	private Draggable draggable;

	private ConfigurableJoint joint;

	private Vector3 RelativePos
	{
		get
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)Container != (Object)null))
			{
				return ((Component)this).transform.localPosition;
			}
			return Container.InverseTransformPoint(((Component)this).transform.position);
		}
	}

	private void Start()
	{
		draggable = ((Component)this).GetComponent<Draggable>();
		if (ClampUpDirection)
		{
			joint = ((Component)draggable.Rb).gameObject.AddComponent<ConfigurableJoint>();
			if ((Object)(object)Anchor == (Object)null && (Object)(object)Container != (Object)null)
			{
				((Component)Container).gameObject.AddComponent<Rigidbody>();
				Anchor = ((Component)Container).gameObject.GetComponent<Rigidbody>();
				Anchor.isKinematic = true;
				Anchor.useGravity = false;
			}
			((Joint)joint).connectedBody = Anchor;
			joint.zMotion = (ConfigurableJointMotion)0;
			joint.angularXMotion = (ConfigurableJointMotion)0;
			joint.angularYMotion = (ConfigurableJointMotion)0;
			joint.angularZMotion = (ConfigurableJointMotion)1;
		}
	}

	public void SetContainer(Transform container)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		Container = container;
		startLocalPos = RelativePos;
		if ((Object)(object)joint != (Object)null && (Object)(object)Anchor == (Object)null && (Object)(object)Container != (Object)null)
		{
			Anchor = ((Component)Container).gameObject.AddComponent<Rigidbody>();
			Anchor.isKinematic = true;
			Anchor.useGravity = false;
			((Joint)joint).connectedBody = Anchor;
		}
	}

	protected virtual void FixedUpdate()
	{
		if (AlignUpToContainerPlane)
		{
			AlignToContainerPlane();
		}
	}

	protected virtual void LateUpdate()
	{
		if (ProportionalZClamp)
		{
			ProportionalClamp();
		}
		if (ClampUpDirection)
		{
			ClampUpRot();
		}
	}

	private void ProportionalClamp()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)Container == (Object)null) && !((Object)(object)draggable == (Object)null))
		{
			float num = Mathf.Clamp(Mathf.Abs(RelativePos.x) / startLocalPos.x, 0f, 1f);
			float num2 = Mathf.Abs(startLocalPos.z) * num;
			Vector3 val = Container.InverseTransformPoint(draggable.originalHitPoint);
			val.z = Mathf.Clamp(val.z, 0f - num2, num2);
			Vector3 originalHitPoint = Container.TransformPoint(val);
			draggable.SetOriginalHitPoint(originalHitPoint);
		}
	}

	private void LockRotationX()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = ((Component)this).transform.rotation * Quaternion.Inverse(Container.rotation);
		Vector3 eulerAngles = ((Quaternion)(ref val)).eulerAngles;
		eulerAngles.x = 0f;
		((Component)this).transform.rotation = Container.rotation * Quaternion.Euler(eulerAngles);
	}

	private void LockRotationY()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = ((Component)this).transform.rotation * Quaternion.Inverse(Container.rotation);
		Vector3 eulerAngles = ((Quaternion)(ref val)).eulerAngles;
		eulerAngles.y = 0f;
		((Component)this).transform.rotation = Container.rotation * Quaternion.Euler(eulerAngles);
	}

	private void AlignToContainerPlane()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		Vector3 forward = Container.forward;
		Quaternion val = Quaternion.LookRotation(forward, ((Component)this).transform.up);
		Vector3 val2 = Vector3.ProjectOnPlane(((Component)this).transform.forward, forward);
		Vector3 normalized = ((Vector3)(ref val2)).normalized;
		_ = Quaternion.FromToRotation(((Component)this).transform.forward, normalized) * val;
		((Component)this).transform.rotation = val;
	}

	private void ClampUpRot()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)joint == (Object)null)
		{
			Console.LogWarning("No joint found on DraggableConstraint, cannot clamp up rotation");
			return;
		}
		Vector3.Angle(((Component)draggable).transform.up, Vector3.up);
		SoftJointLimit angularZLimit = joint.angularZLimit;
		((SoftJointLimit)(ref angularZLimit)).limit = UpDirectionMaxDifference;
		joint.angularZLimit = angularZLimit;
	}
}
