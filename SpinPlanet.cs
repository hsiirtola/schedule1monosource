using UnityEngine;

public class SpinPlanet : MonoBehaviour
{
	public float speed = 4f;

	private void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.Rotate(Vector3.up, speed * Time.deltaTime);
	}
}
