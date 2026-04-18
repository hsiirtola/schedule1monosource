using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Audio;
using ScheduleOne.EntityFramework;
using ScheduleOne.Interaction;
using ScheduleOne.Tiles;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.ObjectScripts;

public class Sprinkler : GridItem
{
	[Header("References")]
	public InteractableObject IntObj;

	public ParticleSystem[] WaterParticles;

	public AudioSourceController ClickSound;

	public AudioSourceController WaterSound;

	[Header("Settings")]
	public float ApplyWaterDelay = 6f;

	public float ParticleStopDelay = 2.5f;

	public float Cooldown;

	public List<Coordinate> TilesToWater = new List<Coordinate>();

	public int MinTilesToWater = 1;

	public UnityEvent onSprinklerStart;

	private bool NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002ESprinklerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EObjectScripts_002ESprinklerAssembly_002DCSharp_002Edll_Excuted;

	public bool IsSprinkling { get; private set; }

	public void Hovered()
	{
		if (!isGhost)
		{
			if (CanWater())
			{
				IntObj.SetMessage("Activate sprinkler");
				IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
			}
			else
			{
				IntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
			}
		}
	}

	public void Interacted()
	{
		if (!isGhost && CanWater())
		{
			SendWater();
		}
	}

	private bool CanWater()
	{
		return !IsSprinkling;
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	private void SendWater()
	{
		RpcWriter___Server_SendWater_2166136261();
		RpcLogic___SendWater_2166136261();
	}

	[ObserversRpc(RunLocally = true)]
	private void Water()
	{
		RpcWriter___Observers_Water_2166136261();
		RpcLogic___Water_2166136261();
	}

	public void AddWater(float normalizedAmount)
	{
		if (InstanceFinder.IsServer)
		{
			List<Pot> pots = GetPots();
			for (int i = 0; i < pots.Count; i++)
			{
				pots[i].ChangeMoistureAmount(pots[i].MoistureCapacity * normalizedAmount);
				pots[i].SyncMoistureData();
			}
		}
	}

	protected virtual List<Pot> GetPots()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		List<Tile> list = new List<Tile>();
		for (int i = 0; i < TilesToWater.Count; i++)
		{
			Coordinate coord = new Coordinate(_originCoordinate) + Coordinate.RotateCoordinates(TilesToWater[i], _rotation);
			Tile tile = base.OwnerGrid.GetTile(coord);
			if ((Object)(object)tile != (Object)null && !list.Contains(tile))
			{
				list.Add(tile);
			}
		}
		List<Pot> list2 = new List<Pot>();
		Dictionary<Pot, int> potTileCounts = new Dictionary<Pot, int>();
		for (int j = 0; j < list.Count; j++)
		{
			foreach (GridItem buildableOccupant in list[j].BuildableOccupants)
			{
				if (buildableOccupant is Pot pot)
				{
					if (!list2.Contains(pot))
					{
						list2.Add(pot);
						potTileCounts.Add(pot, 1);
					}
					else
					{
						potTileCounts[pot]++;
					}
				}
			}
		}
		return list2.FindAll((Pot key) => potTileCounts[key] >= MinTilesToWater);
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002ESprinklerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002ESprinklerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterServerRpc(5u, new ServerRpcDelegate(RpcReader___Server_SendWater_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(6u, new ClientRpcDelegate(RpcReader___Observers_Water_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EObjectScripts_002ESprinklerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EObjectScripts_002ESprinklerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SendWater_2166136261()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkBehaviour)this).IsClientInitialized)
		{
			NetworkManager networkManager = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if (networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((NetworkBehaviour)this).SendServerRpc(5u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SendWater_2166136261()
	{
		Water();
	}

	private void RpcReader___Server_SendWater_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendWater_2166136261();
		}
	}

	private void RpcWriter___Observers_Water_2166136261()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkBehaviour)this).IsServerInitialized)
		{
			NetworkManager networkManager = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if (networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((NetworkBehaviour)this).SendObserversRpc(6u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___Water_2166136261()
	{
		if (!IsSprinkling)
		{
			IsSprinkling = true;
			ClickSound.Play();
			((MonoBehaviour)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			if (onSprinklerStart != null)
			{
				onSprinklerStart.Invoke();
			}
			WaterSound.Play();
			for (int i = 0; i < WaterParticles.Length; i++)
			{
				WaterParticles[i].Play();
			}
			int segments = 5;
			for (int j = 0; j < segments; j++)
			{
				yield return (object)new WaitForSeconds(ApplyWaterDelay / (float)segments);
				if (InstanceFinder.IsServer)
				{
					AddWater(1f / (float)segments);
				}
			}
			yield return (object)new WaitForSeconds(ParticleStopDelay);
			for (int k = 0; k < WaterParticles.Length; k++)
			{
				WaterParticles[k].Stop();
			}
			WaterSound.Stop();
			yield return (object)new WaitForSeconds(Cooldown);
			IsSprinkling = false;
		}
	}

	private void RpcReader___Observers_Water_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___Water_2166136261();
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
