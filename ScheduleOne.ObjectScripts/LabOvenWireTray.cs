using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.ObjectScripts;

public class LabOvenWireTray : MonoBehaviour
{
	public const float HIT_OFFSET_MAX = 0.24f;

	public const float HIT_OFFSET_MIN = -0.25f;

	[Header("References")]
	public Transform Tray;

	public Transform PlaneNormal;

	public Transform ClosedPosition;

	public Transform OpenPosition;

	public LabOvenDoor OvenDoor;

	[Header("Settings")]
	public float MoveSpeed = 2f;

	public AnimationCurve DoorClampCurve;

	private Vector3 clickOffset = Vector3.zero;

	private bool isMoving;

	public bool Interactable { get; private set; }

	public float TargetPosition { get; private set; }

	public float ActualPosition { get; private set; }

	private void Start()
	{
		SetPosition(0f);
		SetInteractable(interactable: false);
	}

	private void LateUpdate()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (isMoving)
		{
			Vector3 val = GetPlaneHit() + clickOffset;
			float y = PlaneNormal.InverseTransformPoint(val).y;
			Debug.Log((object)("Hit offset: " + y));
			y = Mathf.Clamp01(Mathf.InverseLerp(-0.25f, 0.24f, y));
			TargetPosition = y;
		}
		Move();
		ClampAngle();
	}

	private void Move()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.Lerp(ClosedPosition.localPosition, OpenPosition.localPosition, TargetPosition);
		Tray.localPosition = Vector3.Lerp(Tray.localPosition, val, Time.deltaTime * MoveSpeed);
		ActualPosition = Mathf.Lerp(ActualPosition, TargetPosition, Time.deltaTime * MoveSpeed);
	}

	private void ClampAngle()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		float num = DoorClampCurve.Evaluate(OvenDoor.ActualPosition);
		ActualPosition = Mathf.Clamp(ActualPosition, 0f, num);
		Vector3 localPosition = Vector3.Lerp(ClosedPosition.localPosition, OpenPosition.localPosition, ActualPosition);
		Tray.localPosition = localPosition;
	}

	public void SetInteractable(bool interactable)
	{
		Interactable = interactable;
	}

	public void SetPosition(float position)
	{
		TargetPosition = position;
	}

	public void ClickStart(RaycastHit hit)
	{
		isMoving = true;
	}

	private Vector3 GetPlaneHit()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		Plane val = default(Plane);
		((Plane)(ref val))._002Ector(PlaneNormal.forward, PlaneNormal.position);
		Ray val2 = PlayerSingleton<PlayerCamera>.Instance.Camera.ScreenPointToRay(GameInput.MousePosition);
		float num = default(float);
		((Plane)(ref val)).Raycast(val2, ref num);
		return ((Ray)(ref val2)).GetPoint(num);
	}

	public void ClickEnd()
	{
		isMoving = false;
	}
}
