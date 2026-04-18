using ScheduleOne.PlayerTasks;
using ScheduleOne.Tools;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.ObjectScripts;

public class LabOvenHammer : MonoBehaviour
{
	public Draggable Draggable;

	public DraggableConstraint Constraint;

	public RotateRigidbodyToTarget Rotator;

	public Transform CoM;

	public Transform ImpactPoint;

	public SmoothedVelocityCalculator VelocityCalculator;

	[Header("Settings")]
	public float MinHeight;

	public float MaxHeight = 0.3f;

	public float MinAngle = 100f;

	public float MaxAngle = 40f;

	public UnityEvent<Collision> onCollision;

	private void Start()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		Draggable.Rb.centerOfMass = CoM.localPosition;
	}

	private void Update()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		((Behaviour)Rotator).enabled = Draggable.IsHeld;
		if (Draggable.IsHeld)
		{
			Rotator.TargetRotation.z = Mathf.Lerp(MinAngle, MaxAngle, Mathf.Clamp01(Mathf.InverseLerp(MinHeight, MaxHeight, ((Component)this).transform.localPosition.y)));
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (onCollision != null)
		{
			onCollision.Invoke(collision);
		}
	}
}
