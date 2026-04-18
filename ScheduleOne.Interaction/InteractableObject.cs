using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Interaction;

public class InteractableObject : MonoBehaviour
{
	public enum EInteractionType
	{
		Key_Press,
		LeftMouse_Click
	}

	public enum EInteractableState
	{
		Default,
		Invalid,
		Disabled,
		Label
	}

	[Header("Settings")]
	[SerializeField]
	protected string message = "<Message>";

	[SerializeField]
	protected EInteractionType interactionType;

	[SerializeField]
	protected EInteractableState interactionState;

	public float MaxInteractionRange = 5f;

	public bool RequiresUniqueClick = true;

	public int Priority;

	[SerializeField]
	protected Collider displayLocationCollider;

	public Transform displayLocationPoint;

	[Header("Angle Limits")]
	public bool LimitInteractionAngle;

	public float AngleLimit = 90f;

	[Header("Events")]
	public UnityEvent onHovered = new UnityEvent();

	public UnityEvent onInteractStart = new UnityEvent();

	public UnityEvent onInteractEnd = new UnityEvent();

	public EInteractionType _interactionType => interactionType;

	public EInteractableState _interactionState => interactionState;

	public void SetInteractionType(EInteractionType type)
	{
		interactionType = type;
	}

	public void SetInteractableState(EInteractableState state)
	{
		interactionState = state;
	}

	public void SetMessage(string _message)
	{
		message = _message;
	}

	public virtual void Hovered()
	{
		if (onHovered != null)
		{
			onHovered.Invoke();
		}
		if (interactionState != EInteractableState.Disabled)
		{
			ShowMessage();
		}
	}

	public virtual void StartInteract()
	{
		if (interactionState != EInteractableState.Invalid)
		{
			if (onInteractStart != null)
			{
				onInteractStart.Invoke();
			}
			Singleton<InteractionCanvas>.Instance.LerpDisplayScale(0.9f);
		}
	}

	public virtual void EndInteract()
	{
		if (onInteractEnd != null)
		{
			onInteractEnd.Invoke();
		}
		Singleton<InteractionCanvas>.Instance.LerpDisplayScale(1f);
	}

	protected virtual void ShowMessage()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		Vector3 pos = ((Component)this).transform.position;
		if ((Object)(object)displayLocationCollider != (Object)null)
		{
			pos = displayLocationCollider.ClosestPoint(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position);
		}
		else if ((Object)(object)displayLocationPoint != (Object)null)
		{
			pos = displayLocationPoint.position;
		}
		Sprite icon = null;
		string spriteText = string.Empty;
		Color iconColor = Color.white;
		Color white = Color.white;
		switch (interactionState)
		{
		case EInteractableState.Default:
			white = Singleton<InteractionCanvas>.Instance.DefaultMessageColor;
			switch (interactionType)
			{
			case EInteractionType.Key_Press:
				icon = Singleton<InteractionCanvas>.Instance.KeyIcon;
				spriteText = Singleton<InteractionManager>.Instance.InteractKeyStr;
				iconColor = Singleton<InteractionCanvas>.Instance.DefaultKeyColor;
				break;
			case EInteractionType.LeftMouse_Click:
				icon = Singleton<InteractionCanvas>.Instance.LeftMouseIcon;
				iconColor = Singleton<InteractionCanvas>.Instance.DefaultIconColor;
				break;
			default:
				Console.LogWarning("EInteractionType not accounted for!");
				break;
			}
			break;
		case EInteractableState.Invalid:
			icon = Singleton<InteractionCanvas>.Instance.CrossIcon;
			iconColor = Singleton<InteractionCanvas>.Instance.InvalidIconColor;
			white = Singleton<InteractionCanvas>.Instance.InvalidMessageColor;
			break;
		case EInteractableState.Disabled:
			return;
		case EInteractableState.Label:
			icon = null;
			white = Singleton<InteractionCanvas>.Instance.DefaultMessageColor;
			break;
		default:
			Console.LogWarning("EInteractableState not accounted for!");
			return;
		}
		Singleton<InteractionCanvas>.Instance.EnableInteractionDisplay(pos, icon, spriteText, message, white, iconColor);
	}

	public bool CheckAngleLimit(Vector3 interactionSource)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (!LimitInteractionAngle)
		{
			return true;
		}
		Vector3 val = interactionSource - ((Component)this).transform.position;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		return Mathf.Abs(Vector3.SignedAngle(((Component)this).transform.forward, normalized, Vector3.up)) < AngleLimit;
	}
}
