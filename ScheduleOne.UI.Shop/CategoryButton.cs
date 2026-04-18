using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Shop;

public class CategoryButton : MonoBehaviour
{
	public EShopCategory Category;

	private Button button;

	private ShopInterface shop;

	public bool isSelected { get; protected set; }

	private void Awake()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		button = ((Component)this).GetComponent<Button>();
		shop = ((Component)this).GetComponentInParent<ShopInterface>();
		((UnityEvent)button.onClick).AddListener(new UnityAction(Clicked));
		Deselect();
	}

	private void Clicked()
	{
		if (isSelected)
		{
			Deselect();
		}
		else
		{
			Select();
		}
	}

	public void Deselect()
	{
		isSelected = false;
		RefreshUI();
	}

	public void Select()
	{
		isSelected = true;
		RefreshUI();
		shop.CategorySelected(Category);
	}

	private void RefreshUI()
	{
		if ((Object)(object)button == (Object)null)
		{
			Console.LogError("CategoryButton: RefreshUI called but button is null.", (Object)(object)this);
		}
		else
		{
			((Selectable)button).interactable = !isSelected;
		}
	}
}
