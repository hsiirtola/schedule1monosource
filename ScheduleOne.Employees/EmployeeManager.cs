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
using ScheduleOne.AvatarFramework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Property;
using ScheduleOne.Quests;
using ScheduleOne.Variables;
using ScheduleOne.VoiceOver;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ScheduleOne.Employees;

public class EmployeeManager : NetworkSingleton<EmployeeManager>
{
	[Serializable]
	public class EmployeeAppearance
	{
		public AvatarSettings Settings;

		public Sprite Mugshot;
	}

	public const float MALE_EMPLOYEE_CHANCE = 0.67f;

	public List<Employee> AllEmployees = new List<Employee>();

	public Quest_Employees[] EmployeeQuests;

	[Header("Prefabs")]
	public Botanist BotanistPrefab;

	public Packager PackagerPrefab;

	public Chemist ChemistPrefab;

	public Cleaner CleanerPrefab;

	[Header("Appearances")]
	public List<EmployeeAppearance> MaleAppearances;

	public List<EmployeeAppearance> FemaleAppearances;

	[Header("Voices")]
	public VODatabase[] MaleVoices;

	public VODatabase[] FemaleVoices;

	[Header("Names")]
	public string[] MaleFirstNames;

	public string[] FemaleFirstNames;

	public string[] LastNames;

	private List<string> takenNames = new List<string>();

	private List<int> takenMaleAppearances = new List<int>();

	private List<int> takenFemaleAppearances = new List<int>();

	private bool NetworkInitialize___EarlyScheduleOne_002EEmployees_002EEmployeeManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EEmployees_002EEmployeeManagerAssembly_002DCSharp_002Edll_Excuted;

