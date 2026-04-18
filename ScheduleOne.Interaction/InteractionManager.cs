using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dragging;
using ScheduleOne.EntityFramework;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using ScheduleOne.UI;
using ScheduleOne.UI.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ScheduleOne.Interaction;

public class InteractionManager : Singleton<InteractionManager>
{
	public const float RayRadius = 0.075f;

	public const float MaxInteractionRange = 5f;

	[SerializeField]
	protected LayerMask interaction_SearchMask;

	[SerializeField]
	protected float rightClickRange = 5f;

	public EInteractionSearchType interactionSearchType;

	public bool DEBUG;

	[Header("Settings")]
	public InputActionReference InteractInput;

	[Header("Visuals Settings")]
	public Color messageColor_Default;

	public Color iconColor_Default;

	public Color iconColor_Default_Key;

	public Color messageColor_Invalid;

	public Color iconColor_Invalid;

	public Sprite icon_Key;

	public Sprite icon_LeftMouse;

	public Sprite icon_Cross;

	public static float interactCooldown = 0.1f;

	private float timeSinceLastInteractStart;

	private BuildableItem itemBeingDestroyed;

	private float destroyTime;

	private static float timeToDestroy = 0.5f;

	public LayerMask Interaction_SearchMask => interaction_SearchMask;

	public bool CanDestroy { get; set; } = true;

	public InteractableObject HoveredInteractableObject { get; protected set; }

	public InteractableObject HoveredValidInteractableObject { get; protected set; }

	public InteractableObject InteractedObject { get; protected set; }

	public string InteractKeyStr { get; protected set; } = string.Empty;

	protected override void Start()
	{
		base.Start();
		LoadInteractKey();
		Settings settings = Singleton<Settings>.Instance;
		settings.onInputsApplied = (Action)Delegate.Remove(settings.onInputsApplied, new Action(LoadInteractKey));
		Settings settings2 = Singleton<Settings>.Instance;
		settings2.onInputsApplied = (Action)Delegate.Combine(settings2.onInputsApplied, new Action(LoadInteractKey));
	}

	protected override void OnDestroy()
	{
		if (Singleton<Settings>.InstanceExists)
		{
			Settings settings = Singleton<Settings>.Instance;
			settings.onInputsApplied = (Action)Delegate.Remove(settings.onInputsApplied, new Action(LoadInteractKey));
		}
		base.OnDestroy();
	}

	private void LoadInteractKey()
	{
		string text = default(string);
		string controlPath = default(string);
		InputActionRebindingExtensions.GetBindingDisplayString(InteractInput.action, 0, ref text, ref controlPath, (DisplayStringOptions)0);
		InteractKeyStr = Singleton<InputPromptsManager>.Instance.GetDisplayNameForControlPath(controlPath);
	}

	protected virtual void Update()
	{
		timeSinceLastInteractStart += Time.deltaTime;
		if (Singleton<GameInput>.InstanceExists)
		{
			CheckRightClick();
		}
	}

	protected virtual void LateUpdate()
	{
		if (Singleton<GameInput>.InstanceExists)
		{
			CheckHover();
			if ((Object)(object)HoveredInteractableObject != (Object)null)
			{
				HoveredInteractableObject.Hovered();
			}
			CheckInteraction();
		}
	}

