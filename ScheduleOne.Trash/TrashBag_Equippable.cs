using System.Collections.Generic;
using ScheduleOne.Audio;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Equipping;
using ScheduleOne.Interaction;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ScheduleOne.Trash;

public class TrashBag_Equippable : Equippable_Viewmodel
{
	public const float TRASH_CONTAINER_INTERACT_DISTANCE = 2.75f;

	public const float BAG_TRASH_TIME = 1f;

	public const float PICKUP_RANGE = 3f;

	public const float PICKUP_AREA_RADIUS = 0.5f;

	public LayerMask PickupLookMask;

	[Header("References")]
	public DecalProjector PickupAreaProjector;

	public AudioSourceController RustleSound;

	public AudioSourceController BagSound;

	private float _bagTrashTime;

	private TrashContainer _baggedContainer;

	private float _pickupTrashTime;

	public static bool IsHoveringTrash => ((Component)Singleton<TrashBagCanvas>.Instance.InputPrompt).gameObject.activeSelf;

	public bool IsBaggingTrash { get; private set; }

	public bool IsPickingUpTrash { get; private set; }

	public override void Equip(ItemInstance item)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		base.Equip(item);
		((Component)Singleton<TrashBagCanvas>.Instance.InputPrompt).gameObject.SetActive(false);
		Singleton<TrashBagCanvas>.Instance.Open();
		((Component)PickupAreaProjector).transform.SetParent(NetworkSingleton<GameManager>.Instance.Temp);
		((Component)PickupAreaProjector).transform.localScale = Vector3.one;
		((Component)PickupAreaProjector).transform.forward = -Vector3.up;
		((Component)PickupAreaProjector).gameObject.SetActive(false);
	}

	public override void Unequip()
	{
		base.Unequip();
		Singleton<TrashBagCanvas>.Instance.Close();
		Object.Destroy((Object)(object)((Component)PickupAreaProjector).gameObject);
	}

	protected override void Update()
	{
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		((Component)Singleton<TrashBagCanvas>.Instance.InputPrompt).gameObject.SetActive(false);
		TrashContainer hoveredTrashContainer = GetHoveredTrashContainer();
		((Component)PickupAreaProjector).gameObject.SetActive(false);
		if (IsBaggingTrash)
		{
			if (!GameInput.GetButton(GameInput.ButtonCode.Interact) || (Object)(object)hoveredTrashContainer != (Object)(object)_baggedContainer)
			{
				StopBagTrash(complete: false);
				return;
			}
			_bagTrashTime += Time.deltaTime;
			Singleton<TrashBagCanvas>.Instance.InputPrompt.SetLabel("Bag trash");
			((Component)Singleton<TrashBagCanvas>.Instance.InputPrompt).gameObject.SetActive(true);
			Singleton<HUD>.Instance.ShowRadialIndicator(_bagTrashTime / 1f);
			if (_bagTrashTime >= 1f)
			{
				StopBagTrash(complete: true);
			}
		}
		else if (IsPickingUpTrash)
		{
			List<TrashItem> list = new List<TrashItem>();
			if (RaycastLook(out var hit) && IsPickupLocationValid(hit))
			{
				list = GetTrashItemsAtPoint(((RaycastHit)(ref hit)).point);
			}
			if (!GameInput.GetButton(GameInput.ButtonCode.Interact) || list.Count == 0)
			{
				StopPickup(complete: false);
				return;
			}
			_pickupTrashTime += Time.deltaTime;
			Singleton<TrashBagCanvas>.Instance.InputPrompt.SetLabel("Bag trash");
			((Component)Singleton<TrashBagCanvas>.Instance.InputPrompt).gameObject.SetActive(true);
			Singleton<HUD>.Instance.ShowRadialIndicator(_pickupTrashTime / 1f);
			((Component)PickupAreaProjector).transform.position = ((RaycastHit)(ref hit)).point + Vector3.up * 0.1f;
			((Component)PickupAreaProjector).gameObject.SetActive(true);
			if (_pickupTrashTime >= 1f)
			{
				StopPickup(complete: true);
			}
		}
		else if ((Object)(object)hoveredTrashContainer != (Object)null && hoveredTrashContainer.CanBeBagged())
		{
			_baggedContainer = hoveredTrashContainer;
			Singleton<TrashBagCanvas>.Instance.InputPrompt.SetLabel("Bag trash");
			((Component)Singleton<TrashBagCanvas>.Instance.InputPrompt).gameObject.SetActive(true);
			if (GameInput.GetButtonDown(GameInput.ButtonCode.Interact))
			{
				StartBagTrash(hoveredTrashContainer);
			}
		}
		else
		{
			if (!((Object)(object)hoveredTrashContainer == (Object)null) || !RaycastLook(out var hit2) || !IsPickupLocationValid(hit2))
			{
				return;
			}
			((Component)PickupAreaProjector).transform.position = ((RaycastHit)(ref hit2)).point + Vector3.up * 0.1f;
			((Component)PickupAreaProjector).gameObject.SetActive(true);
			if (GetTrashItemsAtPoint(((RaycastHit)(ref hit2)).point).Count > 0)
			{
				PickupAreaProjector.fadeFactor = 0.5f;
				Singleton<TrashBagCanvas>.Instance.InputPrompt.SetLabel("Bag trash");
				((Component)Singleton<TrashBagCanvas>.Instance.InputPrompt).gameObject.SetActive(true);
				if (GameInput.GetButtonDown(GameInput.ButtonCode.Interact))
				{
					StartPickup();
				}
			}
			else
			{
				PickupAreaProjector.fadeFactor = 0.05f;
			}
		}
	}

	private TrashContainer GetHoveredTrashContainer()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (PlayerSingleton<PlayerCamera>.Instance.LookRaycast(2.75f, out var hit, Singleton<InteractionManager>.Instance.Interaction_SearchMask))
		{
			TrashContainer componentInParent = ((Component)((RaycastHit)(ref hit)).collider).GetComponentInParent<TrashContainer>();
			if ((Object)(object)componentInParent != (Object)null)
			{
				return componentInParent;
			}
		}
		return null;
	}

	private bool RaycastLook(out RaycastHit hit)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return PlayerSingleton<PlayerCamera>.Instance.LookRaycast(3f, out hit, PickupLookMask);
	}

	private bool IsPickupLocationValid(RaycastHit hit)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		if (Vector3.Angle(((RaycastHit)(ref hit)).normal, Vector3.up) > 5f)
		{
			return false;
		}
		return true;
	}

	private List<TrashItem> GetTrashItemsAtPoint(Vector3 pos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Collider[] array = Physics.OverlapSphere(pos, 0.45f, LayerMask.op_Implicit(Singleton<InteractionManager>.Instance.Interaction_SearchMask), (QueryTriggerInteraction)2);
		List<TrashItem> list = new List<TrashItem>();
		Collider[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			TrashItem componentInParent = ((Component)array2[i]).GetComponentInParent<TrashItem>();
			if ((Object)(object)componentInParent != (Object)null && componentInParent.CanGoInContainer)
			{
				list.Add(componentInParent);
			}
		}
		return list;
	}

	private void StartBagTrash(TrashContainer container)
	{
		IsBaggingTrash = true;
		_bagTrashTime = 0f;
		_baggedContainer = container;
		RustleSound.Play();
	}

	private void StopBagTrash(bool complete)
	{
		IsBaggingTrash = false;
		_bagTrashTime = 0f;
		RustleSound.Stop();
		if (complete)
		{
			_baggedContainer.BagTrash();
			BagSound.DuplicateAndPlayOneShot();
			((BaseItemInstance)itemInstance).ChangeQuantity(-1);
		}
		_baggedContainer = null;
	}

	private void StartPickup()
	{
		IsPickingUpTrash = true;
		_pickupTrashTime = 0f;
		RustleSound.Play();
	}

	private void StopPickup(bool complete)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		IsPickingUpTrash = false;
		_pickupTrashTime = 0f;
		((Component)PickupAreaProjector).gameObject.SetActive(false);
		RustleSound.Stop();
		if (!complete)
		{
			return;
		}
		List<TrashItem> trashItemsAtPoint = GetTrashItemsAtPoint(((Component)PickupAreaProjector).transform.position);
		foreach (TrashItem item in trashItemsAtPoint)
		{
			item.DestroyTrash();
		}
		((BaseItemInstance)itemInstance).ChangeQuantity(-1);
		TrashContentData content = new TrashContentData(trashItemsAtPoint);
		NetworkSingleton<TrashManager>.Instance.CreateTrashBag(NetworkSingleton<TrashManager>.Instance.TrashBagPrefab.ID, ((Component)PickupAreaProjector).transform.position + Vector3.up * 0.4f, Quaternion.identity, content);
		BagSound.DuplicateAndPlayOneShot();
	}
}
