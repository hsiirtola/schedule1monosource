using ScheduleOne.AvatarFramework.Equipping;
using ScheduleOne.Growing;
using ScheduleOne.ItemFramework;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class WaterPotBehaviour : GrowContainerBehaviour
{
	public AvatarEquippable Equippable;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EWaterPotBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EWaterPotBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EWaterPotBehaviour_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected override float GetActionDuration()
	{
		return 10f;
	}

	protected override string GetAnimationBool()
	{
		return "PourItem";
	}

	protected override AvatarEquippable GetActionEquippable()
	{
		return Equippable;
	}

	protected override void OnActionSuccess(ItemInstance usedItem)
	{
		base._growContainer.SetMoistureAmount(base._growContainer.MoistureCapacity * Random.Range(0.9f, 1f));
	}

	public override bool AreTaskConditionsMetForContainer(GrowContainer container)
	{
		if (!container.IsFullyFilledWithSoil)
		{
			return false;
		}
		if (!container.ContainsGrowable())
		{
			return false;
		}
		if (container.GetGrowthProgressNormalized() >= 1f)
		{
			return false;
		}
		return base.AreTaskConditionsMetForContainer(container);
	}

	public virtual bool AreTaskConditionsMetForContainer(GrowContainer container, float wateringThreshold)
	{
		if (container.NormalizedMoistureAmount >= wateringThreshold)
		{
			return false;
		}
		return AreTaskConditionsMetForContainer(container);
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EWaterPotBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EWaterPotBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EWaterPotBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EWaterPotBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	protected override void Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EWaterPotBehaviour_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
	}
}
