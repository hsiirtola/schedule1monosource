using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Handover;

public class HandoverScreenPriceSelector : MonoBehaviour
{
	public const float MinPrice = 1f;

	public const float MaxPrice = 9999f;

	public InputField InputField;

	public UnityEvent onPriceChanged;

	public float Price { get; private set; } = 1f;

	public void SetPrice(float price)
	{
		Price = Mathf.Clamp(price, 1f, 9999f);
		InputField.SetTextWithoutNotify(Price.ToString());
		if (onPriceChanged != null)
		{
			onPriceChanged.Invoke();
		}
	}

	public void RefreshPrice()
	{
		OnPriceInputChanged(InputField.text);
	}

	public void OnPriceInputChanged(string value)
	{
		if (float.TryParse(value, out var result))
		{
			SetPrice(result);
		}
	}

	public void ChangeAmount(float change)
	{
		SetPrice(Price + change);
	}

	public void ShowOSK()
	{
		OnScreenKeyboard.Show(KBSubmit, KBCancel, "", 32u, Price.ToString());
	}

	private void KBSubmit(string newPrice)
	{
		OnPriceInputChanged(newPrice);
	}

	private void KBCancel()
	{
	}
}
