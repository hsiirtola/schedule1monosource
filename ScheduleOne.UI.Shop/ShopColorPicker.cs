using System;
using ScheduleOne.Clothing;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.ItemFramework;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.UI.Shop;

public class ShopColorPicker : MonoBehaviour
{
	public Image AssetIconImage;

	public TextMeshProUGUI ColorLabel;

	public RectTransform ColorButtonParent;

	public GameObject ColorButtonPrefab;

	public UnityEvent<EClothingColor> onColorPicked = new UnityEvent<EClothingColor>();

	public bool IsOpen => ((Component)this).gameObject.activeInHierarchy;

	public void Start()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		foreach (EClothingColor color in Enum.GetValues(typeof(EClothingColor)))
		{
			GameObject obj = Object.Instantiate<GameObject>(ColorButtonPrefab, (Transform)(object)ColorButtonParent);
			((Graphic)((Component)obj.transform.Find("Color")).GetComponent<Image>()).color = color.GetActualColor();
			((UnityEvent)obj.GetComponent<Button>().onClick).AddListener((UnityAction)delegate
			{
				ColorPicked(color);
			});
			EventTrigger obj2 = obj.AddComponent<EventTrigger>();
			Entry val = new Entry();
			val.eventID = (EventTriggerType)0;
			((UnityEvent<BaseEventData>)(object)val.callback).AddListener((UnityAction<BaseEventData>)delegate
			{
				ColorHovered(color);
			});
			obj2.triggers.Add(val);
		}
		((Component)this).gameObject.SetActive(false);
	}

	private void ColorPicked(EClothingColor color)
	{
		if (onColorPicked != null)
		{
			onColorPicked.Invoke(color);
		}
		Close();
	}

	public void Open(ItemDefinition item)
	{
		AssetIconImage.sprite = ((BaseItemDefinition)item).Icon;
		ColorHovered(EClothingColor.White);
		((Component)this).gameObject.SetActive(true);
	}

	public void Close()
	{
		((Component)this).gameObject.SetActive(false);
	}

	private void ColorHovered(EClothingColor color)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)AssetIconImage).color = color.GetActualColor();
		((TMP_Text)ColorLabel).text = color.GetLabel();
	}
}
