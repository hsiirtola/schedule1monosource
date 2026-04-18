using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Vehicles;

public class VehicleCamera : MonoBehaviour
{
	private const float followDelta = 10f;

	private const float yMinLimit = -20f;

	private const float manualOverrideTime = 0.01f;

	private const float manualOverrideReturnTime = 0.6f;

	private const float xSpeed = 60f;

	private const float ySpeed = 40f;

	private const float yMaxLimit = 89f;

	[Header("References")]
	public LandVehicle vehicle;

	[Header("Camera Settings")]
	[SerializeField]
	protected Transform cameraOrigin;

	[SerializeField]
	protected float lateralOffset = 4f;

	[SerializeField]
	protected float verticalOffset = 1.5f;

	protected bool cameraReversed;

	protected float timeSinceCameraManuallyAdjusted = float.MaxValue;

	protected float orbitDistance;

	protected Vector3 lastFrameCameraOffset = Vector3.zero;

	protected Vector3 lastManualOffset = Vector3.zero;

	private Transform targetTransform;

	private Transform cameraDolly;

	private float x;

	private float y;

	private float mouseIdleCooldown = 1.5f;

	private float mouseIdleTimer;

	private Transform cam => ((Component)PlayerSingleton<PlayerCamera>.Instance).transform;

	private bool NeedSecondaryClick => GameInput.CurrentInputDevice == GameInput.InputDeviceType.KeyboardMouse;

