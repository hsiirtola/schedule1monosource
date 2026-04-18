using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Temperature;
using ScheduleOne.UI;
using ScheduleOne.Variables;
using UnityEngine;

namespace ScheduleOne.Building;

public class BuildStart_AirConditioner : BuildStart_Grid
{
	private AirConditioner ac;

	public override void StartBuilding(ItemInstance itemInstance)
	{
		base.StartBuilding(itemInstance);
		ac = ghostModelClass as AirConditioner;
		if ((Object)(object)ac == (Object)null)
		{
			Console.LogError("Not an AC!");
		}
		ac.SetMode(AirConditioner.EMode.Heating);
		ac.TemperatureDisplay.SetTemperatureGetter(() => ac.TemperatureEmitter.Temperature);
		ac.TemperatureDisplay.SetEnabled(enabled: true);
		if (NetworkSingleton<VariableDatabase>.InstanceExists)
		{
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("TemperatureSystemEnabled", true.ToString(), network: false);
			if (!NetworkSingleton<VariableDatabase>.Instance.GetValue<bool>("ACToggleHintShown"))
			{
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("ACToggleHintShown", true.ToString());
				Singleton<HintDisplay>.Instance.ShowHint_20s("Use <Input_SecondaryClick> to toggle between <h2>heating</h> and <h1>cooling</h> while placing an AC unit.");
			}
		}
	}

	protected override string GetInputPromptsModuleName()
	{
		return "building_airconditioner";
	}
}
