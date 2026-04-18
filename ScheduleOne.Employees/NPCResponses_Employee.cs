using FishNet;
using FishNet.Object;
using ScheduleOne.Combat;
using ScheduleOne.NPCs.Responses;
using ScheduleOne.PlayerScripts;

namespace ScheduleOne.Employees;

public class NPCResponses_Employee : NPCResponses
{
	protected override void RespondToFirstNonLethalAttack(Player perpetrator, Impact impact)
	{
		base.RespondToFirstNonLethalAttack(perpetrator, impact);
		Ow(perpetrator);
	}

	protected override void RespondToLethalAttack(Player perpetrator, Impact impact)
	{
		base.RespondToLethalAttack(perpetrator, impact);
		Ow(perpetrator);
	}

	protected override void RespondToRepeatedNonLethalAttack(Player perpetrator, Impact impact)
	{
		base.RespondToRepeatedNonLethalAttack(perpetrator, impact);
		Ow(perpetrator);
	}

	private void Ow(Player perpetrator)
	{
		base.npc.DialogueHandler.PlayReaction("hurt", 2.5f, network: false);
		base.npc.Avatar.EmotionManager.AddEmotionOverride("Annoyed", "hurt", 20f, 3);
		if (InstanceFinder.IsServer)
		{
			base.npc.Behaviour.FaceTargetBehaviour.SetTarget(((NetworkBehaviour)perpetrator).NetworkObject);
			base.npc.Behaviour.FaceTargetBehaviour.Enable_Networked();
		}
	}
}
