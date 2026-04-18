using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Audio;
using ScheduleOne.Configuration;
using ScheduleOne.Core.Settings.Framework;
using ScheduleOne.Delivery;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Money;
using ScheduleOne.UI.Shop;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone.Delivery;

public class DeliveryApp : App<DeliveryApp>
{
	[Serializable]
	public class DeliveryShopElement
	{
		public DeliveryShop Shop;

		public Button Button;
	}

	private List<DeliveryShop> deliveryShops = new List<DeliveryShop>();

	public DeliveryStatusDisplay StatusDisplayPrefab;

	[Header("References")]
	public Animation OrderSubmittedAnim;

	public AudioSourceController OrderSubmittedSound;

	public RectTransform StatusDisplayContainer;

	public GameObject NoDeliveriesIndicator;

	public GameObject NoPastDeliveriesIndicator;

	public ScrollRect MainScrollRect;

	public LayoutGroup MainLayoutGroup;

	[Header("Components")]
	[SerializeField]
	private DeliveryReceiptDisplay _deliveryReceiptPrefab;

	public RectTransform PastDeliveriesContainer;

	[Header("References")]
	[SerializeField]
	private TabController _tabController;

	[SerializeField]
	private CanvasGroup shopListCanvas;

	[SerializeField]
	private CanvasGroup orderCanvas;

	[SerializeField]
	private List<DeliveryShopElement> _shopElements;

	[Header("Settings")]
	[SerializeField]
	private float shopPanelWidth = 1201f;

	[SerializeField]
	private float shopTransitionDuration = 0.25f;

	private List<DeliveryStatusDisplay> statusDisplays = new List<DeliveryStatusDisplay>();

	private DeliveryReceiptDisplay[] _pastDeliveries;

	private bool started;

	private List<RectTransform> _shopPanels;

	private List<Vector2> _shopPanelInitialAnchors;

	private Coroutine _shopTransitionCoroutine;

	protected override void Awake()
	{
		base.Awake();
		deliveryShops = ((Component)this).GetComponentsInChildren<DeliveryShop>(true).ToList();
	}

	protected override void Start()
	{
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Expected O, but got Unknown
		base.Start();
		if (started)
		{
			return;
		}
		started = true;
		NetworkSingleton<DeliveryManager>.Instance.onDeliveryCreated += CreateDeliveryStatusDisplay;
		NetworkSingleton<DeliveryManager>.Instance.onDeliveryCompleted += DeliveryCompleted;
		NetworkSingleton<TimeManager>.Instance.onUncappedMinutePass += new Action(OnMinPass);
		foreach (DeliveryShopElement shopElement in _shopElements)
		{
			((Component)shopElement.Button).gameObject.SetActive(shopElement.Shop.AvailableByDefault);
			((UnityEvent)shopElement.Button.onClick).AddListener((UnityAction)delegate
			{
				OpenShop(shopElement.Shop);
			});
			DeliveryShop shop = shopElement.Shop;
			shop.OnSelect = (Action<DeliveryShop>)Delegate.Combine(shop.OnSelect, new Action<DeliveryShop>(CloseShop));
			shopElement.Shop.Initialize();
		}
		for (int num = 0; num < NetworkSingleton<DeliveryManager>.Instance.Deliveries.Count; num++)
		{
			CreateDeliveryStatusDisplay(NetworkSingleton<DeliveryManager>.Instance.Deliveries[num]);
		}
		_tabController.SubscribeToTabSelected(OnTabChange);
		_tabController.SetTab(0, instantIndicatorMove: true);
		Singleton<ConfigurationService>.Instance.TryGetConfiguration<DeliveryConfiguration>(out var configuration);
		int value = ((SettingsField<int>)(object)configuration.Settings.OrderHistoryLength).Value;
		_pastDeliveries = new DeliveryReceiptDisplay[value];
		for (int num2 = 0; num2 < value; num2++)
		{
			_pastDeliveries[num2] = Object.Instantiate<DeliveryReceiptDisplay>(_deliveryReceiptPrefab, (Transform)(object)PastDeliveriesContainer);
			_pastDeliveries[num2].Initialise();
			((Component)_pastDeliveries[num2]).gameObject.SetActive(false);
			_pastDeliveries[num2].SubscribeToOnSelect(Reorder);
		}
		RectTransform component = ((Component)orderCanvas).GetComponent<RectTransform>();
		RectTransform component2 = ((Component)shopListCanvas).GetComponent<RectTransform>();
		_shopPanels = new List<RectTransform> { component, component2 };
		_shopPanelInitialAnchors = _shopPanels.Select((RectTransform p) => p.anchoredPosition).ToList();
		UpdatePastDeliveries();
	}

