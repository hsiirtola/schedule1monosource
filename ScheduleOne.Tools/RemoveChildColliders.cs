using UnityEngine;

namespace ScheduleOne.Tools;

public class RemoveChildColliders : MonoBehaviour
{
	private void Start()
	{
		Collider[] componentsInChildren = ((Component)this).GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Object.Destroy((Object)(object)componentsInChildren[i]);
		}
	}
}
