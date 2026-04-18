using UnityEngine;

namespace ScheduleOne.Storage;

public class StoredItemRandomRotation : MonoBehaviour
{
	public Transform ItemContainer;

	public void ApplyRotation()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		ItemContainer.localEulerAngles = new Vector3(ItemContainer.localEulerAngles.x, Random.Range(0f, 360f), ItemContainer.localEulerAngles.z);
	}
}
