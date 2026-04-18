using System;
using ScheduleOne.AvatarFramework;
using ScheduleOne.Clothing;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI.Items;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace ScheduleOne.UI;

public class CharacterDisplay : Singleton<CharacterDisplay>
{
	[Serializable]
	public class SlotAlignmentPoint
	{
		public EClothingSlot SlotType;

		public Transform Point;
	}

	public SlotAlignmentPoint[] AlignmentPoints;

	[Header("References")]
	public Transform Container;

	public Avatar ParentAvatar;

	public Avatar Avatar;

	public Transform AvatarContainer;

	private float targetRotation;

	public bool IsOpen { get; private set; }

	protected override void Awake()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		base.Awake();
		SetOpen(open: false);
		if ((Object)(object)ParentAvatar.CurrentSettings != (Object)null)
		{
			SetAppearance(ParentAvatar.CurrentSettings);
		}
		ParentAvatar.onSettingsLoaded.AddListener((UnityAction)delegate
		{
			SetAppearance(ParentAvatar.CurrentSettings);
		});
		AudioSource[] componentsInChildren = ((Component)Avatar).GetComponentsInChildren<AudioSource>();
		for (int num = 0; num < componentsInChildren.Length; num++)
		{
			((Behaviour)componentsInChildren[num]).enabled = false;
		}
	}

	public void SetOpen(bool open)
	{
		IsOpen = open;
		((Component)Container).gameObject.SetActive(open);
		if (IsOpen)
		{
			LayerUtility.SetLayerRecursively(((Component)Container).gameObject, LayerMask.NameToLayer("Overlay"));
			SetAppearance(ParentAvatar.CurrentSettings);
			Singleton<ItemUIManager>.Instance.EnableQuickMove(PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots(), Player.Local.Clothing.ItemSlots);
		}
	}

	private void Update()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if (IsOpen)
		{
			targetRotation = Mathf.Lerp(targetRotation, Mathf.Lerp(0f, 359f, Singleton<GameplayMenuInterface>.Instance.CharacterInterface.RotationSlider.value), Time.deltaTime * 5f);
			AvatarContainer.localEulerAngles = new Vector3(0f, targetRotation, 0f);
		}
	}

	public void SetAppearance(AvatarSettings settings)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Invalid comparison between Unknown and I4
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Invalid comparison between Unknown and I4
		AvatarSettings settings2 = Object.Instantiate<AvatarSettings>(settings);
		Avatar.LoadAvatarSettings(settings2);
		LayerUtility.SetLayerRecursively(((Component)this).gameObject, LayerMask.NameToLayer("Overlay"));
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
}
