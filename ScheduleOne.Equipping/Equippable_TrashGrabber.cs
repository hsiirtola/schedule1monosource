using System.Collections.Generic;
using ScheduleOne.Audio;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts.WateringCan;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Trash;
using ScheduleOne.UI;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Equipping;

public class Equippable_TrashGrabber : Equippable_Viewmodel
{
	public const float TrashDropSpacing = 0.15f;

	[Header("References")]
	public Transform TrashContent;

	public Transform TrashContent_Min;

	public Transform TrashContent_Max;

	public Animation GrabAnim;

	public Transform Bin;

	public Transform BinRaisedPosition;

	public AudioSourceController TrashDropSound;

	[Header("Settings")]
	public float DropTime = 0.4f;

	public float DropForce = 1f;

	public Vector3 TrashDropOffset;

	public UnityEvent onPickup;

	private TrashGrabberInstance trashGrabberInstance;

	private Pose defaultBinPosition;

	private Vector3 defaultBinScale;

	public static Equippable_TrashGrabber Instance { get; private set; }

	public static bool IsEquipped => (Object)(object)Instance != (Object)null;

	private float currentDropTime { get; set; }

	private float timeSinceLastDrop { get; set; } = 100f;

	public override void Equip(ItemInstance item)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		base.Equip(item);
		trashGrabberInstance = item as TrashGrabberInstance;
		((BaseItemInstance)trashGrabberInstance).onDataChanged += RefreshVisuals;
		defaultBinPosition = new Pose(Bin.localPosition, Bin.localRotation);
		defaultBinScale = Bin.localScale;
		Instance = this;
		Singleton<InputPromptsCanvas>.Instance.LoadModule("trashgrabber");
		RefreshVisuals();
	}

	public override void Unequip()
	{
		base.Unequip();
		((BaseItemInstance)trashGrabberInstance).onDataChanged -= RefreshVisuals;
		Singleton<InputPromptsCanvas>.Instance.UnloadModule();
		Instance = null;
	}

	protected override void Update()
	{
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		timeSinceLastDrop += Time.deltaTime;
		if (GameInput.GetButton(GameInput.ButtonCode.PrimaryClick))
		{
			currentDropTime = Mathf.Clamp(currentDropTime + Time.deltaTime, 0f, DropTime);
			if (trashGrabberInstance.GetTotalSize() > 0)
			{
				if (!TrashDropSound.IsPlaying)
				{
					TrashDropSound.Play();
				}
				TrashDropSound.VolumeMultiplier = Mathf.Lerp(TrashDropSound.VolumeMultiplier, 1f, Time.deltaTime * 4f);
				if (currentDropTime >= DropTime - 0.05f && timeSinceLastDrop >= 0.15f)
				{
					timeSinceLastDrop = 0f;
					EjectTrash();
				}
			}
			else
			{
				TrashDropSound.VolumeMultiplier = Mathf.Lerp(TrashDropSound.VolumeMultiplier, 0f, Time.deltaTime * 4f);
			}
		}
		else
		{
			currentDropTime = Mathf.Clamp(currentDropTime - Time.deltaTime, 0f, DropTime);
			TrashDropSound.VolumeMultiplier = Mathf.Lerp(TrashDropSound.VolumeMultiplier, 0f, Time.deltaTime * 4f);
		}
		float num = Mathf.SmoothStep(0f, 1f, currentDropTime / DropTime);
		Bin.localPosition = Vector3.Lerp(defaultBinPosition.position, BinRaisedPosition.localPosition, num);
		Bin.localRotation = Quaternion.Lerp(defaultBinPosition.rotation, BinRaisedPosition.localRotation, num);
		Bin.localScale = Vector3.Lerp(defaultBinScale, BinRaisedPosition.localScale, num);
	}

	private void EjectTrash()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		if (trashGrabberInstance.GetTotalSize() > 0)
		{
			List<string> trashIDs = trashGrabberInstance.GetTrashIDs();
			string id = trashIDs[trashIDs.Count - 1];
			trashGrabberInstance.RemoveTrash(id, 1);
			NetworkSingleton<TrashManager>.Instance.CreateTrashItem(id, ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.TransformPoint(TrashDropOffset), Random.rotation, PlayerSingleton<PlayerMovement>.Instance.Controller.velocity + ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.forward * DropForce);
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)Instance == (Object)(object)this)
		{
			Instance = null;
		}
	}

	public void PickupTrash(TrashItem item)
	{
		GrabAnim.Stop();
		GrabAnim.Play();
		trashGrabberInstance.AddTrash(item.ID, 1);
		item.DestroyTrash();
		if (onPickup != null)
		{
			onPickup.Invoke();
		}
	}

	public int GetCapacity()
	{
		return 20 - trashGrabberInstance.GetTotalSize();
	}

	private void RefreshVisuals()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Clamp01((float)trashGrabberInstance.GetTotalSize() / 20f);
		TrashContent.localPosition = Vector3.Lerp(TrashContent_Min.localPosition, TrashContent_Max.localPosition, num);
		TrashContent.localScale = Vector3.Lerp(TrashContent_Min.localScale, TrashContent_Max.localScale, num);
		((Component)TrashContent).gameObject.SetActive(num > 0f);
	}
}
