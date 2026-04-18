using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Equipping;

public class Equippable : MonoBehaviour
{
	protected ItemInstance itemInstance;

	public bool CanInteractWhenEquipped = true;

	public bool CanPickUpWhenEquipped = true;

	public virtual void Equip(ItemInstance item)
	{
		itemInstance = item;
		PlayerSingleton<PlayerInventory>.Instance.SetEquippable(this);
		PlayerSingleton<PlayerInventory>.Instance.EquippedSlotChanged();
	}

	public virtual void Unequip()
	{
		PlayerSingleton<PlayerInventory>.Instance.SetEquippable(null);
		PlayerSingleton<PlayerInventory>.Instance.EquippedSlotChanged();
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	protected virtual void Update()
	{
	}
}
