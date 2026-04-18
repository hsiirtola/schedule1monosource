using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Audio;
using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using ScheduleOne.Doors;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Map;

[DisallowMultipleComponent]
public class NPCEnterableBuilding : MonoBehaviour, IGUIDRegisterable
{
	public const float DOOR_SOUND_DISTANCE_LIMIT = 15f;

	[Header("Settings")]
	public string BuildingName;

	[SerializeField]
	protected string BakedGUID = string.Empty;

	[Header("References")]
	public StaticDoor[] Doors;

	[Header("Readonly")]
	[SerializeField]
	private List<NPC> Occupants = new List<NPC>();

	public Guid GUID { get; protected set; }

	public int OccupantCount => Occupants.Count;

	protected virtual void Awake()
	{
		if (!GUIDManager.IsGUIDValid(BakedGUID))
		{
			Console.LogError(((Object)((Component)this).gameObject).name + "'s baked GUID is not valid! Bad.");
		}
		GUID = new Guid(BakedGUID);
		GUIDManager.RegisterObject(this);
		if (Doors.Length == 0)
		{
			GetDoors();
			if (Doors.Length == 0)
			{
				Console.LogError(BuildingName + " has no doors! NPCs won't be able to enter the building.");
			}
		}
		for (int i = 0; i < Doors.Length; i++)
		{
			if ((Object)(object)Doors[i] == (Object)null)
			{
				Console.LogError($"Door {i} in {BuildingName} is null!", (Object)(object)this);
			}
		}
	}

	public void SetGUID(Guid guid)
	{
		GUID = guid;
		GUIDManager.RegisterObject(this);
	}

	public virtual void NPCEnteredBuilding(NPC npc, StaticDoor door)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (!Occupants.Contains(npc))
		{
			Occupants.Add(npc);
		}
		if (PlayerSingleton<PlayerCamera>.InstanceExists && !(Vector3.Distance(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position, npc.Movement.FootPosition) > 15f) && (Object)(object)door.EnterSound != (Object)null)
		{
			door.EnterSound.Play();
		}
	}

	public virtual void NPCExitedBuilding(NPC npc, StaticDoor door)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Occupants.Remove(npc);
		if (PlayerSingleton<PlayerCamera>.InstanceExists && !(Vector3.Distance(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position, ((Component)npc.Avatar).transform.position) > 15f) && Singleton<AudioManager>.InstanceExists && NetworkSingleton<GameManager>.InstanceExists && (Object)(object)door.ExitSound != (Object)null)
		{
			door.ExitSound.Play();
		}
	}

	[Button]
	public void GetDoors()
	{
		Doors = ((Component)this).GetComponentsInChildren<StaticDoor>();
	}

	public List<NPC> GetSummonableNPCs()
	{
		return Occupants.Where((NPC npc) => npc.CanBeSummoned).ToList();
	}

	public StaticDoor GetClosestDoor(Vector3 pos, bool useableOnly)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		return (from door in Doors
			where !useableOnly || door.Usable
			orderby Vector3.Distance(((Component)door).transform.position, pos)
			select door).FirstOrDefault();
	}
}
