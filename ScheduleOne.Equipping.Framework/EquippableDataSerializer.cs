using System;
using FishNet.Serializing;
using ScheduleOne.Core;
using ScheduleOne.Core.Equipping.Framework;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Equipping.Framework;

public static class EquippableDataSerializer
{
	public static void WriteEquippableData(this Writer writer, EquippableData value)
	{
		if ((Object)(object)value == (Object)null)
		{
			writer.WriteString(string.Empty);
		}
		else
		{
			writer.WriteString(((IdentifiedScriptableObject)value).GUID.ToString());
		}
	}

	public static EquippableData ReadEquippableData(this Reader reader)
	{
		if (reader == null)
		{
			return null;
		}
		if (reader.Remaining == 0)
		{
			return null;
		}
		string text = reader.ReadString();
		if (text == string.Empty)
		{
			return null;
		}
		return Singleton<EquippableDataRegistry>.Instance.GetEquippableData(Guid.Parse(text));
	}
}
