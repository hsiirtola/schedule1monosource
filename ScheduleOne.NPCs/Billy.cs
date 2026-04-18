using System.Collections.Generic;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Product;
using ScheduleOne.Quests;
using ScheduleOne.UI.Handover;
using ScheduleOne.VoiceOver;
using UnityEngine;

namespace ScheduleOne.NPCs;

public class Billy : NPC
{
	public const int REQUESTED_PRODUCT_AMOUNT = 20;

	public const string REQUESTED_PRODUCT_ID = "cocaine";

	[Header("References")]
	public Contract TradeContract;

	public ItemDefinition RDXDefinition;

	private Customer customerComp;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBillyAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBillyAssembly_002DCSharp_002Edll_Excuted;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ENPCs_002EBilly_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public void OpenRDXTradeHandover()
	{
		Console.Log("Billy: starting RDX trade");
		Singleton<HandoverScreen>.Instance.Open(TradeContract, customerComp, HandoverScreen.EMode.Contract, HandoverOutcome, GetSucccessChance, _requireFullChanceOfSuccess: true);
		DialogueHandler.SkipNextDialogueBehaviourEnd();
	}

	private void HandoverOutcome(HandoverScreen.EHandoverOutcome outcome, List<ItemInstance> givenItems, float payment)
	{
		Behaviour.GenericDialogueBehaviour.Disable_Server();
		if (outcome == HandoverScreen.EHandoverOutcome.Finalize)
		{
			Singleton<HandoverScreen>.Instance.ClearCustomerSlots(returnToOriginals: false);
			ItemInstance defaultInstance = RDXDefinition.GetDefaultInstance();
			PlayerSingleton<PlayerInventory>.Instance.AddItemToInventory(defaultInstance);
			ShowWorldSpaceDialogue("Thanks, here's your RDX.", 3f);
			PlayVO(EVOLineType.Thanks);
		}
		else
		{
			Singleton<HandoverScreen>.Instance.ClearCustomerSlots(returnToOriginals: true);
		}
	}

	private float GetSucccessChance(List<ItemInstance> items, float price)
	{
		int num = 0;
		foreach (ItemInstance item in items)
		{
			if (item is ProductItemInstance productItemInstance && ((BaseItemInstance)productItemInstance).ID == "cocaine" && productItemInstance.Quality >= customerComp.CustomerData.Standards.GetCorrespondingQuality())
			{
				num += productItemInstance.Amount * ((BaseItemInstance)productItemInstance).Quantity;
			}
		}
		if (num < 20)
		{
			return 0f;
		}
		return 1f;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBillyAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBillyAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBillyAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBillyAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	protected override void Awake_UserLogic_ScheduleOne_002ENPCs_002EBilly_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		customerComp = ((Component)this).GetComponent<Customer>();
		ProductList productList = new ProductList();
		productList.entries.Add(new ProductList.Entry("cocaine", customerComp.CustomerData.Standards.GetCorrespondingQuality(), 20));
		TradeContract.SilentlyInitializeContract("Trade for RDX", string.Empty, null, string.Empty, customerComp, 0f, productList, string.Empty, new QuestWindowConfig(), 0, NetworkSingleton<TimeManager>.Instance.GetDateTime());
	}
}
