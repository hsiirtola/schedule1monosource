using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Tools;

public class PhysicsEvents : MonoBehaviour
{
	public bool DEBUG;

	public UnityEvent<Collider> OnTriggerEnterEvent;

	public UnityEvent<Collider> OnTriggerExitEvent;

	public UnityEvent<Collision> OnCollisionEnterEvent;

	public UnityEvent<Collision> OnCollisionExitEvent;

	public void OnTriggerEnter(Collider other)
	{
		OnTriggerEnterEvent?.Invoke(other);
		if (DEBUG)
		{
			Debug.Log((object)$"OnTriggerEnter: {GetHierarchyString(((Component)other).transform)} at {Time.time}");
		}
	}

	public void OnTriggerExit(Collider other)
	{
		OnTriggerExitEvent?.Invoke(other);
		if (DEBUG)
		{
			Debug.Log((object)$"OnTriggerExit: {GetHierarchyString(((Component)other).transform)} at {Time.time}");
		}
	}

	public void OnCollisionEnter(Collision collision)
	{
		OnCollisionEnterEvent?.Invoke(collision);
		if (DEBUG)
		{
			Debug.Log((object)$"OnCollisionEnter: {GetHierarchyString(((Component)collision.collider).transform)} at {Time.time}");
		}
	}

	public void OnCollisionExit(Collision collision)
	{
		OnCollisionExitEvent?.Invoke(collision);
		if (DEBUG)
		{
			Debug.Log((object)$"OnCollisionExit: {GetHierarchyString(((Component)collision.collider).transform)} at {Time.time}");
		}
	}

	private static string GetHierarchyString(Transform transform)
	{
		return string.Join(" > ", from t in ((Component)transform).GetComponentsInParent<Transform>()
			select ((Object)t).name);
	}
}
