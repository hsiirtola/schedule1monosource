using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.FX;
using ScheduleOne.Map;
using ScheduleOne.Tools;
using ScheduleOne.UI;
using ScheduleOne.Vehicles;
using ScheduleOne.Vision;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace ScheduleOne.PlayerScripts;

public class PlayerMovement : PlayerSingleton<PlayerMovement>
{
	public const float DevSprintMultiplier = 1f;

	public const float WalkSpeed = 3.25f;

	public static float StaticMoveSpeedMultiplier = 1f;

	public const float InputSensitivity = 7f;

	public const float InputDeadZone = 0.001f;

	public const float SlipperyMovementMultiplier = 0.98f;

	public const float GroundedThreshold = 0.05f;

	public const float SlopeThreshold = 5f;

	public const float SlopeForce = 1f;

	public const float SlopeForceRayLength = 1.5f;

	public const float ControllerRadius = 0.35f;

	public const float DefaultCharacterControllerHeight = 1.85f;

	public const float CrouchHeightMultiplier = 0.65f;

	public const float CrouchTime = 0.2f;

	public const float CrouchSpeedMultipler = 0.6f;

	public const float CrouchedVigIntensity = 0.35f;

	public const float CrouchedVigSmoothness = 0.7f;

	public const bool SprintingRequiresStamina = false;

	public const float SprintChangeRate = 4f;

	public const float SprintMultiplier = 1.9f;

	public const float StaminaDrainRate = 12.5f;

	public const float StaminaRestoreRate = 25f;

	public const float StaminaRestoreDelay = 1f;

	public static float StaminaReserveMax = 100f;

	public const float JumpForce = 5.25f;

	public static float JumpMultiplier = 1f;

	public static float GravityMultiplier = 1f;

	public const float BaseGravityMultiplier = 1.4f;

	public const float VerticalLadderSpeedMultiplier = 1.2f;

	public const float LateralLadderSpeedMultiplier = 0.5f;

	public const float LadderTopBuffer = 0.15f;

	public const float LadderPitchAdjustment = 60f;

	public const float DismountForce = 7f;

	public const float DismountForceDuration = 0.5f;

	[Header("References")]
	public Player Player;

	public CharacterController Controller;

	[Header("Jump/fall settings")]
	[FormerlySerializedAs("groundDetectionMask")]
	public LayerMask GroundDetectionMask;

	public readonly FloatStack MoveSpeedMultiplierStack = new FloatStack(1f);

	public Action<float> onStaminaReserveChanged;

	public Action onJump;

	public Action onLand;

	public Action onCrouch;

	public Action onUncrouch;

	private Vector3 movement = Vector3.zero;

	private Vector3 lastFrameMovement = Vector3.zero;

	private float movementY;

	private float timeOnLadderDismount = -100f;

	private Vector3 ladderDismountDir = Vector3.zero;

	private float horizontalAxis;

	private float verticalAxis;

	private Dictionary<int, MotionEvent> movementEvents = new Dictionary<int, MotionEvent>();

	private float timeSinceStaminaDrain = 10000f;

	private bool sprintActive;

	private bool sprintReleased;

	private List<string> sprintBlockers = new List<string>();

	private Vector3 residualVelocityDirection = Vector3.zero;

	private float residualVelocityForce;

	private float residualVelocityDuration;

	private float residualVelocityTimeRemaining;

	private bool teleport;

	private Vector3 teleportPosition = Vector3.zero;

	private float playerLadderYPosOnLastClimbSound;

	private Coroutine playerRotCoroutine;

	public bool CanMove { get; set; } = true;

	public bool CanJump { get; set; } = true;

	public Vector3 Movement => movement;

	public bool IsJumping { get; private set; }

	public float TimeAirborne { get; private set; }

	public float TimeGrounded { get; private set; }

	public bool IsGrounded { get; private set; } = true;

	public bool IsCrouched { get; private set; }

	public float StandingScale { get; private set; } = 1f;

	public bool IsRagdolled { get; private set; }

	public bool IsSprinting { get; private set; }

	public bool ForceSprint { get; set; }

	public float CurrentStaminaReserve { get; private set; } = StaminaReserveMax;

