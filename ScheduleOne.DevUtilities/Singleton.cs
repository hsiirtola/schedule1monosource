using UnityEngine;

namespace ScheduleOne.DevUtilities;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
	private static T instance;

	protected bool Destroyed;

	public static bool InstanceExists => (Object)(object)instance != (Object)null;

	public static T Instance
	{
		get
		{
			return instance;
		}
		protected set
		{
			instance = value;
		}
	}

	protected virtual void Start()
	{
	}

	protected virtual void Awake()
	{
		if ((Object)(object)instance != (Object)null)
		{
			Console.LogWarning("Multiple instances of " + ((Object)this).name + " exist. Destroying this instance.");
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
		else
		{
			instance = (T)this;
		}
	}

	protected virtual void OnDestroy()
	{
		if ((Object)(object)instance == (Object)(object)this)
		{
			instance = null;
		}
	}
}
