using System;
using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Tools;

public class OptimizedColliderGroup : MonoBehaviour
{
	public const int UPDATE_DISTANCE = 5;

	public Collider[] Colliders;

	public float ColliderEnableMaxDistance = 30f;

	private float sqrColliderEnableMaxDistance;

	private bool collidersEnabled = true;

	private void OnEnable()
	{
		sqrColliderEnableMaxDistance = ColliderEnableMaxDistance * ColliderEnableMaxDistance;
		if (PlayerSingleton<PlayerMovement>.InstanceExists)
		{
			RegisterEvent();
		}
		else
		{
			Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(RegisterEvent));
		}
	}

	private void OnDestroy()
	{
		if (PlayerSingleton<PlayerMovement>.InstanceExists)
		{
			PlayerSingleton<PlayerMovement>.Instance.DeregisterMovementEvent(Refresh);
		}
	}

	private void RegisterEvent()
	{
		Player.onLocalPlayerSpawned = (Action)Delegate.Remove(Player.onLocalPlayerSpawned, new Action(RegisterEvent));
		PlayerSingleton<PlayerMovement>.Instance.RegisterMovementEvent(5, Refresh);
	}

	[Button]
	public void GetColliders()
	{
		Colliders = ((Component)this).GetComponentsInChildren<Collider>();
	}

	public void Start()
	{
	}

	private void Refresh()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)Player.Local == (Object)null) && !((Object)(object)Player.Local.Avatar == (Object)null))
		{
			Vector3 val = Player.Local.Avatar.CenterPoint - ((Component)this).transform.position;
			float sqrMagnitude = ((Vector3)(ref val)).sqrMagnitude;
			SetCollidersEnabled(sqrMagnitude < sqrColliderEnableMaxDistance);
		}
	}

	private void SetCollidersEnabled(bool enabled)
	{
		if (collidersEnabled == enabled)
		{
			return;
		}
		collidersEnabled = enabled;
		Collider[] colliders = Colliders;
		foreach (Collider val in colliders)
		{
			if (!((Object)(object)val == (Object)null))
			{
				val.enabled = enabled;
			}
		}
	}
}
