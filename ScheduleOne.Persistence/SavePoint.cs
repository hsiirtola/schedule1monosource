using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Persistence;

public class SavePoint : MonoBehaviour
{
	public const float SAVE_COOLDOWN = 60f;

	public InteractableObject IntObj;

	public UnityEvent onSaveStart;

	public UnityEvent onSaveComplete;

	public void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		IntObj.onHovered.AddListener(new UnityAction(Hovered));
		IntObj.onInteractStart.AddListener(new UnityAction(Interacted));
		Singleton<SaveManager>.Instance.onSaveComplete.RemoveListener(new UnityAction(OnSaveComplete));
		Singleton<SaveManager>.Instance.onSaveComplete.AddListener(new UnityAction(OnSaveComplete));
		Singleton<SaveManager>.Instance.onSaveStart.RemoveListener(new UnityAction(OnSaveStart));
		Singleton<SaveManager>.Instance.onSaveStart.AddListener(new UnityAction(OnSaveStart));
	}

	public void Hovered()
	{
		if (!InstanceFinder.IsServer)
		{
			IntObj.SetMessage("Only host can save");
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Invalid);
			return;
		}
		if (Singleton<SaveManager>.Instance.IsSaving)
		{
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
		}
		if (CanSave(out var reason))
		{
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
			IntObj.SetMessage("Save game");
		}
		else if (Singleton<SaveManager>.Instance.SecondsSinceLastSave < 10f)
		{
			IntObj.SetMessage("Game saved!");
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Label);
		}
		else
		{
			IntObj.SetMessage(reason);
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Invalid);
		}
	}

	private bool CanSave(out string reason)
	{
		reason = string.Empty;
		if (Singleton<SaveManager>.Instance.SecondsSinceLastSave < 60f)
		{
			reason = "Wait " + Mathf.Ceil(60f - Singleton<SaveManager>.Instance.SecondsSinceLastSave) + "s";
			return false;
		}
		if (Singleton<SaveManager>.Instance.SecondsSinceLastSave < 60f)
		{
			reason = "Wait " + Mathf.Ceil(60f - Singleton<SaveManager>.Instance.SecondsSinceLastSave) + "s";
			return false;
		}
		return true;
	}

	public void Interacted()
	{
		if (CanSave(out var _))
		{
			Save();
		}
	}

	private void Save()
	{
		Singleton<SaveManager>.Instance.Save();
	}

	public void OnSaveStart()
	{
		if (onSaveStart != null)
		{
			onSaveStart.Invoke();
		}
	}

	public void OnSaveComplete()
	{
		if (onSaveComplete != null)
		{
			onSaveComplete.Invoke();
		}
	}
}
