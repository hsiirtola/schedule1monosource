using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.PlayerTasks;

public class Clickable : MonoBehaviour
{
	public bool ClickableEnabled = true;

	public bool AutoCalculateOffset = true;

	public bool FlattenZOffset;

	public UnityEvent<RaycastHit> onClickStart;

	public UnityEvent onClickEnd;

	public virtual CursorManager.ECursorType HoveredCursor { get; protected set; } = CursorManager.ECursorType.Finger;

	public Vector3 originalHitPoint { get; protected set; } = Vector3.zero;

	public bool IsHeld { get; protected set; }

	private void Awake()
	{
		LayerUtility.SetLayerRecursively(((Component)this).gameObject, LayerMask.NameToLayer("Task"));
	}

	public virtual void StartClick(RaycastHit hit)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (onClickStart != null)
		{
			onClickStart.Invoke(hit);
		}
		IsHeld = true;
	}

	public virtual void EndClick()
	{
		if (onClickEnd != null)
		{
			onClickEnd.Invoke();
		}
		IsHeld = false;
	}

	public void SetOriginalHitPoint(Vector3 hitPoint)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		originalHitPoint = hitPoint;
	}
}
