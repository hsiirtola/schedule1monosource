using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Shop;

public class ShopAmountSelector : MonoBehaviour
{
	[Header("References")]
	public RectTransform Container;

	public TMP_InputField InputField;

	public UnityEvent<int> onSubmitted;

	public bool IsOpen { get; private set; }

	public int SelectedAmount { get; private set; } = 1;

	private void Awake()
	{
		((Component)Container).gameObject.SetActive(false);
		((UnityEvent<string>)(object)InputField.onSubmit).AddListener((UnityAction<string>)OnSubmitted);
		((UnityEvent<string>)(object)InputField.onValueChanged).AddListener((UnityAction<string>)OnValueChanged);
	}

	public void Open()
	{
		((Component)Container).gameObject.SetActive(true);
		((Transform)Container).SetAsLastSibling();
		InputField.text = string.Empty;
		((Selectable)InputField).Select();
		IsOpen = true;
	}

	public void Close()
	{
		((Component)Container).gameObject.SetActive(false);
		IsOpen = false;
	}

	private void OnSubmitted(string value)
	{
		if (IsOpen)
		{
			OnValueChanged(value);
			if (onSubmitted != null)
			{
				onSubmitted.Invoke(SelectedAmount);
			}
			Close();
		}
	}

	private void OnValueChanged(string value)
	{
		if (int.TryParse(value, out var result))
		{
			SelectedAmount = Mathf.Clamp(result, 1, 999);
			InputField.SetTextWithoutNotify(SelectedAmount.ToString());
		}
		else
		{
			SelectedAmount = 1;
			InputField.SetTextWithoutNotify(string.Empty);
		}
	}
}
