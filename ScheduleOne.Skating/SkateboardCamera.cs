using System;
using FishNet.Object;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Skating;

[RequireComponent(typeof(Skateboard))]
public class SkateboardCamera : NetworkBehaviour
{
	private const float followDelta = 7.5f;

	private const float yMinLimit = -20f;

	private const float manualOverrideTime = 0.01f;

	private const float manualOverrideReturnTime = 0.6f;

	private const float xSpeed = 60f;

	private const float ySpeed = 40f;

	private const float yMaxLimit = 89f;

	[Header("References")]
	public Transform cameraOrigin;

	[Header("Settings")]
	public float CameraFollowSpeed = 10f;

	public float HorizontalOffset = -2.5f;

	public float VerticalOffset = 2f;

	public float CameraDownAngle = 18f;

	[Header("Settings")]
	public float FOVMultiplier_MinSpeed = 1f;

	public float FOVMultiplier_MaxSpeed = 1.3f;

	public float FOVMultiplierChangeRate = 3f;

	private Skateboard board;

	private float currentFovMultiplier = 1f;

	private bool cameraReversed;

	private bool cameraAdjusted;

	private float timeSinceCameraManuallyAdjusted = float.MaxValue;

	private float orbitDistance;

	private Vector3 lastFrameCameraOffset = Vector3.zero;

	private Vector3 lastManualOffset = Vector3.zero;

	private Transform targetTransform;

	private Transform cameraDolly;

	private float x;

	private float y;

	private float mouseIdleCooldown = 1.5f;

	private float mouseIdleTimer;

	private bool NetworkInitialize___EarlyScheduleOne_002ESkating_002ESkateboardCameraAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ESkating_002ESkateboardCameraAssembly_002DCSharp_002Edll_Excuted;

	private Transform cam => ((Component)PlayerSingleton<PlayerCamera>.Instance).transform;

	private bool NeedSecondaryClick => GameInput.CurrentInputDevice == GameInput.InputDeviceType.KeyboardMouse;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ESkating_002ESkateboardCamera_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	private void OnPlayerMountedSkateboard(Skateboard skateboard)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
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

