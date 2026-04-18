using System.Collections.Generic;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Equipping;
using ScheduleOne.ItemFramework;
using UnityEngine;

namespace ScheduleOne.ObjectScripts.Cash;

public class Equippable_Cash : Equippable_Viewmodel
{
	private int amountIndex;

	[Header("References")]
	public Transform Container_Under100;

	public List<Transform> SingleNotes;

	public Transform Container_100_300;

	public List<Transform> Under300Stacks;

	public Transform Container_300Plus;

	public List<Transform> PlusStacks;

	public override void Equip(ItemInstance item)
	{
		base.Equip(item);
		((BaseItemInstance)item).onDataChanged += UpdateCashVisuals;
		UpdateCashVisuals();
	}

	public override void Unequip()
	{
		base.Unequip();
		((BaseItemInstance)itemInstance).onDataChanged -= UpdateCashVisuals;
	}

	private void UpdateCashVisuals()
	{
		if (!(itemInstance is CashInstance { Balance: var balance }))
		{
			((Component)Container_100_300).gameObject.SetActive(false);
			((Component)Container_300Plus).gameObject.SetActive(false);
			((Component)Container_Under100).gameObject.SetActive(false);
			return;
		}
		float num;
		if (balance < 100f)
		{
			num = Mathf.Round(balance / 10f) * 10f;
			int num2 = Mathf.Clamp(Mathf.RoundToInt(num / 10f), 0, 10);
			if (num > 0f)
			{
				num2 = Mathf.Max(1, num2);
			}
			((Component)Container_100_300).gameObject.SetActive(false);
			((Component)Container_300Plus).gameObject.SetActive(false);
			((Component)Container_Under100).gameObject.SetActive(true);
			for (int i = 0; i < SingleNotes.Count; i++)
			{
				if (i < num2)
				{
					((Component)SingleNotes[i]).gameObject.SetActive(true);
				}
				else
				{
					((Component)SingleNotes[i]).gameObject.SetActive(false);
				}
			}
			return;
		}
		num = Mathf.Floor(balance / 100f) * 100f;
		((Component)Container_Under100).gameObject.SetActive(false);
		if (num < 400f)
		{
			((Component)Container_300Plus).gameObject.SetActive(false);
			((Component)Container_100_300).gameObject.SetActive(true);
			for (int j = 0; j < Under300Stacks.Count; j++)
			{
				if ((float)j < num / 100f)
				{
					((Component)Under300Stacks[j]).gameObject.SetActive(true);
				}
				else
				{
					((Component)Under300Stacks[j]).gameObject.SetActive(false);
				}
			}
			return;
		}
		((Component)Container_100_300).gameObject.SetActive(false);
		((Component)Container_300Plus).gameObject.SetActive(true);
		for (int k = 0; k < PlusStacks.Count; k++)
		{
			if ((float)k < num / 100f)
			{
				((Component)PlusStacks[k]).gameObject.SetActive(true);
			}
			else
			{
				((Component)PlusStacks[k]).gameObject.SetActive(false);
			}
		}
	}
}
