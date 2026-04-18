using System.Collections;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.DevUtilities;

public class BirdsEyeView : Singleton<BirdsEyeView>
{
	[Header("Settings")]
	public Vector3 bounds_Min;

	public Vector3 bounds_Max;

	[Header("Camera settings")]
	public float lateralMovementSpeed = 1f;

	public float scrollMovementSpeed = 1f;

	public float targetFollowSpeed = 1f;

	[Header("Camera orbit settings")]
	public float xSpeed = 250f;

	public float ySpeed = 120f;

	public float yMinLimit = -20f;

	public float yMaxLimit = 80f;

	private Vector3 rotationOriginPoint = Vector3.zero;

	private float distance = 10f;

	private float prevDistance;

	private float x;

	private float y;

	private Transform targetTransform;

	private Coroutine originSlideRoutine;

	private Transform playerCam => ((Component)PlayerSingleton<PlayerCamera>.Instance).transform;

	public bool isEnabled { get; protected set; }

	protected override void Awake()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		targetTransform = new GameObject("_TargetCameraTransform").transform;
		targetTransform.SetParent(GameObject.Find("_Temp").transform);
	}

	protected virtual void Update()
	{
		if (isEnabled)
		{
			UpdateLateralMovement();
			UpdateRotation();
			UpdateScrollMovement();
		}
	}

	protected virtual void LateUpdate()
	{
		if (isEnabled)
		{
			FinalizeCameraMovement();
		}
	}

	public void Enable(Vector3 startPosition, Quaternion startRotation)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		isEnabled = true;
		PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(60f, 0f);
		PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(startPosition, startRotation, 0f);
		Vector3 eulerAngles = ((Quaternion)(ref startRotation)).eulerAngles;
		x = eulerAngles.y;
		y = eulerAngles.x;
		targetTransform.position = startPosition;
		targetTransform.rotation = startRotation;
	}

	public void Disable(bool reenableCameraLook = true)
	{
		isEnabled = false;
		PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0f);
		PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0f, reenableCameraLook);
	}

	protected void UpdateLateralMovement()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		float num = GameInput.MotionAxis.y;
		float num2 = GameInput.MotionAxis.x;
		int num3 = 0;
		if (Input.GetKey((KeyCode)32))
		{
			num3++;
		}
		if (Input.GetKey((KeyCode)306))
		{
			num3--;
		}
		if (num != 0f || num3 != 0)
		{
			CancelOriginSlide();
		}
		Vector3 forward = playerCam.forward;
		forward.y = 0f;
		((Vector3)(ref forward)).Normalize();
		Vector3 right = playerCam.right;
		right.y = 0f;
		((Vector3)(ref right)).Normalize();
		Vector3 val = forward * num * lateralMovementSpeed * Time.deltaTime;
		Vector3 val2 = right * num2 * lateralMovementSpeed * Time.deltaTime;
		Vector3 val3 = Vector3.up * (float)num3 * lateralMovementSpeed * Time.deltaTime * 0.5f;
		Transform obj = targetTransform;
		obj.position += val;
		Transform obj2 = targetTransform;
		obj2.position += val2;
		Transform obj3 = targetTransform;
		obj3.position += val3;
		rotationOriginPoint += val;
		rotationOriginPoint += val2;
		rotationOriginPoint += val3;
	}

	protected void UpdateScrollMovement()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		float num = Input.mouseScrollDelta.y;
		Vector3 forward = playerCam.forward;
		Vector3 normalized = ((Vector3)(ref forward)).normalized;
		if (GameInput.GetButton(GameInput.ButtonCode.TertiaryClick) || GameInput.GetButton(GameInput.ButtonCode.SecondaryClick))
		{
			distance += num * scrollMovementSpeed * Time.deltaTime;
			return;
		}
		Transform obj = targetTransform;
		obj.position += normalized * num * scrollMovementSpeed * Time.deltaTime;
	}

	protected void UpdateRotation()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		if (GameInput.GetButtonDown(GameInput.ButtonCode.TertiaryClick) || GameInput.GetButtonDown(GameInput.ButtonCode.SecondaryClick))
		{
			Plane val = default(Plane);
			((Plane)(ref val))._002Ector(Vector3.up, new Vector3(0f, 0f, 0f));
			Ray val2 = default(Ray);
			((Ray)(ref val2))._002Ector(targetTransform.position, targetTransform.forward);
			float num = 0f;
			((Plane)(ref val)).Raycast(val2, ref num);
			distance = num;
			rotationOriginPoint = ((Ray)(ref val2)).GetPoint(num);
		}
		if (GameInput.GetButton(GameInput.ButtonCode.TertiaryClick) || GameInput.GetButton(GameInput.ButtonCode.SecondaryClick))
		{
			x += GameInput.MouseDelta.x * xSpeed * 0.02f;
			y -= GameInput.MouseDelta.y * ySpeed * 0.02f;
			y = ClampAngle(y, yMinLimit, yMaxLimit);
			Quaternion val3 = Quaternion.Euler(y, x, 0f);
			Vector3 position = val3 * new Vector3(0f, 0f, 0f - distance) + rotationOriginPoint;
			targetTransform.rotation = val3;
			targetTransform.position = position;
		}
	}

	private void FinalizeCameraMovement()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		playerCam.position = Vector3.Lerp(playerCam.position, targetTransform.position, Time.deltaTime * targetFollowSpeed);
		playerCam.rotation = Quaternion.Lerp(playerCam.rotation, targetTransform.rotation, Time.deltaTime * targetFollowSpeed);
	}

	private static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360f)
		{
			angle += 360f;
		}
		if (angle > 360f)
		{
			angle -= 360f;
		}
		return Mathf.Clamp(angle, min, max);
	}

	private void CancelOriginSlide()
	{
		if (originSlideRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(originSlideRoutine);
			originSlideRoutine = null;
		}
	}

	public void SlideCameraOrigin(Vector3 position, float offsetDistance, float time = 0f)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		if (originSlideRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(originSlideRoutine);
		}
		Plane val = default(Plane);
		((Plane)(ref val))._002Ector(Vector3.up, new Vector3(0f, 0f, 0f));
		Ray val2 = default(Ray);
		((Ray)(ref val2))._002Ector(targetTransform.position, targetTransform.forward);
		float num = 0f;
		((Plane)(ref val)).Raycast(val2, ref num);
		Vector3 point = ((Ray)(ref val2)).GetPoint(num);
		Vector3 val3 = targetTransform.position - point;
		position += ((Vector3)(ref val3)).normalized * offsetDistance;
		originSlideRoutine = ((MonoBehaviour)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			Vector3 startPosition = ((Component)targetTransform).transform.position;
			for (float i = 0f; i < time; i += Time.deltaTime)
			{
				targetTransform.position = Vector3.Lerp(startPosition, position, i / time);
				yield return (object)new WaitForEndOfFrame();
			}
			targetTransform.position = position;
			originSlideRoutine = null;
		}
	}
}
