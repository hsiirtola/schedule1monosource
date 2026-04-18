using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Tools;

public class MonoBehaviourEvents : MonoBehaviour
{
	public UnityEvent onAwake;

	public UnityEvent onStart;

	public UnityEvent onUpdate;

	private void Awake()
	{
		UnityEvent obj = onAwake;
		if (obj != null)
		{
			obj.Invoke();
		}
	}

	private void Start()
	{
		UnityEvent obj = onStart;
		if (obj != null)
		{
			obj.Invoke();
		}
	}

	private void Update()
	{
		UnityEvent obj = onUpdate;
		if (obj != null)
		{
			obj.Invoke();
		}
	}
}
