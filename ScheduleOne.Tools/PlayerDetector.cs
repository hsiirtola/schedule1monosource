using System;
using System.Collections.Generic;
using FishNet.Object;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Vehicles;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Tools;

[RequireComponent(typeof(Rigidbody))]
public class PlayerDetector : MonoBehaviour
{
	public const float ACTIVATION_DISTANCE_SQ = 400f;

	public bool DetectPlayerInVehicle;

	public UnityEvent<Player> onPlayerEnter;

	public UnityEvent<Player> onPlayerExit;

	public UnityEvent onLocalPlayerEnter;

	public UnityEvent onLocalPlayerExit;

	public List<Player> DetectedPlayers = new List<Player>();

	private bool ignoreExit;

	private bool collidersEnabled = true;

	private Collider[] detectionColliders;

	public bool IgnoreNewDetections { get; protected set; }

	private void Awake()
	{
		Rigidbody val = ((Component)this).GetComponent<Rigidbody>();
		if ((Object)(object)val == (Object)null)
		{
			val = ((Component)this).gameObject.AddComponent<Rigidbody>();
		}
		val.isKinematic = true;
		detectionColliders = ((Component)this).GetComponentsInChildren<Collider>();
	}

	private void Start()
	{
		NetworkSingleton<TimeManager>.Instance.onTick -= new Action(OnTick);
		NetworkSingleton<TimeManager>.Instance.onTick += new Action(OnTick);
	}

	private void OnDestroy()
	{
		if (NetworkSingleton<TimeManager>.InstanceExists)
		{
			NetworkSingleton<TimeManager>.Instance.onTick -= new Action(OnTick);
		}
	}

	private void OnTick()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		for (int i = 0; i < Player.PlayerList.Count; i++)
		{
			if (Vector3.SqrMagnitude(Player.PlayerList[i].Avatar.CenterPoint - ((Component)this).transform.position) < 400f)
			{
				flag = true;
				break;
			}
		}
		if (flag != collidersEnabled)
		{
			collidersEnabled = flag;
			for (int j = 0; j < detectionColliders.Length; j++)
			{
				detectionColliders[j].enabled = collidersEnabled;
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (IgnoreNewDetections)
		{
			return;
		}
		Player componentInParent = ((Component)other).GetComponentInParent<Player>();
		if ((Object)(object)componentInParent != (Object)null && !DetectedPlayers.Contains(componentInParent) && (Object)(object)other == (Object)(object)componentInParent.CapCol)
		{
			DetectedPlayers.Add(componentInParent);
			if (onPlayerEnter != null)
			{
				onPlayerEnter.Invoke(componentInParent);
			}
			if (((NetworkBehaviour)componentInParent).IsOwner && onLocalPlayerEnter != null)
			{
				onLocalPlayerEnter.Invoke();
			}
		}
		if (!DetectPlayerInVehicle)
		{
			return;
		}
		LandVehicle componentInParent2 = ((Component)other).GetComponentInParent<LandVehicle>();
		if (!((Object)(object)componentInParent2 != (Object)null))
		{
			return;
		}
		foreach (Player occupantPlayer in componentInParent2.OccupantPlayers)
		{
			if ((Object)(object)occupantPlayer != (Object)null && !DetectedPlayers.Contains(occupantPlayer))
			{
				DetectedPlayers.Add(occupantPlayer);
				if (onPlayerEnter != null)
				{
					onPlayerEnter.Invoke(occupantPlayer);
				}
				if (((NetworkBehaviour)occupantPlayer).IsOwner && onLocalPlayerEnter != null)
				{
					onLocalPlayerEnter.Invoke();
				}
			}
		}
	}

	private void FixedUpdate()
	{
		for (int i = 0; i < DetectedPlayers.Count; i++)
		{
			if ((Object)(object)DetectedPlayers[i].CurrentVehicle != (Object)null)
			{
				OnTriggerExit((Collider)(object)DetectedPlayers[i].CapCol);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (ignoreExit)
		{
			return;
		}
		Player componentInParent = ((Component)other).GetComponentInParent<Player>();
		if ((Object)(object)componentInParent != (Object)null && DetectedPlayers.Contains(componentInParent) && (Object)(object)other == (Object)(object)componentInParent.CapCol)
		{
			DetectedPlayers.Remove(componentInParent);
			if (onPlayerExit != null)
			{
				onPlayerExit.Invoke(componentInParent);
			}
			if (((NetworkBehaviour)componentInParent).IsOwner && onLocalPlayerExit != null)
			{
				onLocalPlayerExit.Invoke();
			}
		}
		if (!DetectPlayerInVehicle)
		{
			return;
		}
		LandVehicle componentInParent2 = ((Component)other).GetComponentInParent<LandVehicle>();
		if (!((Object)(object)componentInParent2 != (Object)null))
		{
			return;
		}
		foreach (Player occupantPlayer in componentInParent2.OccupantPlayers)
		{
			if ((Object)(object)occupantPlayer != (Object)null && DetectedPlayers.Contains(occupantPlayer))
			{
				DetectedPlayers.Remove(occupantPlayer);
				if (onPlayerExit != null)
				{
					onPlayerExit.Invoke(occupantPlayer);
				}
				if (((NetworkBehaviour)occupantPlayer).IsOwner && onLocalPlayerExit != null)
				{
					onLocalPlayerExit.Invoke();
				}
			}
		}
	}

	public void SetIgnoreNewCollisions(bool ignore)
	{
		IgnoreNewDetections = ignore;
		if (ignore)
		{
			return;
		}
		ignoreExit = true;
		Collider[] componentsInChildren = ((Component)this).GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].isTrigger)
			{
				componentsInChildren[i].enabled = false;
				componentsInChildren[i].enabled = true;
			}
		}
		ignoreExit = false;
	}
}
