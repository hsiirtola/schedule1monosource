using UnityEngine;

namespace ScheduleOne.Tools;

public class CashPile : MonoBehaviour
{
	public const float MAX_AMOUNT = 100000f;

	public Transform Container;

	private Transform[] CashInstances;

	private void Awake()
	{
		CashInstances = (Transform[])(object)new Transform[Container.childCount];
		for (int i = 0; i < CashInstances.Length; i++)
		{
			CashInstances[i] = Container.GetChild(i);
			((Component)CashInstances[i]).gameObject.SetActive(false);
		}
	}

	public void SetDisplayedAmount(float amount)
	{
		int num = Mathf.FloorToInt(amount / 100000f * (float)CashInstances.Length);
		for (int i = 0; i < CashInstances.Length; i++)
		{
			((Component)CashInstances[i]).gameObject.SetActive(i < num);
		}
	}
}
