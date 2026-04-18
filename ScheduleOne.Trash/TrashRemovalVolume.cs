using System;
using System.Collections.Generic;
using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using UnityEngine;

namespace ScheduleOne.Trash;

[RequireComponent(typeof(BoxCollider))]
public class TrashRemovalVolume : MonoBehaviour
{
	public BoxCollider Collider;

	public float RemovalChance = 1f;

	public void Awake()
	{
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onSleepStart = (Action)Delegate.Combine(instance.onSleepStart, new Action(SleepStart));
	}

	private void OnDestroy()
	{
		if (NetworkSingleton<TimeManager>.InstanceExists)
		{
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onSleepStart = (Action)Delegate.Remove(instance.onSleepStart, new Action(SleepStart));
		}
	}

	private void SleepStart()
	{
		if (InstanceFinder.IsServer && !(Random.value > RemovalChance))
		{
			TrashItem[] trash = GetTrash();
			for (int i = 0; i < trash.Length; i++)
			{
				trash[i].DestroyTrash();
			}
		}
	}

	private TrashItem[] GetTrash()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		List<TrashItem> list = new List<TrashItem>();
		Vector3 val = ((Component)Collider).transform.TransformPoint(Collider.center);
		Vector3 val2 = Vector3.Scale(Collider.size, ((Component)Collider).transform.lossyScale) * 0.5f;
		Collider[] array = Physics.OverlapBox(val, val2, ((Component)Collider).transform.rotation, 1 << LayerMask.NameToLayer("Trash"), (QueryTriggerInteraction)2);
		for (int i = 0; i < array.Length; i++)
		{
			TrashItem componentInParent = ((Component)array[i]).GetComponentInParent<TrashItem>();
			if ((Object)(object)componentInParent != (Object)null)
			{
				list.Add(componentInParent);
			}
		}
		return list.ToArray();
	}
}
