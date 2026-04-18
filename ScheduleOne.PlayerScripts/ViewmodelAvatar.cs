using ScheduleOne.AvatarFramework;
using ScheduleOne.DevUtilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace ScheduleOne.PlayerScripts;

public class ViewmodelAvatar : Singleton<ViewmodelAvatar>
{
	[SerializeField]
	private float ArmShift = 1f;

	public Avatar ParentAvatar;

	public Animator Animator;

	public Avatar Avatar;

	public Transform RightHandContainer;

	private Vector3 _leftShoulderDefaultLocalPos;

	private Vector3 _rightShoulderDefaultLocalPos;

	public bool IsVisible { get; private set; }

	protected override void Awake()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		SetVisibility(isVisible: false);
		if ((Object)(object)ParentAvatar.CurrentSettings != (Object)null)
		{
			SetAppearance(ParentAvatar.CurrentSettings);
		}
		ParentAvatar.onSettingsLoaded.AddListener((UnityAction)delegate
		{
			SetAppearance(ParentAvatar.CurrentSettings);
		});
		_leftShoulderDefaultLocalPos = Avatar.LeftShoulder.localPosition;
		_rightShoulderDefaultLocalPos = Avatar.RightShoulder.localPosition;
	}

	public void SetVisibility(bool isVisible)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		Animator.keepAnimatorStateOnDisable = false;
		SetOffset(Vector3.zero);
		IsVisible = isVisible;
		((Component)this).gameObject.SetActive(isVisible);
	}

	private void LateUpdate()
	{
		SetBoneTransforms();
	}

	private void SetBoneTransforms()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		Avatar.HipBone.SetLocalPositionAndRotation(new Vector3(0f, 0f, 0f - ArmShift), Quaternion.identity);
		Transform leftShoulder = Avatar.LeftShoulder;
		leftShoulder.localPosition += Avatar.LeftShoulder.parent.InverseTransformDirection(Avatar.HipBone.forward) * ArmShift;
		Transform rightShoulder = Avatar.RightShoulder;
		rightShoulder.localPosition += Avatar.RightShoulder.parent.InverseTransformDirection(Avatar.HipBone.forward) * ArmShift;
	}

	public void SetAppearance(AvatarSettings settings)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Invalid comparison between Unknown and I4
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Invalid comparison between Unknown and I4
		AvatarSettings avatarSettings = Object.Instantiate<AvatarSettings>(settings);
		avatarSettings.Height = 0.25f;
		Avatar.LoadAvatarSettings(avatarSettings);
		LayerUtility.SetLayerRecursively(((Component)this).gameObject, LayerMask.NameToLayer("Viewmodel"));
		MeshRenderer[] componentsInChildren = ((Component)this).GetComponentsInChildren<MeshRenderer>();
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
		SkinnedMeshRenderer[] componentsInChildren2 = ((Component)this).GetComponentsInChildren<SkinnedMeshRenderer>();
		foreach (SkinnedMeshRenderer val2 in componentsInChildren2)
		{
			((Renderer)val2).allowOcclusionWhenDynamic = false;
			val2.updateWhenOffscreen = true;
			if ((int)((Renderer)val2).shadowCastingMode == 3)
			{
				((Renderer)val2).enabled = false;
			}
			else
			{
				((Renderer)val2).shadowCastingMode = (ShadowCastingMode)0;
			}
		}
	}

	public void SetAnimatorController(RuntimeAnimatorController controller)
	{
		Animator.runtimeAnimatorController = controller;
	}

	public void SetOffset(Vector3 offset)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localPosition = offset;
	}

	public void SetRotationOffset(Vector3 eulerAngles)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localRotation = Quaternion.Euler(eulerAngles);
	}
}
