using System.Collections.Generic;
using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Doors;

[RequireComponent(typeof(Rigidbody))]
public class DoorSensor : MonoBehaviour
{
	public const float ActivationDistance = 30f;

	public EDoorSide DetectorSide = EDoorSide.Exterior;

	public DoorController Door;

	private Collider collider;

	private List<Collider> exclude = new List<Collider>();

	private List<NPC> npcsInContact = new List<NPC>();

	private List<Player> playersInContact = new List<Player>();

	private float maxContactDistanceSqr;

	private void Awake()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		collider = ((Component)this).GetComponent<Collider>();
		((MonoBehaviour)this).InvokeRepeating("UpdateCollider", 0f, 1f);
		((MonoBehaviour)this).InvokeRepeating("RemoveInvalidContacts", Random.Range(0f, 1f), 1f);
		float[] array = new float[3];
		Bounds bounds = collider.bounds;
		array[0] = ((Bounds)(ref bounds)).size.x;
		bounds = collider.bounds;
		array[1] = ((Bounds)(ref bounds)).size.y;
		bounds = collider.bounds;
		array[2] = ((Bounds)(ref bounds)).size.z;
		float num = Mathf.Max(array);
		maxContactDistanceSqr = (num + 1f) * (num + 1f);
	}

	private void UpdateCollider()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)PlayerSingleton<PlayerCamera>.Instance == (Object)null))
		{
			float distance = Vector3.Distance(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position, ((Component)this).transform.position);
			if (InstanceFinder.IsServer)
			{
				Player.GetClosestPlayer(((Component)this).transform.position, out distance);
			}
			collider.enabled = distance < 30f;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (exclude.Contains(other))
		{
			return;
		}
		NPC componentInParent = ((Component)other).GetComponentInParent<NPC>();
		if ((Object)(object)componentInParent != (Object)null && componentInParent.IsConscious && !componentInParent.Avatar.Ragdolled && componentInParent.CanOpenDoors && !npcsInContact.Contains(componentInParent))
		{
			Door.NPCVicinityEnter(DetectorSide);
			npcsInContact.Add(componentInParent);
			return;
		}
		Player componentInParent2 = ((Component)other).GetComponentInParent<Player>();
		if ((Object)(object)componentInParent2 != (Object)null && !playersInContact.Contains(componentInParent2))
		{
			Door.PlayerVicinityEnter(DetectorSide);
			playersInContact.Add(componentInParent2);
		}
		else
		{
			exclude.Add(other);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (exclude.Contains(other))
		{
			exclude.Remove(other);
		}
		NPC componentInParent = ((Component)other).GetComponentInParent<NPC>();
		if ((Object)(object)componentInParent != (Object)null && npcsInContact.Contains(componentInParent))
		{
			npcsInContact.Remove(componentInParent);
			Door.NPCVicinityExit(DetectorSide);
			return;
		}
		Player componentInParent2 = ((Component)other).GetComponentInParent<Player>();
		if ((Object)(object)componentInParent2 != (Object)null && playersInContact.Contains(componentInParent2))
		{
			playersInContact.Remove(componentInParent2);
			Door.PlayerVicinityExit(DetectorSide);
		}
	}

	private void RemoveInvalidContacts()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val;
		for (int i = 0; i < npcsInContact.Count; i++)
		{
			if (!((Object)(object)npcsInContact[i] == (Object)null))
			{
				val = npcsInContact[i].CenterPoint - ((Component)this).transform.position;
				if (!(((Vector3)(ref val)).sqrMagnitude > maxContactDistanceSqr))
				{
					continue;
				}
			}
			npcsInContact.RemoveAt(i);
			i--;
		}
		for (int j = 0; j < playersInContact.Count; j++)
		{
			if (!((Object)(object)playersInContact[j] == (Object)null))
			{
				val = playersInContact[j].CenterPointTransform.position - ((Component)this).transform.position;
				if (!(((Vector3)(ref val)).sqrMagnitude > maxContactDistanceSqr))
				{
					continue;
				}
			}
			playersInContact.RemoveAt(j);
			j--;
		}
	}
}
