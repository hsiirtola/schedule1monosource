using ScheduleOne.Core.Equipping.Framework;
using UnityEngine;

namespace ScheduleOne.Equipping.Framework;

[CreateAssetMenu(fileName = "Equippable (Custom Handler)", menuName = "ScheduleOne/Equipping/Equippable (Custom Handler)")]
public class CustomHandlerEquippableData : EquippableData
{
	[Header("Custom Handler")]
	[Tooltip("If not assigned, the handler will be looked up in the EquippableHandlerService as normal.")]
	public EquippedItemHandler Handler;

	private void OnValidate()
	{
		if ((Object)(object)Handler == (Object)null)
		{
			Debug.LogWarning((object)(((Object)this).name + " does not have a custom handler assigned! It will use the default handler lookup instead."), (Object)(object)this);
		}
	}
}
