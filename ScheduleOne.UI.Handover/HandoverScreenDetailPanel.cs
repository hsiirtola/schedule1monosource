using ScheduleOne.Economy;
using ScheduleOne.ItemFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Handover;

public class HandoverScreenDetailPanel : MonoBehaviour
{
	public LayoutGroup LayoutGroup;

	public RectTransform Container;

	public TextMeshProUGUI NameLabel;

	public RectTransform RelationshipContainer;

	public Scrollbar RelationshipScrollbar;

	public RectTransform AddictionContainer;

	public Scrollbar AdditionScrollbar;

	public Image StandardsStar;

	public TextMeshProUGUI StandardsLabel;

	public TextMeshProUGUI EffectsLabel;

	public void Open(Customer customer)
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		((TMP_Text)NameLabel).text = customer.NPC.fullName;
		if (customer.NPC.RelationData.Unlocked)
		{
			((Component)RelationshipContainer).gameObject.SetActive(true);
			RelationshipScrollbar.value = customer.NPC.RelationData.NormalizedRelationDelta;
			((Component)AddictionContainer).gameObject.SetActive(true);
			AdditionScrollbar.value = customer.CurrentAddiction;
		}
		else
		{
			((Component)RelationshipContainer).gameObject.SetActive(false);
			((Component)AddictionContainer).gameObject.SetActive(false);
		}
		((Graphic)StandardsStar).color = ItemQuality.GetColor(customer.CustomerData.Standards.GetCorrespondingQuality());
		((TMP_Text)StandardsLabel).text = customer.CustomerData.Standards.GetName();
		((Graphic)StandardsLabel).color = ((Graphic)StandardsStar).color;
		((TMP_Text)EffectsLabel).text = string.Empty;
		for (int i = 0; i < customer.CustomerData.PreferredProperties.Count; i++)
		{
			if (i > 0)
			{
				TextMeshProUGUI effectsLabel = EffectsLabel;
				((TMP_Text)effectsLabel).text = ((TMP_Text)effectsLabel).text + "\n";
			}
			string text = "<color=#" + ColorUtility.ToHtmlStringRGBA(customer.CustomerData.PreferredProperties[i].LabelColor) + ">•  " + customer.CustomerData.PreferredProperties[i].Name + "</color>";
			TextMeshProUGUI effectsLabel2 = EffectsLabel;
			((TMP_Text)effectsLabel2).text = ((TMP_Text)effectsLabel2).text + text;
		}
		((Component)this).gameObject.SetActive(true);
		LayoutGroup.CalculateLayoutInputHorizontal();
		LayoutGroup.CalculateLayoutInputVertical();
		LayoutRebuilder.ForceRebuildLayoutImmediate(((Component)LayoutGroup).GetComponent<RectTransform>());
		((Component)LayoutGroup).GetComponent<ContentSizeFitter>().SetLayoutVertical();
		Container.anchoredPosition = new Vector2(0f, (0f - Container.sizeDelta.y) / 2f);
	}

	public void Close()
	{
		((Component)this).gameObject.SetActive(false);
	}
}
