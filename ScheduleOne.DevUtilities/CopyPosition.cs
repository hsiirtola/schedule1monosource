using System.Linq;
using ScheduleOne.UI.WorldspacePopup;
using UnityEngine;

namespace ScheduleOne.DevUtilities;

public class CopyPosition : MonoBehaviour
{
	public Transform ToCopy;

	private void Start()
	{
		UpdateEnabledState();
	}

	private void LateUpdate()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = ToCopy.position;
	}

	public void UpdateEnabledState()
	{
		((Behaviour)this).enabled = ((Component)this).GetComponentsInChildren<WorldspacePopup>().Any((WorldspacePopup component) => ((Behaviour)component).enabled);
	}
}
