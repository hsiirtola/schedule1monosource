using System;
using System.Collections.Generic;
using FishNet.Serializing;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Storage;
using ScheduleOne.Trash;

namespace ScheduleOne.ObjectScripts.WateringCan;

[Serializable]
public class TrashGrabberInstance(ItemDefinition definition, int quantity) : StorableItemInstance(definition, quantity)
{
	public const int TRASH_CAPACITY = 20;

	private TrashContent Content = new TrashContent();

	public override ItemInstance GetCopy(int overrideQuantity = -1)
	{
		int quantity = ((BaseItemInstance)this).Quantity;
		if (overrideQuantity != -1)
		{
			quantity = overrideQuantity;
		}
		TrashGrabberInstance trashGrabberInstance = new TrashGrabberInstance(base.Definition, quantity);
		trashGrabberInstance.Content.LoadFromData(Content.GetData());
		return trashGrabberInstance;
	}

	public void LoadContentData(TrashContentData content)
	{
		Content.LoadFromData(content);
	}

	public override ItemData GetItemData()
	{
		return new TrashGrabberData(((BaseItemInstance)this).ID, ((BaseItemInstance)this).Quantity, Content.GetData());
	}

	public void AddTrash(string id, int quantity)
	{
		Content.AddTrash(id, quantity);
		((BaseItemInstance)this).InvokeDataChange();
	}

	public void RemoveTrash(string id, int quantity)
	{
		Content.RemoveTrash(id, quantity);
		((BaseItemInstance)this).InvokeDataChange();
	}

	public void ClearTrash()
	{
		Content.Clear();
		((BaseItemInstance)this).InvokeDataChange();
	}

	public int GetTotalSize()
	{
		return Content.GetTotalSize();
	}

	public List<string> GetTrashIDs()
	{
		List<string> list = new List<string>();
		foreach (TrashContent.Entry entry in Content.Entries)
		{
			list.Add(entry.TrashID);
		}
		return list;
	}

	public List<int> GetTrashQuantities()
	{
		List<int> list = new List<int>();
		foreach (TrashContent.Entry entry in Content.Entries)
		{
			list.Add(entry.Quantity);
		}
		return list;
	}

	public List<ushort> GetTrashUshortQuantities()
	{
		List<ushort> list = new List<ushort>();
		foreach (TrashContent.Entry entry in Content.Entries)
		{
			list.Add((ushort)entry.Quantity);
		}
		return list;
	}

	public override void Write(Writer writer)
	{
		base.Write(writer);
		string[] array = GetTrashIDs().ToArray();
		writer.WriteArray<string>(array, 0, array.Length);
		ushort[] array2 = GetTrashUshortQuantities().ToArray();
		writer.WriteArray<ushort>(array2, 0, array2.Length);
	}

	public override void Read(Reader reader)
	{
		base.Read(reader);
		string[] array = new string[20];
		ushort[] array2 = new ushort[20];
		int num = reader.ReadArray<string>(ref array);
		reader.ReadArray<ushort>(ref array2);
		for (int i = 0; i < num; i++)
		{
			AddTrash(array[i], array2[i]);
		}
	}
}
