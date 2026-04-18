using System.Collections.Generic;
using ScheduleOne.ItemFramework;
using ScheduleOne.Product;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Items;

public class ProductItemInfoContent : QualityItemInfoContent
{
	public List<TextMeshProUGUI> PropertyLabels = new List<TextMeshProUGUI>();

	public override void Initialize(ItemInstance instance)
	{
		Initialize(instance.Definition);
		base.Initialize(instance);
	}

	public override void Initialize(ItemDefinition definition)
	{
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		base.Initialize(definition);
		ProductDefinition productDefinition = definition as ProductDefinition;
		if ((Object)(object)productDefinition == (Object)null)
		{
			Console.LogError("ProductItemInfoContent can only be used with ProductDefinition!");
			return;
		}
		PropertyUtility.DrugTypeData drugTypeData = PropertyUtility.GetDrugTypeData(productDefinition.DrugTypes[0].DrugType);
		TextMeshProUGUI qualityLabel = QualityLabel;
		((TMP_Text)qualityLabel).text = ((TMP_Text)qualityLabel).text + " " + drugTypeData.Name;
		for (int i = 0; i < PropertyLabels.Count; i++)
		{
			if (productDefinition.Properties.Count > i)
			{
				((TMP_Text)PropertyLabels[i]).text = "• " + productDefinition.Properties[i].Name;
				((Graphic)PropertyLabels[i]).color = productDefinition.Properties[i].LabelColor;
				((Behaviour)PropertyLabels[i]).enabled = true;
			}
			else
			{
				((Behaviour)PropertyLabels[i]).enabled = false;
			}
		}
	}
}
