using System;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Object;
using ScheduleOne.DevUtilities;
using ScheduleOne.Doors;
using ScheduleOne.Law;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Police;
using ScheduleOne.Vehicles;
using UnityEngine;

namespace ScheduleOne.Map;

public class PoliceStation : NPCEnterableBuilding
{
	public enum EDispatchType
	{
		Auto,
		UseVehicle,
		OnFoot
	}

	public static List<PoliceStation> PoliceStations = new List<PoliceStation>();

	public int VehicleLimit = 5;

	[Header("References")]
	public Transform SpawnPoint;

	public Transform[] VehicleSpawnPoints;

	public Transform[] PossessedVehicleSpawnPoints;

	[Header("Prefabs")]
	public LandVehicle[] PoliceVehiclePrefabs;

	public List<PoliceOfficer> OfficerPool = new List<PoliceOfficer>();

	[SerializeField]
	private List<LandVehicle> deployedVehicles = new List<LandVehicle>();

	public float TimeSinceLastDispatch { get; private set; }

	private int deployedVehicleCount => deployedVehicles.Where((LandVehicle v) => (Object)(object)v != (Object)null).Count();

	protected override void Awake()
	{
		base.Awake();
		if (!PoliceStations.Contains(this))
		{
			PoliceStations.Add(this);
		}
		((MonoBehaviour)this).InvokeRepeating("CleanVehicleList", 0f, 5f);
	}

	private void OnDestroy()
	{
		if (PoliceStations.Contains(this))
		{
			PoliceStations.Remove(this);
		}
	}

	private void Update()
	{
		TimeSinceLastDispatch += Time.deltaTime;
	}

	private void CleanVehicleList()
	{
		for (int i = 0; i < deployedVehicles.Count; i++)
		{
			if ((Object)(object)deployedVehicles[i] == (Object)null)
			{
				deployedVehicles.RemoveAt(i);
				i--;
			}
		}
	}

	public void Dispatch(int requestedOfficerCount, Player targetPlayer, EDispatchType type = EDispatchType.Auto, bool beginAsSighted = false)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		if (!InstanceFinder.IsServer)
		{
			Console.LogWarning("Attempted to dispatch officers from a client, this is not allowed.");
		}
		else
		{
			if (requestedOfficerCount <= 0)
			{
				return;
			}
			if (requestedOfficerCount > 4)
			{
				Console.LogWarning("Attempted to dispatch more than 4 officers, this is not allowed.");
				return;
			}
			List<PoliceOfficer> list = new List<PoliceOfficer>();
			for (int i = 0; i < requestedOfficerCount; i++)
			{
				if (OfficerPool.Count > 0)
				{
					list.Add(PullOfficer());
				}
			}
			if (list.Count == 0)
			{
				Console.LogWarning("Attempted to dispatch officers, but there are no officers in the pool.");
				return;
			}
			bool flag = false;
			switch (type)
			{
			case EDispatchType.Auto:
				flag = Vector3.Distance(targetPlayer.CrimeData.LastKnownPosition, SpawnPoint.position) > LawManager.DISPATCH_VEHICLE_USE_THRESHOLD || (Object)(object)targetPlayer.CurrentVehicle != (Object)null;
				break;
			case EDispatchType.UseVehicle:
				flag = true;
				break;
			}
			if (flag && deployedVehicleCount < VehicleLimit)
			{
				LandVehicle landVehicle = CreateVehicle();
				list[0].AssignedVehicle = landVehicle;
				list[0].EnterVehicle(null, landVehicle);
				for (int j = 0; j < list.Count; j++)
				{
					list[j].BeginVehiclePursuit_Networked(targetPlayer.PlayerCode, ((NetworkBehaviour)landVehicle).NetworkObject, beginAsSighted);
				}
			}
			for (int k = 0; k < list.Count; k++)
			{
				list[k].BeginFootPursuit_Networked(targetPlayer.PlayerCode);
			}
			TimeSinceLastDispatch = 0f;
		}
	}

	public PoliceOfficer PullOfficer()
	{
		if (OfficerPool.Count == 0)
		{
			return null;
		}
		PoliceOfficer policeOfficer = OfficerPool[Random.Range(0, OfficerPool.Count)];
		OfficerPool.Remove(policeOfficer);
		policeOfficer.Activate();
		return policeOfficer;
	}

	public LandVehicle CreateVehicle()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		Transform target = VehicleSpawnPoints[0];
		for (int i = 0; i < VehicleSpawnPoints.Length; i++)
		{
			if (IsSpawnPointAvailable(VehicleSpawnPoints[i]))
			{
				target = VehicleSpawnPoints[i];
				break;
			}
		}
		LandVehicle landVehicle = PoliceVehiclePrefabs[Random.Range(0, PoliceVehiclePrefabs.Length)];
		Tuple<Vector3, Quaternion> alignmentTransform = landVehicle.GetAlignmentTransform(target, EParkingAlignment.RearToKerb);
		LandVehicle landVehicle2 = NetworkSingleton<VehicleManager>.Instance.SpawnAndReturnVehicle(landVehicle.VehicleCode, alignmentTransform.Item1, alignmentTransform.Item2, playerOwned: false);
		deployedVehicles.Add(landVehicle2);
		return landVehicle2;
		static bool IsSpawnPointAvailable(Transform spawnPoint)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			Collider[] array = Physics.OverlapSphere(spawnPoint.position, 2f, 1 << LayerMask.NameToLayer("Vehicle"));
			for (int j = 0; j < array.Length; j++)
			{
				if ((Object)(object)((Component)array[j]).GetComponentInParent<LandVehicle>() != (Object)null)
				{
					return false;
				}
			}
			return true;
		}
	}

	public override void NPCEnteredBuilding(NPC npc, StaticDoor door)
	{
		base.NPCEnteredBuilding(npc, door);
		if (npc is PoliceOfficer && !OfficerPool.Contains(npc as PoliceOfficer))
		{
			OfficerPool.Add(npc as PoliceOfficer);
		}
	}

	public override void NPCExitedBuilding(NPC npc, StaticDoor door)
	{
		base.NPCExitedBuilding(npc, door);
		if (npc is PoliceOfficer)
		{
			OfficerPool.Remove(npc as PoliceOfficer);
		}
	}

	public static PoliceStation GetClosestPoliceStation(Vector3 point)
	{
		return PoliceStations[0];
	}
}
