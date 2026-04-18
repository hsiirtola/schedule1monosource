using FishNet;
using ScheduleOne.Core.Equipping.Framework;
using UnityEngine;

namespace ScheduleOne.NPCs.Other;

public class SmokeCigarette : MonoBehaviour
{
	[SerializeField]
	private EquippableData _cigarette;

	private NPC _npc;

	private IEquippedItemHandler _equippedItem;

	private void Awake()
	{
		_npc = ((Component)this).GetComponentInParent<NPC>();
	}

	public void Begin()
	{
		if (InstanceFinder.IsServer)
		{
			_equippedItem = _npc.Equip(_cigarette);
			_npc.Avatar.LookController.OverrideIKWeight(0.3f);
		}
	}

	public void End()
	{
		if (InstanceFinder.IsServer)
		{
			_npc.Unequip(_equippedItem);
			_npc.Avatar.LookController.OverrideIKWeight(0.2f);
		}
	}
}
