using System;
using System.Collections;
using FishNet.Object;
using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.AvatarFramework.Equipping;

public class AvatarEquippable : MonoBehaviour
{
	public enum ETriggerType
	{
		Trigger,
		Bool
	}

	public enum EHand
	{
		Left,
		Right
	}

	[Header("Settings")]
	public Transform AlignmentPoint;

	[Range(0f, 1f)]
	public float Suspiciousness;

	public EHand Hand = EHand.Right;

	public ETriggerType TriggerType;

	public string AnimationTrigger = "RightArm_Hold_ClosedHand";

	private bool _equipped;

	public string AssetPath = string.Empty;

	protected Avatar avatar;

	[Button]
	public void RecalculateAssetPath()
	{
		AssetPath = AssetPathUtility.GetResourcesPath((Object)(object)((Component)this).gameObject);
		string[] array = AssetPath.Split('/', StringSplitOptions.None);
		array[array.Length - 1] = ((Object)((Component)this).gameObject).name;
		AssetPath = string.Join("/", array);
	}

	protected virtual void Awake()
	{
		if (AssetPath == string.Empty)
		{
			Console.LogWarning(((Object)((Component)this).gameObject).name + " does not have an assetpath!");
		}
	}

	public virtual void Equip(Avatar _avatar)
	{
		avatar = _avatar;
		_equipped = true;
		if (Hand == EHand.Right)
		{
			((Component)this).transform.SetParent(avatar.Animation.RightHandContainer);
		}
		else
		{
			((Component)this).transform.SetParent(avatar.Animation.LeftHandContainer);
		}
		PositionAnimationModel();
		InitializeAnimation();
		Player componentInParent = ((Component)avatar).GetComponentInParent<Player>();
		if ((Object)(object)componentInParent != (Object)null && ((NetworkBehaviour)componentInParent).IsOwner && !componentInParent.avatarVisibleToLocalPlayer)
		{
			LayerUtility.SetLayerRecursively(((Component)this).gameObject, LayerMask.NameToLayer("Invisible"));
		}
		Collider[] componentsInChildren = ((Component)this).GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].isTrigger = true;
		}
	}

	public virtual void InitializeAnimation()
	{
		((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Wait());
		IEnumerator Wait()
		{
			yield return null;
			if (_equipped)
			{
				ResetTrigger("EndAction");
				if (TriggerType == ETriggerType.Trigger)
				{
					SetTrigger(AnimationTrigger);
				}
				else
				{
					SetBool(AnimationTrigger, val: true);
				}
			}
		}
	}

	public virtual void Unequip()
	{
		_equipped = false;
		if (TriggerType == ETriggerType.Trigger)
		{
			SetTrigger("EndAction");
		}
		else
		{
			SetBool(AnimationTrigger, val: false);
		}
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	private void PositionAnimationModel()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		Transform val = ((Hand == EHand.Right) ? avatar.Animation.RightHandAlignmentPoint : avatar.Animation.LeftHandAlignmentPoint);
		((Component)this).transform.rotation = val.rotation * (Quaternion.Inverse(AlignmentPoint.rotation) * ((Component)this).transform.rotation);
		((Component)this).transform.position = val.position + (((Component)this).transform.position - AlignmentPoint.position);
	}

	protected void SetTrigger(string anim)
	{
		if ((Object)(object)((Component)avatar).GetComponentInParent<Player>() != (Object)null)
		{
			((Component)avatar).GetComponentInParent<Player>().SetAnimationTrigger(anim);
		}
		else if ((Object)(object)((Component)avatar).GetComponentInParent<NPC>() != (Object)null)
		{
			((Component)avatar).GetComponentInParent<NPC>().SetAnimationTrigger(anim);
		}
	}

	protected void SetBool(string anim, bool val)
	{
		if ((Object)(object)((Component)avatar).GetComponentInParent<Player>() != (Object)null)
		{
			((Component)avatar).GetComponentInParent<Player>().SetAnimationBool(anim, val);
		}
		else if ((Object)(object)((Component)avatar).GetComponentInParent<NPC>() != (Object)null)
		{
			((Component)avatar).GetComponentInParent<NPC>().SetAnimationBool(anim, val);
		}
	}

	protected void ResetTrigger(string anim)
	{
		if ((Object)(object)((Component)avatar).GetComponentInParent<Player>() != (Object)null)
		{
			((Component)avatar).GetComponentInParent<Player>().ResetAnimationTrigger(anim);
		}
		else if ((Object)(object)((Component)avatar).GetComponentInParent<NPC>() != (Object)null)
		{
			((Component)avatar).GetComponentInParent<NPC>().ResetAnimationTrigger(anim);
		}
	}

	public virtual void ReceiveMessage(string message, object parameter)
	{
	}
}
