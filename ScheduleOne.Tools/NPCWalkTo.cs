using ScheduleOne.NPCs;
using UnityEngine;

namespace ScheduleOne.Tools;

[RequireComponent(typeof(NPCMovement))]
public class NPCWalkTo : MonoBehaviour
{
	public Transform Target;

	public float RepathRate = 0.5f;

	private float timeSinceLastPath;

	private void Update()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		timeSinceLastPath += Time.deltaTime;
		if (timeSinceLastPath >= RepathRate)
		{
			timeSinceLastPath = 0f;
			((Component)this).GetComponent<NPCMovement>().SetDestination(Target.position);
		}
	}
}
