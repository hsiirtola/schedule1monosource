using System;
using System.Collections;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.UI.Shop;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne;

public class UIPopupScreen_ModifyAmountMenu : UIPopupScreen
{
	public enum ModifyAmountMenuMode
	{
		Store
	}

	[SerializeField]
	private TMP_Text titleText;

	[SerializeField]
	private TMP_Text topMessageText;

	[SerializeField]
	private TMP_Text bottomMessageText;

	[SerializeField]
	private TMP_InputField amountInputField;

	[SerializeField]
	private Image itemImage;

	[SerializeField]
	private TMP_Text itemNameText;

	[SerializeField]
	private TMP_Text itemCostText;

	[SerializeField]
	private UITrigger confirmButton;

	[SerializeField]
	private UITrigger cancelButton;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private Button tier1DecreaseButton;

	[SerializeField]
	private Button tier2DecreaseButton;

	[SerializeField]
	private Button tier3DecreaseButton;

	[SerializeField]
	private Button tier1IncreaseButton;

	[SerializeField]
	private Button tier2IncreaseButton;

	[SerializeField]
	private Button tier3IncreaseButton;

	[SerializeField]
	private TMP_Text tier1DecreaseText;

	[SerializeField]
	private TMP_Text tier2DecreaseText;

	[SerializeField]
	private TMP_Text tier3DecreaseText;

	[SerializeField]
	private TMP_Text tier1IncreaseText;

	[SerializeField]
	private TMP_Text tier2IncreaseText;

	[SerializeField]
	private TMP_Text tier3IncreaseText;

	[SerializeField]
	private float holdThreshold = 0.5f;

	[SerializeField]
	private float repeatInterval = 0.1f;

	private UIInputDetectBehaviour tier1InputDetect;

	private UIInputDetectBehaviour tier2InputDetect;

	private UIInputDetectBehaviour tier3InputDetect;

	private ModifyAmountMenuMode modifyAmountMenuMode;

	private float itemPrice;

	private float minAmount;

	private float tier1Amount = 1f;

	private float tier2Amount = 10f;

	private float tier3Amount = 100f;

