using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.ItemFramework;
using ScheduleOne.Map;
using ScheduleOne.Money;
using ScheduleOne.NPCs;
using ScheduleOne.NPCs.Relation;
using ScheduleOne.UI.Phone.Map;
using ScheduleOne.Variables;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone.ContactsApp;

public class ContactsDetailPanel : MonoBehaviour
{
	[Header("Settings")]
	public Color DependenceColor_Min;

	public Color DependenceColor_Max;

	[Header("References")]
	public Text NameLabel;

	public Text TypeLabel;

	public Text UnlockHintLabel;

	public RectTransform RelationshipContainer;

	public Scrollbar RelationshipScrollbar;

	public Text RelationshipLabel;

	public RectTransform AddictionContainer;

	public Scrollbar AddictionScrollbar;

	public Text AddictionLabel;

	public RectTransform DebtContainer;

	public Text DebtLabel;

	public RectTransform PropertiesContainer;

	public Text PropertiesLabel;

	public RectTransform MostPurchasedProductsContainer;

	public Text MostPurchasedProductsLabel;

	public RectTransform TotalSpentContainer;

	public Text TotalSpentLabel;

	public Button ShowOnMapButton;

	public RectTransform StandardsContainer;

	public Image StandardsStar;

	public Text StandardsLabel;

	[Header("Fonts")]
	[SerializeField]
	private ColorFont _generalColorFont;

	[SerializeField]
	private ColorFont _proudctColorFont;

	private POI poi;

	private const int MAX_PURCHASED_PRODUCTS_DISPLAYED = 3;

	public NPC SelectedNPC { get; protected set; }

