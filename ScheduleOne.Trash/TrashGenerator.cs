using System;
using System.Collections.Generic;
using FishNet;
using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using UnityEngine;
using UnityEngine.AI;

namespace ScheduleOne.Trash;

[RequireComponent(typeof(BoxCollider))]
public class TrashGenerator : MonoBehaviour, IGUIDRegisterable, ISaveable
{
	public const float TRASH_GENERATION_FRACTION = 0.2f;

	public const float DEFAULT_TRASH_PER_M2 = 0.015f;

	public static List<TrashGenerator> AllGenerators = new List<TrashGenerator>();

	[Range(1f, 200f)]
	[SerializeField]
	private int MaxTrashCount = 10;

	[SerializeField]
	private int TrashCountMultiplier = 1;

	[SerializeField]
	private List<TrashItem> generatedTrash = new List<TrashItem>();

	[Header("Settings")]
	public LayerMask GroundCheckMask;

	private BoxCollider boxCollider;

	public string StaticGUID = string.Empty;

	public string SaveFolderName => "Generator_" + GUID.ToString().Substring(0, 6);

	public string SaveFileName => "Generator_" + GUID.ToString().Substring(0, 6);

	public Loader Loader => null;

	public bool ShouldSaveUnderFolder => false;

	public List<string> LocalExtraFiles { get; set; } = new List<string>();

	public List<string> LocalExtraFolders { get; set; } = new List<string>();

	public bool HasChanged { get; set; }

	public Guid GUID { get; protected set; }

	public void SetGUID(Guid guid)
	{
		GUID = guid;
		GUIDManager.RegisterObject(this);
	}

	private void Awake()
	{
		AllGenerators.Add(this);
	}

	private void Start()
	{
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onSleepStart = (Action)Delegate.Combine(instance.onSleepStart, new Action(SleepStart));
		boxCollider = ((Component)this).GetComponent<BoxCollider>();
		((Collider)boxCollider).isTrigger = true;
		LayerUtility.SetLayerRecursively(((Component)this).gameObject, LayerMask.NameToLayer("Invisible"));
		GUID = new Guid(StaticGUID);
		GUIDManager.RegisterObject(this);
		InitializeSaveable();
	}

	public virtual void InitializeSaveable()
	{
		Singleton<SaveManager>.Instance.RegisterSaveable(this);
	}

	private void OnValidate()
	{
		if (string.IsNullOrEmpty(StaticGUID))
		{
			RegenerateGUID();
		}
	}

	private void OnDestroy()
	{
		AllGenerators.Remove(this);
	}

