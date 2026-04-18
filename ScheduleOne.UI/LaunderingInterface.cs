using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Interaction;
using ScheduleOne.Money;
using ScheduleOne.ObjectScripts.Cash;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Property;
using ScheduleOne.Variables;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class LaunderingInterface : MonoBehaviour
{
	protected const float fovOverride = 65f;

	protected const float lerpTime = 0.15f;

	protected const int minLaunderAmount = 10;

	[Header("References")]
	[SerializeField]
	protected Transform cameraPosition;

	[SerializeField]
	protected InteractableObject intObj;

	[SerializeField]
	protected Button launderButton;

	[SerializeField]
	protected GameObject amountSelectorScreen;

	[SerializeField]
	protected Slider amountSlider;

	[SerializeField]
	protected TMP_InputField amountInputField;

	[SerializeField]
	protected RectTransform notchContainer;

	[SerializeField]
	protected TextMeshProUGUI currentTotalAmountLabel;

	[SerializeField]
	protected TextMeshProUGUI launderCapacityLabel;

	[SerializeField]
	protected TextMeshProUGUI insufficientCashLabel;

	[SerializeField]
	protected RectTransform entryContainer;

	[SerializeField]
	protected RectTransform noEntries;

	public CashStackVisuals[] CashStacks;

	[Header("Prefabs")]
	[SerializeField]
	protected GameObject timelineNotchPrefab;

	[SerializeField]
	protected GameObject entryPrefab;

	[Header("UI references")]
	[SerializeField]
	protected Canvas canvas;

	[SerializeField]
	protected ScrollRect scrollRect;

	[SerializeField]
	protected UIScreen UIScreen;

	[SerializeField]
	protected UIPanel mainPanel;

	[SerializeField]
	protected UIScreen selectorScreen;

	[SerializeField]
	protected UIPanel selectorPanel;

	private int selectedAmountToLaunder;

	private Dictionary<LaunderingOperation, RectTransform> operationToNotch = new Dictionary<LaunderingOperation, RectTransform>();

	private List<RectTransform> notches = new List<RectTransform>();

	private bool ignoreSliderChange = true;

	private Dictionary<LaunderingOperation, RectTransform> operationToEntry = new Dictionary<LaunderingOperation, RectTransform>();

	protected int maxLaunderAmount => (int)Mathf.Min(business.appliedLaunderLimit, NetworkSingleton<MoneyManager>.Instance.cashBalance);

	public Business business { get; private set; }

	public bool isOpen
	{
		get
		{
			if ((Object)(object)canvas != (Object)null)
			{
				return ((Component)canvas).gameObject.activeSelf;
			}
			return false;
		}
	}

	public void Initialize(Business bus)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		business = bus;
		intObj.onHovered.AddListener(new UnityAction(Hovered));
		intObj.onInteractStart.AddListener(new UnityAction(Interacted));
		((TMP_Text)launderCapacityLabel).text = MoneyManager.FormatAmount(business.LaunderCapacity);
		((Component)canvas).gameObject.SetActive(false);
		((Component)noEntries).gameObject.SetActive(operationToEntry.Count == 0);
		Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, (Action)delegate
		{
			canvas.worldCamera = PlayerSingleton<PlayerCamera>.Instance.Camera;
		});
		foreach (LaunderingOperation launderingOperation in business.LaunderingOperations)
		{
			CreateEntry(launderingOperation);
		}
		Business.onOperationStarted = (Action<LaunderingOperation>)Delegate.Combine(Business.onOperationStarted, new Action<LaunderingOperation>(CreateEntry));
		Business.onOperationStarted = (Action<LaunderingOperation>)Delegate.Combine(Business.onOperationStarted, new Action<LaunderingOperation>(UpdateCashStacks));
		Business.onOperationFinished = (Action<LaunderingOperation>)Delegate.Combine(Business.onOperationFinished, new Action<LaunderingOperation>(RemoveEntry));
		Business.onOperationFinished = (Action<LaunderingOperation>)Delegate.Combine(Business.onOperationFinished, new Action<LaunderingOperation>(UpdateCashStacks));
		NetworkSingleton<TimeManager>.Instance.onMinutePass += new Action(MinPass);
		CloseAmountSelector();
	}

	private void OnDestroy()
	{
		Business.onOperationStarted = (Action<LaunderingOperation>)Delegate.Remove(Business.onOperationStarted, new Action<LaunderingOperation>(CreateEntry));
		Business.onOperationStarted = (Action<LaunderingOperation>)Delegate.Remove(Business.onOperationStarted, new Action<LaunderingOperation>(UpdateCashStacks));
		Business.onOperationFinished = (Action<LaunderingOperation>)Delegate.Remove(Business.onOperationFinished, new Action<LaunderingOperation>(RemoveEntry));
		Business.onOperationFinished = (Action<LaunderingOperation>)Delegate.Remove(Business.onOperationFinished, new Action<LaunderingOperation>(UpdateCashStacks));
	}

	protected virtual void MinPass()
	{
		if (isOpen)
		{
			UpdateTimeline();
			RefreshLaunderButton();
			UpdateCurrentTotal();
			UpdateEntryTimes();
		}
	}

	protected void Exit(ExitAction exit)
	{
		if (!exit.Used && isOpen)
		{
			if (amountSelectorScreen.gameObject.activeSelf)
			{
				exit.Used = true;
				CloseAmountSelector();
			}
			else if (exit.exitType == ExitType.Escape)
			{
				exit.Used = true;
				Close();
			}
		}
	}

	protected void UpdateTimeline()
	{
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		foreach (LaunderingOperation launderingOperation in business.LaunderingOperations)
		{
			if (!operationToNotch.ContainsKey(launderingOperation))
			{
				RectTransform component = Object.Instantiate<GameObject>(timelineNotchPrefab, (Transform)(object)notchContainer).GetComponent<RectTransform>();
				((TMP_Text)((Component)((Transform)component).Find("Amount")).GetComponent<TextMeshProUGUI>()).text = MoneyManager.FormatAmount(launderingOperation.amount);
				operationToNotch.Add(launderingOperation, component);
				notches.Add(component);
			}
		}
		List<RectTransform> list = (from x in operationToNotch
			where business.LaunderingOperations.Contains(x.Key)
			select x.Value).ToList();
		for (int num = 0; num < notches.Count; num++)
		{
			if (!list.Contains(notches[num]))
			{
				Object.Destroy((Object)(object)((Component)notches[num]).gameObject);
				notches.RemoveAt(num);
				num--;
			}
		}
		foreach (LaunderingOperation launderingOperation2 in business.LaunderingOperations)
		{
			RectTransform obj = operationToNotch[launderingOperation2];
			Rect rect = notchContainer.rect;
			obj.anchoredPosition = new Vector2(((Rect)(ref rect)).width * (float)launderingOperation2.minutesSinceStarted / (float)launderingOperation2.completionTime_Minutes, operationToNotch[launderingOperation2].anchoredPosition.y);
		}
	}

	protected void UpdateCurrentTotal()
	{
		((TMP_Text)currentTotalAmountLabel).text = MoneyManager.FormatAmount(business.currentLaunderTotal);
	}

	private void CreateEntry(LaunderingOperation op)
	{
		if (!operationToEntry.ContainsKey(op))
		{
			RectTransform component = Object.Instantiate<GameObject>(entryPrefab, (Transform)(object)entryContainer).GetComponent<RectTransform>();
			((Transform)component).SetAsLastSibling();
			((TMP_Text)((Component)((Transform)component).Find("BusinessLabel")).GetComponent<TextMeshProUGUI>()).text = op.business.PropertyName;
			((TMP_Text)((Component)((Transform)component).Find("AmountLabel")).GetComponent<TextMeshProUGUI>()).text = MoneyManager.FormatAmount(op.amount);
			operationToEntry.Add(op, component);
			UpdateEntryTimes();
			if ((Object)(object)noEntries != (Object)null)
			{
				((Component)noEntries).gameObject.SetActive(operationToEntry.Count == 0);
			}
		}
	}

	private void RemoveEntry(LaunderingOperation op)
	{
		if (operationToEntry.ContainsKey(op))
		{
			RectTransform val = operationToEntry[op];
			if ((Object)(object)val != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)val).gameObject);
			}
			operationToEntry.Remove(op);
			((Component)noEntries).gameObject.SetActive(operationToEntry.Count == 0);
		}
	}

	private void UpdateEntryTimes()
	{
		foreach (LaunderingOperation item in operationToEntry.Keys.ToList())
		{
			if (!operationToEntry.ContainsKey(item))
			{
				continue;
			}
			if ((Object)(object)operationToEntry[item] == (Object)null)
			{
				Console.LogWarning("Entry is null for operation " + item.business.PropertyName);
				continue;
			}
			int num = item.completionTime_Minutes - item.minutesSinceStarted;
			if (num > 60)
			{
				int num2 = Mathf.CeilToInt((float)num / 60f);
				((TMP_Text)((Component)((Transform)operationToEntry[item]).Find("TimeLabel")).GetComponent<TextMeshProUGUI>()).text = num2 + " hours";
			}
			else
			{
				((TMP_Text)((Component)((Transform)operationToEntry[item]).Find("TimeLabel")).GetComponent<TextMeshProUGUI>()).text = num + " minutes";
			}
		}
	}

	private void UpdateCashStacks(LaunderingOperation op)
	{
		float num = business.currentLaunderTotal;
		for (int i = 0; i < CashStacks.Length; i++)
		{
			if (num <= 0f)
			{
				CashStacks[i].ShowAmount(0f);
				continue;
			}
			float num2 = Mathf.Min(num, 1000f);
			CashStacks[i].ShowAmount(num2);
			num -= num2;
		}
	}

	private void RefreshLaunderButton()
	{
		((Selectable)launderButton).interactable = business.currentLaunderTotal < business.LaunderCapacity && NetworkSingleton<MoneyManager>.Instance.cashBalance > 10f;
		if (business.currentLaunderTotal >= business.LaunderCapacity)
		{
			((TMP_Text)insufficientCashLabel).text = "The business is already at maximum laundering capacity.";
			((Component)insufficientCashLabel).gameObject.SetActive(true);
		}
		else if (NetworkSingleton<MoneyManager>.Instance.cashBalance <= 10f)
		{
			((TMP_Text)insufficientCashLabel).text = "You need at least " + MoneyManager.FormatAmount(10f) + " cash to launder.";
			((Component)insufficientCashLabel).gameObject.SetActive(true);
		}
		else
		{
			((Component)insufficientCashLabel).gameObject.SetActive(false);
		}
	}

	public void OpenAmountSelector()
	{
		amountSelectorScreen.gameObject.SetActive(true);
		Singleton<UIScreenManager>.Instance.AddScreen(selectorScreen, CloseAmountSelector);
		selectorScreen.SetCurrentSelectedPanel(selectorPanel);
		selectorPanel.SelectSelectable(returnFirstFound: true);
		int num = (selectedAmountToLaunder = Mathf.Clamp(100, 10, maxLaunderAmount));
		amountSlider.minValue = 10f;
		amountSlider.maxValue = maxLaunderAmount;
		amountSlider.SetValueWithoutNotify((float)num);
		amountInputField.SetTextWithoutNotify(num.ToString());
	}

	public void CloseAmountSelector()
	{
		amountSelectorScreen.gameObject.SetActive(false);
		Singleton<UIScreenManager>.Instance.RemoveScreen(selectorScreen);
	}

	public void ConfirmAmount()
	{
		int num = Mathf.Clamp(selectedAmountToLaunder, 10, maxLaunderAmount);
		NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(-num);
		business.StartLaunderingOperation(num);
		float value = NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("LaunderingOperationsStarted");
		NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("LaunderingOperationsStarted", (value + 1f).ToString());
		UpdateTimeline();
		UpdateCurrentTotal();
		RefreshLaunderButton();
		CloseAmountSelector();
	}

	public void SliderValueChanged()
	{
		if (ignoreSliderChange)
		{
			ignoreSliderChange = false;
			return;
		}
		selectedAmountToLaunder = (int)amountSlider.value;
		amountInputField.SetTextWithoutNotify(selectedAmountToLaunder.ToString());
	}

	public void InputValueChanged()
	{
		selectedAmountToLaunder = Mathf.Clamp(int.Parse(amountInputField.text), 10, maxLaunderAmount);
		amountInputField.SetTextWithoutNotify(selectedAmountToLaunder.ToString());
		amountSlider.SetValueWithoutNotify((float)selectedAmountToLaunder);
	}

	public void ChangeSelectorValue(int amount)
	{
		amountSlider.SetValueWithoutNotify((float)(selectedAmountToLaunder + amount));
		SliderValueChanged();
	}

	public void Hovered()
	{
		if (!business.IsOwned || isOpen)
		{
			intObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
		}
		else if (business.IsOwned && !isOpen)
		{
			intObj.SetInteractableState(InteractableObject.EInteractableState.Default);
			intObj.SetMessage("Manage business");
		}
	}

	public void Interacted()
	{
		if (business.IsOwned && !isOpen)
		{
			Open();
		}
	}

	public virtual void Open()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		Singleton<InputPromptsCanvas>.Instance.LoadModule("exitonly");
		intObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
		PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(((Component)cameraPosition).transform.position, cameraPosition.rotation, 0.15f);
		PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(65f, 0.15f);
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
		PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
		PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
		Singleton<UIScreenManager>.Instance.AddScreen(UIScreen, Close);
		UIScreen.SetCurrentSelectedPanel(mainPanel);
		UIScreen.ChangeActiveScrollRect(scrollRect);
		RefreshLaunderButton();
		UpdateTimeline();
		UpdateCurrentTotal();
		((Component)this).gameObject.SetActive(true);
	}

	public virtual void Close()
	{
		Singleton<InputPromptsCanvas>.Instance.UnloadModule();
		intObj.SetInteractableState(InteractableObject.EInteractableState.Default);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
		PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0.15f);
		PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0.15f);
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
		UIScreen.ChangeActiveScrollRect(null);
		Singleton<UIScreenManager>.Instance.RemoveScreen(UIScreen);
		PlayerSingleton<PlayerCamera>.Instance.LockMouse();
		PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
		((Component)this).gameObject.SetActive(false);
	}
}
