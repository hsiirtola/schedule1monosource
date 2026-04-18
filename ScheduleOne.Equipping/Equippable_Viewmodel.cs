using ScheduleOne.AvatarFramework.Equipping;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Rendering;

namespace ScheduleOne.Equipping;

public class Equippable_Viewmodel : Equippable
{
	[Header("Viewmodel settings")]
	public Vector3 localPosition;

	public Vector3 localEulerAngles;

	public Vector3 localScale = Vector3.one;

	[Header("Third person animation settings")]
	public AvatarEquippable AvatarEquippable;

	public override void Equip(ItemInstance item)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Invalid comparison between Unknown and I4
		base.Equip(item);
		((Component)this).transform.localPosition = localPosition;
		((Component)this).transform.localEulerAngles = localEulerAngles;
		((Component)this).transform.localScale = localScale;
		LayerUtility.SetLayerRecursively(((Component)this).gameObject, LayerMask.NameToLayer("Viewmodel"));
		MeshRenderer[] componentsInChildren = ((Component)this).gameObject.GetComponentsInChildren<MeshRenderer>(true);
		foreach (MeshRenderer val in componentsInChildren)
		{
			if ((int)((Renderer)val).shadowCastingMode == 3)
			{
				((Renderer)val).enabled = false;
			}
			else
			{
				((Renderer)val).shadowCastingMode = (ShadowCastingMode)0;
			}
		}
		PlayEquipAnimation();
	}

	public override void Unequip()
	{
		base.Unequip();
		PlayUnequipAnimation();
	}

	protected virtual void PlayEquipAnimation()
	{
		if ((Object)(object)AvatarEquippable != (Object)null)
		{
			Player.Local.SendEquippable_Networked(AvatarEquippable.AssetPath);
		}
	}

	protected virtual void PlayUnequipAnimation()
	{
		if ((Object)(object)AvatarEquippable != (Object)null)
		{
			Player.Local.SendEquippable_Networked(string.Empty);
		}
	}
}
