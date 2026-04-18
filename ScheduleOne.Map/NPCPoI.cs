using ScheduleOne.NPCs;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.Map;

public class NPCPoI : POI
{
	public NPC NPC { get; private set; }

	public override void InitializeUI()
	{
		base.InitializeUI();
		if ((Object)(object)base.IconContainer != (Object)null && (Object)(object)NPC != (Object)null)
		{
			((Component)((Transform)base.IconContainer).Find("Outline/Icon")).GetComponent<Image>().sprite = NPC.MugshotSprite;
		}
	}

	public void SetNPC(NPC npc)
	{
		NPC = npc;
		if ((Object)(object)base.IconContainer != (Object)null && (Object)(object)NPC != (Object)null)
		{
			((Component)((Transform)base.IconContainer).Find("Outline/Icon")).GetComponent<Image>().sprite = NPC.MugshotSprite;
		}
	}
}
