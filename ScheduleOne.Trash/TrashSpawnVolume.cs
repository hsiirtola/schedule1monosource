using System;
using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using UnityEngine;

namespace ScheduleOne.Trash;

public class TrashSpawnVolume : MonoBehaviour
{
	public BoxCollider CreatonVolume;

	public BoxCollider DetectionVolume;

	public int TrashLimit = 10;

	public float TrashSpawnChance = 1f;

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

	public void SleepStart()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		if (!InstanceFinder.IsServer || Random.value > TrashSpawnChance)
		{
			return;
		}
		Collider[] array = Physics.OverlapBox(((Component)DetectionVolume).transform.TransformPoint(DetectionVolume.center), Vector3.Scale(DetectionVolume.size, ((Component)DetectionVolume).transform.lossyScale) * 0.5f, ((Component)DetectionVolume).transform.rotation, 1 << LayerMask.NameToLayer("Trash"), (QueryTriggerInteraction)2);
		int num = 0;
		Collider[] array2 = array;
		foreach (Collider val in array2)
		{
			if (num >= TrashLimit)
			{
				break;
			}
			if ((Object)(object)((Component)val).GetComponentInParent<TrashItem>() != (Object)null)
			{
				num++;
			}
		}
		num = Mathf.Max(Random.Range(0, TrashLimit - num), 0);
		Vector3 posiiton = default(Vector3);
		for (int j = num; j < TrashLimit; j++)
		{
			TrashItem randomGeneratableTrashPrefab = NetworkSingleton<TrashManager>.Instance.GetRandomGeneratableTrashPrefab();
			Bounds bounds = ((Collider)CreatonVolume).bounds;
			float x = ((Bounds)(ref bounds)).min.x;
			bounds = ((Collider)CreatonVolume).bounds;
			float num2 = Random.Range(x, ((Bounds)(ref bounds)).max.x);
			bounds = ((Collider)CreatonVolume).bounds;
			float y = ((Bounds)(ref bounds)).min.y;
			bounds = ((Collider)CreatonVolume).bounds;
			float num3 = Random.Range(y, ((Bounds)(ref bounds)).max.y);
			bounds = ((Collider)CreatonVolume).bounds;
			float z = ((Bounds)(ref bounds)).min.z;
			bounds = ((Collider)CreatonVolume).bounds;
			((Vector3)(ref posiiton))._002Ector(num2, num3, Random.Range(z, ((Bounds)(ref bounds)).max.z));
			NetworkSingleton<TrashManager>.Instance.CreateTrashItem(randomGeneratableTrashPrefab.ID, posiiton, Random.rotation).SetContinuousCollisionDetection();
		}
	}
}
