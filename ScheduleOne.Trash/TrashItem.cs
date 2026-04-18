using System;
using System.Collections.Generic;
using FishNet;
using ScheduleOne.Audio;
using ScheduleOne.Combat;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dragging;
using ScheduleOne.Equipping;
using ScheduleOne.GameTime;
using ScheduleOne.Interaction;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Property;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Trash;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Draggable))]
[RequireComponent(typeof(PhysicsDamageable))]
public class TrashItem : MonoBehaviour, IGUIDRegisterable, ISaveable
{
	public const float POSITION_CHANGE_THRESHOLD = 1f;

	public const float LINEAR_DRAG = 0.1f;

	public const float ANGULAR_DRAG = 0.1f;

	public const float MIN_Y = -100f;

	public const int INTERACTION_PRIORITY = 5;

	public Rigidbody Rigidbody;

	public Draggable Draggable;

	[Header("Settings")]
	public string ID = "trashid";

	[Range(0f, 5f)]
	public int Size = 2;

	[Range(0f, 10f)]
	public int SellValue = 1;

	public bool CanGoInContainer = true;

	public Collider[] colliders;

	private Vector3 lastPosition = Vector3.zero;

	public Action<TrashItem> onDestroyed;

	private bool collidersEnabled = true;

	private float timeOnPhysicsEnabled;

	public Guid GUID { get; protected set; }

	public ScheduleOne.Property.Property CurrentProperty { get; protected set; }

	public string SaveFolderName => "Trash_" + GUID.ToString().Substring(0, 6);

	public string SaveFileName => "Trash_" + GUID.ToString().Substring(0, 6);

	public Loader Loader => null;

	public bool ShouldSaveUnderFolder => false;

	public List<string> LocalExtraFiles { get; set; } = new List<string>();

	public List<string> LocalExtraFolders { get; set; } = new List<string>();

	public bool HasChanged { get; set; }

