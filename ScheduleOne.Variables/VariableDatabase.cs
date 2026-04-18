using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Core;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Variables;

public class VariableDatabase : NetworkSingleton<VariableDatabase>, IBaseSaveable, ISaveable
{
	public enum EVariableType
	{
		Bool,
		Number
	}

	public List<BaseVariable> VariableList = new List<BaseVariable>();

	public Dictionary<string, BaseVariable> VariableDict = new Dictionary<string, BaseVariable>();

	private List<string> playerVariables = new List<string>();

	public VariableCreator[] Creators;

	public StorableItemDefinition[] ItemsToTrackAcquire;

	private VariablesLoader loader = new VariablesLoader();

	private bool NetworkInitialize___EarlyScheduleOne_002EVariables_002EVariableDatabaseAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EVariables_002EVariableDatabaseAssembly_002DCSharp_002Edll_Excuted;

	public string SaveFolderName => "Variables";

	public string SaveFileName => "Variables";

	public Loader Loader => loader;

	public bool ShouldSaveUnderFolder => false;

	public List<string> LocalExtraFiles { get; set; } = new List<string>();

	public List<string> LocalExtraFolders { get; set; } = new List<string>();

	public bool HasChanged { get; set; } = true;

	public int LoadOrder { get; }

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EVariables_002EVariableDatabase_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public virtual void InitializeSaveable()
	{
		Singleton<SaveManager>.Instance.RegisterSaveable(this);
	}

	private void CreateVariables()
	{
		for (int i = 0; i < Creators.Length; i++)
		{
			if (Creators[i].Mode == EVariableMode.Player)
			{
				playerVariables.Add(Creators[i].Name.ToLower());
			}
			else
			{
				CreateVariable(Creators[i].Name, Creators[i].Type, Creators[i].InitialValue, Creators[i].Persistent, EVariableMode.Global, null);
			}
		}
		SetVariableValue("IsDemo", false.ToString());
	}

	public void CreatePlayerVariables(Player owner)
	{
		for (int i = 0; i < Creators.Length; i++)
		{
			if (Creators[i].Mode == EVariableMode.Player)
			{
				CreateVariable(Creators[i].Name, Creators[i].Type, Creators[i].InitialValue, Creators[i].Persistent, EVariableMode.Player, owner, EVariableReplicationMode.Local);
			}
		}
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (connection.IsHost)
		{
			return;
		}
		for (int i = 0; i < VariableList.Count; i++)
		{
			if (VariableList[i].ReplicationMode != EVariableReplicationMode.Local)
			{
				VariableList[i].ReplicateValue(connection);
			}
		}
	}

	public void CreateVariable(string name, EVariableType type, string initialValue, bool persistent, EVariableMode mode, Player owner, EVariableReplicationMode replicationMode = EVariableReplicationMode.Networked)
	{
		switch (type)
		{
		case EVariableType.Bool:
			new BoolVariable(name, replicationMode, persistent, mode, owner, initialValue == "true");
			break;
		case EVariableType.Number:
		{
			float result;
			float value = (float.TryParse(initialValue, out result) ? result : 0f);
			new NumberVariable(name, replicationMode, persistent, mode, owner, value);
			break;
		}
		}
	}

