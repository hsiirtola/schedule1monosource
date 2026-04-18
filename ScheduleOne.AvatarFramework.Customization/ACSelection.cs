using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.AvatarFramework.Customization;

public abstract class ACSelection<T> : MonoBehaviour where T : Object
{
	[Header("References")]
	public GameObject ButtonPrefab;

	[Header("Settings")]
	public int PropertyIndex;

	public List<T> Options = new List<T>();

	public bool Nullable = true;

	public int DefaultOptionIndex;

	protected List<GameObject> buttons = new List<GameObject>();

	protected int SelectedOptionIndex = -1;

	public UnityEvent<T> onValueChange;

	public UnityEvent<T, int> onValueChangeWithIndex;

	protected virtual void Awake()
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		for (int i = 0; i < Options.Count; i++)
		{
			GameObject val = Object.Instantiate<GameObject>(ButtonPrefab, ((Component)this).transform);
			((TMP_Text)((Component)val.transform.Find("Label")).GetComponent<TextMeshProUGUI>()).text = GetOptionLabel(i);
			buttons.Add(val);
			int index = i;
			((UnityEvent)val.GetComponent<Button>().onClick).AddListener((UnityAction)delegate
			{
				SelectOption(index);
			});
		}
	}

	public void SelectOption(int index, bool notify = true)
	{
		int selectedOptionIndex = SelectedOptionIndex;
		if (index != SelectedOptionIndex)
		{
			if (SelectedOptionIndex != -1)
			{
				SetButtonHighlighted(SelectedOptionIndex, h: false);
			}
			SelectedOptionIndex = index;
			SetButtonHighlighted(SelectedOptionIndex, h: true);
		}
		else if (Nullable)
		{
			SetButtonHighlighted(SelectedOptionIndex, h: false);
			SelectedOptionIndex = -1;
		}
		if (selectedOptionIndex != SelectedOptionIndex && notify)
		{
			CallValueChange();
		}
	}

	public abstract void CallValueChange();

	public abstract string GetOptionLabel(int index);

	public abstract int GetAssetPathIndex(string path);

	private void SetButtonHighlighted(int buttonIndex, bool h)
	{
		if (buttonIndex != -1)
		{
			((Component)buttons[buttonIndex].transform.Find("Indicator")).gameObject.SetActive(h);
		}
	}
}
