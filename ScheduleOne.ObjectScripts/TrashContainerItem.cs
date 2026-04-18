using System;
using System.Collections.Generic;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Tiles;
using ScheduleOne.Trash;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

namespace ScheduleOne.ObjectScripts;

[RequireComponent(typeof(TrashContainer))]
public class TrashContainerItem : GridItem, ITransitEntity
{
	public const float MAX_VERTICAL_OFFSET = 2f;

	public TrashContainer Container;

	public ParticleSystem Flies;

	public AudioSourceController TrashAddedSound;

	public DecalProjector PickupAreaProjector;

	public Transform[] accessPoints;

	[Header("Pickup settings")]
	public bool UsableByCleaners = true;

	public float PickupSquareWidth = 3.5f;

	public List<TrashItem> TrashItemsInRadius = new List<TrashItem>();

	public List<TrashBag> TrashBagsInRadius = new List<TrashBag>();

	private float calculatedPickupRadius;

	private bool NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002ETrashContainerItemAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EObjectScripts_002ETrashContainerItemAssembly_002DCSharp_002Edll_Excuted;

	public string Name => GetManagementName();

	public List<ItemSlot> InputSlots { get; set; } = new List<ItemSlot>();

	public List<ItemSlot> OutputSlots { get; set; } = new List<ItemSlot>();

	public Transform LinkOrigin => ((Component)this).transform;

	public Transform[] AccessPoints => accessPoints;

	public bool Selectable { get; }

