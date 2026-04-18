using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Audio;
using ScheduleOne.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Misc;

public class ModularSwitch : NetworkBehaviour
{
	public delegate void ButtonChange(bool isOn);

	public bool isOn;

	[Header("References")]
	[SerializeField]
	protected InteractableObject intObj;

	[SerializeField]
	protected Transform button;

	public AudioSourceController OnAudio;

	public AudioSourceController OffAudio;

	public ToggleableLight[] LightsToControl;

	[Header("Settings")]
	[SerializeField]
	protected List<ModularSwitch> SwitchesToSyncWith = new List<ModularSwitch>();

	public ButtonChange onToggled;

	public UnityEvent switchedOn;

	public UnityEvent switchedOff;

	private bool NetworkInitialize___EarlyScheduleOne_002EMisc_002EModularSwitchAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EMisc_002EModularSwitchAssembly_002DCSharp_002Edll_Excuted;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EMisc_002EModularSwitch_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		SetIsOn(connection, isOn);
	}

	public void Hovered()
	{
		if (isOn)
		{
			intObj.SetMessage("Switch off");
		}
		else
		{
			intObj.SetMessage("Switch on");
		}
	}

	public void Interacted()
	{
		if (isOn)
		{
			SendIsOn(isOn: false);
		}
		else
		{
			SendIsOn(isOn: true);
		}
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	private void SendIsOn(bool isOn)
	{
		RpcWriter___Server_SendIsOn_1140765316(isOn);
		RpcLogic___SendIsOn_1140765316(isOn);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void SetIsOn(NetworkConnection conn, bool isOn)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetIsOn_214505783(conn, isOn);
			RpcLogic___SetIsOn_214505783(conn, isOn);
		}
		else
		{
			RpcWriter___Target_SetIsOn_214505783(conn, isOn);
		}
	}

	public void SwitchOn()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (!isOn)
		{
			isOn = true;
			button.localEulerAngles = new Vector3(-7f, 0f, 0f);
			if (switchedOn != null)
			{
				switchedOn.Invoke();
			}
			if (onToggled != null)
			{
				onToggled(isOn);
			}
			for (int i = 0; i < SwitchesToSyncWith.Count; i++)
			{
				SwitchesToSyncWith[i].SwitchOn();
			}
			OnAudio.Play();
		}
	}

	public void SwitchOff()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (isOn)
		{
			isOn = false;
			button.localEulerAngles = new Vector3(7f, 0f, 0f);
			if (switchedOff != null)
			{
				switchedOff.Invoke();
			}
			if (onToggled != null)
			{
				onToggled(isOn);
			}
			for (int i = 0; i < SwitchesToSyncWith.Count; i++)
			{
				SwitchesToSyncWith[i].SwitchOff();
			}
			OffAudio.Play();
		}
	}

	public override void NetworkInitialize___Early()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EMisc_002EModularSwitchAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EMisc_002EModularSwitchAssembly_002DCSharp_002Edll_Excuted = true;
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_SendIsOn_1140765316));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_SetIsOn_214505783));
			((NetworkBehaviour)this).RegisterTargetRpc(2u, new ClientRpcDelegate(RpcReader___Target_SetIsOn_214505783));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EMisc_002EModularSwitchAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EMisc_002EModularSwitchAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SendIsOn_1140765316(bool isOn)
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
			((Writer)writer).WriteBoolean(isOn);
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SendIsOn_1140765316(bool isOn)
	{
		SetIsOn(null, isOn);
	}

	private void RpcReader___Server_SendIsOn_1140765316(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		bool flag = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendIsOn_1140765316(flag);
		}
	}

	private void RpcWriter___Observers_SetIsOn_214505783(NetworkConnection conn, bool isOn)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteBoolean(isOn);
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetIsOn_214505783(NetworkConnection conn, bool isOn)
	{
		if (isOn)
		{
			SwitchOn();
		}
		else
		{
			SwitchOff();
		}
	}

	private void RpcReader___Observers_SetIsOn_214505783(PooledReader PooledReader0, Channel channel)
	{
		bool flag = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetIsOn_214505783(null, flag);
		}
	}

	private void RpcWriter___Target_SetIsOn_214505783(NetworkConnection conn, bool isOn)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteBoolean(isOn);
			((NetworkBehaviour)this).SendTargetRpc(2u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetIsOn_214505783(PooledReader PooledReader0, Channel channel)
	{
		bool flag = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetIsOn_214505783(((NetworkBehaviour)this).LocalConnection, flag);
		}
	}

	protected virtual void Awake_UserLogic_ScheduleOne_002EMisc_002EModularSwitch_Assembly_002DCSharp_002Edll()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		for (int i = 0; i < SwitchesToSyncWith.Count; i++)
		{
			if (!SwitchesToSyncWith[i].SwitchesToSyncWith.Contains(this))
			{
				SwitchesToSyncWith[i].SwitchesToSyncWith.Add(this);
			}
		}
		for (int j = 0; j < LightsToControl.Length; j++)
		{
			switchedOn.AddListener(new UnityAction(LightsToControl[j].TurnOn));
			switchedOff.AddListener(new UnityAction(LightsToControl[j].TurnOff));
		}
	}
}
