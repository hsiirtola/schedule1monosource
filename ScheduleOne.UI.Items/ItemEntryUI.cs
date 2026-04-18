using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Items;

public class ItemEntryUI : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private Text _nameLabel;

	[SerializeField]
	private Text _quantityLabel;

	[SerializeField]
	private Image _icon;

	public void Set(string name, int quantity, Sprite icon)
	{
		_nameLabel.text = name;
		_quantityLabel.text = quantity + "x";
		_icon.sprite = icon;
	}

	public void SetLabelOnly(string name)
	{
		_nameLabel.text = name;
		((Component)_quantityLabel).gameObject.SetActive(true);
		((Component)_icon).gameObject.SetActive(false);
	}
}
