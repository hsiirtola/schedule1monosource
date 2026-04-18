using System;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Configuration;

public class ConfigurationServiceNetworker : NetworkBehaviour
{
	private bool NetworkInitialize___EarlyScheduleOne_002EConfiguration_002EConfigurationServiceNetworkerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EConfiguration_002EConfigurationServiceNetworkerAssembly_002DCSharp_002Edll_Excuted;

	private ConfigurationService _configurationService => Singleton<ConfigurationService>.Instance;

	public override void OnStartServer()
	{
		((NetworkBehaviour)this).OnStartServer();
		BaseConfiguration[] configurations = _configurationService.Configurations;
		foreach (BaseConfiguration obj in configurations)
		{
			obj.OnConfigurationChanged = (Action<BaseConfiguration>)Delegate.Combine(obj.OnConfigurationChanged, new Action<BaseConfiguration>(OnConfigChanged));
		}
	}

	private void OnDestroy()
	{
		BaseConfiguration[] configurations = _configurationService.Configurations;
		foreach (BaseConfiguration obj in configurations)
		{
			obj.OnConfigurationChanged = (Action<BaseConfiguration>)Delegate.Remove(obj.OnConfigurationChanged, new Action<BaseConfiguration>(OnConfigChanged));
		}
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (!connection.IsHost)
		{
			BaseConfiguration[] configurations = _configurationService.Configurations;
			foreach (BaseConfiguration baseConfiguration in configurations)
			{
				Debug.Log((object)$"Sending settings for configuration {((Object)baseConfiguration).name} to client {connection.ClientId}");
				string settingsJson = baseConfiguration.GetSettings().Serialize();
				ApplySettingsJson(connection, ((Object)baseConfiguration).name, settingsJson);
			}
		}
	}

	private void OnConfigChanged(BaseConfiguration changedConfig)
	{
		string settingsJson = changedConfig.GetSettings().Serialize();
		ApplySettingsJson(null, ((Object)changedConfig).name, settingsJson);
	}

	[ObserversRpc]
	[TargetRpc]
	private void ApplySettingsJson(NetworkConnection conn, string configName, string settingsJson)
	{
		if (conn == null)
		{
			RpcWriter___Observers_ApplySettingsJson_3895153758(conn, configName, settingsJson);
		}
		else
		{
			RpcWriter___Target_ApplySettingsJson_3895153758(conn, configName, settingsJson);
		}
	}

	public override void NetworkInitialize___Early()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EConfiguration_002EConfigurationServiceNetworkerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EConfiguration_002EConfigurationServiceNetworkerAssembly_002DCSharp_002Edll_Excuted = true;
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_ApplySettingsJson_3895153758));
			((NetworkBehaviour)this).RegisterTargetRpc(1u, new ClientRpcDelegate(RpcReader___Target_ApplySettingsJson_3895153758));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EConfiguration_002EConfigurationServiceNetworkerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EConfiguration_002EConfigurationServiceNetworkerAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_ApplySettingsJson_3895153758(NetworkConnection conn, string configName, string settingsJson)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(configName);
			((Writer)writer).WriteString(settingsJson);
			((NetworkBehaviour)this).SendObserversRpc(0u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ApplySettingsJson_3895153758(NetworkConnection conn, string configName, string settingsJson)
	{
		if (!_configurationService.TryGetConfiguration(configName, out var configuration))
		{
			Debug.LogError((object)$"Failed to apply settings for configuration {configName} on client {conn.ClientId}: Configuration not found.");
			return;
		}
		Debug.Log((object)$"Applying settings for configuration {configName} on client {conn.ClientId}: {settingsJson}");
		configuration.GetSettings().Deserialize(settingsJson);
	}

	private void RpcReader___Observers_ApplySettingsJson_3895153758(PooledReader PooledReader0, Channel channel)
	{
		string configName = ((Reader)PooledReader0).ReadString();
		string settingsJson = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ApplySettingsJson_3895153758(null, configName, settingsJson);
		}
	}

	private void RpcWriter___Target_ApplySettingsJson_3895153758(NetworkConnection conn, string configName, string settingsJson)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(configName);
			((Writer)writer).WriteString(settingsJson);
			((NetworkBehaviour)this).SendTargetRpc(1u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_ApplySettingsJson_3895153758(PooledReader PooledReader0, Channel channel)
	{
		string configName = ((Reader)PooledReader0).ReadString();
		string settingsJson = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ApplySettingsJson_3895153758(((NetworkBehaviour)this).LocalConnection, configName, settingsJson);
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}
}