	protected virtual void Start()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		targetTransform = new GameObject("VehicleCameraTargetTransform").transform;
		targetTransform.SetParent(NetworkSingleton<GameManager>.Instance.Temp);
		cameraDolly = new GameObject("VehicleCameraDolly").transform;
		cameraDolly.SetParent(NetworkSingleton<GameManager>.Instance.Temp);
		if ((Object)(object)Player.Local != (Object)null)
		{
			Subscribe();
		}
		else
		{
			Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(Subscribe));
		}
	}

	private void Subscribe()
	{
		Player local = Player.Local;
		local.onEnterVehicle = (Player.VehicleEvent)Delegate.Combine(local.onEnterVehicle, new Player.VehicleEvent(PlayerEnteredVehicle));
	}

	protected virtual void Update()
	{
		if (vehicle.LocalPlayerIsInVehicle)
		{
			if (NeedSecondaryClick)
			{
				CheckForClick();
			}
			else if (GameInput.GetButtonDown(GameInput.ButtonCode.VehicleResetCamera))
			{
				ForceCameraReturn();
			}
			else
			{
				CheckForMouseMovement();
			}
		}
	}

	private void PlayerEnteredVehicle(LandVehicle veh)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)veh != (Object)(object)vehicle))
		{
			timeSinceCameraManuallyAdjusted = 100f;
			mouseIdleTimer = mouseIdleCooldown + 0.6f;
			targetTransform.position = LimitCameraPosition(GetTargetCameraPosition());
			targetTransform.LookAt(cameraOrigin);
			cam.position = targetTransform.position;
			cam.rotation = targetTransform.rotation;
			cameraDolly.position = targetTransform.position;
			cameraDolly.rotation = targetTransform.rotation;
			lastManualOffset = cameraOrigin.InverseTransformPoint(cam.position);
			lastFrameCameraOffset = cameraOrigin.InverseTransformPoint(cam.position);
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(targetTransform.position, targetTransform.rotation, 0f);
		}
	}

	private void CheckForClick()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		timeSinceCameraManuallyAdjusted += Time.deltaTime;
		if (GameInput.GetButton(GameInput.ButtonCode.SecondaryClick))
		{
			if (GameInput.GetButtonDown(GameInput.ButtonCode.SecondaryClick) && timeSinceCameraManuallyAdjusted > 0.01f)
			{
				Quaternion rotation = cam.rotation;
				Vector3 eulerAngles = ((Quaternion)(ref rotation)).eulerAngles;
				x = eulerAngles.y;
				y = eulerAngles.x;
				orbitDistance = Mathf.Sqrt(Mathf.Pow(lateralOffset, 2f) + Mathf.Pow(verticalOffset, 2f));
			}
			timeSinceCameraManuallyAdjusted = 0f;
		}
	}

	private void CheckForMouseMovement()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		Vector2 mouseDelta = GameInput.MouseDelta;
		if (((Vector2)(ref mouseDelta)).sqrMagnitude > 0f)
		{
			mouseIdleTimer = 0f;
			Quaternion rotation = cam.rotation;
			Vector3 eulerAngles = ((Quaternion)(ref rotation)).eulerAngles;
			x = eulerAngles.y;
			y = eulerAngles.x;
			if (y > 180f)
			{
				y -= 360f;
			}
			orbitDistance = Mathf.Sqrt(Mathf.Pow(lateralOffset, 2f) + Mathf.Pow(verticalOffset, 2f));
		}
		else
		{
			mouseIdleTimer += Time.deltaTime;
		}
	}

	protected virtual void LateUpdate()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		if (vehicle.LocalPlayerIsInVehicle)
		{
			if (vehicle.Speed_Kmh > 2f)
			{
				cameraReversed = false;
			}
			else if (vehicle.Speed_Kmh < -15f)
			{
				cameraReversed = true;
			}
			targetTransform.position = LimitCameraPosition(GetTargetCameraPosition());
			targetTransform.LookAt(cameraOrigin);
			cameraDolly.position = Vector3.Lerp(cameraDolly.position, targetTransform.position, Time.deltaTime * 10f);
			cameraDolly.rotation = Quaternion.Lerp(cameraDolly.rotation, targetTransform.rotation, Time.deltaTime * 10f);
			orbitDistance = Mathf.Clamp(Vector3.Distance(cameraOrigin.position, cameraDolly.position), Mathf.Sqrt(Mathf.Pow(lateralOffset, 2f) + Mathf.Pow(verticalOffset, 2f)), 100f);
			if (NeedSecondaryClick)
			{
				HandleSecondaryClickCameraMovement();
			}
			else
			{
				HandleNonSecondaryClickCameraMovement();
			}
		}
	}

	private void HandleNonSecondaryClickCameraMovement()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		if (mouseIdleTimer == 0f)
		{
			x += GameInput.MouseDelta.x * 60f * 0.02f * Singleton<Settings>.Instance.LookSensitivity;
			y -= GameInput.MouseDelta.y * 40f * 0.02f * Singleton<Settings>.Instance.LookSensitivity;
			y = ClampAngle(y, -20f, 89f);
			Quaternion val = Quaternion.Euler(y, x, 0f);
			Vector3 targetPosition = val * new Vector3(0f, 0f, 0f - orbitDistance) + cameraOrigin.position;
			cam.rotation = val;
			cam.position = LimitCameraPosition(targetPosition);
			lastManualOffset = cameraOrigin.InverseTransformPoint(cam.position);
		}
		else
		{
			bool num = mouseIdleTimer > mouseIdleCooldown && mouseIdleTimer < mouseIdleCooldown + 0.6f;
			float num2 = (mouseIdleTimer - mouseIdleCooldown) / 0.6f;
			if (num)
			{
				targetTransform.position = Vector3.Lerp(cameraOrigin.TransformPoint(lastManualOffset), targetTransform.position, num2);
				targetTransform.LookAt(cameraOrigin);
				cam.position = Vector3.Lerp(cam.position, targetTransform.position, Time.deltaTime * 10f);
				cam.rotation = Quaternion.Lerp(cam.rotation, targetTransform.rotation, Time.deltaTime * 10f);
			}
			else if (mouseIdleTimer <= mouseIdleCooldown)
			{
				Vector3 val2 = cameraOrigin.TransformPoint(lastManualOffset);
				cam.position = val2;
				cam.rotation = Quaternion.LookRotation(cameraOrigin.position - val2, Vector3.up);
			}
			else
			{
				cam.position = Vector3.Lerp(cam.position, targetTransform.position, Time.deltaTime * 10f);
				cam.rotation = Quaternion.Lerp(cam.rotation, targetTransform.rotation, Time.deltaTime * 10f);
			}
		}
		lastFrameCameraOffset = cameraOrigin.InverseTransformPoint(cam.position);
	}

	private void HandleSecondaryClickCameraMovement()
	{
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		if (timeSinceCameraManuallyAdjusted <= 0.01f)
		{
			if (GameInput.GetButton(GameInput.ButtonCode.SecondaryClick))
			{
				x += GameInput.MouseDelta.x * 60f * 0.02f * Singleton<Settings>.Instance.LookSensitivity;
				y -= GameInput.MouseDelta.y * 40f * 0.02f * Singleton<Settings>.Instance.LookSensitivity;
				y = ClampAngle(y, -20f, 89f);
				Quaternion val = Quaternion.Euler(y, x, 0f);
				Vector3 targetPosition = val * new Vector3(0f, 0f, 0f - orbitDistance) + cameraOrigin.position;
				cam.rotation = val;
				cam.position = LimitCameraPosition(targetPosition);
			}
			else
			{
				Vector3 val2 = cameraOrigin.TransformPoint(lastFrameCameraOffset) - cameraOrigin.position;
				Vector3 normalized = ((Vector3)(ref val2)).normalized;
				Vector3 targetPosition2 = cameraOrigin.position + normalized * orbitDistance;
				cam.position = LimitCameraPosition(targetPosition2);
				cam.LookAt(cameraOrigin);
				Quaternion rotation = cam.rotation;
				Vector3 eulerAngles = ((Quaternion)(ref rotation)).eulerAngles;
				x = eulerAngles.y;
				y = eulerAngles.x;
			}
			lastManualOffset = cameraOrigin.InverseTransformPoint(cam.position);
		}
		else if (timeSinceCameraManuallyAdjusted < 0.61f)
		{
			targetTransform.position = Vector3.Lerp(cameraOrigin.TransformPoint(lastManualOffset), targetTransform.position, (timeSinceCameraManuallyAdjusted - 0.01f) / 0.6f);
			targetTransform.LookAt(cameraOrigin);
			cam.position = Vector3.Lerp(cam.position, targetTransform.position, Time.deltaTime * 10f);
			cam.rotation = Quaternion.Lerp(cam.rotation, targetTransform.rotation, Time.deltaTime * 10f);
		}
		else
		{
			cam.position = Vector3.Lerp(cam.position, targetTransform.position, Time.deltaTime * 10f);
			cam.rotation = Quaternion.Lerp(cam.rotation, targetTransform.rotation, Time.deltaTime * 10f);
		}
		lastFrameCameraOffset = cameraOrigin.InverseTransformPoint(cam.position);
	}

	private void ForceCameraReturn()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (mouseIdleTimer <= mouseIdleCooldown + 0.6f)
		{
			lastManualOffset = cameraOrigin.InverseTransformPoint(cam.position);
			mouseIdleTimer = mouseIdleCooldown + 0.001f;
		}
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

	private Vector3 GetTargetCameraPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = -((Component)this).transform.forward;
		val.y = 0f;
		((Vector3)(ref val)).Normalize();
		if (cameraReversed)
		{
			val *= -1f;
		}
		return ((Component)this).transform.position + val * lateralOffset + Vector3.up * verticalOffset;
	}

	private Vector3 LimitCameraPosition(Vector3 targetPosition)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = targetPosition;
		LayerMask.op_Implicit(LayerMask.op_Implicit(LayerMask.op_Implicit(LayerMask.op_Implicit(default(LayerMask)) | (1 << LayerMask.NameToLayer("Default")))) | (1 << LayerMask.NameToLayer("Terrain")));
		float num = 0.45f;
		Vector3 val2 = Vector3.Normalize(val - cameraOrigin.position);
		RaycastHit val3 = default(RaycastHit);
		if (Physics.Raycast(cameraOrigin.position, val2, ref val3, Vector3.Distance(((Component)this).transform.position, val) + num, 1 << LayerMask.NameToLayer("Default")))
		{
			val = ((RaycastHit)(ref val3)).point - val2 * num;
		}
		return val;
	}
}
