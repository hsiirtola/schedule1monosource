using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.ObjectScripts;

public class StirringRod : MonoBehaviour
{
	public const float MAX_STIR_RATE = 20f;

	public const float MAX_PIVOT_ANGLE = 7f;

	public float LerpSpeed = 10f;

	[Header("References")]
	public Clickable Clickable;

	public Transform PlaneNormal;

	public Transform Container;

	public Transform RodPivot;

	public AudioSourceController StirSound;

	private Vector3 clickOffset = Vector3.zero;

	private bool isMoving;

	public bool Interactable { get; private set; }

	public float CurrentStirringSpeed { get; private set; }

	private void Start()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		SetInteractable(e: true);
		Clickable.onClickStart.AddListener((UnityAction<RaycastHit>)ClickStart);
		Clickable.onClickEnd.AddListener(new UnityAction(ClickEnd));
	}

	private void Update()
	{
		float volumeMultiplier = Mathf.MoveTowards(StirSound.VolumeMultiplier, CurrentStirringSpeed, Time.deltaTime * 4f);
		StirSound.VolumeMultiplier = volumeMultiplier;
		if (StirSound.VolumeMultiplier > 0f && !StirSound.IsPlaying)
		{
			StirSound.Play();
		}
		else if (StirSound.VolumeMultiplier == 0f)
		{
			StirSound.Stop();
		}
	}

	private void LateUpdate()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		if (isMoving)
		{
			Vector3 forward = Container.forward;
			Vector3 planeHit = GetPlaneHit();
			float num = Vector3.SignedAngle(PlaneNormal.forward, planeHit - PlaneNormal.position, PlaneNormal.up);
			Quaternion val = PlaneNormal.rotation * Quaternion.Euler(Vector3.up * num);
			Container.rotation = Quaternion.Lerp(Container.rotation, val, Time.deltaTime * LerpSpeed);
			float num2 = Vector3.SignedAngle(forward, Container.forward, PlaneNormal.up);
			CurrentStirringSpeed = Mathf.Clamp01(Mathf.Abs(num2) / 20f);
			RodPivot.localEulerAngles = new Vector3(7f * (1f - CurrentStirringSpeed), 0f, 0f);
		}
		else
		{
			CurrentStirringSpeed = 0f;
		}
	}

	public void SetInteractable(bool e)
	{
		Interactable = e;
		Clickable.ClickableEnabled = e;
	}

	public void ClickStart(RaycastHit hit)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		isMoving = true;
		clickOffset = ((Component)Clickable).transform.position - GetPlaneHit();
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
		((Plane)(ref val))._002Ector(PlaneNormal.up, PlaneNormal.position);
		Ray val2 = PlayerSingleton<PlayerCamera>.Instance.Camera.ScreenPointToRay(GameInput.MousePosition);
		float num = default(float);
		((Plane)(ref val)).Raycast(val2, ref num);
		return ((Ray)(ref val2)).GetPoint(num);
	}

	public void ClickEnd()
	{
		isMoving = false;
	}

	public void Destroy()
	{
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}
}
