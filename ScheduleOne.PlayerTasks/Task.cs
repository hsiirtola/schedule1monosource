using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using ScheduleOne.UI.Compass;
using UnityEngine;

namespace ScheduleOne.PlayerTasks;

public class Task
{
	public enum EOutcome
	{
		Cancelled,
		Success,
		Fail
	}

	public const float ClickDetectionRange = 3f;

	public float ClickDetectionRadius;

	protected float MultiGrabRadius = 0.08f;

	public const float MultiGrabForceMultiplier = 1.25f;

	public bool ClickDetectionEnabled = true;

	public EOutcome Outcome;

	public Action onTaskSuccess;

	public Action onTaskFail;

	public Action onTaskStop;

	protected Clickable clickable;

	protected Draggable draggable;

	protected DraggableConstraint constraint;

	protected float hitDistance;

	protected Vector3 relativeHitOffset = Vector3.zero;

	private bool multiDraggingEnabled;

	private Transform multiGrabProjectionPlane;

	private List<Draggable> multiDragTargets = new List<Draggable>();

	private bool isMultiDragging;

	private List<Clickable> forcedClickables = new List<Clickable>();

	protected LayerMask clickablesLayerMask;

	public virtual string TaskName { get; protected set; }

	public string CurrentInstruction { get; protected set; } = string.Empty;

	public bool TaskActive { get; private set; }