	protected override void OnAwake()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Expected O, but got Unknown
		base.OnAwake();
		((UnityEvent)tier1DecreaseButton.onClick).AddListener((UnityAction)delegate
		{
			ChangeCurrentAmount(0f - tier1Amount);
		});
		((UnityEvent)tier2DecreaseButton.onClick).AddListener((UnityAction)delegate
		{
			ChangeCurrentAmount(0f - tier2Amount);
		});
		((UnityEvent)tier3DecreaseButton.onClick).AddListener((UnityAction)delegate
		{
			ChangeCurrentAmount(0f - tier3Amount);
		});
		((UnityEvent)tier1IncreaseButton.onClick).AddListener((UnityAction)delegate
		{
			ChangeCurrentAmount(tier1Amount);
		});
		((UnityEvent)tier2IncreaseButton.onClick).AddListener((UnityAction)delegate
		{
			ChangeCurrentAmount(tier2Amount);
		});
		((UnityEvent)tier3IncreaseButton.onClick).AddListener((UnityAction)delegate
		{
			ChangeCurrentAmount(tier3Amount);
		});
		tier1InputDetect = new UIInputDetectBehaviour();
		tier1InputDetect.Initialize(ChangeCurrentAmountBasedOnInputDetectTier1, holdThreshold, repeatInterval);
		tier2InputDetect = new UIInputDetectBehaviour();
		tier2InputDetect.Initialize(ChangeCurrentAmountBasedOnInputDetectTier2, holdThreshold, repeatInterval);
		tier3InputDetect = new UIInputDetectBehaviour();
		tier3InputDetect.Initialize(ChangeCurrentAmountBasedOnInputDetectTier3, holdThreshold, repeatInterval);
		((UnityEvent<string>)(object)amountInputField.onValueChanged).AddListener((UnityAction<string>)delegate(string value)
		{
			if (float.TryParse(value, out var result))
			{
				CapAmount(result);
			}
		});
	}

	protected override void OnStarted()
	{
		base.OnStarted();
	}

	protected override void Update()
	{
		base.Update();
		tier1InputDetect.DoUpdate(GameInput.UIModifyAmountIncrementTierOneAxis);
		tier2InputDetect.DoUpdate(GameInput.UIModifyAmountIncrementTierTwoAxis);
		tier3InputDetect.DoUpdate(GameInput.UIModifyAmountIncrementTierThreeAxis);
	}

	public override void Close()
	{
		Singleton<UIScreenManager>.Instance.RemoveScreen(this);
		((Behaviour)canvas).enabled = false;
		((UnityEventBase)confirmButton.OnTrigger).RemoveAllListeners();
		((UnityEventBase)cancelButton.OnTrigger).RemoveAllListeners();
	}

	private void Open()
	{
		Singleton<UIScreenManager>.Instance.AddScreen(this, Close);
		((Behaviour)canvas).enabled = true;
		tier1InputDetect.ResetData();
		tier2InputDetect.ResetData();
		tier3InputDetect.ResetData();
		if (GameInput.GetCurrentInputDeviceIsKeyboardMouse())
		{
			((MonoBehaviour)this).StartCoroutine(SelectInputField());
		}
	}

	private IEnumerator SelectInputField()
	{
		yield return null;
		((Selectable)amountInputField).Select();
		amountInputField.ActivateInputField();
		amountInputField.MoveTextEnd(false);
		amountInputField.MoveTextStart(true);
	}

	public override void Open(params object[] args)
	{
		string text = ((args.Length != 0 && args[0] is string) ? ((string)args[0]) : "Modify Amount");
		string text2 = ((args.Length > 1 && args[1] is string) ? ((string)args[1]) : "");
		string text3 = ((args.Length > 2 && args[2] is string) ? ((string)args[2]) : "");
		float currentAmount = ((args.Length > 3 && args[3] is float) ? ((float)args[3]) : 0f);
		Action<float> onConfirm = ((args.Length > 4 && args[4] is Action<float>) ? ((Action<float>)args[4]) : null);
		Action onCancel = ((args.Length > 5 && args[5] is Action) ? ((Action)args[5]) : null);
		modifyAmountMenuMode = ((args.Length > 6 && args[6] is ModifyAmountMenuMode) ? ((ModifyAmountMenuMode)args[6]) : ModifyAmountMenuMode.Store);
		minAmount = ((args.Length > 7 && args[7] is float) ? ((float)args[7]) : 0f);
		tier1Amount = ((args.Length > 8 && args[8] is float) ? ((float)args[8]) : 0f);
		tier2Amount = ((args.Length > 9 && args[9] is float) ? ((float)args[9]) : 0f);
		tier3Amount = ((args.Length > 10 && args[10] is float) ? ((float)args[10]) : 0f);
		amountInputField.characterLimit = ((args.Length > 11 && args[11] is int) ? ((int)args[11]) : 0);
		bottomMessageText.text = text3;
		if (modifyAmountMenuMode == ModifyAmountMenuMode.Store)
		{
			ShopListing shopListing = ((args.Length > 12 && args[12] is ShopListing) ? ((ShopListing)args[12]) : null);
			if (shopListing != null)
			{
				itemImage.sprite = ((BaseItemDefinition)shopListing.Item).Icon;
				itemNameText.text = ((BaseItemDefinition)shopListing.Item).Name;
				itemCostText.text = $"Cost: ${shopListing.Price:F0}";
				itemPrice = shopListing.Price;
			}
			UpdateStoreBottomMessage();
		}
		titleText.text = text;
		topMessageText.text = text2;
		tier1DecreaseText.text = $"-{tier1Amount:F0}";
		tier2DecreaseText.text = $"-{tier2Amount:F0}";
		tier3DecreaseText.text = $"-{tier3Amount:F0}";
		tier1IncreaseText.text = $"+{tier1Amount:F0}";
		tier2IncreaseText.text = $"+{tier2Amount:F0}";
		tier3IncreaseText.text = $"+{tier3Amount:F0}";
		SetCurrentAmount(currentAmount);
		((MonoBehaviour)this).StartCoroutine(RegisterInput(onConfirm, onCancel));
		Open();
	}

	private IEnumerator RegisterInput(Action<float> onConfirm, Action onCancel)
	{
		yield return null;
		if ((Object)(object)confirmButton != (Object)null)
		{
			confirmButton.OnTrigger.AddListener((UnityAction)delegate
			{
				if (float.TryParse(amountInputField.text, out var result))
				{
					onConfirm?.Invoke(result);
				}
				else
				{
					Debug.LogWarning((object)("Invalid amount entered: " + amountInputField.text));
				}
				Close();
			});
		}
		if ((Object)(object)cancelButton != (Object)null)
		{
			cancelButton.OnTrigger.AddListener((UnityAction)delegate
			{
				onCancel?.Invoke();
				Close();
			});
		}
	}

	private void UpdateStoreBottomMessage()
	{
		if (modifyAmountMenuMode == ModifyAmountMenuMode.Store)
		{
			bottomMessageText.text = $"Total Cost: ${itemPrice * GetCurrentAmount():F0}";
		}
	}

	private float GetCurrentAmount()
	{
		if (float.TryParse(amountInputField.text, out var result))
		{
			return result;
		}
		return 0f;
	}

	private void ChangeCurrentAmountBasedOnInputDetectTier1(float inputValue)
	{
		ChangeCurrentAmountBasedOnInputDetect(inputValue, tier1Amount);
	}

	private void ChangeCurrentAmountBasedOnInputDetectTier2(float inputValue)
	{
		ChangeCurrentAmountBasedOnInputDetect(inputValue, tier2Amount);
	}

	private void ChangeCurrentAmountBasedOnInputDetectTier3(float inputValue)
	{
		ChangeCurrentAmountBasedOnInputDetect(inputValue, tier3Amount);
	}

	private void ChangeCurrentAmountBasedOnInputDetect(float inputValue, float tierAmount)
	{
		if (inputValue > 0f)
		{
			ChangeCurrentAmount(tierAmount);
		}
		else if (inputValue < 0f)
		{
			ChangeCurrentAmount(0f - tierAmount);
		}
	}

	private void ChangeCurrentAmount(float increment)
	{
		float currentAmount = GetCurrentAmount();
		currentAmount += increment;
		SetCurrentAmount(currentAmount);
	}

	private void SetCurrentAmount(float amount)
	{
		if (amount < minAmount)
		{
			amount = minAmount;
		}
		amountInputField.text = amount.ToString("F0");
		UpdateStoreBottomMessage();
	}

	private void CapAmount(float amount)
	{
		if (amount < minAmount)
		{
			amount = minAmount;
		}
		SetCurrentAmount(amount);
	}
}
