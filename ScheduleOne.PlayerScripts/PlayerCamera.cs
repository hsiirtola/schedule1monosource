using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.Tools;
using ScheduleOne.UI;
using ScheduleOne.UI.Compass;
using ScheduleOne.UI.Items;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ScheduleOne.PlayerScripts;

public class PlayerCamera : PlayerSingleton<PlayerCamera>
{
	public enum ECameraMode
	{
		Default,
		Vehicle,
		Skateboard
	}

	public const float CAMERA_SHAKE_MULTIPLIER = 0.1f;

	[Header("Settings")]
	public float cameraOffsetFromTop = -0.15f;

	public float SprintFoVBoost = 1.15f;

	public float FoVChangeRate = 4f;

	public float HorizontalCameraBob = 1f;

	public float VerticalCameraBob = 1f;

	public float BobRate = 10f;

	public AnimationCurve HorizontalBobCurve;

	public AnimationCurve VerticalBobCurve;

	public float FreeCamSpeed = 1f;

	public float FreeCamAcceleration = 2f;

	public bool SmoothLook;

	public float SmoothLookSpeed = 1f;

	public FloatSmoother FoVChangeSmoother;

	public FloatSmoother SmoothLookSmoother;

	[Header("References")]
	public Transform CameraContainer;

	public Camera Camera;

	public Camera OverlayCamera;

	public Animator Animator;

	public AnimationClip[] JoltClips;

	public UniversalRenderPipelineAsset[] URPAssets;

	public Transform ViewAvatarCameraPosition;

	public HeartbeatSoundController HeartbeatSoundController;

	public ParticleSystem Flies;

	public AudioSourceController MethRumble;

	public RandomizedAudioSourceController SchizoVoices;

	[HideInInspector]
	public bool blockNextStopTransformOverride;

	private Volume globalVolume;

	private DepthOfField DoF;

	private Coroutine cameraShakeCoroutine;

	private Vector3 cameraLocalPos = Vector3.zero;

	private Vector3 freeCamMovement = Vector3.zero;

	private Coroutine focusRoutine;

	private float focusMouseX;

	private float focusMouseY;

	private Dictionary<int, MotionEvent> movementEvents = new Dictionary<int, MotionEvent>();

	private List<int> movementEventKeys = new List<int>();

	private float freeCamSpeed = 1f;

	private float mouseX;

	private float mouseY;

	private Vector2 seizureJitter = Vector2.zero;

	private float schizoFoV;

	private float timeUntilNextSchizoVoice = 15f;

	private static bool isCursorShowing = true;

	private List<Vector3> gizmos = new List<Vector3>();

	private Vector3 cameralocalPos_PriorOverride = Vector3.zero;

	private Quaternion cameraLocalRot_PriorOverride = Quaternion.identity;

	public Coroutine ILerpCamera_Coroutine;

	private Coroutine lookRoutine;

	private Coroutine DoFCoroutine;

	private Coroutine ILerpCameraFOV_Coroutine;

	public static GraphicsSettings.EAntiAliasingMode AntiAliasingMode { get; private set; } = GraphicsSettings.EAntiAliasingMode.Off;

	public bool canLook { get; protected set; } = true;

	public int activeUIElementCount => activeUIElements.Count;

	public bool transformOverriden { get; protected set; }

	public bool fovOverriden { get; protected set; }

	public bool FreeCamEnabled { get; private set; }

	public bool ViewingAvatar { get; private set; }

	public ECameraMode CameraMode { get; protected set; }

	public bool MethVisuals { get; set; }

	public bool CocaineVisuals { get; set; }

	public float FovJitter { get; private set; }

	public List<string> activeUIElements { get; protected set; } = new List<string>();

	public static bool IsCursorShowing => isCursorShowing;

