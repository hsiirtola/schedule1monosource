using UnityEngine;

public class FanRotator : MonoBehaviour
{
	private Transform thisTransform;

	public float speed = 90f;

	private void Start()
	{
		thisTransform = ((Component)this).GetComponent<Transform>();
	}

	private void Update()
	{
		thisTransform.Rotate(0f, speed * Time.deltaTime, 0f, (Space)1);
	}
}
