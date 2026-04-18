using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Growing;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Trash;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class SowSeedInPotBehaviour : GrowContainerBehaviour
{
	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ESowSeedInPotBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ESowSeedInPotBehaviourAssembly_002DCSharp_002Edll_Excuted;

	protected override float GetActionDuration()
	{
		return 15f;
	}

	protected override string GetAnimationBool()
	{
		return "PatSoil";
	}

	protected override void OnStartPerformAction()
	{
		base.OnStartPerformAction();
		base.Npc.SetCrouched_Networked(crouched: true);
	}

	protected override void OnStopPerformAction()
	{
		base.OnStopPerformAction();
		base.Npc.SetCrouched_Networked(crouched: false);
	}

	protected override string[] GetRequiredItemSuitableIDs(GrowContainer growContainer)
	{
		if (growContainer is Pot pot)
		{
			return (pot.Configuration as PotConfiguration).GetSelectedSeedIDs();
		}
		return base.GetRequiredItemSuitableIDs(growContainer);
	}

	protected override void OnActionSuccess(ItemInstance usedItem)
	{
		if (base._growContainer is Pot pot)
		{
			pot.PlantSeed_Server(((BaseItemInstance)usedItem).ID, 0f);
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
		if (!(container is Pot))
		{
			return false;
		}
		return base.AreTaskConditionsMetForContainer(container);
	}

	protected override TrashItem GetTrashPrefab(ItemInstance usedItem)
	{
		SeedDefinition seedDefinition = usedItem.Definition as SeedDefinition;
		if ((Object)(object)seedDefinition != (Object)null)
		{
			return seedDefinition.FunctionSeedPrefab.TrashPrefab;
		}
		return null;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ESowSeedInPotBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ESowSeedInPotBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ESowSeedInPotBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ESowSeedInPotBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
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
