using ScheduleOne.ObjectScripts;
using UnityEngine;

namespace ScheduleOne.Growing;

public class MushroomBedMoistureDisplay : GrowContainerMoistureDisplay
{
	[SerializeField]
	private GameObject _tooHotIndicator;

	private MushroomBed _bed;

	protected override void Awake()
	{
		base.Awake();
		_bed = GrowContainer as MushroomBed;
	}

	protected override void UpdateCanvasContents()
	{
		base.UpdateCanvasContents();
		_tooHotIndicator.SetActive((Object)(object)_bed.CurrentColony != (Object)null && !_bed.CurrentColony.IsFullyGrown && _bed.CurrentColony.IsTooHotToGrow);
	}
}
