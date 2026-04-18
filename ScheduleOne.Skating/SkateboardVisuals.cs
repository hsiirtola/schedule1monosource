using UnityEngine;

namespace ScheduleOne.Skating;

[RequireComponent(typeof(Skateboard))]
public class SkateboardVisuals : MonoBehaviour
{
	[Header("Settings")]
	public float MaxBoardLean = 8f;

	public float BoardLeanRate = 2f;

	[Header("References")]
	public Transform Board;

	private Skateboard skateboard;

	private void Awake()
	{
		skateboard = ((Component)this).GetComponent<Skateboard>();
	}

	private void LateUpdate()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(0f, 0f, skateboard.CurrentSteerInput * (0f - MaxBoardLean));
		Board.localRotation = Quaternion.Lerp(Board.localRotation, Quaternion.Euler(val), Time.deltaTime * BoardLeanRate);
	}
}
