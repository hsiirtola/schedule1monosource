using ScheduleOne.ObjectScripts;
using UnityEngine;

namespace ScheduleOne.Growing;

public class PotMoistureDisplay : GrowContainerMoistureDisplay
{
	[SerializeField]
	private GameObject _temperatureBoostIndicator;

	private Pot _pot;

	protected override void Awake()
	{
		base.Awake();
		_pot = GrowContainer as Pot;
	}

	protected override void UpdateCanvasContents()
	{
		base.UpdateCanvasContents();
		_temperatureBoostIndicator.SetActive((Object)(object)_pot.Plant != (Object)null && !_pot.Plant.IsFullyGrown && _pot.GetTemperatureGrowthMultiplier() >= 1.01f);
	}
}
