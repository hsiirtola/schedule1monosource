using System.Collections;
using System.Collections.Generic;
using FishNet;
using Pathfinding;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using ScheduleOne.Vehicles;
using UnityEngine;

namespace ScheduleOne.Map;

public class WaterCollider : MonoBehaviour
{
	private bool localPlayerBeingWarped;

	private List<LandVehicle> warpedVehicles = new List<LandVehicle>();

	public AudioSourceController SplashSound;

	public Transform OverrideWarpPoint;

	private void OnTriggerEnter(Collider other)
	{
		Player componentInParent = ((Component)other).GetComponentInParent<Player>();
		if ((Object)(object)componentInParent != (Object)null && (Object)(object)componentInParent == (Object)(object)Player.Local && componentInParent.Health.IsAlive && (Object)(object)componentInParent.CurrentVehicle == (Object)null && !localPlayerBeingWarped && (Object)(object)other == (Object)(object)componentInParent.CapCol && componentInParent.TimeSinceVehicleExit > 0.01f)
		{
			Console.Log("Player entered ocean: " + ((Object)((Component)other).gameObject).name);
			localPlayerBeingWarped = true;
			((MonoBehaviour)this).StartCoroutine(WarpPlayer());
			return;
		}
		if (InstanceFinder.IsServer)
		{
			NPC componentInParent2 = ((Component)other).GetComponentInParent<NPC>();
			if ((Object)(object)componentInParent2 != (Object)null && !componentInParent2.Health.IsDead && componentInParent2.Health.IsKnockedOut)
			{
				componentInParent2.Health.Die();
			}
		}
		LandVehicle componentInParent3 = ((Component)other).GetComponentInParent<LandVehicle>();
		if ((Object)(object)componentInParent3 != (Object)null)
		{
			Debug.Log((object)"Vehicle entered ocean", (Object)(object)((Component)componentInParent3).gameObject);
			if (((Object)(object)componentInParent3.DriverPlayer == (Object)(object)Player.Local || ((Object)(object)componentInParent3.DriverPlayer == (Object)null && InstanceFinder.IsHost)) && !warpedVehicles.Contains(componentInParent3))
			{
				warpedVehicles.Add(componentInParent3);
				((MonoBehaviour)this).StartCoroutine(WarpVehicle(componentInParent3));
			}
		}
	}

	private IEnumerator WarpPlayer()
	{
		((Component)SplashSound).transform.SetParent(((Component)Player.Local).gameObject.transform);
		((Component)SplashSound).transform.localPosition = Vector3.zero;
		SplashSound.Play();
		Singleton<BlackOverlay>.Instance.Open(0.05f);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
		yield return (object)new WaitForSeconds(0.12f);
		if ((Object)(object)OverrideWarpPoint != (Object)null)
		{
			PlayerSingleton<PlayerMovement>.Instance.Teleport(OverrideWarpPoint.position, alignFeetToPosition: true);
		}
		else
		{
			PlayerSingleton<PlayerMovement>.Instance.WarpToNavMesh();
		}
		yield return (object)new WaitForSeconds(0.2f);
		Singleton<BlackOverlay>.Instance.Close(0.3f);
		localPlayerBeingWarped = false;
		((Component)SplashSound).transform.SetParent(((Component)this).transform);
		yield return (object)new WaitForSeconds(0.2f);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
	}

	private IEnumerator WarpVehicle(LandVehicle veh)
	{
		bool faded = false;
		if (veh.LocalPlayerIsDriver)
		{
			faded = true;
			Singleton<BlackOverlay>.Instance.Open(0.15f);
		}
		yield return (object)new WaitForSeconds(0.16f);
		NNConstraint val = new NNConstraint();
		val.graphMask = GraphMask.FromGraphName("Road Nodes");
		NNInfo nearest = AstarPath.active.GetNearest(((Component)veh).transform.position, val);
		((Component)veh).transform.position = nearest.position + ((Component)this).transform.up * veh.BoundingBoxDimensions.y / 2f;
		((Component)veh).transform.rotation = Quaternion.identity;
		veh.Rb.velocity = Vector3.zero;
		veh.Rb.angularVelocity = Vector3.zero;
		yield return (object)new WaitForSeconds(0.2f);
		if (faded)
		{
			Singleton<BlackOverlay>.Instance.Close(0.3f);
		}
		warpedVehicles.Remove(veh);
	}
}
