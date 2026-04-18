using UnityEngine;

namespace ScheduleOne.DevUtilities;

public abstract class PersistentSingleton<T> : Singleton<T> where T : Singleton<T>
{
	protected override void Awake()
	{
		base.Awake();
		if (!Destroyed)
		{
			((Component)this).transform.SetParent((Transform)null);
			Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		}
	}
}
