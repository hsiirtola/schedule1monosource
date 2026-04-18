using System.Linq;
using ScheduleOne.Configuration;
using ScheduleOne.Core;
using ScheduleOne.Core.Audio;
using ScheduleOne.Core.Settings;
using ScheduleOne.Core.Settings.Framework;
using UnityEngine;

namespace ScheduleOne.Audio;

[CreateAssetMenu(fileName = "SFXConfiguration", menuName = "ScheduleOne/Configurations/SFX Configuration")]
public class SFXConfiguration : Configuration<SFXSettings>
{
	public AudioSourceController ImpactSoundPrefab;

	public bool TryGetImpactTypeData(EImpactSound material, out ImpactSound data)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		data = ((SettingsList<ImpactSound>)(object)base.Settings.ImpactSounds).Items.FirstOrDefault((ImpactSound x) => x.ImpactSoundType == material);
		return data != null;
	}

	public bool TryGetFootstepSoundGroup(EMaterialType materialType, out FootstepSound group)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		group = ((SettingsList<FootstepSound>)(object)base.Settings.FootstepSounds).Items.FirstOrDefault((FootstepSound g) => g.AppliesTo.Exists((EMaterialType mt) => mt == materialType));
		return group != null;
	}
}
