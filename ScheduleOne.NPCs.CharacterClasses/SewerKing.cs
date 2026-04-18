using FishNet;
using FishNet.Object;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Property;
using UnityEngine;

namespace ScheduleOne.NPCs.CharacterClasses;

public class SewerKing : NPC
{
	public SewerOffice sewerOffice;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002ESewerKingAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002ESewerKingAssembly_002DCSharp_002Edll_Excuted;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ENPCs_002ECharacterClasses_002ESewerKing_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected override void OnTick()
	{
		base.OnTick();
		if (Health.IsDead || !InstanceFinder.IsServer || Behaviour.CombatBehaviour.Enabled)
		{
			return;
		}
		for (int i = 0; i < Player.PlayerList.Count; i++)
		{
			if ((Object)(object)Player.PlayerList[i].CurrentProperty == (Object)(object)sewerOffice)
			{
				Behaviour.CombatBehaviour.SetTargetAndEnable_Server(((NetworkBehaviour)Player.PlayerList[i]).NetworkObject);
				break;
			}
		}
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002ESewerKingAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002ESewerKingAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002ESewerKingAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002ESewerKingAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	protected override void Awake_UserLogic_ScheduleOne_002ENPCs_002ECharacterClasses_002ESewerKing_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
	}
}
