using System;
using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dialogue;
using ScheduleOne.Economy;
using ScheduleOne.Messaging;
using ScheduleOne.NPCs.Relation;
using ScheduleOne.UI.Phone.Messages;
using ScheduleOne.Variables;

namespace ScheduleOne.NPCs.CharacterClasses;

public class Phil : Supplier
{
	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002EPhilAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002EPhilAssembly_002DCSharp_002Edll_Excuted;

	protected override void CreateMessageConversation()
	{
		base.CreateMessageConversation();
		SendableMessage sendableMessage = base.MSGConversation.CreateSendableMessage("How do I grow shrooms?");
		sendableMessage.onSent = (Action)Delegate.Combine(sendableMessage.onSent, new Action(InstructionsRequested));
	}

	protected virtual void InstructionsRequested()
	{
		if (InstanceFinder.IsServer)
		{
			MessageChain messageChain = DialogueHandler.Database.GetChain(EDialogueModule.Generic, "grow_shrooms_instructions").GetMessageChain();
			base.MSGConversation.SendMessageChain(messageChain, 0.5f, notify: false);
		}
	}

	protected override void SupplierUnlocked(NPCRelationData.EUnlockType type, bool notify)
	{
		base.SupplierUnlocked(type, notify);
		NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("TemperatureSystemEnabled", true.ToString());
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002EPhilAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002EPhilAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002EPhilAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002EPhilAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
