using System;
using System.Collections;
using System.Collections.Generic;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.Money;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.UI.ATM;

public class ATMInterface : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	protected Canvas canvas;

	[SerializeField]
	protected ScheduleOne.Money.ATM atm;

	[SerializeField]
	protected AudioSourceController CompleteSound;

	[Header("Menu")]
	[SerializeField]
	protected RectTransform menuScreen;

	[SerializeField]
	protected Text menu_TitleText;

	[SerializeField]
	protected Button menu_DepositButton;

	[SerializeField]
	protected Button menu_WithdrawButton;

	[Header("Top bar")]
	[SerializeField]
	protected Text depositLimitText;

	[SerializeField]
	protected Text onlineBalanceText;

	[SerializeField]
	protected Text cleanCashText;

	[SerializeField]
	protected RectTransform depositLimitContainer;

	[Header("Amount screen")]
	[SerializeField]
	protected RectTransform amountSelectorScreen;

	[SerializeField]
	protected Text amountSelectorTitle;

	[SerializeField]
	protected List<Button> amountButtons = new List<Button>();

	[SerializeField]
	protected Text amountLabelText;

	[SerializeField]
	protected RectTransform amountBackground;

	[SerializeField]
	protected RectTransform selectedButtonIndicator;

	[SerializeField]
	protected Button confirmAmountButton;

	[SerializeField]
	protected Text confirmButtonText;

	[Header("Processing screen")]
	[SerializeField]
	protected RectTransform processingScreen;

	[SerializeField]
	protected RectTransform processingScreenIndicator;

	[Header("Success screen")]
	[SerializeField]
	protected RectTransform successScreen;

	[SerializeField]
	protected Text successScreenSubtitle;

	[SerializeField]
	protected Button doneButton;

	[Header("Custom UI")]
	[SerializeField]
	protected UIScreen UIScreen;

	[SerializeField]
	protected UIContentPanel MenuPanel;

	[SerializeField]
	protected UIContentPanel AmountSelectorPanel;

	[SerializeField]
	protected UIContentPanel SuccessPanel;

	private RectTransform activeScreen;

	public static int[] amounts = new int[6] { 20, 50, 100, 500, 1000, 5000 };

	private bool depositing = true;

	private int selectedAmountIndex;

	private float selectedAmount;

	public bool isOpen { get; protected set; }

	private float relevantBalance
	{
		get
		{
			if (!depositing)
			{
				return NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance;
			}
			return NetworkSingleton<MoneyManager>.Instance.cashBalance;
		}
	}

	private static float remainingAllowedDeposit => 10000f - ScheduleOne.Money.ATM.WeeklyDepositSum;

	private void Awake()
	{
		Player.onLocalPlayerSpawned = (Action)Delegate.Remove(Player.onLocalPlayerSpawned, new Action(PlayerSpawned));
		Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(PlayerSpawned));
	}

	private void OnDestroy()
	{
		Player.onLocalPlayerSpawned = (Action)Delegate.Remove(Player.onLocalPlayerSpawned, new Action(PlayerSpawned));
	}

	protected virtual void Start()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		GameInput.RegisterExitListener(Exit, 2);
		activeScreen = menuScreen;
		((Behaviour)canvas).enabled = false;
		for (int i = 0; i < amountButtons.Count; i++)
		{
			int cachedIndex = i;
			((UnityEvent)amountButtons[i].onClick).AddListener((UnityAction)delegate
			{
				AmountSelected(cachedIndex);
			});
			if (i == amountButtons.Count - 1)
			{
				((Component)((Component)amountButtons[i]).transform.Find("Text")).GetComponent<Text>().text = "ALL ()";
			}
			else
			{
				((Component)((Component)amountButtons[i]).transform.Find("Text")).GetComponent<Text>().text = MoneyManager.FormatAmount(amounts[i]);
			}
		}
		((Component)depositLimitContainer).gameObject.SetActive(true);
	}

	private void PlayerSpawned()
	{
		canvas.worldCamera = PlayerSingleton<PlayerCamera>.Instance.Camera;
	}

	protected virtual void Update()
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		if (!isOpen)
		{
			return;
		}
		onlineBalanceText.text = MoneyManager.FormatAmount(NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance);
		cleanCashText.text = MoneyManager.FormatAmount(NetworkSingleton<MoneyManager>.Instance.cashBalance);
		depositLimitText.text = MoneyManager.FormatAmount(ScheduleOne.Money.ATM.WeeklyDepositSum) + " / " + MoneyManager.FormatAmount(10000f);
		if (ScheduleOne.Money.ATM.WeeklyDepositSum >= 10000f)
		{
			((Graphic)depositLimitText).color = Color32.op_Implicit(new Color32(byte.MaxValue, (byte)75, (byte)75, byte.MaxValue));
		}
		else
		{
			((Graphic)depositLimitText).color = Color.white;
		}
		if ((Object)(object)activeScreen == (Object)(object)amountSelectorScreen)
		{
			if (depositing)
			{
				((Component)((Component)amountButtons[amountButtons.Count - 1]).transform.Find("Text")).GetComponent<Text>().text = "MAX (" + MoneyManager.FormatAmount(Mathf.Min(NetworkSingleton<MoneyManager>.Instance.cashBalance, remainingAllowedDeposit)) + ")";
			}
			UpdateAvailableAmounts();
			((Selectable)confirmAmountButton).interactable = relevantBalance > 0f;
			if (depositing)
			{
				if (selectedAmountIndex == amounts.Length)
				{
					confirmButtonText.text = "DEPOSIT ALL";
				}
				else
				{
					confirmButtonText.text = "DEPOSIT " + MoneyManager.FormatAmount(selectedAmount);
				}
			}
			else
			{
				confirmButtonText.text = "WITHDRAW " + MoneyManager.FormatAmount(selectedAmount);
			}
			if (relevantBalance < GetAmountFromIndex(selectedAmountIndex, depositing))
			{
				DefaultAmountSelection();
			}
		}
		if ((Object)(object)activeScreen == (Object)(object)menuScreen)
		{
			((Selectable)menu_DepositButton).interactable = ScheduleOne.Money.ATM.WeeklyDepositSum < 10000f;
		}
		if ((Object)(object)activeScreen == (Object)(object)processingScreen)
		{
			((Transform)processingScreenIndicator).localEulerAngles = new Vector3(0f, 0f, ((Transform)processingScreenIndicator).localEulerAngles.z - Time.deltaTime * 360f);
		}
	}

	protected virtual void LateUpdate()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (isOpen && (Object)(object)activeScreen == (Object)(object)amountSelectorScreen)
		{
			if (selectedAmountIndex == -1)
			{
				((Component)selectedButtonIndicator).gameObject.SetActive(false);
				return;
			}
			selectedButtonIndicator.anchoredPosition = ((Component)amountButtons[selectedAmountIndex]).GetComponent<RectTransform>().anchoredPosition;
			((Component)selectedButtonIndicator).gameObject.SetActive(true);
		}
	}

	public virtual void SetIsOpen(bool o)
	{
		if (o != isOpen)
		{
			isOpen = o;
			((Behaviour)canvas).enabled = isOpen;
			EventSystem.current.SetSelectedGameObject((GameObject)null);
			if (isOpen)
			{
				PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
				Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
				PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
				Singleton<UIScreenManager>.Instance.AddScreen(UIScreen);
				SetActiveScreen(menuScreen);
			}
			else
			{
				atm.Exit();
				PlayerSingleton<PlayerCamera>.Instance.LockMouse();
				Singleton<HUD>.Instance.SetCrosshairVisible(vis: true);
				Singleton<UIScreenManager>.Instance.RemoveScreen(UIScreen);
				PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
			}
		}
	}

	public virtual void Exit(ExitAction action)
	{
		if (!action.Used && isOpen && action.exitType == ExitType.Escape)
		{
			action.Used = true;
			if ((Object)(object)activeScreen == (Object)(object)menuScreen || (Object)(object)activeScreen == (Object)(object)successScreen)
			{
				SetIsOpen(o: false);
			}
			else if ((Object)(object)activeScreen == (Object)(object)amountSelectorScreen)
			{
				SetActiveScreen(menuScreen);
			}
		}
	}

	public void SetActiveScreen(RectTransform screen)
	{
		((Component)menuScreen).gameObject.SetActive(false);
		((Component)amountSelectorScreen).gameObject.SetActive(false);
		((Component)processingScreen).gameObject.SetActive(false);
		((Component)successScreen).gameObject.SetActive(false);
		activeScreen = screen;
		((Component)activeScreen).gameObject.SetActive(true);
		if ((Object)(object)activeScreen == (Object)(object)menuScreen)
		{
			menu_TitleText.text = "Hello, " + Player.Local.PlayerName;
			UIScreen.SetCurrentSelectedPanel(MenuPanel);
			((Selectable)menu_DepositButton).Select();
		}
		else if ((Object)(object)activeScreen == (Object)(object)amountSelectorScreen)
		{
			UpdateAvailableAmounts();
			UIScreen.SetCurrentSelectedPanel(AmountSelectorPanel);
			DefaultAmountSelection();
		}
		else if ((Object)(object)activeScreen == (Object)(object)successScreen)
		{
			UIScreen.SetCurrentSelectedPanel(SuccessPanel);
			((Selectable)doneButton).Select();
		}
	}

	private void DefaultAmountSelection()
	{
		if (((Selectable)amountButtons[0]).interactable)
		{
			((Selectable)amountButtons[0]).Select();
			AmountSelected(0);
			return;
		}
		if (((Selectable)amountButtons[amountButtons.Count - 1]).interactable && relevantBalance > 0f)
		{
			((Selectable)amountButtons[amountButtons.Count - 1]).Select();
			AmountSelected(amountButtons.Count - 1);
			return;
		}
		AmountSelected(-1);
		for (int i = 0; i < amountButtons.Count; i++)
		{
		}
	}

	public void DepositButtonPressed()
	{
		amountSelectorTitle.text = "Select amount to deposit";
		depositing = true;
		SetActiveScreen(amountSelectorScreen);
	}

	public void WithdrawButtonPressed()
	{
		amountSelectorTitle.text = "Select amount to withdraw";
		depositing = false;
		((Component)((Component)amountButtons[amountButtons.Count - 1]).transform.Find("Text")).GetComponent<Text>().text = MoneyManager.FormatAmount(amounts[amounts.Length - 1]);
		SetActiveScreen(amountSelectorScreen);
	}

	public void CancelAmountSelection()
	{
		SetActiveScreen(menuScreen);
	}

	public void AmountSelected(int amountIndex)
	{
		selectedAmountIndex = amountIndex;
		SetSelectedAmount(GetAmountFromIndex(amountIndex, depositing));
	}

	private void SetSelectedAmount(float amount)
	{
		float num = 0f;
		num = ((!depositing) ? NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance : Mathf.Min(NetworkSingleton<MoneyManager>.Instance.cashBalance, remainingAllowedDeposit));
		selectedAmount = Mathf.Clamp(amount, 0f, num);
		amountLabelText.text = MoneyManager.FormatAmount(selectedAmount);
	}

	public static float GetAmountFromIndex(int index, bool depositing)
	{
		if (index == -1 || index >= amounts.Length)
		{
			return 0f;
		}
		if (depositing && index == amounts.Length - 1)
		{
			return Mathf.Min(NetworkSingleton<MoneyManager>.Instance.cashBalance, remainingAllowedDeposit);
		}
		return amounts[index];
	}

	private void UpdateAvailableAmounts()
	{
		for (int i = 0; i < amounts.Length; i++)
		{
			if (depositing && i == amounts.Length - 1)
			{
				((Selectable)amountButtons[amountButtons.Count - 1]).interactable = relevantBalance > 0f && remainingAllowedDeposit > 0f;
				break;
			}
			if (depositing)
			{
				((Selectable)amountButtons[i]).interactable = relevantBalance >= (float)amounts[i] && ScheduleOne.Money.ATM.WeeklyDepositSum + (float)amounts[i] <= 10000f;
			}
			else
			{
				((Selectable)amountButtons[i]).interactable = relevantBalance >= (float)amounts[i];
			}
		}
	}

	public void AmountConfirmed()
	{
		((MonoBehaviour)this).StartCoroutine(ProcessTransaction(selectedAmount, depositing));
	}

	public void ChangeAmount(float amount)
	{
		selectedAmountIndex = -1;
		SetSelectedAmount(selectedAmount + amount);
	}

	protected IEnumerator ProcessTransaction(float amount, bool depositing)
	{
		SetActiveScreen(processingScreen);
		yield return (object)new WaitForSeconds(1f);
		CompleteSound.Play();
		if (depositing)
		{
			if (NetworkSingleton<MoneyManager>.Instance.cashBalance >= amount)
			{
				NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - amount);
				NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction("Cash Deposit", amount, 1f, string.Empty);
				ScheduleOne.Money.ATM.WeeklyDepositSum += amount;
				successScreenSubtitle.text = "You have deposited " + MoneyManager.FormatAmount(amount);
				SetActiveScreen(successScreen);
			}
			else
			{
				SetActiveScreen(menuScreen);
			}
		}
		else if (NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance >= amount)
		{
			NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(amount);
			NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction("Cash Withdrawal", 0f - amount, 1f, string.Empty);
			successScreenSubtitle.text = "You have withdrawn " + MoneyManager.FormatAmount(amount);
			SetActiveScreen(successScreen);
		}
		else
		{
			SetActiveScreen(menuScreen);
		}
	}

	public void DoneButtonPressed()
	{
		SetIsOpen(o: false);
	}

	public void ReturnToMenuButtonPressed()
	{
		SetActiveScreen(menuScreen);
	}
}
