using ScheduleOne.DevUtilities;
using ScheduleOne.Management;
using ScheduleOne.Tools;
using UnityEngine;

namespace ScheduleOne.Interaction;

public class IUsableInteractableObject : InteractableObject
{
	[SerializeReference]
	private MonoBehaviour _iUsableMonoBehaviour;

	private string _defaultMessage;

	private IUsable _iUsable;

	private void Awake()
	{
		_iUsable = _iUsableMonoBehaviour as IUsable;
		_defaultMessage = message;
	}

	public override void Hovered()
	{
		if (Singleton<ManagementClipboard>.Instance.IsEquipped)
		{
			SetInteractableState(EInteractableState.Disabled);
		}
		else if (_iUsable.IsInUse)
		{
			SetMessage("In use by " + _iUsable.UserName);
			SetInteractableState(EInteractableState.Invalid);
		}
		else
		{
			SetMessage(_defaultMessage);
			SetInteractableState(EInteractableState.Default);
		}
		base.Hovered();
	}
}
