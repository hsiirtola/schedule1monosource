using UnityEngine;

namespace ScheduleOne.Doors;

public class RollerDoor : MonoBehaviour
{
	[Header("Settings")]
	public Transform Door;

	public Vector3 LocalPos_Open;

	public Vector3 LocalPos_Closed;

	public float LerpTime = 1f;

	public GameObject Blocker;

	private Vector3 startPos = Vector3.zero;

	private float timeSinceValueChange;

	public bool IsOpen { get; protected set; } = true;

	private void Awake()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Door.localPosition = (IsOpen ? LocalPos_Open : LocalPos_Closed);
	}

	private void LateUpdate()
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		timeSinceValueChange += Time.deltaTime;
		if (timeSinceValueChange < LerpTime)
		{
			Vector3 val = (IsOpen ? LocalPos_Open : LocalPos_Closed);
			Door.localPosition = Vector3.Lerp(startPos, val, timeSinceValueChange / LerpTime);
		}
		else
		{
			Door.localPosition = (IsOpen ? LocalPos_Open : LocalPos_Closed);
		}
		if ((Object)(object)Blocker != (Object)null)
		{
			Blocker.gameObject.SetActive(!IsOpen);
		}
	}

	public void Open()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (!IsOpen && CanOpen())
		{
			IsOpen = true;
			timeSinceValueChange = 0f;
			startPos = Door.localPosition;
		}
	}

	public void Close()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (IsOpen)
		{
			IsOpen = false;
			timeSinceValueChange = 0f;
			startPos = Door.localPosition;
		}
	}

	protected virtual bool CanOpen()
	{
		return true;
	}
}
