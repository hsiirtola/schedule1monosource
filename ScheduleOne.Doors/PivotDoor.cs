using System.Collections;
using UnityEngine;

namespace ScheduleOne.Doors;

public class PivotDoor : MonoBehaviour
{
	[Header("Settings")]
	public Transform DoorTransform;

	public bool FlipSide;

	public float OpenInwardsAngle = -100f;

	public float OpenOutwardsAngle = 100f;

	public float SwingSpeed = 5f;

	private bool isUpdatingDoor;

	private float targetDoorAngle;

	protected virtual void Awake()
	{
	}

	public virtual void Opened(EDoorSide openSide)
	{
		switch (openSide)
		{
		case EDoorSide.Interior:
			targetDoorAngle = (FlipSide ? OpenInwardsAngle : OpenOutwardsAngle);
			UpdateDoor();
			break;
		case EDoorSide.Exterior:
			targetDoorAngle = (FlipSide ? OpenOutwardsAngle : OpenInwardsAngle);
			UpdateDoor();
			break;
		}
	}

	public virtual void Closed()
	{
		targetDoorAngle = 0f;
		UpdateDoor();
	}

	private void UpdateDoor()
	{
		if (!isUpdatingDoor)
		{
			isUpdatingDoor = true;
			((MonoBehaviour)this).StartCoroutine(inner());
		}
		IEnumerator inner()
		{
			while (Quaternion.Angle(DoorTransform.localRotation, Quaternion.Euler(0f, targetDoorAngle, 0f)) > 0.01f)
			{
				DoorTransform.localRotation = Quaternion.Lerp(DoorTransform.localRotation, Quaternion.Euler(0f, targetDoorAngle, 0f), Time.deltaTime * SwingSpeed);
				yield return null;
			}
			DoorTransform.localRotation = Quaternion.Euler(0f, targetDoorAngle, 0f);
			isUpdatingDoor = false;
		}
	}
}
