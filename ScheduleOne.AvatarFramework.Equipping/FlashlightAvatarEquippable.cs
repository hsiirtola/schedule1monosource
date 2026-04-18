using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.AvatarFramework.Equipping;

public class FlashlightAvatarEquippable : AvatarEquippable
{
	public OptimizedLight Light;

	public override void Equip(Avatar _avatar)
	{
		base.Equip(_avatar);
		if ((Object)(object)((Component)_avatar).GetComponentInParent<Player>() == (Object)(object)Player.Local)
		{
			Debug.Log((object)"Turning off third person flashlight light for local player");
			((Component)Light).gameObject.SetActive(false);
		}
	}
}
