using UnityEngine;

namespace ScheduleOne.Map;

[RequireComponent(typeof(Ladder))]
public class LadderSizeSetter : MonoBehaviour
{
	public Vector2 Size = new Vector2(0.6f, 3f);
}
