using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.NPCs;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders;

public class NPCLoader : DynamicLoader
{
	public virtual string NPCType => typeof(NPCData).Name;

	public NPCLoader()
	{
		Singleton<LoadManager>.Instance.NPCLoaders.Add(this);
	}

	public override void Load(DynamicSaveData saveData)
	{
		base.Load(saveData);
		NPCData baseData = DynamicLoader.ExtractBaseData<NPCData>(saveData);
		if (baseData == null)
		{
			return;
		}
		NPC nPC = NPCManager.NPCRegistry.FirstOrDefault((NPC x) => x.ID == baseData.ID);
		if ((Object)(object)nPC == (Object)null)
		{
			Console.LogWarning("Failed to find NPC with ID: " + baseData.ID);
			return;
		}
		nPC.Load(saveData, baseData);
		if (saveData.TryGetData("Relationship", out RelationshipData data))
		{
			if (!float.IsNaN(data.RelationDelta) && !float.IsInfinity(data.RelationDelta))
			{
				nPC.RelationData.SetRelationship(data.RelationDelta);
			}
			if (data.Unlocked)
			{
				nPC.RelationData.Unlock(data.UnlockType, notify: false);
			}
		}
		if (saveData.TryGetData("Health", out NPCHealthData data2))
		{
			nPC.Health.Load(data2);
		}
		if (saveData.TryGetData("MessageConversation", out MSGConversationData data3))
		{
			nPC.MSGConversation.Load(data3);
		}
		if (saveData.TryGetData("CustomerData", out ScheduleOne.Persistence.Datas.CustomerData data4) && (Object)(object)((Component)nPC).GetComponent<Customer>() != (Object)null)
		{
			((Component)nPC).GetComponent<Customer>().Load(data4);
		}
		if (saveData.TryGetData("Inventory", out var data5))
		{
			if (ItemSet.TryDeserialize(data5, out var itemSet))
			{
				itemSet.LoadTo(nPC.Inventory.ItemSlots);
			}
			else
			{
				Console.LogWarning("Failed to deserialize inventory for NPC: " + nPC.ID);
			}
		}
	}
}
