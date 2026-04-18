using ScheduleOne.AvatarFramework;
using ScheduleOne.AvatarFramework.Equipping;
using ScheduleOne.Core;
using UnityEngine;

namespace ScheduleOne.Tools;

public class EquipUtility : MonoBehaviour
{
	public AvatarEquippable Equippable;

	public void Update()
	{
		if (Input.GetKeyDown((KeyCode)113))
		{
			Equip();
		}
	}

	[Button]
	public void Equip()
	{
		((Component)this).GetComponent<Avatar>().SetEquippable(Equippable.AssetPath);
	}

	[Button]
	public void Unequip()
	{
		((Component)this).GetComponent<Avatar>().SetEquippable(string.Empty);
	}
}
