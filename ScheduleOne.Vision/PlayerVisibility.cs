using ScheduleOne.DevUtilities;
using ScheduleOne.Law;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Vision;

public class PlayerVisibility : EntityVisibility
{
	private Player player;

	private bool disobeyingCurfewStateApplied;

	private bool NetworkInitialize___EarlyScheduleOne_002EVision_002EPlayerVisibilityAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EVision_002EPlayerVisibilityAssembly_002DCSharp_002Edll_Excuted;

	public override float Suspiciousness
	{
		get
		{
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			float num = 0f;
			if (player.Avatar.Animation.IsCrouched)
			{
				num += 0.3f;
			}
			if ((Object)(object)player.Avatar.CurrentEquippable != (Object)null)
			{
				num += player.Avatar.CurrentEquippable.Suspiciousness;
			}
			Vector3 velocity = player.VelocityCalculator.Velocity;
			if (((Vector3)(ref velocity)).magnitude > 3.25f)
			{
				float num2 = num;
				velocity = player.VelocityCalculator.Velocity;
				num = num2 + 0.3f * Mathf.InverseLerp(3.25f, 6.1749997f, ((Vector3)(ref velocity)).magnitude);
			}
			return Mathf.Clamp01(num);
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EVision_002EPlayerVisibility_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	private void Update()
	{
		if (NetworkSingleton<CurfewManager>.InstanceExists && NetworkSingleton<CurfewManager>.Instance.IsHardCurfewActive)
		{
			if (!disobeyingCurfewStateApplied)
			{
				AddFlag_DisobeyingCurfew();
			}
		}
		else if (disobeyingCurfewStateApplied)
		{
			RemoveFlag_DisobeyingCurfew();
		}
	}

	private void AddFlag_DisobeyingCurfew()
	{
		disobeyingCurfewStateApplied = true;
		ApplyState("DisobeyingCurfew", EVisualState.DisobeyingCurfew);
	}

	private void RemoveFlag_DisobeyingCurfew()
	{
		disobeyingCurfewStateApplied = false;
		RemoveState("DisobeyingCurfew");
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002EVision_002EPlayerVisibilityAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EVision_002EPlayerVisibilityAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EVision_002EPlayerVisibilityAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EVision_002EPlayerVisibilityAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	protected override void Awake_UserLogic_ScheduleOne_002EVision_002EPlayerVisibility_Assembly_002DCSharp_002Edll()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		base.Awake();
		player = ((Component)this).GetComponent<Player>();
		player.Health.onDie.AddListener((UnityAction)delegate
		{
			ClearStates();
		});
		ApplyState("Visible", EVisualState.Visible);
	}
}