	public void AddVariable(BaseVariable variable)
	{
		if (VariableDict.ContainsKey(variable.Name))
		{
			Console.LogError("Variable with name " + variable.Name + " already exists in the database.");
			return;
		}
		VariableList.Add(variable);
		VariableDict.Add(variable.Name.ToLower(), variable);
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SendValue(NetworkConnection conn, string variableName, string value)
	{
		RpcWriter___Server_SendValue_3895153758(conn, variableName, value);
		RpcLogic___SendValue_3895153758(conn, variableName, value);
	}

	[ObserversRpc]
	[TargetRpc]
	public void ReceiveValue(NetworkConnection conn, string variableName, string value)
	{
		if (conn == null)
		{
			RpcWriter___Observers_ReceiveValue_3895153758(conn, variableName, value);
		}
		else
		{
			RpcWriter___Target_ReceiveValue_3895153758(conn, variableName, value);
		}
	}

	public void SetVariableValue(string variableName, string value, bool network = true)
	{
		variableName = variableName.ToLower();
		if (playerVariables.Contains(variableName))
		{
			if ((Object)(object)Player.Local == (Object)null)
			{
				Console.LogWarning("Player.Local is null when trying to set player variable: " + variableName);
			}
			else
			{
				Player.Local.SetVariableValue(variableName, value, network);
			}
		}
		else if (VariableDict.ContainsKey(variableName))
		{
			VariableDict[variableName].SetValue(value, network);
		}
		else
		{
			Console.LogWarning("Failed to find variable with name: " + variableName);
		}
	}

	public BaseVariable GetVariable(string variableName)
	{
		variableName = variableName.ToLower();
		if (playerVariables.Contains(variableName))
		{
			return Player.Local.GetVariable(variableName);
		}
		if (VariableDict.ContainsKey(variableName))
		{
			return VariableDict[variableName];
		}
		Console.LogWarning("Failed to find variable with name: " + variableName);
		return null;
	}

	public T GetValue<T>(string variableName)
	{
		variableName = variableName.ToLower();
		if (playerVariables.Contains(variableName))
		{
			if ((Object)(object)Player.Local == (Object)null)
			{
				Console.LogWarning("Player.Local is null when trying to get player variable: " + variableName);
				return default(T);
			}
			return Player.Local.GetValue<T>(variableName);
		}
		if (VariableDict.ContainsKey(variableName))
		{
			return (T)VariableDict[variableName].GetValue();
		}
		Console.LogError("Variable with name " + variableName + " does not exist in the database.");
		return default(T);
	}

	[Button]
	public void PrintAllVariables()
	{
		for (int i = 0; i < VariableList.Count; i++)
		{
			PrintVariableValue(VariableList[i].Name);
		}
	}

	public void PrintVariableValue(string variableName)
	{
		variableName = variableName.ToLower();
		if (VariableDict.ContainsKey(variableName))
		{
			Console.Log("Value of " + variableName + ": " + VariableDict[variableName].GetValue());
		}
		else
		{
			Console.LogError("Variable with name " + variableName + " does not exist in the database.");
		}
	}

	public void NotifyItemAcquired(string id, int quantity)
	{
		if (VariableDict.ContainsKey(id + "_acquired"))
		{
			float value = GetValue<float>(id + "_acquired");
			SetVariableValue(id + "_acquired", (value + (float)quantity).ToString());
		}
	}

	public virtual string GetSaveString()
	{
		List<VariableData> list = new List<VariableData>();
		for (int i = 0; i < VariableList.Count; i++)
		{
			if (VariableList[i] != null && VariableList[i].Persistent && VariableList[i].VariableMode != EVariableMode.Player)
			{
				list.Add(new VariableData(VariableList[i].Name, VariableList[i].GetValue().ToString()));
			}
		}
		return new VariableCollectionData(list.ToArray()).GetJson();
	}

	public void LoadVariable(VariableData data)
	{
		if (playerVariables.Contains(data.Name.ToLower()))
		{
			Console.Log("Player variable: " + data.Name + " loaded from database. Redirecting to player.");
			Player.Local.SetVariableValue(data.Name, data.Value, network: false);
			return;
		}
		BaseVariable variable = GetVariable(data.Name);
		if (variable == null)
		{
			Console.LogWarning("Failed to find variable with name: " + data.Name);
		}
		else
		{
			variable.SetValue(data.Value);
		}
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EVariables_002EVariableDatabaseAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EVariables_002EVariableDatabaseAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_SendValue_3895153758));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_ReceiveValue_3895153758));
			((NetworkBehaviour)this).RegisterTargetRpc(2u, new ClientRpcDelegate(RpcReader___Target_ReceiveValue_3895153758));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EVariables_002EVariableDatabaseAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EVariables_002EVariableDatabaseAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SendValue_3895153758(NetworkConnection conn, string variableName, string value)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteNetworkConnection(conn);
			((Writer)writer).WriteString(variableName);
			((Writer)writer).WriteString(value);
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendValue_3895153758(NetworkConnection conn, string variableName, string value)
	{
		ReceiveValue(conn, variableName, value);
	}

	private void RpcReader___Server_SendValue_3895153758(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkConnection conn2 = ((Reader)PooledReader0).ReadNetworkConnection();
		string variableName = ((Reader)PooledReader0).ReadString();
		string value = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendValue_3895153758(conn2, variableName, value);
		}
	}

	private void RpcWriter___Observers_ReceiveValue_3895153758(NetworkConnection conn, string variableName, string value)
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
			((Writer)writer).WriteString(variableName);
			((Writer)writer).WriteString(value);
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___ReceiveValue_3895153758(NetworkConnection conn, string variableName, string value)
	{
		variableName = variableName.ToLower();
		if (VariableDict.ContainsKey(variableName))
		{
			VariableDict[variableName].SetValue(value, replicate: false);
		}
	}

	private void RpcReader___Observers_ReceiveValue_3895153758(PooledReader PooledReader0, Channel channel)
	{
		string variableName = ((Reader)PooledReader0).ReadString();
		string value = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ReceiveValue_3895153758(null, variableName, value);
		}
	}

	private void RpcWriter___Target_ReceiveValue_3895153758(NetworkConnection conn, string variableName, string value)
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
			((Writer)writer).WriteString(variableName);
			((Writer)writer).WriteString(value);
			((NetworkBehaviour)this).SendTargetRpc(2u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_ReceiveValue_3895153758(PooledReader PooledReader0, Channel channel)
	{
		string variableName = ((Reader)PooledReader0).ReadString();
		string value = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ReceiveValue_3895153758(((NetworkBehaviour)this).LocalConnection, variableName, value);
		}
	}

	protected override void Awake_UserLogic_ScheduleOne_002EVariables_002EVariableDatabase_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		List<VariableCreator> list = new List<VariableCreator>(Creators);
		for (int i = 0; i < ItemsToTrackAcquire.Length; i++)
		{
			VariableCreator variableCreator = new VariableCreator();
			variableCreator.InitialValue = "0";
			variableCreator.Mode = EVariableMode.Global;
			variableCreator.Type = EVariableType.Number;
			variableCreator.Persistent = true;
			variableCreator.Name = ((BaseItemDefinition)ItemsToTrackAcquire[i]).ID + "_acquired";
			list.Add(variableCreator);
		}
		Creators = list.ToArray();
		CreateVariables();
		InitializeSaveable();
	}
}
