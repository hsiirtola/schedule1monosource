using System;
using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dialogue;
using ScheduleOne.Levelling;
using ScheduleOne.Persistence;
using ScheduleOne.Property;
using ScheduleOne.UI.Phone.Messages;
using ScheduleOne.Variables;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.NPCs.CharacterClasses;

public class Ray : NPC
{
	public DialogueContainer GreetingDialogue;

	public string GreetedVariable = "RayGreeted";

	public string IntroductionMessage;

	public string IntroSentVariable = "RayIntroSent";

	[Header("Intro message conditions")]
	public FullRank IntroRank;

	public int IntroDaysPlayed = 21;

	public float IntroNetworth = 15000f;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002ERayAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002ERayAssembly_002DCSharp_002Edll_Excuted;

	protected override void Start()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		base.Start();
		Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(Loaded));
		Manor manor = ScheduleOne.Property.Property.Properties.Find((ScheduleOne.Property.Property x) => x is Manor) as Manor;
		if ((Object)(object)manor != (Object)null)
		{
			manor.onRebuildComplete = (Action)Delegate.Combine(manor.onRebuildComplete, new Action(NotifyPlayerOfManorRebuild));
		}
	}

	private void Loaded()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(new UnityAction(Loaded));
		if (!NetworkSingleton<VariableDatabase>.Instance.GetValue<bool>(GreetedVariable))
		{
			EnableGreeting();
		}
	}

	private void EnableGreeting()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		((Component)DialogueHandler).GetComponent<DialogueController>().OverrideContainer = GreetingDialogue;
		DialogueHandler.onConversationStart.AddListener(new UnityAction(SetGreeted));
	}

	private void SetGreeted()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		DialogueHandler.onConversationStart.RemoveListener(new UnityAction(SetGreeted));
		NetworkSingleton<VariableDatabase>.Instance.SetVariableValue(GreetedVariable, true.ToString());
		((Component)DialogueHandler).GetComponent<DialogueController>().OverrideContainer = null;
	}

	private void NotifyPlayerOfManorRebuild()
	{
		if (InstanceFinder.IsServer)
		{
			Console.Log("Ray notifiying player of manor rebuild");
			MessageChain messageChain = DialogueHandler.Database.GetChain(EDialogueModule.Generic, "manor_rebuilt").GetMessageChain();
			base.MSGConversation.SendMessageChain(messageChain, 5f);
		}
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002ERayAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002ERayAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002ERayAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002ERayAssembly_002DCSharp_002Edll_Excuted = true;
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
