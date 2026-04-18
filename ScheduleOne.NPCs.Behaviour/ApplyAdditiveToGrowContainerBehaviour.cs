using System.Collections.Generic;
using ScheduleOne.AvatarFramework.Equipping;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Equipping;
using ScheduleOne.Growing;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Trash;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class ApplyAdditiveToGrowContainerBehaviour : GrowContainerBehaviour
{
	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EApplyAdditiveToGrowContainerBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EApplyAdditiveToGrowContainerBehaviourAssembly_002DCSharp_002Edll_Excuted;

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
		List<string> list = new List<string>();
		for (int i = 0; i < growContainer.AllowedAdditives.Length; i++)
		{
			if (!growContainer.IsAdditiveApplied(((BaseItemDefinition)growContainer.AllowedAdditives[i]).ID) && (!(growContainer is Pot pot) || (pot.Configuration as PotConfiguration).IsAdditiveSelected(growContainer.AllowedAdditives[i])) && (!(growContainer is MushroomBed mushroomBed) || (mushroomBed.Configuration as MushroomBedConfiguration).IsAdditiveSelected(growContainer.AllowedAdditives[i])))
			{
				list.Add(((BaseItemDefinition)growContainer.AllowedAdditives[i]).ID);
			}
		}
		return list.ToArray();
	}

	protected override void OnActionSuccess(ItemInstance usedItem)
	{
		base._growContainer.ApplyAdditive_Server(((BaseItemInstance)usedItem).ID);
	}

	public override bool AreTaskConditionsMetForContainer(GrowContainer container)
	{
		if (!container.ContainsGrowable())
		{
			return false;
		}
		string[] requiredItemSuitableIDs = GetRequiredItemSuitableIDs(container);
		if (requiredItemSuitableIDs.Length == 0)
		{
			return false;
		}
		bool flag = false;
		string[] array = requiredItemSuitableIDs;
		for (int i = 0; i < array.Length; i++)
		{
			AdditiveDefinition item = Registry.GetItem<AdditiveDefinition>(array[i]);
			if (container.CanApplyAdditive(item, out var _))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return false;
		}
		if (container.GetGrowthProgressNormalized() >= 1f)
		{
			return false;
		}
		return base.AreTaskConditionsMetForContainer(container);
	}

	protected override TrashItem GetTrashPrefab(ItemInstance usedItem)
	{
		AdditiveDefinition additiveDefinition = usedItem.Definition as AdditiveDefinition;
		if ((Object)(object)additiveDefinition != (Object)null)
		{
			return (additiveDefinition.Equippable as Equippable_Pourable).PourablePrefab.TrashItem;
		}
		return null;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EApplyAdditiveToGrowContainerBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EApplyAdditiveToGrowContainerBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EApplyAdditiveToGrowContainerBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EApplyAdditiveToGrowContainerBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
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
