using UnityEngine;

namespace ScheduleOne.Management;

public class TransitLineVisuals : MonoBehaviour
{
	public LineRenderer Renderer;

	public void SetSourcePosition(Vector3 position)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Renderer.SetPosition(0, position);
	}

	public void SetDestinationPosition(Vector3 position)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Renderer.SetPosition(1, position);
	}
}
