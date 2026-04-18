using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Growing;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;

namespace ScheduleOne.NPCs.Behaviour;

public class ApplySpawnToMushroomBedBehaviour : GrowContainerBehaviour
{
	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EApplySpawnToMushroomBedBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EApplySpawnToMushroomBedBehaviourAssembly_002DCSharp_002Edll_Excuted;

	protected override float GetActionDuration()
	{
		return 15f;
	}

	protected override string GetAnimationBool()
	{
		return "PatSoil";
	}

	protected override string[] GetRequiredItemSuitableIDs(GrowContainer growContainer)
	{
		if (growContainer is MushroomBed mushroomBed)
		{
			return (mushroomBed.Configuration as MushroomBedConfiguration).GetSelectedSeedIDs();
		}
		return base.GetRequiredItemSuitableIDs(growContainer);
	}

	protected override void OnActionSuccess(ItemInstance usedItem)
	{
		if (base._growContainer is MushroomBed mushroomBed)
		{
			mushroomBed.CreateAndAssignColony_Server(((BaseItemDefinition)usedItem.Definition).ID);
		}
	}

	public override bool AreTaskConditionsMetForContainer(GrowContainer container)
	{
		if (!container.IsFullyFilledWithSoil)
		{
			return false;
		}
		if (container.ContainsGrowable())
		{
			return false;
		}
		if (!(container is MushroomBed))
		{
			return false;
		}
		return base.AreTaskConditionsMetForContainer(container);
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EApplySpawnToMushroomBedBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EApplySpawnToMushroomBedBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EApplySpawnToMushroomBedBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EApplySpawnToMushroomBedBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
