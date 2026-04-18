using System.Collections.Generic;
using ScheduleOne.AvatarFramework.Equipping;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Growing;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class HarvestPotBehaviour : GrowContainerBehaviour
{
	public AvatarEquippable TrimmersEquippable;

	private Pot _pot;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EHarvestPotBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EHarvestPotBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EHarvestPotBehaviour_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public override void AssignAndEnable(GrowContainer growContainer)
	{
		_pot = (Pot)growContainer;
		base.AssignAndEnable(growContainer);
	}

	protected override float GetActionDuration()
	{
		return 1f * (float)GetQuantityToHarvest();
	}

	protected override string GetAnimationBool()
	{
		return "Snipping";
	}

	protected override AvatarEquippable GetActionEquippable()
	{
		return TrimmersEquippable;
	}

	protected override void OnActionSuccess(ItemInstance usedItem)
	{
		ItemInstance harvestedProduct = _pot.Plant.GetHarvestedProduct(GetQuantityToHarvest());
		((ITransitEntity)_pot).InsertItemIntoOutput(harvestedProduct, (NPC)null);
		List<int> list = new List<int>(_pot.Plant.ActiveHarvestables);
		for (int i = 0; i < ((BaseItemInstance)harvestedProduct).Quantity; i++)
		{
			_pot.SetHarvestableActive_Server(list[i], active: false);
		}
		TransitRoute route = new TransitRoute(_pot, (_pot.Configuration as PotConfiguration).Destination.SelectedObject as ITransitEntity);
		base._botanist.MoveItemBehaviour.Initialize(route, harvestedProduct, -1, _skipPickup: true);
		base._botanist.MoveItemBehaviour.Enable_Networked();
	}

	private int GetQuantityToHarvest()
	{
		ItemInstance harvestedProduct = _pot.Plant.GetHarvestedProduct();
		return Mathf.Min(new int[4]
		{
			_pot.Plant.ActiveHarvestables.Count,
			base._botanist.Inventory.GetCapacityForItem(harvestedProduct),
			GetDestinationCapacityForItem(_pot, harvestedProduct),
			((ITransitEntity)_pot).GetOutputCapacityForItem(harvestedProduct, (NPC)null)
		});
	}

	public override bool AreTaskConditionsMetForContainer(GrowContainer container)
	{
		if (!container.ContainsGrowable())
		{
			return false;
		}
		if (!(container is Pot))
		{
			return false;
		}
		if (!(container as Pot).Plant.IsFullyGrown)
		{
			return false;
		}
		ItemInstance harvestedProduct = (container as Pot).Plant.GetHarvestedProduct();
		if (base._botanist.Inventory.GetCapacityForItem(harvestedProduct) <= 0)
		{
			return false;
		}
		return base.AreTaskConditionsMetForContainer(container);
	}

	protected override bool CheckSuccess(ItemInstance usedItem)
	{
		return DoesPotHaveValidDestination(_pot);
	}

	public bool DoesPotHaveValidDestination(Pot pot)
	{
		return GetDestinationCapacityForItem(pot, pot.Plant.GetHarvestedProduct()) > 0;
	}

	private int GetDestinationCapacityForItem(Pot pot, ItemInstance item)
	{
		PotConfiguration potConfiguration = pot.Configuration as PotConfiguration;
		if ((Object)(object)potConfiguration.Destination.SelectedObject == (Object)null)
		{
			return 0;
		}
		return (potConfiguration.Destination.SelectedObject as ITransitEntity).GetInputCapacityForItem(item, base._botanist);
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EHarvestPotBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EHarvestPotBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EHarvestPotBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EHarvestPotBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	protected override void Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EHarvestPotBehaviour_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
	}
}
