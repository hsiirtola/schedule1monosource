using System;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.NPCs;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.StationFramework;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Management;

public class ConfigurationReplicator : NetworkBehaviour
{
	public EntityConfiguration Configuration;

	private bool NetworkInitialize___EarlyScheduleOne_002EManagement_002EConfigurationReplicatorAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EManagement_002EConfigurationReplicatorAssembly_002DCSharp_002Edll_Excuted;

	public void ReplicateField(ConfigField field, NetworkConnection conn = null)
	{
		if (Configuration == null)
		{
			Console.LogError("ReplicateField called with null Configuration");
			return;
		}
		int num = Configuration.Fields.IndexOf(field);
		if (num == -1)
		{
			Console.LogError("Failed to find field in configuration");
			return;
		}
		try
		{
			if (field is ItemField)
			{
				ItemField itemField = (ItemField)field;
				SendItemField(num, ((Object)(object)itemField.SelectedItem != (Object)null) ? ((Object)itemField.SelectedItem).name : string.Empty);
			}
			else if (field is NPCField)
			{
				NPCField nPCField = (NPCField)field;
				SendNPCField(num, ((Object)(object)nPCField.SelectedNPC != (Object)null) ? ((NetworkBehaviour)nPCField.SelectedNPC).NetworkObject : null);
			}
			else if (field is ObjectField)
			{
				ObjectField objectField = (ObjectField)field;
				NetworkObject obj = null;
				if ((Object)(object)objectField.SelectedObject != (Object)null)
				{
					obj = ((NetworkBehaviour)objectField.SelectedObject).NetworkObject;
				}
				SendObjectField(num, obj);
			}
			else if (field is ObjectListField)
			{
				ObjectListField objectListField = (ObjectListField)field;
				List<NetworkObject> list = new List<NetworkObject>();
				for (int i = 0; i < objectListField.SelectedObjects.Count; i++)
				{
					list.Add(((NetworkBehaviour)objectListField.SelectedObjects[i]).NetworkObject);
				}
				SendObjectListField(num, list);
			}
			else if (field is StationRecipeField)
			{
				StationRecipeField stationRecipeField = (StationRecipeField)field;
				int recipeIndex = -1;
				if ((Object)(object)stationRecipeField.SelectedRecipe != (Object)null)
				{
					recipeIndex = stationRecipeField.Options.IndexOf(stationRecipeField.SelectedRecipe);
				}
				SendRecipeField(num, recipeIndex);
			}
			else if (field is NumberField)
			{
				NumberField numberField = (NumberField)field;
				SendNumberField(num, numberField.Value);
			}
			else if (field is RouteListField)
			{
				RouteListField routeListField = (RouteListField)field;
				SendRouteListField(num, routeListField.Routes.Select((AdvancedTransitRoute x) => x.GetData()).ToArray());
			}
			else if (field is QualityField)
			{
				QualityField qualityField = (QualityField)field;
				SendQualityField(num, qualityField.Value);
			}
			else if (field is StringField)
			{
				StringField stringField = (StringField)field;
				SendStringField(num, stringField.Value);
			}
			else
			{
				Console.LogError("Failed to find replication method for " + field.GetType());
			}
		}
		catch (Exception ex)
		{
			Console.LogError("Exception in ReplicateField: " + ex);
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void SendItemField(int fieldIndex, string value)
	{
		RpcWriter___Server_SendItemField_2801973956(fieldIndex, value);
	}

	[ObserversRpc]
	private void ReceiveItemField(int fieldIndex, string value)
	{
		RpcWriter___Observers_ReceiveItemField_2801973956(fieldIndex, value);
	}

	[ServerRpc(RequireOwnership = false)]
	private void SendNPCField(int fieldIndex, NetworkObject npcObject)
	{
		RpcWriter___Server_SendNPCField_1687693739(fieldIndex, npcObject);
	}

	[ObserversRpc]
	private void ReceiveNPCField(int fieldIndex, NetworkObject npcObject)
	{
		RpcWriter___Observers_ReceiveNPCField_1687693739(fieldIndex, npcObject);
	}

	[ServerRpc(RequireOwnership = false)]
	private void SendObjectField(int fieldIndex, NetworkObject obj)
	{
		RpcWriter___Server_SendObjectField_1687693739(fieldIndex, obj);
	}

	[ObserversRpc]
	private void ReceiveObjectField(int fieldIndex, NetworkObject obj)
	{
		RpcWriter___Observers_ReceiveObjectField_1687693739(fieldIndex, obj);
	}

	[ServerRpc(RequireOwnership = false)]
	private void SendObjectListField(int fieldIndex, List<NetworkObject> objects)
	{
		RpcWriter___Server_SendObjectListField_690244341(fieldIndex, objects);
	}

	[ObserversRpc]
	private void ReceiveObjectListField(int fieldIndex, List<NetworkObject> objects)
	{
		RpcWriter___Observers_ReceiveObjectListField_690244341(fieldIndex, objects);
	}

	[ServerRpc(RequireOwnership = false)]
	private void SendRecipeField(int fieldIndex, int recipeIndex)
	{
		RpcWriter___Server_SendRecipeField_1692629761(fieldIndex, recipeIndex);
	}

	[ObserversRpc]
	private void ReceiveRecipeField(int fieldIndex, int recipeIndex)
	{
		RpcWriter___Observers_ReceiveRecipeField_1692629761(fieldIndex, recipeIndex);
	}

	[ServerRpc(RequireOwnership = false)]
	private void SendNumberField(int fieldIndex, float value)
	{
		RpcWriter___Server_SendNumberField_1293284375(fieldIndex, value);
	}

	[ObserversRpc]
	private void ReceiveNumberField(int fieldIndex, float value)
	{
		RpcWriter___Observers_ReceiveNumberField_1293284375(fieldIndex, value);
	}

	[ServerRpc(RequireOwnership = false)]
	private void SendRouteListField(int fieldIndex, AdvancedTransitRouteData[] value)
	{
		RpcWriter___Server_SendRouteListField_3226448297(fieldIndex, value);
	}

	[ObserversRpc]
	private void ReceiveRouteListField(int fieldIndex, AdvancedTransitRouteData[] value)
	{
		RpcWriter___Observers_ReceiveRouteListField_3226448297(fieldIndex, value);
	}

	[ServerRpc(RequireOwnership = false)]
	private void SendQualityField(int fieldIndex, EQuality quality)
	{
		RpcWriter___Server_SendQualityField_3536682170(fieldIndex, quality);
	}

	[ObserversRpc]
	private void ReceiveQualityField(int fieldIndex, EQuality value)
	{
		RpcWriter___Observers_ReceiveQualityField_3536682170(fieldIndex, value);
	}

	[ServerRpc(RequireOwnership = false)]
	private void SendStringField(int fieldIndex, string value)
	{
		RpcWriter___Server_SendStringField_2801973956(fieldIndex, value);
	}

	[ObserversRpc]
	private void ReceiveStringField(int fieldIndex, string value)
	{
		RpcWriter___Observers_ReceiveStringField_2801973956(fieldIndex, value);
	}

	public override void NetworkInitialize___Early()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected O, but got Unknown
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Expected O, but got Unknown
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Expected O, but got Unknown
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Expected O, but got Unknown
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Expected O, but got Unknown
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Expected O, but got Unknown
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Expected O, but got Unknown
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Expected O, but got Unknown
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EManagement_002EConfigurationReplicatorAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EManagement_002EConfigurationReplicatorAssembly_002DCSharp_002Edll_Excuted = true;
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_SendItemField_2801973956));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_ReceiveItemField_2801973956));
			((NetworkBehaviour)this).RegisterServerRpc(2u, new ServerRpcDelegate(RpcReader___Server_SendNPCField_1687693739));
			((NetworkBehaviour)this).RegisterObserversRpc(3u, new ClientRpcDelegate(RpcReader___Observers_ReceiveNPCField_1687693739));
			((NetworkBehaviour)this).RegisterServerRpc(4u, new ServerRpcDelegate(RpcReader___Server_SendObjectField_1687693739));
			((NetworkBehaviour)this).RegisterObserversRpc(5u, new ClientRpcDelegate(RpcReader___Observers_ReceiveObjectField_1687693739));
			((NetworkBehaviour)this).RegisterServerRpc(6u, new ServerRpcDelegate(RpcReader___Server_SendObjectListField_690244341));
			((NetworkBehaviour)this).RegisterObserversRpc(7u, new ClientRpcDelegate(RpcReader___Observers_ReceiveObjectListField_690244341));
			((NetworkBehaviour)this).RegisterServerRpc(8u, new ServerRpcDelegate(RpcReader___Server_SendRecipeField_1692629761));
			((NetworkBehaviour)this).RegisterObserversRpc(9u, new ClientRpcDelegate(RpcReader___Observers_ReceiveRecipeField_1692629761));
			((NetworkBehaviour)this).RegisterServerRpc(10u, new ServerRpcDelegate(RpcReader___Server_SendNumberField_1293284375));
			((NetworkBehaviour)this).RegisterObserversRpc(11u, new ClientRpcDelegate(RpcReader___Observers_ReceiveNumberField_1293284375));
			((NetworkBehaviour)this).RegisterServerRpc(12u, new ServerRpcDelegate(RpcReader___Server_SendRouteListField_3226448297));
			((NetworkBehaviour)this).RegisterObserversRpc(13u, new ClientRpcDelegate(RpcReader___Observers_ReceiveRouteListField_3226448297));
			((NetworkBehaviour)this).RegisterServerRpc(14u, new ServerRpcDelegate(RpcReader___Server_SendQualityField_3536682170));
			((NetworkBehaviour)this).RegisterObserversRpc(15u, new ClientRpcDelegate(RpcReader___Observers_ReceiveQualityField_3536682170));
			((NetworkBehaviour)this).RegisterServerRpc(16u, new ServerRpcDelegate(RpcReader___Server_SendStringField_2801973956));
			((NetworkBehaviour)this).RegisterObserversRpc(17u, new ClientRpcDelegate(RpcReader___Observers_ReceiveStringField_2801973956));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EManagement_002EConfigurationReplicatorAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EManagement_002EConfigurationReplicatorAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SendItemField_2801973956(int fieldIndex, string value)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(fieldIndex, (AutoPackType)1);
			((Writer)writer).WriteString(value);
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SendItemField_2801973956(int fieldIndex, string value)
	{
		ReceiveItemField(fieldIndex, value);
	}

	private void RpcReader___Server_SendItemField_2801973956(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int fieldIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		string value = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SendItemField_2801973956(fieldIndex, value);
		}
	}

	private void RpcWriter___Observers_ReceiveItemField_2801973956(int fieldIndex, string value)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(fieldIndex, (AutoPackType)1);
			((Writer)writer).WriteString(value);
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ReceiveItemField_2801973956(int fieldIndex, string value)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		if (Singleton<LoadManager>.Instance.IsLoading)
		{
			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(Do));
		}
		else
		{
			Do();
		}
		void Do()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(new UnityAction(Do));
			ItemField obj = Configuration.Fields[fieldIndex] as ItemField;
			ItemDefinition item = null;
			if (value != string.Empty)
			{
				item = Registry.GetItem(value);
			}
			obj.SetItem(item, network: false);
		}
	}

	private void RpcReader___Observers_ReceiveItemField_2801973956(PooledReader PooledReader0, Channel channel)
	{
		int fieldIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		string value = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ReceiveItemField_2801973956(fieldIndex, value);
		}
	}

	private void RpcWriter___Server_SendNPCField_1687693739(int fieldIndex, NetworkObject npcObject)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(fieldIndex, (AutoPackType)1);
			((Writer)writer).WriteNetworkObject(npcObject);
			((NetworkBehaviour)this).SendServerRpc(2u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SendNPCField_1687693739(int fieldIndex, NetworkObject npcObject)
	{
		ReceiveNPCField(fieldIndex, npcObject);
	}

	private void RpcReader___Server_SendNPCField_1687693739(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int fieldIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		NetworkObject npcObject = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SendNPCField_1687693739(fieldIndex, npcObject);
		}
	}

	private void RpcWriter___Observers_ReceiveNPCField_1687693739(int fieldIndex, NetworkObject npcObject)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(fieldIndex, (AutoPackType)1);
			((Writer)writer).WriteNetworkObject(npcObject);
			((NetworkBehaviour)this).SendObserversRpc(3u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ReceiveNPCField_1687693739(int fieldIndex, NetworkObject npcObject)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		if (Singleton<LoadManager>.Instance.IsLoading)
		{
			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(Do));
		}
		else
		{
			Do();
		}
		void Do()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(new UnityAction(Do));
			NPCField obj = Configuration.Fields[fieldIndex] as NPCField;
			NPC npc = null;
			if ((Object)(object)npcObject != (Object)null)
			{
				npc = ((Component)npcObject).GetComponent<NPC>();
			}
			obj.SetNPC(npc, network: false);
		}
	}

	private void RpcReader___Observers_ReceiveNPCField_1687693739(PooledReader PooledReader0, Channel channel)
	{
		int fieldIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		NetworkObject npcObject = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ReceiveNPCField_1687693739(fieldIndex, npcObject);
		}
	}

	private void RpcWriter___Server_SendObjectField_1687693739(int fieldIndex, NetworkObject obj)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(fieldIndex, (AutoPackType)1);
			((Writer)writer).WriteNetworkObject(obj);
			((NetworkBehaviour)this).SendServerRpc(4u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SendObjectField_1687693739(int fieldIndex, NetworkObject obj)
	{
		ReceiveObjectField(fieldIndex, obj);
	}

	private void RpcReader___Server_SendObjectField_1687693739(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int fieldIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		NetworkObject obj = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SendObjectField_1687693739(fieldIndex, obj);
		}
	}

	private void RpcWriter___Observers_ReceiveObjectField_1687693739(int fieldIndex, NetworkObject obj)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(fieldIndex, (AutoPackType)1);
			((Writer)writer).WriteNetworkObject(obj);
			((NetworkBehaviour)this).SendObserversRpc(5u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ReceiveObjectField_1687693739(int fieldIndex, NetworkObject obj)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		if (Singleton<LoadManager>.Instance.IsLoading)
		{
			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(Do));
		}
		else
		{
			Do();
		}
		void Do()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(new UnityAction(Do));
			if (Configuration == null)
			{
				Console.LogError("ReceiveObjectField called with null Configuration. GameObject name: " + ((Object)((Component)this).gameObject).name);
			}
			else if (!(Configuration.Fields[fieldIndex] is ObjectField objectField))
			{
				Console.LogError("ReceiveObjectField called with non-ObjectField (index " + fieldIndex + ", configuration type " + Configuration.GetType()?.ToString() + ")");
			}
			else
			{
				BuildableItem obj2 = null;
				if ((Object)(object)obj != (Object)null)
				{
					obj2 = ((Component)obj).GetComponent<BuildableItem>();
				}
				objectField.SetObject(obj2, network: false);
			}
		}
	}

	private void RpcReader___Observers_ReceiveObjectField_1687693739(PooledReader PooledReader0, Channel channel)
	{
		int fieldIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		NetworkObject obj = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ReceiveObjectField_1687693739(fieldIndex, obj);
		}
	}

	private void RpcWriter___Server_SendObjectListField_690244341(int fieldIndex, List<NetworkObject> objects)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(fieldIndex, (AutoPackType)1);
			GeneratedWriters___Internal.Write___System_002ECollections_002EGeneric_002EList_00601_003CFishNet_002EObject_002ENetworkObject_003EFishNet_002ESerializing_002EGenerated((Writer)(object)writer, objects);
			((NetworkBehaviour)this).SendServerRpc(6u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SendObjectListField_690244341(int fieldIndex, List<NetworkObject> objects)
	{
		ReceiveObjectListField(fieldIndex, objects);
	}

	private void RpcReader___Server_SendObjectListField_690244341(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int fieldIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		List<NetworkObject> objects = GeneratedReaders___Internal.Read___System_002ECollections_002EGeneric_002EList_00601_003CFishNet_002EObject_002ENetworkObject_003EFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SendObjectListField_690244341(fieldIndex, objects);
		}
	}

	private void RpcWriter___Observers_ReceiveObjectListField_690244341(int fieldIndex, List<NetworkObject> objects)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(fieldIndex, (AutoPackType)1);
			GeneratedWriters___Internal.Write___System_002ECollections_002EGeneric_002EList_00601_003CFishNet_002EObject_002ENetworkObject_003EFishNet_002ESerializing_002EGenerated((Writer)(object)writer, objects);
			((NetworkBehaviour)this).SendObserversRpc(7u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ReceiveObjectListField_690244341(int fieldIndex, List<NetworkObject> objects)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		if (Singleton<LoadManager>.Instance.IsLoading)
		{
			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(Do));
		}
		else
		{
			Do();
		}
		void Do()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(new UnityAction(Do));
			ObjectListField objectListField = Configuration.Fields[fieldIndex] as ObjectListField;
			List<BuildableItem> list = new List<BuildableItem>();
			for (int i = 0; i < objects.Count; i++)
			{
				if (!((Object)(object)objects[i] == (Object)null))
				{
					list.Add(((Component)objects[i]).GetComponent<BuildableItem>());
				}
			}
			objectListField.SetList(list, network: false);
		}
	}

	private void RpcReader___Observers_ReceiveObjectListField_690244341(PooledReader PooledReader0, Channel channel)
	{
		int fieldIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		List<NetworkObject> objects = GeneratedReaders___Internal.Read___System_002ECollections_002EGeneric_002EList_00601_003CFishNet_002EObject_002ENetworkObject_003EFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ReceiveObjectListField_690244341(fieldIndex, objects);
		}
	}

	private void RpcWriter___Server_SendRecipeField_1692629761(int fieldIndex, int recipeIndex)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(fieldIndex, (AutoPackType)1);
			((Writer)writer).WriteInt32(recipeIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendServerRpc(8u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SendRecipeField_1692629761(int fieldIndex, int recipeIndex)
	{
		ReceiveRecipeField(fieldIndex, recipeIndex);
	}

	private void RpcReader___Server_SendRecipeField_1692629761(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int fieldIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		int recipeIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SendRecipeField_1692629761(fieldIndex, recipeIndex);
		}
	}

	private void RpcWriter___Observers_ReceiveRecipeField_1692629761(int fieldIndex, int recipeIndex)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(fieldIndex, (AutoPackType)1);
			((Writer)writer).WriteInt32(recipeIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(9u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ReceiveRecipeField_1692629761(int fieldIndex, int recipeIndex)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		if (Singleton<LoadManager>.Instance.IsLoading)
		{
			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(Do));
		}
		else
		{
			Do();
		}
		void Do()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(new UnityAction(Do));
			StationRecipeField stationRecipeField = Configuration.Fields[fieldIndex] as StationRecipeField;
			StationRecipe recipe = null;
			if (recipeIndex != -1)
			{
				recipe = stationRecipeField.Options[recipeIndex];
			}
			stationRecipeField.SetRecipe(recipe, network: false);
		}
	}

	private void RpcReader___Observers_ReceiveRecipeField_1692629761(PooledReader PooledReader0, Channel channel)
	{
		int fieldIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		int recipeIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ReceiveRecipeField_1692629761(fieldIndex, recipeIndex);
		}
	}

	private void RpcWriter___Server_SendNumberField_1293284375(int fieldIndex, float value)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(fieldIndex, (AutoPackType)1);
			((Writer)writer).WriteSingle(value, (AutoPackType)0);
			((NetworkBehaviour)this).SendServerRpc(10u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SendNumberField_1293284375(int fieldIndex, float value)
	{
		ReceiveNumberField(fieldIndex, value);
	}

	private void RpcReader___Server_SendNumberField_1293284375(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int fieldIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		float value = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SendNumberField_1293284375(fieldIndex, value);
		}
	}

	private void RpcWriter___Observers_ReceiveNumberField_1293284375(int fieldIndex, float value)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(fieldIndex, (AutoPackType)1);
			((Writer)writer).WriteSingle(value, (AutoPackType)0);
			((NetworkBehaviour)this).SendObserversRpc(11u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ReceiveNumberField_1293284375(int fieldIndex, float value)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		if (Singleton<LoadManager>.Instance.IsLoading)
		{
			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(Do));
		}
		else
		{
			Do();
		}
		void Do()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(new UnityAction(Do));
			(Configuration.Fields[fieldIndex] as NumberField).SetValue(value, network: false);
		}
	}

	private void RpcReader___Observers_ReceiveNumberField_1293284375(PooledReader PooledReader0, Channel channel)
	{
		int fieldIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		float value = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ReceiveNumberField_1293284375(fieldIndex, value);
		}
	}

	private void RpcWriter___Server_SendRouteListField_3226448297(int fieldIndex, AdvancedTransitRouteData[] value)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(fieldIndex, (AutoPackType)1);
			GeneratedWriters___Internal.Write___ScheduleOne_002EPersistence_002EDatas_002EAdvancedTransitRouteData_005B_005DFishNet_002ESerializing_002EGenerated((Writer)(object)writer, value);
			((NetworkBehaviour)this).SendServerRpc(12u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SendRouteListField_3226448297(int fieldIndex, AdvancedTransitRouteData[] value)
	{
		ReceiveRouteListField(fieldIndex, value);
	}

	private void RpcReader___Server_SendRouteListField_3226448297(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int fieldIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		AdvancedTransitRouteData[] value = GeneratedReaders___Internal.Read___ScheduleOne_002EPersistence_002EDatas_002EAdvancedTransitRouteData_005B_005DFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SendRouteListField_3226448297(fieldIndex, value);
		}
	}

	private void RpcWriter___Observers_ReceiveRouteListField_3226448297(int fieldIndex, AdvancedTransitRouteData[] value)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(fieldIndex, (AutoPackType)1);
			GeneratedWriters___Internal.Write___ScheduleOne_002EPersistence_002EDatas_002EAdvancedTransitRouteData_005B_005DFishNet_002ESerializing_002EGenerated((Writer)(object)writer, value);
			((NetworkBehaviour)this).SendObserversRpc(13u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ReceiveRouteListField_3226448297(int fieldIndex, AdvancedTransitRouteData[] value)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		if (Singleton<LoadManager>.Instance.IsLoading)
		{
			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(Do));
		}
		else
		{
			Do();
		}
		void Do()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(new UnityAction(Do));
			(Configuration.Fields[fieldIndex] as RouteListField).SetList(value.Select((AdvancedTransitRouteData x) => new AdvancedTransitRoute(x)).ToList(), network: false);
		}
	}

	private void RpcReader___Observers_ReceiveRouteListField_3226448297(PooledReader PooledReader0, Channel channel)
	{
		int fieldIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		AdvancedTransitRouteData[] value = GeneratedReaders___Internal.Read___ScheduleOne_002EPersistence_002EDatas_002EAdvancedTransitRouteData_005B_005DFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ReceiveRouteListField_3226448297(fieldIndex, value);
		}
	}

	private void RpcWriter___Server_SendQualityField_3536682170(int fieldIndex, EQuality quality)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(fieldIndex, (AutoPackType)1);
			GeneratedWriters___Internal.Write___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerated((Writer)(object)writer, quality);
			((NetworkBehaviour)this).SendServerRpc(14u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SendQualityField_3536682170(int fieldIndex, EQuality quality)
	{
		ReceiveQualityField(fieldIndex, quality);
	}

	private void RpcReader___Server_SendQualityField_3536682170(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int fieldIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		EQuality quality = GeneratedReaders___Internal.Read___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SendQualityField_3536682170(fieldIndex, quality);
		}
	}

	private void RpcWriter___Observers_ReceiveQualityField_3536682170(int fieldIndex, EQuality value)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(fieldIndex, (AutoPackType)1);
			GeneratedWriters___Internal.Write___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerated((Writer)(object)writer, value);
			((NetworkBehaviour)this).SendObserversRpc(15u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ReceiveQualityField_3536682170(int fieldIndex, EQuality value)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		if (Singleton<LoadManager>.Instance.IsLoading)
		{
			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(Do));
		}
		else
		{
			Do();
		}
		void Do()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(new UnityAction(Do));
			(Configuration.Fields[fieldIndex] as QualityField).SetValue(value, network: false);
		}
	}

	private void RpcReader___Observers_ReceiveQualityField_3536682170(PooledReader PooledReader0, Channel channel)
	{
		int fieldIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		EQuality value = GeneratedReaders___Internal.Read___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ReceiveQualityField_3536682170(fieldIndex, value);
		}
	}

	private void RpcWriter___Server_SendStringField_2801973956(int fieldIndex, string value)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(fieldIndex, (AutoPackType)1);
			((Writer)writer).WriteString(value);
			((NetworkBehaviour)this).SendServerRpc(16u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SendStringField_2801973956(int fieldIndex, string value)
	{
		ReceiveStringField(fieldIndex, value);
	}

	private void RpcReader___Server_SendStringField_2801973956(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int fieldIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		string value = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SendStringField_2801973956(fieldIndex, value);
		}
	}

	private void RpcWriter___Observers_ReceiveStringField_2801973956(int fieldIndex, string value)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(fieldIndex, (AutoPackType)1);
			((Writer)writer).WriteString(value);
			((NetworkBehaviour)this).SendObserversRpc(17u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ReceiveStringField_2801973956(int fieldIndex, string value)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		if (Singleton<LoadManager>.Instance.IsLoading)
		{
			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(Do));
		}
		else
		{
			Do();
		}
		void Do()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(new UnityAction(Do));
			(Configuration.Fields[fieldIndex] as StringField).SetValue(value, network: false);
		}
	}

	private void RpcReader___Observers_ReceiveStringField_2801973956(PooledReader PooledReader0, Channel channel)
	{
		int fieldIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		string value = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ReceiveStringField_2801973956(fieldIndex, value);
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}
}
