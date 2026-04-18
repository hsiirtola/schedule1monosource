using ScheduleOne.Core.Equipping.Framework;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Equipping;
using ScheduleOne.ItemFramework;
using ScheduleOne.Product;
using UnityEngine;

namespace ScheduleOne.PlayerScripts;

public class HotbarSlot : ItemSlot
{
	public delegate void EquipEvent(bool equipped);

	public EquipEvent onEquipChanged;

	private Equippable _equippable;

	private IEquippedItemHandler _equippedItem;

	public bool IsSelected { get; protected set; }

	public override void SetStoredItem(ItemInstance instance, bool _internal = false)
	{
		if (_internal || base.SlotOwner == null)
		{
			Unequip();
		}
		base.SetStoredItem(instance, _internal);
		if (instance != null)
		{
			ProductManager.CheckDiscovery(instance);
		}
		if ((_internal || base.SlotOwner == null) && IsSelected)
		{
			Equip();
		}
	}

	public override void ClearStoredInstance(bool _internal = false)
	{
		if (_internal || base.SlotOwner == null)
		{
			Unequip();
		}
		base.ClearStoredInstance(_internal);
	}

	public virtual void Select()
	{
		IsSelected = true;
		Equip();
		PlayerSingleton<PlayerInventory>.Instance.EquippedSlotChanged();
		if (onEquipChanged != null)
		{
			onEquipChanged(equipped: true);
		}
	}

	private void Equip()
	{
		if (base.ItemInstance != null && (Object)(object)base.ItemInstance.Equippable != (Object)null)
		{
			if (PlayerSingleton<PlayerInventory>.Instance.onPreItemEquipped != null)
			{
				PlayerSingleton<PlayerInventory>.Instance.onPreItemEquipped.Invoke();
			}
			if (base.ItemInstance.Definition.EquipMode == ItemDefinition.EEquipMode.Legacy)
			{
				_equippable = Object.Instantiate<GameObject>(((Component)base.ItemInstance.Equippable).gameObject, PlayerSingleton<PlayerInventory>.Instance.equipContainer).GetComponent<Equippable>();
				_equippable.Equip(base.ItemInstance);
			}
			else
			{
				_equippedItem = Player.Local.Equip((BaseItemInstance)(object)base.ItemInstance);
			}
		}
	}

	private void Unequip()
	{
		if ((Object)(object)_equippable != (Object)null)
		{
			_equippable.Unequip();
			_equippable = null;
		}
		if (_equippedItem != null)
		{
			Player.Local.Unequip(_equippedItem);
			_equippedItem = null;
		}
	}

	public virtual void Deselect()
	{
		Unequip();
		PlayerSingleton<PlayerInventory>.Instance.EquippedSlotChanged();
		IsSelected = false;
		if (onEquipChanged != null)
		{
			onEquipChanged(equipped: false);
		}
	}

	public override bool CanSlotAcceptCash()
	{
		return false;
	}
}