	public void OpenShop(DeliveryShop shop)
	{
		SetCanvasInteraction(shopListCanvas, interactable: false);
		((Component)shop).gameObject.SetActive(true);
		RefreshContent();
		shop.RefreshShop();
		shop.Open();
		Action onComplete = delegate
		{
			SetCanvasInteraction(orderCanvas, interactable: true);
			((Component)shop).gameObject.SetActive(true);
		};
		if (_shopTransitionCoroutine != null)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(_shopTransitionCoroutine);
		}
		_shopTransitionCoroutine = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(DoShopTransitionRoutine(shopTransitionDuration, -1, _shopPanels, onComplete));
	}

	public void CloseShop(DeliveryShop shop)
	{
		SetCanvasInteraction(orderCanvas, interactable: false);
		shop.Close();
		Action onComplete = delegate
		{
			SetCanvasInteraction(shopListCanvas, interactable: true);
			((Component)shop).gameObject.SetActive(false);
		};
		if (_shopTransitionCoroutine != null)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(_shopTransitionCoroutine);
		}
		_shopTransitionCoroutine = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(DoShopTransitionRoutine(shopTransitionDuration, 1, _shopPanels, onComplete));
	}

	private IEnumerator DoShopTransitionRoutine(float duration, int direction, List<RectTransform> panels, Action onComplete)
	{
		float elapsedTime = 0f;
		List<Vector2> startPos = panels.Select((RectTransform p) => p.anchoredPosition).ToList();
		List<Vector2> targetPos = new List<Vector2>();
		foreach (Vector2 shopPanelInitialAnchor in _shopPanelInitialAnchors)
		{
			Vector2 item = ((direction < 0) ? (shopPanelInitialAnchor + new Vector2(0f - shopPanelWidth, 0f)) : shopPanelInitialAnchor);
			targetPos.Add(item);
		}
		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			float num = elapsedTime / duration;
			for (int num2 = 0; num2 < panels.Count; num2++)
			{
				panels[num2].anchoredPosition = Vector2.Lerp(startPos[num2], targetPos[num2], num);
			}
			yield return null;
		}
		for (int num3 = 0; num3 < panels.Count; num3++)
		{
			panels[num3].anchoredPosition = targetPos[num3];
		}
		onComplete?.Invoke();
		_shopTransitionCoroutine = null;
	}

	public override void Exit(ExitAction exit)
	{
		if (!base.isOpen || exit.Used || !PlayerSingleton<Phone>.Instance.IsOpen)
		{
			base.Exit(exit);
			return;
		}
		if (_tabController.CurrentTabIndex == 0)
		{
			foreach (DeliveryShop deliveryShop in deliveryShops)
			{
				if (deliveryShop.IsOpen)
				{
					CloseShop(deliveryShop);
					exit.Use();
					break;
				}
			}
		}
		base.Exit(exit);
	}

	private void SetCanvasInteraction(CanvasGroup canvas, bool interactable)
	{
		canvas.blocksRaycasts = interactable;
	}

	public override void SetOpen(bool open)
	{
		base.SetOpen(open);
		if (!open)
		{
			return;
		}
		_tabController.SetToSelectedTab(instantIndicatorMove: true);
		foreach (DeliveryStatusDisplay statusDisplay in statusDisplays)
		{
			statusDisplay.RefreshStatus();
		}
		if (MainScrollRect.verticalNormalizedPosition > 1f)
		{
			MainScrollRect.verticalNormalizedPosition = 1f;
		}
		((Component)OrderSubmittedAnim).GetComponent<CanvasGroup>().alpha = 0f;
	}

	private void OnMinPass()
	{
		if (!base.isOpen)
		{
			return;
		}
		foreach (DeliveryStatusDisplay statusDisplay in statusDisplays)
		{
			statusDisplay.RefreshStatus();
		}
		RefreshNotifications();
	}

	public void RefreshContent(bool keepScrollPosition = true)
	{
		float scrollPos = MainScrollRect.verticalNormalizedPosition;
		((MonoBehaviour)this).StartCoroutine(Delay());
		IEnumerator Delay()
		{
			RefreshLayoutGroupsImmediateAndRecursive(((Component)MainLayoutGroup).gameObject);
			if (keepScrollPosition)
			{
				MainScrollRect.verticalNormalizedPosition = scrollPos;
			}
			yield return (object)new WaitForEndOfFrame();
			RefreshLayoutGroupsImmediateAndRecursive(((Component)MainLayoutGroup).gameObject);
			if (keepScrollPosition)
			{
				MainScrollRect.verticalNormalizedPosition = scrollPos;
			}
			yield return (object)new WaitForEndOfFrame();
		}
	}

	public void OnSubmitOrder(DeliveryShop shop)
	{
		CloseShop(shop);
		if (base.isOpen)
		{
			_tabController.SetTab(1, instantIndicatorMove: true);
		}
	}

	public void PlayOrderSubmittedAnim()
	{
		OrderSubmittedAnim.Play();
		OrderSubmittedSound.Play();
	}

	public void Reorder(DeliveryReceipt receipt)
	{
		if (!CanReorder(receipt, out var _))
		{
			UpdatePastDeliveries();
			return;
		}
		DeliveryShop shop = GetShop(receipt.StoreName);
		if ((Object)(object)shop == (Object)null)
		{
			Debug.LogError((object)("No matching shop found for receipt store name " + receipt.StoreName));
		}
		else
		{
			shop.Reorder(receipt);
		}
	}

	public bool CanReorder(DeliveryReceipt receipt, out string reason)
	{
		reason = "";
		DeliveryShop shop = GetShop(receipt.StoreName);
		if ((Object)(object)shop == (Object)null)
		{
			Debug.LogError((object)("No matching shop found for receipt store name " + receipt.StoreName));
			return false;
		}
		return shop.CanReorder(receipt, out reason);
	}

	public float GetDeliveryCost(DeliveryReceipt receipt)
	{
		DeliveryShop shop = GetShop(receipt.StoreName);
		if ((Object)(object)shop == (Object)null)
		{
			Debug.LogError((object)("No matching shop found for receipt store name " + receipt.StoreName));
			return 0f;
		}
		return shop.GetDeliveryCost(receipt);
	}

	private void CreateDeliveryStatusDisplay(DeliveryInstance instance)
	{
		DeliveryStatusDisplay deliveryStatusDisplay = Object.Instantiate<DeliveryStatusDisplay>(StatusDisplayPrefab, (Transform)(object)StatusDisplayContainer);
		deliveryStatusDisplay.AssignDelivery(instance);
		deliveryStatusDisplay.Flash();
		statusDisplays.Add(deliveryStatusDisplay);
		SortStatusDisplays();
		RefreshContent();
		RefreshNoDeliveriesIndicator();
		RefreshNotifications();
	}

	private void DeliveryCompleted(DeliveryInstance instance)
	{
		DeliveryStatusDisplay deliveryStatusDisplay = statusDisplays.FirstOrDefault((DeliveryStatusDisplay d) => d.DeliveryInstance.DeliveryID == instance.DeliveryID);
		if ((Object)(object)deliveryStatusDisplay != (Object)null)
		{
			statusDisplays.Remove(deliveryStatusDisplay);
			Object.Destroy((Object)(object)((Component)deliveryStatusDisplay).gameObject);
		}
		SortStatusDisplays();
		RefreshNoDeliveriesIndicator();
		RefreshNotifications();
	}

	private void SortStatusDisplays()
	{
		statusDisplays = statusDisplays.OrderBy((DeliveryStatusDisplay d) => d.DeliveryInstance.GetTimeStatus()).ToList();
		for (int num = 0; num < statusDisplays.Count; num++)
		{
			((Component)statusDisplays[num]).transform.SetSiblingIndex(num);
		}
	}

	private void RefreshNoDeliveriesIndicator()
	{
		NoDeliveriesIndicator.gameObject.SetActive(statusDisplays.Count == 0);
	}

	public static void RefreshLayoutGroupsImmediateAndRecursive(GameObject root)
	{
		LayoutGroup[] componentsInChildren = root.GetComponentsInChildren<LayoutGroup>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(((Component)componentsInChildren[i]).GetComponent<RectTransform>());
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(((Component)root.GetComponent<LayoutGroup>()).GetComponent<RectTransform>());
	}

	public DeliveryShop GetShop(string shopName)
	{
		return deliveryShops.Find((DeliveryShop x) => x.MatchingShop.ShopName == shopName);
	}

	public void SetIsAvailable(ShopInterface matchingShop, bool available)
	{
		DeliveryShopElement deliveryShopElement = _shopElements.Find((DeliveryShopElement x) => (Object)(object)x.Shop.MatchingShop == (Object)(object)matchingShop);
		Debug.Log((object)("Setting delivery shop " + matchingShop.ShopName + " availability to " + available));
		((Component)deliveryShopElement.Button).gameObject.SetActive(available);
	}

	private void OnTabChange(int index)
	{
		if (index == 2)
		{
			UpdatePastDeliveries();
		}
		((Component)OrderSubmittedAnim).GetComponent<CanvasGroup>().alpha = 0f;
	}

	private void UpdatePastDeliveries()
	{
		List<DeliveryReceipt> displayedDeliveryHistory = NetworkSingleton<DeliveryManager>.Instance.DisplayedDeliveryHistory;
		displayedDeliveryHistory.Reverse();
		bool flag = false;
		for (int i = 0; i < _pastDeliveries.Length; i++)
		{
			bool flag2 = i < displayedDeliveryHistory.Count && IsValidReceipt(displayedDeliveryHistory[i]);
			((Component)_pastDeliveries[i]).gameObject.SetActive(flag2);
			if (flag2)
			{
				flag = true;
				DeliveryReceipt receipt = displayedDeliveryHistory[i];
				float deliveryCost = GetDeliveryCost(receipt);
				bool canAfford = deliveryCost <= NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance;
				_pastDeliveries[i].Set(receipt, deliveryCost, canAfford);
				string reason;
				bool flag3 = CanReorder(receipt, out reason);
				((Selectable)_pastDeliveries[i].ReorderButton).interactable = flag3;
				_pastDeliveries[i].SetActiveTooltip(!flag3);
				if (!flag3)
				{
					_pastDeliveries[i].SetTooltip(reason);
				}
			}
		}
		NoPastDeliveriesIndicator.SetActive(!flag);
	}

	private bool IsValidReceipt(DeliveryReceipt receipt)
	{
		return (receipt != null) & !string.IsNullOrEmpty(receipt.DeliveryID) & !NetworkSingleton<DeliveryManager>.Instance.Deliveries.Any((DeliveryInstance d) => d.DeliveryID == receipt.DeliveryID);
	}

	private void RefreshNotifications()
	{
		int num = NetworkSingleton<DeliveryManager>.Instance.Deliveries.Count((DeliveryInstance x) => x.Status == EDeliveryStatus.Arrived || x.Status == EDeliveryStatus.Waiting);
		SetNotificationCount(num);
		if (num > 0)
		{
			_tabController.SetTabIndicatorText(1, num.ToString());
		}
		else
		{
			_tabController.HideTabIndicator(1);
		}
	}
}
