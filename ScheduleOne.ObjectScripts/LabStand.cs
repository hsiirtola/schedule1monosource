using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.ObjectScripts;

public class LabStand : MonoBehaviour
{
	[Header("Settings")]
	public float MoveSpeed = 1f;

	public bool FunnelEnabled;

	public float FunnelThreshold = 0.05f;

	[Header("References")]
	public Animation Anim;

	public Transform GripTransform;

	public Transform SpinnyThingy;

	public Transform RaisedTransform;

	public Transform LoweredTransform;

	public Transform PlaneNormal;

	public Clickable HandleClickable;

	public Transform Funnel;

	public GameObject Highlight;

	public AudioSourceController LowerSound;

	public AudioSourceController RaiseSound;

	private Vector3 clickOffset = Vector3.zero;

	private bool isMoving;

	public bool Interactable { get; private set; }

	public float CurrentPosition { get; private set; } = 1f;

	private void Start()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		SetPosition(1f);
		SetInteractable(e: false);
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
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		if (isMoving)
		{
			Vector3 val = GetPlaneHit() + clickOffset;
			float position = Mathf.Clamp01(Mathf.InverseLerp(Mathf.Min(LoweredTransform.position.y, RaisedTransform.position.y), Mathf.Max(LoweredTransform.position.y, RaisedTransform.position.y), val.y));
			SetPosition(position);
		}
		Highlight.gameObject.SetActive(Interactable && !isMoving);
		Move();
		((Component)Funnel).gameObject.SetActive(FunnelEnabled && CurrentPosition < FunnelThreshold);
	}

	private void Move()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		float y = GripTransform.localPosition.y;
		Vector3 val = Vector3.Lerp(LoweredTransform.localPosition, RaisedTransform.localPosition, CurrentPosition);
		Quaternion val2 = Quaternion.Lerp(LoweredTransform.localRotation, RaisedTransform.localRotation, CurrentPosition);
		GripTransform.localPosition = Vector3.Lerp(GripTransform.localPosition, val, Time.deltaTime * MoveSpeed);
		GripTransform.localRotation = Quaternion.Lerp(GripTransform.localRotation, val2, Time.deltaTime * MoveSpeed);
		float num = GripTransform.localPosition.y - y;
		SpinnyThingy.Rotate(Vector3.up, num * 1800f, (Space)1);
		UpdateSound(num);
	}

	private void UpdateSound(float difference)
	{
		difference /= 0.05f;
		float num = 0f;
		if (difference < 0f)
		{
			num = Mathf.Abs(difference);
		}
		float num2 = 0f;
		if (difference > 0f)
		{
			num2 = Mathf.Abs(difference);
		}
		LowerSound.VolumeMultiplier = num;
		RaiseSound.VolumeMultiplier = num2;
		if (num > 0f && !LowerSound.IsPlaying)
		{
			LowerSound.Play();
		}
		else if (num == 0f)
		{
			LowerSound.Stop();
		}
		if (num2 > 0f && !RaiseSound.IsPlaying)
		{
			RaiseSound.Play();
		}
		else if (num2 == 0f)
		{
			RaiseSound.Stop();
		}
	}

	public void SetPosition(float position)
	{
		CurrentPosition = position;
	}

	public void SetInteractable(bool e)
	{
		Interactable = e;
		HandleClickable.ClickableEnabled = e;
		if (Interactable)
		{
			Anim.Play();
		}
		else
		{
			Anim.Stop();
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
