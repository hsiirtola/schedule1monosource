using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.ObjectScripts;

public class LabOvenDoor : MonoBehaviour
{
	public const float HIT_OFFSET_MAX = 0.24f;

	public const float HIT_OFFSET_MIN = -0.25f;

	public const float DOOR_ANGLE_CLOSED = 90f;

	public const float DOOR_ANGLE_OPEN = 10f;

	[Header("References")]
	public Clickable HandleClickable;

	public Transform Door;

	public Transform PlaneNormal;

	public AnimationCurve HitMapCurve;

	[Header("Sounds")]
	public AudioSourceController OpenSound;

	public AudioSourceController CloseSound;

	public AudioSourceController ShutSound;

	[Header("Settings")]
	public float DoorMoveSpeed = 2f;

	private Vector3 clickOffset = Vector3.zero;

	private bool isMoving;

	public bool Interactable { get; private set; }

	public float TargetPosition { get; private set; }

	public float ActualPosition { get; private set; }

	private void Start()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		SetPosition(0f);
		SetInteractable(interactable: false);
		HandleClickable.onClickStart.AddListener((UnityAction<RaycastHit>)ClickStart);
		HandleClickable.onClickEnd.AddListener(new UnityAction(ClickEnd));
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
			y = Mathf.Clamp01(Mathf.InverseLerp(-0.25f, 0.24f, y));
			SetPosition(HitMapCurve.Evaluate(y));
		}
		Move();
	}

	private void Move()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Lerp(90f, 10f, TargetPosition);
		Quaternion val = Quaternion.Euler(0f, num, 0f);
		Door.localRotation = Quaternion.Lerp(Door.localRotation, val, Time.deltaTime * DoorMoveSpeed);
		ActualPosition = Mathf.Lerp(ActualPosition, TargetPosition, Time.deltaTime * DoorMoveSpeed);
	}

	public void SetInteractable(bool interactable)
	{
		Interactable = interactable;
		HandleClickable.ClickableEnabled = interactable;
	}

	public void SetPosition(float newPosition)
	{
		float targetPosition = TargetPosition;
		TargetPosition = newPosition;
		if (targetPosition == 0f && newPosition > 0.02f)
		{
			OpenSound.Play();
		}
		else if (targetPosition >= 0.98f && newPosition < 0.98f)
		{
			CloseSound.Play();
		}
		else if (targetPosition > 0.01f && newPosition <= 0.001f)
		{
			ShutSound.Play();
		}
	}

	public void ClickStart(RaycastHit hit)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		isMoving = true;
		clickOffset = ((Component)HandleClickable).transform.position - GetPlaneHit();
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
