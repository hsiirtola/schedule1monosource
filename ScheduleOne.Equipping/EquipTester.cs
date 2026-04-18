using ScheduleOne.Core;
using ScheduleOne.Core.Equipping.Framework;
using ScheduleOne.Equipping.Framework;
using UnityEngine;

namespace ScheduleOne.Equipping;

public class EquipTester : MonoBehaviour
{
	public EquippableData TestEquippable;

	private IEquippedItemHandler _equippedItemHandler;

	[Button]
	public void Equip()
	{
		_equippedItemHandler = ((IEquippableUser)((Component)this).GetComponent<INetworkedEquippableUser>()).Equip(TestEquippable);
	}

	[Button]
	public void EquipLocally()
	{
		_equippedItemHandler = ((Component)this).GetComponent<INetworkedEquippableUser>().EquipLocal(TestEquippable);
	}

	[Button]
	public void Unequip()
	{
		((IEquippableUser)((Component)this).GetComponent<INetworkedEquippableUser>()).Unequip(_equippedItemHandler);
	}

	[Button]
	public void UnequipAll()
	{
		((IEquippableUser)((Component)this).GetComponent<INetworkedEquippableUser>()).UnequipAll();
	}
}