	public void Open(NPC npc)
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_041a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0430: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0599: Unknown result type (might be due to invalid IL or missing references)
		//IL_0587: Unknown result type (might be due to invalid IL or missing references)
		//IL_059e: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0654: Unknown result type (might be due to invalid IL or missing references)
		SelectedNPC = npc;
		if ((Object)(object)npc == (Object)null)
		{
			return;
		}
		bool unlocked = npc.RelationData.Unlocked;
		bool flag = unlocked;
		if (!npc.RelationData.Unlocked && npc.RelationData.IsMutuallyKnown() && NetworkSingleton<VariableDatabase>.InstanceExists)
		{
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("SelectedPotentialCustomer", true.ToString(), network: false);
		}
		poi = null;
		((Component)UnlockHintLabel).gameObject.SetActive(false);
		if (npc is Supplier supplier)
		{
			TypeLabel.text = "Supplier";
			((Graphic)TypeLabel).color = Color32.op_Implicit(Supplier.SupplierLabelColor);
			if (!unlocked)
			{
				UnlockHintLabel.text = "Unlock this supplier by reaching 'friendly' with one of their connections.";
				((Component)UnlockHintLabel).gameObject.SetActive(true);
			}
			if (unlocked)
			{
				poi = supplier.Stash.StashPoI;
			}
			((Component)DebtContainer).gameObject.SetActive(true);
			DebtLabel.text = MoneyManager.FormatAmount(supplier.Debt);
		}
		else if (npc is Dealer)
		{
			TypeLabel.text = "Dealer";
			((Graphic)TypeLabel).color = Color32.op_Implicit(Dealer.DealerLabelColor);
			Dealer dealer = npc as Dealer;
			if (!(npc as Dealer).HasBeenRecommended)
			{
				UnlockHintLabel.text = "Unlock this dealer by reaching 'friendly' with one of their connections.";
				((Component)UnlockHintLabel).gameObject.SetActive(true);
			}
			else if (!dealer.IsRecruited)
			{
				UnlockHintLabel.text = "This dealer is ready to be hired. Go to them and pay their signing free to recruit them.";
				((Component)UnlockHintLabel).gameObject.SetActive(true);
			}
			if (dealer.IsRecruited)
			{
				poi = dealer.DealerPoI;
			}
			else if (dealer.HasBeenRecommended)
			{
				poi = dealer.PotentialDealerPoI;
			}
			((Component)DebtContainer).gameObject.SetActive(false);
		}
		else
		{
			TypeLabel.text = "Customer";
			((Graphic)TypeLabel).color = Color.white;
			if (npc.RelationData.IsMutuallyKnown())
			{
				flag = true;
				if (!unlocked)
				{
					if (!GameManager.IS_TUTORIAL)
					{
						poi = ((Component)npc).GetComponent<Customer>().potentialCustomerPoI;
					}
					UnlockHintLabel.text = "Unlock this customer by giving them a free sample. Use your map to see their approximate location.";
					((Component)UnlockHintLabel).gameObject.SetActive(true);
				}
			}
			((Component)DebtContainer).gameObject.SetActive(false);
		}
		if (flag)
		{
			NameLabel.text = npc.fullName;
			((Component)ShowOnMapButton).gameObject.SetActive((Object)(object)poi != (Object)null);
			if (npc.RelationData.Unlocked)
			{
				RelationshipScrollbar.value = npc.RelationData.RelationDelta / 5f;
				RelationshipLabel.text = "<color=#" + ColorUtility.ToHtmlStringRGB(Color32.op_Implicit(RelationshipCategory.GetColor(RelationshipCategory.GetCategory(npc.RelationData.RelationDelta)))) + ">" + RelationshipCategory.GetCategory(npc.RelationData.RelationDelta).ToString() + "</color>";
				((Behaviour)RelationshipLabel).enabled = true;
				((Component)RelationshipContainer).gameObject.SetActive(true);
			}
			else
			{
				((Component)RelationshipContainer).gameObject.SetActive(false);
			}
			Customer component = ((Component)npc).GetComponent<Customer>();
			((Component)StandardsContainer).gameObject.SetActive(false);
			if ((Object)(object)component != (Object)null)
			{
				((Component)AddictionContainer).gameObject.SetActive(npc.RelationData.Unlocked);
				AddictionScrollbar.value = component.CurrentAddiction;
				AddictionLabel.text = Mathf.FloorToInt(component.CurrentAddiction * 100f) + "%";
				((Graphic)AddictionLabel).color = Color.Lerp(DependenceColor_Min, DependenceColor_Max, component.CurrentAddiction);
				EQuality correspondingQuality = component.CustomerData.Standards.GetCorrespondingQuality();
				((Graphic)StandardsStar).color = ItemQuality.GetColor(correspondingQuality);
				((Graphic)StandardsLabel).color = ((Graphic)StandardsStar).color;
				StandardsLabel.text = component.CustomerData.Standards.GetName();
				((Component)StandardsContainer).gameObject.SetActive(true);
				((Component)PropertiesContainer).gameObject.SetActive(true);
				PropertiesLabel.text = string.Empty;
				for (int i = 0; i < component.CustomerData.PreferredProperties.Count; i++)
				{
					if (i > 0)
					{
						Text propertiesLabel = PropertiesLabel;
						propertiesLabel.text += "\n";
					}
					string text = "<color=#" + ColorUtility.ToHtmlStringRGBA(component.CustomerData.PreferredProperties[i].LabelColor) + ">•  " + component.CustomerData.PreferredProperties[i].Name + "</color>";
					Text propertiesLabel2 = PropertiesLabel;
					propertiesLabel2.text += text;
				}
				((Component)MostPurchasedProductsContainer).gameObject.SetActive(unlocked);
				((Component)TotalSpentContainer).gameObject.SetActive(unlocked);
				if (!unlocked)
				{
					return;
				}
				component.CalculateTopWeeklyPurchases(out var mostPurchasedProducts, out var totalSpent);
				Color val = (((Object)(object)_generalColorFont != (Object)null) ? _generalColorFont.GetColour("FadedText") : Color.gray);
				Color val2 = (((Object)(object)_generalColorFont != (Object)null) ? _generalColorFont.GetColour("Cash") : Color.green);
				int num = Mathf.Min(mostPurchasedProducts.Count, 3);
				MostPurchasedProductsLabel.text = ((num == 0) ? ("<color=#" + ColorUtility.ToHtmlStringRGBA(val) + ">No recent purchases</color>") : string.Empty);
				for (int j = 0; j < num; j++)
				{
					if (j > 0)
					{
						Text mostPurchasedProductsLabel = MostPurchasedProductsLabel;
						mostPurchasedProductsLabel.text += "\n";
					}
					string name = ((BaseItemDefinition)Registry.GetItem(mostPurchasedProducts[j].String)).Name;
					string text2 = string.Format("<color=#{0}>{1}x</color> {2}", ColorUtility.ToHtmlStringRGBA(_proudctColorFont.GetColour("Quantity")), mostPurchasedProducts[j].Int, name);
					Text mostPurchasedProductsLabel2 = MostPurchasedProductsLabel;
					mostPurchasedProductsLabel2.text += text2;
				}
				string text3 = MoneyManager.FormatAmount(totalSpent);
				TotalSpentLabel.text = "<color=#" + ColorUtility.ToHtmlStringRGBA(val2) + ">" + text3 + "</color>";
			}
			else
			{
				((Component)AddictionContainer).gameObject.SetActive(false);
				((Component)PropertiesContainer).gameObject.SetActive(false);
				((Component)MostPurchasedProductsContainer).gameObject.SetActive(false);
				((Component)TotalSpentContainer).gameObject.SetActive(false);
			}
		}
		else
		{
			NameLabel.text = "???";
			((Component)RelationshipContainer).gameObject.SetActive(false);
			((Component)StandardsContainer).gameObject.SetActive(false);
			((Component)AddictionContainer).gameObject.SetActive(false);
			((Component)PropertiesContainer).gameObject.SetActive(false);
			((Component)MostPurchasedProductsContainer).gameObject.SetActive(false);
			((Component)TotalSpentContainer).gameObject.SetActive(false);
			((Component)ShowOnMapButton).gameObject.SetActive(false);
		}
	}

	public void ShowOnMap()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)poi == (Object)null) && !((Object)(object)poi.UI == (Object)null))
		{
			if (NetworkSingleton<VariableDatabase>.InstanceExists)
			{
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("PotentialCustomerShownOnMap", true.ToString(), network: false);
			}
			PlayerSingleton<ContactsApp>.Instance.SetOpen(open: false);
			PlayerSingleton<MapApp>.Instance.FocusPosition(poi.UI.anchoredPosition);
			PlayerSingleton<MapApp>.Instance.SkipFocusPlayer = true;
			PlayerSingleton<MapApp>.Instance.SetOpen(open: true);
		}
	}
}
