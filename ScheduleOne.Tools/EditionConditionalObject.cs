using UnityEngine;

namespace ScheduleOne.Tools;

public class EditionConditionalObject : MonoBehaviour
{
	public enum EType
	{
		ActiveInDemo,
		ActiveInFullGame
	}

	public EType type;

	private void Awake()
	{
		if (type == EType.ActiveInDemo)
		{
			((Component)this).gameObject.SetActive(false);
		}
	}
}
