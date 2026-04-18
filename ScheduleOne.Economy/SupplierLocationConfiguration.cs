using UnityEngine;

namespace ScheduleOne.Economy;

public class SupplierLocationConfiguration : MonoBehaviour
{
	public string SupplierID;

	public void Activate()
	{
		((Component)this).gameObject.SetActive(true);
	}

	public void Deactivate()
	{
		((Component)this).gameObject.SetActive(false);
	}
}
