using HSVPicker;
using UnityEngine;

namespace ScheduleOne.AvatarFramework.Customization;

public class ACColorPickerReplicator : ACReplicator
{
	public ColorPicker picker;

	protected override void AvatarSettingsChanged(AvatarSettings newSettings)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		base.AvatarSettingsChanged(newSettings);
		picker.CurrentColor = (Color)newSettings[propertyName];
	}
}
