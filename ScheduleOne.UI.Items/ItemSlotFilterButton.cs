using System;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Items;

public class ItemSlotFilterButton : MonoBehaviour
{
	public ItemSlotUI ItemSlotUI;

	public Button Button;

	public Image IconImage;

	public Image SpotImage;

	public Image[] FilterItemImages;

	public TextMeshProUGUI FilterMoreItemsLabel;

	public ItemSlot AssignedSlot { get; protected set; }

	private void Awake()
	{
	}

	public void AssignSlot(ItemSlot slot)
	{
		if (AssignedSlot != null)
		{
			UnassignSlot();
		}
		AssignedSlot = slot;
		ItemSlot assignedSlot = AssignedSlot;
		assignedSlot.onFilterChange = (Action)Delegate.Combine(assignedSlot.onFilterChange, new Action(RefreshAppearance));
		RefreshAppearance();
		((Component)this).gameObject.SetActive(true);
	}

	public void UnassignSlot()
	{
		if (AssignedSlot != null)
		{
			ItemSlot assignedSlot = AssignedSlot;
			assignedSlot.onFilterChange = (Action)Delegate.Remove(assignedSlot.onFilterChange, new Action(RefreshAppearance));
			AssignedSlot = null;
			((Component)this).gameObject.SetActive(false);
		}
	}

	public void Clicked()
	{
		if (AssignedSlot != null && AssignedSlot.CanPlayerSetFilter && !AssignedSlot.IsLocked)
		{
			if (Singleton<ItemUIManager>.Instance.FilterConfigPanel.IsOpen && Singleton<ItemUIManager>.Instance.FilterConfigPanel.OpenSlot == AssignedSlot)
			{
				Singleton<ItemUIManager>.Instance.FilterConfigPanel.Close();
			}
			else
			{
				Singleton<ItemUIManager>.Instance.FilterConfigPanel.Open(ItemSlotUI);
			}
		}
	}

	private void RefreshAppearance()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		switch (AssignedSlot.PlayerFilter.Type)
		{
		case SlotFilter.EType.None:
			((Graphic)IconImage).color = Color32.op_Implicit(new Color32((byte)115, (byte)115, (byte)115, (byte)125));
			((Behaviour)SpotImage).enabled = false;
			break;
		case SlotFilter.EType.Whitelist:
			((Graphic)IconImage).color = Color.white;
			((Behaviour)SpotImage).enabled = false;
			break;
		case SlotFilter.EType.Blacklist:
			((Graphic)IconImage).color = Color.white;
			((Behaviour)SpotImage).enabled = true;
			break;
		}
		for (int i = 0; i < FilterItemImages.Length; i++)
		{
			if (AssignedSlot.PlayerFilter.ItemIDs.Count > i)
			{
				FilterItemImages[i].sprite = ((BaseItemDefinition)Registry.GetItem(AssignedSlot.PlayerFilter.ItemIDs[i])).Icon;
				((Component)FilterItemImages[i]).gameObject.SetActive(true);
			}
			else
			{
				((Component)FilterItemImages[i]).gameObject.SetActive(false);
			}
		}
		if (AssignedSlot.PlayerFilter.ItemIDs.Count > FilterItemImages.Length)
		{
			((Component)FilterMoreItemsLabel).gameObject.SetActive(true);
			((TMP_Text)FilterMoreItemsLabel).text = "+" + (AssignedSlot.PlayerFilter.ItemIDs.Count - FilterItemImages.Length);
		}
		else
		{
			((Component)FilterMoreItemsLabel).gameObject.SetActive(false);
		}
	}
}
