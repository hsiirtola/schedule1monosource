using UnityEngine;

public class ResetPosition : MonoBehaviour
{
	public float distanceToReset = 5f;

	private Vector3 startPosition;

	private void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		startPosition = ((Component)this).transform.position;
	}

	private void Update()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (Vector3.Distance(startPosition, ((Component)this).transform.position) >= distanceToReset)
		{
			((Component)this).transform.position = startPosition;
		}
	}
}