	protected override void Awake()
	{
		base.Awake();
		Player.onLocalPlayerSpawned = (Action)Delegate.Remove(Player.onLocalPlayerSpawned, new Action(PlayerSpawned));
		Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(PlayerSpawned));
		GameInput.RegisterExitListener(Exit, 100);
		ApplyAASettings();
	}

	public override void OnStartClient(bool IsOwner)
	{
		base.OnStartClient(IsOwner);
		if (!IsOwner)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
		else
		{
			((Behaviour)Camera).enabled = true;
		}
	}

	protected override void Start()
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		base.Start();
		if (Singleton<Settings>.InstanceExists)
		{
			Camera.fieldOfView = Singleton<Settings>.Instance.CameraFOV;
		}
		if ((Object)(object)GameObject.Find("GlobalVolume") != (Object)null)
		{
			globalVolume = GameObject.Find("GlobalVolume").GetComponent<Volume>();
			globalVolume.sharedProfile.TryGet<DepthOfField>(ref DoF);
			((VolumeComponent)DoF).active = false;
		}
		cameralocalPos_PriorOverride = ((Component)this).transform.localPosition;
		FoVChangeSmoother.Initialize();
		FoVChangeSmoother.SetDefault(0f);
		SmoothLookSmoother.Initialize();
		SmoothLookSmoother.SetDefault(0f);
		SmoothLookSmoother.SetSmoothingSpeed(0.5f);
		LockMouse();
	}

	private void PlayerSpawned()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		Player.Local.onTased.AddListener((UnityAction)delegate
		{
			StartCameraShake(1f, 2f);
		});
		Player.Local.onStruckByLightning.AddListener((UnityAction)delegate
		{
			StartCameraShake(2f, 2f);
		});
		Player.Local.onTasedEnd.AddListener(new UnityAction(StopCameraShake));
	}

	public static void SetAntialiasingMode(GraphicsSettings.EAntiAliasingMode mode)
	{
		AntiAliasingMode = mode;
		if ((Object)(object)PlayerSingleton<PlayerCamera>.Instance != (Object)null)
		{
			PlayerSingleton<PlayerCamera>.Instance.ApplyAASettings();
		}
	}

	public void ApplyAASettings()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		AntialiasingMode val = (AntialiasingMode)0;
		val = (AntialiasingMode)(AntiAliasingMode switch
		{
			GraphicsSettings.EAntiAliasingMode.Off => 0, 
			GraphicsSettings.EAntiAliasingMode.FXAA => 1, 
			GraphicsSettings.EAntiAliasingMode.SMAA => 2, 
			_ => 0, 
		});
		((Component)Camera).GetComponent<UniversalAdditionalCameraData>().antialiasing = val;
	}

	protected virtual void Update()
	{
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		UpdateCameraBob();
		if (canLook)
		{
			RotateCamera();
		}
		if (MethVisuals)
		{
			MethRumble.VolumeMultiplier = Mathf.MoveTowards(MethRumble.VolumeMultiplier, 1f, Time.deltaTime * 0.5f);
			if (!MethRumble.IsPlaying)
			{
				MethRumble.Play();
			}
		}
		else
		{
			MethRumble.VolumeMultiplier = Mathf.MoveTowards(MethRumble.VolumeMultiplier, 0f, Time.deltaTime * 0.5f);
			if (MethRumble.VolumeMultiplier == 0f && MethRumble.IsPlaying)
			{
				MethRumble.Stop();
			}
		}
		if (FreeCamEnabled)
		{
			RotateFreeCam();
			UpdateFreeCamInput();
			MoveFreeCam();
		}
		if (Player.Local.Schizophrenic)
		{
			timeUntilNextSchizoVoice -= Time.deltaTime;
			if (timeUntilNextSchizoVoice <= 0f)
			{
				timeUntilNextSchizoVoice = Random.Range(5f, 20f);
				SchizoVoices.VolumeMultiplier = Random.Range(0.5f, 1f);
				SchizoVoices.PitchMultiplier = Random.Range(0.4f, 1f);
				((Component)SchizoVoices).transform.localPosition = Random.insideUnitSphere * 1f;
				SchizoVoices.Play();
			}
		}
		if (GameInput.GetButton(GameInput.ButtonCode.ViewAvatar))
		{
			if (!ViewingAvatar && activeUIElementCount == 0 && canLook && !GameInput.IsTyping)
			{
				ViewAvatar();
			}
			if (ViewingAvatar)
			{
				Vector3 worldPos = ViewAvatarCameraPosition.position;
				Vector3 val = ((Component)PlayerSingleton<PlayerMovement>.Instance).transform.TransformPoint(new Vector3(0f, GetTargetLocalY(), 0f));
				Vector3 val2 = ViewAvatarCameraPosition.position - val;
				RaycastHit val3 = default(RaycastHit);
				if (Physics.Raycast(val, ((Vector3)(ref val2)).normalized, ref val3, Vector3.Distance(val, ViewAvatarCameraPosition.position), 1 << LayerMask.NameToLayer("Default"), (QueryTriggerInteraction)1))
				{
					worldPos = ((RaycastHit)(ref val3)).point;
				}
				OverrideTransform(worldPos, ViewAvatarCameraPosition.rotation, 0f, keepParented: true);
				((Component)this).transform.LookAt(((Component)Player.Local.Avatar.LowestSpine).transform);
			}
		}
		else if (ViewingAvatar)
		{
			StopViewingAvatar();
		}
		if ((FreeCamEnabled || Application.isEditor) && Input.GetKeyDown((KeyCode)293))
		{
			Screenshot();
		}
		UpdateMovementEvents();
	}

	private void Screenshot()
	{
		((MonoBehaviour)this).StartCoroutine(Routine());
		static IEnumerator Routine()
		{
			yield return (object)new WaitForEndOfFrame();
			string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
			folderPath = Path.Combine(folderPath, "Screenshot_" + DateTime.Now.ToString("HH-mm-ss") + ".png");
			Console.Log("Screenshot saved to: " + folderPath);
			ScreenCapture.CaptureScreenshot(folderPath, 2);
			yield return (object)new WaitForEndOfFrame();
		}
	}

	protected virtual void LateUpdate()
	{
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Camera == (Object)null || (Object)(object)((Component)this).transform == (Object)null || !PlayerSingleton<PlayerMovement>.InstanceExists)
		{
			return;
		}
		if (!transformOverriden && ILerpCamera_Coroutine == null)
		{
			((Component)this).transform.localPosition = new Vector3(0f, GetTargetLocalY(), 0f);
		}
		if (!fovOverriden && ILerpCameraFOV_Coroutine == null)
		{
			float num = Singleton<Settings>.Instance.CameraFOV * (PlayerSingleton<PlayerMovement>.Instance.IsSprinting ? SprintFoVBoost : 1f);
			if (MethVisuals)
			{
				FovJitter = Mathf.Lerp(FovJitter, Random.Range(0f, 1f), Time.deltaTime * 10f);
			}
			else if (CocaineVisuals)
			{
				FovJitter = Mathf.Lerp(FovJitter, 1f, Time.deltaTime * 0.5f);
			}
			else
			{
				FovJitter = Mathf.Lerp(FovJitter, 0f, Time.deltaTime * 3f);
			}
			if (Player.Local.Schizophrenic)
			{
				schizoFoV = 0f - Mathf.Lerp(schizoFoV, Mathf.Sin(Time.time * 0.5f) * 20f, Time.deltaTime);
			}
			else
			{
				schizoFoV = Mathf.Lerp(schizoFoV, 0f, Time.deltaTime);
			}
			num += FovJitter * 6f;
			num += schizoFoV;
			num += FoVChangeSmoother.CurrentValue;
			Camera.fieldOfView = Mathf.MoveTowards(Camera.fieldOfView, num, Time.deltaTime * FoVChangeRate);
		}
		((Component)Camera).transform.localPosition = cameraLocalPos;
		cameraLocalPos = Vector3.zero;
	}

	private void Exit(ExitAction action)
	{
		if (!action.Used)
		{
			if (FreeCamEnabled && action.exitType == ExitType.Escape)
			{
				action.Used = true;
				SetFreeCam(enable: false);
			}
			if (ViewingAvatar && action.exitType == ExitType.Escape)
			{
				action.Used = true;
				StopViewingAvatar();
			}
		}
	}

	public float GetTargetLocalY()
	{
		if (!PlayerSingleton<PlayerMovement>.InstanceExists)
		{
			return 0f;
		}
		return PlayerSingleton<PlayerMovement>.Instance.Controller.height / 2f + cameraOffsetFromTop;
	}

	public void SetCameraMode(ECameraMode mode)
	{
		CameraMode = mode;
	}

	private void RotateCamera()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		float num = GameInput.MouseDelta.x * (Singleton<Settings>.InstanceExists ? Singleton<Settings>.Instance.LookSensitivity : 1f);
		float num2 = GameInput.MouseDelta.y * (Singleton<Settings>.InstanceExists ? Singleton<Settings>.Instance.LookSensitivity : 1f);
		if (Player.Local.Disoriented)
		{
			num2 = 0f - num2;
		}
		if (Player.Local.Seizure)
		{
			Vector2 val = default(Vector2);
			((Vector2)(ref val))._002Ector(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
			seizureJitter = Vector2.Lerp(seizureJitter, val, Time.deltaTime * 10f);
			num += seizureJitter.x;
			num2 += seizureJitter.y;
		}
		if (Player.Local.Schizophrenic)
		{
			num += Mathf.Sin(Time.time * 0.4f) * 0.01f;
			num2 += Mathf.Sin(Time.time * 0.3f) * 0.01f;
		}
		if (SmoothLook)
		{
			mouseX = Mathf.Lerp(mouseX, num, SmoothLookSpeed);
			mouseY = Mathf.Lerp(mouseY, num2, SmoothLookSpeed);
		}
		else if (SmoothLookSmoother.CurrentValue <= 0.01f)
		{
			mouseX = num;
			mouseY = num2;
		}
		else
		{
			float num3 = Mathf.Lerp(50f, 1f, SmoothLookSmoother.CurrentValue);
			mouseX = Mathf.Lerp(mouseX, num, num3);
			mouseY = Mathf.Lerp(mouseY, num2, num3);
		}
		Quaternion val2 = ((Component)this).transform.localRotation;
		Vector3 eulerAngles = ((Quaternion)(ref val2)).eulerAngles;
		val2 = ((Component)Player.Local).transform.rotation;
		Vector3 eulerAngles2 = ((Quaternion)(ref val2)).eulerAngles;
		if (Singleton<Settings>.InstanceExists && Singleton<Settings>.Instance.InvertMouse)
		{
			mouseY = 0f - mouseY;
		}
		mouseX += focusMouseX;
		mouseY += focusMouseY;
		eulerAngles.x -= Mathf.Clamp(mouseY, -89f, 89f);
		eulerAngles2.y += mouseX;
		eulerAngles.z = 0f;
		if (eulerAngles.x >= 180f)
		{
			if (eulerAngles.x < 271f)
			{
				eulerAngles.x = 271f;
			}
		}
		else if (eulerAngles.x > 89f)
		{
			eulerAngles.x = 89f;
		}
		((Component)this).transform.localRotation = Quaternion.Euler(eulerAngles);
		((Component)this).transform.localEulerAngles = new Vector3(((Component)this).transform.localEulerAngles.x, 0f, 0f);
		((Component)Player.Local).transform.rotation = Quaternion.Euler(eulerAngles2);
	}

	public void LockMouse()
	{
		isCursorShowing = false;
		Cursor.lockState = (CursorLockMode)1;
		Cursor.visible = false;
		if (Singleton<HUD>.InstanceExists)
		{
			Singleton<HUD>.Instance.SetCrosshairVisible(vis: true);
		}
	}

	public void FreeMouse()
	{
		isCursorShowing = true;
		if (GameInput.GetCurrentInputDeviceIsKeyboardMouse())
		{
			Cursor.lockState = (CursorLockMode)0;
			Cursor.visible = true;
		}
		if (Singleton<HUD>.InstanceExists)
		{
			Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
		}
	}

	public bool LookRaycast(float range, out RaycastHit hit, LayerMask layerMask, bool includeTriggers = true, float radius = 0f)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (radius == 0f)
		{
			return Physics.Raycast(((Component)this).transform.position, ((Component)this).transform.forward, ref hit, range, LayerMask.op_Implicit(layerMask), (QueryTriggerInteraction)((!includeTriggers) ? 1 : 2));
		}
		return Physics.SphereCast(((Component)this).transform.position, radius, ((Component)this).transform.forward, ref hit, range, LayerMask.op_Implicit(layerMask), (QueryTriggerInteraction)((!includeTriggers) ? 1 : 2));
	}

	public bool LookRaycast_ExcludeBuildables(float range, out RaycastHit hit, LayerMask layerMask, bool includeTriggers = true)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit[] array = Physics.RaycastAll(((Component)this).transform.position, ((Component)this).transform.forward, range, LayerMask.op_Implicit(layerMask), (QueryTriggerInteraction)((!includeTriggers) ? 1 : 2));
		RaycastHit val = default(RaycastHit);
		for (int i = 0; i < array.Length; i++)
		{
			if (!Object.op_Implicit((Object)(object)((Component)((RaycastHit)(ref array[i])).collider).GetComponentInParent<BuildableItem>()) && ((Object)(object)((RaycastHit)(ref val)).collider == (Object)null || Vector3.Distance(((Component)this).transform.position, ((RaycastHit)(ref array[i])).point) < Vector3.Distance(((Component)this).transform.position, ((RaycastHit)(ref val)).point)))
			{
				val = array[i];
			}
		}
		if ((Object)(object)((RaycastHit)(ref val)).collider != (Object)null)
		{
			hit = val;
			return true;
		}
		hit = default(RaycastHit);
		return false;
	}

	private void OnDrawGizmosSelected()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < gizmos.Count; i++)
		{
			Gizmos.DrawSphere(gizmos[i], 0.05f);
		}
		gizmos.Clear();
	}

	public bool Raycast_ExcludeBuildables(Vector3 origin, Vector3 direction, float range, out RaycastHit hit, LayerMask layerMask, bool includeTriggers = false, float radius = 0f, float maxAngleDifference = 0f)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit[] array = ((radius != 0f) ? Physics.SphereCastAll(origin, radius, direction, range, LayerMask.op_Implicit(layerMask), (QueryTriggerInteraction)((!includeTriggers) ? 1 : 2)) : Physics.RaycastAll(origin, direction, range, LayerMask.op_Implicit(layerMask), (QueryTriggerInteraction)((!includeTriggers) ? 1 : 2)));
		_ = 0f;
		RaycastHit val = default(RaycastHit);
		for (int i = 0; i < array.Length; i++)
		{
			if (!(((RaycastHit)(ref array[i])).point == Vector3.zero) && !Object.op_Implicit((Object)(object)((Component)((RaycastHit)(ref array[i])).collider).GetComponentInParent<BuildableItem>()) && (maxAngleDifference == 0f || Vector3.Angle(direction, -((RaycastHit)(ref array[i])).normal) < maxAngleDifference) && ((Object)(object)((RaycastHit)(ref val)).collider == (Object)null || Vector3.Distance(((Component)this).transform.position, ((RaycastHit)(ref array[i])).point) < Vector3.Distance(((Component)this).transform.position, ((RaycastHit)(ref val)).point)))
			{
				val = array[i];
			}
		}
		if ((Object)(object)((RaycastHit)(ref val)).collider != (Object)null)
		{
			hit = val;
			return true;
		}
		hit = default(RaycastHit);
		return false;
	}

	public Ray GetMouseRay()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		return PlayerSingleton<PlayerCamera>.Instance.Camera.ScreenPointToRay(Input.mousePosition);
	}

	public bool MouseRaycast(float range, out RaycastHit hit, LayerMask layerMask, bool includeTriggers = true, float radius = 0f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		Ray mouseRay = GetMouseRay();
		if (radius == 0f)
		{
			return Physics.Raycast(mouseRay, ref hit, range, LayerMask.op_Implicit(layerMask), (QueryTriggerInteraction)((!includeTriggers) ? 1 : 2));
		}
		return Physics.SphereCast(mouseRay, radius, ref hit, range, LayerMask.op_Implicit(layerMask), (QueryTriggerInteraction)((!includeTriggers) ? 1 : 2));
	}

	public bool LookSpherecast(float range, float radius, out RaycastHit hit, LayerMask layerMask)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		return Physics.SphereCast(((Component)this).transform.position, radius, ((Component)this).transform.forward, ref hit, range, LayerMask.op_Implicit(layerMask));
	}

	public void OverrideTransform(Vector3 worldPos, Quaternion rot, float lerpTime, bool keepParented = false)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		canLook = false;
		if (ILerpCamera_Coroutine != null)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(ILerpCamera_Coroutine);
			ILerpCamera_Coroutine = null;
		}
		else if (!transformOverriden)
		{
			cameralocalPos_PriorOverride = ((Component)this).transform.localPosition;
			cameraLocalRot_PriorOverride = ((Component)this).transform.localRotation;
		}
		transformOverriden = true;
		if (!keepParented)
		{
			((Component)this).transform.SetParent((Transform)null);
		}
		ILerpCamera_Coroutine = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(ILerpCamera(worldPos, rot, lerpTime, worldSpace: true));
	}

	protected IEnumerator ILerpCamera(Vector3 endPos, Quaternion endRot, float lerpTime, bool worldSpace, bool returnToRestingPosition = false, bool reenableLook = false)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Vector3 startPos = ((Component)this).transform.localPosition;
		Quaternion startRot = ((Component)this).transform.rotation;
		if (worldSpace)
		{
			startPos = ((Component)this).transform.position;
		}
		float elapsed = 0f;
		while (elapsed < lerpTime)
		{
			if (returnToRestingPosition)
			{
				((Component)this).transform.localPosition = Vector3.Lerp(startPos, new Vector3(0f, GetTargetLocalY(), 0f), elapsed / lerpTime);
			}
			else if (worldSpace)
			{
				((Component)this).transform.position = Vector3.Lerp(startPos, endPos, elapsed / lerpTime);
			}
			else
			{
				((Component)this).transform.localPosition = Vector3.Lerp(startPos, endPos, elapsed / lerpTime);
			}
			((Component)this).transform.rotation = Quaternion.Lerp(startRot, endRot, elapsed / lerpTime);
			elapsed += Time.deltaTime;
			yield return (object)new WaitForEndOfFrame();
		}
		if (returnToRestingPosition)
		{
			((Component)this).transform.localPosition = new Vector3(0f, GetTargetLocalY(), 0f);
		}
		else if (worldSpace)
		{
			((Component)this).transform.position = endPos;
		}
		else
		{
			((Component)this).transform.localPosition = endPos;
		}
		if (reenableLook)
		{
			SetCanLook(c: true);
		}
		((Component)this).transform.rotation = endRot;
		ILerpCamera_Coroutine = null;
	}

	public void StopTransformOverride(float lerpTime, bool reenableCameraLook = true, bool returnToOriginalRotation = true)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		if (blockNextStopTransformOverride)
		{
			blockNextStopTransformOverride = false;
			return;
		}
		if (ILerpCamera_Coroutine != null)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(ILerpCamera_Coroutine);
			ILerpCamera_Coroutine = null;
		}
		transformOverriden = false;
		((Component)this).transform.SetParent(((Component)PlayerSingleton<PlayerMovement>.Instance).transform);
		if (ILerpCamera_Coroutine != null)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(ILerpCamera_Coroutine);
		}
		Quaternion val = ((Component)PlayerSingleton<PlayerMovement>.Instance).transform.rotation * cameraLocalRot_PriorOverride;
		if (!returnToOriginalRotation)
		{
			val = ((Component)this).transform.rotation;
		}
		if (lerpTime == 0f)
		{
			((Component)this).transform.rotation = val;
			((Component)this).transform.localPosition = new Vector3(0f, GetTargetLocalY(), 0f);
			if (reenableCameraLook)
			{
				SetCanLook_True();
			}
		}
		else
		{
			ILerpCamera_Coroutine = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(ILerpCamera(cameralocalPos_PriorOverride, val, lerpTime, worldSpace: false, returnToRestingPosition: true, reenableCameraLook));
		}
	}

	public void LookAt(Vector3 point, float duration = 0.25f)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		if (lookRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(lookRoutine);
		}
		lookRoutine = ((MonoBehaviour)this).StartCoroutine(Look());
		IEnumerator Look()
		{
			Transform transform = ((Component)Player.Local).transform;
			Vector3 val = point - ((Component)this).transform.position;
			Vector3 val2 = transform.InverseTransformDirection(((Vector3)(ref val)).normalized);
			float num = Mathf.Atan2(val2.x, val2.z) * 57.29578f;
			Quaternion playerEndRot = ((Component)Player.Local).transform.rotation * Quaternion.Euler(0f, num, 0f);
			float num2 = (0f - Mathf.Atan2(val2.y, val2.z)) * 57.29578f;
			num2 = Mathf.Clamp(num2, -89f, 89f);
			Quaternion cameraRotation = Quaternion.Euler(num2, 0f, 0f);
			Quaternion playerStartRot = ((Component)Player.Local).transform.rotation;
			Quaternion cameraStartRot = ((Component)this).transform.localRotation;
			for (float i = 0f; i < duration; i += Time.deltaTime)
			{
				((Component)Player.Local).transform.rotation = Quaternion.Lerp(playerStartRot, playerEndRot, i / duration);
				((Component)this).transform.localRotation = Quaternion.Lerp(cameraStartRot, cameraRotation, i / duration);
				yield return (object)new WaitForEndOfFrame();
			}
			((Component)Player.Local).transform.rotation = playerEndRot;
			((Component)this).transform.localRotation = cameraRotation;
			lookRoutine = null;
		}
	}

	private void SetCanLook_True()
	{
		SetCanLook(c: true);
	}

	public void SetCanLook(bool c)
	{
		canLook = c;
	}

	public void SetDoFActive(bool active, float lerpTime)
	{
		if (DoFCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(DoFCoroutine);
		}
		DoFCoroutine = ((MonoBehaviour)this).StartCoroutine(LerpDoF(active, lerpTime));
	}

	private IEnumerator LerpDoF(bool active, float lerpTime)
	{
		if (active)
		{
			((VolumeComponent)DoF).active = true;
		}
		float startFocusDist = ((VolumeParameter<float>)(object)DoF.focusDistance).value;
		float endFocusDist = ((!active) ? 5f : 0.1f);
		for (float i = 0f; i < lerpTime; i += Time.unscaledDeltaTime)
		{
			((VolumeParameter<float>)(object)DoF.focusDistance).value = Mathf.Lerp(startFocusDist, endFocusDist, i / lerpTime);
			yield return (object)new WaitForEndOfFrame();
		}
		((VolumeParameter<float>)(object)DoF.focusDistance).value = endFocusDist;
		if (!active)
		{
			((VolumeComponent)DoF).active = false;
		}
		DoFCoroutine = null;
	}

	public void OverrideFOV(float fov, float lerpTime)
	{
		if (ILerpCameraFOV_Coroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(ILerpCameraFOV_Coroutine);
		}
		fovOverriden = true;
		if (fov == -1f)
		{
			fov = Singleton<Settings>.Instance.CameraFOV;
		}
		ILerpCameraFOV_Coroutine = ((MonoBehaviour)this).StartCoroutine(ILerpFOV(fov, lerpTime));
	}

	protected IEnumerator ILerpFOV(float endFov, float lerpTime)
	{
		float startFov = Camera.fieldOfView;
		for (float i = 0f; i < lerpTime; i += Time.deltaTime)
		{
			Camera.fieldOfView = Mathf.Lerp(startFov, endFov, i / lerpTime);
			yield return (object)new WaitForEndOfFrame();
		}
		Camera.fieldOfView = endFov;
		ILerpCameraFOV_Coroutine = null;
	}

	public void StopFOVOverride(float lerpTime)
	{
		OverrideFOV(-1f, lerpTime);
		fovOverriden = false;
	}

	public void AddActiveUIElement(string name)
	{
		if (!activeUIElements.Contains(name))
		{
			activeUIElements.Add(name);
		}
	}

	public void RemoveActiveUIElement(string name)
	{
		if (activeUIElements.Contains(name))
		{
			activeUIElements.Remove(name);
		}
	}

	public void RegisterMovementEvent(int threshold, Action action)
	{
		if (threshold < 1)
		{
			Console.LogWarning("Movement events min. threshold is 1m!");
			return;
		}
		if (!movementEvents.ContainsKey(threshold * threshold))
		{
			movementEvents.Add(threshold * threshold, new MotionEvent());
			movementEventKeys.Add(threshold * threshold);
		}
		movementEvents[threshold * threshold].Actions.Add(action);
	}

	public void DeregisterMovementEvent(Action action)
	{
		foreach (int key in movementEvents.Keys)
		{
			MotionEvent motionEvent = movementEvents[key];
			if (motionEvent.Actions.Contains(action))
			{
				motionEvent.Actions.Remove(action);
				break;
			}
		}
	}

	private void UpdateMovementEvents()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		foreach (int movementEventKey in movementEventKeys)
		{
			MotionEvent motionEvent = movementEvents[movementEventKey];
			if (Vector3.SqrMagnitude(((Component)this).transform.position - motionEvent.LastUpdatedDistance) > (float)movementEventKey)
			{
				motionEvent.Update(((Component)this).transform.position);
			}
		}
	}

	private void ViewAvatar()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		ViewingAvatar = true;
		AddActiveUIElement("View avatar");
		Vector3 worldPos = ViewAvatarCameraPosition.position;
		Vector3 val = ((Component)PlayerSingleton<PlayerMovement>.Instance).transform.TransformPoint(new Vector3(0f, GetTargetLocalY(), 0f));
		Vector3 val2 = ViewAvatarCameraPosition.position - val;
		RaycastHit val3 = default(RaycastHit);
		if (Physics.Raycast(val, ((Vector3)(ref val2)).normalized, ref val3, Vector3.Distance(val, ViewAvatarCameraPosition.position), 1 << LayerMask.NameToLayer("Default"), (QueryTriggerInteraction)1))
		{
			worldPos = ((RaycastHit)(ref val3)).point;
		}
		OverrideTransform(worldPos, ViewAvatarCameraPosition.rotation, 0f, keepParented: true);
		((Component)this).transform.LookAt(((Component)Player.Local.Avatar.LowestSpine).transform);
		((Behaviour)Singleton<HUD>.Instance.canvas).enabled = false;
		PlayerSingleton<PlayerInventory>.Instance.SetViewmodelVisible(visible: false);
		Player.Local.SetVisibleToLocalPlayer(vis: true);
	}

	private void StopViewingAvatar()
	{
		ViewingAvatar = false;
		RemoveActiveUIElement("View avatar");
		StopTransformOverride(0f);
		((Behaviour)Singleton<HUD>.Instance.canvas).enabled = true;
		PlayerSingleton<PlayerInventory>.Instance.SetViewmodelVisible(visible: true);
		Player.Local.SetVisibleToLocalPlayer(vis: false);
	}

	public void JoltCamera()
	{
		AnimationClip val = JoltClips[Random.Range(0, JoltClips.Length)];
		Animator.Play(((Object)val).name, 0, 0f);
	}

	public bool PointInCameraView(Vector3 point)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Camera.WorldToViewportPoint(point);
		bool num = Is01(val.x) && Is01(val.y);
		bool flag = val.z > 0f;
		bool flag2 = false;
		Vector3 val2 = point - ((Component)Camera).transform.position;
		val2 = ((Vector3)(ref val2)).normalized;
		float num2 = Vector3.Distance(((Component)Camera).transform.position, point);
		RaycastHit val3 = default(RaycastHit);
		if (Physics.Raycast(((Component)Camera).transform.position, val2, ref val3, num2 + 0.05f, 1 << LayerMask.NameToLayer("Default")) && ((RaycastHit)(ref val3)).point != point)
		{
			flag2 = true;
		}
		if (num && flag)
		{
			return !flag2;
		}
		return false;
	}

	public bool Is01(float a)
	{
		if (a > 0f)
		{
			return a < 1f;
		}
		return false;
	}

	public void ResetRotation()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localRotation = Quaternion.identity;
	}

	public void FocusCameraOnTarget(Transform target)
	{
		if (focusRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(focusRoutine);
		}
		focusRoutine = ((MonoBehaviour)this).StartCoroutine(FocusRoutine());
		IEnumerator FocusRoutine()
		{
			Vector3 val2 = default(Vector3);
			Vector3 val3 = default(Vector3);
			for (float duration = 0f; duration < 0.75f; duration += Time.deltaTime)
			{
				if (!canLook)
				{
					break;
				}
				if (CameraMode != ECameraMode.Default)
				{
					break;
				}
				Vector3 val = target.position - ((Component)this).transform.position;
				((Vector3)(ref val2))._002Ector(val.x, 0f, val.z);
				((Vector3)(ref val3))._002Ector(0f, val.y, 0f);
				Vector3 val4 = target.position - ((Component)this).transform.position;
				Vector3 normalized = ((Vector3)(ref val4)).normalized;
				if (Vector3.Angle(((Component)this).transform.forward, normalized) < 5f || duration > 0.5f)
				{
					focusMouseX = 0f;
					focusMouseY = 0f;
					break;
				}
				float num = Vector3.SignedAngle(Vector3.ProjectOnPlane(((Component)this).transform.forward, Vector3.up), Vector3.ProjectOnPlane(normalized, Vector3.up), Vector3.up);
				val4 = ((Component)PlayerSingleton<PlayerMovement>.Instance).transform.TransformPoint(new Vector3(0f, ((Vector3)(ref val3)).magnitude, ((Vector3)(ref val2)).magnitude)) - ((Component)this).transform.position;
				Vector3 normalized2 = ((Vector3)(ref val4)).normalized;
				float num2 = Vector3.SignedAngle(((Component)this).transform.forward, normalized2, ((Component)this).transform.right);
				if (Mathf.Abs(num) > 5f)
				{
					focusMouseX = num * 0.1f;
				}
				else
				{
					focusMouseX = 0f;
				}
				if (Mathf.Abs(num2) > 5f)
				{
					focusMouseY = (0f - num2) * 0.1f;
				}
				else
				{
					focusMouseY = 0f;
				}
				yield return (object)new WaitForEndOfFrame();
			}
			focusMouseX = 0f;
			focusMouseY = 0f;
		}
	}

	public void StopFocus()
	{
		if (focusRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(focusRoutine);
		}
		focusMouseX = 0f;
		focusMouseY = 0f;
	}

	public void OpenInterface(bool keepInventoryVisible = false, bool keepCompassVisible = false)
	{
		SetCanLook(c: false);
		FreeMouse();
		Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
		Singleton<CompassManager>.Instance.SetCompassEnabled(keepCompassVisible);
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(keepInventoryVisible);
		PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
		Singleton<ItemUIManager>.Instance.SetDraggingEnabled(enabled: true);
	}

	public void CloseInterface(float cameraLerpTime = 0.2f, bool reenableCameraInput = true)
	{
		LockMouse();
		Singleton<HUD>.Instance.SetCrosshairVisible(vis: true);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
		Singleton<CompassManager>.Instance.SetCompassEnabled(enabled: true);
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
		PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: true);
		Singleton<ItemUIManager>.Instance.SetDraggingEnabled(enabled: false);
		Singleton<ItemUIManager>.Instance.DisableQuickMove();
		StopFOVOverride(cameraLerpTime);
		StopTransformOverride(cameraLerpTime, reenableCameraInput);
	}

	public void StartCameraShake(float intensity, float duration = -1f, bool decreaseOverTime = true)
	{
		StopCameraShake();
		cameraShakeCoroutine = ((MonoBehaviour)this).StartCoroutine(Shake());
		IEnumerator Shake()
		{
			float timeRemaining = duration;
			while (true)
			{
				float num = intensity;
				if (duration != -1f && decreaseOverTime)
				{
					num *= timeRemaining / duration;
				}
				cameraLocalPos += Random.insideUnitSphere * num * 0.1f;
				timeRemaining -= Time.deltaTime;
				if (timeRemaining <= 0f && duration != -1f)
				{
					break;
				}
				yield return (object)new WaitForEndOfFrame();
			}
			((Component)Camera).transform.localPosition = Vector3.zero;
			cameraShakeCoroutine = null;
		}
	}

	public void StopCameraShake()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (cameraShakeCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(cameraShakeCoroutine);
			((Component)Camera).transform.localPosition = Vector3.zero;
		}
	}

	public void UpdateCameraBob()
	{
		float num = 1f;
		if (PlayerSingleton<PlayerMovement>.InstanceExists)
		{
			num = PlayerSingleton<PlayerMovement>.Instance.CurrentSprintMultiplier - 1f;
		}
		num *= Singleton<Settings>.Instance.CameraBobIntensity;
		cameraLocalPos.x += HorizontalBobCurve.Evaluate(Time.time * BobRate % 1f) * num * HorizontalCameraBob;
		cameraLocalPos.y += VerticalBobCurve.Evaluate(Time.time * BobRate % 1f) * num * VerticalCameraBob;
	}

	public void SetFreeCam(bool enable, bool reenableLook = true)
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		FreeCamEnabled = enable;
		((Behaviour)Singleton<HUD>.Instance.canvas).enabled = !enable;
		PlayerSingleton<PlayerMovement>.Instance.CanMove = !enable;
		Player.Local.SetVisibleToLocalPlayer(enable);
		if (enable)
		{
			OverrideTransform(((Component)this).transform.position, ((Component)this).transform.rotation, 0f);
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
		}
		else
		{
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
			StopTransformOverride(0f, reenableLook);
			freeCamMovement = Vector3.zero;
		}
	}

	private void RotateFreeCam()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		mouseX = Mathf.Lerp(mouseX, GameInput.MouseDelta.x * (Singleton<Settings>.InstanceExists ? Singleton<Settings>.Instance.LookSensitivity : 1f), SmoothLookSpeed);
		mouseY = Mathf.Lerp(mouseY, GameInput.MouseDelta.y * (Singleton<Settings>.InstanceExists ? Singleton<Settings>.Instance.LookSensitivity : 1f), SmoothLookSpeed);
		Quaternion localRotation = ((Component)this).transform.localRotation;
		Vector3 eulerAngles = ((Quaternion)(ref localRotation)).eulerAngles;
		localRotation = ((Component)this).transform.localRotation;
		_ = ((Quaternion)(ref localRotation)).eulerAngles;
		if (Singleton<Settings>.InstanceExists && Singleton<Settings>.Instance.InvertMouse)
		{
			mouseY = 0f - mouseY;
		}
		eulerAngles.x -= Mathf.Clamp(mouseY, -89f, 89f);
		eulerAngles.y += mouseX;
		eulerAngles.z = 0f;
		if (eulerAngles.x >= 180f)
		{
			if (eulerAngles.x < 271f)
			{
				eulerAngles.x = 271f;
			}
		}
		else if (eulerAngles.x > 89f)
		{
			eulerAngles.x = 89f;
		}
		((Component)this).transform.localRotation = Quaternion.Euler(eulerAngles);
		((Component)this).transform.localEulerAngles = new Vector3(((Component)this).transform.localEulerAngles.x, ((Component)this).transform.localEulerAngles.y, 0f);
	}

	private void UpdateFreeCamInput()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.RoundToInt(GameInput.MotionAxis.x);
		int num2 = Mathf.RoundToInt(GameInput.MotionAxis.y);
		int num3 = 0;
		if (GameInput.GetButton(GameInput.ButtonCode.Jump))
		{
			num3 = 1;
		}
		else if (GameInput.GetButton(GameInput.ButtonCode.Crouch))
		{
			num3 = -1;
		}
		if (GameInput.IsTyping)
		{
			num = 0;
			num2 = 0;
			num3 = 0;
		}
		freeCamSpeed += Input.mouseScrollDelta.y * Time.deltaTime;
		freeCamSpeed = Mathf.Clamp(freeCamSpeed, 0f, 10f);
		freeCamMovement = new Vector3(Mathf.MoveTowards(freeCamMovement.x, (float)num, Time.unscaledDeltaTime * FreeCamAcceleration), Mathf.MoveTowards(freeCamMovement.y, (float)num3, Time.unscaledDeltaTime * FreeCamAcceleration), Mathf.MoveTowards(freeCamMovement.z, (float)num2, Time.unscaledDeltaTime * FreeCamAcceleration));
	}

	private void MoveFreeCam()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)this).transform;
		transform.position += ((Component)this).transform.TransformVector(freeCamMovement) * FreeCamSpeed * freeCamSpeed * Time.unscaledDeltaTime * (GameInput.GetButton(GameInput.ButtonCode.Sprint) ? 3f : 1f);
	}
}
