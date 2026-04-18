using UnityEngine;

public class FlockWaypointTrigger : MonoBehaviour
{
	public float _timer = 1f;

	public FlockChild _flockChild;

	public void Start()
	{
		if ((Object)(object)_flockChild == (Object)null)
		{
			_flockChild = ((Component)((Component)this).transform.parent).GetComponent<FlockChild>();
		}
		float num = Random.Range(_timer, _timer * 3f);
		((MonoBehaviour)this).InvokeRepeating("Trigger", num, num);
	}

	public void Trigger()
	{
		_flockChild.Wander(0f);
	}
}
