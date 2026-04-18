using System;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.NPCs;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders;

public class LegacyNPCLoader : Loader
{
	public virtual string NPCType => typeof(NPCData).Name;

	public LegacyNPCLoader()
	{
		Singleton<LoadManager>.Instance.LegacyNPCLoaders.Add(this);
	}

	public override void Load(string mainPath)
	{
		if (!TryLoadFile(mainPath, "NPC", out var contents))
		{
			return;
		}
		NPCData data = null;
		try
		{
			data = JsonUtility.FromJson<NPCData>(contents);
		}
		catch (Exception ex)
		{
			Console.LogError(GetType()?.ToString() + " error reading data: " + ex);
		}
		NPC nPC = NPCManager.NPCRegistry.FirstOrDefault((NPC x) => x.ID == data.ID);
		if ((Object)(object)nPC == (Object)null)
		{
			Console.LogWarning("Failed to find NPC with ID: " + data.ID);
			return;
		}
		nPC.Load(data, mainPath);
		if (TryLoadFile(mainPath, "Relationship", out var contents2))
		{
			RelationshipData relationshipData = null;
			try
			{
				relationshipData = JsonUtility.FromJson<RelationshipData>(contents2);
			}
			catch (Exception ex2)
			{
				Console.LogError(GetType()?.ToString() + " error reading relationship data: " + ex2);
			}
			if (relationshipData != null)
			{
				if (!float.IsNaN(relationshipData.RelationDelta) && !float.IsInfinity(relationshipData.RelationDelta))
				{
					nPC.RelationData.SetRelationship(relationshipData.RelationDelta);
				}
				if (relationshipData.Unlocked)
				{
					nPC.RelationData.Unlock(relationshipData.UnlockType, notify: false);
				}
			}
		}
		TryLoadInventory(mainPath, nPC);
		if (TryLoadFile(mainPath, "Health", out var contents3))
		{
			NPCHealthData nPCHealthData = null;
			try
			{
				nPCHealthData = JsonUtility.FromJson<NPCHealthData>(contents3);
			}
			catch (Exception ex3)
			{
				Console.LogError(GetType()?.ToString() + " error reading health data: " + ex3);
			}
			if (nPCHealthData != null)
			{
				nPC.Health.Load(nPCHealthData);
			}
		}
		if (TryLoadFile(mainPath, "MessageConversation", out var contents4))
		{
			MSGConversationData mSGConversationData = null;
			try
			{
				mSGConversationData = JsonUtility.FromJson<MSGConversationData>(contents4);
			}
			catch (Exception ex4)
			{
				Console.LogError(GetType()?.ToString() + " error reading message data: " + ex4);
			}
			if (mSGConversationData != null)
			{
				nPC.MSGConversation.Load(mSGConversationData);
			}
		}
		if (TryLoadFile(mainPath, "CustomerData", out var contents5))
		{
			ScheduleOne.Persistence.Datas.CustomerData customerData = null;
			try
			{
				customerData = JsonUtility.FromJson<ScheduleOne.Persistence.Datas.CustomerData>(contents5);
			}
			catch (Exception ex5)
			{
				Console.LogError(GetType()?.ToString() + " error reading customer data: " + ex5);
			}
			if (customerData != null && (Object)(object)((Component)nPC).GetComponent<Customer>() != (Object)null)
			{
				((Component)nPC).GetComponent<Customer>().Load(customerData);
			}
		}
	}

	protected void TryLoadInventory(string mainPath, NPC npc)
	{
		if (TryLoadFile(mainPath, "Inventory", out var contents) && ItemSet.TryDeserialize(contents, out var itemSet))
		{
			itemSet.LoadTo(npc.Inventory.ItemSlots);
		}
	}
}
