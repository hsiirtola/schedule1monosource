using UnityEngine;

namespace ScheduleOne.Product;

public static class DrugTypeMethods
{
	public static string GetName(this EDrugType property)
	{
		return PropertyUtility.GetDrugTypeData(property).Name;
	}

	public static Color GetColor(this EDrugType property)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return PropertyUtility.GetDrugTypeData(property).Color;
	}
}
