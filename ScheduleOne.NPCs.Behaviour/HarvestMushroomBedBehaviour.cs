using ScheduleOne.AvatarFramework.Equipping;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Growing;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Product;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class HarvestMushroomBedBehaviour : GrowContainerBehaviour
{
	public AvatarEquippable TrimmersEquippable;

	private MushroomBed _bed;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EHarvestMushroomBedBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EHarvestMushroomBedBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EHarvestMushroomBedBehaviour_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public override void AssignAndEnable(GrowContainer growContainer)
	{
		_bed = (MushroomBed)growContainer;
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
		ItemInstance harvestedShroom = _bed.CurrentColony.GetHarvestedShroom(GetQuantityToHarvest());
		if (harvestedShroom == null || ((BaseItemInstance)harvestedShroom).Quantity <= 0)
		{
			Console.LogError("HarvestMushroomBedBehaviour.OnActionSuccess called but no harvested item was returned!");
			return;
		}
		ProductManager.CheckDiscovery(harvestedShroom);
		((ITransitEntity)_bed).InsertItemIntoOutput(harvestedShroom, (NPC)null);
		for (int i = 0; i < ((BaseItemInstance)harvestedShroom).Quantity; i++)
		{
			_bed.CurrentColony.RemoveRandomShroom();
		}
		TransitRoute route = new TransitRoute(_bed, (_bed.Configuration as MushroomBedConfiguration).Destination.SelectedObject as ITransitEntity);
		base._botanist.MoveItemBehaviour.Initialize(route, harvestedShroom, -1, _skipPickup: true);
		base._botanist.MoveItemBehaviour.Enable_Networked();
	}

	private int GetQuantityToHarvest()
	{
		ItemInstance harvestedShroom = _bed.CurrentColony.GetHarvestedShroom();
		return Mathf.Min(new int[4]
		{
			_bed.CurrentColony.GrownMushroomCount,
			base._botanist.Inventory.GetCapacityForItem(harvestedShroom),
			GetDestinationCapacityForItem(_bed, harvestedShroom),
			((ITransitEntity)_bed).GetOutputCapacityForItem(harvestedShroom, (NPC)null)
		});
	}

	public override bool AreTaskConditionsMetForContainer(GrowContainer container)
	{
		if (!container.ContainsGrowable())
		{
			return false;
		}
		if (!(container is MushroomBed))
		{
			return false;
		}
		if (!(container as MushroomBed).CurrentColony.IsFullyGrown)
		{
			return false;
		}
		ItemInstance harvestedShroom = (container as MushroomBed).CurrentColony.GetHarvestedShroom();
		if (base._botanist.Inventory.GetCapacityForItem(harvestedShroom) <= 0)
		{
			return false;
		}
		return base.AreTaskConditionsMetForContainer(container);
	}

	protected override bool CheckSuccess(ItemInstance usedItem)
	{
		return DoesMushroomBedHaveValidDestination(_bed);
	}

	public bool DoesMushroomBedHaveValidDestination(MushroomBed bed)
	{
		return GetDestinationCapacityForItem(bed, bed.CurrentColony.GetHarvestedShroom()) > 0;
	}

	private int GetDestinationCapacityForItem(MushroomBed bed, ItemInstance item)
	{
		MushroomBedConfiguration mushroomBedConfiguration = bed.Configuration as MushroomBedConfiguration;
		if ((Object)(object)mushroomBedConfiguration.Destination.SelectedObject == (Object)null)
		{
			return 0;
		}
		return (mushroomBedConfiguration.Destination.SelectedObject as ITransitEntity).GetInputCapacityForItem(item, base._botanist);
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EHarvestMushroomBedBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EHarvestMushroomBedBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EHarvestMushroomBedBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EHarvestMushroomBedBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	protected override void Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EHarvestMushroomBedBehaviour_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
	}
}
