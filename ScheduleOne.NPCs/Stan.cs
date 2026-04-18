using ScheduleOne.DevUtilities;
using ScheduleOne.Dialogue;
using ScheduleOne.Persistence;
using ScheduleOne.UI.Shop;
using ScheduleOne.Variables;
using ScheduleOne.VoiceOver;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.NPCs;

public class Stan : NPC
{
	public ShopInterface ShopInterface;

	public DialogueContainer GreetingDialogue;

	public string GreetedVariable = "StanGreeted";

	[Header("Settings")]
	public string[] OrderCompletedLines;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EStanAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EStanAssembly_002DCSharp_002Edll_Excuted;

	protected override void Start()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		base.Start();
		Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(Loaded));
		ShopInterface.onOrderCompleted.AddListener(new UnityAction(OrderCompleted));
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

	private void OrderCompleted()
	{
		PlayVO(EVOLineType.Thanks);
		DialogueHandler.ShowWorldspaceDialogue(OrderCompletedLines[Random.Range(0, OrderCompletedLines.Length)], 5f);
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EStanAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EStanAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EStanAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EStanAssembly_002DCSharp_002Edll_Excuted = true;
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
