using System;
using System.Collections.Generic;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.ItemFramework;
using UnityEngine;

namespace ScheduleOne.Product;

[Serializable]
public class ProductList
{
	[Serializable]
	public class Entry
	{
		public string ProductID;

		public EQuality Quality;

		public int Quantity;

		public Entry(string productID, EQuality quality, int quantity)
		{
			ProductID = productID;
			Quality = quality;
			Quantity = quantity;
		}

		public Entry()
		{
			ProductID = string.Empty;
			Quality = EQuality.Standard;
			Quantity = 1;
		}
	}

	public List<Entry> entries = new List<Entry>();

	public string GetCommaSeperatedString()
	{
		string text = string.Empty;
		foreach (Entry entry in entries)
		{
			if (entry == null)
			{
				continue;
			}
			ItemDefinition item = Registry.GetItem(entry.ProductID);
			if (!((Object)(object)item == (Object)null))
			{
				text = text + entry.Quantity + "x ";
				text += ((BaseItemDefinition)item).Name;
				if (entry != entries[entries.Count - 1])
				{
					text += ", ";
				}
			}
		}
		return text;
	}

	public string GetLineSeperatedString()
	{
		string text = "\n";
		foreach (Entry entry in entries)
		{
			text = text + entry.Quantity + "x ";
			text += ((BaseItemDefinition)Registry.GetItem(entry.ProductID)).Name;
			if (entry != entries[entries.Count - 1])
			{
				text += "\n";
			}
		}
		return text;
	}

	public string GetQualityString()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		Entry entry = entries[0];
		return "<color=#" + ColorUtility.ToHtmlStringRGBA(ItemQuality.GetColor(entry.Quality)) + ">" + entry.Quality.ToString() + "</color> ";
	}

	public int GetTotalQuantity()
	{
		int num = 0;
		foreach (Entry entry in entries)
		{
			num += entry.Quantity;
		}
		return num;
	}
}
