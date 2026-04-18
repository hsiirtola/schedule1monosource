using System;
using System.Collections.Generic;
using ScheduleOne.Core.Items.Framework;

namespace ScheduleOne.ItemFramework;

[Serializable]
public class SlotFilter
{
	public enum EType
	{
		None,
		Whitelist,
		Blacklist
	}

	public EType Type;

	public List<string> ItemIDs = new List<string>();

	public List<EQuality> AllowedQualities = new List<EQuality>();

	public SlotFilter()
	{
		Type = EType.None;
		ItemIDs = new List<string>();
		AllowedQualities = new List<EQuality>
		{
			EQuality.Trash,
			EQuality.Poor,
			EQuality.Standard,
			EQuality.Premium,
			EQuality.Heavenly
		};
	}

	public bool DoesItemMatchFilter(ItemInstance instance)
	{
		if (Type == EType.Whitelist && !ItemIDs.Contains(((BaseItemInstance)instance).ID))
		{
			return false;
		}
		if (Type == EType.Blacklist && ItemIDs.Contains(((BaseItemInstance)instance).ID))
		{
			return false;
		}
		if (instance is QualityItemInstance && !AllowedQualities.Contains(((QualityItemInstance)instance).Quality))
		{
			return false;
		}
		return true;
	}

	public bool IsDefault()
	{
		if (Type == EType.None && ItemIDs.Count == 0)
		{
			return AllowedQualities.Count == 5;
		}
		return false;
	}

	public SlotFilter Clone()
	{
		return new SlotFilter
		{
			Type = Type,
			ItemIDs = new List<string>(ItemIDs),
			AllowedQualities = new List<EQuality>(AllowedQualities)
		};
	}
}