	protected virtual void CheckHover()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		if (IsAnythingBlockingInteraction())
		{
			HoveredInteractableObject = null;
			return;
		}
		Ray val = default(Ray);
		switch (interactionSearchType)
		{
		case EInteractionSearchType.CameraForward:
			((Ray)(ref val)).origin = ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position;
			((Ray)(ref val)).direction = ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.forward;
			break;
		case EInteractionSearchType.Mouse:
			val = PlayerSingleton<PlayerCamera>.Instance.Camera.ScreenPointToRay(GameInput.MousePosition);
			break;
		default:
			Console.LogWarning("EInteractionSearchType type not accounted for");
			return;
		}
		InteractableObject hoveredInteractableObject = HoveredInteractableObject;
		HoveredInteractableObject = null;
		RaycastHit[] array = Physics.SphereCastAll(val, 0.075f, 5f, LayerMask.op_Implicit(interaction_SearchMask), (QueryTriggerInteraction)2);
		RaycastHit[] array2 = Physics.RaycastAll(val, 5f, LayerMask.op_Implicit(interaction_SearchMask), (QueryTriggerInteraction)2);
		if (array.Length != 0)
		{
			Array.Sort(array, (RaycastHit x, RaycastHit y) => ((RaycastHit)(ref x)).distance.CompareTo(((RaycastHit)(ref y)).distance));
			List<InteractableObject> list = new List<InteractableObject>();
			Dictionary<InteractableObject, RaycastHit> objectHits = new Dictionary<InteractableObject, RaycastHit>();
			for (int num = 0; num < array.Length; num++)
			{
				RaycastHit value = array[num];
				InteractableObject componentInParent = ((Component)((RaycastHit)(ref value)).collider).GetComponentInParent<InteractableObject>();
				if ((Object)(object)componentInParent == (Object)null)
				{
					bool flag = false;
					RaycastHit[] array3 = array2;
					for (int num2 = 0; num2 < array3.Length; num2++)
					{
						RaycastHit val2 = array3[num2];
						if ((Object)(object)((RaycastHit)(ref val2)).collider == (Object)(object)((RaycastHit)(ref value)).collider)
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						break;
					}
				}
				else if (!list.Contains(componentInParent) && (Object)(object)componentInParent != (Object)null && Vector3.Distance(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position, ((RaycastHit)(ref value)).point) <= componentInParent.MaxInteractionRange)
				{
					list.Add(componentInParent);
					objectHits.Add(componentInParent, value);
				}
			}
			list.Sort(delegate(InteractableObject x, InteractableObject y)
			{
				//IL_001c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0021: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Unknown result type (might be due to invalid IL or missing references)
				//IL_0038: Unknown result type (might be due to invalid IL or missing references)
				int num4 = y.Priority.CompareTo(x.Priority);
				if (num4 == 0)
				{
					RaycastHit val5 = objectHits[x];
					float distance = ((RaycastHit)(ref val5)).distance;
					val5 = objectHits[y];
					return distance.CompareTo(((RaycastHit)(ref val5)).distance);
				}
				return num4;
			});
			for (int num3 = 0; num3 < list.Count; num3++)
			{
				RaycastHit val3 = objectHits[list[num3]];
				InteractableObject interactableObject = list[num3];
				if ((Object)(object)interactableObject == (Object)null)
				{
					bool flag2 = false;
					RaycastHit[] array3 = array2;
					for (int num2 = 0; num2 < array3.Length; num2++)
					{
						RaycastHit val4 = array3[num2];
						if ((Object)(object)((RaycastHit)(ref val4)).collider == (Object)(object)((RaycastHit)(ref val3)).collider)
						{
							flag2 = true;
							break;
						}
					}
					if (flag2)
					{
						break;
					}
					continue;
				}
				if (!interactableObject.CheckAngleLimit(((Ray)(ref val)).origin))
				{
					interactableObject = null;
				}
				if ((Object)(object)interactableObject != (Object)null && !((Behaviour)interactableObject).enabled)
				{
					interactableObject = null;
				}
				if ((Object)(object)interactableObject != (Object)null && Vector3.Distance(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position, ((RaycastHit)(ref val3)).point) <= interactableObject.MaxInteractionRange)
				{
					HoveredInteractableObject = interactableObject;
					if ((Object)(object)interactableObject != (Object)(object)hoveredInteractableObject)
					{
						Singleton<InteractionCanvas>.Instance.displayScale = 1f;
					}
					break;
				}
			}
		}
		if (DEBUG)
		{
			InteractableObject hoveredInteractableObject2 = HoveredInteractableObject;
			Debug.Log((object)("Hovered interactable object: " + ((hoveredInteractableObject2 != null) ? ((Object)hoveredInteractableObject2).name : null)));
		}
	}

	public bool IsAnythingBlockingInteraction()
	{
		if (!PlayerSingleton<PlayerCamera>.InstanceExists)
		{
			return true;
		}
		if (Singleton<TaskManager>.InstanceExists && Singleton<TaskManager>.Instance.currentTask != null)
		{
			return true;
		}
		if (Singleton<GameplayMenu>.InstanceExists && Singleton<GameplayMenu>.Instance.IsOpen)
		{
			return true;
		}
		if ((Object)(object)PlayerSingleton<PlayerMovement>.Instance.CurrentVehicle != (Object)null)
		{
			return true;
		}
		if (PlayerSingleton<PlayerCamera>.Instance.activeUIElementCount > 0)
		{
			return true;
		}
		if (PlayerSingleton<PlayerInventory>.Instance.isAnythingEquipped && (Object)(object)PlayerSingleton<PlayerInventory>.Instance.equippable != (Object)null && !PlayerSingleton<PlayerInventory>.Instance.equippable.CanInteractWhenEquipped)
		{
			return true;
		}
		if (Player.Local.IsSkating)
		{
			return true;
		}
		if (Singleton<PauseMenu>.Instance.IsPaused)
		{
			return true;
		}
		if (NetworkSingleton<DragManager>.Instance.IsDragging)
		{
			return true;
		}
		return false;
	}

	protected virtual void CheckInteraction()
	{
		HoveredValidInteractableObject = null;
		if ((Object)(object)InteractedObject != (Object)null && ((InteractedObject._interactionType == InteractableObject.EInteractionType.Key_Press && !GameInput.GetButton(GameInput.ButtonCode.Interact)) || (InteractedObject._interactionType == InteractableObject.EInteractionType.LeftMouse_Click && !GameInput.GetButton(GameInput.ButtonCode.PrimaryClick))))
		{
			InteractedObject.EndInteract();
			InteractedObject = null;
		}
		if (!((Object)(object)HoveredInteractableObject == (Object)null) && HoveredInteractableObject._interactionState != InteractableObject.EInteractableState.Disabled && !Singleton<PauseMenu>.Instance.IsPaused)
		{
			HoveredValidInteractableObject = HoveredInteractableObject;
			if (GameInput.GetButton(GameInput.ButtonCode.Interact) && timeSinceLastInteractStart >= interactCooldown && HoveredInteractableObject._interactionType == InteractableObject.EInteractionType.Key_Press && (!HoveredInteractableObject.RequiresUniqueClick || GameInput.GetButtonDown(GameInput.ButtonCode.Interact)))
			{
				timeSinceLastInteractStart = 0f;
				HoveredInteractableObject.StartInteract();
				InteractedObject = HoveredInteractableObject;
			}
			if (GameInput.GetButton(GameInput.ButtonCode.PrimaryClick) && timeSinceLastInteractStart >= interactCooldown && HoveredInteractableObject._interactionType == InteractableObject.EInteractionType.LeftMouse_Click && (!HoveredInteractableObject.RequiresUniqueClick || GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick)))
			{
				timeSinceLastInteractStart = 0f;
				HoveredInteractableObject.StartInteract();
				InteractedObject = HoveredInteractableObject;
			}
		}
	}

	protected virtual void CheckRightClick()
	{
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		if (!PlayerSingleton<PlayerCamera>.InstanceExists)
		{
			return;
		}
		if (Singleton<TaskManager>.Instance.currentTask == null && (!PlayerSingleton<PlayerInventory>.Instance.isAnythingEquipped || ((Object)(object)PlayerSingleton<PlayerInventory>.Instance.equippable != (Object)null && PlayerSingleton<PlayerInventory>.Instance.equippable.CanInteractWhenEquipped && PlayerSingleton<PlayerInventory>.Instance.equippable.CanPickUpWhenEquipped)) && PlayerSingleton<PlayerCamera>.Instance.activeUIElementCount == 0 && CanDestroy && GameInput.GetButton(GameInput.ButtonCode.SecondaryClick))
		{
			BuildableItem hoveredBuildableItem = GetHoveredBuildableItem();
			if ((Object)(object)hoveredBuildableItem != (Object)null)
			{
				if (hoveredBuildableItem.CanBePickedUp(out var reason))
				{
					if ((Object)(object)itemBeingDestroyed == (Object)(object)hoveredBuildableItem)
					{
						destroyTime += Time.deltaTime;
					}
					itemBeingDestroyed = hoveredBuildableItem;
					if (destroyTime >= timeToDestroy)
					{
						itemBeingDestroyed.PickupItem();
						destroyTime = 0f;
					}
					flag = true;
					Singleton<HUD>.Instance.ShowRadialIndicator(destroyTime / timeToDestroy);
				}
				else
				{
					Singleton<HUD>.Instance.CrosshairText.Show(reason, Color32.op_Implicit(new Color32(byte.MaxValue, (byte)100, (byte)100, byte.MaxValue)));
				}
			}
		}
		if (!flag)
		{
			destroyTime = 0f;
		}
	}

	protected virtual BuildableItem GetHoveredBuildableItem()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (PlayerSingleton<PlayerCamera>.Instance.LookRaycast(rightClickRange, out var hit, LayerMask.op_Implicit(1 << LayerMask.NameToLayer("Default"))))
		{
			return ((Component)((RaycastHit)(ref hit)).collider).GetComponentInParent<BuildableItem>();
		}
		return null;
	}

	public void SetCanDestroy(bool canDestroy)
	{
		CanDestroy = canDestroy;
	}
}
