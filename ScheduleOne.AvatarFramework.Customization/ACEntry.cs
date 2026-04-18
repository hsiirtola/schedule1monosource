using UnityEngine;

namespace ScheduleOne.AvatarFramework.Customization;

public class ACEntry : MonoBehaviour
{
	public bool DevOnly;

	private void Awake()
	{
		if (DevOnly && !Application.isEditor)
		{
			((Component)this).gameObject.SetActive(false);
		}
	}
}