	public void CreateNewEmployee(ScheduleOne.Property.Property property, EEmployeeType type)
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		bool male = 0.67f > Random.Range(0f, 1f);
		NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("LifetimeEmployeesRecruited", (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("LifetimeEmployeesRecruited") + 1f).ToString());
		GenerateRandomName(male, out var firstName, out var lastName);
		string id = firstName.ToLower() + "_" + lastName.ToLower();
		GetRandomAppearance(male, out var index, out var _);
		string guid = GUIDManager.GenerateUniqueGUID().ToString();
		CreateEmployee(property, type, firstName, lastName, id, male, index, property.NPCSpawnPoint.position, property.NPCSpawnPoint.rotation, guid);
	}

	[ServerRpc(RequireOwnership = false)]
	public void CreateEmployee(ScheduleOne.Property.Property property, EEmployeeType type, string firstName, string lastName, string id, bool male, int appearanceIndex, Vector3 position, Quaternion rotation, string guid = "")
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		RpcWriter___Server_CreateEmployee_311954683(property, type, firstName, lastName, id, male, appearanceIndex, position, rotation, guid);
	}

	public Employee CreateEmployee_Server(ScheduleOne.Property.Property property, EEmployeeType type, string firstName, string lastName, string id, bool male, int appearanceIndex, Vector3 position, Quaternion rotation, string guid)
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		if (property.Employees.Count >= property.EmployeeCapacity)
		{
			Console.LogError("Property " + property.PropertyCode + " is at capacity.");
			return null;
		}
		Employee employeePrefab = GetEmployeePrefab(type);
		if ((Object)(object)employeePrefab == (Object)null)
		{
			Console.LogError("Failed to find employee prefab for " + type);
			return null;
		}
		guid = ((guid == "") ? Guid.NewGuid().ToString() : guid);
		if (!IsPositionValid(position))
		{
			position = property.NPCSpawnPoint.position;
		}
		if (!IsRotationValid(rotation))
		{
			rotation = property.NPCSpawnPoint.rotation;
		}
		Employee component = ((Component)Object.Instantiate<Employee>(employeePrefab, position, rotation)).GetComponent<Employee>();
		component.Initialize(null, firstName, lastName, id, guid, property.PropertyCode, male, appearanceIndex);
		((NetworkBehaviour)this).NetworkObject.Spawn(((Component)component).gameObject, (NetworkConnection)null, default(Scene));
		component.Movement.Warp(position);
		((Component)component.Movement).transform.rotation = rotation;
		Quest quest = EmployeeQuests.FirstOrDefault((Quest_Employees x) => x.EmployeeType == type);
		if ((Object)(object)quest != (Object)null && quest.State == EQuestState.Inactive)
		{
			quest.Begin();
		}
		return component;
	}

	private bool IsPositionValid(Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (IsFloatValid(position.x) && IsFloatValid(position.y))
		{
			return IsFloatValid(position.z);
		}
		return false;
	}

	private bool IsRotationValid(Quaternion rotation)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (IsFloatValid(rotation.x) && IsFloatValid(rotation.y) && IsFloatValid(rotation.z))
		{
			return IsFloatValid(rotation.w);
		}
		return false;
	}

	private bool IsFloatValid(float value)
	{
		if (!float.IsNaN(value))
		{
			return !float.IsInfinity(value);
		}
		return false;
	}

	public void RegisterName(string name)
	{
		takenNames.Add(name);
	}

	public void RegisterAppearance(bool male, int index)
	{
		if (male)
		{
			takenMaleAppearances.Add(index);
		}
		else
		{
			takenFemaleAppearances.Add(index);
		}
	}

	public void GenerateRandomName(bool male, out string firstName, out string lastName)
	{
		do
		{
			if (male)
			{
				firstName = MaleFirstNames[Random.Range(0, MaleFirstNames.Length)].ToString();
			}
			else
			{
				firstName = FemaleFirstNames[Random.Range(0, FemaleFirstNames.Length)].ToString();
			}
			lastName = LastNames[Random.Range(0, LastNames.Length)].ToString();
		}
		while (takenNames.Contains(firstName + " " + lastName));
	}

	public EmployeeAppearance GetAppearance(bool male, int index)
	{
		if (!male)
		{
			return FemaleAppearances[index];
		}
		return MaleAppearances[index];
	}

	public VODatabase GetVoice(bool male, int index)
	{
		if (!male)
		{
			return FemaleVoices[index % FemaleVoices.Length];
		}
		return MaleVoices[index % MaleVoices.Length];
	}

	public void GetRandomAppearance(bool male, out int index, out AvatarSettings settings)
	{
		List<EmployeeAppearance> list = (male ? MaleAppearances : FemaleAppearances);
		List<int> list2 = (male ? takenMaleAppearances : takenFemaleAppearances);
		index = Random.Range(0, list.Count);
		settings = list[index].Settings;
		if (list2.Count >= list.Count)
		{
			return;
		}
		int num = 0;
		while (list2.Contains(index))
		{
			index++;
			if (index >= list.Count)
			{
				index = 0;
			}
			num++;
			if (num >= list.Count)
			{
				settings = list[index].Settings;
				return;
			}
		}
		settings = list[index].Settings;
	}

	public Employee GetEmployeePrefab(EEmployeeType type)
	{
		switch (type)
		{
		case EEmployeeType.Botanist:
			return BotanistPrefab;
		case EEmployeeType.Handler:
			return PackagerPrefab;
		case EEmployeeType.Chemist:
			return ChemistPrefab;
		case EEmployeeType.Cleaner:
			return CleanerPrefab;
		default:
			Console.LogError("Employee type not found: " + type);
			return null;
		}
	}

	public List<Employee> GetEmployeesByType(EEmployeeType type)
	{
		List<Employee> list = new List<Employee>();
		foreach (Employee allEmployee in AllEmployees)
		{
			if (allEmployee.EmployeeType == type)
			{
				list.Add(allEmployee);
			}
		}
		return list;
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EEmployees_002EEmployeeManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EEmployees_002EEmployeeManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_CreateEmployee_311954683));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EEmployees_002EEmployeeManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EEmployees_002EEmployeeManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_CreateEmployee_311954683(ScheduleOne.Property.Property property, EEmployeeType type, string firstName, string lastName, string id, bool male, int appearanceIndex, Vector3 position, Quaternion rotation, string guid = "")
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
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
			return;
		}
		Channel val = (Channel)0;
		PooledWriter writer = WriterPool.GetWriter();
		GeneratedWriters___Internal.Write___ScheduleOne_002EProperty_002EPropertyFishNet_002ESerializing_002EGenerated((Writer)(object)writer, property);
		GeneratedWriters___Internal.Write___ScheduleOne_002EEmployees_002EEEmployeeTypeFishNet_002ESerializing_002EGenerated((Writer)(object)writer, type);
		((Writer)writer).WriteString(firstName);
		((Writer)writer).WriteString(lastName);
		((Writer)writer).WriteString(id);
		((Writer)writer).WriteBoolean(male);
		((Writer)writer).WriteInt32(appearanceIndex, (AutoPackType)1);
		((Writer)writer).WriteVector3(position);
		((Writer)writer).WriteQuaternion(rotation, (AutoPackType)1);
		((Writer)writer).WriteString(guid);
		((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
		writer.Store();
	}

	public void RpcLogic___CreateEmployee_311954683(ScheduleOne.Property.Property property, EEmployeeType type, string firstName, string lastName, string id, bool male, int appearanceIndex, Vector3 position, Quaternion rotation, string guid = "")
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		CreateEmployee_Server(property, type, firstName, lastName, id, male, appearanceIndex, position, rotation, guid);
	}

	private void RpcReader___Server_CreateEmployee_311954683(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		ScheduleOne.Property.Property property = GeneratedReaders___Internal.Read___ScheduleOne_002EProperty_002EPropertyFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		EEmployeeType type = GeneratedReaders___Internal.Read___ScheduleOne_002EEmployees_002EEEmployeeTypeFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		string firstName = ((Reader)PooledReader0).ReadString();
		string lastName = ((Reader)PooledReader0).ReadString();
		string id = ((Reader)PooledReader0).ReadString();
		bool male = ((Reader)PooledReader0).ReadBoolean();
		int appearanceIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		Vector3 position = ((Reader)PooledReader0).ReadVector3();
		Quaternion rotation = ((Reader)PooledReader0).ReadQuaternion((AutoPackType)1);
		string guid = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___CreateEmployee_311954683(property, type, firstName, lastName, id, male, appearanceIndex, position, rotation, guid);
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
