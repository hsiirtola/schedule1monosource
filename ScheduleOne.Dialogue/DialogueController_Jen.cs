using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Money;
using ScheduleOne.NPCs.Relation;
using ScheduleOne.PlayerScripts;

namespace ScheduleOne.Dialogue;

public class DialogueController_Jen : DialogueController
{
	public string BuyKeyText = "I want to buy a sewer access key";

	public StorableItemDefinition KeyItem;

	public DialogueContainer BuyKeyDialogue;

	public float MinRelationToBuyKey = 3f;

	protected override void Start()
	{
		base.Start();
		DialogueChoice dialogueChoice = new DialogueChoice();
		dialogueChoice.ChoiceText = BuyKeyText;
		dialogueChoice.Conversation = BuyKeyDialogue;
		dialogueChoice.Enabled = true;
		dialogueChoice.isValidCheck = CanBuyKey;
		AddDialogueChoice(dialogueChoice);
	}

	private bool CanBuyKey(out string invalidReason)
	{
		invalidReason = string.Empty;
		if (npc.RelationData.RelationDelta < MinRelationToBuyKey)
		{
			invalidReason = "'" + RelationshipCategory.GetCategory(MinRelationToBuyKey).ToString() + "' relationship required";
			return false;
		}
		return true;
	}

	public override string ModifyChoiceText(string choiceLabel, string choiceText)
	{
		if (choiceLabel == "CHOICE_CONFIRM")
		{
			choiceText = choiceText.Replace("<PRICE>", "<color=#54E717>(" + MoneyManager.FormatAmount(KeyItem.BasePurchasePrice) + ")</color>");
		}
		return base.ModifyChoiceText(choiceLabel, choiceText);
	}

	public override string ModifyDialogueText(string dialogueLabel, string dialogueText)
	{
		if (dialogueLabel == "OFFER")
		{
			dialogueText = dialogueText.Replace("<PRICE>", "<color=#54E717>" + MoneyManager.FormatAmount(KeyItem.BasePurchasePrice) + "</color>");
		}
		return base.ModifyDialogueText(dialogueLabel, dialogueText);
	}

	public override bool CheckChoice(string choiceLabel, out string invalidReason)
	{
		if (choiceLabel == "CHOICE_CONFIRM" && NetworkSingleton<MoneyManager>.Instance.cashBalance < KeyItem.BasePurchasePrice)
		{
			invalidReason = "Insufficient cash";
			return false;
		}
		return base.CheckChoice(choiceLabel, out invalidReason);
	}

	public override void ChoiceCallback(string choiceLabel)
	{
		if (choiceLabel == "CHOICE_CONFIRM")
		{
			NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - KeyItem.BasePurchasePrice, visualizeChange: true, playCashSound: true);
			npc.Inventory.InsertItem(NetworkSingleton<MoneyManager>.Instance.GetCashInstance(KeyItem.BasePurchasePrice));
			PlayerSingleton<PlayerInventory>.Instance.AddItemToInventory(KeyItem.GetDefaultInstance());
		}
		base.ChoiceCallback(choiceLabel);
	}
}
