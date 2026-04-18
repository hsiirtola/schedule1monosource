using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.ItemFramework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone;

public class CustomerSelector : MonoBehaviour
{
	public GameObject ButtonPrefab;

	[Header("References")]
	public RectTransform EntriesContainer;

	public UnityEvent<Customer> onCustomerSelected;

	private List<RectTransform> customerEntries = new List<RectTransform>();

	private Dictionary<RectTransform, Customer> entryToCustomer = new Dictionary<RectTransform, Customer>();

	public void Awake()
	{
		for (int i = 0; i < Customer.UnlockedCustomers.Count; i++)
		{
			CreateEntry(Customer.UnlockedCustomers[i]);
		}
		Customer.onCustomerUnlocked = (Action<Customer>)Delegate.Combine(Customer.onCustomerUnlocked, new Action<Customer>(CreateEntry));
		Close();
	}

	public void Start()
	{
		GameInput.RegisterExitListener(Exit, 7);
	}

	private void OnDestroy()
	{
		Customer.onCustomerUnlocked = (Action<Customer>)Delegate.Remove(Customer.onCustomerUnlocked, new Action<Customer>(CreateEntry));
	}

	private void Exit(ExitAction action)
	{
		if (action != null && !action.Used && PlayerSingleton<Phone>.Instance.IsOpen && (Object)(object)this != (Object)null && (Object)(object)((Component)this).gameObject != (Object)null && ((Component)this).gameObject.activeInHierarchy)
		{
			action.Used = true;
			Close();
		}
	}

	public void Open()
	{
		for (int i = 0; i < customerEntries.Count; i++)
		{
			if ((Object)(object)entryToCustomer[customerEntries[i]].AssignedDealer != (Object)null)
			{
				((Component)customerEntries[i]).gameObject.SetActive(false);
			}
			else
			{
				((Component)customerEntries[i]).gameObject.SetActive(true);
			}
		}
		((Component)this).gameObject.SetActive(true);
	}

	public void Close()
	{
		((Component)this).gameObject.SetActive(false);
	}

	private void CreateEntry(Customer customer)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Expected O, but got Unknown
		if (!customerEntries.Exists((RectTransform x) => (Object)(object)entryToCustomer[x] == (Object)(object)customer))
		{
			RectTransform component = Object.Instantiate<GameObject>(ButtonPrefab, (Transform)(object)EntriesContainer).GetComponent<RectTransform>();
			ColorFont generalColorFont = PlayerSingleton<Phone>.Instance.GeneralColorFont;
			Color val = (((Object)(object)generalColorFont != (Object)null) ? generalColorFont.GetColour("FadedText") : Color.gray);
			Color color = ItemQuality.GetColor(customer.CustomerData.Standards.GetCorrespondingQuality());
			((Component)((Transform)component).Find("Mugshot")).GetComponent<Image>().sprite = customer.NPC.MugshotSprite;
			((Graphic)((Component)((Transform)component).Find("Mugshot/ExpectedQuality")).GetComponent<Image>()).color = color;
			((Component)((Transform)component).Find("Name")).GetComponent<Text>().text = customer.NPC.fullName + $"<color=#{ColorUtility.ToHtmlStringRGBA(val)}> ({customer.NPC.Region})</color>";
			((UnityEvent)((Component)component).GetComponent<Button>().onClick).AddListener((UnityAction)delegate
			{
				CustomerSelected(customer);
			});
			customerEntries.Add(component);
			entryToCustomer.Add(component, customer);
		}
	}

	private void CustomerSelected(Customer customer)
	{
		if ((Object)(object)customer.AssignedDealer == (Object)null && onCustomerSelected != null)
		{
			onCustomerSelected.Invoke(customer);
		}
		Close();
	}
}
