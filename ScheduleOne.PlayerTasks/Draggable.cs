using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.PlayerTasks;

public class Draggable : Clickable
{
	public enum EDragProjectionMode
	{
		CameraForward,
		FlatCameraForward,
		CustomPlane
	}

	public enum ERotationAxis
	{
		FlatCameraForward,
		LocalX,
		LocalY,
		LocalZ
	}

	[Header("Drag Force")]
	public float DragForceMultiplier = 30f;

	public Transform DragForceOrigin;

	[Header("Rotation")]
	public bool RotationEnabled = true;

	public float TorqueMultiplier = 20f;

	public ERotationAxis RotationAxis;

	[Header("Settings")]
	public EDragProjectionMode DragProjectionMode;

	public Transform CustomDragPlane;

	public bool DisableGravityWhenDragged;

	public float NormalRBDrag = 3f;

	public float HeldRBDrag = 15f;

	public bool CanBeMultiDragged = true;

	[Header("Additional force")]
	public float idleUpForce;

	[HideInInspector]
	public bool LocationRestrictionEnabled;

	[HideInInspector]
	public Vector3 Origin = Vector3.zero;

	[HideInInspector]
	public float MaxDistanceFromOrigin = 0.5f;

	public UnityEvent<Collider> onTriggerExit;

	protected DraggableConstraint constraint;

	public Rigidbody Rb { get; protected set; }

	public override CursorManager.ECursorType HoveredCursor { get; protected set; } = CursorManager.ECursorType.OpenHand;

	protected virtual void Awake()
	{
		Rb = ((Component)this).GetComponent<Rigidbody>();
		constraint = ((Component)this).GetComponent<DraggableConstraint>();
		if (((Component)this).gameObject.isStatic)
		{
			Console.LogWarning("Draggable object is static, this will cause issues with dragging.");
		}
	}

	protected virtual void FixedUpdate()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)Rb == (Object)null))
		{
			Rb.drag = (base.IsHeld ? HeldRBDrag : NormalRBDrag);
			if (!base.IsHeld && !Rb.isKinematic)
			{
				Rigidbody rb = Rb;
				Vector3 angularVelocity = Rb.angularVelocity;
				Vector3 val = Rb.angularVelocity;
				rb.angularVelocity = Vector3.ClampMagnitude(angularVelocity, ((Vector3)(ref val)).magnitude * 0.9f);
				Rigidbody rb2 = Rb;
				Vector3 velocity = Rb.velocity;
				val = Rb.velocity;
				rb2.velocity = Vector3.ClampMagnitude(velocity, ((Vector3)(ref val)).magnitude * 0.95f);
				Rb.AddForce(Vector3.up * idleUpForce, (ForceMode)5);
			}
		}
	}

	protected virtual void Update()
	{
	}

	public virtual void PostFixedUpdate()
	{
	}

	protected virtual void LateUpdate()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if (LocationRestrictionEnabled && Vector3.Distance(((Component)this).transform.position, Origin) > MaxDistanceFromOrigin)
		{
			Transform transform = ((Component)this).transform;
			Vector3 origin = Origin;
			Vector3 val = ((Component)this).transform.position - Origin;
			transform.position = origin + ((Vector3)(ref val)).normalized * MaxDistanceFromOrigin;
		}
	}

	protected virtual void OnTriggerExit(Collider other)
	{
		if (onTriggerExit != null)
		{
			onTriggerExit.Invoke(other);
		}
	}

	public override void StartClick(RaycastHit hit)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		base.StartClick(hit);
		if (DisableGravityWhenDragged)
		{
			Rb.useGravity = false;
		}
	}

	public override void EndClick()
	{
		base.EndClick();
		if (DisableGravityWhenDragged && (Object)(object)Rb != (Object)null)
		{
			Rb.useGravity = true;
		}
	}
}
