using ScheduleOne.AvatarFramework.Equipping;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Equipping;
using ScheduleOne.Growing;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts.Soil;
using ScheduleOne.Trash;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class AddSoilToGrowContainerBehaviour : GrowContainerBehaviour
{
	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EAddSoilToGrowContainerBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EAddSoilToGrowContainerBehaviourAssembly_002DCSharp_002Edll_Excuted;

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
		ItemSlot itemSlotContainingRequiredItem = GetItemSlotContainingRequiredItem(base._botanist.Inventory, GetRequiredItemSuitableIDs(base._growContainer));
		if (itemSlotContainingRequiredItem != null)
		{
			return (itemSlotContainingRequiredItem.ItemInstance.Equippable as Equippable_Viewmodel)?.AvatarEquippable;
		}
		return null;
	}

	protected override string[] GetRequiredItemSuitableIDs(GrowContainer growContainer)
	{
		string[] array = new string[growContainer.AllowedSoils.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = ((BaseItemDefinition)growContainer.AllowedSoils[i]).ID;
		}
		return array;
	}

	protected override void OnActionSuccess(ItemInstance usedItem)
	{
		SoilDefinition soilDefinition = usedItem.Definition as SoilDefinition;
		base._growContainer.SetSoil(soilDefinition);
		base._growContainer.SetSoilAmount(base._growContainer.SoilCapacity);
		base._growContainer.SetRemainingSoilUses(soilDefinition.Uses);
		base._growContainer.SyncSoilData();
	}

	public override bool AreTaskConditionsMetForContainer(GrowContainer container)
	{
		if (container.IsFullyFilledWithSoil)
		{
			return false;
		}
		return base.AreTaskConditionsMetForContainer(container);
	}

	protected override TrashItem GetTrashPrefab(ItemInstance usedItem)
	{
		SoilDefinition soilDefinition = usedItem.Definition as SoilDefinition;
		if ((Object)(object)soilDefinition != (Object)null)
		{
			return (soilDefinition.Equippable as Equippable_Soil).PourablePrefab.TrashItem;
		}
		return null;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EAddSoilToGrowContainerBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EAddSoilToGrowContainerBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EAddSoilToGrowContainerBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EAddSoilToGrowContainerBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
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
