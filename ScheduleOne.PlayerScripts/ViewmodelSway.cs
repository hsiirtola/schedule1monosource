using System;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.PlayerScripts;

public class ViewmodelSway : PlayerSingleton<ViewmodelSway>
{
	public bool DEBUG;

	[Header("Settings - Breathing")]
	public bool breatheBobbingEnabled = true;

	[Range(0f, 0.0004f)]
	[SerializeField]
	protected float breathingHeightMultiplier = 5E-05f;

	[Range(0f, 10f)]
	[SerializeField]
	protected float breathingSpeedMultiplier = 1f;

	private float lastHeight;

	private Vector3 breatheBobPos;

	[Header("Settings - Sway - Movement")]
	public bool swayingEnabled = true;

	[Range(0f, 0.1f)]
	[SerializeField]
	protected float horizontalSwayMultiplier = 1f;

	[Range(0f, 0.1f)]
	[SerializeField]
	protected float verticalSwayMultiplier = 1f;

	[Range(0f, 0.5f)]
	[SerializeField]
	protected float maxHorizontal = 0.1f;

	[Range(0f, 0.5f)]
	[SerializeField]
	protected float maxVertical = 0.1f;

	[SerializeField]
	protected float swaySmooth = 3f;

	[SerializeField]
	protected float returnMultiplier = 0.1f;

	private Vector3 initialPos = Vector3.zero;

	private Vector3 swayPos;

	[Header("Settings - Walk Bob")]
	public bool walkBobbingEnabled = true;

	[SerializeField]
	protected AnimationCurve verticalMovement;

	[SerializeField]
	protected AnimationCurve horizontalMovement;

	[Range(0f, 0.1f)]
	[SerializeField]
	protected float verticalBobHeight = 0.1f;

	[Range(0f, 5f)]
	[SerializeField]
	protected float verticalBobSpeed = 2f;

	[Range(0f, 0.1f)]
	[SerializeField]
	protected float horizontalBobWidth = 0.1f;

	[Range(0f, 5f)]
	[SerializeField]
	protected float horizontalBobSpeed = 2f;

	[SerializeField]
	protected float walkBobSmooth = 3f;

	[SerializeField]
	protected float sprintSpeedMultiplier = 1.25f;

	[HideInInspector]
	public float walkBobMultiplier = 1f;

	private Vector3 walkBobPos;

	private float timeSinceWalkStart_vert;

	private float timeSinceWalkStart_horiz;

	[Header("Settings - Jump Jolt")]
	public bool jumpJoltEnabled = true;

	[SerializeField]
	protected AnimationCurve jumpCurve;

	[SerializeField]
	protected float jumpJoltTime = 0.6f;

	[SerializeField]
	protected float jumpJoltHeight = 0.2f;

	[SerializeField]
	protected float jumpJoltSmooth = 5f;

	[Header("Settings - Equip Bop")]
	[SerializeField]
	protected float equipBopVerticalOffset = -0.5f;

	[SerializeField]
	protected float equipBopTime = 0.2f;

	private Vector3 equipBopPos;

	private float timeSinceJumpStart;

	private Vector3 jumpPos = Vector3.zero;

	[Header("Settings - Falling")]
	[Range(0f, 1f)]
	[SerializeField]
	protected float fallOffsetRate = 0.1f;

	[Range(0f, 2f)]
	[SerializeField]
	protected float maxFallOffsetAmount = 0.2f;

	private Vector3 fallOffsetPos = Vector3.zero;

	[Header("Settings - Land Jolt")]
	[SerializeField]
	protected AnimationCurve landCurve;

	[SerializeField]
	protected float landJoltTime = 0.6f;

	[SerializeField]
	protected float landJoltSmooth = 5f;

	private Vector3 landPos = Vector3.zero;

	private float timeSinceLanded;

	private float landJoltMultiplier = 1f;

	protected float calculatedJumpJoltHeight => jumpJoltHeight;

	protected override void Start()
	{
		base.Start();
	}

