using ScheduleOne.ItemFramework;
using UnityEngine;

namespace ScheduleOne.UI.Items;

public class ItemInfoPanel : MonoBehaviour
{
	public const float VERTICAL_THRESHOLD = 200f;

	[Header("References")]
	public RectTransform Container;

	public RectTransform ContentContainer;

	public GameObject TopArrow;

	public GameObject BottomArrow;

	public Canvas Canvas;

	[Header("Settings")]
	public Vector2 Offset = new Vector2(0f, 125f);

	[Header("Prefabs")]
	public ItemInfoContent DefaultContentPrefab;

	private ItemInfoContent content;

	public bool IsOpen { get; protected set; }

	public ItemInstance CurrentItem { get; protected set; }

	private void Awake()
	{
		Close();
	}

	public void Open(ItemInstance item, RectTransform rect)
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		if (IsOpen)
		{
			Close();
		}
		if (item == null)
		{
			Console.LogWarning("Item is null!");
			return;
		}
		CurrentItem = item;
		if ((Object)(object)item.Definition.CustomInfoContent != (Object)null)
		{
			content = Object.Instantiate<ItemInfoContent>(item.Definition.CustomInfoContent, (Transform)(object)ContentContainer);
			content.Initialize(item);
		}
		else
		{
			content = Object.Instantiate<ItemInfoContent>(DefaultContentPrefab, (Transform)(object)ContentContainer);
			content.Initialize(item);
		}
		Container.sizeDelta = new Vector2(Container.sizeDelta.x, content.Height);
		float num = (rect.sizeDelta.y + Container.sizeDelta.y) / 2f + Offset.y;
		num *= Canvas.scaleFactor;
		if (((Transform)rect).position.y > 200f)
		{
			((Transform)Container).position = ((Transform)rect).position - new Vector3(0f, num, 0f);
			TopArrow.SetActive(true);
			BottomArrow.SetActive(false);
		}
		else
		{
			((Transform)Container).position = ((Transform)rect).position + new Vector3(0f, num, 0f);
			TopArrow.SetActive(false);
			BottomArrow.SetActive(true);
		}
		IsOpen = true;
		((Component)Container).gameObject.SetActive(true);
	}

	public void Open(ItemDefinition def, RectTransform rect)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		if (IsOpen)
		{
			Close();
		}
		if ((Object)(object)def == (Object)null)
		{
			Console.LogWarning("Item is null!");
			return;
		}
		CurrentItem = null;
		content = Object.Instantiate<ItemInfoContent>(DefaultContentPrefab, (Transform)(object)ContentContainer);
		content.Initialize(def);
		float num = (rect.sizeDelta.y + Container.sizeDelta.y) / 2f + Offset.y;
		num *= Canvas.scaleFactor;
		if (((Transform)rect).position.y > 200f)
		{
			((Transform)Container).position = ((Transform)rect).position - new Vector3(0f, num, 0f);
			TopArrow.SetActive(true);
			BottomArrow.SetActive(false);
		}
		else
		{
			((Transform)Container).position = ((Transform)rect).position + new Vector3(0f, num, 0f);
			TopArrow.SetActive(false);
			BottomArrow.SetActive(true);
		}
		IsOpen = true;
		((Component)Container).gameObject.SetActive(true);
	}

	public void Close()
	{
		if ((Object)(object)content != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)content).gameObject);
		}
		IsOpen = false;
		CurrentItem = null;
		((Component)Container).gameObject.SetActive(false);
	}
}