	private void OnDrawGizmos()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = Color.green;
		boxCollider = ((Component)this).GetComponent<BoxCollider>();
		Bounds bounds = ((Collider)boxCollider).bounds;
		Gizmos.DrawWireCube(((Bounds)(ref bounds)).center, new Vector3(boxCollider.size.x * ((Component)this).transform.localScale.x, boxCollider.size.y * ((Component)this).transform.localScale.y, boxCollider.size.z * ((Component)this).transform.localScale.z));
	}

	public void AddGeneratedTrash(TrashItem item)
	{
		if (!generatedTrash.Contains(item))
		{
			item.onDestroyed = (Action<TrashItem>)Delegate.Combine(item.onDestroyed, new Action<TrashItem>(RemoveGeneratedTrash));
			generatedTrash.Add(item);
			HasChanged = true;
		}
	}

	public void RemoveGeneratedTrash(TrashItem item)
	{
		item.onDestroyed = (Action<TrashItem>)Delegate.Remove(item.onDestroyed, new Action<TrashItem>(RemoveGeneratedTrash));
		generatedTrash.Remove(item);
		HasChanged = true;
	}

	[Button]
	private void RegenerateGUID()
	{
		StaticGUID = Guid.NewGuid().ToString();
	}

	[Button]
	private void AutoCalculateTrashCount()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		boxCollider = ((Component)this).GetComponent<BoxCollider>();
		float num = boxCollider.size.x * ((Component)this).transform.localScale.x * (boxCollider.size.z * ((Component)this).transform.localScale.z);
		MaxTrashCount = Mathf.FloorToInt(num * 0.015f * (float)TrashCountMultiplier);
	}

	[Button]
	private void GenerateMaxTrash()
	{
		GenerateTrash(MaxTrashCount - generatedTrash.Count);
	}

	private void SleepStart()
	{
		if (InstanceFinder.IsServer)
		{
			int num = Mathf.Min(MaxTrashCount - generatedTrash.Count, Mathf.FloorToInt((float)MaxTrashCount * 0.2f));
			if (num > 0)
			{
				GenerateTrash(num);
			}
		}
	}

	private void GenerateTrash(int count)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = default(Vector3);
		RaycastHit val2 = default(RaycastHit);
		for (int i = 0; i < count; i++)
		{
			Bounds bounds = ((Collider)boxCollider).bounds;
			float x = ((Bounds)(ref bounds)).min.x;
			bounds = ((Collider)boxCollider).bounds;
			float num = Random.Range(x, ((Bounds)(ref bounds)).max.x);
			bounds = ((Collider)boxCollider).bounds;
			float y = ((Bounds)(ref bounds)).min.y;
			bounds = ((Collider)boxCollider).bounds;
			float num2 = Random.Range(y, ((Bounds)(ref bounds)).max.y);
			bounds = ((Collider)boxCollider).bounds;
			float z = ((Bounds)(ref bounds)).min.z;
			bounds = ((Collider)boxCollider).bounds;
			((Vector3)(ref val))._002Ector(num, num2, Random.Range(z, ((Bounds)(ref bounds)).max.z));
			val = (Physics.Raycast(val, Vector3.down, ref val2, 20f, LayerMask.op_Implicit(GroundCheckMask)) ? ((RaycastHit)(ref val2)).point : val);
			int num3 = 0;
			NavMeshHit hit;
			while (!NavMeshUtility.SamplePosition(val, out hit, 1.5f, -1))
			{
				if (num3 > 10)
				{
					Console.Log("Failed to find a valid position for trash item");
					break;
				}
				bounds = ((Collider)boxCollider).bounds;
				float x2 = ((Bounds)(ref bounds)).min.x;
				bounds = ((Collider)boxCollider).bounds;
				float num4 = Random.Range(x2, ((Bounds)(ref bounds)).max.x);
				bounds = ((Collider)boxCollider).bounds;
				float y2 = ((Bounds)(ref bounds)).min.y;
				bounds = ((Collider)boxCollider).bounds;
				float num5 = Random.Range(y2, ((Bounds)(ref bounds)).max.y);
				bounds = ((Collider)boxCollider).bounds;
				float z2 = ((Bounds)(ref bounds)).min.z;
				bounds = ((Collider)boxCollider).bounds;
				((Vector3)(ref val))._002Ector(num4, num5, Random.Range(z2, ((Bounds)(ref bounds)).max.z));
				val = (Physics.Raycast(val, Vector3.down, ref val2, 20f, LayerMask.op_Implicit(GroundCheckMask)) ? ((RaycastHit)(ref val2)).point : val);
				num3++;
			}
			val += Vector3.up * 0.5f;
			TrashItem randomGeneratableTrashPrefab = NetworkSingleton<TrashManager>.Instance.GetRandomGeneratableTrashPrefab();
			TrashItem trashItem = NetworkSingleton<TrashManager>.Instance.CreateTrashItem(randomGeneratableTrashPrefab.ID, val, Random.rotation);
			trashItem.SetContinuousCollisionDetection();
			AddGeneratedTrash(trashItem);
		}
	}

	public bool ShouldSave()
	{
		return generatedTrash.Count > 0;
	}

	public virtual TrashGeneratorData GetSaveData()
	{
		return new TrashGeneratorData(GUID.ToString(), generatedTrash.ConvertAll((TrashItem x) => x.GUID.ToString()).ToArray());
	}

	public string GetSaveString()
	{
		return GetSaveData().GetJson();
	}
}
