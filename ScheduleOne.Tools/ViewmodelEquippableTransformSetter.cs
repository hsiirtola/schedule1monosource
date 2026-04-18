using UnityEngine;

namespace ScheduleOne.Tools;

[ExecuteInEditMode]
public class ViewmodelEquippableTransformSetter : MonoBehaviour
{
	private static Vector3 lastRecordedLocalPosition = Vector3.zero;

	private static Vector3 lastRecordedLocalEulerAngles = Vector3.zero;

	private static Vector3 lastRecordedLocalScale = Vector3.one;

	private static bool transformChangedApplied = true;
}
