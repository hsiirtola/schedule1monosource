using System;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.UI.Phone.ProductManagerApp;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.Product;

public class ProductEntry : MonoBehaviour
{
	public Color SelectedColor;

	public Color DeselectedColor;

	public Color FavouritedColor;

	public Color UnfavouritedColor;

	[Header("References")]
	public Button Button;

	public Image Frame;

	public Image Icon;

	public RectTransform Tick;

	public RectTransform Cross;

	public EventTrigger Trigger;

	public Button FavouriteButton;

	public Image FavouriteIcon;

	public UnityEvent onHovered;

	private bool destroyed;

	public ProductDefinition Definition { get; private set; }

	public void Initialize(ProductDefinition definition)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		Definition = definition;
		Icon.sprite = ((BaseItemDefinition)definition).Icon;
		((UnityEvent)Button.onClick).AddListener(new UnityAction(Clicked));
		((UnityEvent)FavouriteButton.onClick).AddListener(new UnityAction(FavouriteClicked));
		Entry val = new Entry();
		val.eventID = (EventTriggerType)0;
		((UnityEvent<BaseEventData>)(object)val.callback).AddListener((UnityAction<BaseEventData>)delegate
		{
			onHovered.Invoke();
		});
		Trigger.triggers.Add(val);
		UpdateListed();
		UpdateFavourited();
		UpdateDiscovered(Definition);
		ProductManager instance = NetworkSingleton<ProductManager>.Instance;
		instance.onProductDiscovered = (Action<ProductDefinition>)Delegate.Combine(instance.onProductDiscovered, new Action<ProductDefinition>(UpdateDiscovered));
		ProductManager instance2 = NetworkSingleton<ProductManager>.Instance;
		instance2.onProductListed = (Action<ProductDefinition>)Delegate.Combine(instance2.onProductListed, new Action<ProductDefinition>(ProductListedOrDelisted));
		ProductManager instance3 = NetworkSingleton<ProductManager>.Instance;
		instance3.onProductDelisted = (Action<ProductDefinition>)Delegate.Combine(instance3.onProductDelisted, new Action<ProductDefinition>(ProductListedOrDelisted));
		ProductManager instance4 = NetworkSingleton<ProductManager>.Instance;
		instance4.onProductFavourited = (Action<ProductDefinition>)Delegate.Combine(instance4.onProductFavourited, new Action<ProductDefinition>(ProductFavouritedOrUnFavourited));
		ProductManager instance5 = NetworkSingleton<ProductManager>.Instance;
		instance5.onProductUnfavourited = (Action<ProductDefinition>)Delegate.Combine(instance5.onProductUnfavourited, new Action<ProductDefinition>(ProductFavouritedOrUnFavourited));
	}

	public void Destroy()
	{
		destroyed = true;
		((Component)this).gameObject.SetActive(false);
		Object.DestroyImmediate((Object)(object)((Component)this).gameObject);
	}

	private void OnDestroy()
	{
		if (NetworkSingleton<ProductManager>.InstanceExists)
		{
			ProductManager instance = NetworkSingleton<ProductManager>.Instance;
			instance.onProductDiscovered = (Action<ProductDefinition>)Delegate.Remove(instance.onProductDiscovered, new Action<ProductDefinition>(UpdateDiscovered));
			ProductManager instance2 = NetworkSingleton<ProductManager>.Instance;
			instance2.onProductListed = (Action<ProductDefinition>)Delegate.Remove(instance2.onProductListed, new Action<ProductDefinition>(ProductListedOrDelisted));
			ProductManager instance3 = NetworkSingleton<ProductManager>.Instance;
			instance3.onProductDelisted = (Action<ProductDefinition>)Delegate.Remove(instance3.onProductDelisted, new Action<ProductDefinition>(ProductListedOrDelisted));
			ProductManager instance4 = NetworkSingleton<ProductManager>.Instance;
			instance4.onProductFavourited = (Action<ProductDefinition>)Delegate.Remove(instance4.onProductFavourited, new Action<ProductDefinition>(ProductFavouritedOrUnFavourited));
			ProductManager instance5 = NetworkSingleton<ProductManager>.Instance;
			instance5.onProductUnfavourited = (Action<ProductDefinition>)Delegate.Remove(instance5.onProductUnfavourited, new Action<ProductDefinition>(ProductFavouritedOrUnFavourited));
		}
	}

	private void Clicked()
	{
		PlayerSingleton<ProductManagerApp>.Instance.SelectProduct(this);
		UpdateListed();
	}

	private void FavouriteClicked()
	{
		if (ProductManager.DiscoveredProducts.Contains(Definition))
		{
			if (ProductManager.FavouritedProducts.Contains(Definition))
			{
				NetworkSingleton<ProductManager>.Instance.SetProductFavourited(((BaseItemDefinition)Definition).ID, listed: false);
			}
			else
			{
				NetworkSingleton<ProductManager>.Instance.SetProductFavourited(((BaseItemDefinition)Definition).ID, listed: true);
			}
		}
	}

	private void ProductListedOrDelisted(ProductDefinition def)
	{
		if ((Object)(object)def == (Object)(object)Definition)
		{
			UpdateListed();
		}
	}

	public void UpdateListed()
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (!destroyed && !((Object)(object)this == (Object)null) && !((Object)(object)((Component)this).gameObject == (Object)null))
		{
			if (ProductManager.ListedProducts.Contains(Definition))
			{
				((Graphic)Frame).color = SelectedColor;
				((Component)Tick).gameObject.SetActive(true);
				((Component)Cross).gameObject.SetActive(false);
			}
			else
			{
				((Graphic)Frame).color = DeselectedColor;
				((Component)Tick).gameObject.SetActive(false);
				((Component)Cross).gameObject.SetActive(true);
			}
		}
	}

	private void ProductFavouritedOrUnFavourited(ProductDefinition def)
	{
		if ((Object)(object)def == (Object)(object)Definition)
		{
			UpdateFavourited();
		}
	}

	public void UpdateFavourited()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (!destroyed && !((Object)(object)this == (Object)null) && !((Object)(object)((Component)this).gameObject == (Object)null))
		{
			if (ProductManager.FavouritedProducts.Contains(Definition))
			{
				((Graphic)FavouriteIcon).color = FavouritedColor;
			}
			else
			{
				((Graphic)FavouriteIcon).color = UnfavouritedColor;
			}
		}
	}

	public void UpdateDiscovered(ProductDefinition def)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)def == (Object)null)
		{
			Console.LogWarning(((object)def)?.ToString() + " productDefinition is null");
		}
		if (((BaseItemDefinition)def).ID == ((BaseItemDefinition)Definition).ID)
		{
			if (ProductManager.DiscoveredProducts.Contains(Definition))
			{
				((Graphic)Icon).color = Color.white;
			}
			else
			{
				((Graphic)Icon).color = Color.black;
			}
			UpdateListed();
		}
	}
}
