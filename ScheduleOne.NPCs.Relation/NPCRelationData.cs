using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.NPCs.Relation;

[Serializable]
public class NPCRelationData
{
	public enum EUnlockType
	{
		Recommendation,
		DirectApproach
	}

	public const float MinDelta = 0f;

	public const float MaxDelta = 5f;

	public const float DEFAULT_RELATION_DELTA = 2f;

	[SerializeField]
	protected List<NPC> FullGameConnections = new List<NPC>();

	[SerializeField]
	protected List<NPC> DemoConnections = new List<NPC>();

	public Action<float> onRelationshipChange;

	public Action<EUnlockType, bool> onUnlocked;

	public float RelationDelta { get; protected set; } = 2f;

	public float NormalizedRelationDelta => RelationDelta / 5f;

	public bool Unlocked { get; protected set; }

	public EUnlockType UnlockType { get; protected set; }

	public NPC NPC { get; protected set; }

	public List<NPC> Connections => FullGameConnections;

	public void SetNPC(NPC npc)
	{
		NPC = npc;
	}

	public void Init(NPC npc)
	{
		SetNPC(npc);
		for (int i = 0; i < Connections.Count; i++)
		{
			if ((Object)(object)Connections[i] == (Object)null)
			{
				Connections.RemoveAt(i);
				i--;
			}
			else if (!Connections[i].RelationData.Connections.Contains(NPC))
			{
				Connections[i].RelationData.Connections.Add(NPC);
			}
		}
	}

	public virtual void ChangeRelationship(float deltaChange, bool network = true)
	{
		SetRelationship(RelationDelta + deltaChange, network);
	}

	public virtual void SetRelationship(float newDelta, bool network = true)
	{
		if (newDelta != RelationDelta)
		{
			float relationDelta = RelationDelta;
			RelationDelta = Mathf.Clamp(newDelta, 0f, 5f);
			if (RelationDelta - relationDelta != 0f && onRelationshipChange != null)
			{
				onRelationshipChange(RelationDelta - relationDelta);
			}
			if (!Singleton<LoadManager>.Instance.IsLoading)
			{
				Console.Log($"{NPC.fullName} relationship changed from {relationDelta} -> {RelationDelta}");
			}
			if (network)
			{
				NPC.SendRelationship(RelationDelta);
			}
		}
	}

	public virtual void Unlock(EUnlockType type, bool notify = true)
	{
		if (!Unlocked)
		{
			Unlocked = true;
			UnlockType = type;
			if (onUnlocked != null)
			{
				onUnlocked(type, notify);
			}
		}
	}

	public virtual void UnlockConnections()
	{
		for (int i = 0; i < Connections.Count; i++)
		{
			if (!Connections[i].RelationData.Unlocked)
			{
				Connections[i].RelationData.Unlock(EUnlockType.Recommendation);
			}
		}
	}

	public RelationshipData GetSaveData()
	{
		return new RelationshipData(RelationDelta, Unlocked, UnlockType);
	}

	public float GetAverageMutualRelationship()
	{
		float num = 0f;
		int num2 = 0;
		for (int i = 0; i < Connections.Count; i++)
		{
			if (Connections[i].RelationData.Unlocked)
			{
				num2++;
				num += Connections[i].RelationData.RelationDelta;
			}
		}
		if (num2 == 0)
		{
			return 0f;
		}
		return num / (float)num2;
	}

	public bool IsKnown()
	{
		if (!Unlocked)
		{
			return IsMutuallyKnown();
		}
		return true;
	}

	public bool IsMutuallyKnown()
	{
		for (int i = 0; i < Connections.Count; i++)
		{
			if (!((Object)(object)Connections[i] == (Object)null) && Connections[i].RelationData.Unlocked)
			{
				return true;
			}
		}
		return false;
	}

	public List<NPC> GetLockedConnections(bool excludeCustomers = false)
	{
		return Connections.FindAll((NPC x) => !x.RelationData.Unlocked && (!excludeCustomers || (Object)(object)((Component)x).GetComponent<Customer>() == (Object)null));
	}

	public List<NPC> GetLockedDealers(bool excludeRecommended)
	{
		return Connections.FindAll((NPC x) => !x.RelationData.Unlocked && x is Dealer && (!excludeRecommended || !(x as Dealer).HasBeenRecommended));
	}

	public List<NPC> GetLockedSuppliers()
	{
		return Connections.FindAll((NPC x) => !x.RelationData.Unlocked && x is Supplier);
	}
}
