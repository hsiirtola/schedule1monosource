using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Delivery;
using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.EntityFramework;
using ScheduleOne.Interaction;
using ScheduleOne.Management;
using ScheduleOne.Map;
using ScheduleOne.Misc;
using ScheduleOne.Money;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Tiles;
using ScheduleOne.UI.Management;
using ScheduleOne.Variables;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Property;

public class Property : NetworkBehaviour, ISaveable
{
	public delegate void PropertyChange(Property property);

	public static List<Property> Properties = new List<Property>();

	public static List<Property> UnownedProperties = new List<Property>();

	public static List<Property> OwnedProperties = new List<Property>();

	public static PropertyChange onPropertyAcquired;

	public UnityEvent onThisPropertyAcquired;

	[Header("Settings")]
	[SerializeField]
	protected string propertyName = "Property Name";

	public bool AvailableInDemo = true;

	[SerializeField]
	protected string propertyCode = "propertycode";

	public float Price = 1f;

	public float DefaultRotation;

	public int EmployeeCapacity = 10;

	public bool OwnedByDefault;

	public string IsOwnedVariable = string.Empty;

	[Header("Culling Settings")]
	public bool ContentCullingEnabled = true;

	public float MinimumCullingDistance = 50f;

	public GameObject[] ObjectsToCull;

	[Header("References")]
	public Transform EmployeeContainer;

	public Transform SpawnPoint;

	public Transform InteriorSpawnPoint;

	public GameObject ForSaleSign;

	public GameObject BoundingBox;

	public POI PoI;

	public Transform ListingPoster;

	public Transform NPCSpawnPoint;

	public Transform[] EmployeeIdlePoints;

	public List<ModularSwitch> Switches;

	public List<InteractableToggleable> Toggleables;

	public PropertyDisposalArea DisposalArea;

	public LoadingDock[] LoadingDocks;

	[HideInInspector]
	protected List<BuildableItem> BuildableItems = new List<BuildableItem>();

	public List<IConfigurable> Configurables = new List<IConfigurable>();

	public readonly List<Grid> Grids = new List<Grid>();

	protected BoxCollider[] propertyBoundsColliders;

	private PropertyLoader loader = new PropertyLoader();

	private List<string> savedObjectPaths = new List<string>();

	private List<string> savedEmployeePaths = new List<string>();

	private bool NetworkInitialize___EarlyScheduleOne_002EProperty_002EPropertyAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EProperty_002EPropertyAssembly_002DCSharp_002Edll_Excuted;

	public bool IsOwned { get; protected set; }

	public List<Employee> Employees { get; protected set; } = new List<Employee>();

	public RectTransform WorldspaceUIContainer { get; protected set; }

	public bool IsContentCulled { get; set; }

	public string PropertyName => propertyName;

	public string PropertyCode => propertyCode;

	[field: SerializeField]
	public float AmbientTemperature { get; private set; } = 20f;

	public int LoadingDockCount => LoadingDocks.Length;

	public PropertyContentsContainer Container { get; private set; }

	public string SaveFolderName => propertyName;

	public string SaveFileName => SaveManager.MakeFileSafe(propertyName);

	public Loader Loader => loader;

	public bool ShouldSaveUnderFolder => false;

	public List<string> LocalExtraFiles { get; set; } = new List<string>();

	public List<string> LocalExtraFolders { get; set; } = new List<string>();

	public bool HasChanged { get; set; } = true;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EProperty_002EProperty_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public virtual void InitializeSaveable()
	{
		Singleton<SaveManager>.Instance.RegisterSaveable(this);
	}

	protected virtual void Start()
	{
		MoneyManager instance = NetworkSingleton<MoneyManager>.Instance;
		instance.onNetworthCalculation = (Action<MoneyManager.FloatContainer>)Delegate.Combine(instance.onNetworthCalculation, new Action<MoneyManager.FloatContainer>(GetNetworth));
	}

	protected virtual void FixedUpdate()
	{
		UpdateCulling();
	}

	public void AddConfigurable(IConfigurable configurable)
	{
		if (!Configurables.Contains(configurable))
		{
			Configurables.Add(configurable);
		}
	}