	protected override void Awake()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		initialPos = ((Component)this).transform.localPosition;
	}

	public override void OnStartClient(bool IsOwner)
	{
		base.OnStartClient(IsOwner);
		timeSinceLanded = landJoltTime;
		PlayerMovement playerMovement = PlayerSingleton<PlayerMovement>.Instance;
		playerMovement.onJump = (Action)Delegate.Combine(playerMovement.onJump, new Action(StartJump));
		PlayerMovement playerMovement2 = PlayerSingleton<PlayerMovement>.Instance;
		playerMovement2.onLand = (Action)Delegate.Combine(playerMovement2.onLand, new Action(Land));
		PlayerInventory playerInventory = PlayerSingleton<PlayerInventory>.Instance;
		playerInventory.onInventoryStateChanged = (Action<bool>)Delegate.Combine(playerInventory.onInventoryStateChanged, new Action<bool>(InventoryStateChanged));
		PlayerInventory playerInventory2 = PlayerSingleton<PlayerInventory>.Instance;
		playerInventory2.onEquippedSlotChanged = (Action<int>)Delegate.Combine(playerInventory2.onEquippedSlotChanged, new Action<int>(OnEquippedSlotChanged));
	}

	protected void Update()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		if (Time.timeScale != 0f)
		{
			if (breatheBobbingEnabled)
			{
				BreatheBob();
			}
			if (swayingEnabled)
			{
				Sway();
			}
			if (walkBobbingEnabled)
			{
				WalkBob();
			}
			if (jumpJoltEnabled)
			{
				UpdateJump();
			}
			_ = landPos;
			if (PlayerSingleton<PlayerInventory>.Instance.currentEquipTime < equipBopTime)
			{
				equipBopPos = new Vector3(0f, equipBopVerticalOffset * (1f - Mathf.Sqrt(Mathf.Clamp(PlayerSingleton<PlayerInventory>.Instance.currentEquipTime / equipBopTime, 0f, 1f))), 0f);
			}
			else
			{
				equipBopPos = Vector3.zero;
			}
			if (!PlayerSingleton<PlayerInventory>.Instance.HotbarEnabled)
			{
				equipBopPos = new Vector3(0f, equipBopVerticalOffset, 0f);
			}
			if (!PlayerSingleton<PlayerInventory>.Instance.isAnythingEquipped)
			{
				equipBopPos = Vector3.zero;
				breatheBobPos.y = 0f;
			}
			RefreshViewmodel();
		}
	}

	private void InventoryStateChanged(bool active)
	{
		if (active)
		{
			Update();
		}
	}

	private void OnEquippedSlotChanged(int slotIndex)
	{
		Update();
	}

	public void RefreshViewmodel()
	{
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		if (DEBUG)
		{
			Console.Log("Viewmodel position breakdown: \nSway" + ((Vector3)(ref swayPos)).ToString("F6") + "\nBreatheBob" + ((Vector3)(ref breatheBobPos)).ToString("F6") + "\nWalkBob" + ((Vector3)(ref walkBobPos)).ToString("F6") + "\nJump" + ((Vector3)(ref jumpPos)).ToString("F6") + "\nLand" + ((Vector3)(ref landPos)).ToString("F6") + "\nFallOffset" + ((Vector3)(ref fallOffsetPos)).ToString("F6") + "\nEquipBop" + ((Vector3)(ref equipBopPos)).ToString("F6"));
		}
		try
		{
			((Component)this).transform.localPosition = swayPos + breatheBobPos + walkBobPos + jumpPos + landPos + fallOffsetPos + equipBopPos;
		}
		catch
		{
			Console.LogWarning("Viewmodel pos set failed.");
		}
	}

	protected void BreatheBob()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		lastHeight = breatheBobPos.y + (Mathf.Sin(Time.timeSinceLevelLoad * breathingSpeedMultiplier) - lastHeight) * breathingHeightMultiplier;
		breatheBobPos = new Vector3(0f, lastHeight, 0f);
	}

	protected void Sway()
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		float x = swayPos.x;
		float y = swayPos.y;
		float num = 0f;
		float num2 = 0f;
		if (PlayerSingleton<PlayerCamera>.Instance.canLook)
		{
			num = x - GameInput.MouseDelta.x * horizontalSwayMultiplier;
			num2 = y - GameInput.MouseDelta.y * verticalSwayMultiplier;
		}
		num = Mathf.Clamp(num, 0f - maxHorizontal, maxHorizontal);
		num2 = Mathf.Clamp(num2, 0f - maxVertical, maxVertical);
		Vector3 val = Vector3.Lerp(new Vector3(num, num2, 0f), Vector3.zero, Time.deltaTime * returnMultiplier / (1f + Mathf.Sqrt(Mathf.Abs(GameInput.MouseDelta.x) + Mathf.Abs(GameInput.MouseDelta.y))));
		swayPos = Vector3.Lerp(swayPos, val + initialPos, Time.deltaTime * swaySmooth);
	}

	protected void WalkBob()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		float num = Mathf.Abs(PlayerSingleton<PlayerMovement>.Instance.Movement.x) + Mathf.Abs(PlayerSingleton<PlayerMovement>.Instance.Movement.z);
		if (Mathf.Abs(PlayerSingleton<PlayerMovement>.Instance.Movement.x) > 0f || Mathf.Abs(PlayerSingleton<PlayerMovement>.Instance.Movement.z) > 0f)
		{
			flag = true;
		}
		if (!flag)
		{
			timeSinceWalkStart_vert = 0f;
			timeSinceWalkStart_horiz = 0f;
		}
		float num2 = 1f;
		if (PlayerSingleton<PlayerMovement>.Instance.IsSprinting)
		{
			num2 = 1.4f;
		}
		walkBobPos = Vector3.Lerp(walkBobPos, new Vector3(horizontalMovement.Evaluate(timeSinceWalkStart_horiz % 1f) * horizontalBobWidth * num2, verticalMovement.Evaluate(timeSinceWalkStart_vert % 1f) * verticalBobHeight * num2, 0f) * num, Time.deltaTime * walkBobSmooth);
		if (flag)
		{
			float num3 = 1f;
			if (PlayerSingleton<PlayerMovement>.Instance.IsSprinting)
			{
				num3 = 1.6f;
			}
			timeSinceWalkStart_vert += Time.deltaTime * verticalBobSpeed * num3;
			timeSinceWalkStart_horiz += Time.deltaTime * horizontalBobSpeed * num3;
		}
	}

	protected void StartJump()
	{
		timeSinceJumpStart = 0f;
	}

	protected void UpdateJump()
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		if (!PlayerSingleton<PlayerInventory>.Instance.isAnythingEquipped || !PlayerSingleton<PlayerInventory>.Instance.HotbarEnabled)
		{
			return;
		}
		if (PlayerSingleton<PlayerMovement>.Instance.TimeAirborne > 0f)
		{
			timeSinceJumpStart += Time.deltaTime;
			Vector3 val = default(Vector3);
			((Vector3)(ref val))._002Ector(0f, jumpCurve.Evaluate(Mathf.Clamp(timeSinceJumpStart / jumpJoltTime, 0f, 1f)) * calculatedJumpJoltHeight, 0f);
			jumpPos = Vector3.Lerp(jumpPos, val, Time.deltaTime * jumpJoltSmooth);
		}
		else if (PlayerSingleton<PlayerMovement>.Instance.IsGrounded)
		{
			timeSinceJumpStart = 0f;
			Vector3 val2 = default(Vector3);
			((Vector3)(ref val2))._002Ector(0f, landCurve.Evaluate(Mathf.Clamp(timeSinceLanded / landJoltTime, 0f, 1f)) * landJoltMultiplier, 0f);
			if (landJoltMultiplier > 0f)
			{
				landPos = Vector3.Lerp(landPos, val2, Mathf.Abs(Time.deltaTime * landJoltSmooth / landJoltMultiplier));
			}
			else
			{
				landPos = Vector3.zero;
			}
			timeSinceLanded += Time.deltaTime;
			Vector3 zero = Vector3.zero;
			jumpPos = Vector3.Lerp(jumpPos, zero, Time.deltaTime * jumpJoltSmooth);
		}
		if (!PlayerSingleton<PlayerMovement>.Instance.IsGrounded && (timeSinceJumpStart > jumpJoltTime || PlayerSingleton<PlayerMovement>.Instance.TimeAirborne == 0f))
		{
			fallOffsetPos.y += fallOffsetRate * Time.deltaTime;
			fallOffsetPos.y = Mathf.Clamp(fallOffsetPos.y, 0f, maxFallOffsetAmount);
		}
		else
		{
			fallOffsetPos.y = 0f;
		}
	}

	protected void Land()
	{
		landJoltMultiplier = jumpPos.y + fallOffsetPos.y + landPos.y;
		landPos.y = landCurve.Evaluate(Mathf.Clamp(0f / landJoltTime, 0f, 1f)) * landJoltMultiplier;
		timeSinceLanded = 0f;
		jumpPos.y = 0f;
		fallOffsetPos.y = 0f;
	}
}