	protected void Awake()
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		LayerUtility.SetLayerRecursively(((Component)this).gameObject, LayerMask.NameToLayer("Trash"));
		RecheckPosition();
		((MonoBehaviour)this).InvokeRepeating("RecheckPosition", Random.Range(0f, 1f), 1f);
		SetPhysicsActive(active: false);
		Rigidbody.drag = 0.1f;
		Rigidbody.angularDrag = 0.1f;
		Rigidbody.interpolation = (RigidbodyInterpolation)1;
		Rigidbody.collisionDetectionMode = (CollisionDetectionMode)0;
		Rigidbody.sleepThreshold = 0.01f;
		Draggable.onDragStart.AddListener((UnityAction)delegate
		{
			SetContinuousCollisionDetection();
		});
		PhysicsDamageable physicsDamageable = ((Component)this).GetComponent<PhysicsDamageable>();
		if ((Object)(object)physicsDamageable == (Object)null)
		{
			physicsDamageable = ((Component)this).gameObject.AddComponent<PhysicsDamageable>();
		}
		PhysicsDamageable physicsDamageable2 = physicsDamageable;
		physicsDamageable2.onImpacted = (Action<Impact>)Delegate.Combine(physicsDamageable2.onImpacted, (Action<Impact>)delegate(Impact impact)
		{
			if (impact.ImpactForce > 0f)
			{
				SetContinuousCollisionDetection();
			}
		});
	}

	protected void Start()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		InitializeSaveable();
		NetworkSingleton<TimeManager>.Instance.onMinutePass += new Action(MinPass);
		Draggable.onHovered.AddListener(new UnityAction(Hovered));
		Draggable.onInteracted.AddListener(new UnityAction(Interacted));
	}

	public virtual void InitializeSaveable()
	{
		Singleton<SaveManager>.Instance.RegisterSaveable(this);
	}

	protected void OnValidate()
	{
		if ((Object)(object)Rigidbody == (Object)null)
		{
			Rigidbody = ((Component)this).GetComponent<Rigidbody>();
		}
		if ((Object)(object)Draggable == (Object)null)
		{
			Draggable = ((Component)this).GetComponent<Draggable>();
		}
		if (colliders == null || colliders.Length == 0)
		{
			colliders = ((Component)this).GetComponentsInChildren<Collider>();
		}
		if ((Object)(object)((Component)this).GetComponent<RBImpactSounds>() == (Object)null)
		{
			((Component)this).gameObject.AddComponent<RBImpactSounds>();
		}
	}

	protected void MinPass()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)this == (Object)null) && !((Object)(object)((Component)this).transform == (Object)null))
		{
			if (Time.time - timeOnPhysicsEnabled > 30f)
			{
				float num = Vector3.SqrMagnitude(((Component)PlayerSingleton<PlayerMovement>.Instance).transform.position - ((Component)this).transform.position);
				SetCollidersEnabled(num < 900f);
			}
			if (((Component)this).transform.position.y < -100f && InstanceFinder.IsServer)
			{
				Console.LogWarning("Trash item fell below the world. Destroying.");
				DestroyTrash();
			}
		}
	}

	protected void Hovered()
	{
		if (Equippable_TrashGrabber.IsEquipped && CanGoInContainer)
		{
			if (Equippable_TrashGrabber.Instance.GetCapacity() > 0)
			{
				Draggable.IntObj.SetMessage("Pick up");
				Draggable.IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
			}
			else
			{
				Draggable.IntObj.SetMessage("Bin is full");
				Draggable.IntObj.SetInteractableState(InteractableObject.EInteractableState.Invalid);
			}
		}
	}

	protected void Interacted()
	{
		if (Equippable_TrashGrabber.IsEquipped && CanGoInContainer && Equippable_TrashGrabber.Instance.GetCapacity() > 0)
		{
			Equippable_TrashGrabber.Instance.PickupTrash(this);
		}
	}

	public void SetGUID(Guid guid)
	{
		GUID = guid;
		GUIDManager.RegisterObject(this);
		string text = GUID.ToString();
		text = ((text[text.Length - 1] == '1') ? (text.Substring(0, text.Length - 1) + "2") : (text.Substring(0, text.Length - 1) + "1"));
		Draggable.SetGUID(new Guid(text));
	}

	public void SetVelocity(Vector3 velocity)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Rigidbody.velocity = velocity;
		HasChanged = true;
	}

	public void DestroyTrash()
	{
		NetworkSingleton<TrashManager>.Instance.DestroyTrash(this);
	}

	public virtual void Deinitialize()
	{
		NetworkSingleton<TimeManager>.Instance.onMinutePass -= new Action(MinPass);
	}

	private void OnDestroy()
	{
		if (NetworkSingleton<TimeManager>.InstanceExists)
		{
			NetworkSingleton<TimeManager>.Instance.onMinutePass -= new Action(MinPass);
		}
	}

	private void RecheckPosition()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (Vector3.Distance(lastPosition, ((Component)this).transform.position) > 1f)
		{
			lastPosition = ((Component)this).transform.position;
			HasChanged = true;
			RecheckProperty();
		}
	}

	public virtual TrashItemData GetData()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		return new TrashItemData(ID, GUID.ToString(), ((Component)this).transform.position, ((Component)this).transform.rotation);
	}

	public virtual string GetSaveString()
	{
		return string.Empty;
	}

	public virtual bool ShouldSave()
	{
		return true;
	}

	private void RecheckProperty()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)CurrentProperty != (Object)null && CurrentProperty.DoBoundsContainPoint(((Component)this).transform.position))
		{
			return;
		}
		CurrentProperty = null;
		for (int i = 0; i < ScheduleOne.Property.Property.OwnedProperties.Count; i++)
		{
			if (!(Vector3.Distance(((Component)this).transform.position, ScheduleOne.Property.Property.OwnedProperties[i].BoundingBox.transform.position) > 25f) && ScheduleOne.Property.Property.OwnedProperties[i].DoBoundsContainPoint(((Component)this).transform.position))
			{
				CurrentProperty = ScheduleOne.Property.Property.OwnedProperties[i];
				break;
			}
		}
	}

	public void SetContinuousCollisionDetection()
	{
		Rigidbody.collisionDetectionMode = (CollisionDetectionMode)1;
		SetPhysicsActive(active: true);
		((MonoBehaviour)this).CancelInvoke("SetDiscreteCollisionDetection");
		((MonoBehaviour)this).Invoke("SetDiscreteCollisionDetection", 60f);
	}

	public void SetDiscreteCollisionDetection()
	{
		if (!((Object)(object)Rigidbody == (Object)null))
		{
			SetPhysicsActive(active: false);
			Rigidbody.collisionDetectionMode = (CollisionDetectionMode)0;
		}
	}

	public void SetPhysicsActive(bool active)
	{
		Rigidbody.isKinematic = !active;
		SetCollidersEnabled(active);
		if (active)
		{
			timeOnPhysicsEnabled = Time.time;
		}
	}

	public void SetCollidersEnabled(bool enabled)
	{
		if (collidersEnabled != enabled)
		{
			collidersEnabled = enabled;
			for (int i = 0; i < colliders.Length; i++)
			{
				colliders[i].enabled = true;
			}
			if (!collidersEnabled)
			{
				Rigidbody.isKinematic = true;
			}
		}
	}
}
