using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne;

public class UIHorizontalSelector : UIOption
{
	[SerializeField]
	private Button prevButton;

	[SerializeField]
	private Button nextButton;

	[SerializeField]
	private TextMeshProUGUI currentOptionNameText;

	public UnityEvent<OptionInfo> OnChanged;

	private List<OptionInfo> options = new List<OptionInfo>();

	private int currentIndex;

	protected override float NavigationRepeatRateMult => 2f;

	protected override void Awake()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		base.Awake();
		((UnityEvent)prevButton.onClick).AddListener((UnityAction)delegate
		{
			MovePrev();
		});
		((UnityEvent)nextButton.onClick).AddListener((UnityAction)delegate
		{
			MoveNext();
		});
	}

	protected override void OnUpdate()
	{
		DetectInput();
	}

	protected override void MoveLeft()
	{
		base.MoveLeft();
		MovePrev();
	}

	protected override void MoveRight()
	{
		base.MoveRight();
		MoveNext();
	}

	private void MovePrev()
	{
		if (options.Count != 0)
		{
			currentIndex = (currentIndex - 1 + options.Count) % options.Count;
			UpdateCurrentOptionText();
		}
	}

	private void MoveNext()
	{
		if (options.Count != 0)
		{
			currentIndex = (currentIndex + 1) % options.Count;
			UpdateCurrentOptionText();
		}
	}

	private void UpdateCurrentOptionText()
	{
		((TMP_Text)currentOptionNameText).text = options[currentIndex].OptionName;
		OnChanged?.Invoke(options[currentIndex]);
	}

	public void SetOptions(List<OptionInfo> newOptions, int defaultIndex = 0)
	{
		options.Clear();
		options = newOptions;
		if (options.Count == 0)
		{
			currentIndex = 0;
		}
		else
		{
			currentIndex = Mathf.Clamp(defaultIndex, 0, options.Count - 1);
		}
		UpdateCurrentOptionText();
	}
}
