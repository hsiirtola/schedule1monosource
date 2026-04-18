using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.ObjectScripts;

public class BrickPressHandle : MonoBehaviour
{
	private float lastClickPosition;

	[Header("Settings")]
	public float MoveSpeed = 1f;

	public bool Locked;

	[Header("References")]
	public Transform PlaneNormal;

	public Transform RaisedTransform;

	public Transform LoweredTransform;

	public Clickable HandleClickable;

	public AudioSourceController ClickSound;

	private Vector3 clickOffset = Vector3.zero;

	private bool isMoving;

	public bool Interactable { get; private set; }

	public float CurrentPosition { get; private set; }

	public float TargetPosition { get; private set; }

	private void Start()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		SetPosition(0f);
		SetInteractable(e: false);
		HandleClickable.onClickStart.AddListener((UnityAction<RaycastHit>)ClickStart);
		HandleClickable.onClickEnd.AddListener(new UnityAction(ClickEnd));
	}

	private void LateUpdate()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if (!Locked)
		{
			if (isMoving)
			{
				Vector3 val = GetPlaneHit() + clickOffset;
				float position = 1f - Mathf.Clamp01(Mathf.InverseLerp(Mathf.Min(LoweredTransform.position.y, RaisedTransform.position.y), Mathf.Max(LoweredTransform.position.y, RaisedTransform.position.y), val.y));
				SetPosition(position);
			}
			else
			{
				SetPosition(Mathf.MoveTowards(TargetPosition, 0f, Time.deltaTime));
			}
		}
		Move();
	}

	private void Move()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		CurrentPosition = Mathf.MoveTowards(CurrentPosition, TargetPosition, MoveSpeed * Time.deltaTime);
		((Component)this).transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, 360f, CurrentPosition));
		if (Mathf.Abs(CurrentPosition - lastClickPosition) > 0.1666f)
		{
			lastClickPosition = CurrentPosition;
			ClickSound.PitchMultiplier = Mathf.Lerp(0.7f, 1.1f, CurrentPosition);
			ClickSound.Play();
		}
	}

	private void UpdateSound(float difference)
	{
		difference /= 0.05f;
		if (difference < 0f)
		{
			Mathf.Abs(difference);
		}
		if (difference > 0f)
		{
			Mathf.Abs(difference);
		}
	}

	public void SetPosition(float position)
	{
		TargetPosition = position;
	}

	public void SetInteractable(bool e)
	{
		Interactable = e;
		HandleClickable.ClickableEnabled = e;
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

	public void ClickEnd()
	{
		isMoving = false;
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
}