	public void RemoveConfigurable(IConfigurable configurable)
	{
		if (Configurables.Contains(configurable))
		{
			Configurables.Remove(configurable);
		}
	}

	private void UpdateCulling()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (Singleton<LoadManager>.InstanceExists && !Singleton<LoadManager>.Instance.IsLoading && PlayerSingleton<PlayerCamera>.InstanceExists)
		{
			if (!ContentCullingEnabled)
			{
				SetContentCulled(culled: false);
			}
			float num = Vector3.Distance(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position, ((Component)this).transform.position);
			if (num < MinimumCullingDistance)
			{
				SetContentCulled(culled: false);
			}
			else if (num > MinimumCullingDistance + 5f)
			{
				SetContentCulled(culled: true);
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
		for (int i = 0; i < Toggleables.Count; i++)
		{
			if (Toggleables[i].IsActivated)
			{
				SetToggleableState(connection, i, Toggleables[i].IsActivated);
			}
		}
	}

	protected virtual void OnDestroy()
	{
		if (NetworkSingleton<MoneyManager>.InstanceExists)
		{
			MoneyManager instance = NetworkSingleton<MoneyManager>.Instance;
			instance.onNetworthCalculation = (Action<MoneyManager.FloatContainer>)Delegate.Remove(instance.onNetworthCalculation, new Action<MoneyManager.FloatContainer>(GetNetworth));
		}
		Properties.Remove(this);
		UnownedProperties.Remove(this);
		OwnedProperties.Remove(this);
	}

	protected virtual void GetNetworth(MoneyManager.FloatContainer container)
	{
		if (IsOwned)
		{
			container.ChangeValue(Price);
		}
	}

	public override void OnStartServer()
	{
		((NetworkBehaviour)this).OnStartServer();
		if (OwnedByDefault)
		{
			SetOwned_Server();
		}
		if (((NetworkBehaviour)this).NetworkObject.GetInitializeOrder() == 0)
		{
			Console.LogError("Property " + PropertyName + " has an initialize order of 0. This will cause issues.");
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	protected void SetOwned_Server()
	{
		RpcWriter___Server_SetOwned_Server_2166136261();
		RpcLogic___SetOwned_Server_2166136261();
	}

	[ObserversRpc(RunLocally = true, BufferLast = true)]
	private void ReceiveOwned_Networked()
	{
		RpcWriter___Observers_ReceiveOwned_Networked_2166136261();
		RpcLogic___ReceiveOwned_Networked_2166136261();
	}

	protected virtual void RecieveOwned()
	{
		if (!IsOwned)
		{
			IsOwned = true;
			HasChanged = true;
			if (IsOwnedVariable != string.Empty && NetworkSingleton<VariableDatabase>.InstanceExists && InstanceFinder.IsServer)
			{
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue(IsOwnedVariable, "true");
			}
			if (UnownedProperties.Contains(this))
			{
				UnownedProperties.Remove(this);
				OwnedProperties.Add(this);
			}
			if (onPropertyAcquired != null)
			{
				onPropertyAcquired(this);
			}
			if (onThisPropertyAcquired != null)
			{
				onThisPropertyAcquired.Invoke();
			}
			if ((Object)(object)ForSaleSign != (Object)null)
			{
				ForSaleSign.gameObject.SetActive(false);
			}
			if ((Object)(object)ListingPoster != (Object)null)
			{
				((Component)ListingPoster).gameObject.SetActive(false);
			}
			if ((Object)(object)PoI != (Object)null)
			{
				((Component)PoI).gameObject.SetActive(true);
				PoI.SetMainText(propertyName + " (Owned)");
				((MonoBehaviour)this).StartCoroutine(Wait());
			}
		}
		IEnumerator Wait()
		{
			yield return (object)new WaitUntil((Func<bool>)(() => PoI.UISetup));
			((Component)((Transform)PoI.IconContainer).Find("Unowned")).gameObject.SetActive(false);
			((Component)((Transform)PoI.IconContainer).Find("Owned")).gameObject.SetActive(true);
		}
	}

	public virtual bool ShouldSave()
	{
		if (!IsOwned)
		{
			return ((Component)Container).transform.childCount > 0;
		}
		return true;
	}

	public void SetOwned()
	{
		SetOwned_Server();
	}

	public void SetBoundsVisible(bool vis)
	{
	}

	public virtual bool CanBePurchased()
	{
		return true;
	}

	public virtual void SetContentCulled(bool culled)
	{
		if (IsContentCulled == culled)
		{
			return;
		}
		IsContentCulled = culled;
		foreach (BuildableItem buildableItem in BuildableItems)
		{
			if (!((Object)(object)buildableItem == (Object)null))
			{
				buildableItem.SetCulled(culled);
			}
		}
		GameObject[] objectsToCull = ObjectsToCull;
		foreach (GameObject val in objectsToCull)
		{
			if (!((Object)(object)val == (Object)null))
			{
				val.SetActive(!culled);
			}
		}
	}

	public int RegisterEmployee(Employee emp)
	{
		Employees.Add(emp);
		return Employees.IndexOf(emp);
	}

	public void DeregisterEmployee(Employee emp)
	{
		Employees.Remove(emp);
	}

	private void ToggleableActioned(InteractableToggleable toggleable)
	{
		HasChanged = true;
		SendToggleableState(Toggleables.IndexOf(toggleable), toggleable.IsActivated);
	}

	[ServerRpc(RequireOwnership = false)]
	public void SendToggleableState(int index, bool state)
	{
		RpcWriter___Server_SendToggleableState_3658436649(index, state);
	}

	[ObserversRpc]
	[TargetRpc]
	public void SetToggleableState(NetworkConnection conn, int index, bool state)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetToggleableState_338960014(conn, index, state);
		}
		else
		{
			RpcWriter___Target_SetToggleableState_338960014(conn, index, state);
		}
	}

	public void AddBuildableItem(BuildableItem item)
	{
		if (BuildableItems.Contains(item))
		{
			Debug.LogWarning((object)("Trying to add buildable item that is already registered on " + propertyName));
		}
		else
		{
			BuildableItems.Add(item);
		}
	}

	public void RemoveBuildableItem(BuildableItem item)
	{
		if (BuildableItems.Contains(item))
		{
			BuildableItems.Remove(item);
		}
	}

	public virtual string GetSaveString()
	{
		bool[] array = new bool[Switches.Count];
		for (int i = 0; i < Switches.Count; i++)
		{
			if (!((Object)(object)Switches[i] == (Object)null))
			{
				array[i] = Switches[i].isOn;
			}
		}
		bool[] array2 = new bool[Toggleables.Count];
		for (int j = 0; j < Toggleables.Count; j++)
		{
			if (!((Object)(object)Toggleables[j] == (Object)null))
			{
				array2[j] = Toggleables[j].IsActivated;
			}
		}
		return new PropertyData(propertyCode, IsOwned, array, array2, GetEmployeeSaveDatas().ToArray(), GetObjectSaveDatas().ToArray()).GetJson();
	}

	protected List<DynamicSaveData> GetEmployeeSaveDatas()
	{
		List<DynamicSaveData> list = new List<DynamicSaveData>();
		for (int i = 0; i < Employees.Count; i++)
		{
			if (!((Object)(object)Employees[i] == (Object)null))
			{
				list.Add(Employees[i].GetSaveData());
			}
		}
		return list;
	}

	protected List<DynamicSaveData> GetObjectSaveDatas()
	{
		List<DynamicSaveData> list = new List<DynamicSaveData>();
		for (int i = 0; i < BuildableItems.Count; i++)
		{
			if (!((Object)(object)BuildableItems[i] == (Object)null))
			{
				list.Add(BuildableItems[i].GetSaveData());
			}
		}
		return list;
	}

	public virtual List<string> WriteData(string parentFolderPath)
	{
		List<string> result = new List<string>();
		savedObjectPaths.Clear();
		savedEmployeePaths.Clear();
		return result;
	}

	public virtual void DeleteUnapprovedFiles(string parentFolderPath)
	{
	}

	public virtual void Load(PropertyData propertyData, string dataString)
	{
		if (propertyData.IsOwned)
		{
			SetOwned();
		}
		if (propertyData.SwitchStates != null)
		{
			for (int i = 0; i < propertyData.SwitchStates.Length && i < Switches.Count; i++)
			{
				if (propertyData.SwitchStates[i])
				{
					Switches[i].SwitchOn();
				}
			}
		}
		if (propertyData.ToggleableStates == null)
		{
			return;
		}
		for (int j = 0; j < propertyData.ToggleableStates.Length && j < Toggleables.Count; j++)
		{
			if (propertyData.ToggleableStates[j])
			{
				Toggleables[j].Toggle();
			}
		}
	}

	public bool DoBoundsContainPoint(Vector3 point)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		BoxCollider[] array = propertyBoundsColliders;
		foreach (BoxCollider val in array)
		{
			if (!((Object)(object)val == (Object)null) && IsPointInsideBox(point, val))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsPointInsideBox(Vector3 worldPoint, BoxCollider box)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)box == (Object)null)
		{
			Console.LogWarning("BoxCollider is null.");
			return false;
		}
		Vector3 val = ((Component)box).transform.InverseTransformPoint(worldPoint);
		val -= box.center;
		Vector3 val2 = box.size * 0.5f;
		if (Mathf.Abs(val.x) <= val2.x && Mathf.Abs(val.y) <= val2.y)
		{
			return Mathf.Abs(val.z) <= val2.z;
		}
		return false;
	}

	public List<Bed> GetUnassignedBeds()
	{
		return (from x in ((Component)Container).GetComponentsInChildren<Bed>()
			where (Object)(object)x.AssignedEmployee == (Object)null
			select x).ToList();
	}

	public List<T> GetBuildablesOfType<T>() where T : BuildableItem
	{
		List<T> list = new List<T>();
		foreach (BuildableItem buildableItem in BuildableItems)
		{
			if (!((Object)(object)buildableItem == (Object)null) && buildableItem is T)
			{
				list.Add((T)buildableItem);
			}
		}
		return list;
	}

	public virtual bool CanDeliverToProperty()
	{
		return true;
	}

	public virtual bool CanRespawnInsideProperty()
	{
		return true;
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
		if (!NetworkInitialize___EarlyScheduleOne_002EProperty_002EPropertyAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EProperty_002EPropertyAssembly_002DCSharp_002Edll_Excuted = true;
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_SetOwned_Server_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_ReceiveOwned_Networked_2166136261));
			((NetworkBehaviour)this).RegisterServerRpc(2u, new ServerRpcDelegate(RpcReader___Server_SendToggleableState_3658436649));
			((NetworkBehaviour)this).RegisterObserversRpc(3u, new ClientRpcDelegate(RpcReader___Observers_SetToggleableState_338960014));
			((NetworkBehaviour)this).RegisterTargetRpc(4u, new ClientRpcDelegate(RpcReader___Target_SetToggleableState_338960014));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EProperty_002EPropertyAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EProperty_002EPropertyAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SetOwned_Server_2166136261()
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
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	protected void RpcLogic___SetOwned_Server_2166136261()
	{
		ReceiveOwned_Networked();
	}

	private void RpcReader___Server_SetOwned_Server_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetOwned_Server_2166136261();
		}
	}

	private void RpcWriter___Observers_ReceiveOwned_Networked_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, true, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ReceiveOwned_Networked_2166136261()
	{
		RecieveOwned();
	}

	private void RpcReader___Observers_ReceiveOwned_Networked_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ReceiveOwned_Networked_2166136261();
		}
	}

	private void RpcWriter___Server_SendToggleableState_3658436649(int index, bool state)
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
			((Writer)writer).WriteInt32(index, (AutoPackType)1);
			((Writer)writer).WriteBoolean(state);
			((NetworkBehaviour)this).SendServerRpc(2u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendToggleableState_3658436649(int index, bool state)
	{
		SetToggleableState(null, index, state);
	}

	private void RpcReader___Server_SendToggleableState_3658436649(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int index = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		bool state = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SendToggleableState_3658436649(index, state);
		}
	}

	private void RpcWriter___Observers_SetToggleableState_338960014(NetworkConnection conn, int index, bool state)
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
			((Writer)writer).WriteInt32(index, (AutoPackType)1);
			((Writer)writer).WriteBoolean(state);
			((NetworkBehaviour)this).SendObserversRpc(3u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___SetToggleableState_338960014(NetworkConnection conn, int index, bool state)
	{
		Toggleables[index].SetState(state);
	}

	private void RpcReader___Observers_SetToggleableState_338960014(PooledReader PooledReader0, Channel channel)
	{
		int index = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		bool state = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetToggleableState_338960014(null, index, state);
		}
	}

	private void RpcWriter___Target_SetToggleableState_338960014(NetworkConnection conn, int index, bool state)
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
			((Writer)writer).WriteInt32(index, (AutoPackType)1);
			((Writer)writer).WriteBoolean(state);
			((NetworkBehaviour)this).SendTargetRpc(4u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetToggleableState_338960014(PooledReader PooledReader0, Channel channel)
	{
		int index = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		bool state = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetToggleableState_338960014(((NetworkBehaviour)this).LocalConnection, index, state);
		}
	}

	protected virtual void Awake_UserLogic_ScheduleOne_002EProperty_002EProperty_Assembly_002DCSharp_002Edll()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Expected O, but got Unknown
		propertyBoundsColliders = BoundingBox.GetComponentsInChildren<BoxCollider>();
		BoxCollider[] array = propertyBoundsColliders;
		foreach (BoxCollider obj in array)
		{
			((Collider)obj).isTrigger = true;
			((Component)obj).gameObject.layer = LayerMask.NameToLayer("Invisible");
		}
		GameObject val = new GameObject(propertyName + " Contents Container");
		Container = val.AddComponent<PropertyContentsContainer>();
		Container.SetProperty(this);
		Properties.Add(this);
		UnownedProperties.Remove(this);
		UnownedProperties.Add(this);
		PoI.SetMainText(propertyName + " (Unowned)");
		SetBoundsVisible(vis: false);
		((TMP_Text)((Component)ForSaleSign.transform.Find("Name")).GetComponent<TextMeshPro>()).text = propertyName;
		((TMP_Text)((Component)ForSaleSign.transform.Find("Price")).GetComponent<TextMeshPro>()).text = MoneyManager.FormatAmount(Price);
		if (EmployeeIdlePoints.Length < EmployeeCapacity)
		{
			Debug.LogWarning((object)("Property " + PropertyName + " has less idle points than employee capacity."));
		}
		if (!GameManager.IS_TUTORIAL)
		{
			WorldspaceUIContainer = new GameObject(propertyName + " Worldspace UI Container").AddComponent<RectTransform>();
			((Transform)WorldspaceUIContainer).SetParent(((Component)Singleton<ManagementWorldspaceCanvas>.Instance.Canvas).transform);
			((Component)WorldspaceUIContainer).gameObject.SetActive(false);
		}
		if ((Object)(object)ListingPoster != (Object)null)
		{
			((TMP_Text)((Component)ListingPoster.Find("Title")).GetComponent<TextMeshPro>()).text = propertyName;
			((TMP_Text)((Component)ListingPoster.Find("Price")).GetComponent<TextMeshPro>()).text = MoneyManager.FormatAmount(Price);
			((TMP_Text)((Component)ListingPoster.Find("Parking/Text")).GetComponent<TextMeshPro>()).text = LoadingDockCount.ToString();
			((TMP_Text)((Component)ListingPoster.Find("Employee/Text")).GetComponent<TextMeshPro>()).text = EmployeeCapacity.ToString();
		}
		((Component)PoI).gameObject.SetActive(false);
		foreach (ModularSwitch @switch in Switches)
		{
			if (!((Object)(object)@switch == (Object)null))
			{
				@switch.onToggled = (ModularSwitch.ButtonChange)Delegate.Combine(@switch.onToggled, (ModularSwitch.ButtonChange)delegate
				{
					HasChanged = true;
				});
			}
		}
		foreach (InteractableToggleable toggleable2 in Toggleables)
		{
			if (!((Object)(object)toggleable2 == (Object)null))
			{
				InteractableToggleable toggleable1 = toggleable2;
				toggleable2.onToggle.AddListener((UnityAction)delegate
				{
					ToggleableActioned(toggleable1);
				});
			}
		}
		InitializeSaveable();
	}
}