	public bool IsAcceptingItems { get; set; }

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EObjectScripts_002ETrashContainerItem_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected override void Start()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		base.Start();
		Container.onTrashLevelChanged.AddListener(new UnityAction(TrashLevelChanged));
		Container.onTrashAdded.AddListener((UnityAction<string>)TrashAdded);
	}

	public override void InitializeGridItem(ItemInstance instance, Grid grid, Vector2 originCoordinate, int rotation, string GUID)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		base.InitializeGridItem(instance, grid, originCoordinate, rotation, GUID);
		((MonoBehaviour)this).InvokeRepeating("CheckTrashItems", Random.Range(0f, 1f), 1f);
	}

	private void TrashLevelChanged()
	{
		base.HasChanged = true;
		if (Container.NormalizedTrashLevel > 0.75f)
		{
			if (!Flies.isPlaying)
			{
				Flies.Play();
			}
		}
		else if (Flies.isPlaying)
		{
			Flies.Stop();
		}
	}

	public override bool CanBeDestroyed(out string reason)
	{
		if (Container.TrashLevel > 0)
		{
			reason = "Contains trash";
			return false;
		}
		return base.CanBeDestroyed(out reason);
	}

	public override BuildableItemData GetBaseData()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		return new TrashContainerData(base.GUID, base.ItemInstance, 0, base.OwnerGrid, _originCoordinate, _rotation, Container.Content.GetData());
	}

	private void TrashAdded(string trashID)
	{
		if (!((Object)(object)TrashAddedSound == (Object)null))
		{
			float volumeMultiplier = Mathf.Clamp01((float)NetworkSingleton<TrashManager>.Instance.GetTrashPrefab(trashID).Size / 4f);
			TrashAddedSound.VolumeMultiplier = volumeMultiplier;
			TrashAddedSound.Play();
		}
	}

	public override void ShowOutline(Color color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		base.ShowOutline(color);
		((Behaviour)PickupAreaProjector).enabled = true;
	}

	public override void HideOutline()
	{
		base.HideOutline();
		((Behaviour)PickupAreaProjector).enabled = false;
	}

	private void CheckTrashItems()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < TrashItemsInRadius.Count; i++)
		{
			if (!IsTrashValid(TrashItemsInRadius[i]))
			{
				RemoveTrashItemFromRadius(TrashItemsInRadius[i]);
				i--;
			}
		}
		Collider[] array = Physics.OverlapSphere(((Component)this).transform.position, calculatedPickupRadius, LayerMask.GetMask(new string[1] { "Trash" }), (QueryTriggerInteraction)1);
		for (int j = 0; j < array.Length; j++)
		{
			if (IsPointInPickupZone(((Component)array[j]).transform.position))
			{
				TrashItem componentInParent = ((Component)array[j]).GetComponentInParent<TrashItem>();
				if ((Object)(object)componentInParent != (Object)null && IsTrashValid(componentInParent))
				{
					AddTrashToRadius(componentInParent);
				}
			}
		}
	}

	private void AddTrashToRadius(TrashItem trashItem)
	{
		if (trashItem is TrashBag)
		{
			AddTrashBagToRadius(trashItem as TrashBag);
		}
		else if (!TrashItemsInRadius.Contains(trashItem))
		{
			TrashItemsInRadius.Add(trashItem);
			trashItem.onDestroyed = (Action<TrashItem>)Delegate.Combine(trashItem.onDestroyed, new Action<TrashItem>(RemoveTrashItemFromRadius));
		}
	}

	private void AddTrashBagToRadius(TrashBag trashBag)
	{
		if (!TrashBagsInRadius.Contains(trashBag))
		{
			TrashBagsInRadius.Add(trashBag);
			trashBag.onDestroyed = (Action<TrashItem>)Delegate.Combine(trashBag.onDestroyed, new Action<TrashItem>(RemoveTrashItemFromRadius));
		}
	}

	private void RemoveTrashItemFromRadius(TrashItem trashItem)
	{
		if (trashItem is TrashBag)
		{
			RemoveTrashBagFromRadius(trashItem as TrashBag);
		}
		else if (TrashItemsInRadius.Contains(trashItem))
		{
			TrashItemsInRadius.Remove(trashItem);
			trashItem.onDestroyed = (Action<TrashItem>)Delegate.Remove(trashItem.onDestroyed, new Action<TrashItem>(RemoveTrashItemFromRadius));
		}
	}

	private void RemoveTrashBagFromRadius(TrashBag trashBag)
	{
		if (TrashBagsInRadius.Contains(trashBag))
		{
			TrashBagsInRadius.Remove(trashBag);
			trashBag.onDestroyed = (Action<TrashItem>)Delegate.Remove(trashBag.onDestroyed, new Action<TrashItem>(RemoveTrashItemFromRadius));
		}
	}

	private bool IsTrashValid(TrashItem trashItem)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)trashItem == (Object)null)
		{
			return false;
		}
		if (!IsPointInPickupZone(((Component)trashItem).transform.position))
		{
			return false;
		}
		if (trashItem.Draggable.IsBeingDragged)
		{
			return false;
		}
		if (!base.ParentProperty.DoBoundsContainPoint(((Component)trashItem).transform.position))
		{
			return false;
		}
		return true;
	}

	public bool IsPointInPickupZone(Vector3 point)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Abs(point.x - ((Component)this).transform.position.x);
		float num2 = Mathf.Abs(point.z - ((Component)this).transform.position.z);
		if (num > PickupSquareWidth || num2 > PickupSquareWidth)
		{
			return false;
		}
		if (Mathf.Abs(point.y - ((Component)this).transform.position.y) > 2f)
		{
			return false;
		}
		return true;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002ETrashContainerItemAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002ETrashContainerItemAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EObjectScripts_002ETrashContainerItemAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EObjectScripts_002ETrashContainerItemAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	protected override void Awake_UserLogic_ScheduleOne_002EObjectScripts_002ETrashContainerItem_Assembly_002DCSharp_002Edll()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		PickupAreaProjector.size = new Vector3(PickupSquareWidth * 2f, PickupSquareWidth * 2f, 0.2f);
		((Behaviour)PickupAreaProjector).enabled = false;
		calculatedPickupRadius = PickupSquareWidth * Mathf.Sqrt(2f);
	}
}
