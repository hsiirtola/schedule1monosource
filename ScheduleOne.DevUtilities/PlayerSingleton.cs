using UnityEngine;

namespace ScheduleOne.DevUtilities;

public abstract class PlayerSingleton<T> : MonoBehaviour where T : PlayerSingleton<T>
{
	private static T instance;

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

	protected virtual void Awake()
	{
		OnStartClient(IsOwner: true);
	}

	protected virtual void Start()
	{
	}

	public virtual void OnStartClient(bool IsOwner)
	{
		if (!IsOwner)
		{
			Console.Log("Destroying non-local player singleton: " + ((Object)this).name);
			Object.Destroy((Object)(object)this);
		}
		else if ((Object)(object)instance != (Object)null)
		{
			Console.LogWarning("Multiple instances of " + ((Object)this).name + " exist. Keeping prior instance reference.");
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
