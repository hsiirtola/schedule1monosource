using System;
using System.Collections.Generic;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.Effects;
using ScheduleOne.Money;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using ScheduleOne.Product;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class NewMixScreen : Singleton<NewMixScreen>
{
	public const int MAX_PROPERTIES_DISPLAYED = 5;

	[Header("References")]
	[SerializeField]
	protected Canvas canvas;

	public RectTransform Container;

	[SerializeField]
	protected TMP_InputField nameInputField;

	[SerializeField]
	protected GameObject mixAlreadyExistsText;

	[SerializeField]
	protected RectTransform editIcon;

	[SerializeField]
	protected Button randomizeNameButton;

	[SerializeField]
	protected Button confirmButton;

	[SerializeField]
	protected TextMeshProUGUI PropertiesLabel;

	[SerializeField]
	protected TextMeshProUGUI MarketValueLabel;

	public AudioSourceController Sound;

	[Header("Prefabs")]
	[SerializeField]
	protected GameObject attributeEntryPrefab;

	[Header("Name Library")]
	[SerializeField]
	protected List<string> name1Library = new List<string>();

	[SerializeField]
	protected List<string> name2Library = new List<string>();

	public Action<string> onMixNamed;

	public bool IsOpen => ((Behaviour)canvas).enabled;

	protected override void Awake()
	{
		base.Awake();
		((UnityEvent<string>)(object)nameInputField.onValueChanged).AddListener((UnityAction<string>)OnNameValueChanged);
		GameInput.RegisterExitListener(Exit, 3);
		((Behaviour)canvas).enabled = false;
		((Component)Container).gameObject.SetActive(false);
	}

	private void Exit(ExitAction action)
	{
	}

	protected virtual void Update()
	{
		if (IsOpen && ((Selectable)confirmButton).interactable && GameInput.GetButtonDown(GameInput.ButtonCode.Submit))
		{
			ConfirmButtonClicked();
		}
	}

	public void Open(List<Effect> properties, EDrugType drugType, float productMarketValue)
	{
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		((Behaviour)canvas).enabled = true;
		((Component)Container).gameObject.SetActive(true);
		PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
		nameInputField.text = GenerateUniqueName(properties.ToArray(), drugType);
		Singleton<TaskManager>.Instance.PlayTaskCompleteSound();
		((TMP_Text)PropertiesLabel).text = string.Empty;
		for (int i = 0; i < properties.Count; i++)
		{
			Effect effect = properties[i];
			if (((TMP_Text)PropertiesLabel).text.Length > 0)
			{
				TextMeshProUGUI propertiesLabel = PropertiesLabel;
				((TMP_Text)propertiesLabel).text = ((TMP_Text)propertiesLabel).text + "\n";
			}
			if (i == 4 && properties.Count > 5)
			{
				int num = properties.Count - 5 + 1;
				TextMeshProUGUI propertiesLabel2 = PropertiesLabel;
				((TMP_Text)propertiesLabel2).text = ((TMP_Text)propertiesLabel2).text + "+ " + num + " more...";
				break;
			}
			TextMeshProUGUI propertiesLabel3 = PropertiesLabel;
			((TMP_Text)propertiesLabel3).text = ((TMP_Text)propertiesLabel3).text + "<color=#" + ColorUtility.ToHtmlStringRGBA(effect.LabelColor) + ">• " + effect.Name + "</color>";
		}
		((TMP_Text)MarketValueLabel).text = "Market Value: <color=#54E717>" + MoneyManager.FormatAmount(productMarketValue) + "</color>";
	}

	public void Close()
	{
		((Behaviour)canvas).enabled = false;
		((Component)Container).gameObject.SetActive(false);
		PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
	}

	public void RandomizeButtonClicked()
	{
		nameInputField.text = GenerateUniqueName();
	}

	public void ConfirmButtonClicked()
	{
		if (onMixNamed != null)
		{
			onMixNamed(nameInputField.text);
		}
		Sound.Play();
		RandomizeButtonClicked();
		Close();
	}

	public string GenerateUniqueName(Effect[] properties = null, EDrugType drugType = EDrugType.Marijuana)
	{
		Random.InitState((int)(Time.timeSinceLevelLoad * 10f));
		string text = name1Library[Random.Range(0, name1Library.Count)];
		string text2 = name2Library[Random.Range(0, name2Library.Count)];
		if (properties != null)
		{
			int num = 0;
			foreach (Effect effect in properties)
			{
				num += effect.Name.GetHashCode() / 2000;
			}
			num += drugType.GetHashCode() / 1000;
			int num2 = Mathf.Abs(num % name1Library.Count);
			int num3 = Mathf.Abs(num / 2 % name2Library.Count);
			text = name1Library[Mathf.Clamp(num2, 0, name1Library.Count)];
			text2 = name2Library[Mathf.Clamp(num3, 0, name2Library.Count)];
		}
		while (NetworkSingleton<ProductManager>.Instance.ProductNames.Contains(text + " " + text2))
		{
			text = name1Library[Random.Range(0, name1Library.Count)];
			text2 = name2Library[Random.Range(0, name2Library.Count)];
		}
		return text + " " + text2;
	}

	protected void RefreshNameButtons()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		float num = nameInputField.textComponent.preferredWidth / 2f;
		float num2 = 20f;
		editIcon.anchoredPosition = new Vector2(num + num2, editIcon.anchoredPosition.y);
		((Component)randomizeNameButton).GetComponent<RectTransform>().anchoredPosition = new Vector2(0f - num - num2, ((Component)randomizeNameButton).GetComponent<RectTransform>().anchoredPosition.y);
	}

	public void OnNameValueChanged(string newVal)
	{
		if (NetworkSingleton<ProductManager>.Instance.ProductNames.Contains(nameInputField.text) || !ProductManager.IsMixNameValid(nameInputField.text))
		{
			mixAlreadyExistsText.gameObject.SetActive(true);
			((Selectable)confirmButton).interactable = false;
		}
		else
		{
			mixAlreadyExistsText.gameObject.SetActive(false);
			((Selectable)confirmButton).interactable = true;
		}
		RefreshNameButtons();
		((MonoBehaviour)this).Invoke("RefreshNameButtons", 1f / 60f);
	}
}
