using ScheduleOne.AvatarFramework.Equipping;
using UnityEngine;

namespace ScheduleOne.NPCs.Other;

public class DrinkItem : MonoBehaviour
{
	public NPC Npc;

	public AvatarEquippable DrinkPrefab;

	public bool active { get; protected set; }

	private void Awake()
	{
		if ((Object)(object)Npc == (Object)null)
		{
			Npc = ((Component)this).GetComponentInParent<NPC>();
		}
	}

	public void Begin()
	{
		active = true;
		Npc.SetEquippable_Return(DrinkPrefab.AssetPath);
		Npc.Avatar.Animation.SetBool("Drinking", value: true);
		Npc.Avatar.LookController.OverrideIKWeight(0.3f);
	}

	public void End()
	{
		active = false;
		Npc.Avatar.Animation.SetBool("Drinking", value: false);
		Npc.Avatar.LookController.ResetIKWeight();
		Npc.SetEquippable_Return(string.Empty);
	}
}
