using System.Collections;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Shop;

public class ShopInterfaceDetailPanel : MonoBehaviour
{
	[Header("References")]
	public RectTransform Panel;

	public VerticalLayoutGroup LayoutGroup;

	public TextMeshProUGUI DescriptionLabel;

	public TextMeshProUGUI UnlockLabel;

	private ListingUI listing;

	private void Awake()
	{
		((Component)Panel).gameObject.SetActive(false);
	}

	public void Open(ListingUI _listing)
	{
		listing = _listing;
		((TMP_Text)DescriptionLabel).text = ((BaseItemDefinition)listing.Listing.Item).Description;
		if (listing.Listing.Item.RequiresLevelToPurchase && !listing.Listing.Item.IsUnlocked)
		{
			((TMP_Text)UnlockLabel).text = "Unlocks at <color=#2DB92D>" + listing.Listing.Item.RequiredRank.ToString() + "</color>";
			((Component)UnlockLabel).gameObject.SetActive(true);
		}
		else
		{
			((Component)UnlockLabel).gameObject.SetActive(false);
		}
		((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Wait());
		IEnumerator Wait()
		{
			((Behaviour)LayoutGroup).enabled = false;
			yield return (object)new WaitForEndOfFrame();
			((Component)Panel).gameObject.SetActive(true);
			LayoutRebuilder.ForceRebuildLayoutImmediate(Panel);
			((LayoutGroup)LayoutGroup).CalculateLayoutInputVertical();
			((Behaviour)LayoutGroup).enabled = true;
			Position();
		}
	}

	private void LateUpdate()
	{
		Position();
	}

	private void Position()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)listing == (Object)null))
		{
			((Transform)Panel).position = ((Transform)listing.DetailPanelAnchor).position;
			Panel.anchoredPosition = new Vector2(Panel.anchoredPosition.x + Panel.sizeDelta.x / 2f, Panel.anchoredPosition.y);
		}
	}

	public void Close()
	{
		listing = null;
		((Component)Panel).gameObject.SetActive(false);
	}
}
