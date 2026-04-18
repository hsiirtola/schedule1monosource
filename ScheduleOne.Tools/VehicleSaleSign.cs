using ScheduleOne.Money;
using ScheduleOne.Vehicles;
using TMPro;
using UnityEngine;

namespace ScheduleOne.Tools;

public class VehicleSaleSign : MonoBehaviour
{
	public TextMeshPro NameLabel;

	public TextMeshPro PriceLabel;

	public LandVehicle VehiclePrefab;

	private void Awake()
	{
		if ((Object)(object)VehiclePrefab != (Object)null)
		{
			((TMP_Text)NameLabel).text = VehiclePrefab.VehicleName;
			((TMP_Text)PriceLabel).text = MoneyManager.FormatAmount(VehiclePrefab.VehiclePrice);
		}
		else
		{
			Debug.LogWarning((object)("VehicleSaleSign on " + ((Object)((Component)this).gameObject).name + " has no VehiclePrefab assigned."));
		}
	}
}
