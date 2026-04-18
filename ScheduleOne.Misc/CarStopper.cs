using UnityEngine;
using UnityEngine.AI;

namespace ScheduleOne.Misc;

public class CarStopper : MonoBehaviour
{
	public bool isActive;

	[Header("References")]
	[SerializeField]
	protected Transform blocker;

	public NavMeshObstacle Obstacle;

	private float moveTime = 0.5f;

	protected virtual void Update()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		float num = 70f;
		if (isActive)
		{
			((Behaviour)Obstacle).enabled = true;
			blocker.localEulerAngles = new Vector3(0f, 0f, Mathf.Clamp(blocker.localEulerAngles.z + Time.deltaTime * num / moveTime, 0f, num));
		}
		else
		{
			((Behaviour)Obstacle).enabled = false;
			blocker.localEulerAngles = new Vector3(0f, 0f, Mathf.Clamp(blocker.localEulerAngles.z - Time.deltaTime * num / moveTime, 0f, num));
		}
	}
}