	public override void OnStartClient()
	{
		((NetworkBehaviour)this).OnStartClient();
		if (!((NetworkBehaviour)board).IsOwner)
		{
			((Behaviour)this).enabled = false;
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)Player.Local != (Object)null)
		{
			Player local = Player.Local;
			local.onSkateboardMounted = (Action<Skateboard>)Delegate.Remove(local.onSkateboardMounted, new Action<Skateboard>(OnPlayerMountedSkateboard));
		}
		Object.Destroy((Object)(object)((Component)targetTransform).gameObject);
		Object.Destroy((Object)(object)((Component)cameraDolly).gameObject);
	}

	private void Update()
	{
		if (((NetworkBehaviour)this).IsSpawned)
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

	private void CheckForClick()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		timeSinceCameraManuallyAdjusted += Time.deltaTime;
		if (GameInput.GetButton(GameInput.ButtonCode.SecondaryClick))
		{
			if (GameInput.GetButtonDown(GameInput.ButtonCode.SecondaryClick) && timeSinceCameraManuallyAdjusted > 0.01f)
			{
				cameraAdjusted = true;
				Quaternion rotation = cam.rotation;
				Vector3 eulerAngles = ((Quaternion)(ref rotation)).eulerAngles;
				x = eulerAngles.y;
				y = eulerAngles.x;
				orbitDistance = Mathf.Sqrt(Mathf.Pow(HorizontalOffset, 2f) + Mathf.Pow(VerticalOffset, 2f));
			}
			if (cameraAdjusted)
			{
				timeSinceCameraManuallyAdjusted = 0f;
			}
		}
		else
		{
			cameraAdjusted = false;
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
			orbitDistance = Mathf.Sqrt(Mathf.Pow(HorizontalOffset, 2f) + Mathf.Pow(VerticalOffset, 2f));
		}
		else
		{
			mouseIdleTimer += Time.deltaTime;
		}
	}

	private void LateUpdate()
	{
		if (((NetworkBehaviour)this).IsSpawned && PlayerSingleton<PlayerCamera>.InstanceExists && ((NetworkBehaviour)board).Owner.IsLocalClient)
		{
			UpdateCamera();
			UpdateFOV();
		}
	}

	private void UpdateCamera()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		targetTransform.position = LimitCameraPosition(GetTargetCameraPosition());
		targetTransform.LookAt(cameraOrigin);
		cameraDolly.position = Vector3.Lerp(cameraDolly.position, targetTransform.position, Time.deltaTime * 7.5f);
		cameraDolly.rotation = Quaternion.Lerp(cameraDolly.rotation, targetTransform.rotation, Time.deltaTime * 7.5f);
		orbitDistance = Mathf.Clamp(Vector3.Distance(cameraOrigin.position, cameraDolly.position), Mathf.Sqrt(Mathf.Pow(HorizontalOffset, 2f) + Mathf.Pow(VerticalOffset, 2f)), 100f);
		if (NeedSecondaryClick)
		{
			HandleSecondaryClickCameraMovement();
		}
		else
		{
			HandleNonSecondaryClickCameraMovement();
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
				cam.position = Vector3.Lerp(cam.position, targetTransform.position, Time.deltaTime * 7.5f);
				cam.rotation = Quaternion.Lerp(cam.rotation, targetTransform.rotation, Time.deltaTime * 7.5f);
			}
			else if (mouseIdleTimer <= mouseIdleCooldown)
			{
				Vector3 val2 = cameraOrigin.TransformPoint(lastManualOffset);
				cam.position = val2;
				cam.rotation = Quaternion.LookRotation(cameraOrigin.position - val2, Vector3.up);
			}
			else
			{
				cam.position = Vector3.Lerp(cam.position, targetTransform.position, Time.deltaTime * 7.5f);
				cam.rotation = Quaternion.Lerp(cam.rotation, targetTransform.rotation, Time.deltaTime * 7.5f);
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
			cam.position = Vector3.Lerp(cam.position, targetTransform.position, Time.deltaTime * 7.5f);
			cam.rotation = Quaternion.Lerp(cam.rotation, targetTransform.rotation, Time.deltaTime * 7.5f);
		}
		else
		{
			cam.position = Vector3.Lerp(cam.position, targetTransform.position, Time.deltaTime * 7.5f);
			cam.rotation = Quaternion.Lerp(cam.rotation, targetTransform.rotation, Time.deltaTime * 7.5f);
		}
		lastFrameCameraOffset = cameraOrigin.InverseTransformPoint(cam.position);
	}

	private void UpdateFOV()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		float fOVMultiplier_MinSpeed = FOVMultiplier_MinSpeed;
		float fOVMultiplier_MaxSpeed = FOVMultiplier_MaxSpeed;
		Vector3 velocity = board.Rb.velocity;
		float num = Mathf.Lerp(fOVMultiplier_MinSpeed, fOVMultiplier_MaxSpeed, Mathf.Clamp01(((Vector3)(ref velocity)).magnitude / board.TopSpeed_Ms));
		currentFovMultiplier = Mathf.Lerp(currentFovMultiplier, num, Time.deltaTime * FOVMultiplierChangeRate);
		PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(currentFovMultiplier * Singleton<Settings>.Instance.CameraFOV, 0f);
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
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.ProjectOnPlane(((Component)this).transform.forward, Vector3.up);
		Vector3 up = Vector3.up;
		return ((Component)this).transform.position + val * HorizontalOffset + up * VerticalOffset;
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

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ESkating_002ESkateboardCameraAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ESkating_002ESkateboardCameraAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ESkating_002ESkateboardCameraAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ESkating_002ESkateboardCameraAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void Awake_UserLogic_ScheduleOne_002ESkating_002ESkateboardCamera_Assembly_002DCSharp_002Edll()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		board = ((Component)this).GetComponent<Skateboard>();
		targetTransform = new GameObject("VehicleCameraTargetTransform").transform;
		targetTransform.SetParent(NetworkSingleton<GameManager>.Instance.Temp);
		cameraDolly = new GameObject("VehicleCameraDolly").transform;
		cameraDolly.SetParent(NetworkSingleton<GameManager>.Instance.Temp);
		if ((Object)(object)Player.Local != (Object)null)
		{
			Player local = Player.Local;
			local.onSkateboardMounted = (Action<Skateboard>)Delegate.Combine(local.onSkateboardMounted, new Action<Skateboard>(OnPlayerMountedSkateboard));
		}
	}
}
