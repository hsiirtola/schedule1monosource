using System;
using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using ScheduleOne.Interaction;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Dragging;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(InteractableObject))]
public class Draggable : MonoBehaviour, IGUIDRegisterable
{
	public enum EInitialReplicationMode
	{
		Off,
		OnlyIfMoved,
		Full
	}

	public const float INITIAL_REPLICATION_DISTANCE = 1f;

	public const float MAX_DRAG_START_RANGE = 2.5f;

	public const float MAX_TARGET_OFFSET = 1.5f;

	private bool isBeingDragged;

	private Player currentDragger;

	public string BakedGUID = string.Empty;

	[Header("References")]
	public Rigidbody Rigidbody;

	public InteractableObject IntObj;

	public Transform DragOrigin;

	[Header("Settings")]
	public bool CreateCoM = true;

	[Range(0.5f, 2f)]
	public float HoldDistanceMultiplier = 1f;

	[Range(0f, 5f)]
	public float DragForceMultiplier = 1f;

	public EInitialReplicationMode InitialReplicationMode;

	private float timeSinceLastDrag;

	public UnityEvent onDragStart;

	public UnityEvent onDragEnd;

	public UnityEvent onHovered;

	public UnityEvent onInteracted;

	public bool IsBeingDragged => isBeingDragged;

	public Player CurrentDragger
	{
		get
		{
			return currentDragger;
		}
		protected set
		{
			currentDragger = value;
			isBeingDragged = (Object)(object)currentDragger != (Object)null;
			if (!isBeingDragged)
			{
				timeSinceLastDrag = Time.timeSinceLevelLoad;
			}
		}
	}

	public Guid GUID { get; protected set; }

	public Vector3 initialPosition { get; private set; }

	[Button]
	public void RegenerateGUID()
	{
		BakedGUID = Guid.NewGuid().ToString();
	}

	protected virtual void Awake()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		IntObj.MaxInteractionRange = 2.5f;
		IntObj.onHovered.AddListener(new UnityAction(Hovered));
		IntObj.onInteractStart.AddListener(new UnityAction(Interacted));
		IntObj.SetMessage("Pick up");
		initialPosition = ((Component)this).transform.position;
		if (CreateCoM)
		{
			Transform transform = new GameObject("CenterOfMass").transform;
			transform.SetParent(((Component)this).transform);
			transform.localPosition = Rigidbody.centerOfMass;
			IntObj.displayLocationPoint = transform;
			DragOrigin = transform;
		}
		if (!string.IsNullOrEmpty(BakedGUID))
		{
			GUID = new Guid(BakedGUID);
			GUIDManager.RegisterObject(this);
		}
		if (((Component)this).gameObject.isStatic)
		{
			Console.LogWarning("Draggable object '" + ((Object)((Component)this).gameObject).name + "' is marked as static. This may cause issues with dragging.", (Object)(object)((Component)this).gameObject);
		}
	}

	protected virtual void Start()
	{
		NetworkSingleton<DragManager>.Instance.RegisterDraggable(this);
	}

	public void SetGUID(Guid guid)
	{
		GUID = guid;
		GUIDManager.RegisterObject(this, ((Component)this).gameObject);
	}

	protected void OnValidate()
	{
		if ((Object)(object)IntObj == (Object)null)
		{
			IntObj = ((Component)this).GetComponent<InteractableObject>();
		}
		if ((Object)(object)Rigidbody == (Object)null)
		{
			Rigidbody = ((Component)this).GetComponent<Rigidbody>();
		}
	}

	protected void OnDestroy()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkSingleton<DragManager>.InstanceExists)
		{
			if (IsBeingDragged && (Object)(object)NetworkSingleton<DragManager>.Instance.CurrentDraggable == (Object)(object)this)
			{
				NetworkSingleton<DragManager>.Instance.StopDragging(Vector3.zero);
			}
			NetworkSingleton<DragManager>.Instance.Deregister(this);
		}
	}

	public void UpdateDraggable()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (IsBeingDragged && (Object)(object)CurrentDragger != (Object)(object)Player.Local)
		{
			Vector3 targetPosition = CurrentDragger.MimicCamera.position + CurrentDragger.MimicCamera.forward * (1.25f * HoldDistanceMultiplier);
			ApplyDragForces(targetPosition);
		}
	}

	public void ApplyDragForces(Vector3 targetPosition)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)((Component)this).transform == (Object)null))
		{
			Vector3 val = targetPosition - ((Component)this).transform.position;
			if ((Object)(object)DragOrigin != (Object)null)
			{
				val = targetPosition - DragOrigin.position;
			}
			float magnitude = ((Vector3)(ref val)).magnitude;
			Vector3 val2 = ((Vector3)(ref val)).normalized * NetworkSingleton<DragManager>.Instance.DragForce * magnitude;
			val2 -= Rigidbody.velocity * NetworkSingleton<DragManager>.Instance.DampingFactor;
			Rigidbody.AddForce(val2 * DragForceMultiplier, (ForceMode)5);
			Vector3 val3 = Vector3.Cross(((Component)this).transform.up, Vector3.up);
			val3 -= Rigidbody.angularVelocity * NetworkSingleton<DragManager>.Instance.TorqueDampingFactor;
			Rigidbody.AddTorque(val3 * NetworkSingleton<DragManager>.Instance.TorqueForce, (ForceMode)5);
		}
	}

	protected virtual void Hovered()
	{
		if (CanInteract() && ((Behaviour)this).enabled)
		{
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
			IntObj.SetMessage("Pick up");
		}
		else
		{
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
		}
		if (onHovered != null)
		{
			onHovered.Invoke();
		}
	}

	protected virtual void Interacted()
	{
		if (((Behaviour)this).enabled)
		{
			if (onInteracted != null)
			{
				onInteracted.Invoke();
			}
			if (CanInteract())
			{
				NetworkSingleton<DragManager>.Instance.StartDragging(this);
			}
		}
	}

	private bool CanInteract()
	{
		if (IsBeingDragged)
		{
			return false;
		}
		if (Time.timeSinceLevelLoad - timeSinceLastDrag < 0.1f)
		{
			return false;
		}
		if (NetworkSingleton<DragManager>.Instance.IsDragging)
		{
			return false;
		}
		if (!NetworkSingleton<DragManager>.Instance.IsDraggingAllowed())
		{
			return false;
		}
		return true;
	}

	public void StartDragging(Player dragger)
	{
		if (!IsBeingDragged)
		{
			CurrentDragger = dragger;
			Rigidbody.useGravity = false;
			if (onDragStart != null)
			{
				onDragStart.Invoke();
			}
		}
	}

	public void StopDragging()
	{
		if (IsBeingDragged)
		{
			CurrentDragger = null;
			Rigidbody.useGravity = true;
			if (onDragEnd != null)
			{
				onDragEnd.Invoke();
			}
		}
	}
}
