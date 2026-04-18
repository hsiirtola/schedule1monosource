using System;
using System.Collections;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Law;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.NPCs.Other;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.NPCs.Actions;

public class NPCActions : NetworkBehaviour
{
	private NPC npc;

	private bool _canUseUmbrella;

	private UseUmbrella _umbrellaAction;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EActions_002ENPCActionsAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EActions_002ENPCActionsAssembly_002DCSharp_002Edll_Excuted;

	protected NPCBehaviour behaviour => npc.Behaviour;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ENPCs_002EActions_002ENPCActions_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	private void Start()
	{
		_umbrellaAction = npc.Behaviour.ScheduleManager.DiscreteActions.Find((NPCDiscreteAction a) => a is UseUmbrella) as UseUmbrella;
		NetworkSingleton<TimeManager>.Instance.onUncappedMinutePass += new Action(UpdateUmbrellaUse);
	}

	private void OnDestroy()
	{
		if ((Object)(object)NetworkSingleton<TimeManager>.Instance != (Object)null)
		{
			NetworkSingleton<TimeManager>.Instance.onUncappedMinutePass -= new Action(UpdateUmbrellaUse);
		}
	}

	public void Cower()
	{
		behaviour.GetBehaviour("Cowering").Enable_Networked();
		((MonoBehaviour)this).StartCoroutine(Wait());
		IEnumerator Wait()
		{
			yield return (object)new WaitForSeconds(10f);
			behaviour.GetBehaviour("Cowering").Disable_Networked(null);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void CallPolice_Networked(NetworkObject playerObj)
	{
		RpcWriter___Server_CallPolice_Networked_3323014238(playerObj);
		RpcLogic___CallPolice_Networked_3323014238(playerObj);
	}

	public void SetCallPoliceBehaviourCrime(Crime crime)
	{
		npc.Behaviour.CallPoliceBehaviour.ReportedCrime = crime;
	}

	public void FacePlayer(Player player)
	{
	}

	public void SetCanUseUmbrella(bool canUseUmbrella)
	{
		_canUseUmbrella = canUseUmbrella;
		((MonoBehaviour)this).StartCoroutine(Wait());
		IEnumerator Wait()
		{
			yield return null;
			UpdateUmbrellaUse();
		}
	}

	private void UpdateUmbrellaUse()
	{
		float rainy = npc.GetWeatherTolerence().Rainy;
		if (GetRainAmount() > rainy && !npc.IsUnderCover)
		{
			if (_canUseUmbrella && npc.HasUmbrella)
			{
				if (!_umbrellaAction.IsActive)
				{
					_umbrellaAction.Begin();
				}
				return;
			}
			if (_umbrellaAction.IsActive)
			{
				_umbrellaAction.End();
			}
			float speed = npc.Movement.SpeedController.DefaultWalkSpeed * Mathf.Lerp(1f, npc.WalkInRainMaxSpeedMultiplier, (GetRainAmount() - rainy) / (1f - rainy));
			npc.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("rainy", 1, speed));
		}
		else
		{
			if (_umbrellaAction.IsActive)
			{
				_umbrellaAction.End();
			}
			npc.Movement.SpeedController.RemoveSpeedControl("rainy");
		}
	}

	private float GetRainAmount()
	{
		return npc.GetCurrentWeatherConditionsForEnitty()?.Rainy ?? 0f;
	}

	public override void NetworkInitialize___Early()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EActions_002ENPCActionsAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EActions_002ENPCActionsAssembly_002DCSharp_002Edll_Excuted = true;
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_CallPolice_Networked_3323014238));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EActions_002ENPCActionsAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EActions_002ENPCActionsAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_CallPolice_Networked_3323014238(NetworkObject playerObj)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteNetworkObject(playerObj);
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___CallPolice_Networked_3323014238(NetworkObject playerObj)
	{
		if (NetworkSingleton<GameManager>.Instance.IsTutorial)
		{
			return;
		}
		Player component = ((Component)playerObj).GetComponent<Player>();
		if ((Object)(object)component == (Object)null || !npc.IsConscious)
		{
			return;
		}
		Console.Log(npc.fullName + " is calling the police on " + component.PlayerName);
		if (component.CrimeData.CurrentPursuitLevel != PlayerCrimeData.EPursuitLevel.None)
		{
			Console.LogWarning("Player is already being pursued, ignoring call police request.");
			return;
		}
		npc.Behaviour.CallPoliceBehaviour.Target = component;
		if (InstanceFinder.IsServer)
		{
			npc.Behaviour.CallPoliceBehaviour.Enable_Networked();
		}
	}

	private void RpcReader___Server_CallPolice_Networked_3323014238(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkObject playerObj = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___CallPolice_Networked_3323014238(playerObj);
		}
	}

	protected virtual void Awake_UserLogic_ScheduleOne_002ENPCs_002EActions_002ENPCActions_Assembly_002DCSharp_002Edll()
	{
		npc = ((Component)this).GetComponentInParent<NPC>();
	}
}
