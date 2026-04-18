using ScheduleOne.Core.Items.Framework;
using ScheduleOne.ItemFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Items;

public class ItemUI : MonoBehaviour
{
	protected ItemInstance itemInstance;

	[Header("References")]
	public RectTransform Rect;

	public Image IconImg;

	public TextMeshProUGUI QuantityLabel;

	protected int DisplayedQuantity;

	protected bool Destroyed;

	public virtual void Setup(ItemInstance item)
	{
		if (item == null)
		{
			Console.LogError("ItemUI.Setup called and passed null item");
		}
		itemInstance = item;
		((BaseItemInstance)itemInstance).onDataChanged -= UpdateUI;
		((BaseItemInstance)itemInstance).onDataChanged += UpdateUI;
		UpdateUI();
	}

	public virtual void Destroy()
	{
		Destroyed = true;
		((BaseItemInstance)itemInstance).onDataChanged -= UpdateUI;
		itemInstance = null;
		Object.Destroy((Object)(object)((Component)Rect).gameObject);
	}

	public virtual RectTransform DuplicateIcon(Transform parent, int overriddenQuantity = -1)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		int displayedQuantity = DisplayedQuantity;
		if (overriddenQuantity != -1)
		{
			SetDisplayedQuantity(overriddenQuantity);
		}
		RectTransform component = Object.Instantiate<GameObject>(((Component)IconImg).gameObject, parent).GetComponent<RectTransform>();
		((Transform)component).localScale = Vector3.one;
		SetDisplayedQuantity(displayedQuantity);
		return component;
	}

	public virtual void SetVisible(bool vis)
	{
		((Component)Rect).gameObject.SetActive(vis);
	}

	public virtual void UpdateUI()
	{
		if (!Destroyed)
		{
			IconImg.sprite = ((BaseItemInstance)itemInstance).Icon;
			SetDisplayedQuantity(((BaseItemInstance)itemInstance).Quantity);
		}
	}

	public virtual void SetDisplayedQuantity(int quantity)
	{
		DisplayedQuantity = quantity;
		if (quantity > 1)
		{
			((TMP_Text)QuantityLabel).text = quantity + "x";
		}
		else
		{
			((TMP_Text)QuantityLabel).text = string.Empty;
		}
	}
}