	public Task()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		TaskActive = true;
		Singleton<TaskManager>.Instance.StartTask(this);
		Singleton<CompassManager>.Instance.SetVisible(visible: false);
		PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(TaskName);
		clickablesLayerMask = default(LayerMask);
		clickablesLayerMask = LayerMask.op_Implicit(LayerMask.op_Implicit(clickablesLayerMask) | (1 << LayerMask.NameToLayer("Task")));
		clickablesLayerMask = LayerMask.op_Implicit(LayerMask.op_Implicit(clickablesLayerMask) | (1 << LayerMask.NameToLayer("Temporary")));
	}

	public virtual void StopTask()
	{
		Singleton<TaskManager>.Instance.currentTask = null;
		PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(TaskName);
		Singleton<CompassManager>.Instance.SetVisible(visible: true);
		Singleton<CursorManager>.Instance.SetCursorAppearance(CursorManager.ECursorType.Default);
		TaskActive = false;
		if ((Object)(object)clickable != (Object)null)
		{
			clickable.EndClick();
		}
		if (onTaskStop != null)
		{
			onTaskStop();
		}
	}

	public virtual void Success()
	{
		Outcome = EOutcome.Success;
		StopTask();
		Singleton<TaskManager>.Instance.PlayTaskCompleteSound();
		if (onTaskSuccess != null)
		{
			onTaskSuccess();
		}
	}

	public virtual void Fail()
	{
		Outcome = EOutcome.Fail;
		StopTask();
		if (onTaskFail != null)
		{
			onTaskFail();
		}
	}

	public virtual void Update()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (ClickDetectionEnabled && !isMultiDragging)
		{
			if (GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick))
			{
				clickable = GetClickable(out var hit);
				if ((Object)(object)clickable != (Object)null)
				{
					clickable.StartClick(hit);
				}
				if (clickable is Draggable)
				{
					draggable = clickable as Draggable;
					constraint = ((Component)draggable).GetComponent<DraggableConstraint>();
				}
			}
			if ((Object)(object)clickable != (Object)null && (!GameInput.GetButton(GameInput.ButtonCode.PrimaryClick) || !clickable.ClickableEnabled) && !forcedClickables.Contains(clickable))
			{
				clickable.EndClick();
				clickable = null;
				draggable = null;
			}
		}
		else if ((Object)(object)clickable != (Object)null)
		{
			clickable.EndClick();
			clickable = null;
		}
		UpdateCursor();
	}

	protected virtual void UpdateCursor()
	{
		if ((Object)(object)draggable != (Object)null || isMultiDragging)
		{
			Singleton<CursorManager>.Instance.SetCursorAppearance(CursorManager.ECursorType.Grab);
			return;
		}
		RaycastHit hit;
		Clickable clickable = GetClickable(out hit);
		if ((Object)(object)clickable != (Object)null)
		{
			Singleton<CursorManager>.Instance.SetCursorAppearance(clickable.HoveredCursor);
		}
		else
		{
			Singleton<CursorManager>.Instance.SetCursorAppearance(CursorManager.ECursorType.Default);
		}
	}

	public virtual void LateUpdate()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		if (isMultiDragging)
		{
			((Transform)Singleton<TaskManagerUI>.Instance.multiGrabIndicator).position = Input.mousePosition;
			Vector3 multiDragOrigin = GetMultiDragOrigin();
			Vector3 val = PlayerSingleton<PlayerCamera>.Instance.Camera.WorldToScreenPoint(multiDragOrigin);
			Vector3 val2 = multiDragOrigin + ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.right * MultiGrabRadius;
			Vector3 val3 = PlayerSingleton<PlayerCamera>.Instance.Camera.WorldToScreenPoint(val2);
			float num = Vector3.Distance(val, val3) / Singleton<TaskManagerUI>.Instance.canvas.scaleFactor;
			Singleton<TaskManagerUI>.Instance.multiGrabIndicator.sizeDelta = new Vector2(num * 2f, num * 2f);
			((Component)Singleton<TaskManagerUI>.Instance.multiGrabIndicator).gameObject.SetActive(true);
		}
		else
		{
			((Component)Singleton<TaskManagerUI>.Instance.multiGrabIndicator).gameObject.SetActive(false);
		}
	}

	private Vector3 GetMultiDragOrigin()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		Ray val = PlayerSingleton<PlayerCamera>.Instance.Camera.ScreenPointToRay(Input.mousePosition);
		Plane val2 = default(Plane);
		((Plane)(ref val2))._002Ector(multiGrabProjectionPlane.forward, multiGrabProjectionPlane.position);
		float num = default(float);
		((Plane)(ref val2)).Raycast(val, ref num);
		LayerMask layerMask = LayerMask.op_Implicit(LayerMask.op_Implicit(default(LayerMask)) | (1 << LayerMask.NameToLayer("Default")));
		if (PlayerSingleton<PlayerCamera>.Instance.MouseRaycast(num, out var hit, layerMask, includeTriggers: false))
		{
			return ((RaycastHit)(ref hit)).point;
		}
		return ((Ray)(ref val)).GetPoint(num);
	}

	public virtual void FixedUpdate()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		UpdateDraggablePhysics();
		if (ClickDetectionEnabled && multiDraggingEnabled && (Object)(object)multiGrabProjectionPlane != (Object)null && GameInput.GetButton(GameInput.ButtonCode.SecondaryClick) && (Object)(object)this.draggable == (Object)null)
		{
			isMultiDragging = true;
			Vector3 multiDragOrigin = GetMultiDragOrigin();
			Collider[] array = Physics.OverlapSphere(multiDragOrigin, MultiGrabRadius, LayerMask.GetMask(new string[1] { "Task" }));
			List<Draggable> list = new List<Draggable>();
			Collider[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				Draggable componentInParent = ((Component)array2[i]).GetComponentInParent<Draggable>();
				if ((Object)(object)componentInParent != (Object)null && componentInParent.ClickableEnabled && componentInParent.CanBeMultiDragged)
				{
					list.Add(componentInParent);
				}
			}
			foreach (Draggable item in list)
			{
				if (!multiDragTargets.Contains(item))
				{
					multiDragTargets.Add(item);
					item.StartClick(default(RaycastHit));
					item.Rb.useGravity = false;
				}
				Vector3 val = (multiDragOrigin - ((Component)item).transform.position) * 10f * item.DragForceMultiplier * 1.25f;
				item.Rb.AddForce(val, (ForceMode)5);
			}
			Draggable[] array3 = multiDragTargets.ToArray();
			foreach (Draggable draggable in array3)
			{
				if (!list.Contains(draggable))
				{
					multiDragTargets.Remove(draggable);
					draggable.EndClick();
					if ((Object)(object)draggable != (Object)null)
					{
						draggable.Rb.useGravity = true;
					}
				}
			}
		}
		else
		{
			isMultiDragging = false;
			Draggable[] array3 = multiDragTargets.ToArray();
			foreach (Draggable draggable2 in array3)
			{
				multiDragTargets.Remove(draggable2);
				draggable2.EndClick();
				draggable2.Rb.useGravity = true;
			}
		}
	}

	public void ForceStartClick(Clickable _clickable)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (!forcedClickables.Contains(_clickable))
		{
			forcedClickables.Add(_clickable);
		}
		_clickable.StartClick(default(RaycastHit));
	}

	public void ForceEndClick(Clickable _clickable)
	{
		if ((Object)(object)_clickable != (Object)null)
		{
			_clickable.EndClick();
			forcedClickables.Remove(_clickable);
		}
	}

	private void UpdateDraggablePhysics()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)draggable != (Object)null))
		{
			return;
		}
		Vector3 val = Vector3.ProjectOnPlane(((Component)PlayerSingleton<PlayerCamera>.Instance.Camera).transform.forward, Vector3.up);
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		Vector3 val2 = draggable.originalHitPoint;
		Vector3 val3 = Vector3.zero;
		switch (draggable.DragProjectionMode)
		{
		case Draggable.EDragProjectionMode.FlatCameraForward:
			val3 = normalized;
			break;
		case Draggable.EDragProjectionMode.CameraForward:
			val3 = ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.forward;
			break;
		case Draggable.EDragProjectionMode.CustomPlane:
			val3 = draggable.CustomDragPlane.forward;
			val2 = draggable.CustomDragPlane.position;
			break;
		default:
			Debug.LogError((object)("Unknown drag projection mode: " + draggable.DragProjectionMode));
			break;
		}
		if ((Object)(object)constraint != (Object)null && constraint.ProportionalZClamp)
		{
			val3 = constraint.Container.forward;
		}
		Plane val4 = default(Plane);
		((Plane)(ref val4))._002Ector(val3, val2);
		Ray val5 = PlayerSingleton<PlayerCamera>.Instance.Camera.ScreenPointToRay(Input.mousePosition);
		float num = default(float);
		((Plane)(ref val4)).Raycast(val5, ref num);
		Vector3 val6 = (((Ray)(ref val5)).GetPoint(num) - ((Component)draggable).transform.TransformPoint(relativeHitOffset)) * 10f * draggable.DragForceMultiplier;
		if ((Object)(object)draggable.DragForceOrigin != (Object)null)
		{
			draggable.Rb.AddForceAtPosition(val6, draggable.DragForceOrigin.position, (ForceMode)5);
		}
		else
		{
			draggable.Rb.AddForce(val6, (ForceMode)5);
		}
		if (draggable.RotationEnabled)
		{
			float x = GameInput.MotionAxis.x;
			Vector3 val7 = Vector3.zero;
			switch (draggable.RotationAxis)
			{
			case Draggable.ERotationAxis.FlatCameraForward:
				val7 = normalized;
				break;
			case Draggable.ERotationAxis.LocalX:
				val7 = ((Component)draggable).transform.right;
				break;
			case Draggable.ERotationAxis.LocalY:
				val7 = ((Component)draggable).transform.up;
				break;
			case Draggable.ERotationAxis.LocalZ:
				val7 = ((Component)draggable).transform.forward;
				break;
			}
			draggable.Rb.AddTorque(val7 * (0f - x) * draggable.TorqueMultiplier, (ForceMode)5);
		}
		draggable.PostFixedUpdate();
	}

	protected virtual Clickable GetClickable(out RaycastHit hit)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		if (PlayerSingleton<PlayerCamera>.Instance.MouseRaycast(3f, out hit, clickablesLayerMask, includeTriggers: true, ClickDetectionRadius))
		{
			Clickable componentInParent = ((Component)((RaycastHit)(ref hit)).collider).GetComponentInParent<Clickable>();
			if ((Object)(object)componentInParent != (Object)null)
			{
				if (!((Behaviour)componentInParent).enabled)
				{
					return null;
				}
				if (!componentInParent.ClickableEnabled)
				{
					return null;
				}
				if (componentInParent.IsHeld)
				{
					return null;
				}
				hitDistance = Vector3.Distance(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position, ((RaycastHit)(ref hit)).point);
				componentInParent.SetOriginalHitPoint(((RaycastHit)(ref hit)).point);
				if (componentInParent.AutoCalculateOffset)
				{
					relativeHitOffset = ((Component)componentInParent).transform.InverseTransformPoint(((RaycastHit)(ref hit)).point);
					if (componentInParent.FlattenZOffset)
					{
						relativeHitOffset.z = 0f;
					}
				}
				else
				{
					relativeHitOffset = Vector3.zero;
				}
			}
			return componentInParent;
		}
		return null;
	}

	protected void EnableMultiDragging(Transform projectionPlane, float radius = 0.08f)
	{
		multiDraggingEnabled = true;
		multiGrabProjectionPlane = projectionPlane;
		MultiGrabRadius = radius;
	}

	protected void DisableMultiDragging()
	{
		multiDraggingEnabled = false;
		multiGrabProjectionPlane = null;
	}
}