	public float CurrentSprintMultiplier { get; private set; } = 1f;

	public LandVehicle CurrentVehicle { get; protected set; }

	public Ladder CurrentLadder { get; set; }

	public bool IsOnLadder => (Object)(object)CurrentLadder != (Object)null;

	public float MoveSpeedMultiplier => MoveSpeedMultiplierStack.Value;

	protected override void Awake()
	{
		base.Awake();
		Controller.detectCollisions = false;
	}

	protected override void Start()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected O, but got Unknown
		base.Start();
		Player local = Player.Local;
		local.onEnterVehicle = (Player.VehicleEvent)Delegate.Combine(local.onEnterVehicle, new Player.VehicleEvent(EnterVehicle));
		Player local2 = Player.Local;
		local2.onExitVehicle = (Player.VehicleTransformEvent)Delegate.Combine(local2.onExitVehicle, new Player.VehicleTransformEvent(ExitVehicle));
		Player.Local.Health.onRevive.AddListener((UnityAction)delegate
		{
			SetStamina(StaminaReserveMax, notify: false);
		});
	}

	private void Update()
	{
		UpdateHorizontalAxis();
		UpdateVerticalAxis();
		if (IsCrouched)
		{
			StandingScale = Mathf.MoveTowards(StandingScale, 0f, Time.deltaTime / 0.2f);
		}
		else
		{
			StandingScale = Mathf.MoveTowards(StandingScale, 1f, Time.deltaTime / 0.2f);
		}
		UpdatePlayerHeight();
		if (residualVelocityTimeRemaining > 0f)
		{
			residualVelocityTimeRemaining -= Time.deltaTime;
		}
		timeSinceStaminaDrain += Time.deltaTime;
		if (timeSinceStaminaDrain > 1f && CurrentStaminaReserve < StaminaReserveMax)
		{
			ChangeStamina(25f * Time.deltaTime);
		}
		Move();
		UpdateCrouchVignetteEffect();
		UpdateMovementEvents();
	}

	private void FixedUpdate()
	{
		IsGrounded = GetIsGrounded();
	}

	private void LateUpdate()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (teleport)
		{
			((Collider)Controller).enabled = false;
			((Component)Controller).transform.position = teleportPosition;
			((Collider)Controller).enabled = true;
			teleport = false;
		}
	}

	private void Move()
	{
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0539: Unknown result type (might be due to invalid IL or missing references)
		//IL_053e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04da: Unknown result type (might be due to invalid IL or missing references)
		//IL_04df: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0429: Unknown result type (might be due to invalid IL or missing references)
		//IL_0448: Unknown result type (might be due to invalid IL or missing references)
		//IL_044d: Unknown result type (might be due to invalid IL or missing references)
		//IL_045a: Unknown result type (might be due to invalid IL or missing references)
		//IL_045f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0464: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0609: Unknown result type (might be due to invalid IL or missing references)
		//IL_060f: Unknown result type (might be due to invalid IL or missing references)
		//IL_061a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0631: Unknown result type (might be due to invalid IL or missing references)
		//IL_063b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0640: Unknown result type (might be due to invalid IL or missing references)
		//IL_0645: Unknown result type (might be due to invalid IL or missing references)
		//IL_0658: Unknown result type (might be due to invalid IL or missing references)
		//IL_067e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0688: Unknown result type (might be due to invalid IL or missing references)
		//IL_068d: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_06de: Unknown result type (might be due to invalid IL or missing references)
		//IL_076c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0776: Unknown result type (might be due to invalid IL or missing references)
		//IL_077b: Unknown result type (might be due to invalid IL or missing references)
		//IL_071f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0729: Unknown result type (might be due to invalid IL or missing references)
		//IL_0733: Unknown result type (might be due to invalid IL or missing references)
		//IL_0739: Unknown result type (might be due to invalid IL or missing references)
		//IL_073e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0747: Unknown result type (might be due to invalid IL or missing references)
		//IL_0751: Unknown result type (might be due to invalid IL or missing references)
		//IL_0756: Unknown result type (might be due to invalid IL or missing references)
		//IL_0758: Unknown result type (might be due to invalid IL or missing references)
		//IL_075d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0783: Unknown result type (might be due to invalid IL or missing references)
		//IL_0788: Unknown result type (might be due to invalid IL or missing references)
		IsSprinting = false;
		if (!((Collider)Controller).enabled)
		{
			CurrentSprintMultiplier = Mathf.MoveTowards(CurrentSprintMultiplier, 1f, Time.deltaTime * 4f);
		}
		else
		{
			if ((Object)(object)CurrentVehicle != (Object)null)
			{
				return;
			}
			if (IsGrounded)
			{
				TimeGrounded += Time.deltaTime;
			}
			else
			{
				TimeGrounded = 0f;
			}
			if (CanMove && !GameInput.IsTyping && !Singleton<PauseMenu>.Instance.IsPaused && GameInput.GetButtonDown(GameInput.ButtonCode.Crouch) && !UIScreenManager.IsBackTriggeredThisFrame)
			{
				TryToggleCrouch();
			}
			if (IsOnLadder)
			{
				CurrentSprintMultiplier = Mathf.MoveTowards(CurrentSprintMultiplier, 1f, Time.deltaTime * 4f);
				LadderMove();
				if (!GetIsGrounded())
				{
					movement = Vector3.zero;
					if (GameInput.GetButtonDown(GameInput.ButtonCode.Jump))
					{
						ladderDismountDir = -CurrentLadder.LadderTransform.forward;
						timeOnLadderDismount = Time.timeSinceLevelLoad;
						CurrentLadder.PlayClimbSound(Player.CenterPointTransform.position);
						DismountLadder();
						IsJumping = true;
						if (onJump != null)
						{
							onJump();
						}
					}
					return;
				}
			}
			if (CanMove && CanJump && (IsGrounded || IsOnLadder) && !IsJumping && !GameInput.IsTyping && !Singleton<PauseMenu>.Instance.IsPaused && GameInput.GetButtonDown(GameInput.ButtonCode.Jump))
			{
				if (!IsCrouched)
				{
					IsJumping = true;
					if (onJump != null)
					{
						onJump();
					}
					Player.Local.PlayJumpAnimation();
					Jump();
				}
				else
				{
					TryToggleCrouch();
				}
			}
			if (!IsGrounded)
			{
				TimeAirborne += Time.deltaTime;
			}
			else
			{
				IsJumping = false;
				if (TimeAirborne > 0.1f && onLand != null)
				{
					onLand();
				}
				TimeAirborne = 0f;
			}
			if (GameInput.GetButtonDown(GameInput.ButtonCode.Sprint) && !sprintActive)
			{
				sprintActive = true;
				sprintReleased = false;
			}
			else if (GameInput.GetButton(GameInput.ButtonCode.Sprint) && Singleton<Settings>.Instance.SprintMode == InputSettings.EActionMode.Hold)
			{
				sprintActive = true;
			}
			else if (Singleton<Settings>.Instance.SprintMode == InputSettings.EActionMode.Hold)
			{
				sprintActive = false;
			}
			if (!GameInput.GetButton(GameInput.ButtonCode.Sprint))
			{
				sprintReleased = true;
			}
			if (GameInput.GetButtonDown(GameInput.ButtonCode.Sprint) && sprintReleased)
			{
				sprintActive = !sprintActive;
			}
			if (ForceSprint)
			{
				sprintActive = true;
			}
			IsSprinting = false;
			if (sprintActive && CanMove && !IsCrouched && !Player.Local.IsTased && (horizontalAxis != 0f || verticalAxis != 0f) && sprintBlockers.Count == 0 && !IsOnLadder)
			{
				_ = CurrentStaminaReserve;
				_ = 0f;
				CurrentSprintMultiplier = Mathf.MoveTowards(CurrentSprintMultiplier, 1.9f, Time.deltaTime * 4f);
				IsSprinting = true;
			}
			else
			{
				sprintActive = false;
				CurrentSprintMultiplier = Mathf.MoveTowards(CurrentSprintMultiplier, 1f, Time.deltaTime * 4f);
			}
			if (!IsSprinting && timeSinceStaminaDrain > 1f)
			{
				CurrentSprintMultiplier = Mathf.MoveTowards(CurrentSprintMultiplier, 1f, Time.deltaTime * 4f);
			}
			float num = 1f;
			if (IsCrouched)
			{
				num = 1f - 0.39999998f * (1f - StandingScale);
			}
			float num2 = 3.25f * CurrentSprintMultiplier * num * StaticMoveSpeedMultiplier * MoveSpeedMultiplier;
			if (Player.Local.IsTased)
			{
				num2 *= 0.5f;
			}
			if ((Application.isEditor || Debug.isDebugBuild) && IsSprinting)
			{
				num2 *= 1f;
			}
			if (Controller.isGrounded)
			{
				if (CanMove)
				{
					_ = movement;
					movement = new Vector3(horizontalAxis, 0f - Controller.stepOffset, verticalAxis);
					movement = ((Component)this).transform.TransformDirection(movement);
					ClampMovement();
					movement.x *= num2;
					movement.z *= num2;
				}
				else
				{
					movement = new Vector3(0f, 0f - Controller.stepOffset, 0f);
				}
			}
			else if (CanMove)
			{
				movement = new Vector3(horizontalAxis, movement.y, verticalAxis);
				movement = ((Component)this).transform.TransformDirection(movement);
				ClampMovement();
				movement.x *= num2;
				movement.z *= num2;
			}
			else
			{
				movement = new Vector3(0f, movement.y, 0f);
			}
			if (!CanMove)
			{
				movement.x = Mathf.MoveTowards(movement.x, 0f, 7f * Time.deltaTime);
				movement.z = Mathf.MoveTowards(movement.z, 0f, 7f * Time.deltaTime);
			}
			if ((Object)(object)CurrentLadder == (Object)null)
			{
				movement.y += Physics.gravity.y * 1.4f * Time.deltaTime * GravityMultiplier;
			}
			movement.y += movementY;
			movementY = 0f;
			if (residualVelocityTimeRemaining > 0f)
			{
				movement += residualVelocityDirection * residualVelocityForce * Mathf.Clamp01(residualVelocityTimeRemaining / residualVelocityDuration) * Time.deltaTime;
			}
			if (Player.Local.Slippery)
			{
				movement = Vector3.Lerp(movement, new Vector3(lastFrameMovement.x, movement.y, lastFrameMovement.z), 0.98f);
			}
			if (Time.timeSinceLevelLoad - timeOnLadderDismount < 0.5f)
			{
				movement += ladderDismountDir * 7f * (1f - (Time.timeSinceLevelLoad - timeOnLadderDismount) / 0.5f);
			}
			float surfaceAngle = GetSurfaceAngle();
			if ((horizontalAxis != 0f || verticalAxis != 0f) && surfaceAngle > 5f)
			{
				float num3 = Mathf.Clamp01(surfaceAngle / Controller.slopeLimit);
				Vector3 val = Vector3.down * Time.deltaTime * 1f * num3;
				Controller.Move(movement * Time.deltaTime + val);
			}
			else
			{
				Controller.Move(movement * Time.deltaTime);
			}
			lastFrameMovement = movement;
		}
	}

	private void ClampMovement()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		float y = movement.y;
		movement = Vector3.ClampMagnitude(new Vector3(movement.x, 0f, movement.z), 1f);
		movement.y = y;
	}

	private float GetSurfaceAngle()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(((Component)this).transform.position, Vector3.down, ref val, 1.5f, LayerMask.op_Implicit(GroundDetectionMask), (QueryTriggerInteraction)1))
		{
			return Vector3.Angle(((RaycastHit)(ref val)).normal, Vector3.up);
		}
		return 0f;
	}

	private bool GetIsGrounded()
	{
		return Player.Local.GetIsGrounded();
	}

	public unsafe void Teleport(Vector3 position, bool alignFeetToPosition = false)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		if (float.IsNaN(position.x) || float.IsNaN(position.y) || float.IsNaN(position.z))
		{
			Console.LogError("Tried to teleport player to NaN position!");
			return;
		}
		Vector3 val = position;
		Console.Log("Player teleported: " + ((object)(*(Vector3*)(&val))/*cast due to .constrained prefix*/).ToString());
		if ((Object)(object)Player.ActiveSkateboard != (Object)null)
		{
			Player.ActiveSkateboard.Equippable.Dismount();
		}
		((Collider)Controller).enabled = false;
		if (alignFeetToPosition)
		{
			position.y += Controller.height * 0.5f;
		}
		((Component)Controller).transform.position = position;
		((Collider)Controller).enabled = true;
		teleport = true;
		teleportPosition = position;
	}

	public void SetResidualVelocity(Vector3 dir, float force, float time)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		residualVelocityDirection = ((Vector3)(ref dir)).normalized;
		residualVelocityForce = force;
		residualVelocityDuration = time;
		residualVelocityTimeRemaining = time;
	}

	public void WarpToNavMesh(bool clearVelocity = false)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		NavMeshQueryFilter val = default(NavMeshQueryFilter);
		((NavMeshQueryFilter)(ref val)).agentTypeID = Singleton<PlayerManager>.Instance.PlayerRecoverySurface.agentTypeID;
		((NavMeshQueryFilter)(ref val)).areaMask = -1;
		NavMeshHit val2 = default(NavMeshHit);
		if (NavMesh.SamplePosition(((Component)PlayerSingleton<PlayerMovement>.Instance).transform.position, ref val2, 100f, val))
		{
			Teleport(((NavMeshHit)(ref val2)).position + Vector3.up * 0.2f, alignFeetToPosition: true);
		}
		else
		{
			Console.LogError("Failed to find recovery point!");
			Teleport(Vector3.zero + Vector3.up * 5f);
		}
		if (clearVelocity)
		{
			movement = Vector3.zero;
			movementY = 0f;
			residualVelocityForce = 0f;
			residualVelocityTimeRemaining = 0f;
		}
	}

	private void UpdateHorizontalAxis()
	{
		if (Singleton<PauseMenu>.Instance.IsPaused)
		{
			horizontalAxis = 0f;
			return;
		}
		int num = ((!GameInput.IsTyping) ? Mathf.RoundToInt(GameInput.MotionAxis.x) : 0);
		if (Player.Disoriented)
		{
			num = -num;
		}
		if (Player.Schizophrenic && Time.timeSinceLevelLoad % 20f < 1f)
		{
			num = -num;
		}
		float num2 = Mathf.MoveTowards(horizontalAxis, (float)num, 7f * Time.deltaTime);
		horizontalAxis = ((Mathf.Abs(num2) < 0.001f) ? 0f : num2);
	}

	private void UpdateVerticalAxis()
	{
		if (Singleton<PauseMenu>.Instance.IsPaused)
		{
			verticalAxis = 0f;
			return;
		}
		int num = ((!GameInput.IsTyping) ? Mathf.RoundToInt(GameInput.MotionAxis.y) : 0);
		if (Player.Schizophrenic && (Time.timeSinceLevelLoad + 5f) % 25f < 1f)
		{
			num = -num;
		}
		float num2 = Mathf.MoveTowards(verticalAxis, (float)num, 7f * Time.deltaTime);
		verticalAxis = ((Mathf.Abs(num2) < 0.001f) ? 0f : num2);
	}

	public void Jump()
	{
		((MonoBehaviour)this).StartCoroutine(JumpRoutine());
		IEnumerator JumpRoutine()
		{
			float savedSlopeLimit = Controller.slopeLimit;
			Vector3 velocity = Controller.velocity;
			((Vector3)(ref velocity)).Set(Controller.velocity.x, 0f, Controller.velocity.y);
			movementY += 5.25f * JumpMultiplier;
			TimeGrounded = 0f;
			do
			{
				yield return (object)new WaitForEndOfFrame();
			}
			while (TimeGrounded < 0.05f && (int)Controller.collisionFlags != 2 && (Object)(object)CurrentVehicle == (Object)null);
			Controller.slopeLimit = savedSlopeLimit;
		}
	}

	public void SetCrouched(bool c)
	{
		IsCrouched = c;
		Player.SendCrouched(IsCrouched);
		Player.SetCrouchedLocal(IsCrouched);
		VisibilityAttribute attribute = Player.Local.Visibility.GetAttribute("Crouched");
		if (IsCrouched)
		{
			if (attribute == null)
			{
				attribute = new VisibilityAttribute("Crouched", 0f, 0.8f, 1);
			}
		}
		else
		{
			attribute?.Delete();
		}
	}

	private void TryToggleCrouch()
	{
		if (IsCrouched)
		{
			if (CanStand())
			{
				SetCrouched(c: false);
			}
		}
		else
		{
			SetCrouched(c: true);
		}
	}

	private bool CanStand()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		float num = Controller.radius * 0.75f;
		float num2 = 0.1f;
		Vector3 val = ((Component)this).transform.position - Vector3.up * Controller.height * 0.5f + Vector3.up * num + Vector3.up * num2;
		float num3 = 1.85f - num * 2f - num2;
		RaycastHit val2 = default(RaycastHit);
		if (Physics.SphereCast(val, num, Vector3.up, ref val2, num3, LayerMask.op_Implicit(GroundDetectionMask)))
		{
			return false;
		}
		return true;
	}

	private void UpdateCrouchVignetteEffect()
	{
		if (Singleton<PostProcessingManager>.InstanceExists)
		{
			float intensity = Mathf.Lerp(0.35f, Singleton<PostProcessingManager>.Instance.Vig_DefaultIntensity, StandingScale);
			float smoothness = Mathf.Lerp(0.7f, Singleton<PostProcessingManager>.Instance.Vig_DefaultSmoothness, StandingScale);
			Singleton<PostProcessingManager>.Instance.OverrideVignette(intensity, smoothness);
		}
	}

	private void UpdatePlayerHeight()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		float height = Controller.height;
		Controller.height = 1.85f - 0.64750004f * (1f - StandingScale);
		float num = Controller.height - height;
		if (IsGrounded && Mathf.Abs(num) > 1E-05f)
		{
			Controller.Move(Vector3.up * num * 0.5f);
		}
	}

	public void LerpPlayerRotation(Quaternion rotation, float lerpTime)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (playerRotCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(playerRotCoroutine);
		}
		playerRotCoroutine = ((MonoBehaviour)this).StartCoroutine(LerpPlayerRotation_Process(rotation, lerpTime));
	}

	private IEnumerator LerpPlayerRotation_Process(Quaternion endRotation, float lerpTime)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Quaternion startRot = ((Component)Player).transform.rotation;
		((Collider)Controller).enabled = false;
		for (float i = 0f; i < lerpTime; i += Time.deltaTime)
		{
			((Component)Player).transform.rotation = Quaternion.Lerp(startRot, endRotation, i / lerpTime);
			yield return (object)new WaitForEndOfFrame();
		}
		((Component)Player).transform.rotation = endRotation;
		((Collider)Controller).enabled = true;
		playerRotCoroutine = null;
	}

	public void SetPlayerRotation(Quaternion rotation)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		rotation = Quaternion.Euler(0f, ((Quaternion)(ref rotation)).eulerAngles.y, 0f);
		((Collider)Controller).enabled = false;
		((Component)Player).transform.rotation = rotation;
		((Collider)Controller).enabled = true;
	}

	private void EnterVehicle(LandVehicle vehicle)
	{
		CurrentVehicle = vehicle;
		CanMove = false;
		((Collider)Controller).enabled = false;
	}

	private void ExitVehicle(LandVehicle veh, Transform exitPoint)
	{
		CurrentVehicle = null;
		CanMove = true;
		((Collider)Controller).enabled = true;
	}

	public void RegisterMovementEvent(int threshold, Action action)
	{
		if (threshold < 1)
		{
			Console.LogWarning("Movement events min. threshold is 1m!");
			return;
		}
		if (!movementEvents.ContainsKey(threshold))
		{
			movementEvents.Add(threshold, new MotionEvent());
		}
		movementEvents[threshold].Actions.Add(action);
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
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		foreach (int item in movementEvents.Keys.ToList())
		{
			MotionEvent motionEvent = movementEvents[item];
			if (Vector3.Distance(Player.Avatar.CenterPoint, motionEvent.LastUpdatedDistance) > (float)item)
			{
				motionEvent.Update(Player.Avatar.CenterPoint);
			}
		}
	}

	public void ChangeStamina(float change, bool notify = true)
	{
		if (change < 0f)
		{
			timeSinceStaminaDrain = 0f;
		}
		SetStamina(CurrentStaminaReserve + change, notify);
	}

	public void SetStamina(float value, bool notify = true)
	{
		if (CurrentStaminaReserve != value)
		{
			float currentStaminaReserve = CurrentStaminaReserve;
			CurrentStaminaReserve = Mathf.Clamp(value, 0f, StaminaReserveMax);
			if (notify && onStaminaReserveChanged != null)
			{
				onStaminaReserveChanged(CurrentStaminaReserve - currentStaminaReserve);
			}
		}
	}

	public void AddSprintBlocker(string tag)
	{
		if (!sprintBlockers.Contains(tag))
		{
			sprintBlockers.Add(tag);
		}
	}

	public void RemoveSprintBlocker(string tag)
	{
		if (sprintBlockers.Contains(tag))
		{
			sprintBlockers.Remove(tag);
		}
	}

	public void MountLadder(Ladder ladder)
	{
		if (!((Object)(object)ladder == (Object)null) && !((Object)(object)ladder == (Object)(object)CurrentLadder))
		{
			if ((Object)(object)CurrentLadder != (Object)null)
			{
				CurrentLadder = null;
			}
			CurrentLadder = ladder;
			AddSprintBlocker("Climbing");
			PlayLadderClimbSound();
		}
	}

	public void DismountLadder()
	{
		RemoveSprintBlocker("Climbing");
		if ((Object)(object)CurrentLadder != (Object)null)
		{
			CurrentLadder = null;
		}
	}

	private void LadderMove()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		Vector3 zero = Vector3.zero;
		float num = Mathf.Abs(Vector3.SignedAngle(-CurrentLadder.LadderTransform.forward, ((Component)this).transform.forward, Vector3.up));
		zero += ((Component)this).transform.right * horizontalAxis;
		float num2 = 0f - Vector3.SignedAngle(((Component)this).transform.forward, ((Component)PlayerSingleton<PlayerCamera>.Instance.Camera).transform.forward, ((Component)this).transform.right);
		float num3 = num2 + Mathf.Lerp(-60f, 60f, num / 180f);
		zero += ((Component)CurrentLadder).transform.up * Mathf.Clamp(num3 / 60f, -1f, 1f) * verticalAxis;
		zero += ((Component)this).transform.forward * verticalAxis * (1f - Mathf.Abs(num2) / 90f);
		new Plane(CurrentLadder.LadderTransform.forward, CurrentLadder.LadderTransform.position);
		Vector3 val = CurrentLadder.LadderTransform.InverseTransformDirection(zero);
		val.x *= 0.5f;
		val.y *= 1.2f;
		float num4 = 0f - ((Component)CurrentLadder).transform.InverseTransformPoint(((Component)this).transform.position).z;
		float num5 = Controller.radius + 0.05f;
		if (num4 > num5)
		{
			val.z = 0f;
		}
		Vector2 val2 = CurrentLadder.ProjectOnLadderSurface(Player.PlayerBasePosition);
		if (val2.y >= CurrentLadder.LadderSize.y + 0.15f)
		{
			val.y = Mathf.Min(0f, val.y);
		}
		else if (val2.y <= 0f)
		{
			val.y = Mathf.Max(0f, val.y);
		}
		float num6 = (IsCrouched ? (1f - 0.39999998f * (1f - StandingScale)) : 1f);
		float num7 = 3.25f * num6 * StaticMoveSpeedMultiplier * MoveSpeedMultiplier;
		Vector3 val3 = CurrentLadder.LadderTransform.TransformDirection(val);
		Controller.Move(val3 * num7 * Time.deltaTime);
		if (Mathf.Abs(val2.y - playerLadderYPosOnLastClimbSound) >= 0.8f)
		{
			PlayLadderClimbSound();
		}
	}

	private void PlayLadderClimbSound()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		playerLadderYPosOnLastClimbSound = CurrentLadder.ProjectOnLadderSurface(Player.PlayerBasePosition).y;
		CurrentLadder.PlayClimbSound(Player.CenterPointTransform.position);
	}
}
