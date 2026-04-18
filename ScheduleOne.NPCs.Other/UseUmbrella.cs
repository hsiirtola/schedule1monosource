using ScheduleOne.Core.Equipping.Framework;
using UnityEngine;

namespace ScheduleOne.NPCs.Other;

public class UseUmbrella : NPCDiscreteAction
{
	[Header("Components")]
	[SerializeField]
	private NPC _npc;

	[SerializeField]
	private EquippableData _umbrellaData;

	private IEquippedItemHandler _equippedItemHandler;

	private void Awake()
	{
		if ((Object)(object)_npc == (Object)null)
		{
			_npc = ((Component)this).GetComponentInParent<NPC>();
		}
	}

	protected override void BeginOnServer()
	{
		_equippedItemHandler = _npc.Equip(_umbrellaData);
	}

	protected override void EndOnServer()
	{
		if (_equippedItemHandler != null)
		{
			_npc.Unequip(_equippedItemHandler);
		}
	}
}
