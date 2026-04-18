using System;
using System.Collections;
using System.Collections.Generic;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.UI.Management;

public class ItemSelector : ClipboardScreen
{
	[Serializable]
	public class Option
	{
		public string Title;

		public ItemDefinition Item;

		public Option(string title, ItemDefinition item)
		{
			Title = title;
			Item = item;
		}
	}

	[Header("References")]
	public RectTransform OptionContainer;

	public TextMeshProUGUI TitleLabel;

	public TextMeshProUGUI HoveredItemLabel;

	public GameObject OptionPrefab;

	[Header("Settings")]
	public Sprite EmptyOptionSprite;

	private Coroutine lerpRoutine;

	private List<Option> options = new List<Option>();

	private Option selectedOption;

	private List<RectTransform> optionButtons = new List<RectTransform>();

	private Action<Option> optionCallback;

	private UIContentPanel panel;

	public void Initialize(string selectionTitle, List<Option> _options, Option _selectedOption = null, Action<Option> _optionCallback = null)
	{
		((TMP_Text)TitleLabel).text = selectionTitle;
		options = new List<Option>();
		options.AddRange(_options);
		selectedOption = _selectedOption;
		optionCallback = _optionCallback;
		panel = ((Component)this).GetComponent<UIContentPanel>();
		if (!Object.op_Implicit((Object)(object)panel))
		{
			panel = ((Component)this).gameObject.AddComponent<UIContentPanel>();
		}
		DeleteOptions();
		CreateOptions(options);
		((Behaviour)HoveredItemLabel).enabled = false;
		InitializeAfterUIReady();
	}

	public override void Open()
	{
		base.Open();
		Singleton<ManagementInterface>.Instance.MainScreen.Close();
		Singleton<ManagementInterface>.Instance.UIScreen.AddPanel(panel);
		Singleton<ManagementInterface>.Instance.UIScreen.SetCurrentSelectedPanel(panel);
	}

	public override void Close()
	{
		base.Close();
		((Behaviour)HoveredItemLabel).enabled = false;
		Singleton<ManagementInterface>.Instance.MainScreen.Open();
		Singleton<ManagementInterface>.Instance.UIScreen.RemovePanel(panel);
	}

	private void ButtonClicked(Option option)
	{
		if (optionCallback != null)
		{
			optionCallback(option);
		}
		Close();
	}

	private void ButtonHovered(Option option)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		((TMP_Text)HoveredItemLabel).text = option.Title;
		((Behaviour)HoveredItemLabel).enabled = true;
		((Component)HoveredItemLabel).GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -140f - Mathf.Ceil((float)optionButtons.Count / 5f) * optionButtons[0].sizeDelta.y);
	}

	private void ButtonHoverEnd(Option option)
	{
		((Behaviour)HoveredItemLabel).enabled = false;
	}

	private void CreateOptions(List<Option> options)
	{
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Expected O, but got Unknown
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Expected O, but got Unknown
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Expected O, but got Unknown
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Expected O, but got Unknown
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Expected O, but got Unknown
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Expected O, but got Unknown
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < options.Count; i++)
		{
			Button component = Object.Instantiate<GameObject>(OptionPrefab, (Transform)(object)OptionContainer).GetComponent<Button>();
			UISelectable uISelectable = ((Component)component).gameObject.GetComponent<UISelectable>();
			if (!Object.op_Implicit((Object)(object)uISelectable))
			{
				uISelectable = ((Component)component).gameObject.AddComponent<UISelectable>();
			}
			panel.AddSelectable(uISelectable);
			if ((Object)(object)options[i].Item != (Object)null)
			{
				((Component)((Component)component).transform.Find("None")).gameObject.SetActive(false);
				((Component)((Component)component).transform.Find("Icon")).gameObject.GetComponent<Image>().sprite = ((BaseItemDefinition)options[i].Item).Icon;
				((Component)((Component)component).transform.Find("Icon")).gameObject.SetActive(true);
			}
			else
			{
				((Component)((Component)component).transform.Find("None")).gameObject.SetActive(true);
				((Component)((Component)component).transform.Find("Icon")).gameObject.SetActive(false);
			}
			if (options[i] == selectedOption)
			{
				((Graphic)((Component)((Component)component).transform.Find("Outline")).gameObject.GetComponent<Image>()).color = Color32.op_Implicit(new Color32((byte)90, (byte)90, (byte)90, byte.MaxValue));
				panel.SelectSelectable(uISelectable);
			}
			Option opt = options[i];
			((UnityEvent)component.onClick).AddListener((UnityAction)delegate
			{
				ButtonClicked(opt);
			});
			uISelectable.OnTrigger.AddListener((UnityAction)delegate
			{
				ButtonClicked(opt);
			});
			Entry val = new Entry();
			val.eventID = (EventTriggerType)0;
			((UnityEvent<BaseEventData>)(object)val.callback).AddListener((UnityAction<BaseEventData>)delegate
			{
				ButtonHovered(opt);
			});
			((Component)component).GetComponent<EventTrigger>().triggers.Add(val);
			uISelectable.OnSelected.AddListener((UnityAction)delegate
			{
				ButtonHovered(opt);
			});
			val = new Entry();
			val.eventID = (EventTriggerType)1;
			((UnityEvent<BaseEventData>)(object)val.callback).AddListener((UnityAction<BaseEventData>)delegate
			{
				ButtonHoverEnd(opt);
			});
			((Component)component).GetComponent<EventTrigger>().triggers.Add(val);
			uISelectable.OnDeselected.AddListener((UnityAction)delegate
			{
				ButtonHoverEnd(opt);
			});
			optionButtons.Add(((Component)component).GetComponent<RectTransform>());
		}
	}

	private void DeleteOptions()
	{
		for (int i = 0; i < optionButtons.Count; i++)
		{
			Object.Destroy((Object)(object)((Component)optionButtons[i]).gameObject);
		}
		optionButtons.Clear();
		panel.ClearAllSelectables();
	}

	private void InitializeAfterUIReady()
	{
		((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(routine());
		IEnumerator routine()
		{
			yield return null;
			if (GameInput.GetCurrentInputDeviceIsGamepad())
			{
				ButtonHovered(selectedOption);
			}
			else
			{
				((Behaviour)HoveredItemLabel).enabled = false;
			}
		}
	}
}
