using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.CharacterCustomization;

public class CharacterCustomizationCategory : MonoBehaviour
{
	public string CategoryName;

	[Header("References")]
	public TextMeshProUGUI TitleText;

	public Button BackButton;

	public ScrollRect ScrollRect;

	private CharacterCustomizationUI ui;

	private CharacterCustomizationOption[] options;

	public UnityEvent onOpen;

	public UnityEvent onClose;

	private void Awake()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		ui = ((Component)this).GetComponentInParent<CharacterCustomizationUI>();
		options = ((Component)this).GetComponentsInChildren<CharacterCustomizationOption>(true);
		((TMP_Text)TitleText).text = CategoryName;
		((UnityEvent)BackButton.onClick).AddListener(new UnityAction(Back));
		for (int i = 0; i < options.Length; i++)
		{
			CharacterCustomizationOption option = options[i];
			options[i].onSelect.AddListener((UnityAction)delegate
			{
				OptionSelected(option);
			});
			options[i].onDeselect.AddListener((UnityAction)delegate
			{
				OptionDeselected(option);
			});
			options[i].onPurchase.AddListener((UnityAction)delegate
			{
				OptionPurchased(option);
			});
		}
		for (int num = 0; num < options.Length; num++)
		{
			for (int num2 = num + 1; num2 < options.Length; num2++)
			{
				if (options[num2].Price < options[num].Price)
				{
					_ = ((Component)options[num]).transform;
					((Component)options[num]).transform.SetSiblingIndex(num2);
					((Component)options[num2]).transform.SetSiblingIndex(num);
				}
			}
		}
	}

	public void Open()
	{
		bool flag = false;
		for (int i = 0; i < options.Length; i++)
		{
			if (ui.IsOptionCurrentlyApplied(options[i]))
			{
				flag = true;
				options[i].SetPurchased(_purchased: true);
			}
			else
			{
				options[i].SetPurchased(_purchased: false);
				options[i].SetSelected(_selected: false);
			}
		}
		if (!flag && options.Length != 0)
		{
			options[0].SetPurchased(_purchased: true);
		}
		ScrollRect.verticalScrollbar.value = 1f;
		if (onOpen != null)
		{
			onOpen.Invoke();
		}
	}

	public void Back()
	{
		ui.SetActiveCategory(null);
		for (int i = 0; i < options.Length; i++)
		{
			options[i].ParentCategoryClosed();
		}
		if (onClose != null)
		{
			onClose.Invoke();
		}
	}

	private void OptionSelected(CharacterCustomizationOption option)
	{
		ui.OptionSelected(option);
		for (int i = 0; i < options.Length; i++)
		{
			options[i].SiblingOptionSelected(option);
		}
	}

	private void OptionDeselected(CharacterCustomizationOption option)
	{
		ui.OptionDeselected(option);
	}

	private void OptionPurchased(CharacterCustomizationOption option)
	{
		ui.OptionPurchased(option);
		for (int i = 0; i < options.Length; i++)
		{
			options[i].SiblingOptionPurchased(option);
		}
	}
}
