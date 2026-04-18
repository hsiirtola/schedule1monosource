using UnityEngine;

public class RotateAround : MonoBehaviour
{
	public Transform rot_center;

	private void Start()
	{
	}

	private void Update()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.RotateAround(rot_center.position, Vector3.up, 0.25f);
	}
}
