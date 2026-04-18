using ScheduleOne.DevUtilities;
using ScheduleOne.Money;
using ScheduleOne.Property;
using UnityEngine;

namespace ScheduleOne.Dialogue;

public class DialogueHandler_EstateAgent : ControlledDialogueHandler
{
	private ScheduleOne.Property.Property selectedProperty;

	private Business selectedBusiness;

	public override bool CheckChoice(string choiceLabel, out string invalidReason)
	{
		ScheduleOne.Property.Property property = ScheduleOne.Property.Property.UnownedProperties.Find((ScheduleOne.Property.Property x) => x.PropertyCode.ToLower() == choiceLabel.ToLower());
		Business business = Business.UnownedBusinesses.Find((Business x) => x.PropertyCode.ToLower() == choiceLabel.ToLower());
		if ((Object)(object)property != (Object)null && NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance < property.Price)
		{
			invalidReason = "Insufficient balance";
			return false;
		}
		if ((Object)(object)business != (Object)null && NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance < business.Price)
		{
			invalidReason = "Insufficient balance";
			return false;
		}
		return base.CheckChoice(choiceLabel, out invalidReason);
	}

	public override bool ShouldChoiceBeShown(string choiceLabel)
	{
		ScheduleOne.Property.Property property = ScheduleOne.Property.Property.Properties.Find((ScheduleOne.Property.Property x) => x.PropertyCode.ToLower() == choiceLabel.ToLower());
		Business business = Business.Businesses.Find((Business x) => x.PropertyCode.ToLower() == choiceLabel.ToLower());
		if ((Object)(object)property != (Object)null && (property.IsOwned || !property.CanBePurchased()))
		{
			return false;
		}
		if ((Object)(object)business != (Object)null && (business.IsOwned || !business.CanBePurchased()))
		{
			return false;
		}
		return base.ShouldChoiceBeShown(choiceLabel);
	}

	protected override void ChoiceCallback(string choiceLabel)
	{
		ScheduleOne.Property.Property property = ScheduleOne.Property.Property.UnownedProperties.Find((ScheduleOne.Property.Property x) => x.PropertyCode.ToLower() == choiceLabel.ToLower());
		Business business = Business.UnownedBusinesses.Find((Business x) => x.PropertyCode.ToLower() == choiceLabel.ToLower());
		if ((Object)(object)property != (Object)null)
		{
			selectedProperty = property;
		}
		if ((Object)(object)business != (Object)null)
		{
			selectedBusiness = business;
		}
		base.ChoiceCallback(choiceLabel);
	}

	protected override void DialogueCallback(string choiceLabel)
	{
		if (choiceLabel == "CONFIRM_BUY")
		{
			NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction(selectedProperty.PropertyName + " purchase", 0f - selectedProperty.Price, 1f, string.Empty);
			selectedProperty.SetOwned();
		}
		if (choiceLabel == "CONFIRM_BUY_BUSINESS")
		{
			NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction(selectedBusiness.PropertyName + " purchase", 0f - selectedBusiness.Price, 1f, string.Empty);
			selectedBusiness.SetOwned();
		}
		base.DialogueCallback(choiceLabel);
	}

	protected override string ModifyDialogueText(string dialogueLabel, string dialogueText)
	{
		if (dialogueLabel == "CONFIRM")
		{
			return dialogueText.Replace("<PROPERTY>", selectedProperty.PropertyName.ToLower());
		}
		if (dialogueLabel == "CONFIRM_BUSINESS")
		{
			return dialogueText.Replace("<BUSINESS>", selectedBusiness.PropertyName.ToLower());
		}
		return base.ModifyDialogueText(dialogueLabel, dialogueText);
	}

	protected override string ModifyChoiceText(string choiceLabel, string choiceText)
	{
		ScheduleOne.Property.Property property = ScheduleOne.Property.Property.UnownedProperties.Find((ScheduleOne.Property.Property x) => x.PropertyCode.ToLower() == choiceLabel.ToLower());
		Business business = Business.UnownedBusinesses.Find((Business x) => x.PropertyCode.ToLower() == choiceLabel.ToLower());
		if ((Object)(object)property != (Object)null)
		{
			return choiceText.Replace("(<PRICE>)", "<color=#19BEF0>(" + MoneyManager.FormatAmount(property.Price) + ")</color>");
		}
		if ((Object)(object)business != (Object)null)
		{
			return choiceText.Replace("(<PRICE>)", "<color=#19BEF0>(" + MoneyManager.FormatAmount(business.Price) + ")</color>");
		}
		if (choiceLabel == "CONFIRM_CHOICE")
		{
			if ((Object)(object)selectedProperty != (Object)null)
			{
				return choiceText.Replace("(<PRICE>)", "<color=#19BEF0>(" + MoneyManager.FormatAmount(selectedProperty.Price) + ")</color>");
			}
			if ((Object)(object)selectedBusiness != (Object)null)
			{
				return choiceText.Replace("(<PRICE>)", "<color=#19BEF0>(" + MoneyManager.FormatAmount(selectedBusiness.Price) + ")</color>");
			}
		}
		return base.ModifyChoiceText(choiceLabel, choiceText);
	}
}
